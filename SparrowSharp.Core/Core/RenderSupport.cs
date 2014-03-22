using System;
using System.Collections.Generic;
using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Display;
using Sparrow.Utils;
using OpenTK.Graphics.ES20;
using Sparrow.Textures;

namespace Sparrow.Core
{
    public class RenderSupport
    {
		private Matrix _projectionMatrix;
		private Matrix _mvpMatrix;
		private int _numDrawCalls;
		private List<RenderState> _stateStack;
		private RenderState _stateStackTop;
		private int _stateStackIndex;
		private int _stateStackSize;
		private List<QuadBatch> _quadBatches;
		private QuadBatch _quadBatchTop;
		private int _quadBatchIndex;
		private int _quadBatchSize;
		private List<Rectangle> _clipRectStack;
		private int _clipRectStackSize;

        public Matrix ProjectionMatrix
        {
            get { return _projectionMatrix; }
            set
            {
                _projectionMatrix.CopyFromMatrix(value);
                ApplyClipRect();
            }
        }

		public int NumDrawCalls
		{
			get
			{
				return _numDrawCalls;
			}
		}

        public Matrix MvpMatrix
        {
			get
            {
				_mvpMatrix.CopyFromMatrix(_stateStackTop.ModelViewMatrix);
                _mvpMatrix.AppendMatrix(_projectionMatrix);
				return _mvpMatrix;
            }
        }

        public Matrix ModelViewMatrix
        {
            get { return _stateStackTop.ModelViewMatrix; }
        }

        public float Alpha
        {
            get { return _stateStackTop.Alpha; }
            set
            {
                _stateStackTop.Alpha = value;
            }
        }

        public uint BlendMode
        {
            get { return _stateStackTop.BlendMode; }
            set
            {
                if (value != Sparrow.Display.BlendMode.AUTO)
                {
                    _stateStackTop.BlendMode = value;
                }
            }
        }

        public Texture RenderTarget
        {
			get { return SparrowSharpApp.Context.RenderTarget; }
            set
            {
                ApplyClipRect();
				SparrowSharpApp.Context.RenderTarget = value;
            }
        }

        public RenderSupport()
        {
			_projectionMatrix = Matrix.Create();
			_mvpMatrix = Matrix.Create();

            _stateStack = new List<RenderState>();
            _stateStack.Add(new RenderState());
            _stateStackIndex = 0;
			_stateStackSize = 1;
            _stateStackTop = _stateStack[0];

            _quadBatches = new List<QuadBatch>();
            _quadBatches.Add(new QuadBatch());
            _quadBatchIndex = 0;
			_quadBatchSize = 1;
            _quadBatchTop = _quadBatches[0];

            _clipRectStack = new List<Rectangle>();
			_clipRectStackSize = 0;

            SetupOrthographicProjection(0, 320, 0, 480);
        }

        public void PurgeBuffers()
        {
            _quadBatches.Clear();
            _quadBatchTop = new QuadBatch();
            _quadBatches.Add(_quadBatchTop);

            _quadBatchIndex = 0;
			_quadBatchSize = 1;
        }

        public void Clear()
        {
            ClearWithColor(0, 0);
        }

