using Sparrow.Core;

namespace Sparrow.Utils
{
    public static class GLExtensions
    {
        public readonly static bool MapBufferSupported = false;
        public readonly static bool TextureMaxAnisotropySupported = false;

        static GLExtensions()
        {
            MapBufferSupported = Context.DeviceSupportsOpenGLExtension("GL_OES_mapbuffer");

            TextureMaxAnisotropySupported = Context.DeviceSupportsOpenGLExtension("GL_EXT_texture_filter_anisotropic");
        }
    }
}