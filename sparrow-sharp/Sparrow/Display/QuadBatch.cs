using System;
using Sparrow.Geom;
using Sparrow.Core;
using OpenTK;
using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using Sparrow.Utils;
using System.Runtime.InteropServices;

namespace Sparrow.Display
{
	// TODO check if dealloc exists
	public class QuadBatch : DisplayObject
	{
		private int _numQuads;
		private bool _syncRequired;
		private Texture _texture;
		private bool _premultipliedAlpha;
		private bool _tinted;
		private readonly BaseEffect _baseEffect;
		private VertexData _vertexData;
		private int _vertexBufferName;
		private ushort[] _indexData;
		private int _indexBufferName;

		public int NumQuads {
			get { return _numQuads; }
		}

		public bool Tinted {
			get { return _tinted; }
		}

		public bool PremultipliedAlpha {
			get { return _premultipliedAlpha; }
		}

		public VertexData VertexData {
			get { return _vertexData; }
		}

		public QuadBatch ()
		{
			_numQuads = 0;
			_syncRequired = false;
			_vertexData = new VertexData ();
			_baseEffect = new BaseEffect ();
		}

		public void Reset ()
		{
			_numQuads = 0;
			_syncRequired = true;
			_texture = null;
		}

		public void AddQuad (Quad quad)
		{
			AddQuad (quad, quad.Alpha, Sparrow.Display.BlendMode.AUTO, null);
		}

		public void AddQuad (Quad quad, float alpha)
		{
			// TOOD add blendMode to quad
			AddQuad (quad, alpha, Sparrow.Display.BlendMode.AUTO, null);
		}

		public void AddQuad (Quad quad, float alpha, uint blendMode)
		{
			AddQuad (quad, quad.Alpha, blendMode, null);
		}

		public void AddQuad (Quad quad, float alpha, uint blendMode, Matrix matrix)
		{
			if (matrix == null) {
				matrix = quad.TransformationMatrix;
			}

			if (_numQuads + 1 > Capacity) {
				Expand ();
			}

			if (_numQuads == 0) {
				_premultipliedAlpha = quad.PremultipliedAlpha;
				BlendMode = blendMode;
				_vertexData.SetPremultipliedAlpha(_premultipliedAlpha, false);
			}

			int vertexID = _numQuads * 4;

			quad.CopyVertexDataTo (_vertexData, vertexID);
			_vertexData.TransformVerticesWithMatrix (matrix, vertexID, 4);

			if (alpha != 1.0f) {
				_vertexData.ScaleAlphaBy (alpha, vertexID, 4);
			}

			if (!_tinted) {
				_tinted = alpha != 1.0f || quad.Tinted;
			}

			_syncRequired = true;
			_numQuads++;
		}

		public void AddQuadBatch (QuadBatch quadBatch)
		{
			AddQuadBatch (quadBatch, quadBatch.Alpha, quadBatch.BlendMode, null);
		}

		public void AddQuadBatch (QuadBatch quadBatch, float alpha)
		{
			AddQuadBatch (quadBatch, alpha, quadBatch.BlendMode, null);
		}

		public void AddQuadBatch (QuadBatch quadBatch, float alpha, uint blendMode)
		{
			AddQuadBatch (quadBatch, alpha, blendMode, null);
		}

		public void AddQuadBatch (QuadBatch quadBatch, float alpha, uint blendMode, Matrix matrix)
		{
			int vertexID = _numQuads * 4;
			int numQuads = quadBatch.NumQuads;
			int numVertices = numQuads * 4;

			if (matrix == null) {
				matrix = quadBatch.TransformationMatrix;
			}
			if (_numQuads + numQuads > Capacity) {
				Capacity = _numQuads + numQuads;
			}
			if (_numQuads == 0) {
				_premultipliedAlpha = quadBatch.PremultipliedAlpha;
				BlendMode = blendMode;
				_vertexData.SetPremultipliedAlpha (_premultipliedAlpha, false);
			}

			quadBatch.VertexData.CopyToVertexData (_vertexData, vertexID, numVertices);
			_vertexData.TransformVerticesWithMatrix (matrix, vertexID, numVertices);

			if (alpha != 1.0f) {
				_vertexData.ScaleAlphaBy (alpha, vertexID, numVertices);
			}

			if (!_tinted) {
				_tinted = alpha != 1.0f || quadBatch.Tinted;
			}

			_syncRequired = true;
			_numQuads += numQuads;
		}

		public bool IsStateChange (bool tinted, Texture texture, float alpha, bool premultipliedAlpha, uint blendMode, int numQuads)
		{
			if (_numQuads == 0) {
				return false;
			} else if (_numQuads + numQuads > 8192) {
				return true;
			} else if (_texture == null && texture == null) {
				return _premultipliedAlpha != premultipliedAlpha || BlendMode != blendMode;
			} else if (_texture != null && texture != null) {
				return _tinted != (tinted || alpha != 1.0f) ||
				_texture.Name != texture.Name ||
				BlendMode != blendMode;
			} else {
				return true;
			}
		}

