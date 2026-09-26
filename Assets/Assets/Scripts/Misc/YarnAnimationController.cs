using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class YarnAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [YarnCommand("anim_trigger")]
    public void AnimationTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }
}
