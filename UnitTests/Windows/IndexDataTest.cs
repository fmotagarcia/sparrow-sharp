using NUnit.Framework;
using SparrowSharp.Core.Desktop.Rendering;

namespace Tests
{
    [TestFixture]
    class IndexDataTest
    {
        // note: Assert.Equals is a method of object, its not for unit tests

        [Test]
        public void testCreate()
        {
            IndexData indexData = new IndexData();
            Assert.AreEqual(0, indexData.NumIndices);
            Assert.IsTrue(indexData.UseQuadLayout);
        }

        [Test]
        public void testClear()
        {
            IndexData indexData = new IndexData();
            indexData.AddTriangle(1, 2, 4);
            indexData.Clear();

            Assert.AreEqual(0, indexData.NumIndices);
            Assert.IsTrue(indexData.UseQuadLayout);
        }

        [Test]
        public void testSetIndex()
        {
            IndexData indexData = new IndexData();

            // basic quad data
            indexData.SetIndex(0, 0);
            indexData.SetIndex(1, 1);
            indexData.SetIndex(2, 2);

            Assert.IsTrue(indexData.UseQuadLayout);
            Assert.AreEqual(0, indexData.GetIndex(0));
            Assert.AreEqual(1, indexData.GetIndex(1));
            Assert.AreEqual(2, indexData.GetIndex(2));
            Assert.AreEqual(3, indexData.NumIndices);

            // setting outside the bounds while keeping quad index rules -> fill up with quad data
            indexData.SetIndex(5, 2);
            Assert.IsTrue(indexData.UseQuadLayout);
            Assert.AreEqual(2, indexData.NumTriangles);
            Assert.AreEqual(1, indexData.GetIndex(3));
            Assert.AreEqual(3, indexData.GetIndex(4));
            Assert.AreEqual(2, indexData.GetIndex(5));

            // arbitrary data
            indexData.SetIndex(6, 5);
            Assert.IsFalse(indexData.UseQuadLayout);
            Assert.AreEqual(7, indexData.NumIndices);
            Assert.AreEqual(5, indexData.GetIndex(6));

            // settings outside the bounds -> fill up with zeroes
            indexData.SetIndex(9, 1);
            Assert.AreEqual(10, indexData.NumIndices);
            Assert.AreEqual(0, indexData.GetIndex(7));
            Assert.AreEqual(0, indexData.GetIndex(8));
            Assert.AreEqual(1, indexData.GetIndex(9));
        }

        [Test]
        public void testAppendTriangle()
        {
            IndexData indexData = new IndexData();

            // basic quad data

            indexData.AddTriangle(0, 1, 2);
            indexData.AddTriangle(1, 3, 2);

            Assert.IsTrue(indexData.UseQuadLayout);
            Assert.AreEqual(1, indexData.NumQuads);
            Assert.AreEqual(2, indexData.NumTriangles);
            Assert.AreEqual(6, indexData.NumIndices);

            Assert.AreEqual(0, indexData.GetIndex(0));
            Assert.AreEqual(1, indexData.GetIndex(1));
            Assert.AreEqual(2, indexData.GetIndex(2));
            Assert.AreEqual(1, indexData.GetIndex(3));
            Assert.AreEqual(3, indexData.GetIndex(4));
            Assert.AreEqual(2, indexData.GetIndex(5));

            indexData.NumTriangles = 0;
            Assert.AreEqual(0, indexData.NumIndices);
            Assert.AreEqual(0, indexData.NumTriangles);

            // arbitrary data
            indexData.AddTriangle(1, 3, 2);
            Assert.IsFalse(indexData.UseQuadLayout);
            Assert.AreEqual(1, indexData.NumTriangles);
            Assert.AreEqual(3, indexData.NumIndices);

            Assert.AreEqual(1, indexData.GetIndex(0));
            Assert.AreEqual(3, indexData.GetIndex(1));
            Assert.AreEqual(2, indexData.GetIndex(2));
        }

