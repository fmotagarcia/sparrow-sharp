using System;
using System.Collections.Generic;
using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Touches;
using Sparrow.Utils;
using SparrowSharp.Core;
using SparrowSharp.Filters;

namespace Sparrow.Display
{
    /// <summary>
    /// The DisplayObject class is the base class for all objects that are rendered on the screen.
 
    /// In Sparrow, all displayable objects are organized in a display tree. Only objects that are part of
    /// the display tree will be displayed (rendered). 
 
    /// The display tree consists of leaf nodes (Image, Quad) that will be rendered directly to
    /// the screen, and of container nodes (subclasses of DisplayObjectContainer, like Sprite).
    /// A container is simply a display object that has child nodes - which can, again, be either leaf
    /// nodes or other containers. 
 
    /// A display object has properties that define its position in relation to its parent
    /// ('X', 'Y'), as well as its rotation, skewing and scaling factors ('scaleX', 'scaleY'). Use the 
    /// 'Alpha' and 'Visible' properties to make an object translucent or invisible.
 
    /// Every display object may be the target of touch events. If you don't want an object to be
    /// touchable, you can disable the `Touchable` property. When it's disabled, neither the object
    /// nor its children will receive any more touch events.
 
    /// **Points vs. Pixels**
 
    /// All sizes and distances are measured in points. What this means in pixels depends on the 
    /// contentScaleFactor of the device.
 
    /// **Transforming coordinates**
 
    /// Within the display tree, each object has its own local coordinate system. If you rotate a container,
    /// you rotate that coordinate system - and thus all the children of the container.
 
    /// Sometimes you need to know where a certain point lies relative to another coordinate system. 
    /// That's the purpose of the method 'TransformationMatrixToSpace:'. It will create a matrix that
    /// represents the transformation of a point in one coordinate system to another. 
 
    /// **Subclassing DisplayObject**
 
    /// As DisplayObject is an abstract class, you can't instantiate it directly, but have to use one of 
    /// its subclasses instead. There are already a lot of them available, and most of the time they will
    /// suffice. 
 
    /// However, you can create custom display objects as well. That's especially useful when you want to
    /// create an object with a custom render function.
 
    /// You will need to implement the following methods when you subclass DisplayObject:
 
    /// - void Render ( RenderSupport support);
    /// - Rectangle BoundsInSpace ( DisplayObject targetSpace);

    /// Have a look at Quad for a sample implementation of those methods.  
    /// </summary>
    public abstract class DisplayObject
    {
        #region Events

        public delegate void EventHandler(DisplayObject target,DisplayObject currentTarget);

        public delegate void EnterFrameEventHandler(DisplayObject target,float passedTime);

        public delegate void TouchHandler(TouchEvent touch);

        public event EventHandler Added;
        public event EventHandler AddedToStage;
        public event EventHandler Removed;
        public event EventHandler RemovedFromStage;
        public event EnterFrameEventHandler EnterFrame;
        public event TouchHandler Touch;
        public event EventHandler KeyUp;
        public event EventHandler KeyDown;

        #endregion

        private const int MAX_DISPLAY_TREE_DEPTH = 32;
        private float _x;
        private float _y;
        private float _pivotX;
        private float _pivotY;
        private float _scaleX;
        private float _scaleY;
        private float _skewX;
        private float _skewY;
        private float _rotation;
        private float _alpha;
        private bool _orientationChanged;
        private DisplayObjectContainer _parent;
        private double _lastTouchTimestamp;
        private readonly Matrix _transformationMatrix;
        /// <summary>
        ///  The blend mode determines how the object is blended with the objects underneath. Default: BlendMode.AUTO
        /// </summary>
        public uint BlendMode;
        /// <summary>
        /// The visibility of the object. An invisible object will be untouchable.
        /// </summary>
        public bool Visible;
        /// <summary>
        /// Indicates if this object (and its children) will receive touch events.
        /// </summary>
        public bool Touchable;
        /// <summary>
        /// The name of the display object (default: null). Used by 'GetChild(string name)' of display object containers.
        /// </summary>
        public string Name;
        /// <summary>
        /// The filter that is attached to the display object. Beware that you should NOT use the same
        /// filter on more than one object (for performance reasons).
        /// </summary>
        public FragmentFilter Filter;

