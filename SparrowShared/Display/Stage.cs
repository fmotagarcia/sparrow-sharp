

using Sparrow.Geom;
using System;
using OpenGL;
using Sparrow.Filters;

namespace Sparrow.Display
{
    /// <summary>
    /// A Stage is the root of the display tree. It represents the rendering area of the application.

    /// Sparrow will create the stage for you. The root object of your game will be the first child of
    /// the stage. You can access 'root' and 'stage' from any display object using the respective 
    /// properties. 

    /// The stage's 'StageWidth' and 'StageHeight' values define the coordinate system of your game. The color
    /// of the stage defines the background color of your game.
    /// </summary>
    public class Stage : DisplayObjectContainer
    {
        public delegate void ResizeHandler(DisplayObject target);

        /// <summary>
        /// Dispatched when the drawable area changes, e.g. on window resize on PCs or device rotation on mobile.
        /// </summary>
        public event ResizeHandler OnResize;

        /// <summary>
        /// The background color of the stage. Default: black.
        /// </summary>
        public uint Color { get; set; }

        /** Specifies an angle (radian, between zero and PI) for the field of view. This value
         *  determines how strong the perspective transformation and distortion apply to a Sprite3D
         *  object.
         *
         *  <p>A value close to zero will look similar to an orthographic projection; a value
         *  close to PI results in a fisheye lens effect. If the field of view is set to 0 or PI,
         *  nothing is seen on the screen.</p>
         *
         *  @default 1.0
         */
        public float FieldOfView { get; set; }
        private Point _projectionOffset;
        private float _width;
        private float _height;
        /// <summary>
        /// Initializes a stage with a certain size in points. Sparrow calls this automatically on startup.
        /// </summary>
        internal Stage(float width, float height)
        {
            _width = width;
            _height = height;
            Color = 0xFFFFFF;
            FieldOfView = 1.0f;
            _projectionOffset = Point.Create();
           
        }

        internal void SetDrawableArea(uint width, uint height)
        {
            Gl.Viewport(0, 0, (int)width, (int)height);
            if (OnResize != null)
            {
                OnResize(this);
            }
            SetRequiresRedraw();
        }

        override public DisplayObject HitTest(Point localPoint)
        {
            if (!Visible || !Touchable)
            {
                return null;
            }

            // locations outside of the stage area shouldn't be accepted
            if (localPoint.X < 0 || localPoint.X > _width ||
                localPoint.Y < 0 || localPoint.Y > _height)
            {
                return null;
            }
            // if nothing else is hit, the stage returns itself as target
            DisplayObject target = base.HitTest(localPoint);
            if (target == null)
            {
                target = this;
            }
            return target;
        }

        /** Returns the stage bounds (i.e. not the bounds of its contents, but the rectangle
          *  spawned up by 'stageWidth' and 'stageHeight') in another coordinate system. */
        public Rectangle GetStageBounds(DisplayObject targetSpace)
        {
            Rectangle outR = Rectangle.Create(0, 0, _width, _height);

            Matrix2D sMatrix = GetTransformationMatrix(targetSpace);

            return outR.GetBounds(sMatrix);
        }

        // camera positioning

        /** Returns the position of the camera within the local coordinate system of a certain
         *  display object. If you do not pass a space, the method returns the global position.
         *  To change the position of the camera, you can modify the properties 'fieldOfView',
         *  'focalDistance' and 'projectionOffset'.
         */
        public float[] GetCameraPosition(DisplayObject space = null)
        {
            Matrix3D m = GetTransformationMatrix3D(space);

            return m.TransformCoords3D(
                _width / 2 + _projectionOffset.X, _height / 2 + _projectionOffset.Y,
                -FocalLength);
        }


        internal void AdvanceTime(float passedTime)
        {
            BroadcastEnterFrameEvent(passedTime);
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float Width
        {
            set { throw new InvalidOperationException("cannot set width of stage. Use StageWidth instead."); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float Height
        {
            set { throw new InvalidOperationException("cannot set height of stage. Use StageHeight instead."); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float X
        {
            set { throw new InvalidOperationException("cannot set x-coordinate of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float Y
        {
            set { throw new InvalidOperationException("cannot set y-coordinate of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float ScaleX
        {
            set { throw new InvalidOperationException("cannot scale stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float ScaleY
        {
            set { throw new InvalidOperationException("cannot scale stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float Rotation
        {
            set { throw new InvalidOperationException("cannot set rotation of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float SkewX
        {
            set { throw new InvalidOperationException("cannot skew stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float SkewY
        {
            set { throw new InvalidOperationException("cannot skew stage"); }
        }

        public override FragmentFilter Filter
        {
            get { throw new InvalidOperationException("Cannot add filter to stage. Add it to 'root' instead!"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float PivotX
        {
            set { throw new InvalidOperationException("cannot set PivotX of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float PivotY
        {
            set { throw new InvalidOperationException("cannot set PivotY of stage"); }
        }

        /// <summary>
        /// The height of the stage's coordinate system.
        /// Changing Stage size does not affect the size of the rendered area. By default its the same as SparrowSharp.ViewPort.Width,
        /// in this case 1 unit in the Stage equals 1 pixel.
        /// </summary>
        public float StageWidth {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// The width of the stage's coordinate system.
        /// Changing Stage size does not affect the size of the rendered area. By default its the same as SparrowSharp.ViewPort.Height,
        /// in this case 1 unit in the Stage equals 1 pixel.
        /// </summary>
        public float StageHeight {
            get { return _height; }
            set { _height = value; }
        }

        /** The distance between the stage and the camera. Changing this value will update the
         *  field of view accordingly. */
        public float FocalLength
        {
            get { return StageWidth / (2f * (float)Math.Tan(FieldOfView / 2f)); }
            set { FieldOfView = 2 * (float)Math.Atan(StageWidth / (2f * value)); }
        }

        /** A vector that moves the camera away from its default position in the center of the
         *  stage. Use this property to change the center of projection, i.e. the vanishing
         *  point for 3D display objects. <p>CAUTION: not a copy, but the actual object!</p>
         */
        public Point ProjectionOffset {
            get { return _projectionOffset; }
            set { _projectionOffset.SetTo(value.X, value.Y); }
        }

        /** The global position of the camera. This property can only be used to find out the
         *  current position, but not to modify it. For that, use the 'projectionOffset',
         *  'fieldOfView' and 'focalLength' properties. If you need the camera position in
         *  a certain coordinate space, use 'getCameraPosition' instead.
         *
         *  <p>CAUTION: not a copy, but the actual object!</p>
         */
        public float[] CameraPosition
        {
            get { return GetCameraPosition(); }
        }
    }
}