using System;
using OpenTK.Graphics.ES20;

namespace Sparrow.Textures
{
	public class TextureFormat
	{
		public static readonly TextureFormat Rgba8888 = new TextureFormat(All.Rgba, 32, false, All.UnsignedByte);
		public static readonly TextureFormat Alpha8 = new TextureFormat(All.Alpha, 8, false, All.UnsignedByte);
		public static readonly TextureFormat CompressedRgbaPvrtc2Bppv1Img = new TextureFormat(All.CompressedRgbaPvrtc2Bppv1Img, 2, true, All.UnsignedByte);
		public static readonly TextureFormat CompressedRgbPvrtc2Bppv1Img = new TextureFormat(All.CompressedRgbPvrtc2Bppv1Img, 2, true, All.UnsignedByte);
		public static readonly TextureFormat CompressedRgbaPvrtc4Bppv1Img = new TextureFormat(All.CompressedRgbaPvrtc4Bppv1Img, 4, true, All.UnsignedByte);
		public static readonly TextureFormat CompressedRgbPvrtc4Bppv1Img = new TextureFormat(All.CompressedRgbPvrtc4Bppv1Img, 4, true, All.UnsignedByte);
		public static readonly TextureFormat RGB565 = new TextureFormat(All.Rgb, 16, false, All.UnsignedShort565);
		public static readonly TextureFormat Rgb888 = new TextureFormat(All.Rgb, 24, false, All.UnsignedByte);
		public static readonly TextureFormat Rgba5551 = new TextureFormat(All.Rgba, 16, false, All.UnsignedShort5551);
		public static readonly TextureFormat Rgba4444 = new TextureFormat(All.Rgba, 16, false, All.UnsignedShort4444);
		public static readonly TextureFormat AI88 = new TextureFormat(All.LuminanceAlpha, 16, false, All.UnsignedByte);
		public static readonly TextureFormat I8 = new TextureFormat(All.Luminance, 8, false, All.UnsignedByte);

		private All _format;

		public All Format {
			get {
				return _format;
			}
		}

		private int _bitsPerPixel;

		public int BitsPerPixel {
			get {
				return _bitsPerPixel;
			}
		}

		private bool _compressed;

		public bool Compressed {
			get {
				return _compressed;
			}
		}

		private All _texType;

		public All TexType {
			get {
				return _texType;
			}
		}

		private TextureFormat (All format, int bitsPerPixel, bool compressed, All texType)
		{
			_format = format;
			_bitsPerPixel = bitsPerPixel;
			_compressed = compressed;
			_texType = texType;
		}
	}
}

