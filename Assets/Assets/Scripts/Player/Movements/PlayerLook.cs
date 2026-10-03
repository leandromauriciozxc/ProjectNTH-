using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private InputReader input;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private SO_SensivitySettings SensivitySettings;
    private float xRotation;
    public float MouseX { get; private set; }
    private bool canLook = true;

    public void SetCanLook(bool value)
    {
        canLook = value;
    }
    void Update()
    {
        if (UiOtherController.BlocksGameplayInput) { MouseX = 0f; return; }
        if (!canLook)
            return;

        Vector2 look = input.Look;

        var settings = ProjectNTH.Settings.GameSettings.Instance;
        var sensitivity = settings != null && settings.HasSensitivity
            ? Mathf.Lerp(SensivitySettings.minSensitivity, SensivitySettings.maxSensitivity, settings.Sensitivity)
            : SensivitySettings.GetSensitivity();

        MouseX = look.x * sensitivity;

        float mouseY = look.y * sensitivity;
        if (settings != null && settings.HasInvertY ? settings.InvertY : SensivitySettings.invertY)
            mouseY *= -1f;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * MouseX);
        
    }
}
