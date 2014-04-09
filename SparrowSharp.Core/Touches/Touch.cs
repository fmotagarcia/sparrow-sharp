using Sparrow.Geom;
using Sparrow.Display;

namespace Sparrow.Touches
{
    /// <summary>
    /// A Touch contains information about the presence or movement of a finger on the screen.

    /// You receive objects of this type via an TouchEvent. When such an event is triggered, you can 
    /// query it for all touches that are currently present on the screen. One Touch object contains
    /// information about a single touch.
    /// 
    /// **The phase of a touch**
    /// 
    /// Each touch normally moves through the following phases in its life:
    /// 
    /// 'Began -> Moved -> Ended'
    /// 
    /// Furthermore, a touch can enter a 'STATIONARY' phase. That phase does not
    /// trigger a touch event itself, and it can only occur when 'Multitouch' is activated. Picture a 
    /// situation where one finger is moving and the other is stationary. A touch event will
    /// be dispatched only to the object under the _moving_ finger. In the list of touches of
    /// that event, you will find the second touch in the stationary phase.
    /// 
    /// **The position of a touch**
    /// 
    /// You can get the current and last position on the screen with corresponding properties. However, 
    /// you'll want to have the position in a different coordinate system most of the time. 
    /// For this reason, there are methods that convert the current and previous touches into the local
    /// coordinate system of any object.
    /// </summary>
    public class Touch
    {
        /// <summary>
        /// Converts the current location of a touch to the local coordinate system of a display object.
        /// </summary>
        public Point LocationInSpace(DisplayObject space)
        {
            Matrix transformationMatrix = Target.Stage.TransformationMatrixToSpace(space);
            return transformationMatrix.TransformPoint(GlobalX, GlobalY);
        }

        /// <summary>
        /// Converts the previous location of a touch to the local coordinate system of a display object.
        /// </summary>
        public Point PreviousLocationInSpace(DisplayObject space)
        {
            Matrix transformationMatrix = Target.Stage.TransformationMatrixToSpace(space);
            return transformationMatrix.TransformPoint(PreviousGlobalX, PreviousGlobalY);
        }

        /// <summary>
        /// Returns the movement of the touch between the current and previous location.
        /// </summary>
        public Point MovementInSpace(DisplayObject space)
        {
            Matrix transformationMatrix = Target.Stage.TransformationMatrixToSpace(space);
            Point curLoc = transformationMatrix.TransformPoint(GlobalX, GlobalY);
            Point preLoc = transformationMatrix.TransformPoint(PreviousGlobalX, PreviousGlobalY);
            curLoc.SubtractPoint(preLoc);
            return curLoc;
        }

        /// <summary>
        /// The ID of this touch
        /// </summary>
        public int TouchID
        {
            get;
            internal set;
        }

        /// <summary>
        /// The moment the event occurred (in seconds since application start).
        /// </summary>
        public double TimeStamp
        {
            get;
            internal set;
        }

        /// <summary>
        /// he x-position of the touch in screen coordinates
        /// </summary>
        public float GlobalX
        {
            get;
            internal set;
        }

        /// <summary>
        /// The y-position of the touch in screen coordinates
        /// </summary>
        public float GlobalY
        {
            get;
            internal set;
        }

        /// <summary>
        /// The previous x-position of the touch in screen coordinates
        /// </summary>
        public float PreviousGlobalX
        {
            get;
            internal set;
        }

        /// <summary>
        /// The previous y-position of the touch in screen coordinates
        /// </summary>
        public float PreviousGlobalY
        {
            get;
            internal set;
        }

        /// The starting x-position of the touch in screen coordinates
        public float InitialGlobalX
        {
            get;
            internal set;
        }

        /// The starting y-position of the touch in screen coordinates
        public float InitialGlobalY
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns true if this touch is a tap. 
        /// This happens if Phase = TouchPhase.Ended and the touch began a short while ago
        /// </summary>
        public bool IsTap
        {
            get;
            internal set;
        }

        /// <summary>
        /// The current phase the touch is in.
        /// </summary>
        public TouchPhase Phase
        {
            get;
            internal set;
        }

        /// <summary>
        /// The display object at which the touch occurred.
        /// </summary>
        public DisplayObject Target
        {
            get;
            internal set;
        }
    }
}

