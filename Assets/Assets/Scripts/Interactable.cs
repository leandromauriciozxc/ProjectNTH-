using UnityEngine;
using UnityEngine.Events;

/// <summary>Optional checks used by the player's interaction ray, such as a button's front face.</summary>
public interface IInteractionFilter
{
    bool CanInteractFrom(Vector3 viewerPosition);
}

public class Interactable : MonoBehaviour
{
    [SerializeField] private Transform uiAnchor;
    [SerializeField] private string promptText = "[E] Open";

    [Header("Interaction")]
    [SerializeField] private UnityEvent onInteract = new UnityEvent();

    public Transform UIAnchor => uiAnchor != null ? uiAnchor : transform;
    public string PromptText => promptText;

    private MonoBehaviour[] interactionFilters;

    private void Awake()
    {
        interactionFilters = GetComponents<MonoBehaviour>();
    }

    public bool CanInteractFrom(Vector3 viewerPosition)
    {
        if (!isActiveAndEnabled) return false;
        if (interactionFilters == null) interactionFilters = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in interactionFilters)
        {
            if (component != null && component is IInteractionFilter filter
                && !filter.CanInteractFrom(viewerPosition)) return false;
        }
        return true;
    }

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
