using SparrowSharp.Pool;

namespace Sparrow.Fonts
{
    internal sealed class CharLocation : PooledObject
    {
        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        public BitmapChar Char { get; private set; }

        public float Scale;
        public float X;
        public float Y;

        private CharLocation()
        {
        }

        internal static CharLocation Create(BitmapChar bitmapChar, float scale = 1, float x = 0, float y = 0)
        {
            CharLocation charLocation = (CharLocation)_pool.GetObject();
            charLocation.Char = bitmapChar;
            charLocation.Scale = scale;
            charLocation.X = x;
            charLocation.Y = y;

            return charLocation;
        }

        private static CharLocation Init()
        {
            return new CharLocation();
        }
    }
}

