namespace Sparrow.Utils
{
    public static class Extensions
    {
        public static bool IsFinite(this double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static bool IsFinite(this float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
