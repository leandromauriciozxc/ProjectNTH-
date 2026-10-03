using UnityEngine;

namespace ProjectNTH.Settings
{
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Project NTH/Audio/Settings Audio Route")]
    public sealed class SettingsAudioRoute : MonoBehaviour
    {
        [SerializeField] private AudioCategory category = AudioCategory.SoundEffects;
        private void OnEnable() => GameSettings.Route(GetComponent<AudioSource>(), category);
    }
}
