using System;

/// <summary>
/// Immutable record of a single player choice. Stored in ChoiceHistoryTracker.
/// </summary>
[Serializable]
public class ChoiceRecord
{
    public float gameTime;
    public string npcId;
    public string nodePrompt;
    public int choiceIndex;
    public string choiceText;
    public string milestoneTriggered;
    public string skillUsed;
    public bool skillCheckPassed;
}
