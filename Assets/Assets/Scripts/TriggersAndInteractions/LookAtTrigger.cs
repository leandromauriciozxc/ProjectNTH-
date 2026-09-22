using UnityEngine;

public class LookAtTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

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

        PlayerLookAt.Instance.LookAt(target);
    }

    public void StopLookAt()
    {
        if (PlayerLookAt.Instance == null)
            return;

        PlayerLookAt.Instance.StopLookAt();
    }
}