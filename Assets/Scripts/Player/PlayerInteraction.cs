using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private InspectItemViewer inspectViewer;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private InteractionPromptUI promptUI;
    [SerializeField] private Transform pickupAnchor;
    [SerializeField] private PlayerReachController reachController;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float interactRadius = 2.3f;
    [SerializeField] private LayerMask interactionMask = ~0;
    [SerializeField] private float minimumFacingDot = -0.2f;
    [SerializeField] private float interactionReachDuration = 0.26f;

    [Header("Combat")]
    [SerializeField] private float fireDistance = 100f;
    [SerializeField] private LayerMask fireMask = ~0;
    [SerializeField] private float fireCooldown = 0.16f;
    [SerializeField] private float rigidbodyImpactForce = 8f;

    [Header("Fail-Safes")]
    [SerializeField] private bool autoExitInspectOnMovementIntent = true;
    [SerializeField] private bool busyStateFailsafe = true;
    [SerializeField] private float busyFailsafeSeconds = 2.5f;

    private readonly Collider[] overlapResults = new Collider[48];
    private float nextFireTime;
    private float busyEnteredTime;
    private InteractableBase currentInteractable;
    private InspectableItem currentInspectable;
    private bool isBusy;
    private bool transientBusy;
    private Coroutine interactionRoutine;
    private InteractionScanner interactionScanner;
    private InteractionExecutor interactionExecutor;
    private CombatController combatController;

    public PlayerInventory Inventory => inventory;
    public bool IsBusy => isBusy || transientBusy;

    public void Configure(Camera cam, HolstinCameraRig rig, InspectItemViewer viewer)
    {
        viewCamera = cam;
        cameraRig = rig;
        inspectViewer = viewer;
    }

    public void ConfigureRuntimeReferences(PlayerInventory configuredInventory, InteractionPromptUI configuredPromptUI, Transform configuredPickupAnchor = null)
    {
        if (configuredInventory != null)
        {
            inventory = configuredInventory;
        }

        if (configuredPromptUI != null)
        {
            promptUI = configuredPromptUI;
        }

        if (configuredPickupAnchor != null)
        {
            pickupAnchor = configuredPickupAnchor;
        }
        else if (pickupAnchor == null && viewCamera != null)
        {
            pickupAnchor = viewCamera.transform;
        }
    }

    private void Awake()
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

            if (promptUI == null)
            {
                promptUI = context.PromptUI;
            }

            if (context.CameraRig != null && viewCamera == null)
            {
                viewCamera = context.CameraRig.ControlledCamera;
            }
        }

        if (viewCamera == null && Camera.main != null)
        {
            viewCamera = Camera.main;
        }

        if (cameraRig == null)
        {
            cameraRig = FindAnyObjectByType<HolstinCameraRig>();
        }

        if (inspectViewer == null)
        {
            inspectViewer = FindAnyObjectByType<InspectItemViewer>();
        }

        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        if (promptUI == null)
        {
            promptUI = FindAnyObjectByType<InteractionPromptUI>();
        }

        if (pickupAnchor == null)
        {
            pickupAnchor = viewCamera != null ? viewCamera.transform : transform;
        }

        if (reachController == null)
        {
            reachController = GetComponent<PlayerReachController>();
        }

        interactionScanner = new InteractionScanner(overlapResults);
        interactionExecutor = new InteractionExecutor();
        combatController = new CombatController();
    }

    private void Update()
    {
        if (viewCamera == null || cameraRig == null)
        {
            return;
        }

        if (GameplayPauseFacade.IsPaused && !IsBusy)
        {
            promptUI?.HidePrompt();
            return;
        }

        if (IsBusy)
        {
            if (isBusy)
            {
                TryReleaseBusyFailsafe();
            }
            promptUI?.HidePrompt();
            return;
        }

        if (inspectViewer != null && inspectViewer.IsInspecting)
        {
            promptUI?.HidePrompt();
            bool movementIntent = autoExitInspectOnMovementIntent && InputReader.GetMoveVector().sqrMagnitude > 0.01f;
            if (InputReader.CancelPressed() || InputReader.InteractPressed() || movementIntent)
            {
                inspectViewer.EndInspect();
            }
            return;
        }

        if (cameraRig.IsInFirstPerson)
        {
            promptUI?.HidePrompt();
            combatController.HandleCombat(viewCamera, fireDistance, fireMask, fireCooldown, rigidbodyImpactForce, ref nextFireTime);
            return;
        }

        RefreshContextPrompt();

        if (InputReader.InteractPressed())
        {
            TryInteract();
        }
    }

    public void RefreshContextPrompt()
    {
        interactionScanner.FindBestContextCandidate(
            transform,
            viewCamera,
            this,
            inventory,
            interactRadius,
            interactionMask,
            minimumFacingDot,
            out currentInteractable,
            out currentInspectable);

        if (promptUI == null)
        {
            return;
        }

        if (currentInteractable != null)
        {
            promptUI.ShowPrompt(currentInteractable.GetPrompt(this, inventory));
            return;
        }

        if (currentInspectable != null)
        {
            promptUI.ShowPrompt($"[{InputReader.InteractKeyLabel}] Inspect {currentInspectable.ItemName}");
            return;
        }

        promptUI.HidePrompt();
    }

    public Transform GetPickupAnchor()
    {
        return pickupAnchor != null ? pickupAnchor : (viewCamera != null ? viewCamera.transform : transform);
    }

    public void ShowTransientMessage(string text, float duration = 2.2f)
    {
        HolstinFeedback.ShowMessage(text, duration);
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;
        if (busy)
        {
            busyEnteredTime = Time.time;
            promptUI?.HidePrompt();
        }
    }

    private void TryReleaseBusyFailsafe()
    {
        if (!busyStateFailsafe || !Application.isPlaying || !isBusy)
        {
            return;
        }

        if (busyEnteredTime <= 0f || Time.time - busyEnteredTime < Mathf.Max(0.5f, busyFailsafeSeconds))
        {
            return;
        }

        bool inspectRunning = inspectViewer != null && inspectViewer.IsInspecting;
        DialoguePanelUI dialoguePanel = HolstinFeedback.ResolveDialoguePanel();
        bool dialogueRunning = dialoguePanel != null && dialoguePanel.IsShowing;

        if (inspectRunning || dialogueRunning)
        {
            busyEnteredTime = Time.time;
            return;
        }

        SetBusy(false);
    }

    private void TryInteract()
    {
        if (interactionRoutine != null)
        {
            return;
        }

        interactionRoutine = StartCoroutine(TryInteractRoutine());
    }

    private System.Collections.IEnumerator TryInteractRoutine()
    {
        if (reachController != null &&
            ReachTargetResolver.TryResolveTarget(this, currentInteractable, currentInspectable, out Vector3 reachPoint))
        {
            transientBusy = true;
            yield return reachController.PerformReach(reachPoint, interactionReachDuration);
            transientBusy = false;
        }

        interactionExecutor.TryInteract(
            viewCamera,
            interactDistance,
            interactionMask,
            this,
            inventory,
            currentInteractable,
            currentInspectable,
            inspectViewer);

        interactionRoutine = null;
    }

    private void OnDisable()
    {
        if (interactionRoutine != null)
        {
            StopCoroutine(interactionRoutine);
            interactionRoutine = null;
        }

        transientBusy = false;
    }
}
