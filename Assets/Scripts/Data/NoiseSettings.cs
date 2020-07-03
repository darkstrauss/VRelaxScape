using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu()]
public class NoiseSettings : UpdateableData
{
    public Noise.NormalizeMode m_normalizeMode;

    [Range(0.1f, 3.0f)]
    public float m_globalNormalizeAmplifier;

    public float m_scale = 50;

    public int m_octaves = 6;

    [Range(0, 1)]
    public float m_persistance = 0.417f;

    public float m_lacunarity = 2;

    public int m_seed;

    public Vector2 m_offset;

    public bool m_isUsingFalloffMap;
    public float m_heightMultiplier;
    public AnimationCurve m_heightCurve;

    [Range(-10, 10)]
    public float m_falloffSharpness;

    [Range(-10, 10)]
    public float m_falloffRange;

    [Range(-3f, 3f)]
    public float m_falloffMapAmplifier;

    [Range(0, 1)]
    public float m_minTreeRange;

    [Range(0, 1)]
    public float m_maxTreeRange;

    public bool m_autoAnimate = false;
    public bool m_xAxisAnimate = false;
    [Range(0, 10)]
    public float m_animationSpeed = 1;

    public float GetMinHeight
    {
        get
        {
            return m_heightMultiplier * m_heightCurve.Evaluate(0);
        }
    }

    public float GetMaxHeight
    {
        get
        {
            return m_heightMultiplier * m_heightCurve.Evaluate(1);
        }
    }

    public void ValidateValues()
    {
        m_scale = Mathf.Max(m_scale, 0.01f);
        m_octaves = Mathf.Max(m_octaves, 1);
        m_lacunarity = Mathf.Max(m_lacunarity, 1);
        m_persistance = Mathf.Clamp01(m_persistance);
        m_minTreeRange = Mathf.Clamp(m_minTreeRange, 0, m_maxTreeRange);
        m_maxTreeRange = Mathf.Clamp(m_maxTreeRange, m_minTreeRange, 1);
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        ValidateValues();
        base.OnValidate();
    }

#endif



}