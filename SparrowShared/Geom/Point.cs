using System;
using Sparrow.Utils;
using Sparrow.Pool;

namespace Sparrow.Geom
{
    /// <summary>
    /// The Point class describes a two dimensional point or vector. */
    /// </summary>
    public class Point : PooledObject
    {
        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        public static Point Create(float x = 0.0f, float y = 0.0f)
        {
            Point point = (Point)_pool.GetObject();
            point.X = x;
            point.Y = y;
            return point;
        }

        private static Point Init()
        {
            return new Point();
        }

        public float X;
        public float Y;

        private Point() {}

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
            set
            {
                X = X * value;
                Y = Y * value;
            }
        }

        public float Angle
        {
            get
            {
                return (float)Math.Atan2(Y, X);
            }
        }

        public bool IsOrigin
        {
            get
            {
                return X == 0.0f && Y == 0.0f;
            }
        }

        public void Invert()
        {
            X = -X;
            Y = -Y;
        }

        public void AddPoint(Point point)
        {
            X = X + point.X;
            Y = Y + point.Y;
        }

        public void SubtractPoint(Point point)
        {
            X = X - point.X;
            Y = Y - point.Y;
        }

        /// <summary>
        /// Rotates by the specified angle in Radians
        /// </summary>
        public void RotateBy(float angle)
        {
            float sin = MathUtil.FastSin(angle);
            float cos = MathUtil.FastCos(angle);
            X = X * cos - Y * sin;
            Y = X * sin + Y * cos;
        }

        public void Normalize()
        {
            if (IsOrigin)
            {
                return;
            }
            float inverseLength = 1 / Length;
            X = X * inverseLength;
            Y = Y * inverseLength;
        }

        public float Dot(Point other)
        {
            return X * other.X + Y * other.Y;
        }

        public void CopyFromPoint(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public void SetTo(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Determines whether the specified Point's X and Y values is equal to the current Point with
        /// with a small epsilon error margin.
        /// </summary>
        public bool Equals(Point other)
        {
            if (other == this)
            {
                return true;
            } 

            if (other == null)
            {
                return false;
            }

            return MathUtil.Equals(X, other.X) && MathUtil.Equals(Y, other.Y);
        }

        public float Distance(Point p2)
        {
            return (float)Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y));
        }

        override public string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}