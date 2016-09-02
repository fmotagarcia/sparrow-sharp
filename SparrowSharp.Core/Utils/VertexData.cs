using System;
using Sparrow.Geom;
using OpenTK;

namespace Sparrow.Utils
{
    /** The VertexData class manages a raw list of vertex information, allowing direct upload
     *  to Stage3D vertex buffers. <em>You only have to work with this class if you're writing
     *  your own rendering code (e.g. if you create custom display objects).</em>
     *
     *  <p>To render objects with Stage3D, you have to organize vertices and indices in so-called
     *  vertex- and index-buffers. Vertex buffers store the coordinates of the vertices that make
     *  up an object; index buffers reference those vertices to determine which vertices spawn
     *  up triangles. Those buffers reside in graphics memory and can be accessed very
     *  efficiently by the GPU.</p>
     *
     *  <p>Before you can move data into the buffers, you have to set it up in conventional
     *  memory — that is, in a Vector or a ByteArray. Since it's quite cumbersome to manually
     *  create and manipulate those data structures, the IndexData and VertexData classes provide
     *  a simple way to do just that. The data is stored sequentially (one vertex or index after
     *  the other) so that it can easily be uploaded to a buffer.</p>
     *
     *  <strong>Vertex Format</strong>
     *
     *  <p>The VertexData class requires a custom format string on initialization, or an instance
     *  of the VertexDataFormat class. Here is an example:</p>
     *
     *  <listing>
     *  vertexData = new VertexData("position:float2, color:bytes4");
     *  vertexData.setPoint(0, "position", 320, 480);
     *  vertexData.setColor(0, "color", 0xff00ff);</listing>
     *
     *  <p>This instance is set up with two attributes: "position" and "color". The keywords
     *  after the colons depict the format and size of the data that each property uses; in this
     *  case, we store two floats for the position (for the x- and y-coordinates) and four
     *  bytes for the color. Please refer to the VertexDataFormat documentation for details.</p>
     *
     *  <p>The attribute names are then used to read and write data to the respective positions
     *  inside a vertex. Furthermore, they come in handy when copying data from one VertexData
     *  instance to another: attributes with equal name and data format may be transferred between
     *  different VertexData objects, even when they contain different sets of attributes or have
     *  a different layout.</p>
     *
     *  <strong>Colors</strong>
     *
     *  <p>Always use the format <code>bytes4</code> for color data. The color access methods
     *  expect that format, since it's the most efficient way to store color data. Furthermore,
     *  you should always include the string "color" (or "Color") in the name of color data;
     *  that way, it will be recognized as such and will always have its value pre-filled with
     *  pure white at full opacity.</p>
     *
     *  <strong>Premultiplied Alpha</strong>
     *
     *  <p>Per default, color values are stored with premultiplied alpha values, which
     *  means that the <code>rgb</code> values were multiplied with the <code>alpha</code> values
     *  before saving them. You can change this behavior with the <code>premultipliedAlpha</code>
     *  property.</p>
     *
     *  <p>Beware: with premultiplied alpha, the alpha value always affects the resolution of
     *  the RGB channels. A small alpha value results in a lower accuracy of the other channels,
     *  and if the alpha value reaches zero, the color information is lost altogether.</p>
     *
     *  <strong>Tinting</strong>
     *
     *  <p>Some low-end hardware is very sensitive when it comes to fragment shader complexity.
     *  Thus, Starling optimizes shaders for non-tinted meshes. The VertexData class keeps track
     *  of its <code>tinted</code>-state, at least at a basic level: whenever you change color
     *  or alpha value of a vertex to something different than white (<code>0xffffff</code>) with
     *  full alpha (<code>1.0</code>), the <code>tinted</code> property is enabled.</p>
     *
     *  <p>However, that value is not entirely accurate: when you restore the color of just a
     *  range of vertices, or copy just a subset of vertices to another instance, the property
     *  might wrongfully indicate a tinted mesh. If that's the case, you can either call
     *  <code>updateTinted()</code> or assign a custom value to the <code>tinted</code>-property.
     *  </p>
     *
     *  @see VertexDataFormat
     *  @see IndexData
     */
    public class VertexData
    {
        private const float MIN_ALPHA = 5.0f / 255.0f;
        private Vertex[] _vertices;
        private VertexColor[] _vertexColors;
        private int _numVertices;
        private bool _premultipliedAlpha;

