using System;
using UnityEngine;

/// <summary>
/// Extended dialogue choice with skill/reputation requirements and reward hooks.
/// Wraps a standard DialogueChoiceData with RPG gates.
/// </summary>
[Serializable]
public class GatedDialogueChoice
{
    public DialogueChoiceData choice = new DialogueChoiceData();
    public DialogueRequirement requirement = new DialogueRequirement();

    [Header("Rewards on selection")]
    public string reputationFactionId;
    public int reputationDelta;
    public int experienceReward;
    public string grantItemId;

    [Header("Skill check failure")]
    [TextArea(1, 3)] public string failureResponse = "That didn't work.";
}

/// <summary>
/// Full gated dialogue node: speaker line + array of skill-gated options.
/// </summary>
[Serializable]
public class GatedDialogueNode
{
    public string speakerName = "Unknown";
    [TextArea(1, 4)] public string promptLine = "What do you want to say?";
    public GatedDialogueChoice[] choices = new GatedDialogueChoice[3];

    public DialogueNodeData ToBaseNode(SkillSystem skills, CharacterStats stats, ReputationSystem rep, ChoiceHistoryTracker history, InventorySystem inv)
    {
        var node = new DialogueNodeData();
        node.SpeakerName = speakerName;
        node.PromptLine = promptLine;

        var baseChoices = new DialogueChoiceData[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            var gc = choices[i];
            if (gc == null) { baseChoices[i] = new DialogueChoiceData(); continue; }

            var c = new DialogueChoiceData();
            c.IsLeave = gc.choice.IsLeave;
            c.MilestoneId = gc.choice.MilestoneId;

            bool canAttempt = gc.requirement.CanAttempt(skills, stats, rep, history, inv);
            string tag = gc.requirement.GetDisplayTag(skills, stats);

            if (!canAttempt)
            {
                c.Text = $"{tag} {gc.requirement.lockedLabel}";
                c.ResponseLine = "";
            }
            else
            {
                c.Text = string.IsNullOrWhiteSpace(tag) ? gc.choice.Text : $"{tag} {gc.choice.Text}";
                c.ResponseLine = gc.choice.ResponseLine;
            }
            baseChoices[i] = c;
        }
        node.Choices = baseChoices;
        return node;
    }
}
