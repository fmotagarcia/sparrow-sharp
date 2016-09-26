using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Display;
using System.Collections.Generic;
using System;
using Sparrow.Styles;
using Sparrow.Textures;
using Sparrow.Rendering;

namespace Sparrow.Text
{
    public class TextField : DisplayObjectContainer
    {
        
        private string _text;
        private TextOptions _options;
        private TextFormat _format;
        private Rectangle _textBounds;
        private Rectangle _hitArea;
        private ITextCompositor _compositor;
        private bool _requiresRecomposition;
        private DisplayObjectContainer _border;
        private MeshBatch _meshBatch;
        private MeshStyle _style;

        // helper objects
        private static Matrix sMatrix = Matrix.Create();
        private static ITextCompositor sDefaultCompositor = new BitmapFont();
        private static TextureFormat sDefaultTextureFormat = TextureFormat.RGB565;
        private TextFormat _helperFormat = new TextFormat();

        public TextField(float width, float height, string text = "", TextFormat format = null)
        {
            _text = text != null ? text : "";
            _hitArea = Rectangle.Create(0, 0, width, height);
            _requiresRecomposition = true;
            _compositor = sDefaultCompositor;
            _options = new TextOptions();

            _format = format != null ? format.Clone() : new TextFormat();
            _format.ChangeEvent += SetRequiresRecomposition;

            _meshBatch = new MeshBatch();
            _meshBatch.Touchable = false;
            _meshBatch.PixelSnapping = true;
            AddChild(_meshBatch);
        }


        /** Disposes the underlying texture data. */
        public override void Dispose()
        {
            _format.ChangeEvent -= SetRequiresRecomposition;
            _compositor.ClearMeshBatch(_meshBatch);

            base.Dispose();
        }
        
        public override void Render(Painter painter)
        {
            if (_requiresRecomposition) Recompose();
            base.Render(painter);
        }

        /** Forces the text contents to be composed right away.
         *  Normally, it will only do so lazily, i.e. before being rendered. */
        private void Recompose()
        {
            if (_requiresRecomposition)
            {
                _compositor.ClearMeshBatch(_meshBatch);

                string fontName = _format.Font;
                ITextCompositor compositor = GetCompositor(fontName);

                if (compositor == null && fontName == BitmapFont.MINI)
                {
                    compositor = new BitmapFont();
                    RegisterCompositor(compositor, fontName);
                }

                _compositor = compositor != null ? compositor : sDefaultCompositor;

                UpdateText();
                UpdateBorder();

                _requiresRecomposition = false;
            }
        }

        // font and border rendering


        private void UpdateText()
        {
            float width  = _hitArea.Width;
            float height = _hitArea.Height;
            TextFormat format = _helperFormat;

            // By working on a copy of the TextFormat, we make sure that modifications done
            // within the 'fillMeshBatch' method do not cause any side effects.
            //
            // (We cannot use a static variable, because that might lead to problems when
            //  recreating textures after a context loss.)

            format.CopyFrom(_format);

            // Horizontal autoSize does not work for HTML text, since it supports custom alignment.
            // What should we do if one line is aligned to the left, another to the right?

            if (IsHorizontalAutoSize) width = 100000;
            if (IsVerticalAutoSize) height = 100000;

            _meshBatch.X = _meshBatch.Y = 0f;
            _options.TextureScale = SparrowSharp.ContentScaleFactor;
            _options.TextureFormat = sDefaultTextureFormat;
            _compositor.FillMeshBatch(_meshBatch, width, height, _text, format, _options);

            if (_style != null) _meshBatch.Style = _style;
            if (_options.AutoSize != TextFieldAutoSize.NONE)
            {
                _textBounds = _meshBatch.GetBounds(_meshBatch);

                if (IsHorizontalAutoSize)
                {
                    _meshBatch.X = _textBounds.X = -_textBounds.X;
                    _hitArea.Width = _textBounds.Width;
                    _textBounds.X = 0;
                }

                if (IsVerticalAutoSize)
                {
                    _meshBatch.Y = _textBounds.Y = -_textBounds.Y;
                    _hitArea.Height = _textBounds.Height;
                    _textBounds.Y = 0f;
                }
            }
            else
            {
                // hit area doesn't change, and text bounds can be created on demand
                _textBounds = null;
            }
        }