        /// <summary>
        /// Indicates the size of the VertexData object. You can resize the object any time; if you
        /// make it bigger, it will be filled up with vertices that have all properties zeroed, except
        /// for the alpha value (it's '1').
        /// </summary>
        public int NumVertices
        {
            get { return _numVertices; }
            set
            { 
                if (value != _numVertices)
                {
                    if (value > 0)
                    {
                        if (_vertices != null)
                        {
                            Array.Resize(ref _vertices, value);
                            Array.Resize(ref _vertexColors, value);
                        }
                        else
                        {
                            _vertices = new Vertex[value];
                            _vertexColors = new VertexColor[value];
                        }

                        if (value > _numVertices)
                        {
                            for (int i = _numVertices; i < value; i++)
                            {
                                _vertexColors[i] = VertexColorHelper.CreateVertexColor(0, 1.0f);
                            }
                        }
                    }
                    else
                    {
                        _vertices = null;
                    }

                    _numVertices = value;
                }
            }
        }

        /// <summary>
        /// Calculates the bounding rectangle of all vertices.
        /// </summary>
        public Rectangle Bounds
        {
            get { return BoundsAfterTransformation(null, 0, _numVertices); }
        }

        /// <summary>
        /// Returns the raw vertex data.
        /// </summary>
        public Vertex[] Vertices
        {
            get { return _vertices; }
        }

        /// <summary>
        /// Returns the raw vertex color data.
        /// </summary>
        public VertexColor[] VertexColors
        {
            get { return _vertexColors; }
        }

        /// <summary>
        /// Indicates if the rgb values are stored premultiplied with the alpha value. If you change
        /// this property, all color data will be updated accordingly.
        /// </summary>
        public bool PremultipliedAlpha
        {
            get
            {
                return _premultipliedAlpha;
            }
            set
            {
                SetPremultipliedAlpha(value, true);
            }
        }

        /// <summary>
        /// Initializes a VertexData instance with a certain size.
        /// </summary>
        public VertexData(int numVertices = 0, bool premultipliedAlpha = false)
        {
            _premultipliedAlpha = premultipliedAlpha;
            NumVertices = numVertices;
        }

        /// <summary>
        /// Copies the vertex data of this instance to another vertex data object, starting at a certain index.
        /// </summary>
        public void CopyToVertexData(VertexData target, bool copyColor, int atIndex = 0)
        {
            CopyToVertexData(target, copyColor, atIndex, _numVertices);
        }

        /// <summary>
        /// Copies a range of vertices of this instance to another vertex data object.
        /// </summary>
        public void CopyToVertexData(VertexData target, bool copyColor, int atIndex, int numVertices)
        {
            Vertex.Copy(_vertices, 0, target.Vertices, atIndex, numVertices);

            if (copyColor)
            {
                Array.Copy(_vertexColors, 0, target.VertexColors, atIndex, numVertices);
            }
        }

        /// <summary>
        /// Returns the position of a vertex.
        /// </summary>
        public Point PositionAt(int index)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            Vector2 position = _vertices[index].Position;

            return Point.Create(position.X, position.Y);
        }

