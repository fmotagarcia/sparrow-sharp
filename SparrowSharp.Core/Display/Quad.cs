using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace Sparrow.Display
{
    /// <summary>
    /// An SPQuad represents a rectangle with a uniform color or a color gradient. 
 
    /// You can set one color per vertex. The colors will smoothly fade into each other over the area
    /// of the quad. To display a simple linear color gradient, assign one color to vertices 0 and 1 and 
    /// another color to vertices 2 and 3.
 
    /// The indices of the vertices are arranged like this:
 
    ///    0 - 1
    ///    | / |
    ///    2 - 3
 
    /// **Colors**
 
    /// Colors in Sparrow are defined as unsigned integers, that's exactly 8 bit per color. The easiest
    /// way to define a color is by writing it as a hexadecimal number. A color has the following
    /// structure:
 
    /// 0xRRGGBB
 
    /// That means that you can create the base colors like this:
 
    ///   0xFF0000 -> red
    ///   0x00FF00 -> green
    ///   0x0000FF -> blue
 
    /// Other simple colors:
 
    ///   0x000000 or 0x0 -> black
    ///   0xFFFFFF        -> white
    ///   0x808080        -> 50% gray
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

        /// <summary>
        /// Sets the colors of all vertices simultaneously. Returns the color of vertex '0'.
        /// </summary>
        public uint Color
        {
            get
            {
                return ColorOfVertex(0);
            }
            set
            {
                for (int i = 0; i < 4; ++i)
                {
                    _vertexData.SetColor(value, i);
                }

                VertexDataDidChange();

                if (value != 0xffffff)
                { // TODO this is not so good. How to display white quads?
                    Tinted = true;
                }
                else
                {
                    Tinted = Alpha != 1.0f || _vertexData.Tinted;
                }
            }
        }

        public override float Alpha
        {
            get
            {
                return base.Alpha;
            }
            set
            {
                base.Alpha = value;
                if (Alpha != 1.0f)
                {
                    Tinted = true;
                }
                else
                {
                    Tinted = _vertexData.Tinted;
                }
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

        /// <summary>
        /// Indicates if any vertices have a non-white color or are not fully opaque. Any alpha value
        /// other than '1' will also cause tinting.
        /// </summary>
        public bool Tinted { get; private set; }

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

            Tinted = color != 0xffffff; // TODO this is not so nice

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

        /// <summary>
        /// Sets the color of a vertex.
        /// </summary>
        public void SetColor(uint color, int vertexID)
        {
            _vertexData.SetColor(color, vertexID);
            VertexDataDidChange();

            if (color != 0xFFFFFF)
            {
                Tinted = true;	
            }
            else
            {
                Tinted = Alpha != 1.0f || _vertexData.Tinted;
            }
        }

        /// <summary>
        /// Returns the color of a vertex.
        /// </summary>
        public uint ColorOfVertex(int vertexID)
        {
            return _vertexData.ColorAt(vertexID);
        }

        override public void Render(RenderSupport support)
        {
            support.BatchQuad(this);
        }

        /// <summary>
        /// Sets the alpha value of a vertex.
        /// </summary>
        public void SetAlpha(float alpha, int vertexID)
        {
            _vertexData.SetAlpha(alpha, vertexID);
            VertexDataDidChange();

            if (alpha != 1.0f)
            {
                Tinted = true;
            }
            else
            {
                Tinted = Alpha != 1.0f || _vertexData.Tinted;
            }
        }

        /// <summary>
        /// Returns the alpha value of a vertex.
        /// </summary>
        public float AlphaOfVertex(int vertexID)
        {
            return _vertexData.AlphaAt(vertexID);
        }

        protected virtual void VertexDataDidChange()
        {
            // override in subclass
        }

        virtual internal void CopyVertexDataTo(VertexData targetData, int atIndex, bool copyColor)
        {
            copyColor = copyColor || Tinted || Alpha != 1.0f;

            _vertexData.CopyToVertexData(targetData, copyColor, atIndex);
        }
    }
}