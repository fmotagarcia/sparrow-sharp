using System;

namespace Sparrow.Utils
{
	public class VertexColor
	{
		public uint R;
		public uint G;
		public uint B;
		public uint A;

		public VertexColor (uint r, uint g, uint b, uint a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public VertexColor (uint color, double alpha)
		{
			R = ColorUtil.GetR (color);
			G = ColorUtil.GetG (color);
			B = ColorUtil.GetB (color);
			A = Convert.ToUInt32 (alpha * 255.0);
		}

		public static VertexColor PremultiplyAlpha (VertexColor color)
		{
			double alpha = color.A / 255.0;

			if (alpha == 1.0) {
				return color;
			} else {
				return new VertexColor (Convert.ToUInt32 (color.R * alpha),
				                        Convert.ToUInt32 (color.G * alpha),
				                        Convert.ToUInt32 (color.B * alpha),
				                        Convert.ToUInt32 (color.A));
			}
		}

		public static VertexColor UnmultiplyAlpha (VertexColor color)
		{
			double alpha = color.A / 255.0;

			if (alpha == 0.0 || alpha == 1.0) {
				return color;
			} else {
				return new VertexColor (Convert.ToUInt32 (color.R / alpha),
				                        Convert.ToUInt32 (color.G / alpha),
				                        Convert.ToUInt32 (color.B / alpha),
				                        Convert.ToUInt32 (color.A));
			}
		}

		public static bool IsOpaqueWhite (VertexColor color)
		{
			return color.A == 255 && color.R == 255 && color.G == 255 && color.B == 255;
		}
	}
}

