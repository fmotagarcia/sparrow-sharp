using System;
using Sparrow.Utils;

namespace Sparrow.Geom
{
	public class Rectangle
	{

        public float X;

        public float Y;

        public float Width;

        public float Height;

		public float Top {
			get { return Y;}
			set { Y = value;}
		}

		public float Bottom {
			get { return Y + Height;}
			set { Height = value - Y;}
		}

		public float Left {
			get { return X;}
			set { X = value;}
		}

		public float Right {
			get { return X + Width;}
			set { Width = value - X;}
		}

		public Point TopLeft {
			get { return Point.Create (X, Y);}
			set { 
				X = value.X;
				Y = value.Y;
			}
		}

		public Point BottomRight {
			get { return Point.Create (X + Width, Y + Height);}
			set { 
				Right = value.X;
				Bottom = value.Y;
			}
		}

		public Point Size {
			get { return Point.Create (Width, Height);}
			set {
				Width = value.X;
				Height = value.Y;
			}
		}

		public Rectangle (float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
		{ 
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public bool Contains (float x, float y)
		{
			return x >= X && y >= Y && x <= X + Width && y <= Y + Height;
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

			return rX >= X && rX + rWidth <= X + Width &&
				rY >= Y && rY + rHeight <= Y + Height;
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
				(rX <= X && rX + rWidth <= X) || (rX >= X + Width && rX + rWidth >= X + Width) ||
				(rY <= Y && rY + rHeight <= Y) || (rY >= Y + Height && rY + rHeight >= Y + Height);
			return !outside;
		}

        /// <summary>
        /// Returns the intersecting rectangle
        /// </summary>
		public Rectangle Intersection (Rectangle rectangle)
		{
			if (rectangle == null) {
				return null;
			}

			float left = Math.Max (X, rectangle.X);
			float right = Math.Min (X + Width, rectangle.X + rectangle.Width);
			float top = Math.Max (Y, rectangle.Y);
			float bottom = Math.Min (Y + Height, rectangle.Y + rectangle.Height);

			if (left > right || top > bottom) {
				return new Rectangle ();
			} else {
				return new Rectangle (left, top, right - left, bottom - top);
			}
		}

        /// <summary>
        /// Returns a rectangle that encompasses both rectangles
        /// </summary>
		public Rectangle Union (Rectangle rectangle)
		{
			if (rectangle == null) {
				return null;
			}

			float left = Math.Max (X, rectangle.X);
			float right = Math.Min (X + Width, rectangle.X + rectangle.Width);
			float top = Math.Max (Y, rectangle.Y);
			float bottom = Math.Min (Y + Height, rectangle.Y + rectangle.Height);

			return new Rectangle (left, top, right - left, bottom - top);
		}

		public void Inflate (float dx, float dy)
		{
			X -= dx;
			Y -= dy;
			Width += 2.0f * dx;
			Height += 2.0f * dy;
		}

		public void Empty ()
		{
			X = Y = Width = Height = 0.0f;
		}

		public void CopyFromRectangle (Rectangle rectangle)
		{
			X = rectangle.X;
			Y = rectangle.Y;
			Width = rectangle.Width;
			Height = rectangle.Height;
		}

        /// <summary>
        /// Inverts X or Y if they are negative
        /// </summary>
		public void Normalize ()
		{
			if (Width < 0.0f) {
				Width = -Width;
				X -= Width;
			}

			if (Height < 0.0f) {
				Height = -Height;
				Y -= Height;
			}
		}

		public bool IsEmpty ()
		{
			return Width == 0.0f || Height == 0.0f;
		}

        /// <summary>
        /// Determines whether this instance is equal the specified other with a small Epsilon error margin.
        /// </summary>
		public bool IsEqual (Rectangle other)
		{
			if (other == this) {
				return true;
			} 
			if (other == null) {
				return false;
			}

			return NumberUtil.Equals (X, other.X) &&
				NumberUtil.Equals (Y, other.Y) &&
				NumberUtil.Equals (Width, other.Width) &&
				NumberUtil.Equals (Height, other.Height);
		}

		public Rectangle Copy() 
		{
			return new Rectangle (X, Y, Width, Height);
		}
	}
}