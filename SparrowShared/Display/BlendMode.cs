using System;
using System.Collections.Generic;
using OpenGL;

namespace Sparrow.Display
{
    /// <summary>
    /// A helper class for working with Sparrow's blend modes.
    /// A blend mode is always defined by two OpenGL blend factors. A blend factor represents a particular
    /// value that is multiplied with the source or destination color in the blending formula. The 
    /// blending formula is:
    /// <para>result = source * sourceFactor + destination * destinationFactor</para>
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
        public const string AUTO = "auto";
        public const string NONE = "none";
        public const string NORMAL = "normal";
        public const string ADD = "add";
        public const string MULTIPLY = "multiply";
        public const string SCREEN = "screen";
        public const string ERASE = "erase";
        public const string BELOW = "below";
        public const string MASK = "mask";

        private string _name;
        private readonly BlendingFactor _sourceFactor;
        private readonly BlendingFactor _destinationFactor;
        private static Dictionary<string, BlendMode> _sBlendModes;

        /// <summary>
        /// Creates a new BlendMode instance.
        /// </summary>
        private BlendMode(string name, BlendingFactor srcFactor, BlendingFactor dstFactor)
        {
            _name = name;
            _sourceFactor = srcFactor;
            _destinationFactor = dstFactor;
        }

        /// <summary>
        /// Returns the blend mode with the given name.
        /// </summary>
        /// <exception cref="ArgumentException">The mode does not exist.</exception>
        public static BlendMode Get(string modeName)
        {
            if (_sBlendModes == null)
            {
                RegisterDefaults();
            }
            if (_sBlendModes.ContainsKey(modeName))
            {
                return _sBlendModes[modeName];
            }
            throw new ArgumentException("Blend mode not found: " + modeName);
        }

        /// <summary>
        /// Registers a blending mode under a certain name.
        /// </summary>
        public static BlendMode Register(string name, BlendingFactor srcFactor, BlendingFactor dstFactor)
        {
            if (_sBlendModes == null)
            {
                RegisterDefaults();
            }
            BlendMode blendMode = new BlendMode(name, srcFactor, dstFactor);
            _sBlendModes[name] = blendMode;
            return blendMode;
        }

        private static void RegisterDefaults()
        {
            if (_sBlendModes != null)
            {
                return;
            }

            _sBlendModes = new Dictionary<string, BlendMode>();
            Register(NONE, BlendingFactor.One, BlendingFactor.Zero);
            Register(NORMAL, BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            Register(ADD, BlendingFactor.One, BlendingFactor.One);// was srcAlpha, one
            Register(MULTIPLY, BlendingFactor.DstColor, BlendingFactor.OneMinusSrcAlpha);
            Register(SCREEN, BlendingFactor.One, BlendingFactor.OneMinusSrcColor);
            Register(ERASE, BlendingFactor.Zero, BlendingFactor.OneMinusSrcAlpha);
            Register(MASK, BlendingFactor.Zero, BlendingFactor.SrcAlpha);
            Register(BELOW, BlendingFactor.OneMinusDstAlpha, BlendingFactor.DstAlpha);
        }

        /// <summary>
        /// Sets the appropriate blend factors for source and destination on the current context.
        /// </summary>
        public void Activate()
        {
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(_sourceFactor, _destinationFactor);
        }
    }
}

