using Sparrow.Geom;

namespace Sparrow.Textures
{
    internal class TextureInfo
    {
        public readonly Rectangle Region;
        public readonly Rectangle Frame;
        public readonly bool Rotated;

        public TextureInfo(Rectangle region, Rectangle frame, bool rotated)
        {
            if (region != null)
            {
                Region = region.Clone();
            }

            if (frame != null)
            {
                Frame = frame.Clone();
            }

            Rotated = rotated;
        }
    }
}