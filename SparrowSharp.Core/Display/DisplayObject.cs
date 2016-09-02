using System;
using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Touches;
using Sparrow.Utils;
using SparrowSharp.Core;
using SparrowSharp.Filters;
using SparrowSharp.Core.Rendering;

namespace Sparrow.Display
{
    /// <summary>
    /// The DisplayObject class is the base class for all objects that are rendered on the screen.
    /// 
    /// In Sparrow, all displayable objects are organized in a display tree. Only objects that are part of
    /// the display tree will be displayed (rendered). 
    /// 
    /// The display tree consists of leaf nodes (Image, Quad) that will be rendered directly to
    /// the screen, and of container nodes (subclasses of DisplayObjectContainer, like Sprite).
    /// A container is simply a display object that has child nodes - which can, again, be either leaf
    /// nodes or other containers. 
    /// 
    /// A display object has properties that define its position in relation to its parent
    /// ('X', 'Y'), as well as its rotation, skewing and scaling factors. Use the 
    /// 'Alpha' and 'Visible' properties to make an object translucent or invisible.
    /// 
    /// Every display object may be the target of touch events. If you don't want an object to be
    /// touchable, you can disable the `Touchable` property. When it's disabled, neither the object
    /// nor its children will receive any more touch events.
    /// 
    /// **Points vs. Pixels**
    /// 
    /// All sizes and distances are measured in points. What this means in pixels depends on the 
    /// contentScaleFactor of the device.
    /// 
    /// **Transforming coordinates**
    /// 
    /// Within the display tree, each object has its own local coordinate system. If you rotate a container,
    /// you rotate that coordinate system - and thus all the children of the container.
    /// 
    /// Sometimes you need to know where a certain point lies relative to another coordinate system. 
    /// That's the purpose of the method 'TransformationMatrixToSpace'. It will create a matrix that
    /// represents the transformation of a point in one coordinate system to another. 
    /// 
    /// **Subclassing DisplayObject**
    /// 
    /// As DisplayObject is an abstract class, you can't instantiate it directly, but have to use one of 
    /// its subclasses instead.
    /// However, you can create custom display objects as well. That's especially useful when you want to
    /// create an object with a custom render function.
    /// 
    /// You will need to implement the following methods when you subclass DisplayObject:
    /// 
    /// - void Render ( Painter support);
    /// - Rectangle BoundsInSpace ( DisplayObject targetSpace);
    /// 
    /// Have a look at Quad for a sample implementation of those methods.  
    /// </summary>
    public abstract class DisplayObject
    {
        #region Events

        public delegate void EventHandler(DisplayObject target, DisplayObject currentTarget);

        public delegate void EnterFrameEventHandler(DisplayObject target, float passedTime);

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
        
        private float _x;
        private float _y;
        private float _pivotX;
        private float _pivotY;
        private float _scaleX;
        private float _scaleY;
        private float _skewX;
        private float _skewY;
        private float _rotation;
        protected float _alpha;
        private bool _visible;
        private bool _touchable;
        private uint _blendmode;
        private string _name;
        //private bool _useHandCursor; // not implemented
        private readonly Matrix _transformationMatrix;
        //private Matrix3D _transformationMatrix3D;
        private bool _orientationChanged;
        //private bool _is3D;
        private bool _isMask;

        // internal members (for fast access on rendering)
        internal DisplayObjectContainer _parent;
        internal uint _lastParentOrSelfChangeFrameID;
        internal uint _lastChildChangeFrameID;
        internal uint _tokenFrameID;
        internal BatchToken _pushToken = new BatchToken();
        internal BatchToken _popToken = new BatchToken();
        internal bool _hasVisibleArea;
      //TODO  internal FragmentFilter _filter;
        internal DisplayObject _mask;

        private double _lastTouchTimestamp;

        // helper objects
        private static List<DisplayObject> sAncestors = new List<DisplayObject>();
        private static Point sHelperPoint = Point.Create();
        //private static var sHelperPoint3D:Vector3D = new Vector3D();
        //private static var sHelperPointAlt3D:Vector3D = new Vector3D();
        private static Rectangle sHelperRect = new Rectangle();
        private static Matrix sHelperMatrix = Matrix.Create();
        private static Matrix sHelperMatrixAlt = Matrix.Create();
        //private static var sHelperMatrix3D:Matrix3D  = new Matrix3D();
        //private static var sHelperMatrixAlt3D:Matrix3D  = new Matrix3D();

