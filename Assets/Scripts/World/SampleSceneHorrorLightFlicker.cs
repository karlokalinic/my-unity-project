using UnityEngine;

[DisallowMultipleComponent]
public class SampleSceneHorrorLightFlicker : MonoBehaviour
{
    [SerializeField] private float baseIntensity = 2f;
    [SerializeField] private float noiseAmplitude = 0.28f;
    [SerializeField] private float noiseSpeed = 3.2f;
    [SerializeField] private float minIntensityFactor = 0.08f;
    [SerializeField] private float glitchChancePerSecond = 0.015f;
    [SerializeField] private Vector2 glitchDurationRange = new Vector2(0.03f, 0.14f);

    private Light targetLight;
    private float noiseOffset;
    private float glitchTimer;

    public void Configure(float intensity, float amplitude, float speed, float glitchChance)
    {
        baseIntensity = Mathf.Max(0f, intensity);
        noiseAmplitude = Mathf.Clamp(amplitude, 0f, 0.95f);
        noiseSpeed = Mathf.Max(0.1f, speed);
        glitchChancePerSecond = Mathf.Clamp(glitchChance, 0f, 2f);
    }

    private void Awake()
    {
        targetLight = GetComponent<Light>();
        noiseOffset = Random.Range(0f, 100f);
    }

    private void OnEnable()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        noiseOffset = Random.Range(0f, 100f);
        glitchTimer = 0f;
    }

    private void Update()
    {
        if (targetLight == null)
        {
            return;
        }

        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        if (glitchTimer > 0f)
        {
            glitchTimer -= dt;
            targetLight.intensity = Mathf.Max(0f, baseIntensity * minIntensityFactor);
            return;
        }

        float glitchRoll = Random.value;
        if (glitchRoll < (glitchChancePerSecond * dt))
        {
            glitchTimer = Random.Range(glitchDurationRange.x, glitchDurationRange.y);
            targetLight.intensity = Mathf.Max(0f, baseIntensity * minIntensityFactor);
            return;
        }

        float t = Time.time * noiseSpeed;
        float pulseA = Mathf.PerlinNoise(noiseOffset + t, 0.19f);
        float pulseB = Mathf.PerlinNoise(0.31f, noiseOffset + (t * 0.72f));
        float pulse = Mathf.Lerp(pulseA, pulseB, 0.35f);
        float variation = 1f - ((pulse - 0.5f) * 2f * noiseAmplitude);
        targetLight.intensity = Mathf.Max(0f, baseIntensity * variation);
    }
}
