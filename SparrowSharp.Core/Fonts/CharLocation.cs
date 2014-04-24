using SparrowSharp.Pool;

namespace SparrowSharp.Filters
{
    internal sealed class CharLocation : PooledObject
    {
        private static readonly ObjectPool _pool = new ObjectPool(new CreateObject<PooledObject>(Init), 1000);

        public BitmapChar BitmapChar { get; private set; }

        public float Scale { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        private CharLocation()
        {
        }

        internal static CharLocation Create(BitmapChar bitmapChar, float scale = 1, float x = 0, float y = 0)
        {
            CharLocation charLocation = (CharLocation)_pool.GetObject();
            charLocation.BitmapChar = bitmapChar;
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

