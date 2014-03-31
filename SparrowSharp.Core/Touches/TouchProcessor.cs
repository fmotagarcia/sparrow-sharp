using System;
using Sparrow.Display;
using System.Collections.Generic;
using Sparrow.Geom;

namespace Sparrow.Touches
{
	/*
	The TouchProcesser processes raw touch information and dispatches it on display objects.
 
 	_This is an internal class. You do not have to use it manually._
	*/
	internal class TouchProcessor
	{
		/// The root display container to check for touched targets.
		public DisplayObjectContainer Root;

		private List<Touch> _currentTouches;

		/// Initializes a touch processor with a certain root object.
		public TouchProcessor (DisplayObjectContainer root = null)
		{
			Root = root;
			_currentTouches = new List<Touch> (2);

			//[[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(cancelCurrentTouches:)
			//	name:UIApplicationWillResignActiveNotification object:nil];
		}

		/// Processes raw touches and dispatches events on the touched display objects.
		public void ProcessTouches (List<Touch> touches)
		{
			List<Touch> processedTouches = new List<Touch> ();

			// process new touches
			foreach (Touch touch in touches) {
				Touch currentTouch = null;

				foreach (Touch existingTouch in _currentTouches) {
					if (existingTouch.Phase == TouchPhase.Ended || existingTouch.Phase == TouchPhase.Cancelled) {
						continue;
					}

					if (existingTouch._touchID == touch._touchID) {
						// existing touch; update values
						existingTouch.TimeStamp = touch.TimeStamp;
						existingTouch.PreviousGlobalX = touch.PreviousGlobalX;
						existingTouch.PreviousGlobalY = touch.PreviousGlobalY;
						existingTouch.GlobalX = touch.GlobalX;
						existingTouch.GlobalY = touch.GlobalY;
						existingTouch.Phase = touch.Phase;
						existingTouch.TapCount = touch.TapCount;

						if (existingTouch.Target.Stage == null) {
							// target could have been removed from stage -> find new target in that case
							Point touchPosition = Point.Create (touch.GlobalX, touch.GlobalY);
							existingTouch.Target = Root.HitTestPoint (touchPosition);
						}
						currentTouch = existingTouch;
						break;
					}
				}

				if (currentTouch == null) { // new touch
					Point touchPosition = Point.Create (touch.GlobalX, touch.GlobalY);
					touch.Target = Root.HitTestPoint (touchPosition);
					currentTouch = touch;
				}
				processedTouches.Add (currentTouch);
			}

			// dispatch events         
			foreach (Touch touch in processedTouches)
			{       
				TouchEvent touchEvent = new TouchEvent (processedTouches);
				touch.Target.InvokeTouch (touchEvent);
			}
		}

		/* TODO: this needs to be invoked if a touch gets canceled by the OS
		private void CancelCurrentTouches:(NSNotification *)notification
		{
			double now = CACurrentMediaTime();

			for (SPTouch *touch in _currentTouches)
			{
				touch.phase = SPTouchPhaseCancelled;
				touch.timestamp = now;
			}

			for (SPTouch *touch in _currentTouches)
			{
				SPTouchEvent *touchEvent = [[SPTouchEvent alloc] initWithType:SPEventTypeTouch
					touches:_currentTouches];
				[touch.target dispatchEvent:touchEvent];
				[touchEvent release];
			}

			[_currentTouches removeAllObjects];
		}*/

	}
}




