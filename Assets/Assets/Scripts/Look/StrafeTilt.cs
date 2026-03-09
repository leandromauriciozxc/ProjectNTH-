using UnityEngine;

public class StrafeTilt : MonoBehaviour
{
    [SerializeField]
    private float tiltAmount = 2f;
    [SerializeField]
    private float smoothSpeed = 6f;

    float currentTilt;

    public void UpdateTilt(float inputX)
    {
        float targetTilt = -inputX * tiltAmount;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * smoothSpeed);

        transform.localRotation = Quaternion.Euler(0, 0, currentTilt);
    }
}