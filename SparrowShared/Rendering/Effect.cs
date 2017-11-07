
using Sparrow.Utils;
using Sparrow.Geom;
using System;
using System.Collections.Generic;
using OpenGL;
using Sparrow.Core;
using Sparrow.Textures;

namespace Sparrow.Rendering
{
    /** An effect encapsulates all steps of a OpenGL draw operation. It configures the
     *  render context and sets up shader programs as well as index- and vertex-buffers, thus
     *  providing the basic mechanisms of all low-level rendering.
     *
     *  <p><strong>Using the Effect class</strong></p>
     *
     *  <p>Effects are mostly used by the <code>MeshStyle</code> and <code>FragmentFilter</code>
     *  classes. When you extend those classes, you'll be required to provide a custom effect.
     *  Setting it up for rendering is done by the base class, though, so you rarely have to
     *  initiate the rendering yourself. Nevertheless, it's good to know how an effect is doing
     *  its work.</p>
     *
     *  <p>Using an effect always follows steps shown in the example below. You create the
     *  effect, configure it, upload vertex data and then: draw!</p>
     *
     *  <listing>
     *  // create effect
     *  MeshEffect effect = new MeshEffect();
     *  
     *  // configure effect
     *  effect.MvpMatrix3D = Painter.State.MvpMatrix3D;
     *  effect.Texture = GetHeroTexture();
     *  effect.Color = 0xf0f0f0;
     *  
     *  // upload vertex data
     *  effect.UploadIndexData(indexData);
     *  effect.UploadVertexData(vertexData);
     *  
     *  // draw!
     *  effect.Render(0, numTriangles);</listing>
     *
     *  <p>Note that the <code>VertexData</code> being uploaded has to be created with the same
     *  format as the one returned by the effect's <code>vertexFormat</code> property.</p>
     *
     *  <p>Extending the Effect class</p>
     *
     *  <p>The base <code>Effect</code>-class can only render white triangles, which is not much
     *  use in itself. However, it is designed to be extended; subclasses can easily implement any
     *  kinds of shaders.</p>
     *
     *  <p>Normally, you won't extend this class directly, but either <code>FilterEffect</code>
     *  or <code>MeshEffect</code>, depending on your needs (i.e. if you want to create a new
     *  fragment filter or a new mesh style). Whichever base class you're extending, you should
     *  override the following methods:</p>
     *
     *  <ul>
     *    <li><code>Program CreateProgram()</code> — must create the actual program containing 
     *        vertex- and fragment-shaders. A program will be created only once for each render
     *        context; this is taken care of by the base class.</li>
     *    <li><code>uint get programVariantName()</code> (optional) — override this if your
     *        effect requires different programs, depending on its settings. The recommended
     *        way to do this is via a bit-mask that uniquely encodes the current settings.</li>
     *    <li><code>String get VertexFormat()</code> (optional) — must return the
     *        <code>VertexData</code> format that this effect requires for its vertices. If
     *        the effect does not require any special attributes, you can leave this out.</li>
     *    <li><code>beforeDraw()</code> — Set up your context by
     *        configuring program constants and buffer attributes.</li>
     *    <li><code>AfterDraw()</code> — Will be called directly after
     *        <code>context.drawElements()</code>. Clean up any context configuration here.</li>
     *  </ul>
     *
     *  <p>Furthermore, you need to add properties that manage the data you require on rendering,
     *  e.g. the texture(s) that should be used, program constants, etc. I recommend looking at
     *  the implementations of Starling's <code>FilterEffect</code> and <code>MeshEffect</code>
     *  classes to see how to approach sub-classing.</p>
     *
     *  @see FilterEffect
     *  @see MeshEffect
     *  @see Sparrow.Styles.MeshStyle
     *  @see Sparrow.Filters.FragmentFilter
     *  @see Sparrow.Utils.RenderUtil
     */
    public class Effect
    {

        protected uint VertexBufferName;
        protected uint VertexColorsBufferName;
        protected uint IndexBufferName;
        protected int VertexBufferSize; // in number of vertices
        protected int IndexBufferSize;  // in number of indices
        protected bool IndexBufferUsesQuadLayout;

