using System;
using Sparrow.Geom;
using Sparrow.Display;

namespace Sparrow.Touches
{
	/*
	 A Touch contains information about the presence or movement of a finger on the screen.
	 
	 You receive objects of this type via an TouchEvent. When such an event is triggered, you can 
	 query it for all touches that are currently present on the screen. One Touch object contains
	 information about a single touch.
	 
	 **The phase of a touch**
	 
	 Each touch normally moves through the following phases in its life:
	 
	 'Began -> Moved -> Ended'
	 
	 Furthermore, a touch can enter a 'STATIONARY' phase. That phase does not
	 trigger a touch event itself, and it can only occur when 'Multitouch' is activated. Picture a 
	 situation where one finger is moving and the other is stationary. A touch event will
	 be dispatched only to the object under the _moving_ finger. In the list of touches of
	 that event, you will find the second touch in the stationary phase.
	 
	 **The position of a touch**
	 
	 You can get the current and last position on the screen with corresponding properties. However, 
	 you'll want to have the position in a different coordinate system most of the time. 
	 For this reason, there are methods that convert the current and previous touches into the local
	 coordinate system of any object.
	*/
	public class Touch
	{

		internal uint _touchID;

		/// Converts the current location of a touch to the local coordinate system of a display object.
		public Point LocationInSpace(DisplayObject space)
		{
			Matrix transformationMatrix = _target.Root.TransformationMatrixToSpace (space);
			return transformationMatrix.TransformPoint (_globalX, _globalY);
		}

		/// Converts the previous location of a touch to the local coordinate system of a display object.
		public Point PreviousLocationInSpace(DisplayObject space)
		{
			Matrix transformationMatrix = _target.Root.TransformationMatrixToSpace (space);
			return transformationMatrix.TransformPoint (_previousGlobalX, _previousGlobalY);
		}

		/// Returns the movement of the touch between the current and previous location.
		public Point MovementInSpace(DisplayObject space)
		{
			Matrix transformationMatrix = _target.Root.TransformationMatrixToSpace (space);
			Point curLoc = transformationMatrix.TransformPoint (_globalX, _globalY);
			Point preLoc = transformationMatrix.TransformPoint (_previousGlobalX, _previousGlobalY);
			return curLoc.SubtractPoint (preLoc);
		}

		private double _timeStamp;
		/// The moment the event occurred (in seconds since application start).
		public double TimeStamp {
			get {
				return _timeStamp;
			}
			internal set {
				_timeStamp = value;
			}
		}

		private float _globalX;
		/// The x-position of the touch in screen coordinates
		public float GlobalX {
			get {
				return _globalX;
			}
			internal set {
				_globalX = value;
			}
		}

		private float _globalY;
		/// The y-position of the touch in screen coordinates
		public float GlobalY {
			get {
				return _globalY;
			}
			internal set {
				_globalY = value;
			}
		}


		private float _previousGlobalX;
		/// The previous x-position of the touch in screen coordinates
		public float PreviousGlobalX {
			get {
				return _previousGlobalX;
			}
			internal set {
				_previousGlobalX = value;
			}
		}

		private float _previousGlobalY;
		/// The previous y-position of the touch in screen coordinates
		public float PreviousGlobalY {
			get {
				return _previousGlobalY;
			}
			internal set {
				_previousGlobalY = value;
			}
		}

		private int _tapCount;
		/// The number of taps the finger made in a short amount of time. Use this to detect double-taps, etc.
		public int TapCount {
			get {
				return _tapCount;
			}
			internal set {
				_tapCount = value;
			}
		}

		private TouchPhase _phase;
		/// The current phase the touch is in.
		public TouchPhase Phase {
			get {
				return _phase;
			}
			internal set {
				_phase = value;
			}
		}

		private DisplayObject _target;
		/// The display object at which the touch occurred.
		public DisplayObject Target {
			get {
				return _target;
			}
			internal set {
				_target = value;
			}
		}
	}
}

