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

    private void Awake()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        closedLocalPosition = movingPart.localPosition;
        closedLocalRotation = movingPart.localRotation;
        isLocked = startsLocked;
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (isLocked)
        {
            if (inventory != null && inventory.HasItem(requiredItemId))
            {
                return $"[{InputReader.InteractKeyLabel}] Unlock door with {requiredItemDisplayName}";
            }

            return $"[{InputReader.InteractKeyLabel}] Try locked door";
        }

        return isOpen ? $"[{InputReader.InteractKeyLabel}] Close door" : $"[{InputReader.InteractKeyLabel}] Open door";
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
            if (inventory == null || !inventory.HasItem(requiredItemId))
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

        isOpen = !isOpen;
        isAnimating = false;
        interactor.SetBusy(false);
        activeInteractor = null;
        ForceRefreshPrompt(interactor, interactor.Inventory);
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
