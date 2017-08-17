
using Sparrow.Geom;
using System;

namespace Sparrow.Utils
{
    public class MatrixUtil
    {

        /** Updates the given matrix so that it points exactly to pixel boundaries. This works
         *  only if the object is unscaled and rotated by a multiple of 90 degrees.
         *
         *  @param matrix    The matrix to manipulate in place (normally the modelview matrix).
         *  @param pixelSize The size (in points) that represents one pixel in the back buffer.
         */
        public static void SnapToPixels(Matrix2D matrix, float pixelSize)
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

        /** Creates a perspective projection matrix suitable for 2D and 3D rendering.
        *
        *  <p>The first 4 parameters define which area of the stage you want to view (the camera
        *  will 'zoom' to exactly this region). The final 3 parameters determine the perspective
        *  in which you're looking at the stage.</p>
        *
        *  <p>The stage is always on the rectangle that is spawned up between x- and y-axis (with
        *  the given size). All objects that are exactly on that rectangle (z equals zero) will be
        *  rendered in their true size, without any distortion.</p>
        *
        *  <p>If you pass only the first 4 parameters, the camera will be set up above the center
        *  of the stage, with a field of view of 1.0 rad.</p>
        */
        public static Matrix3D CreatePerspectiveProjectionMatrix(
                float x, float y, float width, float height,
                float stageWidth = 0f, float stageHeight = 0f, float[] cameraPos = null)
        {
            Matrix3D outMatrix = Matrix3D.Create();
            if (stageWidth  <= 0) stageWidth = width;
            if (stageHeight <= 0) stageHeight = height;
            if (cameraPos == null)
            {
                cameraPos = new[] {
                    stageWidth / 2f, stageHeight / 2f,// -> center of stage
                    stageWidth / (float)Math.Tan(0.5f) * 0.5f };// -> fieldOfView = 1.0 rad
            }

            float focalLength = Math.Abs(cameraPos[2]);
            float offsetX = cameraPos[0] - stageWidth  / 2f;
            float offsetY = cameraPos[1] - stageHeight / 2f;
            float far    = focalLength * 20f;
            const float near = 1f;
            float scaleX = stageWidth  / width;
            float scaleY = stageHeight / height;

            // set up general perspective
            float[] sMatrixData = new float[16];
            sMatrixData[0] =  2 * focalLength / stageWidth;  // 0,0
            sMatrixData[5] = -2 * focalLength / stageHeight; // 1,1  [negative to invert y-axis]
            sMatrixData[10] =  far / (far - near);           // 2,2
            sMatrixData[14] = -far* near / (far - near);     // 2,3
            sMatrixData[11] =  1;                            // 3,2

            // now zoom in to visible area
            sMatrixData[0] *=  scaleX;
            sMatrixData[5] *=  scaleY;
            sMatrixData[8]  =  scaleX - 1 - 2 * scaleX * (x - offsetX) / stageWidth;
            sMatrixData[9]  = -scaleY + 1 + 2 * scaleY * (y - offsetY) / stageHeight;

            outMatrix.CopyRawDataFrom(sMatrixData);
            outMatrix.PrependTranslation(
                    -stageWidth /2.0f - offsetX,
                    -stageHeight/2.0f - offsetY,
                    focalLength);

            return outMatrix;
        }

    }
}
