using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Easings
{
    public static float Linear(float t)
    {
        return t;
    }

    public static float EaseInQuad(float t)
    {
        return t * t;
    }

    public static float EaseOutQuad(float t)
    {
        return t * (2 - t);
    }

    public static float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    public static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    public static float EaseOutCubic(float t)
    {
        return (--t) * t * t + 1;
    }

    public static float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }

    public static float EaseInSine(float t)
    {
        return 1 - Mathf.Cos(t * Mathf.PI / 2);
    }

    public static float EaseOutSine(float t)
    {
        return Mathf.Sin(t * Mathf.PI / 2);
    }

    public static float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }
    
    public static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
    
    public static float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        return t * t * ((c1 + 1) * t - c1);
    }
    
    public static float EaseInOutBack(float t)
    {
        float c1 = 1.70158f;
        float c2 = c1 * 1.525f;

        return t < 0.5f
            ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
            : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
    }
    
    public static float EaseOutElastic(float t)
    {
        float c4 = (2 * Mathf.PI) / 3;

        return t == 0 ? 0 
            : t >= 1 ? 1 
            : Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.075f) * c4) + 1;
    }
    
    public static float EaseInElastic(float t)
    {
        float c4 = (2 * Mathf.PI) / 3;

        return t == 0 ? 0 
            : t >= 1 ? 1 
            : -Mathf.Pow(2, 10 * (t - 1)) * Mathf.Sin((t - 1.075f) * c4);
    }
    
    public static float EaseOutQuint(float t)
    {
        return 1f - Mathf.Pow(1f - t, 5f);
    }
}
