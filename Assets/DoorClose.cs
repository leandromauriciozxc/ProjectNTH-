using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorClose : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    
    public void CloseDoor()
    {
        m_animator.SetBool("willDoorClose", true);
    }
}
