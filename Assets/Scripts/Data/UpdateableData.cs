using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateableData : ScriptableObject {

    public event System.Action OnValuesUpdated;
    public bool m_autoUpdate;

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        if (m_autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;

        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

#endif


}
