using System;
using Sparrow.Textures;

namespace Sparrow.Textures
{
	public class TextureFactory
	{
		public static ITextureProvider Provider;

		public static Texture CreateTexture (uint id)
		{
			return Provider.CreateTexture (id);
		}
	}
}

