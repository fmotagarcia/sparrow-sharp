using System;
using Sparrow.Core;
using Sparrow;
using Sparrow.Samples;

namespace SparrowSharp.Samples.Desktop
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			new DesktopViewController ((width, height) => SparrowSharpApp.Start(width, height, new Benchmark()));
		}
	}
}