using System;
using Sparrow.Styles;
using Sparrow.Text;
using Sparrow.Textures;
using Sparrow.Touches;
using Rectangle = Sparrow.Geom.Rectangle;

namespace Sparrow.Display
{
    
    /** A simple button composed of an image and, optionally, text.
     *  
     *  <p>You can use different textures for various states of the button. If you're providing
     *  only an up state, the button is simply scaled a little when it is touched.</p>
     *
     *  <p>In addition, you can overlay text on the button. To customize the text, you can use
     *  properties equivalent to those of the TextField class. Move the text to a certain position
     *  by updating the <code>textBounds</code> property.</p>
     *  
     *  <p>To react on touches on a button, there is special <code>Triggered</code> event.
     *  Use this event instead of normal touch events. That way, users can cancel button
     *  activation by moving the mouse/finger away from the button before releasing.</p>
     */
    public class Button : DisplayObjectContainer
    {
        
        public delegate void TriggeredHandler(Button btn);
        public event TriggeredHandler Triggered;

        private const int MaxDragDist = 50;

        private Texture _upState;
        private Texture _downState;
        private Texture _overState;
        private Texture _disabledState;
        
        private readonly Sprite _contents;
        private readonly Image _body;
        private TextField _textField;
        private readonly Rectangle _textBounds;
        private Sprite _overlay;

        //private bool _useHandCursor;
        private bool _enabled;
        private ButtonState _state;
        private Rectangle _triggerBounds;
        
        
        /// <summary>
        /// Creates a button with a set of state-textures and (optionally) some text.
        /// Any state that is left 'null' will display the up-state texture. Beware that all
        /// state textures should have the same dimensions.
        /// </summary>
        /// <exception cref="ArgumentException">When upState is null</exception>
        public Button(Texture upState, string text = "", Texture downState = null,
                      Texture overState = null, Texture disabledState = null)
        {
            if (upState == null) throw new ArgumentException("Texture 'upState' cannot be null");
            
            _upState = upState;
            _downState = downState;
            _overState = overState;
            _disabledState = disabledState;

            _state = ButtonState.Up;
            _body = new Image(upState);
            _body.PixelSnapping = true;
            ScaleWhenDown = downState != null ? 1.0f : 0.9f;
            ScaleWhenOver = AlphaWhenDown = 1.0f;
            AlphaWhenDisabled = disabledState != null ? 1.0f : 0.5f;
            _enabled = true;
            //_useHandCursor = true;
            _textBounds = Rectangle.Create(0, 0, _body.Width, _body.Height);
            _triggerBounds = Rectangle.Create();
            
            _contents = new Sprite();
            _contents.AddChild(_body);
            AddChild(_contents);
            Touch += OnTouch;
            
            //??? this.touchGroup = true;
            Text = text;
        }
        
        public override void Dispose()
        {
            // text field might be disconnected from parent, so we have to dispose it manually
            _textField?.Dispose();
            base.Dispose();
        }
        
        /// <summary>
        /// Readjusts the dimensions of the button according to its current state texture.
        /// Call this method to synchronize button and texture size after assigning a texture
        /// with a different size.
        /// </summary>
        public void ReadjustSize()
        {
            var prevWidth = _body.Width;
            var prevHeight = _body.Height;

            _body.ReadjustSize();

            var scaleX = _body.Width  / prevWidth;
            var scaleY = _body.Height / prevHeight;

            _textBounds.X *= scaleX;
            _textBounds.Y *= scaleY;
            _textBounds.Width *= scaleX;
            _textBounds.Height *= scaleY;

            if (_textField != null) CreateTextField();
        }
        
        private void CreateTextField()
        {
            if (_textField == null)
            {
                _textField = new TextField(_textBounds.Width, _textBounds.Height);
                _textField.PixelSnapping = _body.PixelSnapping;
                _textField.Touchable = false;
                _textField.AutoScale = true;
                _textField.Batchable = true;
            }
            
            _textField.Width  = _textBounds.Width;
            _textField.Height = _textBounds.Height;
            _textField.X = _textBounds.X;
            _textField.Y = _textBounds.Y;
        }

