
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Utils;
using SparrowSharp.Core.Rendering;

namespace SparrowSharp.Core.Utils
{
    /// <summary>
    /// A utility class that helps with tasks that are common when working with meshes.
    /// </summary>
    public class MeshUtil
    {
        // helper objects
        private static Matrix sMatrix = Matrix.Create();
        
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
            Point p0;
            Point p1;
            Point p2;

            for (i = 0; i < numIndices; i += 3)
            {
                p0 = vertexData.GetPoint(i);
                p1 = vertexData.GetPoint(i + 1);
                p2 = vertexData.GetPoint(i + 2);

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
                                                DisplayObject targetSpace,
                                                Rectangle outRect = null)
        {
            if (outRect == null) outRect = new Rectangle();

            Stage stage = sourceSpace.Stage;
            sMatrix = sourceSpace.GetTransformationMatrix(targetSpace);
            outRect = vertexData.BoundsAfterTransformation(sMatrix);
            return outRect;
        }
    }
}
