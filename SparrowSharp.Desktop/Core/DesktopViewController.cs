using System;
using System.Collections.Generic;
using Sparrow.Touches;
using OpenGL;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Sparrow.Core
{
    public class DesktopViewController : Form
    {
        private readonly Type _rootClass;
        private readonly Dictionary<int, Touch> _touches = new Dictionary<int, Touch>();
        private const int pointerId = 1;
        private readonly Stopwatch sw = new Stopwatch();
        GlControl control;

        [System.Security.SuppressUnmanagedCodeSecurity]
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern bool PeekMessage(ref MSG msg, IntPtr hWnd, int messageFilterMin, int messageFilterMax, int flags);

        struct MSG
        {
            public IntPtr HWnd;
            public uint Message;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public POINT Point;

            public override string ToString()
            {
                return string.Format("msg=0x{0:x} ({1}) hwnd=0x{2:x} wparam=0x{3:x} lparam=0x{4:x} pt=0x{5:x}", (int)Message, Message.ToString(), HWnd.ToInt32(), WParam.ToInt32(), LParam.ToInt32(), Point);
            }
        }
        
        struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return "Point {" + X.ToString() + ", " + Y.ToString() + ")";
            }
        }
        MSG msg = new MSG();

        public DesktopViewController(Type rootClass, int windowWidth, int windowHeight)
        {
            _rootClass = rootClass;
            Size = new Size(windowWidth, windowHeight);

            control = new GlControl();
            control.DepthBits = 8;
            control.StencilBits = 8;
            //control.ContextProfile = GlControl.ProfileType.Core;
            control.DebugContext = GlControl.AttributePermission.Enabled;
            control.Size = Size;
     
            Controls.Add(control);


            MouseDown += OnMouseButtonDown;
            MouseUp += OnMouseButtonUp;
            MouseMove += OnMouseMove;
            Resize += OnResize;

            control.ContextCreated += OnContextCreated;
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            SparrowSharp.NativeWindow = this;
            SparrowSharp.Start((uint)Width, (uint)Height, _rootClass);
            // Hook the application’s idle event
            Application.Idle += new EventHandler(OnApplicationIdle);
        }
        
        private void OnApplicationIdle(object sender, EventArgs e)
        {
           
            while (!PeekMessage(ref msg, IntPtr.Zero, 0, 0, 0))
            {
                sw.Restart();
                bool needsSwap = SparrowSharp.Step();
                if (needsSwap)
                {
                    control.Invalidate();
                    Console.WriteLine("SWAP");
                }
                int msToSleep = 16 - (int)sw.ElapsedMilliseconds - 1;
                if (msToSleep > 0)
                {
                    System.Threading.Thread.Sleep(msToSleep);
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
           // SparrowSharp.Stage.SetDrawableArea((uint)Width, (uint)Height);
        }

        
        // TODO unify this with Android code
        private void OnMouseButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                float xConversion = SparrowSharp.Stage.Width / Size.Width;
                float yConversion = SparrowSharp.Stage.Height / Size.Height;
                Touch newTouch = new Touch();
                newTouch.TouchID = pointerId;
                newTouch.TimeStamp = now;
                newTouch.GlobalX = e.X * xConversion;
                newTouch.GlobalY = e.Y * yConversion;
                newTouch.InitialGlobalX = newTouch.GlobalX;
                newTouch.InitialGlobalY = newTouch.GlobalY;
                newTouch.Phase = TouchPhase.Began;
                Geom.Point touchPosition = Geom.Point.Create(newTouch.GlobalX, newTouch.GlobalY);
                newTouch.Target = SparrowSharp.Stage.HitTest(touchPosition);

                _touches.Add(newTouch.TouchID, newTouch);
            }
            ProcessTouch();
        }
       
        private void OnMouseButtonUp(object sender, MouseEventArgs e)
        {
            Touch touchInFocus;
            if (_touches.ContainsKey(pointerId) == false)
            {
                return; // right click/middle click/..
            }
            touchInFocus = _touches[pointerId];
            touchInFocus.Phase = TouchPhase.Ended;
            long downTime = -1; // TODO
            double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            touchInFocus.TimeStamp = now;
            double dist = Math.Sqrt(
                                (touchInFocus.GlobalX - touchInFocus.InitialGlobalX) * (touchInFocus.GlobalX - touchInFocus.InitialGlobalX) +
                                (touchInFocus.GlobalY - touchInFocus.InitialGlobalY) * (touchInFocus.GlobalY - touchInFocus.InitialGlobalY));
            // TODO: move the time out to a constant, make dist DPI dependent
            if (downTime < 300 && dist < 50)
            {
                touchInFocus.IsTap = true;
            }
            ProcessTouch();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _touches.Count > 0)
            {
                double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                float xConversion = SparrowSharp.Stage.Width / Size.Width;
                float yConversion = SparrowSharp.Stage.Height / Size.Height;

                Touch movedTouch = _touches[pointerId];
                movedTouch.PreviousGlobalX = movedTouch.GlobalX;
                movedTouch.PreviousGlobalY = movedTouch.GlobalY;
                movedTouch.TimeStamp = now;
                float xc = e.X * xConversion;
                float yc = e.Y * yConversion;
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
                    Geom.Point updatedTouchPosition = Geom.Point.Create(movedTouch.GlobalX, movedTouch.GlobalY);
                    movedTouch.Target = SparrowSharp.Root.HitTest(updatedTouchPosition);
                }
            }
            ProcessTouch();
        }

        private void ProcessTouch()
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
        

        // default graphics mode does not have a stencil buffer.
        /*public DesktopViewController(Type rootClass, int windowWidth, int windowHeight, string windowTitle, 
                                     GameWindowFlags windowFlags, DisplayDevice device, GraphicsContextFlags flags) 
            : base(windowWidth, windowHeight, GraphicsMode.Default, windowTitle, windowFlags, device, -1, -1, flags)
        {
            Console.WriteLine("Sparrow-sharp: Starting");
            _rootClass = rootClass;

            Load += HandleLoad;

            RenderFrame += HandleRenderFrame;

            Mouse.Move += OnMouseMove;
            Mouse.ButtonDown += OnMouseButtonChange;
            Mouse.ButtonUp += OnMouseButtonChange;

            // Run the game at 60 updates per second
            Run(60.0);
        }

        private void HandleLoad(object sender, EventArgs e)
        {
            // setup settings, load textures, sounds
            SparrowSharp.NativeWindow = this;
            SparrowSharp.Start((uint)Width, (uint)Height, _rootClass);
        }

        private void HandleRenderFrame(object sender, FrameEventArgs e)
        {
            bool needsSwap = SparrowSharp.Step(e.Time);
            if (needsSwap)
            {
                SwapBuffers();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SparrowSharp.Stage.SetDrawableArea((uint)Width, (uint)Height);
        }

*/
    }

}

