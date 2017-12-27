using System;
using System.Collections.Generic;
using Android.Views;
using Sparrow.ResourceLoading;
using Sparrow.Touches;
using Android.Runtime;
using OpenGL;
using System.Runtime.InteropServices;
using System.Threading;
using Android.Graphics;
using System.Diagnostics;

namespace Sparrow.Core
{
    /// <summary>
    /// The Android-specific base class for a Sparrow project.
    /// </summary>
    public class AndroidViewController : SurfaceView, ISurfaceHolderCallback
    {
        /// <summary>
        /// Surface holder.
        /// </summary>
        private ISurfaceHolder _Holder;

        public delegate void OnLoadedAction(int viewWidth,int viewHeight);

        private readonly Type _rootClass;
        public static Android.Content.Context AndroidContext;
        private readonly TouchProcessor touchProcessor;
        private readonly Stopwatch sw = new Stopwatch();
        private IntPtr _NativeWindowHandle;

        /// <summary>
        /// The <see cref="DeviceContext"/> created on this GlSurfaceView.
        /// </summary>
        protected DeviceContext _DeviceContext;

        /// <summary>
        /// The OpenGL context created on this GlSurfaceView.
        /// </summary>
        protected IntPtr _RenderContext;

        /// <summary>
        /// Get native window from surface
        /// </summary>
        /// <param name="jni"></param>
        /// <param name="surface"></param>
        /// <returns></returns>
        [DllImport("android")]
        private static extern IntPtr ANativeWindow_fromSurface(IntPtr jni, IntPtr surface);

        /// <summary>
        /// Get native window from surface
        /// </summary>
        /// <param name="surface"></param>
        [DllImport("android")]
        private static extern void ANativeWindow_release(IntPtr surface);

        /// <summary>
        /// Timer used for triggering rendering.
        /// </summary>
        private Timer _RenderTimer;

        /// <summary>
        /// Due time for triggering rendering, in milliseconds.
        /// </summary>
        private int _RenderTimerDueTime;

        public AndroidViewController(Android.Content.Context context, Type rootClass) : base(context)
        {
            Debug.WriteLine("Sparrow: Starting");
            _rootClass = rootClass;
            touchProcessor = new TouchProcessor();

             _Holder = Holder;
            _Holder.AddCallback(this);

            AndroidContext = context;
            TextureLoader._context = context;
            RequestFocus();
            FocusableInTouchMode = true;
        }


        /// <summary>
        /// This is called immediately after the surface is first created.
        /// </summary>
        /// <param name="holder">
        /// The SurfaceHolder whose surface is being created.
        /// </param>
        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Debug.WriteLine("Sparrow: SurfaceCreated");
            // Get actual native window handle
            _NativeWindowHandle = ANativeWindow_fromSurface(JNIEnv.Handle, holder.Surface.Handle);

            // Create device context
            _DeviceContext = DeviceContext.Create(IntPtr.Zero, _NativeWindowHandle);
            _DeviceContext.IncRef();

            // Set pixel format
            DevicePixelFormatCollection pixelFormats = _DeviceContext.PixelsFormats;
            DevicePixelFormat controlReqFormat = new DevicePixelFormat();

            controlReqFormat.RgbaUnsigned = true;
            controlReqFormat.RenderWindow = true;
            controlReqFormat.ColorBits = 24;
            //controlReqFormat.DepthBits = (int)DepthBits;
            //controlReqFormat.StencilBits = (int)StencilBits;
            //controlReqFormat.MultisampleBits = (int)MultisampleBits;
            //controlReqFormat.DoubleBuffer = true;

            List<DevicePixelFormat> matchingPixelFormats = pixelFormats.Choose(controlReqFormat);
            if (matchingPixelFormats.Count == 0)
                throw new InvalidOperationException("unable to find a suitable pixel format");
            _DeviceContext.SetPixelFormat(matchingPixelFormats[0]);

