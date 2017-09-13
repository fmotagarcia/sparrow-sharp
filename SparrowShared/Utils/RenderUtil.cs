using Sparrow.Textures;
using OpenGL;

namespace Sparrow.Utils
{
    public class RenderUtil
    {

        public static void SetSamplerStateAt(uint name, bool hasMipMaps,
                                      TextureSmoothing smoothing = TextureSmoothing.Bilinear,
                                      bool repeat = false)
        {
            // set repeat
            Gl.BindTexture(TextureTarget.Texture2d, name);
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
