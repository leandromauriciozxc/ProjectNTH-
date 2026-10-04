using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace ProjectNTH.Settings
{
    public enum AudioCategory { Master, Music, SoundEffects, Dialogue }

    /// <summary>One settings owner for all scenes. The UI only edits this service.</summary>
    public sealed class GameSettings : MonoBehaviour
    {
        private const string Prefix = "ProjectNTH.Settings.";
        private static GameSettings instance;
        private readonly float[] volumes = { 1f, 1f, 1f, 1f };
        private readonly AudioMixerGroup[] groups = new AudioMixerGroup[4];
        private static readonly string[] Parameters = { "MasterVolume", "MusicVolume", "SoundEffectsVolume", "DialogueVolume" };
        private static readonly string[] GroupNames = { "Master", "Music", "Sound Effects", "Dialogue" };
        private AudioMixer mixer;
        private bool audioReady;
        public static GameSettings Instance => instance;
        public int FrameRate { get; private set; } = 60;
        public float Sensitivity { get; private set; }
        public bool HasSensitivity { get; private set; }
        public bool InvertY { get; private set; }
        public bool HasInvertY { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance != null) return;
            new GameObject("Game Settings").AddComponent<GameSettings>();
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            mixer = Resources.Load<AudioMixer>("Game Audio");
            for (int i = 0; i < volumes.Length; i++)
            {
                volumes[i] = ReadUnit(Parameters[i], 1f);
                if (mixer != null)
                    foreach (var group in mixer.FindMatchingGroups(string.Empty))
                        if (group.name == GroupNames[i]) { groups[i] = group; break; }
            }
            FrameRate = ReadFrameRate();
            HasSensitivity = PlayerPrefs.HasKey(Prefix + "Sensitivity");
            Sensitivity = ReadUnit("Sensitivity", 0.017f);
            HasInvertY = PlayerPrefs.HasKey(Prefix + "InvertY");
            InvertY = PlayerPrefs.GetInt(Prefix + "InvertY", 0) != 0;
            int quality = PlayerPrefs.GetInt(Prefix + "Quality", QualitySettings.GetQualityLevel());
            if (quality >= 0 && quality < QualitySettings.names.Length && quality != QualitySettings.GetQualityLevel())
                QualitySettings.SetQualityLevel(quality, true);
            ApplyFrameRate();
            RestoreDisplay();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            // Unity requires exposed mixer parameters to be set in Start or later.
            audioReady = true;
            ApplyVolumes();
        }

        private static float ReadUnit(string name, float fallback)
        {
            float value = PlayerPrefs.GetFloat(Prefix + name, fallback);
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp01(value);
        }

        private static int ReadFrameRate()
        {
            int value = PlayerPrefs.GetInt(Prefix + "FrameRate", 60);
            return value == 30 || value == 60 || value == 120 || value == -1 ? value : 60;
        }

        public static void ApplyFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = instance != null ? instance.FrameRate : ReadFrameRate();
        }

        public float GetVolume(AudioCategory category) => volumes[(int)category];
        public void SetVolume(AudioCategory category, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return;
            volumes[(int)category] = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(Prefix + Parameters[(int)category], volumes[(int)category]);
            if (audioReady) ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (mixer == null) return;
            for (int i = 0; i < volumes.Length; i++)
                mixer.SetFloat(Parameters[i], volumes[i] <= 0f ? -80f : Mathf.Log10(volumes[i]) * 20f);
        }

        public void SetSensitivity(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return;
            Sensitivity = Mathf.Clamp01(value);
            HasSensitivity = true;
            PlayerPrefs.SetFloat(Prefix + "Sensitivity", Sensitivity);
        }

        public void SetInvertY(bool value)
        {
            HasInvertY = true;
            InvertY = value;
            PlayerPrefs.SetInt(Prefix + "InvertY", value ? 1 : 0);
        }

        public void SetQuality(int index)
        {
            index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
            if (QualitySettings.GetQualityLevel() != index) QualitySettings.SetQualityLevel(index, true);
            // Quality profiles may enable VSync; keep the selected FPS policy in charge.
            ApplyFrameRate();
            PlayerPrefs.SetInt(Prefix + "Quality", index);
        }

        public void SetFrameRate(int value)
        {
            if (value != 30 && value != 60 && value != 120 && value != -1) return;
            FrameRate = value;
            PlayerPrefs.SetInt(Prefix + "FrameRate", value);
            ApplyFrameRate();
        }

        public void SaveDisplay(int width, int height, FullScreenMode mode)
        {
            PlayerPrefs.SetInt(Prefix + "Width", width);
            PlayerPrefs.SetInt(Prefix + "Height", height);
            PlayerPrefs.SetInt(Prefix + "Fullscreen", mode == FullScreenMode.Windowed ? 0 : 1);
            Save();
        }

        private static void RestoreDisplay()
        {
            if (!PlayerPrefs.HasKey(Prefix + "Width")) return;
            int width = PlayerPrefs.GetInt(Prefix + "Width"), height = PlayerPrefs.GetInt(Prefix + "Height");
            if (width < 640 || height < 480 || width > 16384 || height > 16384) return;
            bool fullscreen = PlayerPrefs.GetInt(Prefix + "Fullscreen", 1) != 0;
            // A different monitor may not support a previously saved fullscreen size.
            if (fullscreen)
            {
                bool supported = false;
                foreach (var resolution in Screen.resolutions)
                    if (resolution.width == width && resolution.height == height) { supported = true; break; }
                if (!supported) return;
            }
            Screen.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        public static void Route(AudioSource source, AudioCategory category)
        {
            if (source != null && instance != null && instance.groups[(int)category] != null)
                source.outputAudioMixerGroup = instance.groups[(int)category];
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Existing scenes have unrouted sources. Preserve explicit assignments and local volumes.
            foreach (var root in scene.GetRootGameObjects())
                foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
                    if (source.outputAudioMixerGroup == null) Route(source, AudioCategory.SoundEffects);
        }

        public void Save() => PlayerPrefs.Save();
        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this) instance = null;
        }
    }
}
