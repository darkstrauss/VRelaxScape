using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class PlayerPanelFader : MonoBehaviour
{
    private Image m_panelImage;
    [SerializeField]
    private Transform m_player;
    [SerializeField]
    private AnimationCurve m_fadeInCurve;
    [SerializeField]
    private AnimationCurve m_fadeOutCurve;
    [SerializeField]
    private GameObject m_teleporterParent;
    private VRTK_BasicTeleport m_teleporter;

    private bool m_isTeleporting = false;

    private void Awake()
    {
        List<GameObject> m_possibleTeleporters = new List<GameObject>();

        for (int i = 0; i < m_teleporterParent.transform.childCount; i++)
        {
            if (m_teleporterParent.transform.GetChild(i).gameObject.activeInHierarchy)
            {
                m_teleporter = m_teleporterParent.transform.GetChild(i).GetComponent<VRTK_BasicTeleport>();
                break;
            }
        }

        if (m_panelImage == null)
        {
            m_panelImage = GetComponentInChildren<Image>();
        }
    }

    private void OnEnable()
    {
        m_teleporter.Teleporting += OnTeleporting;
        m_teleporter.Teleported += OnTeleported;

        StartCoroutine(PanelFade(m_fadeInCurve, 3.0f, 5.0f));
    }

    private void OnDisable()
    {
        m_teleporter.Teleporting -= OnTeleporting;
        m_teleporter.Teleported -= OnTeleported;
    }

    private void OnTeleported(object sender, DestinationMarkerEventArgs e)
    {
        StartCoroutine(PanelFade(m_fadeInCurve, 0.6f));
    }

    private void OnTeleporting(object sender, DestinationMarkerEventArgs e)
    {
        if (!m_isTeleporting)
        {
            m_isTeleporting = true;
            StartCoroutine(PanelFade(m_fadeOutCurve, 0.6f));
        }
    }

    private IEnumerator PanelFade(AnimationCurve i_curve, float i_fadeTime = 1.3f, float i_delay = 0)
    {
        if (i_delay > 0)
        {
            yield return new WaitForSecondsRealtime(i_delay);
        }

        float l_timeTaken = Mathf.Epsilon;
        float l_alphaValue = m_panelImage.color.a;

        while (l_timeTaken < i_fadeTime)
        {
            l_timeTaken += Time.deltaTime;
            l_alphaValue = i_curve.Evaluate(l_timeTaken / i_fadeTime);

            Color l_newColor = new Color(0, 0, 0, l_alphaValue);
            m_panelImage.color = l_newColor;

            yield return new WaitForEndOfFrame();
        }

        m_isTeleporting = false;

        yield return null;
    }
}
