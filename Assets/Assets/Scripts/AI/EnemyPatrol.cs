using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private PatrolPath patrolPath;

    private int currentIndex;

    public int PointCount =>
        patrolPath != null ? patrolPath.Length : 0;

    public Transform GetNextPoint()
    {
        if (patrolPath == null)
        {
            Debug.LogError("PatrolPath is not assigned.");
            return null;
        }

        if (patrolPath.Length == 0)
        {
            Debug.LogError("PatrolPath has no points.");
            return null;
        }

        Transform point = patrolPath.GetPoint(currentIndex);

        currentIndex =
            (currentIndex + 1) % patrolPath.Length;

        return point;
    }
}