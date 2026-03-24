using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    private const string ContextPanelName = "ContextPrompt";
    private const string MessagePanelName = "TransientMessage";
    private const string TextChildName = "Text";

    [Header("Context Prompt")]
    [SerializeField] private CanvasGroup contextGroup;
    [SerializeField] private TextMeshProUGUI contextText;

    [Header("Transient Message")]
    [SerializeField] private CanvasGroup messageGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float fadeSpeed = 9f;

    private const int MaxQueuedMessages = 3;

    private readonly Queue<MessageRequest> messageQueue = new Queue<MessageRequest>();
    private Coroutine messageRoutine;
    private bool contextVisible;

    private void Awake()
    {
        ResolveExistingReferences();
        HidePromptImmediate();
        HideMessageImmediate();
    }

    private void Start()
    {
        EnsureRuntimeReferences(true);
        HidePromptImmediate();
        HideMessageImmediate();
    }

    private void OnValidate()
    {
        ResolveExistingReferences();
    }

    private void Update()
    {
        if (contextGroup != null)
        {
            float targetAlpha = contextVisible ? 1f : 0f;
            contextGroup.alpha = Mathf.MoveTowards(contextGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            contextGroup.blocksRaycasts = false;
            contextGroup.interactable = false;
        }
    }

    public void ShowPrompt(string text)
    {
        EnsureRuntimeReferences(true);
        if (contextText != null)
        {
            contextText.text = text;
        }

        contextVisible = true;
    }

    public void HidePrompt()
    {
        contextVisible = false;
    }

    public void ShowMessage(string text, float duration = 2.2f)
    {
        EnsureRuntimeReferences(true);
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        EnqueueMessage(text, duration);
        if (messageRoutine == null)
        {
            messageRoutine = StartCoroutine(ShowQueuedMessagesRoutine());
        }
    }

    public void HidePromptImmediate()
    {
        contextVisible = false;
        if (contextGroup != null)
        {
            contextGroup.alpha = 0f;
        }
    }

    public void HideMessageImmediate()
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }

        messageQueue.Clear();

        if (messageGroup != null)
        {
            messageGroup.alpha = 0f;
        }
    }

    private void EnqueueMessage(string text, float duration)
    {
        while (messageQueue.Count >= MaxQueuedMessages)
        {
            messageQueue.Dequeue();
        }

        messageQueue.Enqueue(new MessageRequest
        {
            text = text,
            duration = Mathf.Max(0.15f, duration)
        });
    }

    private IEnumerator ShowQueuedMessagesRoutine()
    {
        while (messageQueue.Count > 0)
        {
            MessageRequest request = messageQueue.Dequeue();
            if (messageText != null)
            {
                messageText.text = request.text;
            }

            if (messageGroup != null)
            {
                messageGroup.alpha = 1f;
                messageGroup.blocksRaycasts = false;
                messageGroup.interactable = false;
            }

            float remaining = request.duration;
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                yield return null;
            }

            while (messageGroup != null && messageGroup.alpha > 0f)
            {
                messageGroup.alpha = Mathf.MoveTowards(messageGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
                yield return null;
            }
        }

        messageRoutine = null;
    }

    private struct MessageRequest
    {
        public string text;
        public float duration;
    }

    private void ResolveExistingReferences()
    {
        contextGroup = FindExistingPanelGroup(ContextPanelName);
        messageGroup = FindExistingPanelGroup(MessagePanelName);
        contextText = FindExistingPanelText(contextGroup);
        messageText = FindExistingPanelText(messageGroup);
    }

    private CanvasGroup FindExistingPanelGroup(string panelName)
    {
        Transform panelTransform = transform.Find(panelName);
        return panelTransform != null ? panelTransform.GetComponent<CanvasGroup>() : null;
    }

    private TextMeshProUGUI FindExistingPanelText(CanvasGroup group)
    {
        if (group == null)
        {
            return null;
        }

        Transform textTransform = group.transform.Find(TextChildName);
        return textTransform != null ? textTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    private void EnsureRuntimeReferences(bool allowRebuild)
    {
        EnsureCanvasScaffolding();

        contextGroup = ResolvePanelGroup(ContextPanelName, allowRebuild, new PanelLayout
        {
            anchorMin = new Vector2(0.5f, 0f),
            anchorMax = new Vector2(0.5f, 0f),
            pivot = new Vector2(0.5f, 0f),
            anchoredPosition = new Vector2(0f, 26f),
            sizeDelta = new Vector2(620f, 72f)
        });

        messageGroup = ResolvePanelGroup(MessagePanelName, allowRebuild, new PanelLayout
        {
            anchorMin = new Vector2(0.5f, 1f),
            anchorMax = new Vector2(0.5f, 1f),
            pivot = new Vector2(0.5f, 1f),
            anchoredPosition = new Vector2(0f, -26f),
            sizeDelta = new Vector2(760f, 126f)
        });

        contextText = ResolvePanelText(contextGroup, allowRebuild, 22f, TextAlignmentOptions.Center, "[E] Interact");
        messageText = ResolvePanelText(messageGroup, allowRebuild, 20f, TextAlignmentOptions.Center, "System feedback");

        if (contextText != null)
        {
            contextText.color = new Color(0.93f, 0.73f, 0.36f, 1f);
        }

        if (messageText != null)
        {
            messageText.color = new Color(0.94f, 0.94f, 0.95f, 1f);
        }

        if (contextGroup == messageGroup && contextGroup != null)
        {
            messageGroup = ResolvePanelGroup(MessagePanelName, true, new PanelLayout
            {
                anchorMin = new Vector2(0.5f, 1f),
                anchorMax = new Vector2(0.5f, 1f),
                pivot = new Vector2(0.5f, 1f),
                anchoredPosition = new Vector2(0f, -28f),
                sizeDelta = new Vector2(740f, 132f)
            });
            messageText = ResolvePanelText(messageGroup, true, 20f, TextAlignmentOptions.Center, "System feedback");
        }
    }

    private void EnsureCanvasScaffolding()
    {
        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect == null)
        {
            rootRect = gameObject.AddComponent<RectTransform>();
        }

        if (rootRect.localScale == Vector3.zero)
        {
            rootRect.localScale = Vector3.one;
        }

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

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

    private CanvasGroup ResolvePanelGroup(string panelName, bool allowRebuild, PanelLayout layout)
    {
        Transform panelTransform = transform.Find(panelName);
        if (panelTransform == null && allowRebuild)
        {
            GameObject panel = new GameObject(panelName, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
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

        panelRect.anchorMin = layout.anchorMin;
        panelRect.anchorMax = layout.anchorMax;
        panelRect.pivot = layout.pivot;
        panelRect.anchoredPosition = layout.anchoredPosition;
        panelRect.sizeDelta = layout.sizeDelta;

        CanvasGroup group = panelTransform.GetComponent<CanvasGroup>();
        if (group == null && allowRebuild)
        {
            group = panelTransform.gameObject.AddComponent<CanvasGroup>();
        }

        if (group != null)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        Image image = panelTransform.GetComponent<Image>();
        if (image == null && allowRebuild)
        {
            image = panelTransform.gameObject.AddComponent<Image>();
        }
        if (image != null)
        {
            image.color = panelName == ContextPanelName
                ? new Color(0.05f, 0.06f, 0.075f, 0.88f)
                : new Color(0.06f, 0.07f, 0.09f, 0.84f);
            image.raycastTarget = false;
        }

        return group;
    }

    private TextMeshProUGUI ResolvePanelText(CanvasGroup group, bool allowRebuild, float fontSize, TextAlignmentOptions alignment, string fallbackText)
    {
        if (group == null)
        {
            return null;
        }

        Transform textTransform = group.transform.Find(TextChildName);
        if (textTransform == null && allowRebuild)
        {
            GameObject textObject = new GameObject(TextChildName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(group.transform, false);
            textTransform = textObject.transform;
        }

        if (textTransform == null)
        {
            return null;
        }

        RectTransform textRect = textTransform as RectTransform;
        if (textRect == null)
        {
            textRect = textTransform.gameObject.AddComponent<RectTransform>();
        }
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16f, 10f);
        textRect.offsetMax = new Vector2(-16f, -10f);

        Text legacyText = textTransform.GetComponent<Text>();
        if (legacyText != null)
        {
            if (allowRebuild)
            {
                if (Application.isPlaying)
                {
                    Destroy(legacyText);
                }
                else
                {
                    DestroyImmediate(legacyText);
                }
            }
            else
            {
                return null;
            }
        }

        TextMeshProUGUI tmp = textTransform.GetComponent<TextMeshProUGUI>();
        if (tmp == null && allowRebuild)
        {
            tmp = textTransform.gameObject.AddComponent<TextMeshProUGUI>();
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
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        if (string.IsNullOrWhiteSpace(tmp.text))
        {
            tmp.text = fallbackText;
        }

        return tmp;
    }

    private struct PanelLayout
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
    }
}
