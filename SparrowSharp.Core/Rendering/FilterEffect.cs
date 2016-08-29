using Sparrow.Core;
using Sparrow.Textures;
using System;

namespace SparrowSharp.Core.Rendering
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

        /** The vertex format expected by <code>uploadVertexData</code>:
         *  <code>"position:float2, texCoords:float2"</code> */
        public static readonly VertexDataFormat VERTEX_FORMAT = Effect.VERTEX_FORMAT.extend("texCoords:float2");

        /** The AGAL code for the standard vertex shader that most filters will use.
         *  It simply transforms the vertex coordinates to clip-space and passes the texture
         *  coordinates to the fragment program (as 'v0'). */
        public static readonly string STD_VERTEX_SHADER =
            "m44 op, va0, vc0 \n"+  // 4x4 matrix transform to output clip-space
            "mov v0, va1";          // pass texture coordinates to fragment program

        private Texture _texture;
        private TextureSmoothing _textureSmoothing;
        private bool _textureRepeat;

        /** Creates a new FilterEffect instance. */
        public FilterEffect()
        {
            _textureSmoothing = TextureSmoothing.Bilinear;
        }

        /** Override this method if the effect requires a different program depending on the
         *  current settings. Ideally, you do this by creating a bit mask encoding all the options.
         *  This method is called often, so do not allocate any temporary objects when overriding.
         *
         *  <p>Reserve 4 bits for the variant name of the base class.</p>
         */
        override protected uint ProgramVariantName { get { return RenderUtil.getTextureVariantBits(_texture); } }

        /** @private */
        override protected Program CreateProgram()
        {
            if (_texture != null)
            {
                string vertexShader = STD_VERTEX_SHADER;
                string fragmentShader = Tex("oc", "v0", 0, _texture);
                return Program.FromSource(vertexShader, fragmentShader);
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
        override protected void BeforeDraw(Context3D context)
        {
            base.BeforeDraw(context);

            if (_texture != null)
            {
                bool repeat = _textureRepeat && _texture.root.isPotTexture;
                RenderUtil.setSamplerStateAt(0, _texture.mipMapping, _textureSmoothing, repeat);
                context.setTextureAt(0, _texture.base);
                VertexFormat.SetVertexBufferAt(1, VertexBuffer, "texCoords");
            }
        }

        /** This method is called by <code>render</code>, directly after
         *  <code>context.drawTriangles</code>. Resets texture and vertex buffer attributes. */
        override protected void AfterDraw(Context3D context)
        {
            if (_texture != null)
            {
                context.setTextureAt(0, null);
                context.setVertexBufferAt(1, null);
            }
            base.AfterDraw(context);
        }

        /** Creates an AGAL source string with a <code>tex</code> operation, including an options
         *  list with the appropriate format flag. This is just a convenience method forwarding
         *  to the respective RenderUtil method.
         *
         *  @see starling.utils.RenderUtil#createAGALTexOperation()
         */
        protected static string Tex(string resultReg, string uvReg, int sampler, Texture texture,
                                     bool convertToPmaIfRequired= true)
        {
            return RenderUtil.createAGALTexOperation(resultReg, uvReg, sampler, texture,
                convertToPmaIfRequired);
        }

        /** The data format that this effect requires from the VertexData that it renders:
         *  <code>"position:float2, texCoords:float2"</code> */
        override public VertexDataFormat VertexFormat { get { return VERTEX_FORMAT; } }

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
