using System;
using UnityEngine;

/// <summary>
/// NPC interactable with full RPG dialogue: skill checks, reputation gates,
/// choice history recording, XP/rep rewards, and item gating.
/// Replaces basic NPC dialogue for story-critical characters.
/// </summary>
[RequireComponent(typeof(NpcDialogueController))]
[RequireComponent(typeof(NpcIdentity))]
public class SkillCheckNpcInteractable : InteractableBase
{
    [Header("Dialogue Nodes")]
    [SerializeField] private GatedDialogueNode firstEncounter = new GatedDialogueNode();
    [SerializeField] private GatedDialogueNode repeatEncounter = new GatedDialogueNode();

    [Header("State")]
    [SerializeField] private bool singleEncounter;

    private NpcDialogueController dialogue;
    private NpcIdentity identity;
    private bool hasSpoken;

    // Cached player systems (resolved once)
    private SkillSystem playerSkills;
    private CharacterStats playerStats;
    private ReputationSystem playerRep;
    private ChoiceHistoryTracker playerHistory;
    private ExperienceSystem playerXp;
    private InventorySystem playerInv;

    private void Awake()
    {
        dialogue = GetComponent<NpcDialogueController>();
        identity = GetComponent<NpcIdentity>();
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        string name = identity != null ? identity.NpcName : "NPC";
        return $"[{InputReader.InteractKeyLabel}] Talk to {name}";
    }

    public override int GetPriority(PlayerInteraction interactor) => 5;

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (interactor == null || dialogue == null) return;
        if (singleEncounter && hasSpoken) return;

        ResolvePlayerSystems(interactor);

        GatedDialogueNode activeNode = hasSpoken ? repeatEncounter : firstEncounter;
        DialogueNodeData baseNode = activeNode.ToBaseNode(playerSkills, playerStats, playerRep, playerHistory, playerInv);

        dialogue.StartConversation(interactor, baseNode, result =>
        {
            HandleResult(activeNode, result, interactor);
        });
    }

    private void HandleResult(GatedDialogueNode node, DialogueSelectionResult result, PlayerInteraction interactor)
    {
        hasSpoken = true;
        if (result.Index < 0 || result.Index >= node.choices.Length) return;

        GatedDialogueChoice gc = node.choices[result.Index];
        if (gc == null) return;

        // Evaluate skill check if applicable
        bool passed = gc.requirement.Evaluate(playerSkills, playerStats, playerRep, playerHistory, playerInv);

        // Record the choice
        if (playerHistory != null)
        {
            playerHistory.RecordChoice(new ChoiceRecord
            {
                npcId = identity != null ? identity.NpcName : "unknown",
                nodePrompt = node.promptLine,
                choiceIndex = result.Index,
                choiceText = gc.choice.Text,
                milestoneTriggered = gc.choice.MilestoneId,
                skillUsed = gc.requirement.gateType == DialogueRequirement.GateType.SkillCheck ? gc.requirement.skillId : "",
                skillCheckPassed = passed
            });
        }

        // Show response
        string response = passed ? gc.choice.ResponseLine : gc.failureResponse;
        if (!string.IsNullOrWhiteSpace(response))
        {
            string speaker = identity != null ? identity.NpcName : "NPC";
            interactor.ShowTransientMessage($"{speaker}: {response}", 2.8f);
        }

        if (!passed) return;

        // Apply rewards
        if (playerXp != null && gc.experienceReward > 0)
            playerXp.AddExperience(gc.experienceReward);

        if (playerRep != null && gc.reputationDelta != 0 && !string.IsNullOrWhiteSpace(gc.reputationFactionId))
            playerRep.ModifyReputation(gc.reputationFactionId, gc.reputationDelta);

        if (playerInv != null && !string.IsNullOrWhiteSpace(gc.grantItemId))
        {
            playerInv.AddItem(gc.grantItemId, gc.grantItemId, 1);
            interactor.ShowTransientMessage($"Received: {gc.grantItemId}", 2f);
        }

        // Fire milestone
        if (!string.IsNullOrWhiteSpace(gc.choice.MilestoneId))
            InfectionDirector.NotifyMilestoneGlobal(gc.choice.MilestoneId);
    }

    private void ResolvePlayerSystems(PlayerInteraction interactor)
    {
        if (playerStats != null) return;
        GameObject player = interactor.gameObject;
        playerStats = player.GetComponent<CharacterStats>();
        playerSkills = player.GetComponent<SkillSystem>();
        playerRep = player.GetComponent<ReputationSystem>();
        playerHistory = player.GetComponent<ChoiceHistoryTracker>();
        playerXp = player.GetComponent<ExperienceSystem>();
        playerInv = player.GetComponent<InventorySystem>();
    }
}
