using System;
using System.Collections.Generic;
using Sparrow.Textures;
using System.Xml;
using System.IO;
using Sparrow.Geom;
using Sparrow.Display;
using Sparrow.ResourceLoading;
using Sparrow.Utils;

namespace Sparrow.Text
{
    /** The BitmapFont class parses bitmap font files and arranges the glyphs
     *  in the form of a text.
     *
     *  The class parses the XML format as it is used in the 
     *  <a href="http://www.angelcode.com/products/bmfont/">AngelCode Bitmap Font Generator</a> or
     *  the <a href="http://glyphdesigner.71squared.com/">Glyph Designer</a>. 
     *  This is what the file format looks like:
     *
     *  <pre> 
     *  &lt;font&gt;
     *    &lt;info face="BranchingMouse" size="40" /&gt;
     *    &lt;common lineHeight="40" /&gt;
     *    &lt;pages&gt;  &lt;!-- currently, only one page is supported --&gt;
     *      &lt;page id="0" file="texture.png" /&gt;
     *    &lt;/pages&gt;
     *    &lt;chars&gt;
     *      &lt;char id="32" x="60" y="29" width="1" height="1" xoffset="0" yoffset="27" xadvance="8" /&gt;
     *      &lt;char id="33" x="155" y="144" width="9" height="21" xoffset="0" yoffset="6" xadvance="9" /&gt;
     *    &lt;/chars&gt;
     *    &lt;kernings&gt; &lt;!-- Kerning is optional --&gt;
     *      &lt;kerning first="83" second="83" amount="-4"/&gt;
     *    &lt;/kernings&gt;
     *  &lt;/font&gt;
     *  </pre>
     *  
     *  Pass an instance of this class to the method <code>registerBitmapFont</code> of the
     *  TextField class. Then, set the <code>fontName</code> property of the text field to the 
     *  <code>name</code> value of the bitmap font. This will make the text field use the bitmap
     *  font.  
     */
    public class BitmapFont : ITextCompositor
    {
        /** Use this constant for the <code>fontSize</code> property of the TextField class to 
         *  render the bitmap font in exactly the size it was created. */
        public static readonly int NATIVE_SIZE = -1;
        
        /** The font name of the embedded minimal bitmap font. Use this e.g. for debug output. */
        public const string MINI = "mini";
        
        private const int CHAR_SPACE = 32;
        private const int CHAR_TAB = 9;
        private const int CHAR_NEWLINE = 10;
        private const int CHAR_CARRIAGE_RETURN = 13;


        public Texture _texture { get; private set; }
        private Dictionary<int, BitmapChar> _chars;
        /** The name of the font as it was parsed from the font file. */
        public string Name { get; private set; }
        public float Size { get; private set; }
        /** The height of one line in points. */
        public float LineHeight { get; private set; }
        /** The baseline of the font. This property does not affect text rendering;
        *  it's just an information that may be useful for exact text placement. */
        public float Baseline { get; private set; }
        /** An offset that moves any generated text along the x-axis (in points).
         *  Useful to make up for incorrect font data. @default 0. */
        public float OffsetX { get; private set; }
        /** An offset that moves any generated text along the y-axis (in points).
         *  Useful to make up for incorrect font data. @default 0. */
        public float OffsetY { get; private set; }
        /** The width of a "gutter" around the composed text area, in points.
         *  This can be used to bring the output more in line with standard TrueType rendering:
         *  Flash always draws them with 2 pixels of padding. @default 0.0 */
        public float Padding { get; private set; }
        private Image _helperImage;

        // helper objects
        private static List<List<CharLocation>> sLines = new List<List<CharLocation>>();
        private static TextOptions sDefaultOptions = new TextOptions();

        /// <summary>
        /// Initializes a new instance with an embedded mini font. This font's characters have a height of 5 pixels and a maximum
        /// width of 5 pixels.
        /// </summary>
        public BitmapFont()
        {
            Stream fontTextureData = new MemoryStream(Convert.FromBase64String(MiniBitmapFont.MiniFontImageDataBase64));
            MemoryStream fontXmlData = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(MiniBitmapFont.FontXML));

            ParseFontData(fontTextureData, fontXmlData);
        }

        public BitmapFont(Stream fontTextureData, Stream fontXmlData)
        {
            ParseFontData(fontTextureData, fontXmlData);
        }

        private void ParseFontData(Stream fontTextureData, Stream fontXmlData)
        {
            if (fontTextureData == null || fontXmlData == null)
            {
                throw new InvalidOperationException("Font parsing requires texture and font XML to be set");
            }

            Name = "unknown";
            LineHeight = Size = Baseline = 14;
            OffsetX = OffsetY = Padding = 0.0f;
            
            _texture = new TextureLoader().LoadFromStream(fontTextureData);
            _chars = new Dictionary<int, BitmapChar>();
            _helperImage = new Image(_texture);

            XmlDocument xml = new XmlDocument();
            xml.Load(fontXmlData);


            ParseFontXml(xml);
        }

        /** Disposes the texture of the bitmap font. */
        public void Dispose()
        {
            if (_texture != null)
            {
                _texture.Dispose();
            }
        }

