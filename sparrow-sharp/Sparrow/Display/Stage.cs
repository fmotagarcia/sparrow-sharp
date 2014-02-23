using Sparrow.Core;
using Sparrow.Geom;
using System;
using System.Collections.Generic;

namespace Sparrow.Display
{
	public class Stage : DisplayObjectContainer
	{
        private double _width;
        private double _height;
		private readonly List<DisplayObject> _enterFrameListeners = new List<DisplayObject> ();
		private uint _color;

        override public double Width {
			get { return _width; }
            set { _width = value; }
		}

        override public double Height
        {
			get { return _height; }
			set { _height = value; }
		}

		public uint Color { 
			get { return _color;}
			set { _color = value;}
		}

        public Stage (float initWidth, float initHeight)
		{
            Initialize(initWidth, initHeight);
		}

        public Stage ()
        {
            Initialize(400f, 400f); // TODO get screen size
        }

        private void Initialize(float initWidth, float initHeight)
        {
            _width = initWidth;
            _width = initHeight;
        }

        override public void Render(RenderSupport support)
        {
            support.ClearWithColor(_color, 1.0f);
            support.SetupOrthographicProjection(0, _width, 0, _height);

            base.Render(support);
        }

        public DisplayObject hitTestPoint(Point localPoint)
        {
            if (!visible || !touchable)
            {
                return null;
            }
            // if nothing else is hit, the stage returns itself as target
            DisplayObject target = base.HitTestPoint(localPoint);
            if (target == null) target = this;
            return target;
        }
        
//        override public void AppendDescendantEventListenersOfObject(DisplayObject dispObject, String eventType, List<DisplayObject> listeners)
//        {
//            if (dispObject == this && eventType == EventTypeEnterFrame)
//            {
//                listeners.AddRange(_enterFrameListeners);
//            }
//            else
//            {
//                base.AppendDescendantEventListenersOfObject(dispObject, eventType, listeners);
//            }
//        }

        public void AdvanceTime(double passedTime)
        {
//            EnterFrameEvent enterFrameEvent = new EnterFrameEvent(EventTypeEnterFrame, passedTime);
//            BroadcastEvent(enterFrameEvent);
        }

        public void AddEnterFrameListener(DisplayObject listener)
        {
            _enterFrameListeners.Add(listener);
        }

        public void RemoveEnterFrameListener(DisplayObject listener)
        {
            _enterFrameListeners.Remove( listener );
        }

        override public double X {
			set { throw new Exception("cannot set x-coordinate of stage"); }
		}

		override public double Y {
			set { throw new Exception("cannot set y-coordinate of stage"); }
		}

        override public double ScaleX
        {
            set { throw new Exception("cannot scale stage"); }
        }

        override public double ScaleY
        {
            set { throw new Exception("cannot scale stage"); }
        }

        override public double SkewX
        {
            set { throw new Exception("cannot skew stage"); }
        }

        override public double SkewY
        {
            set { throw new Exception("cannot skew stage"); }
        }

        override public double PivotX
        {
            set { throw new Exception("cannot set PivotX of stage"); }
        }

        override public double PivotY
        {
            set { throw new Exception("cannot set PivotY of stage"); }
        }

        override public double Rotation
        {
            set { throw new Exception("cannot set rotation of stage"); }
        }

	}
}