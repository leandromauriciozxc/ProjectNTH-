using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace ProjectNTH.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/UI/Credits Panel")]
    public sealed class CreditsPanelUI : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private CreditsList credits;
        [Tooltip("Enable for a standalone credits scene or to preview the panel in Play Mode.")]
        [SerializeField] private bool openOnStart;
        [Header("Menu connection (optional)")]
        [Tooltip("A separate menu panel to hide while the credits are open, then restore on Back.")]
        [SerializeField] private GameObject menuToHide;
        [Tooltip("For in-game use, assign movement, camera look and interaction components to suspend while open.")]
        [SerializeField] private Behaviour[] disableWhileOpen = new Behaviour[0];
        [SerializeField] private bool manageCursor = true;
        [SerializeField] private bool closeOnEscape = true;
        [SerializeField] private UnityEvent onOpened = new UnityEvent();
        [SerializeField] private UnityEvent onClosed = new UnityEvent();
        [Header("Rows")]
        [Range(0.35f, 0.75f), SerializeField] private float modelColumnFraction = 0.65f;
        [Min(8f), SerializeField] private float columnGap = 48f;
        [Min(0f), SerializeField] private float rowSpacing = 14f;
        [Min(16f), SerializeField] private float minimumRowHeight = 48f;
        [Header("Prefab references")]
        [SerializeField] private CanvasGroup view;
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform rowTemplate;
        [SerializeField] private RectTransform modelHeading;
        [SerializeField] private RectTransform creatorHeading;
        [SerializeField] private TMP_Text emptyMessage;
        [SerializeField] private Button backButton;

        private sealed class Row
        {
            public RectTransform rect;
            public TMP_Text model;
            public TMP_Text creator;
        }
        private readonly List<Row> rows = new List<Row>();
        private readonly Dictionary<Behaviour, bool> suspended = new Dictionary<Behaviour, bool>();
        private EventSystem ownedEventSystem;
        private GameObject previousSelection;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLock;
        private bool cursorCaptured;
        private bool menuWasActive;
        private bool menuCaptured;
        private bool initialized;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private float lastWidth = -1f;

        public bool IsOpen { get; private set; }
        private static CreditsPanelUI openPanel;
        public static bool AnyOpen => openPanel != null && openPanel.IsOpen;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOpenPanel() => openPanel = null;
        public int CreditCount => rows.Count;

        private void Awake() => Initialize();
        private void Start() { if (openOnStart) Open(); }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            if (view == null || scroll == null || rowTemplate == null || backButton == null)
            {
                Debug.LogError("Credits Panel needs its UI references. Use the supplied Credits Panel prefab.", this);
                enabled = false;
                return;
            }
            backButton.onClick.AddListener(Close);
            SetVisible(false);
            RefreshCredits();
        }

        /// <summary>Connect a menu Button's On Click event to this method.</summary>
        public void Open()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Initialize();
            if (!isActiveAndEnabled || IsOpen || AnyOpen || view == null) return;
            EnsureEventSystem();
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (menuToHide != null && menuToHide != gameObject && !transform.IsChildOf(menuToHide.transform))
            {
                menuWasActive = menuToHide.activeSelf;
                menuCaptured = true;
                menuToHide.SetActive(false);
            }
            suspended.Clear();
            foreach (var behaviour in disableWhileOpen)
            {
                if (behaviour == null || behaviour == this || behaviour.transform.IsChildOf(transform)
                    || suspended.ContainsKey(behaviour)) continue;
                suspended.Add(behaviour, behaviour.enabled);
                behaviour.enabled = false;
            }
            if (manageCursor)
            {
                previousCursorVisible = Cursor.visible;
                previousCursorLock = Cursor.lockState;
                cursorCaptured = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            IsOpen = true;
            openPanel = this;
            SetVisible(true);
            ApplySafeArea();
            Canvas.ForceUpdateCanvases();
            LayoutRows();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(backButton.gameObject);
            onOpened.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (openPanel == this) openPanel = null;
            SetVisible(false);
            if (scroll != null) scroll.StopMovement();
            foreach (var pair in suspended)
                if (pair.Key != null) pair.Key.enabled = pair.Value;
            suspended.Clear();
            if (menuCaptured && menuToHide != null) menuToHide.SetActive(menuWasActive);
            menuCaptured = false;
            if (cursorCaptured)
            {
                Cursor.lockState = previousCursorLock;
                Cursor.visible = previousCursorVisible;
                cursorCaptured = false;
            }
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy
                    ? previousSelection : null);
            previousSelection = null;
            if (ownedEventSystem != null) ownedEventSystem.gameObject.SetActive(false);
            onClosed.Invoke();
        }

        /// <summary>Call after changing the credits list at runtime.</summary>
        public void RefreshCredits()
        {
            if (scroll == null || scroll.content == null || rowTemplate == null) return;
            foreach (var row in rows)
            {
                row.rect.gameObject.SetActive(false);
                Destroy(row.rect.gameObject);
            }
            rows.Clear();
            if (credits != null && credits.entries != null)
                foreach (var entry in credits.entries)
                {
                    if (entry == null || (string.IsNullOrWhiteSpace(entry.modelName) && string.IsNullOrWhiteSpace(entry.creatorName))) continue;
                    var rect = Instantiate(rowTemplate, scroll.content, false);
                    rect.name = "Credit - " + entry.modelName;
                    var row = new Row { rect = rect,
                        model = rect.Find("Model").GetComponent<TMP_Text>(),
                        creator = rect.Find("Creator").GetComponent<TMP_Text>() };
                    row.model.text = entry.modelName ?? string.Empty;
                    row.creator.text = entry.creatorName ?? string.Empty;
                    rect.gameObject.SetActive(true);
                    rows.Add(row);
                }
            if (emptyMessage != null) emptyMessage.gameObject.SetActive(rows.Count == 0);
            lastWidth = -1f;
            LayoutRows();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
        }

        private void LateUpdate()
        {
            if (!IsOpen) return;
            ApplySafeArea();
            if (!Mathf.Approximately(lastWidth, scroll.content.rect.width)) LayoutRows();
            ReadKeyboard();
        }

        private void ReadKeyboard()
        {
            bool close = false, top = false, bottom = false;
            float direction = 0f;
            float page = 0f;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                close = keyboard.escapeKey.wasPressedThisFrame;
                top = keyboard.homeKey.wasPressedThisFrame;
                bottom = keyboard.endKey.wasPressedThisFrame;
                direction = (keyboard.downArrowKey.isPressed ? -1f : 0f) + (keyboard.upArrowKey.isPressed ? 1f : 0f);
                page = (keyboard.pageDownKey.wasPressedThisFrame ? -1f : 0f) + (keyboard.pageUpKey.wasPressedThisFrame ? 1f : 0f);
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            close = Input.GetKeyDown(KeyCode.Escape);
            top = Input.GetKeyDown(KeyCode.Home);
            bottom = Input.GetKeyDown(KeyCode.End);
            direction = (Input.GetKey(KeyCode.DownArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f);
            page = (Input.GetKeyDown(KeyCode.PageDown) ? -1f : 0f) + (Input.GetKeyDown(KeyCode.PageUp) ? 1f : 0f);
#endif
            if (close && closeOnEscape) { Close(); return; }
            if (top || bottom || direction != 0f || page != 0f)
            {
                scroll.StopMovement();
                float range = Mathf.Max(1f, scroll.content.rect.height - scroll.viewport.rect.height);
                scroll.verticalNormalizedPosition = top ? 1f : bottom ? 0f : Mathf.Clamp01(scroll.verticalNormalizedPosition
                    + (direction * 650f * Time.unscaledDeltaTime + page * scroll.viewport.rect.height * 0.85f) / range);
            }
        }

        private void LayoutRows()
        {
            float width = scroll.content.rect.width;
            if (width <= 1f) return;
            float position = scroll.verticalNormalizedPosition;
            lastWidth = width;
            float gap = Mathf.Min(columnGap, width * 0.1f);
            float modelWidth = (width - gap) * modelColumnFraction;
            float creatorX = modelWidth + gap;
            float creatorWidth = width - creatorX;
            SetCell(modelHeading, 0f, modelWidth);
            SetCell(creatorHeading, creatorX, creatorWidth);
            float y = 0f;
            foreach (var row in rows)
            {
                SetCell(row.model.rectTransform, 0f, modelWidth);
                SetCell(row.creator.rectTransform, creatorX, creatorWidth);
                float height = Mathf.Max(minimumRowHeight,
                    row.model.GetPreferredValues(row.model.text, modelWidth, Mathf.Infinity).y,
                    row.creator.GetPreferredValues(row.creator.text, creatorWidth, Mathf.Infinity).y);
                row.rect.anchoredPosition = new Vector2(0f, -y);
                row.rect.sizeDelta = new Vector2(0f, Mathf.Ceil(height));
                y += Mathf.Ceil(height) + rowSpacing;
            }
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                Mathf.Max(0f, y - (rows.Count > 0 ? rowSpacing : 0f)));
            scroll.verticalNormalizedPosition = position;
        }

        private static void SetCell(RectTransform rect, float x, float width)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(x, 0f);
            rect.offsetMax = new Vector2(x + width, 0f);
        }

        private void ApplySafeArea()
        {
            if (safeArea == null || Screen.width <= 0 || Screen.height <= 0) return;
            var size = new Vector2Int(Screen.width, Screen.height);
            var area = Screen.safeArea;
            if (size == lastScreenSize && area == lastSafeArea) return;
            lastScreenSize = size;
            lastSafeArea = area;
            safeArea.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            safeArea.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        }

        private void SetVisible(bool visible)
        {
            if (view == null) return;
            view.alpha = visible ? 1f : 0f;
            view.interactable = visible;
            view.blocksRaycasts = visible;
        }

        private void EnsureEventSystem()
        {
            // Reuse the scene's input setup. A standalone prefab only creates a fallback if needed.
            if (EventSystem.current != null && EventSystem.current.isActiveAndEnabled) return;
            if (ownedEventSystem != null) { ownedEventSystem.gameObject.SetActive(true); return; }
            var target = new GameObject("Credits EventSystem", typeof(EventSystem));
            target.transform.SetParent(transform, false);
            ownedEventSystem = target.GetComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            target.AddComponent<InputSystemUIInputModule>();
#else
            target.AddComponent<StandaloneInputModule>();
#endif
        }

        private void OnDisable() => Close();
        private void OnDestroy()
        {
            Close();
            if (backButton != null) backButton.onClick.RemoveListener(Close);
        }
    }
}
