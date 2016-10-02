
using OpenTK;
using Sparrow.Utils;
using Sparrow.Geom;
using System;
using System.Collections.Generic;
using System.Text;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Rendering
{
    /** An effect encapsulates all steps of a Stage3D draw operation. It configures the
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
     *  var effect:MeshEffect = new MeshEffect();
     *  
     *  // configure effect
     *  effect.mvpMatrix3D = painter.state.mvpMatrix3D;
     *  effect.texture = getHeroTexture();
     *  effect.color = 0xf0f0f0;
     *  
     *  // upload vertex data
     *  effect.uploadIndexData(indexData);
     *  effect.uploadVertexData(vertexData);
     *  
     *  // draw!
     *  effect.render(0, numTriangles);</listing>
     *
     *  <p>Note that the <code>VertexData</code> being uploaded has to be created with the same
     *  format as the one returned by the effect's <code>vertexFormat</code> property.</p>
     *
     *  <p><strong>Extending the Effect class</strong></p>
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
     *    <li><code>createProgram():Program</code> — must create the actual program containing 
     *        vertex- and fragment-shaders. A program will be created only once for each render
     *        context; this is taken care of by the base class.</li>
     *    <li><code>get programVariantName():uint</code> (optional) — override this if your
     *        effect requires different programs, depending on its settings. The recommended
     *        way to do this is via a bit-mask that uniquely encodes the current settings.</li>
     *    <li><code>get vertexFormat():String</code> (optional) — must return the
     *        <code>VertexData</code> format that this effect requires for its vertices. If
     *        the effect does not require any special attributes, you can leave this out.</li>
     *    <li><code>beforeDraw(context:Context3D):void</code> — Set up your context by
     *        configuring program constants and buffer attributes.</li>
     *    <li><code>afterDraw(context:Context3D):void</code> — Will be called directly after
     *        <code>context.drawTriangles()</code>. Clean up any context configuration here.</li>
     *  </ul>
     *
     *  <p>Furthermore, you need to add properties that manage the data you require on rendering,
     *  e.g. the texture(s) that should be used, program constants, etc. I recommend looking at
     *  the implementations of Starling's <code>FilterEffect</code> and <code>MeshEffect</code>
     *  classes to see how to approach sub-classing.</p>
     *
     *  @see FilterEffect
     *  @see MeshEffect
     *  @see starling.styles.MeshStyle
     *  @see starling.filters.FragmentFilter
     *  @see starling.utils.RenderUtil
     */
    public class Effect
    {

        protected int _vertexBufferName;
        protected int _vertexColorsBufferName;
        protected int _indexBufferName;
        protected int _vertexBufferSize; // in number of vertices
        protected int _indexBufferSize;  // in number of indices
        protected bool _indexBufferUsesQuadLayout;

        private Matrix3D _mvpMatrix3D;
        private string _programBaseName;

        // helper objects
        public readonly Dictionary<string, Dictionary<uint, string>> sProgramNameCache = 
                                        new Dictionary<string, Dictionary<uint, string>>();

        /** Creates a new effect. */
        public Effect()
        {
            _mvpMatrix3D = Matrix3D.Create();
            _programBaseName = GetType().Name;
        }

        /** Purges the index- and vertex-buffers. */
        public void Dispose()
        {
            PurgeBuffers();
        }

        /** Purges one or both of the vertex- and index-buffers. */
        public void PurgeBuffers(bool vertexBuffer = true, bool indexBuffer = true)
        {
            if (_vertexBufferName != 0 && vertexBuffer)
            {
                GL.DeleteBuffers(1, ref _vertexBufferName);
                _vertexBufferName = 0;
                if (_vertexColorsBufferName != 0)
                {
                    GL.DeleteBuffers(1, ref _vertexColorsBufferName);
                    _vertexColorsBufferName = 0;
                }
            }

            if (_indexBufferName != 0 && indexBuffer)
            {
                GL.DeleteBuffers(1, ref _indexBufferName);
                _indexBufferName = 0;
            }
        }

        /** Uploads the given index data to the internal index buffer. If the buffer is too
         *  small, a new one is created automatically.
         *
         *  @param indexData   The IndexData instance to upload.
         *  @param bufferUsage The expected buffer usage. Use one of the constants defined in
         *                     <code>Context3DBufferUsage</code>. Only used when the method call
         *                     causes the creation of a new index buffer.
         */
        public void UploadIndexData(IndexData indexData)
        {
            UploadIndexData(indexData, BufferUsageType.StaticDraw);
        }
        /** Uploads the given index data to the internal index buffer. If the buffer is too
         *  small, a new one is created automatically.
         *
         *  @param indexData   The IndexData instance to upload.
         *  @param bufferUsage The expected buffer usage. Use one of the constants defined in
         *                     <code>Context3DBufferUsage</code>. Only used when the method call
         *                     causes the creation of a new index buffer.
         */
        public void UploadIndexData(IndexData indexData, BufferUsageType bufferUsage)
        {
            int numIndices = indexData.NumIndices;
            bool isQuadLayout = indexData.UseQuadLayout;
            bool wasQuadLayout = _indexBufferUsesQuadLayout;

            if (_indexBufferName != 0)
            {
                if (numIndices <= _indexBufferSize)
                {
                    if (!isQuadLayout || !wasQuadLayout)
                    {
                        indexData.UploadToIndexBuffer(_indexBufferName, bufferUsage);
                        _indexBufferUsesQuadLayout = isQuadLayout && numIndices == _indexBufferSize;
                    }
                }
                else
                {
                    PurgeBuffers(false, true);
                }
            }
            if (_indexBufferName == 0)
            {
                _indexBufferName = indexData.CreateIndexBuffer(true, bufferUsage);
                _indexBufferSize = numIndices;
                _indexBufferUsesQuadLayout = isQuadLayout;
            }
        }

        /** Uploads the given vertex data to the internal vertex buffer. If the buffer is too
         *  small, a new one is created automatically.
         *
         *  @param vertexData  The VertexData instance to upload.
         */
        public void UploadVertexData(VertexData vertexData)
        {
            UploadVertexData(vertexData, BufferUsageType.StaticDraw);
        }
        /** Uploads the given vertex data to the internal vertex buffer. If the buffer is too
         *  small, a new one is created automatically.
         *
         *  @param vertexData  The VertexData instance to upload.
         *  @param bufferUsage The expected buffer usage. Use one of the constants defined in
         *                     <code>Context3DBufferUsage</code>. Only used when the method call
         *                     causes the creation of a new vertex buffer.
         */
        public void UploadVertexData(VertexData vertexData, BufferUsageType bufferUsage)
        {
            if (_vertexBufferName != 0)
            {
                if (vertexData.NumVertices <= _vertexBufferSize)
                {
                    vertexData.UploadToVertexBuffer(_vertexBufferName, _vertexColorsBufferName, bufferUsage);
                }
                else
                {
                    PurgeBuffers(true, false);
                }
                    
            }
            if (_vertexBufferName == 0)
            {
                int[] names = vertexData.CreateVertexBuffer(true);
                _vertexBufferName = names[0];
                _vertexColorsBufferName = names[1];
                _vertexBufferSize = vertexData.NumVertices;
            }
        }

        // rendering

        /** Draws the triangles described by the index- and vertex-buffers, or a range of them.
         *  This calls <code>beforeDraw</code>, <code>context.drawTriangles</code>, and
         *  <code>afterDraw</code>, in this order. */
        virtual public void Render(int firstIndex = 0, int numTriangles= -1)
        {
            if (numTriangles < 0) numTriangles = _indexBufferSize;
            if (numTriangles == 0) return;
            
            BeforeDraw();
            GPUInfo.CheckForOpenGLError();

#if __WINDOWS__
            GL.DrawElements(PrimitiveType.Triangles, numTriangles * 3, DrawElementsType.UnsignedShort, IntPtr.Zero);
#elif __ANDROID__
            GL.DrawElements(BeginMode.Triangles, numTriangles * 3, DrawElementsType.UnsignedShort, IntPtr.Zero);
#endif
            AfterDraw();
        }

        /** This method is called by <code>render</code>, directly before
         *  <code>context.drawTriangles</code>. It activates the program and sets up
         *  the context with the following constants and attributes:
         *
         *  <ul>
         *    <li><code>vc0-vc3</code> — MVP matrix</li>
         *    <li><code>va0</code> — vertex position (xy)</li>
         *  </ul>
         */
        virtual protected void BeforeDraw()
        {
            Program.Activate(); // create, upload, use program

            //is this the best place for this?
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferName);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferName);
            
            int attribPosition = Program.Attributes["aPosition"];
            GL.EnableVertexAttribArray(attribPosition);
            GL.VertexAttribPointer(attribPosition, 2, VertexAttribPointerType.Float, false, Vertex.SIZE, (IntPtr)Vertex.POSITION_OFFSET);
            
            int _uMvpMatrix = Program.Uniforms["uMvpMatrix"];
            Matrix4 glkMvpMatrix = MvpMatrix3D.RawData;
            GL.UniformMatrix4(_uMvpMatrix, false, ref glkMvpMatrix);

            // color & alpha are set in subclasses
        }

        /** This method is called by <code>render</code>, directly after
         *  <code>context.drawTriangles</code>. Resets vertex buffer attributes.
         */
        virtual protected void AfterDraw()
        {
            //?? context.setVertexBufferAt(0, null);
        }

        // program management

        /** Creates the program (a combination of vertex- and fragment-shader) used to render
         *  the effect with the current settings. Override this method in a subclass to create
         *  your shaders. This method will only be called once; the program is automatically stored
         *  in the <code>Painter</code> and re-used by all instances of this effect.
         *
         *  <p>The basic implementation always outputs pure white.</p>
         */
        virtual protected Program CreateProgram()
        {
            StringBuilder source = new StringBuilder("");

            // vertex shader
            AddShaderInitCode(source);
            source.AppendLine("attribute vec4 aPosition;");
            source.AppendLine("uniform mat4 uMvpMatrix;");
            // main
            source.AppendLine("void main() {");
            source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
            source.Append("}");

            string vertexShader = source.ToString();
            
            // fragment shader
            source = new StringBuilder("");
            AddShaderInitCode(source);
            // main
            source.AppendLine("void main() {");
            source.AppendLine("  gl_FragColor = vec4(1, 1, 1, 1);");
            source.Append("}");

            string fragmentShader = source.ToString();

            return new Program(vertexShader, fragmentShader);
        }

        /// <summary>
        /// Appends OpenGL shader defines, this is needed for shaders to work on both
        /// desktop OpenGL and OpenGL ES 2+.
        /// </summary>
        public static void AddShaderInitCode(StringBuilder source)
        {
#if __WINDOWS__
            source.AppendLine("#version 110");
            source.AppendLine("#define highp  ");
            source.AppendLine("#define mediump  ");
            source.AppendLine("#define lowp  ");
#else
            source.AppendLine("#version 100");
#endif
        }
        /** Override this method if the effect requires a different program depending on the
         *  current settings. Ideally, you do this by creating a bit mask encoding all the options.
         *  This method is called often, so do not allocate any temporary objects when overriding.
         *
         *  @default 0
         */
        virtual protected uint ProgramVariantName { get { return 0; } }

        /** Returns the base name for the program.
         *  @default the fully qualified class name
         */
        protected string ProgramBaseName
        {
            get { return _programBaseName; }
            set { _programBaseName = value;  }
        }

        /** Returns the full name of the program, which is used to register it at the current
         *  <code>Painter</code>.
         *
         *  <p>The default implementation efficiently combines the program's base and variant
         *  names (e.g. <code>LightEffect#42</code>). It shouldn't be necessary to override
         *  this method.</p>
         */
        protected string ProgramName
        {
            get
            {
                string baseName = ProgramBaseName;
                uint variantName = ProgramVariantName;
                Dictionary<uint, string> nameCache;
                if (!sProgramNameCache.ContainsKey(baseName))
                {
                    nameCache = new Dictionary<uint, string>();
                    sProgramNameCache[baseName] = nameCache;
                }
                else
                {
                    nameCache = sProgramNameCache[baseName];
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

        /** Returns the current program, either by creating a new one (via
         *  <code>createProgram</code>) or by getting it from the <code>Painter</code>.
         *  Do not override this method! Instead, implement <code>createProgram</code>. */
         protected Program Program
         {
            get
            {
                string name = ProgramName;
                Painter painter = Core.SparrowSharp.Painter;
                Program program = painter.GetProgram(name);

                if (program == null)
                {
                    program = CreateProgram();
                    painter.RegisterProgram(name, program);
                }
                return program;
            }
        }

        public Matrix3D MvpMatrix3D
        {
            get { return _mvpMatrix3D; }
            set { _mvpMatrix3D.CopyFrom(value); }
        }
    }
}
