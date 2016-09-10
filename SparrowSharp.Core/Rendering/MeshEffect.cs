
using OpenTK.Graphics.ES20;
using Sparrow.Core;
using System;
using System.Text;

namespace SparrowSharp.Core.Rendering
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
                throw new NotImplementedException();
                /*
                vertexShader =
                    "m44 op, va0, vc0 \n" + // 4x4 matrix transform to output clip-space
                    "mov v0, va1      \n" + // pass texture coordinates to fragment program
                    "mul v1, va2, vc4 \n";  // multiply alpha (vc4) with color (va2), pass to fp

                fragmentShader =
                    Tex("ft0", "v0", 0, Texture) +
                    "mul oc, ft0, v1  \n";  // multiply color with texel color
                */
            }
            else
            {
                //vertexShader =
                //    "m44 op, va0, vc0 \n" + // 4x4 matrix transform to output clipspace
                //    "mul v0, va2, vc4 \n";  // multiply alpha (vc4) with color (va2)
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

                //fragmentShader =
                //    "mov oc, v0       \n";  // output color

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
            //sRenderAlpha[0] = sRenderAlpha[1] = sRenderAlpha[2] = sRenderAlpha[3] = _alpha;
            //context.setProgramConstantsFromVector(Context3DProgramType.VERTEX, 4, sRenderAlpha);

            int _uAlpha = Program.Uniforms["uAlpha"];
            GL.Uniform4(_uAlpha, _alpha, _alpha, _alpha, _alpha);

            //if (_tinted || _alpha != 1.0 || !_optimizeIfNotTinted || texture == null)
            //    vertexFormat.setVertexBufferAt(2, vertexBuffer, "color");
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
