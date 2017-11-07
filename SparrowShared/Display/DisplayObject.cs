using System;
using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Touches;
using Sparrow.Utils;
using Sparrow.Filters;
using Sparrow.Rendering;
using Sparrow.Core;

namespace Sparrow.Display
{
    /// <summary>
    /// <para>
    /// The DisplayObject class is the base class for all objects that are rendered on the screen.
    /// </para>
    /// <para>
    /// In Sparrow, all displayable objects are organized in a display tree. Only objects that are part of
    /// the display tree will be displayed (rendered). 
    /// </para>
    /// <para>
    /// The display tree consists of leaf nodes (Image, Quad) that will be rendered directly to
    /// the screen, and of container nodes (subclasses of DisplayObjectContainer, like Sprite).
    /// A container is simply a display object that has child nodes - which can, again, be either leaf
    /// nodes or other containers. 
    /// </para>
    /// A display object has properties that define its position in relation to its parent
    /// ('X', 'Y'), as well as its rotation, skewing and scaling factors. Use the 
    /// 'Alpha' and 'Visible' properties to make an object translucent or invisible.
    /// 
    /// Every display object may be the target of touch events. If you don't want an object to be
    /// touchable, you can disable the `Touchable` property. When it's disabled, neither the object
    /// nor its children will receive any more touch events.
    /// <para>
    /// **Points vs. Pixels**
    /// </para>
    /// All sizes and distances are measured in points. What this means in pixels depends on the 
    /// contentScaleFactor of the device.
    /// <para>
    /// **Transforming coordinates**
    /// </para>
    /// Within the display tree, each object has its own local coordinate system. If you rotate a container,
    /// you rotate that coordinate system - and thus all the children of the container.
    /// 
    /// Sometimes you need to know where a certain point lies relative to another coordinate system. 
    /// That's the purpose of the method 'TransformationMatrixToSpace'. It will create a matrix that
    /// represents the transformation of a point in one coordinate system to another. 
    /// <para>
    /// **Subclassing DisplayObject**
    /// </para>
    /// As DisplayObject is an abstract class, you can't instantiate it directly, but have to use one of 
    /// its subclasses instead.
    /// However, you can create custom display objects as well. That's especially useful when you want to
    /// create an object with a custom render function.
    /// 
    /// You will need to implement the following methods when you subclass DisplayObject:
    /// <code>
    /// Void Render ( Painter support);
    /// Rectangle BoundsInSpace ( DisplayObject targetSpace);
    /// </code>
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
        private bool _useHandCursor;
        private readonly Matrix2D _transformationMatrix;
        private bool _orientationChanged;
        private DisplayObject _maskee;

        // internal members (for fast access on rendering)
        internal DisplayObjectContainer _parent;
        internal uint _lastParentOrSelfChangeFrameID;
        internal uint _lastChildChangeFrameID;
        internal uint _tokenFrameID;
        internal BatchToken _pushToken = new BatchToken();
        internal BatchToken _popToken = new BatchToken();
        internal bool _hasVisibleArea;
        internal FragmentFilter _filter;
        internal DisplayObject _mask;

        private double _lastTouchTimestamp;

        // helper objects
        private static readonly List<DisplayObject> SAncestors = new List<DisplayObject>();
        private static Matrix2D _sHelperMatrixAlt = Matrix2D.Create();

        protected DisplayObject()
        {
            _x = _y = _pivotX = _pivotY = _rotation = _skewX = _skewY = 0.0f;
            _scaleX = _scaleY = _alpha = 1.0f;
            _visible = _touchable = _hasVisibleArea = true;
            BlendMode = Display.BlendMode.AUTO;
            _transformationMatrix = Matrix2D.Create();
        }

