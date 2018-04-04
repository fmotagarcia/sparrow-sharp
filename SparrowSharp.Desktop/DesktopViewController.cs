using System;
using System.Diagnostics;
using OpenGL.CoreUI;
using Sparrow.Touches;

namespace Sparrow.Core
{
    public class DesktopViewController
    {
        private uint _width;
        private uint _height;
        private Type _rootClass;
        private NativeWindow _nativeWindow;
        private readonly TouchProcessor _touchProcessor;
        private long lastRenderTime;

        public DesktopViewController(Type rootClass, uint windowWidth, uint windowHeight)
        {   
            _width = windowWidth;
            _height = windowHeight;
            _rootClass = rootClass;
            _touchProcessor = new TouchProcessor();
            using (NativeWindow nativeWindow = NativeWindow.Create())
            {
                _nativeWindow = nativeWindow;
                nativeWindow.ContextCreated += NativeWindow_ContextCreated;
                nativeWindow.ContextProfile = NativeWindow.ProfileType.Core;
                nativeWindow.DebugContext = NativeWindow.AttributePermission.Enabled;
                nativeWindow.Create(0, 0, windowWidth, windowHeight, NativeWindowStyle.Caption);
                nativeWindow.Show();
                nativeWindow.Run();
            }
        }

        private void NativeWindow_ContextCreated(object sender, NativeWindowEventArgs e)
        {
            lastRenderTime = DateTime.Now.Ticks;
            SparrowSharp.NativeWindow = this;
            SparrowSharp.Start(_width, _height, _width, _height, _rootClass);
            
            //SparrowSharp.MouseIconChange += OnCursorChange;
            
            _nativeWindow.MouseDown += OnMouseButtonDown;
            _nativeWindow.MouseUp += OnMouseButtonUp;
            _nativeWindow.MouseMove += OnMouseMove;
            
            
            _nativeWindow.Render += Control_Render;
            _nativeWindow.Resize += OnResize;
        }

        private void OnResize(object sender, EventArgs e)
        {
            Debug.WriteLine($"Window resize {_nativeWindow.Width}x{_nativeWindow.Height}");
            SparrowSharp.ViewPort.Width = _nativeWindow.Width;
            SparrowSharp.ViewPort.Height = _nativeWindow.Height;
        }

        private void Control_Render(object sender, NativeWindowEventArgs e)
        {
            long now = DateTime.Now.Ticks;
            long inv60FPS = 10000000 / 60;
            if (now - lastRenderTime > inv60FPS)
            {
                lastRenderTime = now;
                SparrowSharp.Step();
            }
        }

        // touch handling
        private void OnMouseButtonDown(object sender, NativeWindowMouseEventArgs e)
        {
            if (e.Buttons == MouseButton.Left)
            {
                _touchProcessor.OnPointerDown(e.Location.X, e.Location.Y, 1);
            }
        }

        private void OnMouseMove(object sender, NativeWindowMouseEventArgs e)
        {
            if (e.Buttons == MouseButton.Right)
            {
                _touchProcessor.OnPointerMove(e.Location.X, e.Location.Y, 1);
            }
            else
            {
                _touchProcessor.OnMouseHover(e.Location.X, e.Location.Y, 1);
            }
        }

        private void OnMouseButtonUp(object sender, NativeWindowMouseEventArgs e)
        {
            // does not fire if the mouse goes out of the window area and is released
            _touchProcessor.OnPointerUp(1);
        }

        /*
        private void OnCursorChange(MouseCursor cursor)
        {
            switch (cursor)
            {
                case MouseCursor.Default:
                    Cursor = null;
                    break;
                case MouseCursor.Hand:
                    Cursor = Cursors.Hand;
                    break;
                case MouseCursor.Wait:
                    Cursor = Cursors.Wait;
                    break;
            }
        }
        */
    }
}
