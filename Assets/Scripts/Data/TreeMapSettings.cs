using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TreeMapSettings : UpdateableData {

    public NoiseSettings m_noiseSettings;

    public List<GameObject> m_trees = new List<GameObject>();
    public List<GameObject> m_foliage = new List<GameObject>();

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
    }

#endif
}