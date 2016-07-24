using System;
using OpenTK.Graphics.ES20;

namespace Sparrow.Display
{
    /// <summary>
    /// A helper class for working with Sparrow's blend modes.
 
    /// A blend mode is always defined by two OpenGL blend factors. A blend factor represents a particular
    /// value that is multiplied with the source or destination color in the blending formula. The 
    /// blending formula is:
 
    ///     result = source × sourceFactor + destination × destinationFactor
 
    /// In the formula, the source color is the output color of the pixel shader program. The destination
    /// color is the color that currently exists in the color buffer, as set by previous clear and draw
    /// operations.
 
    /// Beware that blending factors produce different output depending on the texture type. Textures may
    /// contain 'premultiplied alpha' (pma), which means that their RGB values were multiplied with their
    /// alpha value. (For example usually Xcode will convert your PNGs to use PMA; other texture types remain 
    /// unmodified.) For this reason, a blending mode may have different factors depending on the pma 
    /// value.
    /// </summary>
    public static class BlendMode
    {
        public const uint AUTO = 0;
        public const uint NONE = 2;
        public const uint NORMAL = 3;
        public const uint ADD = 4;
        public const uint MULTIPLY = 5;
        public const uint SCREEN = 6;
        public const uint ERASE = 7;

        /// <summary>
        /// Makes OpenGL use the blend factors that correspond with a certain blend mode.
        /// </summary>
        public static void ApplyBlendFactors(uint blendMode, bool premultipliedAlpha)
        {
            BlendingFactorSrc srcFactor;
            BlendingFactorDest dstFactor;

            if (blendMode == NONE)
            {
                GL.Disable(EnableCap.Blend);
                return;
            }

            if (premultipliedAlpha)
            {
                switch (blendMode)
                {
                    case NORMAL:
                        srcFactor = BlendingFactorSrc.One;
                        dstFactor = BlendingFactorDest.OneMinusSrcAlpha;
                        break;
                    case ADD:
                        srcFactor = BlendingFactorSrc.SrcAlpha;
                        dstFactor = BlendingFactorDest.One;
                        break;
                    case MULTIPLY:
                        srcFactor = BlendingFactorSrc.DstColor;
                        dstFactor = BlendingFactorDest.OneMinusSrcAlpha;
                        break;
                    case SCREEN:
                        srcFactor = BlendingFactorSrc.One;
                        dstFactor = BlendingFactorDest.OneMinusSrcColor;
                        break;
                    case ERASE:
                        srcFactor = BlendingFactorSrc.Zero;
                        dstFactor = BlendingFactorDest.OneMinusSrcAlpha;
                        break;
                    default:
                        throw new ArgumentException("Invalid blend mode " + blendMode);
                }
            }
            else
            {
                switch (blendMode)
                {
                    case NORMAL:
                        srcFactor = BlendingFactorSrc.SrcAlpha;
                        dstFactor = BlendingFactorDest.OneMinusSrcAlpha;
                        break;
                    case ADD:
                        srcFactor = BlendingFactorSrc.SrcAlpha;
                        dstFactor = BlendingFactorDest.DstAlpha;
                        break;
                    case MULTIPLY:
                        srcFactor = BlendingFactorSrc.DstColor;
                        dstFactor = BlendingFactorDest.OneMinusSrcAlpha;
                        break;
                    case SCREEN:
                        srcFactor = BlendingFactorSrc.SrcAlpha;
                        dstFactor = BlendingFactorDest.One;
                        break;
                    case ERASE:
                        srcFactor = BlendingFactorSrc.Zero;
                        dstFactor = BlendingFactorDest.OneMinusSrcAlpha;
                        break;
                    default:
                        throw new ArgumentException("Invalid blend mode " + blendMode);
                }
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(srcFactor, dstFactor);
        }
    }
}

