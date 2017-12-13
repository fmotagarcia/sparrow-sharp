
using Sparrow.Core;

namespace Sparrow.Textures
{
    public class TextureOptions
    {

        private float _scale;

        /** The <code>TextureFormat</code> of the underlying texture data. */
        public TextureFormat Format;

        /** Indicates if the texture contains mip maps. @default false */
        public int NumMipMaps;

        /** Indicates if the texture will be used as render target. */
        public bool OptimizeForRenderToTexture;

        /** Indicates if the alpha values are premultiplied into the RGB values. This is typically
         *  true for textures created from Images. @default true */
        public bool PremultipliedAlpha;

        /// <summary>
        /// Creates a new instance with the given options.
        /// </summary>
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

        /// <summary>
        /// Creates a clone of the TextureOptions object with the exact same properties.
        /// </summary>
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
