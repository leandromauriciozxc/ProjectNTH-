using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public Transform[] Points { get; private set; }

    void Awake()
    {
        Points = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            Points[i] = transform.GetChild(i);
        }
    }

    public Transform GetPoint(int index)
    {
        return Points[index % Points.Length];
    }

    public int Length => Points.Length;
}