        [Test]
        public void testAppendQuad()
        {
            var indexData = new IndexData();

            // basic quad data

            indexData.addQuad(0, 1, 2, 3);
            indexData.addQuad(4, 5, 6, 7);

            Assert.IsTrue(indexData.UseQuadLayout);
            Assert.AreEqual(2, indexData.NumQuads);
            Assert.AreEqual(4, indexData.NumTriangles);
            Assert.AreEqual(12, indexData.NumIndices);

            Assert.AreEqual(0, indexData.GetIndex(0));
            Assert.AreEqual(1, indexData.GetIndex(1));
            Assert.AreEqual(2, indexData.GetIndex(2));
            Assert.AreEqual(1, indexData.GetIndex(3));
            Assert.AreEqual(3, indexData.GetIndex(4));
            Assert.AreEqual(2, indexData.GetIndex(5));
            Assert.AreEqual(4, indexData.GetIndex(6));
            Assert.AreEqual(5, indexData.GetIndex(7));
            Assert.AreEqual(6, indexData.GetIndex(8));
            Assert.AreEqual(5, indexData.GetIndex(9));
            Assert.AreEqual(7, indexData.GetIndex(10));
            Assert.AreEqual(6, indexData.GetIndex(11));

            indexData.NumTriangles = 0;
            Assert.AreEqual(0, indexData.NumIndices);
            Assert.AreEqual(0, indexData.NumQuads);

            // arbitrary data
            indexData.addQuad(0, 1, 3, 2);
            Assert.IsFalse(indexData.UseQuadLayout);
            Assert.AreEqual(1, indexData.NumQuads);
            Assert.AreEqual(2, indexData.NumTriangles);
            Assert.AreEqual(6, indexData.NumIndices);

            Assert.AreEqual(0, indexData.GetIndex(0));
            Assert.AreEqual(1, indexData.GetIndex(1));
            Assert.AreEqual(3, indexData.GetIndex(2));
            Assert.AreEqual(1, indexData.GetIndex(3));
            Assert.AreEqual(2, indexData.GetIndex(4));
            Assert.AreEqual(3, indexData.GetIndex(5));
        }

        [Test]
        public void testClone()
        {
            IndexData indexData;
            IndexData clone;

            // with basic quad data
            indexData = new IndexData();
            indexData.AddTriangle(1, 2, 3);
            indexData.AddTriangle(4, 5, 6);

            clone = indexData.Clone();
            Assert.AreEqual(2, clone.NumTriangles);
            Assert.AreEqual(1, clone.GetIndex(0));
            Assert.AreEqual(3, clone.GetIndex(2));
            Assert.AreEqual(5, clone.GetIndex(4));

            // with arbitrary data
            indexData = new IndexData();
            indexData.AddTriangle(0, 1, 2);
            indexData.AddTriangle(1, 3, 2);

            clone = indexData.Clone();
            Assert.AreEqual(2, clone.NumTriangles);
            Assert.AreEqual(1, clone.GetIndex(1));
            Assert.AreEqual(2, clone.GetIndex(2));
            Assert.AreEqual(3, clone.GetIndex(4));
        }

        [Test]
        public void testSetNumIndices()
        {
            var indexData = new IndexData();
            indexData.NumIndices = 6;

            Assert.AreEqual(0, indexData.GetIndex(0));
            Assert.AreEqual(1, indexData.GetIndex(1));
            Assert.AreEqual(2, indexData.GetIndex(2));
            Assert.AreEqual(1, indexData.GetIndex(3));
            Assert.AreEqual(3, indexData.GetIndex(4));
            Assert.AreEqual(2, indexData.GetIndex(5));

            indexData.NumIndices = 0;
            Assert.AreEqual(0, indexData.NumIndices);

            indexData.SetIndex(0, 1);
            Assert.IsFalse(indexData.UseQuadLayout);

            indexData.NumIndices = 3;
            Assert.AreEqual(1, indexData.GetIndex(0));
            Assert.AreEqual(0, indexData.GetIndex(1));
            Assert.AreEqual(0, indexData.GetIndex(2));

            indexData.NumIndices = 0;
            Assert.AreEqual(0, indexData.NumIndices);
            Assert.IsTrue(indexData.UseQuadLayout);
        }

