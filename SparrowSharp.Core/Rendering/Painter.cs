using Sparrow.Core;
using Sparrow.Display;
using Sparrow.Geom;
using SparrowSharp.Core.Utils;
using System;
using System.Collections.Generic;

namespace SparrowSharp.Core.Rendering
{

    /** A class that orchestrates rendering of all Starling display objects.
    *
    *  <p>A Starling instance contains exactly one 'Painter' instance that should be used for all
    *  rendering purposes. Each frame, it is passed to the render methods of all rendered display
    *  objects. To access it outside a render method, call <code>Starling.painter</code>.</p>
    *
    *  <p>The painter is responsible for drawing all display objects to the screen. At its
    *  core, it is a wrapper for many Context3D methods, but that's not all: it also provides
    *  a convenient state mechanism, supports masking and acts as middleman between display
    *  objects and renderers.</p>
    *
    *  <strong>The State Stack</strong>
    *
    *  <p>The most important concept of the Painter class is the state stack. A RenderState
    *  stores a combination of settings that are currently used for rendering, e.g. the current
    *  projection- and modelview-matrices and context-related settings. It can be accessed
    *  and manipulated via the <code>state</code> property. Use the methods
    *  <code>pushState</code> and <code>popState</code> to store a specific state and restore
    *  it later. That makes it easy to write rendering code that doesn't have any side effects.</p>
    *
    *  <listing>
    *  painter.pushState(); // save a copy of the current state on the stack
    *  painter.state.renderTarget = renderTexture;
    *  painter.state.transformModelviewMatrix(object.transformationMatrix);
    *  painter.state.alpha = 0.5;
    *  painter.prepareToDraw(); // apply all state settings at the render context
    *  drawSomething(); // insert Stage3D rendering code here
    *  painter.popState(); // restores previous state</listing>
    *
    *  @see RenderState
    */
    public class Painter
    {
        // the key for the programs stored in 'sharedData'
        private static readonly string PROGRAM_DATA_NAME = "starling.rendering.Painter.Programs";

        // members

        private Stage3D _stage3D;
        private Context3D _context;
        private bool _shareContext;
        private int _drawCount;
        private uint _frameID;
        private float _pixelSize;
        private bool _enableErrorChecking;
        private Dictionary<uint, uint> _stencilReferenceValues;
        private List<Rectangle> _clipRectStack;
        private BatchProcessor _batchProcessor;
        private BatchProcessor _batchCache;
        private List<DisplayObject> _batchCacheExclusions;

        private TextureBase _actualRenderTarget;
        private string _actualCulling;
        private string _actualBlendMode;

        private float _backBufferWidth;
        private float _backBufferHeight;
        private float _backBufferScaleFactor;

        private RenderState _state;
        private List<RenderState> _stateStack;
        private int _stateStackPos;
        private int _stateStackLength;

        // helper objects
        private static Matrix sMatrix = Matrix.Create();
        private static Vector3D sPoint3D = new Vector3D();
        private static Matrix3D sMatrix3D = new Matrix3D();
        private static Rectangle sClipRect = new Rectangle();
        private static Rectangle sBufferRect = new Rectangle();
        private static Rectangle sScissorRect = new Rectangle();
        private static MeshSubset sMeshSubset = new MeshSubset();
        
        /** Creates a new Painter object. Normally, it's not necessary to create any custom
         *  painters; instead, use the global painter found on the Starling instance. */
        public Painter(Stage3D stage3D)
        {
            _stage3D = stage3D;
            _stage3D.addEventListener(Event.CONTEXT3D_CREATE, onContextCreated, false, 10, true);
            _context = _stage3D.context3D;
            _shareContext = _context != null && _context.driverInfo != "Disposed";
            _backBufferWidth = _context != null ? _context.backBufferWidth : 0;
            _backBufferHeight = _context != null ? _context.backBufferHeight : 0;
            _backBufferScaleFactor = _pixelSize = 1.0f;
            _stencilReferenceValues = new Dictionary<uint, uint>(); // use weak refs!
            _clipRectStack = new List<Rectangle>();

            _batchProcessor = new BatchProcessor();
            _batchProcessor.onBatchComplete = drawBatch;

            _batchCache = new BatchProcessor();
            _batchCache.onBatchComplete = drawBatch;
            _batchCacheExclusions = new List<DisplayObject>();

            _state = new RenderState();
            _state.onDrawRequired = finishMeshBatch;
            _stateStack = new List<RenderState>();
            _stateStackPos = -1;
            _stateStackLength = 0;
        }

        /** Disposes all mesh batches, programs, and - if it is not being shared -
        *  the render context. */
        public void Dispose()
        {
            _batchProcessor.Dispose();
            _batchCache.Dispose();

            if (!_shareContext)
            {
                _context.Dispose(false);
            }
        }

        // context handling

        /** Requests a context3D object from the stage3D object.
         *  This is called by Starling internally during the initialization process.
         *  You normally don't need to call this method yourself. (For a detailed description
         *  of the parameters, look at the documentation of the method with the same name in the
         *  "RenderUtil" class.)
         *
         *  @see starling.utils.RenderUtil
         */
        public void RequestContext3D(renderMode:String, profile:*)
        {
            RenderUtil.requestContext3D(_stage3D, renderMode, profile);
        }

