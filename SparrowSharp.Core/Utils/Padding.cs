
namespace Sparrow.Utils
{
    public class Padding
    {
        private float _left;
        private float _right;
        private float _top;
        private float _bottom;
        public delegate void OnChanged();
        public OnChanged OnChangedEvent;

        /** Creates a new instance with the given properties. */
        public Padding(float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            SetTo(left, right, top, bottom);
        }

        /** Sets all four values at once. */
        public void SetTo(float left= 0, float right= 0, float top= 0, float bottom= 0)
        {
            bool changed = _left != left || _right != right || _top != top || _bottom != bottom;

            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;

            if (changed)
            {
                OnChangedEvent?.Invoke();
            }
        }

        /** Copies all properties from another Padding instance.
         *  Pass <code>null</code> to reset all values to zero. */
        public void CopyFrom(Padding padding)
        {
            if (padding == null) SetTo(0, 0, 0, 0);
            else SetTo(padding._left, padding._right, padding._top, padding._bottom);
        }

        /** Creates a new instance with the exact same values. */
        public Padding Clone()
        {
            return new Padding(_left, _right, _top, _bottom);
        }

        public float Left
        {
            get { return _left;  }
            set
            {
                if (_left != value)
                {
                    _left = value;
                    OnChangedEvent?.Invoke();
                }

            }
        }

        public float Right
        {
            get { return _right;  }
            set
            {
                if (_right != value)
                {
                    _right = value;
                    OnChangedEvent?.Invoke();
                }
            }
        }

        public float Top
        {
            get { return _top; }
            set
            {
                if (_top != value)
                {
                    _top = value;
                    OnChangedEvent?.Invoke();
                }
            }
        }

        public float Bottom
        {
            get { return _bottom; }
            set
            {
                if (_bottom != value)
                {
                    _bottom = value;
                    OnChangedEvent?.Invoke();
                }
            }
        }
    }
}
