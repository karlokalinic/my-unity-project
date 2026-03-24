using System;
using UnityEngine;

/// <summary>
/// Attached to a DialogueChoiceData to gate a dialogue option behind a skill check,
/// reputation threshold, or prior choice requirement.
/// </summary>
[Serializable]
public class DialogueRequirement
{
    public enum GateType { None, SkillCheck, MinReputation, MaxReputation, HasMilestone, HasItem }

    public GateType gateType = GateType.None;

    [Header("Skill Check")]
    public string skillId;
    public int difficultyClass = 12;

    [Header("Reputation Gate")]
    public string factionId;
    public int reputationThreshold;

    [Header("Milestone / Item Gate")]
    public string requiredId;

    [Header("Display")]
    public string lockedLabel = "[Locked]";

    public bool Evaluate(SkillSystem skills, CharacterStats stats, ReputationSystem rep, ChoiceHistoryTracker history, InventorySystem inventory)
    {
        return gateType switch
        {
            GateType.SkillCheck => skills != null && stats != null && skills.SkillCheck(skillId, difficultyClass, stats),
            GateType.MinReputation => rep != null && rep.GetReputation(factionId) >= reputationThreshold,
            GateType.MaxReputation => rep != null && rep.GetReputation(factionId) <= reputationThreshold,
            GateType.HasMilestone => history != null && history.HasMilestone(requiredId),
            GateType.HasItem => inventory != null && inventory.HasItem(requiredId),
            _ => true,
        };
    }

    public bool CanAttempt(SkillSystem skills, CharacterStats stats, ReputationSystem rep, ChoiceHistoryTracker history, InventorySystem inventory)
    {
        return gateType switch
        {
            GateType.SkillCheck => true,  // always visible, but outcome is rolled
            GateType.MinReputation => rep != null && rep.GetReputation(factionId) >= reputationThreshold,
            GateType.MaxReputation => rep != null && rep.GetReputation(factionId) <= reputationThreshold,
            GateType.HasMilestone => history != null && history.HasMilestone(requiredId),
            GateType.HasItem => inventory != null && inventory.HasItem(requiredId),
            _ => true,
        };
    }

    public string GetDisplayTag(SkillSystem skills, CharacterStats stats)
    {
        if (gateType == GateType.SkillCheck && skills != null && stats != null)
        {
            int effective = skills.GetEffectiveSkillLevel(skillId, stats);
            return $"[{skillId} {effective}/{difficultyClass}]";
        }
        if (gateType == GateType.MinReputation || gateType == GateType.MaxReputation)
            return $"[{factionId} rep]";
        if (gateType == GateType.HasMilestone)
            return "[requires history]";
        if (gateType == GateType.HasItem)
            return $"[requires {requiredId}]";
        return "";
    }
}