        /// <summary>
        /// Disposes all resources of the display object. 
        /// GPU buffers are released, event listeners are removed, filters and masks are disposed.
        /// </summary>
        public virtual void Dispose()
        {
            _filter?.Dispose();
            _mask?.Dispose();
            //TODO RemoveEventListeners();
            Mask = null; // clear 'mask._maskee', just to be sure.
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
        public Matrix2D GetTransformationMatrix(DisplayObject targetSpace)
        {
            DisplayObject commonParent = null;
            DisplayObject currentObject;

            Matrix2D outMatrix = Matrix2D.Create();
            outMatrix.Identity();

            if (targetSpace == this)
            {
                return outMatrix;
            }
            if (targetSpace == _parent || (targetSpace == null && _parent == null))
            {
                outMatrix.CopyFromMatrix(TransformationMatrix);
                return outMatrix;
            }
            if (targetSpace == null || targetSpace == Base)
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
            if (targetSpace.Parent == this)
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

            var sHelperMatrix = Matrix2D.Create();
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
        public virtual DisplayObject HitTest(Point localPoint)
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
            if (_mask == null) return true;
            if (_mask.Stage != null)
            {
                _sHelperMatrixAlt = GetTransformationMatrix(_mask);
            }
            else
            {
                _sHelperMatrixAlt.CopyFromMatrix(_mask.TransformationMatrix);
                _sHelperMatrixAlt.Invert();
            }
            
            var helperPoint = _sHelperMatrixAlt.TransformPoint(localPoint);
            return _mask.HitTest(helperPoint) != null;
        }

        /// <summary>
        /// Transforms a point from the local coordinate system to global (stage) coordinates.
        /// </summary>
        public Point LocalToGlobal(Point localPoint)
        {
            Matrix2D matrix = GetTransformationMatrix(Base);
            return matrix.TransformPoint(localPoint);
        }

        /// <summary>
        /// Transforms a point from global (stage) coordinates to the local coordinate system.
        /// </summary>
        public Point GlobalToLocal(Point globalPoint)
        {
            Matrix2D matrix = GetTransformationMatrix(Base);
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


        // 3D transformation

        /// <summary>
        /// Creates a matrix that represents the transformation from the local coordinate system
        /// to another. This method supports three dimensional objects created via 'Sprite3D'.
        /// </summary>
        public Matrix3D GetTransformationMatrix3D(DisplayObject targetSpace)
        {
            DisplayObject currentObject;

            Matrix3D outM = Matrix3D.Create();

            if (targetSpace == this)
            {
                return outM;
            }
            if (targetSpace == _parent || (targetSpace == null && _parent == null))
            {
                outM.CopyFrom(TransformationMatrix3D);
                return outM;
            }
            if (targetSpace == null || targetSpace == Base)
            {
                // targetCoordinateSpace 'null' represents the target space of the base object.
                // -> move up from this to base

                currentObject = this;
                while (currentObject != targetSpace)
                {
                    outM.Append(currentObject.TransformationMatrix3D);
                    currentObject = currentObject._parent;
                }
                return outM;
            }
            if (targetSpace._parent == this) // optimization
            {
                outM = targetSpace.GetTransformationMatrix3D(this);
                outM.Invert();
                return outM;
            }

            // 1. find a common parent of this and the target space

            var commonParent = FindCommonParent(this, targetSpace);

            // 2. move up from this to common parent

            currentObject = this;
            while (currentObject != commonParent)
            {
                outM.Append(currentObject.TransformationMatrix3D);
                currentObject = currentObject._parent;
            }

            if (commonParent == targetSpace)
                return outM;

            // 3. now move up from target until we reach the common parent

            var sHelperMatrix3D = Matrix3D.Create();
            currentObject = targetSpace;
            while (currentObject != commonParent)
            {
                sHelperMatrix3D.Append(currentObject.TransformationMatrix3D);
                currentObject = currentObject._parent;
            }

            // 4. now combine the two matrices

            sHelperMatrix3D.Invert();
            outM.Append(sHelperMatrix3D);

            return outM;
        }

        internal bool IsMask { get { return _maskee != null; } }

        // render cache

        /// <summary>
        /// Forces the object to be redrawn in the next frame.
        /// This will prevent the object to be drawn from the render cache.
        ///
        /// <para>This method is called every time the object changes in any way. When creating
        /// custom mesh styles or any other custom rendering code, call this method if the object
        /// needs to be redrawn.</para>
        ///
        /// <para>If the object needs to be redrawn just because it does not support the render cache,
        /// call <code>Painter.ExcludeFromCache()</code> in the object's render method instead.
        /// That way, Sparrow's <code>SkipUnchangedFrames</code> policy won't be disrupted.</para>
        /// </summary>
        public virtual void SetRequiresRedraw()
        {
            DisplayObject parent = _parent != null ? _parent : _maskee;
            uint frameId = SparrowSharp.FrameID;

            _lastParentOrSelfChangeFrameID = frameId;
            _hasVisibleArea = _alpha != 0.0f && _visible && _maskee == null && 
                              _scaleX != 0.0f && _scaleY != 0.0f;

            while (parent != null && parent._lastChildChangeFrameID != frameId)
            {
                parent._lastChildChangeFrameID = frameId;
                parent = parent._parent != null ? parent._parent : parent._maskee;
            }
        }

        public bool RequiresRedraw
        {
            get {
                uint frameId = SparrowSharp.FrameID;
                return _lastParentOrSelfChangeFrameID == frameId ||
                       _lastChildChangeFrameID == frameId;
            }
        }

        /// <summary>
        ///  Makes sure the object is not drawn from cache in the next frame.
        ///  This method is meant to be called only from <code>Painter.FinishFrame()</code>,
        ///  since it requires rendering to be concluded.
        /// </summary>
        internal void ExcludeFromCache()
        {
            DisplayObject dObject = this;
            const uint max = 0xffffffff;

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
                SAncestors.Add(currentObject);
                currentObject = currentObject._parent;
            }

            currentObject = object2;
            while (currentObject != null && SAncestors.Contains(currentObject) == false)
            {
                currentObject = currentObject._parent;
            }

            SAncestors.Clear();

            if (currentObject != null) return currentObject;
            throw new ArgumentException("Object not connected to target");
        }


        internal virtual void InvokeAdded(DisplayObject target, DisplayObject currentTarget)
        {
            Added?.Invoke(target, currentTarget);
        }

        internal virtual void InvokeRemoved()
        {
            Removed?.Invoke(this, this);
        }

        internal void BroadcastAddedToStageEvent(DisplayObjectContainer currentTarget)
        {
            AddedToStage?.Invoke(this, currentTarget);
            var displayObjectContainer = this as DisplayObjectContainer;
            if (displayObjectContainer == null) return;
            // We need to make a copy here because the Children list might be modified in an AddedToStage event handler
            List<DisplayObject> copy = new List<DisplayObject>(displayObjectContainer.Children);
            foreach (var child in copy)
            {
                child.BroadcastAddedToStageEvent(currentTarget);
            }
        }

        internal void BroadcastRemovedFromStageEvent(DisplayObjectContainer currentTarget)
        {
            RemovedFromStage?.Invoke(this, currentTarget);
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
        protected void BroadcastEnterFrameEvent(float passedTime)
        {
            EnterFrame?.Invoke(this, passedTime);
            var displayObjectContainer = this as DisplayObjectContainer;
            if (displayObjectContainer == null) return;
            // We need to make a copy here because the Children list might be modified in an EnterFrame event handler
            var copy = new List<DisplayObject>(displayObjectContainer.Children);
            foreach (var child in copy)
            {
                child.BroadcastEnterFrameEvent(passedTime);
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
            KeyUp?.Invoke(target, currentTarget);
        }

        internal virtual void InvokeKeyDown(DisplayObject target, DisplayObject currentTarget)
        {
            KeyDown?.Invoke(target, currentTarget);
        }


        /// <summary>
        /// The transformation matrix of the object relative to its parent.
        /// <returns>CAUTION: not a copy, but the actual object!</returns>
        /// </summary>
        public virtual Matrix2D TransformationMatrix
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
        /// The 3D transformation matrix of the object relative to its parent.
        ///
        /// <para>For 2D objects, this property returns just a 3D version of the 2D transformation
        /// matrix. Only the 'Sprite3D' class supports real 3D transformations.</para>
        ///
        /// <para>CAUTION: not a copy, but the actual object!</para>
        /// </summary>
        public virtual Matrix3D TransformationMatrix3D
        {
            get { return TransformationMatrix.ConvertToMatrix3D(); }
        }

        
        /// <summary>
        /// Indicates if the mouse cursor should transform into a hand while it's over the sprite.
        /// default false
        /// </summary>
        public virtual bool UseHandCursor {
            get { return _useHandCursor; }
            set
            {
                if (value == _useHandCursor) return;
                _useHandCursor = value;

                if (_useHandCursor)
                {
                    Touch += OnTouch;    
                }
                else
                {
                    Touch -= OnTouch;
                }                
            }
        }
        
        private void OnTouch(TouchEvent evt)
        {
            SparrowSharp.MouseCursor = evt.InteractsWith(this) ? MouseCursor.Hand : MouseCursor.Default;
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
        public virtual float Width
        {
            get { return GetBounds(_parent).Width; }
            set
            {
                float actualWidth;

                if (_scaleX == 0.0f || !_scaleX.IsFinite())
                {
                    ScaleX = 1.0f;
                    actualWidth = Width;
                }
                else actualWidth = Math.Abs(Width / _scaleX);

                if (actualWidth != 0.0f) ScaleX = value / actualWidth;
            }
        }

        /// <summary>
        /// The height of the object in points.
        /// </summary>
        public virtual float Height
        {
            get { return GetBounds(_parent).Height; }
            set
            {
                float actualHeight;

                if (_scaleY == 0.0f || !_scaleY.IsFinite())
                {
                    ScaleY = 1.0f;
                    actualHeight = Height;
                }
                else actualHeight = Math.Abs(Height / _scaleY);

                if (actualHeight != 0.0f) ScaleY = value / actualHeight;
            }
        }

        /// <summary>
        /// The x coordinate of the object relative to the local coordinates of the parent.
        /// </summary>
        public virtual float X
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
        public virtual float Y
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
        public virtual float PivotX
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
        public virtual float PivotY
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
        public virtual float ScaleX
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
        public virtual float ScaleY
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
        /// Sets both ScaleX and ScaleY. Returns ScaleX.
        /// </summary>
        public virtual float Scale
        {
            get { return ScaleX; }
            set { ScaleX = ScaleY = value; }
        }

        /// <summary>
        /// The horizontal skew angle in radians.
        /// </summary>
        public virtual float SkewX
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
        public virtual float SkewY
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
        public virtual float Rotation
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

        /// <summary>
        /// Indicates if the object is rotated or skewed in any way.
        /// </summary>
        internal bool IsRotated
        {
            get { return _rotation != 0.0 || _skewX != 0.0 || _skewY != 0.0; }
        }

        /// <summary>
        /// The opacity of the object. 0 = transparent, 1 = opaque.
        /// </summary>
        public virtual float Alpha
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
        public virtual bool Visible
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
        public virtual bool Touchable
        {
            get { return _touchable; }
            set { _touchable = value; }
        }
        /// <summary>
        ///  The blend mode determines how the object is blended with the objects underneath. Default: BlendMode.AUTO
        /// </summary>
        public virtual uint BlendMode
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
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The filter that is attached to the display object. The <code>Sparrow.Filters</code>
        /// package contains several classes that define specific filters you can use. To combine
        /// several filters, assign an instance of the <code>FilterChain</code> class; to remove
        /// all filters, assign <code>null</code>.
        ///
        /// <para>Beware that a filter instance may only be used on one object at a time! Furthermore,
        /// when you remove or replace a filter, it is NOT disposed automatically (since you might
        /// want to reuse it on a different object).</para>
        /// <see cref="FragmentFilter"/>
        /// </summary>
        public virtual FragmentFilter Filter { 
            get { return _filter; }
            set
            {
                if (value != _filter)
                {
                    _filter?.SetTarget(null);
                    value?.SetTarget(this);

                    _filter = value;
                    SetRequiresRedraw();
                }
            }
        }
        
        ///<summary>
        /// The display object that acts as a mask for the current object.
        /// Assign <code>null</code> to remove it.
        ///
        /// <para>A pixel of the masked display object will only be drawn if it is within one of the
        /// mask's polygons. Texture pixels and alpha values of the mask are not taken into
        /// account. The mask object itself is never visible.</para>
        ///
        /// <para>If the mask is part of the display list, masking will occur at exactly the
        /// location it occupies on the stage. If it is not, the mask will be placed in the local
        /// coordinate system of the target object (as if it was one of its children).</para>
        ///
        /// <para>For rectangular masks, you can use simple quads; for other forms (like circles
        /// or arbitrary shapes) it is recommended to use a 'Canvas' instance.</para>
        ///
        /// <para>Beware that a mask will typically cause at least two additional draw calls:
        /// one to draw the mask to the stencil buffer and one to erase it. However, if the
        /// mask object is an instance of <code>Sparrow.Display.Quad</code> and is aligned
        /// parallel to the stage axes, rendering will be optimized: instead of using the
        /// stencil buffer, the object will be clipped using the scissor rectangle. That's
        /// faster and reduces the number of draw calls, so make use of this when possible.</para> 
        /// </summary>
        public DisplayObject Mask
        {
            get { return _mask; }
            set
            {
                if (_mask != value)
                {
                    if (_mask != null)
                    {
                        _mask._maskee = null;
                    }
                    if (value != null)
                    {
                        value._maskee = this;
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
        /// that was passed to the Sparrow constructor), or null if the object is not connected
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
                    currentObject = currentObject._parent;
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
 