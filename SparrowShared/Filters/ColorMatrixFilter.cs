using Sparrow.Rendering;
using OpenGL;
using Sparrow.Geom;

namespace Sparrow.Filters
{
    /** The ColorMatrixFilter class lets you apply a 4x5 matrix transformation to the color
     *  and alpha values of every pixel in the input image to produce a result with a new set
     *  of color and alpha values. This allows saturation changes, hue rotation,
     *  luminance to alpha, and various other effects.
     *
     *  <p>The class contains several convenience methods for frequently used color
     *  adjustments. All those methods change the current matrix, which means you can easily
     *  combine them in one filter:</p>
     *
     *  <listing>
     *  // create an inverted filter with 50% saturation and 180° hue rotation
     *  var filter:ColorMatrixFilter = new ColorMatrixFilter();
     *  filter.Invert();
     *  filter.AdjustSaturation(-0.5);
     *  filter.AdjustHue(1.0);</listing>
     *
     *  <p>If you want to gradually animate one of the predefined color adjustments, either reset
     *  the matrix after each step, or use an identical adjustment value for each step; the
     *  changes will add up.</p>
     */
    public class ColorMatrixFilter : FragmentFilter
    {
        public ColorMatrixFilter(ColorMatrix matrix = null)
        {
            if (matrix != null)
            {
                ColorEffect.Matrix = matrix;
                ColorEffect.UpdateShaderMatrix();
            }
        }

        /// <summary>
        /// Inverts the colors of the filtered objects.
        /// </summary>
        public void Invert()
        {
            ColorEffect.Matrix.Invert();
            ColorEffect.UpdateShaderMatrix();
            SetRequiresRedraw();
        }

        /// <summary>
        /// Changes the saturation. Typical values are in the range(-1, 1). Values above zero will raise,
        /// values below zero will reduce the saturation. '-1' will produce a grayscale image.
        /// </summary>
        public void AdjustSaturation(float saturation)
        {
            ColorEffect.Matrix.AdjustSaturation(saturation);
            ColorEffect.UpdateShaderMatrix();
            SetRequiresRedraw();
        }

        /// <summary>
        /// Changes the contrast. Typical values are in the range(-1, 1). Values above zero will raise,
        /// values below zero will reduce the contrast.
        /// </summary>
        public void AdjustContrast(float contrast)
        {
            ColorEffect.Matrix.AdjustContrast(contrast);
            ColorEffect.UpdateShaderMatrix();
            SetRequiresRedraw();
        }

        /// <summary>
        /// Changes the brightness. Typical values are in the range(-1, 1). Values above zero will make the
        /// image brighter, values below zero will make it darker.
        /// </summary>
        public void AdjustBrightness(float brightness)
        {
            ColorEffect.Matrix.AdjustBrightness(brightness);
            ColorEffect.UpdateShaderMatrix();
            SetRequiresRedraw();
        }

        /// <summary>
        /// Changes the hue of the image. Typical values are in the range(-1, 1).
        /// </summary>
        public void AdjustHue(float hue)
        {
            ColorEffect.Matrix.AdjustHue(hue);
            ColorEffect.UpdateShaderMatrix();
            SetRequiresRedraw();
        }

        /// <summary>
        /// Concatenates the current matrix with another one.
        /// </summary>
        public void ConcatColorMatrix(ColorMatrix colorMatrix)
        {
            ColorEffect.Matrix.ConcatColorMatrix(colorMatrix);
            ColorEffect.UpdateShaderMatrix();
            SetRequiresRedraw();
        }

        protected override Effect CreateEffect()
        {
            return new ColorMatrixEffect();
        }

        public ColorMatrix Matrix
        {
            set
            {
                ColorEffect.Matrix = value;
                ColorEffect.UpdateShaderMatrix();
                SetRequiresRedraw();
            }
            get
            {
                return ColorEffect.Matrix;
            }
        }

        private ColorMatrixEffect ColorEffect { get { return Effect as ColorMatrixEffect; } }
}


    internal class ColorMatrixEffect : Effect
    {
        /// <summary>
        /// The color matrix object used to apply the filter.
        /// </summary>
        public ColorMatrix Matrix;
        
        private Matrix3D _shaderMatrix; // offset in range 0-1, changed order
        // offset in range 0-1, changed order
        private float[] _shaderOffset;

        public ColorMatrixEffect()
        {
            Matrix = new ColorMatrix();
            _shaderMatrix = Matrix3D.Create();
        }
        
        protected override Program CreateProgram()
        {
            string fragmentShader = AddShaderInitCode() + @"
            uniform lowp mat4 uColorMatrix;
            uniform lowp vec4 uColorOffset;
            uniform lowp sampler2D uTexture;
            in lowp vec2 vTexCoords;
            out lowp vec4 fragColor;
            const lowp vec4 MIN_COLOR = vec4(0, 0, 0, 0.0001);
            
            void main() {
              lowp vec4 texColor = texture(uTexture, vTexCoords);   // read texture color
              texColor = max(texColor, MIN_COLOR);                  // avoid division through zero in next step
              texColor.xyz /= texColor.www;                         // restore original(non-PMA) RGB values
              texColor *= uColorMatrix;                             // multiply color with 4x4 matrix
              texColor += uColorOffset;                             // add offset
              texColor.xyz *= texColor.www;                         // multiply with alpha again(PMA)
              fragColor = texColor;
            }";
            return new Program(StdVertexShader, fragmentShader);
        }

        protected override void BeforeDraw()
        {
            base.BeforeDraw();

            int uColorMatrix = Program.Uniforms["uColorMatrix"];
            var sm = _shaderMatrix.RawData;
            Gl.UniformMatrix4(uColorMatrix, 1, false, sm);
            int uColorOffset = Program.Uniforms["uColorOffset"];
            Gl.Uniform4(uColorOffset, 1, _shaderOffset);
        }
        
        /// <summary>
        /// Updates the actual shader matrix.
        /// Always call this after you are finished manipulating the color matrix
        /// </summary>
        public void UpdateShaderMatrix()
        {
            // the shader needs the matrix components in a different order,
            // and it needs the offsets in the range 0-1.
            float[] matrix = Matrix.Values;

            _shaderMatrix = Matrix3D.Create(new float[] {
                matrix[0], matrix[1], matrix[2], matrix[3],
                matrix[5], matrix[6], matrix[7], matrix[8],
                matrix[10], matrix[11], matrix[12], matrix[13],
                matrix[15], matrix[16], matrix[17], matrix[18]
            });

            _shaderOffset = new[] {
                matrix[4] / 255.0f, matrix[9] / 255.0f, matrix[14] / 255.0f, matrix[19] / 255.0f
            };
        }

    }
}

