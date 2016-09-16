using System;
using Sparrow.Utils;
using OpenTK;
using NUnit.Framework;
using Sparrow.Geom;
using System.Reflection;

namespace Tests
{
    public class TestBase
    {
        public T CreateInstance<T>(params object[] args)
        {
            var type = typeof(T);
            var instance = type.Assembly.CreateInstance(
                type.FullName, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, args, null, null);
            return (T)instance;
        }

        public void CompareVertex (Vertex v1, Vertex v2)
        {
            CompareVector(v1.Position, v2.Position);;
            CompareVector(v1.TexCoords, v2.TexCoords);
        }

        public void CompareVector(Vector2 v1, Vector2 v2)
        {
            AssertAreEqualWithSmallError(v1.X, v2.X, "wrong vector.x");
            AssertAreEqualWithSmallError(v1.Y, v2.Y, "wrong vector.y");
        }

        public void AssertAreEqualWithSmallError(float a, float b, string message = "", float epsilon = 0.0001f) {
            if (Math.Abs(a - b) > epsilon)
            {
                Assert.Fail(message + " Difference " + Math.Abs(a - b) + " is greater than " + epsilon);
            }
        }

        public void CompareVertexColor (VertexColor expected, VertexColor v2)
        {
            if (expected.R != v2.R || expected.B != v2.B || expected.G != v2.G || expected.A != v2.A)
            {
                Assert.Fail("Excepted: " + expected.R + " " + expected.G + " " + expected.B + " " + expected.A + " " +
                    " actual: " + v2.R + " " + v2.G + " " + v2.B + " " + v2.A);
            }
        }

        public void CompareRectangles(Rectangle rect1, Rectangle rect2, float e = 0.0001f)
        {
            AssertAreEqualWithSmallError(rect1.X, rect2.X, "Rect X coordinate wrong", e);
            AssertAreEqualWithSmallError(rect1.Y, rect2.Y, "Rect Y coordinate wrong", e);
            AssertAreEqualWithSmallError(rect1.Width, rect2.Width, "Rect width wrong", e);
            AssertAreEqualWithSmallError(rect1.Height, rect2.Height, "Rect height wrong", e);
        }

        public void ComparePoints(Point p1, Point p2, float e = 0.0001f)
        {
            AssertAreEqualWithSmallError(p1.X, p2.X, "Point X coordinate wrong", e);
            AssertAreEqualWithSmallError(p1.Y, p2.Y, "Point X coordinate wrong", e);
        }
}
}

