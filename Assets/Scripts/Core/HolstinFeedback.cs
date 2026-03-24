using UnityEngine;

public static class HolstinFeedback
{
    private static InteractionPromptUI cachedPromptUI;
    private static DialoguePanelUI cachedDialoguePanel;
    private static float nextResolveAttemptTime;

    public static void ShowMessage(string text, float duration = 2.2f)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        InteractionPromptUI promptUI = ResolvePromptUI();
        if (promptUI != null && Application.isPlaying)
        {
            promptUI.ShowMessage(text, duration);
            return;
        }

        Debug.Log(text);
    }

    public static void ShowDialogue(string speakerName, params string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        DialoguePanelUI dialoguePanel = ResolveDialoguePanel();
        if (dialoguePanel != null && Application.isPlaying)
        {
            dialoguePanel.ShowDialogue(speakerName, lines);
            return;
        }

        string message = string.IsNullOrWhiteSpace(speakerName)
            ? string.Join("\n", lines)
            : $"{speakerName}:\n{string.Join("\n", lines)}";
        ShowMessage(message, 4f);
    }

    public static InteractionPromptUI ResolvePromptUI()
    {
        if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context.PromptUI != null)
        {
            cachedPromptUI = context.PromptUI;
            return context.PromptUI;
        }

        if (cachedPromptUI != null)
        {
            return cachedPromptUI;
        }

        if (Application.isPlaying && Time.unscaledTime < nextResolveAttemptTime)
        {
            return null;
        }

        cachedPromptUI = Object.FindAnyObjectByType<InteractionPromptUI>();
        nextResolveAttemptTime = Time.unscaledTime + 0.5f;
        return cachedPromptUI;
    }

    public static DialoguePanelUI ResolveDialoguePanel()
    {
        if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context.DialoguePanel != null)
        {
            cachedDialoguePanel = context.DialoguePanel;
            return context.DialoguePanel;
        }

        if (cachedDialoguePanel != null)
        {
            return cachedDialoguePanel;
        }

        if (Application.isPlaying && Time.unscaledTime < nextResolveAttemptTime)
        {
            return null;
        }

        cachedDialoguePanel = Object.FindAnyObjectByType<DialoguePanelUI>();
        nextResolveAttemptTime = Time.unscaledTime + 0.5f;
        return cachedDialoguePanel;
    }
}
