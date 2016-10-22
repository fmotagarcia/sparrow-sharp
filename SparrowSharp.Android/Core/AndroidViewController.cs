using System;
using System.Collections.Generic;
using Android.Util;
using Android.Views;
using Sparrow.Geom;
using Sparrow.ResourceLoading;
using Sparrow.Touches;
using Android.Runtime;
using OpenGL;
using System.Runtime.InteropServices;
using System.Threading;
using Android.Graphics;

namespace Sparrow.Core
{
    /// <summary>
    /// The device-specific base class for a Sparrow project.
    /// Note that this class contains device specific functionality, so if you override a function
    /// its likely that you will need to define platform specific behaviour using #ifdefs
    /// </summary>
    public class AndroidViewController : SurfaceView, ISurfaceHolderCallback
    {
        /// <summary>
        /// Surface holder.
        /// </summary>
        private ISurfaceHolder _Holder;

        public delegate void OnLoadedAction(int viewWidth,int viewHeight);

        private Type _rootClass;
        public static Android.Content.Context AndroidContext;
        private readonly Dictionary<int, Touch> _touches = new Dictionary<int, Touch>();

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
            Console.WriteLine("Sparrow-sharp: Starting");
            _rootClass = rootClass;
           
            _Holder = Holder;
            _Holder.AddCallback(this);
            _Holder.SetType(SurfaceType.Gpu);

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
            // Get actual native window handle
            _NativeWindowHandle = ANativeWindow_fromSurface(JNIEnv.Handle, holder.Surface.Handle);

            // Create device context
            _DeviceContext = DeviceContext.Create(_NativeWindowHandle);
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

            float fps = 60.0f;
            if (_RenderTimer != null)
            {
                throw new InvalidOperationException("rendering already active");
            }

            if (SparrowSharp.Root == null)
            {
                SparrowSharp.NativeWindow = this;
                SparrowSharp.Start((uint)Width, (uint)Height, _rootClass);
            }

            _RenderTimerDueTime = (int)Math.Ceiling(1000.0f / fps);
            _RenderTimer = new Timer(RenderTimerCallback, null, _RenderTimerDueTime, Timeout.Infinite);
        }

        private void RenderTimerCallback(object state)
        {
            // Rendering on main UI thread
            Android.App.Application.SynchronizationContext.Send(delegate {
                bool needsSwap = SparrowSharp.Step();
                if (needsSwap)
                {
                    _DeviceContext.SwapBuffers();
                }
            }, null);

            _RenderTimer.Change(_RenderTimerDueTime, Timeout.Infinite);
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int w, int h)
        {
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
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
            Console.WriteLine("Sparrow-sharp: Android view size changed");
            base.OnSizeChanged(w, h, oldw, oldh);
            if (SparrowSharp.Stage != null)
            {
                SparrowSharp.Stage.SetDrawableArea((uint)w, (uint)h);
            }
        }
        
        /// <summary>
        /// This function handles touch events. 
        /// It is responsible for maintaining the currently active touch events and dispatching events.
        /// For details see http://developer.android.com/reference/android/view/View.html#onTouchEvent(android.view.MotionEvent)
        /// </summary>
        override public bool OnTouchEvent(MotionEvent e)
        {
            float xConversion = SparrowSharp.Stage.Width / Width;
            float yConversion = SparrowSharp.Stage.Height / Height;

            // get pointer index from the event object
            int pointerIndex = e.ActionIndex;
            Touch touchInFocus;
            // get pointer ID
            int pointerId = e.GetPointerId(pointerIndex);

            double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            // get masked (not specific to a pointer) action
            MotionEventActions maskedAction = e.ActionMasked;

            switch (maskedAction)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    // new pointer
                    Touch newTouch = new Touch();
                    newTouch.TouchID = pointerId;
                    newTouch.TimeStamp = now;
                    newTouch.GlobalX = e.GetX() * xConversion;
                    newTouch.GlobalY = e.GetY() * yConversion;
                    newTouch.InitialGlobalX = newTouch.GlobalX;
                    newTouch.InitialGlobalY = newTouch.GlobalY;
                    newTouch.Phase = TouchPhase.Began;
                    Geom.Point touchPosition = Geom.Point.Create(newTouch.GlobalX, newTouch.GlobalY);
                    newTouch.Target = SparrowSharp.Stage.HitTest(touchPosition);

                    _touches.Add(newTouch.TouchID, newTouch);
                    break;
                case MotionEventActions.Move:
                    for (int size = e.PointerCount, i = 0; i < size; i++)
                    {
                        Touch movedTouch;
                        _touches.TryGetValue(e.GetPointerId(i), out movedTouch);
                        if (movedTouch != null)
                        {
                            // TODO: should we care about historical pointer events?
                            movedTouch.PreviousGlobalX = movedTouch.GlobalX;
                            movedTouch.PreviousGlobalY = movedTouch.GlobalY;
                            movedTouch.TimeStamp = now;
                            float xc = e.GetX(i) * xConversion;
                            float yc = e.GetY(i) * yConversion;
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
                    }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    touchInFocus = _touches[pointerId];
                    touchInFocus.Phase = TouchPhase.Ended;
                    long downTime = Android.OS.SystemClock.UptimeMillis() - e.DownTime;
                    touchInFocus.TimeStamp = now;
                    double dist = Math.Sqrt(
                                      (touchInFocus.GlobalX - touchInFocus.InitialGlobalX) * (touchInFocus.GlobalX - touchInFocus.InitialGlobalX) +
                                      (touchInFocus.GlobalY - touchInFocus.InitialGlobalY) * (touchInFocus.GlobalY - touchInFocus.InitialGlobalY));
                    // TODO: move the time out to a constant, make dist DPI dependent
                    if (downTime < 300 && dist < 50)
                    {
                        touchInFocus.IsTap = true;
                    }
                    break;
                case MotionEventActions.Cancel:
                    touchInFocus = _touches[pointerId];
                    touchInFocus.Phase = TouchPhase.Cancelled;
                    break;
            }

            foreach (Touch tou in _touches.Values)
            {
                TouchEvent touchEvent = new TouchEvent(new List<Touch>(_touches.Values));
                if (tou.Target != null)
                {
                    tou.Target.InvokeTouch(touchEvent);
                }
                //Console.WriteLine ("phase:" + tou.Phase + " ID:" + tou.TouchID + 
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
            return true;
        }
    }
}

