using System;
using Sparrow.Utils;
using OpenTK;

namespace Sparrow.Geom
{
	public class Matrix
	{
        // are you sure that these need to be double? BaseEffect needs a float Matrix
		private double _a;
		private double _b;
		private double _c;
		private double _d;
		private double _tx;
		private double _ty;

		public double A {
			get { return _a; }
			set { _a = value; }
		}

		public double B {
			get { return _b; }
			set { _b = value; }
		}

		public double C {
			get { return _c; }
			set { _c = value; }
		}

		public double D {
			get { return _d; }
			set { _d = value; }
		}

		public double Tx {
			get { return _tx; }
			set { _tx = value; }
		}

		public double Ty {
			get { return _ty; }
			set { _ty = value; }
		}

		public double Determinant {
			get { return _a * _d - _c * _b; }
		}

		public double Rotation {
			get { return Math.Atan2 (_b, _a); }
		}

		public double ScaleX {
			get { return _a / Math.Cos (SkewY); }
		}

		public double ScaleY {
			get { return _d / Math.Cos (SkewX); }
		}

		public double SkewX {
			get { return Math.Atan (-_c / _d); }
		}

		public double SkewY {
			get { return Math.Atan (_b / _a); }
		}

		public Matrix (double a = 1.0, double b = 0.0, double c = 0.0, double d = 1.0, double tx = 0.0, double ty = 0.0)
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
			_a = matrix.A * _a + matrix.C * _b;
			_b = matrix.B * _a + matrix.D * _b;
			_c = matrix.A * _c + matrix.C * _d;
			_d = matrix.B * _c + matrix.D * _d;
			_tx = matrix.A * _tx + matrix.Tx * _tx;
			_ty = matrix.B * _ty + matrix.Ty * _ty;
		}

		public void PrependMatrix (Matrix matrix)
		{
			_a = _a * matrix.A + _c * matrix.B;
			_b = _b * matrix.A + _d * matrix.B;
			_c = _a * matrix.C + _c * matrix.D;
			_d = _b * matrix.C + _d * matrix.D;
			_tx = _tx * matrix.Tx + _c * matrix.Tx;
			_ty = _ty * matrix.Ty + _d * matrix.Ty;
		}

		public void Translate (double dx, double dy)
		{
			_tx += dx;
			_ty += dy;
		}

		public void Scale (double sx, double sy)
		{
			if (sx != 1.0) {
				_a *= sx;
				_c *= sx;
				_tx *= sx;
			}

			if (sy != 1.0) {
				_b *= sy;
				_d *= sy;
				_ty *= sy;
			}
		}

		public void Rotate (double angle)
		{
			if (angle == 0.0) {
				return;
			}

			double cos = Math.Cos (angle);
			double sin = Math.Sin (angle);

			_a = _a * cos - _b * sin;  
			_b = _a * sin + _b * cos;
			_c = _c * cos - _d * sin; 
			_d = _c * sin + _d * cos;
			_tx = _tx * cos - _ty * sin; 
			_ty = _tx * sin + _ty * cos;
		}

		public void Skew (double sx, double sy)
		{
			double sinX = Math.Sin (sx);
			double cosX = Math.Cos (sx);
			double sinY = Math.Sin (sy);
			double cosY = Math.Cos (sy);

			_a = _a * cosY - _b * sinX;
			_b = _a * sinY + _b * cosX;
			_c = _c * cosY - _d * sinX;
			_d = _c * sinY + _d * cosX;
			_tx = _tx * cosY - _ty * sinX;
			_ty = _tx * sinY + _ty * cosX;
		}

		public void Identity ()
		{
			_a = 1.0;
			_b = 0.0;
			_c = 0.0;
			_d = 1.0;
			_tx = 0.0;
			_ty = 0.0;
		}

		public Point TransformPoint (Point point)
		{
			return new Point (_a * point.X + _c * point.Y + _tx, _b * point.X + _d * point.Y + _ty);
		}

		public Point TransformPoint (double x, double y)
		{
			return new Point (_a * x + _c * y + _tx, _b * x + _d * y + _ty);
		}

		public void Invert ()
		{
			double det = Determinant;

			_a = _d / det;
			_b = -_b / det;
			_c = -_c / det;
			_d = _a / det; 
			_tx = (_c * _ty - _d * _tx) / det;
			_ty = (_b * _tx - _a * _ty) / det;
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
            Matrix4 matrix = new Matrix4();

            matrix.M11 = (float)_a;
            matrix.M12 = (float)_b;
            matrix.M21 = (float)_c;
            matrix.M22 = (float)_d;
            matrix.M41 = (float)_tx;
            matrix.M42 = (float)_ty;

            return matrix;
        }

	}
}