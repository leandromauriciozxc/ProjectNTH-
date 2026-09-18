using UnityEngine;

namespace ProjectNTH.Looping
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/Looping/Maze Loop Gate")]
    public sealed class MazeLoopGate : MonoBehaviour
    {
        [SerializeField] private MazeLoopController controller;
        [Tooltip("Width and height in metres, centred on this marker. Scale is ignored; rotate around Y only.")]
        [SerializeField] private Vector2 openingSize = new Vector2(2f, 3f);
        [Tooltip("Arrival markers, visited in order each time THIS gate is crossed. The list wraps forever. Match marker heights to this gate and point their blue arrow in the arrival travel direction.")]
        [SerializeField] private Transform[] destinations = new Transform[0];

        private Vector3 previousPosition;
        private bool hasSample;
        private int sampledPositionVersion;
        private int routeSession = -1;
        private int nextDestination;

        internal Transform CurrentDestination
        {
            get
            {
                RefreshSession();
                if (destinations == null || destinations.Length == 0)
                    return null;
                return destinations[nextDestination % destinations.Length];
            }
        }

        internal void AdvanceDestination()
        {
            if (destinations != null && destinations.Length > 0)
                nextDestination = (nextDestination + 1) % destinations.Length;
        }

        private void RefreshSession()
        {
            if (controller != null && routeSession != controller.SessionId)
            {
                routeSession = controller.SessionId;
                nextDestination = 0;
            }
        }

        private void LateUpdate()
        {
            if (controller == null || !controller.CanTraverse)
            {
                hasSample = false;
                return;
            }

            RefreshSession();
            Vector3 current = controller.TrackingPosition;
            if (!hasSample || sampledPositionVersion != controller.PositionVersion)
            {
                previousPosition = current;
                sampledPositionVersion = controller.PositionVersion;
                hasSample = true;
                return;
            }

            Quaternion inverseYaw = Quaternion.Inverse(Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
            Vector3 from = inverseYaw * (previousPosition - transform.position);
            Vector3 to = inverseYaw * (current - transform.position);
            previousPosition = current;

            if (CrossesOpening(from, to, openingSize))
                controller.TryTraverse(this);
        }

        // Test the swept movement segment, so sprinting across a thin gate cannot skip it.
        // Crossings are positional, not based on the camera or movement-facing direction.
        internal static bool CrossesOpening(Vector3 from, Vector3 to, Vector2 size)
        {
            if (from.z >= 0f || to.z < 0f || size.x <= 0f || size.y <= 0f)
                return false;

            float fraction = -from.z / (to.z - from.z);
            Vector3 crossing = Vector3.LerpUnclamped(from, to, fraction);
            return Mathf.Abs(crossing.x) <= size.x * 0.5f &&
                Mathf.Abs(crossing.y) <= size.y * 0.5f;
        }

        private void OnDisable()
        {
            hasSample = false;
        }

        private void OnValidate()
        {
            openingSize.x = Mathf.Max(0.1f, openingSize.x);
            openingSize.y = Mathf.Max(0.1f, openingSize.y);
        }

        private void OnDrawGizmos()
        {
            Matrix4x4 original = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position,
                Quaternion.Euler(0f, transform.eulerAngles.y, 0f), Vector3.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(openingSize.x, openingSize.y, 0.04f));
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
            Gizmos.DrawLine(Vector3.forward, new Vector3(-0.2f, 0f, 0.7f));
            Gizmos.DrawLine(Vector3.forward, new Vector3(0.2f, 0f, 0.7f));
            Gizmos.matrix = original;
        }

        private void OnDrawGizmosSelected()
        {
            if (destinations == null)
                return;
            Gizmos.color = Color.yellow;
            foreach (Transform destination in destinations)
            {
                if (destination == null)
                    continue;
                Gizmos.DrawLine(transform.position, destination.position);
                Gizmos.DrawWireSphere(destination.position, 0.2f);
                Gizmos.DrawRay(destination.position, destination.forward);
            }
        }
    }
}
