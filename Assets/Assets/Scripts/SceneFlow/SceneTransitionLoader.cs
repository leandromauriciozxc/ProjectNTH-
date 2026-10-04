using UnityEngine;

namespace ProjectNTH.SceneFlow
{
    /// <summary>A scene-local target for a Timeline Signal or UnityEvent.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Project NTH/Scene Flow/Scene Transition Loader")]
    public sealed class SceneTransitionLoader : MonoBehaviour
    {
        [SerializeField] private string destinationScene = "Assets/Scenes/IndoorScene.unity";
        [Header("Fade timing")]
        [Min(0f), SerializeField] private float fadeOutDuration = 0.7f;
        [Min(0f), SerializeField] private float fadeInDuration = 0.7f;
        [Tooltip("Minimum time fully black, including the time spent loading.")]
        [Min(0f), SerializeField] private float minimumBlackDuration = 0.2f;
        [Tooltip("Additional time under black after scene activation and Start, before fading in.")]
        [Min(0f), SerializeField] private float sceneSettleDuration = 0.15f;
        [Header("Loading")]
        [Tooltip("Give loading more processing time and temporarily remove the FPS limit only while fully black. Previous settings are restored before fading in. Loading priority affects builds, not the Editor.")]
        [SerializeField] private bool prioritizeLoadingWhileBlack = true;
        [Tooltip("Write the load/activation, black-screen and total transition times to the Console or Player log.")]
        [SerializeField] private bool logTransitionTimings;
        [Header("Optional gameplay control")]
        [Tooltip("Movement, camera-look or interaction components to suspend during the fade. Components that survive the scene change are restored afterward.")]
        [SerializeField] private Behaviour[] disableDuringTransition = new Behaviour[0];

        private bool requested;
        public string DestinationScene => destinationScene;
        public static bool IsTransitioning => SceneTransitionRunner.IsRunning;

        /// <summary>Connect a Signal Receiver reaction to this method.</summary>
        public void LoadNextScene() => TryLoadNextScene();

        public bool TryLoadNextScene()
        {
            // Timeline preview/scrubbing in Edit Mode must never change scenes.
            if (!Application.isPlaying || !isActiveAndEnabled || requested || IsTransitioning) return false;
            var destination = (destinationScene ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(destination) || !Application.CanStreamedLevelBeLoaded(destination))
            {
                Debug.LogError("Scene transition cannot load '" + destination
                    + "'. Assign a destination scene and enable it in File > Build Settings before playing.", this);
                return false;
            }
            requested = true;
            SceneTransitionRunner.Begin(destination, fadeOutDuration, fadeInDuration,
                minimumBlackDuration, sceneSettleDuration, disableDuringTransition,
                prioritizeLoadingWhileBlack, logTransitionTimings);
            return true;
        }
    }
}
