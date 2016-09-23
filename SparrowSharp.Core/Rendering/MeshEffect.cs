
using System;
using System.Text;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Rendering
{

    public class MeshEffect : FilterEffect
    {
        private float _alpha;

        // helper objects
        private static float[] sRenderAlpha = new float[4];
        
        public MeshEffect()
        {
            _alpha = 1.0f;
        }
        
        override protected uint ProgramVariantName
        {
            get
            {
                // might not be needed..
                return base.ProgramVariantName | (0 << 3);
            }
        }
        
        override protected Program CreateProgram()
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

                source = new StringBuilder("");
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

                source = new StringBuilder("");
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

        /** This method is called by <code>render</code>, directly before
         *  <code>context.drawTriangles</code>. It activates the program and sets up
         *  the context with the following constants and attributes:
         *
         *  <ul>
         *    <li><code>vc0-vc3</code> — MVP matrix</li>
         *    <li><code>vc4</code> — alpha value (same value for all components)</li>
         *    <li><code>va0</code> — vertex position (xy)</li>
         *    <li><code>va1</code> — texture coordinates (uv)</li>
         *    <li><code>va2</code> — vertex color (rgba), using premultiplied alpha</li>
         *    <li><code>fs0</code> — texture</li>
         *  </ul>
         */
        override protected void BeforeDraw()
        {
            base.BeforeDraw();

            int _uAlpha = Program.Uniforms["uAlpha"];
            GL.Uniform4(_uAlpha, _alpha, _alpha, _alpha, _alpha);

            int attribColor = Program.Attributes["aColor"];
            GL.EnableVertexAttribArray(attribColor);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexColorsBufferName);
            GL.VertexAttribPointer(attribColor, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(float), (IntPtr)0);
        }

        /** This method is called by <code>render</code>, directly after
         *  <code>context.drawTriangles</code>. Resets texture and vertex buffer attributes. */
        override protected void AfterDraw()
        {
            //?? context.setVertexBufferAt(2, null);
            base.AfterDraw();
        }

        /** The alpha value of the object rendered by the effect. Must be taken into account
         *  by all subclasses. */
        public float Alpha {
            get { return _alpha; }
            set { _alpha = value;  }
        }

    }
}
