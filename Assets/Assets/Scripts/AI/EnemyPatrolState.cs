using UnityEngine;
public class EnemyPatrolState : EnemyBaseState
{
    Transform targetPoint;

    public EnemyPatrolState(EnemyController enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        SetNextDestination();
    }

    public override void Update()
    {
        if (enemy.Perception.CanSeePlayer)
        {
            stateMachine.ChangeState(new EnemyChaseState(enemy, stateMachine));
            return;
        }

        if (!enemy.Agent.pathPending && enemy.Agent.remainingDistance < 0.5f)
        {
            SetNextDestination();
        }
    }

    void SetNextDestination()
    {
        targetPoint = enemy.Patrol.GetNextPoint();

        if (targetPoint != null)
            enemy.Agent.SetDestination(targetPoint.position);
    }
}