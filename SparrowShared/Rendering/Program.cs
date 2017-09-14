
using System.Collections.Generic;
using System.Diagnostics;
using OpenGL;
using System.Text;
using Sparrow.Core;

namespace Sparrow.Rendering
{
    /// <summary>
    /// A Program represents a pair of a fragment- and vertex-shader.
    ///
    /// <para>It is recommended to store programs in Sparrow's "Painter" instance via the methods
    /// <code>RegisterProgram()</code> and <code>GetProgram()</code>. That way, your programs may
    /// be shared among different display objects or even Sparrow instances.</para>
    ///
    /// <see cref="Painter"/>
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The handle of the program object needed.
        /// </summary>
        public uint Name { get; private set; }

        private string _vertexShader;
        private string _fragmentShader;
        private uint vertexShader;
        private uint fragmentShader;

        public readonly Dictionary<string, int> Uniforms = new Dictionary<string, int>();
        public readonly Dictionary<string, int> Attributes = new Dictionary<string, int>();
        
        protected Program()
        {
        }

        /// <summary>
        /// Initializes a GLSL program by compiling vertex and fragment shaders from source. In debug
        /// mode, compilation erros are logged into the console.
        /// </summary>
        public Program(string vertexShader, string fragmentShader)
        {
            Init(vertexShader, fragmentShader);
            SparrowSharp.ContextCreated += DisposeProgram;
        }

        protected void Init(string initVertexShader, string initFragmentShader)
        {
            _vertexShader = initVertexShader;
            _fragmentShader = initFragmentShader;
        }


        /// <summary>
        /// Disposes the shaders.
        /// </summary>
        public void Dispose()
        {
            SparrowSharp.ContextCreated -= DisposeProgram;
            Gl.DetachShader(Name, vertexShader);
            Gl.DetachShader(Name, fragmentShader);

            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);
            DisposeProgram();
        }


        public void Activate()
        {
            if (Name == 0)
            {
                Compile();
            }
            Gl.UseProgram(Name);
        }

        public string Description()
        {
            return $"[Program {Name}\n## VERTEX SHADER: ##\n{_vertexShader}\n## FRAGMENT SHADER: ##\n{_fragmentShader}]";
        }

        private void Compile()
        {
            uint program = Gl.CreateProgram();
            vertexShader = CompileShader(_vertexShader, Gl.VERTEX_SHADER);
            fragmentShader = CompileShader(_fragmentShader, Gl.FRAGMENT_SHADER);

            Gl.AttachShader(program, vertexShader);
            Gl.AttachShader(program, fragmentShader);

            Gl.LinkProgram(program);

#if DEBUG
            int linked;
            Gl.GetProgram(program, Gl.LINK_STATUS, out linked);
            if (linked == 0)
            {
                int logLength;
                Gl.GetProgram(program, Gl.INFO_LOG_LENGTH, out logLength);

                if (logLength != 0)
                {
                    int logSize;
                    StringBuilder sb = new StringBuilder();
                    Gl.GetProgramInfoLog(program, 9999, out logSize, sb);
                    Debug.WriteLine("Sparrow: Error linking program: " + sb);
                }
            }
#endif

            Name = program;

            UpdateUniforms();
            UpdateAttributes();

            Gl.DetachShader(program, vertexShader);
            Gl.DetachShader(program, fragmentShader);

            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);
        }

        private uint CompileShader(string source, int type)
        {
            uint shader = Gl.CreateShader(type);
            if (shader == 0)
            {
                return shader;
            }

            Gl.ShaderSource(shader, new [] { source });
            Gl.CompileShader(shader);

#if DEBUG
            int compiled;
            Gl.GetShader(shader, Gl.COMPILE_STATUS, out compiled);

            if (compiled == 0)
            {
                int logLength;
                Gl.GetShader(shader, Gl.INFO_LOG_LENGTH, out logLength);

                if (logLength != 0)
                {
                    int logSize;
                    StringBuilder sb = new StringBuilder();
                    Gl.GetShaderInfoLog(shader, 9999, out logSize, sb);
                    Debug.WriteLine("Sparrow: Error compiling {0} shader: {1}", (type == Gl.VERTEX_SHADER ? "vertex" : "fragment"), sb);
                }
                Gl.DeleteShader(shader);
                return 0;
            }
#endif
            return shader;
        }

        private void UpdateUniforms()
        {
            int numUniforms;
            Gl.GetProgram(Name, Gl.ACTIVE_UNIFORMS, out numUniforms);

            Uniforms.Clear();
            StringBuilder sb = new StringBuilder();
            for (uint i = 0; i < numUniforms; i++)
            {
                sb.Clear();
                int size;
                int len;
                int type;
                Gl.GetActiveUniform(Name, i, 200, out len, out size, out type, sb);
                string nameStr = sb.ToString();
                Uniforms.Add(nameStr, Gl.GetUniformLocation(Name, nameStr));// should return uint..
            }
        }

        private void UpdateAttributes()
        {
            int numAttributes;
            Gl.GetProgram(Name, Gl.ACTIVE_ATTRIBUTES, out numAttributes);

            Attributes.Clear();
            StringBuilder sb = new StringBuilder();
            for (uint i = 0; i < numAttributes; i++)
            {
                sb.Clear();
                int size;
                int len;
                int type;
                Gl.GetActiveAttrib(Name, i, 200, out len, out size, out type, sb);
                string nameStr = sb.ToString();
                Attributes.Add(nameStr, Gl.GetAttribLocation(Name, nameStr)); // should return uint..
            }
        }

        private void DisposeProgram()
        {
            if (Name != 0)
            {
                Name = 0;
            }
        }

    }
}