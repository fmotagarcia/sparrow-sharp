
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
                attribute vec4 aPosition;
                attribute vec4 aColor;
                attribute vec2 aTexCoords;

                uniform mat4 uMvpMatrix;
                uniform vec4 uAlpha;

                varying lowp vec4 vColor;
                varying lowp vec2 vTexCoords;
                
                void main() {
                  gl_Position = uMvpMatrix * aPosition;
                  vColor = aColor * uAlpha;
                  vTexCoords  = aTexCoords;
                }";
                
                fragmentShader = AddShaderInitCode() + @"
                varying lowp vec4 vColor;
                varying lowp vec2 vTexCoords;
                uniform lowp sampler2D uTexture;
                
                void main() {
                  gl_FragColor = texture2D(uTexture, vTexCoords) * vColor;
                }";
            }
            else
            {
                vertexShader = AddShaderInitCode() + @"
                attribute vec4 aPosition;
                attribute vec4 aColor;

                uniform mat4 uMvpMatrix;
                uniform vec4 uAlpha;

                varying lowp vec4 vColor;
                
                void main() {
                  gl_Position = uMvpMatrix * aPosition;
                  vColor = aColor * uAlpha;
                }";
                
                fragmentShader = AddShaderInitCode() + @"
                varying lowp vec4 vColor;
                
                void main() {
                  gl_FragColor = vColor;
                }";
            }
            return new Program(vertexShader, fragmentShader);
        }
        
    }
}