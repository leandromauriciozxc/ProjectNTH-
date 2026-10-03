using System.Collections.Generic;
using ProjectNTH.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace ProjectNTH.UI
{
    [DisallowMultipleComponent]
    public sealed class SettingsPanelUI : MonoBehaviour
    {
        [Header("Menu connection")]
        [SerializeField] private GameObject menuToHide;
        [SerializeField] private Behaviour[] disableWhileOpen = new Behaviour[0];
        [SerializeField] private SO_SensivitySettings sensitivityDefaults;
        [Header("Prefab references")]
        [SerializeField] private CanvasGroup view;
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private Button backButton;
        [SerializeField] private Button[] tabs;
        [SerializeField] private GameObject[] pages;
        [SerializeField] private Slider[] volumeSliders;
        [SerializeField] private TMP_Text[] volumeValues;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TMP_Text sensitivityValue;
        [SerializeField] private Button invertButton;
        [SerializeField] private TMP_Text invertValue;
        [SerializeField] private Button[] previousButtons;
        [SerializeField] private Button[] nextButtons;
        [SerializeField] private TMP_Text[] displayValues;
        [SerializeField] private Button applyDisplayButton;
        [SerializeField] private GameObject displayConfirmation;
        [SerializeField] private TMP_Text confirmationMessage;
        [SerializeField] private Button keepDisplayButton;
        [SerializeField] private Button revertDisplayButton;
        private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
        private readonly Dictionary<Behaviour, bool> suspended = new Dictionary<Behaviour, bool>();
        private static readonly int[] FrameRates = { 30, 60, 120, -1 };
        private int resolutionIndex;
        private bool fullscreen;
        private bool initialized, menuWasActive;
        private CursorLockMode cursorLock;
        private bool cursorVisible;
        private GameObject previousSelection;
        private EventSystem ownedEventSystem;
        private Vector2Int oldSize;
        private FullScreenMode oldMode;
        private float confirmUntil;
        private bool confirming;
        public bool IsOpen { get; private set; }

        private void Awake() => Initialize();
        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            view.alpha = 0f;
            view.interactable = view.blocksRaycasts = false;
            displayConfirmation.SetActive(false);
            backButton.onClick.AddListener(Close);
            for (int i = 0; i < tabs.Length; i++)
            {
                int tab = i;
                tabs[i].onClick.AddListener(() => ShowPage(tab));
            }
            for (int i = 0; i < volumeSliders.Length; i++)
            {
                int channel = i;
                volumeSliders[i].onValueChanged.AddListener(value =>
                {
                    GameSettings.Instance.SetVolume((AudioCategory)channel, value);
                    volumeValues[channel].text = Percent(value);
                });
            }
            sensitivitySlider.onValueChanged.AddListener(value =>
            {
                GameSettings.Instance.SetSensitivity(value);
                sensitivityValue.text = Percent(value);
            });
            invertButton.onClick.AddListener(() =>
            {
                GameSettings.Instance.SetInvertY(!CurrentInvert());
                invertValue.text = CurrentInvert() ? "On" : "Off";
            });
            for (int i = 0; i < previousButtons.Length; i++)
            {
                int option = i;
                previousButtons[i].onClick.AddListener(() => ChangeDisplay(option, -1));
                nextButtons[i].onClick.AddListener(() => ChangeDisplay(option, 1));
            }
            applyDisplayButton.onClick.AddListener(ApplyDisplay);
            keepDisplayButton.onClick.AddListener(KeepDisplay);
            revertDisplayButton.onClick.AddListener(RevertDisplay);
        }

        public void Open()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Initialize();
            if (IsOpen || !isActiveAndEnabled || GameSettings.Instance == null) return;
            EnsureEventSystem();
            previousSelection = EventSystem.current.currentSelectedGameObject;
            cursorLock = Cursor.lockState;
            cursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menuWasActive = menuToHide != null && menuToHide.activeSelf && !transform.IsChildOf(menuToHide.transform);
            if (menuWasActive) menuToHide.SetActive(false);
            foreach (var component in disableWhileOpen)
            {
                if (component == null || component == this || component.transform.IsChildOf(transform) || suspended.ContainsKey(component)) continue;
                suspended.Add(component, component.enabled);
                component.enabled = false;
            }
            IsOpen = true;
            view.alpha = 0f;
            view.interactable = view.blocksRaycasts = true;
            var settings = GameSettings.Instance;
            for (int i = 0; i < volumeSliders.Length; i++)
            {
                float value = settings.GetVolume((AudioCategory)i);
                volumeSliders[i].SetValueWithoutNotify(value);
                volumeValues[i].text = Percent(value);
            }
            float sensitivity = settings.HasSensitivity ? settings.Sensitivity : sensitivityDefaults != null ? sensitivityDefaults.sensitivity : 0.017f;
            sensitivitySlider.SetValueWithoutNotify(sensitivity);
            sensitivityValue.text = Percent(sensitivity);
            invertValue.text = CurrentInvert() ? "On" : "Off";
            ReadDisplay();
            ShowPage(0);
            EventSystem.current.SetSelectedGameObject(tabs[0].gameObject);
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (confirming) RevertDisplay();
            IsOpen = false;
            view.alpha = 0f;
            view.interactable = view.blocksRaycasts = false;
            if (menuWasActive && menuToHide != null) menuToHide.SetActive(true);
            menuWasActive = false;
            foreach (var pair in suspended) if (pair.Key != null) pair.Key.enabled = pair.Value;
            suspended.Clear();
            Cursor.lockState = cursorLock;
            Cursor.visible = cursorVisible;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
            if (ownedEventSystem != null) ownedEventSystem.gameObject.SetActive(false);
            if (GameSettings.Instance != null) GameSettings.Instance.Save();
        }

        public void ShowPage(int index)
        {
            if (confirming) return;
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetActive(i == index);
                // Selected tab keeps its pale underline even when focus moves to a control.
                tabs[i].transform.Find("Active").gameObject.SetActive(i == index);
            }
        }

        private bool CurrentInvert() => GameSettings.Instance.HasInvertY ? GameSettings.Instance.InvertY : sensitivityDefaults != null && sensitivityDefaults.invertY;
        private static string Percent(float value) => (value * 100f).ToString("0.#") + "%";

        private void ReadDisplay()
        {
            resolutions.Clear();
            foreach (var item in Screen.resolutions)
            {
                var size = new Vector2Int(item.width, item.height);
                if (size.x >= 640 && size.y >= 480 && !resolutions.Contains(size)) resolutions.Add(size);
            }
            var current = new Vector2Int(Screen.width, Screen.height);
            if (!resolutions.Contains(current)) resolutions.Add(current);
            resolutions.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            resolutionIndex = resolutions.IndexOf(current);
            fullscreen = Screen.fullScreen;
            UpdateDisplayLabels();
        }

        private void ChangeDisplay(int option, int direction)
        {
            if (confirming) return;
            var settings = GameSettings.Instance;
            switch (option)
            {
                case 0: settings.SetQuality(Wrap(QualitySettings.GetQualityLevel() + direction, QualitySettings.names.Length)); break;
                case 1: fullscreen = !fullscreen; break;
                case 2: resolutionIndex = Wrap(resolutionIndex + direction, resolutions.Count); break;
                case 3: settings.SetFrameRate(FrameRates[Wrap(System.Array.IndexOf(FrameRates, settings.FrameRate) + direction, FrameRates.Length)]); break;
            }
            UpdateDisplayLabels();
        }
        private static int Wrap(int index, int count) => (index % count + count) % count;

        private void UpdateDisplayLabels()
        {
            displayValues[0].text = QualitySettings.names[QualitySettings.GetQualityLevel()];
            displayValues[1].text = fullscreen ? "Fullscreen" : "Windowed";
            var size = resolutions[resolutionIndex];
            displayValues[2].text = size.x + " x " + size.y;
            int rate = GameSettings.Instance.FrameRate;
            displayValues[3].text = rate < 0 ? "Unlimited" : rate + " FPS";
            applyDisplayButton.interactable = !confirming && (size.x != Screen.width || size.y != Screen.height || fullscreen != Screen.fullScreen);
        }

        public void ApplyDisplay()
        {
            if (confirming || !applyDisplayButton.interactable) return;
            oldSize = new Vector2Int(Screen.width, Screen.height);
            oldMode = Screen.fullScreenMode;
            var size = resolutions[resolutionIndex];
            Screen.SetResolution(size.x, size.y, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            confirming = true;
            confirmUntil = Time.realtimeSinceStartup + 15f;
            SetConfirmationInput(true);
            displayConfirmation.SetActive(true);
            confirmationMessage.text = "Keep these display settings?\nReverting in 15 seconds.";
            EventSystem.current.SetSelectedGameObject(keepDisplayButton.gameObject);
        }

        public void KeepDisplay()
        {
            if (!confirming) return;
            confirming = false;
            SetConfirmationInput(false);
            GameSettings.Instance.SaveDisplay(Screen.width, Screen.height, Screen.fullScreenMode);
            displayConfirmation.SetActive(false);
            ReadDisplay();
            EventSystem.current.SetSelectedGameObject(tabs[1].gameObject);
        }

        public void RevertDisplay()
        {
            if (!confirming) return;
            confirming = false;
            SetConfirmationInput(false);
            Screen.SetResolution(oldSize.x, oldSize.y, oldMode);
            displayConfirmation.SetActive(false);
            fullscreen = oldMode != FullScreenMode.Windowed;
            resolutionIndex = resolutions.IndexOf(oldSize);
            UpdateDisplayLabels();
            applyDisplayButton.interactable = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(tabs[1].gameObject);
        }

        private void SetConfirmationInput(bool active)
        {
            foreach (var page in pages) page.GetComponent<CanvasGroup>().interactable = !active;
            foreach (var tab in tabs) tab.interactable = !active;
            backButton.interactable = !active;
        }

        private void LateUpdate()
        {
            if (!IsOpen) return;
            view.alpha = Mathf.MoveTowards(view.alpha, 1f, Time.unscaledDeltaTime / 0.18f);
            if (Screen.width > 0 && Screen.height > 0)
            {
                var area = Screen.safeArea;
                safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
                safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            }
            if (confirming)
            {
                float remaining = confirmUntil - Time.realtimeSinceStartup;
                confirmationMessage.text = "Keep these display settings?\nReverting in " + Mathf.CeilToInt(Mathf.Max(0f, remaining)) + " seconds.";
                if (remaining <= 0f) RevertDisplay();
            }
            bool escape = false;
#if ENABLE_INPUT_SYSTEM
            escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escape) { if (confirming) RevertDisplay(); else Close(); }
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null && EventSystem.current.isActiveAndEnabled) return;
            if (ownedEventSystem != null) { ownedEventSystem.gameObject.SetActive(true); return; }
            var target = new GameObject("Settings EventSystem", typeof(EventSystem));
            target.transform.SetParent(transform, false);
            ownedEventSystem = target.GetComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            target.AddComponent<InputSystemUIInputModule>();
#else
            target.AddComponent<StandaloneInputModule>();
#endif
        }
        private void OnDisable() => Close();
    }
}
