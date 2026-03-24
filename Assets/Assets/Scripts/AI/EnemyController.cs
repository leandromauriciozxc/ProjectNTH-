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

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        StateMachine = GetComponent<EnemyStateMachine>();
        Perception = GetComponent<EnemyPerception>();
        Patrol = GetComponent<EnemyPatrol>();
       // Player = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerCameraTransform = PlayerCamera.Instance.transform;
    }

    void Start()
    {
        StateMachine.Initialize(new EnemyPatrolState(this, StateMachine));
    }
}