using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Input;
using System.Collections.Generic;
using Sparrow.Touches;
using Sparrow.Geom;

namespace Sparrow.Core
{
    public class DesktopViewController : GameWindow
    {
        private readonly Type _rootClass;
        private readonly Dictionary<int, Touch> _touches = new Dictionary<int, Touch>();
        private const int pointerId = 1;

        public DesktopViewController(Type rootClass) 
            : base(900, 600, OpenTK.Graphics.GraphicsMode.Default, "title", GameWindowFlags.Default, DisplayDevice.Default, -1, -1,
                  OpenTK.Graphics.GraphicsContextFlags.Debug)
        {
            _rootClass = rootClass;

            Load += HandleLoad;

            RenderFrame += HandleRenderFrame;

            Mouse.Move += OnMouseMove;
            Mouse.ButtonDown += OnMouseButtonChange;
            Mouse.ButtonUp += OnMouseButtonChange;

            // Run the game at 60 updates per second
            Run(60.0);
        }

        // TODO unify this with Android code
        private void OnMouseButtonChange(object sender, MouseButtonEventArgs e)
        {
            double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            float xConversion = SparrowSharpApp.Stage.Width / Size.Width;
            float yConversion = SparrowSharpApp.Stage.Height / Size.Height;
            Touch touchInFocus;

            if (e.Mouse.LeftButton == ButtonState.Pressed)
            {
                Touch newTouch = new Touch();
                newTouch.TouchID = pointerId;
                newTouch.TimeStamp = now;
                newTouch.GlobalX = e.Mouse.X * xConversion;
                newTouch.GlobalY = e.Mouse.Y * yConversion;
                newTouch.InitialGlobalX = newTouch.GlobalX;
                newTouch.InitialGlobalY = newTouch.GlobalY;
                newTouch.Phase = TouchPhase.Began;
                Point touchPosition = Point.Create(newTouch.GlobalX, newTouch.GlobalY);
                newTouch.Target = SparrowSharpApp.Stage.HitTestPoint(touchPosition);

                _touches.Add(newTouch.TouchID, newTouch);
            }
            else
            {
                touchInFocus = _touches[pointerId];
                touchInFocus.Phase = TouchPhase.Ended;
                long downTime = -1; // TODO
                touchInFocus.TimeStamp = now;
                double dist = Math.Sqrt(
                                  (touchInFocus.GlobalX - touchInFocus.InitialGlobalX) * (touchInFocus.GlobalX - touchInFocus.InitialGlobalX) +
                                  (touchInFocus.GlobalY - touchInFocus.InitialGlobalY) * (touchInFocus.GlobalY - touchInFocus.InitialGlobalY));
                // TODO: move the time out to a constant, make dist DPI dependent
                if (downTime < 300 && dist < 50)
                {
                    touchInFocus.IsTap = true;
                }
            }
            processTouch();
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (e.Mouse.LeftButton == ButtonState.Pressed && _touches.Count > 0)
            {
                double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                float xConversion = SparrowSharpApp.Stage.Width / Size.Width;
                float yConversion = SparrowSharpApp.Stage.Height / Size.Height;

                Touch movedTouch = _touches[pointerId];
                movedTouch.PreviousGlobalX = movedTouch.GlobalX;
                movedTouch.PreviousGlobalY = movedTouch.GlobalY;
                movedTouch.TimeStamp = now;
                float xc = e.Mouse.X * xConversion;
                float yc = e.Mouse.Y * yConversion;
                if (movedTouch.GlobalX == xc && movedTouch.GlobalY == yc)
                {
                    movedTouch.Phase = TouchPhase.Stationary;
                }
                else
                {
                    movedTouch.GlobalX = xc;
                    movedTouch.GlobalY = yc;
                    movedTouch.Phase = TouchPhase.Moved;
                }
                if (movedTouch.Target == null || movedTouch.Target.Stage == null)
                {
                    // target could have been removed from stage -> find new target in that case
                    Point updatedTouchPosition = Point.Create(movedTouch.GlobalX, movedTouch.GlobalY);
                    movedTouch.Target = SparrowSharpApp.Root.HitTestPoint(updatedTouchPosition);
                }
            }
            processTouch();
        }

        private void processTouch()
        {
            foreach (Touch tou in _touches.Values)
            {
                TouchEvent touchEvent = new TouchEvent(new List<Touch>(_touches.Values));
                if (tou.Target != null)
                {
                    tou.Target.InvokeTouch(touchEvent);
                }
               // Console.WriteLine ("phase:" + tou.Phase + " ID:" + tou.TouchID + 
                //    " target:" + tou.Target + " isTap:"+ tou.IsTap + " timestamp:" + tou.TimeStamp);
            }

            var touchList = new List<Touch>(_touches.Values);
            foreach (Touch tou in touchList)
            {
                if (tou.Phase == TouchPhase.Ended || tou.Phase == TouchPhase.Cancelled)
                {
                    _touches.Remove(tou.TouchID);
                }
            }
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

