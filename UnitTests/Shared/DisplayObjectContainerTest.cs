using System;
using NUnit.Framework;
using Sparrow.Display;
using Sparrow.Core;

namespace Tests
{
    // note: Assert.Equals is a method of object, its not for unit tests
    [TestFixture]
    public class DisplayObjectContainerTest
    {

        Sprite testRoot;

        [SetUp]
        protected void SetUp()
        {
            Sparrow.Core.SparrowSharp.Start(12, 12, typeof(Sprite));
            testRoot = new Sprite();
            Sparrow.Core.SparrowSharp.Stage.AddChild(testRoot);
        }

        [TearDown]
        protected void TearDown()
        {
            testRoot.RemoveAllChildren();
            Sparrow.Core.SparrowSharp.Destroy();
        }

        [Test]
        public void TestAddingToStageOrder()
        {
            Sprite sp = new Sprite();
            Sprite sp2 = new Sprite();
            sp.AddChild(sp2);

            sp2.AddedToStage += (target, currentTarget) => {
                sp.AddChild(new Sprite());
            };

            testRoot.AddChild(sp);

            Assert.AreEqual(2, sp.NumChildren);
        }

        [Test]
        public void TestAddingToStageCallOrder()
        {
            string order = "";
            Sprite sp = new Sprite();
            Sprite sp2 = new Sprite();
            sp2.AddedToStage += (target, currentTarget) => {
                order = order + "2";
                Sprite sp3 = new Sprite();
                sp3.AddedToStage += (target2, currentTarget2) => {
                    order = order + "3";
                };
                sp.AddChild(sp3);
            };
            sp.AddChild(sp2);

            order = order + "1";
            testRoot.AddChild(sp);
            order = order + "4";

            Assert.AreEqual(2, sp.NumChildren);
            Assert.AreEqual("1234", order);
        }
    }
}

