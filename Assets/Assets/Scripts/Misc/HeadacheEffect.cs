using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;
public class HeadacheEffect : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] CinemachineVirtualCamera cinemachineCamera;
    [Header("Base Values")]
    [SerializeField] private float vignetteBase = 0.55f;
    [SerializeField] private float distortionBase = -0.35f;
    [SerializeField] private float chromaticBase = 0.65f;

    [Header("Fluctuation")]
    [SerializeField] private float vignetteVariation = 0.10f;
    [SerializeField] private float distortionVariation = 0.10f;
    [SerializeField] private float chromaticVariation = 0.15f;

    [SerializeField] private float fluctuationSpeed = 2f;

    private Vignette vignette;
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;

    [SerializeField]
    private bool headacheActive;

    private void Awake()
    {
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out chromaticAberration);

        StopHeadache();
    }

    private void Update()
    {
        if (!headacheActive)
            return;

        float time = Time.time * fluctuationSpeed;

        float vignetteNoise =
            Mathf.PerlinNoise(time, 0f) * 2f - 1f;

        float distortionNoise =
            Mathf.PerlinNoise(time, 10f) * 2f - 1f;

        float chromaticNoise =
            Mathf.PerlinNoise(time, 20f) * 2f - 1f;

        vignette.intensity.value =
            vignetteBase +
            vignetteNoise * vignetteVariation;

        lensDistortion.intensity.value =
            distortionBase +
            distortionNoise * distortionVariation;

        chromaticAberration.intensity.value =
            chromaticBase +
            chromaticNoise * chromaticVariation;
    }

    public void StartHeadache()
    {
        volume.enabled = true;  
        headacheActive = true;
        
    }
    [YarnCommand("stop_headache")]
    public void StopHeadache()
    {
        headacheActive = false;

        if (vignette != null)
            vignette.intensity.value = 0f;

        if (lensDistortion != null)
            lensDistortion.intensity.value = 0f;

        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 0f;

        volume.enabled = false;
    }

    
}