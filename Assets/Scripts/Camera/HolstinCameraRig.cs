using UnityEngine;

public class HolstinCameraRig : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform firstPersonAnchor;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera controlledCamera;

    [Header("Isometric")]
    [SerializeField] private float isoDistance = 10f;
    [SerializeField] private float isoHeight = 7.5f;
    [SerializeField] private float positionSmooth = 10f;
    [SerializeField] private float rotationSmooth = 12f;
    [SerializeField] private float focusHeight = 1.3f;
    [SerializeField] private float isoFov = 50f;
    [SerializeField] private float zoneYaw = 45f;

    [Header("First Person / Aim")]
    [SerializeField] private bool holdRightMouseForAim = true;
    [SerializeField] private float aimBlendSpeed = 12f;
    [SerializeField] private float firstPersonFov = 72f;
    [SerializeField] private float mouseSensitivity = 0.07f;
    [SerializeField] private float minPitch = -75f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private bool rotateTargetWithAim = true;

    [Header("Occlusion")]
    [SerializeField] private LayerMask obstructionMask = ~0;
    [SerializeField] private float obstructionRadius = 0.18f;
    [SerializeField] private float obstructionPadding = 0.2f;
    [SerializeField] private float obstructionMinDistance = 2f;
    [SerializeField] private float obstructionSmoothSpeed = 6f;
    [Tooltip("Disable obstruction push in isometric mode — use wall fading instead.")]
    [SerializeField] private bool disableObstructionInIsometric = true;

    [Header("Dialogue Cinematic")]
    [SerializeField] private float defaultDialogueBlendSeconds = 0.45f;
    [SerializeField] private float defaultDialogueFov = 38f;

    private float currentYaw;
    private float targetYaw;
    private float aimBlend;
    private float lookYaw;
    private float lookPitch;
    private bool aimLatched;
    private Transform dialogueAnchor;
    private float dialogueBlend;
    private float dialogueBlendTarget;
    private float dialogueBlendSpeed = 1f;
    private float dialogueShotFov;
    private float currentObstructionDistance;

    public bool IsInFirstPerson => aimBlend > 0.6f;
    public bool IsInDialogueShot => dialogueBlend > 0.05f || dialogueBlendTarget > 0.05f;
    public Transform CameraTransform => cameraTransform;
    public Camera ControlledCamera => controlledCamera;

    public void Configure(Transform targetTransform, Transform fpAnchor, Transform camTransform, Camera cam)
    {
        target = targetTransform;
        firstPersonAnchor = fpAnchor;
        cameraTransform = camTransform;
        controlledCamera = cam;
    }

    public void SetIsometricYaw(float yaw, bool snapInstantly = false)
    {
        targetYaw = yaw;
        if (snapInstantly)
        {
            currentYaw = yaw;
        }
    }

    public void BeginDialogueShot(Transform anchor, float shotFov = 38f, float blendSeconds = 0.45f)
    {
        if (anchor == null)
        {
            return;
        }

        dialogueAnchor = anchor;
        dialogueShotFov = shotFov > 0f ? shotFov : defaultDialogueFov;
        dialogueBlendTarget = 1f;
        dialogueBlendSpeed = 1f / Mathf.Max(0.01f, blendSeconds > 0f ? blendSeconds : defaultDialogueBlendSeconds);
    }

    public void EndDialogueShot(float blendSeconds = 0.32f)
    {
        dialogueBlendTarget = 0f;
        dialogueBlendSpeed = 1f / Mathf.Max(0.01f, blendSeconds);
    }

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (controlledCamera == null && cameraTransform != null)
        {
            controlledCamera = cameraTransform.GetComponent<Camera>();
        }

        currentYaw = zoneYaw;
        targetYaw = zoneYaw;

        if (target != null)
        {
            lookYaw = target.eulerAngles.y;
        }
    }

    private void Update()
    {
        if (target == null || cameraTransform == null)
        {
            return;
        }

        HandleInput();
        HandleAimLook();
    }

    private void LateUpdate()
    {
        if (target == null || cameraTransform == null)
        {
            return;
        }

        UpdateCamera();
    }

    private void HandleInput()
    {
        if (GameplayPauseFacade.IsPaused)
        {
            return;
        }

        if (!holdRightMouseForAim && InputReader.AimTogglePressed())
        {
            aimLatched = !aimLatched;
        }

        bool wantsAim = holdRightMouseForAim ? InputReader.AimHeld() : aimLatched;
        if (!wantsAim)
        {
            if (InputReader.RotateLeftPressed())
            {
                SetIsometricYaw(targetYaw - 90f);
            }

            if (InputReader.RotateRightPressed())
            {
                SetIsometricYaw(targetYaw + 90f);
            }
        }

        float targetBlend = wantsAim ? 1f : 0f;
        aimBlend = Mathf.MoveTowards(aimBlend, targetBlend, aimBlendSpeed * Time.deltaTime);
    }

    private void HandleAimLook()
    {
        float yawBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationSmooth) * Time.deltaTime);
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, yawBlend);

        if (aimBlend <= 0.001f)
        {
            if (target != null)
            {
                lookYaw = target.eulerAngles.y;
            }
            return;
        }

        Vector2 look = InputReader.GetLookDelta();
        lookYaw += look.x * mouseSensitivity;
        lookPitch -= look.y * mouseSensitivity;
        lookPitch = Mathf.Clamp(lookPitch, minPitch, maxPitch);

        if (rotateTargetWithAim && target != null)
        {
            target.rotation = Quaternion.Euler(0f, lookYaw, 0f);
        }
    }

    private void UpdateCamera()
    {
        float positionBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, positionSmooth) * Time.deltaTime);
        float rotationBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationSmooth) * Time.deltaTime);
        Vector3 focusPoint = target.position + Vector3.up * focusHeight;

        Vector3 isoOffset = Quaternion.Euler(0f, currentYaw, 0f) * new Vector3(0f, isoHeight, -isoDistance);
        Vector3 isoIdealPosition = focusPoint + isoOffset;
        Vector3 isoSafePosition = (disableObstructionInIsometric && aimBlend < 0.5f)
            ? isoIdealPosition
            : ResolveObstruction(focusPoint, isoIdealPosition);
        Quaternion isoRotation = Quaternion.LookRotation((focusPoint - isoIdealPosition).normalized, Vector3.up);

        Vector3 fpPosition = firstPersonAnchor != null ? firstPersonAnchor.position : focusPoint;
        Quaternion fpRotation = Quaternion.Euler(lookPitch, lookYaw, 0f);

        Vector3 basePosition = Vector3.Lerp(isoSafePosition, fpPosition, aimBlend);
        Quaternion baseRotation = Quaternion.Slerp(isoRotation, fpRotation, aimBlend);
        float baseFov = Mathf.Lerp(isoFov, firstPersonFov, aimBlend);

        if (dialogueBlendTarget > 0f && dialogueAnchor == null)
        {
            dialogueBlendTarget = 0f;
        }

        dialogueBlend = Mathf.MoveTowards(dialogueBlend, dialogueBlendTarget, Mathf.Max(0.0001f, dialogueBlendSpeed) * Time.deltaTime);
        if (dialogueBlend <= 0.0001f && dialogueBlendTarget <= 0.0001f)
        {
            dialogueAnchor = null;
        }

        Vector3 desiredPosition = basePosition;
        Quaternion desiredRotation = baseRotation;
        float desiredFov = baseFov;
        if (dialogueAnchor != null && dialogueBlend > 0.0001f)
        {
            desiredPosition = Vector3.Lerp(basePosition, dialogueAnchor.position, dialogueBlend);
            desiredRotation = Quaternion.Slerp(baseRotation, dialogueAnchor.rotation, dialogueBlend);
            desiredFov = Mathf.Lerp(baseFov, dialogueShotFov > 0f ? dialogueShotFov : defaultDialogueFov, dialogueBlend);
        }

        cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, positionBlend);
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, desiredRotation, rotationBlend);

        if (controlledCamera != null)
        {
            controlledCamera.fieldOfView = desiredFov;
        }
    }

    private Vector3 ResolveObstruction(Vector3 focusPoint, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - focusPoint;
        float fullDistance = direction.magnitude;
        if (fullDistance <= 0.001f)
        {
            return desiredPosition;
        }

        direction /= fullDistance;
        float targetDistance = fullDistance;

        if (Physics.SphereCast(focusPoint, obstructionRadius, direction, out RaycastHit hit, fullDistance, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            float hitDistance = Mathf.Max(hit.distance - obstructionPadding, obstructionMinDistance);
            targetDistance = hitDistance;
        }

        currentObstructionDistance = Mathf.Lerp(currentObstructionDistance, targetDistance,
            1f - Mathf.Exp(-obstructionSmoothSpeed * Time.deltaTime));

        if (Mathf.Abs(currentObstructionDistance - fullDistance) < 0.01f)
        {
            currentObstructionDistance = fullDistance;
        }

        return focusPoint + direction * currentObstructionDistance;
    }
}
