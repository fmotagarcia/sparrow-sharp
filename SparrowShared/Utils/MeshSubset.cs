
namespace Sparrow.Utils
{
    public class MeshSubset
    {
        /** The ID of the first vertex. */
        public int VertexId;

        /** The total number of vertices. */
        public int NumVertices;

        /** The ID of the first index. */
        public int IndexId;

        /** The total number of indices. */
        public int NumIndices;

        /** Creates a new MeshSubset. */
        public MeshSubset(int vertexId = 0, int numVertices = -1,
                          int indexId = 0, int numIndices = -1)
        {
            SetTo(vertexId, numVertices, indexId, numIndices);
        }

        /** Changes all properties at once.
         *  Call without any arguments to reference a complete mesh. */
        public void SetTo(int vertexId = 0, int numVertices = -1,
                          int indexId = 0, int numIndices = -1)
        {
            VertexId = vertexId;
            NumVertices = numVertices;
            IndexId = indexId;
            NumIndices = numIndices;
        }
}
}
