using OpenGL;

namespace Sparrow.Textures
{
    public class TextureFormat
    {
        
        // TODO review these
        public static readonly TextureFormat Rgba8888 = new TextureFormat(PixelFormat.Rgba, InternalFormat.Rgba8, 32, false, PixelType.UnsignedByte);
        //public static readonly TextureFormat Alpha8 = new TextureFormat(PixelFormat.Alpha, SizedInternalFormat.Alpha8Ext, 8, false, PixelType.UnsignedByte);
        // these are not supported in iOS for some reason
        //public static readonly TextureFormat CompressedRgbaPvrtc2Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbaPvrtc2Bppv1Img, 2, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat CompressedRgbPvrtc2Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbPvrtc2Bppv1Img, 2, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat CompressedRgbaPvrtc4Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbaPvrtc4Bppv1Img, 4, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat CompressedRgbPvrtc4Bppv1Img = new TextureFormat(PixelInternalFormat.CompressedRgbPvrtc4Bppv1Img, 4, true, PixelType.UnsignedByte);
        //public static readonly TextureFormat RGB565 = new TextureFormat(PixelFormat.Rgb, SizedInternalFormat.Rgb8, 16, false, PixelType.UnsignedShort565);
        public static readonly TextureFormat Rgb565 = new TextureFormat(PixelFormat.Rgb, InternalFormat.Rgba8, 16, false, PixelType.UnsignedShort565);
        //public static readonly TextureFormat Rgb888 = new TextureFormat(PixelFormat.Rgb, SizedInternalFormat.Rgb8, 24, false, PixelType.UnsignedByte);
        public static readonly TextureFormat Rgba5551 = new TextureFormat(PixelFormat.Rgba, InternalFormat.Rgba8, 16, false, PixelType.UnsignedShort5551);
        public static readonly TextureFormat Rgba4444 = new TextureFormat(PixelFormat.Rgba, InternalFormat.Rgba8, 16, false, PixelType.UnsignedShort4444);
        //public static readonly TextureFormat AI88 = new TextureFormat(PixelFormat.LuminanceAlpha, SizedInternalFormat.LuminanceAlpha16fExt, 16, false, PixelType.UnsignedByte);
        //public static readonly TextureFormat I8 = new TextureFormat(PixelFormat.Luminance, SizedInternalFormat.Luminance8Ext, 8, false, PixelType.UnsignedByte);


        public readonly InternalFormat InternalFormat;

        public readonly PixelFormat PixelFormat;

        /// <summary>
        /// Needed for compressed image upload
        /// </summary>
        public readonly int BitsPerPixel;

        public readonly bool Compressed;

        public readonly PixelType PixelType;

        public TextureFormat(PixelFormat format, InternalFormat internalFormat, int bitsPerPixel, bool compressed, PixelType texType)
        {
            PixelFormat = format;
            InternalFormat = internalFormat;
            BitsPerPixel = bitsPerPixel;
            Compressed = compressed;
            PixelType = texType;
        }
    }
}

