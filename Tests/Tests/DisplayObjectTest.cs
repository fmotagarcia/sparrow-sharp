using NUnit.Framework;
using Sparrow.Display;
using Sparrow;
using System;
using Sparrow.Geom;
using Sparrow.Utils;

namespace Tests
{
    // note: Assert.Equals is a method of object, its not for unit tests
    [TestFixture]
    public class DisplayObjectTest : TestBase
    {

        [SetUp]
        protected void SetUp()
        {
            SparrowSharpApp.Start(12, 12, typeof(Sprite));
        }

        [TearDown]
        protected void TearDown()
        {
            SparrowSharpApp.Stage.RemoveAllChildren();
            SparrowSharpApp.Destroy();
        }

        [Test]
        public void TestRoot()
        {
            Sprite root = new Sprite();
            Sprite child = new Sprite();
            Sprite grandChild = new Sprite();
            root.AddChild(child);
            child.AddChild(grandChild);

            Assert.AreEqual(root, grandChild.Root, "Wrong root " + grandChild.Root);

            SparrowSharpApp.Stage.AddChild(root);

            Assert.AreEqual(SparrowSharpApp.Stage, grandChild.Root, "Wrong root " + grandChild.Root);
        }

        [Test]
        public void TestTransformationMatrixToSpace()
        {
            Sprite sprite = new Sprite();
            Sprite child = new Sprite();

            child.X = 30;
            child.Y = 20;
            child.ScaleX = 1.2f;
            child.ScaleY = 1.5f;
            child.Rotation = (float)(Math.PI / 4.0f);    
            sprite.AddChild(child);

            Matrix matrix = sprite.TransformationMatrixToSpace(child);
            Matrix expectedMatrix = child.TransformationMatrix;
            expectedMatrix.Invert();

            Assert.IsTrue(matrix.IsEqual(expectedMatrix));

            matrix = child.TransformationMatrixToSpace(sprite);

            Assert.IsTrue(child.TransformationMatrix.IsEqual(matrix));

            // more is tested indirectly via 'testBoundsInSpace' in DisplayObjectContainerTest
        }

        [Test]
        public void TestTransformationMatrix()
        {
            Sprite sprite = new Sprite();
            sprite.X = 50;
            sprite.Y = 100;
            sprite.Rotation = (float)(Math.PI / 4);
            sprite.ScaleX = 0.5f;
            sprite.ScaleY = 1.5f;

            Matrix matrix = Matrix.Create();
            matrix.Scale(sprite.ScaleX, sprite.ScaleY);
            matrix.Rotate(sprite.Rotation);
            matrix.Translate(sprite.X, sprite.Y);

            Assert.IsTrue(sprite.TransformationMatrix.IsEqual(matrix));
        }

        [Test]
        public void TestSetTransformationMatrix()
        {
            const float x = 50;
            const float y = 100;
            const float scaleX = 0.5f;
            const float scaleY = 1.5f;
            const float rotation = (float)(Math.PI / 4.0f);

            Matrix matrix = Matrix.Create();
            matrix.Scale(scaleX, scaleY);
            matrix.Rotate(rotation);
            matrix.Translate(x, y);

            Sprite sprite = new Sprite();
            sprite.TransformationMatrix = matrix;

            AssertAreEqualWithSmallError(x, sprite.X);
            AssertAreEqualWithSmallError(y, sprite.Y);
            AssertAreEqualWithSmallError(scaleX, sprite.ScaleX);
            AssertAreEqualWithSmallError(scaleY, sprite.ScaleY);
            AssertAreEqualWithSmallError(rotation, sprite.Rotation);
        }

        [Test]
        public void TestSetTransformationMatrixWithRightAngle()
        {
            Sprite sprite = new Sprite();
            float[] angles = { (float)(Math.PI / 2.0f), (float)(-Math.PI / 2.0f) };
            Matrix[] matrices = {
                Matrix.Create(0, 1,-1,0,0,0), 
                Matrix.Create(0,-1, 1,0,0,0)
            };

            for (int i=0; i<2; ++i)
            {
                float angle = angles[i];
                Matrix matrix = matrices[i];
                sprite.TransformationMatrix = matrix;

                AssertAreEqualWithSmallError(0.0f, sprite.X, "wrong x coord");
                AssertAreEqualWithSmallError(0.0f, sprite.Y, "wrong y coord");
                AssertAreEqualWithSmallError(1.0f, sprite.ScaleX, "wrong scaleX");
                AssertAreEqualWithSmallError(1.0f, sprite.ScaleY, "wrong scaleY");
                AssertAreEqualWithSmallError(angle, sprite.Rotation, "wrong rotation");
            }
        }

        [Test]
        public void TestSetTransformationMatrixWithZeroValues()
        {
            Matrix matrix = Matrix.Create(0,0,0,0,0,0);
            Sprite sprite = new Sprite();
            sprite.TransformationMatrix = matrix;

            Assert.AreEqual(0.0f, sprite.X, "wrong x");
            Assert.AreEqual(0.0f, sprite.Y, "wrong y");
            Assert.AreEqual(0.0f, sprite.ScaleX, "wrong scaleX");
            Assert.AreEqual(0.0f, sprite.ScaleY, "wrong scaleY");
            Assert.AreEqual(0.0f, sprite.Rotation, "wrong rotation");
            Assert.AreEqual(0.0f, sprite.SkewX, "wrong skewX");
            Assert.AreEqual(0.0f, sprite.SkewY, "wrong skewY");
        }

        [Test]
        public void TestBounds()
        {
            Quad quad = new Quad(10, 20);
            quad.X = -10;
            quad.Y = 10;
            quad.Rotation = NumberUtil.PIHALF;
            Rectangle bounds = quad.Bounds;

            Assert.IsTrue(NumberUtil.Equals(-30, bounds.X), "wrong bounds.x: " + bounds.X);
            Assert.IsTrue(NumberUtil.Equals(10, bounds.Y), "wrong bounds.y: " + bounds.Y);
            Assert.IsTrue(NumberUtil.Equals(20, bounds.Width), "wrong bounds.width: " + bounds.Width);
            Assert.IsTrue(NumberUtil.Equals(10, bounds.Height), "wrong bounds.height: " + bounds.Height);

            bounds = quad.BoundsInSpace(quad);
            Assert.IsTrue(NumberUtil.Equals(0, bounds.X), "wrong inner bounds.x: " + bounds.X);
            Assert.IsTrue(NumberUtil.Equals(0, bounds.Y), "wrong inner bounds.y: " + bounds.Y);
            Assert.IsTrue(NumberUtil.Equals(10, bounds.Width), "wrong inner bounds.width: " + bounds.Width);
            Assert.IsTrue(NumberUtil.Equals(20, bounds.Height), "wrong innter bounds.height: " + bounds.Height);
        }
    }
}

