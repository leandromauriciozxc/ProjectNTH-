using UnityEngine;
using Yarn.Unity;

public class YarnAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip voiceJan;
    [SerializeField] private AudioClip doorSlam;
    [SerializeField] private AudioClip whisper;

    [YarnCommand("play_sound")]
    public void PlaySound(string soundName)
    {
        AudioClip clip = null;

        switch (soundName)
        {
            case "phone_ring":
                clip = voiceJan;
                break;

            case "door_slam":
                clip = doorSlam;
                break;

            case "whisper":
                clip = whisper;
                break;
        }

        if (clip == null)
        {
            Debug.LogWarning($"Sound not found: {soundName}");
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}