        private void UpdateBorder()
        {
            if (_border == null) return;
            
            float width  = _hitArea.Width;
            var height = _hitArea.Height;

            Quad topLine    = _border.GetChild(0) as Quad;
            Quad rightLine  = _border.GetChild(1) as Quad;
            Quad bottomLine = _border.GetChild(2) as Quad;
            Quad leftLine   = _border.GetChild(3) as Quad;
            
            topLine.Width    = width; topLine.Height    = 1;
            bottomLine.Width = width; bottomLine.Height = 1;
            leftLine.Width   = 1;     leftLine.Height   = height;
            rightLine.Width  = 1;     rightLine.Height  = height;
            rightLine.X  = width  - 1;
            bottomLine.Y = height - 1;
            topLine.Color = rightLine.Color = bottomLine.Color = leftLine.Color = _format.Color;
        }

        /// <summary>
        /// Forces the text to be recomposed before rendering it in the upcoming frame.
        /// </summary>
        protected void SetRequiresRecomposition()
        {
            _requiresRecomposition = true;
            SetRequiresRedraw();
        }

        private bool IsHorizontalAutoSize
        {
            get
            {
                return _options.AutoSize == TextFieldAutoSize.HORIZONTAL ||
                       _options.AutoSize == TextFieldAutoSize.BOTH_DIRECTIONS;
            }
        }

        private bool IsVerticalAutoSize
        {
            get
            {
                return _options.AutoSize == TextFieldAutoSize.VERTICAL ||
                       _options.AutoSize == TextFieldAutoSize.BOTH_DIRECTIONS;
            }
        }

        public Rectangle TextBounds
        {
            get
            {
                if (_requiresRecomposition) Recompose();
                if (_textBounds == null) _textBounds = _meshBatch.GetBounds(this);
                return _textBounds.Clone();
            }
        }

        
        public override Rectangle GetBounds(DisplayObject targetSpace)
        {
            if (_requiresRecomposition) Recompose();
            sMatrix = GetTransformationMatrix(targetSpace);
            return _hitArea.GetBounds(sMatrix);
        }

        public override DisplayObject HitTest(Point localPoint)
        {
            if (!Visible || !Touchable || !HitTestMask(localPoint)) return null;
            else if (_hitArea.Contains(localPoint)) return this;
            else return null;
        }

        override public float Width
        {
            set
            {
                // different to ordinary display objects, changing the size of the text field should
                // not change the scaling, but make the texture bigger/smaller, while the size 
                // of the text/font stays the same (this applies to the height, as well).
                _hitArea.Width = value / Math.Max(ScaleX, 1.0f);
                SetRequiresRecomposition();
            }
        }

        override public float Height
        {
            set
            {
                _hitArea.Height = value / Math.Max(ScaleY, 1.0f);
                SetRequiresRecomposition();
            }
        }

      
        /// <summary>
        /// The displayed text.
        /// </summary>
        public string Text
        {
            set
            {
                if (value == null) value = "";
                if (_text != value)
                {
                    _text = value;
                    SetRequiresRecomposition();
                }
            }
            get
            {
                return _text;
            }
        }

        /** The format describes how the text will be rendered, describing the font name and size,
         *  color, alignment, etc.
         *
         *  <p>Note that you can edit the font properties directly; there's no need to reassign
         *  the format for the changes to show up.</p>
         *
         *  <listing>
         *  var textField:TextField = new TextField(100, 30, "Hello Starling");
         *  textField.format.font = "Arial";
         *  textField.format.color = Color.RED;</listing>
         *
         *  @default Verdana, 12 pt, black, centered
         */
         public TextFormat Format
        {
            get { return _format; }
            set
            {
                if (value == null) throw new ArgumentException("format cannot be null");
                _format.CopyFrom(value);
            }
        }

        /** Draws a border around the edges of the text field. Useful for visual debugging.
        *  @default false */
        public bool Border
        {
            get { return _border != null; }
            set
            {
                if (value && _border == null)
                {
                    _border = new Sprite();
                    AddChild(_border);

                    for (int i = 0; i < 4; ++i)
                    _border.AddChild(new Quad(1.0f, 1.0f));

                    UpdateBorder();
                }
                else if (!value && _border != null)
                {
                    _border.RemoveFromParent(true);
                    _border = null;
                }
            }
        }

