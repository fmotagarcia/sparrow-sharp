using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace Sparrow.Core
{
    /// <summary>
    /// A class that contains helper methods simplifying OpenGL rendering.
 
    /// A RenderSupport instance is passed to any render: method. It saves information about the
    /// current render state, like the alpha value, modelview matrix, and blend mode.
 
    /// It also keeps a list of quad batches, which can be used to render a high number of quads
    /// very efficiently; only changes in the state of added quads trigger OpenGL draw calls.
    /// </summary>
    public class RenderSupport
    {
        private readonly Matrix _projectionMatrix;
        private readonly Matrix _mvpMatrix;
        private int _numDrawCalls;
        private readonly List<RenderState> _stateStack;
        private RenderState _stateStackTop;
        private int _stateStackIndex;
        private int _stateStackSize;
        private readonly List<QuadBatch> _quadBatches;
        private QuadBatch _quadBatchTop;
        private int _quadBatchIndex;
        private int _quadBatchSize;
        private readonly List<Rectangle> _clipRectStack;
        private int _clipRectStackSize;

        /// <summary>
        /// Returns the current projection matrix.
        /// CAUTION: Use with care! Each call returns the same instance.
        /// </summary>
        public Matrix ProjectionMatrix {
            get { return _projectionMatrix; }
            set {
                _projectionMatrix.CopyFromMatrix(value);
                ApplyClipRect();
            }
        }

        /// <summary>
        /// Indicates the number of OpenGL ES draw calls since the last call to 'nextFrame'.
        /// </summary>
        public int NumDrawCalls {
            get {
                return _numDrawCalls;
            }
        }

        /// <summary>
        /// Calculates the product of modelview and projection matrix.
        /// CAUTION: Use with care! Each call returns the same instance.
        /// </summary>
        public Matrix MvpMatrix {
            get {
                _mvpMatrix.CopyFromMatrix(_stateStackTop.ModelViewMatrix);
                _mvpMatrix.AppendMatrix(_projectionMatrix);
                return _mvpMatrix;
            }
        }

        /// <summary>
        /// Returns the current modelview matrix.
        /// CAUTION: Use with care! Returns not a copy, but the internally used instance.
        /// </summary>
        public Matrix ModelViewMatrix {
            get { return _stateStackTop.ModelViewMatrix; }
        }

        /// <summary>
        ///  The current (accumulated) alpha value.
        /// </summary>
        public float Alpha {
            get { return _stateStackTop.Alpha; }
            set {
                _stateStackTop.Alpha = value;
            }
        }

        /// <summary>
        /// The current blend mode.
        /// </summary>
        public uint BlendMode {
            get { return _stateStackTop.BlendMode; }
            set {
                if (value != Sparrow.Display.BlendMode.AUTO) {
                    _stateStackTop.BlendMode = value;
                }
            }
        }

        /// <summary>
        /// The texture that is currently being rendered into, or 'null' to render into the back buffer.
        /// If you set a new target, it is immediately activated.
        /// </summary>
        public Texture RenderTarget {
            get { return SparrowSharpApp.Context.RenderTarget; }
            set {
                ApplyClipRect();
                SparrowSharpApp.Context.RenderTarget = value;
            }
        }

        public RenderSupport() {
            _projectionMatrix = Matrix.Create();
            _mvpMatrix = Matrix.Create();

            _stateStack = new List<RenderState> { new RenderState() };
            _stateStackIndex = 0;
            _stateStackSize = 1;
            _stateStackTop = _stateStack[0];

            _quadBatches = new List<QuadBatch> { new QuadBatch() };
            _quadBatchIndex = 0;
            _quadBatchSize = 1;
            _quadBatchTop = _quadBatches[0];

            _clipRectStack = new List<Rectangle>();
            _clipRectStackSize = 0;

            SetupOrthographicProjection(0, 320, 0, 480);
        }

        /// <summary>
        /// Clears all vertex and index buffers, releasing the associated memory. Useful in low-memory
        /// situations. Don't call from within a render method!
        /// </summary>
        public void PurgeBuffers() {
            _quadBatches.Clear();
            _quadBatchTop = new QuadBatch();
            _quadBatches.Add(_quadBatchTop);

            _quadBatchIndex = 0;
            _quadBatchSize = 1;
        }

        /// <summary>
        /// Clears OpenGL's color buffer with a specified color.
        /// </summary>
        public void Clear(uint color = 0, float alpha = 1.0f) {
            float red = ColorUtil.GetR(color) / 255.0f;
            float green = ColorUtil.GetG(color) / 255.0f;
            float blue = ColorUtil.GetB(color) / 255.0f;

            GL.ClearColor(red, green, blue, alpha);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        /// <summary>
        /// Checks for an OpenGL error. If there is one, it is logged an the error code is returned.
        /// </summary>
        public static uint CheckForOpenGLError() {
            ErrorCode err = (ErrorCode)GL.GetError();
            string errstr = "";
            while (err != ErrorCode.NoError) {
                errstr += "There was an OpenGL error: " + err;
                err = (ErrorCode)GL.GetError();
            }
            if (errstr != "") {
                Console.WriteLine(errstr);
            }
            return (uint)err;
        }

        /// <summary>
        /// Raises the number of draw calls by a specific value. Call this method in custom render methods
        /// to keep the statistics display in sync.
        /// </summary>
        public void AddDrawCalls(int count) {
            _numDrawCalls += count;
        }

        /// <summary>
        /// Sets up the projection matrix for ortographic 2D rendering.
        /// </summary>
        public void SetupOrthographicProjection(float left, float right, float top, float bottom) {
            _projectionMatrix.A = 2.0f / (right - left);
            _projectionMatrix.B = 0.0f;
            _projectionMatrix.C = 0.0f;
            _projectionMatrix.D = 2.0f / (top - bottom);
            _projectionMatrix.Tx = -(right + left) / (right - left);
            _projectionMatrix.Ty = -(top + bottom) / (top - bottom);

            ApplyClipRect();
        }

        /// <summary>
        /// Resets the render state stack to the default.
        /// </summary>
        public void NextFrame() {
            _clipRectStackSize = 0;
            _stateStackIndex = 0;
            _quadBatchIndex = 0;
            _numDrawCalls = 0;
            _quadBatchTop = _quadBatches[0];
            _stateStackTop = _stateStack[0];
        }

        /// <summary>
        /// Adds a quad or image to the current batch of unrendered quads. If there is a state change,
        /// all previous quads are rendered at once, and the batch is reset. Note that the values for
        /// alpha and blend mode are taken from the current render state, not the quad.
        /// </summary>
        public void BatchQuad(Quad quad) {
            float alpha = _stateStackTop.Alpha;
            uint blendMode = _stateStackTop.BlendMode;
            Matrix modelViewMatrix = _stateStackTop.ModelViewMatrix;

            if (_quadBatchTop.IsStateChange(quad.Tinted, quad.Texture, alpha, quad.PremultipliedAlpha, blendMode, 1)) {
                FinishQuadBatch();
            }

            _quadBatchTop.AddQuad(quad, alpha, blendMode, modelViewMatrix);
        }

        /// <summary>
        /// Renders the current quad batch and resets it.
        /// </summary>
        public void FinishQuadBatch() {
            if (_quadBatchTop.NumQuads != 0) {
                _quadBatchTop.Render(_projectionMatrix);
                _quadBatchTop.Reset();

                if (_quadBatchSize == _quadBatchIndex + 1) {
                    _quadBatches.Add(new QuadBatch());
                    _quadBatchSize++;
                }

                _numDrawCalls++;
                _quadBatchTop = _quadBatches[++_quadBatchIndex];
            }
        }

        /// <summary>
        /// Adds a new render state to the stack. The passed matrix is prepended to the modelview matrix;
        /// the alpha value is multiplied with the current alpha; the blend mode replaces the existing
        /// mode (except 'BlendMode.Auto', which will cause the current mode to prevail).
        /// </summary>
        public void PushState(Matrix matrix, float alpha, uint blendMode) {
            RenderState previousState = _stateStackTop;

            if (_stateStackSize == _stateStackIndex + 1) {
                _stateStack.Add(new RenderState());
                _stateStackSize++;
            }

            _stateStackTop = _stateStack[++_stateStackIndex];
            _stateStackTop.Setup(previousState, matrix, alpha, blendMode);
        }

        /// <summary>
        /// Restores the previous render state.
        /// </summary>
        public void PopState() {
            if (_stateStack.Count == 0) {
                throw new InvalidOperationException("The state stack must not be empty");
            }

            _stateStackTop = _stateStack[--_stateStackIndex];
        }

        /// <summary>
        ///  Activates the current blend mode.
        /// </summary>
        public void ApplyBlendMode(bool premultipliedAlpha) {
            Sparrow.Display.BlendMode.ApplyBlendFactors(_stateStackTop.BlendMode, premultipliedAlpha);
        }

        /// <summary>
        /// The clipping rectangle can be used to limit rendering in the current render target to a certain
        /// area. This method expects the rectangle in stage coordinates. Internally, it uses the
        /// 'glScissor' command of OpenGL, which works with pixel coordinates. Any pushed rectangle is
        /// intersected with the previous rectangle; the method returns that intersection.
        /// </summary>
        public Rectangle PushClipRect(Rectangle clipRect) {
            if (_clipRectStack.Count < _clipRectStackSize + 1) {
                _clipRectStack.Add(new Rectangle());
            }
            Rectangle rectangle = _clipRectStack[_clipRectStackSize];
            rectangle.CopyFromRectangle(clipRect);

            if (_clipRectStackSize > 0) {
                rectangle = rectangle.Intersection(_clipRectStack[_clipRectStackSize - 1]);
            }

            _clipRectStackSize++;
            ApplyClipRect();

            return rectangle;
        }

        /// <summary>
        /// Restores the clipping rectangle that was last pushed to the stack.
        /// </summary>
        public void PopClipRect() {
            if (_clipRectStackSize > 0) {
                _clipRectStackSize--;
                ApplyClipRect();
            }
        }

        /// <summary>
        /// Updates the scissor rectangle using the current clipping rectangle. This method is called
        /// automatically when either the projection matrix or the clipping rectangle changes.
        /// </summary>
        private void ApplyClipRect() {
            FinishQuadBatch();

            Context context = SparrowSharpApp.Context;
            if (context == null) {
                return;
            }

            if (_clipRectStackSize > 0) {
                int width;
                int height;
                Rectangle rect = _clipRectStack[_clipRectStackSize - 1];
                Rectangle clipRect = new Rectangle();
                Texture renderTarget = context.RenderTarget;

                if (renderTarget != null) {
                    width = (int)renderTarget.NativeWidth;
                    height = (int)renderTarget.NativeHeight;
                } else {
                    width = SparrowSharpApp.DrawableWidth;
                    height = SparrowSharpApp.DrawableHeight;
                }

                // convert to pixel coordinates (matrix transformation ends up in range [-1, 1])
                Point topLeft = _projectionMatrix.TransformPoint(rect.X, rect.Y);
                if (renderTarget != null) {
                    topLeft.Y = -topLeft.Y;
                }
                clipRect.X = (topLeft.X * 0.5f + 0.5f) * width;
                clipRect.Y = (0.5f - topLeft.Y * 0.5f) * height;

                Point bottomRight = _projectionMatrix.TransformPoint(rect.Right, rect.Bottom);
                if (renderTarget != null) {
                    bottomRight.Y = -bottomRight.Y;
                }
                clipRect.Right = (bottomRight.X * 0.5f + 0.5f) * width;
                clipRect.Bottom = (0.5f - bottomRight.Y * 0.5f) * height;

                // flip y coordiantes when rendering to backbuffer
                if (renderTarget == null) {
                    clipRect.Y = height - clipRect.Y - clipRect.Height;
                }

                Rectangle bufferRect = new Rectangle(0, 0, width, height);
                Rectangle scissorRect = clipRect.Intersection(bufferRect);

                // a negative rectangle is not allowed
                if (scissorRect.Width < 0 || scissorRect.Height < 0) {
                    scissorRect.Empty();
                }

                context.ScissorBox = scissorRect;
            } else {
                context.ScissorBox = null;
            }

        }
    }
}
