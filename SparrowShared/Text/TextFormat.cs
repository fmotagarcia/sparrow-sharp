
using Sparrow.Utils;

namespace Sparrow.Text
{
    /** The TextFormat class represents character formatting information. It is used by the
     *  TextField and BitmapFont classes to characterize the way the glyphs will be rendered.
     *
     *  <p>Note that not all properties are used by all font renderers: bitmap fonts ignore
     *  the "bold", "italic", and "underline" values.</p>
     */
    public class TextFormat
    {

        private string _font;
        private float _size;
        private uint _color;
        //private var _bold:Boolean;
        //private var _italic:Boolean;
        //private var _underline:Boolean;
        private HAlign _horizontalAlign;
        private VAlign _verticalAlign;
        private bool _kerning;
        private float _leading;

        public delegate void Change();
        public Change ChangeEvent; 

        /** Creates a new TextFormat instance with the given properties. */
        public TextFormat(string font = BitmapFont.MINI, float size = 12, uint color = 0x0,
                          HAlign horizontalAlign = HAlign.Center, VAlign verticalAlign = VAlign.Center)
        {
            _font = font;
            _size = size;
            _color = color;
            _horizontalAlign = horizontalAlign;
            _verticalAlign = verticalAlign;
            _kerning = true;
            _leading = 0.0f;
        }

        /** Copies all properties from another TextFormat instance. */
        public void CopyFrom(TextFormat format)
        {
            _font = format._font;
            _size = format._size;
            _color = format._color;
            //_bold = format._bold;
            //_italic = format._italic;
            //_underline = format._underline;
            _horizontalAlign = format._horizontalAlign;
            _verticalAlign = format._verticalAlign;
            _kerning = format._kerning;
            _leading = format._leading;

            ChangeEvent?.Invoke();
        }

        /** Creates a clone of this instance. */
        public TextFormat Clone()
        {
            TextFormat clone = new TextFormat();
            clone.CopyFrom(this);
            return clone;
        }

        /** Sets the most common properties at once. */
        public void SetTo(string font = "Verdana", float size = 12, uint color = 0x0,
                          HAlign horizontalAlign = HAlign.Center, VAlign verticalAlign = VAlign.Center)
        {
            _font = font;
            _size = size;
            _color = color;
            _horizontalAlign = horizontalAlign;
            _verticalAlign = verticalAlign;

            ChangeEvent?.Invoke();
        }

        /** The name of the font. TrueType fonts will be looked up from embedded fonts and
         *  system fonts; bitmap fonts must be registered at the TextField class first.
         *  Beware: If you loaded an embedded font at runtime, you must call
         *  <code>TextField.updateEmbeddedFonts()</code> for Starling to recognize it.
         */
        public string Font
        {
            get { return _font; }
            set {
                if (value != _font)
                {
                    _font = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

        /** The size of the font. For bitmap fonts, use <code>BitmapFont.NATIVE_SIZE</code> for
         *  the original size. */
        public float Size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    _size = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

        /** The color of the text. Note that bitmap fonts should be exported in plain white so
        *  that tinting works correctly. If your bitmap font contains colors, set this property
        *  to <code>0xFFFFFF</code> to get the desired result. @default black */
        public uint Color
        {
            get { return _color; }
            set
            {
                if (value != _color)
                {
                    _color = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

        public HAlign HorizontalAlign
        {
            get { return _horizontalAlign; }
            set
            {
                if (value != _horizontalAlign)
                {
                    _horizontalAlign = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

        public VAlign VerticalAlign
        {
            get { return _verticalAlign; }
            set
            {
                if (value != _verticalAlign)
                {
                    _verticalAlign = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

        /** Indicates whether kerning is enabled. Kerning adjusts the pixels between certain
         *  character pairs to improve readability. @default true */
        public bool Kerning
        {
            get { return _kerning; }
            set
            {
                if (value != _kerning)
                {
                    _kerning = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

        /** The amount of vertical space (called 'leading') between lines. @default 0 */
        public float Leading
        {
            get { return _leading; }
            set
            {
                if (value != _leading)
                {
                    _leading = value;
                    ChangeEvent?.Invoke();
                }
            }
        }

    }
}
