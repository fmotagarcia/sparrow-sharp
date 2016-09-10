using System;
using System.Collections.Generic;

namespace SparrowSharp.Core.Rendering
{
    /** Describes the memory layout of VertexData instances, as used for every single vertex.
     *
     *  <p>The format is set up via a simple String. Here is an example:</p>
     *
     *  <listing>
     *  format = VertexDataFormat.fromString("position:float2, color:bytes4");</listing>
     *
     *  <p>This String describes two attributes: "position" and "color". The keywords after
     *  the colons depict the format and size of the data that each attribute uses; in this
     *  case, we store two floats for the position (taking up the x- and y-coordinates) and four
     *  bytes for the color. (The available formats are the same as those defined in the
     *  <code>Context3DVertexBufferFormat</code> class:
     *  <code>float1, float2, float3, float4, bytes4</code>.)</p>
     *
     *  <p>You cannot create a VertexData instance with its constructor; instead, you must use the
     *  static <code>fromString</code>-method. The reason for this behavior: the class maintains
     *  a cache, and a call to <code>fromString</code> will return an existing instance if an
     *  equivalent format has already been created in the past. That saves processing time and
     *  memory.</p>
     *
     *  <p>VertexDataFormat instances are immutable, i.e. they are solely defined by their format
     *  string and cannot be changed later.</p>
     *
     *  @see VertexData
     */
    public class VertexDataFormat
    {
        /*
        private string _format;
        private int _vertexSize;
        private List<VertexDataAttribute> _attributes;

        // format cache
        private static Dictionary<string, VertexDataFormat> sFormats = new Dictionary<string, VertexDataFormat>();

        //don't use the constructor, but call <code>vertexdataformat.fromstring</code> instead.
        //this allows for efficient format caching.
        public VertexDataFormat()
        {
            _attributes = new List<VertexDataAttribute>();
        }

        // Creates a new VertexDataFormat instance from the given String, or returns one from
        // the cache(if an equivalent String has already been used before).
        //
        //  @param format
        //
        //  Describes the attributes of each vertex, consisting of a comma-separated
        // list of attribute names and their format, e.g.:
        //
        //  <pre>"position:float2, texCoords:float2, color:bytes4"</pre>
        //
        //  <p>This set of attributes will be allocated for each vertex, and they will be
        // stored in exactly the given order.</p>
        //
        //  <ul>
        //    <li>Names are used to access the specific attributes of a vertex.They are
        //        completely arbitrary.</li>
        //    <li>The available formats can be found in the<code> Context3DVertexBufferFormat</code>
        //        class in the<code> flash.display3D</code> package.</li>
        //    <li>Both names and format strings are case-sensitive.</li>
        //    <li>Always use <code>bytes4</code> for color data that you want to access with the
        // respective methods.</li>
        //    <li>Furthermore, the attribute names of colors should include the string "color"
        //        (or the uppercase variant). If that's the case, the "alpha" channel of the color
        //        will automatically be initialized with "1.0" when the VertexData object is
        //        created or resized.</li>
        //  </ul>
        public static VertexDataFormat FromString(string format)
        {
            if (sFormats.ContainsKey(format))
            {
                return sFormats[format];
            }
            else
            {
                VertexDataFormat instance = new VertexDataFormat();
                instance.ParseFormat(format);

                string normalizedFormat = instance._format;

                if (sFormats.ContainsKey(normalizedFormat))
                {
                    instance = sFormats[normalizedFormat];
                }

                sFormats[format] = instance;
                sFormats[normalizedFormat] = instance;

                return instance;
            }
        }

        // Creates a new VertexDataFormat instance by appending the given format string
        // to the current instance's format. 
        public VertexDataFormat Extend(string format)
        {
            return FromString(_format + ", " + format);
        }

        // query methods

        // Returns the size of a certain vertex attribute in bytes.
        public int GetSize(string attrName)
        {
            return GetAttribute(attrName).Size;
        }

        // Returns the size of a certain vertex attribute in 32 bit units.
        public int GetSizeIn32Bits(string attrName)
        {
            return GetAttribute(attrName).Size / 4;
        }

        // Returns the offset(in bytes) of an attribute within a vertex.
        public int GetOffset(string attrName)
        {
            return GetAttribute(attrName).Offset;
        }

        /// Returns the offset (in 32 bit units) of an attribute within a vertex. 
        public int GetOffsetIn32Bits(string attrName)
        {
            return GetAttribute(attrName).Offset / 4;
        }

        // Returns the format of a certain vertex attribute, identified by its name.
        // Typical values: <code>float1, float2, float3, float4, bytes4</code>.
        public string GetFormat(string attrName)
        {
            return GetAttribute(attrName).Format;
        }

        // returns the name of the attribute at the given position within the vertex format. 
        public string GetName(int attrIndex)
        {
            return _attributes[attrIndex].Name;
        }

        // Indicates if the format contains an attribute with the given name. 
        public bool HasAttribute(string attrName)
        {
            int numAttributes = _attributes.Count;

            for (int i = 0; i < numAttributes; ++i)
            {
                if (_attributes[i].Name == attrName)
                {
                    return true;
                }
            }
            return false;
        }

        // context methods

       // * Specifies which vertex data attribute corresponds to a single vertex shader
       //  * program input.This wraps the<code> Context3D</code>-method with the same name,

       //* automatically replacing<code> attrName</code> with the corresponding values for
       //  *  <code>bufferOffset</code> and <code>format</code>.
        public void SetVertexBufferAt(int index, VertexBuffer3D buffer, string attrName)
        {
            VertexDataAttribute attribute = GetAttribute(attrName);
            Starling.context.setVertexBufferAt(index, buffer, attribute.Offset / 4, attribute.Format);
        }

        // parsing

        private void ParseFormat(string format)
        {
            if (format != null && format != "")
            {
                _attributes.Clear();
                _format = "";

                string[] parts = format.Split(',');
                int numParts = parts.Length;
                int offset = 0;

                for (int i = 0; i < numParts; ++i)
                {
                    string attrDesc = parts[i];
                    string[] attrParts = attrDesc.Split(':');

                    if (attrParts.Length != 2)
                    {
                        throw new ArgumentException("Missing colon: " + attrDesc);
                    }

                    string attrName = attrParts[0].Trim();
                    string attrFormat = attrParts[0].Trim();

                    if (attrName.Length == 0 || attrFormat.Length == 0)
                    {
                        throw new ArgumentException(("Invalid format string: " + attrDesc));
                    }

                    VertexDataAttribute attribute = new VertexDataAttribute(attrName, attrFormat, offset);

                    offset += attribute.Size;

                    _format += (i == 0 ? "" : ", ") + attribute.Name + ":" + attribute.Format;
                    _attributes.Add(attribute);
                }

                _vertexSize = offset;
            }
            else
            {
                _format = "";
            }
        }

        // Returns the normalized format string. 
        override public string ToString()
        {
            return _format;
        }

        // internal methods

        // @private 
        internal VertexDataAttribute GetAttribute(string attrName)
        {
            int i;
            VertexDataAttribute attribute;
            int numAttributes = _attributes.Count;

            for (i=0; i<numAttributes; ++i)
            {
                attribute = _attributes[i];
                if (attribute.Name == attrName) return attribute;
            }
            return null;
        }

        // @private 
        public List<VertexDataAttribute> Attributes { get { return _attributes; } }

        // properties

        // Returns the normalized format string. 
        public string FormatString { get { return _format;} }

        // The size (in bytes) of each vertex.
        public int VertexSize { get { return _vertexSize; } }

        // The size (in 32 bit units) of each vertex. 
        public int VertexSizeIn32Bits { get { return _vertexSize / 4; } }

        // The number of attributes per vertex. 
        public int NumAttributes { get { return _attributes.Count; } }
        */
    }
}
