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
			Init (name, width, height, hasMipMaps, scale, premultipliedAlpha);
		}

		private void Init(uint name, float width, float height, bool hasMipMaps, float scale, bool premultipliedAlpha) {
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

		/// Initializes an uncompressed texture with with raw pixel data and a set of properties.
		/// Width and Height are expected pixel dimensions.
		/// imgData is some kind of pointer
		public GLTexture (IntPtr imgData, TextureProperties properties) 
			: base () 
		{
			uint glTexName;
			bool compressed = properties.TextureFormat.Compressed;
			GL.GenTextures (1, out glTexName);
			GL.BindTexture(All.Texture2D, glTexName);

			if (!compressed)
			{
				int levelWidth  = properties.Width;
				int levelHeight = properties.Height;

				for (int level=0; level <= properties.NumMipmaps; ++level)
				{
					int size = levelWidth * levelHeight * properties.TextureFormat.BitsPerPixel / 8;
					GL.TexImage2D (
						All.Texture2D, 
						level, 
						(int)properties.TextureFormat.Format, 
						levelWidth, 
						levelHeight, 
						0, 
						properties.TextureFormat.Format,
						properties.TextureFormat.TexType,
						imgData);
					imgData += size;
					levelWidth  /= 2;
					levelHeight /= 2;
				}

				if (properties.NumMipmaps == 0 && properties.GenerateMipmaps) {
					GL.GenerateMipmap (All.Texture2D);
				}
			}
			else
			{
				int levelWidth  = properties.Width;
				int levelHeight = properties.Height;

				for (int level=0; level <= properties.NumMipmaps; ++level)
				{
					int size = Math.Max(32, levelWidth * levelHeight * properties.TextureFormat.BitsPerPixel / 8);
					GL.CompressedTexImage2D (
						All.Texture2D,
						level, 
						properties.TextureFormat.Format,
						levelWidth, 
						levelHeight,
						0,
						size,
						imgData);
					imgData += size;
					levelWidth  /= 2;
					levelHeight /= 2;
				}
			}

			GL.BindTexture (All.Texture2D, 0);

			bool containsMipmaps = properties.NumMipmaps > 0 || (properties.GenerateMipmaps && !compressed);

			Init(glTexName, properties.Width, properties.Height, containsMipmaps, properties.Scale, properties.PremultipliedAlpha);
		}

		/// Initializes a PVR texture with with a certain scale factor.
		//public GLTexture (PVRData pvrData, float scale) {
		//}


	}
}

