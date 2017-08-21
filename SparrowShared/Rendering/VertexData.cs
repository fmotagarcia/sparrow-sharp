using System;
using Sparrow.Geom;
using Sparrow.Utils;
using OpenGL;

namespace Sparrow.Rendering
{
    /** The VertexData class manages a raw list of vertex information, allowing direct upload
     *  to OpenGL vertex buffers. <em>You only have to work with this class if you're writing
     *  your own rendering code (e.g. if you create custom display objects).</em>
     *
     *  <p>To render objects with OpenGL, you have to organize vertices and indices in so-called
     *  vertex- and index-buffers. Vertex buffers store the coordinates of the vertices that make
     *  up an object; index buffers reference those vertices to determine which vertices spawn
     *  up triangles. Those buffers reside in graphics memory and can be accessed very
     *  efficiently by the GPU.</p>
     *
     *  <p>Before you can move data into the buffers, you have to set it up in conventional
     *  memory that is, in a Vector or a ByteArray. Since it's quite cumbersome to manually
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
     *  @see VertexDataFormat
     *  @see IndexData
     */
    public class VertexData
    {
        private const float MinAlpha = 5.0f / 255.0f;
        private Vertex[] _vertices;
        private VertexColor[] _vertexColors;
        private int _numVertices;
        private bool _premultipliedAlpha;

        /// <summary>
        /// Initializes a VertexData instance with a certain size.
        /// </summary>
        public VertexData(int numVertices = 0, bool premultipliedAlpha = true)
        {
            _premultipliedAlpha = premultipliedAlpha;
            NumVertices = numVertices;
        }

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
                            if (value > _vertices.Length || value < _vertices.Length * 0.6f)
                            {
                                //add some buffer to prevent frequent resizing
                                int newLenght = Math.Max((int)(_vertices.Length * 1.4f), (int)(value * 1.4f));
                                //Console.WriteLine("expand buffer " + newLenght);
                                Array.Resize(ref _vertices, newLenght);
                                Array.Resize(ref _vertexColors, newLenght);
                            }
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
        /// Copies a range of vertices of this instance to another vertex data object.
        /// </summary>
        public void CopyTo(VertexData target, int sourceOffset, int targetOffset, int numVertices, Matrix2D matrix = null)
        {
            if (numVertices < 0 || sourceOffset + numVertices > _numVertices)
            {
                numVertices = _numVertices - sourceOffset;
            }

            if (target.NumVertices < targetOffset + numVertices)
            {
                target.NumVertices = targetOffset + numVertices;
            }
            Vertex.Copy(_vertices, sourceOffset, target.Vertices, targetOffset, numVertices);
            Array.Copy(_vertexColors, sourceOffset, target.VertexColors, targetOffset, numVertices);

            // TODO optimize this. Maybe do it inside Vertex.copy with unsafe code?
            if (matrix != null && !matrix.IsIdentity())
            {
                int len = target._numVertices;
                for (int i = targetOffset; i < targetOffset + numVertices; i++)
                {
                    float x = target._vertices[i].Position.X;
                    float y = target._vertices[i].Position.Y;

                    target._vertices[i].Position.X = matrix.A * x + matrix.C * y + matrix.Tx;
                    target._vertices[i].Position.Y = matrix.D * y + matrix.B * x + matrix.Ty;
                }
            }
        }

        /// <summary>
        /// Returns the position of a vertex.
        /// </summary>
        public Point GetPoint(int index)
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
        public void SetPoint(int atIndex, Point position)
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
        public void SetPoint(int atIndex, float x, float y)
        {
            if (atIndex < 0)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }
            if (atIndex >= _numVertices)
            {
                NumVertices = atIndex + 1;
            }
            _vertices[atIndex].Position = new Vector2(x, y);
        }

        /// <summary>
        /// Returns the texture coordinates of a vertex.
        /// </summary>
        public Point GetTexCoords(int index)
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
        public void SetTexCoords(int atIndex, Point texCoords)
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
        public void SetTexCoords(int atIndex, float x, float y)
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
        public void SetColor(int atIndex, uint color, float alpha)
        {
            if (atIndex < 0 || atIndex >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            alpha = MathUtil.Clamp(alpha, _premultipliedAlpha ? MinAlpha : 0.0f, 1.0f); 

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
                SetColor(i, color, alpha);
            }
        }

