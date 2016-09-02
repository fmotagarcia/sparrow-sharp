
using Sparrow.Core;
using Sparrow.Geom;
using SparrowSharp.Core.Display;
using SparrowSharp.Core.Utils;
using System;
using System.Collections.Generic;

namespace SparrowSharp.Core.Rendering
{
    public class BatchProcessor
    {
        private List<MeshBatch> _batches;
        private BatchPool _batchPool;
        private MeshBatch _currentBatch;
        private Type _currentStyleType;
        private BatchToken _cacheToken;
        public delegate void OnBatchCompleteFunction(MeshBatch mb);
        /** This callback is executed whenever a batch is finished and replaced by a new one.
         *  The finished MeshBatch is passed to the callback. Typically, this callback is used
         *  to actually render it. */
        public OnBatchCompleteFunction OnBatchComplete;

        // helper objects
        private static MeshSubset sMeshSubset = new MeshSubset();

        /** Creates a new batch processor. */
        public BatchProcessor()
        {
            _batches = new List<MeshBatch>();
            _batchPool = new BatchPool();
            _cacheToken = new BatchToken();
        }

        /** Disposes all batches (including those in the reusable pool). */
        public void Dispose()
        {
            foreach(MeshBatch batch in _batches)
            {
                batch.Dispose();
            }
            _batches.Clear();
            _batchPool.Purge();
            _currentBatch = null;
        }

        /** Adds a mesh to the current batch, or to a new one if the current one does not support
        *  it. Whenever the batch changes, <code>onBatchComplete</code> is called for the previous
        *  one.
        *
        *  @param mesh       the mesh to add to the current (or new) batch.
        *  @param state      the render state from which to take the current settings for alpha,
        *                    modelview matrix, and blend mode.
        *  @param subset     the subset of the mesh you want to add, or <code>null</code> for
        *                    the complete mesh.
        *  @param ignoreTransformations   when enabled, the mesh's vertices will be added
        *                    without transforming them in any way (no matter the value of the
        *                    state's <code>modelviewMatrix</code>).
        */
        public void AddMesh(Mesh mesh, RenderState state, MeshSubset subset= null,
                            bool ignoreTransformations= false)
        {
            if (subset == null)
            {
                subset = sMeshSubset;
                subset.VertexID = subset.IndexID = 0;
                subset.NumVertices = mesh.NumVertices;
                subset.NumIndices  = mesh.NumIndices;
            }
            else
            {
                if (subset.NumVertices< 0) subset.NumVertices = mesh.NumVertices - subset.VertexID;
                if (subset.NumIndices< 0) subset.NumIndices  = mesh.NumIndices  - subset.IndexID;
            }

            if (subset.NumVertices > 0)
            {
                if (_currentBatch == null || !_currentBatch.CanAddMesh(mesh, subset.NumVertices))
                {
                    FinishBatch();

                    _currentStyleType = mesh.Style.Type;
                    _currentBatch = _batchPool.GetBatch(_currentStyleType);
                    _currentBatch.BlendMode = state != null ? state.BlendMode : mesh.BlendMode;
                    _cacheToken.SetTo(_batches.Count);
                    _batches[_batches.Count] = _currentBatch;
                }

                Matrix matrix = state != null ? state._modelviewMatrix : null;
                float alpha  = state != null ? state.Alpha : 1.0f;

                _currentBatch.AddMesh(mesh, matrix, alpha, subset, ignoreTransformations);
                _cacheToken.VertexID += subset.NumVertices;
                _cacheToken.IndexID  += subset.NumIndices;
            }
        }

        /** Finishes the current batch, i.e. call the 'onComplete' callback on the batch and
         *  prepares initialization of a new one. */
        public void FinishBatch()
        {
            MeshBatch meshBatch = _currentBatch;

            if (meshBatch != null)
            {
                _currentBatch = null;
                _currentStyleType = null;

                if (OnBatchComplete != null)
                {
                    OnBatchComplete(meshBatch);
                }
            }
        }

        /** Clears all batches and adds them to a pool so they can be reused later. */
        public void Clear()
        {
            int numBatches = _batches.Count;

            for (int i = 0; i < numBatches; ++i)
            {
                _batchPool.Put(_batches[i]);
            }

            _batches.Clear();
            _currentBatch = null;
            _currentStyleType = null;
            _cacheToken.Reset();
        }

        /** Returns the batch at a certain index. */
        public MeshBatch GetBatchAt(int batchID)
        {
            return _batches[batchID];
        }

        /** Disposes all batches that are currently unused. */
        public void Trim()
        {
            _batchPool.Purge();
        }

        public void RewindTo(BatchToken token)
        {
            if (token.BatchID > _cacheToken.BatchID)
                throw new IndexOutOfRangeException("Token outside available range");

            for (int i = _cacheToken.BatchID; i > token.BatchID; --i)
            {
                _batchPool.Put(_batches[_batches.Count - 1]);
                _batches.RemoveAt(_batches.Count - 1);
            }

            if (_batches.Count > token.BatchID)
            {
                MeshBatch batch  = _batches[token.BatchID];
                batch.NumIndices  = Math.Min(batch.NumIndices,  token.IndexID);
                batch.NumVertices = Math.Min(batch.NumVertices, token.VertexID);
            }

            _currentBatch = null;
            _cacheToken.CopyFrom(token);
        }

        /** Sets all properties of the given token so that it describes the current position
         *  within this instance. */
        public BatchToken FillToken(BatchToken token)
        {
            token.BatchID  = _cacheToken.BatchID;
            token.VertexID = _cacheToken.VertexID;
            token.IndexID  = _cacheToken.IndexID;
            return token;
        }

        /** The number of batches currently stored in the BatchProcessor. */
        public int NumBatches { get { return _batches.Count; } }
    }

    class BatchPool
    {
        private Dictionary<Type, List<MeshBatch>> _batchLists;

        public BatchPool()
        {
            _batchLists = new Dictionary<Type, List<MeshBatch>>();
        }

        public void Purge()
        {
            foreach (KeyValuePair<Type, List<MeshBatch>> entry in _batchLists)
            {
                List<MeshBatch> batchList = entry.Value;
                for (int i = 0; i < batchList.Count; ++i)
                {
                    batchList[i].Dispose();
                }
                batchList.Clear();
            }
        }

        public MeshBatch GetBatch(Type styleType)
        {
            List<MeshBatch> batchList = _batchLists[styleType];
            if (batchList == null)
            {
                batchList = new List<MeshBatch>();
                _batchLists[styleType] = batchList;
            }

            if (batchList.Count > 0)
            {
                var ret = batchList[batchList.Count - 1];
                batchList.RemoveAt(batchList.Count - 1);
                return ret;
            }
            else return new MeshBatch();
        }

        public void Put(MeshBatch meshBatch)
        {
            Type styleType = meshBatch.Style.Type;
            List<MeshBatch> batchList = _batchLists[styleType];
            if (batchList == null)
            {
                batchList = new List<MeshBatch>();
                _batchLists[styleType] = batchList;
            }

            meshBatch.Clear();
            batchList[batchList.Count] = meshBatch;
        }
    }
}
