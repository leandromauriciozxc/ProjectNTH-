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
    [SerializeField] float lightCheckRadius = 5f;
    [SerializeField] LayerMask lightMask;

    [SerializeField] float loseSightDelay = 2f;
    [SerializeField] private DetectionMode detectionMode;

    private EnemyController controller;
    private Transform player;
    
    float lastSeenTime;
    float timer;
    float interval = 0.2f;

    public bool CanSeePlayer { get; private set; }
    public bool IsPlayerLookingAtEnemy { get; private set; }
    public bool IsExposedToLight { get; private set; }
    private void Start()
    {
        
        Debug.Log(player == null ? "PLAYER IS NULL ❌" : "PLAYER OK ✔");
    }
    private void Awake()
    {
        controller = GetComponent<EnemyController>();
    }
    private void Update()
    {
        if (player == null)
            return;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;

            CheckVision();
            CheckIfPlayerLooking();
            CheckLight();
        }
    }
    void CheckLight()
    {
        Collider[] hits = Physics.OverlapSphere(
            eyePoint.position,
            lightCheckRadius,
            lightMask
        );

        IsExposedToLight = hits.Length > 0;
    }
    private void CheckVision()
    {
        if (player == null || eyePoint == null)
            return;

        Vector3 directionToPlayer =
            player.position - eyePoint.position;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        bool currentlySeeing = false;


        // =========================================
        // DISTANCE CHECK
        // =========================================

        if (distanceToPlayer <= viewDistance)
        {
            switch (detectionMode)
            {
                // ---------------------------------
                // AREA DETECTION
                // ---------------------------------

                case DetectionMode.AreaOnly:

                    currentlySeeing = true;

                    break;


                // ---------------------------------
                // LINE OF SIGHT
                // ---------------------------------

                case DetectionMode.LineOfSight:

                    Vector3 origin =
                        eyePoint.position +
                        eyePoint.forward * 0.1f;

                    if (Physics.Raycast(
                        origin,
                        directionToPlayer.normalized,
                        out RaycastHit hit,
                        distanceToPlayer,
                        obstructionMask))
                    {
                        currentlySeeing =
                            hit.transform.root.CompareTag("Player");
                    }

                    break;
            }
        }


        // =========================================
        // PLAYER MEMORY
        // =========================================

        if (currentlySeeing)
        {
            lastSeenTime = Time.time;
            CanSeePlayer = true;
        }
        else
        {
            CanSeePlayer =
                Time.time - lastSeenTime < loseSightDelay;
        }
    }

    void CheckIfPlayerLooking()
    {
        Vector3 dirToEnemy = (eyePoint.position - player.position).normalized;

        float dot = Vector3.Dot(player.forward, dirToEnemy);

        // tweak threshold
        IsPlayerLookingAtEnemy = dot > 0.75f;
    }
    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
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