        private readonly Matrix3D _mvpMatrix3D;
        
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
        
        // helper objects
        public readonly Dictionary<string, Dictionary<uint, string>> SProgramNameCache = 
                                        new Dictionary<string, Dictionary<uint, string>>();

        /// <summary>
        /// Creates a new effect.
        /// </summary>
        public Effect()
        {
            _mvpMatrix3D = Matrix3D.Create();
            ProgramBaseName = GetType().Name;
            SparrowSharp.ContextCreated += OnContextCreated;
            TextureSmoothing = TextureSmoothing.Bilinear;
        }
        
        public static string StdVertexShader => AddShaderInitCode() + @"
                    in vec4 aPosition;
                    in vec2 aTexCoords;
                    uniform mat4 uMvpMatrix;
                    out lowp vec2 vTexCoords;
                    // main
                    void main() {
                      gl_Position = uMvpMatrix * aPosition;
                      vTexCoords  = aTexCoords;
                    }";

        /// <summary>
        /// Purges the index- and vertex-buffers.
        /// </summary>
        public void Dispose()
        {
            PurgeBuffers();
            SparrowSharp.ContextCreated -= OnContextCreated;
        }

        private void OnContextCreated()
        {
            PurgeBuffers();
        }

        /// <summary>
        /// Purges one or both of the vertex- and index-buffers.
        /// </summary>
        public void PurgeBuffers(bool vertexBuffer = true, bool indexBuffer = true)
        {
            if (VertexBufferName != 0 && vertexBuffer)
            {
                uint[] buffers = { VertexBufferName };
                Gl.DeleteBuffers(buffers);
                VertexBufferName = 0;
                if (VertexColorsBufferName != 0)
                {
                    uint[] colorBuffers = { VertexColorsBufferName };
                    Gl.DeleteBuffers(colorBuffers);
                    VertexColorsBufferName = 0;
                }
            }

            if (IndexBufferName != 0 && indexBuffer)
            {
                uint[] indexBuffers = { IndexBufferName };
                Gl.DeleteBuffers(indexBuffers);
                IndexBufferName = 0;
            }
        }

        /// <summary>
        /// Uploads the given index data to the internal index buffer. If the buffer is too
        /// small, a new one is created automatically.
        /// </summary>
        /// <param name="indexData">The IndexData instance to upload.</param>
        /// <param name="bufferUsage">The expected buffer usage. Use one of the constants defined in
        ///                    <code>BufferUsageARB</code>. Only used when the method call
        ///                    causes the creation of a new index buffer.</param>
        public void UploadIndexData(IndexData indexData, BufferUsage bufferUsage = BufferUsage.StaticDraw)
        {
            int numIndices = indexData.NumIndices;
            bool isQuadLayout = indexData.UseQuadLayout;
            bool wasQuadLayout = IndexBufferUsesQuadLayout;

            if (IndexBufferName != 0)
            {
                if (numIndices <= IndexBufferSize)
                {
                    if (!isQuadLayout || !wasQuadLayout)
                    {
                        indexData.UploadToIndexBuffer(IndexBufferName, bufferUsage);
                        IndexBufferUsesQuadLayout = isQuadLayout && numIndices == IndexBufferSize;
                    }
                }
                else
                {
                    PurgeBuffers(false);
                }
            }
            if (IndexBufferName == 0)
            {
                IndexBufferName = indexData.CreateIndexBuffer(true, bufferUsage);
                IndexBufferSize = numIndices;
                IndexBufferUsesQuadLayout = isQuadLayout;
            }
        }

