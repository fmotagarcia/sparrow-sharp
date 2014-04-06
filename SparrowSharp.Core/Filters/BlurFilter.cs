using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using Sparrow;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace SparrowSharp.Filters
{
    /// <summary>
    /// The BlurFilter applies a gaussian blur to an object. The strength of the blur can be
    /// set for x- and y-axis separately (always relative to the stage).

    ///	A blur filter can also be set up as a drop shadow or glow filter. Use the respective
    ///	static methods to create such a filter.

    ///	For each blur direction, the number of required passes is ceil(blur).
    ///	blur = 0.5: 1 pass
    ///		blur = 1.0: 1 pass
    ///		blur = 1.5: 2 passes
    ///		blur = 2.0: 2 passes
    ///		etc.
    /// </summary>
    public class BlurFilter : FragmentFilter
    {
        private float _blurX;

        public float BlurX
        { 
            get { return _blurX; }
            set
            { 
                _blurX = value;
                UpdateMarginsAndPasses();
            }
        }

        private float _blurY;

        public float BlurY
        {
            get { return _blurY; }
            set
            { 
                _blurY = value;
                UpdateMarginsAndPasses();
            }
        }

        private bool _enableColorUniform;
        private readonly float[] _offsets = new float[4];
        private readonly float[] _weights = new float[4];
        private readonly float[] _color = new float[4];
        private BlurProgram _program;
        private BlurProgram _tintedProgram;

        /// <summary>
        /// Initializes a blur filter with the specified blur and a resolution.
        /// </summary>
        public BlurFilter(float blur = 1.0f, float resolution = 1.0f) : base(1, resolution)
        {
            _blurX = blur;
            _blurY = blur;
            UpdateMarginsAndPasses();
        }

        /// <summary>
        /// A uniform color will replace the RGB values of the input color, while the alpha value will be
        /// multiplied with the given factor. Pass false as the first parameter to deactivate the uniform color.
        /// </summary>
        public void SetUniformColor(bool enable, uint color = 0x000000, float alpha = 1.0f)
        {
            _color[0] = ColorUtil.GetR(color) / 255.0f;
            _color[1] = ColorUtil.GetG(color) / 255.0f;
            _color[2] = ColorUtil.GetB(color) / 255.0f;
            _color[3] = alpha;
            _enableColorUniform = enable;
        }

        override protected void CreatePrograms()
        {
            if (_program == null)
            {
                string programName = BlurProgram.GetProgramName(false);
                _program = (BlurProgram)SparrowSharpApp.GetProgram(programName);
                if (_program == null)
                {
                    _program = new BlurProgram(false);
                    SparrowSharpApp.RegisterProgram(programName, _program);
                }
            }

            if (_tintedProgram == null)
            {
                string programName = BlurProgram.GetProgramName(true);
                _tintedProgram = (BlurProgram)SparrowSharpApp.GetProgram(programName);

                if (_tintedProgram == null)
                {
                    _tintedProgram = new BlurProgram(true);
                    SparrowSharpApp.RegisterProgram(programName, _tintedProgram);
                }
            }

            VertexPosID = _program.APosition;
            TexCoordsID = _program.ATexCoords;
        }

        override protected void ActivateWithPass(int pass, Texture texture, Matrix mvpMatrix)
        {
            UpdateParamaters(pass, (int)texture.NativeWidth, (int)texture.NativeHeight);
            bool isColorPass = _enableColorUniform && pass == NumPasses - 1;
            BlurProgram program = isColorPass ? _tintedProgram : _program;

            GL.UseProgram(program.Name);
            Matrix4 mvp = mvpMatrix.ConvertToMatrix4();
            GL.UniformMatrix4(program.UMvpMatrix, false, ref mvp);

            GL.Uniform4(program.UOffsets, 1, _offsets);
            GL.Uniform4(program.UWeights, 1, _weights);

            if (isColorPass)
            {
                GL.Uniform4(program.UColor, 1, _color);
            }
        }

        private void UpdateParamaters(int pass, int texWidth, int texHeight)
        {
            const float MAX_SIGMA = 2.0f;

            // algorithm described here:
            // http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
            //
            // Normally, we'd have to use 9 texture lookups in the fragment shader. But by making smart
            // use of linear texture sampling, we can produce the same output with only 5 lookups.
            bool horizontal = pass < _blurX;
            float sigma;
            float pixelSize;

            if (horizontal)
            {
                sigma = Math.Min(1.0f, _blurX - pass) * MAX_SIGMA;
                pixelSize = 1.0f / texWidth;
            }
            else
            {
                sigma = Math.Min(1.0f, _blurY - (pass - (float)Math.Ceiling(_blurX))) * MAX_SIGMA;
                pixelSize = 1.0f / texHeight;
            }

            float twoSigmaSq = 2.0f * sigma * sigma;
            float multiplier = 1.0f / (float)Math.Sqrt(twoSigmaSq * Math.PI);

            // get weights on the exact pixels(sTmpWeights) and calculate sums(_weights)
            float[] sTmpWeights = new float[6];

            for (int i = 0; i < 5; ++i)
                sTmpWeights[i] = multiplier * (float)Math.Exp(-i * i / twoSigmaSq);

            _weights[0] = sTmpWeights[0];
            _weights[1] = sTmpWeights[1] + sTmpWeights[2];
            _weights[2] = sTmpWeights[3] + sTmpWeights[4];

            // normalize weights so that sum equals "1.0"

            float weightSum = _weights[0] + (2.0f * _weights[1]) + (2.0f * _weights[2]);
            float invWeightSum = 1.0f / weightSum;

            _weights[0] *= invWeightSum;
            _weights[1] *= invWeightSum;
            _weights[2] *= invWeightSum;

            // calculate intermediate offsets

            float offset1 = (pixelSize * sTmpWeights[1] + 2 * pixelSize * sTmpWeights[2]) / _weights[1];
            float offset2 = (3 * pixelSize * sTmpWeights[3] + 4 * pixelSize * sTmpWeights[4]) / _weights[2];

            // depending on pass, we move in x- or y-direction

            if (horizontal)
            {
                _offsets[0] = offset1;
                _offsets[1] = 0;
                _offsets[2] = offset2;
                _offsets[3] = 0;
            }
            else
            {
                _offsets[0] = 0;
                _offsets[1] = offset1;
                _offsets[2] = 0;
                _offsets[3] = offset2;
            }
        }

        private void UpdateMarginsAndPasses()
        {
            if (_blurX == 0 && _blurY == 0)
            {
                _blurX = 0.001f;
            }
            NumPasses = (int)Math.Ceiling(_blurX) + (int)Math.Ceiling(_blurY);
            MarginX = 4.0f + (int)Math.Ceiling(_blurX);
            MarginY = 4.0f + (int)Math.Ceiling(_blurY);
        }
    }
}

