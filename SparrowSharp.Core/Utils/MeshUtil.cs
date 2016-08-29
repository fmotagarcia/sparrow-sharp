
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
        private static Vector3D sPoint3D = new Vector3D();
        private static Matrix sMatrix = Matrix.Create();
        private static Matrix3D sMatrix3D = new Matrix3D();
        
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
            Point p0 = Point.Create();
            Point p1 = Point.Create();
            Point p2 = Point.Create();

            for (i = 0; i < numIndices; i += 3)
            {
                vertexData.GetPoint(indexData.GetIndex(i  ), "position", p0);
                vertexData.GetPoint(indexData.GetIndex(i+1), "position", p1);
                vertexData.GetPoint(indexData.GetIndex(i+2), "position", p2);

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

            if (sourceSpace.Is3D && stage)
            {
                stage.GetCameraPosition(targetSpace, sPoint3D);
                sourceSpace.GetTransformationMatrix3D(targetSpace, sMatrix3D);
                vertexData.GetBoundsProjected("position", sMatrix3D, sPoint3D, 0, -1, outRect);
            }
            else
            {
                sourceSpace.GetTransformationMatrix(targetSpace, sMatrix);
                vertexData.GetBounds("position", sMatrix, 0, -1, outRect);
            }
            return outRect;
        }
    }
}