        /// <summary>
        /// Uploads the given vertex data to the internal vertex buffer. If the buffer is too
        /// small, a new one is created automatically.
        /// </summary>
        /// <param name="vertexData">The VertexData instance to upload.</param>
        /// <param name="bufferUsage"> The expected buffer usage. Use one of the constants defined in
        ///                    <code>BufferUsageARB</code>. Only used when the method call
        ///                    causes the creation of a new vertex buffer.</param>
        public void UploadVertexData(VertexData vertexData, BufferUsage bufferUsage = BufferUsage.StaticDraw)
        {
            if (VertexBufferName != 0)
            {
                if (vertexData.NumVertices <= VertexBufferSize)
                {
                    vertexData.UploadToVertexBuffer(VertexBufferName, VertexColorsBufferName, bufferUsage);
                }
                else
                {
                    PurgeBuffers(true, false);
                }
                    
            }
            if (VertexBufferName == 0)
            {
                uint[] names = vertexData.CreateVertexBuffer(true);
                VertexBufferName = names[0];
                VertexColorsBufferName = names[1];
                VertexBufferSize = vertexData.NumVertices;
            }
        }

        // rendering

        /// <summary>
        /// Draws the triangles described by the index- and vertex-buffers, or a range of them.
        /// This calls <code>BeforeDraw</code>, <code>Gl.DrawElements</code>, and
        /// <code>AfterDraw</code>, in this order.
        /// </summary>
        public virtual void Render(int numTriangles= -1)
        {
            if (numTriangles < 0) numTriangles = IndexBufferSize;
            if (numTriangles == 0) return;
            
            BeforeDraw();

            Gl.DrawElements(PrimitiveType.Triangles, numTriangles * 3, DrawElementsType.UnsignedShort, IntPtr.Zero);

            AfterDraw();
        }

        /// <summary>
        /// This method is called by <code>Render</code>, directly before
        /// <code>GL.drawElements</code>. It activates the program and sets up
        /// the GL context with the following constants and attributes:
        /// 
        /// <code>uMvpMatrix</code> — MVP matrix
        /// <code>aPosition</code> — vertex position (xy)
        /// </summary>
        protected virtual void BeforeDraw()
        {
            Program.Activate(); // create, upload, use program
            
            //is this the best place for this?
            Gl.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferName);

            uint attribPosition = (uint)Program.Attributes["aPosition"];
            Gl.EnableVertexAttribArray(attribPosition);
            Gl.VertexAttribPointer(attribPosition, 2, VertexAttribType.Float, false, Vertex.Size, (IntPtr)Vertex.PositionOffset);
            
            int uMvpMatrix = Program.Uniforms["uMvpMatrix"];
            Gl.UniformMatrix4(uMvpMatrix, 1, false, MvpMatrix3D.RawData); // 1 is the number of matrices
            
            if (Texture != null)
            {
                uint aTexCoords = (uint)Program.Attributes["aTexCoords"];
                Gl.EnableVertexAttribArray(aTexCoords);
                Gl.VertexAttribPointer(aTexCoords, 2, VertexAttribType.Float, false, Vertex.Size, (IntPtr)Vertex.TextureOffset);
                Gl.ActiveTexture(TextureUnit.Texture0);
                
                RenderUtil.SetSamplerStateAt(Texture.Base, Texture.NumMipMaps > 0, TextureSmoothing, TextureRepeat);   
            }
            // color & alpha are set in subclasses
        }

        /// <summary>
        /// This method is called by <code>Render</code>, directly after
        /// <code>Gl.DrawElements()</code>. Resets vertex buffer attributes.
        /// </summary>
        protected virtual void AfterDraw()
        {
            if (Texture != null)
            {
                Gl.BindTexture(TextureTarget.Texture2d, 0);
                // do we need to unbind anything else?

                uint aTexCoords = (uint)Program.Attributes["aTexCoords"];
                Gl.DisableVertexAttribArray(aTexCoords);
            }
            uint attribPosition = (uint)Program.Attributes["aPosition"];
            Gl.DisableVertexAttribArray(attribPosition);
        }

        // program management

