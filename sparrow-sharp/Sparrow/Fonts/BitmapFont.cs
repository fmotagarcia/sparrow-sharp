using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Sparrow.Core;
using System.Xml;
using System.IO;
using Sparrow.Geom;

namespace Sparrow.Fonts
{
    public class BitmapFont
    {
        private string _name;
        private Texture _fontTexture;
        private Dictionary<int, BitmapChar> _chars;
        private float _size;
        private float _lineHeight;
        private float _baseline;
        private float _helperImage;
        private Texture.TextureSmoothing _smoothing;

        public BitmapFont()
        {

        }

        private Texture.TextureSmoothing Smoothing()
        {
            return _fontTexture.Smoothing;
        }

        private Texture TextureReferencedByXmlData(Stream data, string folder)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(data);

            Texture texture = null;

            XmlNodeList pageNodes = xml.GetElementsByTagName("page");
            for (int i = 0; i < pageNodes.Count; i++)
            {
                XmlAttributeCollection attributes = pageNodes[i].Attributes;
                int id = Convert.ToInt32(attributes["id"]);
                if (id != 0)
                {
                    // TODO: add support for multiple pages?
                    throw new InvalidOperationException("Bitmap fonts with multiple pages are not supported");
                }

                string filename = attributes["file"].Value;
                string absolutePath = Path.Combine(folder, filename);

                texture = new Texture(absolutePath);
            }

            if (texture == null)
            {
                throw new InvalidDataException("Font XML did not contain path to texture");
            }

            return texture;
        }

        private bool ParseFontData(Stream data)
        {
            if (_fontTexture == null)
            {
                throw new InvalidOperationException("Font parsing requires texture to be set");
            }

            XmlDocument xml = new XmlDocument();
            xml.Load(data);

            float scale = _fontTexture.Scale;
            ParseAndLoadChars(xml, scale);
            ParseAndLoadKerning(xml, scale);
            ParseAndLoadInfo(xml);
            ParseAndLoadCommon(xml);

            return false;
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

                Rectangle region = new Rectangle(x / scale + _fontTexture.Frame.X, y / scale + _fontTexture.Frame.X, width / scale, height / scale);
                SubTexture texture = new SubTexture(region, _fontTexture);

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
                    _smoothing = Texture.TextureSmoothing.None;
                }
            }
        }

        private void ParseAndLoadCommon(XmlDocument xml)
        {
            XmlNodeList commonNode = xml.GetElementsByTagName("common");
            if (commonNode.Count > 0)
            {
                XmlAttributeCollection attributes = commonNode[0].Attributes;
                _lineHeight = Convert.ToSingle(attributes["lineHeight"]);
                _baseline = Convert.ToSingle(attributes["base"]);
            }
        }
    }
}

