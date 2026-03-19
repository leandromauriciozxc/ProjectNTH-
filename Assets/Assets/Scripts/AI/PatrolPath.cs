using System.Linq;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public Transform[] Points { get; private set; }

    void Awake()
    {
        Points = GetComponentsInChildren<PatrolPoint>()
            .Select(p => p.transform)
            .ToArray();
    }

    public Transform GetPoint(int index)
    {
        return Points[index % Points.Length];
    }

    public int Length => Points.Length;


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform point = transform.GetChild(i);

            Gizmos.DrawSphere(point.position, 0.2f);

            if (i < transform.childCount - 1)
            {
                Gizmos.DrawLine(point.position, transform.GetChild(i + 1).position);
            }
        }
    }
}