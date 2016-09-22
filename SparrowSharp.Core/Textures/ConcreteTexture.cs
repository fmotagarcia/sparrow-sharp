
using Sparrow.Utils;
using Sparrow.Rendering;
using System;
using Sparrow.Core;

namespace Sparrow.Textures
{
    /** A ConcreteTexture wraps a Stage3D texture object, storing the properties of the texture
     *  and providing utility methods for data upload, etc.
     *
     *  <p>This class cannot be instantiated directly; create instances using
     *  <code>Texture.fromTextureBase</code> instead. However, that's only necessary when
     *  you need to wrap a <code>TextureBase</code> object in a Starling texture;
     *  the preferred way of creating textures is to use one of the other
     *  <code>Texture.from...</code> factory methods in the <code>Texture</code> class.</p>
     *
     *  @see Texture
     */
    public class ConcreteTexture : Texture
    {
        private int _base;
        private TextureFormat _format;
        private int _width;
        private int _height;
        private int _numMipMaps;
        private bool _premultipliedAlpha;
        private bool _optimizedForRenderTexture;
        private float _scale;

        /** @private
         *
         *  Creates a ConcreteTexture object from a TextureBase, storing information about size,
         *  mip-mapping, and if the channels contain premultiplied alpha values. May only be
         *  called from subclasses.
         *
         *  <p>Note that <code>width</code> and <code>height</code> are expected in pixels,
         *  i.e. they do not take the scale factor into account.</p>
         */
        public ConcreteTexture(int __base, TextureFormat format, int width, int height,
                               int numMipMaps, bool premultipliedAlpha,
                               bool optimizedForRenderTexture = false, float scale = 1f)
        {

            _scale = scale <= 0f ? 1.0f : scale;
            _base = __base;
            _format = format;
            _width = width;
            _height = height;
            _numMipMaps = numMipMaps;
            _premultipliedAlpha = premultipliedAlpha;
            _optimizedForRenderTexture = optimizedForRenderTexture;
        }

        /** Disposes the TextureBase object. */
        public override void Dispose()
        {
            if (_base != 0)
            {
                throw new NotImplementedException();
            }
        }
        
        /** Clears the texture with a certain color and alpha value. The previous contents of the
         *  texture is wiped out. */
        public void Clear(uint color = 0x0, float alpha = 0.0f)
        {
            if (_premultipliedAlpha && alpha< 1.0f)
            {
                color = ColorUtil.GetRGB(
                    (byte)((float)(ColorUtil.GetR(color)) * alpha),
                    (byte)((float)(ColorUtil.GetG(color)) * alpha),
                    (byte)((float)(ColorUtil.GetB(color)) * alpha));
            }
                

            Painter painter = SparrowSharpApp.Painter;
            painter.PushState();
            painter.State.RenderTarget = this;

            painter.Clear(color, alpha);

            painter.PopState();
        }

        // properties
        
        /** Indicates if the base texture was optimized for being used in a render texture. */
        public bool OptimizedForRenderTexture { get { return _optimizedForRenderTexture; } }

        public override int Base { get { return _base; } }

        /** The concrete texture the texture is based on. */
        public override ConcreteTexture Root { get { return this; } }

        /** @inheritDoc */
        public override TextureFormat Format { get { return _format; } }
        
        /** @inheritDoc */
        public override float Width { get { return _width / _scale; } }
        
        /** @inheritDoc */
        public override float Height { get { return _height / _scale; } }
        
        /** @inheritDoc */
        public override float NativeWidth { get { return _width; } }
        
        /** @inheritDoc */
        public override float NativeHeight { get { return _height; } }
        
        /** @inheritDoc */
        public override float Scale { get { return _scale; } }
        
        /** @inheritDoc */
        public override int NumMipMaps { get { return _numMipMaps; } }
        
        /** @inheritDoc */
        public override bool PremultipliedAlpha { get { return _premultipliedAlpha; } }

    }
}
