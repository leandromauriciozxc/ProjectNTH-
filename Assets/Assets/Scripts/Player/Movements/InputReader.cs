using UnityEngine;

public class InputReader : MonoBehaviour
{
    PlayerInputActions input;
    private int suppressThroughFrame;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Run { get; private set; }
    public bool Lookback { get; private set; }

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Enable();
        ClearInput();
        suppressThroughFrame = Time.frameCount;
    }

    void OnDisable()
    {
        input.Disable();
        ClearInput();
    }

    void Update()
    {
        if (Time.frameCount <= suppressThroughFrame) return;
        Move = input.Player.Move.ReadValue<Vector2>();
        Look = input.Player.Look.ReadValue<Vector2>();
        Run = input.Player.Run.ReadValue<float>() > 0;
        Lookback = input.Player.LookBack.ReadValue<float>() > 0;
    }

    private void ClearInput()
    {
        Move = Look = Vector2.zero;
        Run = Lookback = false;
    }
}
