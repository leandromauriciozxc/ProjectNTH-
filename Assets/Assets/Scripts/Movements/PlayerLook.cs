using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    private InputReader input;
    [SerializeField]
    private Transform cameraPivot;
    [SerializeField]
    private float sensitivity;
    private float xRotation;
    public float MouseX { get; private set; }

    void Update()
    {
        Vector2 look = input.Look;

        MouseX = look.x * sensitivity;

        float mouseY = look.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * MouseX);
    }
}