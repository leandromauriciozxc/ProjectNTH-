using UnityEngine;
public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyController enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Update()
    {
        enemy.Agent.SetDestination(enemy.Player.position);

        // YOUR UNIQUE MECHANIC (later refined)
        if (!enemy.Perception.IsPlayerLookingAtEnemy)
        {
            enemy.Agent.isStopped = true;
        }
        else
        {
            enemy.Agent.isStopped = false;
        }
    }
}