        /// <summary>
        /// Updates the color of a vertex. The method always expects non-premultiplied alpha values.
        /// </summary>
        public void SetColor(int atIndex, uint color)
        {
            float alpha = GetAlpha(atIndex);
            SetColor(atIndex, color, alpha);
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
        /// Writes the given RGB and alpha values to all vertices.
        /// </summary>
        public void Colorize(uint color, float alpha)
        {

            Colorize(color, alpha, 0, _numVertices);
        }

        /// <summary>
        /// Writes the given RGB and alpha values to the specified vertices.
        /// </summary>
        public void Colorize(uint color, float alpha, int vertexId, int numVertices)
        {
            if (numVertices < 0 || vertexId + numVertices > _numVertices)
            {
                numVertices = _numVertices - vertexId;
            }

            alpha = MathUtil.Clamp(alpha, _premultipliedAlpha ? MinAlpha : 0.0f, 1.0f);
            VertexColor vertexColor = VertexColorHelper.CreateVertexColor(color, alpha);
            VertexColor col = _premultipliedAlpha ? VertexColorHelper.PremultiplyAlpha(vertexColor) : vertexColor;
            for (int i = vertexId; i < numVertices; i++)
            {
                _vertexColors[i] = col;
            }
        }

        /// <summary>
        /// Updates the alpha value of a vertex.
        /// </summary>
        public void SetAlpha(int atIndex, float alpha)
        {
            uint color = GetColor(atIndex);
            SetColor(atIndex, color, alpha);
        }

        /// <summary>
        /// Updates the alpha value of all vertices.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            for (int i = 0; i < _numVertices; i++)
            {
                SetAlpha(i, alpha);
            }
        }

        /// <summary>
        /// Returns the alpha value of a vertex.
        /// </summary>
        public float GetAlpha(int index)
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
        public void ScaleAlphas(float factor)
        {
            ScaleAlphas(factor, 0, _numVertices);
        }

        /// <summary>
        /// Multiplies a range of alpha values with a certain factor.
        /// </summary>
        public void ScaleAlphas(float factor, int index, int numVertices)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            if (factor == 1.0f)
            {
                return;
            }

            int minAlpha = _premultipliedAlpha ? (int)(MinAlpha * 255.0f) : 0;

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
        public void TransformVertices(Matrix2D matrix, int atIndex, int numVertices)
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
        public Rectangle BoundsAfterTransformation(Matrix2D matrix)
        {
            return BoundsAfterTransformation(matrix, 0, _numVertices);
        }

        /// <summary>
        /// Calculates the bounding rectangle of subsequent vertices after being transformed by a matrix.
        /// </summary>
        public Rectangle BoundsAfterTransformation(Matrix2D matrix, int atIndex, int numVertices)
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

            return Rectangle.Create(minX, minY, maxX - minX, maxY - minY);
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

        public void Clear()
        {
            NumVertices = 0;
        }

        private static uint PremultiplyAlpha(uint rgba)
        {
            uint alpha = rgba & 0xff;

            if (alpha == 0xff) return rgba;
           
            float factor = alpha / 255.0f;
            uint r = (uint)(((rgba >> 24) & 0xff) * factor);
            uint g = (uint)(((rgba >> 16) & 0xff) * factor);
            uint b = (uint)(((rgba >>  8) & 0xff) * factor);

            return (r & 0xff) << 24 |
                   (g & 0xff) << 16 |
                   (b & 0xff) <<  8 | alpha;
        }

        /** Creates a vertex buffer object with the right size to fit the complete data.
         *  Optionally, the current data is uploaded right away. */
        public uint[] CreateVertexBuffer(bool upload = false)
        {
            return CreateVertexBuffer(upload, BufferUsageARB.StaticDraw);
        }

        /** Creates a vertex buffer object with the right size to fit the complete data.
         *  Optionally, the current data is uploaded right away. */
        public uint[] CreateVertexBuffer(bool upload, BufferUsageARB bufferUsage)
        {
            if (_numVertices == 0) return new uint[] { 0, 0 };
            
            uint vertexBufferName = Gl.GenBuffer();
            uint vertexColorsBufferName = Gl.GenBuffer();

            if (upload)
            {
                UploadToVertexBuffer(vertexBufferName, vertexColorsBufferName, bufferUsage);
            }
            uint[] ret = { vertexBufferName, vertexColorsBufferName };
            return ret;
        }

        public void UploadToVertexBuffer(uint vertexBufferName, uint vertexColorsBufferName, BufferUsageARB bufferUsage)
        {
            if (_numVertices > 0)
            {
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexBufferName);
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_numVertices * 4 * sizeof(float)), _vertices, bufferUsage);


                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vertexColorsBufferName);
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_numVertices * sizeof(byte) * 4), VertexColors, bufferUsage);
            }
        }

    }
}