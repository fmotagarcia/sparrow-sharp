using System;
using Sparrow.Geom;
using Sparrow.Textures;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace Sparrow.Core
{
    /// <summary>
    /// BaseEffect simplifies the rendering of colored and/or textured polygons.
    /// Configure a base effect by updating its state properties. When you call 'prepareToDraw', it
    /// will choose the optimal shader program for the given settings and will activate that program.
    /// Alpha and matrix uniforms will be passed to the program automatically, and the texture will be
    /// bound.
    /// </summary>
    public class BaseEffect
    {
        private static string GetProgramName(bool hasTexture, bool useTinting)
        {
            if (hasTexture)
            {
                return useTinting ? "SPQuad#11" : "SPQuad#10";
            }
            return useTinting ? "SPQuad#01" : "SPQuad#00";
        }

        private readonly Matrix _mvpMatrix;
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

        /// <summary>
        /// The index of the vertex attribute storing the position vector.
        /// </summary>
        public int AttribPosition
        {
            get { return _aPosition; }
        }

        /// <summary>
        /// The index of the vertex attribute storing the color vector.
        /// </summary>
        public int AttribColor
        {
            get { return _aColor; }
        }

        /// <summary>
        /// The index of the vertex attribute storing the two texture coordinates.
        /// </summary>
        public int AttribTexCoords
        {
            get { return _aTexCoords; }
        }

        /// <summary>
        /// The texture that's projected onto the quad, or 'null' if there is none. (Default: 'null')
        /// </summary>
        public Texture Texture
        {
            set
            {
                if ((_texture != null && value == null) || (_texture == null && value != null))
                {
                    _program = null;
                }
                _texture = value;
            }
        }

        /// <summary>
        /// Indicates if the color values of texture and vertices use premultiplied alpha. (Default: 'false')
        /// </summary>
        public bool PremultipliedAlpha
        {
            set { _premultipliedAlpha = value; }
        }

        /// <summary>
        /// The modelview-projection matrix used for rendering. Any vertex will be multiplied with this
        /// matrix. (Default: identity matrix)
        /// </summary>
        public Matrix MvpMatrix
        {
            set { _mvpMatrix.CopyFromMatrix(value); }
        }

        /// <summary>
        /// Indicates if the colors of the vertices should tint the texture colors. The iPad 1 profits
        /// immensely from the very simple fragment shader that can be used when tinting is deactivated.
        /// Note that an alpha value different to "1" will still force tinting to be used. (Default: 'true')
        /// </summary>
        public bool UseTinting
        {
            get { return _useTinting; }
            set
            {
                if (value != _useTinting)
                {
                    _useTinting = value;
                    _program = null;
                }
            }
        }

        /// <summary>
        /// The alpha value with which every vertex color will be multiplied. (Default: 1)
        /// </summary>
        public float Alpha
        {
            set
            {
                if ((value >= 1.0f && _alpha < 1.0f) || (value < 1.0f && _alpha >= 1.0f))
                {
                    _program = null;
                }
                _alpha = value;
            }
        }

        public BaseEffect()
        {
            _mvpMatrix = Matrix.Create();
            _premultipliedAlpha = false;
            _useTinting = true;
            _alpha = 1.0f;
        }

        /// <summary>
        /// Activates the optimal shader program for the current settings; alpha and matrix uniforms are
        /// passed to the program right away, and the texture (if available) is bound.
        /// </summary>
        public void PrepareToDraw()
        {
            bool hasTexture = _texture != null;
            bool useTinting = _useTinting || _texture == null || _alpha != 1.0f;

            if (_program == null)
            {
                string programName = GetProgramName(hasTexture, useTinting);

                if (SparrowSharpApp.Programs.ContainsKey(programName))
                {
                    _program = SparrowSharpApp.Programs[programName];
                }

                if (_program == null)
                {
                    string vertexShader = VertexShaderForTexture(_texture, useTinting);
                    string fragmentShader = FragmentShaderForTexture(_texture, useTinting);
                    _program = new Program(vertexShader, fragmentShader);
                    SparrowSharpApp.RegisterProgram(programName, _program);
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
                {
                    GL.Uniform4(_uAlpha, _alpha, _alpha, _alpha, _alpha);
                }
                else
                {
                    GL.Uniform4(_uAlpha, 1.0f, 1.0f, 1.0f, _alpha);
                }
            }

            if (hasTexture)
            {
                #if __ANDROID__
                GL.ActiveTexture(All.Texture0);
                GL.BindTexture(All.Texture2D, _texture.Name);
                #elif __IOS__ || __WINDOWS__
				GL.ActiveTexture (TextureUnit.Texture0);
				GL.BindTexture (TextureTarget.Texture2D, _texture.Name);
                #endif
            }
        }

        private String VertexShaderForTexture(Texture texture, bool useTinting)
        {
            bool hasTexture = (texture != null);
            System.Text.StringBuilder source = new System.Text.StringBuilder("");

            // variables
            source.AppendLine("attribute vec4 aPosition;");
            if (useTinting)
            {
                source.AppendLine("attribute vec4 aColor;");
            }
            if (hasTexture)
            {
                source.AppendLine("attribute vec2 aTexCoords;");
            }
            source.AppendLine("uniform mat4 uMvpMatrix;");
            if (useTinting)
            {
                source.AppendLine("uniform vec4 uAlpha;");
            }
            if (useTinting)
            {
                source.AppendLine("varying lowp vec4 vColor;");
            }

            if (hasTexture)
            {
                source.AppendLine("varying lowp vec2 vTexCoords;");
            }

            // main
            source.AppendLine("void main() {");
            source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
            if (useTinting)
            {
                source.AppendLine("  vColor = aColor * uAlpha;");
            }
            if (hasTexture)
            {
                source.AppendLine("  vTexCoords  = aTexCoords;");
            }
            source.Append("}");

            return source.ToString();
        }

        private String FragmentShaderForTexture(Texture texture, bool useTinting)
        {
            bool hasTexture = (texture != null);
            System.Text.StringBuilder source = new System.Text.StringBuilder("");

            // variables
            if (useTinting)
            {
                source.AppendLine("varying lowp vec4 vColor;");
            }

            if (hasTexture)
            {
                source.AppendLine("varying lowp vec2 vTexCoords;");
#if __WINDOWS__
                source.AppendLine("uniform sampler2D uTexture;");
#else
                source.AppendLine("uniform lowp sampler2D uTexture;");
#endif
            }

            // main
            source.AppendLine("void main() {");
            if (hasTexture)
            {
                if (useTinting)
                {
                    source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords) * vColor;");
                }
                else
                {
                    source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords);");
                }
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

