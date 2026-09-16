using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform promptRoot;
    [SerializeField] private TMP_Text label;

    private RectTransform promptParent;
    private Interactable target;

    private void Awake()
    {
        promptParent = (RectTransform)promptRoot.parent;
        promptRoot.gameObject.SetActive(false);
    }

    public void SetTarget(Interactable newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        // Hide if nothing is selected, or the object was disabled/destroyed.
        if (target == null || !target.isActiveAndEnabled)
        {
            promptRoot.gameObject.SetActive(false);
            return;
        }

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(target.UIAnchor.position);

        // Hide anchors behind the camera or outside its screen area.
        bool visible =
            screenPosition.z > 0f &&
            playerCamera.pixelRect.Contains(
                new Vector2(screenPosition.x, screenPosition.y));

        if (!visible)
        {
            promptRoot.gameObject.SetActive(false);
            return;
        }

        // Convert screen pixels into the UI parent's local coordinates.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            promptParent,
            new Vector2(screenPosition.x, screenPosition.y),
            null,
            out Vector2 localPosition
        );

        promptRoot.localPosition =
            new Vector3(localPosition.x, localPosition.y, 0f);

        label.text = target.PromptText;
        promptRoot.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        target = null;

        if (promptRoot != null)
            promptRoot.gameObject.SetActive(false);
    }
}