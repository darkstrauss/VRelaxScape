using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to generate a noise map based on the Mathf function of PerlinNoise.
/// </summary>
public static class Noise {

    public enum NormalizeMode
    {
        Local,
        Global
    }

    // Constructor
	public static float[,] GenerateNoiseMap(int i_mapWidth, int i_mapHeight, NoiseSettings i_settings, Vector2 i_sampleCenter)
    {
        // We need the size of what this map chunk is going to be
        float[,] l_noiseMap = new float[i_mapWidth, i_mapHeight];

        // Define the seed so the user has the power to generate the same thing over and over.
        System.Random SEED = new System.Random(i_settings.m_seed);

        // Create an offset unique for each octave of depth
        Vector2[] l_octavesOffsets = new Vector2[i_settings.m_octaves];

        float l_maxPossibleHeight = 0;
        float l_amplitude = 1;
        float l_frequency = 1;

        for (int i = 0; i < i_settings.m_octaves; i++)
        {
            // Apply the offset given to the contructor to both axis.
            float l_offsetX = SEED.Next(-100000, 100000) + i_settings.m_offset.x + i_sampleCenter.x;
            float l_offsetY = SEED.Next(-100000, 100000) - i_settings.m_offset.y - i_sampleCenter.y;

            l_octavesOffsets[i] = new Vector2(l_offsetX, l_offsetY);

            l_maxPossibleHeight += l_amplitude;
            l_amplitude *= i_settings.m_persistance;
        }

        // We never want the scale to be 0.
        if (i_settings.m_scale <= 0)
        {
            i_settings.m_scale = 0.0001f;
        }

        // Used to keep track of highest and lowest point in the generated noise map.
        float l_maxLocalNoiseHeight = float.MinValue;
        float l_minLocalNoiseHeight = float.MaxValue;

        // Used for the scaling. Allows the scaling to happen from the middle of the terrain instead of the corner.
        float l_halfWidth = i_mapWidth / 2f;
        float l_halfHeight = i_mapHeight / 2f;

        // Itterate for every single point in the chunk.
        for (int y = 0; y < i_mapHeight; y++)
        {
            for (int x = 0; x < i_mapWidth; x++)
            {
                // Used for sampling X and Y values.
                l_amplitude = 1;
                l_frequency = 1;
                float l_noiseHeight = 0;

                for (int i = 0; i < i_settings.m_octaves; i++)
                {
                    float sampleX = (x - l_halfWidth + l_octavesOffsets[i].x) / i_settings.m_scale * l_frequency;
                    float sampleY = (y - l_halfHeight + l_octavesOffsets[i].y) / i_settings.m_scale * l_frequency;

                    // Get a range from -1 to 1
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    l_noiseHeight += perlinValue * l_amplitude;

                    l_amplitude *= i_settings.m_persistance;
                    l_frequency *= i_settings.m_lacunarity;
                }

                if (l_noiseHeight > l_maxLocalNoiseHeight)
                {
                    l_maxLocalNoiseHeight = l_noiseHeight;
                }

                if (l_noiseHeight < l_minLocalNoiseHeight)
                {
                    l_minLocalNoiseHeight = l_noiseHeight;
                }

                // Set the noise height for this point.
                l_noiseMap[x, y] = l_noiseHeight;

                if (i_settings.m_normalizeMode == NormalizeMode.Global)
                {
                    l_noiseMap[x, y] = (l_noiseMap[x, y] + 1) * i_settings.m_globalNormalizeAmplifier;
                }
            }
        }

        if (i_settings.m_normalizeMode == NormalizeMode.Local)
        {
            // Normalize every point in the hieght map so it is in the 0 - 1 range.
            for (int y = 0; y < i_mapHeight; y++)
            {
                for (int x = 0; x < i_mapWidth; x++)
                {
                    // LHS is 0, RHS is 1
                    l_noiseMap[x, y] = Mathf.InverseLerp(l_minLocalNoiseHeight, l_maxLocalNoiseHeight, l_noiseMap[x, y]);
                }
            }
        }

        return l_noiseMap;
    }
}

