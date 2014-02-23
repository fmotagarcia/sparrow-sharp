using System;

namespace Sparrow.Utils
{
	public class VertexColorHelper
	{
		public static VertexColor CreateVertexColor (uint r, uint g, uint b, uint a)
		{
			VertexColor vertexColor = new VertexColor ();

			vertexColor.R = r;
			vertexColor.G = g;
			vertexColor.B = b;
			vertexColor.A = a;

			return vertexColor;
		}

		public static VertexColor CreateVertexColor (uint color, double alpha)
		{
			VertexColor vertexColor = new VertexColor ();

			vertexColor.R = ColorUtil.GetR (color);
			vertexColor.G = ColorUtil.GetG (color);
			vertexColor.B = ColorUtil.GetB (color);
			vertexColor.A = Convert.ToUInt32 (alpha * 255.0);

			return vertexColor;
		}

		public static VertexColor PremultiplyAlpha (VertexColor color)
		{
			double alpha = color.A / 255.0;

			if (alpha == 1.0) {
				return color;
			} else {
				return VertexColorHelper.CreateVertexColor (Convert.ToUInt32 (color.R * alpha),
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
				return VertexColorHelper.CreateVertexColor (Convert.ToUInt32 (color.R / alpha),
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