        protected DisplayObject()
        {
            _x = _y = _pivotX = _pivotY = _rotation = _skewX = _skewY = 0.0f;
            _scaleX = _scaleY = _alpha = 1.0f;
            _visible = _touchable = _hasVisibleArea = true;
            BlendMode = Display.BlendMode.AUTO;
            _transformationMatrix = Matrix.Create();
        }

        /// <summary>
        /// Disposes all resources of the display object. 
        /// GPU buffers are released, event listeners are removed, filters and masks are disposed.
        /// </summary>
        virtual public void Dispose()
        {
      //TODO      if (_filter != null) _filter.Dispose();
            if (_mask != null) _mask.Dispose();
            //RemoveEventListeners();
            Mask = null; // revert 'isMask' property, just to be sure.
        }

        /// <summary>
        /// Removes the object from its parent, if it has one.
        /// </summary>
        public void RemoveFromParent(bool dispose = false)
        {
            if (Parent != null)
            {
                Parent.RemoveChild(this, dispose);
            }
            else if (dispose)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Creates a matrix that represents the transformation from the local coordinate system to another.
        /// </summary>
        public Matrix GetTransformationMatrix(DisplayObject targetSpace)
        {
            DisplayObject commonParent = null;
            DisplayObject currentObject;

            Matrix outMatrix = Matrix.Create();
            outMatrix.Identity();

            if (targetSpace == this)
            {
                return outMatrix;
            }
            else if (targetSpace == _parent || (targetSpace == null && _parent == null))
            {
                outMatrix.CopyFromMatrix(TransformationMatrix);
                return outMatrix;
            }
            else if (targetSpace == null || targetSpace == Base)
            {
                // targetSpace 'null' represents the target coordinate of the base object.
                // -> move up from this to base
                currentObject = this;
                while (currentObject != targetSpace)
                {
                    outMatrix.AppendMatrix(currentObject.TransformationMatrix);
                    currentObject = currentObject._parent;
                }
                return outMatrix;
            }
            else if (targetSpace.Parent == this)
            {
                outMatrix = targetSpace.GetTransformationMatrix(this);
                outMatrix.Invert();
                return outMatrix;
            }

            // 1.: Find a common parent of this and the target coordinate space.

            commonParent = FindCommonParent(this, targetSpace);

            // 2.: Move up from this to common parent

            currentObject = this;
            while (currentObject != commonParent)
            {
                outMatrix.AppendMatrix(currentObject.TransformationMatrix);
                currentObject = currentObject._parent;
            }

            if (commonParent == targetSpace)
            {
                return outMatrix;
            }

            // 3.: Now move up from target until we reach the common parent

            sHelperMatrix.Identity();
            currentObject = targetSpace;
            while (currentObject != commonParent)
            {
                sHelperMatrix.AppendMatrix(currentObject.TransformationMatrix);
                currentObject = currentObject._parent;
            }

            // 4.: Combine the two matrices

            sHelperMatrix.Invert();
            outMatrix.AppendMatrix(sHelperMatrix);

            return outMatrix;
        }

        /// <summary>
        /// Returns a rectangle that completely encloses the object as it appears in another coordinate system.
        /// </summary>
        public abstract Rectangle GetBounds(DisplayObject targetSpace);

        /// <summary>
        ///  Returns the object that is found topmost on a point in local coordinates, or null if the test fails.
        /// </summary>
        virtual public DisplayObject HitTest(Point localPoint)
        {
            // TODO its kinda stupid that this functions fails if the object is not touchable
            // invisible or untouchable objects cause the test to fail
            if (!_visible || !_touchable)
            {
                return null;
            }

            // if we've got a mask and the hit occurs outside, fail
            if (_mask != null && !HitTestMask(localPoint)) return null;

            // otherwise, check bounding box
            if (GetBounds(this).Contains(localPoint))
            {
                return this;
            }
            return null;
        }

        /// <summary>
        /// Checks if a certain point is inside the display object's mask. If there is no mask,
        /// this method always returns <code>true</code> (because having no mask is equivalent
        /// to having one that's infinitely big).
        /// </summary>
        public bool HitTestMask(Point localPoint)
        {
            if (_mask != null)
            {
                if (_mask.Stage != null)
                {
                    sHelperMatrixAlt = GetTransformationMatrix(_mask);
                }
                else
                {
                    sHelperMatrixAlt.CopyFromMatrix(_mask.TransformationMatrix);
                    sHelperMatrixAlt.Invert();
                }

                Point helperPoint = localPoint == sHelperPoint ? Point.Create() : sHelperPoint;
                helperPoint = sHelperMatrixAlt.TransformPoint(localPoint);
                return _mask.HitTest(helperPoint) != null;
            }
            else return true;
        }

        /// <summary>
        /// Transforms a point from the local coordinate system to global (stage) coordinates.
        /// </summary>
        public Point LocalToGlobal(Point localPoint)
        {
            Matrix matrix = GetTransformationMatrix(Base);
            return matrix.TransformPoint(localPoint);
        }

        /// <summary>
        /// Transforms a point from global (stage) coordinates to the local coordinate system.
        /// </summary>
        public Point GlobalToLocal(Point globalPoint)
        {
            Matrix matrix = GetTransformationMatrix(Base);
            matrix.Invert();
            return matrix.TransformPoint(globalPoint);
        }

        /// <summary>
        /// Renders the display object with the help of a support object. 
        /// </summary>
        public abstract void Render(Painter painter);

        /// <summary>
        /// Moves the pivot point to a certain position within the local coordinate system of the object.
        /// </summary>
        public void AlignPivot(HAlign hAlign, VAlign vAlign)
        {
            Rectangle bounds = GetBounds(this);
            SetOrientationChanged();

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
                    _pivotY = bounds.Y;
                    break;
                case VAlign.Center:
                    _pivotY = bounds.Y + bounds.Height / 2.0f;
                    break;
                case VAlign.Bottom:
                    _pivotY = bounds.Y + bounds.Height;
                    break;
                default:
                    throw new InvalidOperationException("invalid vertical alignment");
            }
        }

