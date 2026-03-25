using UnityEngine;

public class IdleBreathing : MonoBehaviour
{
    [SerializeField] private float breatheSpeed = 1.2f;
    [SerializeField] private float breatheAmount = 0.005f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    public void UpdateBreath(float movementAmount)
    {
        if (movementAmount < 0.1f)
        {
            float y = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;

            transform.localPosition = startPos + new Vector3(0, y, 0);
        }
    }
}