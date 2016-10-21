
using Sparrow.Pool;
using System;

namespace Sparrow.Geom
{

    public class Matrix3D : PooledObject
    {

        private float[] rawData;

        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        private Matrix3D() { }

        public static Matrix3D Create()
        {
            Matrix3D matrix = (Matrix3D)_pool.GetObject();
            matrix.Identity();
            return matrix;
        }

        public static Matrix3D Create(float[] src)
        {
            Matrix3D matrix = (Matrix3D)_pool.GetObject();
            matrix.rawData = src;
            return matrix;
        }

        private static Matrix3D Init()
        {
            return new Matrix3D();
        }

        public void Identity()
        {
            rawData = new float[]
               { 1, 0, 0, 0,
                 0, 1, 0, 0,
                 0, 0, 1, 0,
                 0, 0, 0, 1};
        }

        public void Append(Matrix3D other)
        {
            Mult(this, other);
        }

        public void Prepend(Matrix3D other)
        {
            Mult(other, this);
        }

        /// <summary>
        /// Multiplies two instances. and writes the result to left
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">The Matrix to write the result to. It can be one of the other input parameters too</param>
        public void Mult(Matrix3D left, Matrix3D right)
        {
            float lM11 = left.rawData[0], lM12 = left.rawData[1], lM13 = left.rawData[2], lM14 = left.rawData[3],
                lM21 = left.rawData[4], lM22 = left.rawData[5], lM23 = left.rawData[6], lM24 = left.rawData[7],
                lM31 = left.rawData[8], lM32 = left.rawData[9], lM33 = left.rawData[10], lM34 = left.rawData[11],
                lM41 = left.rawData[12], lM42 = left.rawData[13], lM43 = left.rawData[14], lM44 = left.rawData[15],

                rM11 = right.rawData[0], rM12 = right.rawData[1], rM13 = right.rawData[2], rM14 = right.rawData[3],
                rM21 = right.rawData[4], rM22 = right.rawData[5], rM23 = right.rawData[6], rM24 = right.rawData[7],
                rM31 = right.rawData[8], rM32 = right.rawData[9], rM33 = right.rawData[10], rM34 = right.rawData[11],
                rM41 = right.rawData[12], rM42 = right.rawData[13], rM43 = right.rawData[14], rM44 = right.rawData[15];

            rawData[0] = (((lM11 * rM11) + (lM12 * rM21)) + (lM13 * rM31)) + (lM14 * rM41);
            rawData[1] = (((lM11 * rM12) + (lM12 * rM22)) + (lM13 * rM32)) + (lM14 * rM42);
            rawData[2] = (((lM11 * rM13) + (lM12 * rM23)) + (lM13 * rM33)) + (lM14 * rM43);
            rawData[3] = (((lM11 * rM14) + (lM12 * rM24)) + (lM13 * rM34)) + (lM14 * rM44);
            rawData[4] = (((lM21 * rM11) + (lM22 * rM21)) + (lM23 * rM31)) + (lM24 * rM41);
            rawData[5] = (((lM21 * rM12) + (lM22 * rM22)) + (lM23 * rM32)) + (lM24 * rM42);
            rawData[6] = (((lM21 * rM13) + (lM22 * rM23)) + (lM23 * rM33)) + (lM24 * rM43);
            rawData[7] = (((lM21 * rM14) + (lM22 * rM24)) + (lM23 * rM34)) + (lM24 * rM44);
            rawData[8] = (((lM31 * rM11) + (lM32 * rM21)) + (lM33 * rM31)) + (lM34 * rM41);
            rawData[9] = (((lM31 * rM12) + (lM32 * rM22)) + (lM33 * rM32)) + (lM34 * rM42);
            rawData[10] = (((lM31 * rM13) + (lM32 * rM23)) + (lM33 * rM33)) + (lM34 * rM43);
            rawData[11] = (((lM31 * rM14) + (lM32 * rM24)) + (lM33 * rM34)) + (lM34 * rM44);
            rawData[12] = (((lM41 * rM11) + (lM42 * rM21)) + (lM43 * rM31)) + (lM44 * rM41);
            rawData[13] = (((lM41 * rM12) + (lM42 * rM22)) + (lM43 * rM32)) + (lM44 * rM42);
            rawData[14] = (((lM41 * rM13) + (lM42 * rM23)) + (lM43 * rM33)) + (lM44 * rM43);
            rawData[15] = (((lM41 * rM14) + (lM42 * rM24)) + (lM43 * rM34)) + (lM44 * rM44);
        }

        public void CopyFrom(Matrix3D src)
        {
            rawData = (float[])src.rawData.Clone();
        }

