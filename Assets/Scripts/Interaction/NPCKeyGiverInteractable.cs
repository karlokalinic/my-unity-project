using UnityEngine;

[RequireComponent(typeof(NpcDialogueController))]
public class NPCKeyGiverInteractable : InteractableBase
{
    [Header("NPC")]
    [SerializeField] private string npcName = "Caretaker";
    [SerializeField] [TextArea(2, 5)] private string firstConversation = "Take the service key. Keep your head down and your story short.";
    [SerializeField] [TextArea(2, 5)] private string repeatConversation = "You already have the key. Use it before someone notices.";
    [SerializeField] private DialogueNodeData firstNode = new DialogueNodeData();
    [SerializeField] private DialogueNodeData repeatNode = new DialogueNodeData();
    [SerializeField] private int rewardChoiceIndex;

    [Header("Reward")]
    [SerializeField] private string rewardItemId = "service_key";
    [SerializeField] private string rewardItemDisplayName = "Service Key";
    [SerializeField] private bool grantOnlyOnce = true;
    [SerializeField] private string infectionMilestoneOnReward;

    [Header("Audio")]
    [SerializeField] private AudioClip dialogueSound;
    [SerializeField] private AudioClip rewardSound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;

    private bool rewardGranted;
    private NpcDialogueController dialogueController;

    public void ConfigureReward(string newRewardItemId, string newRewardDisplayName, bool onlyOnce = true, string rewardMilestone = "")
    {
        if (!string.IsNullOrWhiteSpace(newRewardItemId))
        {
            rewardItemId = newRewardItemId;
        }

        if (!string.IsNullOrWhiteSpace(newRewardDisplayName))
        {
            rewardItemDisplayName = newRewardDisplayName;
        }

        grantOnlyOnce = onlyOnce;
        infectionMilestoneOnReward = rewardMilestone;
        EnsureDialogueDefaults();
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (grantOnlyOnce && rewardGranted)
        {
            return $"[{InputReader.GetInteractLabel()}] Talk to {npcName}";
        }

        return $"[{InputReader.GetInteractLabel()}] Ask {npcName}";
    }

    public override int GetPriority(PlayerInteraction interactor)
    {
        return 5;
    }

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (interactor == null || inventory == null)
        {
            return;
        }

        EnsureDialogueDefaults();
        EnsureController();

        DialogueNodeData selectedNode = grantOnlyOnce && rewardGranted ? repeatNode : firstNode;
        if (dialogueController == null)
        {
            // Hard fallback for scenes missing dialogue UI/controller setup.
            string line = grantOnlyOnce && rewardGranted ? repeatConversation : firstConversation;
            line = ApplyPressureDialogueVariant(line);
            interactor.ShowTransientMessage($"{npcName}:\n{line}", 3f);
            if (!(grantOnlyOnce && rewardGranted))
            {
                TryGrantReward(interactor, inventory);
            }
            return;
        }

