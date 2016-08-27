using System;
using Sparrow.Core;
using Sparrow.Samples;
using OpenTK.Graphics;
using OpenTK;

namespace SparrowSharp.Samples.Desktop
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			new DesktopViewController(typeof(Benchmark), 960, 640, "title", GameWindowFlags.Default, DisplayDevice.Default, GraphicsContextFlags.Debug);
		}
	}
}