		override public Rectangle BoundsInSpace (DisplayObject targetSpace)
		{
			Matrix matrix = targetSpace == this ? null : TransformationMatrixToSpace (targetSpace);
			return _vertexData.BoundsAfterTransformation (matrix, 0, _numQuads * 4);
		}

		override public void Render (RenderSupport support)
		{
			if (_numQuads != 0) {
				support.FinishQuadBatch ();
				support.AddDrawCalls (1);
				Render (support.MvpMatrix, support.Alpha, support.BlendMode);
			}
		}

		public void Render (Matrix matrix)
		{
			Render (matrix, 1.0f, BlendMode);
		}

		public void Render (Matrix matrix, float alpha, uint blendMode)
		{
			if (_numQuads == 0) {
				return;
			}

			if (_syncRequired) {
				SyncBuffers ();
			}

			if (blendMode == Sparrow.Display.BlendMode.AUTO) {
				throw new InvalidOperationException ("Cannot render object with blend mode AUTO");
			}

			_baseEffect.Texture = _texture;
			_baseEffect.PremultipliedAlpha = _premultipliedAlpha;
			_baseEffect.MvpMatrix = matrix;
			_baseEffect.UseTinting = _tinted || alpha != 1.0f;
			_baseEffect.Alpha = alpha;

			_baseEffect.PrepareToDraw ();

			Sparrow.Display.BlendMode.ApplyBlendFactors (blendMode, _premultipliedAlpha);

			int attribPosition = _baseEffect.AttribPosition;
			int attribColor = _baseEffect.AttribColor;
			int attribTexCoords = _baseEffect.AttribTexCoords;

			GL.EnableVertexAttribArray (attribPosition);
			GL.EnableVertexAttribArray (attribColor);

			if (_texture != null) {
				GL.EnableVertexAttribArray (attribTexCoords);
			}
			GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);
			GL.BindBuffer (All.ElementArrayBuffer, _indexBufferName);

			int sizeOfVertex = Marshal.SizeOf(typeof(Vertex));//5 * sizeof(float);
			IntPtr positionOffset = Marshal.OffsetOf (typeof(Vertex), "Position");
			IntPtr colorOffset = Marshal.OffsetOf (typeof(Vertex), "Color");
			IntPtr textureOffset = Marshal.OffsetOf (typeof(Vertex), "TexCoords");
		
			GL.VertexAttribPointer (attribPosition, 2, All.Float, false, sizeOfVertex, positionOffset);
			GL.VertexAttribPointer (attribColor, 4, All.Byte, true, sizeOfVertex, colorOffset);
			if (_texture != null) {
				GL.VertexAttribPointer (attribTexCoords, 2, All.Float, false, sizeOfVertex, textureOffset);
			}
			int numIndices = _numQuads * 6;
			GL.DrawElements (All.Triangles, numIndices, All.UnsignedShort, IntPtr.Zero);
		}

		public List<QuadBatch> Compile (DisplayObject displayObject)
		{
			return Compile (displayObject, null);
		}

		public List<QuadBatch> Compile (DisplayObject displayObject, List<QuadBatch> quadBatches)
		{
			if (quadBatches == null) {
				quadBatches = new List<QuadBatch> ();
			}

			Compile (displayObject, quadBatches, -1, new Matrix (), 1.0f, Sparrow.Display.BlendMode.AUTO);

			return Compile (displayObject);
		}