        /// <summary>
        /// Updates the position of a vertex.
        /// </summary>
        public void SetPosition(Point position, int atIndex)
        {
            if (atIndex < 0 || atIndex >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            _vertices[atIndex].Position = new Vector2(position.X, position.Y);
        }

        /// <summary>
        /// Updates the position of a vertex.
        /// </summary>
        public void SetPosition(float x, float y, int atIndex)
        {
            if (atIndex < 0 || atIndex >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            _vertices[atIndex].Position = new Vector2(x, y);
        }

        /// <summary>
        /// Returns the texture coordinates of a vertex.
        /// </summary>
        public Point TexCoordsAt(int index)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            Vector2 texCoords = _vertices[index].TexCoords;
            return Point.Create(texCoords.X, texCoords.Y);
        }

        /// <summary>
        /// Updates the texture coordinates of a vertex.
        /// </summary>
        public void SetTexCoords(Point texCoords, int atIndex)
        {
            if (atIndex < 0 || atIndex >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            _vertices[atIndex].TexCoords = new Vector2(texCoords.X, texCoords.Y);
        }

        /// <summary>
        /// Updates the texture coordinates of a vertex.
        /// </summary>
        public void SetTexCoords(float x, float y, int atIndex)
        {
            if (atIndex < 0 || atIndex >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            _vertices[atIndex].TexCoords = new Vector2(x, y);
        }

        /// <summary>
        /// Updates the color of a vertex. The method always expects non-premultiplied alpha values.
        /// </summary>
        public void SetColor(uint color, float alpha, int atIndex)
        {
            if (atIndex < 0 || atIndex >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            alpha = MathUtil.Clamp(alpha, _premultipliedAlpha ? MIN_ALPHA : 0.0f, 1.0f); 

            VertexColor vertexColor = VertexColorHelper.CreateVertexColor(color, alpha);
            _vertexColors[atIndex] = _premultipliedAlpha ? VertexColorHelper.PremultiplyAlpha(vertexColor) : vertexColor;
        }

        /// <summary>
        /// Updates the color of all vertexes. The method always expects non-premultiplied alpha values.
        /// </summary>
        public void SetColor(uint color, float alpha)
        {
            for (int i = 0; i < _numVertices; i++)
            {
                SetColor(color, alpha, i);
            }
        }

        /// <summary>
        /// Updates the color of a vertex. The method always expects non-premultiplied alpha values.
        /// </summary>
        public void SetColor(uint color, int atIndex)
        {
            float alpha = AlphaAt(atIndex);
            SetColor(color, alpha, atIndex);
        }

        /// <summary>
        /// Updates the color of all vertexes. The method always expects non-premultiplied alpha values.
        /// </summary>
        public void SetColor(uint color)
        {
            for (int i = 0; i < _numVertices; i++)
            {
                SetColor(color, i);
            }
        }

        /// <summary>
        /// Returns the RGB color of a vertex (without premultiplied alpha).
        /// </summary>
        public uint GetColor(int index)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            VertexColor vertexColor = _vertexColors[index];
            if (_premultipliedAlpha)
            {
                vertexColor = VertexColorHelper.UnmultiplyAlpha(vertexColor);
            }

            return ColorUtil.GetRGB(vertexColor.R, vertexColor.G, vertexColor.B);
        }

        /// <summary>
        /// Updates the alpha value of a vertex.
        /// </summary>
        public void SetAlpha(float alpha, int atIndex)
        {
            uint color = GetColor(atIndex);
            SetColor(color, alpha, atIndex);
        }

        /// <summary>
        /// Updates the alpha value of all vertices.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            for (int i = 0; i < _numVertices; i++)
            {
                SetAlpha(alpha, i);
            }
        }

        /// <summary>
        /// Returns the alpha value of a vertex.
        /// </summary>
        public float AlphaAt(int index)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }
            return _vertexColors[index].A / 255.0f;
        }

        /// <summary>
        /// Multiplies all alpha values with a certain factor.
        /// </summary>
        public void ScaleAlphaBy(float factor)
        {
            ScaleAlphaBy(factor, 0, _numVertices);
        }

