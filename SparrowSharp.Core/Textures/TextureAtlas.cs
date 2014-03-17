using System;
using Sparrow.Geom;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sparrow.Textures
{

	/**
	 A texture atlas is a collection of many smaller textures in one big image. The class
	 TextureAtlas is used to access textures from such an atlas.
	 
	 Using a texture atlas for your textures solves two problems:
	 
	 - Whenever you switch between textures, the batching of image objects is disrupted.
	 - Some OpenGL textures need to have side lengths that are powers of two. Sparrow hides this
	   limitation from you, but you will nevertheless use more memory if you do not follow that rule.
	 
	 By using a texture atlas, you avoid both texture switches and the power-of-two limitation. All 
	 textures are within one big "super-texture", and Sparrow takes care that the correct part of this 
	 texture is displayed.
	 
	 There are several ways to create a texture atlas. One is to use the atlas generator script that
	 is provided with Sparrow-objective C. Here is a sample on how to use it:
	 
		# creates "atlas.xml" and "atlas.png" from the	provided images 
		./generate_atlas.rb *.png output/atlas.xml
	 
	 The atlas generator can be found in the 'utils' directory in the Sparrow package. A README file
	 shows you how to install and use it. If you want to have more control over your atlas, you will
	 find great alternative tools on the Internet, like [Texture Packer](http://www.texturepacker.com).
	 
	 Whatever tool you use, Sparrow expects the following file format:

		<TextureAtlas imagePath='atlas.png'>
		  <SubTexture name='texture_1' x='0'  y='0' width='50' height='50'/>
		  <SubTexture name='texture_2' x='50' y='0' width='20' height='30'/> 
		</TextureAtlas>
	 
	 If your images have transparent areas at their edges, you can make use of the 'frame' property
	 of 'Texture'. Trim the texture by removing the transparent edges and specify the original 
	 texture size like this:

		<SubTexture name='trimmed' x='0' y='0' height='10' width='10'
		            frameX='-10' frameY='-10' frameWidth='30' frameHeight='30'/>
	*/
	public class TextureAtlas
	{
		private readonly Texture _atlasTexture;
		private readonly Dictionary<string, TextureInfo> _textureInfos;

		/// The number of available subtextures.
		public int NumTextures{ get {return _textureInfos.Count;}}

		/// All texture names of the atlas, sorted alphabetically.
        public List<string> Names { get { return GetNamesStartingWith(null); } }

		/// All textures of the atlas, sorted alphabetically.
        public List<Texture> Textures { get { return GetTexturesStartingWith(null); } }

		/// The base texture that makes up the atlas.
		public Texture Texture{ get {return _atlasTexture;}}

		/// Initializes a texture atlas from an XML file and a custom texture.
		public TextureAtlas (string xml, Texture texture) {
            _textureInfos = new Dictionary<string, TextureInfo>();
			_atlasTexture = texture;
            ParseAtlasXml(xml);
		}

        /// Initializes a teture atlas from a texture. Add the regions manually with 'AddRegion'.
		public TextureAtlas (Texture texture) : this(null, texture) {}

		/// Retrieve a subtexture by name. Returns 'null' if it is not found.
		public SubTexture GetTextureByName(string name) {
			TextureInfo info = _textureInfos[name];
		    if (info != null)
			{
                return new SubTexture(info.Region, info.Frame, info.Rotated, _atlasTexture);
			}
		    return null;
		}

		/// The region rectangle associated with a specific name.
		public Rectangle GetRegionByName(string name) {
			return _textureInfos[name].Region;
		}

		/// The frame rectangle of a specific region, or 'null' if that region has no frame.
		public Rectangle GetFrameByName(String name) {
			return _textureInfos[name].Frame;
		}

		/// Returns all textures that start with a certain string, sorted alphabetically
		/// (especially useful for 'MovieClip').
        public List<Texture> GetTexturesStartingWith(string prefix)
        {
            List<string> names = GetNamesStartingWith(prefix);
            List<Texture> textures = new List<Texture>(names.Count);
			for (int i = 0; i < names.Count; i++) {
				textures[i] = GetTextureByName(names[i]);
			}
			return textures;
		}

		/// Returns all texture names that start with a certain string, sorted alphabetically.
        public List<string> GetNamesStartingWith(string prefix)
        {
			List<string> names = new List<string>();
			if (prefix != null) {
			    names.AddRange(_textureInfos.Keys.Where(name => name.StartsWith(prefix)));
			} else {
				names.AddRange(_textureInfos.Keys);
			}
			names.Sort (); // todo check if default comparer sorts correctly
			return names;
		}

		/// Creates a region for a subtexture and gives it a name.
		public void AddRegion(Rectangle region, string name) {
			AddRegion(region, name, null, false);
		}

		/// Creates a region for a subtexture with a frame and gives it a name.
		public void AddRegion(Rectangle region, string name, Rectangle frame) {
			AddRegion(region, name, frame, false);
		}

		/// Creates a region for a subtexture with a frame and gives it a name. If 'rotated' is 'true',
		/// the subtexture will show the region rotated by 90 degrees (CCW).
		public void AddRegion(Rectangle region, string name, Rectangle frame, bool rotated) {
			TextureInfo info = new TextureInfo(region, frame, rotated);
			_textureInfos[name] = info;
		}

		/// Removes a region with a certain name.
		public void RemoveRegion(string name) {
			_textureInfos.Remove (name);
		}

		protected void ParseAtlasXml(string xmlString)
		{
		    XDocument xml = XDocument.Parse(xmlString);
		    bool parsed = false;
            foreach (var subTexture in xml.Descendants("SubTexture"))
            {
                float scale = _atlasTexture.Scale;
                float x = (float)subTexture.Element("x") / scale;
                float y = (float)subTexture.Element("y") / scale;
                float width = (float)subTexture.Element("width") / scale;
                float height = (float)subTexture.Element("height") / scale;
                float frameX = (float)subTexture.Element("frameX") / scale;
                float frameY = (float)subTexture.Element("frameY") / scale;
                float frameWidth = (float)subTexture.Element("frameWidth") / scale;
                float frameHeight = (float)subTexture.Element("frameHeight") / scale;
                bool rotated = (bool)subTexture.Element("rotated");
                string name = subTexture.Element("name").Value;

                Rectangle region = new Rectangle(x, y, width, height);
                Rectangle frame = null;
                if (frameWidth > 0.0f && frameHeight > 0.0f)
                {
                    frame = new Rectangle(frameX, frameY, frameWidth, frameHeight);
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

	}
}

