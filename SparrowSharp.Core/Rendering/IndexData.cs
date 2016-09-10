
using System;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#endif

namespace SparrowSharp.Core.Rendering
{
    /// <summary>
    /// The IndexData class manages a raw list of vertex indices, allowing direct upload
    /// to OpenGL index buffers. <em>You only have to work with this class if you're writing
    /// your own rendering code (e.g. if you create custom display objects).</em>
    ///
    /// <p>To render objects with OpenGL, you have to organize vertices and indices in so-called
    /// vertex- and index-buffers. Vertex buffers store the coordinates of the vertices that make
    /// up an object; index buffers reference those vertices to determine which vertices spawn
    /// up triangles. Those buffers reside in graphics memory and can be accessed very
    /// efficiently by the GPU.</p>
    ///
    /// <p>Before you can move data into the buffers, you have to set it up in conventional
    /// memory — that is, in a Vector or a ByteArray. Since it's quite cumbersome to manually
    /// create and manipulate those data structures, the IndexData and VertexData classes provide
    /// a simple way to do just that. The data is stored in a Byte Array (one index or vertex after
    /// the other) that can easily be uploaded to a buffer.</p>
    /// 
    /// Basic Quad Layout
    ///
    ///  <p>In many cases, the indices we are working with will reference just quads, i.e.
    /// triangles composing rectangles. That means that many IndexData instances will contain
    /// similar or identical data — a great opportunity for optimization!</p>
    ///
    ///  <p>If an IndexData instance follows a specific layout, it will be recognized
    /// automatically and many operations can be executed much faster. In Starling, that
    /// layout is called "basic quad layout". In order to recognize this specific sequence,
    ///  the indices of each quad have to use the following order:</p>
    ///
    ///  <pre>n, n+1, n+2, n+1, n+3, n+2</pre>
    ///
    ///  <p>The subsequent quad has to use <code>n+4</code> as starting value, the next one
    ///  <code>n+8</code>, etc. Here is an example with 3 quads / 6 triangles:</p>
    ///
    ///  <pre>0, 1, 2, 1, 3, 2,   4, 5, 6, 5, 7, 6,   8, 9, 10, 9, 11, 10</pre>
    ///
    ///  <p>If you are describing quad-like meshes, make sure to always use this layout.</p>
    ///
    ///  @see VertexData
    ///</summary>
    public class IndexData
    {

        private short[] _rawData; 
        private int _numIndices;
        private bool _useQuadLayout;

        // basic quad layout
        private static short[] sQuadData = new short[0];
        private static uint sQuadDataNumIndices = 0;

        // helper objects
        private static short[] sVector = new short[0];
        private static short[] sTrimData = new short[0];

        /// <summary>
        /// Creates an empty IndexData instance.
        /// </summary>
        public IndexData()
        {
            _numIndices = 0;
            _useQuadLayout = true;
        }

        /// <summary>
        /// Explicitly frees up the memory, thus removing all indices.
        /// Quad layout will be restored (until adding data violating that layout).
        /// </summary>
        public void Clear()
        {
            if (_rawData != null)
            {
                Array.Resize(ref _rawData, 0);
            }
            _numIndices = 0;
            _useQuadLayout = true;
        }

        /// <summary>
        /// Creates a duplicate of the IndexData object.
        /// </summary>
        public IndexData Clone()
        {
            var clone = new IndexData();
            clone._numIndices = _numIndices;
            if (!_useQuadLayout)
            {
                clone.SwitchToGenericData();
                Array.Copy(_rawData, clone._rawData, _rawData.Length);
            }
            return clone;
        }

