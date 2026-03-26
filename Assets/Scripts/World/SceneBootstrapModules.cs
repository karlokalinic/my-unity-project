using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Light-weight installers that break the VerticalSliceScenaBootstrap orchestration
/// into focused modules. Each module delegates to the existing bootstrap utilities,
/// keeping behavior identical while reducing the responsibility of the entrypoint.
/// </summary>
internal sealed class SceneCoreInstaller
{
    private readonly VerticalSliceScenaBootstrap host;

    public SceneCoreInstaller(VerticalSliceScenaBootstrap host)
    {
        this.host = host;
    }

    public void Apply()
    {
        EnsureRootGroups();
        ReparentSceneAnchors();
        EnsureTemplateSeparationLayout();
        EnsureCoreSystems();
    }

    private void EnsureRootGroups()
    {
        host.CoreRoot = VerticalSliceScenaBootstrap.EnsureRootObject("_Core").transform;
        host.WorldRoot = VerticalSliceScenaBootstrap.EnsureRootObject("_World").transform;
        host.GameplayRoot = VerticalSliceScenaBootstrap.EnsureRootObject("_Gameplay").transform;
        host.LightingRoot = VerticalSliceScenaBootstrap.EnsureRootObject("_Lighting").transform;
        host.DebugRoot = VerticalSliceScenaBootstrap.EnsureRootObject("_Debug").transform;
        host.UiRoot = VerticalSliceScenaBootstrap.EnsureChildObject(host.CoreRoot, "UI").transform;
    }

