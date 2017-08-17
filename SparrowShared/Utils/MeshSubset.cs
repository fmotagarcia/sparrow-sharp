
namespace Sparrow.Utils
{
    public class MeshSubset
    {
        /** The ID of the first vertex. */
        public int VertexID;

        /** The total number of vertices. */
        public int NumVertices;

        /** The ID of the first index. */
        public int IndexID;

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
            VertexID = vertexId;
            NumVertices = numVertices;
            IndexID = indexId;
            NumIndices = numIndices;
        }
}
}
