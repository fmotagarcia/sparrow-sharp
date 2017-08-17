
using System;
using Sparrow.Textures;
using Sparrow.Utils;
using Sparrow.Rendering;
using System.Text;
using OpenGL;

namespace Sparrow.Filters
{
    /** The BlurFilter applies a Gaussian blur to an object. The strength of the blur can be
     *  set for x- and y-axis separately. */
    public class BlurFilter : FragmentFilter
    {
        private float _blurX;
        private float _blurY;

        /// <summary>
        /// Initializes a blur filter with the specified blur and a resolution.
        /// </summary>
        public BlurFilter(float blurX = 1.0f, float blurY = 1.0f, float resolution = 1.0f)
        {
            _blurX = blurX;
            _blurY = blurY;
            Resolution = resolution;
        }

        public override Texture Process(Painter painter, IFilterHelper helper,
                                        Texture input0 = null, Texture input1 = null,
                                        Texture input2 = null, Texture input3 = null)
        {
            BlurEffect effect = Effect as BlurEffect;

            if (_blurX == 0 && _blurY == 0)
            {
                effect.Strength = 0;
                return base.Process(painter, helper, input0);
            }

            float blurX = Math.Abs(_blurX);
            float blurY = Math.Abs(_blurY);
            Texture outTexture = input0;
            Texture inTexture;

            effect.Direction = BlurEffect.BlurDirection.HORIZONTAL;

            while (blurX > 0)
            {
                effect.Strength = (float)Math.Min(1.0, blurX);

                blurX -= effect.Strength;
                inTexture = outTexture;
                outTexture = base.Process(painter, helper, inTexture);

                if (inTexture != input0) helper.PutTexture(inTexture);
            }

            effect.Direction = BlurEffect.BlurDirection.VERTICAL;

            while (blurY > 0)
            {
                effect.Strength = (float)Math.Min(1.0, blurY);

                blurY -= effect.Strength;
                inTexture = outTexture;
                outTexture = base.Process(painter, helper, inTexture);

                if (inTexture != input0) helper.PutTexture(inTexture);
            }

            return outTexture;
        }

        /** @private */
        protected override FilterEffect CreateEffect()
        {
            return new BlurEffect();
        }

        /** @private */
        public override float Resolution
        {
            set
            {
                base.Resolution = value;
                UpdatePadding();
            }
        }

        public override int NumPasses
        {
            get {
                int blurVal = (int)Math.Ceiling(_blurX) + (int)Math.Ceiling(_blurY);
                int ret = blurVal != 0 ? blurVal : 1;
                return ret;
            }
        }

        private void UpdatePadding()
        {
            float paddingX = (_blurX != 0f ? (float)Math.Ceiling(Math.Abs(_blurX)) + 3f : 1f) / Resolution;
            float paddingY = (_blurY != 0f ? (float)Math.Ceiling(Math.Abs(_blurY)) + 3f : 1f) / Resolution;

            Padding.SetTo(paddingX, paddingX, paddingY, paddingY);
        }

        public float BlurX
        { 
            get { return _blurX; }
            set
            { 
                _blurX = value;
                UpdatePadding();
            }
        }

        
        public float BlurY
        {
            get { return _blurY; }
            set
            { 
                _blurY = value;
                UpdatePadding();
            }
        }

    }


    internal class BlurEffect : FilterEffect
    {

        public enum BlurDirection
        {
            HORIZONTAL, VERTICAL
        }

        private static readonly float MAX_SIGMA = 2.0f;

        private float _strength;
        private BlurDirection _direction;

        private float[] _offsets = new float[] { 0, 0, 0, 0 };
        private float[] _weights = new float[] { 0, 0, 0, 0 };

        // helpers
        private float[] sTmpWeights = new float[5];

        /** Creates a new BlurEffect.
         *
         *  @param direction     horizontal or vertical
         *  @param strength      range 0-1
         */
        public BlurEffect(BlurDirection direction = BlurDirection.HORIZONTAL, float strength = 1)
        {
            Strength  = strength;
            Direction = direction;
        }

