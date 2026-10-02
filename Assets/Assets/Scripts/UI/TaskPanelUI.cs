using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectNTH.UI
{
    /// <summary>An animated task reminder with a cross-out and exit sequence on completion.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/UI/Task Panel UI")]
    public sealed class TaskPanelUI : MonoBehaviour
    {
        [Header("Current task")]
        [Tooltip("The task shown on entering the scene. Leave empty until a game event calls SetTask.")]
        [TextArea(2, 5), SerializeField] private string taskMessage = "Go to locker";
        [Tooltip("Open the message when this component is enabled. Otherwise start with the small TAB reminder.")]
        [SerializeField] private bool showOnEnable = true;

        [Header("Timing")]
        [Tooltip("Seconds the message stays fully open. Pressing Tab restarts this timer.")]
        [Min(0.1f), SerializeField] private float displayDuration = 4f;
        [Tooltip("Seconds for the opening/shrinking animation. Set to 0 for instant changes.")]
        [Min(0f), SerializeField] private float animationDuration = 0.3f;
        [Tooltip("Seconds for a first or new task to slide in from off-screen.")]
        [Min(0f), SerializeField] private float entranceDuration = 0.4f;

        [Header("Completion animation")]
        [Tooltip("Seconds to draw the cross-out across the message, including wrapped lines.")]
        [Min(0f), SerializeField] private float crossoutDuration = 0.45f;
        [Tooltip("Seconds to hold the crossed-out message before it leaves.")]
        [Min(0f), SerializeField] private float completionPause = 0.35f;
        [Tooltip("Seconds for the completed panel to retract off the left edge.")]
        [Min(0f), SerializeField] private float exitDuration = 0.3f;

        [Header("Position and size")]
        [Tooltip("Inset from the upper-left safe area, in 1920 x 1080 reference pixels.")]
        [SerializeField] private Vector2 screenOffset = new Vector2(0f, 132f);
        [Min(160f), SerializeField] private float minimumWidth = 450f;
        [Min(160f), SerializeField] private float maximumWidth = 800f;
        [Min(56f), SerializeField] private float panelHeight = 84f;
        [Tooltip("Overlay order. Keep below your pause menus and screen fades.")]
        [SerializeField] private int sortingOrder = 0;

        [Header("Appearance")]
        [Tooltip("Optional TextMesh Pro font. The supplied prefab uses your OldCupboard font.")]
        [SerializeField] private TMP_FontAsset font;
        [Min(16f), SerializeField] private float fontSize = 56f;
        [SerializeField] private Color backgroundColor = new Color(0.19f, 0.19f, 0.19f, 0.94f);
        [SerializeField] private Color borderColor = new Color(0.78f, 0.78f, 0.76f, 1f);
        [SerializeField] private Color textColor = new Color(0.9f, 0.89f, 0.85f, 1f);
        [SerializeField] private Color iconColor = new Color(1f, 0.91f, 0.12f, 1f);

        private const float CompactWidth = 128f;
        private const float TextInset = 112f;
        private const float RightPadding = 24f;
        private GameObject overlay;
        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform root;
        private RectTransform panel;
        private RectTransform textViewport;
        private RectTransform diamond;
        private TMP_Text messageText;
        private TMP_Text tabHint;
        private CanvasGroup messageGroup;
        private CanvasGroup hintGroup;
        private float expansion;
        private float animationFrom;
        private float animationTarget;
        private float animationStarted;
        private float collapseAt;
        private bool expanded;
        private bool layoutDirty = true;
        private Vector2 lastCanvasSize;
        private Rect lastSafeArea;
        private float expandedWidth;
        private float expandedHeight;
        private enum Phase { Hidden, Entering, Ready, CompletingReveal, CrossingOut, CompletionHold, Exiting }
        private Phase phase;
        private float phaseStarted;
        private float presence;
        private float completionRevealFrom;
        private float strikeProgress;
        private string nextTaskMessage = string.Empty;
        private Vector2 homePosition;
        private readonly List<RectTransform> strikeLines = new List<RectTransform>();
        private readonly List<float> strikeWidths = new List<float>();
        private float totalStrikeWidth;

        public string CurrentTask => taskMessage;
        public bool HasTask => !string.IsNullOrWhiteSpace(taskMessage);
        public bool IsExpanded => HasTask && expanded;
        public bool IsCompleting => phase == Phase.CompletingReveal || phase == Phase.CrossingOut
            || phase == Phase.CompletionHold || phase == Phase.Exiting;

        /// <summary>Animate a new task in. During completion, queue it until the old task has left.</summary>
        public void SetTask(string message)
        {
            if (IsCompleting)
            {
                nextTaskMessage = message ?? string.Empty;
                return;
            }
            taskMessage = message ?? string.Empty;
            layoutDirty = true;
            if (!isActiveAndEnabled) return;
            if (HasTask) BeginEntrance(true);
            else ClearTask();
        }

        /// <summary>Cross out and dismiss the current task, leaving no next task.</summary>
        public void CompleteTask() => CompleteAndShowNext(string.Empty);

        /// <summary>Cross out this task, slide it away, then animate in the supplied next instruction.</summary>
        public void CompleteAndShowNext(string nextMessage)
        {
            // Repeated trigger/interaction events cannot restart a completion already in progress.
            if (IsCompleting) return;
            if (!HasTask || !isActiveAndEnabled)
            {
                SetTask(nextMessage);
                return;
            }
            nextTaskMessage = nextMessage ?? string.Empty;
            completionRevealFrom = presence;
            SetExpanded(true);
            SetPhase(Phase.CompletingReveal);
        }

        /// <summary>Show the current message again and restart its display timer.</summary>
        public void Reveal()
        {
            if (!isActiveAndEnabled || !HasTask || IsCompleting) return;
            SetExpanded(true);
            collapseAt = Time.unscaledTime + Mathf.Max(0.1f, displayDuration)
                + (expansion < 1f ? Mathf.Max(0f, animationDuration) : 0f);
            root.gameObject.SetActive(true);
        }

        public void Collapse()
        {
            if (IsCompleting) return;
            SetExpanded(false);
        }

        /// <summary>Remove the current task, including the icon and Tab hint.</summary>
        public void ClearTask()
        {
            taskMessage = string.Empty;
            nextTaskMessage = string.Empty;
            phase = Phase.Hidden;
            presence = strikeProgress = 0f;
            ResetStrikeLines();
            expanded = false;
            expansion = animationFrom = animationTarget = 0f;
            layoutDirty = true;
            if (root != null) root.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            CreateUI();
            if (HasTask) BeginEntrance(showOnEnable);
            else ClearTask();
        }

        private void BeginEntrance(bool openMessage)
        {
            ResetStrikeLines();
            presence = strikeProgress = 0f;
            expanded = false;
            expansion = animationFrom = animationTarget = 0f;
            SetExpanded(openMessage);
            SetPhase(Phase.Entering);
            // Pose the whole UI outside the screen before activating it, avoiding a one-frame flash.
            RefreshLayout();
            ApplyAnimation();
            root.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!HasTask)
            {
                if (phase != Phase.Hidden) ClearTask();
                return;
            }
            if (phase == Phase.Hidden) BeginEntrance(showOnEnable);
            if (TabPressed() && !IsTyping()) Reveal();
            if (phase == Phase.Ready && expanded && Time.unscaledTime >= collapseAt) Collapse();
            float duration = Mathf.Max(0f, animationDuration);
            float t = duration == 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - animationStarted) / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            expansion = Mathf.Lerp(animationFrom, animationTarget, eased);
            if (layoutDirty || canvasRect.rect.size != lastCanvasSize || Screen.safeArea != lastSafeArea)
                RefreshLayout();
            AdvanceSequence();
            ApplyAnimation();
        }

        private void SetPhase(Phase value)
        {
            phase = value;
            phaseStarted = Time.unscaledTime;
        }

        private float PhaseProgress(float duration) => duration <= 0f ? 1f
            : Mathf.Clamp01((Time.unscaledTime - phaseStarted) / duration);

        private static float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private void AdvanceSequence()
        {
            // A bounded loop allows zero-duration settings to finish without extra blank frames.
            for (int step = 0; step < 6; step++)
            {
                switch (phase)
                {
                    case Phase.Entering:
                        presence = EaseOut(PhaseProgress(entranceDuration));
                        if (presence < 1f || (expanded && expansion < 1f)) return;
                        SetPhase(Phase.Ready);
                        collapseAt = Time.unscaledTime + Mathf.Max(0.1f, displayDuration);
                        return;
                    case Phase.CompletingReveal:
                        presence = Mathf.Lerp(completionRevealFrom, 1f, EaseOut(PhaseProgress(entranceDuration)));
                        if (presence < 1f || expansion < 1f) return;
                        ApplyAnimation();
                        BuildStrikeLines();
                        SetPhase(Phase.CrossingOut);
                        break;
                    case Phase.CrossingOut:
                        strikeProgress = PhaseProgress(crossoutDuration);
                        if (strikeProgress < 1f) return;
                        SetPhase(Phase.CompletionHold);
                        break;
                    case Phase.CompletionHold:
                        if (PhaseProgress(completionPause) < 1f) return;
                        SetPhase(Phase.Exiting);
                        break;
                    case Phase.Exiting:
                        float progress = PhaseProgress(exitDuration);
                        presence = 1f - progress * progress * progress;
                        if (progress < 1f) return;
                        string next = nextTaskMessage;
                        ClearTask();
                        if (!string.IsNullOrWhiteSpace(next)) SetTask(next);
                        return;
                    default:
                        return;
                }
            }
        }

        private void SetExpanded(bool value)
        {
            expanded = value;
            float target = value ? 1f : 0f;
            if (animationTarget == target) return;
            animationFrom = expansion;
            animationTarget = target;
            animationStarted = Time.unscaledTime;
        }

        private static bool TabPressed()
        {
            #if ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Keyboard.current != null
                && UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame;
            #elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Tab);
            #else
            return false;
            #endif
        }

        private static bool IsTyping()
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (selected == null) return false;
            var tmpInput = selected.GetComponentInParent<TMP_InputField>();
            if (tmpInput != null && tmpInput.isFocused) return true;
            var legacyInput = selected.GetComponentInParent<InputField>();
            return legacyInput != null && legacyInput.isFocused;
        }

        private void RefreshLayout()
        {
            lastCanvasSize = canvasRect.rect.size;
            lastSafeArea = Screen.safeArea;
            layoutDirty = false;
            canvas.sortingOrder = Mathf.Clamp(sortingOrder, short.MinValue, short.MaxValue);
            float scale = Mathf.Max(0.001f, canvas.scaleFactor);
            float left = lastSafeArea.xMin / scale + Mathf.Max(0f, screenOffset.x);
            float top = (Screen.height - lastSafeArea.yMax) / scale + Mathf.Max(0f, screenOffset.y);
            homePosition = new Vector2(left, -top);
            float availableWidth = Mathf.Max(160f, lastSafeArea.width / scale - Mathf.Max(0f, screenOffset.x) - 24f);
            float widthLimit = Mathf.Min(Mathf.Max(160f, maximumWidth), availableWidth);
            float widthFloor = Mathf.Min(Mathf.Max(160f, minimumWidth), widthLimit);
            messageText.text = taskMessage;
            messageText.font = tabHint.font = font != null ? font : TMP_Settings.defaultFontAsset;
            messageText.fontSize = Mathf.Max(16f, fontSize);
            tabHint.fontSize = Mathf.Max(16f, fontSize * 0.82f);
            messageText.color = tabHint.color = textColor;
            float preferredWidth = messageText.GetPreferredValues(taskMessage).x + TextInset + RightPadding;
            expandedWidth = Mathf.Clamp(preferredWidth, widthFloor, widthLimit);
            float textWidth = expandedWidth - TextInset - RightPadding;
            float textHeight = messageText.GetPreferredValues(taskMessage, textWidth, Mathf.Infinity).y;
            float availableHeight = Mathf.Max(panelHeight, lastSafeArea.height / scale - screenOffset.y - 24f);
            expandedHeight = Mathf.Min(Mathf.Max(panelHeight, textHeight + 28f), availableHeight);
            messageText.rectTransform.sizeDelta = new Vector2(textWidth, expandedHeight - 20f);
            messageText.rectTransform.anchoredPosition = new Vector2(0f, -10f);
            if (phase == Phase.CrossingOut || phase == Phase.CompletionHold || phase == Phase.Exiting)
                BuildStrikeLines();
        }

        private void ApplyAnimation()
        {
            float height = Mathf.Lerp(Mathf.Max(56f, panelHeight), expandedHeight, expansion);
            float width = Mathf.Lerp(CompactWidth, expandedWidth, expansion);
            // Include the safe-area inset and the TAB label so every part clears the left screen edge.
            float hiddenX = -Mathf.Max(expandedWidth, CompactWidth + 110f) - 24f;
            root.anchoredPosition = new Vector2(Mathf.Lerp(hiddenX, homePosition.x, presence), homePosition.y);
            panel.sizeDelta = new Vector2(width, height);
            textViewport.sizeDelta = new Vector2(Mathf.Max(0f, width - TextInset - RightPadding), height);
            diamond.anchoredPosition = new Vector2(72f, -height * 0.5f);
            tabHint.rectTransform.anchoredPosition = new Vector2(CompactWidth + 10f, -panelHeight * 0.5f);
            messageGroup.alpha = Mathf.Clamp01(expansion * 3f);
            hintGroup.alpha = phase == Phase.Ready ? 1f - Mathf.Clamp01(expansion * 5f) : 0f;
            ApplyStrikeAnimation();
        }

        private void ResetStrikeLines()
        {
            foreach (RectTransform line in strikeLines)
                if (line != null) line.gameObject.SetActive(false);
            strikeWidths.Clear();
            totalStrikeWidth = 0f;
        }

        private void BuildStrikeLines()
        {
            ResetStrikeLines();
            // Use the font's actual glyph positions, so the cross-out also follows wrapped text.
            messageText.ForceMeshUpdate();
            TMP_TextInfo info = messageText.textInfo;
            for (int i = 0; i < info.lineCount; i++)
            {
                TMP_LineInfo line = info.lineInfo[i];
                if (line.visibleCharacterCount == 0) continue;
                TMP_CharacterInfo first = info.characterInfo[line.firstVisibleCharacterIndex];
                TMP_CharacterInfo last = info.characterInfo[line.lastVisibleCharacterIndex];
                float width = Mathf.Max(0f, last.topRight.x - first.bottomLeft.x);
                if (width <= 0f) continue;
                int index = strikeWidths.Count;
                if (index == strikeLines.Count)
                {
                    RectTransform rect = NewRect("Completion cross-out " + (index + 1), messageText.transform, Vector2.zero);
                    rect.pivot = new Vector2(0f, 0.5f);
                    var image = rect.gameObject.AddComponent<Image>();
                    image.raycastTarget = false;
                    strikeLines.Add(rect);
                }
                RectTransform stroke = strikeLines[index];
                // OldCupboard's strike metric sits near the baseline. Center on the rendered
                // letter bodies instead, including its small caps and any replacement font.
                float y = 0f;
                int visibleGlyphs = 0;
                for (int character = line.firstVisibleCharacterIndex; character <= line.lastVisibleCharacterIndex; character++)
                {
                    TMP_CharacterInfo glyph = info.characterInfo[character];
                    if (!glyph.isVisible) continue;
                    y += (glyph.bottomLeft.y + glyph.topRight.y) * 0.5f;
                    visibleGlyphs++;
                }
                y /= Mathf.Max(1, visibleGlyphs);
                stroke.anchoredPosition = new Vector2(first.bottomLeft.x, y);
                stroke.GetComponent<Image>().color = textColor;
                strikeWidths.Add(width);
                totalStrikeWidth += width;
            }
            ApplyStrikeAnimation();
        }

        private void ApplyStrikeAnimation()
        {
            float remaining = strikeProgress * totalStrikeWidth;
            for (int i = 0; i < strikeWidths.Count; i++)
            {
                float length = Mathf.Clamp(remaining, 0f, strikeWidths[i]);
                strikeLines[i].sizeDelta = new Vector2(length, Mathf.Max(2f, fontSize * 0.05f));
                strikeLines[i].gameObject.SetActive(length > 0f);
                remaining -= strikeWidths[i];
            }
        }

        private void CreateUI()
        {
            if (overlay != null) return;
            overlay = new GameObject("Task Panel UI - " + name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            SceneManager.MoveGameObjectToScene(overlay, gameObject.scene);
            canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvasRect = (RectTransform)overlay.transform;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            MakePassive(overlay.GetComponent<CanvasGroup>());
            root = NewRect("Task reminder", canvasRect, Vector2.zero);
            panel = NewRect("Panel", root, new Vector2(CompactWidth, panelHeight));
            Image background = panel.gameObject.AddComponent<Image>();
            background.color = backgroundColor;
            background.raycastTarget = false;
            AddBorder("Top", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -2f));
            AddBorder("Bottom", Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 2f));
            AddBorder("Left", Vector2.zero, new Vector2(0f, 1f), new Vector2(2f, 0f));
            AddBorder("Right", new Vector2(1f, 0f), Vector2.one, new Vector2(-2f, 0f));
            diamond = NewRect("Task diamond", panel, new Vector2(30f, 30f));
            diamond.pivot = new Vector2(0.5f, 0.5f);
            // Standard UI images keep the small outlined diamond crisp at every Canvas scale.
            RectTransform outline = IconSquare("Outline", diamond, 20f, iconColor);
            outline.localRotation = Quaternion.Euler(0f, 0f, 45f);
            Color iconBackground = backgroundColor;
            iconBackground.a = 1f;
            IconSquare("Inset", outline, 14f, iconBackground);
            IconSquare("Center", outline, 5f, iconColor);
            textViewport = NewRect("Message clip", panel, Vector2.zero);
            textViewport.anchoredPosition = new Vector2(TextInset, 0f);
            textViewport.gameObject.AddComponent<RectMask2D>();
            messageGroup = textViewport.gameObject.AddComponent<CanvasGroup>();
            MakePassive(messageGroup);
            messageText = CreateText("Task message", textViewport, new Vector2(300f, panelHeight));
            tabHint = CreateText("TAB hint", root, new Vector2(100f, panelHeight));
            tabHint.rectTransform.pivot = new Vector2(0f, 0.5f);
            tabHint.text = "TAB";
            hintGroup = tabHint.gameObject.AddComponent<CanvasGroup>();
            MakePassive(hintGroup);
        }

        private static RectTransform IconSquare(string name, Transform parent, float size, Color color)
        {
            RectTransform rect = NewRect(name, parent, Vector2.one * size);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private void AddBorder(string name, Vector2 min, Vector2 max, Vector2 thickness)
        {
            RectTransform edge = NewRect(name + " border", panel, Vector2.zero);
            edge.anchorMin = min;
            edge.anchorMax = max;
            edge.offsetMin = new Vector2(Mathf.Min(0f, thickness.x), Mathf.Min(0f, thickness.y));
            edge.offsetMax = new Vector2(Mathf.Max(0f, thickness.x), Mathf.Max(0f, thickness.y));
            var image = edge.gameObject.AddComponent<Image>();
            image.color = borderColor;
            image.raycastTarget = false;
        }

        private TMP_Text CreateText(string name, Transform parent, Vector2 size)
        {
            var label = NewRect(name, parent, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font != null ? font : TMP_Settings.defaultFontAsset;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.richText = false;
            label.raycastTarget = false;
            return label;
        }

        private static void MakePassive(CanvasGroup group)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private static RectTransform NewRect(string name, Transform parent, Vector2 size)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            return rect;
        }

        private void OnValidate()
        {
            displayDuration = Mathf.Max(0.1f, displayDuration);
            animationDuration = Mathf.Max(0f, animationDuration);
            entranceDuration = Mathf.Max(0f, entranceDuration);
            crossoutDuration = Mathf.Max(0f, crossoutDuration);
            completionPause = Mathf.Max(0f, completionPause);
            exitDuration = Mathf.Max(0f, exitDuration);
            panelHeight = Mathf.Max(56f, panelHeight);
            maximumWidth = Mathf.Max(160f, maximumWidth);
            minimumWidth = Mathf.Clamp(minimumWidth, 160f, maximumWidth);
            layoutDirty = true;
        }

        private void OnDisable()
        {
            // Completion has already been accepted. A cutscene disabling the HUD must not revive it.
            if (IsCompleting) taskMessage = nextTaskMessage;
            nextTaskMessage = string.Empty;
            phase = Phase.Hidden;
            strikeLines.Clear();
            strikeWidths.Clear();
            if (overlay != null)
            {
                overlay.SetActive(false);
                Destroy(overlay);
            }
            overlay = null;
            canvas = null;
            root = null;
        }
    }
}
