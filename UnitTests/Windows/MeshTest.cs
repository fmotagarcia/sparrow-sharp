using NUnit.Framework;
using Sparrow.Geom;
using Sparrow.Utils;
using Sparrow.Rendering;
using System;

namespace Tests
{
    [TestFixture]
    class MeshTest : TestBase
    {

        [Test]
        public void TestGetBounds()
        {
            VertexData vertexData = new VertexData();
            vertexData.SetPoint(0, 10, 10);
            vertexData.SetPoint(1, 20, 10);
            vertexData.SetPoint(2, 10, 20);

            IndexData indexData = new IndexData();
            indexData.AddTriangle(0, 1, 2);

            Mesh mesh = new Mesh(vertexData, indexData);
            Rectangle expected = Rectangle.Create(10, 10, 10, 10);
            CompareRectangles(expected, mesh.Bounds);
            CompareRectangles(expected, mesh.GetBounds(mesh));

            mesh.Rotation = (float)Math.PI / 2.0f;
            expected.SetTo(-20, 10, 10, 10);
            CompareRectangles(expected, mesh.Bounds);
        }

        [Test]
        public void TestColor()
        {
            VertexData vertexData = new VertexData();
            vertexData.NumVertices = 3;

            IndexData indexData = new IndexData();
            indexData.AddTriangle(0, 1, 2);

            Mesh mesh = new Mesh(vertexData, indexData);
            mesh.SetVertexColor(0, 0xff0000);
            mesh.SetVertexColor(1, 0x00ff00);
            mesh.SetVertexColor(2, 0x0000ff);

            Assert.AreEqual(0xff0000, mesh.GetVertexColor(0));
            Assert.AreEqual(0x00ff00, mesh.GetVertexColor(1));
            Assert.AreEqual(0x0000ff, mesh.GetVertexColor(2));

            mesh.Color = 0xf0f0f0;

            for (int i = 0; i < 3; ++i)
                Assert.AreEqual(0xf0f0f0, mesh.GetVertexColor(i));
        }

        [Test]
        public void TestAlpha()
        {
            VertexData vertexData = new VertexData();
            vertexData.NumVertices = 3;

            IndexData indexData = new IndexData();
            indexData.AddTriangle(0, 1, 2);

            Mesh mesh = new Mesh(vertexData, indexData);
            mesh.SetVertexAlpha(0, 0.2f);
            mesh.SetVertexAlpha(1, 0.5f);
            mesh.SetVertexAlpha(2, 0.8f);

            float E = 0.02f;
            AssertAreEqualWithSmallError(mesh.GetVertexAlpha(0), 0.2f, "", E);
            AssertAreEqualWithSmallError(mesh.GetVertexAlpha(1), 0.5f, "", E);
            AssertAreEqualWithSmallError(mesh.GetVertexAlpha(2), 0.8f, "", E);
        }

        [Test]
        public void TestTexCoords()
        {
            /* TODO
            Texture rootTexture = new MockTexture(100, 100);
            Texture subTexture = Texture.FromTexture(rootTexture, Rectangle.Create(50, 50, 50, 50));

            VertexData vertexData = new VertexData();
            vertexData.SetPoint(0, 0, 0);
            vertexData.SetPoint(1, 1, 0);
            vertexData.SetPoint(2, 0, 1);
            vertexData.SetPoint(3, 1, 1);
            vertexData.SetTexCoords(0, 0, 0);
            vertexData.SetTexCoords(1, 1, 0);
            vertexData.SetTexCoords(2, 0, 1);
            vertexData.SetTexCoords(3, 1, 1);

            IndexData indexData = new IndexData();
            indexData.AddQuad(0, 1, 2, 3);

            Mesh mesh = new Mesh(vertexData, indexData);

            ComparePoints(Point.Create(0, 0), mesh.GetTexCoords(0));
            ComparePoints(Point.Create(1, 1), mesh.GetTexCoords(3));

            mesh.Texture = subTexture; // should change internal texture coordinates

            ComparePoints(Point.Create(0, 0), mesh.GetTexCoords(0));
            ComparePoints(Point.Create(1, 1), mesh.GetTexCoords(3));

            ComparePoints(Point.Create(0.5f, 0.5f), vertexData.GetPoint(0, "texCoords"));
            ComparePoints(Point.Create(1.0f, 1.0f), vertexData.GetPoint(3, "texCoords"));

            mesh.SetTexCoords(2, 0.25f, 0.75f);

            ComparePoints(Point.Create(0.25f,  0.75f),  mesh.GetTexCoords(2));
            ComparePoints(Point.Create(0.625f, 0.875f), vertexData.GetPoint(2, "texCoords"));

            mesh.Texture = rootTexture;

            ComparePoints(Point.Create(0f, 0f), mesh.GetTexCoords(0));
            ComparePoints(Point.Create(1f, 1f), mesh.GetTexCoords(3));

            ComparePoints(Point.Create(0f, 0f), vertexData.GetPoint(0, "texCoords"));
            ComparePoints(Point.Create(1f, 1f), vertexData.GetPoint(3, "texCoords"));
            ComparePoints(Point.Create(0.25f,  0.75f),  vertexData.GetPoint(2, "texCoords"));
            */
        }

        [Test]
        public void TestVertexPosition()
        {
            VertexData vertexData = new VertexData();
            vertexData.NumVertices = 3;

            IndexData indexData = new IndexData();
            indexData.AddTriangle(0, 1, 2);

            Mesh mesh = new Mesh(vertexData, indexData);
            mesh.SetVertexPosition(1, 1, 0);
            mesh.SetVertexPosition(2, 1, 1);

            ComparePoints(mesh.GetVertexPosition(0), Point.Create());
            ComparePoints(mesh.GetVertexPosition(1), Point.Create(1, 0));
            ComparePoints(mesh.GetVertexPosition(2), Point.Create(1, 1));
        }

        [Test]
        public void TestHitTest()
        {
            // +  0
            //   /|
            //  / |
            // 1--2--3
            //    | /
            //    |/
            //    4

            VertexData vertexData = new VertexData();
            vertexData.SetPoint(0, 1, 0);
            vertexData.SetPoint(1, 0, 1);
            vertexData.SetPoint(2, 1, 1);
            vertexData.SetPoint(3, 2, 1);
            vertexData.SetPoint(4, 1, 2);

            IndexData indexData = new IndexData();
            indexData.AddTriangle(0, 2, 1);
            indexData.AddTriangle(2, 3, 4);

            Mesh mesh = new Mesh(vertexData, indexData);
            Assert.IsNull(mesh.HitTest(Point.Create(0.49f, 0.49f)));
            Assert.IsNull(mesh.HitTest(Point.Create(1.01f, 0.99f)));
            Assert.IsNull(mesh.HitTest(Point.Create(0.99f, 1.01f)));
            Assert.IsNull(mesh.HitTest(Point.Create(1.51f, 1.51f)));
            Assert.AreEqual(mesh, mesh.HitTest(Point.Create(0.51f, 0.51f)));
            Assert.AreEqual(mesh, mesh.HitTest(Point.Create(0.99f, 0.99f)));
            Assert.AreEqual(mesh, mesh.HitTest(Point.Create(1.01f, 1.01f)));
            Assert.AreEqual(mesh, mesh.HitTest(Point.Create(1.49f, 1.49f)));

            mesh.Visible = false;
            Assert.IsNull(mesh.HitTest(Point.Create(0.75f, 0.75f)));

            mesh.Visible = true;
            mesh.Touchable = false;
            Assert.IsNull(mesh.HitTest(Point.Create(0.75f, 0.75f)));
        }

}
}
