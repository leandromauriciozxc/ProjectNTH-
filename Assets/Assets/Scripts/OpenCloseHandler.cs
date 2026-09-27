using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseHandler : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string openTriggerParam;
    [SerializeField]
    private string closeTriggerParam;

    private bool isOpen;
    public void InteractionHandler()
    {
        if (!isOpen)
        {
            animator.SetTrigger(openTriggerParam);
            isOpen = true;
        }
        else
        {
            animator.SetTrigger(closeTriggerParam);
            isOpen = true;
        }
    }
}
