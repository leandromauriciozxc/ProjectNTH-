using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField]
    private InputReader input;
    [SerializeField]
    private Transform cameraPivot;
    [SerializeField] private SO_SensivitySettings SensivitySettings;
    private float xRotation;
    public float MouseX { get; private set; }

    void Update()
    {
        Vector2 look = input.Look;

        var sensitivity = SensivitySettings.GetSensitivity();

        MouseX = look.x * sensitivity;

        float mouseY = look.y * sensitivity;
        if (SensivitySettings.invertY)
            mouseY *= -1f;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * MouseX);
        
    }
}