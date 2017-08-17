
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Rendering;

namespace Sparrow.Utils
{
    /// <summary>
    /// A utility class that helps with tasks that are common when working with meshes.
    /// </summary>
    public class MeshUtil
    {
        // helper objects
        private static Matrix2D _sMatrix = Matrix2D.Create();
        
        private MeshUtil() { }

        /// <summary>
        /// Determines if a point is inside a mesh that is spawned up by the given
        /// vertex- and index-data.
        /// </summary>
        public static bool ContainsPoint(VertexData vertexData, IndexData indexData, Point point)
        {
            int i;
            bool result = false;
            int numIndices = indexData.NumIndices;

            for (i = 0; i < numIndices; i += 3)
            {
                var p0 = vertexData.GetPoint(indexData.GetIndex(i));
                var p1 = vertexData.GetPoint(indexData.GetIndex(i + 1));
                var p2 = vertexData.GetPoint(indexData.GetIndex(i + 2));

                if (MathUtil.IsPointInTriangle(point, p0, p1, p2))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Calculates the bounds of the given vertices in the target coordinate system
        /// </summary>
        public static Rectangle CalculateBounds(VertexData vertexData,
                                                DisplayObject sourceSpace,
                                                DisplayObject targetSpace)
        {
            _sMatrix = sourceSpace.GetTransformationMatrix(targetSpace);
            var outRect = vertexData.BoundsAfterTransformation(_sMatrix);
            return outRect;
        }
    }
}
