using System;
using OpenTK.Graphics.ES20;
using System.Collections.Generic;

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
    public class BlendMode
    {
        public const uint AUTO = 0;
        public const uint NONE = 2;
        public const uint NORMAL = 3;
        public const uint ADD = 4;
        public const uint MULTIPLY = 5;
        public const uint SCREEN = 6;
        public const uint ERASE = 7;
        public const uint BELOW = 8;
        public const uint MASK = 9;

        private uint _name;
        private BlendingFactorSrc _sourceFactor;
        private BlendingFactorDest _destinationFactor;
        private static Dictionary<uint, BlendMode> sBlendModes;

        /** Creates a new BlendMode instance. */
        private BlendMode(uint name, BlendingFactorSrc srcFactor, BlendingFactorDest dstFactor)
        {
            _name = name;
            _sourceFactor = srcFactor;
            _destinationFactor = dstFactor;
        }

        /** Returns the blend mode with the given name.
         *  Throws an ArgumentError if the mode does not exist. */
        public static BlendMode Get(uint modeName)
        {
            if (sBlendModes == null) RegisterDefaults();
            if (sBlendModes.ContainsKey(modeName)) return sBlendModes[modeName];
            else throw new ArgumentException("Blend mode not found: " + modeName);
        }

        /** Registers a blending mode under a certain name. */
        public static BlendMode Register(uint name, BlendingFactorSrc srcFactor, BlendingFactorDest dstFactor)
        {
            if (sBlendModes == null) RegisterDefaults();
            BlendMode blendMode = new BlendMode(name, srcFactor, dstFactor);
            sBlendModes[name] = blendMode;
            return blendMode;
        }

        private static void RegisterDefaults()
        {
            if (sBlendModes != null) return;

            sBlendModes = new Dictionary<uint, BlendMode>();
            Register(NONE, BlendingFactorSrc.One, BlendingFactorDest.Zero);
            Register(NORMAL, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            Register(ADD, BlendingFactorSrc.One, BlendingFactorDest.One);// was srcAlpha, one
            Register(MULTIPLY, BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
            Register(SCREEN, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcColor);
            Register(ERASE, BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcAlpha);
            Register(MASK, BlendingFactorSrc.Zero, BlendingFactorDest.SrcAlpha);
            Register(BELOW, BlendingFactorSrc.OneMinusDstAlpha, BlendingFactorDest.DstAlpha);
        }

        /** Sets the appropriate blend factors for source and destination on the current context. */
        public void Activate()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(_sourceFactor, _destinationFactor);
        }

}
}

