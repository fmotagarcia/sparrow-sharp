using System;

namespace SparrowSharp.Filters
{
    /// <summary>
    /// A color matrix class containing an array of 20 floats arranged as a 4x5 matrix.
    /// </summary>
    public class ColorMatrix
    {
        /// <summary>
        /// Returns a point to the internal color matrix array.
        /// </summary>
        public float[] Values { get { return _m; } }

        /// <summary>
        /// Returns the count of values (always 20).
        /// </summary>
        public readonly int NumValues = 20;
        protected float[] _m = new float[20];
        private readonly float[] matrix4x5Identity =
            {
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0
        };
        private const float LUMA_R = 0.299f;
        private const float LUMA_G = 0.587f;
        private const float LUMA_B = 0.114f;

        /// <summary>
        /// Initializes a color matrix with an array of 20 floats.
        /// If no value is specified it initializes with an identity matrix.
        /// </summary>
        public ColorMatrix(float[] values = null)
        {
            if (values != null)
            {
                Array.Copy(values, _m, 20);
            }
            else
            {
                Array.Copy(matrix4x5Identity, _m, 20);
            }
        }

        /// <summary>
        /// Inverts the colors.
        /// </summary>
        public void Invert()
        {
            float[] mtx =
            {
                -1, 0,  0,  0, 255,
                0, -1,  0,  0, 255,
                0,  0, -1,  0, 255,
                0,  0,  0,  1,   0
            };
            ConcatMatrix(this, mtx);
        }

        /// <summary>
        /// Changes the saturation. Typical values are in the range [-1, 1].
        /// Values above zero will raise, values below zero will reduce the saturation.
        /// '-1' will produce a grayscale image.
        /// </summary>
        public void AdjustSaturation(float saturation)
        {
            saturation += 1.0f;

            float invSat = 1.0f - saturation;
            float invLumR = invSat * LUMA_R;
            float invLumG = invSat * LUMA_G;
            float invLumB = invSat * LUMA_B;

            float[] mtx =
            {
                (invLumR + saturation),  invLumG,               invLumB,               0, 0,
                invLumR,                (invLumG + saturation), invLumB,               0, 0,
                invLumR,                 invLumG,              (invLumB + saturation), 0, 0,
                0,                       0,                     0,                     1, 0
            };

            ConcatMatrix(this, mtx);
        }

        /// <summary>
        /// Changes the contrast. Typical values are in the range [-1, 1].
        /// Values above zero will raise, values below zero will reduce the contrast.
        /// </summary>
        public void AdjustContrast(float contrast)
        {
            float s = contrast + 1.0f;
            float o = 128 * (1.0f - s);

            float[] mtx =
            {
                s, 0, 0, 0, o,
                0, s, 0, 0, o,
                0, 0, s, 0, o,
                0, 0, 0, s, 0
            };

            ConcatMatrix(this, mtx);
        }

        /// <summary>
        /// Changes the brightness. Typical values are in the range [-1, 1].
        /// Values above zero will make the image brighter, values below zero will make it darker.
        /// </summary>
        public void AdjustBrightness(float brightness)
        { 
            brightness *= 255;

            float[] mtx =
            {
                1, 0, 0, 0, brightness,
                0, 1, 0, 0, brightness,
                0, 0, 1, 0, brightness,
                0, 0, 0, 1, 0
            };

            ConcatMatrix(this, mtx);
        }

        /// <summary>
        /// Changes the hue. Typical values are in the range [-1, 1].
        /// </summary>
        public void AdjustHue(float hue)
        {
            hue *= (float)Math.PI;

            float cos = (float)Math.Cos(hue);
            float sin = (float)Math.Sin(hue);

            float[] mtx =
            {
                // r1
                ((LUMA_R + (cos * (1.0f - LUMA_R))) + (sin * -(LUMA_R))),
                ((LUMA_G + (cos * -(LUMA_G))) + (sin * -(LUMA_G))),
                ((LUMA_B + (cos * -(LUMA_B))) + (sin * (1.0f - LUMA_B))),
                0.0f,
                0.0f,

                // r2
                ((LUMA_R + (cos * -(LUMA_R))) + (sin * 0.143f)),
                ((LUMA_G + (cos * (1.0f - LUMA_G))) + (sin * 0.14f)),
                ((LUMA_B + (cos * -(LUMA_B))) + (sin * -0.283f)),
                0.0f,
                0.0f,

                // r3
                ((LUMA_R + (cos * -(LUMA_R))) + (sin * -((1.0f - LUMA_R)))),
                ((LUMA_G + (cos * -(LUMA_G))) + (sin * LUMA_G)),
                ((LUMA_B + (cos * (1.0f - LUMA_B))) + (sin * LUMA_B)),
                0.0f,
                0.0f,

                // r4
                0.0f,
                0.0f,
                0.0f,
                1.0f,
                0.0f
            };

            ConcatMatrix(this, mtx);
        }

        /// <summary>
        /// Changes the color matrix into an identity matrix.
        /// </summary>
        public void Identity()
        {
            Array.Copy(matrix4x5Identity, _m, 20);
        }

        /// <summary>
        /// Concatenates the receiving color matrix with another one.
        /// </summary>
        public void ConcatColorMatrix(ColorMatrix colorMatrix)
        {
            ConcatMatrix(this, colorMatrix._m);
        }

        private void ConcatMatrix(ColorMatrix target, float[] source)
        {
            int i = 0;
            float[] temp = new float[20];
            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 5; ++x)
                {
                    temp[i + x] = source[i] * target._m[x] +
                    source[i + 1] * target._m[x + 5] +
                    source[i + 2] * target._m[x + 10] +
                    source[i + 3] * target._m[x + 15] + (x == 4 ? source[i + 4] : 0);
                }
                i += 5;
            }
            target._m = temp;
        }
    }
}

