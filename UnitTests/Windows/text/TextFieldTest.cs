
using NUnit.Framework;
using Sparrow.Display;
using Sparrow.Text;

namespace Tests
{
    [TestFixture]
    public class TextFieldTest : TestBase
    {

        [Test]
        public void TestTextField()
        {
            TextField.DefaultCompositor = new MockCompositor();
            TextField.DefaultCompositor = new MockCompositor();
            TextField textField = new TextField(240, 50, "test text");
            Assert.AreEqual("test text", textField.Text);
        }

        [Test]
        public void TestWidthAndHeight()
        {
            TextField.DefaultCompositor = new MockCompositor();
            TextField.RegisterCompositor(TextField.DefaultCompositor, "mini");
            TextField textField = new TextField(100, 50, "test");

            AssertAreEqualWithSmallError(textField.Width, 100);
            AssertAreEqualWithSmallError(textField.Height, 50);
            AssertAreEqualWithSmallError(textField.ScaleX, 1.0f);
            AssertAreEqualWithSmallError(textField.ScaleY, 1.0f);

            textField.Scale = 0.5f;

            AssertAreEqualWithSmallError(textField.Width, 50);
            AssertAreEqualWithSmallError(textField.Height, 25);
            AssertAreEqualWithSmallError(textField.ScaleX, 0.5f);
            AssertAreEqualWithSmallError(textField.ScaleY, 0.5f);

            textField.Width = 100;
            textField.Height = 50;

            AssertAreEqualWithSmallError(textField.Width, 100);
            AssertAreEqualWithSmallError(textField.Height, 50);
            AssertAreEqualWithSmallError(textField.ScaleX, 0.5f);
            AssertAreEqualWithSmallError(textField.ScaleY, 0.5f);
        }

    }

    internal class MockCompositor : ITextCompositor
    {
        public void ClearMeshBatch(MeshBatch meshBatch) {}

        public void Dispose() {}

        public void FillMeshBatch(MeshBatch meshBatch, float width, float height, string text, 
                                  TextFormat format, TextOptions options = null) {}
    }
}
