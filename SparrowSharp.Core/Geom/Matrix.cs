using System;
using Sparrow.Utils;
using OpenTK;
using Android.Graphics;

namespace Sparrow.Geom
{
	public class Matrix
	{
		private Matrix4 _matrix4 = new Matrix4 () {
			Row0 = new OpenTK.Vector4 (1, 0, 0, 0),
			Row1 = new OpenTK.Vector4 (0, 1, 0, 0),
			Row2 = new OpenTK.Vector4 (0, 0, 1, 0),
			Row3 = new OpenTK.Vector4 (0, 0, 0, 1),
		};
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
			get { return (float)Math.Atan2 (B, A); }
		}

		public float ScaleX {
			get { return A / (float)Math.Cos (SkewY); }
		}

		public float ScaleY {
			get { return D / (float)Math.Cos (SkewX); }
		}

		public float SkewX {
			get { return (float)Math.Atan (-C / D); }
		}

		public float SkewY {
			get { return (float)Math.Atan (B / A); }
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
			float a = matrix.A * A + matrix.C * B;
			float b = matrix.B * A + matrix.D * B;
			float c = matrix.A * C + matrix.C * D;
			float d = matrix.B * C + matrix.D * D;
			float tx = matrix.A * Tx + matrix.C * matrix.Ty + matrix.Tx;
			float ty = matrix.B * Tx + matrix.D * matrix.Ty + matrix.Ty;

			A = a;
			B = b;
			C = c;
			D = d;
			Tx = tx;
			Ty = ty;
		}

		public void PrependMatrix (Matrix matrix)
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
			float sin = NumberUtil.FastSin(angle);
			float cos = NumberUtil.FastCos(angle);

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

		public void Skew (float sx, float sy)
		{
			// TODO: optimize sin / cos
			float sinX = (float)Math.Sin (sx);
			float cosX = (float)Math.Cos (sx);
			float sinY = (float)Math.Sin (sy);
			float cosY = (float)Math.Cos (sy);

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
			return new Point (A * point.X + C * point.Y + Tx, B * point.X + D * point.Y + Ty);
		}

		public Point TransformPoint (float x, float y)
		{
			return new Point (A * x + C * y + Tx, B * x + D * y + Ty);
		}

		public void Invert ()
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
				NumberUtil.Equals (D, other.D) &&
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

		public Matrix4 ConvertToMatrix4 ()
		{
			_matrix4.M11 = A;
			_matrix4.M12 = B;
			_matrix4.M21 = C;
			_matrix4.M22 = D;
			_matrix4.M41 = Tx;
			_matrix4.M42 = Ty;

			return _matrix4;
		}
	}
}