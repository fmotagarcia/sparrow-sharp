
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Utils;
using System;
using System.Collections.Generic;
using Sparrow.Textures;
using System.Diagnostics;
using OpenGL;

namespace Sparrow.Rendering
{

    /// <summary>
    /// A class that orchestrates rendering of all Sparrow display objects.
    ///
    /// <para>A Sparrow instance contains exactly one 'Painter' instance that should be used for all
    /// rendering purposes. Each frame, it is passed to the render methods of all rendered display
    /// objects. To access it outside a render method, call <code>Sparrow.Painter</code>.</para>
    ///
    /// <strong>The State Stack</strong>
    /// 
    /// <para>The most important concept of the Painter class is the state stack. A RenderState
    /// stores a combination of settings that are currently used for rendering, e.g. the current
    /// projection- and modelview-matrices and context-related settings. It can be accessed
    /// and manipulated via the <code>State</code> property. Use the methods
    /// <code>PushState</code> and <code>PopState</code> to store a specific state and restore
    /// it later. That makes it easy to write rendering code that doesn't have any side effects.</para>
    /// 
    /// <code>
    /// painter.PushState(); // save a copy of the current state on the stack
    /// painter.State.RenderTarget = renderTexture;
    /// painter.State.TransformModelviewMatrix(object.TransformationMatrix);
    /// painter.State.Alpha = 0.5;
    /// painter.PrepareToDraw(); // apply all state settings at the render context
    /// DrawSomething(); // insert OpenGL rendering code here
    /// painter.PopState(); // restores previous state
    /// </code>
    /// 
    /// <see cref="RenderState"/>
    /// </summary>
    public class Painter
    {
        
        private Dictionary<string, Program> _programs;
        private readonly Dictionary<uint, uint> _framebufferCache;

        private int _drawCount;
        private uint _frameId;
        private float _pixelSize;
        private readonly Stack<Rectangle> _clipRectStack;
        private readonly List<DisplayObject> _batchCacheExclusions;

        private BatchProcessor _batchProcessor;
        private BatchProcessor _batchProcessorCurr; // current  processor
        private BatchProcessor _batchProcessorPrev; // previous processor (cache)
        private readonly BatchProcessor _batchProcessorSpec; // special  processor (no cache)

        private uint _actualRenderTarget;
        private uint _actualBlendMode;

        private float _backBufferWidth;
        private float _backBufferHeight;

        private readonly RenderState _state;
        private readonly List<RenderState> _stateStack;
        private int _stateStackPos;
        private int _stateStackLength;

        // helper objects
        private static readonly Matrix2D sMatrix = Matrix2D.Create();
        private static Rectangle sClipRect = Rectangle.Create();
        private static readonly Rectangle sBufferRect = Rectangle.Create();
        private static Rectangle sScissorRect = Rectangle.Create();
        private static readonly MeshSubset sMeshSubset = new MeshSubset();
        
        /// <summary>
        /// Creates a new Painter object. Normally, it's not necessary to create any custom
        ///  painters; instead, use the global painter found on the Sparrow instance.
        /// </summary>
        public Painter(float width, float height)
        {
            _framebufferCache = new Dictionary<uint, uint>();
            _actualBlendMode = 0;

            _backBufferWidth = width;
            _backBufferHeight = height;
            _pixelSize = 1.0f;
            _clipRectStack = new Stack<Rectangle>();

            _batchProcessorCurr = new BatchProcessor();
            _batchProcessorCurr.OnBatchComplete = DrawBatch;
            
            _batchProcessorPrev = new BatchProcessor();
            _batchProcessorPrev.OnBatchComplete = DrawBatch;

            _batchProcessorSpec = new BatchProcessor();
            _batchProcessorSpec.OnBatchComplete = DrawBatch;
            
            _batchProcessor = _batchProcessorCurr;
            _batchCacheExclusions = new List<DisplayObject>();

            _state = new RenderState();
            _state.OnDrawRequired = FinishMeshBatch;
            _stateStack = new List<RenderState>();
            _stateStackPos = -1;
            _stateStackLength = 0;
        }

