using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private EnemyBaseState currentState;

    public void Initialize(EnemyBaseState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(EnemyBaseState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}