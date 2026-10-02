using UnityEngine;

public class EmissionFlicker : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    [Header("Emission")]
    [SerializeField] private Color emissionColor = Color.white;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1f;

    [Header("Flicker")]
    [SerializeField] private float minInterval = 0.03f;
    [SerializeField] private float maxInterval = 0.15f;

    private MaterialPropertyBlock propertyBlock;
    private float timer;
    private float nextFlicker;

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        nextFlicker = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < nextFlicker)
            return;

        timer = 0f;
        nextFlicker = Random.Range(minInterval, maxInterval);

        float intensity = Random.Range(minIntensity, maxIntensity);

        propertyBlock.SetColor(
            EmissionColor,
            emissionColor * intensity
        );

        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}
