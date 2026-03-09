using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    
    public float walkSpeed = 3.5f;
    public float runSpeed = 5.5f;
    public float gravity = -9.81f;

    CharacterController controller;
    InputReader input;

    Vector3 velocity;

    public Vector3 MoveDirection { get; private set; }
    public bool IsRunning { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<InputReader>();
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
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}