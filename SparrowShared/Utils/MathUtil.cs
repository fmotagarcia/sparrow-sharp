using Sparrow.Geom;
using System;

namespace Sparrow.Utils
{
    public static class MathUtil
    {
        /// <summary>
        /// Lookup table for fast sine calculations
        /// </summary>
        public readonly static float[] SinLUT = new float[2048];
        /// <summary>
        /// Lookup table for fast cosine calculations
        /// </summary>
        public readonly static float[] CosLUT = new float[2048];

        public readonly static float PIQUARTER = (float)(Math.PI / 4.0f);
        public readonly static float PIHALF = (float)(Math.PI / 2.0f);

        static MathUtil()
        {
            for (int i = 0; i < 2048; i++)
            {
                SinLUT[i & 2047] = (float)Math.Sin(i * 0.00306796157577128245943617517898f); //0.003067 = 2PI/2048
                CosLUT[i & 2047] = (float)Math.Cos(i * 0.00306796157577128245943617517898f);
            }
        }

        /** Calculates the intersection point between the xy-plane and an infinite line
        *  that is defined by two 3D points in the same coordinate system. */
        public static Point IntersectLineWithXYPlane(float[] pointA, float[] pointB)
        {
            Point outP = Point.Create();

            float vectorX = pointB[0] - pointA[0];
            float vectorY = pointB[1] - pointA[1];
            float vectorZ = pointB[2] - pointA[2];
            float lambda = -pointA[2] / vectorZ;

            outP.X = pointA[0] + lambda * vectorX;
            outP.Y = pointA[0] + lambda * vectorY;

            return outP;
        }

        /// <summary>
        /// checks if two numbers are equal with a small margin of error
        /// </summary>
        public static bool Equals(float a, float b, float E = 0.000005f)
        {
            return Math.Abs(a - b) < E;
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

        /// <summary>
        /// Calculates if the point <code>p</code> is inside the triangle <code>a-b-c</code>. 
        /// </summary>
        public static bool IsPointInTriangle(Point p, Point a, Point b, Point c)
        {
            // This algorithm is described well in this article:
            // http://www.blackpawn.com/texts/pointinpoly/default.html
            float v0x = c.X - a.X;
            float v0y = c.Y - a.Y;
            float v1x = b.X - a.X;
            float v1y = b.Y - a.Y;
            float v2x = p.X - a.X;
            float v2y = p.Y - a.Y;

            float dot00 = v0x* v0x + v0y* v0y;
            float dot01 = v0x* v1x + v0y* v1y;
            float dot02 = v0x* v2x + v0y* v2y;
            float dot11 = v1x* v1x + v1y* v1y;
            float dot12 = v1x* v2x + v1y* v2y;

            float invDen = 1.0f / (dot00* dot11 - dot01* dot01);
            float u = (dot11* dot02 - dot01* dot12) * invDen;
            float v = (dot00* dot12 - dot01* dot02) * invDen;

            return (u >= 0) && (v >= 0) && (u + v< 1);
        }

        public static float Deg2rad(float deg)
        {
            return deg / 180.0f * (float)Math.PI;   
        }

        /// <summary>
        /// Divides the value of the x, y, and z properties of the current Vector3D object 
        /// by the value of its w property.
        ///
        /// If the current Vector3D object is the result of multiplying a Vector3D object 
        /// by a projection Matrix3D object, the w property can hold the transform value.
        /// The ProjectVector3D() method then can complete the projection by dividing the
        /// elements by the w property.
        /// </summary>
        public static float[] ProjectVector3D(ref float[] vector)
        {
            vector[0] = vector[0] / vector[3];
            vector[1] = vector[1] / vector[3];
            vector[2] = vector[2] / vector[3];
            return vector;
        }

        /** Indicates if two float values are equal, give or take <code>epsilon</code>. */
        public static bool IsEquivalent(float a, float b, float epsilon = 0.0001f)
        {
            return (a - epsilon<b) && (a + epsilon > b);
        }
}
}
