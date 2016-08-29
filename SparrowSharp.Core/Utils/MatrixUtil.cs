using Sparrow.Geom;
using System;

namespace SparrowSharp.Core.Utils
{
    public class MatrixUtil
    {

        /** Updates the given matrix so that it points exactly to pixel boundaries. This works
         *  only if the object is unscaled and rotated by a multiple of 90 degrees.
         *
         *  @param matrix    The matrix to manipulate in place (normally the modelview matrix).
         *  @param pixelSize The size (in points) that represents one pixel in the back buffer.
         */
        public static void SnapToPixels(Matrix matrix, float pixelSize)
        {
            // Snapping only makes sense if the object is unscaled and rotated only by
            // multiples of 90 degrees. If that's the case can be found out by looking
            // at the modelview matrix.

            const float E = 0.0001f;

            bool doSnap = false;
            float aSq, bSq, cSq, dSq;

            if (matrix.B + E > 0 && matrix.B - E< 0 && matrix.C + E> 0 && matrix.C - E< 0)
            {
                // what we actually want is 'Math.abs(matrix.a)', but squaring
                // the value works just as well for our needs & is faster.

                aSq = matrix.A* matrix.A;
                dSq = matrix.D* matrix.D;
                doSnap = aSq + E > 1 && aSq - E< 1 && dSq + E> 1 && dSq - E< 1;
            }
            else if (matrix.A + E > 0 && matrix.A - E< 0 && matrix.D + E> 0 && matrix.D - E< 0)
            {
                bSq = matrix.B* matrix.B;
                cSq = matrix.C* matrix.C;
                doSnap = bSq + E > 1 && bSq - E< 1 && cSq + E> 1 && cSq - E< 1;
            }

            if (doSnap)
            {
                matrix.Tx = (float)Math.Round(matrix.Tx / pixelSize) * pixelSize;
                matrix.Ty = (float)Math.Round(matrix.Ty / pixelSize) * pixelSize;
            }
        }
    }
}
