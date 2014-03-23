using System.Collections.Generic;
using Sparrow.Textures;

namespace Sparrow.Fonts
{
	public class BitmapChar
	{
		private int _charId;
		private Dictionary<int, float> _kernings;

		public  Texture Texture { get; private set; }

		public float XOffset { get; private set; }

		public float YOffset { get; private set; }

		public float XAdvance { get; private set; }

		public float Width
		{
			get { return Texture.Width; }
		}

		public float Height
		{
			get { return Texture.Height; }
		}

		public BitmapChar(int charId, Texture texture, float xOffset, float yOffset, float xAdvance)
		{
			_charId = charId;
			XAdvance = xAdvance;
			Texture = texture;
			XOffset = xOffset;
			YOffset = yOffset;
		}

		public void AddKerning(float amount, int charId)
		{
			// TODO: sparrow lazy loads the kernings dictionary, revisit to check that we really want this
			if (_kernings == null)
			{
				_kernings = new Dictionary<int, float>();
			}

			_kernings.Add(charId, amount);
		}

		public float KerningToChar(int charId)
		{
			return _kernings[charId];
		}
	}
}