        internal bool IsMask { get { return _isMask; } }

        // render cache

        /** Forces the object to be redrawn in the next frame.
         *  This will prevent the object to be drawn from the render cache.
         *
         *  <p>This method is called every time the object changes in any way. When creating
         *  custom mesh styles or any other custom rendering code, call this method if the object
         *  needs to be redrawn.</p>
         *
         *  <p>If the object needs to be redrawn just because it does not support the render cache,
         *  call <code>painter.excludeFromCache()</code> in the object's render method instead.
         *  That way, Starling's <code>skipUnchangedFrames</code> policy won't be disrupted.</p>
         */

        public void SetRequiresRedraw()
        {
            DisplayObject parent = _parent;
            uint frameID = SparrowSharpApp.FrameID;

            _hasVisibleArea = _alpha != 0.0f && _visible && !_isMask && _scaleX != 0.0f && _scaleY != 0.0f;
            _lastParentOrSelfChangeFrameID = frameID;

            while (parent != null && parent._lastChildChangeFrameID != frameID)
            {
                parent._lastChildChangeFrameID = frameID;
                if (parent._mask != null) parent._mask.SetRequiresRedraw();
                parent = parent._parent;
            }

            if (_isMask) SparrowSharpApp.SetRequiresRedraw(); // notify 'skipUnchangedFrames'
            else if (_mask != null) _mask.SetRequiresRedraw();         // propagate into mask
        }

        public bool RequiresRedraw
        {
            get {
                uint frameID = SparrowSharpApp.FrameID;
                return _lastParentOrSelfChangeFrameID == frameID ||
                       _lastChildChangeFrameID == frameID;
            }
        }

        /** @private Makes sure the object is not drawn from cache in the next frame.
         *  This method is meant to be called only from <code>Painter.finishFrame()</code>,
         *  since it requires rendering to be concluded. */
        internal void ExcludeFromCache()
        {
            DisplayObject dObject = this;
            uint max = 0xffffffff;

            while (dObject != null && dObject._tokenFrameID != max)
            {
                dObject._tokenFrameID = max;
                dObject = dObject._parent;
            }
        }

