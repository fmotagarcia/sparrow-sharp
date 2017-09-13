﻿
using System;
using System.Text;
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
    /// <see cref="Sparrow.Styles.MeshStyle"/>
    /// </summary>
    public class MeshEffect : FilterEffect
    {
        /// <summary>
        /// The alpha value of the object rendered by the effect. Must be taken into account
        /// by all subclasses
        /// </summary>
        public float Alpha;
        
        public MeshEffect()
        {
            Alpha = 1.0f;
        }
        
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
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexColorsBufferName);
            Gl.VertexAttribPointer(attribColor, 4, Gl.UNSIGNED_BYTE, true, sizeof(float), (IntPtr)0);
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
