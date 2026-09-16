using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool openOutwards = true;

    private Interactable interactable;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();

        // Matches the default DoorClosed state in your screenshot.
        interactable.SetPromptText("[E] Open");
    }

    public void ToggleDoor()
    {
        if (!isActiveAndEnabled ||
            animator == null ||
            !animator.isActiveAndEnabled)
        {
            return;
        }

        // Wait until any transition has finished.
        if (animator.IsInTransition(0))
        {
            return;
        }

        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Base Layer.DoorLeftClosed"))
        {
            SetDoorOpen(true);
        }
        else if(state.IsName("Base Layer.DoorRightClosed"))
        {
            SetDoorOpen(true);
        }
        else if (
            state.IsName("Base Layer.DoorLeftOpenedIn") ||
            state.IsName("Base Layer.DoorLeftOpenedOut"))
        {
            SetDoorOpen(false);
        }else if(
            state.IsName("Base Layer.DoorRightOpenedIn") ||
            state.IsName("Base Layer.DoorRightOpenedOut"))
        {
            SetDoorOpen(false);
        }

        // Input during the opening/closing states is ignored.
    }

    private void SetDoorOpen(bool open)
    {
        // Only one opening direction can be active.
        animator.SetBool("DoorOpenIn", open && !openOutwards);
        animator.SetBool("DoorOpenOut", open && openOutwards);

        interactable.SetPromptText(open ? "[E] Close" : "[E] Open");
    }
}