using System;
using Sparrow.Geom;
using SparrowSharp.Core.Rendering;
using System.Collections.Generic;
using SparrowSharp.Core.Styles;

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
        private byte[] _rawData;
        private int _numVertices;
        private VertexDataFormat _format;
        private List<VertexDataAttribute> _attributes;
        private int _numAttributes;
        private bool _premultipliedAlpha;
        private bool _tinted;

        private int _posOffset;  // in bytes
        private int _colOffset;  // in bytes
        private int _vertexSize; // in bytes

        // helper objects
        private static Point sHelperPoint = Point.Create();
        private static byte[] sBytes = new byte[0];

        public VertexData(int initialCapacity = 32)
        {
            _format = MeshStyle.VERTEX_FORMAT;
            Init(initialCapacity);
        }

        public VertexData(string format, int initialCapacity= 32)
        {
            _format = VertexDataFormat.FromString(format);
            Init(initialCapacity);
        }

        public VertexData(VertexDataFormat format, int initialCapacity = 32)
        {
            _format = format;
            Init(initialCapacity);
        }

        private void Init(int initialCapacity)
        {
            if (_format == null) _format = MeshStyle.VERTEX_FORMAT;
            _attributes = _format.Attributes;
            _numAttributes = _attributes.Count;
            _posOffset = _format.HasAttribute("position") ? _format.GetOffset("position") : 0;
            _colOffset = _format.HasAttribute("color") ? _format.GetOffset("color") : 0;
            _vertexSize = _format.VertexSize;
            _numVertices = 0;
            _premultipliedAlpha = true;
            _rawData = new byte[0];
            _rawData.endian = sBytes.endian = Endian.LITTLE_ENDIAN;
        }

        /** Explicitly frees up the memory used by the ByteArray. */
        public void Clear()
        {
            _rawData.Clear();
            _numVertices = 0;
            _tinted = false;
        }

        /** Creates a duplicate of the vertex data object. */
        public VertexData clone()
        {
            VertexData clone = new VertexData(_format, _numVertices);
            clone._rawData.writeBytes(_rawData);
            clone._numVertices = _numVertices;
            clone._premultipliedAlpha = _premultipliedAlpha;
            clone._tinted = _tinted;
            return clone;
        }

        /** Copies the vertex data (or a range of it, defined by 'vertexID' and 'numVertices')
         *  of this instance to another vertex data object, starting at a certain target index.
         *  If the target is not big enough, it will be resized to fit all the new vertices.
         *
         *  <p>If you pass a non-null matrix, the 2D position of each vertex will be transformed
         *  by that matrix before storing it in the target object. (The position being either an
         *  attribute with the name "position" or, if such an attribute is not found, the first
         *  attribute of each vertex. It must consist of two float values containing the x- and
         *  y-coordinates of the vertex.)</p>
         *
         *  <p>Source and target do not need to have the exact same format. Only properties that
         *  exist in the target will be copied; others will be ignored. If a property with the
         *  same name but a different format exists in the target, an exception will be raised.
         *  Beware, though, that the copy-operation becomes much more expensive when the formats
         *  differ.</p>
         */
        public void CopyTo(VertexData target, int targetVertexID = 0, Matrix matrix= null,
                           int vertexID = 0, int numVertices = -1)
        {
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            if (_format == target._format)
            {
                if (target._numVertices<targetVertexID + numVertices)
                    target._numVertices = targetVertexID + numVertices;

                target._tinted = target._tinted || _tinted;

                // In this case, it's fastest to copy the complete range in one call
                // and then overwrite only the transformed positions.

                ByteArray targetRawData = target._rawData;
                targetRawData.position = targetVertexID* _vertexSize;
                targetRawData.writeBytes(_rawData, vertexID* _vertexSize, numVertices * _vertexSize);

                if (matrix != null)
                {
                    float x, y;
                    int pos = targetVertexID* _vertexSize + _posOffset;
                    int endPos = pos + (numVertices* _vertexSize);

                    while (pos<endPos)
                    {
                        targetRawData.position = pos;
                        x = targetRawData.readFloat();
                        y = targetRawData.readFloat();

                        targetRawData.position = pos;
                        targetRawData.writeFloat(matrix.a* x + matrix.c* y + matrix.tx);
                        targetRawData.writeFloat(matrix.d* y + matrix.b* x + matrix.ty);

                        pos += _vertexSize;
                    }
                }
            }
            else
            {
                if (target._numVertices<targetVertexID + numVertices)
                    target.NumVertices  = targetVertexID + numVertices; // ensure correct alphas!

                for (int i =0; i < _numAttributes; ++i)
                {
                    VertexDataAttribute srcAttr = _attributes[i];
                    VertexDataAttribute tgtAttr = target.GetAttribute(srcAttr.Name);

                    if (tgtAttr != null) // only copy attributes that exist in the target, as well
                    {
                        if (srcAttr.Offset == _posOffset)
                            CopyAttributeTo_internal(target, targetVertexID, matrix,
                                    srcAttr, tgtAttr, vertexID, numVertices);
                        else
                            CopyAttributeTo_internal(target, targetVertexID, null,
                                    srcAttr, tgtAttr, vertexID, numVertices);
                    }
                }
            }
        }

        /** Copies a specific attribute of all contained vertices (or a range of them, defined by
         *  'vertexID' and 'numVertices') to another VertexData instance. Beware that both name
         *  and format of the attribute must be identical in source and target.
         *  If the target is not big enough, it will be resized to fit all the new vertices.
         *
         *  <p>If you pass a non-null matrix, the specified attribute will be transformed by
         *  that matrix before storing it in the target object. It must consist of two float
         *  values.</p>
         */
        public void CopyAttributeTo(VertexData target, int targetVertexID, string attrName,
                                    Matrix matrix = null, int vertexID = 0, int numVertices= -1)
        {
            VertexDataAttribute sourceAttribute = GetAttribute(attrName);
            VertexDataAttribute targetAttribute = target.GetAttribute(attrName);

            if (sourceAttribute == null)
                throw new ArgumentException("Attribute '" + attrName + "' not found in source data");

            if (targetAttribute == null)
                throw new ArgumentException("Attribute '" + attrName + "' not found in target data");

            if (sourceAttribute.IsColor)
                target._tinted = _tinted || _tinted;

            CopyAttributeTo_internal(target, targetVertexID, matrix,
                    sourceAttribute, targetAttribute, vertexID, numVertices);
        }

        private void CopyAttributeTo_internal(
               VertexData target, int targetVertexID, Matrix matrix,
               VertexDataAttribute sourceAttribute, VertexDataAttribute targetAttribute,
               int vertexID, int numVertices)
        {
            if (sourceAttribute.Format != targetAttribute.Format)
                throw new InvalidOperationException("Attribute formats differ between source and target");

            if (numVertices < 0 || vertexID + numVertices > _numVertices)
                numVertices = _numVertices - vertexID;

            if (target._numVertices<targetVertexID + numVertices)
                target._numVertices = targetVertexID + numVertices;

            int i, j;
            float x, y;
            ByteArray sourceData = _rawData;
            ByteArray targetData = target._rawData;
            int sourceDelta = _vertexSize - sourceAttribute.Size;
            int targetDelta = target._vertexSize - targetAttribute.Size;
            int attributeSizeIn32Bits = sourceAttribute.Size / 4;

            sourceData.position = vertexID* _vertexSize + sourceAttribute.Offset;
            targetData.position = targetVertexID* target._vertexSize + targetAttribute.Offset;

            if (matrix != null)
            {
                for (i=0; i<numVertices; ++i)
                {
                    x = sourceData.readFloat();
                    y = sourceData.readFloat();

                    targetData.writeFloat(matrix.A * x + matrix.C * y + matrix.Tx);
                    targetData.writeFloat(matrix.D * y + matrix.B * x + matrix.Ty);

                    sourceData.position += sourceDelta;
                    targetData.position += targetDelta;
                }
            }
            else
            {
                for (i = 0; i<numVertices; ++i)
                {
                    for (j = 0; j < attributeSizeIn32Bits; ++j)
                        targetData.writeUnsignedInt(sourceData.readUnsignedInt());

                    sourceData.position += sourceDelta;
                    targetData.position += targetDelta;
                }
            }
        }

        /** Returns a string representation of the VertexData object,
         *  describing both its format and size. */
        override public string ToString()
        {
            return String.Format("[VertexData format=\"{0}\" numVertices={1}]",
                    _format.FormatString, _numVertices);
        }

        // read / write attributes

        /** Reads an unsigned integer value from the specified vertex and attribute. */
        public uint GetUnsignedInt(int vertexID, string attrName)
        {
            _rawData.position = vertexID * _vertexSize + GetAttribute(attrName).Offset;
            return _rawData.readUnsignedInt();
        }

        /** Writes an unsigned integer value to the specified vertex and attribute. */
        public void SetUnsignedInt(int vertexID, string attrName, uint value)
        {
            if (_numVertices<vertexID + 1)
                NumVertices = vertexID + 1;

            _rawData.position = vertexID * _vertexSize + GetAttribute(attrName).Offset;
            _rawData.writeUnsignedInt(value);
        }

        /** Reads a float value from the specified vertex and attribute. */
        public float GetFloat(int vertexID, string attrName)
        {
            _rawData.position = vertexID * _vertexSize + GetAttribute(attrName).Offset;
            return _rawData.readFloat();
        }

        /** Writes a float value to the specified vertex and attribute. */
        public void SetFloat(int vertexID, string attrName, float value)
        {
            if (_numVertices < vertexID + 1)
                 NumVertices = vertexID + 1;

            _rawData.position = vertexID* _vertexSize + GetAttribute(attrName).Offset;
            _rawData.writeFloat(value);
        }

        /** Reads a Point from the specified vertex and attribute. */
        public Point GetPoint(int vertexID, string attrName, Point outP= null)
        {
            outP = Point.Create();

            var offset = attrName == "position" ? _posOffset : GetAttribute(attrName).Offset;
            _rawData.position = vertexID * _vertexSize + offset;
            outP.X = _rawData.readFloat();
            outP.Y = _rawData.readFloat();

            return outP;
        }

        /** Writes the given coordinates to the specified vertex and attribute. */
        public void SetPoint(int vertexID, string attrName, float x, float y)
        {
            if (_numVertices < vertexID + 1)
                 NumVertices = vertexID + 1;

            int  offset = attrName == "position" ? _posOffset : GetAttribute(attrName).Offset;
            _rawData.position = vertexID * _vertexSize + offset;
            _rawData.writeFloat(x);
            _rawData.writeFloat(y);
        }

        /** Reads an RGB color from the specified vertex and attribute (no alpha). */
        public uint GetColor(int vertexID, string attrName = "color")
        {
            int offset = attrName == "color" ? _colOffset : GetAttribute(attrName).Offset;
            _rawData.position = vertexID* _vertexSize + offset;
            uint rgba = SwitchEndian(_rawData.readUnsignedInt());
            if (_premultipliedAlpha) rgba = UnmultiplyAlpha(rgba);
            return (rgba >> 8) & 0xffffff;
        }

        /** Writes the RGB color to the specified vertex and attribute (alpha is not changed). */
        public void SetColor(int vertexID, string attrName, uint color)
        {
            if (_numVertices<vertexID + 1)
                 NumVertices = vertexID + 1;

            float alpha = GetAlpha(vertexID, attrName);
            Colorize(attrName, color, alpha, vertexID, 1);
        }

        /** Reads the alpha value from the specified vertex and attribute. */
        public float GetAlpha(int vertexID, string attrName= "color")
        {
            int offset = attrName == "color" ? _colOffset : GetAttribute(attrName).Offset;
            _rawData.position = vertexID* _vertexSize + offset;
            uint rgba = SwitchEndian(_rawData.readUnsignedInt());
            return (rgba & 0xff) / 255.0f;
        }

        /** Writes the given alpha value to the specified vertex and attribute (range 0-1). */
        public void SetAlpha(int vertexID, string attrName, float alpha)
        {
            if (_numVertices<vertexID + 1)
                 NumVertices = vertexID + 1;

            uint color = GetColor(vertexID, attrName);
            Colorize(attrName, color, alpha, vertexID, 1);
        }

        // bounds helpers

        /** Calculates the bounds of the 2D vertex positions identified by the given name.
         *  The positions may optionally be transformed by a matrix before calculating the bounds.
         *  If you pass an 'out' Rectangle, the result will be stored in this rectangle
         *  instead of creating a new object. To use all vertices for the calculation, set
         *  'numVertices' to '-1'. */
        public Rectangle GetBounds(string attrName = "position", Matrix matrix = null,
                                   int vertexID = 0, int numVertices = -1)
        {
            Rectangle outR = new Rectangle();
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            if (numVertices == 0)
            {
                if (matrix == null)
                    outR.Empty();
                else
                {
                    sHelperPoint = matrix.TransformPoint(0, 0);
                    outR.SetTo(sHelperPoint.X, sHelperPoint.Y, 0, 0);
                }
            }
            else
            {
                float minX = float.MaxValue, maxX = -float.MaxValue;
                float minY = float.MaxValue, maxY = -float.MaxValue;
                int offset = attrName == "position" ? _posOffset : GetAttribute(attrName).Offset;
                int position = vertexID* _vertexSize + offset;
                float x, y;
                int i;

                if (matrix == null)
                {
                    for (i=0; i<numVertices; ++i)
                    {
                        _rawData.position = position;
                        x = _rawData.readFloat();
                        y = _rawData.readFloat();
                        position += _vertexSize;

                        if (minX > x) minX = x;
                        if (maxX<x) maxX = x;
                        if (minY > y) minY = y;
                        if (maxY<y) maxY = y;
                    }
                }
                else
                {
                    for (i=0; i<numVertices; ++i)
                    {
                        _rawData.position = position;
                        x = _rawData.readFloat();
                        y = _rawData.readFloat();
                        position += _vertexSize;

                        sHelperPoint = matrix.TransformPoint(x, y);

                        if (minX > sHelperPoint.X) minX = sHelperPoint.X;
                        if (maxX<sHelperPoint.X) maxX = sHelperPoint.X;
                        if (minY > sHelperPoint.Y) minY = sHelperPoint.Y;
                        if (maxY<sHelperPoint.Y) maxY = sHelperPoint.Y;
                    }
                }

                outR.SetTo(minX, minY, maxX - minX, maxY - minY);
            }

            return outR;
        }


        /** Indicates if color attributes should be stored premultiplied with the alpha value.
         *  Changing this value does <strong>not</strong> modify any existing color data.
         *  If you want that, use the <code>setPremultipliedAlpha</code> method instead.
         *  @default true */
        public bool PremultipledAlpha
        {
            get { return _premultipliedAlpha;  }
            set { SetPremultipliedAlpha(value, false); }
        }

        /** Changes the way alpha and color values are stored. Optionally updates all existing
         *  vertices. */
        public void SetPremultipliedAlpha(bool value, bool updateData)
        {
            if (updateData && value != _premultipliedAlpha)
            {
                for (int  i =0; i < _numAttributes; ++i)
                {
                    VertexDataAttribute attribute = _attributes[i];
                    if (attribute.IsColor)
                    {
                        int pos = attribute.Offset;
                        uint oldColor;
                        uint newColor;

                        for (int j = 0; j < _numVertices; ++j)
                        {
                            _rawData.position = pos;
                            oldColor = SwitchEndian(_rawData.readUnsignedInt());
                            newColor = value? PremultiplyAlpha(oldColor) : UnmultiplyAlpha(oldColor);

                            _rawData.position = pos;
                            _rawData.writeUnsignedInt(SwitchEndian(newColor));

                            pos += _vertexSize;
                        }
                    }
                }
            }
            _premultipliedAlpha = value;
        }

        // modify multiple attributes

        /** Transforms the 2D positions of subsequent vertices by multiplication with a
         *  transformation matrix. */
        public void TransformPoints(string attrName, Matrix matrix,
                                    int vertexID = 0, int numVertices= -1)
        {
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            float x, y;
            int offset = attrName == "position" ? _posOffset : GetAttribute(attrName).Offset;
            int pos = vertexID* _vertexSize + offset;
            int endPos = pos + numVertices* _vertexSize;

            while (pos < endPos)
            {
                _rawData.position = pos;
                x = _rawData.readFloat();
                y = _rawData.readFloat();

                _rawData.position = pos;
                _rawData.writeFloat(matrix.A * x + matrix.C* y + matrix.Tx);
                _rawData.writeFloat(matrix.D * y + matrix.B* x + matrix.Ty);

                pos += _vertexSize;
            }
        }

        /** Translates the 2D positions of subsequent vertices by a certain offset. */
        public void TranslatePoints(string attrName, float deltaX, float deltaY,
                                    int vertexID = 0, int numVertices= -1)
        {
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            float x, y;
            int offset = attrName == "position" ? _posOffset : GetAttribute(attrName).Offset;
            int pos = vertexID* _vertexSize + offset;
            int endPos = pos + numVertices* _vertexSize;

            while (pos<endPos)
            {
                _rawData.position = pos;
                x = _rawData.readFloat();
                y = _rawData.readFloat();

                _rawData.position = pos;
                _rawData.writeFloat(x + deltaX);
                _rawData.writeFloat(y + deltaY);

                pos += _vertexSize;
            }
        }

        /** Multiplies the alpha values of subsequent vertices by a certain factor. */
        public void ScaleAlphas(string attrName, float factor,
                                int vertexID = 0, int numVertices = -1)
        {
            if (factor == 1.0) return;
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            _tinted = true; // factor must be != 1, so there's definitely tinting.

            int i;
            int offset = attrName == "color" ? _colOffset : GetAttribute(attrName).offset;
            int colorPos = vertexID* _vertexSize + offset;
            int alphaPos;
            float alpha;
            uint rgba;

            for (i=0; i<numVertices; ++i)
            {
                alphaPos = colorPos + 3;
                alpha = _rawData[alphaPos] / 255.0f * factor;

                if (alpha > 1.0)      alpha = 1.0f;
                else if (alpha< 0.0) alpha = 0.0f;

                if (alpha == 1.0 || !_premultipliedAlpha)
                {
                    _rawData[alphaPos] = int(alpha* 255.0f);
                }
                else
                {
                    _rawData.position = colorPos;
                    rgba = UnmultiplyAlpha(SwitchEndian(_rawData.readUnsignedInt()));
                    rgba = (rgba & 0xffffff00) | (int(alpha* 255.0f) & 0xff);
                    rgba = PremultiplyAlpha(rgba);

                    _rawData.position = colorPos;
                    _rawData.writeUnsignedInt(SwitchEndian(rgba));
                }

                colorPos += _vertexSize;
            }
        }

        /** Writes the given RGB and alpha values to the specified vertices. */
        public void Colorize(string attrName = "color", uint color = 0xffffff, float alpha = 1.0f,
                             int vertexID = 0, int numVertices= -1)
        {
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            int offset = attrName == "color" ? _colOffset : GetAttribute(attrName).Offset;
            int pos = vertexID* _vertexSize + offset;
            int endPos = pos + (numVertices* _vertexSize);

            if (alpha > 1.0)      alpha = 1.0f;
            else if (alpha< 0.0) alpha = 0.0f;

            uint rgba = ((color << 8) & 0xffffff00) | ((uint)(alpha * 255.0) & 0xff);

            if (rgba == 0xffffffff && numVertices == _numVertices) _tinted = false;
            else if (rgba != 0xffffffff) _tinted = true;

            if (_premultipliedAlpha && alpha != 1.0) rgba = PremultiplyAlpha(rgba);

            _rawData.position = vertexID* _vertexSize + offset;
            _rawData.writeUnsignedInt(SwitchEndian(rgba));

            while (pos<endPos)
            {
                _rawData.position = pos;
                _rawData.writeUnsignedInt(SwitchEndian(rgba));
                pos += _vertexSize;
            }
        }

        // format helpers

        /** Returns the format of a certain vertex attribute, identified by its name.
          * Typical values: <code>float1, float2, float3, float4, bytes4</code>. */
        public string getFormat(string attrName)
        {
            return GetAttribute(attrName).Format;
        }

        /** Returns the size of a certain vertex attribute in bytes. */
        public int getSize(string attrName)
        {
            return GetAttribute(attrName).Size;
        }

        /** Returns the size of a certain vertex attribute in 32 bit units. */
        public int getSizeIn32Bits(string attrName)
        {
            return GetAttribute(attrName).Size / 4;
        }

        /** Returns the offset (in bytes) of an attribute within a vertex. */
        public int getOffset(string attrName)
        {
            return GetAttribute(attrName).Offset;
        }

        /** Returns the offset (in 32 bit units) of an attribute within a vertex. */
        public int getOffsetIn32Bits(string attrName)
        {
            return GetAttribute(attrName).Offset / 4;
        }

        /** Indicates if the VertexData instances contains an attribute with the specified name. */
        public bool HasAttribute(string attrName)
        {
            return GetAttribute(attrName) != null;
        }

        // VertexBuffer helpers

        /** Creates a vertex buffer object with the right size to fit the complete data.
         *  Optionally, the current data is uploaded right away. */
        public VertexBuffer3D CreateVertexBuffer(bool upload = false,
                                                 string bufferUsage = "staticDraw")
        {
            Context3D context = Starling.context;
            if (context == null) throw new Exception("missing context");
            if (_numVertices == 0) return null;

            VertexBuffer3D buffer = context.createVertexBuffer(
                _numVertices, _vertexSize / 4, bufferUsage);

            if (upload) uploadToVertexBuffer(buffer);
            return buffer;
        }

        /** Uploads the complete data (or a section of it) to the given vertex buffer. */
        public void UploadToVertexBuffer(VertexBuffer3D buffer, int vertexID = 0, int numVertices = -1)
        {
            if (numVertices< 0 || vertexID + numVertices> _numVertices)
                numVertices = _numVertices - vertexID;

            if (numVertices > 0)
                buffer.uploadFromByteArray(_rawData, 0, vertexID, numVertices);
        }

        private VertexDataAttribute GetAttribute(string attrName)
        {
            int i;
            VertexDataAttribute attribute;

            for (i=0; i<_numAttributes; ++i)
            {
                attribute = _attributes[i];
                if (attribute.Name == attrName) return attribute;
            }
            return null;
        }
        
        private static uint SwitchEndian(uint value)
        {
            return ( value        & 0xff) << 24 |
                   ((value >>  8) & 0xff) << 16 |
                   ((value >> 16) & 0xff) <<  8 |
                   ((value >> 24) & 0xff);
        }

        private static uint PremultiplyAlpha(uint rgba)
        {
            uint alpha = rgba & 0xff;

            if (alpha == 0xff) return rgba;
            else
            {
                float factor = alpha / 255.0f;
                uint r = (uint)(((rgba >> 24) & 0xff) * factor);
                uint g = (uint)(((rgba >> 16) & 0xff) * factor);
                uint b = (uint)(((rgba >>  8) & 0xff) * factor);

                return (r & 0xff) << 24 |
                       (g & 0xff) << 16 |
                       (b & 0xff) <<  8 | alpha;
            }
        }

        private static uint UnmultiplyAlpha(uint rgba)
        {
            uint alpha = rgba & 0xff;

            if (alpha == 0xff || alpha == 0x0) return rgba;
            else
            {
                float factor = alpha / 255.0f;
                uint r = (uint)(((rgba >> 24) & 0xff) / factor);
                uint g = (uint)(((rgba >> 16) & 0xff) / factor);
                uint b = (uint)(((rgba >>  8) & 0xff) / factor);

                return (r & 0xff) << 24 |
                       (g & 0xff) << 16 |
                       (b & 0xff) <<  8 | alpha;
            }
        }

        // properties

        /** The total number of vertices. If you make the object bigger, it will be filled up with
         *  <code>1.0</code> for all alpha values and zero for everything else. */
         public int NumVertices
        {
            get { return _numVertices; }
            set
            {
                if (value > _numVertices)
                {
                    int oldLength = _numVertices * VertexSize;
                    int newLength = value * _vertexSize;

                    if (_rawData.length > oldLength)
                    {
                        _rawData.position = oldLength;
                        while (_rawData.bytesAvailable) _rawData.writeUnsignedInt(0);
                    }

                    if (_rawData.length < newLength)
                        _rawData.length = newLength;

                    for (int i = 0; i < _numAttributes; ++i)
                    {
                        VertexDataAttribute attribute = _attributes[i];
                        if (attribute.IsColor) // initialize color values with "white" and full alpha
                        {
                            int pos = _numVertices * _vertexSize + attribute.Offset;
                            for (int j = _numVertices; j < value; ++j)
                        {
                                _rawData.position = pos;
                                _rawData.writeUnsignedInt(0xffffffff);
                                pos += _vertexSize;
                            }
                        }
                    }
                }
                if (value == 0) _tinted = false;
                _numVertices = value;
            }
        }

        /** The raw vertex data; not a copy! */
        public byte[] RawData { get { return _rawData; } }

        /** The format that describes the attributes of each vertex.
         *  When you assign a different format, the raw data will be converted accordingly,
         *  i.e. attributes with the same name will still point to the same data.
         *  New properties will be filled up with zeros (except for colors, which will be
         *  initialized with an alpha value of 1.0). As a side-effect, the instance will also
         *  be trimmed. */
         public VertexDataFormat Format
         {
            get { return _format; }
            set
            {
                if (_format == value) return;

                int a, i, pos;
                int srcVertexSize = _format.VertexSize;
                int tgtVertexSize = value.VertexSize;
                int numAttributes = value.NumAttributes;

                sBytes.length = value.VertexSize * _numVertices;

                for (a = 0; a < numAttributes; ++a)
                {
                    VertexDataAttribute tgtAttr = value.Attributes[a];
                    VertexDataAttribute srcAttr = GetAttribute(tgtAttr.Name);

                    if (srcAttr != null) // copy attributes that exist in both targets
                    {
                        pos = tgtAttr.Offset;

                        for (i = 0; i < _numVertices; ++i)
                        {
                            sBytes.position = pos;
                            sBytes.writeBytes(_rawData, srcVertexSize * i + srcAttr.Offset, srcAttr.Size);
                            pos += tgtVertexSize;
                        }
                    }
                    else if (tgtAttr.IsColor) // initialize color values with "white" and full alpha
                    {
                        pos = tgtAttr.Offset;

                        for (i = 0; i < _numVertices; ++i)
                        {
                            sBytes.position = pos;
                            sBytes.writeUnsignedInt(0xffffffff);
                            pos += tgtVertexSize;
                        }
                    }
                }

                _rawData.clear();
                _rawData.length = sBytes.length;
                _rawData.writeBytes(sBytes);
                sBytes.clear();

                _format = value;
                _attributes = _format.Attributes;
                _numAttributes = _attributes.Count;
                _vertexSize = _format.VertexSize;
                _posOffset = _format.HasAttribute("position") ? _format.GetOffset("position") : 0;
                _colOffset = _format.HasAttribute("color") ? _format.GetOffset("color") : 0;
            }
        }

        /** The format string that describes the attributes of each vertex. */
        public string FormatString { get { return _format.FormatString; } }

        /** The size (in bytes) of each vertex. */
        public int VertexSize { get { return _vertexSize; } }

        /** The size (in 32 bit units) of each vertex. */
        public int VertexSizeIn32Bits { get { return _vertexSize / 4; } }

        /** The size (in bytes) of the raw vertex data. */
        public int Size { get { return _numVertices * _vertexSize; } }

        /** The size (in 32 bit units) of the raw vertex data. */
        public int SizeIn32Bits { get { return _numVertices * _vertexSize / 4; } }

    }
}