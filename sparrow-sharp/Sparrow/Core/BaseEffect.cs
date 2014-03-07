using System;
using Sparrow.Geom;
using Sparrow.Core;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace Sparrow.Core
{
    public class BaseEffect
    {
        public static string GetProgramName(bool hasTexture, bool useTinting)
        {
            if (hasTexture)
            {
                if (useTinting)
                    return "SPQuad#11";
                else
                    return "SPQuad#10";
            }
            else
            {
                if (useTinting)
                    return "SPQuad#01";
                else
                    return "SPQuad#00";
            }
        }

        private Matrix _mvpMatrix;
        private Texture _texture;
        private float _alpha;
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
            set {
				if ((_texture != null && value == null) || (_texture == null && value != null))
				{
					_program = null;
				}
				_texture = value;
			}
        }

        public bool PremultipliedAlpha
        {
            get { return _premultipliedAlpha; }
            set { _premultipliedAlpha = value; }
        }

        public Matrix MvpMatrix
        {
            get { return _mvpMatrix; }
			set { _mvpMatrix.CopyFromMatrix(value); }
        }

        public bool UseTinting
        {
            get { return _useTinting; }
            set {
				if (value != _useTinting)
				{
					_useTinting = value;
					_program = null;
				}
			}
        }

        public float Alpha
        {
            get { return _alpha; }
			set { 
				if ((value >= 1.0f && _alpha < 1.0f) || (value < 1.0f && _alpha >= 1.0f)) {
					_program = null;
				}
				_alpha = value; 
			}
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

            if (_program == null)
            {
                string programName = GetProgramName(hasTexture, useTinting);

				if (SP.Programs.ContainsKey(programName))
				{
				    _program = SP.Programs[programName];
				}
        
                if (_program == null)
                {
                    string vertexShader = VertexShaderForTexture(_texture, useTinting);
                    string fragmentShader = FragmentShaderForTexture(_texture, useTinting);
                    _program = new Program(vertexShader, fragmentShader);
					SP.RegisterProgram(programName, _program);
                }
        
                _aPosition = _program.Attributes["aPosition"];
                
				if (_program.Attributes.ContainsKey("aColor"))
				{
					_aColor = _program.Attributes["aColor"];
				}
                if (_program.Attributes.ContainsKey("aTexCoords"))
                {
                    _aTexCoords = _program.Attributes["aTexCoords"];
                }

                _uMvpMatrix = _program.Uniforms["uMvpMatrix"];

				if (_program.Uniforms.ContainsKey("uAlpha"))
				{
					_uAlpha = _program.Uniforms["uAlpha"];
				}
            }

            Matrix4 glkMvpMatrix = _mvpMatrix.ConvertToMatrix4();
            GL.UseProgram(_program.Name);
            GL.UniformMatrix4(_uMvpMatrix, false, ref glkMvpMatrix); // TODO check; was glUniformMatrix4fv(_uMvpMatrix, 1, NO, glkMvpMatrix.m);
    
            if (useTinting)
            {
                if (_premultipliedAlpha)
					GL.Uniform4(_uAlpha, _alpha, _alpha, _alpha, _alpha);
                else
					GL.Uniform4(_uAlpha, 1.0f, 1.0f, 1.0f, _alpha);
            }
    
            if (hasTexture)
            {
                GL.ActiveTexture(All.Texture0);
                GL.BindTexture(All.Texture2D, _texture.Name);
            }
        }

        private String VertexShaderForTexture(Texture texture, bool useTinting)
        {
            bool hasTexture = (texture != null);
            System.Text.StringBuilder source = new System.Text.StringBuilder("");
            
            // variables
            source.AppendLine("attribute vec4 aPosition;");
            if (useTinting)
                source.AppendLine("attribute vec4 aColor;");
            if (hasTexture)
                source.AppendLine("attribute vec2 aTexCoords;");

            source.AppendLine("uniform mat4 uMvpMatrix;");
            if (useTinting)
                source.AppendLine("uniform vec4 uAlpha;");
            if (useTinting)
                source.AppendLine("varying lowp vec4 vColor;");
            if (hasTexture)
                source.AppendLine("varying lowp vec2 vTexCoords;");
    
            // main
            source.AppendLine("void main() {");
            source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
            if (useTinting)
                source.AppendLine("  vColor = aColor * uAlpha;");
            if (hasTexture)
                source.AppendLine("  vTexCoords  = aTexCoords;");
            source.Append("}");
    
            return source.ToString();
        }

        private String FragmentShaderForTexture(Texture texture, bool useTinting)
        {
            bool hasTexture = (texture != null);
            System.Text.StringBuilder source = new System.Text.StringBuilder("");
    
            // variables
    
            if (useTinting)
                source.AppendLine("varying lowp vec4 vColor;");
    
            if (hasTexture)
            {
                source.AppendLine("varying lowp vec2 vTexCoords;");
                source.AppendLine("uniform lowp sampler2D uTexture;");
            }
    
            // main
    
            source.AppendLine("void main() {");
    
            if (hasTexture)
            {
                if (useTinting)
                    source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords) * vColor;");
                else
                    source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords);");
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

