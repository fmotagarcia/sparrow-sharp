using System;
using SparrowSharp.Samples.Desktop.Core;
using Sparrow.Samples.Desktop;

namespace SparrowSharp.Samples.Desktop
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			SampleGameWindow game = new SampleGameWindow ();
			game.Start(typeof (Sparrow.Samples.Desktop.SampleGame));
		}
	}
}