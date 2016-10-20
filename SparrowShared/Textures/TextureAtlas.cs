using System;
using System.Collections.Generic;
using System.Xml;
using Sparrow.Geom;

namespace Sparrow.Textures
{
    /// <summary>
    /// A texture atlas is a collection of many smaller textures in one big image. The class
    /// TextureAtlas is used to access textures from such an atlas.
    /// 
    /// Using a texture atlas for your textures solves two problems:
    /// 
    /// - Whenever you switch between textures, the batching of image objects is disrupted.
    /// - Some OpenGL textures need to have side lengths that are powers of two. Sparrow hides this
    /// limitation from you, but you will nevertheless use more memory if you do not follow that rule.
    /// 
    /// By using a texture atlas, you avoid both texture switches and the power-of-two limitation. All 
    /// textures are within one big "super-texture", and Sparrow takes care that the correct part of this 
    /// texture is displayed.
    /// 
    /// There are several ways to create a texture atlas. One is to use the atlas generator script that
    /// is provided with Sparrow-objective C. Here is a sample on how to use it:
    /// 
    /// # creates "atlas.xml" and "atlas.png" from the  provided images 
    /// ./generate_atlas.rb *.png output/atlas.xml
    /// 
    /// The atlas generator can be found in the 'utils' directory in the Sparrow package. A README file
    /// shows you how to install and use it. If you want to have more control over your atlas, you will
    /// find great alternative tools on the Internet, like Texture Packer(http://www.texturepacker.com).
    /// 
    /// Whatever tool you use, Sparrow expects the following file format:
    /// 
    /// <TextureAtlas imagePath='atlas.png'>
    ///     <SubTexture name='texture_1' x='0'  y='0' width='50' height='50'/>
    ///     <SubTexture name='texture_2' x='50' y='0' width='20' height='30'/> 
    /// </TextureAtlas>
    /// 
    /// If your images have transparent areas at their edges, you can make use of the 'frame' property
    /// of 'Texture'. Trim the texture by removing the transparent edges and specify the original 
    /// texture size like this:
    /// 
    /// <SubTexture name='trimmed' x='0' y='0' height='10' width='10'
    ///             frameX='-10' frameY='-10' frameWidth='30' frameHeight='30'/>
    /// </summary>
    public class TextureAtlas
    {
        
        private readonly Texture _atlasTexture;
        private readonly Dictionary<string, TextureInfo> _textureInfos;

        /// <summary>
        /// The number of available subtextures.
        /// </summary>
        /// <value>The number textures.</value>
        public int NumTextures{ get { return _textureInfos.Count; } }

        /// <summary>
        /// All texture names of the atlas, sorted alphabetically.
        /// </summary>
        public List<string> Names { get { return GetNames(null); } }

        /// <summary>
        /// All textures of the atlas, sorted alphabetically.
        /// </summary>
        public List<Texture> Textures { get { return GetTextures(null); } }

        /// <summary>
        /// The base texture that makes up the atlas.
        /// </summary>
        public Texture Texture{ get { return _atlasTexture; } }

        /// <summary>
        /// Initializes a teture atlas from a texture. Add the regions manually with 'AddRegion'.
        /// </summary>
        public TextureAtlas(Texture texture) : this(null, texture)
        {
        }

        /// <summary>
        /// Initializes a texture atlas from an XML file and a custom texture.
        /// </summary>
        public TextureAtlas(string xml, Texture texture)
        {
            _textureInfos = new Dictionary<string, TextureInfo>();
            _atlasTexture = texture;
            ParseAtlasXml(xml);
        }

        protected void ParseAtlasXml(string xmlString)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlString);

            bool parsed = false;

            XmlNodeList subTextures = xml.SelectNodes("TextureAtlas/SubTexture");
            foreach (XmlNode subTexture in subTextures)
            {
                float scale = _atlasTexture.Scale;
                float x = GetFloat(subTexture, "x") / scale;
                float y = GetFloat(subTexture, "y") / scale;
                float width = GetFloat(subTexture, "width") / scale;
                float height = GetFloat(subTexture, "height") / scale;
                float frameX = GetFloat(subTexture, "frameX") / scale;
                float frameY = GetFloat(subTexture, "frameY") / scale;
                float frameWidth = GetFloat(subTexture, "frameWidth") / scale;
                float frameHeight = GetFloat(subTexture, "frameHeight") / scale;
                bool rotated = GetBool(subTexture, "rotated");
                string name = GetString(subTexture, "name");

                Rectangle region = Rectangle.Create(x, y, width, height);
                Rectangle frame = null;
                if (frameWidth > 0.0f && frameHeight > 0.0f)
                {
                    frame = Rectangle.Create(frameX, frameY, frameWidth, frameHeight);
                }
                AddRegion(region, name, frame, rotated);
                parsed = true;
            }

