using System;
using Sparrow.Utils;
using SparrowSharp.Pool;

namespace Sparrow.Geom
{
	public class Point : PooledObject
	{
		private static readonly ObjectPool _pool = new ObjectPool (new CreateObject<PooledObject> (Init), 1000);

		public static Point Create (float x = 0.0f, float y = 0.0f)
		{
			Point point = (Point)_pool.GetObject ();
			point.X = x;
			point.Y = y;

			return point;
		}

		private static Point Init ()
		{
			return new Point ();
		}

		private float _x;
		private float _y;

		public float X {
			get { return _x; }
			set { _x = value; }
		}

		public float Y {
			get { return _y; }
			set { _y = value; }
		}

		public float Length {
			get {
				return (float)Math.Sqrt (Math.Pow (_x, 2.0f) + Math.Pow (_y, 2.0f));
			}
		}

		public float SquaredLength {
			get {
				return (float)(Math.Pow (_x, 2.0f) + Math.Pow (_y, 2.0f));
			}
		}

		public float Angle {
			get {
				return (float)Math.Atan2 (_y, _x);
			}
		}

		public bool IsOrigin {
			get {
				return _x == 0.0f && _y == 0.0f;
			}
		}

		private Point (float x = 0.0f, float y = 0.0f)
		{
			_x = x;
			_y = y;
		}

		public Point Invert ()
		{
			return Point.Create (-_x, -_y);
		}

		public Point AddPoint (Point point)
		{
			return Point.Create (_x + point.X, _y + point.Y);
		}

		public Point SubtractPoint (Point point)
		{
			return Point.Create (_x - point.X, _y - point.Y);
		}

		public Point ScaleBy (float scalar)
		{
			return Point.Create (_x * scalar, _y * scalar);
		}

		public Point RotateBy (float angle)
		{
			float sin = NumberUtil.FastSin (angle);
			float cos = NumberUtil.FastCos (angle);
			return Point.Create (_x * cos - _y * sin, _x * sin + _y * cos);
		}

		public Point Normalize ()
		{
			if (IsOrigin) {
				// TODO: throw exception "Cannot normalize point in the origin"
			}

			float inverseLength = 1 / Length;
			return Point.Create (_x * inverseLength, _y * inverseLength);

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

		public float Distance(Point p2) {
			return (float)Math.Sqrt ((_x - p2.X) * (_x - p2.X) + (_y - p2.Y) * (_y - p2.Y));
		}
	}
}