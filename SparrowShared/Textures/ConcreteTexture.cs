
using Sparrow.Utils;
using Sparrow.Rendering;
using System;
using Sparrow.Core;
using OpenGL;

namespace Sparrow.Textures
{
    /// <summary>
    /// A ConcreteTexture wraps an OpenGL texture object, storing the properties of the texture
    /// and providing utility methods for data upload, etc.
    ///
    /// <para>This class cannot be instantiated directly; create instances using
    /// <code>Texture.fromTextureBase</code> instead. However, that's only necessary when
    /// you need to wrap a <code>TextureBase</code> object in a Starling texture;
    /// the preferred way of creating textures is to use one of the other
    /// <code>Texture.From...</code> factory methods in the <code>Texture</code> class.</para>
    ///  
    /// See <see cref="Texture"/>
    ///
    /// </summary>
    public class ConcreteTexture : Texture
    {
        protected TextureFormat _format;
        protected int _width;
        protected int _height;
        protected int _numMipMaps;
        protected bool _premultipliedAlpha;
        protected bool _optimizedForRenderTexture;
        protected uint _base;
        protected float _scale;
        /// <summary>
        /// Stores the pointer/array for the texture in the normal memory.
        /// This is needed for the app be able to restore it on a context loss.
        /// </summary>
        protected object rawData;
        /// <summary>
        ///  Creates a ConcreteTexture object from a TextureBase, storing information about size,
        ///  mip-mapping, and if the channels contain premultiplied alpha values. May only be
        ///  called from subclasses.
        ///
        /// <para>Note that <code>width</code> and <code>height</code> are expected in pixels,
        ///  i.e. they do not take the scale factor into account.</para>
        /// </summary>
        public ConcreteTexture(TextureFormat format, int width, int height,
                               int numMipMaps, bool premultipliedAlpha,
                               bool optimizedForRenderTexture = false, float scale = 1f)
        {
            _scale = scale <= 0f ? 1.0f : scale;
            _format = format;
            _width = width;
            _height = height;
            _numMipMaps = numMipMaps;
            _premultipliedAlpha = premultipliedAlpha;
            _optimizedForRenderTexture = optimizedForRenderTexture;
            SparrowSharp.ContextCreated += OnContextCreated;

            InitGPUTextureStorage();
        }

        protected virtual void InitGPUTextureStorage()
        {
            _base = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, _base);

            Gl.TexStorage2D(Gl.TEXTURE_2D,
                _numMipMaps + 1, // mipmap level, min 1
                _format.InternalFormat,
                _width,
                _height);
        }
        
        public override void Dispose()
        {
            SparrowSharp.ContextCreated -= OnContextCreated;
            if (_base != 0)
            {
                SparrowSharp.Painter.DestroyFramebufferForTexture(this);
                Gl.DeleteTextures(_base);
                _base = 0;
            }
        }

        protected virtual void OnContextCreated()
        {
            SparrowSharp.Painter.DestroyFramebufferForTexture(this);

            InitGPUTextureStorage();

            if (rawData != null)
            {
                UploadData(rawData);
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Uploads image data to the GPU. It also generates bitmaps if specified.
        /// Call this immediately after creating the texture otherwise the OpenGL state might
        /// get corrupted.
        /// </summary>
        /// <param name="pixels">The image data, either an IntPtr or a number primitive array
        /// </param>
        public void UploadData(object pixels)
        {
            rawData = pixels;
            if (_format.Compressed)
            {
                int size = Math.Max(32, _width * _height * _format.BitsPerPixel / 8);
                Gl.CompressedTexSubImage2D(TextureTarget.Texture2d,
                                           0, // level
                                           0, // xOffset
                                           0, // yOffset
                                           _width,
                                           _height,
                                           _format.PixelFormat,
                                           size,
                                           pixels);
            }
            else
            {
                Gl.TexSubImage2D(TextureTarget.Texture2d,
                                 0, // level
                                 0, // xOffset
                                 0, // yOffset
                                 _width,
                                 _height,
                                 _format.PixelFormat,
                                 _format.PixelType,
                                 pixels);
            }

            if (_numMipMaps > 0)
            {
                Gl.GenerateMipmap(Gl.TEXTURE_2D);
            }
        }

        /// <summary>
        /// Clears the texture with a certain color and alpha value. The previous contents of the
        /// texture is wiped out.
        /// </summary>
        public void Clear(uint color = 0x0, float alpha = 0f)
        {
            if (_premultipliedAlpha && alpha < 1.0f)
            {
                color = ColorUtil.GetRGB(
                    (byte)((ColorUtil.GetR(color)) * alpha),
                    (byte)((ColorUtil.GetG(color)) * alpha),
                    (byte)((ColorUtil.GetB(color)) * alpha));
            }


            Painter painter = SparrowSharp.Painter;
            painter.PushState();
            painter.State.RenderTarget = this;

            painter.Clear(color, alpha);

            painter.PopState();
        }

        // properties

        /// <summary>
        /// Indicates if the base texture was optimized for being used in a render texture.
        /// </summary>
        public bool OptimizedForRenderTexture { get { return _optimizedForRenderTexture; } }

        public override uint Base { get { return _base; } }
        
        public override ConcreteTexture Root { get { return this; } }
        
        public override TextureFormat Format { get { return _format; } }
        
        public override float Width { get { return _width / _scale; } }
        
        public override float Height { get { return _height / _scale; } }
        
        public override float NativeWidth { get { return _width; } }
        
        public override float NativeHeight { get { return _height; } }
        
        public override float Scale { get { return _scale; } }
        
        public override int NumMipMaps { get { return _numMipMaps; } }
        
        public override bool PremultipliedAlpha { get { return _premultipliedAlpha; } }

    }
}
