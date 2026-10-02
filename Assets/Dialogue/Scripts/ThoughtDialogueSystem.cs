using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectNTH.Dialogue
{
    /// <summary>A separate Yarn runner and passive subtitle presenter for the player's thoughts.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/Dialogue/Thought Dialogue System")]
    public sealed class ThoughtDialogueSystem : DialoguePresenterBase
    {
        [Header("Yarn")]
        [SerializeField] private YarnProject yarnProject;
        [Tooltip("Your normal conversation runner. If empty, finds the Dialogue System Controller in this scene.")]
        [SerializeField] private DialogueRunner conversationRunner;
        [Tooltip("Share story variables when both runners use the same Yarn Project.")]
        [SerializeField] private bool shareConversationVariables = true;

        [Header("Each thought line")]
        [Min(0f), SerializeField] private float fadeInDuration = 0.35f;
        [Tooltip("Minimum time each line remains fully visible, excluding fades.")]
        [Min(0f), SerializeField] private float holdDuration = 3f;
        [Tooltip("Long lines get extra reading time. Set to 0 to use only Hold Duration.")]
        [Min(0f), SerializeField] private float secondsPerWord = 0.3f;
        [Min(0f), SerializeField] private float fadeOutDuration = 0.35f;
        [Min(0f), SerializeField] private float gapBetweenLines = 0.2f;

        [Header("Bottom subtitle")]
        [SerializeField] private TMP_FontAsset font;
        [Min(16f), SerializeField] private float fontSize = 38f;
        [Min(200f), SerializeField] private float maximumWidth = 1100f;
        [Min(0f), SerializeField] private float bottomOffset = 72f;
        [SerializeField] private Color textColor = new Color(0.95f, 0.94f, 0.90f, 1f);
        [SerializeField] private Color backgroundColor = new Color(0.035f, 0.04f, 0.045f, 0.8f);
        [Tooltip("Keep below pause menus and screen fades.")]
        [SerializeField] private int sortingOrder;

        private readonly Queue<string> pending = new Queue<string>();
        private DialogueRunner thoughtRunner;
        private bool processing;
        private int revision;
        private GameObject overlay;
        private Canvas canvas;
        private RectTransform panel;
        private TMP_Text lineText;
        private CanvasGroup group;
        private Vector2 lastCanvasSize;
        private Rect lastSafeArea;

        public YarnProject YarnProject => yarnProject != null ? yarnProject : conversationRunner != null ? conversationRunner.YarnProject : null;
        public string CurrentNode { get; private set; }
        public string CurrentThought { get; private set; } = string.Empty;
        public int QueuedCount => pending.Count;
        public bool IsConversationRunning => conversationRunner != null && conversationRunner.IsDialogueRunning;

        /// <summary>UnityEvent / Yarn entry point. Returns immediately and never captures player input.</summary>
        [YarnCommand("thought")]
        public void PlayThought(string nodeName) => TryPlayThought(nodeName);

        /// <summary>Returns false for a missing project/node or disabled system. Duplicate requests are coalesced.</summary>
        public bool TryPlayThought(string nodeName)
        {
            if (!isActiveAndEnabled || string.IsNullOrWhiteSpace(nodeName)) return false;
            ResolveConversation();
            var project = YarnProject;
            if (project == null || project.Program == null || Array.IndexOf(project.NodeNames, nodeName) < 0)
            {
                Debug.LogWarning($"Thought '{nodeName}' was not queued. Assign a Yarn Project containing that node.", this);
                return false;
            }
            if (nodeName == CurrentNode || pending.Contains(nodeName)) return true;
            pending.Enqueue(nodeName);
            return true;
        }

        public void ClearThoughts()
        {
            revision++;
            pending.Clear();
            CurrentNode = null;
            HideLine();
            if (thoughtRunner != null)
            {
                if (thoughtRunner.IsDialogueRunning) thoughtRunner.Stop().Forget();
                // A cancelled Yarn line can still be unwinding. Retire its runner so that
                // a new request cannot share its cancellation state.
                Destroy(thoughtRunner.gameObject);
                thoughtRunner = null;
            }
        }

        private void OnEnable()
        {
            ResolveConversation();
            CreateUI();
        }

        private void OnDisable()
        {
            ClearThoughts();
            if (overlay != null)
            {
                overlay.SetActive(false);
                Destroy(overlay);
            }
            overlay = null;
            group = null;
        }

        private void Update()
        {
            if (canvas != null && (lastCanvasSize != ((RectTransform)canvas.transform).rect.size || lastSafeArea != Screen.safeArea))
                RefreshLayout();
            if (!processing && pending.Count > 0 && !IsConversationRunning) ProcessQueue().Forget();
            // Hide immediately, including when a normal conversation starts between two async frames.
            if (IsConversationRunning && group != null) group.alpha = 0f;
        }

        private void ResolveConversation()
        {
            if (conversationRunner != null) return;
            foreach (var controller in FindObjectsOfType<DialogueSystemController>(true))
                if (controller.gameObject.scene == gameObject.scene && controller.DialogueRunner != null)
                {
                    conversationRunner = controller.DialogueRunner;
                    break;
                }
        }

        private void PrepareRunner()
        {
            if (thoughtRunner == null)
            {
                var child = new GameObject("Thought Yarn Runner");
                child.SetActive(false);
                child.transform.SetParent(transform, false);
                thoughtRunner = child.AddComponent<DialogueRunner>();
                thoughtRunner.autoStart = false;
                thoughtRunner.DialoguePresenters = new DialoguePresenterBase[] { this };
                child.SetActive(true);
            }
            if (shareConversationVariables && conversationRunner != null && conversationRunner.YarnProject == YarnProject)
                thoughtRunner.VariableStorage = conversationRunner.VariableStorage;
            thoughtRunner.SetProject(YarnProject);
        }

        private async YarnTask ProcessQueue()
        {
            processing = true;
            int runRevision = revision;
            try
            {
                while (this != null && isActiveAndEnabled && runRevision == revision && pending.Count > 0)
                {
                    while (this != null && IsConversationRunning && runRevision == revision && isActiveAndEnabled) await YarnTask.Yield();
                    if (this == null || runRevision != revision || !isActiveAndEnabled) break;
                    PrepareRunner();
                    CurrentNode = pending.Dequeue();
                    var runner = thoughtRunner;
                    await runner.StartDialogue(CurrentNode);
                    if (runner != null) await runner.DialogueTask;
                    CurrentNode = null;
                    // Let Yarn finish its completion callbacks before starting another node.
                    await YarnTask.Yield();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                if (thoughtRunner != null && thoughtRunner.IsDialogueRunning) thoughtRunner.Stop().Forget();
            }
            finally
            {
                CurrentNode = null;
                processing = false;
            }
        }

        public override YarnTask OnDialogueStartedAsync() { HideLine(); return YarnTask.CompletedTask; }
        public override YarnTask OnDialogueCompleteAsync() { HideLine(); return YarnTask.CompletedTask; }

        public override YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] options, LineCancellationToken token)
        {
            Debug.LogWarning("Thoughts cannot ask the player to choose. Options were skipped; use ordinary lines and <<if>> conditions.", this);
            // Our private runner uses Yarn's default option fallthrough; never select an answer for the player.
            return YarnTask<DialogueOption>.FromResult(null);
        }

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            int lineRevision = revision;
            if (Cancelled(lineRevision, token)) return;
            string message = line.TextWithoutCharacterName.Text;
            if (string.IsNullOrWhiteSpace(message)) return;
            CurrentThought = message;
            lineText.text = message;
            RefreshLayout();
            int wordCount = message.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
            float fadeIn = Mathf.Max(0f, fadeInDuration);
            float hold = Mathf.Max(0f, holdDuration, wordCount * Mathf.Max(0f, secondsPerWord));
            float fadeOut = Mathf.Max(0f, fadeOutDuration);
            float total = fadeIn + hold + fadeOut;
            float elapsed = 0f;
            float previousTime = Time.unscaledTime;
            group.alpha = 0f;
            while (!Cancelled(lineRevision, token) && elapsed < total)
            {
                if (IsConversationRunning)
                {
                    group.alpha = 0f;
                    // Resume with a fresh fade and full reading time once the conversation ends.
                    elapsed = 0f;
                }
                else
                {
                    if (elapsed < fadeIn) group.alpha = Ease(elapsed / fadeIn);
                    else if (elapsed < fadeIn + hold) group.alpha = 1f;
                    else group.alpha = fadeOut > 0f ? 1f - Ease((elapsed - fadeIn - hold) / fadeOut) : 0f;
                    elapsed += Mathf.Max(0f, Time.unscaledTime - previousTime);
                }
                previousTime = Time.unscaledTime;
                await YarnTask.Yield();
            }
            if (lineRevision != revision || this == null) return;
            HideLine();
            float end = Time.unscaledTime + Mathf.Max(0f, gapBetweenLines);
            while (!Cancelled(lineRevision, token) && Time.unscaledTime < end) await YarnTask.Yield();
        }

        private bool Cancelled(int expectedRevision, LineCancellationToken token) =>
            this == null || !isActiveAndEnabled || revision != expectedRevision || token.IsNextContentRequested || group == null;

        private static float Ease(float value) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(value));

        private void HideLine()
        {
            CurrentThought = string.Empty;
            if (group != null) group.alpha = 0f;
        }

        private void CreateUI()
        {
            overlay = new GameObject("Thought Dialogue Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            SceneManager.MoveGameObjectToScene(overlay, gameObject.scene);
            canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            panel = new GameObject("Thought", typeof(RectTransform), typeof(CanvasGroup), typeof(Image)).GetComponent<RectTransform>();
            panel.SetParent(overlay.transform, false);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0f);
            panel.pivot = new Vector2(0.5f, 0f);
            group = panel.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            var background = panel.GetComponent<Image>();
            background.color = backgroundColor;
            background.raycastTarget = false;
            lineText = new GameObject("Thought Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
            lineText.transform.SetParent(panel, false);
            lineText.font = font != null ? font : TMP_Settings.defaultFontAsset;
            lineText.fontSize = fontSize;
            lineText.color = textColor;
            lineText.alignment = TextAlignmentOptions.Center;
            lineText.enableWordWrapping = true;
            lineText.richText = false;
            lineText.raycastTarget = false;
            lineText.text = string.Empty;
            var textRect = lineText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 14f);
            textRect.offsetMax = new Vector2(-24f, -14f);
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            if (canvas == null || lineText == null) return;
            lastCanvasSize = ((RectTransform)canvas.transform).rect.size;
            lastSafeArea = Screen.safeArea;
            float scale = Mathf.Max(0.001f, canvas.scaleFactor);
            Rect safe = lastSafeArea;
            float width = Mathf.Max(64f, Mathf.Min(maximumWidth, safe.width / scale - 80f));
            var preferred = lineText.GetPreferredValues(lineText.text, Mathf.Max(16f, width - 48f), Mathf.Infinity);
            panel.sizeDelta = new Vector2(Mathf.Min(width, preferred.x + 48f), preferred.y + 28f);
            panel.anchoredPosition = new Vector2((safe.center.x - Screen.width * 0.5f) / scale, safe.yMin / scale + bottomOffset);
        }
    }
}
