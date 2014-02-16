using System;
using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Display;
using Sparrow.Utils;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;

namespace Sparrow.Core
{
	public class RenderSupport
	{
		Matrix _projectionMatrix;
		Matrix _mvpMatrix;
		int _numDrawCalls;
		List<RenderState> _stateStack;
		RenderState _stateStackTop;
		int _stateStackIndex;
		List<QuadBatch> _quadBatches;
		QuadBatch _quadBatchTop;
		int _quadBatchIndex;
		List<Rectangle> _clipRectStack;


		public Matrix ProjectionMatrix {
			get { return _projectionMatrix;}
			set {
				_projectionMatrix.CopyFromMatrix (value);
				ApplyClipRect ();
			}
		}

		public Matrix MvpMatrix {
			get { return _mvpMatrix;}
			set {
				_mvpMatrix.CopyFromMatrix (value);
				_mvpMatrix.AppendMatrix (_projectionMatrix);
			}
		}

		public Matrix ModelViewMatrix {
			get { return _stateStackTop.ModelViewMatrix;}
		}

		 public double Alpha {
			get { return _stateStackTop.Alpha; }
			set {
				_stateStackTop.Alpha = value;
			}
		}

		public uint BlendMode {
			get { return _stateStackTop.BlendMode; }
			set {
				if (value != Sparrow.Display.BlendMode.AUTO) {
					_stateStackTop.BlendMode = value;
				}
			}
		}

//		public Texture RenderTarget {
//			get { return Sparrow.Context.RenderTarget; }
//			set {
//				ApplyClipRect ();
//				Sparrow.Context.RenderTarget = valye;
//			}
//		}

		public RenderSupport ()
		{
			_projectionMatrix = new Matrix ();
			_mvpMatrix = new Matrix ();

			_stateStack = new List<RenderState> ();
			_stateStack.Add (new RenderState ());

			_stateStackIndex = 0;
			_stateStackTop = _stateStack [0];

			_quadBatches = new List<QuadBatch> ();
			_quadBatches.Add (new QuadBatch ());
			_quadBatchIndex = 0;
			_quadBatchTop = _quadBatches [0];

			_clipRectStack = new List<Rectangle> ();


			SetupOrthographicProjection (0, 320, 0, 480);
		}

		public void PurgeBuffers ()
		{
			_quadBatches.Clear ();
			_quadBatchTop = new QuadBatch ();
			_quadBatches.Add (_quadBatchTop);

			_quadBatchIndex = 0;
		}

		public void Clear ()
		{
			ClearWithColor (0, 0);
		}

		public void ClearWithColor (uint color = 0, double alpha = 1)
		{
			double red = ColorUtil.GetR (color) / 255.0;
			double green = ColorUtil.GetG (color) / 255.0;
			double blue = ColorUtil.GetB (color) / 255.0;

			GL.ClearColor ((float)red, (float)green, (float)blue, (float)alpha);
			GL.Clear (ClearBufferMask.ColorBufferBit);
		}

		public void CheckForOpenGLError ()
		{
			// TODO implement
		}

		public void AddDrawCalls (int count)
		{
			_numDrawCalls += count;
		}

		public void SetupOrthographicProjection (double left, double right, double top, double bottom)
		{
			_projectionMatrix.A = 2.0 / (right - left);
			_projectionMatrix.B = 0.0;
			_projectionMatrix.C = 0.0;
			_projectionMatrix.D = 2.0 / (top - bottom);
			_projectionMatrix.Tx = -(right + left) / (right - left);
			_projectionMatrix.Ty = -(top + bottom) / (top - bottom);

			ApplyClipRect ();
		}

		public void NextFrame ()
		{
			_clipRectStack.Clear ();
			_stateStackIndex = 0;
			_quadBatchIndex = 0;
			_numDrawCalls = 0;
			_quadBatchTop = _quadBatches [0];
			_stateStackTop = _stateStack [0];
		}

