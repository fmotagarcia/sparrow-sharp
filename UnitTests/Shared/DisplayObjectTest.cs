using NUnit.Framework;
using Sparrow.Display;
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
        public void TestBase()
        {
            Sprite object1 = new Sprite();
            Sprite object2 = new Sprite();
            Sprite object3 = new Sprite();

            object1.AddChild(object2);
            object2.AddChild(object3);

            Assert.AreEqual(object1, object1.Base);
            Assert.AreEqual(object1, object2.Base);
            Assert.AreEqual(object1, object3.Base);

            Quad quad = new Quad(100, 100);
            Assert.AreEqual(quad, quad.Base);
        }

        [Test]
        public void TestRootAndStage()
        {
            Sprite object1 = new Sprite();
            Sprite object2 = new Sprite();
            Sprite object3 = new Sprite();

            object1.AddChild(object2);
            object2.AddChild(object3);

            Assert.AreEqual(null, object1.Root);
            Assert.AreEqual(null, object2.Root);
            Assert.AreEqual(null, object3.Root);
            Assert.AreEqual(null, object1.Stage);
            Assert.AreEqual(null, object2.Stage);
            Assert.AreEqual(null, object3.Stage);

            Stage stage = CreateInstance<Stage>(100, 100);
            
            stage.AddChild(object1);

            Assert.AreEqual(object1, object1.Root);
            Assert.AreEqual(object1, object2.Root);
            Assert.AreEqual(object1, object3.Root);
            Assert.AreEqual(stage, object1.Stage);
            Assert.AreEqual(stage, object2.Stage);
            Assert.AreEqual(stage, object3.Stage);
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

            Matrix2D matrix = sprite.GetTransformationMatrix(child);
            Matrix2D expectedMatrix = child.TransformationMatrix;
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

            Matrix2D matrix = Matrix2D.Create();
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

            Matrix2D matrix = Matrix2D.Create();
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
        public void TestSetTransformationMatrixWithPivot()
        {
            // pivot point information is redundant; instead, x/y properties will be modified.
            Sprite sprite = new Sprite();
            sprite.PivotX = 50;
            sprite.PivotY = 20;

            Matrix2D matrix = sprite.TransformationMatrix;
            sprite.TransformationMatrix = matrix;
            
            AssertAreEqualWithSmallError(sprite.X, -50);
            AssertAreEqualWithSmallError(sprite.Y, -20);
            AssertAreEqualWithSmallError(sprite.PivotX, 0.0f);
            AssertAreEqualWithSmallError(sprite.PivotY, 0.0f);
        }

        [Test]
        public void TestSetTransformationMatrixWithRightAngle()
        {
            Sprite sprite = new Sprite();
            float[] angles = { (float)(Math.PI / 2.0f), (float)(-Math.PI / 2.0f) };
            Matrix2D[] matrices = {
                Matrix2D.Create(0, 1,-1, 0),
                Matrix2D.Create(0,-1, 1, 0)
            };

            for (int i=0; i<2; ++i)
            {
                float angle = angles[i];
                Matrix2D matrix = matrices[i];
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
            Matrix2D matrix = Matrix2D.Create(0,0,0,0,0,0);
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
            AssertAreEqualWithSmallError(-30, bounds.X, "wrong bounds.x: " + bounds.X);
            AssertAreEqualWithSmallError(10, bounds.Y, "wrong bounds.y: " + bounds.Y);
            AssertAreEqualWithSmallError(20, bounds.Width, "wrong bounds.width: " + bounds.Width);
            AssertAreEqualWithSmallError(10, bounds.Height, "wrong bounds.height: " + bounds.Height);

            bounds = quad.GetBounds(quad);
            AssertAreEqualWithSmallError(0, bounds.X, "wrong inner bounds.x: " + bounds.X);
            AssertAreEqualWithSmallError(0, bounds.Y, "wrong inner bounds.y: " + bounds.Y);
            AssertAreEqualWithSmallError(10, bounds.Width, "wrong inner bounds.width: " + bounds.Width);
            AssertAreEqualWithSmallError(20, bounds.Height, "wrong innter bounds.height: " + bounds.Height);
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

            quad = new Quad(100, 200);
            quad.Width = 0.0f;
            quad.Height = 0.0f;
            AssertAreEqualWithSmallError(0.0f, quad.Width, "wrong width");
            AssertAreEqualWithSmallError(0.0f, quad.Height, "wrong height");

            quad.Width = 50.0f;
            quad.Height = 100.0f;
            AssertAreEqualWithSmallError(0.5f, quad.ScaleX, "wrong scaleX");
            AssertAreEqualWithSmallError(0.5f, quad.ScaleY, "wrong scaleY");
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
        public void TestGlobalToLocal()
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

            Point globalPoint = Point.Create(160, 220);
            Point localPoint = sprite2.GlobalToLocal(globalPoint);
            Point expectedPoint = Point.Create();
            ComparePoints(expectedPoint, localPoint);
            
            // the position of the root object should be irrelevant -- we want the coordinates
            // *within* the root coordinate system!
            root.X = 50;
            localPoint = sprite2.GlobalToLocal(globalPoint);
            ComparePoints(expectedPoint, localPoint);
        }

        // + add missing test

        [Test]
        public void TestPivotPoint()
        {
            float width = 100.0f;
            float height = 150.0f;

            // a quad with a pivot point should behave exactly as a quad without 
            // pivot point inside a sprite

            Sprite sprite = new Sprite();
            Quad innerQuad = new Quad(width, height);
            sprite.AddChild(innerQuad);
            Quad quad = new Quad(width, height);
            CompareRectangles(sprite.Bounds, quad.Bounds);
            
            innerQuad.X = -50;
            quad.PivotX = 50;            
            innerQuad.Y = -20;
            quad.PivotY = 20;            
            CompareRectangles(sprite.Bounds, quad.Bounds);
            
            sprite.Rotation = quad.Rotation = MathUtil.Deg2Rad(45);
            CompareRectangles(sprite.Bounds, quad.Bounds);
            
            sprite.ScaleX = quad.ScaleX = 1.5f;
            sprite.ScaleY = quad.ScaleY = 0.6f;
            CompareRectangles(sprite.Bounds, quad.Bounds);
            
            sprite.X = quad.X = 5;
            sprite.Y = quad.Y = 20;
            CompareRectangles(sprite.Bounds, quad.Bounds);
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

