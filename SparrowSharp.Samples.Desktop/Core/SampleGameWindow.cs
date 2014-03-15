using OpenTK;
using OpenTK.Graphics.ES20;
using Sparrow.Display;
using System;
using OpenTK.Graphics;
using SparrowSharp.Core;
using Sparrow.Core;
using ActiproSoftware.Products;
using OpenTK.Input;
using Sparrow;
using System.Diagnostics;

namespace SparrowSharp.Samples.Desktop.Core
{
	public class SampleGameWindow : GameWindow, IViewController
    {

		Stopwatch stopwatch = new Stopwatch();

        public SampleGameWindow() : base()
        {
			Load += HandleLoad;

			Resize += HandleResize;

			UpdateFrame += HandleUpdateFrame;

			RenderFrame += HandleRenderFrame;
        }


        void HandleRenderFrame (object sender, FrameEventArgs e)
        {


			//Console.WriteLine ("Number of draw calls: " + RenderSupport.NumDrawCalls);
			stopwatch.Restart ();

			SwapBuffers();
        }

		private void HandleUpdateFrame (object sender, FrameEventArgs e)
        {
			SP.Step(e.Time);
			// add game logic, input handling
			if (Keyboard[Key.Escape])
			{
				Exit();
			}
        }

		private void HandleResize (object sender, EventArgs e)
        {
			GL.Viewport(0, 0, Width, Height);

        }

		private void HandleLoad (object sender, EventArgs e)
        {
			// setup settings, load textures, sounds
			VSync = VSyncMode.On;
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);

			FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

			SP.Start(typeof (Sparrow.Samples.Desktop.SampleGame));
			SP.InitApp (Size.Width, Size.Height);

        }

        public void Start()
        {
			// Run the game at 60 updates per second
			Run(60.0);
        }

    }
}

