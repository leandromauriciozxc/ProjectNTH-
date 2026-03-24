using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance { get; private set; }

    public Transform Transform => transform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}