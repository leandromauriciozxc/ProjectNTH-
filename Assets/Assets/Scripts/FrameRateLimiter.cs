using UnityEngine;

/// <summary>
/// Applies the game's 60 FPS target automatically when Play Mode or a build starts.
/// No GameObject or scene setup is required.
/// </summary>
public static class FrameRateLimiter
{
    private const int TargetFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyLimit()
    {
        // On desktop, VSync overrides Application.targetFrameRate unless disabled.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
    }
}
