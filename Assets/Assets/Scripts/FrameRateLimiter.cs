using UnityEngine;

/// <summary>
/// Applies the saved FPS target (60 by default) when Play Mode or a build starts.
/// No GameObject or scene setup is required.
/// </summary>
public static class FrameRateLimiter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyLimit()
    {
        // On desktop, VSync overrides Application.targetFrameRate unless disabled.
        ProjectNTH.Settings.GameSettings.ApplyFrameRate();
    }
}
