using System;
using System.Diagnostics;
using Sparrow.Geom;

namespace Sparrow.Textures
{
    /// <summary>
    /// A SubTexture represents a section of another texture. This is achieved solely by 
    /// manipulation of texture coordinates, making the class very efficient. 
    /// Note that it is OK to create subtextures of subtextures.
    /// </summary>
    public class SubTexture : Texture
    {
        
        private Texture _parent;
        private bool _ownsParent;
        private Rectangle _region;
        private Rectangle _frame;
        private bool _rotated;
        private float _width;
        private float _height;
        private float _scale;
        private Matrix2D _transformationMatrix;
        private Matrix2D _transformationMatrixToRoot;

        /** Creates a new SubTexture containing the specified region of a parent texture.
         *
         *  @param parent     The texture you want to create a SubTexture from.
         *  @param region     The region of the parent texture that the SubTexture will show
         *                    (in points). If <code>null</code>, the complete area of the parent.
         *  @param ownsParent If <code>true</code>, the parent texture will be disposed
         *                    automatically when the SubTexture is disposed.
         *  @param frame      If the texture was trimmed, the frame rectangle can be used to restore
         *                    the trimmed area.
         *  @param rotated    If true, the SubTexture will show the parent region rotated by
         *                    90 degrees (CCW).
         *  @param scaleModifier  The scale factor of the SubTexture will be calculated by
         *                    multiplying the parent texture's scale factor with this value.
         */
        public SubTexture(Texture parent, Rectangle region = null,
                          bool ownsParent = false, Rectangle frame= null,
                          bool rotated = false, float scaleModifier = 1)
        {
            SetTo(parent, region, ownsParent, frame, rotated, scaleModifier);
        }

        protected SubTexture()
        {

        }

        /** @private
         *
         *  <p>Textures are supposed to be immutable, and Starling uses this assumption for
         *  optimizations and simplifications all over the place. However, in some situations where
         *  the texture is not accessible to the outside, this can be overruled in order to avoid
         *  allocations.</p>
         */
        internal void SetTo(Texture parent, Rectangle region = null,
                            bool ownsParent = false, Rectangle frame = null,
                            bool rotated = false, float scaleModifier = 1)
        {
            if (_region == null) _region = Rectangle.Create();
            if (region != null) _region.CopyFrom(region);
            else _region.SetTo(0, 0, parent.Width, parent.Height);

            if (frame != null)
            {
                if (_frame != null) _frame.CopyFrom(frame);
                else _frame = frame.Clone();
            }
            else _frame = null;

            _parent = parent;
            _ownsParent = ownsParent;
            _rotated = rotated;
            _width  = (rotated? _region.Height : _region.Width)  / scaleModifier;
            _height = (rotated? _region.Width  : _region.Height) / scaleModifier;
            _scale = _parent.Scale * scaleModifier;

            if (_frame != null && (_frame.X > 0f || _frame.Y > 0f ||
                _frame.Right < _width || _frame.Bottom< _height))
            {
                Debug.WriteLine("[Sparrow] Warning: frames inside the texture's region are unsupported.");
            }

            UpdateMatrices();
        }

        private void UpdateMatrices()
        {
            if (_transformationMatrix != null) _transformationMatrix.Identity();
            else _transformationMatrix = Matrix2D.Create();

            if (_transformationMatrixToRoot != null) _transformationMatrixToRoot.Identity();
            else _transformationMatrixToRoot = Matrix2D.Create();

            if (_rotated)
            {
                _transformationMatrix.Translate(0, -1);
                _transformationMatrix.Rotate((float)Math.PI / 2.0f);
            }

            _transformationMatrix.Scale(_region.Width  / _parent.Width,
                                        _region.Height / _parent.Height);
            _transformationMatrix.Translate(_region.X  / _parent.Width,
                                            _region.Y  / _parent.Height);

            SubTexture texture = this;
            while (texture != null)
            {
                _transformationMatrixToRoot.AppendMatrix(texture._transformationMatrix);
                texture = texture.Parent as SubTexture;
            }
        }

        /** Disposes the parent texture if this texture owns it. */
        public override void Dispose()
        {
            if (_ownsParent)
            {
                _parent.Dispose();
                _parent = null;
            }
        }

        /** The texture which the SubTexture is based on. */
        public Texture Parent { get { return _parent; } }

        /** Indicates if the parent texture is disposed when this object is disposed. */
        public bool OwnsParent { get { return _ownsParent; } }

        /** If true, the SubTexture will show the parent region rotated by 90 degrees (CCW). */
        public bool Rotated { get { return _rotated; } }

        /** The region of the parent texture that the SubTexture is showing (in points).
         *
         *  <p>CAUTION: not a copy, but the actual object! Do not modify!</p> */
        public Rectangle Region { get { return _region; } }

        public override Matrix2D TransformationMatrix { get { return _transformationMatrix; } }

        public override Matrix2D TransformationMatrixToRoot { get { return _transformationMatrixToRoot; } }

        override public uint Base { get { return _parent.Base; } }

        override public ConcreteTexture Root { get { return _parent.Root; } }

        override public TextureFormat Format { get { return _parent.Format; } }

        override public float Width { get { return _width; } }

        override public float Height { get { return _height; } }

        override public float NativeWidth { get { return _width * _scale; } }

        override public float NativeHeight { get { return _height * _scale; } }

        override public int NumMipMaps { get { return _parent.NumMipMaps; } }

        override public bool PremultipliedAlpha { get { return _parent.PremultipliedAlpha; } }
        
        override public float Scale { get { return _parent.Scale; } }

        override public Rectangle Frame { get { return _frame; } }
    }
}

