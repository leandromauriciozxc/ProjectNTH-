using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headPivot;

    [Header("Settings")]
    [SerializeField] private float lookDistance = 8f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float maxYaw = 55f;
    [SerializeField] private float maxPitch = 20f;

    private Transform player;
    private Quaternion defaultRotation;

    private void Start()
    {
        defaultRotation = headPivot.localRotation;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position - headPivot.position;

        if (direction.magnitude > lookDistance)
        {
            ReturnToDefault();
            return;
        }

        Vector3 localDirection =
            transform.InverseTransformDirection(direction.normalized);

        float yaw = Mathf.Atan2(
            localDirection.x,
            localDirection.z
        ) * Mathf.Rad2Deg;

        float pitch = -Mathf.Asin(
            localDirection.y
        ) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(
            yaw,
            -maxYaw,
            maxYaw
        );

        pitch = Mathf.Clamp(
            pitch,
            -maxPitch,
            maxPitch
        );

        Quaternion targetRotation =
            defaultRotation *
            Quaternion.Euler(pitch, yaw, 0f);

        headPivot.localRotation =
            Quaternion.Slerp(
                headPivot.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    private void ReturnToDefault()
    {
        headPivot.localRotation =
            Quaternion.Slerp(
                headPivot.localRotation,
                defaultRotation,
                rotationSpeed * Time.deltaTime
            );
    }
}