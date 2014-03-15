using System;
using Sparrow.Utils;

namespace Sparrow.Geom
{
	public class Point
	{
		private float _x;
		private float _y;

		public float X {
			get { return _x;}
			set { _x = value;}
		}

		public float Y {
			get { return _y;}
			set { _y = value;}
		}

		public float Length {
			get {
				return (float) Math.Sqrt (Math.Pow (_x, 2.0f) +  Math.Pow (_y, 2.0f));
			}
		}

		public float SquaredLength {
			get {
				return (float) (Math.Pow (_x, 2.0f) + Math.Pow (_y, 2.0f));
			}
		}

		public float Angle {
			get {
				return (float) Math.Atan2 (_y, _x);
			}
		}

		public bool IsOrigin {
			get {
				return _x == 0.0f && _y == 0.0f;
			}
		}

		public Point (float x = 0.0f, float y = 0.0f)
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

		public Point ScaleBy (float scalar)
		{
			return new Point (_x * scalar, _y * scalar);
		}

		public Point RotateBy (float angle)
		{
			float sin;
			float cos;

			//always wrap input angle to -PI..PI
			if (angle < -3.14159265f)
				angle += 6.28318531f;
			else
				if (angle >  3.14159265f)
					angle -= 6.28318531f;

			//compute sine
			if (angle < 0.0f)
				sin = 1.27323954f * angle + .405284735f * angle * angle;
			else
				sin = 1.27323954f * angle - 0.405284735f * angle * angle;

			//compute cosine: sin(x + PI/2) = cos(x)
			angle += 1.57079632f;
			if (angle>  3.14159265f)
				angle -= 6.28318531f;

			if (angle < 0.0f)
				cos = 1.27323954f * angle + 0.405284735f * angle * angle;
			else
				cos = 1.27323954f * angle - 0.405284735f * angle * angle;

			return new Point (_x * cos - _y * sin, _x * sin + _y * cos);
		}

		public Point Normalize ()
		{
			if (IsOrigin) {
				// TODO: throw exception "Cannot normalize point in the origin"
			}

			float inverseLength = 1 / Length;
			return new Point (_x * inverseLength, _y * inverseLength);

		}

		public float Dot (Point other)
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