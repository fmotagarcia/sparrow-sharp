using System;
using Sparrow.Utils;
using Sparrow.Geom;
using Sparrow.Core;

namespace Sparrow.Display
{
	public class Quad : DisplayObject
	{
		private const double MIN_SIZE = 0.1;
		private bool _tinted;
		private VertexData _vertexData;

		public uint Color {
			get {
				return ColorOfVertex (0);
			}
			set {
				for (int i = 0; i < 4; ++i) {
					_vertexData.SetColor (value, i);
				}
				
				VertexDataDidChange ();

				if (value != 0xffffff) {
					_tinted = true;
				} else {
					_tinted = Alpha != 1.0 || _vertexData.Tinted;
				}
			}
		}

		public override double Alpha {
			get {
				return base.Alpha;
			}
			set {
				base.Alpha = value;
				if (Alpha != 1.0) {
					_tinted = true;
				} else {
					_tinted = _vertexData.Tinted;
				}
			}
		}

		public bool PremultipliedAlpha {
			get {
				return _vertexData.PremultipliedAlpha;
			}
			set {
				if (value != _vertexData.PremultipliedAlpha)
					_vertexData.PremultipliedAlpha = value;
			}
		}

		public bool Tinted {
			get {
				return _tinted;
			}
		}

		public Sparrow.Core.Texture Texture {
			get { return null; }
		}

		public Quad (double width = 32, double height = 32, uint color = 0xffffff, bool premultipliedAlpha = false)
		{
			if (width <= MIN_SIZE)
				width = MIN_SIZE;
			if (height <= MIN_SIZE)
				height = MIN_SIZE;

			_tinted = color != 0xffffff;

			_vertexData = new VertexData (4, premultipliedAlpha);
			_vertexData.Vertices [1].Position.X = width;
			_vertexData.Vertices [2].Position.Y = height;
			_vertexData.Vertices [3].Position.X = width;
			_vertexData.Vertices [3].Position.Y = height;

			for (int i = 0; i < 4; ++i) {
				_vertexData.Vertices [i].Color = new VertexColor (color, 1.0);
			}

			VertexDataDidChange ();
		}

		public override Rectangle BoundsInSpace (DisplayObject targetSpace)
		{
			Point bottomRight;

			if (targetSpace == this) { // optimization
				bottomRight = _vertexData.PositionAtIndex (3);
				return new Rectangle (0.0, 0.0, bottomRight.X, bottomRight.Y);
			} else if (targetSpace == Parent && Rotation == 0.0) { // optimization
				double scaleX = ScaleX;
				double scaleY = ScaleY;

				bottomRight = _vertexData.PositionAtIndex (3);
				Rectangle resultRect = new Rectangle (X - PivotX * scaleX, 
					                       Y - PivotY * scaleY,
					                       bottomRight.X * ScaleX,
					                       bottomRight.Y * ScaleY); 


				if (scaleX < 0.0) { 
					resultRect.Width *= -1.0; 
					resultRect.X -= resultRect.Width;  
				}

				if (scaleY < 0.0) { 
					resultRect.Height *= -1.0; 
					resultRect.Y -= resultRect.Height; 
				}

				return resultRect;
			} else {
				Matrix transformationMatrix = TransformationMatrixToSpace (targetSpace);
				return _vertexData.BoundsAfterTransformation (transformationMatrix, 0, 4);
			}
		}

		public void SetColor (uint color, int vertexID)
		{
			_vertexData.SetColor (color, vertexID);
			VertexDataDidChange ();

			if (color != 0xFFFFFF) {
				_tinted = true;	
			} else {
				_tinted = Alpha != 1.0 || _vertexData.Tinted;
			}
		}

		public uint ColorOfVertex (int vertexID)
		{
			return _vertexData.ColorAtIndex (vertexID);
		}

		override public void Render (RenderSupport support)
		{
			support.BatchQuad (this);
		}

		public void SetAlpha (double alpha, int vertexID)
		{
			_vertexData.SetAlpha (alpha, vertexID);
			VertexDataDidChange ();

			if (alpha != 1.0) {
				_tinted = true;
			} else {
				_tinted = Alpha != 1.0 || _vertexData.Tinted;
			}
		}

		public double AlphaOfVertex (int vertexID)
		{
			return _vertexData.AlphaAtIndex (vertexID);
		}

		public void VertexDataDidChange ()
		{

		}

		public void CopyVertexDataTo (VertexData targetData, int atIndex)
		{
			_vertexData.CopyToVertexData (targetData, atIndex);
		}
	}
}