using System.Collections;
using UnityEngine;

public class DoorInteractable : InteractableBase
{
    public enum DoorMotionType
    {
        Slide,
        Swing
    }

    [Header("Door")]
    [SerializeField] private Transform movingPart;
    [SerializeField] private DoorMotionType motionType = DoorMotionType.Slide;
    [SerializeField] private Vector3 openLocalPositionOffset = new Vector3(1.6f, 0f, 0f);
    [SerializeField] private Vector3 openLocalEuler = new Vector3(0f, 110f, 0f);
    [SerializeField] private float animationDuration = 0.55f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Lock")]
    [SerializeField] private bool startsLocked;
    [SerializeField] private string requiredItemId = "old_key";
    [SerializeField] private string requiredItemDisplayName = "Old Key";
    [SerializeField] private bool consumeRequiredItem;
    [SerializeField] private string infectionMilestoneOnUnlock;
    [SerializeField] [TextArea(2, 4)] private string lockedMessage = "Locked. It needs a key.";

    [Header("Audio")]
    [SerializeField] private AudioClip lockedAttemptSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip toggleDoorSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;

    private Vector3 closedLocalPosition;
    private Quaternion closedLocalRotation;
    private bool isOpen;
    private bool isAnimating;
    private bool isLocked;
    private PlayerInteraction activeInteractor;
    private Collider[] movingColliders;

    public void ConfigureLock(string requiredId, string requiredDisplayName, bool startsLockedAtRuntime, bool consumeKey, string unlockMilestone = "")
    {
        if (!string.IsNullOrWhiteSpace(requiredId))
        {
            requiredItemId = requiredId;
        }

        if (!string.IsNullOrWhiteSpace(requiredDisplayName))
        {
            requiredItemDisplayName = requiredDisplayName;
        }

        startsLocked = startsLockedAtRuntime;
        isLocked = startsLockedAtRuntime;
        consumeRequiredItem = consumeKey;
        infectionMilestoneOnUnlock = unlockMilestone;
    }

    public void ConfigureMotion(
        Transform part,
        DoorMotionType type,
        Vector3 openPositionOffset,
        Vector3 openEuler,
        float duration)
    {
        movingPart = part != null ? part : transform;
        motionType = type;
        openLocalPositionOffset = openPositionOffset;
        openLocalEuler = openEuler;
        animationDuration = Mathf.Max(0.05f, duration);
        CaptureClosedPose();
        CacheMovingColliders();
    }

    private void Awake()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        CaptureClosedPose();
        CacheMovingColliders();
        isLocked = startsLocked;
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (isLocked)
        {
            bool hasRequiredItem = (inventory != null && inventory.HasItem(requiredItemId)) ||
                                   (SliceState.TryGet(out SliceState state) && state.HasKeyItem(requiredItemId));
            if (hasRequiredItem)
            {
                return $"[{InputReader.GetInteractLabel()}] Unlock door with {requiredItemDisplayName}";
            }

            return $"[{InputReader.GetInteractLabel()}] Try locked door";
        }

