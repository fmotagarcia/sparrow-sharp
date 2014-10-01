using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace Sparrow.Display
{
    /// <summary>
    /// An Quad represents a rectangle with a uniform color or a color gradient. 
    ///<para></para>
    /// You can set one color per vertex. The colors will smoothly fade into each other over the area
    /// of the quad. To display a simple linear color gradient, assign one color to vertices 0 and 1 and 
    /// another color to vertices 2 and 3.
    ///<para></para>
    /// The indices of the vertices are arranged like this:
    ///<para></para>
    ///<para>    0 - 1</para>
    /// <para>   | / |</para>
    /// <para>   2 - 3</para>
    ///<para></para>
    /// **Colors**
    ///<para></para>
    /// Colors in Sparrow are defined as unsigned integers, that's exactly 8 bit per color. The easiest
    /// way to define a color is by writing it as a hexadecimal number. A color has the following
    /// structure:
    ///<para></para>
    /// 0xRRGGBB
    ///<para></para>
    /// That means that you can create the base colors like this:
    ///<para></para>
    ///   <para>0xFF0000 -> red</para>
    ///   <para>0x00FF00 -> green</para>
    ///   <para>0x0000FF -> blue</para>
    /// </summary>
    public class Quad : DisplayObject
    {
        private const float MIN_SIZE = 0.1f;
        protected VertexData _vertexData;

        /// <summary>
        /// Initializes a quad with a certain size and color. The 'premultipliedAlpha' parameter indicates how the colors
        /// of the object are stored.
        /// </summary>
        public Quad(float width = 32, float height = 32, uint color = 0xffffff, bool premultipliedAlpha = false)
        {
            Init(width, height, color, premultipliedAlpha);
        }

        /// <summary>
        /// Always null on Quads
        /// </summary>
        virtual public Texture Texture { get { return null; } set { } }

        protected uint _color;
        /// <summary>
        /// Sets the colors of all vertices simultaneously.
        /// </summary>
        public uint Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                for (int i = 0; i < 4; ++i)
                {
                    _vertexData.SetColor(value, i);
                }

                VertexDataDidChange();
            }
        }

        /// <summary>
        /// Indicates if the rgb values are stored premultiplied with the alpha value. This can have
        /// effect on the rendering. (Most developers don't have to care, though.)
        /// </summary>
        public bool PremultipliedAlpha
        {
            get
            {
                return _vertexData.PremultipliedAlpha;
            }
            set
            {
                if (value != _vertexData.PremultipliedAlpha)
                {
                    _vertexData.PremultipliedAlpha = value;
                }
            }
        }

        public virtual bool Tinted 
        { 
            get  { 
                return true;
            }
        }

        protected void Init(float width = 32, float height = 32, uint color = 0xffffff, bool premultipliedAlpha = false)
        {
            if (width <= MIN_SIZE)
            {
                width = MIN_SIZE;
            }
            if (height <= MIN_SIZE)
            {
                height = MIN_SIZE;
            }
            _color = color;
            _vertexData = new VertexData(4, premultipliedAlpha);
            _vertexData.Vertices[1].Position.X = width;
            _vertexData.Vertices[2].Position.Y = height;
            _vertexData.Vertices[3].Position.X = width;
            _vertexData.Vertices[3].Position.Y = height;

            for (int i = 0; i < 4; ++i)
            {
                _vertexData.VertexColors[i] = VertexColorHelper.CreateVertexColor(color, 1.0f);
            }

            VertexDataDidChange();
        }

        public override Rectangle BoundsInSpace(DisplayObject targetSpace)
        {
            Point bottomRight;

            if (targetSpace == this)
            { 
                // optimization
                bottomRight = _vertexData.PositionAt(3);
                return new Rectangle(0.0f, 0.0f, bottomRight.X, bottomRight.Y);
            }
            else if (targetSpace == Parent && Rotation == 0.0f)
            { 
                // optimization
                float scaleX = ScaleX;
                float scaleY = ScaleY;

                bottomRight = _vertexData.PositionAt(3);
                Rectangle resultRect = new Rectangle(X - PivotX * scaleX, 
                                           Y - PivotY * scaleY,
                                           bottomRight.X * ScaleX,
                                           bottomRight.Y * ScaleY); 

                if (scaleX < 0.0f)
                { 
                    resultRect.Width *= -1.0f; 
                    resultRect.X -= resultRect.Width;  
                }

                if (scaleY < 0.0f)
                { 
                    resultRect.Height *= -1.0f; 
                    resultRect.Top -= resultRect.Height; 
                }

                return resultRect;
            }
            else
            {
                Matrix transformationMatrix = TransformationMatrixToSpace(targetSpace);
                return _vertexData.BoundsAfterTransformation(transformationMatrix, 0, 4);
            }
        }

        override public void Render(RenderSupport support)
        {
            support.BatchQuad(this);
        }

        protected virtual void VertexDataDidChange()
        {
            // override in subclass
        }

        virtual internal void CopyVertexDataTo(VertexData targetData, int atIndex, bool copyColor)
        {
            copyColor = copyColor || Tinted;

            _vertexData.CopyToVertexData(targetData, copyColor, atIndex);
        }
    }
}