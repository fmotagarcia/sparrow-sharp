using System;

namespace Sparrow.Utils
{
	public class NumberUtil
	{

		public readonly static float[] sinLUT = new float[2048];
		public readonly static float[] cosLUT = new float[2048];

        static NumberUtil()
        {
			for (int i = 0; i < 2048; i++)
            {
				sinLUT[i & 2047] = (float)Math.Sin(i * 0.00306796157577128245943617517898f); //0.003067 = 2PI/2048
				cosLUT[i & 2047] = (float)Math.Cos(i * 0.00306796157577128245943617517898f);
            }
        }

		public static bool Equals (float a, float b)
		{
			return Math.Abs (a - b) < 0.000001f;
		}

		public static float Clamp (float value, float min, float max)
		{
			return Math.Min (max, Math.Max (value, min));
		}
        
        public static float FastSin(float angle)
        {
			return sinLUT[(int)(angle * 325.94932345220164765467394738691f) & 2047]; 
        }

		public static float FastCos(float angle)
        {
			return cosLUT[(int)(angle * 325.94932345220164765467394738691f) & 2047]; //325.949 is 2048/2PI    
        }

	}
}
