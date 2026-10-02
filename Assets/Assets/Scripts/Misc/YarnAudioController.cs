using UnityEngine;
using Yarn.Unity;

public class YarnAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Clips")]
    [SerializeField] private AudioClip voiceJan;
    [SerializeField] private AudioClip Jan1;
    [SerializeField] private AudioClip Jan2;
    [SerializeField] private AudioClip Jan3;
    [SerializeField] private AudioClip Jan4;
    [SerializeField] private AudioClip MainChar1;
    [SerializeField] private AudioClip MainChar2;
    [SerializeField] private AudioClip MainChar3;
    [SerializeField] private AudioClip Guard1;
    [SerializeField] private AudioClip Guard2;
    [SerializeField] private AudioClip Step1;
    [SerializeField] private AudioClip Step2;
    [SerializeField] private AudioClip Step3;
    [SerializeField] private AudioClip doorSlam;
    [SerializeField] private AudioClip whisper;

    [YarnCommand("play_sound")]
    public void PlaySound(string soundName)
    {
        AudioClip clip = null;

        switch (soundName)
        {
            case "Jan1":
                clip = Jan1;
                break;

            case "Jan2":
                clip = Jan2;
                break;

            case "Jan3.1":
                clip = Jan3;
                break;

            case "Jan3.2":
                clip = Jan4;
                break;

            case "MainChar1":
                clip = MainChar1;
                break;

            case "MainChar2.1":
                clip = MainChar2;
                break;

            case "MainChar2.2":
                clip = MainChar3;
                break;

            case "Guard1":
                clip = Guard1;
                break;

            case "Guard2":
                clip = Guard2;
                break;

            case "Step1":
                clip = Step1;
                break;

            case "Step2":
                clip = Step2;
                break;

            case "Step3":
                clip = Step3;
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