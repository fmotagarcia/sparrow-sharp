
namespace SparrowSharp.Core.Utils
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
        public MeshSubset(int vertexID = 0, int numVertices = -1,
                          int indexID = 0, int numIndices = -1)
        {
            SetTo(vertexID, numVertices, indexID, numIndices);
        }

        /** Changes all properties at once.
         *  Call without any arguments to reference a complete mesh. */
        public void SetTo(int vertexID = 0, int numVertices = -1,
                          int indexID = 0, int numIndices = -1)
        {
            VertexID = vertexID;
            NumVertices = numVertices;
            IndexID = indexID;
            NumIndices = numIndices;
        }
}
}
