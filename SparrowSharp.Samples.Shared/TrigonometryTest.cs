using System;
using System.Diagnostics;
using Sparrow.Utils;

namespace Sparrow.Samples
{
    public class TrigonometryTest
    {
        public string TestSinCosSpeed()
        {
            const int NUMTRIES = 1000000;
            float angle = 0.0f;

            float ret0 = 0;
            Stopwatch watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < NUMTRIES; i++)
            {
                angle += 0.01f;
                ret0 = MathUtil.FastSin(angle);
            }
            var res0 = watch.ElapsedMilliseconds;

            float ret1 = 0;
            watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < NUMTRIES; i++)
            {
                angle += 0.01f;
                ret1 = MathUtil.SinLUT[(int)(angle * 159.154943092f + 0.5f) & 1000];
            }
            var res1 = watch.ElapsedMilliseconds;

            float ret2 = 0.0f;
            angle = 0;
            watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < NUMTRIES; i++)
            {
                angle += 0.01f;
                ret2 = (float)Math.Sin(angle);
            }
            var res2 = watch.ElapsedMilliseconds;

            float ret3 = 0.0f;
            angle = 0;
            watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < NUMTRIES; i++)
            {
                angle += 0.01f;
                //always wrap input angle to -PI..PI
                if (angle < -3.14159265f)
                    angle += 6.28318531f;
                else if (angle > 3.14159265f)
                    angle -= 6.28318531f;
                //compute sine
                if (angle < 0.0f)
                    ret3 = 1.27323954f * angle + .405284735f * angle * angle;
                else
                    ret3 = 1.27323954f * angle - 0.405284735f * angle * angle;

            }
            var res3 = watch.ElapsedMilliseconds;

            return "LUT func call: " + res0 + " LUT inline: " + res1 + " Math.Sin: " + res2 + " fastcalc: " + res3;
        }

        public string TestLUTAccuracy()
        {
            float angle = -1000;
            int numIter = (int)Math.Abs(angle) * 200;
            float angleDiff = Math.Abs(angle) * 2 / numIter;
            double biggestError = 0;
            for (int i = 0; i < numIter; i++)
            {
                angle += angleDiff;
                var sinDiff = Math.Abs(Math.Abs(Math.Sin(angle)) - Math.Abs(MathUtil.FastSin(angle))); 
                var cosDiff = Math.Abs(Math.Abs(Math.Cos(angle)) - Math.Abs(MathUtil.FastCos(angle)));
                if (sinDiff > biggestError)
                    biggestError = sinDiff;
                if (cosDiff > biggestError)
                    biggestError = sinDiff;
            }
            return "largest error: " + biggestError; // around 0.0045
        }
    }
}

