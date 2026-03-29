using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraForwardSource;
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private InspectItemViewer inspectViewer;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4.2f;
    [SerializeField] private float sprintSpeed = 6.2f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 24f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -28f;
    [SerializeField] private float jumpHeight = 1.15f;
    [SerializeField] private float jumpCoyoteTime = 0.12f;
    [SerializeField] private float groundedStickForce = -4f;
    [SerializeField] private float terminalVelocity = -45f;

    [Header("Grounding")]
    [SerializeField] private bool snapToGroundOnStart = true;
    [SerializeField] private float startGroundProbeDistance = 4f;
    [SerializeField] private float startGroundPadding = 0.04f;
    [SerializeField] private LayerMask groundSnapMask = ~0;

    private CharacterController controller;
    private PlayerInteraction playerInteraction;
    private Vector3 velocity;
    private Vector3 planarVelocity;
    private float lastGroundedTime = -999f;
    private bool jumpQueued;

    public float CurrentPlanarSpeed => planarVelocity.magnitude;
    public Vector3 CurrentPlanarVelocity => planarVelocity;

    public void SetCameraForwardSource(Transform source)
    {
        cameraForwardSource = source;
    }

    public void ConfigureRuntimeReferences(HolstinCameraRig configuredCameraRig, InspectItemViewer configuredInspectViewer)
    {
        if (configuredCameraRig != null)
        {
            cameraRig = configuredCameraRig;
        }

        if (configuredInspectViewer != null)
        {
            inspectViewer = configuredInspectViewer;
        }
    }

    public void ResetMotion()
    {
        velocity = Vector3.zero;
        planarVelocity = Vector3.zero;
        jumpQueued = false;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInteraction = GetComponent<PlayerInteraction>();
        ResolveSceneReferences(false, false);

        NormalizeControllerAlignment();
        controller.minMoveDistance = 0f;
        controller.stepOffset = 0.35f;
        controller.skinWidth = 0.03f;
        controller.slopeLimit = 45f;
    }

    private void Start()
    {
        ResolveSceneReferences(true, false);

        if (snapToGroundOnStart)
        {
            SnapToGroundIfPossible();
        }
    }

    private void Update()
    {
        if (cameraRig == null || inspectViewer == null)
        {
            ResolveSceneReferences(true, false);
        }

        if (!CanUseController())
        {
            return;
        }

        if ((inspectViewer != null && inspectViewer.IsInspecting) || (playerInteraction != null && playerInteraction.IsBusy))
        {
            planarVelocity = Vector3.zero;
            jumpQueued = false;
            ApplyGravityOnly();
            return;
        }

        Vector2 input = InputReader.GetMoveVector();
        bool sprint = InputReader.SprintHeld();
        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (InputReader.JumpPressed())
        {
            jumpQueued = true;
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        if (cameraForwardSource != null)
        {
            forward = Vector3.ProjectOnPlane(cameraForwardSource.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(cameraForwardSource.right, Vector3.up).normalized;
        }

        Vector3 move = (right * input.x) + (forward * input.y);
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        bool inFirstPerson = cameraRig != null && cameraRig.IsInFirstPerson;
        float speed = sprint ? sprintSpeed : walkSpeed;
        Vector3 targetPlanarVelocity = move * speed;
        float response = (move.sqrMagnitude > 0.0001f ? acceleration : deceleration) * Time.deltaTime;
        planarVelocity = Vector3.MoveTowards(planarVelocity, targetPlanarVelocity, Mathf.Max(0f, response));

        if (!CanUseController())
        {
            return;
        }

        controller.Move(planarVelocity * Time.deltaTime);

        if (!inFirstPerson && planarVelocity.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(planarVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        ApplyGravityOnly();
    }

    private void ResolveSceneReferences(bool allowFallbackLookup, bool logFallbackWarnings)
    {
        if (HolstinSceneContext.TryGet(out HolstinSceneContext context))
        {
            if (cameraRig == null)
            {
                cameraRig = context.CameraRig;
            }

            if (inspectViewer == null)
            {
                inspectViewer = context.InspectViewer;
            }
        }

        if (!allowFallbackLookup)
        {
            return;
        }

        if (cameraRig == null)
        {
            cameraRig = FindInScene<HolstinCameraRig>(gameObject.scene);
            if (cameraRig != null && logFallbackWarnings)
            {
                Debug.LogWarning("BOOTSTRAP_FALLBACK: PlayerMover resolved HolstinCameraRig via FindAnyObjectByType.");
            }
        }

        if (inspectViewer == null)
        {
            inspectViewer = FindInScene<InspectItemViewer>(gameObject.scene);
            if (inspectViewer != null && logFallbackWarnings)
            {
                Debug.LogWarning("BOOTSTRAP_FALLBACK: PlayerMover resolved InspectItemViewer via FindAnyObjectByType.");
            }
        }
    }

    private void ApplyGravityOnly()
    {
        if (!CanUseController())
        {
            return;
        }

        bool grounded = controller.isGrounded;
        if (grounded)
        {
            lastGroundedTime = Time.time;
        }

        bool canJump = jumpQueued && (grounded || (Time.time - lastGroundedTime) <= Mathf.Max(0f, jumpCoyoteTime));
        if (canJump)
        {
            float jumpSpeed = Mathf.Sqrt(Mathf.Max(0.01f, jumpHeight) * Mathf.Abs(gravity) * 2f);
            velocity.y = jumpSpeed;
            jumpQueued = false;
            grounded = false;
        }
        else if (grounded && velocity.y < 0f)
        {
            velocity.y = groundedStickForce;
        }

        if (!grounded || velocity.y > groundedStickForce)
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        }

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    private bool CanUseController()
    {
        return controller != null &&
               controller.enabled &&
               controller.gameObject != null &&
               controller.gameObject.activeInHierarchy &&
               isActiveAndEnabled;
    }

    private void NormalizeControllerAlignment()
    {
        if (controller == null)
        {
            return;
        }

        // Keep the root pivot at feet for stable spawn/grounding, regardless of whether
        // the renderers live on this GameObject or on children.
        controller.height = Mathf.Max(1.8f, controller.height);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
    }

    private void SnapToGroundIfPossible()
    {
        if (controller == null)
        {
            return;
        }

        bool wasEnabled = controller.enabled;
        if (wasEnabled)
        {
            controller.enabled = false;
        }

        float probeLift = Mathf.Max(1.5f, controller.height + 0.5f);
        Vector3 rayOrigin = transform.position + Vector3.up * probeLift;
        float rayDistance = probeLift + Mathf.Max(1f, startGroundProbeDistance);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, groundSnapMask, QueryTriggerInteraction.Ignore))
        {
            float bottomOffset = controller.center.y - (controller.height * 0.5f);
            float desiredY = hit.point.y - bottomOffset + Mathf.Max(startGroundPadding, controller.skinWidth);
            float delta = desiredY - transform.position.y;
            if (Mathf.Abs(delta) <= Mathf.Max(0.5f, startGroundProbeDistance))
            {
                transform.position = new Vector3(transform.position.x, desiredY, transform.position.z);
                velocity = Vector3.zero;
            }
        }

        if (wasEnabled)
        {
            controller.enabled = true;
        }
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