            // Create OpenGL context using compatibility profile
            if ((_RenderContext = _DeviceContext.CreateContext(IntPtr.Zero)) == IntPtr.Zero)
                throw new InvalidOperationException("unable to create render context");
            // Make context current
            if (_DeviceContext.MakeCurrent(_RenderContext) == false)
                throw new InvalidOperationException("unable to make context current");

            Invalidate();
            
            if (_RenderTimer != null)
            {
                throw new InvalidOperationException("rendering already active");
            }

            if (SparrowSharp.Root == null)
            {
                SparrowSharp.NativeWindow = this;
                // TODO get viewport dimensions?
                SparrowSharp.Start((uint)Width, (uint)Height, (uint)Width, (uint)Height, _rootClass);
            }
            else
            {
                SparrowSharp.OnContextCreated();
            }

            _RenderTimerDueTime = (int)Math.Ceiling(1000.0f / 60.0f);
            _RenderTimer = new Timer(RenderTimerCallback, null, _RenderTimerDueTime, Timeout.Infinite);
        }

        private void RenderTimerCallback(object state)
        {
            // Rendering on main UI thread
            Android.App.Application.SynchronizationContext.Send(delegate {
                if (_DeviceContext == null)
                {
                    return;
                }
                bool needsSwap = SparrowSharp.Step();
                if (needsSwap)
                {
                    _DeviceContext.SwapBuffers();
                }
                
                int elapsed = (int)sw.ElapsedMilliseconds - 1;
                if (elapsed > _RenderTimerDueTime)
                {
                    elapsed = _RenderTimerDueTime;
                }
                _RenderTimer.Change(_RenderTimerDueTime - elapsed, Timeout.Infinite);
                sw.Restart();
            }, null);
           
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int w, int h)
        {
            Debug.WriteLine("Sparrow: SurfaceChanged");
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Debug.WriteLine("Sparrow: SurfaceDestroyed");
            if (_RenderTimer != null)
            {
                _RenderTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _RenderTimer.Dispose();
                _RenderTimer = null;
            }

            if (_DeviceContext != null)
            {
                if (_RenderContext != IntPtr.Zero)
                {
                    _DeviceContext.DeleteContext(_RenderContext);
                }
                _DeviceContext.Dispose();
                _DeviceContext = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Release native window handle
            if (_NativeWindowHandle != IntPtr.Zero)
            {
                ANativeWindow_release(_NativeWindowHandle);
                _NativeWindowHandle = IntPtr.Zero;
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            Debug.WriteLine("Sparrow: Android view size changed");
            base.OnSizeChanged(w, h, oldw, oldh);
            SparrowSharp.ViewPort.Width = w;
            SparrowSharp.ViewPort.Height = h;
        }
        
        /// <summary>
        /// This function handles touch events. 
        /// It is responsible for maintaining the currently active touch events and dispatching events.
        /// For details see http://developer.android.com/reference/android/view/View.html#onTouchEvent(android.view.MotionEvent)
        /// </summary>
        public override bool OnTouchEvent(MotionEvent e)
        {
            float xConversion = SparrowSharp.Stage.StageWidth / Width;
            float yConversion = SparrowSharp.Stage.StageHeight / Height;

            // get pointer index from the event object
            int pointerIndex = e.ActionIndex;
            // get pointer ID
            int pointerId = e.GetPointerId(pointerIndex);

            // get masked (not specific to a pointer) action
            MotionEventActions maskedAction = e.ActionMasked;

            switch (maskedAction)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    touchProcessor.OnPointerDown(e.GetX() * xConversion, e.GetY() * yConversion, pointerId);
                    break;
                case MotionEventActions.Move:
                    for (int size = e.PointerCount, i = 0; i < size; i++)
                    {
                        touchProcessor.OnPointerMove(e.GetX(i) * xConversion, e.GetY(i) * yConversion, e.GetPointerId(i));
                    }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    touchProcessor.OnPointerUp(pointerId);
                    break;
                case MotionEventActions.Cancel:
                    touchProcessor.OnPointerUp(pointerId);
                    break;
            }
            return true;
        }
    }
}

