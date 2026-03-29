using UnityEngine;

[DisallowMultipleComponent]
public class PlayerFlashlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lightAnchor;
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private Transform firstPersonLookSource;

    [Header("Setup")]
    [SerializeField] private bool startEnabled = true;
    [SerializeField] private Vector3 localPosition = new Vector3(0.08f, -0.05f, 0.18f);
    [SerializeField] private Vector3 localEuler = Vector3.zero;

    [Header("Light")]
    [SerializeField] private Color lightColor = new Color(1f, 0.96f, 0.86f, 1f);
    [SerializeField] private float intensity = 9.2f;
    [SerializeField] private float range = 32f;
    [SerializeField] private float spotAngle = 63f;
    [SerializeField] private float innerSpotAngle = 43f;
    [SerializeField] private bool castShadows = true;

    private Light flashlight;
    private bool isOn;

    public bool IsOn => isOn;

    public void Configure(Transform anchor, HolstinCameraRig rig = null, Transform lookSource = null)
    {
        if (anchor != null)
        {
            lightAnchor = anchor;
        }

        if (rig != null)
        {
            cameraRig = rig;
        }

        if (lookSource != null)
        {
            firstPersonLookSource = lookSource;
        }

        EnsureFlashlight();
    }

    private void Awake()
    {
        EnsureFlashlight();
        SetEnabled(startEnabled);
    }

    private void OnEnable()
    {
        EnsureFlashlight();
    }

    private void Update()
    {
        ResolveReferences();
        if (GameplayPauseFacade.IsPaused)
        {
            return;
        }

        if (InputReader.FlashlightTogglePressed())
        {
            SetEnabled(!isOn);
        }
    }

    private void LateUpdate()
    {
        UpdateLightTransform();
    }

    private void ResolveReferences()
    {
        if (cameraRig == null)
        {
            cameraRig = FindAnyObjectByType<HolstinCameraRig>();
        }

        if (firstPersonLookSource == null && cameraRig != null && cameraRig.ControlledCamera != null)
        {
            firstPersonLookSource = cameraRig.ControlledCamera.transform;
        }
    }

    private void EnsureFlashlight()
    {
        if (lightAnchor == null)
        {
            Transform headAnchor = transform.Find("HeadAnchor");
            lightAnchor = headAnchor != null ? headAnchor : transform;
        }

        if (flashlight == null)
        {
            Transform existing = lightAnchor.Find("PlayerFlashlight");
            if (existing != null)
            {
                flashlight = existing.GetComponent<Light>();
            }

            if (flashlight == null)
            {
                GameObject lightObject = new GameObject("PlayerFlashlight");
                lightObject.transform.SetParent(lightAnchor, false);
                flashlight = lightObject.AddComponent<Light>();
            }
        }

        if (flashlight == null)
        {
            return;
        }

        if (flashlight.transform.parent != lightAnchor)
        {
            flashlight.transform.SetParent(lightAnchor, false);
        }

        flashlight.transform.localPosition = localPosition;
        flashlight.transform.localRotation = Quaternion.Euler(localEuler);
        flashlight.type = LightType.Spot;
        flashlight.color = lightColor;
        flashlight.intensity = intensity;
        flashlight.range = range;
        flashlight.spotAngle = spotAngle;
        flashlight.innerSpotAngle = Mathf.Clamp(innerSpotAngle, 1f, spotAngle - 1f);
        flashlight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
        flashlight.renderMode = LightRenderMode.Auto;
        UpdateLightTransform();
    }

    private void UpdateLightTransform()
    {
        if (flashlight == null || lightAnchor == null)
        {
            return;
        }

        flashlight.transform.localPosition = localPosition;

        bool firstPerson = cameraRig != null && cameraRig.FirstPersonBlend > 0.55f;
        if (firstPerson && firstPersonLookSource != null)
        {
            flashlight.transform.rotation = firstPersonLookSource.rotation * Quaternion.Euler(localEuler);
            return;
        }

        flashlight.transform.localRotation = Quaternion.Euler(localEuler);
    }

    private void SetEnabled(bool enabled)
    {
        isOn = enabled;
        if (flashlight != null)
        {
            flashlight.enabled = enabled;
        }
    }
}