            // TODO suppport texture atlases that specify the path to the image
            //if (!parsed)  
            //{
            //    foreach (var subTexture in xml.Descendants("TextureAtlas"))
            //    {
            //        string name = subTexture.Element("imagePath").Value;
            //        parsed = true;
            //    }   
            //}
            if (!parsed)
            {
                throw new Exception("could not parse texture atlas");
            }
        }

        /// <summary>
        /// Retrieve a subtexture by name. Returns 'null' if it is not found.
        /// </summary>
        public SubTexture GetTexture(string name)
        {
            TextureInfo info = _textureInfos[name];
            if (info != null)
            {
                return new SubTexture(_atlasTexture, info.Region, false, info.Frame, info.Rotated);
            }
            return null;
        }
        
        /// <summary>
        /// Returns all textures that start with a certain string, sorted alphabetically
        /// (especially useful for 'MovieClip').
        /// </summary>
        public List<Texture> GetTextures(string prefix)
        {
            List<string> names = GetNames(prefix);
            List<Texture> textures = new List<Texture>();
            foreach (string name in names)
            {
                textures.Add(GetTexture(name));
            }
            return textures;
        }

        /// <summary>
        /// Returns all texture names that start with a certain string, sorted alphabetically.
        /// </summary>
        public List<string> GetNames(string prefix)
        {
            List<string> names = new List<string>();
            if (prefix != null)
            {
                foreach (string name in _textureInfos.Keys)
                {
                    if (name.IndexOf(prefix) == 0)
                    {
                        names.Add(name);
                    }
                }
            }
            else
            {
                names.AddRange(_textureInfos.Keys);
            }

            names.Sort(new AlphanumComparatorFast());

            return names;
        }

        /// <summary>
        /// The region rectangle associated with a specific name.
        /// </summary>
        public Rectangle GetRegion(string name)
        {
            return _textureInfos[name].Region;
        }

        /// <summary>
        /// The frame rectangle of a specific region, or 'null' if that region has no frame.
        /// </summary>
        public Rectangle GetFrame(String name)
        {
            return _textureInfos[name].Frame;
        }

        /// <summary>
        /// Creates a region for a subtexture with a frame and gives it a name. If 'rotated' is 'true',
        /// the subtexture will show the region rotated by 90 degrees (CCW).
        /// </summary>
        public void AddRegion(Rectangle region, string name, Rectangle frame = null, bool rotated = false)
        {
            TextureInfo info = new TextureInfo(region, frame, rotated);
            _textureInfos[name] = info;
        }

        /// <summary>
        /// Removes a region with a certain name.
        /// </summary>
        public void RemoveRegion(string name)
        {
            _textureInfos.Remove(name);
        }

        float GetFloat(XmlNode node, string attribute)
        {
            float result = 0;
            if (node.Attributes.GetNamedItem(attribute) != null)
            {
                result = float.Parse(node.Attributes.GetNamedItem(attribute).Value);
            }
            return result;
        }

        bool GetBool(XmlNode node, string attribute)
        {
            bool result = false;
            if (node.Attributes.GetNamedItem(attribute) != null)
            {
                result = bool.Parse(node.Attributes.GetNamedItem(attribute).Value);
            }
            return result;
        }

        string GetString(XmlNode node, string attribute)
        {
            string result = "";
            if (node.Attributes.GetNamedItem(attribute) != null)
            {
                result = node.Attributes.GetNamedItem(attribute).Value;
            }
            return result;
        }
    }

    public class AlphanumComparatorFast : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null)
            {
                return 0;
            }

            if (y == null)
            {
                return 0;
            }

            int len1 = x.Length;
            int len2 = y.Length;
            int marker1 = 0;
            int marker2 = 0;

            // Walk through two the strings with two markers.
            while (marker1 < len1 && marker2 < len2)
            {
                char ch1 = x[marker1];
                char ch2 = y[marker2];

                // Some buffers we can build up characters in for each chunk.
                char[] space1 = new char[len1];
                int loc1 = 0;
                char[] space2 = new char[len2];
                int loc2 = 0;

                // Walk through all following characters that are digits or
                // characters in BOTH strings starting at the appropriate marker.
                // Collect char arrays.
                do
                {
                    space1[loc1++] = ch1;
                    marker1++;

                    if (marker1 < len1)
                    {
                        ch1 = x[marker1];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                do
                {
                    space2[loc2++] = ch2;
                    marker2++;

                    if (marker2 < len2)
                    {
                        ch2 = y[marker2];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                // If we have collected numbers, compare them numerically.
                // Otherwise, if we have strings, compare them alphabetically.
                string str1 = new string(space1);
                string str2 = new string(space2);

                int result;

                if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                {
                    int thisNumericChunk = int.Parse(str1);
                    int thatNumericChunk = int.Parse(str2);
                    result = thisNumericChunk.CompareTo(thatNumericChunk);
                }
                else
                {
                    result = str1.CompareTo(str2);
                }

                if (result != 0)
                {
                    return result;
                }
            }
            return len1 - len2;
        }
    }
}