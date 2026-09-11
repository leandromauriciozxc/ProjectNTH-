using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Transform uiAnchor;
    [SerializeField] private string promptText = "[E] Open";

    public Transform UIAnchor => uiAnchor != null ? uiAnchor : transform;
    public string PromptText => promptText;
}