        /** Indicates whether the font size is automatically reduced if the complete text does
        *  not fit into the TextField. @default false */
        public bool AutoScale
        {
            get { return _options.AutoScale; }
            set
            {
                if (_options.AutoScale != value)
                {
                    _options.AutoScale = value;
                    SetRequiresRecomposition();
                }
            }
        }

        public TextFieldAutoSize AutoSize
        {
            get { return _options.AutoSize; }
            set
            {
                if (_options.AutoSize != value)
                {
                    _options.AutoSize = value;
                    SetRequiresRecomposition();
                }
            }
        }

        /** Indicates if the text should be wrapped at word boundaries if it does not fit into
         *  the TextField otherwise. @default true */
         public bool WordWrap
        {
            get { return _options.WordWrap; }
            set
            {
                if (value != _options.WordWrap)
                {
                    _options.WordWrap = value;
                    SetRequiresRecomposition();
                }
            }
        }

        /** Indicates if TextField should be batched on rendering.
         *
         *  <p>This works only with bitmap fonts, and it makes sense only for TextFields with no
         *  more than 10-15 characters. Otherwise, the CPU costs will exceed any gains you get
         *  from avoiding the additional draw call.</p>
         *
         *  @default false
         */
         public bool Batchable
         {
            get { return _meshBatch.Batchable; }
            set
            {
                _meshBatch.Batchable = value;
            }
         }

        /** Controls whether or not the instance snaps to the nearest pixel. This can prevent the
        *  object from looking blurry when it's not exactly aligned with the pixels of the screen.
        *  @default true */
        public bool PixelSnapping {
            get { return _meshBatch.PixelSnapping; }
            set { _meshBatch.PixelSnapping = value; }
        }


        /** The style that is used to render the text's mesh. */
        public MeshStyle Style
        {
            get { return _meshBatch.Style; }
            set
            {
                _meshBatch.Style = _style = value;
                SetRequiresRecomposition();
            }
        }

        /** The Context3D texture format that is used for rendering of all TrueType texts.
         *  The default provides a good compromise between quality and memory consumption;
         *  use <pre>Context3DTextureFormat.BGRA</pre> for the highest quality.
         *
         *  @default Context3DTextureFormat.BGRA_PACKED */
        public static TextureFormat DefaultTextureFormat { 
            get { return sDefaultTextureFormat; }
            set { sDefaultTextureFormat = value; }
        }

        /** The default compositor used to arrange the letters of the text.
         *  If a specific compositor was registered for a font, it takes precedence.
         *
         *  @default TrueTypeCompositor
         */
        public static ITextCompositor DefaultCompositor { 
            get { return sDefaultCompositor; }
            set { sDefaultCompositor = value; }
        }

        // compositor registration

        /** Makes a text compositor (like a <code>BitmapFont</code>) available to any TextField in
         *  the current stage3D context. The font is identified by its <code>name</code> (not
         *  case sensitive). */
        public static void RegisterCompositor(ITextCompositor compositor, string name)
        {
            if (name == null) throw new ArgumentException("name must not be null");
            Compositors[name.ToLower()] = compositor;
        }

        /** Unregisters the text compositor and, optionally, disposes it. */
        public static void UnregisterCompositor(string name, bool dispose = true)
        {
            name = name.ToLower();
            if (Compositors.ContainsKey(name))
            {
                if (dispose)
                {
                    Compositors[name].Dispose();
                }
                Compositors.Remove(name);
            }
        }

        /** Returns a registered text compositor (or null, if the font has not been registered).
         *  The name is not case sensitive. */
        public static ITextCompositor GetCompositor(string name)
        {
            ITextCompositor val;
            Compositors.TryGetValue(name.ToLower(), out val);
            return val;
        }

        /** Returns a registered bitmap font compositor (or null, if no compositor has been
         *  registered with that name, or if it's not a bitmap font). The name is not case
         *  sensitive. */
        public static BitmapFont GetBitmapFont(string name)
        {
            return GetCompositor(name) as BitmapFont;
        }

        /** Stores the currently available text compositors. Since compositors will only work
         *  in one Stage3D context, they are saved in Starling's 'contextData' property. */
        private static Dictionary<string, ITextCompositor> Compositors
        {
            get
            {
                Dictionary<string, ITextCompositor> compositors = SparrowSharp.Painter.Compositors;

                if (compositors == null)
                {
                    compositors = new Dictionary<string, ITextCompositor>();
                    SparrowSharp.Painter.Compositors = compositors;
                }

                return compositors;
            }
        }

    }
}

