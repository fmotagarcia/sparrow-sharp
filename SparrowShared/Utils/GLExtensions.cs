
using System.Collections.Generic;
using OpenGL;

namespace Sparrow.Utils
{
    public static class GLExtensions
    {
        private static readonly HashSet<string> Extensions;
        private static readonly Dictionary<uint, uint> FramebufferCache;
        public readonly static bool MapBufferSupported = false;
        public readonly static bool TextureMaxAnisotropySupported = false;

        static GLExtensions()
        {
            Extensions = new HashSet<string>();
            FramebufferCache = new Dictionary<uint, uint>();
            
            string extensionsString = Gl.GetString(StringName.Extensions);

            if (!string.IsNullOrEmpty(extensionsString))
            {
                string[] extensions = extensionsString.Split(' ');
                for (int i = 0; i < extensions.Length; i++)
                {
                    Extensions.Add(extensions[i]);
                }
            }
            MapBufferSupported = DeviceSupportsOpenGLExtension("GL_OES_mapbuffer");

            TextureMaxAnisotropySupported = DeviceSupportsOpenGLExtension("GL_EXT_texture_filter_anisotropic");
        }

        /// <summary>
        /// Checks if the device supports the specified OpenGL ES extension
        /// </summary>
        public static bool DeviceSupportsOpenGLExtension(string extensionName)
        {
            return Extensions.Contains(extensionName);
        }

    }
}