using System;
using Sparrow.Utils;
using OpenTK;

namespace Sparrow.Geom
{
	public class Matrix
	{
		private static void SetValues(Matrix matrix, float a, float b, float c, float d, float tx, float ty) {
			matrix._a = a;
			matrix._b = b;
			matrix._c = c;
			matrix._d = d;
			matrix._tx = tx;
			matrix._ty = ty;  	
		}

		private float _a;
		private float _b;
		private float _c;
		private float _d;
		private float _tx;
		private float _ty;

		public float A {
			get { return _a; }
			set { _a = value; }
		}

		public float B {
			get { return _b; }
			set { _b = value; }
		}

		public float C {
			get { return _c; }
			set { _c = value; }
		}

		public float D {
			get { return _d; }
			set { _d = value; }
		}

		public float Tx {
			get { return _tx; }
			set { _tx = value; }
		}

		public float Ty {
			get { return _ty; }
			set { _ty = value; }
		}

		public float Determinant {
			get { return _a * _d - _c * _b; }
		}

		public float Rotation {
			get { return (float) Math.Atan2 (_b, _a); }
		}

		public float ScaleX {
			get { return _a / (float) Math.Cos (SkewY); }
		}

		public float ScaleY {
			get { return _d / (float) Math.Cos (SkewX); }
		}

		public float SkewX {
			get { return (float) Math.Atan (-_c / _d); }
		}

		public float SkewY {
			get { return (float) Math.Atan (_b / _a); }
		}

		public Matrix (float a = 1.0f, float b = 0.0f, float c = 0.0f, float d = 1.0f, float tx = 0.0f, float ty = 0.0f)
		{
			_a = a;
			_b = b;
			_c = c;
			_d = d;
			_tx = tx;
			_ty = ty;
		}

		public void AppendMatrix (Matrix matrix)
		{
			SetValues(this, 
				matrix.A * _a + matrix.C * _b,
				matrix.B * _a + matrix.D * _b,
				matrix.A * _c + matrix.C * _d,
				matrix.B * _c + matrix.D * _d,
				matrix.A * _tx + matrix.C * matrix.Ty + matrix.Tx,
				matrix.B * _tx + matrix.D * matrix.Ty + matrix.Ty);
		}

		public void PrependMatrix (Matrix matrix)
		{
			SetValues (this, 
				_a * matrix.A + _c * matrix.B,
				_b * matrix.A + _d * matrix.B,
				_a * matrix.C + _c * matrix.D,
				_b * matrix.C + _d * matrix.D,
				_tx + _a * matrix.Tx + _c * matrix.Ty,
				_ty + _b * matrix.Tx + _d * matrix.Ty);
		}

		public void Translate (float dx, float dy)
		{
			_tx += dx;
			_ty += dy;
		}

		public void Scale (float sx, float sy)
		{
			if (sx != 1.0f) {
				_a *= sx;
				_c *= sx;
				_tx *= sx;
			}

			if (sy != 1.0f) {
				_b *= sy;
				_d *= sy;
				_ty *= sy;
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
				_a * cos - _b * sin,
				_a * sin + _b * cos,
				_c * cos - _d * sin,
				_c * sin + _d * cos,
				_tx * cos - _ty * sin,
				_tx * sin + _ty * cos);
		}

		public void Skew (float sx, float sy)
		{
			float sinX = (float) Math.Sin (sx);
			float cosX = (float) Math.Cos (sx);
			float sinY = (float) Math.Sin (sy);
			float cosY = (float) Math.Cos (sy);

			SetValues (this,
				_a * cosY - _b * sinX,
				_a * sinY + _b * cosX,
				_c * cosY - _d * sinX,
				_c * sinY + _d * cosX,
				_tx * cosY - _ty * sinX,
				_tx * sinY + _ty * cosX);
		}

		public void Identity ()
		{
			_a = 1.0f;
			_b = 0.0f;
			_c = 0.0f;
			_d = 1.0f;
			_tx = 0.0f;
			_ty = 0.0f;
		}

		public Point TransformPoint (Point point)
		{
			return new Point (_a * point.X + _c * point.Y + _tx, _b * point.X + _d * point.Y + _ty);
		}

		public Point TransformPoint (float x, float y)
		{
			return new Point (_a * x + _c * y + _tx, _b * x + _d * y + _ty);
		}

		public void Invert ()
		{
			float det = Determinant;

			SetValues(this,
				_a = _d / det,
				_b = -_b / det,
				_c = -_c / det,
				_d = _a / det,
				_tx = (_c * _ty - _d * _tx) / det,
				_ty = (_b * _tx - _a * _ty) / det);
		}

		public void CopyFromMatrix (Matrix matrix)
		{
			_a = matrix.A;
			_b = matrix.B;
			_c = matrix.C;
			_d = matrix.D;
			_tx = matrix.Tx;
			_ty = matrix.Ty;
		}

		public bool IsEqual (Matrix other)
		{
			if (other == this) {
				return true;
			} else if (other == null) {
				return false; 
			} else {
				return NumberUtil.Equals (_a, other.A) &&
				NumberUtil.Equals (_b, other.B) &&
				NumberUtil.Equals (_c, other.C) &&
				NumberUtil.Equals (_d, other.D) &&
				NumberUtil.Equals (_tx, other.Tx) &&
				NumberUtil.Equals (_ty, other.Ty);
			}
		}

		public Matrix4d ConvertToMatrix4d ()
		{
			Matrix4d matrix = new Matrix4d ();

			matrix.M11 = _a;
			matrix.M12 = _b;
			matrix.M21 = _c;
			matrix.M22 = _d;
			matrix.M41 = _tx;
			matrix.M42 = _ty;

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

            matrix.M11 = _a;
            matrix.M12 = _b;
            matrix.M21 = _c;
            matrix.M22 = _d;
            matrix.M41 = _tx;
            matrix.M42 = _ty;

            return matrix;
        }

	}
}