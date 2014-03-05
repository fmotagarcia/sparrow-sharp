using System;
using OpenTK.Graphics.ES20;

namespace Sparrow.Display
{
	public class BlendMode
	{
		public const uint AUTO = 0;
		public const uint NONE = 2;
		public const uint NORMAL = 3;
		public const uint ADD = 4;
		public const uint MULTIPLY = 5;
		public const uint SCREEN = 6;
		public const uint ERASE = 7;

		public static void ApplyBlendFactors (uint blendMode, bool premultipliedAlpha)
		{
			All srcFactor;
			All dstFactor;

			if (blendMode == NONE) {
				GL.Disable (All.Blend);

				return;
			}

			if (premultipliedAlpha) {
				switch (blendMode) {
				case NORMAL:
					srcFactor = All.One;
					dstFactor = All.OneMinusSrcAlpha;
					break;
				case ADD:
					srcFactor = All.SrcAlpha;
					dstFactor = All.One;
					break;
				case MULTIPLY:
					srcFactor = All.DstColor;
					dstFactor = All.OneMinusSrcAlpha;
					break;
				case SCREEN:
					srcFactor = All.One;
					dstFactor = All.OneMinusSrcColor;
					break;
				case ERASE:
					srcFactor = All.Zero;
					dstFactor = All.OneMinusSrcAlpha;
					break;
				default:
					break;
				}
			} else {
				switch (blendMode) {
				case NORMAL:
					srcFactor = All.SrcAlpha;
					dstFactor = All.OneMinusSrcAlpha;
					break;
				case ADD:
					srcFactor = All.SrcAlpha;
					dstFactor = All.DstAlpha;
					break;
				case MULTIPLY:
					srcFactor = All.DstColor;
					dstFactor = All.OneMinusSrcAlpha;
					break;
				case SCREEN:
					srcFactor = All.SrcAlpha;
					dstFactor = All.One;
					break;
				case ERASE:
					srcFactor = All.Zero;
					dstFactor = All.OneMinusSrcAlpha;
					break;
				default:
					break;
				}
			}

			GL.Enable (All.Blend);
			GL.BlendFunc (srcFactor, dstFactor);
		}
	}
}

