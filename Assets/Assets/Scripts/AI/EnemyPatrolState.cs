using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    private Transform targetPoint;

    private float waitTimer;
    private float waitDuration = 2f;
    private bool waiting;

    public EnemyPatrolState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        waiting = false;
        waitTimer = 0f;

        enemy.Agent.isStopped = false;

        SetNextDestination();
    }

    public override void Update()
    {
        // =========================================
        // PLAYER DETECTED
        // =========================================

        if (enemy.Perception.CanSeePlayer)
        {
            stateMachine.ChangeState(
                new EnemyChaseState(enemy, stateMachine)
            );

            return;
        }


        // =========================================
        // WAITING AT PATROL POINT
        // =========================================

        if (waiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitDuration)
            {
                Debug.Log("WAIT FINISHED - GETTING NEXT POINT");

                waiting = false;
                waitTimer = 0f;

                SetNextDestination();
            }

            return;
        }


        // =========================================
        // REACHED PATROL POINT
        // =========================================

        if (!waiting && targetPoint != null && !enemy.Agent.pathPending &&
    (
        !enemy.Agent.hasPath ||
        enemy.Agent.remainingDistance <= 0.7f
    ))
        {
            Debug.Log(
                $"ARRIVED AT: {targetPoint.name} | " +
                $"Distance: {enemy.Agent.remainingDistance}"
            );

            waiting = true;
            waitTimer = 0f;

            enemy.Agent.isStopped = true;
        }
    }





    private void SetNextDestination()
    {
        if (enemy.Patrol == null)
        {
            Debug.LogError("EnemyPatrol is NULL.");
            return;
        }

        targetPoint = enemy.Patrol.GetNextPoint();

        if (targetPoint == null)
        {
            Debug.LogError("Patrol point is NULL.");
            return;
        }

        enemy.Agent.isStopped = false;

        bool accepted =
            enemy.Agent.SetDestination(targetPoint.position);

        Debug.Log(
            $"PATROL DESTINATION: {targetPoint.name}\n" +
            $"Position: {targetPoint.position}\n" +
            $"On NavMesh: {enemy.Agent.isOnNavMesh}\n" +
            $"Stopped: {enemy.Agent.isStopped}\n" +
            $"Accepted: {accepted}\n" +
            $"Path Pending: {enemy.Agent.pathPending}\n" +
            $"Has Path: {enemy.Agent.hasPath}\n" +
            $"Path Status: {enemy.Agent.pathStatus}"
        );
    }


    public override void Exit()
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.ResetPath();

        waiting = false;
        waitTimer = 0f;
    }
}