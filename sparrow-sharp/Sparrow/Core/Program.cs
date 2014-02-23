using OpenTK.Graphics.ES20;
using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace Sparrow.Core
{
    public class Program
    {
        private const int MaxNameLength = 64;

        public int Name { get; private set; }

        private string _vertexShader, _fragmentShader;
        private readonly Dictionary<String, int> _uniforms;
        private readonly Dictionary<String, int> _attributes;

        public Dictionary<String, int> Uniforms
        {
            get
            {
                return _uniforms;
            }
        }

        public Dictionary<String, int> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        public Program(string vertexShader, string fragmentShader)
        {
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _uniforms = new Dictionary<string, int>();
            _attributes = new Dictionary<string, int>();

            Compile();
            UpdateUniforms();
            UpdateAttributes();
        }

        public string Description()
        {
            return string.Format("[Program {0}\n## VERTEX SHADER: ##\n{1}\n## FRAGMENT SHADER: ##\n{2}]", Name, _vertexShader, _fragmentShader);
        }

        private void Compile()
        {
            int program = GL.CreateProgram();
            int vertexShader = CompileShader(_vertexShader, All.VertexShader);
            int fragmentShader = CompileShader(_fragmentShader, All.FragmentShader);
    
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
    
            GL.LinkProgram(program);
    
            #if DEBUG
            int linked = 0;
            GL.GetProgram(program, All.LinkStatus, out linked);
    
            if (linked != 0)
            {
                int logLength = 0;
                GL.GetProgram(program, All.InfoLogLength, out logLength);
                if (logLength != 0)
                {
                    String log = "";
                    GL.GetProgramInfoLog(program, out log);
                    Debug.WriteLine("Error linking program: " + log);
                }
            }
            #endif
    
            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
    
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
    
            Name = program;
        }

        private int CompileShader(string source, All type)
        {
            int shader = GL.CreateShader(type);
            if (shader == 0)
                return shader;
        
            GL.ShaderSource(shader, source); // TODO: this looked like glShaderSource(shader, 1, &utfSource, NULL); check if this is OK 
            GL.CompileShader(shader);
    
            #if DEBUG
            int compiled;
            GL.GetShader(shader, All.CompileStatus, out compiled);
            if (compiled == 0)
            {
                int logLength;
                GL.GetShader(shader, All.InfoLogLength, out logLength);
        
                if (logLength != 0)
                {
                    string log;
                    GL.GetShaderInfoLog(shader, out log);
                    Debug.WriteLine(string.Format("Error compiling {0} shader: {1}", (type == All.VertexShader ? "vertex" : "fragment"), log));
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
            GL.GetProgram(Name, All.ActiveUniforms, out numUniforms);

            Uniforms.Clear();
            for (int i = 0; i < numUniforms; i++)
            {
                StringBuilder rawName = new StringBuilder();
                GL.GetActiveUniform(Name, i, MaxNameLength, new int[0], new int[0], new All[0], rawName);
                Uniforms[rawName.ToString()] = GL.GetUniformLocation(Name, rawName);
            }
        }

        private void UpdateAttributes()
        {
            int numAttributes;
            GL.GetProgram(Name, All.ActiveAttributes, out numAttributes);

            Attributes.Clear();
            for (int i = 0; i < numAttributes; i++)
            {
                StringBuilder rawName = new StringBuilder();
                GL.GetActiveAttrib(Name, i, MaxNameLength, new int[0], new int[0], new All[0], rawName);
                Uniforms[rawName.ToString()] = GL.GetAttribLocation(Name, rawName);
            }
        }
    }
}