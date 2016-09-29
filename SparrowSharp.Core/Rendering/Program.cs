
using System.Collections.Generic;
using System.Diagnostics;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Rendering
{
    /** A Program represents a pair of a fragment- and vertex-shader.
    *
    *  <p>This class is a convenient replacement for Stage3Ds "Program3D" class. Its main
    *  advantage is that it survives a context loss; furthermore, it makes it simple to
    *  create a program from AGAL source without having to deal with the assembler.</p>
    *
    *  <p>It is recommended to store programs in Starling's "Painter" instance via the methods
    *  <code>registerProgram</code> and <code>getProgram</code>. That way, your programs may
    *  be shared among different display objects or even Starling instances.</p>
    *
    *  @see Painter
    */
    public class Program
    {
        /// <summary>
        /// The handle of the program object needed.
        /// </summary>
        public int Name { get; private set; }

        private string _vertexShader;
        private string _fragmentShader;
        private int vertexShader;
        private int fragmentShader;

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
        }

        protected void Init(string vertexShader, string fragmentShader)
        {
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
        }

        public void Activate()
        {
            if (Name == 0)
            {
                Compile();
            }
            GL.UseProgram(Name);
        }

        public string Description()
        {
            return string.Format("[Program {0}\n## VERTEX SHADER: ##\n{1}\n## FRAGMENT SHADER: ##\n{2}]", Name, _vertexShader, _fragmentShader);
        }

        private void Compile()
        {
            int program = GL.CreateProgram();
            vertexShader = CompileShader(_vertexShader, ShaderType.VertexShader);
            fragmentShader = CompileShader(_fragmentShader, ShaderType.FragmentShader);

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program);

#if DEBUG
            int linked;
#if __WINDOWS__
            GetProgramParameterName status = GetProgramParameterName.LinkStatus;
#elif __ANDROID__
            ProgramParameter status = ProgramParameter.LinkStatus;
#endif
            GL.GetProgram(program, status, out linked);
            if (linked == 0)
            {
                int logLength;
#if __WINDOWS__
                GetProgramParameterName logLen = GetProgramParameterName.InfoLogLength;
#elif __ANDROID__
            ProgramParameter logLen = ProgramParameter.InfoLogLength;
#endif
                GL.GetProgram(program, logLen, out logLength);

                if (logLength != 0)
                {
                    string log;
                    GL.GetProgramInfoLog(program, out log);
                    Debug.WriteLine("Sparrow: Error linking program: " + log);
                }
            }
#endif

            Name = program;

            UpdateUniforms();
            UpdateAttributes();

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void Dispose()
        {
            if (Name != 0)
            {
                GL.DetachShader(Name, vertexShader);
                GL.DetachShader(Name, fragmentShader);

                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
            }
        }

        private int CompileShader(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            if (shader == 0)
            {
                return shader;
            }

            GL.ShaderSource(shader, 1, new string[] { source }, (int[])null);
            GL.CompileShader(shader);

#if DEBUG
            int compiled;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compiled);

            if (compiled == 0)
            {
                int logLength;
                GL.GetShader(shader, ShaderParameter.InfoLogLength, out logLength);

                if (logLength != 0)
                {
                    string log;
                    GL.GetShaderInfoLog(shader, out log);
                    Debug.WriteLine(string.Format("Sparrow: Error compiling {0} shader: {1}", (type == ShaderType.VertexShader ? "vertex" : "fragment"), log));
                }
                GL.DeleteShader(shader);
                return 0;
            }
#endif
            return shader;
        }

        private void UpdateUniforms()
        {
            int numUniforms;
#if __WINDOWS__
            GetProgramParameterName paramName = GetProgramParameterName.ActiveUniforms;
#elif __ANDROID__
            ProgramParameter paramName = ProgramParameter.ActiveUniforms;
#endif
            GL.GetProgram(Name, paramName, out numUniforms);

            Uniforms.Clear();
            for (int i = 0; i < numUniforms; i++)
            {
                int size;
                ActiveUniformType type;
                string rawName = GL.GetActiveUniform(Name, i, out size, out type);
                Uniforms.Add(rawName, GL.GetUniformLocation(Name, rawName));
            }
        }

        private void UpdateAttributes()
        {
            int numAttributes;
#if __WINDOWS__
            GetProgramParameterName paramName = GetProgramParameterName.ActiveAttributes;
#elif __ANDROID__
            ProgramParameter paramName = ProgramParameter.ActiveAttributes;
#endif
            GL.GetProgram(Name, paramName, out numAttributes);

            Attributes.Clear();
            for (int i = 0; i < numAttributes; i++)
            {
                int size;
                ActiveAttribType type;
                string rawName = GL.GetActiveAttrib(Name, i, out size, out type);
                Attributes.Add(rawName, GL.GetAttribLocation(Name, rawName));
            }
        }

    }
}