using System;

namespace Sparrow.Utils
{
	public class ColorUtil
	{
		public static uint GetRGB (uint r, uint g, uint b)
		{
			return (r << 16 | g << 8 | b);
		}

		public static uint GetA(uint color) {
			return ((color >> 24) & 0xFF);
		}

		public static uint GetR(uint color) {
			return ((color >> 16) & 0xFF);
		}

		public static uint GetG(uint color) {
			return ((color >> 8) & 0xFF);
		}

		public static uint GetB(uint color) {
			return (color & 0xFF);
		}
	}
}

