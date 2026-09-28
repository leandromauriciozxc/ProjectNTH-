using UnityEngine;
using UnityEngine.InputSystem;

namespace FSR
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Camera _camera;
        [SerializeField] private FSR_Player fSR_Player;

        [Header("Movement")]
        [SerializeField] private float speed = 1f;
        [SerializeField] private float jumpForce = 1f;

        [Header("Camera")]
        [SerializeField] private float sensitivity = 1f;

        [Header("Footsteps")]
        [SerializeField] private float stepfrequency = 0.5f;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;

        private Vector2 inputDirection;
        private Vector2 lookInput;

        private float stepTimer;
        private float rotationX;
        private float rotationY;

        private void OnEnable()
        {
            moveAction.action.Enable();
            lookAction.action.Enable();
            jumpAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            lookAction.action.Disable();
            jumpAction.action.Disable();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleFootsteps();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleJump();
        }

        private void HandleMovement()
        {
            inputDirection = moveAction.action.ReadValue<Vector2>();

            Vector3 movement = transform.TransformDirection(
                new Vector3(inputDirection.x, 0f, inputDirection.y)
            );

            Vector3 velocity = _rigidbody.velocity;

            _rigidbody.velocity = new Vector3(movement.x * speed, velocity.y, movement.z * speed);
        }

        private void HandleJump()
        {
            if (jumpAction.action.IsPressed())
            {
                _rigidbody.AddForce(Vector3.up * jumpForce);
            }
        }

        private void HandleLook()
        {
            lookInput = lookAction.action.ReadValue<Vector2>();

            rotationY += lookInput.x * sensitivity;
            rotationX += lookInput.y * sensitivity;

            rotationX = Mathf.Clamp(rotationX, -45f, 45f);

            transform.rotation = Quaternion.Euler(0f, rotationY,0f);

            _camera.transform.localRotation = Quaternion.Euler(-rotationX, 0f, 0f);
        }

        private void HandleFootsteps()
        {
            stepTimer += Time.deltaTime;

            Vector3 horizontalVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);

            if (stepTimer > stepfrequency && horizontalVelocity.magnitude > 0.1f)
            {
                stepTimer = 0f;
                fSR_Player.step();
            }
        }
    }
}

//using UnityEngine;

//namespace FSR
//{
//    public class PlayerMovement : MonoBehaviour
//    {
//        [SerializeField] private Rigidbody _rigidbody;
//        [SerializeField] private Camera _camera;
//        [SerializeField] private float speed = 1;
//        [SerializeField] private float jumpForce = 1;
//        [SerializeField] private float sensitivity = 1;
//        [SerializeField] private FSR_Player fSR_Player;
//        [SerializeField] private float stepfrequency = 0.5f;
//        private float stepTimer;
//        float rotationX = 0F;
//        float rotationY = 0F;
//        private Vector2 inputDirection;

//        void FixedUpdate()
//        {
//            inputDirection = Vector3.zero;
//            if (Input.GetKey(KeyCode.W))
//            {
//                inputDirection.y += 1;
//            }
//            if (Input.GetKey(KeyCode.A))
//            {
//                inputDirection.x -= 1;
//            }
//            if (Input.GetKey(KeyCode.S))
//            {
//                inputDirection.y -= 1;
//            }
//            if (Input.GetKey(KeyCode.D))
//            {
//                inputDirection.x += 1;
//            }

//            inputDirection.Normalize();

//            _rigidbody.velocity = transform.TransformDirection(new Vector3(inputDirection.x, _rigidbody.velocity.y / speed, inputDirection.y)) * speed;


//            if (Input.GetKey(KeyCode.Space))
//            {
//                _rigidbody.AddForce(Vector3.up * jumpForce);
//            }
//        }

//        private void Update()
//        {
//            stepTimer += Time.deltaTime;
//            if (stepTimer > stepfrequency && _rigidbody.velocity.magnitude > 0.1f)
//            {
//                stepTimer = 0;
//                fSR_Player.step();
//            }

//            Cursor.lockState = CursorLockMode.Locked;
//            Cursor.visible = false;
//            rotationY += Input.GetAxis("Mouse X") * sensitivity;
//            rotationX += Input.GetAxis("Mouse Y") * sensitivity;

//            transform.eulerAngles = new Vector3(0, rotationY, 0);

//            rotationX = Mathf.Clamp(rotationX, -45, 45);

//            _camera.transform.eulerAngles = new Vector3(-rotationX, _camera.transform.eulerAngles.y, _camera.transform.eulerAngles.z);
//        }
//    }
//}