		public int Compile (DisplayObject displayObject, List<QuadBatch> quadBatches, int quadBatchID, 
		                    Matrix transformationMatrix, float alpha, uint blendMode)
		{
			bool isRootObject = false;
			float objectAlpha = displayObject.Alpha;

			Quad quad = displayObject is Quad ? (Quad)displayObject : null;
			QuadBatch batch = displayObject is QuadBatch ? (QuadBatch)displayObject : null;
			DisplayObjectContainer container = displayObject is DisplayObjectContainer ? (DisplayObjectContainer)displayObject : null;

			if (quadBatchID == -1) {
				isRootObject = true;
				quadBatchID = 0;
				objectAlpha = 1.0f;
				blendMode = displayObject.BlendMode;

				if (quadBatches.Count == 0) {
					quadBatches.Add (new QuadBatch ());
				} else {
					quadBatches [0].Reset ();
				}
			}

			if (container != null) {
				Matrix childMatrix = new Matrix ();

				int numChildren = container.NumChildren;
				for (int i = 0; i < numChildren; i++) {
					DisplayObject child = container.GetChild (i);
					if (child.HasVisibleArea) {
						uint childBlendMode = child.BlendMode;
						if (childBlendMode == Sparrow.Display.BlendMode.AUTO) {
							childBlendMode = blendMode;
						}

						childMatrix.CopyFromMatrix (transformationMatrix);
						childMatrix.PrependMatrix (child.TransformationMatrix);

						quadBatchID = Compile (child, quadBatches, quadBatchID, childMatrix, alpha * objectAlpha, childBlendMode);
					}
				}
			} else if (quad != null) {
				Texture texture = quad.Texture;
				bool tinted = quad.Tinted;
				bool pma = quad.PremultipliedAlpha;
				int numQuads = 1;

				QuadBatch currentBatch = quadBatches [quadBatchID];
				if (currentBatch.IsStateChange (tinted, texture, alpha * objectAlpha, pma, blendMode, numQuads)) {
					quadBatchID++;

					if (quadBatches.Count <= quadBatchID) {
						quadBatches.Add (new QuadBatch ());
					}

					currentBatch = quadBatches [quadBatchID];
					currentBatch.Reset ();
				}

				currentBatch.AddQuad (quad, alpha * objectAlpha, blendMode, transformationMatrix);
			} else if (batch != null) {
				Texture texture = quad.Texture;
				bool tinted = quad.Tinted;
				bool pma = quad.PremultipliedAlpha;
				int numQuads = batch.NumQuads;

				QuadBatch currentBatch = quadBatches [quadBatchID];
				if (currentBatch.IsStateChange (tinted, texture, alpha * objectAlpha, pma, blendMode, numQuads)) {
					quadBatchID++;

					if (quadBatches.Count <= quadBatchID) {
						quadBatches.Add (new QuadBatch ());
					}

					currentBatch = quadBatches [quadBatchID];
					currentBatch.Reset ();
				}

				currentBatch.AddQuadBatch (batch, alpha * objectAlpha, blendMode, transformationMatrix);
			} else {
				throw new InvalidOperationException ("Unsupported display object");
			}

			if (isRootObject) {
				// remove unused batches
				for (int i = quadBatches.Count - 1; i > quadBatchID; --i) {
					quadBatches.RemoveAt (quadBatches.Count - 1);
				}
			}

			return quadBatchID;
		}

		private void Expand ()
		{
			int oldCapacity = Capacity;
			Capacity = oldCapacity < 8 ? 16 : oldCapacity * 2;
		}

		private void CreateBuffers ()
		{
			DestroyBuffers ();

			int numVertices = _vertexData.NumVertices;
			int numIndices = numVertices / 4 * 6;
			if (numVertices == 0) {
				return;
			}

			GL.GenBuffers (1, out _vertexBufferName);
			GL.GenBuffers (1, out _indexBufferName);

			if (_vertexBufferName == 0 || _indexBufferName == 0) {
				throw new InvalidOperationException ("Could not create vertex buffers");
			}

			GL.BindBuffer (All.ElementArrayBuffer, _indexBufferName);
			GL.BufferData (All.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * numIndices), _indexData, All.StaticDraw);

			_syncRequired = true; 
		}

		private void DestroyBuffers ()
		{
			if (_vertexBufferName != 0) {
				GL.DeleteBuffers (1, ref _vertexBufferName);
				_vertexBufferName = 0;
			}

			if (_vertexBufferName != 0) {
				GL.DeleteBuffers (1, ref _vertexBufferName);
				_indexBufferName = 0;
			}
		}

		public void SyncBuffers ()
		{
			if (_vertexBufferName == 0) {
				CreateBuffers ();
			}

			GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);
			GL.BufferData (All.ArrayBuffer, (IntPtr)(_vertexData.NumVertices * 5 * sizeof(float)), _vertexData.Vertices, All.StaticDraw);

			_syncRequired = false;
		}

		private int Capacity {
			get { return _vertexData.NumVertices / 4; }
			set {
				if (value == 0) {
					throw new Exception ("Capacity must not be zero");
				}

				uint oldCapacity = (uint)Capacity;
				int numVertices = value * 4;
				int numIndices = value * 6;

				_vertexData.NumVertices = numVertices;

				_indexData = new ushort[numIndices];

				for (uint i = oldCapacity; i < value; ++i) {
					_indexData [i * 6] = (ushort)(i * 4);
					_indexData [i * 6 + 1] = (ushort)(i * 4 + 1);
					_indexData [i * 6 + 2] = (ushort)(i * 4 + 2);
					_indexData [i * 6 + 3] = (ushort)(i * 4 + 1);
					_indexData [i * 6 + 4] = (ushort)(i * 4 + 3);
					_indexData [i * 6 + 5] = (ushort)(i * 4 + 2);
				}

				DestroyBuffers ();
				_syncRequired = true;
			}
		}
	}
}