        return isOpen ? $"[{InputReader.GetInteractLabel()}] Close door" : $"[{InputReader.GetInteractLabel()}] Open door";
    }

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (isAnimating || movingPart == null || interactor == null)
        {
            return;
        }

        bool unlockedNow = false;

        if (isLocked)
        {
            bool hasRequiredItem = (inventory != null && inventory.HasItem(requiredItemId)) ||
                                   (SliceState.TryGet(out SliceState state) && state.HasKeyItem(requiredItemId));
            if (!hasRequiredItem)
            {
                interactor.ShowTransientMessage(lockedMessage, 2.2f);
                HolstinAudio.PlayOneShot(lockedAttemptSound, transform, soundVolume);
                return;
            }

            if (consumeRequiredItem)
            {
                inventory.TryConsumeItem(requiredItemId, 1);
                interactor.ShowTransientMessage($"Used and lost: {requiredItemDisplayName}", 2f);
            }
            else
            {
                interactor.ShowTransientMessage($"Used: {requiredItemDisplayName}", 2f);
            }

            isLocked = false;
            unlockedNow = true;
            HolstinAudio.PlayOneShot(unlockSound, transform, soundVolume);
        }

        if (unlockedNow && !string.IsNullOrWhiteSpace(infectionMilestoneOnUnlock))
        {
            if (SliceState.TryGet(out SliceState sliceState))
            {
                sliceState.MarkMilestone(infectionMilestoneOnUnlock);
                sliceState.SetCurrentObjective("npc_reward_key");
            }

            InfectionDirector.NotifyMilestoneGlobal(infectionMilestoneOnUnlock);
        }

        HolstinAudio.PlayOneShot(toggleDoorSound, transform, soundVolume);
        StartCoroutine(AnimateDoorRoutine(interactor));
    }

    private IEnumerator AnimateDoorRoutine(PlayerInteraction interactor)
    {
        isAnimating = true;
        activeInteractor = interactor;
        interactor.SetBusy(true);

        Vector3 fromPosition = movingPart.localPosition;
        Quaternion fromRotation = movingPart.localRotation;
        Vector3 targetPosition = isOpen ? closedLocalPosition : closedLocalPosition + openLocalPositionOffset;
        Quaternion targetRotation = isOpen ? closedLocalRotation : closedLocalRotation * Quaternion.Euler(openLocalEuler);

        float duration = Mathf.Max(0.05f, animationDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 previousPosition = movingPart.localPosition;
            Quaternion previousRotation = movingPart.localRotation;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = animationCurve != null ? animationCurve.Evaluate(t) : t;

            if (motionType == DoorMotionType.Slide)
            {
                movingPart.localPosition = Vector3.Lerp(fromPosition, targetPosition, curveT);
                movingPart.localRotation = fromRotation;
            }
            else
            {
                movingPart.localPosition = fromPosition;
                movingPart.localRotation = Quaternion.Slerp(fromRotation, targetRotation, curveT);
            }

            if (IntersectsInteractor(interactor))
            {
                movingPart.localPosition = previousPosition;
                movingPart.localRotation = previousRotation;
                AbortAnimation(interactor);
                yield break;
            }

            yield return null;
        }

        if (motionType == DoorMotionType.Slide)
        {
            movingPart.localPosition = targetPosition;
        }
        else
        {
            movingPart.localRotation = targetRotation;
        }

        if (IntersectsInteractor(interactor))
        {
            movingPart.localPosition = fromPosition;
            movingPart.localRotation = fromRotation;
            AbortAnimation(interactor);
            yield break;
        }

        isOpen = !isOpen;
        isAnimating = false;
        interactor.SetBusy(false);
        activeInteractor = null;
        ForceRefreshPrompt(interactor, interactor.Inventory);
    }

    private void CaptureClosedPose()
    {
        if (movingPart == null)
        {
            return;
        }

        closedLocalPosition = movingPart.localPosition;
        closedLocalRotation = movingPart.localRotation;
    }

    private void CacheMovingColliders()
    {
        movingColliders = movingPart != null
            ? movingPart.GetComponentsInChildren<Collider>(true)
            : null;
    }

    private bool IntersectsInteractor(PlayerInteraction interactor)
    {
        if (interactor == null || movingColliders == null || movingColliders.Length == 0)
        {
            return false;
        }

        CharacterController controller = interactor.GetComponent<CharacterController>();
        if (controller == null || !controller.enabled)
        {
            return false;
        }

        Bounds actorBounds = controller.bounds;
        for (int i = 0; i < movingColliders.Length; i++)
        {
            Collider doorCollider = movingColliders[i];
            if (doorCollider == null || !doorCollider.enabled || doorCollider.isTrigger)
            {
                continue;
            }

            if (doorCollider.bounds.Intersects(actorBounds))
            {
                return true;
            }
        }

        return false;
    }

    private void AbortAnimation(PlayerInteraction interactor)
    {
        isAnimating = false;
        if (interactor != null)
        {
            interactor.SetBusy(false);
            ForceRefreshPrompt(interactor, interactor.Inventory);
        }

        activeInteractor = null;
    }

    private void OnDisable()
    {
        ReleaseBusyIfAnimating();
    }

    private void OnDestroy()
    {
        ReleaseBusyIfAnimating();
    }

    private void ReleaseBusyIfAnimating()
    {
        if (!isAnimating)
        {
            return;
        }

        if (activeInteractor != null)
        {
            activeInteractor.SetBusy(false);
        }

        activeInteractor = null;
        isAnimating = false;
    }
}
