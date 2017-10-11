
using Sparrow.Rendering;

namespace Sparrow.Samples.CustomStyle
{
    public class CustomMeshEffect : MeshEffect
    {
        // just copypaste for now
        
        protected override Program CreateProgram()
        {
            string vertexShader;
            string fragmentShader;

            if (Texture != null)
            {
                vertexShader = AddShaderInitCode() + @"
                in vec4 aPosition;
                in vec4 aColor;
                in vec2 aTexCoords;

                uniform mat4 uMvpMatrix;
                uniform vec4 uAlpha;

                out lowp vec4 vColor;
                out lowp vec2 vTexCoords;
                
                void main() {
                  gl_Position = uMvpMatrix * aPosition;
                  vColor = aColor * uAlpha;
                  vTexCoords  = aTexCoords;
                }";
                
                fragmentShader = AddShaderInitCode() + @"
                in lowp vec4 vColor;
                in lowp vec2 vTexCoords;
                out lowp vec4 fragColor;
                uniform lowp sampler2D uTexture;

                void main() {
                  fragColor = texture(uTexture, vTexCoords) * vColor;
                }";
            }
            else
            {
                vertexShader = AddShaderInitCode() + @"
                in vec4 aPosition;
                in vec4 aColor;

                uniform mat4 uMvpMatrix;
                uniform vec4 uAlpha;

                out lowp vec4 vColor;
                
                void main() {
                  gl_Position = uMvpMatrix * aPosition;
                  vColor = aColor * uAlpha;
                }";
                
                fragmentShader = AddShaderInitCode() + @"
                in lowp vec4 vColor;
                out lowp vec4 fragColor;

                void main() {
                  fragColor = vColor;
                }";
            }
            return new Program(vertexShader, fragmentShader);
        }
        
        
    }
}