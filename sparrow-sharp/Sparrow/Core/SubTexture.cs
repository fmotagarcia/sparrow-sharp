using System;
using Sparrow.Geom;
using Sparrow.Utils;
using OpenTK.Graphics.ES20;
using OpenTK;
using System.Runtime.InteropServices;
using Sparrow.Display;

namespace Sparrow.Core
{
	public class SubTexture : Texture
	{
		private Texture _parent;
		/// The texture which the subtexture is based on.
		public Texture Parent { get {return _parent;} }

		/// The clipping rectangle, which is the region provided on initialization, scaled into [0.0, 1.0].
		public Rectangle Clipping { get {
				Point topLeft      = _transformationMatrix.TransformPoint(0.0f, 0.0f);
				Point bottomRight  = _transformationMatrix.TransformPoint(1.0f, 1.0f);
				Rectangle clipping = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
				clipping.Normalize();
				return clipping;
			}
		}

		private Matrix _transformationMatrix;
		/// The matrix that is used to transform the texture coordinates into the coordinate
		/// space of the parent texture (used internally by the "adjust..."-methods).
		/// CAUTION: Use with care! Each call returns the same instance.
		public Matrix TransformationMatrix { get {return _transformationMatrix;} }

		private float _scale;
		private float _height;
		private float _width;
		private Rectangle _frame;
		override public float Width { get { return _width;} }
		override public float Height { get { return _height;} }
		override public float NativeWidth { get { return _width * _scale;} }
		override public float NativeHeight { get { return _height * _scale;} }
		//TODO override public GLTexture Root { get {return _parent.Root;} }
		override public uint Name { get {return _parent.Name;} }
		override public bool PremultipliedAlpha { get { return _parent.PremultipliedAlpha; } }
		//TODO override public bool Format { get { return _parent.Format; } }
		override public bool MipMaps { get { return _parent.MipMaps; } }
		override public float Scale { get {return _parent.Scale;} }
		override public bool Repeat { 
			get {return _parent.Repeat;}
			set {_parent.Repeat = value;}
		}
		// TODO override public TextureSmoothing Smoothing { 
		//	get {_parent.Smoothing;}
		//	set {_parent.Smoothing = value;}
		//}

		/// Initializes a subtexture with a region (in points) of another texture.
		public SubTexture (Rectangle region, Texture texture) 
			: this(region, null, texture){
		}

		/// Initializes a subtexture with a region (in points) of another texture, using a frame rectangle
		/// to place the texture within an image.
		public SubTexture (Rectangle region, Rectangle frame, Texture texture) 
			: this(region, frame, false, texture){
		}

		/// Initializes a subtexture with a region (in points) of another texture, using a frame rectangle
		/// to place the texture within an image. If `rotated` is `true`, the subtexture will show the base
		/// region rotated by 90 degrees (CCW). _Designated Initializer_.
		public SubTexture (Rectangle region, Rectangle frame, bool rotated, Texture texture)
			: base(){
			if (region == null)
				region = new Rectangle(0.0f, 0.0f, texture.Width, texture.Height);

			_parent = texture;
			_frame = frame.Copy ();
			_transformationMatrix = new Matrix();
			_width  = rotated ? region.Height : region.Width;
			_height = rotated ? region.Width  : region.Height;

			if (rotated)
			{
				_transformationMatrix.Translate(0, -1);
				_transformationMatrix.Rotate( (float)Math.PI / 2.0f);
			}

			_transformationMatrix.Scale(region.Width / texture.Width, region.Height / texture.Height);

			_transformationMatrix.Translate(region.X  / texture.Width, region.Y / texture.Height);
		}

		override public void AdjustVertexData(VertexData vertexData, int index, int count)
		{
			Vertex[] vertices = vertexData.Vertices;
			int stride = Marshal.SizeOf(typeof(Vertex)) - Marshal.SizeOf(typeof(Vector2));

			AdjustPositions(vertices[index].Position, count, stride);
			AdjustTexCoords(vertices[index].TexCoords, count, stride);
		}

		override public void AdjustTexCoords(Object data, int count, int stride)
		{
			Texture texture = this;
			Matrix matrix = new Matrix();

			do
			{
				SubTexture subTexture = (SubTexture)texture;
				matrix.AppendMatrix(subTexture.TransformationMatrix);
				texture = subTexture._parent;
			}
			while ( texture.GetType().IsEquivalentTo(typeof(SubTexture)) );

			Matrix3 glkMatrix = matrix.ConvertToGLKMatrix3;
			int step =  Marshal.SizeOf(typeof(Vector2)) + stride;

			for (int i=0; i<count; ++i)
			{
				Vector2 currentCoord = (Vector2)data;
				currentCoord = TransformVector2WithMatrix3(glkMatrix, currentCoord);
				data = data + step;
			}
		}

		override public void AdjustPositions(Object data, int count, int stride)
		{
			if (_frame != null)
			{
				if (count != 4) 
					throw new InvalidOperationException(@"Textures with a frame can only be used on quads");

				float deltaRight  = _frame.Width  + _frame.X - _width;
				float deltaBottom = _frame.Height + _frame.Y - _height;

				int step = Marshal.SizeOf(typeof(Vector2)) + stride;
				Vector2 pos;

				// top left
				pos = (Vector2)data;
				pos.X -= _frame.X;
				pos.Y -= _frame.Y;

				// top right
				pos = (Vector2)(data + step);
				pos.X -= deltaRight;
				pos.Y -= _frame.Y;

				// bottom left
				pos = (Vector2)(data + 2*step);
				pos.X -= _frame.X;
				pos.Y -= deltaBottom;

				// bottom right
				pos = (Vector2)(data + 3*step);
				pos.X -= deltaRight;
				pos.Y -= deltaBottom;
			}
		}

		/// Factory method.
		public static SubTexture GenerateTexture(Rectangle region, Texture texture)
		{
			return new SubTexture (region, texture);
		}
			
		private static Vector2 TransformVector2WithMatrix3(Matrix3 glkMatrix, Vector2 vector)
		{
			return new Vector2(
				glkMatrix.m00*vector.X + glkMatrix.m10*vector.Y + glkMatrix.m20,
				glkMatrix.m11*vector.Y + glkMatrix.m01*vector.X + glkMatrix.m21
			);
		}

	}
}

