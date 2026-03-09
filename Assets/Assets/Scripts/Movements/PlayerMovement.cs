using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float walkSpeed = 3.5f;
    [SerializeField]
    private float runSpeed = 5.5f;
    [SerializeField]
    private float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    public Vector3 MoveDirection { get; private set; }
    public bool IsRunning { get; private set; }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Run.performed += ctx => IsRunning = true;
        inputActions.Player.Run.canceled += ctx => IsRunning = false;
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float speed = IsRunning ? runSpeed : walkSpeed;

        MoveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        controller.Move(MoveDirection * speed * Time.deltaTime);

        ApplyGravity();
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}