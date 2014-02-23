using OpenTK.Graphics.ES20;
using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace sparrowsharp.Sparrow.Core
{

class Program
{
    private int _name;
    private String _vertexShader;
    private String _fragmentShader;
    private readonly Dictionary<String, int> _uniforms = new Dictionary<string, int>();
    private readonly Dictionary<String, int> _attributes = new Dictionary<string, int>();

    public int Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public Program(String vertexShader, String fragmentShader)
    {
        _vertexShader = vertexShader;
        _fragmentShader = fragmentShader;
        
        Compile();
        UpdateUniforms();
        UpdateAttributes();
    }

    public int uniformByName(String name)
    {
        return _uniforms[name];
    }

    public int attributeByName(String name)
    {
        return _attributes[name];
    }

    public String description()
    {
        return "[Program " + _name + " VERTEX SHADER: " + _vertexShader + " FRAGMENT SHADER: " + _fragmentShader + "]";
    }

    private void Compile() 
    {
        int program = GL.CreateProgram();
        int vertexShader   = CompileShader(_vertexShader, All.VertexShader);
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
    
        _name = program;
    }

    private int CompileShader(String source, All type)
    {
        int shader = GL.CreateShader(type);
        if (shader == 0) return shader;
        
        GL.ShaderSource(shader, source); // TODO: this looked like glShaderSource(shader, 1, &utfSource, NULL); check if this is OK 
        GL.CompileShader(shader);
    
        #if DEBUG
    
        int compiled = 0;
        GL.GetShader(shader, All.CompileStatus, out compiled);
        if (compiled == 0)
        {
            int logLength = 0;
            GL.GetShader(shader, All.InfoLogLength, out logLength);
        
            if (logLength != 0)
            {
                String log = "";
                GL.GetShaderInfoLog(shader, out log);
                Debug.WriteLine(@"Error compiling shader: " + (type == All.VertexShader ? "vertex" : "fragment") + log);
            }
            GL.DeleteShader(shader);
            return 0;
        }
    
        #endif
    
        return shader;
    }

    private void UpdateUniforms()
    {
        int MAX_NAME_LENGTH = 64;
    
        int numUniforms = 0;
        GL.GetProgram(_name, All.ActiveUniforms, out numUniforms);

        _uniforms.Clear();
    
        for (int i=0; i<numUniforms; ++i)
        {
            StringBuilder builder = new StringBuilder("");
            //TODO was glGetActiveUniform(_name, i, MAX_NAME_LENGTH, NULL, NULL, NULL, rawName);
            // this is likely wrong
            int length = 0;
            int size = 0;
            All type = new All();
            GL.GetActiveUniform(_name, i, MAX_NAME_LENGTH, out length, out size, out type, builder);
            _uniforms[builder.ToString()] = GL.GetUniformLocation(_name, builder);
        }
    }

    private void UpdateAttributes()
    {
        int MAX_NAME_LENGTH = 64;
    
        int numAttributes = 0;
        GL.GetProgram(_name, All.ActiveAttributes, out numAttributes);

        _attributes.Clear();
    
        for (int i=0; i<numAttributes; ++i)
        {
            // glGetActiveAttrib(_name, i, MAX_NAME_LENGTH, NULL, NULL, NULL, rawName);
            StringBuilder builder = new StringBuilder("");
            // TODO this is likely wrong
            int length = 0;
            int size = 0;
            All type = new All();
            GL.GetActiveAttrib(_name, i, MAX_NAME_LENGTH, out length, out size, out type, builder);
            _attributes[builder.ToString()] = GL.GetAttribLocation(_name, builder);
        }
    }

}
}