        dialogueController.StartConversation(interactor, selectedNode, result =>
        {
            HandleSelection(result, interactor, inventory, selectedNode);
        });
    }

    private void Awake()
    {
        EnsureController();
        EnsureDialogueDefaults();
    }

    private void OnValidate()
    {
        EnsureDialogueDefaults();
    }

    private void EnsureController()
    {
        if (dialogueController == null)
        {
            dialogueController = GetComponent<NpcDialogueController>();
        }
    }

    private void HandleSelection(
        DialogueSelectionResult selection,
        PlayerInteraction interactor,
        InventorySystem inventory,
        DialogueNodeData node)
    {
        DialogueChoiceData choice = selection.Choice;
        bool leaveChoice = choice != null && choice.IsLeave;

        if (choice != null && !string.IsNullOrWhiteSpace(choice.ResponseLine))
        {
            interactor.ShowTransientMessage($"{npcName}: {choice.ResponseLine}", 2.7f);
        }

        if (choice != null && !string.IsNullOrWhiteSpace(choice.MilestoneId))
        {
            if (SliceState.TryGet(out SliceState sliceState))
            {
                sliceState.MarkMilestone(choice.MilestoneId);
            }

            InfectionDirector.NotifyMilestoneGlobal(choice.MilestoneId);
        }

        bool milestoneAlreadyTriggeredByChoice = choice != null &&
                                                 !string.IsNullOrWhiteSpace(choice.MilestoneId) &&
                                                 string.Equals(choice.MilestoneId, infectionMilestoneOnReward, System.StringComparison.OrdinalIgnoreCase);

        if (!leaveChoice && selection.Index == rewardChoiceIndex)
        {
            TryGrantReward(interactor, inventory, milestoneAlreadyTriggeredByChoice);
        }
        else if (!leaveChoice)
        {
            HolstinAudio.PlayOneShot(dialogueSound, transform, soundVolume);
        }

        ForceRefreshPrompt(interactor, inventory);
    }

    private void TryGrantReward(PlayerInteraction interactor, InventorySystem inventory, bool skipRewardMilestone = false)
    {
        if (grantOnlyOnce && rewardGranted)
        {
            HolstinAudio.PlayOneShot(dialogueSound, transform, soundVolume);
            return;
        }

        rewardGranted = true;
        inventory.AddItem(rewardItemId, rewardItemDisplayName, 1);
        HolstinAudio.PlayOneShot(rewardSound != null ? rewardSound : dialogueSound, transform, soundVolume);
        interactor.ShowTransientMessage($"Received: {rewardItemDisplayName}", 2.6f);

        if (SliceState.TryGet(out SliceState sliceState))
        {
            sliceState.AcquireKeyItem(rewardItemId);
            sliceState.SetCurrentObjective("console_service_unlock");
        }

        if (!skipRewardMilestone && !string.IsNullOrWhiteSpace(infectionMilestoneOnReward))
        {
            InfectionDirector.NotifyMilestoneGlobal(infectionMilestoneOnReward);
        }
    }

    private void EnsureDialogueDefaults()
    {
        firstNode ??= new DialogueNodeData();
        repeatNode ??= new DialogueNodeData();

        firstNode.SpeakerName = string.IsNullOrWhiteSpace(npcName) ? "NPC" : npcName;
        repeatNode.SpeakerName = string.IsNullOrWhiteSpace(npcName) ? "NPC" : npcName;
        firstNode.PromptLine = "Choose your response.";
        repeatNode.PromptLine = "Choose your response.";

        EnsureChoiceArray(firstNode, true);
        EnsureChoiceArray(repeatNode, false);
        rewardChoiceIndex = Mathf.Clamp(rewardChoiceIndex, 0, 2);
    }

    private void EnsureChoiceArray(DialogueNodeData node, bool firstDialogue)
    {
        if (node.Choices == null || node.Choices.Length != 3)
        {
            node.Choices = new DialogueChoiceData[3];
        }

        for (int i = 0; i < node.Choices.Length; i++)
        {
            node.Choices[i] ??= new DialogueChoiceData();
            node.Choices[i].IsLeave = false;
            node.Choices[i].MilestoneId = string.Empty;
        }

        if (firstDialogue)
        {
            node.Choices[0].Text = $"I need the {rewardItemDisplayName}.";
            node.Choices[0].ResponseLine = ApplyPressureDialogueVariant(firstConversation);
            node.Choices[0].MilestoneId = infectionMilestoneOnReward;

            node.Choices[1].Text = "Any quick advice?";
            node.Choices[1].ResponseLine = ApplyPressureDialogueVariant("Stay calm, stay brief, and don't draw attention.");
        }
        else
        {
            node.Choices[0].Text = "Do you have anything else for me?";
            node.Choices[0].ResponseLine = ApplyPressureDialogueVariant(repeatConversation);

            node.Choices[1].Text = "Any update?";
            node.Choices[1].ResponseLine = ApplyPressureDialogueVariant("No updates. Keep moving.");
        }

        node.Choices[2].Text = "Leave conversation.";
        node.Choices[2].ResponseLine = "You step back from the conversation.";
        node.Choices[2].IsLeave = true;
    }

    private static string ApplyPressureDialogueVariant(string baseLine)
    {
        if (!PressureManager.TryGet(out PressureManager pressureManager))
        {
            return baseLine;
        }

        if (pressureManager.CurrentStage >= 3)
        {
            return $"{baseLine} We are out of time.";
        }

        if (pressureManager.CurrentStage >= 2)
        {
            return $"{baseLine} Move before pressure rises again.";
        }

        return baseLine;
    }
}
