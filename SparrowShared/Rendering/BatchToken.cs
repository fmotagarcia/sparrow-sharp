
namespace Sparrow.Rendering
{
    /// <summary>
    /// Points to a location within a list of MeshBatches.
    ///
    /// <para>Starling uses these tokens in its render cache. Each call to
    /// <code>painter.PushState()</code> or <code>painter.PopState()</code> provides a token
    /// referencing the current location within the cache. In the next frame, if the relevant
    /// part of the display tree has not changed, these tokens can be used to render directly
    /// from the cache instead of constructing new MeshBatches.</para>
    ///
    /// <see cref="Painter"/>
    /// </summary>
    public class BatchToken
    {
        /// <summary>
        /// The ID of the current MeshBatch.
        /// </summary>
        public int BatchID;

        /// <summary>
        /// The ID of the next vertex within the current MeshBatch.
        /// </summary>
        public int VertexID;

        /// <summary>
        /// The ID of the next index within the current MeshBatch.
        /// </summary>
        public int IndexID;

        /// <summary>
        /// Creates a new BatchToken.
        /// </summary>
        public BatchToken(int batchId = 0, int vertexId = 0, int indexId = 0)
        {
            SetTo(batchId, vertexId, indexId);
        }

        /// <summary>
        /// Copies the properties from the given token to this instance.
        /// </summary>
        public void CopyFrom(BatchToken token)
        {
            BatchID  = token.BatchID;
            VertexID = token.VertexID;
            IndexID  = token.IndexID;
        }

        /// <summary>
        /// Changes all properties at once.
        /// </summary>
        public void SetTo(int batchId = 0, int vertexId = 0, int indexId = 0)
        {
            BatchID = batchId;
            VertexID = vertexId;
            IndexID = indexId;
        }

        /// <summary>
        /// Resets all properties to zero.
        /// </summary>
        public void Reset()
        {
            BatchID = VertexID = IndexID = 0;
        }

        /// <summary>
        /// Indicates if this token contains the same values as the given one.
        /// </summary>
        public bool Equals(BatchToken other)
        {
            return BatchID == other.BatchID && VertexID == other.VertexID && IndexID == other.IndexID;
        }

        /// <summary>
        /// Creates a String representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return $"[BatchToken batchID={BatchID} vertexID={VertexID} indexID={IndexID}]";
        }
    }
}
