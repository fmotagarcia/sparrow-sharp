
using NUnit.Framework;
using Sparrow.Utils;
using Sparrow.Geom;
using System;
using Sparrow.Rendering;

namespace Tests
{
    [TestFixture]
    public class VertexDataTest : TestBase
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
            const int numVertices = 4;

            Vertex defaultVertex = DefaultVertex();

            VertexData vertexData = new VertexData(numVertices);

            Assert.AreEqual(numVertices, vertexData.NumVertices, "wrong number of vertices");
            Assert.True(vertexData.Vertices != null, "vertex array not accessible");

            for (int i = 0; i < numVertices; ++i)
            {
                CompareVertex(defaultVertex, vertexData.Vertices[i]);
            }
        }

        [Test]
        public void TestSetAllColors()
        {
            uint color = 0xabcdef;
            VertexData vertexData = new VertexData(4);
            vertexData.SetColor(color);

            for (int i = 0; i < vertexData.NumVertices; ++i)
            {
                Assert.AreEqual(color, vertexData.GetColor(i), "wrong color");
            }
        }

        [Test]
        public void TestSetAllAlphas()
        {
            VertexData vertexData = new VertexData(4);
            vertexData.SetAlpha(0.5f);

            for (int i = 0; i < vertexData.NumVertices; ++i)
            {
                AssertAreEqualWithSmallError(0.5f, vertexData.GetAlpha(i), "wrong alpha", 0.005f);
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
                Assert.AreEqual(color, vertexData.GetColor(i), "wrong color");
                AssertAreEqualWithSmallError(alpha, vertexData.GetAlpha(i), "wrong alpha", 0.005f);
            }
        }

        [Test]
        public void TestResize()
        {
            Vertex vertex = AnyVertex();
            Vertex defaultVertex = DefaultVertex();
            VertexData vertexData = new VertexData(4);

            Assert.AreEqual(4, vertexData.NumVertices, "wrong number of vertices");

            vertexData.Vertices[1] = vertex;
            vertexData.Vertices[2] = vertex;
            vertexData.Vertices[3] = vertex;

            vertexData.NumVertices = 2;

            Assert.AreEqual(2, vertexData.NumVertices, "wrong number of vertices");

            CompareVertex(defaultVertex, vertexData.Vertices[0]);
            CompareVertex(vertex, vertexData.Vertices[1]);

            vertexData.NumVertices = 4;

            CompareVertex(defaultVertex, vertexData.Vertices[0]);
            CompareVertex(vertex, vertexData.Vertices[1]);
        }

        [Test]
        public void TestPremultipliedAlpha()
        {
            VertexData vertexData = new VertexData(1);
          
            vertexData.SetColor(ColorUtil.GetRGB(80, 60, 40), 204/255.0f);

            vertexData.SetPremultipliedAlpha(true, true);

            CompareVertexColor(VertexColorHelper.CreateVertexColor(64, 48, 32, 204), vertexData.VertexColors[0]);

            vertexData.SetPremultipliedAlpha(false, true);

            CompareVertexColor(VertexColorHelper.CreateVertexColor(80, 60, 40, 204), vertexData.VertexColors[0]);
        }

        [Test]
        public void TestScaleAlphaWithoutPMA()
        {
            VertexData vertexData = new VertexData(1, false);

            vertexData.SetColor(ColorUtil.GetRGB(80, 60, 40), 128/255.0f);

            vertexData.ScaleAlphas(0.5f);

            CompareVertexColor(VertexColorHelper.CreateVertexColor(80, 60, 40, 64), vertexData.VertexColors[0]);
        }
            
        [Test]
        public void TestScaleAlphaWithPMA()
        {
            VertexData vertexData = new VertexData(1, true);

            vertexData.SetColor(ColorUtil.GetRGB(80, 60, 40), 204/255.0f);

            vertexData.ScaleAlphas(0.8f);

            CompareVertexColor(VertexColorHelper.CreateVertexColor(
                (byte)(80 * 0.64f + 0.5f), 
                (byte)(60 * 0.64f + 0.5f), 
                (byte)(40 * 0.64f + 0.5f), 
                (byte)(204 * 0.8f + 0.5f)), vertexData.VertexColors[0]);
        }

        [Test]
        public void TestTransformVertices()
        {
            VertexData vertexData = new VertexData(3, true);

            Vertex defaultVertex = DefaultVertex();
            Vertex secondVertex = DefaultVertex();
            secondVertex.Position.X = 1.0f;
            secondVertex.Position.Y = 2.0f;

            vertexData.Vertices[0] = defaultVertex;
            vertexData.Vertices[1] = secondVertex;
            vertexData.Vertices[2] = defaultVertex;

            Matrix2D matrix = Matrix2D.Create();
            matrix.Rotate((float)Math.PI);
           
            vertexData.TransformVertices(matrix, 1, 1);

            Vertex expected = DefaultVertex();
            expected.Position.X = -1.0f;
            expected.Position.Y = -2.0f;

            CompareVertex(vertexData.Vertices[0], DefaultVertex());
            CompareVertex(vertexData.Vertices[1], expected);
            CompareVertex(vertexData.Vertices[2], DefaultVertex());
        }

        [Test]
        public void TestCopy()
        {
            VertexData sourceData = new VertexData(3);

            Vertex defaultVertex = DefaultVertex();
            Vertex vertex = AnyVertex();

            sourceData.Vertices[0] = vertex;
            sourceData.Vertices[1] = defaultVertex;
            sourceData.Vertices[2] = vertex;

            VertexData targetData = new VertexData(5, false);

            sourceData.CopyTo(targetData, 0, 2, 3);

            CompareVertex(defaultVertex, targetData.Vertices[0]);
            CompareVertex(defaultVertex, targetData.Vertices[1]);
            CompareVertex(vertex, targetData.Vertices[2]);
            CompareVertex(defaultVertex, targetData.Vertices[3]);
            CompareVertex(vertex, targetData.Vertices[4]);
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

