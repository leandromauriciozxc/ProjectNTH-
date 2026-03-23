using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    Transform targetPoint;

    float waitTimer;
    float waitDuration = 2f; // tweak in inspector later if you want
    bool waiting;

    public EnemyPatrolState(EnemyController enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        SetNextDestination();
        waiting = false;
    }

    public override void Update()
    {
        Debug.Log("Patrol Update Running");
        // 🔥 Transition to Chase
        if (enemy.Perception.CanSeePlayer)
        {
            stateMachine.ChangeState(
                new EnemyChaseState(enemy, stateMachine)
            );
            return;
        }

        if (waiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitDuration)
            {
                SetNextDestination();
                waiting = false;
            }

            return;
        }

        // Reached point
        if (!enemy.Agent.pathPending && enemy.Agent.remainingDistance < 0.5f)
        {
            waiting = true;
            waitTimer = 0f;

            enemy.Agent.isStopped = true;
        }
        Debug.Log("CanSeePlayer: " + enemy.Perception.CanSeePlayer);
    }

    void SetNextDestination()
    {
        targetPoint = enemy.Patrol.GetNextPoint();

        if (targetPoint != null)
        {
            enemy.Agent.isStopped = false;
            enemy.Agent.SetDestination(targetPoint.position);
        }
    }
}