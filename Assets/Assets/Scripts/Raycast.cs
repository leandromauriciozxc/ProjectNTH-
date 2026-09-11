using System;
using UnityEngine;
using UnityEngine.UI;

public class Raycast : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private LayerMask m_layermask;
    [SerializeField] private GameObject m_startPoint;
    [SerializeField] private float m_raycastLength = 3f;

    [Header("UI")]
    [SerializeField] private Image m_targetImage;
    [SerializeField] private Sprite m_openHand;
    [SerializeField] private Sprite m_closeHand;

    // NEW: Reference to the prompt controller on your Canvas.
    [SerializeField] private InteractionPromptUI m_promptUI;

    public static event Action<bool> OnInteraction;

    private void Update()
    {
        CheckRaycast();
    }

    private void CheckRaycast()
    {
        Ray ray = new Ray(
            m_startPoint.transform.position,
            m_startPoint.transform.forward
        );

        Interactable target = null;

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            m_raycastLength,
            m_layermask))
        {
            // Finds the component even if the collider is on a child.
            target = hit.collider.GetComponentInParent<Interactable>();

            if (target != null && !target.isActiveAndEnabled)
            {
                target = null;
            }
        }

        // Open hand only when a valid interactable is detected.
        m_targetImage.sprite =
            target != null ? m_openHand : m_closeHand;

        // A target makes the prompt follow its anchor.
        // Null hides the prompt when you look away or leave range.
        m_promptUI.SetTarget(target);
        // Read E using whichever input system is enabled.
        #if ENABLE_INPUT_SYSTEM
        bool interactPressed =
            UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame;
        #else
        bool interactPressed = Input.GetKeyDown(KeyCode.E);
        #endif

        if (target != null && interactPressed)
        {
            target.Interact();
        }
    }

    private void OnDisable()
    {
        if (m_promptUI != null)
        {
            m_promptUI.SetTarget(null);
        }

        if (m_targetImage != null)
        {
            m_targetImage.sprite = m_closeHand;
        }
    }

    private void OnDrawGizmos()
    {
        if (m_startPoint == null)
        {
            return;
        }

        Gizmos.DrawRay(
            m_startPoint.transform.position,
            m_startPoint.transform.forward * m_raycastLength
        );
    }
}