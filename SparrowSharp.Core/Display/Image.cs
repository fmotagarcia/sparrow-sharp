using System;
using Sparrow.Utils;
using Sparrow.Geom;
using Sparrow.Textures;

namespace Sparrow.Display
{
	public class Image : Quad
	{
		private VertexData _vertexDataCache;
		private bool _vertexDataCacheInvalid;
		private Texture _texture;

		public Image (Texture texture)
		{
			if (texture == null) throw new Exception(@"texture cannot be null!");

			Rectangle frame = texture.Frame;    
			float width  = (frame != null) ? frame.Width  : texture.Width;
			float height = (frame != null) ? frame.Height : texture.Height;
			bool pma = texture.PremultipliedAlpha;

			Init(width, height, 0xFFFFFF, pma);

			_vertexData.Vertices[1].TexCoords.X = 1.0f;
			_vertexData.Vertices[2].TexCoords.Y = 1.0f;
			_vertexData.Vertices[3].TexCoords.X = 1.0f;
			_vertexData.Vertices[3].TexCoords.Y = 1.0f;

			_texture = texture;
			_vertexDataCache = new VertexData(4, pma);
			_vertexDataCacheInvalid = true;
		}

		/// Sets the texture coordinates of a vertex. Coordinates are in the range [0, 1].
		public void setTexCoords(Point coords, int vertexID)
		{
			_vertexData.SetTexCoords(coords, vertexID);
			VertexDataDidChange();
		}

		/// Sets the texture coordinates of a vertex. Coordinates are in the range [0, 1].
		public void SetTexCoords(float x, float y, int vertexID)
		{
			_vertexData.SetTexCoords(x, y, vertexID);
			VertexDataDidChange ();
		}

		/// Gets the texture coordinates of a vertex.
		public Point TexCoordsOfVertex(int vertexID)
		{
			return _vertexData.TexCoordsAtIndex(vertexID);
		}

		/// Readjusts the dimensions of the image according to its current texture. Call this method 
		/// to synchronize image and texture size after assigning a texture with a different size.
		public void ReadjustSize()
		{
			Rectangle frame = _texture.Frame;    
			float width  = (frame != null) ? frame.Width  : Texture.Width;
			float height = (frame != null) ? frame.Height : Texture.Height;

			_vertexData.Vertices[1].Position.X = width;
			_vertexData.Vertices[2].Position.Y = height;
			_vertexData.Vertices[3].Position.X = width;
			_vertexData.Vertices[3].Position.Y = height;
			VertexDataDidChange ();
		}

		override public void VertexDataDidChange()
		{
			_vertexDataCacheInvalid = true;
		}

		override public void CopyVertexDataTo(VertexData targetData, int atIndex, bool copyColor)
		{
			copyColor = copyColor || Tinted || Alpha != 1.0f;

			if (_vertexDataCacheInvalid)
			{
				_vertexDataCacheInvalid = false;
				_vertexData.CopyToVertexData(_vertexDataCache, copyColor);
				_texture.AdjustVertexData(_vertexDataCache, 0, 4);
			}
			_vertexDataCache.CopyToVertexData(targetData, atIndex, 4, copyColor);
		}

		override public Texture Texture 
		{ 
			set {
				if (value == null)
				{
					throw new Exception(@"texture cannot be null!");
				}
				else if (value != _texture)
				{
					_texture = value;
					_vertexData.SetPremultipliedAlpha(_texture.PremultipliedAlpha, true);
					_vertexDataCache.SetPremultipliedAlpha(_texture.PremultipliedAlpha, false);
					VertexDataDidChange();
				}
			}
			get { return _texture; }
		}

	}
}