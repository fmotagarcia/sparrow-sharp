using NUnit.Framework;
using Sparrow.Core;
using Sparrow.Display;

namespace Tests
{
    // note: Assert.Equals is a method of object, its not for unit tests
    [TestFixture]
    public class DisplayObjectContainerTest
    {
        private Sprite _testRoot;

        [SetUp]
        protected void SetUp()
        {
            SparrowSharp.Start(32, 32, 32, 32, typeof(Sprite));
            _testRoot = new Sprite();
            SparrowSharp.Stage.AddChild(_testRoot);
        }

        [TearDown]
        protected void TearDown()
        {
            _testRoot.RemoveAllChildren();
            SparrowSharp.Destroy();
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

            _testRoot.AddChild(sp);

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
            _testRoot.AddChild(sp);
            order = order + "4";

            Assert.AreEqual(2, sp.NumChildren);
            Assert.AreEqual("1234", order);
        }
    }
}

