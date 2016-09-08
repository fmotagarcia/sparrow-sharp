using System;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Textures
{
    /// <summary>
    /// The GLTexture class is a concrete implementation of the abstract class Texture,
    /// containing a standard 2D OpenGL texture. 
    /// </summary>
    public class GLTexture : Texture
    {
        private TextureSmoothing _smoothing;
        private uint _name;
        private float _width;
        private float _height;
        private float _scale;
        private bool _repeat;
        private bool _premultipliedAlpha;
        private bool _mipmaps;

        override public uint Name { get { return _name; } }

        override public float NativeWidth { get { return _width; } }

        override public float NativeHeight { get { return _height; } }

        override public float Height { get { return _height / _scale; } }

        override public float Width { get { return _width / _scale; } }

        override public GLTexture Base { get { return this; } }

        override public bool MipMaps { get { return _mipmaps; } }

        override public bool PremultipliedAlpha { get { return _premultipliedAlpha; } }

        override public bool Repeat
        { 
            get { return _repeat; }
            set
            {
                if (value != _repeat)
                {
                    _repeat = value;
                    GL.BindTexture(TextureTarget.Texture2D, _name);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, _repeat ? (int)All.Repeat : (int)All.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, _repeat ? (int)All.Repeat : (int)All.ClampToEdge);
                }
            }
        }

        override public TextureSmoothing Smoothing
        { 
            get { return _smoothing; }
            set
            {
                if (value != _smoothing)
                {
                    _smoothing = value;
                   
                    TextureMagFilter magFilter; 
                    TextureMinFilter minFilter;

                    if (value == TextureSmoothing.None)
                    {
                        magFilter = TextureMagFilter.Nearest;
                        minFilter = _mipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                    }
                    else if (value == TextureSmoothing.Bilinear)
                    {
                        magFilter = TextureMagFilter.Linear;
                        minFilter = _mipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                    }
                    else
                    {
                        magFilter = TextureMagFilter.Linear;
                        minFilter = _mipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                    }
                    GL.BindTexture(TextureTarget.Texture2D, _name);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
                }
            }
        }

        /// <summary>
        /// Initializes a texture with the given properties. Width and height are expected pixels.
        /// </summary>
        public GLTexture(uint name, float width, float height, bool hasMipMaps, float scale, bool premultipliedAlpha)
        {
            Init(name, width, height, hasMipMaps, scale, premultipliedAlpha);
        }

        private void Init(uint name, float width, float height, bool hasMipMaps, float scale, bool premultipliedAlpha)
        {
            if (width <= 0.0f)
                throw new InvalidOperationException("invalid width");
            if (height <= 0.0f)
                throw new InvalidOperationException("invalid height");
            if (scale <= 0.0f)
                throw new InvalidOperationException("invalid scale");

            _name = name;
            _width = width;
            _height = height;
            _mipmaps = hasMipMaps;
            _scale = scale;
            _premultipliedAlpha = premultipliedAlpha;

            _repeat = true; // force first update
            Repeat = false;
            _smoothing = (TextureSmoothing)9999999; // force first update
            Smoothing = TextureSmoothing.Bilinear;
        }

        /// <summary>
        /// Initializes an uncompressed texture with with raw pixel data and a set of properties.
        /// Width and Height are expected pixel dimensions.
        /// </summary>
        public GLTexture(IntPtr imgData, TextureProperties properties)
        {
            uint glTexName;
            bool compressed = properties.TextureFormat.Compressed;
            GL.GenTextures(1, out glTexName);
            GL.BindTexture(TextureTarget.Texture2D, glTexName);

#if __WINDOWS__
            GL.TexStorage2D(TextureTarget2d.Texture2D,
#elif __ANDROID__
            GL.TexStorage2D(TextureTarget2D.Texture2D,
#endif
                properties.NumMipmaps + 1, // mipmap level, min 1
                properties.TextureFormat.InternalFormat,
                properties.Width,
                properties.Height);

            if (!compressed)
            {
                if (imgData != IntPtr.Zero)
                {
                    GL.TexSubImage2D(TextureTarget.Texture2D,
                        0, // level
                        0, // xOffset
                        0, // yOffset
                        properties.Width,
                        properties.Height,
                        properties.TextureFormat.PixelFormat,
                        properties.TextureFormat.PixelType,
                        imgData);
                }
            }
            else
            {
                // TODO this is not tested!
                if (imgData != IntPtr.Zero)
                {
                    int size = Math.Max(32, properties.Width * properties.Height * properties.TextureFormat.BitsPerPixel / 8);
                    GL.CompressedTexSubImage2D(TextureTarget.Texture2D,
                        0, // level
                        0, // xOffset
                        0, // yOffset
                        properties.Width,
                        properties.Height,
                        properties.TextureFormat.PixelFormat,
                        size,
                        imgData);
                }
            }

            if (properties.NumMipmaps > 0)
            {
#if __WINDOWS__
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
#elif __ANDROID__
                GL.GenerateMipmap(TextureTarget.Texture2D);
#endif
            }

            bool containsMipmaps = properties.NumMipmaps > 0;

            Init(glTexName, properties.Width, properties.Height, containsMipmaps, properties.Scale, properties.PremultipliedAlpha);
        }

        /// Initializes a PVR texture with with a certain scale factor.
        //public GLTexture (PVRData pvrData, float scale) {
        //}
    }
}

