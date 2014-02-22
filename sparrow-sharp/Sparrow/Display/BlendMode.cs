using System;
using OpenTK.Graphics.ES20;
using Javax.Xml.Validation;

namespace Sparrow.Display
{
	public class BlendMode
	{
		public const uint AUTO = 0;
		public const uint NORMAL = 1;
		public const uint NONE = 2;

		public static void ApplyBlendFactors (uint blendMode, bool premultipliedAlpha)
		{
			All srcFactor;
			All dstFactor;

			if (blendMode == NONE) {
				GL.Disable (All.Blend);

				return;
			}

			DecodeBlendMode (blendMode, premultipliedAlpha, out srcFactor, out dstFactor);

			GL.Enable (All.Blend);
			GL.BlendFunc (srcFactor, dstFactor);
		}

		public static void DecodeBlendMode (uint blendMode, bool premultipliedAlpha, out All sFactor, out All dFactor)
		{
//			if (premultipliedAlpha) {
//				sFactor = DecodeFactor ((blendMode & 0x00f0) >> 4);
//				dFactor = DecodeFactor (blendMode & 0x000f);
//			} else {
//				sFactor = DecodeFactor ((blendMode & 0xf000) >> 12);
//				dFactor = DecodeFactor ((blendMode & 0x0f00) >> 8);
//			}
			sFactor = All.Zero;
			dFactor = All.One;
		}
		//		public static All DecodeFactor (All factor)
		//		{
		//			if (factor == All.Zero || factor == All.One) {
		//				return factor;
		//			} else {
		//				return factor + 0x0300 - 2;
		//			}
		//		}
	}
}

