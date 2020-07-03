using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BellInteraction : InteractionSubscriber
{
    private AudioSource m_audioSource;
    private Animator m_animator;

    protected override void Awake()
    {
        base.Awake();

        try
        {
            m_audioSource = GetComponent<AudioSource>();
            m_animator = GetComponent<Animator>();
        }
        catch (Exception)
        {
            Debug.Log("Getting some of the components failed, check: " + gameObject.name);
            throw;
        }
    }

    protected override void OnUseActivated(object sender, InteractableObjectEventArgs e)
    {
        base.OnUseActivated(sender, e);
        m_audioSource.Play();
        m_animator.Play("Ring");
    }
}
