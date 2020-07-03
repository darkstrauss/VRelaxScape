using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalloffGenerator {

    public static float[,] GenerateFalloffMap(int i_size, float i_sharpness, float i_range)
    {
        float[,] l_map = new float[i_size, i_size];

        for (int x = 0; x < i_size; x++)
        {
            for (int y = 0; y < i_size; y++)
            {
                float u = x / (float)i_size * 2 - 1;
                float v = y / (float)i_size * 2 - 1;

                float l_value = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v));

                l_map[x, y] = EvaluateCurve(l_value, i_sharpness, i_range);
            }
        }

        return l_map;
    }

    private static float EvaluateCurve(float i_value, float i_sharpness, float i_range)
    {
        float a = i_sharpness;
        float b = i_range;

        return Mathf.Pow(i_value, a) / (Mathf.Pow(i_value, a) + Mathf.Pow(b - b * i_value, a));

        //return i_curve.Evaluate(i_value);
    }
}