        public void Invert()
        {
            int[] colIdx = { 0, 0, 0, 0 };
            int[] rowIdx = { 0, 0, 0, 0 };
            int[] pivotIdx = { -1, -1, -1, -1 };

            // convert the matrix to an array for easy looping
            float[,] inverse = {{rawData[0], rawData[1], rawData[2], rawData[3]},
                                {rawData[4], rawData[5], rawData[6], rawData[7]},
                                {rawData[8], rawData[9], rawData[10], rawData[10]},
                                {rawData[12], rawData[13], rawData[14], rawData[15]} };
            int icol = 0;
            int irow = 0;
            for (int i = 0; i < 4; i++)
            {
                // Find the largest pivot value
                float maxPivot = 0.0f;
                for (int j = 0; j < 4; j++)
                {
                    if (pivotIdx[j] != 0)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            if (pivotIdx[k] == -1)
                            {
                                float absVal = Math.Abs(inverse[j, k]);
                                if (absVal > maxPivot)
                                {
                                    maxPivot = absVal;
                                    irow = j;
                                    icol = k;
                                }
                            }
                            else if (pivotIdx[k] > 0)
                            {
                                return;
                            }
                        }
                    }
                }

                ++(pivotIdx[icol]);

                // Swap rows over so pivot is on diagonal
                if (irow != icol)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        float f = inverse[irow, k];
                        inverse[irow, k] = inverse[icol, k];
                        inverse[icol, k] = f;
                    }
                }

                rowIdx[i] = irow;
                colIdx[i] = icol;

                float pivot = inverse[icol, icol];
                // check for singular matrix
                if (pivot == 0.0f)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
                }

                // Scale row so it has a unit diagonal
                float oneOverPivot = 1.0f / pivot;
                inverse[icol, icol] = 1.0f;
                for (int k = 0; k < 4; ++k)
                    inverse[icol, k] *= oneOverPivot;

                // Do elimination of non-diagonal elements
                for (int j = 0; j < 4; ++j)
                {
                    // check this isn't on the diagonal
                    if (icol != j)
                    {
                        float f = inverse[j, icol];
                        inverse[j, icol] = 0.0f;
                        for (int k = 0; k < 4; ++k)
                            inverse[j, k] -= inverse[icol, k] * f;
                    }
                }
            }

            for (int j = 3; j >= 0; --j)
            {
                int ir = rowIdx[j];
                int ic = colIdx[j];
                for (int k = 0; k < 4; ++k)
                {
                    float f = inverse[k, ir];
                    inverse[k, ir] = inverse[k, ic];
                    inverse[k, ic] = f;
                }
            }

            rawData[0] = inverse[0, 0];
            rawData[1] = inverse[0, 1];
            rawData[2] = inverse[0, 2];
            rawData[3] = inverse[0, 3];
            rawData[4] = inverse[1, 0];
            rawData[5] = inverse[1, 1];
            rawData[6] = inverse[1, 2];
            rawData[7] = inverse[1, 3];
            rawData[8] = inverse[2, 0];
            rawData[9] = inverse[2, 1];
            rawData[10] = inverse[2, 2];
            rawData[11] = inverse[2, 3];
            rawData[12] = inverse[3, 0];
            rawData[13] = inverse[3, 1];
            rawData[14] = inverse[3, 2];
            rawData[15] = inverse[3, 3];
        }

        /// <summary>
        /// The underlying raw Matrix data. Its structure looks like
        /// [0, 1, 2, 3,
        ///  4, 5, 6, 7,
        ///  8, 9, 10,11,
        ///  12,13,14,15]
        ///  WARNING: Not a copy
        /// </summary>
        public float[] RawData
        {
            get { return rawData; }
        }

        public void CopyRawDataFrom(float[] arr, bool clone = true)
        {
            if (clone)
            {
                rawData = (float[])arr.Clone();
            }
            else
            {
                rawData = arr;
            }
        }

        public void PrependTranslation(float x, float y, float z)
        {
            rawData[12] += rawData[0] * x + rawData[4] * y + rawData[8] * z;
            rawData[13] += rawData[1] * x + rawData[5] * y + rawData[9] * z;
            rawData[14] += rawData[2] * x + rawData[6] * y + rawData[10] * z;
            rawData[15] += rawData[3] * x + rawData[7] * y + rawData[11] * z;
        }
        /** Uses a matrix to transform 3D coordinates into a different space. If you pass a
         *  'resultVector', the result will be stored in this vector3D instead of creating a
         *  new object. */
        public float[] TransformCoords3D(float x, float y, float z)
        {
            float[] outV = new float[4];
            outV[0] = x * rawData[0] + y * rawData[4] + z * rawData[8] + rawData[12]; // x
            outV[1] = x * rawData[1] + y * rawData[5] + z * rawData[9] + rawData[13]; // y
            outV[2] = x * rawData[2] + y * rawData[6] + z * rawData[10] + rawData[14]; // z
            outV[3] = x * rawData[3] + y * rawData[7] + z * rawData[11] + rawData[15]; // w
            return outV;
        }

        /// <summary>
        /// Flips the matrix by the Y axis
        /// </summary>
        public void Flip(float height)
        {
            rawData[4] = -rawData[4];
            rawData[5] = -rawData[5];
            rawData[13] = -rawData[13];
        }

    }
}
