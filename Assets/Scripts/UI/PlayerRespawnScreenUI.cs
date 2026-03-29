using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PlayerRespawnScreenUI : MonoBehaviour
{
    private const string RespawnCanvasName = "PlayerRespawnScreenCanvas";

    [Header("References")]
    [SerializeField] private CheckpointLiteManager checkpointManager;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Rules")]
    [SerializeField] private float fallDeathHeight = -8f;

    [Header("Text")]
    [SerializeField] private string title = "You Fell";
    [SerializeField] private string subtitle = "Respawn to the last checkpoint.";
    [SerializeField] private string buttonLabel = "Respawn";
    [SerializeField] private string hintLabel = "[Enter] Respawn";

    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private GraphicRaycaster raycaster;
    private RectTransform panel;
    private Button respawnButton;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;
    private TextMeshProUGUI hintText;
    private bool isShown;
    private int pauseToken;
    private bool ownsInputContext;
    private bool ownsCanvasObject;

    public void Configure(CheckpointLiteManager checkpoint, PlayerMover mover, PlayerInteraction interaction, float deathHeight)
    {
        if (checkpoint != null)
        {
            checkpointManager = checkpoint;
        }

        if (mover != null)
        {
            playerMover = mover;
        }

        if (interaction != null)
        {
            playerInteraction = interaction;
        }

        fallDeathHeight = deathHeight;
    }

    private void Awake()
    {
        if (playerMover == null)
        {
            playerMover = GetComponent<PlayerMover>();
        }

        if (playerInteraction == null)
        {
            playerInteraction = GetComponent<PlayerInteraction>();
        }

        if (checkpointManager == null)
        {
            checkpointManager = GetComponent<CheckpointLiteManager>();
        }

        BuildUi();
        HideImmediate();
    }

    private void Update()
    {
        if (!Application.isPlaying || playerMover == null)
        {
            return;
        }

        if (!isShown)
        {
            if (playerMover.transform.position.y < fallDeathHeight)
            {
                ShowScreen();
            }

            return;
        }

        if (InputReader.DialogueSubmitPressed() || InputReader.InteractPressed())
        {
            Respawn();
        }
    }

    private void OnDisable()
    {
        if (isShown)
        {
            HideImmediate();
        }
        else
        {
            ReleaseUiContext();
            ReleasePause();
        }
    }

    private void BuildUi()
    {
        if (panel != null)
        {
            return;
        }

        canvas = FindRespawnCanvasInScene();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                RespawnCanvasName,
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
        canvas.sortingOrder = 150;

        canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        panel = CreateRect("RespawnPanel", canvas.transform);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(520f, 240f);
        panel.anchoredPosition = Vector2.zero;

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

        RectTransform titleRect = CreateRect("Title", panel);
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -26f);
        titleRect.sizeDelta = new Vector2(-40f, 54f);
        titleText = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 40f;
        titleText.color = Color.white;
        titleText.text = title;

        RectTransform subtitleRect = CreateRect("Subtitle", panel);
        subtitleRect.anchorMin = new Vector2(0f, 1f);
        subtitleRect.anchorMax = new Vector2(1f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -92f);
        subtitleRect.sizeDelta = new Vector2(-48f, 40f);
        subtitleText = subtitleRect.gameObject.AddComponent<TextMeshProUGUI>();
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.fontSize = 22f;
        subtitleText.color = new Color(0.83f, 0.84f, 0.89f, 1f);
        subtitleText.text = subtitle;

        RectTransform buttonRect = CreateRect("RespawnButton", panel);
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 26f);
        buttonRect.sizeDelta = new Vector2(260f, 56f);

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.23f, 0.5f, 0.88f, 1f);

        respawnButton = buttonRect.gameObject.AddComponent<Button>();
        ColorBlock colors = respawnButton.colors;
        colors.normalColor = new Color(0.23f, 0.5f, 0.88f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.58f, 0.94f, 1f);
        colors.pressedColor = new Color(0.17f, 0.39f, 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        respawnButton.colors = colors;
        respawnButton.onClick.AddListener(Respawn);

        RectTransform buttonTextRect = CreateRect("ButtonText", buttonRect);
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI buttonText = buttonTextRect.gameObject.AddComponent<TextMeshProUGUI>();
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 24f;
        buttonText.color = Color.white;
        buttonText.text = buttonLabel;

        RectTransform hintRect = CreateRect("Hint", panel);
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 6f);
        hintRect.sizeDelta = new Vector2(-40f, 24f);
        hintText = hintRect.gameObject.AddComponent<TextMeshProUGUI>();
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.fontSize = 18f;
        hintText.color = new Color(0.84f, 0.85f, 0.9f, 0.95f);
        hintText.text = hintLabel;
    }

    private void ShowScreen()
    {
        isShown = true;
        panel.gameObject.SetActive(true);
        titleText.text = title;
        subtitleText.text = subtitle;
        hintText.text = hintLabel;

        AcquireUiContext();
        AcquirePause();

        if (playerMover != null)
        {
            playerMover.ResetMotion();
            playerMover.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.SetBusy(false);
            playerInteraction.enabled = false;
        }
    }

    private void Respawn()
    {
        if (!isShown)
        {
            return;
        }

        checkpointManager?.RespawnNow();

        if (playerMover != null)
        {
            playerMover.ResetMotion();
            playerMover.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        isShown = false;
        panel.gameObject.SetActive(false);
        ReleaseUiContext();
        ReleasePause();
    }

    private void HideImmediate()
    {
        isShown = false;
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }

    private void AcquirePause()
    {
        if (pauseToken != 0)
        {
            return;
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

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<RectTransform>();
    }

    private Canvas FindRespawnCanvasInScene()
    {
        Scene scene = gameObject.scene;
        Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas candidate = canvases[i];
            if (candidate == null ||
                candidate.gameObject.scene != scene ||
                !string.Equals(candidate.gameObject.name, RespawnCanvasName, System.StringComparison.Ordinal))
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
