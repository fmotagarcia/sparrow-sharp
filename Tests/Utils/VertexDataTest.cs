using System;
using NUnit.Framework;
using Sparrow.Utils;
using OpenTK;

namespace Tests
{
	[TestFixture]
	public class TestsSample : TestBase
	{
		// note: Assert.Equals is a method of object, its not for unit tests
		[Test]
		public void TestEmpty ()
		{
			VertexData vertexData = new VertexData();

			Assert.AreEqual(0, vertexData.NumVertices, "wrong number of vertices");
			Assert.True(vertexData.Vertices == null, "vertex array should be null");
		}

		[Test]
		public void TestBasicMethods ()
		{
			int numVertices = 4;

			Vertex vertex = AnyVertex();
			Vertex defaultVertex = DefaultVertex();

			VertexData vertexData = new VertexData(numVertices);

			Assert.AreEqual(numVertices, vertexData.NumVertices, "wrong number of vertices");
			Assert.True(vertexData.Vertices != null, "vertex array not accessible");

            for (int i = 0; i < numVertices; ++i)
            {
                CompareVertex(defaultVertex, vertexData.Vertices[i]);
            }
				
            /*
			vertexData.SetVertex(vertex, 1);

			CompareVertex(defaultVertex, vertexData.VertexAtIndex(0));
			CompareVertex(vertex,        vertexData.VertexAtIndex(1));
			CompareVertex(defaultVertex, vertexData.VertexAtIndex(2));
			CompareVertex(defaultVertex, vertexData.VertexAtIndex(3));*/
		}

		[Test]
		public void TestSetAllColors()
		{
			uint color = 0xabcdef;
			VertexData vertexData = new VertexData(4);
			vertexData.SetColor(color);

			for (int i=0; i<vertexData.NumVertices; ++i)
				Assert.AreEqual(color, vertexData.ColorAt(i), "wrong color");
		}

		[Test]
		public void TestSetAllAlphas()
		{
			VertexData vertexData = new VertexData(4);
			vertexData.SetAlpha(0.5f);

            for (int i = 0; i < vertexData.NumVertices; ++i)
            {
                AssertEqualsWithSmallError(0.5f, vertexData.AlphaAt(i), "wrong alpha", 0.005f);
            }
		}

		[Test]
		public void TestSetAllColorsAndAlphas()
		{
			uint color = 0xabcdef;
			float alpha = 0.5f;
			VertexData vertexData = new VertexData(4);
			vertexData.SetColor(color);
			vertexData.SetAlpha(alpha);

			for (int i=0; i<vertexData.NumVertices; ++i)
			{
				Assert.AreEqual(color, vertexData.ColorAt(i), "wrong color");
				AssertEqualsWithSmallError(alpha, vertexData.AlphaAt(i), "wrong alpha", 0.005f);
			}
		}

		[Test]
		public void TestResize()
		{
			Vertex vertex = AnyVertex();
			Vertex defaultVertex = DefaultVertex();
			VertexData vertexData = new VertexData(4);

			Assert.AreEqual(4, vertexData.NumVertices, "wrong number of vertices");
            /*
			vertexData.Vertices[1] = vertex;
			vertexData.Vertices[2] = vertex;
			vertexData.Vertices[3] = vertex;

			vertexData.NumVertices = 2;

			Assert.AreEqual(2, vertexData.NumVertices, "wrong number of vertices");
			CompareVertex(defaultVertex, vertexData.VertexAtIndex(0));
			CompareVertex(vertex, vertexData.VertexAtIndex(1));

			vertexData.NumVertices = 4;

			CompareVertex(defaultVertex, vertexData.VertexAtIndex(2));
			CompareVertex(defaultVertex, vertexData.VertexAtIndex(3));*/
		}

		private Vertex AnyVertex() {
			Vertex vertex = new Vertex();
			vertex.Position = new Vector2 (1.0f, 2.0f);
			vertex.TexCoords = new Vector2(3.0f, 4.0f);
			return vertex;
		}

		private Vertex DefaultVertex() {
			Vertex vertex = new Vertex();
			vertex.Position = new Vector2 (0.0f, 0.0f);
			vertex.TexCoords = new Vector2(0.0f, 0.0f);
			return vertex;
		}
			

	}
}

