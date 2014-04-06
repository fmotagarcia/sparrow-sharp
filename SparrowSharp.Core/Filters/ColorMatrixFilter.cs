using Sparrow.Core;
using OpenTK;
using Sparrow;
using OpenTK.Graphics.ES20;
using System.Text;

namespace SparrowSharp.Filters
{
    /// <summary>
    /// The ColorMatrixFilter class lets you apply a 4x5 matrix transformation on the RGBA color and 
    /// alpha values of every pixel in the input image to produce a result with a new set of RGBA color and 
    /// alpha values. It allows saturation changes, hue rotation, luminance to alpha, and various other 
    /// effects.

    /// The class contains several convenience methods for frequently used color adjustments. All those 
    ///	methods change the current matrix, which means you can easily combine them in one filter:

    ///	// create an inverted filter with 50% saturation and 180° hue rotation
    ///	ColorMatrixFilter filter = new ColorMatrixFilter();
    ///	filter.Invert();
    /// filter.AdjustSaturation(-0.5);
    /// filter.AdjustHue(1.0);

    /// If you want to gradually animate one of the predefined color adjustments, either reset the matrix 
    /// after each step, or use an identical adjustment value for each step; the changes will add up.
    /// </summary>
    public class ColorMatrixFilter : FragmentFilter
    {
        /// <summary>
        /// The color matrix object used to apply the filter.
        /// </summary>
        public ColorMatrix ColorMatrix;
        private static readonly string ColorMatrixProgram = "ColorMatrixProgram";
        private Program _shaderProgram;
        private bool _colorMatrixDirty;
        private Matrix4 _shaderMatrix;
        // offset in range 0-1, changed order
        private Vector4 _shaderOffset;
        private int _uMvpMatrix;
        private int _uColorMatrix;
        private int _uColorOffset;

        public ColorMatrixFilter(ColorMatrix matrix = null)
        {
            this.ColorMatrix = matrix ?? new ColorMatrix();
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Inverts the colors of the filtered objects.
        /// </summary>
        public void Invert()
        {
            this.ColorMatrix.Invert();
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Changes the saturation. Typical values are in the range(-1, 1). Values above zero will raise,
        /// values below zero will reduce the saturation. '-1' will produce a grayscale image.
        /// </summary>
        public void AdjustSaturation(float saturation)
        {
            this.ColorMatrix.AdjustSaturation(saturation);
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Changes the contrast. Typical values are in the range(-1, 1). Values above zero will raise,
        /// values below zero will reduce the contrast.
        /// </summary>
        public void AdjustContrast(float contrast)
        {
            this.ColorMatrix.AdjustContrast(contrast);
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Changes the brightness. Typical values are in the range(-1, 1). Values above zero will make the
        /// image brighter, values below zero will make it darker.
        /// </summary>
        public void AdjustBrightness(float brightness)
        {
            this.ColorMatrix.AdjustBrightness(brightness);
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Changes the hue of the image. Typical values are in the range(-1, 1).
        /// </summary>
        public void AdjustHue(float hue)
        {
            this.ColorMatrix.AdjustHue(hue);
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Changes the filter matrix back to the identity matrix.
        /// </summary>
        public void Reset()
        {
            this.ColorMatrix.Identity();
            _colorMatrixDirty = true;
        }

        /// <summary>
        /// Concatenates the current matrix with another one.
        /// </summary>
        public void ConcatColorMatrix(ColorMatrix colorMatrix)
        {
            this.ColorMatrix.ConcatColorMatrix(colorMatrix);
            _colorMatrixDirty = true;
        }

        override protected void CreatePrograms()
        {
            if (_shaderProgram == null)
            {
                _shaderProgram = SparrowSharpApp.GetProgram(ColorMatrixProgram);

                if (_shaderProgram == null)
                {
                    _shaderProgram = new Program(FragmentFilter.StandardVertexShader(), GetFragmentShader());

                    SparrowSharpApp.RegisterProgram(ColorMatrixProgram, _shaderProgram);
                }

                VertexPosID = _shaderProgram.Attributes["aPosition"];
                TexCoordsID = _shaderProgram.Attributes["aTexCoords"];

                _uColorMatrix = _shaderProgram.Uniforms["uColorMatrix"];
                _uColorOffset = _shaderProgram.Uniforms["uColorOffset"];
                _uMvpMatrix = _shaderProgram.Uniforms["uMvpMatrix"];
            }
        }

        override protected void ActivateWithPass(int pass, Sparrow.Textures.Texture texture, Sparrow.Geom.Matrix mvpMatrix)
        {
            if (_colorMatrixDirty)
            {
                UpdateShaderMatrix();
            }

            GL.UseProgram(_shaderProgram.Name);

            Matrix4 mvp = mvpMatrix.ConvertToMatrix4();
            GL.UniformMatrix4(_uMvpMatrix, false, ref mvp);

            GL.UniformMatrix4(_uColorMatrix, false, ref _shaderMatrix);

            GL.Uniform4(_uColorOffset, ref _shaderOffset);
        }

        private void UpdateShaderMatrix()
        {
            // the shader needs the matrix components in a different order,
            // and it needs the offsets in the range 0-1.
            float[] matrix = ColorMatrix.Values;

            _shaderMatrix = new Matrix4(
                matrix[0], matrix[1], matrix[2], matrix[3],
                matrix[5], matrix[6], matrix[7], matrix[8],
                matrix[10], matrix[11], matrix[12], matrix[13],
                matrix[15], matrix[16], matrix[17], matrix[18]
            );

            _shaderOffset = new Vector4(
                matrix[4] / 255.0f, matrix[9] / 255.0f, matrix[14] / 255.0f, matrix[19] / 255.0f
            );

            _colorMatrixDirty = false;
        }

        private string GetFragmentShader()
        {

            StringBuilder source = new StringBuilder("");
            #if __WINDOWS__
			source.AppendLine("uniform mat4 uColorMatrix;");
			source.AppendLine("uniform vec4 uColorOffset;");
			source.AppendLine("uniform sampler2D uTexture;");
			source.AppendLine("varying vec2 vTexCoords;");
			source.AppendLine("const vec4 MIN_COLOR = vec4(0, 0, 0, 0.0001);");
            #else
            source.AppendLine("uniform lowp mat4 uColorMatrix;");
            source.AppendLine("uniform lowp vec4 uColorOffset;");
            source.AppendLine("uniform lowp sampler2D uTexture;");
            source.AppendLine("varying lowp vec2 vTexCoords;");
            source.AppendLine("const lowp vec4 MIN_COLOR = vec4(0, 0, 0, 0.0001);");
            #endif

            source.AppendLine("void main() {");

            source.AppendLine("  lowp vec4 texColor = texture2D(uTexture, vTexCoords);"); // read texture color
            source.AppendLine("  texColor = max(texColor, MIN_COLOR);");                  // avoid division through zero in next step
            source.AppendLine("  texColor.xyz /= texColor.www;");                         // restore original(non-PMA) RGB values
            source.AppendLine("  texColor *= uColorMatrix;");                             // multiply color with 4x4 matrix
            source.AppendLine("  texColor += uColorOffset;");                             // add offset
            source.AppendLine("  texColor.xyz *= texColor.www;");                         // multiply with alpha again(PMA)
            source.AppendLine("  gl_FragColor = texColor;");

            source.AppendLine("}");

            return source.ToString();
        }
    }
}

