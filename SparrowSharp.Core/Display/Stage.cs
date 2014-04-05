using Sparrow.Core;
using Sparrow.Geom;
using System;

namespace Sparrow.Display
{
    public class Stage : DisplayObjectContainer
    {
        private float _width;
        private float _height;
        private uint _color;

        override public float Width
        {
            get { return _width; }
            set { _width = value; }
        }

        override public float Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public uint Color
        { 
            get { return _color; }
            set { _color = value; }
        }

        public Stage(float initWidth, float initHeight)
        {
            _width = initWidth;
            _width = initHeight;
        }

        override public void Render(RenderSupport support)
        {
			support.Clear(_color, 1.0f);
			support.SetupOrthographicProjection(0, _width, 0, _height);

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