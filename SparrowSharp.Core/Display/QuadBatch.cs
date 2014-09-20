using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace Sparrow.Display
{
    /// <summary>
    /// Optimizes rendering of a number of quads with an identical state.

    /// The majority of all rendered objects in Sparrow are quads. In fact, all the default
    /// leaf nodes of Sparrow are quads (the 'Image' and 'Quad' classes). The rendering of those
    /// quads can be accelerated by a big factor if all quads with an identical state are sent
    /// to the GPU in just one call. That's what the 'QuadBatch' class can do.
    /// 
    /// The 'Flatten' method of the 'Sprite' class uses this class internally to optimize its
    /// rendering performance. In most situations, it is recommended to stick with flattened
    /// sprites, because they are easier to use. Sometimes, however, it makes sense
    /// to use the QuadBatch class directly: e.g. you can add one quad multiple times to
    /// a quad batch, whereas you can only add it once to a sprite. Furthermore, this class
    /// does not dispatch 'ADDED' or 'ADDED_TO_STAGE' events when a quad
    /// is added, which makes it more lightweight.

    /// One QuadBatch object is bound to a specific render state. The first object you add to a
    /// batch will decide on the QuadBatch's state, that is: its texture, its settings for
    /// smoothing and repetition, and if it's tinted (colored vertices and/or transparency).
    /// When you reset the batch, it will accept a new state on the next added quad.
    /// </summary>
    public class QuadBatch : DisplayObject
    {
        private const int INDICES_PER_QUAD = 6;
        private int _numQuads;
        private bool _syncRequired;
        private Texture _texture;
        private bool _premultipliedAlpha;
        private bool _tinted;
        private readonly BaseEffect _baseEffect;
        private readonly VertexData _vertexData;
        private int _vertexBufferName;
        private int _vertexColorsBufferName;
        private ushort[] _indexData;
        private bool _vertexBufferNeedsReInit = true;
        private bool _colorBufferNeedsReInit = true;
        private int _capacity;
        private int _indexBufferName;

        /// <summary>
        /// The current texture of the batch, if there is one.
        /// </summary>
        public Texture QuadTexture
        {
            get { return _texture; }
        }

        /// <summary>
        /// The number of quads that has been added to the batch.
        /// </summary>
        public int NumQuads
        {
            get { return _numQuads; }
        }

        /// <summary>
        /// Indicates if any vertices have a non-white color or are not fully opaque.
        /// </summary>
        public bool Tinted
        {
            get { return _tinted; }
        }

        /// <summary>
        /// Indicates if the rgb values are stored premultiplied with the alpha value.
        /// </summary>
        public bool PremultipliedAlpha
        {
            get { return _premultipliedAlpha; }
        }

        public VertexData VertexData
        {
            get { return _vertexData; }
        }

        public QuadBatch()
        {
            _numQuads = 0;
            _syncRequired = false;
            _vertexData = new VertexData();
            _baseEffect = new BaseEffect();
        }

        /// <summary>
        /// Resets the batch. The vertex- and index-buffers keep their size, so that they can be reused.
        /// </summary>
        public void Reset()
        {
            _numQuads = 0;
            _syncRequired = true;
            _texture = null;
        }

        /// <summary>
        ///  Adds a quad or image. Make sure you only add quads with an equal state.
        /// </summary>
        public void AddQuad(Quad quad)
        {
            AddQuad(quad, quad.Alpha, quad.BlendMode);
        }

        /// <summary>
        /// Adds a quad or image using a custom alpha value (ignoring the quad's original alpha).
        /// Make sure you only add quads with an equal state.
        /// </summary>
        public void AddQuad(Quad quad, float alpha)
        {
            AddQuad(quad, alpha, quad.BlendMode);
        }

        /// <summary>
        /// Adds a quad or image to the batch, using custom alpha and blend mode values (ignoring the
        /// quad's original values) and transforming each vertex by a certain transformation matrix.
        /// Make sure you only add quads with an equal state.
        /// </summary>
        public void AddQuad(Quad quad, float alpha, uint blendMode, Matrix matrix = null)
        {
            if (matrix == null)
            {
                matrix = quad.TransformationMatrix;
            }

            if (_numQuads + 1 > _capacity)
            {
                Capacity = _capacity < 8 ? 16 : _capacity * 2;
            }

            if (_numQuads == 0)
            {
                _texture = quad.Texture;
                _premultipliedAlpha = quad.PremultipliedAlpha;
                BlendMode = blendMode;
                _vertexData.SetPremultipliedAlpha(_premultipliedAlpha, false);
            }

            int vertexID = _numQuads * 4;

            if (!_tinted)
            {
                _tinted = alpha != 1.0f || quad.Tinted;
            }

            quad.CopyVertexDataTo(_vertexData, vertexID, _tinted);
            _vertexData.TransformVertices(matrix, vertexID, 4);

            if (alpha != 1.0f)
            {
                _vertexData.ScaleAlphaBy(alpha, vertexID, 4);
            }

            _syncRequired = true;
            _numQuads++;
        }

        /// <summary>
        /// Adds another quad batch to this batch.
        /// </summary>
        public void AddQuadBatch(QuadBatch quadBatch)
        {
            AddQuadBatch(quadBatch, quadBatch.Alpha, quadBatch.BlendMode);
        }

        /// <summary>
        /// Adds another quad batch to this batch, using a custom alpha value (ignoring the batch's
        /// original alpha).
        /// </summary>
        public void AddQuadBatch(QuadBatch quadBatch, float alpha)
        {
            AddQuadBatch(quadBatch, alpha, quadBatch.BlendMode);
        }

        /// <summary>
        /// Adds another quad batch to this batch, using custom alpha and blend mode values (ignoring the
        /// batch's original values) and transforming each vertex by a certain transformation matrix. Just
        /// like the 'AddQuad' method, you have to make sure that you only add batches with an equal state.
        /// </summary>
        public void AddQuadBatch(QuadBatch quadBatch, float alpha, uint blendMode, Matrix matrix = null)
        {
            int vertexID = _numQuads * 4;
            int numQuads = quadBatch.NumQuads;
            int numVertices = numQuads * 4;

            if (matrix == null)
            {
                matrix = quadBatch.TransformationMatrix;
            }
            if (_numQuads + numQuads > _capacity)
            {
                Capacity = _numQuads + numQuads;
            }
            if (_numQuads == 0)
            {
                _texture = quadBatch.QuadTexture;
                _premultipliedAlpha = quadBatch.PremultipliedAlpha;
                BlendMode = blendMode;
                _vertexData.SetPremultipliedAlpha(_premultipliedAlpha, false);
            }

            if (!_tinted)
            {
                _tinted = alpha != 1.0f || quadBatch.Tinted;
            }

            quadBatch.VertexData.CopyToVertexData(_vertexData, _tinted, vertexID, numVertices);
            _vertexData.TransformVertices(matrix, vertexID, numVertices);

            if (alpha != 1.0f)
            {
                _vertexData.ScaleAlphaBy(alpha, vertexID, numVertices);
            }

            _syncRequired = true;
            _numQuads += numQuads;
        }

        /// <summary>
        /// Indicates if specific quads can be added to the batch without causing a state change.
        /// A state change occurs if the quad uses a different base texture, has a different 'Smoothing',
        /// 'Repeat' or 'Tinted' setting, or if the batch is full (one batch can contain up to 8192 quads).
        /// </summary>
        public bool IsStateChange(bool tinted, Texture texture, float alpha, bool premultipliedAlpha, uint blendMode, int numQuads)
        {
            if (_numQuads == 0)
            {
                return false;
            }
            else if (_numQuads + numQuads > 8192)
            {
                return true;
            }
            else if (_texture == null && texture == null)
            {
                return _premultipliedAlpha != premultipliedAlpha || BlendMode != blendMode;
            }
            else if (_texture != null && texture != null)
            {
                return _tinted != (tinted || alpha != 1.0f) ||
                _texture.Name != texture.Name ||
                BlendMode != blendMode;
            }
            return true;
        }

        override public Rectangle BoundsInSpace(DisplayObject targetSpace)
        {
            Matrix matrix = targetSpace == this ? null : TransformationMatrixToSpace(targetSpace);
            return _vertexData.BoundsAfterTransformation(matrix, 0, _numQuads * 4);
        }

        override public void Render(RenderSupport support)
        {
            if (_numQuads != 0)
            {
                support.FinishQuadBatch();
                support.AddDrawCalls(1);
                Render(support.MvpMatrix, support.Alpha, support.BlendMode);
            }
        }

        /// <summary>
        /// Renders the batch with a custom mvp matrix.
        /// </summary>
        public void Render(Matrix matrix)
        {
            Render(matrix, 1.0f, BlendMode);
        }

        /// <summary>
        /// Renders the batch with custom alpha and blend mode values, as well as a custom mvp matrix.
        /// </summary>
        public void Render(Matrix matrix, float alpha, uint blendMode)
        {
            if (_numQuads == 0)
            {
                return;
            }

            if (_syncRequired)
            {
                SyncBuffers(alpha);
            }

            if (blendMode == Sparrow.Display.BlendMode.AUTO)
            {
                throw new InvalidOperationException("Cannot render object with blend mode AUTO");
            }
            bool useTinting = _tinted || alpha != 1.0f;
            _baseEffect.Texture = _texture;
            _baseEffect.PremultipliedAlpha = _premultipliedAlpha;
            _baseEffect.MvpMatrix = matrix;
            _baseEffect.UseTinting = useTinting;
            _baseEffect.Alpha = alpha;

            _baseEffect.PrepareToDraw();

            Sparrow.Display.BlendMode.ApplyBlendFactors(blendMode, _premultipliedAlpha);

            int attribPosition = _baseEffect.AttribPosition;
            GL.EnableVertexAttribArray(attribPosition);

            int attribColor = _baseEffect.AttribColor;
            if (useTinting)
            {
                GL.EnableVertexAttribArray(attribColor);
            }

            int attribTexCoords = _baseEffect.AttribTexCoords;
            if (_texture != null)
            {
                GL.EnableVertexAttribArray(attribTexCoords);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferName);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferName);

            GL.VertexAttribPointer(attribPosition, 2, VertexAttribPointerType.Float, false, Vertex.SIZE, (IntPtr)Vertex.POSITION_OFFSET);

            if (_texture != null)
            {
                GL.VertexAttribPointer(attribTexCoords, 2, VertexAttribPointerType.Float, false, Vertex.SIZE, (IntPtr)Vertex.TEXTURE_OFFSET);
            }

            if (useTinting)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexColorsBufferName);
                GL.VertexAttribPointer(attribColor, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float), (IntPtr)0);
            }

            int numIndices = _numQuads * INDICES_PER_QUAD;
            GL.DrawElements(BeginMode.Triangles, numIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);
