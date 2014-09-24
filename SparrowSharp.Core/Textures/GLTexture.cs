using System;
using OpenTK.Graphics.ES20;

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

        override public GLTexture Root { get { return this; } }

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
                    GL.BindTexture(TextureTarget.Texture2D, _name);

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

            if (!compressed)
            {
                int levelWidth = properties.Width;
                int levelHeight = properties.Height;

                for (int level = 0; level <= properties.NumMipmaps; ++level)
                {
                    int size = levelWidth * levelHeight * properties.TextureFormat.BitsPerPixel / 8;
                    GL.TexImage2D(
                        TextureTarget.Texture2D, 
                        level, 
                        properties.TextureFormat.InternalFormat, 
                        levelWidth, 
                        levelHeight, 
                        0, 
                        properties.TextureFormat.Format,
                        properties.TextureFormat.TexType,
                        imgData);
                    imgData += size;
                    levelWidth /= 2;
                    levelHeight /= 2;
                }

                if (properties.NumMipmaps == 0 && properties.GenerateMipmaps)
                {
                    GL.GenerateMipmap(TextureTarget.Texture2D);
                }
            }
            else
            {
                int levelWidth = properties.Width;
                int levelHeight = properties.Height;

                for (int level = 0; level <= properties.NumMipmaps; ++level)
                {
                    int size = Math.Max(32, levelWidth * levelHeight * properties.TextureFormat.BitsPerPixel / 8);
                    GL.CompressedTexImage2D(
                        TextureTarget.Texture2D,
                        level, 
                        properties.TextureFormat.InternalFormat,
                        levelWidth, 
                        levelHeight,
                        0,
                        size,
                        imgData);
                    imgData += size;
                    levelWidth /= 2;
                    levelHeight /= 2;
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, 0); // TODO: this should be glTexName ?

            bool containsMipmaps = properties.NumMipmaps > 0 || (properties.GenerateMipmaps && !compressed);

            Init(glTexName, properties.Width, properties.Height, containsMipmaps, properties.Scale, properties.PremultipliedAlpha);
        }
        /// Initializes a PVR texture with with a certain scale factor.
        //public GLTexture (PVRData pvrData, float scale) {
        //}
    }
}

