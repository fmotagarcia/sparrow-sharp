using System;
using Sparrow.Utils;
using Sparrow.Geom;
using Sparrow.Core;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using sparrowsharp.Sparrow.Core;

namespace Sparrow.Core
{
	public class BaseEffect
	{
        
        public static String GetProgramName(bool hasTexture, bool useTinting)
        {
            if (hasTexture)
            {
                if (useTinting) return "SPQuad#11";
                else            return "SPQuad#10";
            }
            else
            {
                if (useTinting) return "SPQuad#01";
                else            return "SPQuad#00";
            }
        }

        private Matrix _mvpMatrix;
        private Texture _texture;
        private double _alpha; // TODO I think this should be float
        private bool _useTinting;
        private bool _premultipliedAlpha;

        private Program _program;
        private int _aPosition;
        private int _aColor;
        private int _aTexCoords;
        private int _uMvpMatrix;
        private int _uAlpha;

        public int AttribPosition 
        {
	        get { return _aPosition; }
	        set { _aPosition = value; }
        }

        public int AttribColor 
        {
	        get { return _aColor; }
	        set { _aColor = value; }
        }

        public int AttribTexCoords 
        {
	        get { return _aTexCoords; }
	        set { _aTexCoords = value; }
        }
               
        public Texture Texture 
        {
            get { return _texture; }
            set { _texture = value; }
        }
               
        public bool PremultipliedAlpha 
        {
            get { return _premultipliedAlpha; }
            set { _premultipliedAlpha = value; }
        }
                
        public Matrix MvpMatrix 
        {
            get { return _mvpMatrix; }
            set { _mvpMatrix = value; }
        }

        public bool UseTinting 
        {
            get { return _useTinting; }
            set { _useTinting = value; }
        }

        public double Alpha 
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

		public BaseEffect()
		{
            _mvpMatrix = new Matrix();
            _premultipliedAlpha = false;
            _useTinting = true;
            _alpha = 1.0f;
		}

        public void PrepareToDraw()
        {
            bool hasTexture = _texture != null;
            bool useTinting = _useTinting || _texture == null || _alpha != 1.0f;

            if (_program != null)
            {
                String programName = GetProgramName(hasTexture, useTinting);
                _program = SP.CurrentController.ProgramByName(programName);
        
                if (_program == null)
                {
                    String vertexShader = VertexShaderForTexture(_texture, useTinting);
                    String fragmentShader = FragmentShaderForTexture(_texture, useTinting);
                    _program = new Program(vertexShader, fragmentShader);
                    SP.CurrentController.RegisterProgram(_program, programName);
                }
        
                _aPosition  = _program.attributeByName("aPosition");
                _aColor     = _program.attributeByName("aColor");
                _aTexCoords = _program.attributeByName("aTexCoords");
                _uMvpMatrix = _program.uniformByName("uMvpMatrix");
                _uAlpha     = _program.uniformByName("uAlpha");
            }

            Matrix4 glkMvpMatrix = _mvpMatrix.ConvertToMatrix4();
            GL.UseProgram(_program.Name);
            GL.UniformMatrix4(_uMvpMatrix, false, ref glkMvpMatrix); // TODO check; was glUniformMatrix4fv(_uMvpMatrix, 1, NO, glkMvpMatrix.m);
    
            if (useTinting)
            {
                float alphaF = (float)_alpha; // TODO this is not nice
                if (_premultipliedAlpha) GL.Uniform4(_uAlpha, alphaF, alphaF, alphaF, alphaF);
                else GL.Uniform4(_uAlpha, 1.0f, 1.0f, 1.0f, alphaF);
            }
    
            if (hasTexture)
            {
                GL.ActiveTexture(All.Texture0);
                GL.BindTexture(All.Texture2D, _texture.Name);
            }
        }

        public void SetMvpMatrix(Matrix value)
        {
            _mvpMatrix.CopyFromMatrix(value);
        }

        public void SetAlpha(float value)
        {
            if ((value >= 1.0f && _alpha < 1.0f) || (value < 1.0f && _alpha >= 1.0f))
            {
                SP_RELEASE_AND_NIL(_program); // what is this?
            }
            _alpha = value;
        }

        public void SetUseTinting(bool value)
        {
            if (value != _useTinting)
            {
                _useTinting = value;
                SP_RELEASE_AND_NIL(_program);
            }
        }

        public void SetTexture(Texture value)
        {
            if ((_texture != null && value == null) || (_texture == null && value != null))
            {
                SP_RELEASE_AND_NIL(_program);
            }
            SP_RELEASE_AND_RETAIN(_texture, value);
        }

        private String VertexShaderForTexture(Texture texture, bool useTinting)
        {
            bool hasTexture = (texture != null);
            System.Text.StringBuilder source = new System.Text.StringBuilder("");
            
            // variables
            source.AppendLine("attribute vec4 aPosition;");
            if (useTinting) source.AppendLine("attribute vec4 aColor;");
            if (hasTexture) source.AppendLine("attribute vec2 aTexCoords;");

            source.AppendLine("uniform mat4 uMvpMatrix;");
            if (useTinting) source.AppendLine("uniform vec4 uAlpha;");
            if (useTinting) source.AppendLine("varying lowp vec4 vColor;");
            if (hasTexture) source.AppendLine("varying lowp vec2 vTexCoords;");
    
            // main
            source.AppendLine("void main() {");
            source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
            if (useTinting) source.AppendLine("  vColor = aColor * uAlpha;");
            if (hasTexture) source.AppendLine("  vTexCoords  = aTexCoords;");
            source.Append("}");
    
            return source.ToString();
        }

        private String FragmentShaderForTexture(Texture texture, bool useTinting)
        {
            bool hasTexture = (texture != null);
            System.Text.StringBuilder source = new System.Text.StringBuilder("");
    
            // variables
    
            if (useTinting) source.AppendLine("varying lowp vec4 vColor;");
    
            if (hasTexture)
            {
                source.AppendLine("varying lowp vec2 vTexCoords;");
                source.AppendLine("uniform lowp sampler2D uTexture;");
            }
    
            // main
    
            source.AppendLine("void main() {");
    
            if (hasTexture)
            {
                if (useTinting) source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords) * vColor;");
                else source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords);");
            }
            else
            {
                source.AppendLine("  gl_FragColor = vColor;");
            }
            source.Append("}");
    
            return source.ToString();
        }

	}
}

