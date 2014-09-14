using System;
using Sparrow.Core;
using Sparrow.Samples;

namespace SparrowSharp.Samples.Desktop
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			new DesktopViewController (typeof(Benchmark));
		}
	}
}