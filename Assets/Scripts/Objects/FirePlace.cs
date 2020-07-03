using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlace : MonoBehaviour
{
    private ParticleSystem m_particle;

    private void Awake()
    {
        m_particle = GetComponentInChildren<ParticleSystem>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Fire")
        {
            m_particle.Play(true);
        }
        else if (collision.gameObject.tag == "Water")
        {
            m_particle.Stop(true);
        }
    }
}
