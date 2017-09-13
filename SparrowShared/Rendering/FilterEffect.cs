using Sparrow.Textures;
using Sparrow.Utils;
using System;
using System.Text;
using OpenGL;

namespace Sparrow.Rendering
{
    /// <summary>
    /// An effect drawing a mesh of textured vertices.
    /// This is the standard effect that is the base for all fragment filters;
    /// if you want to create your own fragment filters, you will have to extend this class.
    ///
    /// <para>For more information about the usage and creation of effects, please have a look at
    /// the documentation of the parent class, "Effect".</para>
    ///
    /// <see cref="Effect"/>
    /// <see cref="MeshEffect"/>
    /// <see cref="Sparrow.Filters.FragmentFilter"/>
    /// </summary>
    public class FilterEffect : Effect
    {

        /// <summary>
        /// The texture to be mapped onto the vertices.
        /// </summary>
        public Texture Texture;

        /// <summary>
        /// The smoothing filter that is used for the texture. @default bilinear
        /// </summary>
        public TextureSmoothing TextureSmoothing;

        /// <summary>
        /// Indicates if pixels at the edges will be repeated or clamped.
        /// Only works for power-of-two textures. @default false
        /// </summary>
        public bool TextureRepeat;
        
        public FilterEffect()
        {
            TextureSmoothing = TextureSmoothing.Bilinear;
        }

        public static string StdVertexShader
        {
            get
            {
                StringBuilder source = new StringBuilder("");
                // vertex shader
                AddShaderInitCode(source);
                source.AppendLine("attribute vec4 aPosition;");
                source.AppendLine("attribute vec2 aTexCoords;");
                source.AppendLine("uniform mat4 uMvpMatrix;");
                source.AppendLine("varying lowp vec2 vTexCoords;");
                // main
                source.AppendLine("void main() {");
                source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
                source.AppendLine("  vTexCoords  = aTexCoords;");
                source.Append("}");

                return source.ToString();
            }
        }
        
        /// <summary>
        /// Override this method if the effect requires a different program depending on the
        /// current settings. Ideally, you do this by creating a bit mask encoding all the options.
        /// This method is called often, so do not allocate any temporary objects when overriding.
        ///
        /// <para>Reserve 4 bits for the variant name of the base class.</para>
        /// </summary>
        protected override uint ProgramVariantName {
            get { return Texture == null ? 0 : 1u; }
        }
        
        protected override Program CreateProgram()
        {
            if (Texture != null)
            {
                string vertexShader = StdVertexShader;

                // fragment shader
                var source = new StringBuilder("");
                AddShaderInitCode(source);
                // variables
                source.AppendLine("varying lowp vec2 vTexCoords;");
                source.AppendLine("uniform lowp sampler2D uTexture;");
                // main
                source.AppendLine("void main() {");
                source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords);");
                source.Append("}");

                string fragmentShader = source.ToString();
                return new Program(vertexShader, fragmentShader);
            }
            return base.CreateProgram();
        }

        /// <summary>
        /// This method is called by <code>Render</code>, directly before
        /// <code>Gl.DrawElements</code>. It activates the program, activetes
        /// GL Texture Unit 0 and sets up the GL context with the following 
        /// constants and attributes:
        /// 
        /// <code>uMvpMatrix</code> — MVP matrix
        /// <code>aPosition</code> — vertex position (xy)
        /// <code>aTexCoords</code> — texture coordinates (uv)
        /// </summary>
        protected override void BeforeDraw()
        {
            base.BeforeDraw();

            if (Texture == null)
            {
                return;
            }
            uint aTexCoords = (uint)Program.Attributes["aTexCoords"];
            Gl.EnableVertexAttribArray(aTexCoords);
            Gl.VertexAttribPointer(aTexCoords, 2, Gl.FLOAT, false, Vertex.Size, (IntPtr)Vertex.TextureOffset);

            Gl.ActiveTexture(Gl.TEXTURE0);
                
            RenderUtil.SetSamplerStateAt(Texture.Base, Texture.NumMipMaps > 0, 
                TextureSmoothing, TextureRepeat);
        }

        /// <summary>
        /// This method is called by <code>Render</code>, directly after
        /// <code>GL.drawElement</code>. Resets texture and vertex buffer attributes.
        /// </summary>
        protected override void AfterDraw()
        {
            if (Texture != null)
            {
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                // do we need to unbind anything else?

                uint aTexCoords = (uint)Program.Attributes["aTexCoords"];
                Gl.DisableVertexAttribArray(aTexCoords);
            }
            base.AfterDraw();
        }

    }
}
