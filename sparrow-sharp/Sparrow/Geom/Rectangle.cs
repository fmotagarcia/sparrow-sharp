using System;
using Sparrow.Utils;

namespace Sparrow.Geom
{
	public class Rectangle
	{
		private float _x;
		private float _y;
		private float _width;
		private float _height;

		public float X {
			get { return _x;}
			set { _x = value;}
		}

		public float Y {
			get { return _y;}
			set { _y = value;}
		}

		public float Width {
			get { return _width;}
			set { _width = value;}
		}

		public float Height {
			get { return _height;}
			set { _height = value;}
		}

		public float Top {
			get { return _y;}
			set { _y = value;}
		}

		public float Bottom {
			get { return _y + _height;}
			set { _height = value - _y;}
		}

		public float Left {
			get { return _x;}
			set { _x = value;}
		}

		public float Right {
			get { return _x + _width;}
			set { _width = value - _x;}
		}

		public Point TopLeft {
			get { return new Point (_x, _y);}
			set { 
				_x = value.X;
				_y = value.Y;
			}
		}

		public Point BottomRight {
			get { return new Point (_x + _width, _y + _height);}
			set { 
				Right = value.X;
				Bottom = value.Y;
			}
		}

		public Point Size {
			get { return new Point (_width, _height);}
			set {
				_width = value.X;
				_height = value.Y;
			}
		}

		public Rectangle (float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
		{ 
			_x = x;
			_y = y;
			_width = width;
			_height = height;
		}

		public bool Contains (float x, float y)
		{
			return x >= _x && y >= _y && x <= _x + _width && y <= _y + _height;
		}

		public bool Contains (Point point)
		{
			return Contains (point.X, point.Y);
		}

		public bool Contains (Rectangle rectangle)
		{
			if (rectangle == null) {
				return false;
			}

			float rX = rectangle.X;
			float rY = rectangle.Y;
			float rWidth = rectangle.Width;
			float rHeight = rectangle.Height;

			return rX >= _x && rX + rWidth <= _x + _width &&
				rY >= _y && rY + rHeight <= _y + _height;
		}

		public bool Intersects (Rectangle rectangle)
		{
			if (rectangle == null) {
				return false;
			}

			float rX = rectangle.X;
			float rY = rectangle.Y;
			float rWidth = rectangle.Width;
			float rHeight = rectangle.Height;

			bool outside = 
				(rX <= _x && rX + rWidth <= _x) || (rX >= _x + _width && rX + rWidth >= _x + _width) ||
				(rY <= _y && rY + rHeight <= _y) || (rY >= _y + _height && rY + rHeight >= _y + _height);
			return !outside;
		}

		public Rectangle Intersection (Rectangle rectangle)
		{
			if (rectangle == null) {
				return null;
			}

			float left = Math.Max (_x, rectangle.X);
			float right = Math.Min (_x + _width, rectangle.X + rectangle.Width);
			float top = Math.Max (_y, rectangle.Y);
			float bottom = Math.Min (_y + _height, rectangle.Y + rectangle.Height);

			if (left > right || top > bottom) {
				return new Rectangle ();
			} else {
				return new Rectangle (left, top, right - left, bottom - top);
			}
		}

		public Rectangle Union (Rectangle rectangle)
		{
			if (rectangle == null) {
				return null;
			}

			float left = Math.Max (_x, rectangle.X);
			float right = Math.Min (_x + _width, rectangle.X + rectangle.Width);
			float top = Math.Max (_y, rectangle.Y);
			float bottom = Math.Min (_y + _height, rectangle.Y + rectangle.Height);

			return new Rectangle (left, top, right - left, bottom - top);
		}

		public void Inflate (float dx, float dy)
		{
			_x -= dx;
			_y -= dy;
			_width += 2.0f * dx;
			_height += 2.0f * dy;
		}

		public void Empty ()
		{
			_x = _y = _width = _height = 0.0f;
		}

		public void CopyFromRectangle (Rectangle rectangle)
		{
			_x = rectangle.X;
			_y = rectangle.Y;
			_width = rectangle.Width;
			_height = rectangle.Height;
		}

		public void Normalize ()
		{
			if (_width < 0.0f) {
				_width = -_width;
				_x -= _width;
			}

			if (_height < 0.0f) {
				_height = -_height;
				_y -= _height;
			}
		}

		public bool IsEmpty ()
		{
			return _width == 0.0f || _height == 0.0f;
		}

		public bool isEqual (Rectangle other)
		{
			if (other == this) {
				return true;
			} 
			if (other == null) {
				return false;
			}

			return NumberUtil.Equals (_x, other.X) &&
				NumberUtil.Equals (_y, other.Y) &&
				NumberUtil.Equals (_width, other.Width) &&
				NumberUtil.Equals (_height, other.Height);

		}
	}
}