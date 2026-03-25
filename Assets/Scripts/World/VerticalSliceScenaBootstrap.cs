using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class VerticalSliceScenaBootstrap : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Scena";
    [SerializeField] private string secondaryTargetSceneName = "INTERAKCIJA";
    [SerializeField] private bool autoApplyInPlayMode = true;
    [SerializeField] private bool autoApplyInEditMode = true;
    [SerializeField] private bool logOperations;

    [SerializeField] private Transform coreRoot;
    [SerializeField] private Transform worldRoot;
    [SerializeField] private Transform gameplayRoot;
    [SerializeField] private Transform lightingRoot;
    [SerializeField] private Transform debugRoot;
    [SerializeField] private Transform uiRoot;
    [SerializeField] private InfectionDirector infectionDirector;

    [Header("Template Layout")]
    [SerializeField] private bool enforceTemplateSeparation = true;
    [SerializeField] private float minTemplateSeparation = 18f;
    [SerializeField] private Vector3 exteriorTemplateOrigin = new Vector3(-18f, 0f, -6f);
    [SerializeField] private Vector3 interiorTemplateOrigin = new Vector3(14f, 0f, -2f);
    [SerializeField] private Vector3 underpassTemplateOrigin = new Vector3(12f, -5.5f, 10f);

    private bool queuedEditorApply;

    private void OnEnable()
    {
        if (!IsTargetScene())
        {
            return;
        }

        if (Application.isPlaying)
        {
            if (autoApplyInPlayMode)
            {
                ApplyRetrofit();
            }
            return;
        }

#if UNITY_EDITOR
        if (autoApplyInEditMode && !queuedEditorApply)
        {
            queuedEditorApply = true;
            EditorApplication.delayCall += ApplyRetrofitDelayed;
        }
#endif
    }

#if UNITY_EDITOR
    private void ApplyRetrofitDelayed()
    {
        queuedEditorApply = false;
        if (this == null || Application.isPlaying || !autoApplyInEditMode || !IsTargetScene())
        {
            return;
        }

        ApplyRetrofit();
    }