//			GL.DrawElements (All.TriangleStrip, numIndices, All.UnsignedShort, IntPtr.Zero);
        }

        /// <summary>
        /// Analyses an object that is made up exclusively of quads (or other containers) and saves the
        /// resulting quad batches into the specified an array; batches inside that array are reused.
        /// </summary>
        public static List<QuadBatch> Compile(DisplayObject displayObject, List<QuadBatch> quadBatches = null)
        {
            if (quadBatches == null)
            {
                quadBatches = new List<QuadBatch>();
            }

            Matrix identity = Matrix.Create();
            identity.Identity();

            Compile(displayObject, quadBatches, -1, identity, 1.0f, Sparrow.Display.BlendMode.AUTO);

            return quadBatches;
        }

        public static int Compile(DisplayObject displayObject, List<QuadBatch> quadBatches, int quadBatchID, 
                             Matrix transformationMatrix, float alpha, uint blendMode)
        {
            bool isRootObject = false;
            float objectAlpha = displayObject.Alpha;

            Quad quad = displayObject is Quad ? (Quad)displayObject : null;
            QuadBatch batch = displayObject is QuadBatch ? (QuadBatch)displayObject : null;
            DisplayObjectContainer container = displayObject is DisplayObjectContainer ? (DisplayObjectContainer)displayObject : null;

            if (quadBatchID == -1)
            {
                isRootObject = true;
                quadBatchID = 0;
                objectAlpha = 1.0f;
                blendMode = displayObject.BlendMode;

                if (quadBatches.Count == 0)
                {
                    quadBatches.Add(new QuadBatch());
                }
                else
                {
                    quadBatches[0].Reset();
                }
            }

            if (container != null)
            {
                Matrix childMatrix = Matrix.Create();
                childMatrix.Identity();
                int numChildren = container.NumChildren;
                for (int i = 0; i < numChildren; i++)
                {
                    DisplayObject child = container.GetChild(i);
                    if (child.HasVisibleArea)
                    {
                        uint childBlendMode = child.BlendMode;
                        if (childBlendMode == Sparrow.Display.BlendMode.AUTO)
                        {
                            childBlendMode = blendMode;
                        }

                        childMatrix.CopyFromMatrix(transformationMatrix);
                        childMatrix.PrependMatrix(child.TransformationMatrix);

                        quadBatchID = Compile(child, quadBatches, quadBatchID, childMatrix, alpha * objectAlpha, childBlendMode);
                    }
                }
            }
            else if (quad != null)
            {
                Texture texture = quad.Texture;
                bool tinted = quad.Tinted;
                bool pma = quad.PremultipliedAlpha;

                QuadBatch currentBatch = quadBatches[quadBatchID];
                if (currentBatch.IsStateChange(tinted, texture, alpha * objectAlpha, pma, blendMode, 1))
                {
                    quadBatchID++;

                    if (quadBatches.Count <= quadBatchID)
                    {
                        quadBatches.Add(new QuadBatch());
                    }

                    currentBatch = quadBatches[quadBatchID];
                    currentBatch.Reset();
                }

                currentBatch.AddQuad(quad, alpha * objectAlpha, blendMode, transformationMatrix);
            }
            else if (batch != null)
            {
                Texture texture = quad.Texture;
                bool tinted = quad.Tinted;
                bool pma = quad.PremultipliedAlpha;
                int numQuads = batch.NumQuads;

                QuadBatch currentBatch = quadBatches[quadBatchID];
                if (currentBatch.IsStateChange(tinted, texture, alpha * objectAlpha, pma, blendMode, numQuads))
                {
                    quadBatchID++;

                    if (quadBatches.Count <= quadBatchID)
                    {
                        quadBatches.Add(new QuadBatch());
                    }

                    currentBatch = quadBatches[quadBatchID];
                    currentBatch.Reset();
                }

                currentBatch.AddQuadBatch(batch, alpha * objectAlpha, blendMode, transformationMatrix);
            }
            else
            {
                throw new InvalidOperationException("Unsupported display object");
            }

            if (!isRootObject)
            {
                return quadBatchID;
            }

            // remove unused batches
            for (int i = quadBatches.Count - 1; i > quadBatchID; --i)
            {
                quadBatches.RemoveAt(quadBatches.Count - 1);
            }
            return quadBatchID;
        }

        private void CreateBuffers()
        {
            DestroyBuffers();

            int numVertices = _vertexData.NumVertices;
            int numIndices = numVertices / 4 * INDICES_PER_QUAD;
            if (numVertices == 0)
            {
                return;
            }

            GL.GenBuffers(1, out _vertexBufferName);
            GL.GenBuffers(1, out _vertexColorsBufferName);
            GL.GenBuffers(1, out _indexBufferName);

            if (_vertexBufferName == 0 || _vertexColorsBufferName == 0 || _indexBufferName == 0)
            {
                throw new InvalidOperationException("Could not create vertex buffers");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferName);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * numIndices), _indexData, BufferUsage.StaticDraw);

