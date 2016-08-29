using System;

namespace SparrowSharp.Core.Rendering
{
    /** Points to a location within a list of MeshBatches.
     *
     *  <p>Starling uses these tokens in its render cache. Each call to
     *  <code>painter.pushState()</code> or <code>painter.popState()</code> provides a token
     *  referencing the current location within the cache. In the next frame, if the relevant
     *  part of the display tree has not changed, these tokens can be used to render directly
     *  from the cache instead of constructing new MeshBatches.</p>
     *
     *  @see Painter
     */
    public class BatchToken
    {
        /** The ID of the current MeshBatch. */
        public int BatchID;

        /** The ID of the next vertex within the current MeshBatch. */
        public int VertexID;

        /** The ID of the next index within the current MeshBatch. */
        public int IndexID;

        /** Creates a new BatchToken. */
        public BatchToken(int batchID = 0, int vertexID = 0, int indexID = 0)
        {
            SetTo(batchID, vertexID, indexID);
        }

        /** Copies the properties from the given token to this instance. */
        public void CopyFrom(BatchToken token)
        {
            BatchID  = token.BatchID;
            VertexID = token.VertexID;
            IndexID  = token.IndexID;
        }

        /** Changes all properties at once. */
        public void SetTo(int batchID = 0, int vertexID = 0, int indexID = 0)
        {
            this.BatchID = batchID;
            this.VertexID = vertexID;
            this.IndexID = indexID;
        }

        /** Resets all properties to zero. */
        public void Reset()
        {
            BatchID = VertexID = IndexID = 0;
        }

        /** Indicates if this token contains the same values as the given one. */
        public bool Equals(BatchToken other)
        {
            return BatchID == other.BatchID && VertexID == other.VertexID && IndexID == other.IndexID;
        }

        /** Creates a String representation of this instance. */
        override public string ToString()
        {
            return String.Format("[BatchToken batchID={0} vertexID={1} indexID={2}]",
                BatchID, VertexID, IndexID);
        }
    }
}
