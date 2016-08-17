using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Input;

namespace Sparrow.Core
{
    public class DesktopViewController : GameWindow
    {
        private readonly Type _rootClass;

        public DesktopViewController(Type rootClass) 
            : base(900, 600, OpenTK.Graphics.GraphicsMode.Default, "title", GameWindowFlags.Default, DisplayDevice.Default, -1, -1,
                  OpenTK.Graphics.GraphicsContextFlags.Debug)
        {
            _rootClass = rootClass;

            Load += HandleLoad;

            RenderFrame += HandleRenderFrame;

            // Run the game at 60 updates per second
            Run(60.0);
        }

        private void HandleRenderFrame(object sender, FrameEventArgs e)
        { 
            SparrowSharpApp.Step(e.Time);
            // add game logic, input handling
            if (Keyboard[Key.Escape])
            {
                Exit();
            }
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            SparrowSharpApp.ReadjustStageSize(Width, Height);
        }

        private void HandleLoad(object sender, EventArgs e)
        {
            // setup settings, load textures, sounds
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Dither);
            GL.Enable(EnableCap.Blend);
            
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Framebuffer error: " + status);
            }
            SparrowSharpApp.NativeWindow = this;
            SparrowSharpApp.Start(Width, Height, _rootClass);
        }
    }
}

