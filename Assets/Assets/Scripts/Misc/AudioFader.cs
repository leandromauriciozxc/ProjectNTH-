using System.Collections;
using UnityEngine;

public class AudioFader : MonoBehaviour
{
    public AudioSource audioSource;

    // Call this method from another script or a button UI to trigger the fade
    public void FadeOutAndStop(float duration)
    {
        StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = audioSource.volume;

        // Gradually reduce volume over the specified duration
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null; // Wait for the next frame
        }

        // Stop the playback cleanly and reset the volume
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
