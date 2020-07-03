using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainTextureGenerator
{

    public static Texture2D TextureFromColorMap(Color[] i_colorMap, int i_width, int i_height)
    {
        Texture2D l_texture = new Texture2D(i_width, i_height);
        l_texture.filterMode = FilterMode.Point;
        l_texture.wrapMode = TextureWrapMode.Clamp;
        l_texture.SetPixels(i_colorMap);
        l_texture.Apply();

        return l_texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap i_heightMap)
    {
        int l_width = i_heightMap.m_values.GetLength(0);
        int l_height = i_heightMap.m_values.GetLength(1);

        Color[] l_colorMap = new Color[l_width * l_height];

        for (int y = 0; y < l_height; y++)
        {
            for (int x = 0; x < l_width; x++)
            {
                l_colorMap[y * l_width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(i_heightMap.m_minValue, i_heightMap.m_maxValue, i_heightMap.m_values[x, y]));
            }
        }

        return TextureFromColorMap(l_colorMap, l_width, l_height);
    }

}
