using System;
using Sparrow.Utils;

namespace Sparrow.Geom
{
	public class Point
	{
		private double _x;
		private double _y;

		public double X {
			get { return _x;}
			set { _x = value;}
		}

		public double Y {
			get { return _y;}
			set { _y = value;}
		}

		public double Length {
			get {
				return Math.Sqrt (Math.Pow (_x, 2.0) +  Math.Pow (_y, 2.0));
			}
		}

		public double SquaredLength {
			get {
				return Math.Pow (_x, 2.0) + Math.Pow (_y, 2.0);
			}
		}

		public double Angle {
			get {
				return Math.Atan2 (_y, _x);
			}
		}

		public bool IsOrigin {
			get {
				return _x == 0.0 && _y == 0.0;
			}
		}

		public Point (double x = 0.0, double y = 0.0)
		{
			_x = x;
			_y = y;
		}

		public Point Invert ()
		{
			return new Point (-_x, -_y);
		}

		public Point AddPoint (Point point)
		{
			return new Point (_x + point.X, _y + point.Y);
		}

		public Point SubtractPoint (Point point)
		{
			return new Point (_x - point.X, _y - point.Y);
		}

		public Point ScaleBy (double scalar)
		{
			return new Point (_x * scalar, _y * scalar);
		}

		public Point RotateBy (double angle)
		{
			double sin = Math.Sin (angle);
			double cos = Math.Cos (angle);

			return new Point (_x * cos - _y * sin, _x * sin + _y * cos);
		}

		public Point Normalize ()
		{
			if (IsOrigin) {
				// TODO: throw exception "Cannot normalize point in the origin"
			}

			double inverseLength = 1 / Length;
			return new Point (_x * inverseLength, _y * inverseLength);

		}

		public double Dot (Point other)
		{
			return _x * other.X + _y * other.Y;
		}

		public void CopyFromPoint (Point point)
		{
			_x = point.X;
			_y = point.Y;
		}

		public bool Equals (Point other)
		{
			if (other == this) {
				return true;
			} 

			if (other == null) {
				return false;
			}

			return NumberUtil.Equals (_x, other.X) && NumberUtil.Equals (_y, other.Y);
		}
	}
}