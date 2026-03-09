using UnityEngine;

public class LookSway : MonoBehaviour
{
    [SerializeField]
    private float swayAmount = 0.02f;
    [SerializeField]
    private float smoothSpeed = 6f;

    Vector3 initialPos;
    Vector3 currentOffset;

    void Start()
    {
        initialPos = transform.localPosition;
    }

    public void UpdateSway(float mouseX)
    {
        float sway = -mouseX * swayAmount;

        currentOffset.x = Mathf.Lerp(currentOffset.x, sway, Time.deltaTime * smoothSpeed);

        transform.localPosition = initialPos + currentOffset;
    }
}