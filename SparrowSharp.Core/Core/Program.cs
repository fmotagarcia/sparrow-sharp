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
			#if __ANDROID__
            GL.GetProgram(program, All.LinkStatus, out linked);
			#elif __IOS__ || __WINDOWS__
            GL.GetProgram(program, ProgramParameter.LinkStatus, out linked);
            #endif
            if (linked == 0)
            {
				int logLength = 0;
				#if __ANDROID__
                GL.GetProgram(program, All.InfoLogLength, out logLength);
				#elif __IOS__ || __WINDOWS__
                GL.GetProgram(program, ProgramParameter.InfoLogLength, out logLength);
                #endif

                if (logLength != 0)
                {
                    String log = "";
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

        private int CompileShader(string source, All type)
        {
            int shader = GL.CreateShader(type);
            if (shader == 0)
                return shader;
        
            //GL.ShaderSource (shader, source); // TODO: this looked like glShaderSource(shader, 1, &utfSource, NULL); check if this is OK 
            GL.ShaderSource(shader, 1, new string [] { source }, (int[])null); // sohuld be same as above just faster
            GL.CompileShader(shader);
    
            #if DEBUG
			int compiled = 0;
			#if __ANDROID__
            GL.GetShader(shader, All.CompileStatus, out compiled);
			#elif __IOS__ || __WINDOWS__
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compiled);
            #endif

            if (compiled == 0)
            {
				int logLength = 0;
				#if __ANDROID__
                GL.GetShader(shader, All.InfoLogLength, out logLength);
				#elif __IOS__ || __WINDOWS__
                GL.GetShader(shader, ShaderParameter.InfoLogLength, out logLength);
                #endif
        
                if (logLength != 0)
                {
                    string log;
                    GL.GetShaderInfoLog(shader, out log);
                    Debug.WriteLine(string.Format("Sparrow: Error compiling {0} shader: {1}", (type == All.VertexShader ? "vertex" : "fragment"), log));
                }
                GL.DeleteShader(shader);
                return 0;
            }
            #endif
    
            return shader;
        }

        private void UpdateUniforms()
        {
			int numUniforms = 0;
			#if __ANDROID__
            GL.GetProgram(Name, All.ActiveUniforms, out numUniforms);
			#elif __IOS__ || __WINDOWS__
            GL.GetProgram(Name, ProgramParameter.ActiveUniforms, out numUniforms);
            #endif

            Uniforms.Clear();
            for (int i = 0; i < numUniforms; i++)
            {
                int size;
				#if __ANDROID__
                All type;
                string rawName = GL.GetActiveUniform(Name, i, out size, out type);
                Uniforms.Add(rawName, GL.GetUniformLocation(Name, new StringBuilder(rawName)));
				#elif __IOS__ || __WINDOWS__
                ActiveUniformType type;
                string rawName = GL.GetActiveUniform(Name, i, out size, out type);
                Uniforms.Add(rawName, GL.GetUniformLocation(Name, rawName));
                #endif
            }
        }

        private void UpdateAttributes()
        {
			int numAttributes = 0;
			#if __ANDROID__ 
            GL.GetProgram(Name, All.ActiveAttributes, out numAttributes);
			#elif __IOS__ || __WINDOWS__
            GL.GetProgram(Name, ProgramParameter.ActiveAttributes, out numAttributes);
            #endif

            Attributes.Clear();
            for (int i = 0; i < numAttributes; i++)
            {
                int size;
				#if __ANDROID__
                All type;
                string rawName = GL.GetActiveAttrib(Name, i, out size, out type);
                Attributes.Add(rawName, GL.GetAttribLocation(Name, new StringBuilder(rawName)));
				#elif __IOS__ || __WINDOWS__
                ActiveAttribType type;
                string rawName = GL.GetActiveAttrib(Name, i, out size, out type);
                Attributes.Add(rawName, GL.GetAttribLocation(Name, rawName));
                #endif
            }
        }
    }
}