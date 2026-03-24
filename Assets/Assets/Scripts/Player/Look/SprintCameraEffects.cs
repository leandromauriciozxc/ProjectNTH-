using UnityEngine;

public class SprintCameraEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera playerCamera;
    [SerializeField] PlayerMovement movement;
    [SerializeField] HeadBob headBob;
    [SerializeField] LookSway lookSway;

    [Header("FOV")]
    [SerializeField] float normalFOV = 70f;
    [SerializeField] float sprintFOV = 80f;

    [SerializeField] float fovSpeed = 6f;

    [Header("HeadBob")]
    [SerializeField] float sprintBobMultiplier = 1.5f;

    [Header("Sway")]
    [SerializeField] float sprintSwayMultiplier = 1.4f;

    [Header("Tilt")]
    [SerializeField] Transform tiltPivot;
    [SerializeField] float sprintForwardTilt = 2f;
    [SerializeField] float tiltSpeed = 6f;

    float currentForwardTilt;

    void Update()
    {
        HandleFOV();
        HandleTilt();
        HandleBob();
        HandleSway();
    }

    void HandleFOV()
    {
        float targetFOV = movement.IsRunning ? sprintFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * fovSpeed
        );
    }

    void HandleTilt()
    {
        float targetTilt = movement.IsRunning ? sprintForwardTilt : 0f;

        currentForwardTilt = Mathf.Lerp(
            currentForwardTilt,
            targetTilt,
            Time.deltaTime * tiltSpeed
        );

        tiltPivot.localRotation = Quaternion.Euler(
            currentForwardTilt,
            tiltPivot.localEulerAngles.y,
            tiltPivot.localEulerAngles.z
        );
    }

    void HandleBob()
    {
        if (movement.IsRunning)
            headBob.SetSprintMultiplier(sprintBobMultiplier);
        else
            headBob.SetSprintMultiplier(1f);
    }

    void HandleSway()
    {
        if (movement.IsRunning)
            lookSway.SetSprintMultiplier(sprintSwayMultiplier);
        else
            lookSway.SetSprintMultiplier(1f);
    }
}