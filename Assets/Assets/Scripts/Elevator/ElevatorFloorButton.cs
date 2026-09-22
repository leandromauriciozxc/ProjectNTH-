using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectNTH.Elevators
{
    /// <summary>Wire Interactable.onInteract to Press; the setup menu does this automatically.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Interactable), typeof(BoxCollider))]
    public sealed class ElevatorFloorButton : MonoBehaviour
    {
        [SerializeField] private ElevatorController elevator;
        [Tooltip("Use the floor's array index, starting at 0.")]
        [Min(0), SerializeField] private int floorIndex;
        [SerializeField] private bool openDoorsOnly;
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
            if (openDoorsOnly) elevator.OpenDoors();
            else elevator.SelectFloor(floorIndex);
        }

        private void Refresh()
        {
            if (elevator == null) return;
            string floor = elevator.GetFloorLabel(floorIndex);
            if (label != null) label.text = openDoorsOnly ? "OPEN" : floor;
            GetComponent<Interactable>().SetPromptText(elevator.IsBusy ? "Elevator moving"
                : openDoorsOnly ? "[E] Open elevator" : "[E] Floor " + floor);
            if (background != null)
                background.color = !openDoorsOnly && elevator.TargetFloorIndex == floorIndex
                    ? selectedColor : idleColor;
        }
    }
}
