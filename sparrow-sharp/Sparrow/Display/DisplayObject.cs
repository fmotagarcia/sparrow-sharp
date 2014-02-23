using System;
using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Utils;
using Sparrow.Core;

namespace Sparrow.Display
{
	public class DisplayObject
	{
		private const int MAX_DISPLAY_TREE_DEPTH = 32;
		private double _x;
		private double _y;
		private double _pivotX;
		private double _pivotY;
		private double _scaleX;
		private double _scaleY;
		private double _skewX;
		private double _skewY;
		private double _rotation;
		private double _alpha;
		private uint _blendMode;
		private bool _orientationChanged;
		private DisplayObjectContainer _parent;
		private Matrix _transformationMatrix;
		//		private double _lastTouchTimestamp;
		//		private SPFragmentFilter *_filter;
		public bool visible;
		public bool touchable;
		public string name;

		virtual public double Width {
			get { return BoundsInSpace (_parent).Width; }
			set {
				ScaleX = 1.0;
				double actualWidth = Width;
				if (actualWidth != 0.0) {
					ScaleX = value / actualWidth;
				}
			}
		}

        virtual public double Height
        {
			get { return BoundsInSpace (_parent).Height; }
			set {
				ScaleY = 1.0;
				double actualHeight = Width;
				if (actualHeight != 0.0) {
					ScaleY = value / actualHeight;
				}
			}
		}

        virtual public double X
        {
			get { return _x; }
			set {
				if (value != _x) {
					_x = value;
					_orientationChanged = true;
				}
			}
		}

        virtual public double Y
        {
			get { return _y; }
			set {
				if (value != _y) {
					_y = value;
					_orientationChanged = true;
				}
			}
		}

		public double ScaleX {
			get { return _scaleX; }
			set {
				if (value != _scaleX) {
					_scaleX = value;
					_orientationChanged = true;
				}
			}
		}

		public double ScaleY {
			get { return _scaleY; }
			set {
				if (value != _scaleY) {
					_scaleY = value;
					_orientationChanged = true;
				}
			}
		}

		public double SkewX {
			get { return _skewX; }
			set {
				if (value != _skewX) {
					_skewX = value;
					_orientationChanged = true;
				}
			}
		}

		public double SkewY {
			get { return _skewY; }
			set {
				if (value != _skewY) {
					_skewY = value;
					_orientationChanged = true;
				}
			}
		}

		public double PivotX {
			get { return _pivotX; }
			set {
				if (value != _pivotX) {
					_pivotX = value;
					_orientationChanged = true;
				}
			}
		}

		public double PivotY {
			get { return _pivotY; }
			set {
				if (value != _pivotY) {
					_pivotY = value;
					_orientationChanged = true;
				}
			}
		}

		public double Rotation {
			get { return _rotation; }
			set {
				// move to equivalent value in range [0 deg, 360 deg] without a loop
				value = value % 2.0 * Math.PI;

				// move to [-180 deg, +180 deg]
				if (value < -Math.PI)
					value += 2.0 * Math.PI;
				if (value > Math.PI)
					value -= 2.0 * Math.PI;

				_rotation = value;
				_orientationChanged = true;
			}
		}

		virtual public double Alpha {
			get { return _alpha; }
			set {
				_alpha = NumberUtil.Clamp (value, 0.0, 1.0);
			}
		}

		public DisplayObject Base {
			get {
				DisplayObject currentObject = this;
				while (currentObject.Parent != null) {
					currentObject = currentObject.Parent;
				}
				return currentObject;
			}
		}

		public DisplayObject Root {
			get {
				DisplayObject currentObject = this;
				while (currentObject.Parent != null) {
					if (currentObject.Parent is Stage) {
						return currentObject;
					} else {
						currentObject = currentObject.Parent;
					}
				}
				return null;
			}
		}

		public Stage stage {
			get {
				if (Base is Stage) {
					return (Stage)Base;
				} else {
					return null;
				}
			}
		}

