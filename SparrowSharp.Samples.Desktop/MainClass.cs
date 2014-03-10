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
			SampleGameWindow game = new SampleGameWindow ();
			game.Start (typeof(SampleGame));
		}
	}
}