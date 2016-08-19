using Sparrow.Core;
using Sparrow.Geom;
using System;

namespace Sparrow.Display
{
    /// <summary>
    /// A Stage is the root of the display tree. It represents the rendering area of the application.

    /// Sparrow will create the stage for you. The root object of your game will be the first child of
    /// the stage. You can access 'root' and 'stage' from any display object using the respective 
    /// properties. 

    /// The stage's 'Width' and 'Height' values define the coordinate system of your game. The color
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
        /// The drawable width in pixels. This is the size of the game window on PCs and the size of the app on mobile.
        /// </summary>
        public uint DrawableWidth { get; private set; }

        /// <summary>
        /// The drawable width in pixels. This is the size of the game window on PCs and the size of the app on mobile.
        /// </summary>
        public uint DrawableHeight { get; private set; }

        /// <summary>
        /// The height of the stage's coordinate system.
        /// Changing Stage size does not affect the size of the rendered area. If its the same as the DrawableWidht/DrawableHeight,
        /// 1 unit in the Stage equals 1 pixel.
        /// </summary>
        override public float Width { get; set; }

        /// <summary>
        /// The width of the stage's coordinate system.
        /// Changing Stage size does not affect the size of the rendered area. If its the same as the DrawableWidht/DrawableHeight,
        /// 1 unit in the Stage equals 1 pixel.
        /// </summary>
        override public float Height { get; set; }

        /// <summary>
        /// The background color of the stage. Default: black.
        /// </summary>
        public uint Color { get; set; }

        /// <summary>
        /// Initializes a stage with a certain size in points. Sparrow calls this automatically on startup.
        /// </summary>
        internal Stage(float width, float height)
        {
            Width = width;
            Height = height;
            DrawableWidth = (uint)width;
            DrawableHeight = (uint)height;
        }

        internal void SetDrawableArea(uint width, uint height)
        {
            DrawableWidth = width;
            DrawableHeight = height;
            if (OnResize != null)
            {
                OnResize(this);
            }
        }

        override public void Render(RenderSupport support)
        {
            support.Clear(Color, 1.0f);
            support.SetupOrthographicProjection(0, Width, 0, Height);

            base.Render(support);
        }

        override public DisplayObject HitTestPoint(Point localPoint)
        {
            if (!Visible || !Touchable)
            {
                return null;
            }
            // if nothing else is hit, the stage returns itself as target
            DisplayObject target = base.HitTestPoint(localPoint);
            if (target == null)
            {
                target = this;
            }
            return target;
        }

        internal void AdvanceTime(float passedTime)
        {
            BroadcastEnterFrameEvent(passedTime);
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float X
        {
            set { throw new Exception("cannot set x-coordinate of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float Y
        {
            set { throw new Exception("cannot set y-coordinate of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float ScaleX
        {
            set { throw new Exception("cannot scale stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float ScaleY
        {
            set { throw new Exception("cannot scale stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float SkewX
        {
            set { throw new Exception("cannot skew stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float SkewY
        {
            set { throw new Exception("cannot skew stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float PivotX
        {
            set { throw new Exception("cannot set PivotX of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float PivotY
        {
            set { throw new Exception("cannot set PivotY of stage"); }
        }

        /// <summary>
        /// Cannot be set on the Stage, trying to set it will throw an exception
        /// </summary>
        override public float Rotation
        {
            set { throw new Exception("cannot set rotation of stage"); }
        }
    }
}