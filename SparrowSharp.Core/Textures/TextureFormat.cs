using OpenTK.Graphics.ES20;

namespace Sparrow.Textures
{
    public class TextureFormat
    {
        public static readonly TextureFormat Rgba8888 = new TextureFormat(PixelFormat.Rgba, PixelInternalFormat.Rgba, 32, false, PixelType.UnsignedByte);
        public static readonly TextureFormat Alpha8 = new TextureFormat(PixelFormat.Alpha, PixelInternalFormat.Alpha, 8, false, PixelType.UnsignedByte);
        // TODO: these are not supported in iOS for some reason
        //public static readonly TextureFormat CompressedRgbaPvrtc2Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbaPvrtc2Bppv1Img, 2, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat CompressedRgbPvrtc2Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbPvrtc2Bppv1Img, 2, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat CompressedRgbaPvrtc4Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbaPvrtc4Bppv1Img, 4, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat CompressedRgbPvrtc4Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbPvrtc4Bppv1Img, 4, true, PixelType.UnsignedByte);
        public static readonly TextureFormat RGB565 = new TextureFormat(PixelFormat.Rgb, PixelInternalFormat.Rgb, 16, false, PixelType.UnsignedShort565);
        public static readonly TextureFormat Rgb888 = new TextureFormat(PixelFormat.Rgb, PixelInternalFormat.Rgb, 24, false, PixelType.UnsignedByte);
        public static readonly TextureFormat Rgba5551 = new TextureFormat(PixelFormat.Rgba, PixelInternalFormat.Rgba, 16, false, PixelType.UnsignedShort5551);
        public static readonly TextureFormat Rgba4444 = new TextureFormat(PixelFormat.Rgba, PixelInternalFormat.Rgba, 16, false, PixelType.UnsignedShort4444);
        public static readonly TextureFormat AI88 = new TextureFormat(PixelFormat.LuminanceAlpha, PixelInternalFormat.LuminanceAlpha, 16, false, PixelType.UnsignedByte);
        public static readonly TextureFormat I8 = new TextureFormat(PixelFormat.Luminance, PixelInternalFormat.Luminance, 8, false, PixelType.UnsignedByte);
        private readonly PixelInternalFormat _internalformat;
        private readonly PixelFormat _format;

        public PixelInternalFormat InternalFormat
        {
            get
            {
                return _internalformat;
            }
        }

        public PixelFormat Format
        {
            get
            {
                return _format;
            }
        }


        private readonly int _bitsPerPixel;

        public int BitsPerPixel
        {
            get
            {
                return _bitsPerPixel;
            }
        }

        private readonly bool _compressed;

        public bool Compressed
        {
            get
            {
                return _compressed;
            }
        }

        private readonly PixelType _texType;

        public PixelType TexType
        {
            get
            {
                return _texType;
            }
        }

        private TextureFormat(PixelFormat format, PixelInternalFormat internalFormat, int bitsPerPixel, bool compressed, PixelType texType)
        {
            _format = format;
            _internalformat = internalFormat;
            _bitsPerPixel = bitsPerPixel;
            _compressed = compressed;
            _texType = texType;
        }
    }
}

