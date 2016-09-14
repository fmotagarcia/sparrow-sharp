using NUnit.Framework;
using Sparrow.Display;
using SparrowSharp.Core.Rendering;
using SparrowSharp.Core.Styles;
using System;

namespace Tests
{
    [TestFixture]
    class MeshStyleTest : TestBase
    {

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
            /* todo
            var eventCount:int = 0;
            var event:EnterFrameEvent = new EnterFrameEvent(Event.ENTER_FRAME, 0.1);
            var style:MeshStyle = new MeshStyle();
            var quad0:Quad = new Quad(100, 100);
            var quad1:Quad = new Quad(100, 100);

            style.addEventListener(Event.ENTER_FRAME, onEvent);
                quad0.dispatchEvent(event);
            assertEquals(0, eventCount);

            quad0.style = style;
            quad0.dispatchEvent(event);
            assertEquals(1, eventCount);

            quad0.dispatchEvent(event);
            assertEquals(2, eventCount);

            quad1.style = style;
            quad0.dispatchEvent(event);
            assertEquals(2, eventCount);

            quad0.style = style;
            quad0.dispatchEvent(event);
            assertEquals(3, eventCount);

            style.removeEventListener(Event.ENTER_FRAME, onEvent);
            quad0.dispatchEvent(event);
            assertEquals(3, eventCount);

            function onEvent(event:EnterFrameEvent):void
            {
                ++eventCount;
            }
            */
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

    class MockStyle : MeshStyle { }
}