		public void ClearWithColor(uint color = 0, float alpha = 1.0f)
        {
            float red = ((float)ColorUtil.GetR(color)) / 255.0f;
            float green = ((float)ColorUtil.GetG(color)) / 255.0f;
            float blue = ((float)ColorUtil.GetB(color)) / 255.0f;

            GL.ClearColor(red, green, blue, alpha);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

		public static void CheckForOpenGLError()
		{
			ErrorCode err = (ErrorCode)GL.GetError ();
			string errstr = "";
			while (err != ErrorCode.NoError)
			{
				errstr += "There was an OpenGL error: " + err.ToString ();
				err = (ErrorCode)GL.GetError ();
			}
			if (errstr != "") {
				Console.WriteLine(errstr);
			}
        }

        public void AddDrawCalls(int count)
        {
            _numDrawCalls += count;
        }

        public void SetupOrthographicProjection(float left, float right, float top, float bottom)
        {
            _projectionMatrix.A = 2.0f / (right - left);
            _projectionMatrix.B = 0.0f;
            _projectionMatrix.C = 0.0f;
            _projectionMatrix.D = 2.0f / (top - bottom);
            _projectionMatrix.Tx = -(right + left) / (right - left);
            _projectionMatrix.Ty = -(top + bottom) / (top - bottom);

            ApplyClipRect();
        }

        public void NextFrame()
        {
			_clipRectStackSize = 0;
            _stateStackIndex = 0;
            _quadBatchIndex = 0;
            _numDrawCalls = 0;
            _quadBatchTop = _quadBatches[0];
            _stateStackTop = _stateStack[0];
        }

        public void BatchQuad(Quad quad)
        {
            float alpha = _stateStackTop.Alpha;
            uint blendMode = _stateStackTop.BlendMode;
            Matrix modelViewMatrix = _stateStackTop.ModelViewMatrix;

			if (_quadBatchTop.IsStateChange(quad.Tinted, quad.Texture, alpha, quad.PremultipliedAlpha, blendMode, 1))
            {
                FinishQuadBatch();
            }

            _quadBatchTop.AddQuad(quad, alpha, blendMode, modelViewMatrix);
        }

        public void FinishQuadBatch()
        {
            if (_quadBatchTop.NumQuads != 0)
            {
                _quadBatchTop.Render(_projectionMatrix);
                _quadBatchTop.Reset();

				if (_quadBatchSize == _quadBatchIndex + 1)
                {
                    _quadBatches.Add(new QuadBatch());
					_quadBatchSize++;
                }					

                _numDrawCalls++;
                _quadBatchTop = _quadBatches[++_quadBatchIndex];
            }
        }

        public void PushState(Matrix matrix, float alpha, uint blendMode)
        {
            RenderState previousState = _stateStackTop;

			if (_stateStackSize == _stateStackIndex + 1)
            {
                _stateStack.Add(new RenderState());
				_stateStackSize++;
            }

            _stateStackTop = _stateStack[++_stateStackIndex];
            _stateStackTop.Setup(previousState, matrix, alpha, blendMode);
        }

        public void PopState()
        {
            if (_stateStack.Count == 0)
            {
                throw new InvalidOperationException("The state stack must not be empty");
            }

            _stateStackTop = _stateStack[--_stateStackIndex];
        }

        public void ApplyBlendMode(bool premultipliedAlpha)
        {
            Sparrow.Display.BlendMode.ApplyBlendFactors(_stateStackTop.BlendMode, premultipliedAlpha);
        }

        public Rectangle PushClipRect(Rectangle clipRect)
        {
			if (_clipRectStack.Count < _clipRectStackSize + 1) {
				_clipRectStack.Add (new Rectangle ());
			}
			Rectangle rectangle = _clipRectStack [_clipRectStackSize];
            rectangle.CopyFromRectangle(clipRect);

			if (_clipRectStackSize > 0)
            {
				rectangle = rectangle.Intersection(_clipRectStack[_clipRectStackSize - 1]);
            }

			_clipRectStackSize++;
            ApplyClipRect();

            return rectangle;
        }

        public void PopClipRect()
        {
			if (_clipRectStackSize > 0)
            {
				_clipRectStackSize--;
                ApplyClipRect();
            }
        }

		private void ApplyClipRect()
        {
            FinishQuadBatch();

            Context context = SparrowSharpApp.Context;
            if (context == null)
            {
                return;
            }

			if (_clipRectStackSize > 0)
            {
                int width;
                int height;
				Rectangle rect = _clipRectStack[_clipRectStackSize - 1];
                Rectangle clipRect = new Rectangle();
				Texture renderTarget = context.RenderTarget;

				if (renderTarget != null)
				{
					width = (int)renderTarget.NativeWidth;
					height = (int)renderTarget.NativeHeight;
				}
				else
				{
	                width = (int)SparrowSharpApp.DrawableWidth;
	                height = (int)SparrowSharpApp.DrawableHeight;
				}

                // convert to pixel coordinates (matrix transformation ends up in range [-1, 1])
                Point topLeft = _projectionMatrix.TransformPoint(rect.X, rect.Y);
				if (renderTarget!= null) {
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
				if (renderTarget == null) 
				{
                	clipRect.Y = height - clipRect.Y - clipRect.Height;
				}

                Rectangle bufferRect = new Rectangle(0, 0, width, height);
                Rectangle scissorRect = clipRect.Intersection(bufferRect);

                // a negative rectangle is not allowed
                if (scissorRect.Width < 0 || scissorRect.Height < 0)
                {
                    scissorRect.Empty();
                }

                context.ScissorBox = scissorRect;
            }
            else
            {
				context.ScissorBox = null;
            }

        }
    }
}
