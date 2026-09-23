using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Police1 : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private Animator m_animator2;

    public void SetBoolTrigger()
    {
        m_animator.SetBool("stopRun",true); 
    }
    public void SetBoolTriggerForRun()
    {
        m_animator2.SetBool("toRun",true);
    }
}
