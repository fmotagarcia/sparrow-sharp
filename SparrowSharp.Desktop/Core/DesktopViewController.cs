using System;
using Sparrow.Touches;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

namespace Sparrow.Core
{
    public class DesktopViewController : Form
    {
        private readonly Type _rootClass;
        private readonly TouchProcessor touchProcessor;
        private const int pointerId = 1;
        private readonly Stopwatch sw = new Stopwatch();
        private GlControl control;

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
            var windowSize = new Size(windowWidth, windowHeight);
            ClientSize = windowSize;
            touchProcessor = new TouchProcessor();

            control = new GlControl();
            control.DepthBits = 8;
            control.StencilBits = 8;
            //control.ContextProfile = GlControl.ProfileType.Core;
            control.DebugContext = GlControl.AttributePermission.Enabled;
            control.Size = windowSize;
     
            Controls.Add(control);

            control.MouseDown += OnMouseButtonDown;
            control.MouseUp += OnMouseButtonUp;
            control.MouseMove += OnMouseMove;
            control.Resize += OnResize;

            control.ContextCreated += OnContextCreated;
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            SparrowSharp.NativeWindow = this; 
            SparrowSharp.Start((uint)ClientSize.Width, (uint)ClientSize.Height, _rootClass);
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

        // touch handling
        private void OnMouseButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float xConversion = SparrowSharp.Stage.StageWidth / Size.Width;
                float yConversion = SparrowSharp.Stage.StageHeight / Size.Height;

                touchProcessor.OnPointerDown(e.X * xConversion, e.Y * yConversion, pointerId);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float xConversion = SparrowSharp.Stage.StageWidth / Size.Width;
                float yConversion = SparrowSharp.Stage.StageHeight / Size.Height;
                touchProcessor.OnPointerMove(e.X * xConversion, e.Y * yConversion, pointerId);
            }
        }

        private void OnMouseButtonUp(object sender, MouseEventArgs e)
        {
            touchProcessor.OnPointerUp(pointerId);
        }
        
    }
}