        private void ParseFontXml(XmlDocument xml)
        {
            float scale = _texture.Scale;
            Rectangle frame = _texture.Frame;
            float frameX = frame != null ? frame.X : 0f;
            float frameY = frame != null ? frame.Y : 0f;

            // parse info
            XmlNodeList infoNodes = xml.GetElementsByTagName("info");
            if (infoNodes.Count > 0)
            {
                XmlAttributeCollection attributes = infoNodes[0].Attributes;
                Name = attributes["face"].Value;
                Size = Convert.ToSingle(attributes["size"].Value) / scale;
                if (attributes["smooth"].Value == "0")
                {
                    Smoothing = TextureSmoothing.None;
                }
            }

            // parse common
            XmlNodeList commonNode = xml.GetElementsByTagName("common");
            if (commonNode.Count > 0)
            {
                XmlAttributeCollection attributes = commonNode[0].Attributes;
                LineHeight = Convert.ToSingle(attributes["lineHeight"].Value) / scale;
                Baseline = Convert.ToSingle(attributes["base"].Value) / scale;
            }

            if (Size <= 0)
            {
                Console.WriteLine("[Sparrow] Warning: invalid font size in '" + Name + "' font.");
                Size = (Size == 0.0f ? 16.0f : Size * -1.0f);
            }

            // parse chars
            XmlNodeList charNodes = xml.GetElementsByTagName("char");
            for (int i = 0; i < charNodes.Count; i++)
            {
                XmlAttributeCollection attributes = charNodes[i].Attributes;

                int charId = Convert.ToInt32(attributes["id"].Value);
                float xOffset = Convert.ToSingle(attributes["xoffset"].Value) / scale;
                float yOffset = Convert.ToSingle(attributes["yoffset"].Value) / scale;
                float xAdvance = Convert.ToSingle(attributes["xadvance"].Value) / scale;

                Rectangle region = Rectangle.Create();
                region.X = Convert.ToSingle(attributes["x"].Value) / scale + frameX;
                region.Y = Convert.ToSingle(attributes["y"].Value) / scale + frameY;
                region.Width = Convert.ToSingle(attributes["width"].Value) / scale;
                region.Height = Convert.ToSingle(attributes["height"].Value) / scale;

                Texture texture = Texture.FromTexture(_texture, region);
                
                BitmapChar bitmapChar = new BitmapChar(charId, texture, xOffset, yOffset, xAdvance);
                AddChar(charId, bitmapChar);
            }

            //kerning
            XmlNodeList kerningNodes = xml.GetElementsByTagName("kerning");
            for (int i = 0; i < kerningNodes.Count; i++)
            {
                XmlAttributeCollection attributes = kerningNodes[i].Attributes;

                int first = Convert.ToInt32(attributes["first"].Value);
                int second = Convert.ToInt32(attributes["second"].Value);
                float amount = Convert.ToSingle(attributes["ammount"].Value) / scale;

                BitmapChar sec = GetChar(second);
                if (sec != null)
                {
                    sec.AddKerning(amount, first);
                }
            }
           
        }

        /** Returns a single bitmap char with a certain character ID. */
        public BitmapChar GetChar(int charID)
        {
            BitmapChar ret;
            _chars.TryGetValue(charID, out ret);
            return ret;
        }

        /** Adds a bitmap char with a certain character ID. */
        public void AddChar(int charID, BitmapChar bitmapChar)
        {
            _chars.Add(charID, bitmapChar);
        }

        /** Draws text into a QuadBatch. */
        public void FillMeshBatch(MeshBatch meshBatch, float width, float height, string text,
                                 TextFormat format, TextOptions options= null)
        {
            List<CharLocation> charLocations = ArrangeChars(
                    width, height, text, format, options);
            int numChars = charLocations.Count;
            _helperImage.Color = format.Color;
            
            for (int i = 0; i < numChars; ++i)
            {
                CharLocation charLocation = charLocations[i];
                _helperImage.Texture = charLocation.Char.Texture;
                _helperImage.ReadjustSize();
                _helperImage.X = charLocation.X;
                _helperImage.Y = charLocation.Y;
                _helperImage.Scale = charLocation.Scale;
                meshBatch.AddMesh(_helperImage);
            }
        }

        /** @inheritDoc */
        public void ClearMeshBatch(MeshBatch meshBatch)
        {
            meshBatch.Clear();
        }

