using System;
using SparrowSharp.Samples.Desktop.Core;
using Sparrow.Samples.Desktop;
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

		private void RegisterResources() {
			DesktopTextureProvider provider = new DesktopTextureProvider ();
			provider.RegisterResource((uint) DesktopResources.Sparrow, "benchmark_object.png");

			TextureFactory.Provider = provider;
		}
	}
}