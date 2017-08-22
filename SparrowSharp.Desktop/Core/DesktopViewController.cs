using System;
using Sparrow.Touches;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using OpenGL;
using Sparrow.Utils;

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
                return $"msg=0x{(int) Message:x} ({Message}) hwnd=0x{HWnd.ToInt32():x} wparam=0x{WParam.ToInt32():x} lparam=0x{LParam.ToInt32():x} pt=0x{Point:x}";
            }
        }
        
        struct POINT
        {
            private int X;
            private int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return "Point {" + X + ", " + Y + ")";
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
            control.ContextProfile = GlControl.ProfileType.Core;
            control.DebugContext = GlControl.AttributePermission.Enabled;
            control.Size = windowSize;
     
            Controls.Add(control);

            control.MouseDown += OnMouseButtonDown;
            control.MouseUp += OnMouseButtonUp;
            control.MouseMove += OnMouseMove;
            control.Resize += OnResize;

            control.ContextCreated += OnContextCreated;
            control.ContextDestroying += ContextDestroying;
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            SparrowSharp.NativeWindow = this; 
            SparrowSharp.Start((uint)ClientSize.Width, (uint)ClientSize.Height, _rootClass);
            SparrowSharp.MouseIconChange += OnCursorChange;
            // Hook the application’s idle event
            Application.Idle += OnApplicationIdle;
        }

        private void ContextDestroying(object sender, GlControlEventArgs e)
        {
            Console.WriteLine("Context loss on Windows is not impletented");
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
                touchProcessor.OnPointerDown(e.X, e.Y, pointerId);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                touchProcessor.OnPointerMove(e.X, e.Y, pointerId);
            }
            else
            {
                touchProcessor.OnMouseHover(e.X, e.Y, pointerId);
            }
        }

        private void OnMouseButtonUp(object sender, MouseEventArgs e)
        {
            touchProcessor.OnPointerUp(pointerId);
        }

        private void OnCursorChange(MouseCursor cursor)
        {
            switch (cursor)
            {
                case MouseCursor.Default:
                    Cursor = DefaultCursor;
                    break;
                case MouseCursor.Hand:
                    Cursor = Cursors.Hand;
                    break;
                case MouseCursor.Wait:
                    Cursor = Cursors.WaitCursor;
                    break;
            }
            
        }
    }
}