        /// <summary>
        /// The width of the object in points.
        /// </summary>
        virtual public float Width
        {
            get { return BoundsInSpace(_parent).Width; }
            set
            {
                ScaleX = 1.0f;
                float actualWidth = Width;
                if (actualWidth != 0.0f)
                {
                    ScaleX = value / actualWidth;
                }
            }
        }

        /// <summary>
        /// The height of the object in points.
        /// </summary>
        virtual public float Height
        {
            get { return BoundsInSpace(_parent).Height; }
            set
            {
                ScaleY = 1.0f;
                float actualHeight = Width;
                if (actualHeight != 0.0f)
                {
                    ScaleY = value / actualHeight;
                }
            }
        }

        /// <summary>
        /// The x coordinate of the object relative to the local coordinates of the parent.
        /// </summary>
        virtual public float X
        {
            get { return _x; }
            set
            {
                if (value != _x)
                {
                    _x = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The y coordinate of the object relative to the local coordinates of the parent.
        /// </summary>
        virtual public float Y
        {
            get { return _y; }
            set
            {
                if (value != _y)
                {
                    _y = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The horizontal scale factor. "1" means no scale, negative values flip the object.
        /// </summary>
        virtual public float ScaleX
        {
            get { return _scaleX; }
            set
            {
                if (value != _scaleX)
                {
                    _scaleX = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The vertical scale factor. "1" means no scale, negative values flip the object.
        /// </summary>
        virtual public float ScaleY
        {
            get { return _scaleY; }
            set
            {
                if (value != _scaleY)
                {
                    _scaleY = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The horizontal skew angle in radians.
        /// </summary>
        virtual public float SkewX
        {
            get { return _skewX; }
            set
            {
                if (value != _skewX)
                {
                    _skewX = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The vertical skew angle in radians.
        /// </summary>
        virtual public float SkewY
        {
            get { return _skewY; }
            set
            {
                if (value != _skewY)
                {
                    _skewY = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The x coordinate of the object's origin in its own coordinate space (default: 0).
        /// </summary>
        virtual public float PivotX
        {
            get { return _pivotX; }
            set
            {
                if (value != _pivotX)
                {
                    _pivotX = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The y coordinate of the object's origin in its own coordinate space (default: 0).
        /// </summary>
        virtual public float PivotY
        {
            get { return _pivotY; }
            set
            {
                if (value != _pivotY)
                {
                    _pivotY = value;
                    _orientationChanged = true;
                }
            }
        }

        /// <summary>
        /// The rotation of the object in radians. (In Sparrow, all angles are measured in radians.)
        /// </summary>
        virtual public float Rotation
        {
            get { return _rotation; }
            set
            {
                // move to equivalent value in range [0 deg, 360 deg] without a loop
                value = value % (2.0f * (float)Math.PI);
                // move to [-180 deg, +180 deg]
                if (value < -Math.PI)
                {
                    value += 2.0f * (float)Math.PI;
                }
                else if (value > Math.PI)
                {
                    value -= 2.0f * (float)Math.PI;
                }
				
                _rotation = value;
                _orientationChanged = true;
            }
        }

        /// <summary>
        /// The opacity of the object. 0 = transparent, 1 = opaque.
        /// </summary>
        virtual public float Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = NumberUtil.Clamp(value, 0.0f, 1.0f);
            }
        }

        /// <summary>
        /// The topmost object in the display tree the object is part of.
        /// </summary>
        public DisplayObject Base
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject.Parent != null)
                {
                    currentObject = currentObject.Parent;
                }
                return currentObject;
            }
        }

        /// <summary>
        /// The root object the display object is connected to (i.e. an instance of the class
        /// that was passed to 'SparrowSharpApp.Start()'), or null if the object is not connected
        /// to it.
        /// </summary>
        public DisplayObject Root
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject.Parent != null)
                {
                    if (currentObject.Parent is Stage)
                    {
                        return currentObject;
                    }
                    currentObject = currentObject.Parent;
                }
                return null;
            }
        }

        /// <summary>
        /// The Stage the display object is connected to, or null if it is not connected to a Stage.
        /// </summary>
        public Stage Stage
        {
            get
            {
                if (Base is Stage)
                {
                    return (Stage)Base;
                }
                return null;
            }
        }

        /// <summary>
        /// The transformation matrix of the object relative to its parent.
        /// @returns CAUTION: not a copy, but the actual object!
        /// </summary>
        public Matrix TransformationMatrix
        {
            get
            {
                if (_orientationChanged)
                {
                    _orientationChanged = false;

                    if (_skewX == 0.0f && _skewY == 0.0f)
                    {
                        // optimization: no skewing / rotation simplifies the matrix math
                        if (_rotation == 0.0f)
                        {
                            _transformationMatrix.A = _scaleX;
                            _transformationMatrix.B = 0.0f;
                            _transformationMatrix.C = 0.0f;
                            _transformationMatrix.D = _scaleY;
                            _transformationMatrix.Tx = _x - _pivotX * _scaleX;
                            _transformationMatrix.Ty = _y - _pivotY * _scaleY;
                        }
                        else
                        {

                            float sin = NumberUtil.SinLUT[(int)(_rotation * 325.94932345220164765467394738691f) & 2047];
                            float cos = NumberUtil.CosLUT[(int)(_rotation * 325.94932345220164765467394738691f) & 2047];

                            float a = _scaleX * cos;
                            float b = _scaleX * sin;
                            float c = _scaleY * -sin;
                            float d = _scaleY * cos;
                            float tx = _x - _pivotX * a - _pivotY * c;
                            float ty = _y - _pivotX * b - _pivotY * d;

                            _transformationMatrix.A = a;
                            _transformationMatrix.B = b;
                            _transformationMatrix.C = c;
                            _transformationMatrix.D = d;
                            _transformationMatrix.Tx = tx;
                            _transformationMatrix.Ty = ty;
                        }
                    }
                    else
                    {
                        _transformationMatrix.Identity();
                        _transformationMatrix.Scale(_scaleX, _scaleY);
                        _transformationMatrix.Skew(_skewX, _skewY);
                        _transformationMatrix.Rotate(_rotation);
                        _transformationMatrix.Translate(_x, _y);

                        if (_pivotX != 0.0f || _pivotY != 0.0f)
                        {
                            // prepend pivot transformation
                            _transformationMatrix.Tx = _x - _transformationMatrix.A * _pivotX
                            - _transformationMatrix.C * _pivotY;
                            _transformationMatrix.Ty = _y - _transformationMatrix.B * _pivotX
                            - _transformationMatrix.D * _pivotY;
                        }
                    }
                }

                return _transformationMatrix;
            }

            set
            {
                _orientationChanged = false;
                _transformationMatrix.CopyFromMatrix(value);


                _pivotX = 0.0f;
                _pivotY = 0.0f;

                _x = value.Tx;
                _y = value.Ty;

                _skewX = (float)Math.Atan(-value.C / value.D);
                _skewY = (float)Math.Atan(value.B / value.A);

                _scaleX = value.A / (float)Math.Cos(_skewY);
                _scaleY = value.D / (float)Math.Cos(_skewX);

                if (NumberUtil.Equals(_skewX, _skewY))
                {
                    _rotation = _skewX;
                    _skewX = _skewY = 0.0f;
                }
                else
                {
                    _rotation = 0.0f;
                }
            }
        }

        /// <summary>
        /// Indicates if an object occupies any visible area. (Which is the case when its 'Alpha',
        /// 'ScaleX' and 'ScaleY' values are not zero, and its 'Visible' property is true.)
        /// </summary>
        public bool HasVisibleArea
        {
            get
            {
                return _alpha != 0.0f && Visible && _scaleX != 0.0f && _scaleY != 0.0f;
            }
        }

        /// <summary>
        /// The bounds of the object relative to the local coordinates of the parent.
        /// </summary>
        public Rectangle Bounds
        {
            get { return BoundsInSpace(Parent); }
        }

        /// <summary>
        /// The DisplayObjectContainer that contains this display object.
        /// </summary>
        public DisplayObjectContainer Parent
        {
            get { return _parent; }
            internal set
            {
                DisplayObject ancestor = value;
                while (ancestor != this && ancestor != null)
                {
                    ancestor = ancestor.Parent;
                }
                if (ancestor == this)
                {
                    throw new InvalidOperationException("An object cannot be added as a child to itself or one of its children");
                }
                _parent = value;
            }	
        }

        protected DisplayObject()
        {
            _alpha = 1.0f;
            _scaleX = 1.0f;
            _scaleY = 1.0f;
            Visible = true;
            Touchable = true;
            _transformationMatrix = Matrix.Create();
            _orientationChanged = false;
            BlendMode = Sparrow.Display.BlendMode.AUTO;
        }

        /// <summary>
        /// Renders the display object with the help of a support object. 
        /// </summary>
        public abstract void Render(RenderSupport support);

        /// <summary>
        /// Removes the object from its parent, if it has one.
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent != null)
            {
                Parent.RemoveChild(this);
            }
        }

        /// <summary>
        /// Moves the pivot point to the center of the object.
        /// </summary>
        public void AlignPivotToCenter()
        {
            Align(HAlign.Center, VAlign.Center);
        }

        /// <summary>
        /// Moves the pivot point to a certain position within the local coordinate system of the object.
        /// </summary>
        public void Align(HAlign hAlign, VAlign vAlign)
        {
            Rectangle bounds = BoundsInSpace(this);
            _orientationChanged = true;

            switch (hAlign)
            {
                case HAlign.Left:
                    _pivotX = bounds.X;
                    break;
                case HAlign.Center:
                    _pivotX = bounds.X + bounds.Width / 2.0f;
                    break;
                case HAlign.Right:
                    _pivotX = bounds.X + bounds.Width; 
                    break;
                default:
                    throw new InvalidOperationException("invalid horizontal alignment");
            }

            switch (vAlign)
            {
                case VAlign.Top:
                    _pivotY = bounds.Top;  
                    break;
                case VAlign.Center:
                    _pivotY = bounds.Top + bounds.Height / 2.0f; 
                    break;
                case VAlign.Bottom:
                    _pivotY = bounds.Top + bounds.Height; 
                    break;
                default:
                    throw new InvalidOperationException("invalid vertical alignment");
            }
        }

        /// <summary>
        /// Creates a matrix that represents the transformation from the local coordinate system to another.
        /// </summary>
        public Matrix TransformationMatrixToSpace(DisplayObject targetSpace)
        {     
            DisplayObject currentObject;
            Matrix selfMatrix;
            Matrix targetMatrix;

            if (targetSpace == this)
            {
                Matrix identity = Matrix.Create();
                identity.Identity();

                return identity;
            }
            else if (targetSpace == _parent || (targetSpace == null && _parent == null))
            {
                Matrix transformationMatrix = Matrix.Create();
                transformationMatrix.CopyFromMatrix(TransformationMatrix);
                return transformationMatrix;
            }
            else if (targetSpace == null || targetSpace == Base)
            {
                // targetSpace 'null' represents the target coordinate of the base object.
                // -> move up from this to base
                selfMatrix = Matrix.Create();
                selfMatrix.Identity();
                currentObject = this;
                while (currentObject != targetSpace)
                {
                    selfMatrix.AppendMatrix(currentObject.TransformationMatrix);
                    currentObject = currentObject.Parent;
                }        
                return selfMatrix; 
            }
            else if (targetSpace.Parent == this)
            {
                targetMatrix = Matrix.Create();
                targetMatrix.CopyFromMatrix(targetSpace.TransformationMatrix);
                targetMatrix.Invert();
                return targetMatrix;
            }

            // 1.: Find a common parent of this and the target coordinate space.
            List<DisplayObject> ancestors = new List<DisplayObject>();

            int count = 0;
            DisplayObject commonParent = null;
            currentObject = this;
            while (currentObject != null && count < MAX_DISPLAY_TREE_DEPTH)
            {
                ancestors.Add(currentObject);
                currentObject = currentObject.Parent;
                count++;
            }

            currentObject = targetSpace;    
            while (currentObject != null && commonParent == null)
            {        
                for (int i = 0; i < count; ++i)
                {
                    if (currentObject == ancestors[i])
                    {
                        commonParent = ancestors[i];
                        break;                
                    }            
                }
                currentObject = currentObject.Parent;
            }

            if (commonParent == null)
            {
                throw new Exception("Object not connected to target");
            }

            // 2.: Move up from this to common parent
            selfMatrix = Matrix.Create();
            selfMatrix.Identity();
            currentObject = this;    
            while (currentObject != commonParent)
            {
                selfMatrix.AppendMatrix(currentObject.TransformationMatrix);
                currentObject = currentObject.Parent;
            }

            // 3.: Now move up from target until we reach the common parent
            targetMatrix = Matrix.Create();
            targetMatrix.Identity();
            currentObject = targetSpace;
            while (currentObject != null && currentObject != commonParent)
            {
                targetMatrix.AppendMatrix(currentObject.TransformationMatrix);
                currentObject = currentObject.Parent;
            }    

            // 4.: Combine the two matrices
            targetMatrix.Invert();
            selfMatrix.AppendMatrix(targetMatrix);

            return selfMatrix;
        }

        /// <summary>
        /// Returns a rectangle that completely encloses the object as it appears in another coordinate system.
        /// </summary>
        public abstract Rectangle BoundsInSpace(DisplayObject targetSpace);

        /// <summary>
        ///  Returns the object that is found topmost on a point in local coordinates, or null if the test fails.
        /// </summary>
        virtual public DisplayObject HitTestPoint(Point localPoint)
        {
            // invisible or untouchable objects cause the test to fail
            if (!Visible || !Touchable)
            {
                return null;
            }

            // otherwise, check bounding box
            if (BoundsInSpace(this).Contains(localPoint))
            {
                return this; 
            }
            return null;
        }

        /// <summary>
        /// Transforms a point from the local coordinate system to global (stage) coordinates.
        /// </summary>
        public Point LocalToGlobal(Point localPoint)
        {
            Matrix matrix = TransformationMatrixToSpace(Base);
            return matrix.TransformPoint(localPoint);
        }

        /// <summary>
        /// Transforms a point from global (stage) coordinates to the local coordinate system.
        /// </summary>
        public Point GlobalToLocal(Point globalPoint)
        {
            Matrix matrix = TransformationMatrixToSpace(Base);
            return matrix.TransformPoint(globalPoint);
        }

        internal virtual void InvokeAdded(DisplayObject target, DisplayObject currentTarget)
        {
            if (Added != null)
            {
                Added(target, currentTarget);
            }
        }

        internal void BroadcastAddedToStageEvent(DisplayObjectContainer currentTarget)
        {
            if (AddedToStage != null)
            {
                AddedToStage(this, currentTarget);
            }
            var displayObjectContainer = this as DisplayObjectContainer;
            if (displayObjectContainer != null)
            {
                foreach (var child in displayObjectContainer.Children)
                {
                    child.BroadcastAddedToStageEvent(currentTarget);
                }
            }
        }

        internal void BroadcastRemovedFromStageEvent(DisplayObjectContainer currentTarget)
        {
            if (RemovedFromStage != null)
            {
                RemovedFromStage(this, currentTarget);
            }
            var displayObjectContainer = this as DisplayObjectContainer;
            if (displayObjectContainer != null)
            {
                foreach (var child in displayObjectContainer.Children)
                {
                    child.BroadcastRemovedFromStageEvent(currentTarget);
                }
            }
        }

        internal virtual void InvokeRemoved()
        {
            if (Removed != null)
            {
                Removed(this, this);
            }
        }
        // TODO this is optmized in Sparrow-s; it maintains an array of things on the Stage
        internal void BroadcastEnterFrameEvent(float passedTime)
        {
            if (EnterFrame != null)
            {
                EnterFrame(this, passedTime);
            }
            var displayObjectContainer = this as DisplayObjectContainer;
            if (displayObjectContainer != null)
            {
                foreach (var child in displayObjectContainer.Children)
                {
                    child.BroadcastEnterFrameEvent(passedTime);
                }
            }
        }

        internal virtual void InvokeTouch(TouchEvent touchEvent)
        {
            if (Touch != null)
            {
                if (touchEvent.Timestamp == _lastTouchTimestamp)
                {
                    return;
                } 
                _lastTouchTimestamp = touchEvent.Timestamp;
                Touch(touchEvent);
            }
        }

        internal virtual void InvokeKeyUp(DisplayObject target, DisplayObject currentTarget)
        {
            if (KeyUp != null)
            {
                KeyUp(target, currentTarget);
            }
        }

        internal virtual void InvokeKeyDown(DisplayObject target, DisplayObject currentTarget)
        {
            if (KeyDown != null)
            {
                KeyDown(target, currentTarget);
            }
        }
    }
}