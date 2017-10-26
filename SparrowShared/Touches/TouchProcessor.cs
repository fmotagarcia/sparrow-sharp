
using Sparrow.Core;
using Sparrow.Geom;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sparrow.Touches
{
    public class TouchProcessor
    {

        private readonly Dictionary<int, Touch> _touches = new Dictionary<int, Touch>();

        public void OnPointerDown(float xPosition, float yPosition, int touchId)
        {
            if (SparrowSharp.Stage == null) return;
            double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Touch newTouch = new Touch();
            newTouch.TouchID = touchId;
            newTouch.TimeStamp = now;
            newTouch.GlobalX = xPosition;
            newTouch.GlobalY = yPosition;
            newTouch.InitialGlobalX = newTouch.GlobalX;
            newTouch.InitialGlobalY = newTouch.GlobalY;
            newTouch.Phase = TouchPhase.Began;
            Point touchPosition = Point.Create(newTouch.GlobalX, newTouch.GlobalY);
            newTouch.Target = SparrowSharp.Stage.HitTest(touchPosition);
            Debug.WriteLine("touch target: " + newTouch.Target);
            _touches.Add(newTouch.TouchID, newTouch);
            ProcessTouch();
        }

        public void OnPointerMove(float xPosition, float yPosition, int touchId)
        {
            if (SparrowSharp.Stage == null) return;
            Touch movedTouch;
            _touches.TryGetValue(touchId, out movedTouch);
            if (movedTouch != null)
            {
                double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                // TODO: should we care about historical pointer events?
                movedTouch.PreviousGlobalX = movedTouch.GlobalX;
                movedTouch.PreviousGlobalY = movedTouch.GlobalY;
                movedTouch.TimeStamp = now;
                if (movedTouch.GlobalX == xPosition && movedTouch.GlobalY == yPosition)
                {
                    movedTouch.Phase = TouchPhase.Stationary;
                }
                else
                {
                    movedTouch.GlobalX = xPosition;
                    movedTouch.GlobalY = yPosition;
                    movedTouch.Phase = TouchPhase.Moved;
                }
                if (movedTouch.Target == null || movedTouch.Target.Stage == null)
                {
                    // target could have been removed from stage -> find new target in that case
                    Point updatedTouchPosition = Point.Create(movedTouch.GlobalX, movedTouch.GlobalY);
                    movedTouch.Target = SparrowSharp.Root.HitTest(updatedTouchPosition);
                }
            }
            ProcessTouch();
        }

        private Touch _hoverTouch;
        
        public void OnMouseHover(float xPosition, float yPosition, int touchId)
        {
            if (SparrowSharp.Stage == null) return;
            if (_hoverTouch == null)
            {
                _hoverTouch = new Touch();
                _hoverTouch.Phase = TouchPhase.Hover;
            }
            double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            _hoverTouch.TouchID = touchId;
            _hoverTouch.TimeStamp = now;
            _hoverTouch.PreviousGlobalX = _hoverTouch.GlobalX;
            _hoverTouch.PreviousGlobalY = _hoverTouch.GlobalY;
            _hoverTouch.InitialGlobalX = _hoverTouch.GlobalX;
            _hoverTouch.InitialGlobalY = _hoverTouch.GlobalY;
            _hoverTouch.GlobalX = xPosition;
            _hoverTouch.GlobalY = yPosition;    
            Point touchPosition = Point.Create(xPosition, yPosition);
            var oldTarget = _hoverTouch.Target;
            _hoverTouch.Target = SparrowSharp.Stage.HitTest(touchPosition);
            //Console.WriteLine("hover target: " + _hoverTouch.Target);
            
            var touches = new List<Touch> {_hoverTouch};
            TouchEvent touchEvent = new TouchEvent(touches);
            // invoke the event one last time on the old target
            if (oldTarget != _hoverTouch.Target)
            {
              
                if (touchEvent.GetTouch(oldTarget) == null)
                {
                    oldTarget?.InvokeTouch(touchEvent);   
                }
            }
            _hoverTouch.Target?.InvokeTouch(touchEvent);
        }

        public void OnPointerUp(int touchId)
        {
            if (SparrowSharp.Stage == null) return;
            if (_touches.ContainsKey(touchId) == false)
            {
                return; // right click/middle click/..
            }
            var touchInFocus = _touches[touchId];
            touchInFocus.Phase = TouchPhase.Ended;
            long downTime = -1; // TODO Android.OS.SystemClock.UptimeMillis() - e.DownTime;
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

        public void OnPointerCancel(int touchId)
        {
            if (SparrowSharp.Stage == null) return;
            Touch touchInFocus = _touches[touchId];
            touchInFocus.Phase = TouchPhase.Cancelled;
            ProcessTouch();
        }

        private void ProcessTouch()
        {
            foreach (Touch tou in _touches.Values)
            {
                TouchEvent touchEvent = new TouchEvent(new List<Touch>(_touches.Values));
                tou.Target?.InvokeTouch(touchEvent);
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
    }
}
