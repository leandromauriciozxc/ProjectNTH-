using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform eyePoint;
    [SerializeField] Transform player;
    [SerializeField] LayerMask obstructionMask;

    [Header("Vision Settings")]
    [SerializeField] float viewDistance = 10f;
    [SerializeField] float viewAngle = 90f;

    public bool CanSeePlayer { get; private set; }
    public bool IsPlayerLookingAtEnemy { get; private set; }

    void Update()
    {
        CheckVision();
        CheckIfPlayerLooking();
    }

    void CheckVision()
    {
        Vector3 dirToPlayer = player.position - eyePoint.position;
        float distance = dirToPlayer.magnitude;

        if (distance > viewDistance)
        {
            CanSeePlayer = false;
            return;
        }

        float angle = Vector3.Angle(eyePoint.forward, dirToPlayer);

        if (angle > viewAngle * 0.5f)
        {
            CanSeePlayer = false;
            return;
        }

        // Raycast (check obstruction)
        if (Physics.Raycast(
            eyePoint.position,
            dirToPlayer.normalized,
            out RaycastHit hit,
            viewDistance,
            obstructionMask
        ))
        {
            if (hit.transform == player)
            {
                CanSeePlayer = true;
            }
            else
            {
                CanSeePlayer = false;
            }
        }
    }

    void CheckIfPlayerLooking()
    {
        Vector3 dirToEnemy = (eyePoint.position - player.position).normalized;

        float dot = Vector3.Dot(player.forward, dirToEnemy);

        // tweak threshold
        IsPlayerLookingAtEnemy = dot > 0.75f;
    }
    void OnDrawGizmos()
    {
        if (eyePoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePoint.position, player.position);
    }
}