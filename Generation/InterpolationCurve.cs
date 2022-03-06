using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JStuff.Generation
{
    [System.Serializable]
    public class InterpolationCurve
    {
        public AnimationCurve curve;
        public float factor = 1;
        public int numOfCachedValues = 100;

        float[] cachedValues;
        bool preEvaluated = false;

        public void PreEvaluate()
        {
            cachedValues = new float[numOfCachedValues];

            for (int i = 0; i < numOfCachedValues; i++)
            {
                cachedValues[i] = (curve.Evaluate((float)i / numOfCachedValues) * factor).Clamp(0,1);
            }

            preEvaluated = true;
        }

        public float Evaluate(float t)
        {
            if (preEvaluated)
            {
                return cachedValues[Mathf.FloorToInt(t.Clamp(0, 1) * (numOfCachedValues - 1))];
            } else
            {
                float retval = curve.Evaluate(t) * factor;

                if (retval < 0) return 0;
                if (retval > 1) return 1;
                return retval;
            }
        }
    }
}