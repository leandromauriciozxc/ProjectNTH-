using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] float minStateDuration = 1f;
    float stateEnterTime;
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
        // ❗ prevent rapid switching
        if (Time.time - stateEnterTime < minStateDuration)
            return;

        currentState?.Exit();

        currentState = newState;
        currentState.Enter();

        stateEnterTime = Time.time;
    }
}