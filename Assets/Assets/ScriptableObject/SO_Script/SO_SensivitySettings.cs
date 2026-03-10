using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Sensitivity Settings")]
public class SO_SensivitySettings : ScriptableObject
{
    [Header("Sensitivity")]
    [Range(0f, 1f)]
    public float sensitivity = 0.5f;

    [Header("Options")]
    public bool invertY = false;

    [Header("Limits")]
    public float minSensitivity = 50f;
    public float maxSensitivity = 300f;

    public float GetSensitivity()
    {
        return Mathf.Lerp(minSensitivity, maxSensitivity, sensitivity);
    }
}