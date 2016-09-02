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
        /*
        private Program _currentProgram;
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
        /// Activates the optimal shader program for the current settings; alpha and matrix uniforms are
        /// passed to the program right away, and the texture (if available) is bound.
        /// Parameters:
        /// mvpMatrix: The modelview-projection matrix used for rendering. Any vertex will be multiplied with this matrix.
        /// premultipliedAlpha:  Indicates if the color values of texture and vertices use premultiplied alpha.
        /// alpha: The alpha value with which every vertex color will be multiplied.
        /// useTinting: Set to true if you dont use textures or want to use alpha.
        /// texture: The texture that's projected onto the quad, or 'null' if there is none.
        /// </summary>
        public void PrepareToDraw(Matrix mvpMatrix, bool premultipliedAlpha, float alpha, bool useTinting, Texture texture = null)
        {
            bool hasTexture = texture != null;

            string programName;
            if (hasTexture)
            {
                programName = useTinting ? "SparrowAlphaTextureProgram" : "SparrowTextureProgram";
            }
            else
            {
                programName = "SparrowQuadProgram";
            }

            if (_currentProgram == null || _currentProgram != SparrowSharpApp.GetProgram(programName))
            {
                if (SparrowSharpApp.Programs.ContainsKey(programName))
                {
                    _currentProgram = SparrowSharpApp.GetProgram(programName);
                }
                else
                {
                    string vertexShader = VertexShaderString(hasTexture, useTinting);
                    string fragmentShader = FragmentShaderString(hasTexture, useTinting);
                    _currentProgram = new Program(vertexShader, fragmentShader);
                    SparrowSharpApp.RegisterProgram(programName, _currentProgram);
                }

                _aPosition = _currentProgram.Attributes["aPosition"];

                if (_currentProgram.Attributes.ContainsKey("aColor"))
                {
                    _aColor = _currentProgram.Attributes["aColor"];
                }
                if (_currentProgram.Attributes.ContainsKey("aTexCoords"))
                {
                    _aTexCoords = _currentProgram.Attributes["aTexCoords"];
                }

                _uMvpMatrix = _currentProgram.Uniforms["uMvpMatrix"];

                if (_currentProgram.Uniforms.ContainsKey("uAlpha"))
                {
                    _uAlpha = _currentProgram.Uniforms["uAlpha"];
                }
            }

            Matrix4 glkMvpMatrix = mvpMatrix.ConvertToMatrix4();
            GL.UseProgram(_currentProgram.Name);
            GL.UniformMatrix4(_uMvpMatrix, false, ref glkMvpMatrix); // TODO check; was glUniformMatrix4fv(_uMvpMatrix, 1, NO, glkMvpMatrix.m);

            if (useTinting)
            {
                if (premultipliedAlpha)
                {
                    GL.Uniform4(_uAlpha, alpha, alpha, alpha, alpha);
                }
                else
                {
                    GL.Uniform4(_uAlpha, 1.0f, 1.0f, 1.0f, alpha);
                }
            }

            if (hasTexture)
            {
                GL.ActiveTexture (TextureUnit.Texture0);
                GL.BindTexture (TextureTarget.Texture2D, texture.Name);
            }
        }

        private string VertexShaderString(bool hasTexture, bool useTinting)
        {
            System.Text.StringBuilder source = new System.Text.StringBuilder("");

            // variables
#if __WINDOWS__
            source.AppendLine("#version 110");
            source.AppendLine("#define highp  ");
            source.AppendLine("#define mediump  ");
            source.AppendLine("#define lowp  ");
#else
            source.AppendLine("#version 100");
#endif
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

        private string FragmentShaderString(bool hasTexture, bool useTinting)
        {
            System.Text.StringBuilder source = new System.Text.StringBuilder("");
#if __WINDOWS__
            source.AppendLine("#version 110");
            source.AppendLine("#define highp  ");
            source.AppendLine("#define mediump  ");
            source.AppendLine("#define lowp  ");
#else
            source.AppendLine("#version 100");
#endif
            // variables
            if (useTinting)
            {
                source.AppendLine("varying lowp vec4 vColor;");
            }

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
        }*/
    }
}

