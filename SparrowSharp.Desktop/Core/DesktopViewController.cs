using System;
using System.Diagnostics;
using Sparrow.Touches;
using OpenGL;
using Sparrow.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Cursors = System.Windows.Input.Cursors;

namespace Sparrow.Core
{
    public class DesktopViewController : Window
    {
        private readonly Type _rootClass;
        private readonly TouchProcessor _touchProcessor;
        private const int PointerId = 1;
        private readonly GlControl _control;
        private float _scale = 1;
        private bool _contextCreated;

        /// <summary>
        /// Initializes the game and the native window where your app will run.
        /// </summary>
        /// <param name="rootClass">The starting class of your app. Should be a subclass of Sprite.</param>
        /// <param name="windowWidth">The width of the window. Note that the OS might scale this up on HDPI displays</param>
        /// <param name="windowHeight">The height of the window. Note that the OS might scale this up on HDPI displays</param>
        public DesktopViewController(Type rootClass, int windowWidth, int windowHeight)
        {
            SizeToContent = SizeToContent.WidthAndHeight; // size is determined by the children
            _rootClass = rootClass;
            _touchProcessor = new TouchProcessor();
            
            Grid grid = new Grid();
            grid.Width = windowWidth;
            grid.Height = windowHeight;
            Content = grid;
            grid.Margin = new Thickness(0, 0, 0, 0);
            var host = new WindowsFormsHost();
            host.Margin = new Thickness(0, 0, 0, 0);
            grid.Children.Add(host);

            Gl.Initialize();
            _control = new GlControl();
            host.Child = _control;
            //_control.DepthBits = 8;
            //_control.StencilBits = 8;
            _control.ContextProfile = GlControl.ProfileType.Core;
            _control.DebugContext = GlControl.AttributePermission.Enabled;
            _control.Animation = true;
            _control.AnimationTime = 15;
            _control.ContextCreated += OnContextCreated;
            _control.ContextDestroying += ContextDestroying;
            
            Loaded += OnLoaded;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_contextCreated)
            {
                InitApp();
            }
        }

        private void OnContextCreated(object sender, GlControlEventArgs glControlEventArgs)
        {
            _contextCreated = true;
            if (IsLoaded)
            {
                InitApp();
            }
        }

        private void InitApp()
        {
            SparrowSharp.NativeWindow = this;
            var wi = ((Grid) Content).ActualWidth;
            var he = ((Grid) Content).ActualHeight;
            SparrowSharp.Start((uint)wi, (uint)he, (uint)_control.Width, (uint)_control.Height, _rootClass);
            _scale = (float)(_control.Width / wi);
            SparrowSharp.MouseIconChange += OnCursorChange;
            
            _control.MouseDown += OnMouseButtonDown;
            _control.MouseUp += OnMouseButtonUp;
            _control.MouseMove += OnMouseMove;
            
            _control.Render += Control_Render;
            _control.Resize += OnResize;
        }
        
        private void OnResize(object sender, EventArgs e)
        {
            Debug.WriteLine($"Window resize {_control.Width}x{_control.Height}");
            SparrowSharp.ViewPort.Width = _control.Width;
            SparrowSharp.ViewPort.Height = _control.Height;
        }

        private void Control_Render(object sender, GlControlEventArgs e)
        {
            SparrowSharp.Step();
        }

        private void ContextDestroying(object sender, GlControlEventArgs e)
        {
            Debug.WriteLine("Context loss on Windows is not impletented");
        }
        
        // touch handling
        private void OnMouseButtonDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _touchProcessor.OnPointerDown(e.X / _scale, e.Y / _scale, PointerId);
            }
        }

        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _touchProcessor.OnPointerMove(e.X / _scale, e.Y / _scale, PointerId);
            }
            else
            {
                _touchProcessor.OnMouseHover(e.X / _scale, e.Y / _scale, PointerId);
            }
        }

        private void OnMouseButtonUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _touchProcessor.OnPointerUp(PointerId);
        }
        
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
    }
}