        /// <summary>
        /// Disposes all mesh batches, programs, and the OpenGL render context.
        /// </summary>
        public void Dispose()
        {
            _batchProcessorCurr.Dispose();
            _batchProcessorPrev.Dispose();
            _batchProcessorSpec.Dispose();
            // + dispose GL context?
        }

        // context handling

        /** Sets the viewport dimensions and other attributes of the rendering buffer.
         *  Sparrow will call this method internally, so most apps won't need to mess with this.
         *
         * @param viewPort                the position and size of the area that should be rendered
         *                                into, in pixels.
         */
        public void ConfigureBackBuffer(Rectangle viewPort)
        {
            _backBufferWidth  = viewPort.Width;
            _backBufferHeight = viewPort.Height;
            Gl.Viewport(0, 0, (int)_backBufferWidth, (int)_backBufferHeight);
        }

        // program management

        /// <summary>
        /// Registers a program under a certain name.
        /// If the name was already used, the previous program is overwritten.
        /// </summary>
        public void RegisterProgram(string name, Program program)
        {
            DeleteProgram(name);
            Programs.Add(name, program);
        }

        /// <summary>
        /// Deletes the program of a certain name.
        /// </summary>
        public void DeleteProgram(string name)
        {
            Program program = GetProgram(name);
            if (program != null)
            {
                program.Dispose();
                Programs.Remove(name);
            }
        }

        /// <summary>
        /// Returns the program registered under a certain name, or null if no program with
        ///  this name has been registered.
        /// </summary>
        public Program GetProgram(string name)
        {
            Programs.TryGetValue(name, out var ret);
            return ret;
        }

        /// <summary>
        /// Indicates if a program is registered under a certain name.
        /// </summary>
        public bool HasProgram(string name)
        {
            return Programs.ContainsKey(name);
        }

        // state stack