        protected override Program CreateProgram()
        {
            if (_strength == 0) return base.CreateProgram();

            StringBuilder source = new StringBuilder("");
            // vertex shader
            AddShaderInitCode(source);
            // attributes
            source.AppendLine("attribute vec4 aPosition;");
            source.AppendLine("attribute lowp vec2 aTexCoords;");
            // uniforms
            source.AppendLine("uniform mat4 uMvpMatrix;");
            source.AppendLine("uniform lowp vec4 uOffsets;");
            // varying
            source.AppendLine("varying lowp vec2 v0;");
            source.AppendLine("varying lowp vec2 v1;");
            source.AppendLine("varying lowp vec2 v2;");
            source.AppendLine("varying lowp vec2 v3;");
            source.AppendLine("varying lowp vec2 v4;");

            // main
            source.AppendLine("void main() {");
            source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");     // 4x4 matrix transform to output space
            source.AppendLine("  v0 = aTexCoords;");                          // pos:  0 |
            source.AppendLine("  v1 = aTexCoords - uOffsets.zw;");            // pos: -2 |
            source.AppendLine("  v2 = aTexCoords - uOffsets.xy;");            // pos: -1 | --> kernel positions
            source.AppendLine("  v3 = aTexCoords + uOffsets.xy;");            // pos: +1 |     (only 1st two parts are relevant)
            source.AppendLine("  v4 = aTexCoords + uOffsets.zw;");            // pos: +2 |
            source.AppendLine("}");
            string vertexShader = source.ToString();

            source = new StringBuilder("");
            //fragment shader
            AddShaderInitCode(source);
            // variables
            source.AppendLine("varying lowp vec2 v0;");
            source.AppendLine("varying lowp vec2 v1;");
            source.AppendLine("varying lowp vec2 v2;");
            source.AppendLine("varying lowp vec2 v3;");
            source.AppendLine("varying lowp vec2 v4;");
            
            source.AppendLine("uniform sampler2D uTexture;");
            source.AppendLine("uniform lowp vec4 uWeights;");

            // main
            source.AppendLine("void main() {");

            source.AppendLine("  lowp vec4 ft0;");
            source.AppendLine("  lowp vec4 ft1;");
            source.AppendLine("  lowp vec4 ft2;");
            source.AppendLine("  lowp vec4 ft3;");
            source.AppendLine("  lowp vec4 ft4;");
            source.AppendLine("  lowp vec4 ft5;");

            source.AppendLine("  ft0 = texture2D(uTexture,v0);");  // read center pixel
            source.AppendLine("  ft5 = ft0 * uWeights.xxxx;");     // multiply with center weight

            source.AppendLine("  ft1 = texture2D(uTexture,v1);");  // read pixel -2
            source.AppendLine("  ft1 = ft1 * uWeights.zzzz;");     // multiply with weight
            source.AppendLine("  ft5 = ft5 + ft1;");               // add to output color

            source.AppendLine("  ft2 = texture2D(uTexture,v2);");  // read pixel -1
            source.AppendLine("  ft2 = ft2 * uWeights.yyyy;");     // multiply with weight
            source.AppendLine("  ft5 = ft5 + ft2;");               // add to output color

            source.AppendLine("  ft3 = texture2D(uTexture,v3);");  // read pixel +1
            source.AppendLine("  ft3 = ft3 * uWeights.yyyy;");     // multiply with weight
            source.AppendLine("  ft5 = ft5 + ft3;");               // add to output color

            source.AppendLine("  ft4 = texture2D(uTexture,v4);");  // read pixel +2
            source.AppendLine("  ft4 = ft4 * uWeights.zzzz;");     // multiply with weight

            source.AppendLine("  gl_FragColor = ft5 + ft4;");      // add to output color

            source.AppendLine("}");
            string fragmentShader = source.ToString();

            return new Program(vertexShader, fragmentShader);
        }

        protected override void BeforeDraw()
        {
            base.BeforeDraw();

            if (_strength != 0)
            {
                UpdateParameters();

                Gl.UseProgram(Program.Name);

                int uOffsets = Program.Uniforms["uOffsets"];
                Gl.Uniform4(uOffsets, 1, _offsets);
                int uWeights = Program.Uniforms["uWeights"];
                Gl.Uniform4(uWeights, 1, _weights);
            }
        }

        protected override uint ProgramVariantName
        {
            get
            {
                return base.ProgramVariantName | (_strength != 0f ? 1u << 4 : 0);
            }
        }

        private void UpdateParameters()
        {
            // algorithm described here:
            // http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
            //
            // To run in constrained mode, we can only make 5 texture look-ups in the fragment
            // shader. By making use of linear texture sampling, we can produce similar output
            // to what would be 9 look-ups.

            float sigma;
            float pixelSize;

            if (_direction == BlurDirection.HORIZONTAL)
            {
                sigma = _strength * MAX_SIGMA;
                pixelSize = 1.0f / Texture.Root.Width;
            }
            else
            {
                sigma = _strength * MAX_SIGMA;
                pixelSize = 1.0f / Texture.Root.Height;
            }

            float twoSigmaSq = 2 * sigma * sigma;
            float multiplier = 1.0f / (float)Math.Sqrt(twoSigmaSq* Math.PI);

            // get weights on the exact pixels (sTmpWeights) and calculate sums (_weights)
            for (int i = 0; i < 5; ++i)
            {
                sTmpWeights[i] = multiplier * (float)Math.Exp(-i * i / twoSigmaSq);
            }
            
            _weights[0] = sTmpWeights[0];
            _weights[1] = sTmpWeights[1] + sTmpWeights[2];
            _weights[2] = sTmpWeights[3] + sTmpWeights[4];

            // normalize weights so that sum equals "1.0"
            float weightSum = _weights[0] + 2* _weights[1] + 2* _weights[2];
            float invWeightSum = 1.0f / weightSum;

            _weights[0] *= invWeightSum;
            _weights[1] *= invWeightSum;
            _weights[2] *= invWeightSum;

            // calculate intermediate offsets
            float offset1 = (  pixelSize* sTmpWeights[1] + 2* pixelSize * sTmpWeights[2]) / _weights[1];
            float offset2 = (3* pixelSize * sTmpWeights[3] + 4* pixelSize * sTmpWeights[4]) / _weights[2];

            // depending on pass, we move in x- or y-direction
            if (_direction == BlurDirection.HORIZONTAL)
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

        public BlurDirection Direction { 
            get { return _direction; }
            set { _direction = value; }
        }

        public float Strength {
            get { return _strength; }
            set { _strength = MathUtil.Clamp(value, 0, 1); }
        }

    }

}

