using System.Collections;
using UnityEngine;

/// <summary>
/// A physically represented hollow container. The shell stays static, the visible lid
/// is the moving blocker, and contents are unavailable until the lid is fully open.
/// </summary>
public sealed class PhysicalChestInteractable : InteractableBase
{
    [SerializeField] private Transform lidPivot;
    [SerializeField] private BoxCollider lidCollider;
    [SerializeField] private GameObject contentsRoot;
    [SerializeField] private float openAngleDegrees = 105f;
    [SerializeField] private float animationDuration = 0.55f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly Collider[] overlapBuffer = new Collider[12];
    private Quaternion closedRotation;
    private bool isOpen;
    private bool isAnimating;
    private PlayerInteraction activeInteractor;

    public bool IsOpen => isOpen;

    public void Configure(Transform physicalLidPivot, BoxCollider physicalLidCollider, GameObject concealedContents, float openAngle = 105f, float duration = 0.55f)
    {
        lidPivot = physicalLidPivot;
        lidCollider = physicalLidCollider;
        contentsRoot = concealedContents;
        openAngleDegrees = Mathf.Clamp(openAngle, 70f, 125f);
        animationDuration = Mathf.Max(0.1f, duration);
        CaptureClosedPose();

        if (lidCollider != null)
        {
            lidCollider.enabled = true;
            lidCollider.isTrigger = false;
        }

        if (contentsRoot != null)
        {
            contentsRoot.SetActive(isOpen);
        }
    }

    private void Awake()
    {
        CaptureClosedPose();
        if (lidCollider != null)
        {
            lidCollider.enabled = true;
            lidCollider.isTrigger = false;
        }

        if (contentsRoot != null)
        {
            contentsRoot.SetActive(isOpen);
        }
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        return isOpen
            ? $"[{InputReader.GetInteractLabel()}] Close chest"
            : $"[{InputReader.GetInteractLabel()}] Open chest";
    }

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (interactor == null || lidPivot == null || lidCollider == null || isAnimating)
        {
            return;
        }

        StartCoroutine(AnimateLid(interactor, !isOpen));
    }

    private IEnumerator AnimateLid(PlayerInteraction interactor, bool targetOpen)
    {
        isAnimating = true;
        activeInteractor = interactor;
        interactor.SetBusy(true);

        if (!targetOpen && contentsRoot != null)
        {
            contentsRoot.SetActive(false);
        }

        Quaternion fromRotation = lidPivot.localRotation;
        Quaternion targetRotation = targetOpen
            ? closedRotation * Quaternion.Euler(openAngleDegrees, 0f, 0f)
            : closedRotation;

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, animationDuration);
        while (elapsed < duration)
        {
            Quaternion previousRotation = lidPivot.localRotation;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = animationCurve != null ? animationCurve.Evaluate(t) : t;
            lidPivot.localRotation = Quaternion.Slerp(fromRotation, targetRotation, eased);
            Physics.SyncTransforms();

            if (LidOverlapsCharacter())
            {
                lidPivot.localRotation = previousRotation;
                Physics.SyncTransforms();
                if (contentsRoot != null)
                {
                    contentsRoot.SetActive(isOpen);
                }

                interactor.ShowTransientMessage("The lid is blocked.", 1.25f);
                ReleaseInteractor();
                yield break;
            }

            yield return null;
        }

        lidPivot.localRotation = targetRotation;
        Physics.SyncTransforms();
        isOpen = targetOpen;

        if (contentsRoot != null)
        {
            contentsRoot.SetActive(isOpen);
        }

        ReleaseInteractor();
        ForceRefreshPrompt(interactor, interactor.Inventory);
    }

    private bool LidOverlapsCharacter()
    {
        if (lidCollider == null || !lidCollider.enabled)
        {
            return false;
        }

        Transform colliderTransform = lidCollider.transform;
        Vector3 lossy = colliderTransform.lossyScale;
        Vector3 halfExtents = Vector3.Scale(
            lidCollider.size * 0.5f,
            new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z)));
        Vector3 center = colliderTransform.TransformPoint(lidCollider.center);

        // Use all layers: the canonical player can intentionally live on Ignore Raycast,
        // but physical obstruction checks must still see the CharacterController.
        int count = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            overlapBuffer,
            colliderTransform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider candidate = overlapBuffer[i];
            if (candidate == null || candidate == lidCollider)
            {
                continue;
            }

            CharacterController character = candidate.GetComponentInParent<CharacterController>();
            if (character != null && character.enabled)
            {
                return true;
            }
        }

        return false;
    }

    private void CaptureClosedPose()
    {
        if (lidPivot != null)
        {
            closedRotation = lidPivot.localRotation;
        }
    }

    private void ReleaseInteractor()
    {
        if (activeInteractor != null)
        {
            activeInteractor.SetBusy(false);
        }

        activeInteractor = null;
        isAnimating = false;
    }

    private void OnDisable()
    {
        ReleaseInteractor();
    }

    private void OnDestroy()
    {
        ReleaseInteractor();
    }
}
