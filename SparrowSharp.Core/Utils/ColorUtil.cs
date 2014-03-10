using System;

namespace Sparrow.Utils
{
	public class ColorUtil
	{
		public static uint GetRGB (byte r, byte g, byte b)
		{
			return (uint)(r << 16 | g << 8 | b);
		}

		public static byte GetA (uint color)
		{
			return (byte)((color >> 24) & 0xFF);
		}

		public static byte GetR (uint color)
		{
			return (byte)((color >> 16) & 0xFF);
		}

		public static byte GetG (uint color)
		{
			return (byte)((color >> 8) & 0xFF);
		}

		public static byte GetB (uint color)
		{
			return (byte)(color & 0xFF);
		}
	}
}

