using UnityEngine;

namespace CoreToolkit.Runtime.Extensions
{
    public static class AnimationCurveExtensions
    {
        /// <summary>
        /// Makes the curve linear
        /// </summary>
        /// <param name="curve"></param>
        public static void MakeLinear(this AnimationCurve curve)
        {
            for (int i = 0; i < curve.keys.Length; ++i)
            {
                float intangent = 0;
                float outtangent = 0;
                bool intangent_set = false;
                bool outtangent_set = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                Keyframe key = curve[i];

                if (i == 0)
                {
                    intangent = 0; intangent_set = true;
                }

                if (i == curve.keys.Length - 1)
                {
                    outtangent = 0; outtangent_set = true;
                }

                if (!intangent_set)
                {
                    point1.x = curve.keys[i - 1].time;
                    point1.y = curve.keys[i - 1].value;
                    point2.x = curve.keys[i].time;
                    point2.y = curve.keys[i].value;

                    deltapoint = point2 - point1;

                    intangent = deltapoint.y / deltapoint.x;
                }
                if (!outtangent_set)
                {
                    point1.x = curve.keys[i].time;
                    point1.y = curve.keys[i].value;
                    point2.x = curve.keys[i + 1].time;
                    point2.y = curve.keys[i + 1].value;

                    deltapoint = point2 - point1;

                    outtangent = deltapoint.y / deltapoint.x;
                }

                key.inTangent = intangent;
                key.outTangent = outtangent;
                curve.MoveKey(i, key);
            }
        }

        /// <summary>
        /// Returns the max value in the the curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static float GetMaxValue(this AnimationCurve curve)
        {
            float value = 0;

            foreach (var key in curve.keys)
            {
                if (key.value > value)
                {
                    value = key.value;
                }
            }

            return value;
        }

        /// <summary>
        /// Returns the max time in the the curve
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static float GetMaxTime(this AnimationCurve curve)
        {
            float value = 0;

            foreach (var key in curve.keys)
            {
                if (key.time > value)
                {
                    value = key.time;
                }
            }

            return value;
        }
    }
}
