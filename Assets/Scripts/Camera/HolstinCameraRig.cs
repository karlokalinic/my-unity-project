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
    [SerializeField] private float aimBlendSpeed = 8f;
    [SerializeField] private float aimEnterBlendSpeed = 9f;
    [SerializeField] private float aimExitBlendSpeed = 8f;
    [SerializeField] private bool instantFirstPersonTransition = false;
    [SerializeField] private float firstPersonFov = 72f;
    [SerializeField] private float firstPersonHeightFromFeet = 1.68f;
    [SerializeField] private float firstPersonMinimumHeightFromFeet = 1.56f;
    [SerializeField] private float firstPersonForwardOffset = 0.03f;
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
    [Tooltip("Disable obstruction push in isometric mode - use wall fading instead.")]
    [SerializeField] private bool disableObstructionInIsometric = true;

    [Header("Transition Collision")]
    [SerializeField] private LayerMask transitionCollisionMask = ~0;
    [SerializeField] private float transitionCollisionRadius = 0.17f;
    [SerializeField] private float transitionCollisionPadding = 0.04f;

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
    private bool snappedToInitialIsometric;
    private bool previousWantsAim;

    public bool IsInFirstPerson => aimBlend > 0.6f;
    public float FirstPersonBlend => aimBlend;
    public bool IsInDialogueShot => dialogueBlend > 0.05f || dialogueBlendTarget > 0.05f;
    public Transform CameraTransform => cameraTransform;
    public Camera ControlledCamera => controlledCamera;

    public void Configure(Transform targetTransform, Transform fpAnchor, Transform camTransform, Camera cam)
    {
        target = targetTransform;
        firstPersonAnchor = fpAnchor;
        cameraTransform = camTransform;
        controlledCamera = cam;
        EnsureFirstPersonAnchorExists();
    }

    public void ConfigureAimTransition(bool instant, float enterSpeed, float exitSpeed)
    {
        instantFirstPersonTransition = instant;
        aimEnterBlendSpeed = Mathf.Max(0.1f, enterSpeed);
        aimExitBlendSpeed = Mathf.Max(0.1f, exitSpeed);
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
        ResolveMissingReferences();

        currentYaw = zoneYaw;
        targetYaw = zoneYaw;
        aimBlend = 0f;

        if (target != null)
        {
            lookYaw = target.eulerAngles.y;
        }

        SnapToIsometricImmediately();
    }

    private void Update()
    {
        ResolveMissingReferences();
        if (target == null || cameraTransform == null)
        {
            return;
        }

        if (!snappedToInitialIsometric)
        {
            SnapToIsometricImmediately();
        }

        HandleInput();
        HandleAimLook();
    }

    private void LateUpdate()
    {
        ResolveMissingReferences();
        if (target == null || cameraTransform == null)
        {
            return;
        }

        if (!snappedToInitialIsometric)
        {
            SnapToIsometricImmediately();
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
        float speed = wantsAim ? Mathf.Max(aimBlendSpeed, aimEnterBlendSpeed) : Mathf.Max(aimBlendSpeed, aimExitBlendSpeed);
        if (instantFirstPersonTransition && wantsAim != previousWantsAim)
        {
            aimBlend = targetBlend;
        }
        else
        {
            aimBlend = Mathf.MoveTowards(aimBlend, targetBlend, speed * Time.deltaTime);
        }

        previousWantsAim = wantsAim;
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

        Quaternion fpRotation = Quaternion.Euler(lookPitch, lookYaw, 0f);
        Vector3 fpPosition = ResolveFirstPersonPosition(fpRotation, focusPoint);

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

        Vector3 constrainedDesiredPosition = ResolveTransitionCollisionFromFocus(focusPoint, desiredPosition);
        Vector3 smoothedPosition = Vector3.Lerp(cameraTransform.position, constrainedDesiredPosition, positionBlend);
        Vector3 collisionSafePosition = ResolveTransitionCollision(cameraTransform.position, smoothedPosition);
        collisionSafePosition = ResolveTransitionCollisionFromFocus(focusPoint, collisionSafePosition);
        cameraTransform.position = collisionSafePosition;
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

    private Vector3 ResolveTransitionCollision(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= 0.001f)
        {
            return to;
        }

        Vector3 direction = delta / distance;
        RaycastHit[] hits = Physics.SphereCastAll(
            from,
            Mathf.Max(0.01f, transitionCollisionRadius),
            direction,
            distance + transitionCollisionPadding,
            transitionCollisionMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            return to;
        }

        System.Array.Sort(hits, static (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (ShouldIgnoreTransitionCollider(hitCollider))
            {
                continue;
            }

            float safeDistance = Mathf.Max(0f, hits[i].distance - transitionCollisionPadding);
            return from + (direction * safeDistance);
        }

        return to;
    }

    private Vector3 ResolveTransitionCollisionFromFocus(Vector3 focusPoint, Vector3 desiredPosition)
    {
        Vector3 offset = desiredPosition - focusPoint;
        float distance = offset.magnitude;
        if (distance <= 0.001f)
        {
            return desiredPosition;
        }

        Vector3 direction = offset / distance;
        RaycastHit[] hits = Physics.SphereCastAll(
            focusPoint,
            Mathf.Max(0.01f, transitionCollisionRadius),
            direction,
            distance + transitionCollisionPadding,
            transitionCollisionMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            return desiredPosition;
        }

        System.Array.Sort(hits, static (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (ShouldIgnoreTransitionCollider(hitCollider))
            {
                continue;
            }

            float safeDistance = Mathf.Max(0f, hits[i].distance - transitionCollisionPadding);
            return focusPoint + (direction * safeDistance);
        }

        return desiredPosition;
    }

    private bool ShouldIgnoreTransitionCollider(Collider collider)
    {
        if (collider == null)
        {
            return true;
        }

        Transform hitTransform = collider.transform;
        if (target != null && hitTransform.IsChildOf(target))
        {
            return true;
        }

        if (cameraTransform != null && hitTransform.IsChildOf(cameraTransform))
        {
            return true;
        }

        return false;
    }

    private void ResolveMissingReferences()
    {
        if (target == null)
        {
            if (HolstinSceneContext.TryGet(out HolstinSceneContext context) &&
                context != null &&
                context.gameObject.scene == gameObject.scene &&
                context.PlayerTransform != null)
            {
                target = context.PlayerTransform;
            }
            else
            {
                PlayerMover playerMover = FindInScene<PlayerMover>(gameObject.scene);
                if (playerMover != null)
                {
                    target = playerMover.transform;
                }
            }
        }

        if (cameraTransform == null)
        {
            if (controlledCamera != null)
            {
                cameraTransform = controlledCamera.transform;
            }
            else if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        if (controlledCamera == null && cameraTransform != null)
        {
            controlledCamera = cameraTransform.GetComponent<Camera>();
        }

        EnsureFirstPersonAnchorExists();
    }

    private void EnsureFirstPersonAnchorExists()
    {
        if (target == null)
        {
            return;
        }

        if (firstPersonAnchor == null)
        {
            Transform headAnchor = target.Find("HeadAnchor");
            if (headAnchor == null)
            {
                GameObject anchorObject = new GameObject("HeadAnchor");
                headAnchor = anchorObject.transform;
                headAnchor.SetParent(target, false);
            }

            firstPersonAnchor = headAnchor;
        }

        if (firstPersonAnchor != null && firstPersonAnchor.parent == target)
        {
            Vector3 local = firstPersonAnchor.localPosition;
            float minHeight = Mathf.Max(1f, firstPersonMinimumHeightFromFeet);
            local.x = 0f;
            local.y = Mathf.Max(local.y, minHeight);
            local.z = Mathf.Max(local.z, 0.02f);
            firstPersonAnchor.localPosition = local;
        }
    }

    private Vector3 ResolveFirstPersonPosition(Quaternion fpRotation, Vector3 focusPoint)
    {
        Vector3 fallbackPosition = target != null
            ? target.position + Vector3.up * Mathf.Max(1f, firstPersonHeightFromFeet)
            : focusPoint;

        Vector3 resolved = firstPersonAnchor != null ? firstPersonAnchor.position : fallbackPosition;
        if (target != null)
        {
            float minWorldY = target.position.y + Mathf.Max(1f, firstPersonMinimumHeightFromFeet);
            if (resolved.y < minWorldY)
            {
                resolved = fallbackPosition;
            }
        }

        return resolved + (fpRotation * new Vector3(0f, 0f, firstPersonForwardOffset));
    }

    private void SnapToIsometricImmediately()
    {
        if (target == null || cameraTransform == null)
        {
            return;
        }

        Vector3 focusPoint = target.position + Vector3.up * focusHeight;
        Vector3 isoOffset = Quaternion.Euler(0f, zoneYaw, 0f) * new Vector3(0f, isoHeight, -isoDistance);
        Vector3 isoPosition = focusPoint + isoOffset;
        Quaternion isoRotation = Quaternion.LookRotation((focusPoint - isoPosition).normalized, Vector3.up);
        cameraTransform.SetPositionAndRotation(isoPosition, isoRotation);

        if (controlledCamera != null)
        {
            controlledCamera.fieldOfView = isoFov;
        }

        currentYaw = zoneYaw;
        targetYaw = zoneYaw;
        lookYaw = target.eulerAngles.y;
        lookPitch = 0f;
        dialogueBlend = 0f;
        dialogueBlendTarget = 0f;
        dialogueAnchor = null;
        snappedToInitialIsometric = true;
    }

    private static T FindInScene<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
    {
        T[] components = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Exclude);
        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];
            if (component != null && component.gameObject.scene == scene)
            {
                return component;
            }
        }

        return null;
    }
}



