using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DialoguePanelUI : MonoBehaviour
{
    private const string DialoguePanelName = "DialoguePanel";
    private const string SpeakerTextName = "Speaker";
    private const string BodyTextName = "Body";
    private const string HintTextName = "Hint";
    private const string OptionsRootName = "OptionsRoot";
    private const int OptionCount = 3;

    private enum DialogueMode
    {
        None = 0,
        Lines = 1,
        Choices = 2
    }

    [Header("References")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private RectTransform optionsRoot;

    [Header("Style")]
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private Color panelColor = new Color(0.05f, 0.06f, 0.075f, 0.93f);
    [SerializeField] private Color accentColor = new Color(0.92f, 0.72f, 0.35f, 1f);
    [SerializeField] private Color optionIdleColor = new Color(0.12f, 0.13f, 0.15f, 0.9f);
    [SerializeField] private Color optionSelectedColor = new Color(0.22f, 0.2f, 0.12f, 0.98f);
    [SerializeField] private Color optionLeaveColor = new Color(0.44f, 0.16f, 0.16f, 0.92f);
    [SerializeField] private Color optionSelectedLeaveColor = new Color(0.68f, 0.2f, 0.2f, 0.98f);
    [SerializeField] private float initialInputIgnoreSeconds = 0.2f;

    private readonly Queue<string> queuedLines = new Queue<string>();
    private readonly List<OptionWidget> optionWidgets = new List<OptionWidget>();
    private bool visible;
    private string speakerName;
    private DialogueMode mode;
    private DialogueNodeData activeChoiceNode;
    private Action<DialogueSelectionResult> selectionCallback;
    private int selectedOptionIndex;
    private bool movementExitArmed;
    private bool ownsInputContext;
    private float inputUnlockTime;

    public bool IsShowing => visible;
    public event Action Closed;

    private void Awake()
    {
        EnsureRuntimeReferences(true);
        HideImmediate();
    }

    private void Start()
    {
        EnsureRuntimeReferences(true);
        HideImmediate();
    }

    private void Update()
    {
        if (panelGroup != null)
        {
            float target = visible ? 1f : 0f;
            panelGroup.alpha = Mathf.MoveTowards(panelGroup.alpha, target, fadeSpeed * Time.deltaTime);
            panelGroup.blocksRaycasts = visible;
            panelGroup.interactable = visible;
        }

        if (!visible)
        {
            return;
        }

        if (mode == DialogueMode.Lines)
        {
            UpdateLineDialogueMode();
            return;
        }

        if (mode == DialogueMode.Choices)
        {
            UpdateChoiceDialogueMode();
        }
    }

    public void ShowDialogue(string newSpeakerName, params string[] lines)
    {
        EnsureRuntimeReferences(true);
        AcquireDialogueContext();
        HidePromptLayerDuringDialogue();
        queuedLines.Clear();
        mode = DialogueMode.Lines;
        activeChoiceNode = null;
        selectionCallback = null;

        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    queuedLines.Enqueue(lines[i].Trim());
                }
            }
        }

        if (queuedLines.Count == 0)
        {
            CloseDialogue();
            return;
        }

        speakerName = string.IsNullOrWhiteSpace(newSpeakerName) ? "Unknown" : newSpeakerName;
        SetSpeakerVisible(true);
        visible = true;
        inputUnlockTime = Time.unscaledTime + Mathf.Max(0.01f, initialInputIgnoreSeconds);
        ShowNextQueuedLine();
        RefreshChoiceVisibility();
    }

    public void ShowChoiceDialogue(DialogueNodeData node, Action<DialogueSelectionResult> onSelection)
    {
        EnsureRuntimeReferences(true);
        AcquireDialogueContext();
        HidePromptLayerDuringDialogue();
        mode = DialogueMode.Choices;
        queuedLines.Clear();
        activeChoiceNode = node;
        selectionCallback = onSelection;
        selectedOptionIndex = 0;
        movementExitArmed = !InputReader.MovementIntentPressed();
        visible = true;
        inputUnlockTime = Time.unscaledTime + Mathf.Max(0.01f, initialInputIgnoreSeconds);

        if (activeChoiceNode == null)
        {
            CommitChoice(-1);
            return;
        }

        speakerName = string.IsNullOrWhiteSpace(activeChoiceNode.SpeakerName) ? "Unknown" : activeChoiceNode.SpeakerName;
        SetSpeakerVisible(false);

        if (bodyText != null)
        {
            bodyText.text = string.IsNullOrWhiteSpace(activeChoiceNode.PromptLine)
                ? "Choose your response."
                : activeChoiceNode.PromptLine;
            bodyText.color = Color.white;
        }

        if (hintText != null)
        {
            hintText.text = $"[{InputReader.GetDialogueSubmitLabel()}] Confirm    [{InputReader.GetDialogueCancelLabel()}] Leave";
        }

        RefreshChoiceWidgets();
        RefreshChoiceVisibility();
    }

    public void HideImmediate()
    {
        visible = false;
        mode = DialogueMode.None;
        ReleaseDialogueContext();
        queuedLines.Clear();
        activeChoiceNode = null;
        selectionCallback = null;

        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.blocksRaycasts = false;
            panelGroup.interactable = false;
        }

        SetSpeakerVisible(true);
    }

    private void UpdateLineDialogueMode()
    {
        if (Time.unscaledTime < inputUnlockTime)
        {
            return;
        }

        if (InputReader.CancelPressed())
        {
            CloseDialogue();
            return;
        }

        if (InputReader.InteractPressed() || InputReader.DialogueSubmitPressed())
        {
            AdvanceDialogue();
        }
    }

    private void UpdateChoiceDialogueMode()
    {
        if (Time.unscaledTime < inputUnlockTime)
        {
            return;
        }

        if (!movementExitArmed)
        {
            if (!InputReader.MovementIntentPressed())
            {
                movementExitArmed = true;
            }
        }
        else if (InputReader.MovementIntentStarted())
        {
            CommitChoice(FindLeaveOrFallbackIndex());
            return;
        }

        Vector2 navigate = InputReader.GetDialogueNavigateVector();
        if (navigate.y > 0.1f)
        {
            MoveSelection(-1);
        }
        else if (navigate.y < -0.1f)
        {
            MoveSelection(1);
        }

        if (InputReader.DialogueSubmitPressed())
        {
            CommitChoice(selectedOptionIndex);
            return;
        }

        if (InputReader.CancelPressed() || InteractFallbackPressed())
        {
            CommitChoice(FindLeaveOrFallbackIndex());
        }
    }

    private void AdvanceDialogue()
    {
        if (queuedLines.Count == 0)
        {
            CloseDialogue();
            return;
        }

        ShowNextQueuedLine();
    }

    private void ShowNextQueuedLine()
    {
        if (speakerText != null)
        {
            speakerText.text = speakerName;
            speakerText.color = accentColor;
        }

        if (bodyText != null)
        {
            bodyText.text = queuedLines.Count > 0 ? queuedLines.Dequeue() : string.Empty;
            bodyText.color = Color.white;
        }

        if (hintText != null)
        {
            hintText.text = queuedLines.Count > 0
                ? $"[{InputReader.GetInteractLabel()}] Next    [{InputReader.GetDialogueCancelLabel()}] Skip"
                : $"[{InputReader.GetInteractLabel()}] Close    [{InputReader.GetDialogueCancelLabel()}] Skip";
        }
    }

    private void MoveSelection(int delta)
    {
        if (optionWidgets.Count == 0)
        {
            return;
        }

        selectedOptionIndex += delta;
        if (selectedOptionIndex < 0)
        {
            selectedOptionIndex = optionWidgets.Count - 1;
        }
        else if (selectedOptionIndex >= optionWidgets.Count)
        {
            selectedOptionIndex = 0;
        }

        RefreshOptionVisuals();
    }

    private void CommitChoice(int index)
    {
        DialogueSelectionResult result = ResolveSelection(index);
        Action<DialogueSelectionResult> callback = selectionCallback;
        selectionCallback = null;
        callback?.Invoke(result);
        CloseDialogue();
    }

    private DialogueSelectionResult ResolveSelection(int index)
    {
        if (activeChoiceNode?.Choices == null || activeChoiceNode.Choices.Length == 0)
        {
            return new DialogueSelectionResult(-1, null);
        }

        int safeIndex = Mathf.Clamp(index, 0, activeChoiceNode.Choices.Length - 1);
        DialogueChoiceData choice = activeChoiceNode.Choices[safeIndex];
        return new DialogueSelectionResult(safeIndex, choice);
    }

    private int FindLeaveOrFallbackIndex()
    {
        if (activeChoiceNode?.Choices == null || activeChoiceNode.Choices.Length == 0)
        {
            return -1;
        }

        for (int i = 0; i < activeChoiceNode.Choices.Length; i++)
        {
            DialogueChoiceData choice = activeChoiceNode.Choices[i];
            if (choice != null && choice.IsLeave)
            {
                return i;
            }
        }

        return activeChoiceNode.Choices.Length - 1;
    }

    private void CloseDialogue()
    {
        bool wasVisible = visible;
        visible = false;
        mode = DialogueMode.None;
        ReleaseDialogueContext();
        queuedLines.Clear();
        activeChoiceNode = null;
        selectionCallback = null;
        movementExitArmed = false;
        RefreshChoiceVisibility();
        SetSpeakerVisible(true);

        if (wasVisible)
        {
            Closed?.Invoke();
        }
    }

    private static bool InteractFallbackPressed()
    {
        return InputReader.InteractPressed();
    }

    private void OnDisable()
    {
        ReleaseDialogueContext();
    }

    private void AcquireDialogueContext()
    {
        if (ownsInputContext)
        {
            return;
        }

        InputReader.PushContext(InputReader.InputContext.Dialogue);
        ownsInputContext = true;
    }

    private void ReleaseDialogueContext()
    {
        if (!ownsInputContext)
        {
            return;
        }

        InputReader.PopContext(InputReader.InputContext.Gameplay);
        ownsInputContext = false;
    }

    private void EnsureRuntimeReferences(bool allowRebuild)
    {
        EnsureCanvasScaffolding();
        panelGroup = ResolvePanelGroup(allowRebuild);
        RemoveLegacyPanelArtifacts(panelGroup);
        speakerText = ResolveTextElement(panelGroup, SpeakerTextName, allowRebuild, 30f, TextAlignmentOptions.Left, "Speaker", new Vector2(22f, -14f), new Vector2(-22f, -56f));
        bodyText = ResolveTextElement(panelGroup, BodyTextName, allowRebuild, 24f, TextAlignmentOptions.TopLeft, "Dialogue line.", new Vector2(22f, -62f), new Vector2(-22f, -124f));
        hintText = ResolveTextElement(panelGroup, HintTextName, allowRebuild, 17f, TextAlignmentOptions.TopRight, "[Enter] Confirm    [Esc] Leave", new Vector2(22f, -48f), new Vector2(-22f, -14f));
        optionsRoot = ResolveOptionsRoot(panelGroup, allowRebuild);
        EnsureChoiceWidgets(allowRebuild);
    }

    private void EnsureCanvasScaffolding()
    {
        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect == null)
        {
            rootRect = gameObject.AddComponent<RectTransform>();
        }

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.localScale = Vector3.one;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private CanvasGroup ResolvePanelGroup(bool allowRebuild)
    {
        Transform panelTransform = transform.Find(DialoguePanelName);
        if (panelTransform == null && allowRebuild)
        {
            GameObject panel = new GameObject(DialoguePanelName, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panel.transform.SetParent(transform, false);
            panelTransform = panel.transform;
        }

        if (panelTransform == null)
        {
            return null;
        }

        RectTransform panelRect = panelTransform as RectTransform;
        if (panelRect == null)
        {
            panelRect = panelTransform.gameObject.AddComponent<RectTransform>();
        }

        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 28f);
        panelRect.sizeDelta = new Vector2(1080f, 332f);

        CanvasGroup group = panelTransform.GetComponent<CanvasGroup>();
        if (group == null && allowRebuild)
        {
            group = panelTransform.gameObject.AddComponent<CanvasGroup>();
        }

        Image image = panelTransform.GetComponent<Image>();
        if (image == null && allowRebuild)
        {
            image = panelTransform.gameObject.AddComponent<Image>();
        }
        if (image != null)
        {
            image.color = panelColor;
            image.raycastTarget = true;
        }

        return group;
    }

    private void SetSpeakerVisible(bool visibleState)
    {
        if (speakerText == null)
        {
            return;
        }

        if (speakerText.gameObject.activeSelf != visibleState)
        {
            speakerText.gameObject.SetActive(visibleState);
        }
    }

    private static void RemoveLegacyPanelArtifacts(CanvasGroup group)
    {
        if (group == null)
        {
            return;
        }

        for (int i = group.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = group.transform.GetChild(i);
            if (child == null)
            {
                continue;
            }

            string childName = child.name;
            bool known = string.Equals(childName, SpeakerTextName, StringComparison.Ordinal) ||
                         string.Equals(childName, BodyTextName, StringComparison.Ordinal) ||
                         string.Equals(childName, HintTextName, StringComparison.Ordinal) ||
                         string.Equals(childName, OptionsRootName, StringComparison.Ordinal);
            if (known)
            {
                continue;
            }

            if (child.GetComponent<TextMeshProUGUI>() == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void HidePromptLayerDuringDialogue()
    {
        InteractionPromptUI prompt = HolstinFeedback.ResolvePromptUI();
        if (prompt == null)
        {
            return;
        }

        prompt.HidePromptImmediate();
        prompt.HideMessageImmediate();
    }

    private RectTransform ResolveOptionsRoot(CanvasGroup group, bool allowRebuild)
    {
        if (group == null)
        {
            return null;
        }

        Transform existing = group.transform.Find(OptionsRootName);
        if (existing == null && allowRebuild)
        {
            GameObject root = new GameObject(OptionsRootName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(group.transform, false);
            existing = root.transform;
        }

        if (existing == null)
        {
            return null;
        }

        RectTransform rect = existing as RectTransform;
        if (rect == null)
        {
            rect = existing.gameObject.AddComponent<RectTransform>();
        }
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(18f, 18f);
        rect.offsetMax = new Vector2(-18f, 166f);

        VerticalLayoutGroup layout = existing.GetComponent<VerticalLayoutGroup>();
        if (layout == null && allowRebuild)
        {
            layout = existing.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        if (layout != null)
        {
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        ContentSizeFitter fitter = existing.GetComponent<ContentSizeFitter>();
        if (fitter == null && allowRebuild)
        {
            fitter = existing.gameObject.AddComponent<ContentSizeFitter>();
        }

        if (fitter != null)
        {
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        return rect;
    }

    private void EnsureChoiceWidgets(bool allowRebuild)
    {
        if (optionsRoot == null)
        {
            return;
        }

        optionWidgets.Clear();

        for (int i = 0; i < OptionCount; i++)
        {
            string optionName = $"Option_{i + 1}";
            Transform existing = optionsRoot.Find(optionName);
            if (existing == null && allowRebuild)
            {
                GameObject option = new GameObject(optionName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                option.transform.SetParent(optionsRoot, false);

                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(option.transform, false);
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(14f, 8f);
                labelRect.offsetMax = new Vector2(-14f, -8f);

                existing = option.transform;
            }

            if (existing == null)
            {
                continue;
            }

            RectTransform rect = existing as RectTransform;
            if (rect == null)
            {
                rect = existing.gameObject.AddComponent<RectTransform>();
            }

            LayoutElement element = existing.GetComponent<LayoutElement>();
            if (element == null && allowRebuild)
            {
                element = existing.gameObject.AddComponent<LayoutElement>();
            }
            if (element != null)
            {
                element.preferredHeight = 44f;
            }

            Image background = existing.GetComponent<Image>();
            if (background == null && allowRebuild)
            {
                background = existing.gameObject.AddComponent<Image>();
            }

            Button button = existing.GetComponent<Button>();
            if (button == null && allowRebuild)
            {
                button = existing.gameObject.AddComponent<Button>();
            }

            Transform labelTransform = existing.Find("Label");
            TextMeshProUGUI label = labelTransform != null ? labelTransform.GetComponent<TextMeshProUGUI>() : null;
            if (label != null)
            {
                if (TMP_Settings.defaultFontAsset != null)
                {
                    label.font = TMP_Settings.defaultFontAsset;
                }
                label.fontSize = 20f;
                label.alignment = TextAlignmentOptions.Left;
                label.color = Color.white;
                label.textWrappingMode = TextWrappingModes.Normal;
                label.raycastTarget = false;
            }

            int index = i;
            if (button != null)
            {
                button.transition = Selectable.Transition.None;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => CommitChoice(index));
            }

            EventTrigger trigger = existing.GetComponent<EventTrigger>();
            if (trigger == null && allowRebuild)
            {
                trigger = existing.gameObject.AddComponent<EventTrigger>();
            }
            if (trigger != null)
            {
                trigger.triggers ??= new List<EventTrigger.Entry>();
                trigger.triggers.Clear();
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                entry.callback.AddListener(_ =>
                {
                    selectedOptionIndex = index;
                    RefreshOptionVisuals();
                });
                trigger.triggers.Add(entry);
            }

            optionWidgets.Add(new OptionWidget
            {
                background = background,
                button = button,
                label = label
            });
        }
    }

    private void RefreshChoiceWidgets()
    {
        DialogueChoiceData[] choices = activeChoiceNode?.Choices ?? Array.Empty<DialogueChoiceData>();

        for (int i = 0; i < optionWidgets.Count; i++)
        {
            OptionWidget widget = optionWidgets[i];
            DialogueChoiceData choice = i < choices.Length ? choices[i] : null;

            bool hasChoice = choice != null && !string.IsNullOrWhiteSpace(choice.Text);
            if (widget.button != null)
            {
                widget.button.gameObject.SetActive(hasChoice);
            }

            if (widget.label != null && hasChoice)
            {
                widget.label.text = $"{i + 1}. {choice.Text}";
            }
        }

        RefreshOptionVisuals();
    }

    private void RefreshOptionVisuals()
    {
        DialogueChoiceData[] choices = activeChoiceNode?.Choices ?? Array.Empty<DialogueChoiceData>();

        for (int i = 0; i < optionWidgets.Count; i++)
        {
            OptionWidget widget = optionWidgets[i];
            if (widget.background == null || i >= choices.Length || choices[i] == null)
            {
                continue;
            }

            bool selected = i == selectedOptionIndex;
            bool isLeave = choices[i].IsLeave;
            widget.background.color = isLeave
                ? (selected ? optionSelectedLeaveColor : optionLeaveColor)
                : (selected ? optionSelectedColor : optionIdleColor);
        }
    }

    private void RefreshChoiceVisibility()
    {
        if (optionsRoot != null)
        {
            optionsRoot.gameObject.SetActive(mode == DialogueMode.Choices && visible);
        }
    }

    private static TextMeshProUGUI ResolveTextElement(
        CanvasGroup group,
        string name,
        bool allowRebuild,
        float fontSize,
        TextAlignmentOptions alignment,
        string fallback,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        if (group == null)
        {
            return null;
        }

        Transform elementTransform = group.transform.Find(name);
        if (elementTransform == null && allowRebuild)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(group.transform, false);
            elementTransform = textObject.transform;
        }

        if (elementTransform == null)
        {
            return null;
        }

        RectTransform elementRect = elementTransform as RectTransform;
        if (elementRect == null)
        {
            elementRect = elementTransform.gameObject.AddComponent<RectTransform>();
        }

        elementRect.anchorMin = Vector2.zero;
        elementRect.anchorMax = Vector2.one;
        elementRect.offsetMin = offsetMin;
        elementRect.offsetMax = offsetMax;

        TextMeshProUGUI tmp = elementTransform.GetComponent<TextMeshProUGUI>();
        if (tmp == null && allowRebuild)
        {
            tmp = elementTransform.gameObject.AddComponent<TextMeshProUGUI>();
        }

        if (tmp == null)
        {
            return null;
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Truncate;
        tmp.raycastTarget = false;
        if (string.IsNullOrWhiteSpace(tmp.text))
        {
            tmp.text = fallback;
        }

        return tmp;
    }

    private struct OptionWidget
    {
        public Image background;
        public Button button;
        public TextMeshProUGUI label;
    }
}
