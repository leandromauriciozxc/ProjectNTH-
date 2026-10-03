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
    [SerializeField] private AudioClip MainChar4;
    [SerializeField] private AudioClip MainChar5;
    [SerializeField] private AudioClip MainChar6;
    [SerializeField] private AudioClip MainChar7;
    [SerializeField] private AudioClip MainChar8;
    [SerializeField] private AudioClip MainChar9;
    [SerializeField] private AudioClip MainChar10;
    [SerializeField] private AudioClip MainChar11;
    [SerializeField] private AudioClip MainChar12;
    [SerializeField] private AudioClip MainChar13;
    [SerializeField] private AudioClip MainChar14;
    [SerializeField] private AudioClip MainChar15;
    [SerializeField] private AudioClip Guard1;
    [SerializeField] private AudioClip Guard2;
    [SerializeField] private AudioClip Guard3;
    [SerializeField] private AudioClip Guard4;
    [SerializeField] private AudioClip Guard5;
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

            case "MainChar4":
                clip = MainChar4;
                break;

            case "MainChar5":
                clip = MainChar5;
                break;

            case "MainChar6":
                clip = MainChar6;
                break;

            case "MainChar7":
                clip = MainChar7;
                break;

            case "MainChar8":
                clip = MainChar8;
                break;

            case "MainChar9":
                clip = MainChar9;
                break;

            case "MainChar10":
                clip = MainChar10;
                break;

            case "MainChar11":
                clip = MainChar11;
                break;

            case "MainChar12":
                clip = MainChar12;
                break;

            case "MainChar13":
                clip = MainChar13;
                break;

            case "MainChar14":
                clip = MainChar14;
                break;

            case "MainChar15":
                clip = MainChar15;
                break;

            case "Guard1":
                clip = Guard1;
                break;

            case "Guard2":
                clip = Guard2;
                break;
            case "Guard3":
                clip = Guard3;
                break;
            case "Guard4":
                clip = Guard4;
                break;
            case "Guard5":
                clip = Guard5;
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

        if (audioSource == null)
        {
            Debug.LogWarning("Yarn audio needs an Audio Source.", this);
            return;
        }
        audioSource.Stop();
        bool effect = soundName == "door_slam";
        ProjectNTH.Settings.GameSettings.Route(audioSource, effect
            ? ProjectNTH.Settings.AudioCategory.SoundEffects : ProjectNTH.Settings.AudioCategory.Dialogue);
        audioSource.clip = clip;
        audioSource.Play();
    }
}
