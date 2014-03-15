using System;

namespace Sparrow.Textures
{
	public interface ITextureProvider
	{
		Texture CreateTexture (uint id);
	}
}

