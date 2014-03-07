using System;
using Sparrow.Utils;
using Sparrow.Geom;
using Sparrow.Core;

namespace Sparrow.Display
{
	public class Image : Quad
	{
		VertexData _vertexDataCache;
		bool _vertexDataCacheInvalid;
		public Texture _texture;

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

		private void ReadjustSize()
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

		override public void CopyVertexDataTo(VertexData targetData, int atIndex)
		{
			if (_vertexDataCacheInvalid)
			{
				_vertexDataCacheInvalid = false;
				_vertexData.CopyToVertexData(_vertexDataCache);
				_texture.AdjustVertexData(_vertexDataCache, 0, 4);
			}
			_vertexDataCache.CopyToVertexData(targetData, atIndex, 4);
		}

		override public Texture Texture 
		{ 
			set {
				if (value == null)
				{
					throw new Exception(@"texture cannot be nil!");
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

