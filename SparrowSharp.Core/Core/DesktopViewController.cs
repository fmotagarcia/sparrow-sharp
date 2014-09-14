using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Input;
using Sparrow;

namespace Sparrow.Core
{
    public class DesktopViewController : GameWindow
    {
        private Type _rootClass;

        public DesktopViewController(Type rootClass)
        {
            _rootClass = rootClass;

            Load += HandleLoad;

            Resize += HandleResize;

            UpdateFrame += HandleUpdateFrame;

            RenderFrame += HandleRenderFrame;

            // Run the game at 60 updates per second
            Run(60.0);
        }

        private void HandleRenderFrame(object sender, FrameEventArgs e)
        {
            SwapBuffers();
        }

        private void HandleUpdateFrame(object sender, FrameEventArgs e)
        {
            SparrowSharpApp.Step(e.Time);
            // add game logic, input handling
            if (Keyboard[Key.Escape])
            {
                Exit();
            }
        }

        private void HandleResize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        private void HandleLoad(object sender, EventArgs e)
        {
            // setup settings, load textures, sounds
            VSync = VSyncMode.On;
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Framebuffer error: " + status);
            }
            SparrowSharpApp.Start(Size.Width, Size.Height, _rootClass);
        }
    }
}

