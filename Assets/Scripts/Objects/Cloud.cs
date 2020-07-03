using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Cloud : InteractionSubscriber {

    [SerializeField]
    private GameObject m_particle;
    [SerializeField]
    private bool m_isTargeted = false;
    private float m_disappearRate = 10;

    private CloudData m_cloudData;
    public CloudData GetSetCloudData
    {
        get
        {
            return m_cloudData;
        }
        set
        {
            m_cloudData = value;
        }
    }

    private float m_visibleDistanceThreshold = 150;
    public float GetSetVisibleDistanceThreshold
    {
        get
        {
            return m_visibleDistanceThreshold;
        }
        set
        {
            m_visibleDistanceThreshold = value;
        }
    }

    private float m_movementSpeed = 1;
    public float GetSetMovementSpeed
    {
        get
        {
            return m_movementSpeed;
        }
        set
        {
            m_movementSpeed = Mathf.Clamp(value, 0.01f, 2.0f);
        }
    }

    private float m_height;
    public float GetSetHeight
    {
        get
        {
            return m_height;
        }
        set
        {
            m_height = value;
        }
    }

    [SerializeField]
    private bool m_autoAnimate = false;
    public bool GetSetAutoAnimate
    {
        get
        {
            return m_autoAnimate;
        }
        set
        {
            m_autoAnimate = value;
        }
    }

    private GameObject m_playerGameObject;
    public GameObject GetSetPlayerGameObject
    {
        get
        {
            return m_playerGameObject;
        }
        set
        {
            m_playerGameObject = value;
        }
    }

    public void LoadCloudBehaviour(CloudData i_cloudData, GameObject i_viewer, Transform i_parent)
    {
        m_cloudData = i_cloudData;
        transform.parent = i_parent;
        GetSetAutoAnimate = this.m_cloudData.m_autoAnimate;
        GetSetHeight = Random.Range(m_cloudData.m_cloudHeightInWorld - m_cloudData.m_cloudHeightInWorld / 10, m_cloudData.m_cloudHeightInWorld + m_cloudData.m_cloudHeightInWorld / 10);
        GetSetMovementSpeed = Random.Range(0, 2);
        m_playerGameObject = i_viewer;
        GetSetVisibleDistanceThreshold = m_cloudData.m_visibleDistanceThreshold;
        m_disappearRate = m_cloudData.m_disappearRate;
        SpawnAtRandomPointInSquare(new Vector3(m_playerGameObject.transform.position.x, GetSetHeight, m_playerGameObject.transform.position.z), m_cloudData.m_visibleDistanceThreshold);
        
    }

    // Update is called once per frame
    private void Update () {

        if (m_autoAnimate)
        {
            UpdateScaleBasedOnDistance();

            transform.Translate(-(Vector3.forward + Vector3.left) * m_movementSpeed * Time.deltaTime);
        }

        if (m_isTargeted)
        {
            float l_scale = Mathf.Clamp01(transform.localScale.x - m_disappearRate * Time.deltaTime);
            Vector3 l_newScale = new Vector3(l_scale, l_scale, l_scale);

            if (l_newScale == Vector3.zero)
            {
                m_particle = Instantiate(m_cloudData.m_cloudBurstParticles[Random.Range(0, m_cloudData.m_cloudBurstParticles.Count)], transform.position, Quaternion.identity, transform.parent.transform) as GameObject;

                m_particle.GetComponent<ParticleSystem>().Play(true);
            }

            transform.localScale = l_newScale;
        }
    }

    private void UpdateScaleBasedOnDistance()
    {
        float l_distance = Vector3.Distance(transform.position, new Vector3(m_playerGameObject.transform.position.x, m_height, m_playerGameObject.transform.position.z));
        float l_scaleBasedOnDistance = Mathf.InverseLerp(m_visibleDistanceThreshold, m_height, l_distance);

        transform.localScale = new Vector3(l_scaleBasedOnDistance, l_scaleBasedOnDistance, l_scaleBasedOnDistance);
    }

    public void SpawnAtRandomPointInSquare(Vector3 i_center, float i_size)
    {
        Vector3 l_spawnPoint = i_center + new Vector3((Random.value - 0.5f) * i_size, m_height, (Random.value - 0.5f) * i_size);
        transform.position = l_spawnPoint;
    }

    protected override void OnUseActivated(object sender, InteractableObjectEventArgs e)
    {
        base.OnUseActivated(sender, e);
        Debug.Log("ACTIVATE");
        m_autoAnimate = false;
        m_isTargeted = true;
    }

    protected override void OnUnuseActivated(object sender, InteractableObjectEventArgs e)
    {
        base.OnUnuseActivated(sender, e);
        Debug.Log("DEACTIVATE");

        //m_autoAnimate = true;
        //m_isTargeted = false;
    }
}
