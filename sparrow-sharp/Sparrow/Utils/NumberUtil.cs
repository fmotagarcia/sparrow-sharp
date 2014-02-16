using System;

namespace Sparrow.Utils
{
	public class NumberUtil
	{
		public static bool Equals (double a, double b)
		{
			return Equals (a, b, 0.000001);
		}

		public static bool Equals (double a, double b, double tolerance)
		{
			return Math.Abs (a - b) < tolerance;
		}

		public static double Clamp(double value, double min, double max) {
			return Math.Min (max, Math.Max (value, min));
		}
	}
}