        [Test]
        public void testCopyTo()
        {
            // arbitrary data -> arbitrary data
            var source = new IndexData();
            source.AddTriangle(1, 2, 3);
            source.AddTriangle(4, 5, 6);

            var target = new IndexData();
            target.AddTriangle(7, 8, 9);
            source.CopyTo(target, 0, 0, 3, 3);

            Assert.AreEqual(3, target.NumIndices);
            Assert.AreEqual(4, target.GetIndex(0));
            Assert.AreEqual(5, target.GetIndex(1));
            Assert.AreEqual(6, target.GetIndex(2));

            source.CopyTo(target, 3);
            Assert.AreEqual(9, target.NumIndices);

            // quad data -> quad data
            source.Clear();
            target.Clear();

            source.AddTriangle(0, 1, 2);
            target.addQuad(0, 1, 2, 3);
            source.CopyTo(target, 6, 4);

            Assert.IsTrue(target.UseQuadLayout);
            Assert.AreEqual(9, target.NumIndices);
            Assert.AreEqual(2, target.GetIndex(5));
            Assert.AreEqual(4, target.GetIndex(6));
            Assert.AreEqual(5, target.GetIndex(7));
            Assert.AreEqual(6, target.GetIndex(8));

            // quad data -> arbitrary data
            target.Clear();
            target.addQuad(1, 2, 3, 4);
            source.CopyTo(target, 6, 4);

            Assert.IsTrue(source.UseQuadLayout);
            Assert.IsFalse(target.UseQuadLayout);
            Assert.AreEqual(9, target.NumIndices);
            Assert.AreEqual(3, target.GetIndex(5));
            Assert.AreEqual(4, target.GetIndex(6));
            Assert.AreEqual(5, target.GetIndex(7));
            Assert.AreEqual(6, target.GetIndex(8));

            // arbitrary data -> quad data
            source.Clear();
            source.AddTriangle(1, 2, 3);
            target.Clear();
            target.addQuad(0, 1, 2, 3);
            source.CopyTo(target, 6, 4);

            Assert.IsFalse(source.UseQuadLayout);
            Assert.IsFalse(target.UseQuadLayout);
            Assert.AreEqual(9, target.NumIndices);
            Assert.AreEqual(2, target.GetIndex(5));
            Assert.AreEqual(5, target.GetIndex(6));
            Assert.AreEqual(6, target.GetIndex(7));
            Assert.AreEqual(7, target.GetIndex(8));
        }

        [Test]
        public void testCopyToEdgeCases()
        {
            var source = new IndexData();
            source.NumIndices = 6;

            var target = new IndexData();
            target.NumIndices = 6;

            source.CopyTo(target, 1, 1, 0, 1);
            Assert.IsTrue(target.UseQuadLayout);

            source.CopyTo(target, 3, 0, 1, 1);
            Assert.IsTrue(target.UseQuadLayout);

            source.CopyTo(target, 1, 1, 0, 2);
            Assert.IsTrue(target.UseQuadLayout);

            source.CopyTo(target, 10, 5, 2, 2);
            Assert.IsTrue(target.UseQuadLayout);

            source.CopyTo(target, 13, 8, 1, 4);
            Assert.IsTrue(target.UseQuadLayout);

            source.CopyTo(target, 10, 3, 4, 1);
            Assert.IsFalse(target.UseQuadLayout);
            Assert.AreEqual(6, target.GetIndex(10));
        }

        [Test]
        public void testCopyToWithOffset()
        {
            var source = new IndexData();
            source.AddTriangle(1, 2, 3);
            source.AddTriangle(4, 5, 6);

            var target = new IndexData();
            target.AddTriangle(7, 8, 9);
            source.CopyTo(target, 1, 10, 3, 3);

            Assert.AreEqual(4, target.NumIndices);
            Assert.AreEqual(7, target.GetIndex(0));
            Assert.AreEqual(14, target.GetIndex(1));
            Assert.AreEqual(15, target.GetIndex(2));
            Assert.AreEqual(16, target.GetIndex(3));
        }

        [Test]
        public void testOffsetIndices()
        {
            var indexData = new IndexData();
            indexData.AddTriangle(1, 2, 3);
            indexData.AddTriangle(4, 5, 6);

            indexData.OffsetIndices(10, 1, 3);
            Assert.AreEqual( 1, indexData.GetIndex(0));
            Assert.AreEqual(12, indexData.GetIndex(1));
            Assert.AreEqual(13, indexData.GetIndex(2));
            Assert.AreEqual(14, indexData.GetIndex(3));
            Assert.AreEqual( 5, indexData.GetIndex(4));
        }

        [Test]
        public void testToVector()
        {
            var source = new IndexData();
            source.AddTriangle(1, 2, 3);
            source.AddTriangle(4, 5, 6);

            short[] expected = new short[6] { 1, 2, 3, 4, 5, 6 };
            short[] actual = new short[1];
            source.ToVector(ref actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void testSetIsBasicQuadData()
        {
            var indexData = new IndexData();
            indexData.NumIndices = 6;
            Assert.IsTrue(indexData.UseQuadLayout);
            Assert.AreEqual(1, indexData.GetIndex(3));

            indexData.SetIndex(3, 10);
            Assert.IsFalse(indexData.UseQuadLayout);

            indexData.UseQuadLayout = true;
            Assert.AreEqual(1, indexData.GetIndex(3));

            // basic quad data must be sized correctly
            indexData.UseQuadLayout = false;
            indexData.NumIndices = 12;
            indexData.UseQuadLayout = true;
            indexData.UseQuadLayout = false;
            Assert.AreEqual(6, indexData.GetIndex(11));
        }

    }
}