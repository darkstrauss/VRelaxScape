using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

    public static HeightMap GenerateHeightMap(int i_width, int i_height, NoiseSettings i_settings, Vector2 i_sampleCenter)
    {
        float[,] l_values = Noise.GenerateNoiseMap(i_width, i_height, i_settings, i_sampleCenter);

        AnimationCurve l_heightCurve_threadSafe = new AnimationCurve(i_settings.m_heightCurve.keys);

        float l_minValue = float.MaxValue;
        float l_maxValue = float.MinValue;

        for (int i = 0; i < i_width; i++)
        {
            for (int j = 0; j < i_height; j++)
            {
                l_values[i, j] *= l_heightCurve_threadSafe.Evaluate(l_values[i, j]) * i_settings.m_heightMultiplier;

                if (l_values[i,j] > l_maxValue)
                {
                    l_maxValue = l_values[i, j];
                }

                if (l_values[i,j] < l_minValue)
                {
                    l_minValue = l_values[i, j];
                }
            }
        }

        return new HeightMap(l_values, l_minValue, l_maxValue);
    }

    /// <summary>
    /// Used to generate a comparison height map for placing objects based on the terrain map and a secondary noise map.
    /// </summary>
    /// <param name="i_width">Chunk width</param>
    /// <param name="i_height">Chunk height</param>
    /// <param name="i_firstMap">Terrain Noise Data</param>
    /// <param name="i_secondMap">Other Noise Data</param>
    /// <param name="i_sampleCenter">Chunk center</param>
    /// <returns></returns>
    public static HeightMap GenerateHeightMapByComparison(int i_width, int i_height, NoiseSettings i_firstMap, NoiseSettings i_secondMap, Vector2 i_sampleCenter)
    {
        float[,] l_valuesFirstMap = Noise.GenerateNoiseMap(i_width, i_height, i_firstMap, i_sampleCenter);
        float[,] l_valuesSecondMap = Noise.GenerateNoiseMap(i_width, i_height, i_secondMap, i_sampleCenter);

        float[,] l_values = new float[i_width, i_height];

        for (int i = 0; i < i_width; i++)
        {
            for (int j = 0; j < i_height; j++)
            {
                if (l_valuesFirstMap[i, j] <= i_secondMap.m_maxTreeRange && l_valuesFirstMap[i, j] >= i_secondMap.m_minTreeRange && l_valuesSecondMap[i, j] <= i_secondMap.m_maxTreeRange && l_valuesSecondMap[i, j] >= i_secondMap.m_minTreeRange)
                {
                    l_values[i, j] = 1;
                }
                else
                {
                    l_values[i, j] = 0;
                }
            }
        }

        AnimationCurve l_heightCurve_threadSafe = new AnimationCurve(i_secondMap.m_heightCurve.keys);


        float l_minValue = float.MaxValue;
        float l_maxValue = float.MinValue;

        for (int i = 0; i < i_width; i++)
        {
            for (int j = 0; j < i_height; j++)
            {
                l_values[i, j] *= l_heightCurve_threadSafe.Evaluate(l_values[i, j]) * i_secondMap.m_heightMultiplier;

                if (l_values[i, j] > l_maxValue)
                {
                    l_maxValue = l_values[i, j];
                }

                if (l_values[i, j] < l_minValue)
                {
                    l_minValue = l_values[i, j];
                }
            }
        }

        return new HeightMap(l_values, l_minValue, l_maxValue);
    }
}


/// <summary>
/// This is used to store the data for each vertex point in HeightMap. Every point has a height value and color value.
/// </summary>
public struct HeightMap
{
    public readonly float[,] m_values;
    public readonly float m_minValue;
    public readonly float m_maxValue;

    public HeightMap(float[,] i_values, float i_minValue, float i_maxValue)
    {
        this.m_values = i_values;
        this.m_minValue = i_minValue;
        this.m_maxValue = i_maxValue;
    }
}
