using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleHandler : MonoBehaviour {

    [SerializeField]
    private Transform m_player;

    private bool m_isParticleActive = false;

    [SerializeField]
    private GameObject m_ambientParticleSystem;
    public GameObject GetSetAmbientParticleSystem
    {
        get
        {
            return m_ambientParticleSystem;
        }
        set
        {
            m_ambientParticleSystem = value;
        }
    }

    private List<GameObject> m_activeParticles = new List<GameObject>();

    private Vector3 m_currentPosition;

    public static PlayerParticleHandler instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        m_currentPosition = m_player.transform.position;

        GenerateParticlesAroundPlayer();
    }


    private void GenerateParticlesAroundPlayer()
    {

        if (!m_isParticleActive)
        {
            GameObject l_newParticle = Instantiate(GetSetAmbientParticleSystem, this.transform) as GameObject;

            m_activeParticles.Add(l_newParticle);

            m_isParticleActive = true;
        }

        var l_particleShape = m_activeParticles[0].GetComponent<ParticleSystem>().shape;

        l_particleShape.position = m_currentPosition;
    }
}
