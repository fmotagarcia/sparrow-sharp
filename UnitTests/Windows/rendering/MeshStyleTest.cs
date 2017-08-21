using NUnit.Framework;
using Sparrow.Display;
using Sparrow.Styles;
using System;

namespace Tests
{
    [TestFixture]
    class MeshStyleTest : TestBase
    {

        private int _eventCount;

        [Test]
        public void TestAssignment()
        {
            Quad quad0 = new Quad(100, 100);
            Quad quad1 = new Quad(100, 100);
            MeshStyle style = new MeshStyle();
            Type meshStyleType = typeof(MeshStyle);

            quad0.Style = style;
            Assert.AreEqual(style, quad0.Style);
            Assert.AreEqual(style.Target, quad0);

            quad1.Style = style;
            Assert.AreEqual(style, quad1.Style);
            Assert.AreEqual(style.Target, quad1);
            Assert.IsFalse(quad0.Style == style);
            Assert.AreEqual(quad0.Style.Type, meshStyleType);

            quad1.Style = null;
            Assert.AreEqual(quad1.Style.Type, meshStyleType);
            Assert.IsNull(style.Target);
        }
       
        [Test]
        public void TestEnterFrameEvent()
        {
            _eventCount = 0;
            MeshStyle style = new MeshStyle();
            TestQuad quad0 = new TestQuad(100, 100);
            TestQuad quad1 = new TestQuad(100, 100);

            style.EnterFrame += StyleEnterFrame;
            quad0.DispatchEnterFrame();
            Assert.AreEqual(0, _eventCount);

            quad0.Style = style;
            quad0.DispatchEnterFrame();
            Assert.AreEqual(1, _eventCount);

            quad0.DispatchEnterFrame();
            Assert.AreEqual(2, _eventCount);

            quad1.Style = style;
            quad0.DispatchEnterFrame();
            Assert.AreEqual(2, _eventCount);

            quad0.Style = style;
            quad0.DispatchEnterFrame();
            Assert.AreEqual(3, _eventCount);

            style.EnterFrame -= StyleEnterFrame;
            quad0.DispatchEnterFrame();
            Assert.AreEqual(3, _eventCount);
        }

        private void StyleEnterFrame(DisplayObject target, float passedTime)
        {
            ++_eventCount;
        }

        [Test]
        public void TestDefaultStyle()
        {
            Type origStyle = Mesh.DefaultStyle;
            Quad quad = new Quad(100, 100);
            Assert.AreEqual(quad.Style.GetType(), origStyle);

            Mesh.DefaultStyle = typeof(MockStyle);

            quad = new Quad(100, 100);
            Assert.IsTrue(quad.Style is MockStyle);

            Mesh.DefaultStyle = origStyle;
        }



    }

    internal class MockStyle : MeshStyle { }

    internal class TestQuad : Quad
    {
        public TestQuad(float w, float h) : base(w, h) { }
                
        public void DispatchEnterFrame()
        {
            BroadcastEnterFrameEvent(0.5f);
        }
    }
}