#endif

    [ContextMenu("Apply Vertical Slice Retrofit")]
    public void ApplyRetrofit()
    {
        if (!IsTargetScene())
        {
            return;
        }

        EnsureRootGroups();
        ReparentSceneAnchors();
        EnsureTemplateSeparationLayout();
        EnsureCoreSystems();
        EnsureNarrativeInteractionChain();
        EnsureCinematicVolumes();
        EnsureInfectionSystem();
        EnsureCharacterPresentationSystems();

#if UNITY_EDITOR
        RemoveMissingScriptsInActiveScene();
#endif

        if (logOperations)
        {
            Debug.Log("VerticalSliceScenaBootstrap applied.");
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
#endif
    }

    private bool IsTargetScene()
    {
        Scene scene = gameObject.scene;
        if (!scene.IsValid())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName) && string.IsNullOrWhiteSpace(secondaryTargetSceneName))
        {
            return true;
        }

        return SceneNameMatches(scene.name, targetSceneName) ||
               SceneNameMatches(scene.name, secondaryTargetSceneName);
    }

    private static bool SceneNameMatches(string sceneName, string targetName)
    {
        return !string.IsNullOrWhiteSpace(targetName) &&
               string.Equals(sceneName, targetName, StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureRootGroups()
    {
        coreRoot = EnsureRootObject("_Core").transform;
        worldRoot = EnsureRootObject("_World").transform;
        gameplayRoot = EnsureRootObject("_Gameplay").transform;
        lightingRoot = EnsureRootObject("_Lighting").transform;
        debugRoot = EnsureRootObject("_Debug").transform;
        uiRoot = EnsureChildObject(coreRoot, "UI").transform;
    }

    private void ReparentSceneAnchors()
    {
        ReparentByName("Player", coreRoot);
        ReparentByName("HolstinCameraRig", coreRoot);
        ReparentByName("NavMeshSurfaceRoot", coreRoot);
        ReparentByName("EventSystem", coreRoot);
        ReparentByName("HolstinSceneContext", coreRoot);
        ReparentByName("InteractionPromptUI", uiRoot);
        ReparentByName("DialoguePanelUI", uiRoot);
        ReparentByName("HealthStaminaHUD", uiRoot);
        ReparentByName("InventoryPanelUI", uiRoot);
        ReparentByName("TurnBasedCombatUI", uiRoot);
        ReparentByName("ShopWindowUI", uiRoot);

        ReparentByName("SceneGround", worldRoot);
        ReparentByName("Template_Exterior_FogCourtyard", worldRoot);
        ReparentByName("Template_Interior_BoardingHouse", worldRoot);
        ReparentByName("Template_Underpass_Catacombs", worldRoot);
        ReparentByName("HouseToUnderpassSteps", worldRoot);

        ReparentByName("Directional Light", lightingRoot);
        ReparentByName("VS_CinematicVolumes", lightingRoot);
    }

    private void EnsureTemplateSeparationLayout()
    {
        if (!enforceTemplateSeparation)
        {
            return;
        }

        GameObject exterior = FindSceneObject("Template_Exterior_FogCourtyard");
        GameObject interior = FindSceneObject("Template_Interior_BoardingHouse");
        GameObject underpass = FindSceneObject("Template_Underpass_Catacombs");

        if (exterior != null && exterior.transform.position.sqrMagnitude < 0.01f)
        {
            exterior.transform.position = exteriorTemplateOrigin;
        }

        if (interior != null && interior.transform.position.sqrMagnitude < 0.01f)
        {
            interior.transform.position = interiorTemplateOrigin;
        }

        if (underpass != null && underpass.transform.position.sqrMagnitude < 0.01f)
        {
            underpass.transform.position = underpassTemplateOrigin;
        }

        EnsurePairSeparation(exterior, interior, interiorTemplateOrigin - exteriorTemplateOrigin);
        EnsurePairSeparation(interior, underpass, underpassTemplateOrigin - interiorTemplateOrigin);
        EnsurePairSeparation(exterior, underpass, underpassTemplateOrigin - exteriorTemplateOrigin);
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
        if (distance >= minTemplateSeparation)
        {
            return;
        }

        Vector3 planarPreferred = new Vector3(preferredDirection.x, 0f, preferredDirection.z);
        if (planarPreferred.sqrMagnitude < 0.001f)
        {
            planarPreferred = Vector3.right;
        }

        Vector3 direction = distance > 0.001f ? planarDelta.normalized : planarPreferred.normalized;
        Vector3 target = first.transform.position + direction * minTemplateSeparation;
        second.transform.position = new Vector3(target.x, second.transform.position.y, target.z);
    }

    private void EnsureCoreSystems()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
            eventObject.transform.SetParent(coreRoot, false);
            eventSystem = eventObject.GetComponent<EventSystem>();
        }

        if (eventSystem.transform.parent != coreRoot)
        {
            eventSystem.transform.SetParent(coreRoot, true);
        }

#if ENABLE_INPUT_SYSTEM && !UNITY_DISABLE_INPUT_SYSTEM
        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
#endif

        InteractionPromptUI promptUI = FindAnyObjectByType<InteractionPromptUI>();
        if (promptUI == null)
        {
            GameObject promptObject = new GameObject(
                "InteractionPromptUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(InteractionPromptUI));
            promptObject.transform.SetParent(uiRoot, false);
            promptUI = promptObject.GetComponent<InteractionPromptUI>();
        }
        else if (promptUI.transform.parent != uiRoot)
        {
            promptUI.transform.SetParent(uiRoot, true);
        }

        DialoguePanelUI dialoguePanel = FindAnyObjectByType<DialoguePanelUI>();
        if (dialoguePanel == null)
        {
            GameObject dialogueObject = new GameObject(
                "DialoguePanelUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(DialoguePanelUI));
            dialogueObject.transform.SetParent(uiRoot, false);
            dialoguePanel = dialogueObject.GetComponent<DialoguePanelUI>();
        }
        else if (dialoguePanel.transform.parent != uiRoot)
        {
            dialoguePanel.transform.SetParent(uiRoot, true);
        }

        HolstinSceneContext sceneContext = FindAnyObjectByType<HolstinSceneContext>();
        if (sceneContext == null)
        {
            GameObject contextObject = new GameObject("HolstinSceneContext");
            contextObject.transform.SetParent(coreRoot, false);
            sceneContext = contextObject.AddComponent<HolstinSceneContext>();
        }
        else if (sceneContext.transform.parent != coreRoot)
        {
            sceneContext.transform.SetParent(coreRoot, true);
        }

        PlayerMover playerMover = FindAnyObjectByType<PlayerMover>();
        if (playerMover == null)
        {
            return;
        }

        if (playerMover.transform.parent != coreRoot)
        {
            playerMover.transform.SetParent(coreRoot, true);
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

        HolstinCameraRig cameraRig = FindAnyObjectByType<HolstinCameraRig>();
        if (cameraRig != null && cameraRig.transform.parent != coreRoot)
        {
            cameraRig.transform.SetParent(coreRoot, true);
        }

        Camera cameraComponent = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        InspectItemViewer inspectViewer = FindAnyObjectByType<InspectItemViewer>();
        if (cameraComponent != null)
        {
            Transform pickupAnchor = EnsureChildObject(cameraComponent.transform, "PickupAnchor").transform;
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
        ItemDefinition pistol = FindItemAssetById("wpn_pistol");
        ItemDefinition rifle = FindItemAssetById("wpn_rifle");
        ItemDefinition shotgun = FindItemAssetById("wpn_shotgun");
        ItemDefinition knife = FindItemAssetById("wpn_knife");

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

        EnsureHumanoidActorSystems(playerMover.gameObject, true);

        sceneContext.Configure(cameraRig, promptUI, inspectViewer, playerMover, dialoguePanel);
        sceneContext.ResolveMissingReferences();

        // Runtime UI singletons
        EnsureRuntimeUISingleton<HealthStaminaHUD>("HealthStaminaHUD");
        EnsureRuntimeUISingleton<InventoryPanelUI>("InventoryPanelUI");
        EnsureRuntimeUISingleton<TurnBasedCombatUI>("TurnBasedCombatUI");
        EnsureRuntimeUISingleton<ShopWindowUI>("ShopWindowUI");
        EnsureRuntimeUISingleton<CombatReticleUI>("CombatReticleUI");
        EnsureRuntimeUISingleton<VerticalSliceObjectiveUI>("VerticalSliceObjectiveUI");
    }

    private void EnsureRuntimeUISingleton<T>(string objectName) where T : MonoBehaviour
    {
        if (FindAnyObjectByType<T>() != null) return;
        GameObject go = new GameObject(objectName);
        go.AddComponent<T>();
        if (uiRoot != null) go.transform.SetParent(uiRoot, false);
    }

    private void EnsureNarrativeInteractionChain()
    {
        GameObject sandboxRoot = FindSceneObject("Template_Interactable_Sandbox");
        if (sandboxRoot != null)
        {
            EnsureLegacyInteractableSandboxExpansion(sandboxRoot.transform.position);
            EnsureReachAnchorsForInteractables();
            return;
        }

        Transform loopRoot = EnsureChildObject(gameplayRoot, "VS_InteractionLoop").transform;
        Vector3 exteriorOrigin = ResolveTemplateOrigin("Template_Exterior_FogCourtyard", new Vector3(-18f, 0f, -6f));
        Vector3 interiorOrigin = ResolveTemplateOrigin("Template_Interior_BoardingHouse", new Vector3(14f, 0f, -2f));
        Vector3 underpassOrigin = ResolveTemplateOrigin("Template_Underpass_Catacombs", new Vector3(12f, -5.5f, 10f));

        EnsureExteriorPickup(loopRoot, exteriorOrigin);
        EnsureInteriorDoor(loopRoot, interiorOrigin);
        EnsureMidpointNpc(loopRoot, interiorOrigin);
        EnsureUnderpassConsole(loopRoot, underpassOrigin);
        EnsureCheckpointZones(loopRoot, exteriorOrigin, interiorOrigin, underpassOrigin);
        EnsureCombatEncounters(loopRoot, exteriorOrigin, underpassOrigin);
        EnsureReachAnchorsForInteractables();
    }

    private void EnsureCharacterPresentationSystems()
    {
        PlayerMover playerMover = FindAnyObjectByType<PlayerMover>();
        if (playerMover != null)
        {
            EnsureHumanoidActorSystems(playerMover.gameObject, true);
        }

        // Legacy key-giver NPCs
        NPCKeyGiverInteractable[] dialogueNpcs = FindObjectsByType<NPCKeyGiverInteractable>(FindObjectsInactive.Exclude);
        for (int i = 0; i < dialogueNpcs.Length; i++)
        {
            NPCKeyGiverInteractable npc = dialogueNpcs[i];
            if (npc == null) continue;
            EnsureHumanoidActorSystems(npc.gameObject, false);
            EnsureNpcDialogueCameraAnchor(npc.gameObject);
        }

        // RPG skill-check NPCs
        SkillCheckNpcInteractable[] skillNpcs = FindObjectsByType<SkillCheckNpcInteractable>(FindObjectsInactive.Exclude);
        for (int i = 0; i < skillNpcs.Length; i++)
        {
            SkillCheckNpcInteractable npc = skillNpcs[i];
            if (npc == null) continue;
            EnsureHumanoidActorSystems(npc.gameObject, false);
            EnsureNpcDialogueCameraAnchor(npc.gameObject);
        }

        // Shop merchant NPCs
        ShopInteractable[] shopNpcs = FindObjectsByType<ShopInteractable>(FindObjectsInactive.Exclude);
        for (int i = 0; i < shopNpcs.Length; i++)
        {
            ShopInteractable npc = shopNpcs[i];
            if (npc == null) continue;
            EnsureHumanoidActorSystems(npc.gameObject, false);
        }

        // Combat enemies
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude);
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null) continue;
            EnsureHumanoidActorSystems(enemy.gameObject, false);
        }
    }

    private static void EnsureHumanoidActorSystems(GameObject actorRoot, bool playerControlled)
    {
        if (actorRoot == null)
        {
            return;
        }

        ProceduralHumanoidRig rig = actorRoot.GetComponent<ProceduralHumanoidRig>();
        if (rig == null)
        {
            rig = actorRoot.AddComponent<ProceduralHumanoidRig>();
        }
        rig.ConfigureRendererVisibility(false, false);
        rig.EnsureBuilt();

        if (actorRoot.GetComponent<ActiveRagdollMotor>() == null)
        {
            actorRoot.AddComponent<ActiveRagdollMotor>();
        }

        if (actorRoot.GetComponent<DeathRagdollController>() == null)
        {
            actorRoot.AddComponent<DeathRagdollController>();
        }

        if (playerControlled)
        {
            if (actorRoot.GetComponent<PlayerAnimationController>() == null)
            {
                actorRoot.AddComponent<PlayerAnimationController>();
            }

            if (actorRoot.GetComponent<PlayerReachController>() == null)
            {
                actorRoot.AddComponent<PlayerReachController>();
            }
        }
    }

    private static void EnsureNpcDialogueCameraAnchor(GameObject npcObject)
    {
        if (npcObject == null)
        {
            return;
        }

        NpcDialogueController dialogueController = npcObject.GetComponent<NpcDialogueController>();
        if (dialogueController == null)
        {
            dialogueController = npcObject.AddComponent<NpcDialogueController>();
        }

        Transform anchor = npcObject.transform.Find("DialogueCameraAnchor");
        if (anchor == null)
        {
            GameObject anchorObject = new GameObject("DialogueCameraAnchor");
            anchorObject.transform.SetParent(npcObject.transform, false);
            anchor = anchorObject.transform;
        }

        anchor.localPosition = new Vector3(0.65f, 1.55f, 1.35f);
        anchor.LookAt(npcObject.transform.position + Vector3.up * 1.35f);
        dialogueController.ConfigureCameraAnchor(anchor);
    }

    private static void EnsureReachAnchorsForInteractables()
    {
        InteractableBase[] interactables = FindObjectsByType<InteractableBase>(FindObjectsInactive.Exclude);
        for (int i = 0; i < interactables.Length; i++)
        {
            InteractableBase interactable = interactables[i];
            if (interactable == null)
            {
                continue;
            }

            if (interactable.GetComponentInChildren<ReachTargetAnchor>() == null)
            {
                interactable.gameObject.AddComponent<ReachTargetAnchor>();
            }
        }
    }

    private void EnsureCinematicVolumes()
    {
        Transform volumeRoot = EnsureChildObject(lightingRoot, "VS_CinematicVolumes").transform;
        Volume globalVolume = EnsureVolume(
            EnsureChildObject(volumeRoot, "VS_GlobalVolume").transform,
            true,
            new Vector3(1f, 1f, 1f),
            new Color(0.96f, 0.95f, 0.91f, 1f),
            -0.08f,
            0.32f,
            0.22f);
        globalVolume.priority = -10f;
        globalVolume.weight = 0.35f;

        Vector3 exteriorOrigin = ResolveTemplateOrigin("Template_Exterior_FogCourtyard", new Vector3(-18f, 0f, -6f));
        Vector3 interiorOrigin = ResolveTemplateOrigin("Template_Interior_BoardingHouse", new Vector3(14f, 0f, -2f));
        Vector3 underpassOrigin = ResolveTemplateOrigin("Template_Underpass_Catacombs", new Vector3(12f, -5.5f, 10f));

        EnsureVolume(EnsureChildObject(volumeRoot, "VS_Volume_Exterior").transform, false, new Vector3(36f, 8f, 24f), new Color(0.9f, 0.96f, 0.95f, 1f), -0.1f, 0.34f, 0.2f).transform.position = exteriorOrigin + new Vector3(0f, 2f, 0f);
        EnsureVolume(EnsureChildObject(volumeRoot, "VS_Volume_Interior").transform, false, new Vector3(26f, 10f, 18f), new Color(1f, 0.94f, 0.86f, 1f), -0.2f, 0.22f, 0.25f).transform.position = interiorOrigin + new Vector3(0f, 4f, 0f);
        EnsureVolume(EnsureChildObject(volumeRoot, "VS_Volume_Underpass").transform, false, new Vector3(30f, 10f, 20f), new Color(0.78f, 0.84f, 0.9f, 1f), -0.45f, 0.45f, 0.36f).transform.position = underpassOrigin + new Vector3(4f, 2f, 0f);
    }

    private void EnsureInfectionSystem()
    {
        Transform infectionRoot = EnsureChildObject(gameplayRoot, "VS_InfectionSystem").transform;
        InfectionNode exteriorNode = EnsureNode(infectionRoot, "VS_Node_Exterior", "Exterior District");
        InfectionNode interiorNode = EnsureNode(infectionRoot, "VS_Node_Interior", "Boarding House");
        InfectionNode underpassNode = EnsureNode(infectionRoot, "VS_Node_Underpass", "Underpass");

        GameObject streetEnemy = FindSceneObject("StreetProwler_Root");
        GameObject courtyardEnemy = FindSceneObject("CourtyardProwler_Root");
        GameObject tunnelEnemy = FindSceneObject("TunnelStalker_Root");

        Volume exteriorVolume = FindSceneObject("VS_Volume_Exterior")?.GetComponent<Volume>();
        Volume interiorVolume = FindSceneObject("VS_Volume_Interior")?.GetComponent<Volume>();
        Volume underpassVolume = FindSceneObject("VS_Volume_Underpass")?.GetComponent<Volume>();

        exteriorNode.Configure("Exterior District", InfectionStage.Dormant, new[]
        {
            new InfectionNode.StagePayload { stage = InfectionStage.Dormant, deactivateObjects = FilterNull(streetEnemy, courtyardEnemy), volumes = FilterNull(exteriorVolume), volumeWeight = 0.1f },
            new InfectionNode.StagePayload { stage = InfectionStage.Active, activateObjects = FilterNull(streetEnemy), volumes = FilterNull(exteriorVolume), volumeWeight = 0.42f, narrativeOverride = "Exterior district destabilized." },
            new InfectionNode.StagePayload { stage = InfectionStage.Overrun, activateObjects = FilterNull(streetEnemy, courtyardEnemy), volumes = FilterNull(exteriorVolume), volumeWeight = 0.82f, narrativeOverride = "Exterior district overrun." }
        });

        interiorNode.Configure("Boarding House", InfectionStage.Dormant, new[]
        {
            new InfectionNode.StagePayload { stage = InfectionStage.Dormant, volumes = FilterNull(interiorVolume), volumeWeight = 0.14f },
            new InfectionNode.StagePayload { stage = InfectionStage.Active, volumes = FilterNull(interiorVolume), volumeWeight = 0.44f, narrativeOverride = "Boarding house contamination active." },
            new InfectionNode.StagePayload { stage = InfectionStage.Overrun, volumes = FilterNull(interiorVolume), volumeWeight = 0.84f, narrativeOverride = "Boarding house overrun." }
        });

        underpassNode.Configure("Underpass", InfectionStage.Dormant, new[]
        {
            new InfectionNode.StagePayload { stage = InfectionStage.Dormant, deactivateObjects = FilterNull(tunnelEnemy), volumes = FilterNull(underpassVolume), volumeWeight = 0.18f },
            new InfectionNode.StagePayload { stage = InfectionStage.Active, activateObjects = FilterNull(tunnelEnemy), volumes = FilterNull(underpassVolume), volumeWeight = 0.52f, narrativeOverride = "Underpass contamination active." },
            new InfectionNode.StagePayload { stage = InfectionStage.Overrun, activateObjects = FilterNull(tunnelEnemy), volumes = FilterNull(underpassVolume), volumeWeight = 0.9f, narrativeOverride = "Underpass overrun." }
        });

        if (infectionDirector == null)
        {
            infectionDirector = FindAnyObjectByType<InfectionDirector>();
        }

        if (infectionDirector == null)
        {
            infectionDirector = EnsureChildObject(infectionRoot, "InfectionDirector").AddComponent<InfectionDirector>();
        }

        infectionDirector.ConfigureGraph(
            new[] { exteriorNode, interiorNode, underpassNode },
            new[]
            {
                new InfectionDirector.SpreadStep { node = exteriorNode, targetStage = InfectionStage.Active, note = "Exterior active." },
                new InfectionDirector.SpreadStep { node = interiorNode, targetStage = InfectionStage.Active, note = "Interior active." },
                new InfectionDirector.SpreadStep { node = underpassNode, targetStage = InfectionStage.Active, note = "Underpass active." },
                new InfectionDirector.SpreadStep { node = exteriorNode, targetStage = InfectionStage.Overrun, note = "Exterior overrun." },
                new InfectionDirector.SpreadStep { node = interiorNode, targetStage = InfectionStage.Overrun, note = "Interior overrun." },
                new InfectionDirector.SpreadStep { node = underpassNode, targetStage = InfectionStage.Overrun, note = "Underpass overrun." }
            },
            new[]
            {
                new InfectionDirector.MilestoneSpreadJump { milestoneId = "pickup_exterior_key", steps = 1, consumeOnce = true, note = "Pickup spread jump." },
                new InfectionDirector.MilestoneSpreadJump { milestoneId = "unlock_interior_gate", steps = 1, consumeOnce = true, note = "Door spread jump." },
                new InfectionDirector.MilestoneSpreadJump { milestoneId = "npc_reward_key", steps = 1, consumeOnce = true, note = "NPC spread jump." },
                new InfectionDirector.MilestoneSpreadJump { milestoneId = "console_service_unlock", steps = 2, consumeOnce = true, note = "Console spread jump." }
            },
            95f,
            true);
    }

    private void EnsureExteriorPickup(Transform parent, Vector3 exteriorOrigin)
    {
        GameObject pickupRoot = EnsurePrimitiveObject(PrimitiveType.Cube, "VS_ExteriorKeyPickup", parent, exteriorOrigin + new Vector3(4.3f, 0.85f, 1.6f), new Vector3(0.32f, 0.1f, 0.32f));
        EnsurePrimitiveObject(PrimitiveType.Cylinder, "VS_ExteriorKeyPickup_Pedestal", pickupRoot.transform, pickupRoot.transform.position + new Vector3(0f, -0.35f, 0f), new Vector3(0.36f, 0.34f, 0.36f));
        BoxCollider colliderComponent = pickupRoot.GetComponent<BoxCollider>();
        colliderComponent.size = new Vector3(0.65f, 0.32f, 0.65f);
        colliderComponent.center = Vector3.zero;

        PickupInteractable pickup = pickupRoot.GetComponent<PickupInteractable>();
        if (pickup == null) pickup = pickupRoot.AddComponent<PickupInteractable>();
        pickup.ConfigureItem("old_key", "Old Key", "Service key recovered from exterior checkpoint crates.", "pickup_exterior_key");
    }

    private void EnsureInteriorDoor(Transform parent, Vector3 interiorOrigin)
    {
        GameObject frameRoot = EnsureChildObject(parent, "VS_InteriorGate_Frame");
        frameRoot.transform.position = interiorOrigin + new Vector3(-2.1f, 0f, -1f);
        Vector3 pos = frameRoot.transform.position;
        EnsurePrimitiveObject(PrimitiveType.Cube, "FrameLeft", frameRoot.transform, pos + new Vector3(0f, 1.35f, -1.1f), new Vector3(0.35f, 2.7f, 0.22f));
        EnsurePrimitiveObject(PrimitiveType.Cube, "FrameRight", frameRoot.transform, pos + new Vector3(0f, 1.35f, 1.1f), new Vector3(0.35f, 2.7f, 0.22f));
        EnsurePrimitiveObject(PrimitiveType.Cube, "FrameTop", frameRoot.transform, pos + new Vector3(0f, 2.5f, 0f), new Vector3(0.35f, 0.25f, 2.5f));
        GameObject doorLeaf = EnsurePrimitiveObject(PrimitiveType.Cube, "VS_InteriorGate_Door", frameRoot.transform, pos + new Vector3(0f, 1.1f, 0f), new Vector3(0.25f, 2.2f, 2f));
        DoorInteractable door = doorLeaf.GetComponent<DoorInteractable>();
        if (door == null) door = doorLeaf.AddComponent<DoorInteractable>();
        door.ConfigureLock("old_key", "Old Key", true, false, "unlock_interior_gate");
    }

    private void EnsureMidpointNpc(Transform parent, Vector3 interiorOrigin)
    {
        GameObject npc = EnsurePrimitiveObject(PrimitiveType.Capsule, "VS_CaretakerNPC", parent, interiorOrigin + new Vector3(-4f, 1f, 0.7f), new Vector3(0.8f, 1f, 0.8f));
        NpcIdentity placeholder = npc.GetComponent<NpcIdentity>();
        if (placeholder == null) placeholder = npc.AddComponent<NpcIdentity>();
        PlayerMover player = FindAnyObjectByType<PlayerMover>();
        placeholder.Configure("Caretaker", "Keeps exits open and explanations short.", player != null ? player.transform : null);
        NPCKeyGiverInteractable keyGiver = npc.GetComponent<NPCKeyGiverInteractable>();
        if (keyGiver == null) keyGiver = npc.AddComponent<NPCKeyGiverInteractable>();
        keyGiver.ConfigureReward("service_key", "Service Key", true, "npc_reward_key");
    }

    private void EnsureUnderpassConsole(Transform parent, Vector3 underpassOrigin)
    {
        GameObject barrier = EnsurePrimitiveObject(PrimitiveType.Cube, "VS_ServiceBarrier", parent, underpassOrigin + new Vector3(3.7f, 1.15f, 1f), new Vector3(0.22f, 2.3f, 2.2f));
        barrier.SetActive(true);

        GameObject beaconRoot = EnsureChildObject(parent, "VS_ServiceBeacon");
        beaconRoot.transform.position = underpassOrigin + new Vector3(6.3f, 2.2f, 1f);
        Light beaconLight = beaconRoot.GetComponent<Light>();
        if (beaconLight == null) beaconLight = beaconRoot.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.range = 8f;
        beaconLight.intensity = 4.6f;
        beaconLight.color = new Color(0.52f, 0.78f, 1f, 1f);
        beaconRoot.SetActive(false);

        GameObject console = EnsurePrimitiveObject(PrimitiveType.Cube, "VS_ServiceConsole", parent, underpassOrigin + new Vector3(2.3f, 0.9f, -0.8f), new Vector3(1.1f, 1.8f, 0.9f));
        EnsurePrimitiveObject(PrimitiveType.Cube, "VS_ServiceConsoleScreen", console.transform, console.transform.position + new Vector3(0f, 0.48f, 0.46f), new Vector3(0.72f, 0.34f, 0.08f));
        ItemConsumeInteractable consume = console.GetComponent<ItemConsumeInteractable>();
        if (consume == null) consume = console.AddComponent<ItemConsumeInteractable>();
        consume.ConfigureRequirement(
            "service_key",
            "Service Key",
            false,
            "Console accepts the service key. Pathway unlocked.",
            "The console rejects you without a service key.",
            new[] { beaconRoot },
            new[] { barrier },
            true,
            "console_service_unlock");
    }

    private void EnsureCheckpointZones(Transform parent, Vector3 exteriorOrigin, Vector3 interiorOrigin, Vector3 underpassOrigin)
    {
        EnsureCheckpointZone(
            parent,
            "VS_Checkpoint_Exterior",
            exteriorOrigin + new Vector3(-0.5f, 1f, 0f),
            new Vector3(3.5f, 2f, 3.5f),
            "Checkpoint set: Exterior district.");

        EnsureCheckpointZone(
            parent,
            "VS_Checkpoint_Interior",
            interiorOrigin + new Vector3(-1.7f, 1f, 0f),
            new Vector3(3.2f, 2f, 3.2f),
            "Checkpoint set: Boarding house.");

        EnsureCheckpointZone(
            parent,
            "VS_Checkpoint_Underpass",
            underpassOrigin + new Vector3(2.2f, 1f, -1f),
            new Vector3(3.5f, 2f, 3.5f),
            "Checkpoint set: Underpass.");
    }

    private void EnsureCombatEncounters(Transform parent, Vector3 exteriorOrigin, Vector3 underpassOrigin)
    {
        EnsureTutorialEnemy(
            parent,
            "VS_ExteriorProwler",
            exteriorOrigin + new Vector3(8.3f, 1f, -1.6f),
            new[]
            {
                exteriorOrigin + new Vector3(6.1f, 0f, -3.7f),
                exteriorOrigin + new Vector3(9.8f, 0f, -3.2f),
                exteriorOrigin + new Vector3(9.4f, 0f, 0.6f),
                exteriorOrigin + new Vector3(6.2f, 0f, 1.1f)
            });

        EnsureTutorialEnemy(
            parent,
            "VS_UnderpassStalker",
            underpassOrigin + new Vector3(5f, 1f, 2.2f),
            new[]
            {
                underpassOrigin + new Vector3(3.4f, 0f, 0.2f),
                underpassOrigin + new Vector3(6.8f, 0f, 0.4f),
                underpassOrigin + new Vector3(6.6f, 0f, 3.4f),
                underpassOrigin + new Vector3(3.5f, 0f, 3.7f)
            });
    }

    private void EnsureLegacyInteractableSandboxExpansion(Vector3 sandboxOrigin)
    {
        GameObject legacyEastWall = FindSceneObject("EastWall");
        if (legacyEastWall == null || Mathf.Abs(legacyEastWall.transform.position.x - (sandboxOrigin.x + 10f)) > 0.75f)
        {
            return;
        }

        Transform expansionRoot = EnsureChildObject(gameplayRoot, "INT_LegacyExpansion").transform;
        legacyEastWall.SetActive(false);

        EnsurePrimitiveObject(PrimitiveType.Cube, "INT_Floor_Extension", expansionRoot, sandboxOrigin + new Vector3(20f, -0.05f, 0f), new Vector3(20f, 0.2f, 10f));
        EnsurePrimitiveObject(PrimitiveType.Cube, "INT_NorthWall_Extension", expansionRoot, sandboxOrigin + new Vector3(20f, 1.6f, 5f), new Vector3(20f, 3.2f, 0.35f));
        EnsurePrimitiveObject(PrimitiveType.Cube, "INT_SouthWall_Extension", expansionRoot, sandboxOrigin + new Vector3(20f, 1.6f, -5f), new Vector3(20f, 3.2f, 0.35f));
        EnsurePrimitiveObject(PrimitiveType.Cube, "INT_EastWall_Extension", expansionRoot, sandboxOrigin + new Vector3(30f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 10f));

        EnsurePartitionWithOpening(expansionRoot, "INT_RelayPartition", sandboxOrigin + new Vector3(14f, 0f, 0f));
        EnsurePartitionWithOpening(expansionRoot, "INT_ExitPartition", sandboxOrigin + new Vector3(22f, 0f, 0f));

        EnsureNarrativeZone(expansionRoot, "INT_RelayNarrative", sandboxOrigin + new Vector3(10f, 1f, 0f), new Vector3(7f, 2f, 8f), "Relay Hall", "Two rails. Two fuses. Both must go live.");
        EnsureNarrativeZone(expansionRoot, "INT_ArchiveNarrative", sandboxOrigin + new Vector3(18f, 1f, 0f), new Vector3(7f, 2f, 8f), "Archive Lane", "Records remain when witnesses do not.");
        EnsureNarrativeZone(expansionRoot, "INT_FinalNarrative", sandboxOrigin + new Vector3(26f, 1f, 0f), new Vector3(7f, 2f, 8f), "Exit Chamber", "Seal, verify, and move.");
        EnsureCameraZone(expansionRoot, "INT_RelayCam", sandboxOrigin + new Vector3(10f, 1f, 0f), new Vector3(7f, 2f, 8f), 45f);
        EnsureCameraZone(expansionRoot, "INT_ArchiveCam", sandboxOrigin + new Vector3(18f, 1f, 0f), new Vector3(7f, 2f, 8f), 135f);
        EnsureCameraZone(expansionRoot, "INT_FinalCam", sandboxOrigin + new Vector3(26f, 1f, 0f), new Vector3(7f, 2f, 8f), 45f);

        EnsureTutorialPickup(expansionRoot, "INT_RelayFuseA", sandboxOrigin + new Vector3(8.6f, 0.84f, 2.25f), "relay_fuse_a", "Relay Fuse A", "Stamped for rail segment A.", "relay_fuse_a_picked");
        EnsureTutorialPickup(expansionRoot, "INT_RelayFuseB", sandboxOrigin + new Vector3(11.9f, 0.84f, -2.25f), "relay_fuse_b", "Relay Fuse B", "Stamped for rail segment B.", "relay_fuse_b_picked");

        GameObject relayBarrierNorth = EnsurePrimitiveObject(PrimitiveType.Cube, "INT_RelayBarrierNorth", expansionRoot, sandboxOrigin + new Vector3(14f, 1.1f, 1.25f), new Vector3(0.22f, 2.2f, 1.2f));
        GameObject relayBarrierSouth = EnsurePrimitiveObject(PrimitiveType.Cube, "INT_RelayBarrierSouth", expansionRoot, sandboxOrigin + new Vector3(14f, 1.1f, -1.25f), new Vector3(0.22f, 2.2f, 1.2f));

        GameObject relayIndicatorA = EnsurePointLight(expansionRoot, "INT_RelayIndicatorA", sandboxOrigin + new Vector3(13f, 2.25f, 2.4f), new Color(0.44f, 0.78f, 1f, 1f), 6f, 4.2f);
        GameObject relayIndicatorB = EnsurePointLight(expansionRoot, "INT_RelayIndicatorB", sandboxOrigin + new Vector3(13f, 2.25f, -2.4f), new Color(0.44f, 0.78f, 1f, 1f), 6f, 4.2f);
        relayIndicatorA.SetActive(false);
        relayIndicatorB.SetActive(false);

        EnsureTutorialConsole(
            expansionRoot,
            "INT_RelayConsoleA",
            sandboxOrigin + new Vector3(9f, 0.9f, 2.45f),
            "relay_fuse_a",
            "Relay Fuse A",
            true,
            "Rail A online. One rail left.",
            "Console A needs Relay Fuse A.",
            relayIndicatorA,
            relayBarrierNorth,
            "relay_a_online");

        EnsureTutorialConsole(
            expansionRoot,
            "INT_RelayConsoleB",
            sandboxOrigin + new Vector3(12f, 0.9f, -2.45f),
            "relay_fuse_b",
            "Relay Fuse B",
            true,
            "Rail B online. Passage clear.",
            "Console B needs Relay Fuse B.",
            relayIndicatorB,
            relayBarrierSouth,
            "relay_b_online");

        EnsureTutorialEnemy(expansionRoot, "INT_ArchiveEnforcer", sandboxOrigin + new Vector3(18.2f, 1f, 2.3f), new[]
        {
            sandboxOrigin + new Vector3(16.5f, 0f, 2.4f),
            sandboxOrigin + new Vector3(19.6f, 0f, 2.4f),
            sandboxOrigin + new Vector3(19.6f, 0f, -2.4f),
            sandboxOrigin + new Vector3(16.5f, 0f, -2.4f)
        });

        EnsureTutorialNpc(
            expansionRoot,
            "INT_ArchivistNPC",
            sandboxOrigin + new Vector3(18.4f, 1f, -1.6f),
            "Archivist",
            "Protects passes and stories with equal suspicion.",
            "archive_key",
            "Archive Key",
            "archive_key_granted",
            "You made it through relay. Take the archive key.",
            "The archive key is already with you.");

        EnsureTutorialLockedDoor(
            expansionRoot,
            "INT_ArchiveExit",
            sandboxOrigin + new Vector3(22f, 0f, 0f),
            "archive_key",
            "Archive Key",
            "Archive lock: no key, no exit.",
            "archive_door_opened");

        GameObject finalBarrier = EnsurePrimitiveObject(PrimitiveType.Cube, "INT_FinalSecurityBarrier", expansionRoot, sandboxOrigin + new Vector3(26f, 1.1f, 0f), new Vector3(0.24f, 2.2f, 2.4f));
        GameObject finalBeacon = EnsurePointLight(expansionRoot, "INT_FinalBeacon", sandboxOrigin + new Vector3(27.8f, 2.3f, -2.2f), new Color(1f, 0.62f, 0.42f, 1f), 8f, 4.8f);
        finalBeacon.SetActive(false);

        EnsureTutorialConsole(
            expansionRoot,
            "INT_FinalConsole",
            sandboxOrigin + new Vector3(24.1f, 0.9f, -1.9f),
            "service_key",
            "Service Key",
            false,
            "Seal acknowledged. Exit chamber unlocked.",
            "The chamber console expects a service key.",
            finalBeacon,
            finalBarrier,
            "console_service_unlock");

        EnsureCheckpointZone(expansionRoot, "INT_Checkpoint_Relay", sandboxOrigin + new Vector3(10f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Relay hall.");
        EnsureCheckpointZone(expansionRoot, "INT_Checkpoint_Archive", sandboxOrigin + new Vector3(18f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Archive lane.");
        EnsureCheckpointZone(expansionRoot, "INT_Checkpoint_Final", sandboxOrigin + new Vector3(26f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Exit chamber.");
    }

    private static void EnsurePartitionWithOpening(Transform parent, string name, Vector3 center)
    {
        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_South", parent, center + new Vector3(0f, 1.6f, -3.9f), new Vector3(0.35f, 3.2f, 2.1f));
        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_North", parent, center + new Vector3(0f, 1.6f, 3.9f), new Vector3(0.35f, 3.2f, 2.1f));
        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_Lintel", parent, center + new Vector3(0f, 2.8f, 0f), new Vector3(0.35f, 0.8f, 2.4f));
    }

    private static GameObject EnsurePointLight(Transform parent, string name, Vector3 position, Color color, float range, float intensity)
    {
        GameObject root = EnsureChildObject(parent, name);
        root.transform.position = position;
        Light light = root.GetComponent<Light>();
        if (light == null) light = root.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;

        TutorialAmbientMotion ambient = root.GetComponent<TutorialAmbientMotion>();
        if (ambient == null) ambient = root.AddComponent<TutorialAmbientMotion>();
        ambient.Configure(root.transform, light);

        return root;
    }

    private static void EnsureNarrativeZone(Transform parent, string name, Vector3 center, Vector3 size, string locationName, string summary)
    {
        GameObject zoneObject = EnsureChildObject(parent, name);
        zoneObject.transform.position = center;
        BoxCollider colliderComponent = zoneObject.GetComponent<BoxCollider>();
        if (colliderComponent == null) colliderComponent = zoneObject.AddComponent<BoxCollider>();
        colliderComponent.isTrigger = true;
        colliderComponent.size = size;
        NarrativeZone narrativeZone = zoneObject.GetComponent<NarrativeZone>();
        if (narrativeZone == null) narrativeZone = zoneObject.AddComponent<NarrativeZone>();
        narrativeZone.Configure(locationName, summary, true);
    }

    private static void EnsureCameraZone(Transform parent, string name, Vector3 center, Vector3 size, float yaw)
    {
        GameObject zoneObject = EnsureChildObject(parent, name);
        zoneObject.transform.position = center;
        BoxCollider colliderComponent = zoneObject.GetComponent<BoxCollider>();
        if (colliderComponent == null) colliderComponent = zoneObject.AddComponent<BoxCollider>();
        colliderComponent.isTrigger = true;
        colliderComponent.size = size;
        CameraAngleZone cameraZone = zoneObject.GetComponent<CameraAngleZone>();
        if (cameraZone == null) cameraZone = zoneObject.AddComponent<CameraAngleZone>();
        cameraZone.Configure(yaw, false);
    }

    private static PickupInteractable EnsureTutorialPickup(Transform parent, string name, Vector3 worldPosition, string itemId, string displayName, string description, string milestone)
    {
        GameObject pickup = EnsurePrimitiveObject(PrimitiveType.Cube, name, parent, worldPosition, new Vector3(0.34f, 0.1f, 0.24f));
        BoxCollider colliderComponent = pickup.GetComponent<BoxCollider>();
        colliderComponent.size = new Vector3(0.7f, 0.35f, 0.45f);
        PickupInteractable interactable = pickup.GetComponent<PickupInteractable>();
        if (interactable == null) interactable = pickup.AddComponent<PickupInteractable>();
        interactable.ConfigureItem(itemId, displayName, description, milestone);
        return interactable;
    }

    private static ItemConsumeInteractable EnsureTutorialConsole(
        Transform parent,
        string name,
        Vector3 worldPosition,
        string requiredItemId,
        string requiredDisplayName,
        bool consumeItem,
        string successMessage,
        string missingMessage,
        GameObject activateOnSuccess,
        GameObject deactivateOnSuccess,
        string milestoneId)
    {
        GameObject console = EnsurePrimitiveObject(PrimitiveType.Cube, name, parent, worldPosition, new Vector3(1.1f, 1.8f, 0.9f));
        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_Screen", console.transform, worldPosition + new Vector3(0f, 0.5f, 0.46f), new Vector3(0.72f, 0.34f, 0.08f));

        ItemConsumeInteractable consume = console.GetComponent<ItemConsumeInteractable>();
        if (consume == null) consume = console.AddComponent<ItemConsumeInteractable>();
        consume.ConfigureRequirement(
            requiredItemId,
            requiredDisplayName,
            consumeItem,
            successMessage,
            missingMessage,
            activateOnSuccess != null ? new[] { activateOnSuccess } : Array.Empty<GameObject>(),
            deactivateOnSuccess != null ? new[] { deactivateOnSuccess } : Array.Empty<GameObject>(),
            true,
            milestoneId);
        return consume;
    }

    private static void EnsureTutorialLockedDoor(
        Transform parent,
        string name,
        Vector3 origin,
        string requiredItemId,
        string requiredDisplayName,
        string lockedMessage,
        string milestoneId)
    {
        GameObject frameRoot = EnsureChildObject(parent, name + "_Frame");
        frameRoot.transform.position = origin;

        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_FrameLeft", frameRoot.transform, origin + new Vector3(0f, 1.4f, -1.15f), new Vector3(0.38f, 2.8f, 0.24f));
        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_FrameRight", frameRoot.transform, origin + new Vector3(0f, 1.4f, 1.15f), new Vector3(0.38f, 2.8f, 0.24f));
        EnsurePrimitiveObject(PrimitiveType.Cube, name + "_FrameTop", frameRoot.transform, origin + new Vector3(0f, 2.55f, 0f), new Vector3(0.38f, 0.24f, 2.55f));
        GameObject leaf = EnsurePrimitiveObject(PrimitiveType.Cube, name + "_Leaf", frameRoot.transform, origin + new Vector3(0f, 1.15f, 0f), new Vector3(0.26f, 2.3f, 2.1f));

        DoorInteractable door = leaf.GetComponent<DoorInteractable>();
        if (door == null) door = leaf.AddComponent<DoorInteractable>();
        door.ConfigureLock(requiredItemId, requiredDisplayName, true, false, milestoneId);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SerializedObject so = new SerializedObject(door);
            SerializedProperty lockedProp = so.FindProperty("lockedMessage");
            if (lockedProp != null) lockedProp.stringValue = lockedMessage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }

    private static void EnsureTutorialNpc(
        Transform parent,
        string name,
        Vector3 worldPosition,
        string npcName,
        string role,
        string rewardItemId,
        string rewardDisplayName,
        string rewardMilestone,
        string firstConversation,
        string repeatConversation)
    {
        GameObject npc = EnsurePrimitiveObject(PrimitiveType.Capsule, name, parent, worldPosition, new Vector3(0.8f, 1f, 0.8f));
        NpcIdentity placeholder = npc.GetComponent<NpcIdentity>();
        if (placeholder == null) placeholder = npc.AddComponent<NpcIdentity>();
        PlayerMover player = FindAnyObjectByType<PlayerMover>();
        placeholder.Configure(npcName, role, player != null ? player.transform : null);

        NPCKeyGiverInteractable keyGiver = npc.GetComponent<NPCKeyGiverInteractable>();
        if (keyGiver == null) keyGiver = npc.AddComponent<NPCKeyGiverInteractable>();
        keyGiver.ConfigureReward(rewardItemId, rewardDisplayName, true, rewardMilestone);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SerializedObject so = new SerializedObject(keyGiver);
            SerializedProperty first = so.FindProperty("firstConversation");
            if (first != null) first.stringValue = firstConversation;
            SerializedProperty repeat = so.FindProperty("repeatConversation");
            if (repeat != null) repeat.stringValue = repeatConversation;
            SerializedProperty npcLabel = so.FindProperty("npcName");
            if (npcLabel != null) npcLabel.stringValue = npcName;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }

    private static void EnsureTutorialEnemy(Transform parent, string rootName, Vector3 worldPosition, Vector3[] patrolPoints)
    {
        GameObject enemyRoot = EnsureChildObject(parent, rootName + "_Root");
        GameObject enemy = EnsurePrimitiveObject(PrimitiveType.Capsule, rootName, enemyRoot.transform, worldPosition, new Vector3(0.9f, 1f, 0.9f));

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent == null) agent = enemy.AddComponent<NavMeshAgent>();
        agent.radius = 0.35f;
        agent.speed = 3.25f;
        agent.angularSpeed = 720f;
        agent.acceleration = 18f;
        if (!NavMesh.SamplePosition(enemy.transform.position, out _, 2.5f, NavMesh.AllAreas))
        {
            agent.enabled = false;
        }

        Transform[] points = new Transform[patrolPoints.Length];
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            GameObject point = EnsureChildObject(enemyRoot.transform, rootName + "_Patrol_" + i);
            point.transform.position = patrolPoints[i];
            points[i] = point.transform;
        }

        CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
        if (enemyStats == null) enemyStats = enemy.AddComponent<CharacterStats>();

        Damageable enemyDamageable = enemy.GetComponent<Damageable>();
        if (enemyDamageable == null) enemyDamageable = enemy.AddComponent<Damageable>();
        enemyDamageable.Configure(50f, false, false);

        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController == null) enemyController = enemy.AddComponent<EnemyController>();
        enemyController.SetPatrolPoints(points);

        EnsureHumanoidActorSystems(enemy, false);
    }

    private static void EnsureCheckpointZone(Transform parent, string name, Vector3 worldPosition, Vector3 size, string message)
    {
        GameObject zone = EnsureChildObject(parent, name);
        zone.transform.position = worldPosition;
        zone.transform.rotation = Quaternion.identity;

        BoxCollider colliderComponent = zone.GetComponent<BoxCollider>();
        if (colliderComponent == null)
        {
            colliderComponent = zone.AddComponent<BoxCollider>();
        }
        colliderComponent.isTrigger = true;
        colliderComponent.size = size;
        colliderComponent.center = Vector3.zero;

        CheckpointZone checkpointZone = zone.GetComponent<CheckpointZone>();
        if (checkpointZone == null)
        {
            checkpointZone = zone.AddComponent<CheckpointZone>();
        }
        checkpointZone.Configure(message, true);
    }

    private InfectionNode EnsureNode(Transform parent, string name, string nodeLabel)
    {
        GameObject nodeObject = EnsureChildObject(parent, name);
        InfectionNode node = nodeObject.GetComponent<InfectionNode>();
        if (node == null) node = nodeObject.AddComponent<InfectionNode>();
        nodeObject.name = name;
        return node;
    }

    private static Volume EnsureVolume(Transform transformTarget, bool isGlobal, Vector3 localSize, Color colorFilter, float postExposure, float bloom, float vignette)
    {
        Volume volume = transformTarget.GetComponent<Volume>();
        if (volume == null) volume = transformTarget.gameObject.AddComponent<Volume>();
        volume.isGlobal = isGlobal;
        volume.priority = 5f;
        volume.blendDistance = isGlobal ? 0f : 1.5f;
        volume.weight = isGlobal ? 0.35f : 0f;

        if (volume.profile == null || !volume.profile.name.StartsWith("VS_Profile_", StringComparison.Ordinal))
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "VS_Profile_" + transformTarget.name;
            ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.active = true;
            colorAdjustments.colorFilter.overrideState = true;
            colorAdjustments.colorFilter.value = colorFilter;
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = postExposure;
            Bloom bloomEffect = profile.Add<Bloom>(true);
            bloomEffect.active = true;
            bloomEffect.intensity.overrideState = true;
            bloomEffect.intensity.value = bloom;
            Vignette vignetteEffect = profile.Add<Vignette>(true);
            vignetteEffect.active = true;
            vignetteEffect.intensity.overrideState = true;
            vignetteEffect.intensity.value = vignette;
            volume.profile = profile;
        }

        if (!isGlobal)
        {
            BoxCollider colliderComponent = transformTarget.GetComponent<BoxCollider>();
            if (colliderComponent == null) colliderComponent = transformTarget.gameObject.AddComponent<BoxCollider>();
            colliderComponent.isTrigger = true;
            colliderComponent.size = localSize;
        }

        return volume;
    }

    private Vector3 ResolveTemplateOrigin(string rootName, Vector3 fallback)
    {
        GameObject root = FindSceneObject(rootName);
        return root != null ? root.transform.position : fallback;
    }

    private static T[] FilterNull<T>(params T[] values) where T : UnityEngine.Object
    {
        List<T> result = new List<T>();
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != null) result.Add(values[i]);
        }
        return result.ToArray();
    }

#if UNITY_EDITOR
    private static void RemoveMissingScriptsInActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        int removedCount = 0;
        for (int i = 0; i < roots.Length; i++)
        {
            removedCount += RemoveMissingScriptsRecursive(roots[i].transform);
        }

        if (removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} missing script component(s) from scene '{scene.name}'.");
        }
    }

    private static int RemoveMissingScriptsRecursive(Transform root)
    {
        if (root == null)
        {
            return 0;
        }

        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root.gameObject);
        for (int i = 0; i < root.childCount; i++)
        {
            removed += RemoveMissingScriptsRecursive(root.GetChild(i));
        }

        return removed;
    }

    private static ItemDefinition FindItemAssetById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition", new[] { "Assets/Data/Items" });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item != null && string.Equals(item.itemId, itemId, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return null;
    }
#endif

    private static GameObject EnsureRootObject(string name)
    {
        GameObject existing = FindRootObject(name);
        return existing ?? new GameObject(name);
    }

    private static GameObject EnsureChildObject(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        GameObject created = new GameObject(name);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static GameObject EnsurePrimitiveObject(PrimitiveType type, string name, Transform parent, Vector3 worldPosition, Vector3 scale)
    {
        GameObject existing = FindSceneObject(name);
        GameObject primitive = existing ?? GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent, true);
        primitive.transform.position = worldPosition;
        primitive.transform.localScale = scale;
        return primitive;
    }

    private static void ReparentByName(string objectName, Transform parent)
    {
        GameObject sceneObject = FindSceneObject(objectName);
        if (sceneObject != null && sceneObject.transform.parent != parent)
        {
            sceneObject.transform.SetParent(parent, true);
        }
    }

    private static GameObject FindRootObject(string name)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == name) return roots[i];
        }
        return null;
    }

    private static GameObject FindSceneObject(string name)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindRecursive(roots[i].transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindRecursive(Transform parent, string target)
    {
        if (parent.name == target) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindRecursive(parent.GetChild(i), target);
            if (found != null) return found;
        }
        return null;
    }
}
