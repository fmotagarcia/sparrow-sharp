using System;
using Sparrow.Utils;
using OpenTK;
using OpenTK.Graphics.ES20;
using NUnit.Framework;

namespace Tests
{
	public class TestBase
	{
		public void CompareVertex (Vertex v1, Vertex v2)
		{
			CompareVector(v1.Position, v2.Position);
			CompareVector(v1.TexCoords, v2.TexCoords);
		}

		public void CompareVector(Vector2 v1, Vector2 v2)
		{
			AssertEqualsWithSmallError(v1.X, v2.X, "wrong vector.x");
			AssertEqualsWithSmallError(v1.Y, v2.Y, "wrong vector.y");
		}

		public void AssertEqualsWithSmallError(float a, float b, string message = "", float epsilon = 0.0001f) {
            if (Math.Abs(a - b) > epsilon)
            {
                Assert.Fail(message + " Difference " + Math.Abs(a - b) + " is greater than " + epsilon);
            }
		}
	}
}

