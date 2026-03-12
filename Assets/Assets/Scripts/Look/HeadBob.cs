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
    [Header("Impact")]
    [SerializeField] float stepImpactAmount = 0.015f;
    [SerializeField] float impactRecoverSpeed = 8f;

    float sprintMultiplier = 1f;

    float bobTimer;
    float stepImpact;
    float previousWave;
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

            // Footstep impact detection
            if (previousWave > 0f && wave <= 0f)
            {
                if (isRunning)
                    stepImpact = stepImpactAmount;
            }

            previousWave = wave;

            float vertical = wave * Mathf.Abs(wave) * verticalAmount;
            float horizontal = Mathf.Cos(bobTimer * 0.5f) * horizontalAmount;
            float forward = Mathf.Sin(bobTimer * 0.5f) * forwardAmount;

            float intensity = Mathf.Clamp01(movementAmount);

            // impact recovery
            stepImpact = Mathf.Lerp(stepImpact, 0f, Time.deltaTime * impactRecoverSpeed);

            currentOffset = new Vector3(horizontal, vertical - stepImpact, forward) * intensity * sprintMultiplier;
        }
        else
        {
            currentOffset = Vector3.Lerp(currentOffset, Vector3.zero, Time.deltaTime * returnSpeed);
        }

        transform.localPosition = startPos + currentOffset;
    }
}