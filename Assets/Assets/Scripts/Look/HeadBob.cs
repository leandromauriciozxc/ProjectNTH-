using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] float walkBobSpeed = 6f;
    [SerializeField] float runBobSpeed = 9f;

    [Header("Motion Amount")]
    [SerializeField] float verticalAmount = 0.025f;
    [SerializeField] float horizontalAmount = 0.015f;
    [SerializeField] float forwardAmount = 0.01f;

    [Header("Smoothness")]
    [SerializeField] float bobFadeSpeed = 6f;
    [SerializeField] float returnSpeed = 5f;

    float bobCycle;
    float bobWeight;

    Vector3 startPos;
    Vector3 currentOffset;

    float sprintMultiplier = 1f;

    void Start()
    {
        startPos = transform.localPosition;
    }

    public void SetSprintMultiplier(float value)
    {
        sprintMultiplier = value;
    }

    public void UpdateBob(Vector3 moveDirection, bool isRunning)
    {
        float speed = moveDirection.magnitude;

        // smooth enable / disable
        float targetWeight = speed > 0.1f ? 1f : 0f;

        bobWeight = Mathf.Lerp(
            bobWeight,
            targetWeight,
            Time.deltaTime * bobFadeSpeed
        );

        if (speed > 0.1f)
        {
            float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;

            // STEP CYCLE (independent of direction)
            bobCycle += Time.deltaTime * bobSpeed * speed;

            float vertical = Mathf.Sin(bobCycle) * verticalAmount;

            float horizontal = Mathf.Sin(bobCycle * 0.5f) * horizontalAmount;

            float forward = Mathf.Abs(Mathf.Sin(bobCycle)) * forwardAmount;

            currentOffset = new Vector3(
                horizontal,
                vertical,
                forward
            ) * sprintMultiplier * bobWeight;
        }
        else
        {
            // smooth settle
            currentOffset = Vector3.Lerp(
                currentOffset,
                Vector3.zero,
                Time.deltaTime * returnSpeed
            );
        }

        transform.localPosition = startPos + currentOffset;
    }
}