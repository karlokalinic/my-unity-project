using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PauseSettingsMenuUI : MonoBehaviour
{
    private const string PauseCanvasName = "PauseSettingsCanvas";

    [SerializeField] private RuntimeGraphicsQualityController qualityController;

    private Canvas canvas;
    private RectTransform panel;
    private RectTransform settingsRoot;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI renderQualityText;
    private TextMeshProUGUI textureQualityText;
    private Button renderQualityButton;
    private Button textureQualityButton;

    private readonly Dictionary<InputReader.KeyboardBindingAction, TextMeshProUGUI> bindingLabels = new Dictionary<InputReader.KeyboardBindingAction, TextMeshProUGUI>();

    private bool ownsCanvasObject;
    private bool ownsInputContext;
    private bool isOpen;
    private bool settingsVisible;
    private int pauseToken;
    private bool ownsTimeScale;
    private float previousTimeScale = 1f;
    private InputReader.KeyboardBindingAction? pendingRebind;

    private void Awake()
    {
        BuildUi();
        HideImmediate();
    }

    private void Update()
    {
        ResolveQualityController();

        if (!isOpen)
        {
            if (InputReader.CancelPressed())
            {
                OpenMenu();
            }

            return;
        }

        if (pendingRebind.HasValue)
        {
            CaptureRebind();
            return;
        }

        if (InputReader.CancelPressed())
        {
            if (settingsVisible)
            {
                SetSettingsVisible(false);
            }
            else
            {
                CloseMenu();
            }
        }
    }

    private void OnDisable()
    {
        CancelPendingRebind();
        ReleaseUiContext();
        ReleasePause();
    }

    private void BuildUi()
    {
        if (panel != null)
        {
            return;
        }

        canvas = FindPauseCanvasInScene();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                PauseCanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Scene scene = gameObject.scene;
            if (scene.IsValid())
            {
                SceneManager.MoveGameObjectToScene(canvasObject, scene);
            }

            canvas = canvasObject.GetComponent<Canvas>();
            ownsCanvasObject = true;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreateRect("PauseRoot", canvas.transform);
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(1020f, 760f);
        panel = root;

        Image panelImage = root.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.045f, 0.06f, 0.96f);

        RectTransform titleRect = CreateRect("Title", root);
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(-40f, 56f);
        TextMeshProUGUI titleText = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 48f;
        titleText.color = Color.white;
        titleText.text = "Paused";

        RectTransform mainButtons = CreateRect("MainButtons", root);
        mainButtons.anchorMin = new Vector2(0f, 1f);
        mainButtons.anchorMax = new Vector2(0f, 1f);
        mainButtons.pivot = new Vector2(0f, 1f);
        mainButtons.anchoredPosition = new Vector2(36f, -96f);
        mainButtons.sizeDelta = new Vector2(260f, 180f);

        Button resumeButton = CreateActionButton(mainButtons, "ResumeButton", "Resume", new Vector2(0f, 0f));
        resumeButton.onClick.AddListener(CloseMenu);

        Button settingsButton = CreateActionButton(mainButtons, "SettingsButton", "Settings", new Vector2(0f, -66f));
        settingsButton.onClick.AddListener(() => SetSettingsVisible(true));

        Button closeSettingsButton = CreateActionButton(mainButtons, "BackButton", "Back", new Vector2(0f, -132f));
        closeSettingsButton.onClick.AddListener(() => SetSettingsVisible(false));

        settingsRoot = CreateRect("SettingsRoot", root);
        settingsRoot.anchorMin = new Vector2(0f, 0f);
        settingsRoot.anchorMax = new Vector2(1f, 1f);
        settingsRoot.offsetMin = new Vector2(320f, 24f);
        settingsRoot.offsetMax = new Vector2(-24f, -86f);

        RectTransform qualityHeaderRect = CreateRect("QualityHeader", settingsRoot);
        qualityHeaderRect.anchorMin = new Vector2(0f, 1f);
        qualityHeaderRect.anchorMax = new Vector2(1f, 1f);
        qualityHeaderRect.pivot = new Vector2(0.5f, 1f);
        qualityHeaderRect.anchoredPosition = new Vector2(0f, 0f);
        qualityHeaderRect.sizeDelta = new Vector2(0f, 36f);
        TextMeshProUGUI qualityHeader = qualityHeaderRect.gameObject.AddComponent<TextMeshProUGUI>();
        qualityHeader.alignment = TextAlignmentOptions.MidlineLeft;
        qualityHeader.fontSize = 28f;
        qualityHeader.color = Color.white;
        qualityHeader.text = "Settings";

        renderQualityButton = CreateInlineValueButton(settingsRoot, "RenderQuality", "Render Quality", new Vector2(0f, -54f), out renderQualityText);
        renderQualityButton.onClick.AddListener(CycleRenderQuality);

        textureQualityButton = CreateInlineValueButton(settingsRoot, "TextureQuality", "Texture Quality", new Vector2(0f, -108f), out textureQualityText);
        textureQualityButton.onClick.AddListener(ToggleTextureQuality);

        RectTransform keymapHeaderRect = CreateRect("KeymapHeader", settingsRoot);
        keymapHeaderRect.anchorMin = new Vector2(0f, 1f);
        keymapHeaderRect.anchorMax = new Vector2(1f, 1f);
        keymapHeaderRect.pivot = new Vector2(0.5f, 1f);
        keymapHeaderRect.anchoredPosition = new Vector2(0f, -166f);
        keymapHeaderRect.sizeDelta = new Vector2(0f, 32f);
        TextMeshProUGUI keymapHeader = keymapHeaderRect.gameObject.AddComponent<TextMeshProUGUI>();
        keymapHeader.alignment = TextAlignmentOptions.MidlineLeft;
        keymapHeader.fontSize = 24f;
        keymapHeader.color = new Color(0.91f, 0.93f, 0.98f, 1f);
        keymapHeader.text = "Keyboard Controls";

        RectTransform rowsRoot = CreateRect("RowsRoot", settingsRoot);
        rowsRoot.anchorMin = new Vector2(0f, 1f);
        rowsRoot.anchorMax = new Vector2(1f, 1f);
        rowsRoot.pivot = new Vector2(0.5f, 1f);
        rowsRoot.anchoredPosition = new Vector2(0f, -206f);
        rowsRoot.sizeDelta = new Vector2(0f, 420f);

        IReadOnlyList<InputReader.KeyboardBindingAction> bindingOrder = InputReader.GetKeyboardBindingOrder();
        float rowHeight = 30f;
        for (int i = 0; i < bindingOrder.Count; i++)
        {
            InputReader.KeyboardBindingAction action = bindingOrder[i];
            RectTransform row = CreateRect("Row_" + action, rowsRoot);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -(i * rowHeight));
            row.sizeDelta = new Vector2(0f, rowHeight - 2f);

            RectTransform actionLabelRect = CreateRect("ActionLabel", row);
            actionLabelRect.anchorMin = new Vector2(0f, 0f);
            actionLabelRect.anchorMax = new Vector2(0.64f, 1f);
            actionLabelRect.offsetMin = Vector2.zero;
            actionLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI actionLabel = actionLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            actionLabel.alignment = TextAlignmentOptions.MidlineLeft;
            actionLabel.fontSize = 19f;
            actionLabel.color = new Color(0.86f, 0.88f, 0.93f, 1f);
            actionLabel.text = InputReader.GetBindingActionDisplayName(action);

            RectTransform keyButtonRect = CreateRect("KeyButton", row);
            keyButtonRect.anchorMin = new Vector2(0.66f, 0f);
            keyButtonRect.anchorMax = new Vector2(1f, 1f);
            keyButtonRect.offsetMin = new Vector2(0f, 2f);
            keyButtonRect.offsetMax = new Vector2(0f, -2f);

            Image keyButtonImage = keyButtonRect.gameObject.AddComponent<Image>();
            keyButtonImage.color = new Color(0.16f, 0.2f, 0.3f, 0.96f);
            Button keyButton = keyButtonRect.gameObject.AddComponent<Button>();
            ColorBlock keyColors = keyButton.colors;
            keyColors.normalColor = keyButtonImage.color;
            keyColors.highlightedColor = new Color(0.22f, 0.28f, 0.4f, 1f);
            keyColors.pressedColor = new Color(0.12f, 0.16f, 0.26f, 1f);
            keyColors.selectedColor = keyColors.highlightedColor;
            keyButton.colors = keyColors;

            RectTransform keyLabelRect = CreateRect("KeyLabel", keyButtonRect);
            keyLabelRect.anchorMin = Vector2.zero;
            keyLabelRect.anchorMax = Vector2.one;
            keyLabelRect.offsetMin = Vector2.zero;
            keyLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI keyLabel = keyLabelRect.gameObject.AddComponent<TextMeshProUGUI>();
            keyLabel.alignment = TextAlignmentOptions.Center;
            keyLabel.fontSize = 18f;
            keyLabel.color = Color.white;

            InputReader.KeyboardBindingAction capturedAction = action;
            keyButton.onClick.AddListener(() => BeginRebind(capturedAction));
            bindingLabels[capturedAction] = keyLabel;
        }

        RectTransform mouseHeaderRect = CreateRect("MouseHeader", settingsRoot);
        mouseHeaderRect.anchorMin = new Vector2(0f, 0f);
        mouseHeaderRect.anchorMax = new Vector2(1f, 0f);
        mouseHeaderRect.pivot = new Vector2(0.5f, 0f);
        mouseHeaderRect.anchoredPosition = new Vector2(0f, 98f);
        mouseHeaderRect.sizeDelta = new Vector2(0f, 28f);
        TextMeshProUGUI mouseHeader = mouseHeaderRect.gameObject.AddComponent<TextMeshProUGUI>();
        mouseHeader.alignment = TextAlignmentOptions.MidlineLeft;
        mouseHeader.fontSize = 22f;
        mouseHeader.color = new Color(0.91f, 0.93f, 0.98f, 1f);
        mouseHeader.text = "Mouse Controls";

        RectTransform mouseTextRect = CreateRect("MouseText", settingsRoot);
        mouseTextRect.anchorMin = new Vector2(0f, 0f);
        mouseTextRect.anchorMax = new Vector2(1f, 0f);
        mouseTextRect.pivot = new Vector2(0.5f, 0f);
        mouseTextRect.anchoredPosition = new Vector2(0f, 20f);
        mouseTextRect.sizeDelta = new Vector2(0f, 72f);
        TextMeshProUGUI mouseText = mouseTextRect.gameObject.AddComponent<TextMeshProUGUI>();
        mouseText.alignment = TextAlignmentOptions.TopLeft;
        mouseText.fontSize = 18f;
        mouseText.color = new Color(0.82f, 0.85f, 0.91f, 1f);
        mouseText.text = "Look/Aim: Mouse Move\nAim (First Person): Right Mouse Button\nFire: Left Mouse Button";

        RectTransform statusRect = CreateRect("Status", root);
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.anchoredPosition = new Vector2(0f, 12f);
        statusRect.sizeDelta = new Vector2(-40f, 30f);
        statusText = statusRect.gameObject.AddComponent<TextMeshProUGUI>();
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontSize = 18f;
        statusText.color = new Color(0.9f, 0.9f, 0.95f, 0.95f);
        statusText.text = "Esc = pause/resume";
    }

    private void OpenMenu()
    {
        isOpen = true;
        panel.gameObject.SetActive(true);
        SetSettingsVisible(false);
        RefreshAllLabels();
        AcquirePause();
        AcquireUiContext();
    }

    private void CloseMenu()
    {
        CancelPendingRebind();
        isOpen = false;
        panel.gameObject.SetActive(false);
        ReleaseUiContext();
        ReleasePause();
    }

    private void HideImmediate()
    {
        isOpen = false;
        settingsVisible = false;
        pendingRebind = null;
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void SetSettingsVisible(bool visible)
    {
        settingsVisible = visible;
        if (settingsRoot != null)
        {
            settingsRoot.gameObject.SetActive(visible);
        }

        if (!visible)
        {
            CancelPendingRebind();
            statusText.text = "Esc = pause/resume";
            return;
        }

        RefreshAllLabels();
    }

    private void RefreshAllLabels()
    {
        RefreshQualityLabels();
        RefreshBindingLabels();
    }

    private void RefreshQualityLabels()
    {
        ResolveQualityController();
        if (qualityController == null)
        {
            if (renderQualityText != null) renderQualityText.text = "Unavailable";
            if (textureQualityText != null) textureQualityText.text = "Unavailable";
            return;
        }

        if (renderQualityText != null)
        {
            renderQualityText.text = qualityController.GetCurrentRenderQuality().ToString();
        }

        if (textureQualityText != null)
        {
            textureQualityText.text = qualityController.GetCurrentQuality().ToString();
        }
    }

    private void RefreshBindingLabels()
    {
        foreach (KeyValuePair<InputReader.KeyboardBindingAction, TextMeshProUGUI> pair in bindingLabels)
        {
            if (pair.Value == null)
            {
                continue;
            }

            Key boundKey = InputReader.GetKeyboardBinding(pair.Key);
            pair.Value.text = boundKey.ToString().ToUpperInvariant();
        }
    }

    private void CycleRenderQuality()
    {
        ResolveQualityController();
        if (qualityController == null)
        {
            return;
        }

        RuntimeGraphicsQualityController.RenderQualityOption next = qualityController.GetCurrentRenderQuality() switch
        {
            RuntimeGraphicsQualityController.RenderQualityOption.Low => RuntimeGraphicsQualityController.RenderQualityOption.Medium,
            RuntimeGraphicsQualityController.RenderQualityOption.Medium => RuntimeGraphicsQualityController.RenderQualityOption.High,
            RuntimeGraphicsQualityController.RenderQualityOption.High => RuntimeGraphicsQualityController.RenderQualityOption.Ultra,
            _ => RuntimeGraphicsQualityController.RenderQualityOption.Low
        };

        qualityController.SetRenderQuality(next);
        RefreshQualityLabels();
        statusText.text = $"Render quality: {next}";
    }

    private void ToggleTextureQuality()
    {
        ResolveQualityController();
        if (qualityController == null)
        {
            return;
        }

        RuntimeGraphicsQualityController.TextureQualityOption next = qualityController.GetCurrentQuality() == RuntimeGraphicsQualityController.TextureQualityOption.K2
            ? RuntimeGraphicsQualityController.TextureQualityOption.K4
            : RuntimeGraphicsQualityController.TextureQualityOption.K2;

        if (next == RuntimeGraphicsQualityController.TextureQualityOption.K2)
        {
            qualityController.SetTextureQuality2K();
        }
        else
        {
            qualityController.SetTextureQuality4K();
        }

        RefreshQualityLabels();
        statusText.text = $"Texture quality: {next}";
    }

    private void BeginRebind(InputReader.KeyboardBindingAction action)
    {
        pendingRebind = action;
        statusText.text = $"Press a key for {InputReader.GetBindingActionDisplayName(action)} (Esc cancels).";
    }

    private void CaptureRebind()
    {
        if (!pendingRebind.HasValue)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelPendingRebind();
            return;
        }

        var allKeys = keyboard.allKeys;
        for (int i = 0; i < allKeys.Count; i++)
        {
            KeyControl keyControl = allKeys[i];
            if (keyControl == null || !keyControl.wasPressedThisFrame)
            {
                continue;
            }

            Key key = keyControl.keyCode;
            InputReader.KeyboardBindingAction action = pendingRebind.Value;
            InputReader.SetKeyboardBinding(action, key);
            pendingRebind = null;
            RefreshBindingLabels();
            statusText.text = $"{InputReader.GetBindingActionDisplayName(action)} bound to {key.ToString().ToUpperInvariant()}.";
            return;
        }
    }

    private void CancelPendingRebind()
    {
        pendingRebind = null;
    }

    private void AcquirePause()
    {
        if (pauseToken != 0)
        {
            return;
        }

        bool alreadyPaused = GameplayPauseFacade.IsPaused;
        if (!alreadyPaused)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            ownsTimeScale = true;
        }
        else
        {
            ownsTimeScale = false;
        }

        pauseToken = GameplayPauseFacade.PushPause();
    }

    private void ReleasePause()
    {
        if (pauseToken == 0)
        {
            return;
        }

        GameplayPauseFacade.PopPause(pauseToken);
        pauseToken = 0;

        if (ownsTimeScale)
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }

        ownsTimeScale = false;

    }

    private void AcquireUiContext()
    {
        if (ownsInputContext)
        {
            return;
        }

        InputReader.PushContext(InputReader.InputContext.UI);
        ownsInputContext = true;
    }

    private void ReleaseUiContext()
    {
        if (!ownsInputContext)
        {
            return;
        }

        InputReader.PopContext(InputReader.InputContext.Gameplay);
        ownsInputContext = false;
    }

    private void ResolveQualityController()
    {
        if (qualityController != null)
        {
            return;
        }

        qualityController = FindAnyObjectByType<RuntimeGraphicsQualityController>();
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static Button CreateActionButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(250f, 56f);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.29f, 0.42f, 0.96f);
        Button button = rect.gameObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.28f, 0.36f, 0.51f, 1f);
        colors.pressedColor = new Color(0.17f, 0.23f, 0.34f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        RectTransform textRect = CreateRect("Label", rect);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24f;
        text.color = Color.white;
        text.text = label;
        return button;
    }

    private static Button CreateInlineValueButton(Transform parent, string name, string label, Vector2 anchoredPosition, out TextMeshProUGUI valueText)
    {
        RectTransform row = CreateRect(name + "Row", parent);
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = anchoredPosition;
        row.sizeDelta = new Vector2(0f, 42f);

        RectTransform labelRect = CreateRect("Label", row);
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0.45f, 1f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI labelText = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.fontSize = 20f;
        labelText.color = new Color(0.88f, 0.9f, 0.95f, 1f);
        labelText.text = label;

        RectTransform buttonRect = CreateRect("Button", row);
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.offsetMin = new Vector2(0f, 2f);
        buttonRect.offsetMax = new Vector2(0f, -2f);

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.17f, 0.22f, 0.33f, 0.96f);
        Button button = buttonRect.gameObject.AddComponent<Button>();

        RectTransform valueRect = CreateRect("Value", buttonRect);
        valueRect.anchorMin = Vector2.zero;
        valueRect.anchorMax = Vector2.one;
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;
        valueText = valueRect.gameObject.AddComponent<TextMeshProUGUI>();
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.fontSize = 20f;
        valueText.color = Color.white;
        valueText.text = "-";

        return button;
    }

    private Canvas FindPauseCanvasInScene()
    {
        Scene scene = gameObject.scene;
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas candidate = canvases[i];
            if (candidate == null ||
                candidate.gameObject.scene != scene ||
                !string.Equals(candidate.gameObject.name, PauseCanvasName, System.StringComparison.Ordinal))
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    private void OnDestroy()
    {
        if (!ownsCanvasObject || canvas == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(canvas.gameObject);
        }
        else
        {
            DestroyImmediate(canvas.gameObject);
        }
    }
}
