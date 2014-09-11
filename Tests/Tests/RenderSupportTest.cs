using System;
using NUnit.Framework;
using Sparrow;
using Sparrow.Display;
using Sparrow.Core;

namespace Tests
{
    public class RenderSupportTest
    {
        Sprite testRoot;

        [SetUp]
        protected void SetUp()
        {
            SparrowSharpApp.Start(12, 12, new Sprite());
            testRoot = new Sprite();
            SparrowSharpApp.Stage.AddChild(testRoot);
        }

        [TearDown]
        protected void TearDown()
        {
            testRoot.RemoveAllChildren();
            SparrowSharpApp.Destroy();
        }

        [Test]
        public void TestForOpenGLErrors()
        {
            Quad q1 = new Quad();
            testRoot.AddChild(q1);
            SparrowSharpApp.Step(15);
            Quad q2 = new Quad();
            q2.Alpha = 0.6f;
            testRoot.AddChild(q2);
            SparrowSharpApp.Step(15);

            Assert.IsFalse(RenderSupport.HasOpenGLError);
        }
    }
}

