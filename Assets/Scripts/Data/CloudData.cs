using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CloudData : UpdateableData {

    public bool m_autoAnimate = true;

    [Range(50, 500)]
    public float m_visibleDistanceThreshold;

    [Range(0, 35)]
    public int m_maxPossibleVisibleClouds = 15;

    // When you use the clouds
    [Range(0.1f, 2)]
    public float m_disappearRate = 10;

    [Range(30, 100)]
    public float m_cloudHeightInWorld;
    [Range(0.01f, 2.0f)]
    public float m_baseMovementSpeed;
    public List<GameObject> m_clouds = new List<GameObject>();
    public List<GameObject> m_cloudBurstParticles = new List<GameObject>();
}
