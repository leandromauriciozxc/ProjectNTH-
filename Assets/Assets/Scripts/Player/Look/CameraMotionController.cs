using UnityEngine;

public class CameraMotionController : MonoBehaviour
{
    [SerializeField] private InputReader InputReader;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerLook look;
    [SerializeField] private HeadBob headBob;
    [SerializeField] private LookSway lookSway;
    [SerializeField] private StrafeTilt strafeTilt;
    [SerializeField] private IdleBreathing idleBreathing;

    void Update()
    {
        headBob.UpdateBob(movement.MoveDirection, movement.IsRunning);
        strafeTilt.UpdateTilt(InputReader.Move);
        lookSway.UpdateSway(look.MouseX);
        idleBreathing.UpdateBreath(movement.MoveDirection.magnitude);
    }

    public void SetMovementAndSpeed()
    {
        movement.walkSpeed = 1f;
        movement.runSpeed = 1.5f;
    }
    public void ReturnDefaultMovementSpeed()
    {
        movement.walkSpeed = 2f;
        movement.runSpeed = 3f;
    }
}