using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;
using SparrowSharp.Core.Rendering;
using SparrowSharp.Core.Styles;

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
    public class Quad : Mesh
    {
        private Rectangle _bounds;

        /// <summary>
        /// Initializes a quad with a certain size and color. The 'premultipliedAlpha' parameter indicates how the colors
        /// of the object are stored.
        /// </summary>
        public Quad(float width = 32, float height = 32, uint color = 0xffffff) 
            : base(new VertexData(4), new IndexData())
        {
            Init(width, height, color);
        }

        protected void Init(float width = 32, float height = 32, uint color = 0xffffff)
        {
            if (width <= 0f || height <= 0f)
            {
                throw new System.ArgumentException("Invalid size: width and height must not be zero");
            }
            _bounds = new Rectangle(0, 0, width, height);

            SetupVertices();
            Color = color;
        }

        protected void SetupVertices()
        {
            Texture texture = Style.Texture;
            VertexData vertexData = VertexData;
            IndexData indexData = IndexData;

            indexData.NumIndices = 0;
            indexData.addQuad(0, 1, 2, 3);

            if (vertexData.NumVertices != 4)
            {
                vertexData.NumVertices = 4;
            }

            if (Texture != null)
            {
 //               texture.SetupVertexPositions(vertexData, 0, "position", _bounds);
 //               texture.SetupTextureCoordinates(vertexData, 0, texAttr);
            }
            else
            {
                vertexData.SetPoint(0, _bounds.Left,  _bounds.Top);
                vertexData.SetPoint(1, _bounds.Right, _bounds.Top);
                vertexData.SetPoint(2, _bounds.Left,  _bounds.Bottom);
                vertexData.SetPoint(3,  _bounds.Right, _bounds.Bottom);

                vertexData.SetTexCoords(0, 0.0f, 0.0f);
                vertexData.SetTexCoords(1, 1.0f, 0.0f);
                vertexData.SetTexCoords(2, 0.0f, 1.0f);
                vertexData.SetTexCoords(3, 1.0f, 1.0f);
            }

            SetRequiresRedraw();
        }

        public override Rectangle GetBounds(DisplayObject targetSpace)
        {
            Rectangle outRect = new Rectangle();

            if (targetSpace == this)// optimization
            {
                outRect.CopyFrom(Bounds);
            }
            else if (targetSpace == Parent && !IsRotated)// optimization
            {
                float scaleX = ScaleX;
                float scaleY = ScaleY;

                outRect = new Rectangle(X - PivotX * scaleX,
                                       Y - PivotY * scaleY,
                                       _bounds.Width * ScaleX,
                                       _bounds.Height * ScaleY);
                if (scaleX < 0.0f)
                {
                    outRect.Width *= -1.0f;
                    outRect.X -= outRect.Width;
                }
                if (scaleY < 0.0f)
                {
                    outRect.Height *= -1.0f;
                    outRect.Top -= outRect.Height;
                }
            }
            else
            {
                Matrix sMatrix = GetTransformationMatrix(targetSpace);
                outRect = _bounds.GetBounds(sMatrix);
            }
            return outRect;
        }
        
        override public DisplayObject HitTest(Point localPoint)
        {
            if (!Visible || !Touchable || !HitTestMask(localPoint)) return null;
            else if (_bounds.Contains(localPoint)) return this;
            else return null;
        }

        /** Readjusts the dimensions of the quad. Use this method without any arguments to
         *  synchronize quad and texture size after assigning a texture with a different size.
         *  You can also force a certain width and height by passing positive, non-zero
         *  values for width and height. */
        public void ReadjustSize(float width = -1, float height = -1)
        {
            if (width  <= 0) width  = /*Texture != null ? Texture.FrameWidth  :*/ _bounds.Width;
            if (height <= 0) height = /*Texture != null ? Texture.FrameHeight :*/ _bounds.Height;

            if (width != _bounds.Width || height != _bounds.Height)
            {
                _bounds.SetTo(0, 0, width, height);
                SetupVertices();
            }
        }

        /** Creates a quad from the given texture.
        *  The quad will have the same size as the texture. */
        public static Quad FromTexture(Texture texture)
        {
            Quad quad = new Quad(100, 100);
            quad.Texture = texture;
            quad.ReadjustSize();
            return quad;
        }

        /** The texture that is mapped to the quad (or <code>null</code>, if there is none).
         *  Per default, it is mapped to the complete quad, i.e. to the complete area between the
         *  top left and bottom right vertices. This can be changed with the
         *  <code>setTexCoords</code>-method.
         *
         *  <p>Note that the size of the quad will not change when you assign a texture, which
         *  means that the texture might be distorted at first. Call <code>readjustSize</code> to
         *  synchronize quad and texture size.</p>
         *
         *  <p>You could also set the texture via the <code>style.texture</code> property.
         *  That way, however, the texture frame won't be taken into account. Since only rectangular
         *  objects can make use of a texture frame, only a property on the Quad class can do that.
         *  </p>
         */
        public override Texture Texture {
            get { return base.Texture; }
            set
            {
                if (value != Texture)
                {
                    base.Texture = value;
                    SetupVertices();
                }
            }
        }
        
    }
}