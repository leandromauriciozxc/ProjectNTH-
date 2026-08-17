using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private float minStateDuration = 0.5f;

    private float stateEnterTime;
    private EnemyBaseState currentState;

    public void Initialize(EnemyBaseState startingState)
    {
        if (startingState == null)
        {
            Debug.LogError("Starting state is null.");
            return;
        }

        currentState = startingState;
        stateEnterTime = Time.time;

        currentState.Enter();
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(EnemyBaseState newState, bool force = false)
    {
        if (newState == null)
            return;

        // Prevent rapid state switching,
        // unless this transition is important enough to force.
        if (!force &&
            Time.time - stateEnterTime < minStateDuration)
        {
            return;
        }

        Debug.Log(
            $"STATE CHANGE: " +
            $"{currentState?.GetType().Name} → " +
            $"{newState.GetType().Name}"
        );

        currentState?.Exit();

        currentState = newState;
        stateEnterTime = Time.time;

        currentState.Enter();
    }
}