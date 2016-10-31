
using Sparrow.Textures;

namespace Tests
{

    public class MockTexture : ConcreteTexture
    {
        public MockTexture(int width = 16, int height = 16, float scale = 1) :
            base(TextureFormat.Rgba4444, width, height, 0, true, false, scale)
        {
        }
    }
}
