using UnityEngine;
using UnityEngine.Events;

namespace ProjectNTH.Looping
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/Looping/Maze Loop Story Trigger")]
    public sealed class MazeLoopStoryTrigger : MonoBehaviour
    {
        [SerializeField] private MazeLoopController controller;
        [Tooltip("Volume size in metres. Centre this object inside production. Scale is ignored; rotate around Y only.")]
        [SerializeField] private Vector3 size = new Vector3(3f, 3f, 3f);
        [Tooltip("For a dialogue/cutscene, turn this off and call BeginLoop from the story when the reminder finishes.")]
        [SerializeField] private bool beginLoopOnEntry = true;
        [SerializeField] private UnityEvent onFirstEntry = new UnityEvent();

        private bool fired;
        private bool wasInside;
        private bool hasSample;
        private int sampledPositionVersion;

        private void LateUpdate()
        {
            if (fired || controller == null || controller.Player == null ||
                !controller.isActiveAndEnabled || !controller.Player.gameObject.activeInHierarchy ||
                controller.IsTransitioning)
                return;

            Quaternion inverseYaw = Quaternion.Inverse(Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
            Vector3 local = inverseYaw * (controller.TrackingPosition - transform.position);
            bool inside = new Bounds(Vector3.zero, size).Contains(local);

            // Entering by teleport is not a new story beat. Starting the game inside is allowed.
            if (hasSample && sampledPositionVersion != controller.PositionVersion)
            {
                sampledPositionVersion = controller.PositionVersion;
                wasInside = inside;
                return;
            }

            sampledPositionVersion = controller.PositionVersion;
            hasSample = true;
            bool entered = inside && !wasInside;
            wasInside = inside;
            if (!entered)
                return;

            fired = true;
            if (beginLoopOnEntry)
                controller.BeginLoop();
            onFirstEntry.Invoke();
        }

        public void ResetTrigger()
        {
            // The player must leave and enter again after a reset.
            fired = false;
        }

        private void OnValidate()
        {
            size.x = Mathf.Max(0.1f, size.x);
            size.y = Mathf.Max(0.1f, size.y);
            size.z = Mathf.Max(0.1f, size.z);
        }

        private void OnDrawGizmos()
        {
            Matrix4x4 original = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position,
                Quaternion.Euler(0f, transform.eulerAngles.y, 0f), Vector3.one);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = original;
        }
    }
}
