using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform eyePoint;
    [SerializeField] LayerMask obstructionMask;

    [Header("Vision Settings")]
    [SerializeField] float viewDistance = 10f;
    //[SerializeField] float viewAngle = 90f;

    [SerializeField] float loseSightDelay = 2f;
    private EnemyController controller;
    private Transform player;

    float lastSeenTime;
    float timer;
    float interval = 0.2f;

    public bool CanSeePlayer { get; private set; }
    public bool IsPlayerLookingAtEnemy { get; private set; }
    private void Start()
    {
        player = controller.PlayerCameraTransform;
    }
    private void Awake()
    {
        controller = GetComponent<EnemyController>();
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            CheckVision();
            CheckIfPlayerLooking();
        }
    }

    void CheckVision()
    {
        float distance = Vector3.Distance(eyePoint.position, player.position);

        bool currentlySeeing = distance <= viewDistance;

        if (currentlySeeing)
        {
            lastSeenTime = Time.time;
            CanSeePlayer = true;
        }
        else
        {
            // MEMORY
            CanSeePlayer = (Time.time - lastSeenTime) < loseSightDelay;
        }
        Debug.Log("CanSeePlayer: " + CanSeePlayer);
    }

    void CheckIfPlayerLooking()
    {
        Vector3 dirToEnemy = (eyePoint.position - player.position).normalized;

        float dot = Vector3.Dot(player.forward, dirToEnemy);

        // tweak threshold
        IsPlayerLookingAtEnemy = dot > 0.75f;
    }
    void OnDrawGizmos()
    {
        Transform debugPlayer = player;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var p = FindObjectOfType<PlayerCamera>();
            if (p != null)
                debugPlayer = p.transform;
        }
#endif

        if (eyePoint == null || debugPlayer == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePoint.position, debugPlayer.position);
    }
}