using System;
using Sparrow.Utils;
using OpenTK;
using NUnit.Framework;

namespace Tests
{
	public class TestBase
	{
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
	}
}

