using Sparrow.Textures;
using OpenGL;

namespace Sparrow.Utils
{
    public class RenderUtil
    {

        /** Returns a bit field uniquely describing texture format and premultiplied alpha,
         *  so that each required AGAL variant will get its unique ID. This method is most
         *  useful when overriding the <code>programVariantName</code> method of custom
         *  effects.
         *
         *  @return a bit field using the 3 least significant bits.
         */
        public static uint GetTextureVariantBits(Texture texture)
        {
            if (texture == null) return 0;

            uint bitField = 0;
            uint formatBits = 0;

            /* TODO?
            switch (texture.Format)
            {
                case Context3DTextureFormat.COMPRESSED_ALPHA:
                    formatBits = 3; break;
                case Context3DTextureFormat.COMPRESSED:
                    formatBits = 2; break;
                default:
                    formatBits = 1;
            }
            */
            formatBits = 1;

            bitField |= formatBits;

            if (!texture.PremultipliedAlpha)
                bitField |= 1 << 2;

            return bitField;
        }

        public static void SetSamplerStateAt(uint _name, bool hasMipMaps,
                                      TextureSmoothing smoothing = TextureSmoothing.Bilinear,
                                      bool repeat = false)
        {
            // set repeat
            Gl.BindTexture(TextureTarget.Texture2d, _name);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, repeat ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, repeat ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);

            // set smoothing
            TextureMagFilter magFilter;
            TextureMinFilter minFilter;

            if (smoothing == TextureSmoothing.None)
            {
                magFilter = TextureMagFilter.Nearest;
                minFilter = hasMipMaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
            }
            else if (smoothing == TextureSmoothing.Bilinear)
            {
                magFilter = TextureMagFilter.Linear;
                minFilter = hasMipMaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
            }
            else
            {
                magFilter = TextureMagFilter.Linear;
                minFilter = hasMipMaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
            }
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)magFilter);
            Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)minFilter);
        }
    }
}
