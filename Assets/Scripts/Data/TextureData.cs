using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdateableData {

    public Flare m_directionalLightFlare;
    [Range(1, 3)]
    public float m_colorAmplify;
    public Layer[] layers;

    float savedMinHeight;
    float savedMaxHeight;

	public void ApplyToMaterial(Material i_material)
    {
        i_material.SetInt("_layerCount", layers.Length);
        i_material.SetFloat("_ColorAmplify", m_colorAmplify);

        i_material.SetColor("_Water", layers[0].color);
        i_material.SetFloat("_baseStartHeights", layers[0].startHeight);
        i_material.SetFloat("_baseBlends", layers[0].blendStrength);

        i_material.SetColor("_Coast", layers[1].color);
        i_material.SetFloat("_CoastStartHeight", layers[1].startHeight);
        i_material.SetFloat("_CoastBlendStrength", layers[1].blendStrength);

        i_material.SetColor("_Grass", layers[2].color);
        i_material.SetFloat("_GrassStartHeight", layers[2].startHeight);
        i_material.SetFloat("_GrassBlendStrength", layers[2].blendStrength);

        i_material.SetColor("_Grass2", layers[3].color);
        i_material.SetFloat("_Grass2StartHeight", layers[3].startHeight);
        i_material.SetFloat("_Grass2BlendStrength", layers[3].blendStrength);

        i_material.SetColor("_Rock", layers[4].color);
        i_material.SetFloat("_RockStartHeight", layers[4].startHeight);
        i_material.SetFloat("_RockBlendStrength", layers[4].blendStrength);

        i_material.SetColor("_Snow", layers[5].color);
        i_material.SetFloat("_SnowStartHeight", layers[5].startHeight);
        i_material.SetFloat("_SnowBlendStrength", layers[5].blendStrength);

        UpdatedMeshHeight(i_material, savedMinHeight, savedMaxHeight);
    }

    public void UpdatedMeshHeight(Material i_material, float i_minHeight, float i_maxHeight)
    {
        savedMinHeight = i_minHeight;
        savedMaxHeight = i_maxHeight;

        i_material.SetFloat("_minHeight", i_minHeight);
        i_material.SetFloat("_maxHeight", i_maxHeight);
    }

    [System.Serializable]
    public class Layer
    {
        public string layerName;
        public Color color;
        [Range(0,1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;

    }
}
