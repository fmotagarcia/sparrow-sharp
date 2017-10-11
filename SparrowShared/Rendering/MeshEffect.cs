
using System;
using OpenGL;

namespace Sparrow.Rendering
{

    /// <summary>
    /// An effect drawing a mesh of textured, colored vertices.
    /// This is the standard effect that is the base for all mesh styles;
    /// if you want to create your own mesh styles, you will have to extend this class.
    ///
    /// <para>For more information about the usage and creation of effects, please have a look at
    /// the documentation of the root class, "Effect".</para>
    ///
    /// <see cref="Effect"/> 
    /// <see cref="FilterEffect"/>
    /// <see cref="Styles.MeshStyle"/>
    /// </summary>
    public class MeshEffect : FilterEffect
    {
        /// <summary>
        /// The alpha value of the object rendered by the effect. Must be taken into account
        /// by all subclasses
        /// </summary>
        public float Alpha = 1.0f;
        
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
                uniform lowp sampler2D uTexture;
                out lowp vec4 fragColor;
                
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

        /// <summary>
        /// This method is called by <code>Render</code>, directly before
        /// <code>Gl.DrawElements</code>. It activates the program, activates
        /// GL Texture Unit 0 and sets up  the GL context with the following 
        /// constants and attributes:
        /// 
        /// <code>uMvpMatrix</code> — MVP matrix
        /// <code>aPosition</code> — vertex position (xy)
        /// <code>aTexCoords</code> — texture coordinates (uv)
        /// <code>uAlpha</code> — texture alpha
        /// <code>aColor</code> — texture tint
        /// </summary>
        protected override void BeforeDraw()
        {
            base.BeforeDraw();

            int uAlpha = Program.Uniforms["uAlpha"];
            Gl.Uniform4(uAlpha, Alpha, Alpha, Alpha, Alpha);

            uint attribColor = (uint)Program.Attributes["aColor"];
            Gl.EnableVertexAttribArray(attribColor);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _vertexColorsBufferName);
            Gl.VertexAttribPointer(attribColor, 4, VertexAttribType.UnsignedByte, true, sizeof(float), (IntPtr)0);
        }

        /// <summary>
        /// This method is called by <code>Render</code>, directly after
        /// <code>Gl.DrawElements</code>. Resets vertex buffer attributes.
        /// </summary>
        protected override void AfterDraw()
        {
            //?? context.setVertexBufferAt(2, null);
            uint attribColor = (uint)Program.Attributes["aColor"];
            Gl.DisableVertexAttribArray(attribColor);
            base.AfterDraw();
        }

    }
}
