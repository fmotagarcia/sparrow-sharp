using System;
using Sparrow.Utils;
using OpenTK;

namespace Sparrow.Geom
{
	public class Matrix
	{
		private static void SetValues(Matrix matrix, float a, float b, float c, float d, float tx, float ty) {
			matrix.A = a;
			matrix.B = b;
			matrix.C = c;
            matrix.D = d;
			matrix.Tx = tx;
			matrix.Ty = ty;  	
		}

	    public float A;
	    public float B;
	    public float C;
	    public float D;

	    public float Tx;
	    public float Ty;

	    public float Determinant {
            get { return A * D - C * B; }
		}

		public float Rotation {
			get { return (float) Math.Atan2 (B, A); }
		}

		public float ScaleX {
			get { return A / (float) Math.Cos (SkewY); }
		}

		public float ScaleY {
            get { return D / (float)Math.Cos(SkewX); }
		}

		public float SkewX {
            get { return (float)Math.Atan(-C / D); }
		}

		public float SkewY {
			get { return (float) Math.Atan (B / A); }
		}

		public Matrix (float a = 1.0f, float b = 0.0f, float c = 0.0f, float d = 1.0f, float tx = 0.0f, float ty = 0.0f)
		{
			A = a;
			B = b;
			C = c;
            D = d;
			Tx = tx;
			Ty = ty;
		}

		public void AppendMatrix (Matrix matrix)
		{
			SetValues(this, 
				matrix.A * A + matrix.C * B,
				matrix.B * A + matrix.D * B,
                matrix.A * C + matrix.C * D,
                matrix.B * C + matrix.D * D,
				matrix.A * Tx + matrix.C * matrix.Ty + matrix.Tx,
				matrix.B * Tx + matrix.D * matrix.Ty + matrix.Ty);
		}

		public void PrependMatrix (Matrix matrix)
		{
			SetValues (this, 
				A * matrix.A + C * matrix.B,
                B * matrix.A + D * matrix.B,
				A * matrix.C + C * matrix.D,
                B * matrix.C + D * matrix.D,
				Tx + A * matrix.Tx + C * matrix.Ty,
                Ty + B * matrix.Tx + D * matrix.Ty);
		}

		public void Translate (float dx, float dy)
		{
			Tx += dx;
			Ty += dy;
		}

		public void Scale (float sx, float sy)
		{
			if (sx != 1.0f) {
				A *= sx;
				C *= sx;
				Tx *= sx;
			}

			if (sy != 1.0f) {
				B *= sy;
                D *= sy;
				Ty *= sy;
			}
		}

		public void Rotate (float angle)
		{
			if (angle == 0.0f) {
				return;
			}

			float cos = (float) Math.Cos (angle);
			float sin = (float) Math.Sin (angle);

			SetValues(this,
				A * cos - B * sin,
				A * sin + B * cos,
                C * cos - D * sin,
                C * sin + D * cos,
				Tx * cos - Ty * sin,
				Tx * sin + Ty * cos);
		}

		public void Skew (float sx, float sy)
		{
			float sinX = (float) Math.Sin (sx);
			float cosX = (float) Math.Cos (sx);
			float sinY = (float) Math.Sin (sy);
			float cosY = (float) Math.Cos (sy);

			SetValues (this,
				A * cosY - B * sinX,
				A * sinY + B * cosX,
                C * cosY - D * sinX,
                C * sinY + D * cosX,
				Tx * cosY - Ty * sinX,
				Tx * sinY + Ty * cosX);
		}

		public void Identity ()
		{
			A = 1.0f;
			B = 0.0f;
			C = 0.0f;
            D = 1.0f;
			Tx = 0.0f;
			Ty = 0.0f;
		}

		public Point TransformPoint (Point point)
		{
            return new Point(A * point.X + C * point.Y + Tx, B * point.X + D * point.Y + Ty);
		}

		public Point TransformPoint (float x, float y)
		{
            return new Point(A * x + C * y + Tx, B * x + D * y + Ty);
		}

		public void Invert ()
		{
			float det = Determinant;

			SetValues(this,
                A = D / det,
				B = -B / det,
				C = -C / det,
                D = A / det,
                Tx = (C * Ty - D * Tx) / det,
				Ty = (B * Tx - A * Ty) / det);
		}

		public void CopyFromMatrix (Matrix matrix)
		{
			A = matrix.A;
			B = matrix.B;
			C = matrix.C;
            D = matrix.D;
			Tx = matrix.Tx;
			Ty = matrix.Ty;
		}

		public bool IsEqual (Matrix other)
		{
			if (other == this) {
				return true;
			} else if (other == null) {
				return false; 
			} else {
				return NumberUtil.Equals (A, other.A) &&
				NumberUtil.Equals (B, other.B) &&
				NumberUtil.Equals (C, other.C) &&
                NumberUtil.Equals(D, other.D) &&
				NumberUtil.Equals (Tx, other.Tx) &&
				NumberUtil.Equals (Ty, other.Ty);
			}
		}

		public Matrix4d ConvertToMatrix4d ()
		{
			Matrix4d matrix = new Matrix4d ();

			matrix.M11 = A;
			matrix.M12 = B;
			matrix.M21 = C;
            matrix.M22 = D;
			matrix.M41 = Tx;
			matrix.M42 = Ty;

			return matrix;
		}

        public Matrix4 ConvertToMatrix4()
        {
			Matrix4 matrix = new Matrix4()
			{
				Row0 = new OpenTK.Vector4(1, 0, 0, 0),
				Row1 = new OpenTK.Vector4(0, 1, 0, 0),
				Row2 = new OpenTK.Vector4(0, 0, 1, 0),
				Row3 = new OpenTK.Vector4(0, 0, 0, 1),
			};

            matrix.M11 = A;
            matrix.M12 = B;
            matrix.M21 = C;
            matrix.M22 = D;
            matrix.M41 = Tx;
            matrix.M42 = Ty;

            return matrix;
        }

	}
}