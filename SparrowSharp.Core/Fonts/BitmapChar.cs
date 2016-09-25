
using System.Collections.Generic;
using Sparrow.Textures;
using Sparrow.Display;

namespace Sparrow.Fonts
{
    /// <summary>
    /// A BitmapChar contains the information about one char of a bitmap font.
    ///
    /// _You don't have to use this class directly in most cases._
    /// The TextField class contains methods that handle bitmap fonts for you.
    /// </summary>
    public class BitmapChar
    {
        private Dictionary<int, float> _kernings;
        

        /// <summary>
        /// Initializes a char with a texture and its properties.
        /// </summary>
        public BitmapChar(int charId, Texture texture, float xOffset, float yOffset, float xAdvance)
        {
            CharID = charId;
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
            CharID = 0;
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
            if (_kernings == null)
            {
                _kernings = new Dictionary<int, float>();
            }

            _kernings.Add(charId, amount);
        }

        /// <summary>
        /// Retrieve kerning information relative to the given character ID.
        /// </summary>
        public float GetKerning(int charId)
        {
            float kerning = 0.0f;
            if (_kernings != null)
            {
                _kernings.TryGetValue(charId, out kerning);
            }
            return kerning;
        }
        
        /// <summary>
        /// Creates an image of the char.
        /// </summary>
        public Image CreateImage()
        {
            return new Image(Texture);
        }

        /// <summary>
        ///  The unicode ID of the char.
        /// </summary>
        public readonly int CharID;
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
        /// The texture of the character.
        /// </summary>
        public readonly Texture Texture;
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
    }
}

