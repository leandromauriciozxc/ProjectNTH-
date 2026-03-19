using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] PatrolPath patrolPath;

    int currentIndex;

    public Transform GetNextPoint()
    {
        if (patrolPath == null || patrolPath.Length == 0)
            return null;

        Transform point = patrolPath.GetPoint(currentIndex);
        currentIndex = (currentIndex + 1) % patrolPath.Length;

        return point;
    }
}