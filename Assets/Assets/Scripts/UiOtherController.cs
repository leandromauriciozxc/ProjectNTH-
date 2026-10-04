using System.Collections.Generic;
using ProjectNTH.SceneFlow;
using ProjectNTH.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

[DefaultExecutionOrder(-1000)]
public class UiOtherController : MonoBehaviour
{
    [Header("Pause menu")]
    [SerializeField] private GameObject pauseMenu;
    [Tooltip("The separate main-menu button group. Leave empty in scenes without a main menu.")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Selectable firstSelected;
    [SerializeField] private SettingsPanelUI settingsPanel;
    [Tooltip("Used if this scene has no Settings panel yet.")]
    [SerializeField] private SettingsPanelUI settingsPanelPrefab;
    [Header("Gameplay")]
    [SerializeField] private bool pauseAudio = true;
    [Tooltip("Additional components to suspend. Player movement, look, input and interaction are found automatically.")]
    [SerializeField] private Behaviour[] disableWhilePaused = new Behaviour[0];

    private static UiOtherController owner;
    private static double pausedSeconds;
    private static double pauseStarted;
    private static int resumedFrame = -1;
    private readonly Dictionary<Behaviour, bool> suspended = new Dictionary<Behaviour, bool>();
    private float previousTimeScale;
    private bool previousAudioPause, previousCursorVisible;
    private CursorLockMode previousCursorLock;
    private GameObject previousSelection;
    private EventSystem ownedEventSystem;
    private SettingsPanelUI ownedSettings;
    public bool IsPaused { get; private set; }
    public static bool IsGamePaused => owner != null && owner.IsPaused;
    public static bool BlocksGameplayInput => IsGamePaused || Time.frameCount == resumedFrame;
    // HUD/thought timers keep using real time, except while the pause menu owns the game.
    public static double GameplayUnscaledTime => Time.unscaledTimeAsDouble - pausedSeconds
        - (IsGamePaused ? Time.unscaledTimeAsDouble - pauseStarted : 0d);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { owner = null; pausedSeconds = pauseStarted = 0d; resumedFrame = -1; }

    private void Awake()
    {
        if (pauseMenu == null) return;
        pauseMenu.SetActive(false);
        // Keep the authored menu above the HUD but below Settings and screen fades.
        var canvas = pauseMenu.GetComponent<Canvas>();
        if (canvas == null) canvas = pauseMenu.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 90;
        if (pauseMenu.GetComponent<GraphicRaycaster>() == null) pauseMenu.AddComponent<GraphicRaycaster>();
    }

    private void Start()
    {
        // Outdoor starts at its menu; scenes without a main menu start in gameplay.
        if (!IsGamePaused && !HasOpenPanel()) SetMenuCursor(IsMainMenuVisible);
    }

    /// <summary>Add to the Play button alongside its existing gameplay-start events.</summary>
    public void BeginGameplay()
    {
        if (!isActiveAndEnabled || IsGamePaused || HasOpenPanel()) return;
        if (mainMenu != null) mainMenu.SetActive(false);
        SetMenuCursor(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private bool IsMainMenuVisible => mainMenu != null && mainMenu.activeInHierarchy;

    private static void SetMenuCursor(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }

    private void Update()
    {
        // Settings handles Escape in LateUpdate. Let it consume the key first, one menu per press.
        if (HasOpenPanel()) return;
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        pressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        pressed = Input.GetKeyDown(KeyCode.Escape);
#endif
        if (pressed) { if (IsPaused) Resume(); else Pause(); }
    }

    public void Pause()
    {
        if (IsPaused || IsGamePaused || !isActiveAndEnabled || !CanPause()) return;
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        owner = this;
        pauseStarted = Time.unscaledTimeAsDouble;
        IsPaused = true;
        // These components read input in Update; a zero time scale alone does not stop them.
        foreach (var component in FindObjectsOfType<MonoBehaviour>(true))
            if (component.gameObject.scene == gameObject.scene &&
                (component is InputReader || component is PlayerMovement || component is PlayerLook
                || component is Raycast || component is LookBack || component is CameraMotionController
                || component is SprintCameraEffects)) Suspend(component);
        foreach (var component in disableWhilePaused) Suspend(component);
        Time.timeScale = 0f;
        if (pauseAudio) AudioListener.pause = true;
        SetMenuCursor(true);
        pauseMenu.SetActive(true);
        EnsureEventSystem();
        EventSystem.current.SetSelectedGameObject(firstSelected != null ? firstSelected.gameObject : null);
    }

    public void Resume() => EndPause(true);

    private void EndPause(bool returnToGameplay)
    {
        if (!IsPaused) return;
        // Close the child first so its cursor/menu restoration cannot undo ours.
        if (settingsPanel != null && settingsPanel.IsOpen) settingsPanel.Close();
        pausedSeconds += Time.unscaledTimeAsDouble - pauseStarted;
        IsPaused = false;
        resumedFrame = Time.frameCount;
        if (owner == this) owner = null;
        if (pauseMenu != null) pauseMenu.SetActive(false);
        Time.timeScale = previousTimeScale;
        if (pauseAudio) AudioListener.pause = previousAudioPause;
        foreach (var pair in suspended) if (pair.Key != null) pair.Key.enabled = pair.Value;
        suspended.Clear();
        if (returnToGameplay)
            SetMenuCursor(IsMainMenuVisible);
        else
        {
            // Disabling/unloading the controller is cleanup, not a gameplay Resume click.
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
        previousSelection = null;
        if (ownedEventSystem != null) ownedEventSystem.gameObject.SetActive(false);
    }

    public void OpenSettings()
    {
        if (!IsPaused) return;
        if (settingsPanel == null)
            foreach (var panel in FindObjectsOfType<SettingsPanelUI>(true))
                if (panel.gameObject.scene == gameObject.scene) { settingsPanel = panel; break; }
        if (settingsPanel == null && settingsPanelPrefab != null)
        {
            ownedSettings = settingsPanel = Instantiate(settingsPanelPrefab);
            SceneManager.MoveGameObjectToScene(settingsPanel.gameObject, gameObject.scene);
        }
        if (settingsPanel != null) settingsPanel.OpenFrom(pauseMenu);
        else Debug.LogWarning("Assign a Settings Panel or Settings Panel Prefab on UiOtherController.", this);
    }

    private bool CanPause()
    {
        if (pauseMenu == null || Time.timeScale <= 0f || SceneTransitionLoader.IsTransitioning
            || (mainMenu != null && mainMenu.activeInHierarchy) || HasOpenPanel()) return false;
        foreach (var conversation in FindObjectsOfType<DialogueSystemController>())
            if (conversation.DialogueRunner != null && conversation.DialogueRunner.IsDialogueRunning) return false;
        foreach (var director in FindObjectsOfType<PlayableDirector>())
            if (director.state == PlayState.Playing) return false;
        foreach (var movement in FindObjectsOfType<PlayerMovement>())
            if (movement.gameObject.scene == gameObject.scene && movement.enabled) return true;
        return false;
    }

    private bool HasOpenPanel()
    {
        // These flags avoid searching scene objects every frame.
        return SettingsPanelUI.AnyOpen || CreditsPanelUI.AnyOpen;
    }

    private void Suspend(Behaviour component)
    {
        if (component == null || component == this || component.transform.IsChildOf(transform)
            || suspended.ContainsKey(component)) return;
        suspended.Add(component, component.enabled);
        component.enabled = false;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null && EventSystem.current.isActiveAndEnabled) return;
        if (ownedEventSystem != null) { ownedEventSystem.gameObject.SetActive(true); return; }
        var target = new GameObject("Pause EventSystem", typeof(EventSystem));
        target.transform.SetParent(transform, false);
        ownedEventSystem = target.GetComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        target.AddComponent<InputSystemUIInputModule>();
#else
        target.AddComponent<StandaloneInputModule>();
#endif
    }

    private void OnDisable() => EndPause(false);
    private void OnDestroy()
    {
        EndPause(false);
        if (ownedSettings != null) Destroy(ownedSettings.gameObject);
    }
    public void Quit()
    {
        Resume();
        Application.Quit();
    }
}
