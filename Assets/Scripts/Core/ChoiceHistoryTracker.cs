using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks every dialogue choice the player has made. Queried by NPCs and narrative systems
/// to react to the player's history ("You said X to Y earlier...").
/// </summary>
public class ChoiceHistoryTracker : MonoBehaviour
{
    [SerializeField] private List<ChoiceRecord> history = new List<ChoiceRecord>();

    private readonly HashSet<string> triggeredMilestones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ChoiceRecord> History => history;

    public event Action<ChoiceRecord> ChoiceMade;

    public void RecordChoice(ChoiceRecord record)
    {
        if (record == null) return;
        record.gameTime = Time.time;
        history.Add(record);

        if (!string.IsNullOrWhiteSpace(record.milestoneTriggered))
            triggeredMilestones.Add(record.milestoneTriggered);

        ChoiceMade?.Invoke(record);
    }

    public bool HasMilestone(string milestoneId)
    {
        return !string.IsNullOrWhiteSpace(milestoneId) && triggeredMilestones.Contains(milestoneId);
    }

    public bool HasChosenForNpc(string npcId, int choiceIndex)
    {
        for (int i = 0; i < history.Count; i++)
        {
            if (string.Equals(history[i].npcId, npcId, StringComparison.OrdinalIgnoreCase) && history[i].choiceIndex == choiceIndex)
                return true;
        }
        return false;
    }

    public int CountChoicesForNpc(string npcId)
    {
        int count = 0;
        for (int i = 0; i < history.Count; i++)
        {
            if (string.Equals(history[i].npcId, npcId, StringComparison.OrdinalIgnoreCase))
                count++;
        }
        return count;
    }

    public bool HasPassedSkillCheck(string skillId)
    {
        for (int i = 0; i < history.Count; i++)
        {
            if (history[i].skillCheckPassed && string.Equals(history[i].skillUsed, skillId, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
