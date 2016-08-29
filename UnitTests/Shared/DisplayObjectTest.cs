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

        [Test]
        public void TestRoot()
        {
            SparrowSharpApp.Start(12, 12, typeof(Sprite));

            Sprite root = new Sprite();
            Sprite child = new Sprite();
            Sprite grandChild = new Sprite();
            root.AddChild(child);
            child.AddChild(grandChild);

            Assert.AreEqual(root, grandChild.Root, "Wrong root " + grandChild.Root);

            SparrowSharpApp.Stage.AddChild(root);

            Assert.AreEqual(SparrowSharpApp.Stage, grandChild.Root, "Wrong root " + grandChild.Root);

            SparrowSharpApp.Stage.RemoveAllChildren();
            SparrowSharpApp.Destroy();
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

            Matrix matrix = sprite.GetTransformationMatrix(child);
            Matrix expectedMatrix = child.TransformationMatrix;
            expectedMatrix.Invert();

            Assert.IsTrue(matrix.IsEqual(expectedMatrix));

            matrix = child.GetTransformationMatrix(sprite);

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
            quad.Rotation = MathUtil.PIHALF;
            Rectangle bounds = quad.Bounds;

            Assert.IsTrue(MathUtil.Equals(-30, bounds.X), "wrong bounds.x: " + bounds.X);
            Assert.IsTrue(MathUtil.Equals(10, bounds.Y), "wrong bounds.y: " + bounds.Y);
            Assert.IsTrue(MathUtil.Equals(20, bounds.Width), "wrong bounds.width: " + bounds.Width);
            Assert.IsTrue(MathUtil.Equals(10, bounds.Height), "wrong bounds.height: " + bounds.Height);

            bounds = quad.GetBounds(quad);
            Assert.IsTrue(MathUtil.Equals(0, bounds.X), "wrong inner bounds.x: " + bounds.X);
            Assert.IsTrue(MathUtil.Equals(0, bounds.Y), "wrong inner bounds.y: " + bounds.Y);
            Assert.IsTrue(MathUtil.Equals(10, bounds.Width), "wrong inner bounds.width: " + bounds.Width);
            Assert.IsTrue(MathUtil.Equals(20, bounds.Height), "wrong innter bounds.height: " + bounds.Height);
        }

        [Test]
        public void TestZeroSize()
        {
            Sprite sprite = new Sprite();

            AssertAreEqualWithSmallError(1.0f, sprite.ScaleX);
            AssertAreEqualWithSmallError(1.0f, sprite.ScaleY);

            // sprite is empty, scaling should thus have no effect!
            sprite.Width = 100;
            sprite.Height = 200;
            AssertAreEqualWithSmallError(1.0f, sprite.ScaleX, "wrong scaleX value");
            AssertAreEqualWithSmallError(1.0f, sprite.ScaleY, "wrong scaleY value");
            AssertAreEqualWithSmallError(0.0f, sprite.Width, "wrong width");
            AssertAreEqualWithSmallError(0.0f, sprite.Height, "wrong height");

            // setting a value to zero should be no problem -- and the original size should be remembered.
            Quad quad = new Quad(100, 200);
            quad.ScaleX = 0.0f;
            quad.ScaleY = 0.0f;
            AssertAreEqualWithSmallError(0.0f, quad.Width, "wrong width");
            AssertAreEqualWithSmallError(0.0f, quad.Height, "wrong height");

            quad.ScaleX = 1.0f;
            quad.ScaleY = 1.0f;
            AssertAreEqualWithSmallError(100.0f, quad.Width, "wrong width");
            AssertAreEqualWithSmallError(200.0f, quad.Height, "wrong height");
            AssertAreEqualWithSmallError(1.0f, quad.ScaleX, "wrong scaleX value");
            AssertAreEqualWithSmallError(1.0f, quad.ScaleY, "wrong scaleY value");
        }

        [Test]
        public void TestLocalToGlobal()
        {
            Sprite root = new Sprite();
            Sprite sprite = new Sprite();
            sprite.X = 10;
            sprite.Y = 20;
            root.AddChild(sprite);
            Sprite sprite2 = new Sprite();
            sprite2.X = 150;
            sprite2.Y = 200;    
            sprite.AddChild(sprite2);

            Point localPoint = Point.Create(0, 0);
            Point globalPoint = sprite2.LocalToGlobal(localPoint);
            Point expectedPoint = Point.Create(160, 220);
            Assert.IsTrue(globalPoint.Equals(expectedPoint));
            // the position of the root object should be irrelevant -- we want the coordinates
            // *within* the root coordinate system!
            root.X = 50;
            globalPoint = sprite2.LocalToGlobal(localPoint);
            Assert.IsTrue(globalPoint.Equals(expectedPoint));
        }

        [Test]
        public void TestLocalToGlobalWithPivot()
        {
            Sprite sprite = new Sprite();
            Quad quad = new Quad(40, 30);
            quad.X = 10;
            quad.Y = 20;
            quad.PivotX = quad.Width;
            quad.PivotY = quad.Height;
            sprite.AddChild(quad);

            Point point = Point.Create(0, 0);
            Point globalPoint = quad.LocalToGlobal(point);
            AssertAreEqualWithSmallError(-30.0f, globalPoint.X, "wrong global point with pivot");
            AssertAreEqualWithSmallError(-10.0f, globalPoint.Y, "wrong global point with pivot");
        }
    }
}

