using System;
using Sparrow.Utils;
using Sparrow.Pool;

namespace Sparrow.Geom
{
    /// <summary>
    /// The Matrix class describes an affine, 2D transformation Matrix. It provides methods to
    /// manipulate the matrix in convenient ways, and can be used to transform points.
    /// 
    /// The matrix has the following form:
    /// 
    /// |a  c tx|
    /// |b  d ty|
    /// |0  0  1| 
    /// </summary>
    public class Matrix2D : PooledObject
    {
        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        public static Matrix2D Create(float a = 1.0f, float b = 0.0f, float c = 0.0f, float d = 1.0f, float tx = 0.0f, float ty = 0.0f)
        {
            Matrix2D matrix = (Matrix2D)_pool.GetObject();
            matrix.A = a;
            matrix.B = b;
            matrix.C = c;
            matrix.D = d;
            matrix.Tx = tx;
            matrix.Ty = ty;

            return matrix;
        }

        private static Matrix2D Init()
        {
            return new Matrix2D();
        }

        public float A;
        public float B;
        public float C;
        public float D;
        public float Tx;
        public float Ty;

        public float Determinant
        {
            get { return A * D - C * B; }
        }

        public float Rotation
        {
            get { return (float)Math.Atan2(B, A); }
        }

        public float ScaleX
        {
            get { return A / (float)Math.Cos(SkewY); }
        }

        public float ScaleY
        {
            get { return D / (float)Math.Cos(SkewX); }
        }

        public float SkewX
        {
            get { return (float)Math.Atan(-C / D); }
        }

        public float SkewY
        {
            get { return (float)Math.Atan(B / A); }
        }

        private Matrix2D(float a = 1.0f, float b = 0.0f, float c = 0.0f, float d = 1.0f, float tx = 0.0f, float ty = 0.0f)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            Tx = tx;
            Ty = ty;
        }

        // matrix multiplication, called concat in AS3
        public void AppendMatrix(Matrix2D matrix)
        {
            float a = matrix.A * A + matrix.C * B;
            float b = matrix.B * A + matrix.D * B;
            float c = matrix.A * C + matrix.C * D;
            float d = matrix.B * C + matrix.D * D;
            float tx = matrix.A * Tx + matrix.C * Ty + matrix.Tx;
            float ty = matrix.B * Tx + matrix.D * Ty + matrix.Ty;

            A = a;
            B = b;
            C = c;
            D = d;
            Tx = tx;
            Ty = ty;
        }

        public void PrependMatrix(Matrix2D matrix)
        {
            float a = A * matrix.A + C * matrix.B;
            float b = B * matrix.A + D * matrix.B;
            float c = A * matrix.C + C * matrix.D;
            float d = B * matrix.C + D * matrix.D;
            float tx = Tx + A * matrix.Tx + C * matrix.Ty;
            float ty = Ty + B * matrix.Tx + D * matrix.Ty;

            A = a;
            B = b;
            C = c;
            D = d;
            Tx = tx;
            Ty = ty;
        }

        public void Translate(float dx, float dy)
        {
            Tx += dx;
            Ty += dy;
        }

        public void PrependTranslation(float dx, float dy)
        {
            Tx += A * dx + C * dy;
            Ty += B * dx + D * dy;
        }

        public void Scale(float sx, float sy)
        {
            if (sx != 1.0f)
            {
                A *= sx;
                C *= sx;
                Tx *= sx;
            }

            if (sy != 1.0f)
            {
                B *= sy;
                D *= sy;
                Ty *= sy;
            }
        }

        /// <summary>
        /// Applies a rotation on the matrix (angle in radians).
        /// </summary>
        public void Rotate(float angleInRadians)
        {
            if (angleInRadians == 0.0f)
            {
                return;
            }
            float sin = MathUtil.FastSin(angleInRadians);
            float cos = MathUtil.FastCos(angleInRadians);

            float a = A * cos - B * sin;
            float b = A * sin + B * cos;
            float c = C * cos - D * sin;
            float d = C * sin + D * cos;
            float tx = Tx * cos - Ty * sin;
            float ty = Tx * sin + Ty * cos;

            A = a;
            B = b;
            C = c;
            D = d;
            Tx = tx;
            Ty = ty;
        }

        /// <summary>
        /// Appends a skew transformation to a matrix (angles in radians).
        /// The skew matrix has the following form:
        ///
        ///     | cos(skewY)  -sin(skewX)  0 |
        ///     | sin(skewY)   cos(skewX)  0 |
        ///     |     0            0       1 |
        /// </summary>
        public void Skew(float sx, float sy)
        {
            float sinX = (float)Math.Sin(sx);
            float cosX = (float)Math.Cos(sx);
            float sinY = (float)Math.Sin(sy);
            float cosY = (float)Math.Cos(sy);

            float a = A * cosY - B * sinX;
            float b = A * sinY + B * cosX;
            float c = C * cosY - D * sinX;
            float d = C * sinY + D * cosX;
            float tx = Tx * cosY - Ty * sinX;
            float ty = Tx * sinY + Ty * cosX;

            A = a;
            B = b;
            C = c;
            D = d;
            Tx = tx;
            Ty = ty;
        }

        /// <summary>
        /// Converts the matrix to an Identity matrix
        /// </summary>
        public void Identity()
        {
            A = 1.0f;
            B = 0.0f;
            C = 0.0f;
            D = 1.0f;
            Tx = 0.0f;
            Ty = 0.0f;
        }

        public bool IsIdentity()
        {
            if (A == 1.0f && B == 0.0f && C == 0.0f && 
                D == 1.0f && Tx == 0.0f && Ty == 0.0f)
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a point that is transformed by this matrix
        /// </summary>
        public Point TransformPoint(Point point)
        {
            return Point.Create(A * point.X + C * point.Y + Tx, B * point.X + D * point.Y + Ty);
        }

        public Point TransformPoint(float x, float y)
        {
            return Point.Create(A * x + C * y + Tx, B * x + D * y + Ty);
        }

        public Matrix2D Invert()
        {
            float det = Determinant;

            float a = D / det;
            float b = -B / det;
            float c = -C / det;
            float d = A / det;
            float tx = (C * Ty - D * Tx) / det;
            float ty = (B * Tx - A * Ty) / det;

            A = a;
            B = b;
            C = c;
            D = d;
            Tx = tx;
            Ty = ty;
            return this;
        }

        public void CopyFromMatrix(Matrix2D matrix)
        {
            A = matrix.A;
            B = matrix.B;
            C = matrix.C;
            D = matrix.D;
            Tx = matrix.Tx;
            Ty = matrix.Ty;
        }

        public bool IsEqual(Matrix2D other)
        {
            if (other == this)
            {
                return true;
            }
            else if (other == null)
            {
                return false; 
            }
            else
            {
                return MathUtil.Equals(A, other.A) &&
                MathUtil.Equals(B, other.B) &&
                MathUtil.Equals(C, other.C) &&
                MathUtil.Equals(D, other.D) &&
                MathUtil.Equals(Tx, other.Tx) &&
                MathUtil.Equals(Ty, other.Ty);
            }
        }

        public Matrix3D ConvertToMatrix3D()
        {
            float[] matrix = {
                             A, B, 0, 0,
                             C, D, 0, 0,
                             0, 0, 1, 0,
                             Tx, Ty, 0, 1
                             };
            return Matrix3D.Create(matrix);
        }

        public override string ToString() 
        {
            return "A: " + A + " B: " + B + " C: " + C + " D: " + D + " Tx: " + Tx + " Ty: " + Ty;
        }
    }
}