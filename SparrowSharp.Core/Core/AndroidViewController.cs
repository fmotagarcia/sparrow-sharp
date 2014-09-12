using System;
using System.Collections.Generic;
using Android.Util;
using Android.Views;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.Android;
using Sparrow.Geom;
using Sparrow.ResourceLoading;
using Sparrow.Touches;
using Sparrow.Display;

namespace Sparrow.Core
{
    /// <summary>
    /// The device-specific base class for a Sparrow project.
    /// Note that this class contains device specific functionality, so if you override a function
    /// its likely that you will need to define platform specific behaviour using #ifdefs
    /// </summary>
    public class AndroidViewController : AndroidGameView
    {
        public delegate void OnLoadedAction(int viewWidth,int viewHeight);

        // TODO this should dispatch a resize event on the Stage
        private readonly OnLoadedAction _onLoadedAction;

        private Type _rootClass;
        public static Android.Content.Context AndroidContext;
        private readonly Dictionary<int, Touch> _touches = new Dictionary<int, Touch>();

        public AndroidViewController(Android.Content.Context context, Type rootClass) : base(context)
        {
            _rootClass = rootClass;
            Setup(context);
        }

        private void Setup(Android.Content.Context context)
        {
            AndroidContext = context;
            TextureLoader._context = context;
            RequestFocus();
            FocusableInTouchMode = true;
        }
        // This method is called everytime the context needs to be recreated.
        protected override void CreateFrameBuffer()
        {
            Log.Verbose("Sparrow", "AndroidGameWindow.CreateFrameBuffer");
            try
            {
                ContextRenderingApi = GLVersion.ES2;
                try
                {
                    GraphicsMode = new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, 0, false);
                    base.CreateFrameBuffer();
                }
                catch (Exception)
                {
                    Log.Verbose("Sparrow", "Failed to create desired format, falling back to defaults");
                    // try again using a more basic mode with a 16 bit depth buffer which hopefully the device will support 
                    GraphicsMode = new AndroidGraphicsMode(new ColorFormat(0, 0, 0, 0), 16, 0, 0, 0, false);
                    try
                    {
                        base.CreateFrameBuffer();
                    }
                    catch (Exception)
                    {
                        // ok we are right back to getting the default
                        GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);
                        base.CreateFrameBuffer();
                    }
                }
                Log.Verbose("Sparrow", "Created format {0}", GraphicsContext.GraphicsMode);
                FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                Log.Verbose("Sparrow", "Framebuffer Status: " + status);
            }
            catch (Exception)
            {
                throw new NotSupportedException("Could not create OpenGLES 2.0 frame buffer");
            }
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Dither);
            GL.Enable(EnableCap.Blend);

            MakeCurrent();
        }

        // This gets called when the drawing surface is ready (=on startup and when the app regains focus)
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MakeCurrent();

            if (SparrowSharpApp.Root == null)
            {
                SparrowSharpApp.Start(Size.Width, Size.Height, _rootClass);
            }

            if (_onLoadedAction != null)
            {
                _onLoadedAction(Size.Width, Size.Height);
            }
            // Run the render loop
            Run();
        }
        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            SparrowSharpApp.Step(e.Time);

            SwapBuffers();
        }

        protected override void DestroyFrameBuffer()
        {
            base.DestroyFrameBuffer();
        }

        /// <summary>
        /// This function handles touch events. 
        /// It is responsible for maintaining the currently active touch events and dispatching events.
        /// For details see http://developer.android.com/reference/android/view/View.html#onTouchEvent(android.view.MotionEvent)
        /// </summary>
        override public bool OnTouchEvent(MotionEvent e)
        {
            float xConversion = SparrowSharpApp.Stage.Width / Size.Width;
            float yConversion = SparrowSharpApp.Stage.Height / Size.Height;

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
                    Point touchPosition = Point.Create(newTouch.GlobalX, newTouch.GlobalY);
                    newTouch.Target = SparrowSharpApp.Root.HitTestPoint(touchPosition);

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
                                Point updatedTouchPosition = Point.Create(movedTouch.GlobalX, movedTouch.GlobalY);
                                movedTouch.Target = SparrowSharpApp.Root.HitTestPoint(updatedTouchPosition);
                            }
                        }
                    }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    touchInFocus = _touches[pointerId];
                    touchInFocus.Phase = TouchPhase.Ended;
                    long downTime = Android.OS.SystemClock.UptimeMillis() - e.DownTime;

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
                //Console.WriteLine ("PHASE: " + tou.Phase + " ID: " + tou._touchID + " target: " + tou.Target + " isTap: "+ tou.IsTap);
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

