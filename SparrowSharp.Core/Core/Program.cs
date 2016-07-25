using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.ES20;

namespace Sparrow.Core
{
    /// <summary>
    /// A Program wraps a GLSL program (containing the source code for both vertex and fragment shader)
    /// into an object.

    /// Use the 'Uniforms' and 'Attributes' dictionaries to query the index of the respective
    /// variables.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The handle of the program object needed.
        /// </summary>
        public int Name { get; private set; }

        private string _vertexShader;
        private string _fragmentShader;
        public readonly Dictionary<String, int> Uniforms = new Dictionary<string, int>();
        public readonly Dictionary<String, int> Attributes = new Dictionary<string, int>();
        // Used by derived classes that need to manually call Init()
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

            Compile();
        }

        public string Description()
        {
            return string.Format("[Program {0}\n## VERTEX SHADER: ##\n{1}\n## FRAGMENT SHADER: ##\n{2}]", Name, _vertexShader, _fragmentShader);
        }

        private void Compile()
        {
            int program = GL.CreateProgram();
            int vertexShader = CompileShader(_vertexShader, ShaderType.VertexShader);
            int fragmentShader = CompileShader(_fragmentShader, ShaderType.FragmentShader);

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program);

#if DEBUG
            int linked;
            GL.GetProgram(program, ProgramParameter.LinkStatus, out linked);
            if (linked == 0)
            {
                int logLength;
                GL.GetProgram(program, ProgramParameter.InfoLogLength, out logLength);

                if (logLength != 0)
                {
                    String log;
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
            GL.GetProgram(Name, ProgramParameter.ActiveUniforms, out numUniforms);

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
            GL.GetProgram(Name, ProgramParameter.ActiveAttributes, out numAttributes);

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