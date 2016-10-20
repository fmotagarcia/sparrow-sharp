
using Sparrow.Core;

namespace Sparrow.Textures
{
    public class TextureOptions
    {

        private float _scale;

        /** The <code>Context3DTextureFormat</code> of the underlying texture data. Only used
         *  for textures that are created from Bitmaps; the format of ATF files is set when they
         *  are created. @default BGRA */
        public TextureFormat Format;

        /** Indicates if the texture contains mip maps. @default false */
        public int NumMipMaps;

        /** Indicates if the texture will be used as render target. */
        public bool OptimizeForRenderToTexture = false;

        /** Indicates if the alpha values are premultiplied into the RGB values. This is typically
         *  true for textures created from BitmapData and false for textures created from ATF data.
         *  This property will only be read by the <code>Texture.fromTextureBase</code> factory
         *  method. @default true */
        public bool PremultipliedAlpha;

        /** Creates a new instance with the given options. */
        public TextureOptions(TextureFormat format, 
                              float scale = 1.0f,
                              int numMipMaps = 0,
                              bool premultipliedAlpha = true)
        {
            _scale = scale;
            Format = format;
            NumMipMaps = numMipMaps;
            PremultipliedAlpha = premultipliedAlpha;
        }

        /** Creates a clone of the TextureOptions object with the exact same properties. */
        public TextureOptions Clone()
        {
            TextureOptions clone = new TextureOptions(Format, _scale, NumMipMaps, PremultipliedAlpha);
            clone.OptimizeForRenderToTexture = OptimizeForRenderToTexture;
            return clone;
        }

        /** The scale factor, which influences width and height properties. If you pass '-1',
         *  the current global content scale factor will be used. @default 1.0 */
         public float Scale
        {
            get { return _scale; }
            set { _scale = value > 0 ? value : SparrowSharp.ContentScaleFactor; }
        }
        
    }
}
