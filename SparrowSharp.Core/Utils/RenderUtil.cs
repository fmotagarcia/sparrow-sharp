using Sparrow.Textures;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

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

        public static void SetSamplerStateAt(int _name, bool hasMipMaps,
                                      TextureSmoothing smoothing = TextureSmoothing.Bilinear,
                                      bool repeat = false)
        {
            // set repeat
            GL.BindTexture(TextureTarget.Texture2D, _name);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, repeat ? (int)All.Repeat : (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, repeat ? (int)All.Repeat : (int)All.ClampToEdge);

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
        }
    }
}