        /// <summary>
        /// Creates the program (a combination of vertex- and fragment-shader) used to render
        /// the effect with the current settings. Override this method in a subclass to create
        /// your shaders. This method will only be called once; the program is automatically stored
        /// in the <code>Painter</code> and re-used by all instances of this effect.
        ///
        /// <para>The basic implementation always outputs pure white.</para>
        /// </summary>
        protected virtual Program CreateProgram()
        {
            if (Texture == null)
            {
                string vertexShader = AddShaderInitCode() + @"
                    in vec4 aPosition;
                    uniform mat4 uMvpMatrix;
                    
                    void main() {
                      gl_Position = uMvpMatrix * aPosition;
                    }";
                    
                string fragmentShader = AddShaderInitCode() + @"
                    out lowp vec4 fragColor;
        
                    void main() {
                        fragColor = vec4(1, 1, 1, 1);
                    }";
                return new Program(vertexShader, fragmentShader);       
            }
            var fragmentShaderTex = AddShaderInitCode() + @"
                in lowp vec2 vTexCoords;
                uniform lowp sampler2D uTexture;
                out lowp vec4 fragColor;
                
                void main() {
                  fragColor = texture(uTexture, vTexCoords);
                }";
            return new Program(StdVertexShader, fragmentShaderTex);

        }

        /// <summary>
        /// Appends OpenGL shader defines, this is needed for shaders to work on both
        /// desktop OpenGL and OpenGL ES 3+.
        /// </summary>
        public static string AddShaderInitCode()
        {
            string ret;
#if __WINDOWS__
            ret = 
                "#version 430\n" + 
                "#define highp\n" +
                "#define mediump\n" +
                "#define lowp\n";
#else
            ret = @"#version 300 es\n";
#endif
            return ret;
        }

        /// <summary>
        /// Override this method if the effect requires a different program depending on the
        /// current settings. Ideally, you do this by creating a bit mask encoding all the options.
        /// This method is called often, so do not allocate any temporary objects when overriding.
        ///
        /// @default 0
        /// </summary>
        protected virtual uint ProgramVariantName => Texture == null ? 0 : 1u;

        /// <summary>
        /// Returns the base name for the program. @default the fully qualified class name
        /// </summary>
        protected readonly string ProgramBaseName;

        /// <summary>
        /// Returns the full name of the program, which is used to register it at the current
        /// <code>Painter</code>.
        ///
        /// <para>The default implementation efficiently combines the program's base and variant
        /// names (e.g. <code>LightEffect#42</code>). It shouldn't be necessary to override
        /// this method.</para>
        /// </summary>
        protected string ProgramName
        {
            get
            {
                string baseName = ProgramBaseName;
                uint variantName = ProgramVariantName;
                Dictionary<uint, string> nameCache;
                if (!SProgramNameCache.ContainsKey(baseName))
                {
                    nameCache = new Dictionary<uint, string>();
                    SProgramNameCache[baseName] = nameCache;
                }
                else
                {
                    nameCache = SProgramNameCache[baseName];
                }

                string name;
                if (nameCache.ContainsKey(variantName))
                {
                    name = nameCache[variantName];
                }
                else
                {
                    if (variantName != 0) name = baseName + "#" + variantName.ToString("X"); // hex string conversion
                    else name = baseName;
                    nameCache[variantName] = name;
                }
                return name;
            }
        }

        /// <summary>
        /// Returns the current program, either by creating a new one (via
        /// <code>CreateProgram</code>) or by getting it from the <code>Painter</code>.
        /// Do not override this method! Instead, implement <code>CreateProgram</code>.
        /// </summary>
        protected Program Program
        {
            get
            {
                string name = ProgramName;
                Painter painter = SparrowSharp.Painter;
                Program program = painter.GetProgram(name);

                if (program == null)
                {
                    program = CreateProgram();
                    painter.RegisterProgram(name, program);
                }
                return program;
            }
        }

        /** The function that you provide here will be called after a context loss.
         *  Call both "Upload..." methods from within the callback to restore any vertex or
         *  index buffers. */
        public Action OnRestore;

        public Matrix3D MvpMatrix3D
        {
            get => _mvpMatrix3D;
            set { _mvpMatrix3D.CopyFrom(value); }
        }
    }
}