    private void ReparentSceneAnchors()
    {
        VerticalSliceScenaBootstrap.ReparentByName("Player", host.CoreRoot);
        VerticalSliceScenaBootstrap.ReparentByName("HolstinCameraRig", host.CoreRoot);
        VerticalSliceScenaBootstrap.ReparentByName("NavMeshSurfaceRoot", host.CoreRoot);
        VerticalSliceScenaBootstrap.ReparentByName("EventSystem", host.CoreRoot);
        VerticalSliceScenaBootstrap.ReparentByName("HolstinSceneContext", host.CoreRoot);
        VerticalSliceScenaBootstrap.ReparentByName("InteractionPromptUI", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("DialoguePanelUI", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("HealthStaminaHUD", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("InventoryPanelUI", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("TurnBasedCombatUI", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("ShopWindowUI", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("CombatReticleUI", host.UiRoot);
        VerticalSliceScenaBootstrap.ReparentByName("VerticalSliceObjectiveUI", host.UiRoot);

        VerticalSliceScenaBootstrap.ReparentByName("SceneGround", host.WorldRoot);
        VerticalSliceScenaBootstrap.ReparentByName("Template_Exterior_FogCourtyard", host.WorldRoot);
        VerticalSliceScenaBootstrap.ReparentByName("Template_Interior_BoardingHouse", host.WorldRoot);
        VerticalSliceScenaBootstrap.ReparentByName("Template_Underpass_Catacombs", host.WorldRoot);
        VerticalSliceScenaBootstrap.ReparentByName("HouseToUnderpassSteps", host.WorldRoot);

        VerticalSliceScenaBootstrap.ReparentByName("Directional Light", host.LightingRoot);
        VerticalSliceScenaBootstrap.ReparentByName("VS_CinematicVolumes", host.LightingRoot);
    }

    private void EnsureTemplateSeparationLayout()
    {
        if (!host.EnforceTemplateSeparation)
        {
            return;
        }

        GameObject exterior = VerticalSliceScenaBootstrap.FindSceneObject("Template_Exterior_FogCourtyard");
        GameObject interior = VerticalSliceScenaBootstrap.FindSceneObject("Template_Interior_BoardingHouse");
        GameObject underpass = VerticalSliceScenaBootstrap.FindSceneObject("Template_Underpass_Catacombs");

        if (exterior != null && exterior.transform.position.sqrMagnitude < 0.01f)
        {
            exterior.transform.position = host.ExteriorTemplateOrigin;
        }

        if (interior != null && interior.transform.position.sqrMagnitude < 0.01f)
        {
            interior.transform.position = host.InteriorTemplateOrigin;
        }

        if (underpass != null && underpass.transform.position.sqrMagnitude < 0.01f)
        {
            underpass.transform.position = host.UnderpassTemplateOrigin;
        }

        EnsurePairSeparation(exterior, interior, host.InteriorTemplateOrigin - host.ExteriorTemplateOrigin);
        EnsurePairSeparation(interior, underpass, host.UnderpassTemplateOrigin - host.InteriorTemplateOrigin);
        EnsurePairSeparation(exterior, underpass, host.UnderpassTemplateOrigin - host.ExteriorTemplateOrigin);
    }

    private void EnsurePairSeparation(GameObject first, GameObject second, Vector3 preferredDirection)
    {
        if (first == null || second == null)
        {
            return;
        }

        Vector3 delta = second.transform.position - first.transform.position;
        Vector3 planarDelta = new Vector3(delta.x, 0f, delta.z);
        float distance = planarDelta.magnitude;
        if (distance >= host.MinTemplateSeparation)
        {
            return;
        }

        Vector3 planarPreferred = new Vector3(preferredDirection.x, 0f, preferredDirection.z);
        if (planarPreferred.sqrMagnitude < 0.001f)
        {
            planarPreferred = Vector3.right;
        }

        Vector3 direction = distance > 0.001f ? planarDelta.normalized : planarPreferred.normalized;
        Vector3 target = first.transform.position + direction * host.MinTemplateSeparation;
        second.transform.position = new Vector3(target.x, second.transform.position.y, target.z);
    }

    private void EnsureCoreSystems()
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
            eventObject.transform.SetParent(host.CoreRoot, false);
            eventSystem = eventObject.GetComponent<EventSystem>();
        }

        if (eventSystem.transform.parent != host.CoreRoot)
        {
            eventSystem.transform.SetParent(host.CoreRoot, true);
        }

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (host.InputActions == null)
        {
            host.InputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (host.InputActions != null)
        {
            InputReader.BindActions(host.InputActions);
        }
#endif

        InteractionPromptUI promptUI = Object.FindAnyObjectByType<InteractionPromptUI>();
        if (promptUI == null)
        {
            GameObject promptObject = new GameObject(
                "InteractionPromptUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(InteractionPromptUI));
            promptObject.transform.SetParent(host.UiRoot, false);
            promptUI = promptObject.GetComponent<InteractionPromptUI>();
        }
        else if (promptUI.transform.parent != host.UiRoot)
        {
            promptUI.transform.SetParent(host.UiRoot, true);
        }

        DialoguePanelUI dialoguePanel = Object.FindAnyObjectByType<DialoguePanelUI>();
        if (dialoguePanel == null)
        {
            GameObject dialogueObject = new GameObject(
                "DialoguePanelUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(DialoguePanelUI));
            dialogueObject.transform.SetParent(host.UiRoot, false);
            dialoguePanel = dialogueObject.GetComponent<DialoguePanelUI>();
        }
        else if (dialoguePanel.transform.parent != host.UiRoot)
        {
            dialoguePanel.transform.SetParent(host.UiRoot, true);
        }

        HolstinSceneContext sceneContext = Object.FindAnyObjectByType<HolstinSceneContext>();
        if (sceneContext == null)
        {
            GameObject contextObject = new GameObject("HolstinSceneContext");
            contextObject.transform.SetParent(host.CoreRoot, false);
            sceneContext = contextObject.AddComponent<HolstinSceneContext>();
        }
        else if (sceneContext.transform.parent != host.CoreRoot)
        {
            sceneContext.transform.SetParent(host.CoreRoot, true);
        }

        PlayerMover playerMover = Object.FindAnyObjectByType<PlayerMover>();
        if (playerMover == null)
        {
            return;
        }

        if (playerMover.transform.parent != host.CoreRoot)
        {
            playerMover.transform.SetParent(host.CoreRoot, true);
        }

        InventorySystem inventory = playerMover.GetComponent<InventorySystem>();
        if (inventory == null)
        {
            inventory = playerMover.gameObject.AddComponent<InventorySystem>();
        }

        PlayerInteraction interaction = playerMover.GetComponent<PlayerInteraction>();
        if (interaction == null)
        {
            interaction = playerMover.gameObject.AddComponent<PlayerInteraction>();
        }

        HolstinCameraRig cameraRig = Object.FindAnyObjectByType<HolstinCameraRig>();
        if (cameraRig != null && cameraRig.transform.parent != host.CoreRoot)
        {
            cameraRig.transform.SetParent(host.CoreRoot, true);
        }

        if (cameraRig != null && cameraRig.GetComponent<PlayModeCursorLock>() == null)
        {
            cameraRig.gameObject.AddComponent<PlayModeCursorLock>();
        }

        Camera cameraComponent = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
        InspectItemViewer inspectViewer = Object.FindAnyObjectByType<InspectItemViewer>();
        if (cameraComponent != null)
        {
            Transform pickupAnchor = VerticalSliceScenaBootstrap.EnsureChildObject(cameraComponent.transform, "PickupAnchor").transform;
            pickupAnchor.localPosition = new Vector3(0.2f, -0.15f, 0.62f);
            pickupAnchor.localRotation = Quaternion.identity;

            playerMover.SetCameraForwardSource(cameraComponent.transform);
            interaction.ConfigureRuntimeReferences(inventory, promptUI, pickupAnchor);
        }

        if (cameraComponent != null && cameraRig != null && inspectViewer != null)
        {
            interaction.Configure(cameraComponent, cameraRig, inspectViewer);
        }

        CheckpointLiteManager checkpointManager = playerMover.GetComponent<CheckpointLiteManager>();
        if (checkpointManager == null)
        {
            checkpointManager = playerMover.gameObject.AddComponent<CheckpointLiteManager>();
        }
        checkpointManager.Configure(inventory, playerMover.GetComponent<CharacterController>(), playerMover);

        if (playerMover.GetComponent<RealTimeCombat>() == null)
        {
            playerMover.gameObject.AddComponent<RealTimeCombat>();
        }

        StarterLoadout starterLoadout = playerMover.GetComponent<StarterLoadout>();
        if (starterLoadout == null)
        {
            starterLoadout = playerMover.gameObject.AddComponent<StarterLoadout>();
        }
#if UNITY_EDITOR
        ItemDefinition pistol = VerticalSliceScenaBootstrap.FindItemAssetById("wpn_pistol");
        ItemDefinition rifle = VerticalSliceScenaBootstrap.FindItemAssetById("wpn_rifle");
        ItemDefinition shotgun = VerticalSliceScenaBootstrap.FindItemAssetById("wpn_shotgun");
        ItemDefinition knife = VerticalSliceScenaBootstrap.FindItemAssetById("wpn_knife");

        starterLoadout.Configure(
            pistol != null ? pistol : knife,
            new[] { pistol, rifle, shotgun, knife },
            new[]
            {
                ("ammo_pistol", 60),
                ("ammo_rifle", 32),
                ("ammo_shell", 20)
            });
#endif

        VerticalSliceScenaBootstrap.EnsureHumanoidActorSystems(playerMover.gameObject, true);

        sceneContext.Configure(cameraRig, promptUI, inspectViewer, playerMover, dialoguePanel);
        sceneContext.ResolveMissingReferences();

        EnsureRuntimeUISingleton<HealthStaminaHUD>("HealthStaminaHUD");
        EnsureRuntimeUISingleton<InventoryPanelUI>("InventoryPanelUI");
        EnsureRuntimeUISingleton<TurnBasedCombatUI>("TurnBasedCombatUI");
        EnsureRuntimeUISingleton<ShopWindowUI>("ShopWindowUI");
        EnsureRuntimeUISingleton<CombatReticleUI>("CombatReticleUI");
        EnsureRuntimeUISingleton<VerticalSliceObjectiveUI>("VerticalSliceObjectiveUI");
    }

    private void EnsureRuntimeUISingleton<T>(string objectName) where T : MonoBehaviour
    {
        if (Object.FindAnyObjectByType<T>() != null)
        {
            return;
        }

        GameObject go = new GameObject(objectName);
        go.AddComponent<T>();
        if (host.UiRoot != null)
        {
            go.transform.SetParent(host.UiRoot, false);
        }
    }
}

internal sealed class SceneInteractionInstaller
{
    private readonly VerticalSliceScenaBootstrap host;

    public SceneInteractionInstaller(VerticalSliceScenaBootstrap host)
    {
        this.host = host;
    }

    public void Apply()
    {
        host.EnsureNarrativeInteractionChain();
    }
}

internal sealed class SceneNarrativeInstaller
{
    private readonly VerticalSliceScenaBootstrap host;

    public SceneNarrativeInstaller(VerticalSliceScenaBootstrap host)
    {
        this.host = host;
    }

    public void Apply()
    {
        host.EnsureCinematicVolumes();
        host.EnsureInfectionSystem();
    }
}

internal sealed class SceneCombatInstaller
{
    private readonly VerticalSliceScenaBootstrap host;

    public SceneCombatInstaller(VerticalSliceScenaBootstrap host)
    {
        this.host = host;
    }

    public void Apply()
    {
        host.EnsureCharacterPresentationSystems();
    }
}
