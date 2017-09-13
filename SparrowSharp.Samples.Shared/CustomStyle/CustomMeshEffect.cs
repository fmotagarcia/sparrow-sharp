using System.Text;
using Sparrow.Rendering;

namespace Sparrow.Samples.CustomStyle
{
    public class CustomMeshEffect : MeshEffect
    {
        // just copypaste for now
        protected override Program CreateProgram()
        {
            StringBuilder source = new StringBuilder("");
            string vertexShader;
            string fragmentShader;

            if (Texture != null)
            {
                AddShaderInitCode(source);
                source.AppendLine("attribute vec4 aPosition;");
                source.AppendLine("attribute vec4 aColor;");
                source.AppendLine("attribute vec2 aTexCoords;");
                source.AppendLine("uniform mat4 uMvpMatrix;");
                source.AppendLine("uniform vec4 uAlpha;");
                source.AppendLine("varying lowp vec4 vColor;");
                source.AppendLine("varying lowp vec2 vTexCoords;");
                // main
                source.AppendLine("void main() {");
                source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
                source.AppendLine("  vColor = aColor * uAlpha;");
                source.AppendLine("  vTexCoords  = aTexCoords;");
                source.Append("}");
                vertexShader = source.ToString();
                
                source.Clear();
                AddShaderInitCode(source);
                // variables
                source.AppendLine("varying lowp vec4 vColor;");
                source.AppendLine("varying lowp vec2 vTexCoords;");
                source.AppendLine("uniform lowp sampler2D uTexture;");
                // main
                source.AppendLine("void main() {");
                source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords) * vColor;");
                source.Append("}");
                fragmentShader = source.ToString();
            }
            else
            {
                AddShaderInitCode(source);
                source.AppendLine("attribute vec4 aPosition;");
                source.AppendLine("attribute vec4 aColor;");
                source.AppendLine("uniform mat4 uMvpMatrix;");
                source.AppendLine("uniform vec4 uAlpha;");
                source.AppendLine("varying lowp vec4 vColor;");
                // main
                source.AppendLine("void main() {");
                source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
                source.AppendLine("  vColor = aColor * uAlpha;");
                source.Append("}");
                vertexShader = source.ToString();

                source.Clear();
                AddShaderInitCode(source);
                // variables
                source.AppendLine("varying lowp vec4 vColor;");
                // main
                source.AppendLine("void main() {");
                source.AppendLine("  gl_FragColor = vColor;");
                source.Append("}");
                fragmentShader = source.ToString();
            }
            return new Program(vertexShader, fragmentShader);
        }
        
    }
}