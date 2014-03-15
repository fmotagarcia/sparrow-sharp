using System;
using SparrowSharp.Samples.Desktop.Core;
using Sparrow.Textures;

namespace SparrowSharp.Samples.Desktop
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			RegisterResources ();

			SampleGameWindow game = new SampleGameWindow ();
			game.Start ();
		}

		private static void RegisterResources() {
			DesktopTextureProvider provider = new DesktopTextureProvider ();
			provider.RegisterResource((uint) DesktopResources.Sparrow, 0);

			TextureFactory.Provider = provider;
		}
	}
}