        private void OnTouch(TouchEvent evt)
        {
            //Mouse.cursor = (_useHandCursor && _enabled && evt.interactsWith(this)) ? MouseCursor.BUTTON : MouseCursor.AUTO;
            
            Touch touch = evt.GetTouch(this);

            if (!_enabled)
            {
                return;
            }
            if (touch == null)
            {
                State = ButtonState.Up;
            }
            else if (touch.Phase == TouchPhase.Stationary)
            {
                State = ButtonState.Over;
            }
            else if (touch.Phase == TouchPhase.Began && _state != ButtonState.Down)
            {
                _triggerBounds = GetBounds(Stage);
                _triggerBounds.Inflate(MaxDragDist, MaxDragDist);

                State = ButtonState.Down;
            }
            else if (touch.Phase == TouchPhase.Moved)
            {
                var isWithinBounds = _triggerBounds.Contains(touch.GlobalX, touch.GlobalY);

                if (_state == ButtonState.Down && !isWithinBounds)
                {
                    // reset button when finger is moved too far away ...
                    State = ButtonState.Up;
                }
                else if (_state == ButtonState.Up && isWithinBounds)
                {
                    // ... and reactivate when the finger moves back into the bounds.
                    State = ButtonState.Down;
                }
            }
            else if (touch.Phase == TouchPhase.Ended && _state == ButtonState.Down)
            {
                State = ButtonState.Up;
                Triggered?.Invoke(this);
            }
        }
        
        /// <summary>
        /// The current state of the button.
        /// </summary>
        /// <exception cref="ArgumentException">When setting an invalid state</exception>
        public ButtonState State
        {
            get { return _state; }
            set
            {
                _state = value;
                _contents.X = _contents.Y = 0;
                _contents.ScaleX = _contents.ScaleY = _contents.Alpha = 1.0f;

                switch (_state)
                {
                    case ButtonState.Down:
                        SetStateTexture(_downState);
                        _contents.Alpha = AlphaWhenDown;
                        _contents.ScaleX = _contents.ScaleY = ScaleWhenDown;
                        _contents.X = (1.0f - ScaleWhenDown) / 2.0f * _body.Width;
                        _contents.Y = (1.0f - ScaleWhenDown) / 2.0f * _body.Height;
                        break;
                    case ButtonState.Up:
                        SetStateTexture(_upState);
                        break;
                    case ButtonState.Over:
                        SetStateTexture(_overState);
                        _contents.ScaleX = _contents.ScaleY = ScaleWhenOver;
                        _contents.X = (1.0f - ScaleWhenOver) / 2.0f * _body.Width;
                        _contents.Y = (1.0f - ScaleWhenOver) / 2.0f * _body.Height;
                        break;
                    case ButtonState.Disabled:
                        SetStateTexture(_disabledState);
                        _contents.Alpha = AlphaWhenDisabled;
                        break;
                    default:
                        throw new ArgumentException("Invalid button state: " + _state);
                }
            }
        }
        
        private void SetStateTexture(Texture texture)
        {
            _body.Texture = texture != null ? texture : _upState;
        }
        
        /** The scale factor of the button on touch. Per default, a button without a down state
        *  texture will be made slightly smaller, while a button with a down state texture
        *  remains unscaled. */
        public float ScaleWhenDown { get; set; }

        /** The scale factor of the button while the mouse cursor hovers over it. @default 1.0 */
        public float ScaleWhenOver { get; set; }

        /** The alpha value of the button on touch. @default 1.0 */
        public float AlphaWhenDown { get; set; }

        /** The alpha value of the button when it is disabled. @default 0.5 */
        public float AlphaWhenDisabled { get; set; }

