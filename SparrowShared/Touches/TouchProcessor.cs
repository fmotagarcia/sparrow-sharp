
using Sparrow.Core;
using Sparrow.Geom;
using System;
using System.Collections.Generic;

namespace Sparrow.Touches
{
    public class TouchProcessor
    {

        private readonly Dictionary<int, Touch> _touches = new Dictionary<int, Touch>();

        public void OnPointerDown(float xPosition, float yPosition, int touchId)
        {
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
            Console.WriteLine("TARGET: " + newTouch.Target);
            _touches.Add(newTouch.TouchID, newTouch);
            ProcessTouch();
        }

        public void OnPointerMove(float xPosition, float yPosition, int touchId)
        {
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

        public void OnPointerUp(int touchId)
        {
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
