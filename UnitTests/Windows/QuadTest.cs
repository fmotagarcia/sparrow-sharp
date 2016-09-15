using NUnit.Framework;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;

namespace Tests
{
    // note: Assert.Equals is a method of object, its not for unit tests
    [TestFixture]
    public class QuadTest : TestBase
    {

        private static readonly float E = 0.0001f;

        [Test]
        public void TestQuad()
        {
            Quad quad = new Quad(100, 200, 0x123456);
            Assert.AreEqual(0x123456, quad.Color);
        }

        [Test]
        public void TestColors()
        {
            Quad quad = new Quad(100, 100);
            quad.SetVertexColor(0, 0x1);
            quad.SetVertexColor(1, 0x2);
            quad.SetVertexColor(2, 0x3);
            quad.SetVertexColor(3, 0x4);


            Assert.AreEqual(0x1, quad.GetVertexColor(0));
            Assert.AreEqual(0x2, quad.GetVertexColor(1));
            Assert.AreEqual(0x3, quad.GetVertexColor(2));
            Assert.AreEqual(0x4, quad.GetVertexColor(3));
        }

        [Test]
        public void TestBounds()
        {
            Quad quad = new Quad(100, 200);
            CompareRectangles(Rectangle.Create(0, 0, 100, 200), quad.Bounds);
            
            quad.PivotX = 50f;
            CompareRectangles(Rectangle.Create(-50, 0, 100, 200), quad.Bounds);
            
            quad.PivotY = 60f;
            CompareRectangles(Rectangle.Create(-50, -60, 100, 200), quad.Bounds);
            
            quad.ScaleX = 2f;
            CompareRectangles(Rectangle.Create(-100, -60, 200, 200), quad.Bounds);
            
            quad.ScaleY = 0.5f;
            CompareRectangles(Rectangle.Create(-100, -30, 200, 100), quad.Bounds);
            
            quad.X = 10;
            CompareRectangles(Rectangle.Create(-90, -30, 200, 100), quad.Bounds);
            
            quad.Y = 20;
            CompareRectangles(Rectangle.Create(-90, -10, 200, 100), quad.Bounds);

            Sprite parent = new Sprite();
            parent.AddChild(quad);
            
            CompareRectangles(parent.Bounds, quad.Bounds);
        }

        [Test]
        public void TestWidthAndHeight()
        {
            Quad quad = new Quad(100, 50);
            Assert.AreEqual(100, quad.Width);
            Assert.AreEqual(50, quad.Height);

            quad.ScaleX = -1;
            Assert.AreEqual(100, quad.Width);

            quad.PivotX = 100;
            Assert.AreEqual(100, quad.Width);

            quad.PivotX = -10;
            Assert.AreEqual(100, quad.Width);

            quad.ScaleY = -1;
            Assert.AreEqual(50, quad.Height);

            quad.PivotY = 20;
            Assert.AreEqual(50, quad.Height);
        }

        [Test]
        public void TestHitTest()
        {
            Quad quad = new Quad(100, 50);
            Assert.AreEqual(quad, quad.HitTest(Point.Create(0.1f, 0.1f)));
            Assert.AreEqual(quad, quad.HitTest(Point.Create(99.9f, 49.9f)));
            Assert.IsNull(quad.HitTest(Point.Create(-0.1f, -0.1f)));
            Assert.IsNull(quad.HitTest(Point.Create(100.1f, 25f)));
            Assert.IsNull(quad.HitTest(Point.Create(50f, 50.1f)));
            Assert.IsNull(quad.HitTest(Point.Create(100.1f, 50.1f)));
        }

        [Test]
        public void TestReadjustSize()
        {
            Texture texture = new MockTexture(100, 50);
            Quad quad = new Quad(10, 20);
            quad.Texture = texture;

            AssertAreEqualWithSmallError(quad.Width, 10, "", E);
            AssertAreEqualWithSmallError(quad.Height, 20, "", E);
            Assert.AreEqual(texture, quad.Texture);

            quad.ReadjustSize();

            AssertAreEqualWithSmallError(quad.Width, texture.FrameWidth, "", E);
            AssertAreEqualWithSmallError(quad.Height, texture.FrameHeight, "", E);
            
            float newWidth  = 64;
            float newHeight = 32;

            quad.ReadjustSize(newWidth, newHeight);

            AssertAreEqualWithSmallError(quad.Width, newWidth, "", E);
            AssertAreEqualWithSmallError(quad.Height, newHeight, "", E);

            quad.Texture = null;
            quad.ReadjustSize(); // shouldn't change anything

            AssertAreEqualWithSmallError(quad.Width, newWidth, "", E);
            AssertAreEqualWithSmallError(quad.Height, newHeight, "", E);
        }


}
}