		public void BatchQuad (Quad quad)
		{
			double alpha = _stateStackTop.Alpha;
			uint blendMode = _stateStackTop.BlendMode;
			Matrix modelViewMatrix = _stateStackTop.ModelViewMatrix;

			if (_quadBatchTop.IsStateChange (quad.Tinted, null, alpha, quad.PremultipliedAlpha, blendMode, 1)) {
				FinishQuadBatch ();
			}

			_quadBatchTop.AddQuad (quad, alpha, blendMode, modelViewMatrix);
		}

		public void FinishQuadBatch ()
		{
			if (_quadBatchTop.NumQuads != 0) {
				_quadBatchTop.Render ();
				_quadBatchTop.Reset ();

				if (_quadBatches.Count == _quadBatchIndex + 1) {
					_quadBatches.Add (new QuadBatch ());
				}					

				_numDrawCalls++;
				_quadBatchTop = _quadBatches [++_quadBatchIndex];
			}
		}

		public void PushState (Matrix matrix, double alpha, uint blendMode)
		{
			RenderState previousState = _stateStackTop;

			if (_stateStack.Count == _stateStackIndex + 1) {
				_stateStack.Add (new RenderState ());
			}

			_stateStackTop = _stateStack [_stateStackIndex];
			_stateStackTop.Setup (previousState, matrix, alpha, blendMode);
		}

		public void PopState ()
		{
			if (_stateStack.Count == 0) {
				throw new InvalidOperationException ("The state stack must not be empty");
			}

			_stateStackTop = _stateStack [--_stateStackIndex];
		}

		public void ApplyBlendMode (bool premultipliedAlpha)
		{
			BlendMode.ApplyBlendFactors (_stateStackTop.BlendMode, premultipliedAlpha);
		}

		public Rectangle pushClipRect (Rectangle clipRect)
		{
			Rectangle rectangle = new Rectangle ();
			rectangle.CopyFromRectangle (clipRect);

			_clipRectStack.Add (rectangle);

			if (_clipRectStack.Count > 1) {
				rectangle = rectangle.Intersection (_clipRectStack [_clipRectStack.Count - 2]);
			}

			ApplyClipRect ();

			return rectangle;
		}

		public void PopClipRect ()
		{
			if (_clipRectStack.Count > 0) {
				ApplyClipRect ();
			}
		}

		public void ApplyClipRect ()
		{
			FinishQuadBatch ();

			Context context = Sparrow.Context;
			if (context == null) {
				return;
			}

			if (_clipRectStack.Count > 0) {
				int width;
				int height;
				Rectangle rect = _clipRectStack [_clipRectStack.Count - 1];
				Rectangle clipRect = new Rectangle ();
				//SPTexture *renderTarget = context.renderTarget;

//				if (renderTarget != null)
//				{
//					width = renderTarget.NativeWidth;
//					height = renderTarget.NativeHeight;
//				}
//				else
//				{
				width = Sparrow.CurrentController.DrawableWidth;
				height = Sparrow.CurrentController.DrawableHeight;
//				}

				// convert to pixel coordinates (matrix transformation ends up in range [-1, 1])
				Point topLeft = _projectionMatrix.TransformPoint (rect.X, rect.Y);
//				if (renderTarget!= null) {
//					topLeft.Y = -topLeft.Y;
//				}
				clipRect.X = (topLeft.X * 0.5 + 0.5) * width;
				clipRect.Y = (0.5 - topLeft.Y * 0.5) * height;

				Point bottomRight = _projectionMatrix.TransformPoint (rect.Right, rect.Bottom);
//				if (renderTarget != null) {
//					bottomRight.Y = -bottomRight.Y;
//				}
				clipRect.Right = (bottomRight.X * 0.5 + 0.5) * width;
				clipRect.Bottom = (0.5 - bottomRight.Y * 0.5) * height;

				// flip y coordiantes when rendering to backbuffer
//				if (renderTarget == null) 
//				{
				clipRect.Y = height - clipRect.Y - clipRect.Height;
//				}

				Rectangle bufferRect = new Rectangle (0, 0, width, height);
				Rectangle scissorRect = clipRect.Intersection (bufferRect);

				// a negative rectangle is not allowed
				if (scissorRect.Width < 0 || scissorRect.Height < 0) {
					scissorRect.Empty ();
				}

				context.ScissorBox = scissorRect;
			} else {
				context.ScissorBox = null;
			}
		}
	}
}
