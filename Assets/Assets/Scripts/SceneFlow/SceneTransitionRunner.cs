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
        public static bool IsRunning => active != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => active = null;

        internal static void Begin(string scene, float fadeOut, float fadeIn, float minimumBlack,
            float settle, Behaviour[] suspend)
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
            runner.StartCoroutine(runner.Transition(scene, fadeOut, fadeIn, minimumBlack, settle));
        }

        private IEnumerator Transition(string scene, float fadeOut, float fadeIn, float minimumBlack, float settle)
        {
            try
            {
                yield return FadeTo(1f, fadeOut);
                float blackStarted = Time.realtimeSinceStartup;
                // Present a fully black frame before loading/activation can occupy the main thread.
                yield return null;
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
                    // sceneLoaded precedes Start. Allow the destination to initialize under black.
                    yield return null;
                    if (settle > 0f) yield return new WaitForSecondsRealtime(settle);
                }
                float remaining = Mathf.Max(0f, minimumBlack) - (Time.realtimeSinceStartup - blackStarted);
                if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);
                yield return FadeTo(0f, fadeIn);
            }
            finally
            {
                Release();
                Destroy(gameObject);
            }
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
