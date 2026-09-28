using FSR;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    
    public float walkSpeed = 3.5f;
    public float runSpeed = 5.5f;
    public float gravity = -9.81f;
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;

    float stepTimer;
    CharacterController controller;
    InputReader input;

    Vector3 velocity;
    FSR_Player footstepAudioHandler;

    public Vector3 MoveDirection { get; private set; }
    public bool IsRunning { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
        footstepAudioHandler = GetComponent<FSR_Player>();
    }

    void Update()
    {
        Move();
        ApplyGravity();
    }

    void Move()
    {
        Vector2 move = input.Move;

        IsRunning = input.Run;

        float speed = IsRunning ? runSpeed : walkSpeed;

        MoveDirection = transform.right * move.x + transform.forward * move.y;

        controller.Move(MoveDirection * speed * Time.deltaTime);

        HandleFootsteps(move);
    }
    void HandleFootsteps(Vector2 move)
    {
        //if (!controller.isGrounded)
        //    return;

        if (move.sqrMagnitude < 0.01f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            footstepAudioHandler.step();

            stepTimer = IsRunning ? runStepInterval : walkStepInterval;
        }
    }
    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}