//			GL.BufferData (All.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * numIndices), IntPtr.Zero, All.StaticDraw);
//			IntPtr videoMemoryPtr = GL.Oes.MapBuffer( All.ElementArrayBuffer, All.WriteOnlyOes );
//			unsafe
//			{
//				ushort* ptr = (ushort*)videoMemoryPtr.ToPointer ();
//				for (int i = 0; i < numVertices; i++) {
//					*ptr++ = _indexData [i];
//				}
//			}
//			GL.Oes.UnmapBuffer( All.ElementArrayBuffer );

            _syncRequired = true; 
        }

        private void DestroyBuffers()
        {
            if (_vertexBufferName != 0)
            {
                GL.DeleteBuffers(1, ref _vertexBufferName);
                _vertexBufferName = 0;
            }

            if (_vertexColorsBufferName != 0)
            {
                GL.DeleteBuffers(1, ref _vertexColorsBufferName);
                _vertexColorsBufferName = 0;
            }

            if (_indexBufferName != 0)
            {
                GL.DeleteBuffers(1, ref _indexBufferName);
                _indexBufferName = 0;
            }
        }

        private void SyncBuffers(float alpha)
        {
            if (_vertexBufferName == 0)
            {
                CreateBuffers();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferName);

            if (GLExtensions.MapBufferSupported)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertexData.NumVertices * 4 * sizeof(float)), IntPtr.Zero, BufferUsage.StaticDraw);
                IntPtr vertexBuffer = GL.Oes.MapBuffer(All.ArrayBuffer, All.WriteOnlyOes);

                unsafe
                {
                    int numVertices = _vertexData.Vertices.Length;
                    Vertex[] vertices = _vertexData.Vertices;

                    float* ptr = (float*)vertexBuffer.ToPointer();
                    for (int i = 0; i < numVertices; i++)
                    {
                        Vertex vertex = vertices[i];
                        *ptr++ = vertex.Position.X;
                        *ptr++ = vertex.Position.Y;
                        *ptr++ = vertex.TexCoords.X;
                        *ptr++ = vertex.TexCoords.Y;
                    }
                }
                GL.Oes.UnmapBuffer(All.ArrayBuffer);
            }
            else
            {
                // this optimization does not seem to increase framerate (although openGL docs state it should)
                if (_vertexBufferNeedsReInit)
                {
                    _vertexBufferNeedsReInit = false;
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertexData.NumVertices * 4 * sizeof(float)), _vertexData.Vertices, BufferUsage.StaticDraw);
                }
                else
                {
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(_vertexData.NumVertices * 4 * sizeof(float)), _vertexData.Vertices);
                }
            }

            if (_tinted || alpha != 1.0f)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexColorsBufferName);

                if (GLExtensions.MapBufferSupported)
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertexData.NumVertices * sizeof(byte) * 4), IntPtr.Zero, BufferUsage.StaticDraw);
                    IntPtr colorBuffer = GL.Oes.MapBuffer(All.ArrayBuffer, All.WriteOnlyOes);
                    unsafe
                    {
                        int numVertices = _vertexData.Vertices.Length;
                        VertexColor[] colors = _vertexData.VertexColors;

                        byte* ptr = (byte*)colorBuffer.ToPointer();
                        for (int i = 0; i < numVertices; i++)
                        {
                            VertexColor color = colors[i];
                            *ptr++ = color.R;
                            *ptr++ = color.G;
                            *ptr++ = color.B;
                            *ptr++ = color.A;
                        }
                    }
                    GL.Oes.UnmapBuffer(All.ArrayBuffer);
                }
                else
                {
                    // this optimization does not seem to increase framerate on (although openGL docs state it should)
                    if (_colorBufferNeedsReInit)
                    {
                        _colorBufferNeedsReInit = false;
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertexData.NumVertices * sizeof(byte) * 4 ), _vertexData.VertexColors, BufferUsage.StaticDraw);
                    }
                    else
                    {
                        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(_vertexData.NumVertices * sizeof(byte) * 4), _vertexData.VertexColors);
                    }
                }
            }
            _syncRequired = false;
        }

        private int Capacity
        {
            get { return _capacity; }
            set
            {
                if (value == 0)
                {
                    throw new Exception("Capacity must not be zero");
                }
                _vertexBufferNeedsReInit = true;
                _colorBufferNeedsReInit = true;
                uint oldCapacity = (uint)_capacity;
                _capacity = value;
                int numVertices = value * 4;
                int numIndices = value * INDICES_PER_QUAD;

                _vertexData.NumVertices = numVertices;
                Array.Resize<ushort>(ref _indexData, numIndices);

                for (uint i = oldCapacity; i < value; ++i)
                {
                    // indexed
                    _indexData[i * INDICES_PER_QUAD] = (ushort)(i * 4);
                    _indexData[i * INDICES_PER_QUAD + 1] = (ushort)(i * 4 + 1);
                    _indexData[i * INDICES_PER_QUAD + 2] = (ushort)(i * 4 + 2);
                    _indexData[i * INDICES_PER_QUAD + 3] = (ushort)(i * 4 + 1);
                    _indexData[i * INDICES_PER_QUAD + 4] = (ushort)(i * 4 + 3);
                    _indexData[i * INDICES_PER_QUAD + 5] = (ushort)(i * 4 + 2);

                    // triangle strip
//					_indexData [i * NUM_INDICES + 0] = (ushort)(i * 4);
//					_indexData [i * NUM_INDICES + 1] = (ushort)(i * 4 + 1);
//					_indexData [i * NUM_INDICES + 2] = (ushort)(i * 4 + 2);
//					_indexData [i * NUM_INDICES + 3] = (ushort)(i * 4 + 3);
//					_indexData [i * NUM_INDICES + 4] = (ushort)(i * 4 + 3);
//					_indexData [i * NUM_INDICES + 5] = (ushort)(i * 4 * 2);
//					_indexData [i * NUM_INDICES + 6] = (ushort)(i * 4 * 2);
//					_indexData [i * NUM_INDICES + 7] = (ushort)(i * 4 * 2 + 1);
                }

                DestroyBuffers();
                _syncRequired = true;
            }
        }
    }
}