using UnityEngine;

public class HolstinSceneContext : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private InteractionPromptUI promptUI;
    [SerializeField] private InspectItemViewer inspectViewer;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private DialoguePanelUI dialoguePanel;

    public static HolstinSceneContext Instance { get; private set; }

    public HolstinCameraRig CameraRig => cameraRig;
    public InteractionPromptUI PromptUI => promptUI;
    public InspectItemViewer InspectViewer => inspectViewer;
    public PlayerMover PlayerMover => playerMover;
    public Transform PlayerTransform => playerMover != null ? playerMover.transform : null;
    public DialoguePanelUI DialoguePanel => dialoguePanel;

    public static bool TryGet(out HolstinSceneContext context)
    {
        context = Instance;
        return context != null;
    }

    public void Configure(
        HolstinCameraRig configuredCameraRig,
        InteractionPromptUI configuredPromptUI,
        InspectItemViewer configuredInspectViewer,
        PlayerMover configuredPlayerMover,
        DialoguePanelUI configuredDialoguePanel = null)
    {
        if (configuredCameraRig != null)
        {
            cameraRig = configuredCameraRig;
        }

        if (configuredPromptUI != null)
        {
            promptUI = configuredPromptUI;
        }

        if (configuredInspectViewer != null)
        {
            inspectViewer = configuredInspectViewer;
        }

        if (configuredPlayerMover != null)
        {
            playerMover = configuredPlayerMover;
        }

        if (configuredDialoguePanel != null)
        {
            dialoguePanel = configuredDialoguePanel;
        }
    }

    public void ResolveMissingReferences()
    {
        if (cameraRig == null)
        {
            cameraRig = FindAnyObjectByType<HolstinCameraRig>();
        }

        if (promptUI == null)
        {
            promptUI = FindAnyObjectByType<InteractionPromptUI>();
        }

        if (inspectViewer == null)
        {
            inspectViewer = FindAnyObjectByType<InspectItemViewer>();
        }

        if (playerMover == null)
        {
            playerMover = FindAnyObjectByType<PlayerMover>();
        }

        if (dialoguePanel == null)
        {
            dialoguePanel = FindAnyObjectByType<DialoguePanelUI>();
        }
    }

    private void Awake()
    {
        RegisterInstance();
        ResolveMissingReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        ResolveMissingReferences();
    }

    private void RegisterInstance()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple HolstinSceneContext instances detected. Keeping the first instance.");
            return;
        }

        Instance = this;
    }
}
