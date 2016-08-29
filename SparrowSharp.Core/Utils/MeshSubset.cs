﻿
namespace SparrowSharp.Core.Utils
{
    public class MeshSubset
    {
        /** The ID of the first vertex. */
        public int vertexID;

        /** The total number of vertices. */
        public int numVertices;

        /** The ID of the first index. */
        public int indexID;

        /** The total number of indices. */
        public int numIndices;

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
            this.vertexID = vertexID;
            this.numVertices = numVertices;
            this.indexID = indexID;
            this.numIndices = numIndices;
        }
}
}