using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }
    //public Transform Player { get; private set; }
    public Transform PlayerCameraTransform { get; private set; }
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyPerception Perception { get; private set; }
    public EnemyPatrol Patrol { get; private set; }

    [Header("AI Settings")]
    [SerializeField] private bool isPatrolAllowed = true;

    public bool IsPatrolAllowed => isPatrolAllowed;
    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        StateMachine = GetComponent<EnemyStateMachine>();
        Perception = GetComponent<EnemyPerception>();
        Patrol = GetComponent<EnemyPatrol>();
       
    }
    public void ReturnFromChase()
    {
        if (IsPatrolAllowed)
        {
            StateMachine.ChangeState(
                new EnemyPatrolState(this, StateMachine)
            );
        }
        else
        {
            StateMachine.ChangeState(
                new EnemyIdleState(this, StateMachine)
            );
        }
    }
    void Start()
    {
        if (PlayerCamera.Instance != null)
        {
            PlayerCameraTransform = PlayerCamera.Instance.Transform;
            Perception.Initialize(PlayerCameraTransform);
        }
        if (IsPatrolAllowed)
        {
            StateMachine.Initialize(
                new EnemyPatrolState(this, StateMachine)
            );
        }
        else
        {
            StateMachine.Initialize(
                new EnemyIdleState(this, StateMachine)
            );
        }
    }
    public enum MovementMode
    {
        Normal,
        LookBased,
        LightBased
    }

    public MovementMode movementMode;
}



