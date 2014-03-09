using System;
using Sparrow.Core;
using OpenTK.Graphics.ES20;

namespace Sparrow.Display
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
		virtual public bool PremultipliedAlpha { get { return _premultipliedAlpha; } }
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

		// TODO this had a TextureFormat format parameter too, but it seems to be never used
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


		/// Initializes an uncompressed texture with with raw pixel data and a set of properties.
		/// Width and height are expected pixel dimensions.
		//public GLTexture (byte[] imgData, TextureProperties properties) {
		//}

		/// Initializes a PVR texture with with a certain scale factor.
		//public GLTexture (PVRData pvrData, float scale) {
		//}


	}
}

