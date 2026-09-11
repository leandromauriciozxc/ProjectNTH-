using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Transform uiAnchor;
    [SerializeField] private string promptText = "[E] Open";

    [Header("Interaction")]
    [SerializeField] private UnityEvent onInteract = new UnityEvent();

    public Transform UIAnchor => uiAnchor != null ? uiAnchor : transform;
    public string PromptText => promptText;

    public void Interact()
    {
        if (isActiveAndEnabled)
        {
            onInteract.Invoke();
        }
    }

    public void SetPromptText(string text)
    {
        promptText = text;
    }
}