        /// <summary>
        /// Copies the index data (or a range of it, defined by 'indexID' and 'numIndices')
        /// of this instance to another IndexData object, starting at a certain target index.
        /// If the target is not big enough, it will grow to fit all the new indices.
        ///
        /// <p>By passing a non-zero <code>offset</code>, you can raise all copied indices
        /// by that value in the target object.</p>
        /// </summary>
        public void CopyTo(IndexData target, int targetIndexID = 0, int offset = 0,
                           int indexID = 0, int numIndices = -1)
        {
            if (numIndices< 0 || indexID + numIndices> _numIndices)
            {
                numIndices = _numIndices - indexID;
            }

            short[] sourceData;
            int newNumIndices = targetIndexID + numIndices;

            if (target._numIndices < newNumIndices)
            {
                target._numIndices = newNumIndices; 
                if (sQuadDataNumIndices < newNumIndices)
                {
                    EnsureQuadDataCapacity(newNumIndices);
                }
            }

            if (_useQuadLayout)
            {
                if (target._useQuadLayout)
                {
                    bool keepsQuadLayout = true;
                    int distance = targetIndexID - indexID;
                    int distanceInQuads = distance / 6;
                    int offsetInQuads = offset / 4;

                    // This code is executed very often. If it turns out that both IndexData
                    // instances use a quad layout, we don't need to do anything here.
                    //
                    // When "distance / 6 == offset / 4 && distance % 6 == 0 && offset % 4 == 0",
                    // the copy operation preserves the quad layout. In that case, we can exit
                    // right away. The code below is a little more complex, though, to avoid the
                    // (surprisingly costly) mod-operations.
                    if (distanceInQuads == offsetInQuads && (offset & 3) == 0 &&
                        distanceInQuads * 6 == distance)
                    {
                        keepsQuadLayout = true;
                    }
                    else if (numIndices > 2)
                    {
                        keepsQuadLayout = false;
                    }
                    else
                    {
                        for (int i = 0; i < numIndices; ++i)
                        {
                            keepsQuadLayout = keepsQuadLayout &&
                               (GetBasicQuadIndexAt(indexID + i) + offset == GetBasicQuadIndexAt(targetIndexID + i));
                        }
                    }

                    if (keepsQuadLayout) return;
                    else target.SwitchToGenericData();
                }

                sourceData = sQuadData;

                if ((offset & 3) == 0) // => offset % 4 == 0
                {
                    indexID += 6 * offset / 4;
                    offset = 0;
                    EnsureQuadDataCapacity(indexID + numIndices);
                }
            }
            else
            {
                if (target._useQuadLayout)
                {
                    target.SwitchToGenericData();
                }
                sourceData = _rawData;
            }
            
            if (target._rawData.Length < targetIndexID + numIndices)
            {
                Array.Resize(ref target._rawData, targetIndexID + numIndices);
            }
            if (offset == 0)
            {
                Array.Copy(sourceData, indexID, target._rawData, targetIndexID, numIndices);
            }
            else
            {
                int pos = indexID;
                int targetPos = targetIndexID;
                for (int i = 0; i < numIndices; i++)
                {
                    target._rawData[targetPos] = (short)(sourceData[pos] + offset);
                    pos++;
                    targetPos++;
                }
            }
        }

        /// <summary>
        /// Sets an index at the specified position.
        /// </summary>
        public void SetIndex(int indexID, int index)
        {
            if (_numIndices < indexID + 1)
            {
                NumIndices = indexID + 1;
            }
            if (_useQuadLayout)
            {
                if (GetBasicQuadIndexAt(indexID) == index)
                {
                    return;
                }
                else
                {
                    SwitchToGenericData();
                }
            }
            _rawData[indexID] = (short)index;
        }

        /// <summary>
        /// Reads the index from the specified position.
        /// </summary>
        public int GetIndex(int indexID)
        {
            if (_useQuadLayout)
            {
                if (indexID < _numIndices)
                {
                    return GetBasicQuadIndexAt(indexID);
                }
                else
                {
                    throw new IndexOutOfRangeException();
                } 
            }
            else
            {
                return _rawData[indexID];
            }
        }

        /// <summary>
        /// Adds an offset to all indices in the specified range
        /// </summary>
        public void OffsetIndices(int offset , int indexID = 0, int numIndices = -1)
        {
            if (numIndices< 0 || indexID + numIndices> _numIndices)
            {
                numIndices = _numIndices - indexID;
            }
            int endIndex = indexID + numIndices;
            for (int i = indexID; i < endIndex; ++i)
            {
                SetIndex(i, GetIndex(i) + offset);
            }
        }

