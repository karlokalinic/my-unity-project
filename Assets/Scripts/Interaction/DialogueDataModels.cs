using System;
using UnityEngine;

[Serializable]
public class DialogueChoiceData
{
    [SerializeField] private string text = "Response option";
    [SerializeField] [TextArea(1, 3)] private string responseLine = "Acknowledged.";
    [SerializeField] private string milestoneId;
    [SerializeField] private bool isLeave;

    public string Text
    {
        get => text;
        set => text = value;
    }

    public string ResponseLine
    {
        get => responseLine;
        set => responseLine = value;
    }

    public string MilestoneId
    {
        get => milestoneId;
        set => milestoneId = value;
    }

    public bool IsLeave
    {
        get => isLeave;
        set => isLeave = value;
    }
}

[Serializable]
public class DialogueNodeData
{
    [SerializeField] private string speakerName = "Unknown";
    [SerializeField] [TextArea(1, 4)] private string promptLine = "What do you want to say?";
    [SerializeField] private DialogueChoiceData[] choices = new DialogueChoiceData[3];

    public string SpeakerName
    {
        get => speakerName;
        set => speakerName = value;
    }

    public string PromptLine
    {
        get => promptLine;
        set => promptLine = value;
    }

    public DialogueChoiceData[] Choices
    {
        get => choices;
        set => choices = value;
    }
}

public readonly struct DialogueSelectionResult
{
    public DialogueSelectionResult(int index, DialogueChoiceData choice)
    {
        Index = index;
        Choice = choice;
    }

    public int Index { get; }
    public DialogueChoiceData Choice { get; }
}
