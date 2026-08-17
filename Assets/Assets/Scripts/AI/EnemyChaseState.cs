using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(
        EnemyController enemy,
        EnemyStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
    }

    private bool canMove;

    public override void Enter()
    {
        canMove = false;

        enemy.Agent.isStopped = false;
    }

    public override void Update()
    {
        // =========================================
        // 1. PLAYER DETECTION
        // =========================================

        if (!enemy.Perception.CanSeePlayer)
        {
            enemy.ReturnFromChase();
            return;
        }


        // =========================================
        // 2. MOVEMENT PERMISSION
        // =========================================

        switch (enemy.movementMode)
        {
            case EnemyController.MovementMode.Normal:

                canMove = true;

                break;


            case EnemyController.MovementMode.LookBased:

                canMove =
                    enemy.Perception.IsPlayerLookingAtEnemy;

                break;


            case EnemyController.MovementMode.LightBased:

                canMove =
                    enemy.Perception.IsExposedToLight;

                break;
        }


        // =========================================
        // 3. MOVE OR FREEZE
        // =========================================

        if (canMove)
        {
            enemy.Agent.isStopped = false;

            enemy.Agent.SetDestination(
                enemy.PlayerCameraTransform.position
            );
        }
        else
        {
            enemy.Agent.isStopped = true;
        }
    }

    public override void Exit()
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.ResetPath();
    }
}