        /// <summary>
        /// Appends three indices representing a triangle. Reference the vertices clockwise,
        /// as this defines the front side of the triangle.
        /// </summary>
        public void AddTriangle(short a, short b, short c)
        {
            if (_useQuadLayout)
            {
                if (a == GetBasicQuadIndexAt(_numIndices))
                {
                    bool oddTriangleID = (_numIndices & 1) != 0;
                    bool evenTriangleID = !oddTriangleID;

                    if ((evenTriangleID && b == a + 1 && c == b + 1) ||
                         (oddTriangleID && c == a + 1 && b == c + 1))
                    {
                        _numIndices += 3;
                        EnsureQuadDataCapacity(_numIndices);
                        return;
                    }
                }
                SwitchToGenericData();
            }
            Array.Resize(ref _rawData, _rawData.Length + 3);
            int pos = _numIndices;
            _rawData[pos] = a;
            pos++;
            _rawData[pos] = b;
            pos++;
            _rawData[pos] = c;
            _numIndices += 3;
        }

        /// <summary>
        /// Appends two triangles spawning up the quad with the given indices.
        ///  The indices of the vertices are arranged like this:
        ///
        ///  <pre>
        ///  a - b
        ///  | / |
        ///  c - d
        ///  </pre>
        ///
        ///  <p>To make sure the indices will follow the basic quad layout, make sure each
        /// parameter increments the one before it(e.g. <code>0, 1, 2, 3</code>).</p>
        /// </summary>
        public void addQuad(short a, short b, short c, short d)
        {
            if (_useQuadLayout)
            {
                if (a == GetBasicQuadIndexAt(_numIndices) &&
                    b == a + 1 && c == b + 1 && d == c + 1)
                {
                    _numIndices += 6;
                    EnsureQuadDataCapacity(_numIndices);
                    return;
                }
                else
                {
                    SwitchToGenericData();
                }
            }
            Array.Resize(ref _rawData, _rawData.Length + 6);
            int pos = _numIndices;
            _rawData[pos] = a;
            pos++;
            _rawData[pos] = b;
            pos++;
            _rawData[pos] = c;
            pos++;
            _rawData[pos] = b;
            pos++;
            _rawData[pos] = d;
            pos++;
            _rawData[pos] = c;
            _numIndices += 6;
        }

        public void ToVector(ref short[] outVec)
        {
            if (outVec.Length != _numIndices)
            {
                Array.Resize(ref outVec, _numIndices);
            }

            short[] rawData = _useQuadLayout? sQuadData : _rawData;
            for (int i = 0; i < _numIndices; ++i)
            {
                outVec[i] = _rawData[i];
            }
        }

        override public string ToString()
        {
            short[] vec = new short[1];
            ToVector(ref sVector);
            string str = String.Format("[IndexData numIndices={0} indices=\"{1}\"]",
                _numIndices, vec);
            Array.Resize(ref sVector, 0);
            return str;
        }

        private void SwitchToGenericData()
        {
            if (_useQuadLayout)
            {
                _useQuadLayout = false;
                if (_rawData == null)
                {
                    _rawData = new short[_numIndices];
                }

                if (_numIndices > 0)
                {
                    int numToWrite = Math.Min(_numIndices, sQuadData.Length);
                    if (_rawData.Length < numToWrite)
                    {
                        Array.Resize(ref _rawData, numToWrite);
                    }
                    Array.Copy(sQuadData, _rawData, numToWrite);
                }
            }
        }

        /// <summary>
        /// Makes sure that the ByteArray containing the normalized, basic quad data contains at
        /// least <code>numIndices</code> indices. The array might grow, but it will never be
        /// made smaller.
        /// </summary>
        private void EnsureQuadDataCapacity(int numIndices)
        {
            if (sQuadDataNumIndices >= numIndices) return;

            short i;
            uint oldNumQuads = sQuadDataNumIndices / 6;
            uint newNumQuads = (uint)Math.Ceiling((double)numIndices / 6d);
            
            var pos = sQuadData.Length;
            sQuadDataNumIndices = newNumQuads * 6;

            int newSize = pos + (int)(newNumQuads - oldNumQuads) * 6;
            Array.Resize(ref sQuadData, newSize);
            for (i = (short)oldNumQuads; i < newNumQuads; ++i)
            {
                sQuadData[pos] = (short)(4 * i);
                pos++;
                sQuadData[pos] = (short)(4 * i + 1);
                pos++;
                sQuadData[pos] = (short)(4 * i + 2);
                pos++;
                sQuadData[pos] = (short)(4 * i + 1);
                pos++;
                sQuadData[pos] = (short)(4 * i + 3);
                pos++;
                sQuadData[pos] = (short)(4 * i + 2);
                pos++;
            }
        }

