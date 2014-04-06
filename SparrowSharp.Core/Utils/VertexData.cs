using System;
using Sparrow.Geom;
using OpenTK;

namespace Sparrow.Utils
{
    /// <summary>
    /// The SPVertexData class manages a raw list of vertex information, allowing direct upload
    /// to OpenGL vertex buffers. 
    /// 
    /// _You only have to work with this class if you create display objects with a custom render 
    /// function. If you don't plan to do that, you can safely ignore it._
    /// 
    /// To render objects with OpenGL, you have to organize vertex data in so-called
    /// vertex buffers. Those buffers reside in graphics memory and can be accessed very
    /// efficiently by the GPU. Before you can move data into vertex buffers, you have to
    /// set it up in conventional memory - that is, in a byte array. That array contains
    /// all vertex information (the coordinates, color, and texture coordinates) - one
    /// vertex after the other.
    /// 
    /// To simplify creating and working with such a bulky list, the VertexData class was
    /// created. It contains methods to specify and modify vertex data. The raw array managed
    /// by the class can then easily be uploaded to a vertex buffer.
    /// 
    /// **Premultiplied Alpha**
    /// 
    /// The color values of texture files may contain premultiplied alpha values, which
    /// means that the `RGB` values were multiplied with the `alpha` value
    /// before saving them. On rendering, it makes a difference in which way the alpha value is saved;
    /// for that reason, the VertexData class mimics this behavior. You can choose how the alpha
    /// values should be handled via the `premultipliedAlpha` property.
    /// </summary>
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
        /// Indicates if any vertices have a non-white color or are not fully opaque.
        /// </summary>
        public bool Tinted
        {
            get
            {
                for (int i = 0; i < _numVertices; ++i)
                {
                    if (!VertexColorHelper.IsOpaqueWhite(_vertexColors[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
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
        /// Copies the vertex data of this instance to another vertex data object, starting at element 0.
        /// </summary>
        public void CopyToVertexData(VertexData target, bool copyColor)
        {
            CopyToVertexData(target, 0, _numVertices, copyColor);
        }

        /// <summary>
        /// Copies the vertex data of this instance to another vertex data object, starting at a certain index.
        /// </summary>
        public void CopyToVertexData(VertexData target, int atIndex, bool copyColor)
        {
            CopyToVertexData(target, atIndex, _numVertices, copyColor);
        }

        /// <summary>
        /// Copies a range of vertices of this instance to another vertex data object.
        /// </summary>
        public void CopyToVertexData(VertexData target, int atIndex, int numVertices, bool copyColor)
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
        public Point PositionAtIndex(int index)
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
        public Point TexCoordsAtIndex(int index)
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

            alpha = NumberUtil.Clamp(alpha, _premultipliedAlpha ? MIN_ALPHA : 0.0f, 1.0f); 

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
            float alpha = AlphaAtIndex(atIndex);
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
        public uint ColorAtIndex(int index)
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
            uint color = ColorAtIndex(atIndex);
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
        public float AlphaAtIndex(int index)
        {
            if (index < 0 || index >= _numVertices)
            {
                throw new IndexOutOfRangeException("Invalid vertex index");
            }

            //            return _vertices[index].Color.A / 255.0f;
            return 1.0f;
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

            int minAlpha = _premultipliedAlpha ? (int)(MIN_ALPHA * 255.0) : 0;

            for (int i = index; i < index + numVertices; ++i)
            {
                VertexColor vertexColor = _vertexColors[i];
                byte newAlpha = Convert.ToByte(NumberUtil.Clamp(vertexColor.A * factor, minAlpha, 255));

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
        public void TransformVerticesWithMatrix(Matrix matrix, int atIndex, int numVertices)
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