using UnityEngine;

public class LookSway : MonoBehaviour
{
    [SerializeField] private float swayAmount = 0.02f;
    [SerializeField] private float smoothSpeed = 6f;

    Vector3 initialPos;
    Vector3 currentOffset;
    float sprintMultiplier = 1f;
    void Start()
    {
        initialPos = transform.localPosition;
    }
   

    public void SetSprintMultiplier(float value)
    {
        sprintMultiplier = value;
    }

    public void UpdateSway(float mouseX)
    {
        float sway = -mouseX * swayAmount * sprintMultiplier;

        currentOffset.x = Mathf.Lerp(currentOffset.x, sway, Time.deltaTime * smoothSpeed);

        if (Mathf.Abs(mouseX) < 0.01f)
            currentOffset.x = Mathf.Lerp(currentOffset.x, 0f, Time.deltaTime * smoothSpeed);

        transform.localPosition = initialPos + currentOffset;
    }
}