        /// <summary>
        /// Multiplies a range of alpha values with a certain factor.
        /// </summary>
        public void ScaleAlphaBy(float factor, int index, int numVertices)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            if (factor == 1.0f)
            {
                return;
            }

            int minAlpha = _premultipliedAlpha ? (int)(MIN_ALPHA * 255.0f) : 0;

            for (int i = index; i < index + numVertices; ++i)
            {
                VertexColor vertexColor = _vertexColors[i];
                byte newAlpha = Convert.ToByte(MathUtil.Clamp(vertexColor.A * factor, minAlpha, 255));

                if (_premultipliedAlpha)
                {
                    vertexColor = VertexColorHelper.UnmultiplyAlpha(vertexColor);
                    vertexColor.A = newAlpha;
                    _vertexColors[i] = VertexColorHelper.PremultiplyAlpha(vertexColor);
                }
                else
                {
                    _vertexColors[i] = VertexColorHelper.CreateVertexColor(vertexColor.R, vertexColor.G, vertexColor.B, newAlpha);
                }
            }
        }

        /// <summary>
        /// Transforms the positions of subsequent vertices by multiplication with a transformation matrix
        /// </summary>
        public void TransformVertices(Matrix matrix, int atIndex, int numVertices)
        {
            if (matrix == null)
            {
                return;
            }

            for (int i = atIndex, end = atIndex + numVertices; i < end; ++i)
            {
                Vector2 pos = _vertices[i].Position;
                float x = matrix.A * pos.X + matrix.C * pos.Y + matrix.Tx;
                float y = matrix.D * pos.Y + matrix.B * pos.X + matrix.Ty;

                _vertices[i].Position.X = x;
                _vertices[i].Position.Y = y;
            }
        }

        /// <summary>
        /// Calculates the bounding rectangle of all vertices after being transformed by a matrix.
        /// </summary>
        public Rectangle BoundsAfterTransformation(Matrix matrix)
        {
            return BoundsAfterTransformation(matrix, 0, _numVertices);
        }

        /// <summary>
        /// Calculates the bounding rectangle of subsequent vertices after being transformed by a matrix.
        /// </summary>
        public Rectangle BoundsAfterTransformation(Matrix matrix, int atIndex, int numVertices)
        {
            if (atIndex < 0 || atIndex + numVertices > _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            if (numVertices == 0)
                return null;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            int endIndex = atIndex + numVertices;

            if (matrix != null)
            {
                for (int i = atIndex; i < endIndex; ++i)
                {
                    Vector2 position = _vertices[i].Position;
                    Point transformedPoint = matrix.TransformPoint(position.X, position.Y);
                    float tfX = transformedPoint.X;
                    float tfY = transformedPoint.Y;
                    minX = Math.Min(minX, tfX);
                    maxX = Math.Max(maxX, tfX);
                    minY = Math.Min(minY, tfY);
                    maxY = Math.Max(maxY, tfY);
                }
            }
            else
            {
                for (int i = atIndex; i < endIndex; ++i)
                {
                    Vector2 position = _vertices[i].Position;
                    minX = Math.Min(minX, position.X);
                    maxX = Math.Max(maxX, position.X);
                    minY = Math.Min(minY, position.Y);
                    maxY = Math.Max(maxY, position.Y);
                }
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Changes the way alpha and color values are stored.
        /// Optionally, all exisiting vertices are updated.
        /// </summary>
        public void SetPremultipliedAlpha(bool value, bool updateVertices)
        {
            if (value == _premultipliedAlpha)
            {
                return;
            }

            if (updateVertices)
            {
                if (value)
                {
                    for (int i = 0; i < _numVertices; ++i)
                    {
                        _vertexColors[i] = VertexColorHelper.PremultiplyAlpha(_vertexColors[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < _numVertices; ++i)
                    {
                        _vertexColors[i] = VertexColorHelper.UnmultiplyAlpha(_vertexColors[i]);
                    }
                }
            }

            _premultipliedAlpha = value;
        }
    }
}