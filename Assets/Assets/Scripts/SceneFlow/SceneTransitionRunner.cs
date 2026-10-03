using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectNTH.SceneFlow
{
    /// <summary>Owns the temporary overlay so unloading the trigger's scene cannot cancel the fade.</summary>
    [AddComponentMenu("")]
    public sealed class SceneTransitionRunner : MonoBehaviour
    {
        private static SceneTransitionRunner active;
        private CanvasGroup fade;
        private readonly Dictionary<Behaviour, bool> previousStates = new Dictionary<Behaviour, bool>();
        private bool loadingSettingsOverridden;
        private ThreadPriority previousLoadingPriority;
        private int previousTargetFrameRate;
        private int previousVSyncCount;
        public static bool IsRunning => active != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => active = null;

        internal static void Begin(string scene, float fadeOut, float fadeIn, float minimumBlack,
            float settle, Behaviour[] suspend, bool prioritizeLoading, bool logTimings)
        {
            if (IsRunning) return;
            var overlay = new GameObject("Scene Transition Overlay", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            overlay.layer = 5;
            DontDestroyOnLoad(overlay);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var runner = overlay.AddComponent<SceneTransitionRunner>();
            active = runner;
            runner.fade = overlay.GetComponent<CanvasGroup>();
            runner.fade.alpha = 0f;
            runner.fade.blocksRaycasts = true;
            runner.fade.interactable = true;
            var black = new GameObject("Black", typeof(RectTransform), typeof(Image));
            black.layer = 5;
            black.transform.SetParent(overlay.transform, false);
            var rect = black.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            black.GetComponent<Image>().color = Color.black;
            if (suspend != null)
                foreach (var behaviour in suspend)
                {
                    if (behaviour == null || runner.previousStates.ContainsKey(behaviour)) continue;
                    runner.previousStates.Add(behaviour, behaviour.enabled);
                    behaviour.enabled = false;
                }
            runner.StartCoroutine(runner.Transition(scene, fadeOut, fadeIn, minimumBlack, settle,
                prioritizeLoading, logTimings));
        }

        private IEnumerator Transition(string scene, float fadeOut, float fadeIn, float minimumBlack,
            float settle, bool prioritizeLoading, bool logTimings)
        {
            double transitionStarted = Time.realtimeSinceStartupAsDouble;
            try
            {
                yield return FadeTo(1f, fadeOut);
                double blackStarted = Time.realtimeSinceStartupAsDouble;
                // Present a fully black frame before loading/activation can occupy the main thread.
                yield return null;
                // Favor throughput only after the fade is complete. Visible animation keeps its normal frame pacing.
                if (prioritizeLoading) BoostLoading();
                double loadStarted = Time.realtimeSinceStartupAsDouble;
                AsyncOperation operation = null;
                try
                {
                    operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Scene transition failed to load '" + scene + "': " + exception.Message);
                }
                if (operation != null)
                {
                    // Activation proceeds normally while the persistent overlay stays opaque.
                    yield return operation;
                }
                double loadSeconds = Time.realtimeSinceStartupAsDouble - loadStarted;
                RestoreLoadingSettings();
                if (operation != null)
                {
                    // sceneLoaded precedes Start. Allow the destination to initialize under black.
                    yield return null;
                    if (settle > 0f) yield return new WaitForSecondsRealtime(settle);
                }
                float remaining = Mathf.Max(0f, minimumBlack) - (float)(Time.realtimeSinceStartupAsDouble - blackStarted);
                if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);
                double blackSeconds = Time.realtimeSinceStartupAsDouble - blackStarted;
                yield return FadeTo(0f, fadeIn);
                if (logTimings && operation != null)
                    Debug.Log($"[Scene Transition] '{scene}': load + activation {loadSeconds:F2}s; "
                        + $"fully black {blackSeconds:F2}s; total {Time.realtimeSinceStartupAsDouble - transitionStarted:F2}s. "
                        + $"Loading boost: {(prioritizeLoading ? "on" : "off")}.");
            }
            finally
            {
                Release();
                Destroy(gameObject);
            }
        }

        private void BoostLoading()
        {
            previousLoadingPriority = Application.backgroundLoadingPriority;
            previousTargetFrameRate = Application.targetFrameRate;
            previousVSyncCount = QualitySettings.vSyncCount;
            loadingSettingsOverridden = true;
            // In a Player, High allows up to 50 ms/frame for async asset integration.
            Application.backgroundLoadingPriority = ThreadPriority.High;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }

        private void RestoreLoadingSettings()
        {
            if (!loadingSettingsOverridden) return;
            loadingSettingsOverridden = false;
            Application.backgroundLoadingPriority = previousLoadingPriority;
            QualitySettings.vSyncCount = previousVSyncCount;
            Application.targetFrameRate = previousTargetFrameRate;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float from = fade.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                fade.alpha = Mathf.Lerp(from, target, progress);
                yield return null;
            }
            fade.alpha = target;
        }

        private void Release()
        {
            // Also restore settings if loading fails or the temporary runner is destroyed.
            RestoreLoadingSettings();
            if (fade != null)
            {
                fade.alpha = 0f;
                fade.blocksRaycasts = false;
            }
            foreach (var pair in previousStates)
                if (pair.Key != null) pair.Key.enabled = pair.Value;
            previousStates.Clear();
            if (active == this) active = null;
        }

        private void OnDestroy() => Release();
    }
}
