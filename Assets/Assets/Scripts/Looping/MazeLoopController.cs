using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectNTH.Looping
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/Looping/Maze Loop Controller")]
    public sealed class MazeLoopController : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private CharacterController player;
        [Tooltip("Enabled scripts under the player are paused automatically during a transition. Add any movement, camera or interaction scripts located elsewhere here.")]
        [SerializeField] private Behaviour[] additionalScriptsToPause = new Behaviour[0];

        [Header("Story")]
        [Tooltip("Leave off for the story. Call BeginLoop after the medicine reminder.")]
        [SerializeField] private bool beginOnPlay;

        [Header("Transition")]
        [Tooltip("Creates a temporary overlay at runtime. Turn off only when both sides of the doorway hide the relocation.")]
        [SerializeField] private bool fadeToBlack = true;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.12f;
        [SerializeField, Min(0f)] private float blackHoldSeconds = 0.05f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 0.18f;

        [Header("Events")]
        [SerializeField] private UnityEvent onLoopStarted = new UnityEvent();
        [Tooltip("Called after relocation, while the screen is covered. The integer is the completed passage count, starting at 1. Room variations can be added here later.")]
        [SerializeField] private UnityEvent<int> onPassageCompleted = new UnityEvent<int>();
        [SerializeField] private UnityEvent onLoopEnded = new UnityEvent();

        private readonly List<Behaviour> pausedScripts = new List<Behaviour>();
        private Coroutine transition;
        private CanvasGroup fade;
        private int transitionGeneration;

        public CharacterController Player => player;
        public bool IsLooping { get; private set; }
        public bool IsTransitioning { get; private set; }
        public int CompletedPassages { get; private set; }
        public int SessionId { get; private set; }
        public int PositionVersion { get; private set; }
        public bool CanTraverse => isActiveAndEnabled && IsLooping && !IsTransitioning &&
            player != null && player.enabled && player.gameObject.activeInHierarchy;

        // Use the capsule centre for doorway checks, but move the complete player root.
        public Vector3 TrackingPosition => player.transform.TransformPoint(player.center);

        private void Start()
        {
            if (beginOnPlay)
                BeginLoop();
        }

        [ContextMenu("Begin Loop (Play Mode)")]
        public void BeginLoop()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || IsLooping)
                return;

            if (player == null)
            {
                Debug.LogError("Assign the active player's CharacterController to MazeLoopController.", this);
                return;
            }

            CompletedPassages = 0;
            SessionId++;
            PositionVersion++;
            IsLooping = true;
            onLoopStarted.Invoke();
        }

        [ContextMenu("End Loop (Play Mode)")]
        public void EndLoop()
        {
            if (!IsLooping && !IsTransitioning)
                return;

            IsLooping = false;
            CancelTransition();
            PositionVersion++;
            onLoopEnded.Invoke();
        }

        // Call this after an unrelated cutscene/respawn teleports the player.
        public void NotifyPlayerRepositioned()
        {
            PositionVersion++;
        }

        internal bool TryTraverse(MazeLoopGate gate)
        {
            if (!CanTraverse || gate == null || !gate.isActiveAndEnabled)
                return false;

            Transform destination = gate.CurrentDestination;
            if (destination == null)
            {
                Debug.LogWarning("This maze gate needs an assigned destination for its current passage.", gate);
                return false;
            }

            if (destination.IsChildOf(player.transform) || gate.transform.IsChildOf(player.transform))
            {
                Debug.LogError("Maze gates and destination markers must be outside the player hierarchy.", gate);
                return false;
            }

            // Mark busy before running any callbacks or coroutine code.
            IsTransitioning = true;
            int generation = ++transitionGeneration;
            Coroutine started = StartCoroutine(Traverse(gate, destination, generation));
            // With fading off, the coroutine can finish before StartCoroutine returns.
            if (IsTransitioning && transitionGeneration == generation)
                transition = started;
            return true;
        }

        private IEnumerator Traverse(MazeLoopGate gate, Transform destination, int generation)
        {
            try
            {
                PausePlayerScripts();
                if (fadeToBlack)
                {
                    EnsureFade();
                    yield return FadeTo(1f, fadeOutSeconds);
                }

                if (player == null || gate == null || destination == null ||
                    !IsLooping || generation != transitionGeneration)
                    yield break;

                // Map yaw and relative position between equally sized, upright doorways.
                // The camera's local pitch and the player's lateral offset are preserved.
                Quaternion turn = Quaternion.AngleAxis(
                    Mathf.DeltaAngle(gate.transform.eulerAngles.y, destination.eulerAngles.y), Vector3.up);
                Vector3 arrival = destination.position + turn * (player.transform.position - gate.transform.position);
                Quaternion facing = turn * player.transform.rotation;

                bool controllerWasEnabled = player.enabled;
                try
                {
                    player.enabled = false;
                    player.transform.SetPositionAndRotation(arrival, facing);
                }
                finally
                {
                    if (player != null)
                        player.enabled = controllerWasEnabled;
                }

                Physics.SyncTransforms();
                PositionVersion++;
                gate.AdvanceDestination();
                CompletedPassages++;
                onPassageCompleted.Invoke(CompletedPassages);

                if (!IsLooping || generation != transitionGeneration)
                    yield break;

                if (fadeToBlack)
                {
                    if (blackHoldSeconds > 0f)
                        yield return new WaitForSecondsRealtime(blackHoldSeconds);
                    yield return FadeTo(0f, fadeInSeconds);
                }
            }
            finally
            {
                if (generation == transitionGeneration)
                    RestoreTransitionState();
            }
        }

        private void PausePlayerScripts()
        {
            // Snapshot only enabled scripts, so intentionally disabled scripts stay disabled.
            foreach (MonoBehaviour script in player.GetComponentsInChildren<MonoBehaviour>(true))
                PauseScript(script);
            foreach (Behaviour script in additionalScriptsToPause)
                PauseScript(script);
        }

        private void PauseScript(Behaviour script)
        {
            if (script == null || script == this || !script.enabled || pausedScripts.Contains(script))
                return;
            pausedScripts.Add(script);
            script.enabled = false;
        }

        private void RestoreTransitionState()
        {
            foreach (Behaviour script in pausedScripts)
            {
                if (script != null)
                    script.enabled = true;
            }
            pausedScripts.Clear();
            if (fade != null)
                fade.alpha = 0f;
            IsTransitioning = false;
            transition = null;
        }

        private void CancelTransition()
        {
            transitionGeneration++;
            if (transition != null)
                StopCoroutine(transition);
            RestoreTransitionState();
        }

        private void EnsureFade()
        {
            if (fade != null)
                return;

            GameObject overlay = new GameObject("Maze transition fade", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            overlay.transform.SetParent(transform, false);
            Canvas canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            fade = overlay.GetComponent<CanvasGroup>();
            fade.alpha = 0f;
            fade.interactable = false;
            fade.blocksRaycasts = false;

            GameObject panel = new GameObject("Black", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(overlay.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = panel.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }

        private IEnumerator FadeTo(float target, float seconds)
        {
            float start = fade.alpha;
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                fade.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / seconds));
                yield return null;
            }
            fade.alpha = target;
        }

        private void OnDisable()
        {
            IsLooping = false;
            PositionVersion++;
            CancelTransition();
        }
    }
}
