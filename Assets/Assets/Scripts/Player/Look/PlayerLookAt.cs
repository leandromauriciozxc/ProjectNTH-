using UnityEngine;
using Cinemachine;

public class PlayerLookAt : MonoBehaviour
{
    public static PlayerLookAt Instance { get; private set; }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera vcamMain;
    [SerializeField] private CinemachineVirtualCamera vcamDialogue;

    [Header("Settings")]
    [SerializeField] private float playerTurnSpeed = 6f;
    [SerializeField] private int mainPriority = 10;
    [SerializeField] private int dialoguePriority = 20;

    private Transform lookTarget;
    private bool isLooking;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!isLooking || lookTarget == null)
            return;

        RotatePlayerTowardsTarget();
    }

    private void RotatePlayerTowardsTarget()
    {
        Vector3 direction =
            lookTarget.position - transform.position;

        // Only rotate the Player horizontally.
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            playerTurnSpeed * Time.deltaTime
        );
    }

    public void LookAt(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("PlayerLookAt received a null target.");
            return;
        }

        lookTarget = target;
        isLooking = true;

        // Give the dialogue camera the target.
        vcamDialogue.LookAt = target;

        // Activate dialogue camera.
        vcamMain.Priority = mainPriority;
        vcamDialogue.Priority = dialoguePriority;
    }

    public void StopLookAt()
    {
        isLooking = false;

        // Return to normal gameplay camera.
        vcamDialogue.Priority = 0;
        vcamMain.Priority = mainPriority;

        lookTarget = null;

        // Keep LookAt assigned during the blend back.
        // You can clear it later if you want.
    }
}