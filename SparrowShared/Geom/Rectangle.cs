using System;
using Sparrow.Utils;
using Sparrow.Pool;

namespace Sparrow.Geom
{
    public class Rectangle : PooledObject
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        public static Rectangle Create(float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
        {
            Rectangle point = (Rectangle)_pool.GetObject();
            point.X = x;
            point.Y = y;
            point.Width = width;
            point.Height = height;
            return point;
        }

        private static Rectangle Init()
        {
            return new Rectangle();
        }

        private Rectangle() {}

        public float Top
        {
            get { return Y; }
            set { Y = value; }
        }

        public float Bottom
        {
            get { return Y + Height; }
            set { Height = value - Y; }
        }

        public float Left
        {
            get { return X; }
            set { X = value; }
        }

        public float Right
        {
            get { return X + Width; }
            set { Width = value - X; }
        }

        public Point TopLeft
        {
            get { return Point.Create(X, Y); }
            set
            { 
                X = value.X;
                Y = value.Y;
            }
        }

        public Point BottomRight
        {
            get { return Point.Create(X + Width, Y + Height); }
            set
            { 
                Right = value.X;
                Bottom = value.Y;
            }
        }

        public Point Size
        {
            get { return Point.Create(Width, Height); }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public bool Contains(float x, float y)
        {
            return x >= X && y >= Y && x <= X + Width && y <= Y + Height;
        }

        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        public bool Contains(Rectangle rectangle)
        {
            if (rectangle == null)
            {
                return false;
            }

            float rX = rectangle.X;
            float rY = rectangle.Y;
            float rWidth = rectangle.Width;
            float rHeight = rectangle.Height;

            return rX >= X && rX + rWidth <= X + Width &&
            rY >= Y && rY + rHeight <= Y + Height;
        }

        public void SetTo(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        /// <summary>
        /// Returns the intersecting rectangle
        /// </summary>
        public Rectangle Intersection(Rectangle rectangle)
        {
            if (rectangle == null)
            {
                return null;
            }

            float left = Math.Max(X, rectangle.X);
            float right = Math.Min(X + Width, rectangle.X + rectangle.Width);
            float top = Math.Max(Y, rectangle.Y);
            float bottom = Math.Min(Y + Height, rectangle.Y + rectangle.Height);

            if (left > right || top > bottom)
            {
                return Create();
            }
            return Create(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Returns a rectangle that encompasses both rectangles
        /// </summary>
        public Rectangle Union(Rectangle rectangle)
        {
            if (rectangle == null)
            {
                return null;
            }

            float left = Math.Max(X, rectangle.X);
            float right = Math.Min(X + Width, rectangle.X + rectangle.Width);
            float top = Math.Max(Y, rectangle.Y);
            float bottom = Math.Min(Y + Height, rectangle.Y + rectangle.Height);

            return Create(left, top, right - left, bottom - top);
        }

        public void Inflate(float dx, float dy)
        {
            X -= dx;
            Y -= dy;
            Width += 2.0f * dx;
            Height += 2.0f * dy;
        }

        public void Empty()
        {
            X = Y = Width = Height = 0.0f;
        }

        public void CopyFrom(Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        /// <summary>
        /// Inverts X or Y if they are negative
        /// </summary>
        public void Normalize()
        {
            if (Width < 0.0f)
            {
                Width = -Width;
                X -= Width;
            }

            if (Height < 0.0f)
            {
                Height = -Height;
                Y -= Height;
            }
        }

        public bool IsEmpty()
        {
            return Width == 0.0f || Height == 0.0f;
        }

        /// <summary>
        /// Determines whether this instance is equal the specified other with a small Epsilon error margin.
        /// </summary>
        public bool IsEqual(Rectangle other)
        {
            if (other == this)
            {
                return true;
            } 
            if (other == null)
            {
                return false;
            }

            return MathUtil.Equals(X, other.X) &&
            MathUtil.Equals(Y, other.Y) &&
            MathUtil.Equals(Width, other.Width) &&
            MathUtil.Equals(Height, other.Height);
        }

        /// <summary>
        /// Calculates the bounds of a rectangle after transforming it by a matrix.
        /// </summary>
        public Rectangle GetBounds(Matrix2D matrix)
        {
            Rectangle outRect = new Rectangle();

            float minX = float.MaxValue, maxX = -float.MaxValue;
            float minY = float.MaxValue, maxY = -float.MaxValue;
            Point[] positions = GetPositions();
            Point sPoint = Point.Create();

            for (int i = 0; i < 4; ++i)
            {
                sPoint = matrix.TransformPoint(positions[i]);

                if (minX > sPoint.X) minX = sPoint.X;
                if (maxX<sPoint.X) maxX = sPoint.X;
                if (minY > sPoint.Y) minY = sPoint.Y;
                if (maxY<sPoint.Y) maxY = sPoint.Y;
            }

            outRect.SetTo(minX, minY, maxX - minX, maxY - minY);
            return outRect;
        }

        /** Calculates the bounds of a rectangle projected into the XY-plane of a certain 3D space
         *  as they appear from the given camera position. Note that 'camPos' is expected in the
         *  target coordinate system (the same that the XY-plane lies in).
         **/
        public void SetBoundsProjected(Matrix3D matrix, float[] camPos)
        {
            if (camPos == null) throw new ArgumentNullException("camPos must not be null");

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            var positions = GetPositions();
            float[] sPoint3D;
            for (int i=0; i<4; ++i)
            {
                Point position = positions[i];

                if (matrix != null)
                    sPoint3D = matrix.TransformCoords3D(position.X, position.Y, 0);
                else
                    sPoint3D = new float[] { position.X, position.Y, 0};

                Point sPoint = MathUtil.IntersectLineWithXYPlane(camPos, sPoint3D);

                if (minX > sPoint.X) minX = sPoint.X;
                if (maxX < sPoint.X) maxX = sPoint.X;
                if (minY > sPoint.Y) minY = sPoint.Y;
                if (maxY < sPoint.Y) maxY = sPoint.Y;
            }

            X = minX;
            Y = minY;
            Width = maxX - minX;
            Height = maxY - minY;
        }

        /// <summary>
        /// Returns a vector containing the positions of the four edges of the given rectangle. 
        /// </summary>
        public Point[] GetPositions()
        {
            Point[] outP = new Point[4];

            for (int i = 0; i < 4; ++i)
            {
                outP[i] = Point.Create();
            }
            outP[0].X = Left; outP[0].Y = Top;
            outP[1].X = Right; outP[1].Y = Top;
            outP[2].X = Left;  outP[2].Y = Bottom;
            outP[3].X = Right; outP[3].Y = Bottom;
            return outP;
        }

        public Rectangle Clone()
        {
            return Create(X, Y, Width, Height);
        }

        /// <summary>
        /// Extends the bounds of the rectangle in all four directions.
        /// </summary>
        public void Extend(float left = 0f, float right = 0f,
                           float top = 0f, float bottom = 0f)
        {
            X -= left;
            Y -= top;
            Width += left + right;
            Height += top + bottom;
        }

        public override string ToString()
        {
            return "[x=" + X + " y=" + Y + " width=" + Width + " height=" + Height + "]";
        }

        // static functions 

        /** Compares all properties of the given rectangle, returning true only if
         *  they are equal (with the given accuracy 'e'). */
        public static bool Compare(Rectangle r1, Rectangle r2, float e = 0.0001f)
        {
            if (r1 == null) return r2 == null;
            else if (r2 == null) return false;
            else
            {
                return r1.X > r2.X - e && r1.X < r2.X + e &&
                       r1.Y > r2.Y - e && r1.Y < r2.Y + e &&
                       r1.Width> r2.Width  - e && r1.Width < r2.Width  + e &&
                       r1.Height> r2.Height - e && r1.Height<r2.Height + e;
            }
        }
    }
}