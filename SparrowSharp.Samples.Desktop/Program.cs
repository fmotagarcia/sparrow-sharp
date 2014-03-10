using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SparrowSharp.Samples.Desktop.Core;
using Sparrow.Samples.Desktop;

namespace SparrowSharp.Samples.Desktop
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			ViewController game = new ViewController ();
			game.Start (typeof(SampleGame));
		}
	}
}