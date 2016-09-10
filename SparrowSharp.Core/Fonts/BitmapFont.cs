using System;
using System.Collections.Generic;
using Sparrow.Textures;
using System.Xml;
using System.IO;
using Sparrow.Geom;
using Sparrow.Display;
using SparrowSharp.Core;
using Sparrow.ResourceLoading;

namespace SparrowSharp.Fonts
{
    /// <summary>
    /// The BitmapFont class parses bitmap font files and arranges the glyphs in the form of a text.
    /// 
    /// The class parses the XML format as it is used in the AngelCode Bitmap Font Generator. This is what
    /// the file format looks like:
    /// 
    /// <font>
    /// <info face="BranchingMouse" size="40" />
    ///    <common lineHeight="40" />
    ///    <pages>  <!-- currently, only one page is supported -->
    ///     <page id="0" file="texture.png" />
    ///    </pages>
    ///    <chars>
    ///       <char id="32" x="60" y="29" width="1" height="1" xoffset="0" yoffset="27" xadvance="8" />
    ///       <char id="33" x="155" y="144" width="9" height="21" xoffset="0" yoffset="6" xadvance="9" />
    ///    </chars>
    ///    <kernings> <!-- Kerning is optional -->
    ///       <kerning first="83" second="83" amount="-4"/>
    ///    </kernings>
    /// </font>
    /// 
    ///     _You don't have to use this class directly in most cases. TextField contains methods that
    ///    handle bitmap fonts for you._
    /// </summary>
    public class BitmapFont
    {
        /*
        private const uint MAX_TEXT_CHAR_COUNT = 8192;
        private Image _helperImage;
        private Texture _fontTexture;
        private Dictionary<int, BitmapChar> _chars;
        private string _name;
        public string Name { get { return _name; } }
        private float _size;
        private float _lineHeight;
        private float _baseline;

        /// <summary>
        /// The smoothing filter used for the texture.
        /// </summary>
        public TextureSmoothing Smoothing
        {
            get { return _fontTexture.Smoothing; }
            private set { _fontTexture.Smoothing = value; }
        }

        /// <summary>
        /// Initializes a new instance with an embedded mini font. This font's characters have a height of 5 pixels and a maximum
        /// width of 5 pixels.
        /// </summary>
        public BitmapFont()
        {
            Stream fontTextureData = new MemoryStream(Convert.FromBase64String(MiniFont.MiniFontImageDataBase64));
            MemoryStream fontXmlData = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(MiniFont.FontXML));

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
            TextureLoader texLoader = new TextureLoader();
            GLTexture tex = texLoader.LoadFromStream(fontTextureData);
            _fontTexture = new SubTexture(tex);

            _chars = new Dictionary<int,BitmapChar>();
            _helperImage = new Image(_fontTexture);

            XmlDocument xml = new XmlDocument();
            xml.Load(fontXmlData);

            float scale = _fontTexture.Scale;
            ParseAndLoadChars(xml, scale);
            ParseAndLoadKerning(xml, scale);
            ParseAndLoadInfo(xml);
            ParseAndLoadCommon(xml);
        }

        /// <summary>
        ///  Draws text into a quad batch.
        /// </summary>
        public void FillQuadBatch(QuadBatch quadBatch, float width, float height, string text, float size, uint color, HAlign hAlign, VAlign vAlign, bool autoScale, bool kerning)
        {
            List<CharLocation> charLocations = ArrangeCharsInArea(width, height, text, size, hAlign, vAlign, autoScale, kerning);
            _helperImage.Color = color;

            if (charLocations.Count > MAX_TEXT_CHAR_COUNT)
            {
                throw new InvalidDataException(string.Format("Bitmap font text is limited to {0} characters", MAX_TEXT_CHAR_COUNT));
            }

            CharLocation charLocation;
            for (int i = 0; i < charLocations.Count; i++)
            {
                charLocation = charLocations[i];
                _helperImage.Texture = charLocation.BitmapChar.Texture;
                _helperImage.X = charLocation.X;
                _helperImage.Y = charLocation.Y;
                _helperImage.ScaleX = _helperImage.ScaleY = charLocation.Scale;
                _helperImage.ReadjustSize();
                quadBatch.AddQuad(_helperImage);
            }

        }

        private Texture TextureReferencedByXmlData(TextureLoader textureLoader, Stream fontXmlData)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(fontXmlData);

            Texture texture = null;
            XmlNodeList pageNodes = xml.GetElementsByTagName("page");
            for (int i = 0; i < pageNodes.Count; i++)
            {
                XmlAttributeCollection attributes = pageNodes[i].Attributes;
                int id = Convert.ToInt32(attributes["id"]);
                if (id != 0)
                {
                    throw new Exception("Bitmap fonts with multiple pages are not supported");
                }

                string filename = attributes["file"].Value;
                string absolutePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Resources), filename); // NSBundle.MainBundle.BundlePath
                texture = textureLoader.LoadLocalImage(absolutePath);
            }

            if (texture == null)
            {
                throw new InvalidDataException("Font XML doesn't contain path to texture");
            }

            return texture;
        }

        private void ParseAndLoadChars(XmlDocument xml, float scale)
        {
            XmlNodeList charNodes = xml.GetElementsByTagName("char");
            for (int i = 0; i < charNodes.Count; i++)
            {
                XmlAttributeCollection attributes = charNodes[i].Attributes;

                float x = Convert.ToSingle(attributes["x"].Value);
                float y = Convert.ToSingle(attributes["y"].Value);
                float width = Convert.ToSingle(attributes["width"].Value);
                float height = Convert.ToSingle(attributes["height"].Value);
                float frameX = 0;
                if (_fontTexture.Frame != null)
                {
                    frameX = _fontTexture.Frame.X;
                }
                float frameY = 0;
                if (_fontTexture.Frame != null)
                {
                    frameY = _fontTexture.Frame.Y;
                }

                Rectangle region = new Rectangle(x / scale + frameX, y / scale + frameY, width / scale, height / scale);
                SubTexture texture = new SubTexture(_fontTexture, region);

                int charId = Convert.ToInt32(attributes["id"].Value);
                float xOffset = Convert.ToSingle(attributes["xoffset"].Value);
                float yOffset = Convert.ToSingle(attributes["yoffset"].Value);
                float xAdvance = Convert.ToSingle(attributes["xadvance"].Value);

                BitmapChar bitmapChar = new BitmapChar(charId, texture, xOffset, yOffset, xAdvance);
                _chars.Add(charId, bitmapChar);
            }
        }

        private void ParseAndLoadKerning(XmlDocument xml, float scale)
        {
            XmlNodeList kerningNodes = xml.GetElementsByTagName("kerning");
            for (int i = 0; i < kerningNodes.Count; i++)
            {
                XmlAttributeCollection attributes = kerningNodes[i].Attributes;

                int first = Convert.ToInt32(attributes["first"].Value);
                int second = Convert.ToInt32(attributes["second"].Value);
                float amount = Convert.ToSingle(attributes["ammount"].Value) / scale;

                _chars[second].AddKerning(amount, first);
            }
        }

        private void ParseAndLoadInfo(XmlDocument xml)
        {
            XmlNodeList infoNodes = xml.GetElementsByTagName("info");
            if (infoNodes.Count > 0)
            {
                XmlAttributeCollection attributes = infoNodes[0].Attributes;

                _name = attributes["face"].Value;
                _size = Convert.ToSingle(attributes["size"].Value);

                if (attributes["smooth"].Value == "0")
                {
                    Smoothing = TextureSmoothing.None;
                }
            }
        }

        private void ParseAndLoadCommon(XmlDocument xml)
        {
            XmlNodeList commonNode = xml.GetElementsByTagName("common");
            if (commonNode.Count > 0)
            {
                XmlAttributeCollection attributes = commonNode[0].Attributes;
                _lineHeight = Convert.ToSingle(attributes["lineHeight"].Value);
                _baseline = Convert.ToSingle(attributes["base"].Value);
            }
        }

        /// <summary>
        /// Returns a single bitmap char with a certain character ID.
        /// </summary>
        public BitmapChar CharById(int charId)
        {
            BitmapChar ret;
            _chars.TryGetValue(charId, out ret);
            return ret;
        }

        private List<CharLocation> ArrangeCharsInArea(float width, float height, string text, float size, HAlign hAlign, VAlign vAlign, bool autoScale, bool kerning)
        {
            if (text.Length == 0)
            {
                return new List<CharLocation>();
            }

            if (size < 0)
            {
                size *= -_size;
            }

            bool isFinished = false;
            float scale = 0;
            float containerWidth = 0;
            float containerHeight = 0;

            List<List<CharLocation>> lines = new List<List<CharLocation>>();
            while (!isFinished)
            {
                scale = size / _size;
                containerWidth = width / scale;
                containerHeight = height / scale;

                if (_lineHeight <= containerHeight)
                {
                    int lastWhiteSpace = -1;
                    int lastCharId = -1;
                    int numChars = text.Length;
                    float currentX = 0;
                    float currentY = 0;
                    List<CharLocation> currentLine = new List<CharLocation>();

                    for (int i = 0; i < numChars; i++)
                    {
                        bool isLineFull = false;
                        int charId = text[i];
                        BitmapChar bitmapChar = CharById(charId);

                        if (charId == NewLineAsciiCode || charId == CarriageReturnAsciiCode)
                        {
                            isLineFull = true;
                        }
                        else if (bitmapChar == null)
                        {
                            Console.WriteLine("Missing character: " + charId);
                        }
                        else
                        {
                            if (charId == SpaceAsciiCode || charId == TabAsciiCode)
                            {
                                lastWhiteSpace = i;
                            }
                                
                            if (kerning)
                            {
                                currentX += bitmapChar.KerningToChar(lastCharId);
                            }
                            CharLocation charLocation = CharLocation.Create(bitmapChar, 1.0f, currentX + bitmapChar.XOffset, currentY + bitmapChar.YOffset);
                            currentLine.Add(charLocation);
                            currentX += bitmapChar.XAdvance;
                            lastCharId = charId;

                            if (charLocation.X + bitmapChar.Width > containerWidth)
                            {
                                int numCharsToRemove = (lastWhiteSpace == -1) ? 1 : i - lastWhiteSpace;
                                int removeIndex = currentLine.Count - numCharsToRemove;
                                currentLine.RemoveRange(removeIndex, numCharsToRemove);

                                if (currentLine.Count == 0)
                                {
                                    break;
                                }

                                i -= numCharsToRemove;
                                isLineFull = true;
                            }
                        }

                        if (i == numChars - 1)
                        {
                            lines.Add(currentLine);
                            isFinished = true;
                        }
                        else if (isLineFull)
                        {
                            lines.Add(currentLine);

                            if (lastWhiteSpace == i)
                            {
                                currentLine.RemoveAt(currentLine.Count - 1);
                            }

                            if (currentY + 2 * _lineHeight <= containerHeight)
                            {
                                currentLine = new List<CharLocation>();
                                currentX = 0;
                                currentY += _lineHeight;
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

                if (autoScale && !isFinished)
                {
                    size -= 1;
                    lines.Clear();
                }
                else
                {
                    isFinished = true;
                }
            }

            List<CharLocation> finalLocations = new List<CharLocation>();
            int numLines = lines.Count;
            float bottom = numLines * _lineHeight;
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
            for (int i = 0; i < lines.Count; i++)
            {
                line = lines[i];
                int numChars = line.Count;
                if (numChars == 0)
                {
                    continue;
                }

                int xOffset = 0;
                CharLocation lastLocation = line[line.Count - 1];
                float right = lastLocation.X - lastLocation.BitmapChar.XOffset + lastLocation.BitmapChar.XAdvance;

                if (hAlign == HAlign.Right)
                {
                    xOffset = (int)(containerWidth - right);
                }
                else if (hAlign == HAlign.Center)
                {
                    xOffset = (int)((containerWidth - right) / 2);
                }

                CharLocation charLocation;
                for (int j = 0; j < line.Count; j++)
                {
                    charLocation = line[j];
                    charLocation.X = scale * (charLocation.X + xOffset);
                    charLocation.Y = scale * (charLocation.Y + yOffset);
                    charLocation.Scale = scale;

                    if (charLocation.BitmapChar.Width > 0 && charLocation.BitmapChar.Height > 0)
                    {
                        finalLocations.Add(charLocation);
                    }
                }
            }
                
            return finalLocations;
        }

        #region ASCII Codes

        private const int SpaceAsciiCode = 32;
        private const int TabAsciiCode = 39;
        private const int NewLineAsciiCode = 10;
        private const int CarriageReturnAsciiCode = 13;

        #endregion
        */
    }
}

