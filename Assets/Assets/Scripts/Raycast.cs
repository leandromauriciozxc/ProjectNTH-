using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Raycast : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField]
    private LayerMask m_layermask;
    [SerializeField]
    private GameObject m_startPoint;
    [SerializeField]
    private float m_raycastLength;
    [Header("UI")]
    [SerializeField]
    private Image m_targetImage;
    [SerializeField]
    private Sprite m_openHand;
    [SerializeField]
    private Sprite m_closeHand;

    public static event Action<bool> OnInteraction;
    void Update()
    {
        CheckRaycast();
    }

    private void CheckRaycast()
    {
        Ray ray = new Ray(m_startPoint.transform.position, m_startPoint.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, m_raycastLength, m_layermask))
        {
            m_targetImage.sprite = m_openHand;
        }
        else
        {
            m_targetImage.sprite = m_closeHand;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(m_startPoint.transform.position, m_startPoint.transform.forward *  m_raycastLength);
    }
}
