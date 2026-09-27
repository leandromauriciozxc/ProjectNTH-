using UnityEngine;

public class LookAtTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Tooltip("Hold the target's starting position throughout the conversation so character animation cannot pull the camera away. Disable to follow a moving target.")]
    [SerializeField] private bool holdInitialFocus = true;

    public void LookAtTarget()
    {
        if (target == null)
        {
            Debug.LogWarning(
                $"LookAtTrigger on {gameObject.name} has no target assigned."
            );

            return;
        }

        if (PlayerLookAt.Instance == null)
        {
            Debug.LogWarning(
                "PlayerLookAt Instance could not be found."
            );

            return;
        }

        if (holdInitialFocus)
            PlayerLookAt.Instance.LookAtFixed(target);
        else
            PlayerLookAt.Instance.LookAt(target);
    }

    public void StopLookAt()
    {
        if (PlayerLookAt.Instance == null)
            return;

        PlayerLookAt.Instance.StopLookAt();
    }
}
