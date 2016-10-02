using Sparrow.Core;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using System.Collections.Generic;
using System;
using Sparrow.Utils;

namespace Sparrow.Filters
{
    internal class FilterHelper : IFilterHelper
    {
        private float _width;
        private float _height;
        private int _nativeWidth;
        private int _nativeHeight;
        private Stack<Texture> _pool;
        private float _preferredScale;
        private float _scale;
        private int _sizeStep;
        private int _numPasses;
        private Matrix3D _projectionMatrix;
        private Rectangle _targetBounds;
        private DisplayObject _target;
        /** The render target that was active when the filter started processing. */
        public Texture RenderTarget;
        /** The texture format of the returned textures. */
        public TextureFormat TextureFormat;

        /** Creates a new, empty instance. */
        public FilterHelper(TextureFormat textureFormat)
        {
            _preferredScale = SparrowSharp.ContentScaleFactor;
            TextureFormat = textureFormat;
            _sizeStep = 64; // must be POT!
            _pool = new Stack<Texture>();
            _projectionMatrix = Matrix3D.Create();
            _targetBounds = Rectangle.Create();

            SetSize(_sizeStep, _sizeStep);
        }

        /** Purges the pool. */
        public void Dispose()
        {
            Purge();
        }

        /** Starts a new round of rendering. If <code>numPasses</code> is greater than zero, each
         *  <code>getTexture()</code> call will be counted as one pass; the final pass will then
         *  return <code>null</code> instead of a texture, to indicate that this pass should be
         *  rendered to the back buffer.
         */
        public void Start(int numPasses, bool drawLastPassToBackBuffer)
        {
            _numPasses = drawLastPassToBackBuffer ? numPasses : -1;
        }

        public Texture GetTexture(float resolution = 1)
        {
            Texture texture;
            SubTexture subTexture;

            if (_numPasses >= 0)
            {
                if (_numPasses-- == 0) return null;
            }

            if (_pool.Count > 0)
            {
                texture = _pool.Pop();
            }
            else
            {
                texture = Texture.Empty(_nativeWidth / _scale, _nativeHeight / _scale,
                    true, 0, true, _scale, TextureFormat);
            }

            if (!MathUtil.Equals(texture.Width, _width, 0.1f) ||
                !MathUtil.Equals(texture.Height, _height, 0.1f) ||
                !MathUtil.Equals(texture.Scale, _scale * resolution))
            {
                Rectangle sRegion = Rectangle.Create(0, 0, _width * resolution, _height * resolution);
                subTexture = texture as SubTexture;

                if (subTexture != null)
                {
                    subTexture.SetTo(texture.Root, sRegion, true, null, false, resolution);
                }
                else
                {
                    texture = new SubTexture(texture.Root, sRegion, true, null, false, resolution);
                }
            }
            texture.Root.Clear();
            return texture;
        }

        public void PutTexture(Texture texture)
        {
            if (texture != null)
            {
                if (texture.Root.NativeWidth == _nativeWidth && texture.Root.NativeHeight == _nativeHeight)
                {
                    _pool.Push(texture);
                }
                else
                {
                    texture.Dispose();
                }
            }
        }

        /** Purges the pool and disposes all textures. */
        public void Purge()
        {
            foreach (var item in _pool)
            {
                item.Dispose();
            }
            _pool.Clear();
        }

        /** Updates the size of the returned textures. Small size changes may allow the
         *  existing textures to be reused; big size changes will automatically dispose
         *  them. */
        private void SetSize(float width, float height)
        {
            float factor;
            float newScale = _preferredScale;
            int maxNativeSize   = Texture.MaxSize;
            float newNativeWidth  = GetNativeSize(width, newScale);
            float newNativeHeight = GetNativeSize(height, newScale);

            if (newNativeWidth > maxNativeSize || newNativeHeight > maxNativeSize)
            {
                factor = maxNativeSize / Math.Max(newNativeWidth, newNativeHeight);
                newNativeWidth  *= factor;
                newNativeHeight *= factor;
                newScale *= factor;
            }

            if (_nativeWidth != newNativeWidth || _nativeHeight != newNativeHeight ||
                _scale != newScale)
            {
                Purge();

                _scale = newScale;
                _nativeWidth  = (int)newNativeWidth;
                _nativeHeight = (int)newNativeHeight;
            }

            _width  = width;
            _height = height;
        }

        private int GetNativeSize(float size, float textureScale)
        {
            return (int)Math.Ceiling(size * textureScale / _sizeStep) * _sizeStep;
        }

        /** The projection matrix that was active when the filter started processing. */
        public Matrix3D ProjectionMatrix3D {
            get { return _projectionMatrix; }
            set { _projectionMatrix.CopyFrom(value); }
        }

        public Rectangle TargetBounds
        {
            get{ return _targetBounds;  }
            set
            {
                _targetBounds.CopyFrom(value);
                SetSize(value.Width, value.Height);
            }
        }

        public DisplayObject Target
        {
            get { return _target; }
            set { _target = value; }
        }

        /** The scale factor of the returned textures. */
        public float TextureScale { 
            get { return _preferredScale; }
            set { _preferredScale = value > 0 ? value : SparrowSharp.ContentScaleFactor; }
        }

    }
}