        private List<CharLocation> ArrangeChars(float width, float height, string text, TextFormat format, TextOptions options)
        {
            if (text.Length == 0)
            {
                return new List<CharLocation>();
            }
            if (options == null) options = sDefaultOptions;

            bool kerning = format.Kerning;
            float leading = format.Leading;
            HAlign hAlign = format.HorizontalAlign;
            VAlign vAlign = format.VerticalAlign;
            float fontSize = format.Size;
            bool autoScale = options.AutoScale;
            bool wordWrap = options.WordWrap;

            bool finished = false;
            CharLocation charLocation;
            int numChars;
            float containerWidth = 0;
            float containerHeight = 0;
            float scale = 0;
            int i;

            if (fontSize < 0f) fontSize *= -Size;

            float currentY = 0;

            while (!finished)
            {
                sLines.Clear();
                scale = fontSize / Size;
                containerWidth = (width - 2 * Padding) / scale;
                containerHeight = (height - 2 * Padding) / scale;

                if (LineHeight <= containerHeight)
                {
                    int lastWhiteSpace = -1;
                    int lastCharId = -1;
                    float currentX = 0;
                    currentY = 0;
                    List<CharLocation> currentLine = new List<CharLocation>();

                    numChars = text.Length;
                    for (i = 0; i < numChars; i++)
                    {
                        bool lineFull = false;
                        int charID = text[i]; // casting to int automatically returns the ASCII value
                        BitmapChar bitmapChar = GetChar(charID);

                        if (charID == CHAR_NEWLINE || charID == CHAR_CARRIAGE_RETURN)
                        {
                            lineFull = true;
                        }
                        else if (bitmapChar == null)
                        {
                            Console.WriteLine("font " + Name + " missing character: " + text[i] + " ID:" + charID);
                        }
                        else
                        {
                            if (charID == CHAR_SPACE || charID == CHAR_TAB)
                            {
                                lastWhiteSpace = i;
                            }
                                
                            if (kerning)
                            {
                                currentX += bitmapChar.GetKerning(lastCharId);
                            }
                            charLocation = CharLocation.Create(bitmapChar, 1.0f, currentX + bitmapChar.XOffset, currentY + bitmapChar.YOffset);
                            currentLine.Add(charLocation);

                            currentX += bitmapChar.XAdvance;
                            lastCharId = charID;

                            if (charLocation.X + bitmapChar.Width > containerWidth)
                            {
                                if (wordWrap)
                                {
                                    // when autoscaling, we must not split a word in half -> restart
                                    if (autoScale && lastWhiteSpace == -1)
                                    {
                                        break;
                                    }

                                    // remove characters and add them again to next line
                                    int numCharsToRemove = lastWhiteSpace == -1 ? 1 : i - lastWhiteSpace;

                                    currentLine.RemoveRange(currentLine.Count - numCharsToRemove, numCharsToRemove);

                                    if (currentLine.Count == 0)
                                    {
                                        break;
                                    }
                                    i -= numCharsToRemove;
                                }
                                else
                                {
                                    if (autoScale) break;
                                    currentLine.RemoveAt(currentLine.Count - 1);

                                    // continue with next line, if there is one
                                    while (i < numChars - 1 && text[i] != CHAR_NEWLINE)
                                    {
                                        ++i;
                                    } 
                                }

                                lineFull = true;
                            }
                        }

                        if (i == numChars - 1)
                        {
                            sLines.Add(currentLine);
                            finished = true;
                        }
                        else if (lineFull)
                        {
                            sLines.Add(currentLine);

                            if (lastWhiteSpace == i)
                            {
                                currentLine.RemoveAt(currentLine.Count - 1);
                            }

                            if (currentY + leading + 2 * LineHeight <= containerHeight)
                            {
                                currentLine = new List<CharLocation>();
                                currentX = 0;
                                currentY += LineHeight + leading;
                                lastWhiteSpace = -1;
                                lastCharId = -1;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                if (autoScale && !finished && fontSize > 3)
                {
                    fontSize -= 1;
                }
                else
                {
                    finished = true;
                }
            }

            List<CharLocation> finalLocations = new List<CharLocation>();
            int numLines = sLines.Count;
            float bottom = currentY + LineHeight;
            int yOffset = 0;

            if (vAlign == VAlign.Bottom)
            {
                yOffset = (int)(containerHeight - bottom);
            }
            else if (vAlign == VAlign.Center)
            {
                yOffset = (int)((containerHeight - bottom) / 2);
            }

            List<CharLocation> line;
            for (int lineID = 0; lineID < numLines; ++lineID)
            {
                line = sLines[lineID];
                numChars = line.Count;
                if (numChars == 0)
                {
                    continue;
                }

                int xOffset = 0;
                CharLocation lastLocation = line[line.Count - 1];
                float right = lastLocation.X - lastLocation.Char.XOffset + lastLocation.Char.XAdvance;

                if (hAlign == HAlign.Right)
                {
                    xOffset = (int)(containerWidth - right);
                }
                else if (hAlign == HAlign.Center)
                {
                    xOffset = (int)((containerWidth - right) / 2);
                }
                
                for (int c = 0; c < numChars; ++c)
                {
                    charLocation = line[c];
                    charLocation.X = scale * (charLocation.X + xOffset + OffsetX) + Padding;
                    charLocation.Y = scale * (charLocation.Y + yOffset + OffsetY) + Padding;
                    charLocation.Scale = scale;

                    if (charLocation.Char.Width > 0 && charLocation.Char.Height > 0)
                    {
                        finalLocations.Add(charLocation);
                    }
                }
            }
                
            return finalLocations;
        }

        public TextureSmoothing Smoothing
        {
            get { return _helperImage.TextureSmoothing; }
            set { _helperImage.TextureSmoothing = value; }
        }
    }
}

