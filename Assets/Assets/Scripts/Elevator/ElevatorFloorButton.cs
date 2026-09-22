using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectNTH.Elevators
{
    /// <summary>Wire Interactable.onInteract to Press; the setup menu does this automatically.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Interactable), typeof(BoxCollider))]
    public sealed class ElevatorFloorButton : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum ButtonAction { SelectFloor, OpenDoors, CloseDoors }

        [SerializeField] private ElevatorController elevator;
        [SerializeField] private ButtonAction action;
        [Tooltip("Only used for Select Floor. Use the floor's array index, starting at 0.")]
        [Min(0), SerializeField] private int floorIndex;
        // Retain the old serialized field so existing hallway OPEN buttons migrate correctly.
        [SerializeField, HideInInspector] private bool openDoorsOnly;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;
        [SerializeField] private Color idleColor = new Color(0.12f, 0.14f, 0.16f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.22f, 0.48f, 0.38f, 1f);

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
            if (elevator == null) return;
            switch (action)
            {
                case ButtonAction.OpenDoors: elevator.OpenDoors(); break;
                case ButtonAction.CloseDoors: elevator.CloseDoors(); break;
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
            if (label != null) label.text = isFloor ? elevator.GetFloorLabel(floorIndex) : opens ? "OPEN" : "CLOSE";
            bool blocked = isFloor ? elevator.IsBusy : !elevator.CanOperateDoors;
            GetComponent<Interactable>().SetPromptText(blocked ? "Elevator busy"
                : isFloor ? "[E] Floor " + elevator.GetFloorLabel(floorIndex)
                : opens ? "[E] Open doors" : "[E] Close doors");
            bool highlighted = isFloor ? elevator.TargetFloorIndex == floorIndex
                : opens ? elevator.State == ElevatorController.RideState.Opening
                : elevator.State == ElevatorController.RideState.Closing;
            if (background != null)
                background.color = highlighted ? selectedColor : idleColor;
        }
    }
}
