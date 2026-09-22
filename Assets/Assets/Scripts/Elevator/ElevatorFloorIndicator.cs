using TMPro;
using UnityEngine;

namespace ProjectNTH.Elevators
{
    [DisallowMultipleComponent]
    public sealed class ElevatorFloorIndicator : MonoBehaviour
    {
        [SerializeField] private ElevatorController elevator;
        [SerializeField] private TMP_Text floorText;
        [SerializeField] private TMP_Text directionText;

        private void OnEnable()
        {
            if (elevator != null) elevator.DisplayChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (elevator != null) elevator.DisplayChanged -= Refresh;
        }

        private void Refresh()
        {
            if (elevator == null) return;
            if (floorText != null) floorText.text = elevator.GetFloorLabel(elevator.CurrentFloorIndex);
            if (directionText == null) return;
            // The bundled font may not contain arrow glyphs. Keep the direction legible without new assets.
            char arrow = elevator.Direction > 0 ? '\u2191' : '\u2193';
            directionText.text = elevator.Direction == 0 ? string.Empty
                : directionText.font != null && directionText.font.HasCharacter(arrow)
                    ? arrow.ToString() : elevator.Direction > 0 ? "UP" : "DOWN";
        }
    }
}
