using System.Collections.Generic;
using Sparrow.Textures;

namespace SparrowSharp.Filters
{
    public class BitmapChar
    {
        public readonly int CharId;
        private Dictionary<int, float> _kernings;
        /// <summary>
        /// The texture of the character.
        /// </summary>
        public readonly Texture Texture;
        /// <summary>
        /// The number of pixels to move the char in x direction on character arrangement.
        /// </summary>
        public readonly float XOffset;
        /// <summary>
        /// The number of pixels to move the char in y direction on character arrangement.
        /// </summary>
        public readonly float YOffset;
        /// <summary>
        /// The number of pixels the cursor has to be moved to the right for the next char.
        /// </summary>
        public readonly float XAdvance;

        /// <summary>
        /// The width of the character in points.
        /// </summary>
        public float Width
        {
            get { return Texture.Width; }
        }

        /// <summary>
        /// The height of the character in points.
        /// </summary>
        public float Height
        {
            get { return Texture.Height; }
        }

        /// <summary>
        /// Initializes a char with a texture and its properties.
        /// </summary>
        public BitmapChar(int charId, Texture texture, float xOffset, float yOffset, float xAdvance)
        {
            CharId = charId;
            XAdvance = xAdvance;
            Texture = texture;
            XOffset = xOffset;
            YOffset = yOffset;
        }

        /// <summary>
        /// Initializes a char with a texture.
        /// </summary>
        public BitmapChar(Texture texture)
        {
            CharId = 0;
            XAdvance = texture.Width;
            Texture = texture;
            XOffset = 0;
            YOffset = 0;
        }

        /// <summary>
        ///  Adds kerning information relative to a specific other character ID.
        /// </summary>
        public void AddKerning(float amount, int charId)
        {
            // TODO: sparrow lazy loads the kernings dictionary, revisit to check that we really want this
            if (_kernings == null)
            {
                _kernings = new Dictionary<int, float>();
            }

            _kernings.Add(charId, amount);
        }

        /// <summary>
        /// Retrieve kerning information relative to the given character ID.
        /// </summary>
        public float KerningToChar(int charId)
        {
            float kerning = 1.0f;
            if (_kernings != null)
            {
                _kernings.TryGetValue(charId, out kerning);
            }
            return kerning;
        }
    }
}

