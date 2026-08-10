using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        enemy.Agent.isStopped = true;
    }

    public override void Update()
    {
        if (enemy.Perception.CanSeePlayer)
        {
            stateMachine.ChangeState(
                new EnemyChaseState(enemy, stateMachine)
            );
        }
    }

    public override void Exit()
    {
        enemy.Agent.isStopped = false;
    }
}