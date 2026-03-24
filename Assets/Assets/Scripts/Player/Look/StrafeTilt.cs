using UnityEngine;

public class StrafeTilt : MonoBehaviour
{
    [Header("Tilt Amounts")]
    [SerializeField] float strafeTiltAmount = 2f;
    [SerializeField] float forwardTiltAmount = 1.2f;

    [Header("Smoothing")]
    [SerializeField] float tiltSpeed = 8f;
    [SerializeField] float tiltReturnSpeed = 4f;

    Vector2 currentTilt;

    public void UpdateTilt(Vector2 moveInput)
    {
        Vector2 targetTilt = new Vector2(
            -moveInput.y * forwardTiltAmount,   // W / S tilt
            -moveInput.x * strafeTiltAmount     // A / D tilt
        );

        float speed = moveInput.sqrMagnitude > 0.01f ? tiltSpeed : tiltReturnSpeed;

        currentTilt = Vector2.Lerp(
            currentTilt,
            targetTilt,
            Time.deltaTime * speed
        );

        transform.localRotation = Quaternion.Euler(
            currentTilt.x,
            0f,
            currentTilt.y
        );
    }
}