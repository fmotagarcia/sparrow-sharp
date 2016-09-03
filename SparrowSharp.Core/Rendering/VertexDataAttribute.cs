
using System;
using System.Collections.Generic;

namespace SparrowSharp.Core.Rendering
{
    /** Holds the properties of a single attribute in a VertexDataFormat instance.
     *  The member variables must never be changed; they are only <code>public</code>
     *  for performance reasons. */
    public class VertexDataAttribute
    {
        private static readonly Dictionary<string, int> FORMAT_SIZES = new Dictionary<string, int>
        {
            {"bytes4", 4 },
            {"float1", 4 },
            {"float2", 8 },
            {"float3", 12 },
            {"float4", 16 }
        };

        public string Name;
        public string Format;
        public bool IsColor;
        public int Offset; // in bytes
        public int Size;   // in bytes

        /** Creates a new instance with the given properties. */
        public VertexDataAttribute(string name, string format, int offset)
        {
            if (!(FORMAT_SIZES.ContainsKey(format)))
                throw new ArgumentException(
                    "Invalid attribute format: " + format + ". " +
                    "Use one of the following: 'float1'-'float4', 'bytes4'");

            this.Name = name;
            this.Format = format;
            this.Offset = offset;
            this.Size = FORMAT_SIZES[format];
            this.IsColor = name.Contains("color") || name.Contains("Color");
        }
    }
}
