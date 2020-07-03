using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(AudioSource))]
public class InteractionSubscriber : MonoBehaviour
{
    private VRTK_InteractableObject m_interactableObject;

    [SerializeField]
    protected bool m_playSoundOnInteract = false;
    [SerializeField]
    protected AudioClip m_audioClipToPlayOnUse;
    [SerializeField]
    protected AudioClip m_audioClipToPlayOnUnuse;
    [SerializeField]
    protected AudioClip m_audioClipToPlayOnTouch;
    [SerializeField]
    protected AudioClip m_audioClipToPlayOnGrab;


    protected virtual void Awake()
    {
        try
        {
            m_interactableObject = GetComponent<VRTK_InteractableObject>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Could not find interactable object script on gameObject: " + gameObject.name);
            throw;
        }
    }

    private void OnValidate()
    {
        if (m_interactableObject == null)
        {
            try
            {
                m_interactableObject = GetComponent<VRTK_InteractableObject>();
            }
            catch (System.Exception)
            {
                Debug.LogError("Could not find interactable object script on gameObject: " + gameObject.name);
                throw;
            }
        }
    }

    private void OnEnable()
    {
        m_interactableObject.SubscribeToInteractionEvent(VRTK_InteractableObject.InteractionType.Use, OnUseActivated);
        m_interactableObject.SubscribeToInteractionEvent(VRTK_InteractableObject.InteractionType.Unuse, OnUnuseActivated);
        m_interactableObject.SubscribeToInteractionEvent(VRTK_InteractableObject.InteractionType.Touch, OnTouchActivated);
        m_interactableObject.SubscribeToInteractionEvent(VRTK_InteractableObject.InteractionType.Grab, OnGrabActivated);
        
    }

    private void OnDisable()
    {
        m_interactableObject.UnsubscribeFromInteractionEvent(VRTK_InteractableObject.InteractionType.Use, OnUseActivated);
        m_interactableObject.UnsubscribeFromInteractionEvent(VRTK_InteractableObject.InteractionType.Unuse, OnUnuseActivated);
        m_interactableObject.UnsubscribeFromInteractionEvent(VRTK_InteractableObject.InteractionType.Touch, OnTouchActivated);
        m_interactableObject.UnsubscribeFromInteractionEvent(VRTK_InteractableObject.InteractionType.Grab, OnGrabActivated);
    }

    protected virtual void OnUseActivated(object sender, InteractableObjectEventArgs e)
    {
        PlaySoundOnInteraction(m_audioClipToPlayOnUse);
    }

    protected virtual void OnUnuseActivated(object sender, InteractableObjectEventArgs e)
    {
        PlaySoundOnInteraction(m_audioClipToPlayOnUnuse);
    }

    protected virtual void OnTouchActivated(object sender, InteractableObjectEventArgs e)
    {
        PlaySoundOnInteraction(m_audioClipToPlayOnTouch);
    }

    protected virtual void OnGrabActivated(object sender, InteractableObjectEventArgs e)
    {
        PlaySoundOnInteraction(m_audioClipToPlayOnGrab);
    }

    private void PlaySoundOnInteraction(AudioClip i_clip)
    {
        if (!m_playSoundOnInteract || i_clip == null)
        {
            Debug.Log("No audio clip set for " + gameObject.name + " m_playSoundOnInteract == " + m_playSoundOnInteract);
            return;
        }

        if (m_playSoundOnInteract)
        {
            try
            {
                AudioSource l_audioSource = GetComponent<AudioSource>();
                l_audioSource.clip = i_clip;
                l_audioSource.Play();
            }
            catch (Exception)
            {
                Debug.LogError(gameObject.name + " is trying to play an audio clip, but cannot find the source!");
                throw;
            }
        }
    }

}
