using Sparrow.Core;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;
using SparrowSharp.Core.Display;
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

        // members
        private Dictionary<string, Program> programs;

        private Stage3D _stage3D;
        private Context3D _context;
        private bool _shareContext;
        private int _drawCount;
        private uint _frameID;
        private float _pixelSize;
        private bool _enableErrorChecking;
        private Dictionary<DisplayObject, uint> _stencilReferenceValues;
        private List<Rectangle> _clipRectStack;
        private BatchProcessor _batchProcessor;
        private BatchProcessor _batchCache;
        private List<DisplayObject> _batchCacheExclusions;

        private TextureBase _actualRenderTarget;
        private string _actualCulling;
        private uint _actualBlendMode;

        private float _backBufferWidth;
        private float _backBufferHeight;

        private RenderState _state;
        private List<RenderState> _stateStack;
        private int _stateStackPos;
        private int _stateStackLength;

        // helper objects
        private static Matrix sMatrix = Matrix.Create();
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
            _pixelSize = 1.0f;
            _stencilReferenceValues = new Dictionary<DisplayObject, uint>(); // use weak refs!
            _clipRectStack = new List<Rectangle>();

            _batchProcessor = new BatchProcessor();
            _batchProcessor.OnBatchComplete = DrawBatch;

            _batchCache = new BatchProcessor();
            _batchCache.OnBatchComplete = DrawBatch;
            _batchCacheExclusions = new List<DisplayObject>();

            _state = new RenderState();
            _state._onDrawRequired = FinishMeshBatch;
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
        public void RequestContext3D(string renderMode, string profile)
        {
            RenderUtil.RequestContext3D(_stage3D, renderMode, profile);
        }

        private void onContextCreated(Object eventObj)
        {
            _context = _stage3D.context3D;
            _context.enableErrorChecking = _enableErrorChecking;
            _actualBlendMode = 0;
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
         */
        public void ConfigureBackBuffer(Rectangle viewPort)
        {
            _backBufferWidth  = viewPort.Width;
            _backBufferHeight = viewPort.Height;
        }

        // program management

        /** Registers a program under a certain name.
         *  If the name was already used, the previous program is overwritten. */
        public void registerProgram(string name, Program program)
        {
            DeleteProgram(name);
            Programs.Add(name, program);
        }

        /** Deletes the program of a certain name. */
        public void DeleteProgram(string name)
        {
            Program program = GetProgram(name);
            if (program != null)
            {
                program.Dispose();
                Programs.Remove(name);
            }
        }

        /** Returns the program registered under a certain name, or null if no program with
         *  this name has been registered. */
        public Program GetProgram(string name)
        {
            return Programs[name] as Program;
        }

        /** Indicates if a program is registered under a certain name. */
        public bool HasProgram(string name)
        {
            return Programs.ContainsKey(name);
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
            if (transformationMatrix != null) _state._modelviewMatrix.PrependMatrix(transformationMatrix);
            if (alphaFactor != 1.0) _state.Alpha *= alphaFactor;
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
        public void PopState(BatchToken token = null)
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

        /** Draws a display object into the stencil buffer, incrementing the buffer on each
        *  used pixel. The stencil reference value is incremented as well; thus, any subsequent
        *  stencil tests outside of this area will fail.
        *
        *  <p>If 'mask' is part of the display list, it will be drawn at its conventional stage
        *  coordinates. Otherwise, it will be drawn with the current modelview matrix.</p>
        *
        *  <p>As an optimization, this method might update the clipping rectangle of the render
        *  state instead of utilizing the stencil buffer. This is possible when the mask object
        *  is of type <code>starling.display.Quad</code> and is aligned parallel to the stage
        *  axes.</p>
        *
        *  <p>Note that masking breaks the render cache; the masked object must be redrawn anew
        *  in the next frame. If you pass <code>maskee</code>, the method will automatically
        *  call <code>excludeFromCache(maskee)</code> for you.</p>
        */
        public void DrawMask(DisplayObject mask, DisplayObject maskee = null)
        {
            if (_context == null) return;

            FinishMeshBatch();

            if (IsRectangularMask(mask, maskee, sMatrix))
            {
                sClipRect = mask.GetBounds(mask);
                sClipRect = sClipRect.GetBounds(sMatrix);
                PushClipRect(sClipRect);
            }
            else
            {
                _context.setStencilActions(Context3DTriangleFace.FRONT_AND_BACK,
                    Context3DCompareMode.EQUAL, Context3DStencilAction.INCREMENT_SATURATE);

                RenderMask(mask);
                StencilReferenceValue++;

                _context.setStencilActions(Context3DTriangleFace.FRONT_AND_BACK,
                    Context3DCompareMode.EQUAL, Context3DStencilAction.KEEP);
            }

            ExcludeFromCache(maskee);
        }

        /** Draws a display object into the stencil buffer, decrementing the
         *  buffer on each used pixel. This effectively erases the object from the stencil buffer,
         *  restoring the previous state. The stencil reference value will be decremented.
         *
         *  <p>Note: if the mask object meets the requirements of using the clipping rectangle,
         *  it will be assumed that this erase operation undoes the clipping rectangle change
         *  caused by the corresponding <code>drawMask()</code> call.</p>
         */
        public void EraseMask(DisplayObject mask, DisplayObject maskee = null)
        {
            if (_context == null) return;

            FinishMeshBatch();

            if (IsRectangularMask(mask, maskee, sMatrix))
            {
                PopClipRect();
            }
            else
            {
                _context.setStencilActions(Context3DTriangleFace.FRONT_AND_BACK,
                    Context3DCompareMode.EQUAL, Context3DStencilAction.DECREMENT_SATURATE);

                RenderMask(mask);
                StencilReferenceValue--;

                _context.setStencilActions(Context3DTriangleFace.FRONT_AND_BACK,
                    Context3DCompareMode.EQUAL, Context3DStencilAction.KEEP);
            }
        }

        private void RenderMask(DisplayObject mask)
        {
            PushState();
            _state.Alpha = 0.0f;

            Matrix matrix = null;

            if (mask.Stage != null)
            {
                _state.SetModelviewMatricesToIdentity();
                matrix   = mask.GetTransformationMatrix(null);
            }
            else
            {
                matrix   = mask.TransformationMatrix;
            }

            _state.TransformModelviewMatrix(matrix);

            mask.Render(this);
            FinishMeshBatch();

            PopState();
        }

        private void PushClipRect(Rectangle clipRect)
        {
            List<Rectangle> stack = _clipRectStack;
            int stackLength = stack.Count;
            Rectangle intersection = new Rectangle(); // TODO add pooling

            if (stackLength != 0)
                intersection = clipRect.Intersection(stack[stackLength - 1]);
            else
                intersection.CopyFrom(clipRect);

            stack[stackLength] = intersection;
            _state.ClipRect = intersection;
        }

        private void PopClipRect()
        {
            List<Rectangle> stack = _clipRectStack;
            int stackLength = stack.Count;

            if (stackLength == 0)
                throw new Exception("Trying to pop from empty clip rectangle stack");

            stackLength--;
            stack.RemoveAt(stack.Count - 1);
            _state.ClipRect = stackLength != 0 ? stack[stackLength - 1] : null;
        }

        /** Figures out if the mask can be represented by a scissor rectangle; this is possible
        *  if it's just a simple (untextured) quad that is parallel to the stage axes. The 'out'
        *  parameter will be filled with the transformation matrix required to move the mask into
        *  stage coordinates. */
        private bool IsRectangularMask(DisplayObject mask, DisplayObject maskee, Matrix outMatrix)
        {
            Quad quad = mask as Quad;
            bool is3D = (maskee != null && mask.Stage == null);

            if (quad != null && !is3D && quad.Texture == null)
            {
                if (mask.Stage != null) outMatrix = mask.GetTransformationMatrix(null);
                else
                {
                    outMatrix.CopyFromMatrix(mask.TransformationMatrix);
                    outMatrix.AppendMatrix(_state.ModelviewMatrix);
                }
                return (MathUtil.Equals(outMatrix.A, 0f) && MathUtil.Equals(outMatrix.D, 0f)) ||
                       (MathUtil.Equals(outMatrix.B, 0f) && MathUtil.Equals(outMatrix.C, 0f));
            }
            return false;
        }

        // mesh rendering

        /** Adds a mesh to the current batch of unrendered meshes. If the current batch is not
         *  compatible with the mesh, all previous meshes are rendered at once and the batch
         *  is cleared.
         *
         *  @param mesh    The mesh to batch.
         *  @param subset  The range of vertices to be batched. If <code>null</code>, the complete
         *                 mesh will be used.
         */
        public void BatchMesh(Mesh mesh, MeshSubset subset= null)
        {
            _batchProcessor.AddMesh(mesh, _state, subset);
        }

        /** Finishes the current mesh batch and prepares the next one. */
        public void FinishMeshBatch()
        {
            _batchProcessor.FinishBatch();
        }

        /** Completes all unfinished batches, cleanup procedures. */
        public void FinishFrame()
        {
            if (_frameID % 99 == 0) // odd number -> alternating processors
                _batchProcessor.Trim();

            _batchProcessor.FinishBatch();
            SwapBatchProcessors();
            _batchProcessor.Clear();
            ProcessCacheExclusions();
        }

        private void SwapBatchProcessors()
        {
            BatchProcessor tmp = _batchProcessor;
            _batchProcessor = _batchCache;
            _batchCache = tmp;
        }

        private void ProcessCacheExclusions()
        {
            int i;
            int length = _batchCacheExclusions.Count;
            for (i=0; i<length; ++i) _batchCacheExclusions[i].ExcludeFromCache();
            _batchCacheExclusions.Clear();
        }


        /** Resets the current state, state stack, batch processor, stencil reference value,
         *  clipping rectangle, and draw count. Furthermore, depth testing is disabled. */
        public void NextFrame()
        {
            // enforce reset of basic context settings
            _actualBlendMode = 0;
            _actualCulling = null;
            _context.setDepthTest(false, Context3DCompareMode.ALWAYS);

            // reset everything else
            StencilReferenceValue = 0;
            _clipRectStack.Clear();
            _drawCount = 0;
            _stateStackPos = -1;
            _batchProcessor.Clear();
            _state.Reset();
        }


        /** Draws all meshes from the render cache between <code>startToken</code> and
         *  (but not including) <code>endToken</code>. The render cache contains all meshes
         *  rendered in the previous frame. */
        public void DrawFromCache(BatchToken startToken, BatchToken endToken)
        {
            MeshBatch meshBatch;
            MeshSubset subset = sMeshSubset;

            if (!startToken.Equals(endToken))
            {
                PushState();

                for (int i = startToken.BatchID; i <= endToken.BatchID; ++i)
                {
                    meshBatch = _batchCache.GetBatchAt(i);
                    subset.SetTo(); // resets subset

                    if (i == startToken.BatchID)
                    {
                        subset.VertexID = startToken.VertexID;
                        subset.IndexID  = startToken.IndexID;
                        subset.NumVertices = meshBatch.NumVertices - subset.VertexID;
                        subset.NumIndices  = meshBatch.NumIndices  - subset.IndexID;
                    }

                    if (i == endToken.BatchID)
                    {
                        subset.NumVertices = endToken.VertexID - subset.VertexID;
                        subset.NumIndices  = endToken.IndexID  - subset.IndexID;
                    }

                    if (subset.NumVertices != 0)
                    {
                        _state.Alpha = 1.0f;
                        _state.BlendMode = meshBatch.BlendMode;
                        _batchProcessor.AddMesh(meshBatch, _state, subset, true);
                    }
                }
                PopState();
            }
        }

        /** Removes all parts of the render cache past the given token. Beware that some display
        *  objects might still reference those parts of the cache! Only call it if you know
        *  exactly what you're doing. */
        public void RewindCacheTo(BatchToken token)
        {
            _batchProcessor.RewindTo(token);
        }

        /** Prevents the object from being drawn from the render cache in the next frame.
         *  Different to <code>setRequiresRedraw()</code>, this does not indicate that the object
         *  has changed in any way, but just that it doesn't support being drawn from cache.
         *
         *  <p>Note that when a container is excluded from the render cache, its children will
         *  still be cached! This just means that batching is interrupted at this object when
         *  the display tree is traversed.</p>
         */
        public void ExcludeFromCache(DisplayObject obj)
        {
            if (obj != null) _batchCacheExclusions[_batchCacheExclusions.Count] = obj;
        }

        private void DrawBatch(MeshBatch meshBatch)
        {
            PushState();

            State.BlendMode = meshBatch.BlendMode;
            State.ModelviewMatrix.Identity();
            State.Alpha = 1.0f;

            meshBatch.Render(this);

            PopState();
        }

        // helper methods

        /** Applies all relevant state settings to at the render context. This includes
         *  blend mode, render target and clipping rectangle. Always call this method before
         *  <code>context.drawTriangles()</code>.
         */
        public void PrepareToDraw()
        {
            ApplyBlendMode();
            ApplyRenderTarget();
            ApplyClipRect();
            ApplyCulling();
        }

        /** Clears the render context with a certain color and alpha value. Since this also
        *  clears the stencil buffer, the stencil reference value is also reset to '0'. */
        public void Clear(uint rgb = 0, float alpha= 0.0f)
        {
            ApplyRenderTarget();
            StencilReferenceValue = 0;
            RenderUtil.clear(rgb, alpha);
        }

        /** Resets the render target to the back buffer and displays its contents. */
        public void Present()
        {
            _state.RenderTarget = null;
            _actualRenderTarget = null;
            _context.present();
        }


        private void ApplyBlendMode()
        {
            uint blendMode = _state.BlendMode;

            if (blendMode != _actualBlendMode)
            {
                BlendMode.Get(_state.BlendMode).Activate();
                _actualBlendMode = blendMode;
            }
        }

        private void ApplyCulling()
        {
            string culling = _state.Culling;

            if (culling != _actualCulling)
            {
                _context.setCulling(culling);
                _actualCulling = culling;
            }
        }

        private void ApplyRenderTarget()
        {
            TextureBase target = _state.RenderTargetBase;

            if (target != _actualRenderTarget)
            {
                if (target)
                {
                    int antiAlias  = _state.RenderTargetAntiAlias;
                    bool depthAndStencil = _state.RenderTargetSupportsDepthAndStencil;
                    _context.setRenderToTexture(target, depthAndStencil, antiAlias);
                }
                else
                    _context.setRenderToBackBuffer();

                _context.setStencilReferenceValue(StencilReferenceValue);
                _actualRenderTarget = target;
            }
        }

        private void ApplyClipRect()
        {
            Rectangle clipRect = _state.ClipRect;

            if (clipRect != null)
            {
                int width, height;
                Matrix3D projMatrix = _state.ProjectionMatrix3D;
                Texture renderTarget = _state.RenderTarget;

                if (renderTarget != null)
                {
                    width  = renderTarget.Root.NativeWidth;
                    height = renderTarget.Root.NativeHeight;
                }
                else
                {
                    width  = _backBufferWidth;
                    height = _backBufferHeight;
                }

                // convert to pixel coordinates (matrix transformation ends up in range [-1, 1])
                MatrixUtil.TransformCoords3D(projMatrix, clipRect.X, clipRect.Y, 0.0, sPoint3D);
                sPoint3D.project(); // eliminate w-coordinate
                sClipRect.X = (sPoint3D.X * 0.5 + 0.5) * width;
                sClipRect.Y = (0.5 - sPoint3D.Y * 0.5) * height;

                MatrixUtil.TransformCoords3D(projMatrix, clipRect.Right, clipRect.Bottom, 0.0, sPoint3D);
                sPoint3D.project(); // eliminate w-coordinate
                sClipRect.Right  = (sPoint3D.x* 0.5 + 0.5) * width;
                sClipRect.Bottom = (0.5 - sPoint3D.y* 0.5) * height;

                sBufferRect.SetTo(0, 0, width, height);
                sScissorRect = sClipRect.Intersection(sBufferRect);

                // an empty rectangle is not allowed, so we set it to the smallest possible size
                if (sScissorRect.Width < 1 || sScissorRect.Height< 1)
                    sScissorRect.SetTo(0, 0, 1, 1);

                _context.setScissorRectangle(sScissorRect);
            }
            else
            {
                _context.setScissorRectangle(null);
            }
        }

        // properties

        /** Indicates the number of stage3D draw calls. */
        public int DrawCount
        {
            set { _drawCount = value; }
            get { return _drawCount; }
        }

        /** The current stencil reference value of the active render target. This value
         *  is typically incremented when drawing a mask and decrementing when erasing it.
         *  The painter keeps track of one stencil reference value per render target.
         *  Only change this value if you know what you're doing!
         */
        public uint StencilReferenceValue
        {
            get
            {
                var key = _state.RenderTarget != null ? _state.RenderTargetBase : this;
                if (_stencilReferenceValues.ContainsKey(key)) 
                {
                    return _stencilReferenceValues[key];
                }
                else return 0;
            }
            set
            {
                var key = _state.RenderTarget != null ? _state.RenderTargetBase : this;
                _stencilReferenceValues[key] = value;

                if (contextValid)
                    _context.setStencilReferenceValue(value);
            }
        }

        /** The current render state, containing some of the context settings, projection- and
         *  modelview-matrix, etc. Always returns the same instance, even after calls to "pushState"
         *  and "popState".
         *
         *  <p>When you change the current RenderState, and this change is not compatible with
         *  the current render batch, the batch will be concluded right away. Thus, watch out
         *  for changes of blend mode, clipping rectangle, render target or culling.</p>
         */
        public RenderState State { get { return _state; } }

        /** The Stage3D instance this painter renders into. */
        public Stage3D Stage3D { get { return _stage3D; } }

        /** The Context3D instance this painter renders into. */
        public Context3D Context { get { return _context; } }

        /** The number of frames that have been rendered with the current Starling instance. */
        public uint FrameID {
            get { return _frameID; }
            set { _frameID = value;  }
        }

        /** The size (in points) that represents one pixel in the back buffer. */
        public float PixelSize
        {
            get { return _pixelSize; }
            set { _pixelSize = value; }
        }

        /** Indicates if another Starling instance (or another Stage3D framework altogether)
         *  uses the same render context. @default false */
         // TODO remove
        public bool ShareContext
        {
            get { return _shareContext; }
            set { _shareContext = value; }
        }

        /** Indicates if Stage3D render methods will report errors. Activate only when needed,
         *  as this has a negative impact on performance. @default false */
        public bool EnableErrorChecking
        {
            get { return _enableErrorChecking; }
            set
            {
                _enableErrorChecking = value;
                if (_context != null) _context.enableErrorChecking = value;
            }
        }

        /** Returns the current width of the back buffer. In most cases, this value is in pixels;
         *  however, if the app is running on an HiDPI display with an activated
         *  'supportHighResolutions' setting, you have to multiply with 'backBufferPixelsPerPoint'
         *  for the actual pixel count. Alternatively, use the Context3D-property with the
         *  same name: it will return the exact pixel values. */
        public int BackBufferWidth { get { return _backBufferWidth; } }

        /** Returns the current height of the back buffer. In most cases, this value is in pixels;
         *  however, if the app is running on an HiDPI display with an activated
         *  'supportHighResolutions' setting, you have to multiply with 'backBufferPixelsPerPoint'
         *  for the actual pixel count. Alternatively, use the Context3D-property with the
         *  same name: it will return the exact pixel values. */
        public int BackBufferHeight { get { return _backBufferHeight; } }

        /** The number of pixels per point returned by the 'backBufferWidth/Height' properties.
         *  Except for desktop HiDPI displays with an activated 'supportHighResolutions' setting,
         *  this will always return '1'. */
        public float BackBufferScaleFactor { get { return _backBufferScaleFactor; } }

        private Dictionary<string, Program> Programs {
            get
            {
                if (programs == null)
                {
                    programs = new Dictionary<string, Program>();
                }
                return programs;
            }
        }
       
}
}
