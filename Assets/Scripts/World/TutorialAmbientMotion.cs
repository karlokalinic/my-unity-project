using UnityEngine;

[DisallowMultipleComponent]
public class TutorialAmbientMotion : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 bobAmplitude = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float bobSpeed = 1.15f;
    [SerializeField] private float yawSpeed = 10f;
    [SerializeField] [Range(1f, 30f)] private float positionSmoothing = 14f;
    [SerializeField] [Range(1f, 30f)] private float rotationSmoothing = 12f;
    [SerializeField] private bool randomizePhaseOnAwake = true;
    [SerializeField] private float phaseOffset;

    [Header("Optional Light Pulse")]
    [SerializeField] private Light targetLight;
    [SerializeField] private float pulseAmplitude = 0.25f;
    [SerializeField] private float pulseSpeed = 1.35f;
    [SerializeField] [Range(1f, 30f)] private float lightSmoothing = 14f;

    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float baseLightIntensity;
    private bool initialized;

    public void Configure(Transform animatedTransform, Light animatedLight)
    {
        targetTransform = animatedTransform;
        targetLight = animatedLight;
        CacheBaseline();
    }

    private void Awake()
    {
        if (randomizePhaseOnAwake)
        {
            phaseOffset = Random.Range(0f, 100f);
        }

        CacheBaseline();
    }

    private void OnValidate()
    {
        CacheBaseline();
    }

    private void Update()
    {
        if (!initialized)
        {
            CacheBaseline();
        }

        float dt = Time.deltaTime;
        float blendPos = 1f - Mathf.Exp(-positionSmoothing * dt);
        float blendRot = 1f - Mathf.Exp(-rotationSmoothing * dt);
        float blendLight = 1f - Mathf.Exp(-lightSmoothing * dt);
        float t = Time.time + phaseOffset;

        if (targetTransform != null)
        {
            float bobSignal = Mathf.Sin(t * Mathf.Max(0.05f, bobSpeed));
            Vector3 offset = bobAmplitude * bobSignal;
            Vector3 targetPosition = baseLocalPosition + offset;
            targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, targetPosition, blendPos);

            float yawOffset = Mathf.Sin(t * Mathf.Max(0.05f, bobSpeed * 0.85f)) * yawSpeed;
            Quaternion targetRotation = baseLocalRotation * Quaternion.Euler(0f, yawOffset, 0f);
            targetTransform.localRotation = Quaternion.Slerp(targetTransform.localRotation, targetRotation, blendRot);
        }

        if (targetLight != null)
        {
            float pulse = Mathf.Sin(t * Mathf.Max(0.05f, pulseSpeed)) * pulseAmplitude;
            float desiredIntensity = Mathf.Max(0f, baseLightIntensity + pulse);
            targetLight.intensity = Mathf.Lerp(targetLight.intensity, desiredIntensity, blendLight);
        }
    }

    private void CacheBaseline()
    {
        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        baseLocalPosition = targetTransform.localPosition;
        baseLocalRotation = targetTransform.localRotation;

        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (targetLight != null)
        {
            baseLightIntensity = targetLight.intensity;
        }

        initialized = true;
    }
}
