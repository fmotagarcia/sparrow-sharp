using OpenTK;
using OpenTK.Graphics.ES20;
using System;
using OpenTK.Graphics;
using OpenTK.Platform.Android;
using Android.Util;
using SparrowSharp.Core;
using Android.OS;
using Sparrow.Textures;
using Sparrow.ResourceLoading;
using Android.Views;
using System.Collections.Generic;
using Sparrow.Touches;
using Sparrow.Geom;

namespace Sparrow.Core
{
    public class AndroidViewController : AndroidGameView, IViewController
    {
        public delegate void OnLoadedAction(int viewWidth,int viewHeight);

        private OnLoadedAction _onLoadedAction;
        private bool _contextWasLost = false;
        public static Android.Content.Context AndroidContext;
		private long _lastTouchTimestamp;

        public AndroidViewController(Android.Content.Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Setup(context);
        }

        public AndroidViewController(Android.Content.Context context, OnLoadedAction onLoadedAction) : base(context)
        {
            this._onLoadedAction = onLoadedAction;
            Setup(context);
        }

		private Dictionary<int, Touch> touches = new Dictionary<int, Touch>();

		override public bool OnTouchEvent(MotionEvent evt)
		{
			float xConversion = SparrowSharpApp.Stage.Width / Size.Width;
			float yConversion = SparrowSharpApp.Stage.Height / Size.Height;

			// get pointer index from the event object
			int pointerIndex = evt.ActionIndex;
			Dictionary<int, Touch> processedTouches = new Dictionary<int, Touch> ();

			// get pointer ID
			int pointerId = evt.GetPointerId (pointerIndex);

			double now = DateTime.Now.Ticks; // TODO Use C# date

			// get masked (not specific to a pointer) action
			MotionEventActions maskedAction = evt.ActionMasked;

			switch (maskedAction) {
			case MotionEventActions.Down:
			case MotionEventActions.PointerDown:
				// new pointer
				Touch newTouch = new Touch ();
				newTouch._touchID = pointerId;
				newTouch.TimeStamp = now;
				newTouch.GlobalX = evt.GetX () * xConversion;
				newTouch.GlobalY = evt.GetY () * yConversion;
				newTouch.Phase = TouchPhase.Began;
				Point touchPosition = Point.Create (newTouch.GlobalX, newTouch.GlobalY);
				newTouch.Target = SparrowSharpApp.Root.HitTestPoint (touchPosition);

				touches.Add (newTouch._touchID, newTouch);
				processedTouches.Add (newTouch._touchID, newTouch);
				break;
			case MotionEventActions.Move: 
				for (int size = evt.PointerCount, i = 0; i < size; i++) {
					Touch movedTouch; 
					touches.TryGetValue(evt.GetPointerId (i), out movedTouch); 
					if (movedTouch != null) {
						// TODO: what to do with historical pointer events?
						movedTouch.PreviousGlobalX = movedTouch.GlobalX;
						movedTouch.PreviousGlobalY = movedTouch.GlobalY;

						float xc = evt.GetX (i) * xConversion;
						float yc = evt.GetY (i) * yConversion;
						if (movedTouch.GlobalX == xc && movedTouch.GlobalY == yc) {
							movedTouch.Phase = TouchPhase.Stationary;
						} else {
							movedTouch.GlobalX = xc;
							movedTouch.GlobalY = yc;
							movedTouch.Phase = TouchPhase.Moved;
						}
						processedTouches.Add (movedTouch._touchID, movedTouch);

						//touch.TapCount = (int)uiTouch.tapCount; TODO figure out how to do it in Android

						if (movedTouch.Target == null || movedTouch.Target.Stage == null) {
							// target could have been removed from stage -> find new target in that case
							Point updatedTouchPosition = Point.Create (movedTouch.GlobalX, movedTouch.GlobalY);
							movedTouch.Target = SparrowSharpApp.Root.HitTestPoint (updatedTouchPosition);
						}
					}
				}
				break;
			case MotionEventActions.Up:
			case MotionEventActions.PointerUp:
				touches [pointerId].Phase = TouchPhase.Ended;
				processedTouches.Add (touches [pointerId]._touchID, touches [pointerId]);
				break;
			case MotionEventActions.Cancel:
				touches [pointerId].Phase = TouchPhase.Cancelled;
				processedTouches.Add (touches [pointerId]._touchID, touches [pointerId]);
				break;
			}

			foreach (Touch tou in touches.Values) {
				if (processedTouches.ContainsKey (tou._touchID) == false) {
					tou.Phase = TouchPhase.Stationary;
				}
				TouchEvent touchEvent = new TouchEvent (new List<Touch>(touches.Values));
				if (tou.Target != null) {
					tou.Target.InvokeTouch (touchEvent);
				}
				Console.WriteLine ("PHASE: " + tou.Phase + " ID: " + tou._touchID + " target: " + tou.Target);
			}

			_lastTouchTimestamp = evt.EventTime;

			var touchList = new List<Touch> (touches.Values);
			foreach (Touch tou in touchList) {
				if (tou.Phase == TouchPhase.Ended || tou.Phase == TouchPhase.Cancelled) {
					touches.Remove (tou._touchID);
				}
			}
			return true;
		}

        public void Setup(Android.Content.Context context)
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
                Log.Verbose("Sparrow", "Created format {0}", this.GraphicsContext.GraphicsMode);
                All status = GL.CheckFramebufferStatus(All.Framebuffer);
                Log.Verbose("Sparrow", "Framebuffer Status: " + status.ToString());
            }
            catch (Exception)
            {
                throw new NotSupportedException("Could not create OpenGLES 2.0 frame buffer");
            }
            GL.Disable(All.CullFace);
            GL.Disable(All.DepthTest);
            GL.Disable(All.Dither);
            GL.Enable(All.Blend);

            if (_contextWasLost)
            {
                // todo reload context, ReadjustStageSize, ...
            }

            MakeCurrent();
        }
        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MakeCurrent();

            _onLoadedAction(Size.Width, Size.Height);

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

            _contextWasLost = GraphicsContext == null || GraphicsContext.IsDisposed;
        }
    }
}

