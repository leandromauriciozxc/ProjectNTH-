using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

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
    private Transform fixedLookTarget;
    private bool isLooking;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (fixedLookTarget != null)
        {
            if (vcamDialogue != null && vcamDialogue.LookAt == fixedLookTarget)
                vcamDialogue.LookAt = null;

            Destroy(fixedLookTarget.gameObject);
        }
    }

    private void OnDisable()
    {
        StopLookAt();
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
        BeginLookAt(target, false);
    }

    public void LookAtFixed(Transform target)
    {
        BeginLookAt(target, true);
    }

    private void BeginLookAt(Transform target, bool holdInitialFocus)
    {
        if (target == null)
        {
            Debug.LogWarning("PlayerLookAt received a null target.", this);
            return;
        }

        if (vcamMain == null || vcamDialogue == null)
        {
            Debug.LogWarning("PlayerLookAt needs both the main and dialogue cameras assigned.", this);
            return;
        }

        if (holdInitialFocus)
        {
            if (fixedLookTarget == null)
            {
                var focusObject = new GameObject("Dialogue Focus (Runtime)");
                // Keep it outside animated/player hierarchies, but in the player's scene.
                SceneManager.MoveGameObjectToScene(focusObject, gameObject.scene);
                fixedLookTarget = focusObject.transform;
            }

            fixedLookTarget.SetPositionAndRotation(target.position, target.rotation);
            lookTarget = fixedLookTarget;
        }
        else
        {
            lookTarget = target;
        }

        isLooking = true;

        // Camera aim and player turning must use the same stable focus.
        vcamDialogue.LookAt = lookTarget;

        // Activate dialogue camera.
        vcamMain.Priority = mainPriority;
        vcamDialogue.Priority = dialoguePriority;
    }

    public void StopLookAt()
    {
        isLooking = false;

        // Return to normal gameplay camera.
        if (vcamDialogue != null)
            vcamDialogue.Priority = 0;
        if (vcamMain != null)
            vcamMain.Priority = mainPriority;

        lookTarget = null;

        // Keep the stable focus alive during the blend back. Reuse it next time
        // and destroy it with this component, rather than guessing a blend delay.
    }
}
