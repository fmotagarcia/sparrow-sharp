using Sparrow.Core;
using Sparrow.Textures;
using Sparrow.Utils;
using System;
using System.Text;
using OpenGL;

namespace Sparrow.Rendering
{
    /** An effect drawing a mesh of textured vertices.
    *  This is the standard effect that is the base for all fragment filters;
    *  if you want to create your own fragment filters, you will have to extend this class.
    *
    *  <p>For more information about the usage and creation of effects, please have a look at
    *  the documentation of the parent class, "Effect".</p>
    *
    *  @see Effect
    *  @see MeshEffect
    *  @see starling.filters.FragmentFilter
    */
    public class FilterEffect : Effect
    {

        private Texture _texture;
        private TextureSmoothing _textureSmoothing;
        private bool _textureRepeat;
        
        public FilterEffect()
        {
            _textureSmoothing = TextureSmoothing.Bilinear;
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
        /** Override this method if the effect requires a different program depending on the
         *  current settings. Ideally, you do this by creating a bit mask encoding all the options.
         *  This method is called often, so do not allocate any temporary objects when overriding.
         *
         *  <p>Reserve 4 bits for the variant name of the base class.</p>
         */
        override protected uint ProgramVariantName {
            get { return RenderUtil.GetTextureVariantBits(_texture); }
        }

        /** @private */
        override protected Program CreateProgram()
        {
            if (_texture != null)
            {
                StringBuilder source = new StringBuilder("");

                string vertexShader = StdVertexShader;

                // fragment shader
                source = new StringBuilder("");
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
            else
            {
                return base.CreateProgram();
            }
        }

        /** This method is called by <code>render</code>, directly before
         *  <code>context.drawTriangles</code>. It activates the program and sets up
         *  the context with the following constants and attributes:
         *
         *  <ul>
         *    <li><code>vc0-vc3</code> — MVP matrix</li>
         *    <li><code>va0</code> — vertex position (xy)</li>
         *    <li><code>va1</code> — texture coordinates (uv)</li>
         *    <li><code>fs0</code> — texture</li>
         *  </ul>
         */
        override protected void BeforeDraw()
        {
            base.BeforeDraw();

            if (_texture != null)
            {
                uint aTexCoords = (uint)Program.Attributes["aTexCoords"];
                Gl.EnableVertexAttribArray(aTexCoords);
                Gl.VertexAttribPointer(aTexCoords, 2, Gl.FLOAT, false, Vertex.SIZE, (IntPtr)Vertex.TEXTURE_OFFSET);

                Gl.ActiveTexture(Gl.TEXTURE0);
                
                RenderUtil.SetSamplerStateAt(_texture.Base, _texture.NumMipMaps > 0, 
                                _textureSmoothing, _textureRepeat);
            }
        }

        /** This method is called by <code>render</code>, directly after
         *  <code>context.drawTriangles</code>. Resets texture and vertex buffer attributes. */
        override protected void AfterDraw()
        {
            if (_texture != null)
            {
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                // do we need to unbind anything else?
            }
            base.AfterDraw();
        }

        /** The texture to be mapped onto the vertices. */
        public Texture Texture {
            get { return _texture; }
            set { _texture = value; }
        }

        /** The smoothing filter that is used for the texture. @default bilinear */
        public TextureSmoothing TextureSmoothing
        {
            get { return _textureSmoothing; }
            set { _textureSmoothing = value; }
        }

        /** Indicates if pixels at the edges will be repeated or clamped.
         *  Only works for power-of-two textures. @default false */
        public bool TextureRepeat
        {
            get { return _textureRepeat; }
            set { _textureRepeat = value; }
        }
    }
}
