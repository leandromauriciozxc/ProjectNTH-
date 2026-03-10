using UnityEngine;

public class StrafeTilt : MonoBehaviour
{
    [SerializeField]
    private float tiltAmount = 2f;
    [SerializeField]
    private float smoothSpeed = 6f;
    [SerializeField] 
    private float tiltReturnSpeed = 4f;
    [SerializeField] 
    private float tiltSpeed = 8f;

    float currentTilt;

    public void UpdateTilt(float inputX)
    {
        float targetTilt = -inputX * tiltAmount;

        float speed = Mathf.Abs(inputX) > 0.01f ? tiltSpeed : tiltReturnSpeed;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * speed);

        transform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}