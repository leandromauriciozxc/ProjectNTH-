using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectNTH.Elevators
{
    /// <summary>Wire Interactable.onInteract to Press; the setup menu does this automatically.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Interactable), typeof(BoxCollider))]
    public sealed class ElevatorFloorButton : MonoBehaviour, ISerializationCallbackReceiver, IInteractionFilter
    {
        public enum ButtonAction { SelectFloor = 0, OpenDoors = 1, CloseDoors = 2, CallUp = 3, CallDown = 4 }

        [SerializeField] private ElevatorController elevator;
        [SerializeField] private ButtonAction action;
        [Tooltip("Select Floor: destination. Call Up/Down: the hallway where this button is mounted. Ignored by door controls.")]
        [Min(0), SerializeField] private int floorIndex;
        // Retain the old serialized field so existing hallway OPEN buttons migrate correctly.
        [SerializeField, HideInInspector] private bool openDoorsOnly;
        [SerializeField] private TMP_Text label;
        [Tooltip("Turn off to keep your TMP text. When on, use the controller's floor name, OPEN/CLOSE or UP/DOWN.")]
        [SerializeField] private bool automaticLabel = true;
        [SerializeField] private Image background;
        [SerializeField] private Color idleColor = new Color(0.12f, 0.14f, 0.16f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.22f, 0.48f, 0.38f, 1f);

        [Header("Interaction side")]
        [SerializeField] private bool frontFaceOnly = true;
        [Tooltip("Canvas text faces local -Z. Use (0, 0, 1) if your custom button faces the opposite way.")]
        [SerializeField] private Vector3 frontDirection = Vector3.back;

        public ElevatorController Elevator => elevator;
        public int FloorIndex => floorIndex;
        public ButtonAction Action => action;

        public bool CanInteractFrom(Vector3 viewerPosition)
        {
            if (!isActiveAndEnabled) return false;
            if (!frontFaceOnly) return true;
            Vector3 front = frontDirection.sqrMagnitude > 0f ? frontDirection.normalized : Vector3.back;
            return Vector3.Dot(viewerPosition - transform.position, transform.TransformDirection(front)) > 0.001f;
        }

        private void OnEnable()
        {
            if (elevator != null) elevator.DisplayChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (elevator != null) elevator.DisplayChanged -= Refresh;
        }

        public void Press()
        {
            if (elevator == null || !isActiveAndEnabled) return;
            switch (action)
            {
                case ButtonAction.OpenDoors: elevator.OpenDoors(); break;
                case ButtonAction.CloseDoors: elevator.CloseDoors(); break;
                case ButtonAction.CallUp: elevator.CallToFloor(floorIndex, 1); break;
                case ButtonAction.CallDown: elevator.CallToFloor(floorIndex, -1); break;
                default: elevator.SelectFloor(floorIndex); break;
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (!openDoorsOnly) return;
            if (action == ButtonAction.SelectFloor) action = ButtonAction.OpenDoors;
            openDoorsOnly = false;
        }

        private void Refresh()
        {
            if (elevator == null) return;
            bool isFloor = action == ButtonAction.SelectFloor;
            bool opens = action == ButtonAction.OpenDoors;
            bool isCall = action == ButtonAction.CallUp || action == ButtonAction.CallDown;
            int callDirection = action == ButtonAction.CallUp ? 1 : -1;
            string automaticText = isFloor ? elevator.GetFloorLabel(floorIndex)
                : isCall ? (callDirection > 0 ? "UP" : "DOWN") : opens ? "OPEN" : "CLOSE";
            if (automaticLabel && label != null)
                label.text = automaticText;
            bool pending = isCall && elevator.IsHallwayCallPending(floorIndex, callDirection);
            bool blocked = isCall ? !elevator.IsValidHallwayCall(floorIndex, callDirection)
                : isFloor ? elevator.IsBusy : !elevator.CanOperateDoors;
            GetComponent<Interactable>().SetPromptText(blocked ? (isCall ? "No floor in this direction" : "Elevator busy")
                : pending ? "Elevator called"
                : isCall ? (callDirection > 0 ? "[E] Call elevator - up" : "[E] Call elevator - down")
                : isFloor ? "[E] Floor " + elevator.GetFloorLabel(floorIndex)
                : opens ? "[E] Open doors" : "[E] Close doors");
            bool highlighted = isCall ? pending : isFloor ? elevator.TargetFloorIndex == floorIndex
                : opens ? elevator.State == ElevatorController.RideState.Opening
                : elevator.State == ElevatorController.RideState.Closing;
            if (background != null)
                background.color = highlighted ? selectedColor : idleColor;
        }
    }
}
