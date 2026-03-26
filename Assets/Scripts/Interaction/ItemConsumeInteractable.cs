using UnityEngine;

public class ItemConsumeInteractable : InteractableBase
{
    [Header("Required Item")]
    [SerializeField] private string requiredItemId = "service_key";
    [SerializeField] private string requiredItemDisplayName = "Service Key";
    [SerializeField] private bool consumeItem = true;

    [Header("Outcome")]
    [SerializeField] [TextArea(2, 4)] private string successMessage = "The console accepts the key. Pathway unlocked.";
    [SerializeField] [TextArea(2, 4)] private string missingItemMessage = "The console requires a service key.";
    [SerializeField] private GameObject[] activateOnSuccess;
    [SerializeField] private GameObject[] deactivateOnSuccess;
    [SerializeField] private bool usableOnlyOnce = true;
    [SerializeField] private string infectionMilestoneOnSuccess;

    [Header("Audio")]
    [SerializeField] private AudioClip failedUseSound;
    [SerializeField] private AudioClip successUseSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;

    private bool hasBeenUsed;

    public void ConfigureRequirement(
        string itemId,
        string displayName,
        bool shouldConsumeItem,
        string onSuccessMessage,
        string onMissingMessage,
        GameObject[] activateTargets,
        GameObject[] deactivateTargets,
        bool oneTimeUse,
        string milestoneId = "")
    {
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            requiredItemId = itemId;
        }

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            requiredItemDisplayName = displayName;
        }

        consumeItem = shouldConsumeItem;

        if (!string.IsNullOrWhiteSpace(onSuccessMessage))
        {
            successMessage = onSuccessMessage;
        }

        if (!string.IsNullOrWhiteSpace(onMissingMessage))
        {
            missingItemMessage = onMissingMessage;
        }

        activateOnSuccess = activateTargets ?? System.Array.Empty<GameObject>();
        deactivateOnSuccess = deactivateTargets ?? System.Array.Empty<GameObject>();
        usableOnlyOnce = oneTimeUse;
        infectionMilestoneOnSuccess = milestoneId;
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (hasBeenUsed && usableOnlyOnce)
        {
            return "Already used";
        }

        return $"[{InputReader.GetInteractLabel()}] Use {requiredItemDisplayName}";
    }

    public override bool IsAvailable(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (!base.IsAvailable(interactor, inventory))
        {
            return false;
        }

        return !usableOnlyOnce || !hasBeenUsed;
    }

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (interactor == null || inventory == null)
        {
            return;
        }

        if (hasBeenUsed && usableOnlyOnce)
        {
            interactor.ShowTransientMessage("Nothing else happens.", 1.8f);
            return;
        }

        if (!inventory.HasItem(requiredItemId))
        {
            interactor.ShowTransientMessage(missingItemMessage, 2.2f);
            HolstinAudio.PlayOneShot(failedUseSound, transform, soundVolume);
            return;
        }

        if (consumeItem)
        {
            inventory.TryConsumeItem(requiredItemId, 1);
            interactor.ShowTransientMessage($"{successMessage}\n\nLost: {requiredItemDisplayName}", 3f);
        }
        else
        {
            interactor.ShowTransientMessage($"{successMessage}\n\nUsed: {requiredItemDisplayName}", 3f);
        }

        HolstinAudio.PlayOneShot(successUseSound, transform, soundVolume);

        if (!string.IsNullOrWhiteSpace(infectionMilestoneOnSuccess))
        {
            InfectionDirector.NotifyMilestoneGlobal(infectionMilestoneOnSuccess);
        }

        for (int i = 0; i < activateOnSuccess.Length; i++)
        {
            if (activateOnSuccess[i] != null)
            {
                activateOnSuccess[i].SetActive(true);
            }
        }

        for (int i = 0; i < deactivateOnSuccess.Length; i++)
        {
            if (deactivateOnSuccess[i] != null)
            {
                deactivateOnSuccess[i].SetActive(false);
            }
        }

        hasBeenUsed = true;
        ForceRefreshPrompt(interactor, inventory);
    }
}