        /// <summary>
        /// Pushes the current render state to a stack from which it can be restored later.
        ///
        /// <para>If you pass a BatchToken, it will be updated to point to the current location within
        /// the render cache. That way, you can later reference this location to render a subset of
        /// the cache.</para>
        /// </summary>
        public void PushState(BatchToken token = null)
        {
            _stateStackPos++;

            if (_stateStackLength < _stateStackPos + 1)
            {
                _stateStackLength++;
                _stateStack.Add(new RenderState());
            }
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
        public void SetStateTo(Matrix2D transformationMatrix, float alphaFactor = 1.0f,
                               uint blendMode = BlendMode.AUTO)
        {
            if (transformationMatrix != null) _state._modelviewMatrix.PrependMatrix(transformationMatrix);
            if (alphaFactor != 1.0f) _state.Alpha *= alphaFactor;
            if (blendMode != BlendMode.AUTO) _state.BlendMode = blendMode;
        }

        /// <summary>
        /// Restores the render state that was last pushed to the stack. If this changes
        /// blend mode, clipping rectangle, render target or culling, the current batch
        /// will be drawn right away.
        ///
        /// <para>If you pass a BatchToken, it will be updated to point to the current location within
        /// the render cache. That way, you can later reference this location to render a subset of
        /// the cache.</para>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">if the state stack is empty</exception>
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
        *  is of type <code>Sparrow.Display.Quad</code> and is aligned parallel to the stage
        *  axes.</p>
        *
        *  <p>Note that masking breaks the render cache; the masked object must be redrawn anew
        *  in the next frame. If you pass <code>maskee</code>, the method will automatically
        *  call <code>ExcludeFromCache(maskee)</code> for you.</p>
        */
        public void DrawMask(DisplayObject mask, DisplayObject maskee = null)
        {
            FinishMeshBatch();

            if (IsRectangularMask(mask, sMatrix))
            {
                sClipRect = mask.GetBounds(mask);
                sClipRect = sClipRect.GetBounds(sMatrix);
                PushClipRect(sClipRect);
            }
            else
            {
                throw new NotImplementedException();
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
           
            FinishMeshBatch();

            if (IsRectangularMask(mask, sMatrix))
            {
                PopClipRect();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void PushClipRect(Rectangle clipRect)
        {
            Rectangle intersection;
            if (_clipRectStack.Count != 0)
            {
                intersection = clipRect.Intersection(_clipRectStack.Peek());
            }
            else
            {
                intersection = clipRect.Clone();
            }
            _clipRectStack.Push(intersection);
            _state.ClipRect = intersection;
        }

        private void PopClipRect()
        {
            int stackLength = _clipRectStack.Count;
            if (stackLength == 0)
            {
                throw new Exception("Trying to pop from empty clip rectangle stack");
            }
            stackLength--;
            _clipRectStack.Pop();
            _state.ClipRect = stackLength != 0 ? _clipRectStack.Peek() : null;
        }

        /// <summary>
        /// Figures out if the mask can be represented by a scissor rectangle; this is possible
        /// if it's just a simple (untextured) quad that is parallel to the stage axes. The 'out'
        /// parameter will be filled with the transformation matrix required to move the mask into
        /// stage coordinates.
        /// </summary>
        private bool IsRectangularMask(DisplayObject mask, Matrix2D outMatrix)
        {
            Quad quad = mask as Quad;

            if (quad != null && quad.Texture == null)
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
            if (_frameId % 99 == 0) _batchProcessorCurr.Trim(); // odd number -> alternating processors
            if (_frameId % 150 == 0) _batchProcessorSpec.Trim();

            _batchProcessor.FinishBatch();
            _batchProcessor = _batchProcessorSpec; // no cache between frames
            ProcessCacheExclusions();
        }

        private void ProcessCacheExclusions()
        {
            int i;
            int length = _batchCacheExclusions.Count;
            for (i = 0; i < length; ++i) _batchCacheExclusions[i].ExcludeFromCache();
            _batchCacheExclusions.Clear();
        }


        /** Resets the current state, state stack, batch processor, stencil reference value,
         *  clipping rectangle, and draw count. Furthermore, depth testing is disabled. */
        public void NextFrame()
        {
            // update batch processors
            _batchProcessor = SwapBatchProcessors();
            _batchProcessor.Clear();
            _batchProcessorSpec.Clear();

            // enforce reset of basic context settings
            _actualBlendMode = 0;
            Gl.DepthFunc(DepthFunction.Always);

            // reset everything else
            _clipRectStack.Clear();
            _drawCount = 0;
            _stateStackPos = -1;
            _state.Reset();
        }

        private BatchProcessor SwapBatchProcessors()
        {
            BatchProcessor tmp = _batchProcessorPrev;
            _batchProcessorPrev = _batchProcessorCurr;
            return _batchProcessorCurr = tmp;
        }
        /** Draws all meshes from the render cache between <code>startToken</code> and
         *  (but not including) <code>endToken</code>. The render cache contains all meshes
         *  rendered in the previous frame. */
        public void DrawFromCache(BatchToken startToken, BatchToken endToken)
        {
            MeshSubset subset = sMeshSubset;

            if (!startToken.Equals(endToken))
            {
                PushState();

                for (int i = startToken.BatchID; i <= endToken.BatchID; ++i)
                {
                    var meshBatch = _batchProcessorPrev.GetBatchAt(i);
                    subset.SetTo(); // resets subset

                    if (i == startToken.BatchID)
                    {
                        subset.VertexId = startToken.VertexID;
                        subset.IndexId  = startToken.IndexID;
                        subset.NumVertices = meshBatch.NumVertices - subset.VertexId;
                        subset.NumIndices  = meshBatch.NumIndices  - subset.IndexId;
                    }

                    if (i == endToken.BatchID)
                    {
                        subset.NumVertices = endToken.VertexID - subset.VertexId;
                        subset.NumIndices  = endToken.IndexID  - subset.IndexId;
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

        /// <summary>
        /// Prevents the object from being drawn from the render cache in the next frame.
        /// Different to <code>SetRequiresRedraw()</code>, this does not indicate that the object
        /// has changed in any way, but just that it doesn't support being drawn from cache.
        ///
        /// <para>Note that when a container is excluded from the render cache, its children will
        /// still be cached! This just means that batching is interrupted at this object when
        /// the display tree is traversed.</para>
        /// </summary>
        public void ExcludeFromCache(DisplayObject obj)
        {
            if (obj != null)
            {
                _batchCacheExclusions.Add(obj);
            }
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

        /// <summary>
        /// Applies all relevant state settings to at the render context. This includes
        /// blend mode, render target and clipping rectangle. Always call this method before
        /// <code>Gl.DrawElements()</code>.
        /// </summary>
        public void PrepareToDraw()
        {
            ApplyBlendMode();
            ApplyRenderTarget();
            ApplyClipRect();
        }

        /** Clears the render context with a certain color and alpha value. Since this also
        *  clears the stencil buffer, the stencil reference value is also reset to '0'. */
        public void Clear(uint rgb = 0, float alpha= 0.0f)
        {
            ApplyRenderTarget();

            float red = ColorUtil.GetR(rgb) / 255.0f;
            float green = ColorUtil.GetG(rgb) / 255.0f;
            float blue = ColorUtil.GetB(rgb) / 255.0f;
            Gl.ClearColor(red, green, blue, alpha);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);
    }

        /// <summary>
        /// Resets the render target to the back buffer
        /// </summary>
        public void Present()
        {
            _state.RenderTarget = null;
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

        private void ApplyRenderTarget()
        {
            uint target = _state.RenderTargetBase;
            if (target != _actualRenderTarget)
            {
                if (target != 0)
                {
                    // TODO set this uint antiAlias  = _state.RenderTargetAntiAlias;
                    if (!_framebufferCache.TryGetValue(target, out var framebuffer))
                    {
                        uint[] fb = new uint[1];
                        Gl.GenFramebuffers(fb);
                        framebuffer = fb[0];
                        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

                        Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, target, 0);

                        if (Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferStatus.FramebufferComplete)
                        {
                            Debug.WriteLine("Failed to create framebuffer for render texture");
                        }
                        _framebufferCache.Add(target, framebuffer);
                    }
                    else
                    {
                        if (framebuffer == 0)
                        {
                            throw new InvalidOperationException("Attempted to bind framebuffer ID 0");
                        }
                        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                    }
                    Gl.Viewport(0, 0, (int)_state.RenderTarget.NativeWidth, (int)_state.RenderTarget.NativeHeight);
                }
                else
                {
                    // TODO: double check these on a device, the ifdef seems to be unneeded
#if __IOS__
                    Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
#else
                    Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
#endif
                    Gl.Viewport(0, 0, (int)_backBufferWidth, (int)_backBufferHeight);
                }
                _actualRenderTarget = target;
            }
        }

        public void DestroyFramebufferForTexture(Texture texture)
        {
            if (_framebufferCache.TryGetValue(texture.Base, out var framebuffer))
            {
                Gl.DeleteFramebuffers(framebuffer);
                _framebufferCache.Remove(texture.Base);
            }
        }

        private void ApplyClipRect() // used by rectangular masks & render textures
        {
            Rectangle clipRect = _state.ClipRect;

            if (clipRect != null)
            {
                float width, height;
                Matrix3D projMatrix = _state.ProjectionMatrix3D;
                Texture renderTarget = _state.RenderTarget;

                if (renderTarget != null)
                {
                    width = renderTarget.Root.NativeWidth;
                    height = renderTarget.Root.NativeHeight;
                }
                else
                {
                    width = _backBufferWidth;
                    height = _backBufferHeight;
                }

                // convert to pixel coordinates (matrix transformation ends up in range [-1, 1])
                float[] sPoint3D = projMatrix.TransformCoords3D(clipRect.X, clipRect.Y, 0.0f);
                MathUtil.ProjectVector3D(ref sPoint3D); // eliminate w-coordinate
                sClipRect.X = (sPoint3D[0] * 0.5f + 0.5f) * width;
                sClipRect.Y = (0.5f - sPoint3D[1] * 0.5f) * height;

                sPoint3D = projMatrix.TransformCoords3D(clipRect.Right, clipRect.Bottom, 0.0f);
                MathUtil.ProjectVector3D(ref sPoint3D); // eliminate w-coordinate
                sClipRect.Right = (sPoint3D[0] * 0.5f + 0.5f) * width;
                sClipRect.Bottom = (0.5f - sPoint3D[1] * 0.5f) * height;

                if (renderTarget == null)
                { 
                    // OpenGL positions the scissor rectangle from the bottom of the screen :(
                    // flip it, if we're rendering to the backbuffer
                    sClipRect.Y = (int)(_backBufferHeight - sClipRect.Height - sClipRect.Y);
                }
                sBufferRect.SetTo(0, 0, width, height);
                sScissorRect = sClipRect.Intersection(sBufferRect);

                // an empty rectangle is not allowed, so we set it to the smallest possible size
                if (sScissorRect.Width < 1f || sScissorRect.Height < 1f)
                {
                    sScissorRect.SetTo(0, 0, 1, 1);
                    Debug.WriteLine("WARNING: Clip rectangle has zero size, setting it to 1x1");
                }
                Gl.Enable(EnableCap.ScissorTest);
                Gl.Clear(ClearBufferMask.StencilBufferBit);
                Gl.Scissor((int)sScissorRect.X, (int)sScissorRect.Y, (int)sScissorRect.Width, (int)sScissorRect.Height);
            }
            else
            {       
                if (Gl.IsEnabled(EnableCap.ScissorTest))
                {
                    Gl.Disable(EnableCap.ScissorTest);
                }
            }
        }

        // properties

        /** Indicates the number of stage3D draw calls. */
        public int DrawCount
        {
            set { _drawCount = value; }
            get { return _drawCount; }
        }

        /** Indicates if the render cache is enabled. Normally, this should be left at the default;
         *  however, some custom rendering logic might require to change this property temporarily.
         *  Also note that the cache is automatically reactivated each frame, right before the
         *  render process.
         *
         *  @default true
         */
        public bool CacheEnabled {
            get { return _batchProcessor == _batchProcessorCurr; }
            set
            {
                if (value != CacheEnabled)
                {
                    FinishMeshBatch();

                    if (value) _batchProcessor = _batchProcessorCurr;
                    else _batchProcessor = _batchProcessorSpec;
                }
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

        /** Returns the index of the current frame <strong>if</strong> the render cache is enabled;
         *  otherwise, returns zero. To get the frameID regardless of the render cache, call
         *  <code>Sparrow.frameID</code> instead. */
        public uint FrameID {
            get { return _batchProcessor == _batchProcessorCurr ? _frameId : 0; }
            set { _frameId = value;  }
        }

        /** The size (in points) that represents one pixel in the back buffer. */
        public float PixelSize
        {
            get { return _pixelSize; }
            set { _pixelSize = value; }
        }

        /** Returns the current width of the back buffer. In most cases, this value is in pixels;
         *  however, if the app is running on an HiDPI display with an activated
         *  'supportHighResolutions' setting, you have to multiply with 'backBufferPixelsPerPoint'
         *  for the actual pixel count. Alternatively, use the Context3D-property with the
         *  same name: it will return the exact pixel values. */
        public float BackBufferWidth { get { return _backBufferWidth; } }

        /** Returns the current height of the back buffer. In most cases, this value is in pixels;
         *  however, if the app is running on an HiDPI display with an activated
         *  'supportHighResolutions' setting, you have to multiply with 'backBufferPixelsPerPoint'
         *  for the actual pixel count. Alternatively, use the Context3D-property with the
         *  same name: it will return the exact pixel values. */
        public float BackBufferHeight { get { return _backBufferHeight; } }

        private Dictionary<string, Program> Programs {
            get
            {
                if (_programs == null)
                {
                    _programs = new Dictionary<string, Program>();
                }
                return _programs;
            }
        }
    }
}
