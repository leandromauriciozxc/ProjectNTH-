using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : MonoBehaviour
{
    [SerializeField]
    private GameObject m_objectToDestroy;
    [SerializeField]
    private AudioSource m_ratSound;

    public void RatSequence()
    {
        Destroy(this.m_objectToDestroy);
    }
}
