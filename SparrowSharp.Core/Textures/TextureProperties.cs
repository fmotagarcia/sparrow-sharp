using System;
using OpenTK.Graphics.ES20;

namespace Sparrow.Textures
{
	public struct TextureProperties
	{
		public TextureFormat TextureFormat;
		public float Scale;
		public int Width;
		public int Height;
		public int NumMipmaps;
		public bool GenerateMipmaps;
		public bool PremultipliedAlpha;
	}
}

