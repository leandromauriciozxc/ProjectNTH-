using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] float walkBobSpeed = 6f;
    [SerializeField] float runBobSpeed = 9f;

    [Header("Motion Amount")]
    [SerializeField] float verticalAmount = 0.025f;
    [SerializeField] float horizontalAmount = 0.015f;
    [SerializeField] float forwardAmount = 0.008f;

    [Header("Return Speed")]
    [SerializeField] float returnSpeed = 5f;
    float sprintMultiplier = 1f;

    float bobTimer;

    Vector3 startPos;
    Vector3 currentOffset;

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
        float movementAmount = moveDirection.magnitude;

        if (movementAmount > 0.1f)
        {
            float speed = isRunning ? runBobSpeed : walkBobSpeed;

            bobTimer += movementAmount * Time.deltaTime * speed;

            float wave = Mathf.Sin(bobTimer);

            // Vertical head drop during step impact
            float vertical = wave * Mathf.Abs(wave) * verticalAmount;

            // Side sway
            float horizontal = Mathf.Cos(bobTimer * 0.5f) * horizontalAmount;

            // Forward momentum
            float forward = Mathf.Sin(bobTimer * 0.5f) * forwardAmount;

            // NEW: step weight shift
            float stepWeight = Mathf.Sin(bobTimer * 0.5f);
            horizontal += stepWeight * horizontalAmount * 0.5f;

            float intensity = Mathf.Clamp01(movementAmount);

            currentOffset = new Vector3(horizontal, vertical, forward) * intensity * sprintMultiplier;
        }
        else
        {
            currentOffset = Vector3.Lerp(
                currentOffset,
                Vector3.zero,
                Time.deltaTime * returnSpeed
            );
        }

        transform.localPosition = startPos + currentOffset;
    }
}