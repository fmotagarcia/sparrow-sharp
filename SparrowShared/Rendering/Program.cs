
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
        /// Constructor
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
            vertexShader = CompileShader(_vertexShader, ShaderType.VertexShader);
            fragmentShader = CompileShader(_fragmentShader, ShaderType.FragmentShader);

            Gl.AttachShader(program, vertexShader);
            Gl.AttachShader(program, fragmentShader);

            Gl.LinkProgram(program);

#if DEBUG
            Gl.GetProgram(program, ProgramProperty.LinkStatus, out var linked);
            if (linked == 0)
            {
                Gl.GetProgram(program, ProgramProperty.LinkStatus, out var logLength);

                if (logLength != 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.EnsureCapacity(999);
                    Gl.GetProgramInfoLog(program, 999, out _, sb);
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

        private uint CompileShader(string source, ShaderType type)
        {
            uint shader = Gl.CreateShader(type);
            if (shader == 0)
            {
                return shader;
            }

            Gl.ShaderSource(shader, new [] { source });
            Gl.CompileShader(shader);

#if DEBUG
            Gl.GetShader(shader,ShaderParameterName.CompileStatus, out var compiled);

            if (compiled == 0)
            {
                Gl.GetShader(shader, ShaderParameterName.InfoLogLength, out var logLength);

                if (logLength != 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.EnsureCapacity(999); // this is needed due to a rare Mono bug
                    Gl.GetShaderInfoLog(shader, 999, out int _, sb);
                    Debug.WriteLine("Sparrow: Error compiling shader: " + sb);
                }
                Gl.DeleteShader(shader);
                return 0;
            }
#endif
            return shader;
        }

        private void UpdateUniforms()
        {
            Gl.GetProgram(Name, ProgramProperty.ActiveUniforms, out var numUniforms);

            Uniforms.Clear();
            StringBuilder sb = new StringBuilder();
            for (uint i = 0; i < numUniforms; i++)
            {
                sb.Clear();
                Gl.GetActiveUniform(Name, i, 200, out int _, out int _, out int _, sb);
                string nameStr = sb.ToString();
                int uLoc = Gl.GetUniformLocation(Name, nameStr);
                Uniforms.Add(nameStr, uLoc);// should return uint..
            }
        }

        private void UpdateAttributes()
        {
            Gl.GetProgram(Name, ProgramProperty.ActiveAttributes, out var numAttributes);

            Attributes.Clear();
            StringBuilder sb = new StringBuilder();
            sb.EnsureCapacity(200);
            for (uint i = 0; i < numAttributes; i++)
            {
                sb.Clear();
                Gl.GetActiveAttrib(Name, i, 200, out _, out _, out _, sb);
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