using System;
using Sparrow.Core;
using System.Text;

namespace SparrowSharp.Filters
{
	// TODO: implement remaining methods
	internal class BlurProgram : Program
	{

		public static string GetProgramName(bool tinting)
		{
			if (tinting) return "SPBlurFilter#01";
			else return "SPBlurFilter#00";
		}
			
		public readonly bool Tinted;
		public readonly int APosition;
		public readonly int ATexCoords;
		public readonly int UOffsets;
		public readonly int UWeights;
		public readonly int UColor;
		public readonly int UMvpMatrix;

		public BlurProgram (bool isTinted) : base()
		{
			Init(GetVertexShader(), GetFragmentShader(isTinted));
			Tinted = isTinted;
			APosition = Attributes["aPosition"];
			ATexCoords = Attributes["aTexCoords"];
			UOffsets = Uniforms["uOffsets"];
			UWeights = Uniforms["uWeights"];
			if (isTinted) {
				UColor = Uniforms["uColor"];
			}
			UMvpMatrix = Uniforms["uMvpMatrix"];
		}

		private string GetVertexShader (){
			StringBuilder source = new StringBuilder("");

			// attributes
			source.AppendLine("attribute vec4 aPosition;");
			source.AppendLine("attribute lowp vec2 aTexCoords;");

			// uniforms
			source.AppendLine("uniform mat4 uMvpMatrix;");
			source.AppendLine("uniform lowp vec4 uOffsets;");

			// varying
			source.AppendLine("varying lowp vec2 v0;");
			source.AppendLine("varying lowp vec2 v1;");
			source.AppendLine("varying lowp vec2 v2;");
			source.AppendLine("varying lowp vec2 v3;");
			source.AppendLine("varying lowp vec2 v4;");

			// main
			source.AppendLine("void main() {");

			source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");     // 4x4 matrix transform to output space
			source.AppendLine("  v0 = aTexCoords;");                          // pos:  0 |
			source.AppendLine("  v1 = aTexCoords - uOffsets.zw;");            // pos: -2 |
			source.AppendLine("  v2 = aTexCoords - uOffsets.xy;");            // pos: -1 | --> kernel positions
			source.AppendLine("  v3 = aTexCoords + uOffsets.xy;");            // pos: +1 |     (only 1st two parts are relevant)
			source.AppendLine("  v4 = aTexCoords + uOffsets.zw;");            // pos: +2 |

			source.AppendLine("}");

			return source.ToString ();
		}

		private string GetFragmentShader ( bool isTinted){
			StringBuilder source = new StringBuilder("");

			// variables
			source.AppendLine("varying lowp vec2 v0;");
			source.AppendLine("varying lowp vec2 v1;");
			source.AppendLine("varying lowp vec2 v2;");
			source.AppendLine("varying lowp vec2 v3;");
			source.AppendLine("varying lowp vec2 v4;");

			if (isTinted) source.AppendLine("uniform lowp vec4 uColor;");
			source.AppendLine("uniform sampler2D uTexture;");
			source.AppendLine("uniform lowp vec4 uWeights;");

			// main
			source.AppendLine("void main() {");

			source.AppendLine("  lowp vec4 ft0;");
			source.AppendLine("  lowp vec4 ft1;");
			source.AppendLine("  lowp vec4 ft2;");
			source.AppendLine("  lowp vec4 ft3;");
			source.AppendLine("  lowp vec4 ft4;");
			source.AppendLine("  lowp vec4 ft5;");

			source.AppendLine("  ft0 = texture2D(uTexture,v0);");  // read center pixel
			source.AppendLine("  ft5 = ft0 * uWeights.xxxx;");     // multiply with center weight

			source.AppendLine("  ft1 = texture2D(uTexture,v1);");  // read pixel -2
			source.AppendLine("  ft1 = ft1 * uWeights.zzzz;");     // multiply with weight
			source.AppendLine("  ft5 = ft5 + ft1;");               // add to output color

			source.AppendLine("  ft2 = texture2D(uTexture,v2);");  // read pixel -1
			source.AppendLine("  ft2 = ft2 * uWeights.yyyy;");     // multiply with weight
			source.AppendLine("  ft5 = ft5 + ft2;");               // add to output color

			source.AppendLine("  ft3 = texture2D(uTexture,v3);");  // read pixel +1
			source.AppendLine("  ft3 = ft3 * uWeights.yyyy;");     // multiply with weight
			source.AppendLine("  ft5 = ft5 + ft3;");               // add to output color

			source.AppendLine("  ft4 = texture2D(uTexture,v4);");  // read pixel +2
			source.AppendLine("  ft4 = ft4 * uWeights.zzzz;");     // multiply with weight

			if (isTinted)
			{
				source.AppendLine("  ft5 = ft5 + ft4;");                   // add to output color
				source.AppendLine("  ft5.xyz = uColor.xyz * ft5.www;");    // set rgb with correct alpha
				source.AppendLine("  gl_FragColor = ft5 * uColor.wwww;");  // multiply alpha
			}
			else
			{
				source.AppendLine("  gl_FragColor = ft5 + ft4;");          // add to output color
			}

			source.AppendLine("}");

			return source.ToString ();
		}
	}
}