        // helpers

        private void SetOrientationChanged()
        {
            _orientationChanged = true;
            SetRequiresRedraw();
        }

        private static DisplayObject FindCommonParent(DisplayObject object1,
                                                      DisplayObject object2)
        {
            DisplayObject currentObject = object1;

            while (currentObject != null)
            {
                sAncestors.Add(currentObject);
                currentObject = currentObject._parent;
            }

            currentObject = object2;
            while (currentObject != null && sAncestors.Contains(currentObject) == false)
            {
                currentObject = currentObject._parent;
            }

            sAncestors.Clear();

            if (currentObject != null) return currentObject;
            else throw new ArgumentException("Object not connected to target");
        }


        internal virtual void InvokeAdded(DisplayObject target, DisplayObject currentTarget)
        {
            if (Added != null)
            {
                Added(target, currentTarget);
            }
        }

        internal virtual void InvokeRemoved()
        {
            if (Removed != null)
            {
                Removed(this, this);
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
                // We need to make a copy here because the Children list might be modified in an AddedToStage event handler
                List<DisplayObject> copy = new List<DisplayObject>(displayObjectContainer.Children);
                foreach (var child in copy)
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
                // We need to make a copy here because the Children list might be modified in an RemovedFromStage event handler
                List<DisplayObject> copy = new List<DisplayObject>(displayObjectContainer.Children);
                foreach (var child in copy)
                {
                    child.BroadcastRemovedFromStageEvent(currentTarget);
                }
            }
        }

        // TODO this is optimized in Sparrow-s; it maintains an array of things on the Stage
        internal void BroadcastEnterFrameEvent(float passedTime)
        {
            if (EnterFrame != null)
            {
                EnterFrame(this, passedTime);
            }
            var displayObjectContainer = this as DisplayObjectContainer;
            if (displayObjectContainer != null)
            {
                // We need to make a copy here because the Children list might be modified in an EnterFrame event handler
                List<DisplayObject> copy = new List<DisplayObject>(displayObjectContainer.Children);
                foreach (var child in copy)
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
                            // inline for fast calculation
                            float sin = MathUtil.SinLUT[(int)(_rotation * 325.94932345220164765467394738691f + 0.5f) & 2047];
                            float cos = MathUtil.CosLUT[(int)(_rotation * 325.94932345220164765467394738691f + 0.5f) & 2047];

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

                _skewX = (value.D == 0.0f) ?
                    MathUtil.PIHALF * Math.Sign(-value.C)
                    : (float)Math.Atan(-value.C / value.D);
                _skewY = (value.A == 0.0f) ?
                    MathUtil.PIHALF * Math.Sign(value.B)
                    : (float)Math.Atan(value.B / value.A);

                _scaleY = (_skewX > -MathUtil.PIQUARTER && _skewX < MathUtil.PIQUARTER) ?
                    value.D / MathUtil.FastCos(_skewX)
                    : -value.C / MathUtil.FastSin(_skewX);
                _scaleX = (_skewY > -MathUtil.PIQUARTER && _skewY < MathUtil.PIQUARTER) ?
                    value.A / MathUtil.FastCos(_skewY)
                    : value.B / MathUtil.FastSin(_skewY);

                if (MathUtil.Equals(_skewX, _skewY))
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
        /// The bounds of the object relative to the local coordinates of the parent.
        /// </summary>
        public Rectangle Bounds
        {
            get { return GetBounds(Parent); }
        }

        /// <summary>
        /// The width of the object in points.
        /// </summary>
        virtual public float Width
        {
            get { return GetBounds(_parent).Width; }
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
            get { return GetBounds(_parent).Height; }
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                    SetOrientationChanged();
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
                value = value % (float)(2.0f * Math.PI);
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
                SetOrientationChanged();
            }
        }

        /** @private Indicates if the object is rotated or skewed in any way. */
        internal bool IsRotated
        {
            get { return _rotation != 0.0 || _skewX != 0.0 || _skewY != 0.0; }
        }

    /// <summary>
    /// The opacity of the object. 0 = transparent, 1 = opaque.
    /// </summary>
    virtual public float Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = MathUtil.Clamp(value, 0.0f, 1.0f);
                SetRequiresRedraw();
            }
        }