        /** Indicates if the button can be triggered. */
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    State = value ? ButtonState.Up : ButtonState.Disabled;
                }
            }
        }
        
        /** The text that is displayed on the button. */
        public string Text
        {
            get { return _textField != null ? _textField.Text : ""; }
            set
            {
                if (value.Length == 0)
                {
                    if (_textField != null)
                    {
                        _textField.Text = value;
                        _textField.RemoveFromParent();
                    }
                }
                else
                {
                    CreateTextField();
                    _textField.Text = value;
                
                    if (_textField.Parent == null)
                        _contents.AddChild(_textField);
                }
            }
        }

        /// <summary>
        /// The format of the button's TextField.
        /// </summary>
        public TextFormat TextFormat
        {
            get
            {
                if (_textField == null) CreateTextField();
                return _textField.Format;
            }
            set
            {
                if (_textField == null) CreateTextField();
                _textField.Format = value;
            }
        }

        /// <summary>
        /// The style that is used to render the button.
        /// Note that a style instance may only be used on one mesh at a time.
        /// </summary>
        public MeshStyle Style
        {
            get { return _body.Style; }
            set { _body.Style = value; }
        }
        
        public Texture UpState
        {
            get { return _upState; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Texture 'upState' cannot be null");   
                }

                if (_upState != value)
                {
                    _upState = value;
                    if ( _state == ButtonState.Up ||
                         (_state == ButtonState.Disabled && _disabledState == null) ||
                         (_state == ButtonState.Down && _downState == null) ||
                         (_state == ButtonState.Over && _overState == null))
                    {
                        SetStateTexture(value);
                    }
                }
            }
        }

        public Texture DownState
        {
            get { return _downState; }
            set
            {
                if (_downState != value)
                {
                    _downState = value;
                    if (_state == ButtonState.Down) SetStateTexture(value);
                }
            }
        }

        public Texture OverState
        {
            get { return _overState; }
            set
            {
                if (_overState != value)
                {
                    _overState = value;
                    if (_state == ButtonState.Over) SetStateTexture(value);
                }
            }
        }

        public Texture DisabledState
        {
            get { return _disabledState; }
            set
            {
                if (_disabledState != value)
                {
                    _disabledState = value;
                    if (_state == ButtonState.Disabled) SetStateTexture(value);
                }
            }
        }

        /// <summary>
        /// The bounds of the button's TextField. Allows moving the text to a custom position.
        /// CAUTION: not a copy, but the actual object! Text will only update on re-assignment.
        /// </summary>
        public Rectangle TextBounds
        {
            get { return _textBounds; }
            set
            {
                _textBounds.CopyFrom(value);
                CreateTextField();
            }
        }

        /// <summary>
        /// The color of the button's state image. Just like every image object, each pixel's
        /// color is multiplied with this value. Default: white
        /// </summary>
        public uint Color
        {
            get { return _body.Color; }
            set { _body.Color = value; }
        }

        /// <summary>
        /// The smoothing type used for the button's state image.
        /// </summary>
        public TextureSmoothing TextureSmoothing
        {
            get { return _body.TextureSmoothing; }
            set { _body.TextureSmoothing = value; }
        }

        /// <summary>
        /// The overlay sprite is displayed on top of the button contents. It scales with the
        /// button when pressed. Use it to add additional objects to the button (e.g. an icon).
        /// </summary>
        public Sprite Overlay
        {
            get
            {
                if (_overlay == null)
                {
                    _overlay = new Sprite();   
                }
                _contents.AddChild(_overlay); // make sure it's always on top
                return _overlay;
            }
        }

        public bool PixelSnapping
        {
            get { return _body.PixelSnapping; }
            set
            {
                _body.PixelSnapping = value;
                if (_textField != null) _textField.PixelSnapping = value;
            }
        }

        public override float Width
        {
            set
            {
                // The Button might use a Scale9Grid ->
                // we must update the body width/height manually for the grid to scale properly.
                float newWidth = value / (ScaleX == 0f ? 1.0f : ScaleX);
                float scale = newWidth / (_body.Width == 0f ? 1.0f : _body.Width);

                _body.Width = newWidth;
                _textBounds.X *= scale;
                _textBounds.Width *= scale;

                if (_textField != null) _textField.Width = newWidth;
            }
        }

        public override float Height
        {
            set
            {
                float newHeight = value /  (ScaleY == 0f ? 1.0f : ScaleY);
                float scale = newHeight / (_body.Height == 0f ? 1.0f : _body.Height);

                _body.Height = newHeight;
                _textBounds.Y *= scale;
                _textBounds.Height *= scale;

                if (_textField != null) _textField.Height = newHeight;
            }
        }
        
        /// <summary>
        /// The current scaling grid used for the button's state image. Use this property to create
        /// buttons that resize in a smart way, i.e. with the four corners keeping the same size
        /// and only stretching the center area. Default: null
        /// <see cref="Image.Scale9Grid"/>
        /// </summary>
        public Rectangle Scale9Grid
        {
            get { return _body.Scale9Grid; }
            set { _body.Scale9Grid = value; }
        }
        
        // +add UseHandCursor
    }
}