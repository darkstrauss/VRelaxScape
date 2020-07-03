using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PointerBehaviorHandler : MonoBehaviour
{
    
    private List<VRTK_Pointer> m_pointers = new List<VRTK_Pointer>();

    private void Awake()
    {
        for (int i = 0; i < (GetComponents<VRTK_Pointer>().Length); i++)
        {
            m_pointers.Add(GetComponents<VRTK_Pointer>()[i]);
        }

        for (int i = 0; i < m_pointers.Count - 1; i++)
        {
            m_pointers[i].ActivationButtonPressed += OnActivationButtonPressed;
            m_pointers[i].ActivationButtonReleased += OnActivationButtonReleased;
            m_pointers[i].SelectionButtonPressed += OnSelectionButtonPressed;
            m_pointers[i].SelectionButtonReleased += OnSelectionButtonReleased;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < m_pointers.Count - 1; i++)
        {
            m_pointers[i].ActivationButtonPressed -= OnActivationButtonPressed;
            m_pointers[i].ActivationButtonReleased -= OnActivationButtonReleased;
            m_pointers[i].SelectionButtonPressed -= OnSelectionButtonPressed;
            m_pointers[i].SelectionButtonReleased -= OnSelectionButtonReleased;

        }
    }

    private void OnSelectionButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        
    }

    private void OnSelectionButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        
    }

    private void OnActivationButtonReleased(object sender, ControllerInteractionEventArgs e)
    {
        
    }

    private void OnActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        
    }

    private void CheckUserBehavior()
    {
        
    }
}
