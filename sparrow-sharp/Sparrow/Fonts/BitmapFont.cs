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

        public BitmapFont()
        {

        }

        private bool ParseFontData(Stream data)
        {
            if (_fontTexture == null)
            {
                throw InvalidOperationException("Font parsing requires texture to be set");
            }

            XmlDocument parser = new XmlDocument();
            parser.Load(data);

            float scale = _fontTexture.Scale;

            XmlNodeList chars = parser.GetElementsByTagName("char");
            for (int i = 0; i < chars.Count; i++)
            {
                XmlAttributeCollection attributes = chars[i].Attributes;

                int charId = Convert.ToInt32(attributes["id"].Value);

                float x = Convert.ToSingle(attributes["x"]);
                float y = Convert.ToSingle(attributes["y"]);
                float width = Convert.ToSingle(attributes["width"]);
                float height = Convert.ToSingle(attributes["height"]);

                Rectangle region = new Rectangle(x / scale + _fontTexture.Frame.X, y / scale + _fontTexture.Frame.X, width / scale, height / scale);

                SubTexture texture = new SubTexture();

            }

            return false;
        }
        //        <font>
        //        <info face="BranchingMouse" size="40" />
        //            <common lineHeight="40" />
        //            <pages>  <!-- currently, only one page is supported -->
        //        <page id="0" file="texture.png" />
        //            </pages>
        //            <chars>
        //            <char id="32" x="60" y="29" width="1" height="1" xoffset="0" yoffset="27" xadvance="8" />
        //            <char id="33" x="155" y="144" width="9" height="21" xoffset="0" yoffset="6" xadvance="9" />
        //            </chars>
        //            <kernings> <!-- Kerning is optional -->
        //            <kerning first="83" second="83" amount="-4"/>
        //            </kernings>
        //        </font>
    }
}

