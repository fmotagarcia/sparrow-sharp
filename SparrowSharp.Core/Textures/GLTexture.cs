using System;
using Sparrow.Core;
using OpenTK.Graphics.ES20;

namespace Sparrow.Textures
{
	public class GLTexture : Texture
	{

		private TextureFormat _format;
		private TextureSmoothing _smoothing;
		private uint _name;
		private float _width;
		private float _height;
		private float _scale;
		private bool _repeat;
		private bool _premultipliedAlpha;
		private bool _mipmaps;
		override public uint Name { get { return _name; } }
		override public float NativeWidth { get {return _width;} }
		override public float NativeHeight { get {return _height;} }
		override public float Height { get {return _height / _scale;} }
		override public float Width { get {return _width / _scale;} }
		override public GLTexture Root { get {return this;} }
		override public bool MipMaps { get { return _mipmaps; } }
		override public bool PremultipliedAlpha { get { return _premultipliedAlpha; } }
		override public bool Repeat { 
			get {return _repeat; }
			set {
				if (value != _repeat)
				{
					_repeat = value;
					GL.BindTexture(All.Texture2D, _name);
					GL.TexParameter(All.Texture2D, All.TextureWrapS, _repeat ? (int)All.Repeat : (int)All.ClampToEdge);
					GL.TexParameter(All.Texture2D, All.TextureWrapT, _repeat ? (int)All.Repeat : (int)All.ClampToEdge);
				}
			}
		}

		override public TextureSmoothing Smoothing { 
			get {return _smoothing;}
			set {
				if (value != _smoothing)
				{
					_smoothing = value;
					GL.BindTexture(All.Texture2D, _name);

					All magFilter; 
					All minFilter;

					if (value == TextureSmoothing.None)
					{
						magFilter = All.Nearest;
						minFilter = _mipmaps ? All.NearestMipmapNearest : All.Nearest;
					}
					else if (value == TextureSmoothing.Bilinear)
					{
						magFilter = All.Linear;
						minFilter = _mipmaps ? All.LinearMipmapNearest : All.Linear;
					}
					else
					{
						magFilter = All.Linear;
						minFilter = _mipmaps ? All.LinearMipmapLinear : All.Linear;
					}
					GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)magFilter);
					GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)minFilter);
				}
			}
		}

		/// Initializes a texture with the given properties. Width and height are expected pixel dimensions.
		public GLTexture (uint name, float width, float height, bool hasMipMaps, float scale, bool premultipliedAlpha) 
			: base () 
		{
			if (width <= 0.0f) throw new InvalidOperationException("invalid width");
			if (height <= 0.0f) throw new InvalidOperationException("invalid height");
			if (scale <= 0.0f) throw new InvalidOperationException("invalid scale");

			_name = name;
			_width = width;
			_height = height;
			_mipmaps = hasMipMaps;
			_scale = scale;
			_premultipliedAlpha = premultipliedAlpha;

			_repeat = true; // force first update
			Repeat = false;
			Smoothing = TextureSmoothing.Bilinear;
		}

		public GLTexture (object imgData, TextureProperties properties) 
			: base () 
		{
			uint glTexName;
			bool compressed = properties.TextureFormat.Compressed;
			GL.GenTextures (1, out glTexName);
			GL.BindTexture(All.Texture2D, glTexName);
			/* TODO
			if (!compressed)
			{
				int levelWidth  = properties.Width;
				int levelHeight = properties.Height;
				unsigned char *levelData = (unsigned char *)imgData;

				for (int level=0; level<=properties.numMipmaps; ++level)
				{
					int size = levelWidth * levelHeight * bitsPerPixel / 8;
					glTexImage2D(GL_TEXTURE_2D, level, glTexFormat, levelWidth, levelHeight,
						0, glTexFormat, glTexType, levelData);
					levelData += size;
					levelWidth  /= 2;
					levelHeight /= 2;
				}

				if (properties.numMipmaps == 0 && properties.generateMipmaps)
					glGenerateMipmap(GL_TEXTURE_2D);
			}
			else
			{
				int levelWidth  = properties.width;
				int levelHeight = properties.height;
				unsigned char *levelData = (unsigned char *)imgData;

				for (int level=0; level<=properties.numMipmaps; ++level)
				{
					int size = MAX(32, levelWidth * levelHeight * bitsPerPixel / 8);
					glCompressedTexImage2D(GL_TEXTURE_2D, level, glTexFormat,
						levelWidth, levelHeight, 0, size, levelData);
					levelData += size;
					levelWidth  /= 2;
					levelHeight /= 2;
				}
			}

			glBindTexture(GL_TEXTURE_2D, 0);

			BOOL containsMipmaps = properties.numMipmaps > 0 || (properties.generateMipmaps && !compressed);

			return [self initWithName:glTexName format:properties.format
				width:properties.width height:properties.height
				containsMipmaps:containsMipmaps scale:properties.scale
				premultipliedAlpha:properties.premultipliedAlpha];
			*/
		}


		/// Initializes an uncompressed texture with with raw pixel data and a set of properties.
		/// Width and height are expected pixel dimensions.
		//public GLTexture (byte[] imgData, TextureProperties properties) {
		//}

		/// Initializes a PVR texture with with a certain scale factor.
		//public GLTexture (PVRData pvrData, float scale) {
		//}


	}
}

