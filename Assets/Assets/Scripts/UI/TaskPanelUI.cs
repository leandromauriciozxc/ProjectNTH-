using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectNTH.UI
{
    /// <summary>A timed task reminder that contracts to an icon and opens again with Tab.</summary>
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

        public string CurrentTask => taskMessage;
        public bool HasTask => !string.IsNullOrWhiteSpace(taskMessage);
        public bool IsExpanded => HasTask && expanded;

        /// <summary>Assign a task and display it immediately; usable from UnityEvents and Timeline signals.</summary>
        public void SetTask(string message)
        {
            taskMessage = message ?? string.Empty;
            layoutDirty = true;
            if (!isActiveAndEnabled) return;
            if (HasTask) Reveal();
            else ClearTask();
        }

        /// <summary>Show the current message again and restart its display timer.</summary>
        public void Reveal()
        {
            if (!isActiveAndEnabled || !HasTask) return;
            SetExpanded(true);
            collapseAt = Time.unscaledTime + Mathf.Max(0.1f, displayDuration)
                + (expansion < 1f ? Mathf.Max(0f, animationDuration) : 0f);
            root.gameObject.SetActive(true);
        }

        public void Collapse()
        {
            SetExpanded(false);
        }

        /// <summary>Remove the current task, including the icon and Tab hint.</summary>
        public void ClearTask()
        {
            taskMessage = string.Empty;
            expanded = false;
            expansion = animationFrom = animationTarget = 0f;
            layoutDirty = true;
            if (root != null) root.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            CreateUI();
            layoutDirty = true;
            expanded = false;
            expansion = animationFrom = animationTarget = 0f;
            if (showOnEnable) Reveal();
            RefreshLayout();
            ApplyAnimation();
            root.gameObject.SetActive(HasTask);
        }

        private void Update()
        {
            if (root.gameObject.activeSelf != HasTask) root.gameObject.SetActive(HasTask);
            if (!HasTask) return;
            if (TabPressed() && !IsTyping()) Reveal();
            if (expanded && Time.unscaledTime >= collapseAt) Collapse();
            float duration = Mathf.Max(0f, animationDuration);
            float t = duration == 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - animationStarted) / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            expansion = Mathf.Lerp(animationFrom, animationTarget, eased);
            if (layoutDirty || canvasRect.rect.size != lastCanvasSize || Screen.safeArea != lastSafeArea)
                RefreshLayout();
            ApplyAnimation();
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
            root.anchoredPosition = new Vector2(left, -top);
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
        }

        private void ApplyAnimation()
        {
            float height = Mathf.Lerp(Mathf.Max(56f, panelHeight), expandedHeight, expansion);
            float width = Mathf.Lerp(CompactWidth, expandedWidth, expansion);
            panel.sizeDelta = new Vector2(width, height);
            textViewport.sizeDelta = new Vector2(Mathf.Max(0f, width - TextInset - RightPadding), height);
            diamond.anchoredPosition = new Vector2(72f, -height * 0.5f);
            tabHint.rectTransform.anchoredPosition = new Vector2(CompactWidth + 10f, -panelHeight * 0.5f);
            messageGroup.alpha = Mathf.Clamp01(expansion * 3f);
            hintGroup.alpha = 1f - Mathf.Clamp01(expansion * 5f);
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
            panelHeight = Mathf.Max(56f, panelHeight);
            maximumWidth = Mathf.Max(160f, maximumWidth);
            minimumWidth = Mathf.Clamp(minimumWidth, 160f, maximumWidth);
            layoutDirty = true;
        }

        private void OnDisable()
        {
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