        private void onContextCreated(Object eventObj)
        {
            _context = _stage3D.context3D;
            _context.enableErrorChecking = _enableErrorChecking;
            _actualBlendMode = null;
            _actualCulling = null;
        }

        /** Sets the viewport dimensions and other attributes of the rendering buffer.
         *  Starling will call this method internally, so most apps won't need to mess with this.
         *
         *  <p>Beware: if <code>shareContext</code> is enabled, the method will only update the
         *  painter's context-related information (like the size of the back buffer), but won't
         *  make any actual changes to the context.</p>
         *
         * @param viewPort                the position and size of the area that should be rendered
         *                                into, in pixels.
         * @param contentScaleFactor      only relevant for Desktop (!) HiDPI screens. If you want
         *                                to support high resolutions, pass the 'contentScaleFactor'
         *                                of the Flash stage; otherwise, '1.0'.
         * @param antiAlias               from 0 (none) to 16 (very high quality).
         * @param enableDepthAndStencil   indicates whether the depth and stencil buffers should
         *                                be enabled. Note that on AIR, you also have to enable
         *                                this setting in the app-xml (application descriptor);
         *                                otherwise, this setting will be silently ignored.
         */
        public void ConfigureBackBuffer(Rectangle viewPort, float contentScaleFactor,
                                        int antiAlias, bool enableDepthAndStencil)
        {
            if (!_shareContext)
            {
                enableDepthAndStencil &&= SystemUtil.supportsDepthAndStencil;

                // Changing the stage3D position might move the back buffer to invalid bounds
                // temporarily. To avoid problems, we set it to the smallest possible size first.

                if (_context.profile == "baselineConstrained")
                    _context.configureBackBuffer(32, 32, antiAlias, enableDepthAndStencil);

                // If supporting HiDPI mode would exceed the maximum buffer size
                // (can happen e.g in software mode), we stick to the low resolution.

                if (viewPort.Width * contentScaleFactor > _context.maxBackBufferWidth ||
                    viewPort.Height * contentScaleFactor > _context.maxBackBufferHeight)
                {
                    contentScaleFactor = 1.0f;
                }

                _stage3D.x = viewPort.X;
                _stage3D.y = viewPort.Y;

                _context.configureBackBuffer(viewPort.Width, viewPort.Height,
                    antiAlias, enableDepthAndStencil, contentScaleFactor != 1.0);
            }

            _backBufferWidth  = viewPort.Width;
            _backBufferHeight = viewPort.Height;
            _backBufferScaleFactor = contentScaleFactor;
        }

        // program management

        /** Registers a program under a certain name.
         *  If the name was already used, the previous program is overwritten. */
        public void registerProgram(string name, Program program)
        {
            DeleteProgram(name);
            programs[name] = program;
        }

        /** Deletes the program of a certain name. */
        public void DeleteProgram(string name)
        {
            Program program = GetProgram(name);
            if (program != null)
            {
                program.Dispose();
                delete programs[name];
            }
        }

        /** Returns the program registered under a certain name, or null if no program with
         *  this name has been registered. */
        public Program GetProgram(string name)
        {
            return programs[name] as Program;
        }

        /** Indicates if a program is registered under a certain name. */
        public bool HasProgram(string name)
        {
            return name in programs;
        }

        // state stack

        /** Pushes the current render state to a stack from which it can be restored later.
         *
         *  <p>If you pass a BatchToken, it will be updated to point to the current location within
         *  the render cache. That way, you can later reference this location to render a subset of
         *  the cache.</p>
         */
        public void PushState(BatchToken token = null)
        {
            _stateStackPos++;

            if (_stateStackLength < _stateStackPos + 1) _stateStack[_stateStackLength++] = new RenderState();
            if (token != null) _batchProcessor.FillToken(token);

            _stateStack[_stateStackPos].CopyFrom(_state);
        }

        /** Modifies the current state with a transformation matrix, alpha factor, and blend mode.
         *
         *  @param transformationMatrix Used to transform the current <code>modelviewMatrix</code>.
         *  @param alphaFactor          Multiplied with the current alpha value.
         *  @param blendMode            Replaces the current blend mode; except for "auto", which
         *                              means the current value remains unchanged.
         */
        public void SetStateTo(Matrix transformationMatrix, float alphaFactor= 1.0f,
                               uint blendMode = BlendMode.AUTO)
        {
            if (transformationMatrix != null) MatrixUtil.PrependMatrix(_state._modelviewMatrix, transformationMatrix);
            if (alphaFactor != 1.0) _state._alpha *= alphaFactor;
            if (blendMode != BlendMode.AUTO) _state.BlendMode = blendMode;
        }

        /** Restores the render state that was last pushed to the stack. If this changes
        *  blend mode, clipping rectangle, render target or culling, the current batch
        *  will be drawn right away.
        *
        *  <p>If you pass a BatchToken, it will be updated to point to the current location within
        *  the render cache. That way, you can later reference this location to render a subset of
        *  the cache.</p>
        */
        public void popState(BatchToken token = null)
        {
            if (_stateStackPos < 0)
            {
                throw new IndexOutOfRangeException("Cannot pop empty state stack");
            }
            _state.CopyFrom(_stateStack[_stateStackPos]); // -> might cause 'finishMeshBatch'
            _stateStackPos--;
            if (token != null)
            {
                _batchProcessor.FillToken(token);
            }
        }

        // masks

        // TODO
    }
}