        /// <summary>
        /// The visibility of the object. An invisible object will be untouchable.
        /// </summary>
        virtual public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                SetRequiresRedraw();
            }
        }
        /// <summary>
        /// Indicates if this object (and its children) will receive touch events.
        /// </summary>
        virtual public bool Touchable
        {
            get { return _touchable; }
            set { _touchable = value; }
        }
        /// <summary>
        ///  The blend mode determines how the object is blended with the objects underneath. Default: BlendMode.AUTO
        /// </summary>
        virtual public uint BlendMode
        {
            get { return _blendmode; }
            set {
                _blendmode = value;
                SetRequiresRedraw();
            }
        }

        /// <summary>
        /// The name of the display object (default: null). Used by 'GetChild(string name)' of display object containers.
        /// </summary>
        virtual public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /** The filter that is attached to the display object. The <code>starling.filters</code>
        *  package contains several classes that define specific filters you can use. To combine
        *  several filters, assign an instance of the <code>FilterChain</code> class; to remove
        *  all filters, assign <code>null</code>.
        *
        *  <p>Beware that a filter instance may only be used on one object at a time! Furthermore,
        *  when you remove or replace a filter, it is NOT disposed automatically (since you might
        *  want to reuse it on a different object).</p>
        *
        *  @default null
        *  @see starling.filters.FragmentFilter
        *  @see starling.filters.FilterChain
        */
        public FragmentFilter Filter { 
            get { return null;/*_filter;*/ }
            set
            {
                // TODO
                /*if (value != _filter)
                {
                    if (_filter != null) _filter.SetTarget(null);
                    if (value != null) value.SetTarget(this);

                    _filter = value;
                    SetRequiresRedraw();
                }*/
            }
        }

        /** The display object that acts as a mask for the current object.
         *  Assign <code>null</code> to remove it.
         *
         *  <p>A pixel of the masked display object will only be drawn if it is within one of the
         *  mask's polygons. Texture pixels and alpha values of the mask are not taken into
         *  account. The mask object itself is never visible.</p>
         *
         *  <p>If the mask is part of the display list, masking will occur at exactly the
         *  location it occupies on the stage. If it is not, the mask will be placed in the local
         *  coordinate system of the target object (as if it was one of its children).</p>
         *
         *  <p>For rectangular masks, you can use simple quads; for other forms (like circles
         *  or arbitrary shapes) it is recommended to use a 'Canvas' instance.</p>
         *
         *  <p>Beware that a mask will typically cause at least two additional draw calls:
         *  one to draw the mask to the stencil buffer and one to erase it. However, if the
         *  mask object is an instance of <code>starling.display.Quad</code> and is aligned
         *  parallel to the stage axes, rendering will be optimized: instead of using the
         *  stencil buffer, the object will be clipped using the scissor rectangle. That's
         *  faster and reduces the number of draw calls, so make use of this when possible.</p>
         *
         *  @see Canvas
         *  @default null
         */
        public DisplayObject Mask
        {
            get { return _mask; }
            set
            {
                if (_mask != value)
                {
                    if (_mask != null) _mask._isMask = false;
                    if (value != null)
                    {
                        value._isMask = true;
                        value._hasVisibleArea = false;
                    }

                    _mask = value;
                    SetRequiresRedraw();
                }
            }
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

        /// <summary>
        /// The topmost object in the display tree the object is part of.
        /// </summary>
        public DisplayObject Base
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject._parent != null)
                {
                    currentObject = currentObject.Parent;
                }
                return currentObject;
            }
        }

        /// <summary>
        /// The root object the display object is connected to (i.e. an instance of the class 
        /// that was passed to the Starling constructor), or null if the object is not connected
        /// to the stage.
        /// </summary>
        public DisplayObject Root
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject._parent != null)
                {
                    DisplayObject currParent = currentObject._parent;
                    if (currParent.GetType() == typeof(Stage) ) return currentObject;
                    else currentObject = currentObject._parent;
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
            get{ return Base as Stage; }
        }
}
}
 