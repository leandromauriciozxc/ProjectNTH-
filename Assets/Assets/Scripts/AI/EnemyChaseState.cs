using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyController enemy, EnemyStateMachine stateMachine)
        : base(enemy, stateMachine) { }
    bool canMove = false;

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
        enemy.Agent.SetDestination(enemy.PlayerCameraTransform.position);

        //YOUR CORE MECHANIC
        switch (enemy.movementMode)
        {
            case EnemyController.MovementMode.Normal:
                canMove = true;
                break;

            case EnemyController.MovementMode.LookBased:
                canMove = enemy.Perception.IsPlayerLookingAtEnemy;
                break;

            case EnemyController.MovementMode.LightBased:
                canMove = enemy.Perception.IsExposedToLight;
                break;
        }
        enemy.Agent.isStopped = !canMove;
    }
}