using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    public enum DetectionMode
    {
        AreaOnly,
        LineOfSight
    }

    [Header("References")]
    [SerializeField] Transform eyePoint;
    [SerializeField] LayerMask obstructionMask;

    [Header("Vision Settings")]
    [SerializeField] float viewDistance = 10f;

    [SerializeField] float loseSightDelay = 2f;
    [SerializeField] private DetectionMode detectionMode;
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
        Vector3 dirToPlayer = player.position - eyePoint.position;
        float distance = dirToPlayer.magnitude;

        bool currentlySeeing = false;

        if (distance <= viewDistance)
        {
            switch (detectionMode)
            {
                case DetectionMode.AreaOnly:
                    currentlySeeing = true;
                    break;

                case DetectionMode.LineOfSight:
                    Vector3 origin = eyePoint.position + eyePoint.forward * 0.1f;
                    Debug.Log(" out side raycast");
                    if (Physics.Raycast(
                        origin,
                        dirToPlayer.normalized,
                        out RaycastHit hit,
                        viewDistance,
                        obstructionMask
                    ))
                    {
                        Debug.Log("Hit out side if statement: " + hit.transform.name);
                        if (hit.transform.root.CompareTag("Player"))
                        {
                            Debug.Log("Hit: " + hit.transform.name);
                            currentlySeeing = true;
                        }
                    }
                    break;
            }
        }

        // 🔥 KEEP YOUR MEMORY SYSTEM
        if (currentlySeeing)
        {
            lastSeenTime = Time.time;
            CanSeePlayer = true;
        }
        else
        {
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
        var debugPlayer = player;

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