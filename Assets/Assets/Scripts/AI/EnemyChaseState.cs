using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyController enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.Agent.isStopped = false;
    }

    public override void Update()
    {
        // Return to patrol ONLY after memory expires
        if (!enemy.Perception.CanSeePlayer)
        {
            stateMachine.ChangeState(
                new EnemyPatrolState(enemy, stateMachine)
            );
            return;
        }

        // Always follow player
        enemy.Agent.SetDestination(enemy.Player.position);

        // 🔥 YOUR CORE MECHANIC
        if (enemy.Perception.IsPlayerLookingAtEnemy)
        {
            enemy.Agent.isStopped = false;
        }
        else
        {
            enemy.Agent.isStopped = true;
        }
    }
}