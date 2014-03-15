using System;

namespace Sparrow.Utils
{
	public class NumberUtil
	{
		public static bool Equals (float a, float b)
		{
			return Math.Abs (a - b) < 0.000001f;
		}

		public static float Clamp (float value, float min, float max)
		{
			return Math.Min (max, Math.Max (value, min));
		}
	}
}