		public Matrix TransformationMatrix {
			get {
				if (_orientationChanged) {
					_orientationChanged = false;

					if (_skewX == 0.0 && _skewY == 0.0) {
						// optimization: no skewing / rotation simplifies the matrix math
						if (_rotation == 0.0) {
							_transformationMatrix.A = _scaleX;
							_transformationMatrix.B = 0.0;
							_transformationMatrix.C = 0.0;
							_transformationMatrix.C = _scaleY;
							_transformationMatrix.Tx = _x - _pivotX * _scaleX;
							_transformationMatrix.Ty = _pivotY * _scaleY;
						} else {
							double cos = Math.Cos (_rotation);
							double sin = Math.Sin (_rotation);
							_transformationMatrix.A = _scaleX * cos;
							_transformationMatrix.B = _scaleX * sin;
							_transformationMatrix.C = _scaleY * -sin;
							_transformationMatrix.D = _scaleY * cos;
							_transformationMatrix.Tx = _x - _pivotX * _transformationMatrix.A - _pivotY * _transformationMatrix.C;
							_transformationMatrix.Ty = _y - _pivotX * _transformationMatrix.B - _pivotY * _transformationMatrix.D;
						}
					} else {
						_transformationMatrix.Identity ();
						_transformationMatrix.Scale (_scaleX, _scaleY);
						_transformationMatrix.Skew (_skewX, _skewY);
						_transformationMatrix.Rotate (_rotation);
						_transformationMatrix.Translate (_x, _y);

						if (_pivotX != 0.0 || _pivotY != 0.0) {
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

			set {
				_orientationChanged = false;
				_transformationMatrix.CopyFromMatrix (value);


				_pivotX = 0.0;
				_pivotY = 0.0;

				_x = value.Tx;
				_y = value.Ty;

				_skewX = Math.Atan (-value.C / value.D);
				_skewY = Math.Atan (value.B / value.A);

				_scaleX = value.A / Math.Cos (_skewY);
				_scaleY = value.D / Math.Cos (_skewX);

				if (NumberUtil.Equals (_skewX, _skewY)) {
					_rotation = _skewX;
					_skewX = _skewY = 0.0;
				} else {
					_rotation = 0.0;
				}
			}
		}

		public bool HasVisibleArea {
			get {
				return _alpha != 0.0 && visible && _scaleX != 0.0 && _scaleY != 0.0;
			}
		}

		public Rectangle Bounds {
			get { return this.BoundsInSpace (Parent); }

		}

		public DisplayObjectContainer Parent {
			get { return _parent; }

			set {
				DisplayObject ancestor = value;
				while (ancestor != this && ancestor != null) {
					ancestor = ancestor.Parent;
				}

				if (ancestor == this) {
					throw new InvalidOperationException ("An object cannot be added as a child to itself or one of its children");
				} else {
					_parent = value;
				}
			}	
		}

		public uint BlendMode {
			get { return _blendMode; }
			set {
				_blendMode = value;
			}
		}

		public DisplayObject ()
		{
			_alpha = 1.0;
			_scaleX = 1.0;
			_scaleY = 1.0;
			visible = true;
			touchable = true;
			_transformationMatrix = new Matrix ();
			_orientationChanged = false;
			_blendMode = Sparrow.Display.BlendMode.AUTO;
		}

		public virtual void Render (RenderSupport support)
		{
			// TODO abstract
		}

		public void RemoveFromParent ()
		{
			// TODO implement
		}

		public void AlignPivotToCenter ()
		{
			Align (HAlign.CENTER, VAlign.CENTER);
		}

		public void Align (string hAlign, string vAlign)
		{
			Rectangle bounds = BoundsInSpace (this);
			_orientationChanged = true;

			switch (hAlign) {
			case HAlign.LEFT:
				_pivotX = bounds.X;
				break;
			case HAlign.CENTER:
				_pivotX = bounds.X + bounds.Width / 2.0;
				break;
			case HAlign.RIGHT:
				_pivotX = bounds.X + bounds.Width; 
				break;
			default:
				throw new InvalidOperationException ("invalid horizontal alignment");
			}

			switch (vAlign) {
			case VAlign.TOP:
				_pivotY = bounds.Y;  
				break;
			case VAlign.CENTER:
				_pivotY = bounds.Y + bounds.Height / 2.0; 
				break;
			case VAlign.BOTTOM:
				_pivotY = bounds.Y + bounds.Height; 
				break;
			default:
				throw new InvalidOperationException ("invalid vertical alignment");
			}
		}

		public Matrix TransformationMatrixToSpace (DisplayObject targetSpace)
		{     
			DisplayObject currentObject;
			Matrix selfMatrix;
			Matrix targetMatrix;

			if (targetSpace == this) {
				return new Matrix ();
			} else if (targetSpace == _parent || (targetSpace == null && _parent == null)) {
				Matrix transformationMatrix = new Matrix ();
				transformationMatrix.CopyFromMatrix (TransformationMatrix);
				return transformationMatrix;
			} else if (targetSpace == null || targetSpace == this.Base) {
				// targetSpace 'null' represents the target coordinate of the base object.
				// -> move up from this to base
				selfMatrix = new Matrix ();
				currentObject = this;
				while (currentObject != targetSpace) {
					selfMatrix.AppendMatrix (currentObject.TransformationMatrix);
					currentObject = currentObject.Parent;
				}        
				return selfMatrix; 
			} else if (targetSpace.Parent == this) {
				targetMatrix = new Matrix ();
				targetMatrix.CopyFromMatrix (_transformationMatrix);
				targetMatrix.Invert ();

				return targetMatrix;
			}

			// 1.: Find a common parent of this and the target coordinate space.
			List<DisplayObject> ancestors = new List<DisplayObject> ();

			int count = 0;
			DisplayObject commonParent = null;
			currentObject = this;
			while (currentObject != null && count < MAX_DISPLAY_TREE_DEPTH) {
				ancestors.Add (currentObject);
				currentObject = currentObject.Parent;
			}

			currentObject = targetSpace;    
			while (currentObject != null && commonParent == null) {        
				for (int i = 0; i < count; ++i) {
					if (currentObject == ancestors [i]) {
						commonParent = ancestors [i];
						break;                
					}            
				}
				currentObject = currentObject.Parent;
			}

			if (commonParent == null) {
				throw new Exception ("Object not connected to target");
			}

			// 2.: Move up from this to common parent
			selfMatrix = new Matrix ();
			currentObject = this;    
			while (currentObject != commonParent) {
				selfMatrix.AppendMatrix (currentObject.TransformationMatrix);
				currentObject = currentObject.Parent;
			}

			// 3.: Now move up from target until we reach the common parent
			targetMatrix = new Matrix ();
			currentObject = targetSpace;
			while (currentObject != null && currentObject != commonParent) {
				targetMatrix.AppendMatrix (currentObject.TransformationMatrix);
				currentObject = currentObject.Parent;
			}    

			// 4.: Combine the two matrices
			targetMatrix.Invert ();
			selfMatrix.AppendMatrix (targetMatrix);

			return selfMatrix;
		}

		virtual public Rectangle BoundsInSpace (DisplayObject targetSpace)
		{
			return null;
			// TODO abstract
		}

		public DisplayObject HitTestPoint (Point localPoint)
		{
			// invisible or untouchable objects cause the test to fail
			if (!visible || !touchable) {
				return null;
			}

			// otherwise, check bounding box
			if (BoundsInSpace (this).Contains (localPoint)) {
				return this; 
			} else {
				return null;
			}
		}

		Point LocalToGlobal (Point localPoint)
		{
			Matrix matrix = TransformationMatrixToSpace (Base);

			return matrix.TransformPoint (localPoint);
		}

		Point GlobalToLocal (Point globalPoint)
		{
			Matrix matrix = TransformationMatrixToSpace (Base);
			return matrix.TransformPoint (globalPoint);
		}
	}
}