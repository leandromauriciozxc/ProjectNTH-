using UnityEngine;

public class LookBack : MonoBehaviour
{
    

    [Header("Settings")]
    [SerializeField] float lookBackAngle = 180f;
    [SerializeField] float cameraLag = 0.12f;
    [SerializeField] InputReader input;

    float currentAngle;
    float targetAngle;
    float velocity;

    public bool IsLookingBack { get; private set; }

    void Update()
    {
        HandleLookBack();
    }

    void HandleLookBack()
    {
        if (input.Lookback)
        {
            targetAngle = lookBackAngle;
            IsLookingBack = true;
        }
        else
        {
            targetAngle = 0f;
            IsLookingBack = false;
        }

        currentAngle = Mathf.SmoothDampAngle(
            currentAngle,
            targetAngle,
            ref velocity,
            cameraLag
        );

        transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}