        /// <summary>
        /// Returns the index that's expected at this position if following basic quad layout.
        /// </summary>
        private static int GetBasicQuadIndexAt(int indexID)
        {
            int quadID = indexID / 6;
            int posInQuad = indexID - quadID * 6; // => indexID % 6
            int offset;
            if (posInQuad == 0) offset = 0;
            else if (posInQuad == 1 || posInQuad == 3) offset = 1;
            else if (posInQuad == 2 || posInQuad == 5) offset = 2;
            else offset = 3;
            return quadID * 4 + offset;
        }

        /// <summary>
        /// Creates an index buffer object with the right size to fit the complete data.
        /// Optionally, the current data is uploaded right away.
        /// </summary>
        /// <returns>the name of the created buffer</returns>
        public int CreateIndexBuffer(bool upload = false,
                                     BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            if (_numIndices == 0) return -1;

            //IndexBuffer3D buffer = context.createIndexBuffer(_numIndices, bufferUsage);
            int buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            if (upload) UploadToIndexBuffer(buffer, bufferUsage);
            return buffer;
        }

        /// <summary>
        /// Uploads the complete data (or a section of it) to the given buffer.
        /// </summary>
        public void UploadToIndexBuffer(int buffer, BufferUsageHint hint, int numIndices = -1)
        {
            if (numIndices < 0 || numIndices > _numIndices)
            {
                numIndices = _numIndices;
            }
            if (numIndices > 0)
            {
                //buffer.uploadFromByteArray(rawData, 0, indexID, numIndices);
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                //short[] temp = new short[] { 0, 1, 2, 3, 4, 5 };
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * numIndices), RawData, hint);
                
            }
        }

        #region properties

        /// <summary>
        /// The total number of indices.
        ///
        /// <p>If this instance contains only standardized, basic quad indices, resizing
        /// will automatically fill up with appropriate quad indices. Otherwise, it will fill
        /// up with zeroes.</p>
        ///
        /// <p>If you set the number of indices to zero, quad layout will be restored.</p>
        /// </summary>
        public int NumIndices
        {
            get
            {
                return _numIndices;
            }
            set
            {
                if (value != _numIndices)
                {
                    if (_useQuadLayout) EnsureQuadDataCapacity(value);
                    else Array.Resize(ref _rawData, value);
                    if (value == 0) _useQuadLayout = true;

                    _numIndices = value;
                }
            }
        }

        /// <summary>
        /// The number of triangles that can be spawned up with the contained indices.
        /// (In other words: the number of indices divided by three.)
        /// </summary>
        public int NumTriangles
        {
            get { return _numIndices / 3; }
            set { NumIndices = value* 3; }
        }

        /// <summary>
        /// The number of quads that can be spawned up with the contained indices.
        /// (In other words: the number of triangles divided by two.)
        /// </summary>
        public int NumQuads
        {
            get { return _numIndices / 6; }
            set { NumIndices = value * 6;}
        }

        /// <summary>
        /// Indicates if all indices are following the basic quad layout.
        ///
        ///  <p>This property is automatically updated if an index is set to a value that violates
        ///  basic quad layout. Once the layout was violated, the instance will always stay that
        ///  way, even if you fix that violating value later. Only calling <code>clear</code> or
        ///  manually enabling the property will restore quad layout.</p>
        ///
        ///  <p>If you enable this property on an instance, all indices will immediately be
        ///  replaced with indices following standard quad layout.</p>
        ///
        ///  <p>Please look at the class documentation for more information about that kind
        ///  of layout, and why it is important.</p>
        ///
        ///  @default true
        /// </summary>
        public bool UseQuadLayout
        {
            get { return _useQuadLayout; }
            set
            {
                if (value != _useQuadLayout)
                {
                    if (value)
                    {
                        EnsureQuadDataCapacity(_numIndices);
                        Array.Resize(ref _rawData, 0);
                        _useQuadLayout = true;
                    }
                    else
                    {
                        SwitchToGenericData();
                    }
                        
                }
            }
        }

        /// <summary>
        /// The raw index data; not a copy! Beware: the referenced ByteArray may change any time.
        /// Never store a reference to it, and never modify its contents manually.
        /// </summary>
        public short[] RawData
        {
            get
            {
                if (_useQuadLayout) return sQuadData;
                else return _rawData;
            }
        }

    #endregion

}
}
