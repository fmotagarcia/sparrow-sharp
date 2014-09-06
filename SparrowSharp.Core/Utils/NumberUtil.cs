using System;

namespace Sparrow.Utils
{
    public static class NumberUtil
    {
        /// <summary>
        /// Lookup table for fast sine calculations
        /// </summary>
        public readonly static float[] SinLUT = new float[2048];
        /// <summary>
        /// Lookup table for fast cosine calculations
        /// </summary>
        public readonly static float[] CosLUT = new float[2048];

        static NumberUtil()
        {
            for (int i = 0; i < 2048; i++)
            {
                SinLUT[i & 2047] = (float)Math.Sin(i * 0.00306796157577128245943617517898f); //0.003067 = 2PI/2048
                CosLUT[i & 2047] = (float)Math.Cos(i * 0.00306796157577128245943617517898f);
            }
        }

        /// <summary>
        /// checks if two numbers are equal with a small margin of error
        /// </summary>
        public static bool Equals(float a, float b)
        {
            return Math.Abs(a - b) < 0.000001f;
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(max, Math.Max(value, min));
        }

        /// <summary>
        /// Fast sinus calculation with a look up table.
        /// Note that it is less accurate than Math.Sin; the largest error is around 0.3 degrees
        /// </summary>
        public static float FastSin(float angle)
        {
            return SinLUT[(int)(angle * 325.94932345220164765467394738691f + 0.5f) & 2047]; 
        }

        /// <summary>
        /// Fast cosinus calculation with a look up table.
        /// Note that it is less accurate than Math.Cos; the largest error is around 0.3 degrees;
        /// </summary>
        public static float FastCos(float angle)
        {
            return CosLUT[(int)(angle * 325.94932345220164765467394738691f + 0.5f) & 2047]; //325.949 is 2048/2PI    
        }

        public static int NextPowerOfTwo(int number)
        {    
            int result = 1; 
            while (result < number)
            {
                result *= 2;
            }
            return result;    
        }

        public static int NextPowerOfTwo(float number)
        {    
            int result = 1; 
            while (result < number)
            {
                result *= 2;
            }
            return result;    
        }
    }
}
