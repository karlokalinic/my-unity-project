using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Runtime-only setup for SampleScene so it can serve as a focused player/combat testbed.
/// </summary>
[DisallowMultipleComponent]
public class SampleSceneTestingBootstrap : MonoBehaviour
{
    private enum TextureProfile
    {
        None,
        Vitruvian,
        Antonia,
        MbMale
    }

    private const string SampleSceneName = "SampleScene";
    private const string AuthoringConfigObjectName = "SampleSceneAuthoringConfig";
    private const string DefaultGroundObjectName = "InteractableGround";
    private const string DefaultInteriorRootName = "SampleInteriorShell";
    private const string FriendlyNpcName = "FriendlyNpc_Dialogue";
    private const string HostileNpcName = "HostileNpc_Enemy";
    private const string CombatUiName = "CombatReticleUI";
    private const string DialogueUiName = "DialoguePanelUI";
    private const string InventoryUiName = "InventoryPanelUI";
    private const string LegacyPlayerModelPath = "Assets/Game/Art/Characters/Source/Ch01_nonPBR@Double Dagger Stab.fbx";
    private const string ExportedPlayerModelPath = "Assets/Prefabs/Characters/CHAR_PrisonerA.prefab";
    private const string ImportedPlayerModelPath = "Assets/Scenes/PrisonerAPrefab.prefab";
    private const string FriendlyModelPath = "Assets/Game/Art/Characters/Source/Ch02_nonPBR@Double Dagger Stab.fbx";
    private const string EnemyModelPath = "Assets/Game/Art/Characters/Source/Ch11_nonPBR@Double Dagger Stab.fbx";
    private const string GeneratedInteriorPropsModelPath = "Assets/Game/Art/Environment/Generated/InteriorProps_Custom.fbx";
    private const string KitRootPath = "Assets/Game/Art/Environment/Kits";
    private const string KitSciFiDeskPath = KitRootPath + "/SciFiEssentials/FBX/Prop_Desk_Medium.fbx";
    private const string KitSciFiLockerPath = KitRootPath + "/SciFiEssentials/FBX/Prop_Locker.fbx";
    private const string KitSciFiCratePath = KitRootPath + "/SciFiEssentials/FBX/Prop_Crate_Large.fbx";
    private const string KitSciFiShelfPath = KitRootPath + "/SciFiEssentials/FBX/Prop_Shelves_WideTall.fbx";
    private const string KitSciFiChairPath = KitRootPath + "/SciFiEssentials/FBX/Prop_Chair.fbx";
    private const string KitSciFiBarrelPath = KitRootPath + "/SciFiEssentials/FBX/Prop_Barrel1.fbx";
    private const string KitSciFiChestPath = KitRootPath + "/SciFiEssentials/FBX/Prop_Chest.fbx";
    private const string KitSciFiGunPistolPath = KitRootPath + "/SciFiEssentials/FBX/Gun_Pistol.fbx";
    private const string KitSciFiGunRiflePath = KitRootPath + "/SciFiEssentials/FBX/Gun_Rifle.fbx";
    private const string KitSciFiHealthPath = KitRootPath + "/SciFiEssentials/FBX/Prop_HealthPack.fbx";
    private const string KitSciFiKeyCardPath = KitRootPath + "/SciFiEssentials/FBX/Prop_KeyCard.fbx";
    private const string KitWallDarkPath = KitRootPath + "/ModularSciFi/FBX/Walls/ShortWall_DarkMetal2_Straight.fbx";
    private const string KitWallPlatePath = KitRootPath + "/ModularSciFi/FBX/Walls/ShortWall_MetalPlates_Straight.fbx";
    private const string KitTopCablesPath = KitRootPath + "/ModularSciFi/FBX/Walls/TopCables_Straight_Hanging.fbx";
    private const string KitPropComputerPath = KitRootPath + "/ModularSciFi/FBX/Props/Prop_Computer.fbx";
    private const string KitPropVentPath = KitRootPath + "/ModularSciFi/FBX/Props/Prop_Vent_Big.fbx";
    private const string KitPropLightPath = KitRootPath + "/ModularSciFi/FBX/Props/Prop_Light_Wide.fbx";
    private const string KitQuarantineRoomPath = KitRootPath + "/Quarantine/Reom.fbx";
    private const string TextureRootPath = "Assets/Game/Art/Characters/Textures/CharMorphDb";
    private const string LocomotionControllerResourcePath = "Animators/SampleSceneLocomotion";
    private const string PrisonerAvatarPath = "Assets/Avatar/Prisoner_AAvatar.asset";

    private const string AntoniaUnderwearPath = "Assets/Game/Art/Characters/CharMorph/2K/Antonia/Antonia_2K.fbm/underwear.png";
    private const string AntoniaArmsAlbedoPath = TextureRootPath + "/Antonia/arms_albedo.png";
    private const string AntoniaBodyAlbedoPath = TextureRootPath + "/Antonia/body_albedo.png";
    private const string AntoniaHeadAlbedoPath = TextureRootPath + "/Antonia/head_albedo.png";
    private const string AntoniaLegsAlbedoPath = TextureRootPath + "/Antonia/legs_albedo.png";
    private const string AntoniaMouthPath = TextureRootPath + "/Antonia/mouth.png";
    private const string AntoniaEyesPath = TextureRootPath + "/Antonia/eyes.png";
    private const string MbMaleAlbedoPath = TextureRootPath + "/MbMale/albedo.png";
    private const string MbMaleLipPath = TextureRootPath + "/MbMale/lipmap.png";
    private const string CommonIrisColorPath = TextureRootPath + "/Common/iris_color.png";

    private static readonly Vector3 DefaultPlayerStartPosition = new Vector3(-7f, 1.2f, 0f);
    private static readonly Quaternion DefaultPlayerStartRotation = Quaternion.identity;
    private static readonly Vector3 DefaultFriendlyStartPosition = new Vector3(-4.6f, 1.2f, 1.8f);
    private static readonly Vector3 DefaultEnemyStartPosition = new Vector3(12.2f, 4.5f, 0.4f);
    private static readonly Vector3 DefaultGroundPosition = new Vector3(0f, -0.01f, 0f);
    private static readonly Vector3 DefaultGroundScale = new Vector3(34f, 0.2f, 22f);

    private static string groundObjectName = DefaultGroundObjectName;
    private static string interiorRootName = DefaultInteriorRootName;
    private static Vector3 playerStartPosition = DefaultPlayerStartPosition;
    private static Quaternion playerStartRotation = DefaultPlayerStartRotation;
    private static Vector3 friendlyStartPosition = DefaultFriendlyStartPosition;
    private static Vector3 enemyStartPosition = DefaultEnemyStartPosition;
    private static Vector3 groundPosition = DefaultGroundPosition;
    private static Vector3 groundScale = DefaultGroundScale;
    private static Vector3 interiorOrigin = Vector3.zero;
    private static float roomHalfX = 10.7f;
    private static float roomHalfZ = 6.7f;
    private static float wallHeight = 3.4f;
    private static float wallThickness = 0.22f;
    private static float ceilingThickness = 0.14f;
    private static float frontDoorWidth = 2.6f;
    private static float frontDoorHeight = 2.35f;
    private static float floorFinishYOffset = 0.01f;
    private static float actorGroundYOffset = 0.005f;
    private static float visualGroundOffset = 0.008f;

    [SerializeField] private SampleSceneAuthoringConfig authoringConfig;
    private static readonly Dictionary<string, Material> fallbackMaterialCache = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Texture2D> fallbackTextureCache = new Dictionary<string, Texture2D>();
    private static readonly Dictionary<string, Texture2D> importedTextureCache = new Dictionary<string, Texture2D>();
    private static bool locomotionControllerEnsured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallAfterSceneLoad()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!IsSampleScene(activeScene))
        {
            return;
        }

        if (FindInActiveScene<SampleSceneTestingBootstrap>() != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(SampleSceneTestingBootstrap));
        host.hideFlags = HideFlags.DontSave;
        host.AddComponent<SampleSceneTestingBootstrap>();
    }

    private static bool IsSampleScene(Scene scene)
    {
        return scene.IsValid() &&
               string.Equals(scene.name, SampleSceneName, StringComparison.OrdinalIgnoreCase);
    }

    private void Start()
    {
        ApplySampleSceneUpgradeFromAuthoringObject();
        StartCoroutine(ReassertSpawnLayoutDeferred());
    }

    private void ApplySampleSceneUpgradeFromAuthoringObject()
    {
        RefreshRuntimeValuesFromAuthoringConfig();
        ApplySampleSceneUpgrade();
    }

    private IEnumerator ReassertSpawnLayoutDeferred()
    {
        yield return null;

        PlayerMover player = FindInActiveScene<PlayerMover>();
        if (player == null)
        {
            yield break;
        }

        ReassertSampleSpawnLayout(player, includeEnemy: true, relocateActors: true);

        for (int i = 0; i < 36; i++)
        {
            yield return null;
            if (InputReader.MovementIntentPressed())
            {
                break;
            }

            ReassertSampleSpawnLayout(player, includeEnemy: false, relocateActors: false);
        }
    }

    private void RefreshRuntimeValuesFromAuthoringConfig()
    {
        ResetRuntimeValuesToDefaults();
        authoringConfig = ResolveAuthoringConfig();
        if (authoringConfig == null)
        {
            return;
        }

        playerStartPosition = authoringConfig.PlayerSpawnPosition;
        playerStartRotation = authoringConfig.PlayerSpawnRotation;
        friendlyStartPosition = authoringConfig.FriendlyNpcSpawnPosition;
        enemyStartPosition = authoringConfig.HostileNpcSpawnPosition;

        groundObjectName = authoringConfig.GroundObjectName;
        groundPosition = authoringConfig.GroundPosition;
        groundScale = authoringConfig.GroundScale;

        interiorRootName = authoringConfig.InteriorRootName;
        interiorOrigin = authoringConfig.InteriorOrigin;
        roomHalfX = authoringConfig.RoomHalfX;
        roomHalfZ = authoringConfig.RoomHalfZ;
        wallHeight = authoringConfig.WallHeight;
        wallThickness = authoringConfig.WallThickness;
        ceilingThickness = authoringConfig.CeilingThickness;
        frontDoorWidth = authoringConfig.FrontDoorWidth;
        frontDoorHeight = authoringConfig.FrontDoorHeight;
        floorFinishYOffset = authoringConfig.FloorFinishYOffset;
        actorGroundYOffset = authoringConfig.ActorGroundYOffset;
        visualGroundOffset = authoringConfig.VisualGroundOffset;
    }

    private static void ResetRuntimeValuesToDefaults()
    {
        groundObjectName = DefaultGroundObjectName;
        interiorRootName = DefaultInteriorRootName;
        playerStartPosition = DefaultPlayerStartPosition;
        playerStartRotation = DefaultPlayerStartRotation;
        friendlyStartPosition = DefaultFriendlyStartPosition;
        enemyStartPosition = DefaultEnemyStartPosition;
        groundPosition = DefaultGroundPosition;
        groundScale = DefaultGroundScale;
        interiorOrigin = Vector3.zero;
        roomHalfX = 10.7f;
        roomHalfZ = 6.7f;
        wallHeight = 3.4f;
        wallThickness = 0.22f;
        ceilingThickness = 0.14f;
        frontDoorWidth = 2.6f;
        frontDoorHeight = 2.35f;
        floorFinishYOffset = 0.01f;
        actorGroundYOffset = 0.005f;
        visualGroundOffset = 0.008f;
    }

    private SampleSceneAuthoringConfig ResolveAuthoringConfig()
    {
        if (authoringConfig != null)
        {
            return authoringConfig;
        }

        authoringConfig = FindInActiveScene<SampleSceneAuthoringConfig>();
        if (authoringConfig != null)
        {
            return authoringConfig;
        }

        GameObject configRoot = FindGameObjectInActiveScene(AuthoringConfigObjectName, includeInactive: true);
        if (configRoot == null)
        {
            configRoot = new GameObject(AuthoringConfigObjectName);
        }

        authoringConfig = EnsureComponent<SampleSceneAuthoringConfig>(configRoot);
        return authoringConfig;
    }

    private void ApplySampleSceneUpgrade()
    {
        ResetRuntimeMaterialCaches();

        PlayerMover player = FindInActiveScene<PlayerMover>();
        if (player == null)
        {
            GameObject playerObject = FindGameObjectInActiveScene("Player");
            if (playerObject != null)
            {
                player = EnsureComponent<PlayerMover>(playerObject);
            }
        }

        if (player == null)
        {
            Debug.LogWarning("SampleSceneTestingBootstrap: PlayerMover not found in SampleScene.");
            return;
        }

        Camera mainCamera = FindInActiveScene<Camera>();
        HolstinCameraRig cameraRig = FindInActiveScene<HolstinCameraRig>();
        InspectItemViewer inspectViewer = FindInActiveScene<InspectItemViewer>();
        InteractionPromptUI promptUi = FindInActiveScene<InteractionPromptUI>();
        DialoguePanelUI dialoguePanel = EnsureDialoguePanelUi();
        EnsureGroundBaseline();
        EnsureInteriorLayout();

        SetPlayerToStartPose(player);
        cameraRig = EnsureCameraRig(player, mainCamera, cameraRig, inspectViewer);
        ForceSnapPlayerToGround(player);
        InputReader.ResetContextStack(InputReader.InputContext.Gameplay);

        ConfigureSceneContext(cameraRig, promptUi, inspectViewer, player, dialoguePanel);
        ConfigurePlayerSystems(player, mainCamera, cameraRig, inspectViewer, promptUi);
        EnsureFriendlyNpc(player);
        EnsureHostileNpc(player);
        ReassertSampleSpawnLayout(player, includeEnemy: true, relocateActors: true);
        EnsureCombatReticleUi();
        EnsureInventoryPanelUi(player);
        EnsureRuntimeGraphicsQualityController();
        HideLegacySampleTextArtifact();
    }

    private static void ConfigureSceneContext(
        HolstinCameraRig cameraRig,
        InteractionPromptUI promptUi,
        InspectItemViewer inspectViewer,
        PlayerMover player,
        DialoguePanelUI dialoguePanel)
    {
        HolstinSceneContext sceneContext = FindInActiveScene<HolstinSceneContext>();
        if (sceneContext == null)
        {
            GameObject contextObject = new GameObject("HolstinSceneContext");
            sceneContext = contextObject.AddComponent<HolstinSceneContext>();
        }

        sceneContext.Configure(cameraRig, promptUi, inspectViewer, player, dialoguePanel);
        sceneContext.ResolveMissingReferences();
    }

    private static void ConfigurePlayerSystems(
        PlayerMover player,
        Camera mainCamera,
        HolstinCameraRig cameraRig,
        InspectItemViewer inspectViewer,
        InteractionPromptUI promptUi)
    {
        if (player == null)
        {
            return;
        }

        GameObject playerObject = player.gameObject;
        StripPrimitiveVisualComponents(playerObject);
        RemoveComponent<Canvas>(playerObject);
        RemoveComponent<CanvasScaler>(playerObject);
        RemoveComponent<GraphicRaycaster>(playerObject);

        InventorySystem inventory = EnsureComponent<InventorySystem>(playerObject);
        PlayerInventory legacyInventoryAdapter = EnsureComponent<PlayerInventory>(playerObject);
        _ = legacyInventoryAdapter;
        StarterLoadout starterLoadout = EnsureComponent<StarterLoadout>(playerObject);
        starterLoadout.ConfigureGrantBehavior(shouldGrantOnStart: true, onlyIfEmpty: false);
        starterLoadout.ConfigureByItemIds(
            weaponItemId: "wpn_pistol",
            itemIds: Array.Empty<string>(),
            ammo: new[] { ("ammo_pistol", 36) });
        starterLoadout.TryGrantLoadout();
        EnsureComponent<CharacterStats>(playerObject);
        EnsureComponent<Damageable>(playerObject);
        EnsureComponent<SkillSystem>(playerObject);
        EnsureComponent<ReputationSystem>(playerObject);
        EnsureComponent<CurrencyWallet>(playerObject);
        EnsureComponent<RealTimeCombat>(playerObject);
        CheckpointLiteManager checkpoint = EnsureComponent<CheckpointLiteManager>(playerObject);
        checkpoint.ConfigureFallAutoRespawn(false);

        RemoveComponent<ProceduralHumanoidRig>(playerObject);
        RemoveComponent<ActiveRagdollMotor>(playerObject);
        RemoveComponent<DeathRagdollController>(playerObject);
        RemoveComponent<PlayerAnimationController>(playerObject);
        RemoveComponent<HumanoidIdlePoseController>(playerObject);
        RemoveLegacyRagdollHierarchy(playerObject);
        Transform headAnchor = player.transform.Find("HeadAnchor");

        if (cameraRig != null && mainCamera != null)
        {
            headAnchor = EnsureChild(player.transform, "HeadAnchor");
            headAnchor.localPosition = new Vector3(0f, 1.68f, 0.03f);
            headAnchor.localRotation = Quaternion.identity;

            cameraRig.Configure(player.transform, headAnchor, mainCamera.transform, mainCamera);
            player.ConfigureRuntimeReferences(cameraRig, inspectViewer);
            player.SetCameraForwardSource(mainCamera.transform);
        }

        if (inspectViewer != null && mainCamera != null)
        {
            Transform inspectPreviewRoot = EnsureChild(mainCamera.transform, "InspectPreviewRoot");
            inspectViewer.SetPreviewRoot(inspectPreviewRoot);
        }

        PlayerInteraction interaction = EnsureComponent<PlayerInteraction>(playerObject);
        if (interaction != null)
        {
            if (mainCamera != null && cameraRig != null && inspectViewer != null)
            {
                interaction.Configure(mainCamera, cameraRig, inspectViewer);
            }

            Transform pickupAnchor = mainCamera != null ? EnsureChild(mainCamera.transform, "PickupAnchor") : null;
            if (pickupAnchor != null)
            {
                pickupAnchor.localPosition = new Vector3(0.2f, -0.15f, 0.62f);
                pickupAnchor.localRotation = Quaternion.identity;
            }

            interaction.ConfigureRuntimeReferences(inventory, promptUi, pickupAnchor);
            interaction.ConfigureInteractionSettings(radius: 3.25f, facingDot: -1f, interactMaxDistance: 3.4f);
        }

        PlayerRespawnScreenUI respawnUi = EnsureComponent<PlayerRespawnScreenUI>(playerObject);
        respawnUi.Configure(checkpoint, player, interaction, -10f);

        EnsureActorModel(playerObject, "Visual_Player_Ch01", ResolvePlayerModelPath(), 1.78f);

        if (mainCamera != null)
        {
            SampleSceneFirstPersonPresentation firstPersonPresentation = EnsureComponent<SampleSceneFirstPersonPresentation>(mainCamera.gameObject);
            firstPersonPresentation.Configure(cameraRig, player, EnsureComponent<RealTimeCombat>(playerObject));
        }

        if (cameraRig != null)
        {
            IsometricFrontWallHider wallHider = EnsureComponent<IsometricFrontWallHider>(cameraRig.gameObject);
            Transform interiorRoot = FindGameObjectInActiveScene(interiorRootName)?.transform;
            wallHider.Configure(cameraRig, player.transform, mainCamera, interiorRoot);
        }

        PlayerFlashlightController flashlight = EnsureComponent<PlayerFlashlightController>(playerObject);
        Transform flashlightAnchor = headAnchor != null ? headAnchor : player.transform;
        flashlight.Configure(flashlightAnchor, cameraRig, mainCamera != null ? mainCamera.transform : null);

        EnsureComponent<PauseSettingsMenuUI>(playerObject);
    }

    private static void EnsureFriendlyNpc(PlayerMover player)
    {
        if (player == null)
        {
            return;
        }

        GameObject npc = EnsureCapsuleActor(
            FriendlyNpcName,
            friendlyStartPosition,
            new Vector3(0.82f, 1.05f, 0.82f),
            snapToGround: true);

        NpcIdentity identity = EnsureComponent<NpcIdentity>(npc);
        identity.Configure("Marta", "Friendly Interactable NPC", player.transform, "boarding_house");

        EnsureComponent<CharacterStats>(npc);
        EnsureComponent<Damageable>(npc);
        PrepareNpcPhysicsActor(npc, lockHorizontalPosition: true);

        NPCKeyGiverInteractable keyGiver = EnsureComponent<NPCKeyGiverInteractable>(npc);
        keyGiver.ConfigureReward("service_key", "Service Key", true, "sample_scene_friendly_reward");
        keyGiver.ConfigureInteractionProfile(3.4f, false);

        EnsureActorModel(npc, "Visual_FriendlyNpc_Ch02", FriendlyModelPath, 1.72f);
    }

    private static void EnsureHostileNpc(PlayerMover player)
    {
        if (player == null)
        {
            return;
        }

        GameObject enemy = EnsureCapsuleActor(
            HostileNpcName,
            enemyStartPosition,
            new Vector3(0.9f, 1.05f, 0.9f),
            snapToGround: true);

        EnsureComponent<CharacterStats>(enemy);
        Damageable damageable = EnsureComponent<Damageable>(enemy);
        damageable.Configure(75f, false, false);

        Transform patrolRoot = EnsureChild(enemy.transform, "Patrol");
        Transform patrolA = EnsureChild(patrolRoot, "PointA");
        Transform patrolB = EnsureChild(patrolRoot, "PointB");
        patrolA.position = enemy.transform.position + (enemy.transform.right * 2f);
        patrolB.position = enemy.transform.position - (enemy.transform.right * 2f);

        // Let hostile actor obey gravity naturally while still spawning on platform ground.
        PrepareNpcPhysicsActor(enemy, lockHorizontalPosition: false);
        EnsureComponent<FallDespawnOnOutOfBounds>(enemy).Configure(-12f, disableObject: true);

        EnsureActorModel(enemy, "Visual_Enemy_Ch11", EnemyModelPath, 1.74f);
    }

    private static void EnsureCombatReticleUi()
    {
        if (FindInActiveScene<CombatReticleUI>() != null)
        {
            return;
        }

        GameObject hud = new GameObject(CombatUiName);
        hud.AddComponent<CombatReticleUI>();
    }

    private static DialoguePanelUI EnsureDialoguePanelUi()
    {
        DialoguePanelUI panel = FindInActiveScene<DialoguePanelUI>();
        if (panel != null)
        {
            return panel;
        }

        GameObject panelObject = new GameObject(DialogueUiName);
        panel = panelObject.AddComponent<DialoguePanelUI>();
        return panel;
    }

    private static void EnsureInventoryPanelUi(PlayerMover player)
    {
        if (FindInActiveScene<InventoryPanelUI>() != null)
        {
            return;
        }

        GameObject panelObject = new GameObject(InventoryUiName);
        panelObject.AddComponent<InventoryPanelUI>();
        _ = player;
    }

    private static HolstinCameraRig EnsureCameraRig(
        PlayerMover player,
        Camera mainCamera,
        HolstinCameraRig existingRig,
        InspectItemViewer inspectViewer)
    {
        HolstinCameraRig rig = existingRig;
        if (rig == null)
        {
            GameObject rigObject = new GameObject("HolstinCameraRig");
            rig = rigObject.AddComponent<HolstinCameraRig>();
            if (inspectViewer == null)
            {
                inspectViewer = rigObject.AddComponent<InspectItemViewer>();
            }
        }

        if (mainCamera != null)
        {
            if (mainCamera.transform.parent != rig.transform)
            {
                mainCamera.transform.SetParent(rig.transform, true);
            }

            Transform headAnchor = EnsureChild(player.transform, "HeadAnchor");
            headAnchor.localPosition = new Vector3(0f, 1.68f, 0.03f);
            headAnchor.localRotation = Quaternion.identity;

            rig.Configure(player.transform, headAnchor, mainCamera.transform, mainCamera);
            rig.ConfigureAimTransition(instant: false, enterSpeed: 12f, exitSpeed: 10f);
        }

        rig.SetIsometricYaw(45f, true);
        rig.EndDialogueShot(0.01f);
        return rig;
    }

    private static void ForceSnapPlayerToGround(PlayerMover player)
    {
        if (player == null)
        {
            return;
        }

        CharacterController controller = EnsureComponent<CharacterController>(player.gameObject);
        bool wasEnabled = controller.enabled;
        if (wasEnabled)
        {
            controller.enabled = false;
        }

        Vector3 origin = player.transform.position + Vector3.up * 8f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 30f, ~0, QueryTriggerInteraction.Ignore))
        {
            float halfHeight = controller.height * 0.5f;
            float groundedY = hit.point.y - controller.center.y + halfHeight + Mathf.Max(0.08f, controller.skinWidth);
            player.transform.position = new Vector3(player.transform.position.x, groundedY, player.transform.position.z);
            player.ResetMotion();
        }

        if (wasEnabled)
        {
            controller.enabled = true;
        }
    }

    private static void SetPlayerToStartPose(PlayerMover player)
    {
        if (player == null)
        {
            return;
        }

        ResolveSpawnLayout(out Vector3 playerSpawn, out _, out _);
        player.transform.SetPositionAndRotation(playerSpawn, playerStartRotation);
        player.ResetMotion();
    }

    private static void ConfigureInitialCheckpoint(PlayerMover player)
    {
        if (player == null)
        {
            return;
        }

        CheckpointLiteManager checkpoint = player.GetComponent<CheckpointLiteManager>();
        if (checkpoint == null)
        {
            return;
        }

        checkpoint.SetShowCheckpointMessages(false);
        checkpoint.SetCheckpoint(player.transform.position, player.transform.rotation, "sample_start_silent");
        checkpoint.SetShowCheckpointMessages(true);
    }

    private static void ReassertSampleSpawnLayout(PlayerMover player, bool includeEnemy, bool relocateActors)
    {
        if (player == null)
        {
            return;
        }

        EnsureGroundBaseline();
        ResolveSpawnLayout(out Vector3 playerSpawn, out Vector3 friendlySpawn, out Vector3 enemySpawn);

        if (relocateActors)
        {
            player.transform.SetPositionAndRotation(playerSpawn, playerStartRotation);
            ForceSnapPlayerToGround(player);
            KeepActorAboveGround(player.gameObject, 0.08f);
            AlignActorVisualRootsToGround(player.gameObject, visualGroundOffset);
            player.ResetMotion();
            ConfigureInitialCheckpoint(player);
        }
        else
        {
            KeepActorAboveGround(player.gameObject, 0.08f);
            AlignActorVisualRootsToGround(player.gameObject, visualGroundOffset);
        }

        GameObject friendly = FindGameObjectInActiveScene(FriendlyNpcName);
        if (friendly != null)
        {
            if (relocateActors)
            {
                friendly.transform.SetPositionAndRotation(friendlySpawn, Quaternion.identity);
                CapsuleCollider capsule = EnsureComponent<CapsuleCollider>(friendly);
                SnapActorToGround(friendly, capsule);
            }

            KeepActorAboveGround(friendly, 0.08f);
            AlignActorVisualRootsToGround(friendly, visualGroundOffset);

            Rigidbody body = friendly.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        GameObject enemy = FindGameObjectInActiveScene(HostileNpcName);
        if (enemy == null)
        {
            return;
        }

        CapsuleCollider enemyCapsule = EnsureComponent<CapsuleCollider>(enemy);
        if (includeEnemy && relocateActors)
        {
            enemy.transform.SetPositionAndRotation(enemySpawn, Quaternion.identity);
            SnapActorToGround(enemy, enemyCapsule);
        }

        KeepActorAboveGround(enemy, 0.08f);
        AlignActorVisualRootsToGround(enemy, visualGroundOffset);

        enemyCapsule.isTrigger = false;
        Rigidbody enemyBody = enemy.GetComponent<Rigidbody>();
        if (enemyBody != null)
        {
            enemyBody.isKinematic = false;
            enemyBody.useGravity = true;
            enemyBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            if (relocateActors)
            {
                enemyBody.linearVelocity = Vector3.zero;
                enemyBody.angularVelocity = Vector3.zero;
            }
            enemyBody.WakeUp();
        }
    }

    private static void ResolveSpawnLayout(out Vector3 playerSpawn, out Vector3 friendlySpawn, out Vector3 enemySpawn)
    {
        playerSpawn = playerStartPosition;
        friendlySpawn = friendlyStartPosition;
        enemySpawn = enemyStartPosition;

        GameObject ground = FindGameObjectInActiveScene(groundObjectName);
        if (ground == null)
        {
            return;
        }

        if (!ground.TryGetComponent(out Collider groundCollider))
        {
            return;
        }

        Bounds bounds = groundCollider.bounds;
        float topY = bounds.max.y;
        float insetX = Mathf.Max(0.9f, bounds.extents.x * 0.18f);
        float insetZ = Mathf.Max(0.9f, bounds.extents.z * 0.18f);
        float clampedEnemyX = Mathf.Clamp(enemyStartPosition.x, bounds.min.x + insetX, bounds.max.x - insetX);
        float clampedEnemyZ = Mathf.Clamp(enemyStartPosition.z, bounds.min.z + insetZ, bounds.max.z - insetZ);

        playerSpawn = new Vector3(playerStartPosition.x, topY + 0.25f, playerStartPosition.z);
        friendlySpawn = new Vector3(friendlyStartPosition.x, topY + 0.22f, friendlyStartPosition.z);
        enemySpawn = new Vector3(clampedEnemyX, topY + 0.24f, clampedEnemyZ);
    }

    private static void EnsureGroundBaseline()
    {
        GameObject ground = FindGameObjectInActiveScene(groundObjectName);
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = groundObjectName;
        }

        ground.transform.SetPositionAndRotation(groundPosition, Quaternion.identity);
        ground.transform.localScale = groundScale;
        MeshRenderer groundRenderer = ground.GetComponent<MeshRenderer>();
        if (groundRenderer != null)
        {
            groundRenderer.sharedMaterial = GetFallbackMaterial("interior_concrete", new Color(0.14f, 0.15f, 0.16f, 1f));
        }

        if (ground.TryGetComponent(out Collider collider))
        {
            collider.isTrigger = false;
        }
    }

    private static void EnsureInteriorLayout()
    {
        GameObject ground = FindGameObjectInActiveScene(groundObjectName);
        if (ground == null || !ground.TryGetComponent(out Collider groundCollider))
        {
            return;
        }

        float groundTop = groundCollider.bounds.max.y;
        GameObject root = FindGameObjectInActiveScene(interiorRootName, includeInactive: true);
        if (root == null)
        {
            root = new GameObject(interiorRootName);
        }

        root.transform.SetPositionAndRotation(interiorOrigin, Quaternion.identity);
        root.transform.localScale = Vector3.one;

        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = root.transform.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Material wallMaterial = GetFallbackMaterial("interior_wall_plaster", new Color(0.23f, 0.25f, 0.28f, 1f));
        Material floorMaterial = GetFallbackMaterial("interior_floor_wood", new Color(0.16f, 0.17f, 0.18f, 1f));
        Material trimMaterial = GetFallbackMaterial("interior_trim", new Color(0.27f, 0.28f, 0.30f, 1f));
        Material fabricMaterial = GetFallbackMaterial("interior_fabric", new Color(0.16f, 0.10f, 0.10f, 1f));
        Material metalMaterial = GetFallbackMaterial("interior_metal", new Color(0.21f, 0.23f, 0.25f, 1f));
        Material glassMaterial = GetFallbackMaterial("interior_glass", new Color(0.36f, 0.40f, 0.44f, 0.92f));
        Material accentMaterial = GetFallbackMaterial("interior_accent", new Color(0.19f, 0.20f, 0.22f, 1f));
        Material hazardMaterial = GetFallbackMaterial("interior_hazard", new Color(0.78f, 0.66f, 0.14f, 1f));
        Material bloodMaterial = GetFallbackMaterial("interior_blood", new Color(0.28f, 0.02f, 0.02f, 1f));

        float wallCenterY = groundTop + (wallHeight * 0.5f);
        float roomWidth = roomHalfX * 2f;
        float roomDepth = roomHalfZ * 2f;

        CreateInteriorBlock(root.transform, "FloorFinish", new Vector3(0f, groundTop + floorFinishYOffset, 0f), new Vector3(roomWidth, 0.02f, roomDepth), floorMaterial);
        CreateInteriorBlock(root.transform, "Ceiling", new Vector3(0f, groundTop + wallHeight + (ceilingThickness * 0.5f), 0f), new Vector3(roomWidth, ceilingThickness, roomDepth), wallMaterial);

        CreateInteriorBlock(root.transform, "Wall_Back", new Vector3(0f, wallCenterY, -roomHalfZ), new Vector3(roomWidth, wallHeight, wallThickness), wallMaterial);
        CreateInteriorBlock(root.transform, "Wall_Left", new Vector3(-roomHalfX, wallCenterY, 0f), new Vector3(wallThickness, wallHeight, roomDepth), wallMaterial);
        CreateInteriorBlock(root.transform, "Wall_Right", new Vector3(roomHalfX, wallCenterY, 0f), new Vector3(wallThickness, wallHeight, roomDepth), wallMaterial);

        float frontSpan = roomWidth;
        float doorWidth = frontDoorWidth;
        float doorHeight = frontDoorHeight;
        float sideSpan = (frontSpan - doorWidth) * 0.5f;
        float doorLintelHeight = wallHeight - doorHeight;

        CreateInteriorBlock(
            root.transform,
            "Wall_Front_L",
            new Vector3(-(doorWidth * 0.5f) - (sideSpan * 0.5f), wallCenterY, roomHalfZ),
            new Vector3(sideSpan, wallHeight, wallThickness),
            wallMaterial);
        CreateInteriorBlock(
            root.transform,
            "Wall_Front_R",
            new Vector3((doorWidth * 0.5f) + (sideSpan * 0.5f), wallCenterY, roomHalfZ),
            new Vector3(sideSpan, wallHeight, wallThickness),
            wallMaterial);
        CreateInteriorBlock(
            root.transform,
            "Wall_Front_Top",
            new Vector3(0f, groundTop + doorHeight + (doorLintelHeight * 0.5f), roomHalfZ),
            new Vector3(doorWidth, doorLintelHeight, wallThickness),
            wallMaterial);

        CreateInteriorBlock(root.transform, "Trim_FrontDoorTop", new Vector3(0f, groundTop + doorHeight + 0.04f, roomHalfZ - 0.02f), new Vector3(doorWidth + 0.22f, 0.08f, 0.08f), hazardMaterial);
        CreateInteriorBlock(root.transform, "Trim_FrontDoorLeft", new Vector3(-(doorWidth * 0.5f) - 0.03f, groundTop + (doorHeight * 0.5f), roomHalfZ - 0.02f), new Vector3(0.08f, doorHeight, 0.08f), hazardMaterial);
        CreateInteriorBlock(root.transform, "Trim_FrontDoorRight", new Vector3((doorWidth * 0.5f) + 0.03f, groundTop + (doorHeight * 0.5f), roomHalfZ - 0.02f), new Vector3(0.08f, doorHeight, 0.08f), hazardMaterial);

        CreateInteriorBlock(root.transform, "Divider_A", new Vector3(2.4f, wallCenterY, -1.7f), new Vector3(0.18f, wallHeight, 6.4f), wallMaterial);
        CreateInteriorBlock(root.transform, "Divider_B", new Vector3(5.0f, wallCenterY, -3.7f), new Vector3(5.2f, wallHeight, 0.18f), wallMaterial);
        CreateInteriorBlock(root.transform, "KitchenCounter", new Vector3(6.2f, groundTop + 0.48f, -5.6f), new Vector3(6.4f, 0.96f, 1.1f), accentMaterial);

        CreateInteriorBlock(root.transform, "Spill_Main", new Vector3(-5.8f, groundTop + 0.03f, -1.6f), new Vector3(6.6f, 0.01f, 3.2f), bloodMaterial);
        CreateInteriorBlock(root.transform, "Sofa_Base", new Vector3(-7.2f, groundTop + 0.36f, -1.6f), new Vector3(3.7f, 0.72f, 1.25f), fabricMaterial);
        CreateInteriorBlock(root.transform, "Sofa_Back", new Vector3(-7.2f, groundTop + 0.84f, -2.1f), new Vector3(3.7f, 0.68f, 0.24f), fabricMaterial);
        CreateInteriorBlock(root.transform, "Sofa_Arm_L", new Vector3(-8.95f, groundTop + 0.72f, -1.6f), new Vector3(0.28f, 0.72f, 1.2f), fabricMaterial);
        CreateInteriorBlock(root.transform, "Sofa_Arm_R", new Vector3(-5.45f, groundTop + 0.72f, -1.6f), new Vector3(0.28f, 0.72f, 1.2f), fabricMaterial);
        CreateInteriorBlock(root.transform, "CoffeeTable_Top", new Vector3(-5.6f, groundTop + 0.32f, -1.0f), new Vector3(1.8f, 0.08f, 0.9f), trimMaterial);
        CreateInteriorBlock(root.transform, "CoffeeTable_Leg1", new Vector3(-6.3f, groundTop + 0.16f, -1.3f), new Vector3(0.1f, 0.32f, 0.1f), metalMaterial);
        CreateInteriorBlock(root.transform, "CoffeeTable_Leg2", new Vector3(-4.9f, groundTop + 0.16f, -1.3f), new Vector3(0.1f, 0.32f, 0.1f), metalMaterial);
        CreateInteriorBlock(root.transform, "CoffeeTable_Leg3", new Vector3(-6.3f, groundTop + 0.16f, -0.7f), new Vector3(0.1f, 0.32f, 0.1f), metalMaterial);
        CreateInteriorBlock(root.transform, "CoffeeTable_Leg4", new Vector3(-4.9f, groundTop + 0.16f, -0.7f), new Vector3(0.1f, 0.32f, 0.1f), metalMaterial);

        CreateInteriorBlock(root.transform, "Bookcase", new Vector3(-9.5f, groundTop + 1.05f, -5.2f), new Vector3(0.82f, 2.1f, 1.6f), metalMaterial);
        CreateInteriorBlock(root.transform, "Bookcase_Shelf1", new Vector3(-9.5f, groundTop + 0.66f, -5.2f), new Vector3(0.86f, 0.05f, 1.62f), trimMaterial);
        CreateInteriorBlock(root.transform, "Bookcase_Shelf2", new Vector3(-9.5f, groundTop + 1.16f, -5.2f), new Vector3(0.86f, 0.05f, 1.62f), trimMaterial);
        CreateInteriorBlock(root.transform, "Bookcase_Shelf3", new Vector3(-9.5f, groundTop + 1.66f, -5.2f), new Vector3(0.86f, 0.05f, 1.62f), trimMaterial);

        CreateInteriorBlock(root.transform, "DiningTable_Top", new Vector3(7.2f, groundTop + 0.72f, 2.2f), new Vector3(2.1f, 0.08f, 1.3f), trimMaterial);
        CreateInteriorBlock(root.transform, "DiningTable_Leg1", new Vector3(6.3f, groundTop + 0.36f, 1.7f), new Vector3(0.12f, 0.72f, 0.12f), metalMaterial);
        CreateInteriorBlock(root.transform, "DiningTable_Leg2", new Vector3(8.1f, groundTop + 0.36f, 1.7f), new Vector3(0.12f, 0.72f, 0.12f), metalMaterial);
        CreateInteriorBlock(root.transform, "DiningTable_Leg3", new Vector3(6.3f, groundTop + 0.36f, 2.7f), new Vector3(0.12f, 0.72f, 0.12f), metalMaterial);
        CreateInteriorBlock(root.transform, "DiningTable_Leg4", new Vector3(8.1f, groundTop + 0.36f, 2.7f), new Vector3(0.12f, 0.72f, 0.12f), metalMaterial);

        // Industrial pipes and support frames.
        CreateInteriorBlock(root.transform, "Pipe_Run_A", new Vector3(0f, groundTop + wallHeight - 0.34f, -4.9f), new Vector3(18.6f, 0.16f, 0.16f), metalMaterial);
        CreateInteriorBlock(root.transform, "Pipe_Run_B", new Vector3(8.8f, groundTop + wallHeight - 0.64f, 0f), new Vector3(0.16f, 0.16f, 11.6f), metalMaterial);
        CreateInteriorBlock(root.transform, "Pipe_Run_C", new Vector3(-8.9f, groundTop + wallHeight - 0.58f, 1.6f), new Vector3(0.16f, 0.16f, 8.8f), metalMaterial);
        CreateInteriorBlock(root.transform, "SupportBeam_A", new Vector3(-2.0f, wallCenterY, 0f), new Vector3(0.18f, wallHeight, 0.18f), metalMaterial);
        CreateInteriorBlock(root.transform, "SupportBeam_B", new Vector3(2.0f, wallCenterY, 0f), new Vector3(0.18f, wallHeight, 0.18f), metalMaterial);

        // Disturbing scene dressing.
        CreateInteriorBlockRotated(root.transform, "Gurney_Frame", new Vector3(0.8f, groundTop + 0.34f, -0.7f), new Vector3(1.8f, 0.1f, 0.72f), new Vector3(0f, 0f, 15f), metalMaterial);
        CreateInteriorBlockRotated(root.transform, "Gurney_Leg1", new Vector3(0.15f, groundTop + 0.17f, -1.0f), new Vector3(0.08f, 0.34f, 0.08f), new Vector3(0f, 0f, 15f), metalMaterial);
        CreateInteriorBlockRotated(root.transform, "Gurney_Leg2", new Vector3(1.45f, groundTop + 0.17f, -1.0f), new Vector3(0.08f, 0.34f, 0.08f), new Vector3(0f, 0f, 15f), metalMaterial);
        CreateInteriorBlockRotated(root.transform, "Gurney_Leg3", new Vector3(0.15f, groundTop + 0.17f, -0.4f), new Vector3(0.08f, 0.34f, 0.08f), new Vector3(0f, 0f, 15f), metalMaterial);
        CreateInteriorBlockRotated(root.transform, "Gurney_Leg4", new Vector3(1.45f, groundTop + 0.17f, -0.4f), new Vector3(0.08f, 0.34f, 0.08f), new Vector3(0f, 0f, 15f), metalMaterial);
        CreateInteriorBlock(root.transform, "BodyBag_Proxy", new Vector3(0.85f, groundTop + 0.43f, -0.73f), new Vector3(1.44f, 0.18f, 0.48f), fabricMaterial);
        CreateInteriorBlock(root.transform, "Blood_Drip_Wall", new Vector3(2.45f, groundTop + 1.45f, -1.68f), new Vector3(0.02f, 0.9f, 1.3f), bloodMaterial);
        CreateInteriorBlock(root.transform, "Blood_Pool_Secondary", new Vector3(1.25f, groundTop + 0.02f, -0.9f), new Vector3(1.8f, 0.01f, 1.2f), bloodMaterial);

        CreateInteriorBlock(root.transform, "Window_Left", new Vector3(-4.0f, groundTop + 1.8f, -roomHalfZ + 0.06f), new Vector3(2.8f, 1.4f, 0.05f), glassMaterial);
        CreateInteriorBlock(root.transform, "Window_Right", new Vector3(4.0f, groundTop + 1.8f, -roomHalfZ + 0.06f), new Vector3(2.8f, 1.4f, 0.05f), glassMaterial);
        CreateInteriorBlock(root.transform, "WindowFrame_Left", new Vector3(-4.0f, groundTop + 1.8f, -roomHalfZ + 0.01f), new Vector3(2.94f, 1.54f, 0.06f), trimMaterial);
        CreateInteriorBlock(root.transform, "WindowFrame_Right", new Vector3(4.0f, groundTop + 1.8f, -roomHalfZ + 0.01f), new Vector3(2.94f, 1.54f, 0.06f), trimMaterial);

        CreateInteriorBlock(root.transform, "PendantRod", new Vector3(-5.6f, groundTop + wallHeight - 0.45f, -1.0f), new Vector3(0.05f, 0.8f, 0.05f), metalMaterial);
        CreateInteriorBlock(root.transform, "PendantShade", new Vector3(-5.6f, groundTop + wallHeight - 0.92f, -1.0f), new Vector3(0.5f, 0.28f, 0.5f), hazardMaterial);
        CreateInteriorBlock(root.transform, "PendantBulb", new Vector3(-5.6f, groundTop + wallHeight - 1.06f, -1.0f), new Vector3(0.12f, 0.12f, 0.12f), bloodMaterial);

#if UNITY_EDITOR
        TryPlaceGeneratedInteriorProps(root.transform, groundTop, metalMaterial);
        TryPlaceKitWorldInspiredLayout(root.transform, groundTop, metalMaterial);
#endif
        EnsureMissionFlowLayout(root.transform, groundTop, wallMaterial, trimMaterial, hazardMaterial, bloodMaterial);
        EnsureInteriorLighting(root.transform, groundTop + wallHeight - 0.2f);
        ConfigureInteriorAtmosphere();
        EnsureComponent<SampleSceneAmbientSoundscape>(root).Configure(0.16f, 0.12f);
    }

    private static void CreateInteriorBlock(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = localPosition;
        block.transform.localRotation = Quaternion.identity;
        block.transform.localScale = localScale;

        MeshRenderer renderer = block.GetComponent<MeshRenderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void CreateInteriorBlockRotated(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Vector3 localEuler, Material material)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = localPosition;
        block.transform.localRotation = Quaternion.Euler(localEuler);
        block.transform.localScale = localScale;

        MeshRenderer renderer = block.GetComponent<MeshRenderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void EnsureMissionFlowLayout(
        Transform root,
        float groundTop,
        Material wallMaterial,
        Material trimMaterial,
        Material hazardMaterial,
        Material bloodMaterial)
    {
        if (root == null)
        {
            return;
        }

        Transform missionRoot = EnsureChild(root, "MissionRouteLayout");
        for (int i = missionRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = missionRoot.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Mission flow inspired by Holstin-style staging:
        // 1) get service key from the friendly NPC area,
        // 2) unlock maintenance lane,
        // 3) collect old key,
        // 4) unlock front escape shutter.
        CreateInteriorBlock(missionRoot, "ObjectivePath_A", new Vector3(-4.8f, groundTop + 0.02f, 1.8f), new Vector3(3.6f, 0.01f, 1.1f), bloodMaterial);
        CreateInteriorBlock(missionRoot, "ObjectivePath_B", new Vector3(-1.2f, groundTop + 0.02f, 3.9f), new Vector3(4.2f, 0.01f, 0.8f), bloodMaterial);
        CreateInteriorBlock(missionRoot, "ObjectivePath_C", new Vector3(3.2f, groundTop + 0.02f, 3.9f), new Vector3(3.4f, 0.01f, 0.8f), bloodMaterial);
        CreateInteriorBlock(missionRoot, "ObjectivePath_D", new Vector3(5.8f, groundTop + 0.02f, 1.1f), new Vector3(1.2f, 0.01f, 4.6f), bloodMaterial);

        CreateInteriorBlock(missionRoot, "ServiceGate_FrameLeft", new Vector3(-0.2f, groundTop + 1.05f, 3.95f), new Vector3(0.18f, 2.1f, 0.18f), wallMaterial);
        CreateInteriorBlock(missionRoot, "ServiceGate_FrameRight", new Vector3(1.55f, groundTop + 1.05f, 3.95f), new Vector3(0.18f, 2.1f, 0.18f), wallMaterial);
        CreateInteriorBlock(missionRoot, "ServiceGate_FrameTop", new Vector3(0.675f, groundTop + 2.1f, 3.95f), new Vector3(1.94f, 0.16f, 0.2f), trimMaterial);
        GameObject serviceGate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        serviceGate.name = "ServiceGate_Door";
        serviceGate.transform.SetParent(missionRoot, false);
        serviceGate.transform.localPosition = new Vector3(0.675f, groundTop + 1.04f, 3.95f);
        serviceGate.transform.localRotation = Quaternion.identity;
        serviceGate.transform.localScale = new Vector3(1.72f, 2.08f, 0.14f);
        if (serviceGate.TryGetComponent(out MeshRenderer serviceRenderer))
        {
            serviceRenderer.sharedMaterial = hazardMaterial;
        }

        DoorInteractable serviceDoor = EnsureComponent<DoorInteractable>(serviceGate);
        serviceDoor.ConfigureLock("service_key", "Service Key", true, false, "sample_scene_service_gate_opened");
        serviceDoor.ConfigureInteractionProfile(3.1f, false);

        CreateInteriorBlock(missionRoot, "MaintenanceCage_FrameLeft", new Vector3(6.1f, groundTop + 1.08f, -0.85f), new Vector3(0.18f, 2.15f, 0.18f), wallMaterial);
        CreateInteriorBlock(missionRoot, "MaintenanceCage_FrameRight", new Vector3(6.1f, groundTop + 1.08f, -3.05f), new Vector3(0.18f, 2.15f, 0.18f), wallMaterial);
        CreateInteriorBlock(missionRoot, "MaintenanceCage_FrameTop", new Vector3(6.1f, groundTop + 2.16f, -1.95f), new Vector3(0.22f, 0.16f, 2.38f), trimMaterial);
        GameObject archiveGate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        archiveGate.name = "ArchiveGate_Door";
        archiveGate.transform.SetParent(missionRoot, false);
        archiveGate.transform.localPosition = new Vector3(6.1f, groundTop + 1.07f, -1.95f);
        archiveGate.transform.localRotation = Quaternion.identity;
        archiveGate.transform.localScale = new Vector3(0.14f, 2.1f, 2.18f);
        if (archiveGate.TryGetComponent(out MeshRenderer archiveRenderer))
        {
            archiveRenderer.sharedMaterial = hazardMaterial;
        }

        DoorInteractable archiveDoor = EnsureComponent<DoorInteractable>(archiveGate);
        archiveDoor.ConfigureLock("old_key", "Maintenance Key", true, false, "sample_scene_exit_gate_opened");
        archiveDoor.ConfigureInteractionProfile(3.1f, false);

        GameObject oldKeyPickup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        oldKeyPickup.name = "OldKey_Pickup";
        oldKeyPickup.transform.SetParent(missionRoot, false);
        oldKeyPickup.transform.localPosition = new Vector3(5.25f, groundTop + 0.9f, -4.45f);
        oldKeyPickup.transform.localRotation = Quaternion.Euler(90f, 18f, 0f);
        oldKeyPickup.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
        if (oldKeyPickup.TryGetComponent(out MeshRenderer keyRenderer))
        {
            keyRenderer.sharedMaterial = trimMaterial;
        }

        PickupInteractable oldKeyInteractable = EnsureComponent<PickupInteractable>(oldKeyPickup);
        oldKeyInteractable.ConfigureItem(
            "old_key",
            "Maintenance Key",
            "Stamped key from the maintenance archive cabinet.",
            infectionMilestone: "sample_scene_old_key_acquired",
            requiredMilestone: "sample_scene_service_gate_opened");
        oldKeyInteractable.ConfigureInteractionProfile(2.8f, false);

        EnsureSampleSliceStateObjective();
    }

    private static void EnsureSampleSliceStateObjective()
    {
        SliceState state = FindInActiveScene<SliceState>();
        if (state == null)
        {
            GameObject stateObject = new GameObject("SliceState");
            state = stateObject.AddComponent<SliceState>();
        }

        if (!state.HasMilestone("sample_scene_bootstrap_initialized"))
        {
            state.MarkMilestone("sample_scene_bootstrap_initialized");
        }

        if (state.HasMilestone("sample_scene_exit_gate_opened"))
        {
            state.SetCurrentObjective("sample_escape_route_clear");
            return;
        }

        if (!state.HasKeyItem("service_key"))
        {
            state.SetCurrentObjective("sample_get_service_key");
            return;
        }

        if (!state.HasMilestone("sample_scene_service_gate_opened"))
        {
            state.SetCurrentObjective("sample_unlock_service_gate");
            return;
        }

        if (!state.HasKeyItem("old_key"))
        {
            state.SetCurrentObjective("sample_find_old_key");
            return;
        }

        state.SetCurrentObjective("sample_unlock_archive_gate");
    }

    private static void ConfigureInteriorAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.12f, 0.14f, 0.16f, 1f);
        RenderSettings.fogDensity = 0.0032f;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.23f, 0.25f, 0.28f, 1f);
        RenderSettings.reflectionIntensity = 1f;

        Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null || camera.gameObject.scene != SceneManager.GetActiveScene())
            {
                continue;
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.065f, 0.075f, 0.085f, 1f);
            camera.nearClipPlane = Mathf.Clamp(camera.nearClipPlane, 0.05f, 0.2f);

            UniversalAdditionalCameraData additionalData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (additionalData != null)
            {
                additionalData.renderPostProcessing = true;
                additionalData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                additionalData.antialiasingQuality = AntialiasingQuality.Medium;
            }
        }

        EnsureRuntimePostProcessingVolume();
    }

    private static void EnsureInteriorLighting(Transform root, float lightY)
    {
        if (root == null)
        {
            return;
        }

        Light[] existingLights = root.GetComponentsInChildren<Light>(true);
        for (int i = 0; i < existingLights.Length; i++)
        {
            if (existingLights[i] != null)
            {
                existingLights[i].enabled = true;
            }
        }

        Light key = EnsureInteriorPointLight(
            root,
            "InteriorKeyLight",
            new Vector3(-5.6f, lightY, -1.0f),
            new Color(1f, 0.16f, 0.14f, 1f),
            7.2f,
            18f,
            flickerAmplitude: 0.18f,
            flickerSpeed: 3.4f,
            glitchChance: 0.015f);
        Light fill = EnsureInteriorPointLight(
            root,
            "InteriorFillLight",
            new Vector3(6.8f, lightY - 0.2f, 2.2f),
            new Color(0.44f, 0.58f, 0.72f, 1f),
            4.4f,
            16f,
            flickerAmplitude: 0.12f,
            flickerSpeed: 1.8f,
            glitchChance: 0.008f);
        Light accent = EnsureInteriorSpotLight(
            root,
            "InteriorAccentLight",
            new Vector3(-9.2f, lightY - 0.25f, -5.4f),
            Quaternion.Euler(58f, 56f, 0f),
            new Color(1f, 0.24f, 0.2f, 1f),
            4.6f,
            12f,
            52f,
            flickerAmplitude: 0.2f,
            flickerSpeed: 4.6f,
            glitchChance: 0.02f);
        Light doorHazard = EnsureInteriorSpotLight(
            root,
            "InteriorDoorHazard",
            new Vector3(0f, lightY - 0.05f, 6.35f),
            Quaternion.Euler(80f, 180f, 0f),
            new Color(1f, 0.14f, 0.1f, 1f),
            3.8f,
            11f,
            64f,
            flickerAmplitude: 0.14f,
            flickerSpeed: 3.8f,
            glitchChance: 0.015f);

        _ = key;
        _ = fill;
        _ = accent;
        _ = doorHazard;

        Light[] sceneLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        Light directional = null;
        for (int i = 0; i < sceneLights.Length; i++)
        {
            Light candidate = sceneLights[i];
            if (candidate != null &&
                candidate.type == LightType.Directional &&
                candidate.gameObject.scene == SceneManager.GetActiveScene())
            {
                directional = candidate;
                break;
            }
        }

        if (directional != null)
        {
            directional.intensity = 1.05f;
            directional.color = new Color(0.72f, 0.78f, 0.84f, 1f);
            directional.transform.rotation = Quaternion.Euler(18f, -27f, 0f);
        }
    }

    private static void EnsureRuntimePostProcessingVolume()
    {
        GameObject volumeObject = FindGameObjectInActiveScene("SampleScene_RuntimeVolume", includeInactive: true);
        if (volumeObject == null)
        {
            volumeObject = new GameObject("SampleScene_RuntimeVolume");
        }

        Volume volume = EnsureComponent<Volume>(volumeObject);
        volume.isGlobal = true;
        volume.priority = 30f;
        volume.weight = 1f;
        if (volume.sharedProfile == null || !string.Equals(volume.sharedProfile.name, "SampleScene_RuntimeProfile", StringComparison.Ordinal))
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "SampleScene_RuntimeProfile";

            ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = 0.45f;
            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.contrast.value = -4f;
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = -8f;

            Bloom bloom = profile.Add<Bloom>(true);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1.1f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.22f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.55f;

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.16f;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.36f;

            Tonemapping tone = profile.Add<Tonemapping>(true);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.ACES;

            volume.sharedProfile = profile;
        }
    }

#if UNITY_EDITOR
    private static void TryPlaceGeneratedInteriorProps(Transform root, float groundTop, Material fallbackMaterial)
    {
        if (root == null)
        {
            return;
        }

        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(GeneratedInteriorPropsModelPath);
        if (modelAsset == null)
        {
            return;
        }

        Transform importedRoot = EnsureChild(root, "ImportedCustomProps");
        for (int i = importedRoot.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(importedRoot.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(importedRoot.GetChild(i).gameObject);
            }
        }

        GameObject loungeProps = Instantiate(modelAsset, importedRoot);
        loungeProps.name = "InteriorProps_Lounge";
        loungeProps.transform.localPosition = new Vector3(-2.6f, groundTop + 0.01f, 3.6f);
        loungeProps.transform.localRotation = Quaternion.Euler(0f, 28f, 0f);
        loungeProps.transform.localScale = Vector3.one;
        EnsureRenderersHaveMaterials(loungeProps, fallbackMaterial);

        GameObject diningProps = Instantiate(modelAsset, importedRoot);
        diningProps.name = "InteriorProps_Dining";
        diningProps.transform.localPosition = new Vector3(3.5f, groundTop + 0.01f, -0.8f);
        diningProps.transform.localRotation = Quaternion.Euler(0f, -64f, 0f);
        diningProps.transform.localScale = Vector3.one * 0.92f;
        EnsureRenderersHaveMaterials(diningProps, fallbackMaterial);
    }

    private static void TryPlaceKitWorldInspiredLayout(Transform root, float groundTop, Material fallbackMaterial)
    {
        if (root == null)
        {
            return;
        }

        Transform kitRoot = EnsureChild(root, "ImportedKitLayout");
        for (int i = kitRoot.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(kitRoot.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(kitRoot.GetChild(i).gameObject);
            }
        }

        TryInstantiateKitAsset(kitRoot, KitWallDarkPath, "KitWall_CorridorA", new Vector3(-1.8f, groundTop + 0.01f, 4.6f), Vector3.zero, Vector3.one * 2.35f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitWallPlatePath, "KitWall_CorridorB", new Vector3(0.95f, groundTop + 0.01f, 4.6f), Vector3.zero, Vector3.one * 2.35f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitWallDarkPath, "KitWall_CorridorC", new Vector3(3.75f, groundTop + 0.01f, 4.6f), Vector3.zero, Vector3.one * 2.35f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitTopCablesPath, "KitCeiling_CablesA", new Vector3(-0.35f, groundTop + 2.88f, 4.6f), Vector3.zero, Vector3.one * 2.5f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitTopCablesPath, "KitCeiling_CablesB", new Vector3(4.8f, groundTop + 2.88f, 0.2f), new Vector3(0f, 90f, 0f), Vector3.one * 2.2f, fallbackMaterial);

        TryInstantiateKitAsset(kitRoot, KitSciFiDeskPath, "KitDesk_Main", new Vector3(5.1f, groundTop + 0.01f, -4.9f), new Vector3(0f, 180f, 0f), Vector3.one * 1.02f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitPropComputerPath, "KitComputer_Main", new Vector3(5.28f, groundTop + 0.93f, -4.7f), new Vector3(0f, 180f, 0f), Vector3.one * 0.95f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiLockerPath, "KitLocker_BackA", new Vector3(-9.1f, groundTop + 0.01f, -4.2f), new Vector3(0f, 90f, 0f), Vector3.one * 1.08f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiLockerPath, "KitLocker_BackB", new Vector3(-9.1f, groundTop + 0.01f, -2.4f), new Vector3(0f, 90f, 0f), Vector3.one * 1.08f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiShelfPath, "KitShelf_Evidence", new Vector3(7.8f, groundTop + 0.01f, -5.0f), new Vector3(0f, -90f, 0f), Vector3.one * 1.04f, fallbackMaterial);

        TryInstantiateKitAsset(kitRoot, KitSciFiCratePath, "KitCrate_A", new Vector3(1.7f, groundTop + 0.01f, 3.5f), new Vector3(0f, 16f, 0f), Vector3.one * 1.02f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiCratePath, "KitCrate_B", new Vector3(2.5f, groundTop + 0.01f, 2.9f), new Vector3(0f, -14f, 0f), Vector3.one * 0.92f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiBarrelPath, "KitBarrel_BarricadeA", new Vector3(-1.4f, groundTop + 0.02f, 6.7f), Vector3.zero, Vector3.one * 1.12f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiBarrelPath, "KitBarrel_BarricadeB", new Vector3(-0.7f, groundTop + 0.02f, 7.1f), new Vector3(0f, 22f, 0f), Vector3.one * 1.06f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiChestPath, "KitChest_FrontExit", new Vector3(0.95f, groundTop + 0.02f, 6.9f), new Vector3(0f, 178f, 0f), Vector3.one * 1.18f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiChairPath, "KitChair_Inverted", new Vector3(4.9f, groundTop + 0.03f, -3.9f), new Vector3(-88f, 36f, 14f), Vector3.one * 1.15f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitPropVentPath, "KitVent_Open", new Vector3(-6.7f, groundTop + 0.02f, 5.5f), new Vector3(0f, 180f, 0f), Vector3.one * 1.18f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitPropLightPath, "KitLight_Floor", new Vector3(-3.2f, groundTop + 0.05f, -3.2f), new Vector3(0f, 32f, 0f), Vector3.one * 1.14f, fallbackMaterial);

        TryInstantiateKitAsset(kitRoot, KitSciFiHealthPath, "KitHealthPack", new Vector3(5.45f, groundTop + 0.96f, -4.55f), new Vector3(0f, 24f, 0f), Vector3.one * 1.28f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiKeyCardPath, "KitKeyCard", new Vector3(5.63f, groundTop + 0.95f, -4.44f), new Vector3(0f, -12f, 0f), Vector3.one * 1.55f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiGunPistolPath, "KitWeapon_Pistol", new Vector3(5.34f, groundTop + 0.95f, -4.78f), new Vector3(0f, 172f, 0f), Vector3.one * 1.38f, fallbackMaterial);
        TryInstantiateKitAsset(kitRoot, KitSciFiGunRiflePath, "KitWeapon_Rifle", new Vector3(7.85f, groundTop + 1.14f, -4.78f), new Vector3(0f, -86f, 0f), Vector3.one * 1.28f, fallbackMaterial);

        // Unique geometry beat so the space is not just a direct pack copy.
        TryInstantiateKitAsset(kitRoot, KitQuarantineRoomPath, "KitQuarantineWing", new Vector3(10.6f, groundTop + 0.02f, -0.3f), new Vector3(0f, -90f, 0f), Vector3.one * 1.65f, fallbackMaterial);
    }

    private static void TryInstantiateKitAsset(
        Transform parent,
        string assetPath,
        string instanceName,
        Vector3 localPosition,
        Vector3 localEuler,
        Vector3 localScale,
        Material fallbackMaterial)
    {
        if (parent == null || string.IsNullOrWhiteSpace(assetPath))
        {
            return;
        }

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            return;
        }

        GameObject instance = Instantiate(asset, parent);
        instance.name = instanceName;
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(localEuler);
        instance.transform.localScale = localScale;
        EnsureRenderersHaveMaterials(instance, fallbackMaterial);
        EnsureStaticCollision(instance);
        SetLayerRecursive(instance.transform, parent.gameObject.layer);
    }
#endif

    private static void EnsureRenderersHaveMaterials(GameObject root, Material fallbackMaterial)
    {
        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                if (fallbackMaterial != null)
                {
                    renderer.sharedMaterial = fallbackMaterial;
                }

                continue;
            }

            bool changed = false;
            for (int m = 0; m < materials.Length; m++)
            {
                if (materials[m] == null && fallbackMaterial != null)
                {
                    materials[m] = fallbackMaterial;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
            }
        }
    }

    private static void EnsureStaticCollision(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Collider[] existing = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < existing.Length; i++)
        {
            Collider collider = existing[i];
            if (collider != null && collider.enabled && !collider.isTrigger)
            {
                return;
            }
        }

        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        int addedMeshColliders = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter filter = meshFilters[i];
            if (filter == null || filter.sharedMesh == null)
            {
                continue;
            }

            if (filter.TryGetComponent(out Collider existingCollider) && existingCollider != null)
            {
                existingCollider.isTrigger = false;
                existingCollider.enabled = true;
                continue;
            }

            MeshCollider meshCollider = filter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
            meshCollider.convex = false;
            meshCollider.isTrigger = false;
            addedMeshColliders++;
        }

        if (addedMeshColliders > 0)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }
        }

        BoxCollider fallbackCollider = EnsureComponent<BoxCollider>(root);
        fallbackCollider.isTrigger = false;
        fallbackCollider.center = root.transform.InverseTransformPoint(combinedBounds.center);

        Vector3 worldSize = combinedBounds.size;
        Vector3 lossyScale = root.transform.lossyScale;
        fallbackCollider.size = new Vector3(
            worldSize.x / Mathf.Max(0.0001f, Mathf.Abs(lossyScale.x)),
            worldSize.y / Mathf.Max(0.0001f, Mathf.Abs(lossyScale.y)),
            worldSize.z / Mathf.Max(0.0001f, Mathf.Abs(lossyScale.z)));
    }

    private static Light EnsureInteriorPointLight(
        Transform parent,
        string name,
        Vector3 localPosition,
        Color color,
        float intensity,
        float range,
        float flickerAmplitude,
        float flickerSpeed,
        float glitchChance)
    {
        Transform lightTransform = parent.Find(name);
        GameObject lightObject = lightTransform != null ? lightTransform.gameObject : new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = localPosition;
        lightObject.transform.localRotation = Quaternion.identity;

        Light light = EnsureComponent<Light>(lightObject);
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
        light.enabled = true;
        SampleSceneHorrorLightFlicker flicker = EnsureComponent<SampleSceneHorrorLightFlicker>(lightObject);
        flicker.Configure(intensity, flickerAmplitude, flickerSpeed, glitchChance);
        return light;
    }

    private static Light EnsureInteriorSpotLight(
        Transform parent,
        string name,
        Vector3 localPosition,
        Quaternion localRotation,
        Color color,
        float intensity,
        float range,
        float spotAngle,
        float flickerAmplitude,
        float flickerSpeed,
        float glitchChance)
    {
        Transform lightTransform = parent.Find(name);
        GameObject lightObject = lightTransform != null ? lightTransform.gameObject : new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.localPosition = localPosition;
        lightObject.transform.localRotation = localRotation;

        Light light = EnsureComponent<Light>(lightObject);
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = spotAngle;
        light.shadows = LightShadows.Soft;
        light.enabled = true;
        SampleSceneHorrorLightFlicker flicker = EnsureComponent<SampleSceneHorrorLightFlicker>(lightObject);
        flicker.Configure(intensity, flickerAmplitude, flickerSpeed, glitchChance);
        return light;
    }

    private static GameObject EnsureCapsuleActor(string name, Vector3 worldPosition, Vector3 worldScale, bool snapToGround = true)
    {
        GameObject existing = FindGameObjectInActiveScene(name);
        if (existing == null)
        {
            existing = new GameObject(name);
        }

        existing.transform.position = worldPosition;
        existing.transform.rotation = Quaternion.identity;
        existing.transform.localScale = Vector3.one;

        StripPrimitiveVisualComponents(existing);

        CapsuleCollider capsuleCollider = EnsureComponent<CapsuleCollider>(existing);
        capsuleCollider.radius = Mathf.Max(0.2f, Mathf.Max(worldScale.x, worldScale.z) * 0.35f);
        capsuleCollider.height = Mathf.Max(capsuleCollider.radius * 2f, worldScale.y * 1.9f);
        capsuleCollider.center = new Vector3(0f, capsuleCollider.height * 0.5f, 0f);
        capsuleCollider.isTrigger = false;
        if (snapToGround)
        {
            SnapActorToGround(existing, capsuleCollider);
        }

        return existing;
    }

    private static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    private static Transform EnsureChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child.transform;
    }

    private static void EnsureActorModel(GameObject actorRoot, string visualRootName, string modelPath, float targetHeight)
    {
        if (actorRoot == null)
        {
            return;
        }

#if UNITY_EDITOR
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        bool isCharMorphModel = IsCharMorphModelPath(modelPath);
        if (!isCharMorphModel)
        {
            EnsureModelImporterConfigured(modelPath);
        }
        modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (modelAsset == null)
        {
            Debug.LogWarning($"SampleSceneTestingBootstrap: model not found at '{modelPath}'.");
            return;
        }

        RemoveLegacyVisualRoots(actorRoot.transform, visualRootName);
        Transform visualRoot = EnsureChild(actorRoot.transform, visualRootName);
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(visualRoot.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(visualRoot.GetChild(i).gameObject);
            }
        }

        GameObject modelInstance = Instantiate(modelAsset, visualRoot);
        modelInstance.name = modelAsset.name;
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;

        StripModelRuntimeBehaviours(modelInstance);
        DisableModelPhysics(modelInstance);
        EnsureClothingPartsEnabled(modelInstance);
        ConfigureModelRenderers(modelInstance, modelPath);
        EnsureFallbackEyeballs(modelInstance, actorRoot.layer);
        if (isCharMorphModel)
        {
            DisableModelAnimators(modelInstance);
        }
        else
        {
            _ = PrepareModelAnimationForPoseControl(modelInstance, modelPath);
        }

        FitModelToTargetHeight(visualRoot, targetHeight);
        AlignModelFeetToWorldY(visualRoot, ResolveActorGroundY(actorRoot) + actorGroundYOffset);
        SetLayerRecursive(visualRoot, actorRoot.layer);
        AlignActorVisualRootsToGround(actorRoot, visualGroundOffset);

        if (isCharMorphModel)
        {
            RemoveComponent<SampleSceneLocomotionAnimatorDriver>(actorRoot);
            SampleSceneSourceModelIdlePose idlePose = EnsureComponent<SampleSceneSourceModelIdlePose>(actorRoot);
            idlePose.Configure(visualRoot);
            idlePose.SetNeutralMode(true);
            idlePose.enabled = true;
        }
        else
        {
            SampleSceneLocomotionAnimatorDriver animatorDriver = EnsureComponent<SampleSceneLocomotionAnimatorDriver>(actorRoot);
            animatorDriver.Configure(visualRoot);
            SampleSceneSourceModelIdlePose idlePose = EnsureComponent<SampleSceneSourceModelIdlePose>(actorRoot);
            idlePose.Configure(visualRoot);
            idlePose.SetNeutralMode(true);
            idlePose.enabled = false;
        }
#else
        _ = visualRootName;
        _ = modelPath;
        _ = targetHeight;
#endif
    }

    private static string ResolvePlayerModelPath()
    {
#if UNITY_EDITOR
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ExportedPlayerModelPath) != null)
        {
            return ExportedPlayerModelPath;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(ImportedPlayerModelPath) != null)
        {
            return ImportedPlayerModelPath;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(LegacyPlayerModelPath) != null)
        {
            return LegacyPlayerModelPath;
        }
#endif
        return LegacyPlayerModelPath;
    }

    private static void ConfigureActorRigForSourceVisual(GameObject actorRoot)
    {
        if (actorRoot == null)
        {
            return;
        }

        ProceduralHumanoidRig rig = actorRoot.GetComponent<ProceduralHumanoidRig>();
        if (rig == null)
        {
            return;
        }

        rig.EnsureBuilt();
        rig.ConfigureRendererVisibility(false, false);

        if (rig.PhysicalRoot != null)
        {
            SetLayerRecursive(rig.PhysicalRoot, actorRoot.layer);
        }
    }

    private static void PrepareNpcPhysicsActor(GameObject actorRoot, bool lockHorizontalPosition)
    {
        if (actorRoot == null)
        {
            return;
        }

        RemoveComponent<CharacterController>(actorRoot);
        RemoveComponent<PlayerMover>(actorRoot);
        RemoveComponent<PlayerInteraction>(actorRoot);
        RemoveComponent<RealTimeCombat>(actorRoot);
        RemoveComponent<EnemyController>(actorRoot);
        RemoveComponent<NavMeshAgent>(actorRoot);
        RemoveComponent<ProceduralHumanoidRig>(actorRoot);
        RemoveComponent<ActiveRagdollMotor>(actorRoot);
        RemoveComponent<DeathRagdollController>(actorRoot);
        RemoveComponent<PlayerAnimationController>(actorRoot);
        RemoveComponent<HumanoidIdlePoseController>(actorRoot);
        RemoveLegacyRagdollHierarchy(actorRoot);

        Rigidbody body = EnsureComponent<Rigidbody>(actorRoot);
        body.mass = 75f;
        body.linearDamping = 0.35f;
        body.angularDamping = 4f;
        body.isKinematic = false;
        body.useGravity = true;
        body.constraints = RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ |
            (lockHorizontalPosition ? RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ : RigidbodyConstraints.None);
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.WakeUp();
    }

    private static void DisableModelPhysics(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return;
        }

        Collider[] colliders = modelRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        Rigidbody[] rigidbodies = modelRoot.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i] != null)
            {
                rigidbodies[i].isKinematic = true;
                rigidbodies[i].useGravity = false;
            }
        }
    }

    private static void StripModelRuntimeBehaviours(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return;
        }

        MonoBehaviour[] behaviours = modelRoot.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(behaviour);
            }
            else
            {
                DestroyImmediate(behaviour);
            }
        }
    }

    private static void DisableModelAnimators(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return;
        }

        Animator[] animators = modelRoot.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
            {
                continue;
            }

            animator.runtimeAnimatorController = null;
            animator.applyRootMotion = false;
            animator.enabled = false;
        }
    }

    private static void EnsureClothingPartsEnabled(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return;
        }

        Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform == null)
            {
                continue;
            }

            string name = transform.name.ToLowerInvariant();
            bool shouldEnable = name.Contains("shirt") ||
                                name.Contains("pants") ||
                                name.Contains("cloth") ||
                                name.Contains("sneaker") ||
                                name.Contains("shoe") ||
                                name.Contains("hair") ||
                                name.Contains("eyelash");
            if (!shouldEnable)
            {
                continue;
            }

            if (!transform.gameObject.activeSelf)
            {
                transform.gameObject.SetActive(true);
            }

            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    private static bool PrepareModelAnimationForPoseControl(GameObject modelRoot, string modelPath)
    {
        if (modelRoot == null)
        {
            return false;
        }

        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(LocomotionControllerResourcePath);
#if UNITY_EDITOR
        if (!locomotionControllerEnsured || controller == null)
        {
            try
            {
                InvokeEditorLocomotionBuilder();
                AssetDatabase.Refresh();
                controller = Resources.Load<RuntimeAnimatorController>(LocomotionControllerResourcePath);
                locomotionControllerEnsured = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SampleSceneTestingBootstrap: unable to build locomotion controller. {ex.Message}");
            }
        }
#endif
        bool hasController = controller != null;
        Avatar preferredAvatar = ResolvePreferredAvatar(modelPath);

        Animator[] animators = modelRoot.GetComponentsInChildren<Animator>(true);
        Animator primaryAnimator = ResolvePrimaryModelAnimator(animators, modelRoot.transform);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
            {
                continue;
            }

            bool isPrimary = primaryAnimator == null || animator == primaryAnimator;
            if (!isPrimary)
            {
                animator.runtimeAnimatorController = null;
                animator.applyRootMotion = false;
                animator.enabled = false;
                continue;
            }

            if ((animator.avatar == null || !animator.avatar.isValid) && preferredAvatar != null)
            {
                animator.avatar = preferredAvatar;
            }

            if (hasController)
            {
                animator.runtimeAnimatorController = controller;
            }
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.enabled = true;
            animator.Rebind();
            if (animator.gameObject.activeInHierarchy)
            {
                animator.Update(0f);
            }
        }

        Animation[] legacyAnimations = modelRoot.GetComponentsInChildren<Animation>(true);
        for (int i = 0; i < legacyAnimations.Length; i++)
        {
            Animation animation = legacyAnimations[i];
            if (animation == null)
            {
                continue;
            }

            animation.Stop();
            animation.enabled = false;
            animation.playAutomatically = false;
        }

        return hasController;
    }

    private static Avatar ResolvePreferredAvatar(string modelPath)
    {
#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(modelPath))
        {
            string normalized = modelPath.Replace('\\', '/').ToLowerInvariant();
            if (normalized.Contains("prisoner"))
            {
                Avatar prisonerAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(PrisonerAvatarPath);
                if (prisonerAvatar != null && prisonerAvatar.isValid)
                {
                    return prisonerAvatar;
                }
            }

            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            for (int i = 0; i < subAssets.Length; i++)
            {
                if (subAssets[i] is Avatar avatar && avatar != null && avatar.isValid)
                {
                    return avatar;
                }
            }
        }
#endif
        return null;
    }

    private static Animator ResolvePrimaryModelAnimator(Animator[] animators, Transform modelRoot)
    {
        if (animators == null || animators.Length == 0)
        {
            return null;
        }

        Animator best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < animators.Length; i++)
        {
            Animator candidate = animators[i];
            if (candidate == null)
            {
                continue;
            }

            int score = ScoreAnimatorCandidate(candidate, modelRoot);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            best = candidate;
        }

        return best;
    }

    private static int ScoreAnimatorCandidate(Animator animator, Transform modelRoot)
    {
        if (animator == null)
        {
            return int.MinValue;
        }

        int score = 0;
        if (animator.transform == modelRoot)
        {
            score += 1200;
        }

        if (animator.avatar != null && animator.avatar.isValid)
        {
            score += animator.isHuman ? 420 : 160;
        }

        if (animator.runtimeAnimatorController != null)
        {
            score += 140;
        }

        SkinnedMeshRenderer[] skinned = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        score += Mathf.Min(skinned.Length, 64) * 12;

        int depth = ResolveDepthFromRoot(modelRoot, animator.transform);
        if (depth >= 0)
        {
            score += Mathf.Max(0, 80 - (depth * 7));
        }

        if (!animator.gameObject.activeInHierarchy)
        {
            score -= 80;
        }

        return score;
    }

    private static int ResolveDepthFromRoot(Transform root, Transform target)
    {
        if (root == null || target == null)
        {
            return -1;
        }

        int depth = 0;
        Transform walker = target;
        while (walker != null)
        {
            if (walker == root)
            {
                return depth;
            }

            depth++;
            walker = walker.parent;
        }

        return -1;
    }

#if UNITY_EDITOR
    private static void InvokeEditorLocomotionBuilder()
    {
        Type builderType = Type.GetType("SampleSceneLocomotionControllerBuilder");
        if (builderType == null)
        {
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                System.Reflection.Assembly assembly = assemblies[i];
                if (assembly == null)
                {
                    continue;
                }

                builderType = assembly.GetType("SampleSceneLocomotionControllerBuilder");
                if (builderType != null)
                {
                    break;
                }
            }
        }

        if (builderType == null)
        {
            Debug.LogWarning("SampleSceneTestingBootstrap: locomotion builder type not found.");
            return;
        }

        System.Reflection.MethodInfo buildMethod = builderType.GetMethod(
            "BuildController",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        buildMethod?.Invoke(null, null);
    }
#endif

    private static void ConfigureModelRenderers(GameObject modelRoot, string modelPath)
    {
        if (modelRoot == null)
        {
            return;
        }

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        bool isCharMorphModel = IsCharMorphModelPath(modelPath);
        TextureProfile textureProfile = ResolveTextureProfile(modelPath);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            string rendererName = renderer.name.ToLowerInvariant();
            if (rendererName.Contains("dagger") ||
                rendererName.Contains("sword") ||
                rendererName.Contains("weapon"))
            {
                renderer.enabled = false;
                continue;
            }

            if (renderer is SkinnedMeshRenderer skinnedRenderer)
            {
                skinnedRenderer.updateWhenOffscreen = true;
                Bounds bounds = skinnedRenderer.localBounds;
                if (bounds.size.sqrMagnitude < 0.001f)
                {
                    bounds = new Bounds(Vector3.zero, Vector3.one * 2f);
                }
                else
                {
                    bounds.Expand(Vector3.one * 0.3f);
                }

                skinnedRenderer.localBounds = bounds;
            }
        }

        if (isCharMorphModel)
        {
            bool charMorphImportedApplied = textureProfile != TextureProfile.None &&
                                            ApplyImportedCharacterMaterials(renderers, textureProfile);
            if (!charMorphImportedApplied)
            {
                bool embeddedApplied = ApplyEmbeddedModelMaterials(renderers, modelPath);
                if (!embeddedApplied)
                {
                    ApplyFallbackCharacterMaterials(renderers, true);
                }
                else
                {
                    ApplyFallbackCharacterMaterials(renderers);
                }

                return;
            }

            ApplyFallbackCharacterMaterials(renderers);
            return;
        }

        if (textureProfile == TextureProfile.None)
        {
            bool embeddedApplied = ApplyEmbeddedModelMaterials(renderers, modelPath);
            if (!embeddedApplied)
            {
                ApplyFallbackCharacterMaterials(renderers, !ShouldPreferNativeMaterials(modelPath));
            }
            else
            {
                ApplyFallbackCharacterMaterials(renderers);
            }
            return;
        }

        if (ApplyEmbeddedModelMaterials(renderers, modelPath))
        {
            return;
        }

        bool importedApplied = ApplyImportedCharacterMaterials(renderers, textureProfile);
        if (!importedApplied)
        {
            ApplyFallbackCharacterMaterials(renderers);
        }
    }

    private static void EnsureFallbackEyeballs(GameObject modelRoot, int targetLayer)
    {
        if (modelRoot == null)
        {
            return;
        }

        RemoveAllFallbackEyes(modelRoot);
        if (HasUsableEyeRenderers(modelRoot))
        {
            return;
        }

        Transform head = ResolveHeadTransform(modelRoot);
        if (head == null)
        {
            return;
        }

        if (!TryResolveHeadBounds(head, out Vector3 headCenter, out Vector3 headExtents))
        {
            return;
        }

        Vector3 leftEyeLocal = default;
        Vector3 rightEyeLocal = default;
        float eyeScale = 0.016f;
        bool resolved = TryResolveEyeAnchorsFromHumanoidBones(modelRoot, head, out leftEyeLocal, out rightEyeLocal, out eyeScale) ||
                        TryResolveEyeAnchorsFromRenderers(modelRoot, head, out leftEyeLocal, out rightEyeLocal, out eyeScale);

        if (!resolved)
        {
            float eyeOffsetX = Mathf.Clamp(headExtents.x * 0.30f, 0.012f, 0.034f);
            float eyeY = headCenter.y + Mathf.Clamp(headExtents.y * 0.15f, 0.012f, 0.055f);
            float eyeZ = headCenter.z + Mathf.Clamp(headExtents.z * 0.72f, 0.04f, 0.11f);
            leftEyeLocal = new Vector3(headCenter.x - eyeOffsetX, eyeY, eyeZ);
            rightEyeLocal = new Vector3(headCenter.x + eyeOffsetX, eyeY, eyeZ);
            eyeScale = Mathf.Clamp(Mathf.Min(headExtents.x, headExtents.y) * 0.30f, 0.010f, 0.019f);
        }

        ConstrainFallbackEyesToFaceBounds(head, ref leftEyeLocal, ref rightEyeLocal, ref eyeScale);
        Transform eyesRoot = EnsureFallbackEyesRoot(head);
        EnsureFallbackEye(eyesRoot, "FallbackEye_L", leftEyeLocal, eyeScale, targetLayer);
        EnsureFallbackEye(eyesRoot, "FallbackEye_R", rightEyeLocal, eyeScale, targetLayer);
    }

    private static bool HasUsableEyeRenderers(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return false;
        }

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            string rendererName = renderer.name.ToLowerInvariant();
            bool eyeNamedRenderer = IsDedicatedEyeToken(rendererName);
            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                continue;
            }

            for (int m = 0; m < materials.Length; m++)
            {
                Material material = materials[m];
                if (material == null)
                {
                    continue;
                }

                string materialName = material.name.ToLowerInvariant();
                bool eyeLikeMaterial = eyeNamedRenderer || IsDedicatedEyeToken(materialName);
                if (!eyeLikeMaterial)
                {
                    continue;
                }

                if (HasAlbedoTexture(material))
                {
                    return true;
                }

                Color baseColor = Color.black;
                if (material.HasProperty("_BaseColor"))
                {
                    baseColor = material.GetColor("_BaseColor");
                }
                else if (material.HasProperty("_Color"))
                {
                    baseColor = material.GetColor("_Color");
                }

                float luminance = (0.2126f * baseColor.r) + (0.7152f * baseColor.g) + (0.0722f * baseColor.b);
                if (luminance > 0.22f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void RemoveAllFallbackEyes(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return;
        }

        Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
        for (int i = transforms.Length - 1; i >= 0; i--)
        {
            Transform current = transforms[i];
            if (current == null)
            {
                continue;
            }

            string n = current.name;
            if (!string.Equals(n, "FallbackEyes", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(n, "FallbackEye_L", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(n, "FallbackEye_R", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(current.gameObject);
            }
            else
            {
                DestroyImmediate(current.gameObject);
            }
        }
    }

    private static bool HasDedicatedEyeballs(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return false;
        }

        if (TryResolveHumanoidEyeBones(modelRoot, out _, out _))
        {
            return true;
        }

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            string rendererName = renderer.name.ToLowerInvariant();
            if (IsDedicatedEyeToken(rendererName))
            {
                return true;
            }

            Material[] materials = renderer.sharedMaterials;
            if (materials == null)
            {
                continue;
            }

            for (int m = 0; m < materials.Length; m++)
            {
                Material material = materials[m];
                if (material == null)
                {
                    continue;
                }

                string materialName = material.name.ToLowerInvariant();
                if (IsDedicatedEyeToken(materialName))
                {
                    return true;
                }
            }
        }

        Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            if (current == null)
            {
                continue;
            }

            if (IsDedicatedEyeToken(current.name.ToLowerInvariant()))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryResolveEyeAnchorsFromHumanoidBones(
        GameObject modelRoot,
        Transform head,
        out Vector3 leftEyeLocal,
        out Vector3 rightEyeLocal,
        out float eyeScale)
    {
        leftEyeLocal = default;
        rightEyeLocal = default;
        eyeScale = 0.022f;

        if (!TryResolveHumanoidEyeBones(modelRoot, out Transform leftEyeBone, out Transform rightEyeBone) ||
            head == null)
        {
            return false;
        }

        Vector3 leftWorld = leftEyeBone.position;
        Vector3 rightWorld = rightEyeBone.position;
        if (leftWorld.x > rightWorld.x)
        {
            (leftWorld, rightWorld) = (rightWorld, leftWorld);
        }

        leftEyeLocal = head.InverseTransformPoint(leftWorld);
        rightEyeLocal = head.InverseTransformPoint(rightWorld);

        float spacing = Mathf.Abs(rightEyeLocal.x - leftEyeLocal.x);
        eyeScale = Mathf.Clamp(spacing * 0.34f, 0.012f, 0.026f);
        return true;
    }

    private static bool TryResolveEyeAnchorsFromRenderers(
        GameObject modelRoot,
        Transform head,
        out Vector3 leftEyeLocal,
        out Vector3 rightEyeLocal,
        out float eyeScale)
    {
        leftEyeLocal = default;
        rightEyeLocal = default;
        eyeScale = 0.04f;

        if (modelRoot == null || head == null)
        {
            return false;
        }

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;
        Bounds eyeBounds = default;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            string name = renderer.name.ToLowerInvariant();
            bool anchorLike = IsDedicatedEyeToken(name) ||
                              name.Contains("eyelash") ||
                              name.Contains("_lash") ||
                              name.Contains(" lash");
            if (!anchorLike)
            {
                continue;
            }

            if (!initialized)
            {
                eyeBounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                eyeBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!initialized)
        {
            return false;
        }

        Vector3 eyeCenter = eyeBounds.center;
        float eyeSpacing = Mathf.Clamp(eyeBounds.extents.x * 0.5f, 0.015f, 0.032f);
        float inset = Mathf.Clamp(eyeBounds.extents.z * 0.18f, 0.001f, 0.01f);

        Vector3 leftWorld = eyeCenter - (head.right * eyeSpacing) + (head.forward * inset);
        Vector3 rightWorld = eyeCenter + (head.right * eyeSpacing) + (head.forward * inset);

        leftEyeLocal = head.InverseTransformPoint(leftWorld);
        rightEyeLocal = head.InverseTransformPoint(rightWorld);
        eyeScale = Mathf.Clamp(Mathf.Min(eyeBounds.extents.x, eyeBounds.extents.y) * 0.62f, 0.010f, 0.022f);
        if (Mathf.Abs(rightEyeLocal.x - leftEyeLocal.x) < 0.012f)
        {
            return false;
        }

        return true;
    }

    private static Transform EnsureFallbackEyesRoot(Transform parent)
    {
        Transform eyesRoot = parent.Find("FallbackEyes");
        if (eyesRoot != null)
        {
            eyesRoot.localPosition = Vector3.zero;
            eyesRoot.localRotation = Quaternion.identity;
            eyesRoot.localScale = Vector3.one;
            return eyesRoot;
        }

        GameObject eyesObject = new GameObject("FallbackEyes");
        eyesRoot = eyesObject.transform;
        eyesRoot.SetParent(parent, false);
        eyesRoot.localPosition = Vector3.zero;
        eyesRoot.localRotation = Quaternion.identity;
        eyesRoot.localScale = Vector3.one;
        return eyesRoot;
    }

    private static Vector3 ConvertWorldExtentsToLocal(Transform reference, Vector3 worldExtents)
    {
        if (reference == null)
        {
            return worldExtents;
        }

        return new Vector3(
            Mathf.Abs(worldExtents.x / Mathf.Max(0.0001f, reference.lossyScale.x)),
            Mathf.Abs(worldExtents.y / Mathf.Max(0.0001f, reference.lossyScale.y)),
            Mathf.Abs(worldExtents.z / Mathf.Max(0.0001f, reference.lossyScale.z)));
    }

    private static bool TryResolveEyeAnchorRenderer(GameObject modelRoot, out Renderer anchorRenderer)
    {
        anchorRenderer = null;
        if (modelRoot == null)
        {
            return false;
        }

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        int bestScore = int.MinValue;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            string key = renderer.name.ToLowerInvariant();
            int score = 0;
            if (key.Contains("eyelash") || key.Contains("_lash") || key.Contains(" lash"))
            {
                score += 120;
            }
            else if (IsDedicatedEyeToken(key))
            {
                score += 80;
            }

            if (score == 0)
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null)
                {
                    continue;
                }

                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material == null)
                    {
                        continue;
                    }

                    string matName = material.name.ToLowerInvariant();
                    if (matName.Contains("eyelash") || matName.Contains("_lash") || matName.Contains(" lash"))
                    {
                        score += 100;
                        break;
                    }

                    if (IsDedicatedEyeToken(matName))
                    {
                        score += 70;
                        break;
                    }
                }
            }

            if (score == 0)
            {
                continue;
            }

            Bounds bounds = renderer.bounds;
            float volume = bounds.size.x * bounds.size.y * bounds.size.z;
            if (bounds.size.x > 0.65f || bounds.size.y > 0.45f)
            {
                score -= 120;
            }

            score -= Mathf.RoundToInt(Mathf.Clamp(volume, 0f, 0.15f) * 1000f);
            if (score > bestScore)
            {
                bestScore = score;
                anchorRenderer = renderer;
            }
        }

        return anchorRenderer != null;
    }

    private static bool TryResolveEyelashAnchorInHeadSpace(
        GameObject modelRoot,
        Transform head,
        Vector3 headCenter,
        Vector3 headExtents,
        out Vector3 anchorCenter,
        out Vector3 anchorExtents)
    {
        anchorCenter = default;
        anchorExtents = default;
        if (modelRoot == null || head == null)
        {
            return false;
        }

        Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
        int bestScore = int.MinValue;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            string name = renderer.name.ToLowerInvariant();
            bool eyelashLike = name.Contains("eyelash") || name.Contains("_lash") || name.Contains(" lash");
            if (!eyelashLike)
            {
                continue;
            }

            Vector3 localCenter = head.InverseTransformPoint(renderer.bounds.center);
            Vector3 localExtents = ConvertWorldExtentsToLocal(head, renderer.bounds.extents);
            if (localCenter.y > headCenter.y + (headExtents.y * 0.78f) ||
                localCenter.y < headCenter.y - (headExtents.y * 0.28f) ||
                localCenter.z < headCenter.z + (headExtents.z * 0.2f) ||
                localCenter.z > headCenter.z + (headExtents.z * 1.15f))
            {
                continue;
            }

            float yDelta = Mathf.Abs(localCenter.y - (headCenter.y + headExtents.y * 0.12f));
            float zDelta = Mathf.Abs(localCenter.z - (headCenter.z + headExtents.z * 0.86f));
            float volume = localExtents.x * localExtents.y * localExtents.z;
            int score = 200
                - Mathf.RoundToInt(yDelta * 180f)
                - Mathf.RoundToInt(zDelta * 150f)
                - Mathf.RoundToInt(Mathf.Clamp(volume, 0f, 0.03f) * 1500f);
            if (score > bestScore)
            {
                bestScore = score;
                anchorCenter = localCenter;
                anchorExtents = localExtents;
            }
        }

        return bestScore > int.MinValue;
    }

    private static bool TryResolveHumanoidEyeBones(GameObject modelRoot, out Transform leftEye, out Transform rightEye)
    {
        leftEye = null;
        rightEye = null;
        if (modelRoot == null)
        {
            return false;
        }

        Animator animator = modelRoot.GetComponentInChildren<Animator>(true);
        if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.isHuman)
        {
            return false;
        }

        leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
        rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
        return leftEye != null && rightEye != null;
    }

    private static bool IsDedicatedEyeToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string token = value.ToLowerInvariant();
        bool hasEyeWord = token.Contains("eye") || token.Contains("iris") || token.Contains("sclera") ||
                          token.Contains("pupil") || token.Contains("cornea") || token.Contains("eyeball");
        if (!hasEyeWord)
        {
            return false;
        }

        bool lashOrLid = token.Contains("eyelash") || token.Contains("_lash") || token.Contains(" lash") ||
                         token.Contains("brow") || token.Contains("lid");
        return !lashOrLid;
    }

    private static void ConstrainFallbackEyesToFaceBounds(
        Transform head,
        ref Vector3 leftEyeLocal,
        ref Vector3 rightEyeLocal,
        ref float eyeScale)
    {
        if (head == null || !TryResolveHeadBounds(head, out Vector3 center, out Vector3 extents))
        {
            eyeScale = Mathf.Clamp(eyeScale, 0.010f, 0.026f);
            return;
        }

        float maxOffsetX = Mathf.Clamp(extents.x * 0.46f, 0.018f, 0.048f);
        float minY = center.y - Mathf.Clamp(extents.y * 0.10f, 0.006f, 0.028f);
        float maxY = center.y + Mathf.Clamp(extents.y * 0.22f, 0.018f, 0.052f);
        float minZ = center.z + Mathf.Clamp(extents.z * 0.30f, 0.026f, 0.058f);
        float maxZ = center.z + Mathf.Clamp(extents.z * 0.76f, 0.055f, 0.11f);
        float targetZ = Mathf.Clamp((leftEyeLocal.z + rightEyeLocal.z) * 0.5f, minZ, maxZ);

        leftEyeLocal.x = Mathf.Clamp(leftEyeLocal.x, -maxOffsetX, -0.006f);
        rightEyeLocal.x = Mathf.Clamp(rightEyeLocal.x, 0.006f, maxOffsetX);
        leftEyeLocal.y = Mathf.Clamp(leftEyeLocal.y, minY, maxY);
        rightEyeLocal.y = Mathf.Clamp(rightEyeLocal.y, minY, maxY);
        leftEyeLocal.z = targetZ;
        rightEyeLocal.z = targetZ;

        float spacing = Mathf.Abs(rightEyeLocal.x - leftEyeLocal.x);
        float minScale = Mathf.Clamp(spacing * 0.18f, 0.010f, 0.02f);
        float maxScale = Mathf.Clamp(spacing * 0.38f, 0.015f, 0.028f);
        eyeScale = Mathf.Clamp(eyeScale, minScale, maxScale);
    }

    private static bool TryResolveHeadBounds(Transform head, out Vector3 localCenter, out Vector3 localExtents)
    {
        localCenter = Vector3.zero;
        localExtents = Vector3.zero;
        if (head == null)
        {
            return false;
        }

        Renderer[] renderers = head.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;
        Bounds worldBounds = default;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            string n = renderer.name.ToLowerInvariant();
            if (n.Contains("hair") || n.Contains("eyelash"))
            {
                continue;
            }

            if (!initialized)
            {
                worldBounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                worldBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!initialized)
        {
            return false;
        }

        localCenter = head.InverseTransformPoint(worldBounds.center);
        Vector3 worldExtents = worldBounds.extents;
        localExtents = new Vector3(
            Mathf.Abs(worldExtents.x / Mathf.Max(0.0001f, head.lossyScale.x)),
            Mathf.Abs(worldExtents.y / Mathf.Max(0.0001f, head.lossyScale.y)),
            Mathf.Abs(worldExtents.z / Mathf.Max(0.0001f, head.lossyScale.z)));
        return true;
    }

    private static void EnsureFallbackEye(Transform parent, string name, Vector3 localPosition, float scale, int targetLayer)
    {
        Transform eye = parent.Find(name);
        GameObject eyeObject;
        if (eye == null)
        {
            eyeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeObject.name = name;
            eyeObject.transform.SetParent(parent, false);
            RemoveComponent<Collider>(eyeObject);
        }
        else
        {
            eyeObject = eye.gameObject;
        }

        eyeObject.transform.localPosition = localPosition;
        eyeObject.transform.localRotation = Quaternion.identity;
        eyeObject.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        eyeObject.layer = targetLayer;

        MeshRenderer renderer = eyeObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material eyesMaterial = GetImportedMaterial("fallback_eyes", Color.white, CommonIrisColorPath, 0.9f, 0f);
            renderer.sharedMaterial = eyesMaterial != null
                ? eyesMaterial
                : GetFallbackMaterial("eyes", new Color(0.95f, 0.95f, 0.96f, 1f));
            renderer.enabled = true;
        }
    }

    private static Transform ResolveHeadTransform(GameObject modelRoot)
    {
        Animator animator = modelRoot.GetComponentInChildren<Animator>(true);
        if (animator != null && animator.avatar != null && animator.avatar.isValid && animator.isHuman)
        {
            Transform boneHead = animator.GetBoneTransform(HumanBodyBones.Head);
            if (boneHead != null)
            {
                return boneHead;
            }
        }

        Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
        Transform best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null)
            {
                continue;
            }

            string n = candidate.name.ToLowerInvariant();
            if (!n.Contains("head"))
            {
                continue;
            }

            int score = 0;
            if (n == "head" || n.EndsWith(":head") || n.EndsWith("_head"))
            {
                score += 100;
            }
            else
            {
                score += 10;
            }

            if (n.Contains("headtop") || n.Contains("top_end") || n.Contains("headtop_end") || n.Contains("end"))
            {
                score -= 80;
            }

            if (n.Contains("hair") || n.Contains("eyelash"))
            {
                score -= 40;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private static TextureProfile ResolveTextureProfile(string modelPath)
    {
        string normalized = (modelPath ?? string.Empty).ToLowerInvariant();
        if (normalized.Contains("prisoner"))
        {
            return TextureProfile.None;
        }

        if (normalized.Contains("/ch11_") || normalized.Contains("\\ch11_"))
        {
            return TextureProfile.None;
        }

        if (normalized.Contains("/ch01_") || normalized.Contains("/ch02_") ||
            normalized.Contains("\\ch01_") || normalized.Contains("\\ch02_"))
        {
            return TextureProfile.None;
        }

        if (normalized.Contains("antonia"))
        {
            return TextureProfile.Antonia;
        }

        if (normalized.Contains("mb-lab_male"))
        {
            return TextureProfile.MbMale;
        }

        if (normalized.Contains("vitruvian"))
        {
            return TextureProfile.None;
        }

        return TextureProfile.Vitruvian;
    }

    private static bool ShouldPreferNativeMaterials(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return false;
        }

        string normalized = modelPath.Replace('\\', '/').ToLowerInvariant();
        return normalized.Contains("prisoneraprefab") ||
               normalized.Contains("char_prisonera");
    }

    private static bool IsCharMorphModelPath(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return false;
        }

        string normalized = modelPath.Replace('\\', '/').ToLowerInvariant();
        return normalized.Contains("/charmorph/");
    }

    private static bool ApplyImportedCharacterMaterials(Renderer[] renderers, TextureProfile profile)
    {
        if (renderers == null || renderers.Length == 0)
        {
            return false;
        }

        bool anyImported = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                Material importedSingle = ResolveImportedMaterialForRenderer(profile, renderer.name);
                if (importedSingle != null)
                {
                    renderer.sharedMaterial = importedSingle;
                    anyImported = true;
                }
                continue;
            }

            bool changed = false;
            for (int m = 0; m < shared.Length; m++)
            {
                Material current = shared[m];
                string slotKey = $"{renderer.name}_{m}";
                Material imported = ResolveImportedMaterialForRenderer(profile, slotKey);
                if (imported != null)
                {
                    shared[m] = imported;
                    changed = true;
                    anyImported = true;
                    continue;
                }

                if (current == null || !HasAlbedoTexture(current))
                {
                    Material fallback = ResolveMaterialForRenderer(slotKey);
                    if (fallback != null)
                    {
                        shared[m] = fallback;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = shared;
            }
        }

        return anyImported;
    }

    private static bool ApplyEmbeddedModelMaterials(Renderer[] renderers, string modelPath)
    {
        if (renderers == null || renderers.Length == 0 || string.IsNullOrWhiteSpace(modelPath))
        {
            return false;
        }

#if UNITY_EDITOR
        UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        if (subAssets == null || subAssets.Length == 0)
        {
            return false;
        }

        List<Material> embeddedMaterials = new List<Material>();
        List<Texture2D> embeddedTextures = new List<Texture2D>();
        for (int i = 0; i < subAssets.Length; i++)
        {
            UnityEngine.Object asset = subAssets[i];
            if (asset is Material embeddedMaterial)
            {
                embeddedMaterials.Add(embeddedMaterial);
                continue;
            }

            if (asset is Texture2D embeddedTexture)
            {
                embeddedTextures.Add(embeddedTexture);
            }
        }

        bool anyAssigned = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                shared = new Material[1];
            }

            bool changed = false;
            for (int m = 0; m < shared.Length; m++)
            {
                Material current = shared[m];
                if (current != null && HasAlbedoTexture(current))
                {
                    continue;
                }

                string slotKey = $"{renderer.name}_{m}";
                Material replacement = ResolveEmbeddedMaterial(embeddedMaterials, embeddedTextures, slotKey);
                if (replacement == null)
                {
                    continue;
                }

                shared[m] = replacement;
                changed = true;
                anyAssigned = true;
            }

            if (changed)
            {
                renderer.sharedMaterials = shared;
            }
        }

        return anyAssigned;
#else
        _ = modelPath;
        return false;
#endif
    }

    private static Material ResolveEmbeddedMaterial(List<Material> embeddedMaterials, List<Texture2D> embeddedTextures, string slotKey)
    {
        string normalized = (slotKey ?? string.Empty).ToLowerInvariant();

        for (int i = 0; i < embeddedMaterials.Count; i++)
        {
            Material candidate = embeddedMaterials[i];
            if (candidate == null)
            {
                continue;
            }

            string candidateName = candidate.name.ToLowerInvariant();
            bool candidateMatches = normalized.Contains(candidateName) ||
                                    candidateName.Contains("body") && normalized.Contains("body") ||
                                    candidateName.Contains("shirt") && normalized.Contains("shirt") ||
                                    candidateName.Contains("pants") && normalized.Contains("pants") ||
                                    candidateName.Contains("shoe") && (normalized.Contains("shoe") || normalized.Contains("sneaker")) ||
                                    candidateName.Contains("hair") && normalized.Contains("hair") ||
                                    candidateName.Contains("eye") && normalized.Contains("eye");
            if (candidateMatches && HasAlbedoTexture(candidate))
            {
                return candidate;
            }
        }

        Texture2D texture = ResolveEmbeddedTexture(embeddedTextures, normalized);
        if (texture == null)
        {
            return null;
        }

        string cacheKey = $"embedded_{texture.name.ToLowerInvariant()}";
        if (fallbackMaterialCache.TryGetValue(cacheKey, out Material cached) && cached != null)
        {
            return cached;
        }

        Material material = BuildTexturedLitMaterial(cacheKey, texture, Color.white, 0.42f, 0f);
        fallbackMaterialCache[cacheKey] = material;
        return material;
    }

    private static Texture2D ResolveEmbeddedTexture(List<Texture2D> textures, string slotKey)
    {
        if (textures == null || textures.Count == 0)
        {
            return null;
        }

        string[] preferredTokens;
        if (slotKey.Contains("eye") || slotKey.Contains("iris") || slotKey.Contains("sclera"))
        {
            preferredTokens = new[] { "eye", "iris", "sclera" };
        }
        else if (slotKey.Contains("hair") || slotKey.Contains("lash") || slotKey.Contains("brow"))
        {
            preferredTokens = new[] { "hair", "lash", "brow" };
        }
        else if (slotKey.Contains("mouth") || slotKey.Contains("lip") || slotKey.Contains("teeth"))
        {
            preferredTokens = new[] { "mouth", "lip", "teeth" };
        }
        else if (slotKey.Contains("shirt") || slotKey.Contains("upper") || slotKey.Contains("cloth"))
        {
            preferredTokens = new[] { "shirt", "upper", "cloth", "torso" };
        }
        else if (slotKey.Contains("pants") || slotKey.Contains("leg") || slotKey.Contains("lower"))
        {
            preferredTokens = new[] { "pants", "leg", "lower", "trousers" };
        }
        else if (slotKey.Contains("shoe") || slotKey.Contains("sneaker") || slotKey.Contains("boot"))
        {
            preferredTokens = new[] { "shoe", "sneaker", "boot", "footwear" };
        }
        else
        {
            preferredTokens = new[] { "body", "skin", "diffuse", "albedo", "basecolor", "base_color", "color" };
        }

        for (int tokenIndex = 0; tokenIndex < preferredTokens.Length; tokenIndex++)
        {
            string token = preferredTokens[tokenIndex];
            for (int textureIndex = 0; textureIndex < textures.Count; textureIndex++)
            {
                Texture2D texture = textures[textureIndex];
                if (texture == null)
                {
                    continue;
                }

                string textureName = texture.name.ToLowerInvariant();
                if (textureName.Contains(token))
                {
                    return texture;
                }
            }
        }

        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D texture = textures[i];
            if (texture == null)
            {
                continue;
            }

            string name = texture.name.ToLowerInvariant();
            if (name.Contains("diffuse") || name.Contains("albedo") || name.Contains("base") || name.Contains("color"))
            {
                return texture;
            }
        }

        return textures[0];
    }

    private static Material ResolveImportedMaterialForRenderer(TextureProfile profile, string rendererName)
    {
        string key = (rendererName ?? string.Empty).ToLowerInvariant();
        Color skinBaseColor = ResolveSkinBaseColor(profile);

        string skinTexture = profile switch
        {
            TextureProfile.Antonia => AntoniaBodyAlbedoPath,
            TextureProfile.MbMale => MbMaleAlbedoPath,
            TextureProfile.Vitruvian => MbMaleAlbedoPath,
            _ => MbMaleAlbedoPath
        };

        string headTexture = profile == TextureProfile.Antonia ? AntoniaHeadAlbedoPath : skinTexture;
        string armTexture = profile == TextureProfile.Antonia ? AntoniaArmsAlbedoPath : skinTexture;
        string legTexture = profile == TextureProfile.Antonia ? AntoniaLegsAlbedoPath : skinTexture;
        string mouthTexture = profile == TextureProfile.Antonia ? AntoniaMouthPath : MbMaleLipPath;
        string eyesTexture = profile == TextureProfile.Antonia ? AntoniaEyesPath : CommonIrisColorPath;

        bool isLash = key.Contains("eyelash") || key.Contains("_lash") || key.Contains(" lash");
        bool isEyes = !isLash && (key.Contains("eye") || key.Contains("iris") || key.Contains("sclera") || key.Contains("tear") || key.Contains("pupil") || key.Contains("cornea"));
        bool isHair = key.Contains("hair") || isLash || key.Contains("brow");
        bool isMouth = key.Contains("mouth") || key.Contains("lip") || key.Contains("teeth") || key.Contains("tongue");
        bool isHead = key.Contains("head") || key.Contains("face") || key.Contains("neck") || key.Contains("ear");
        bool isArms = key.Contains("arm") || key.Contains("hand");
        bool isLegs = key.Contains("leg") || key.Contains("foot") || key.Contains("thigh") || key.Contains("calf");
        bool isUnderwear = key.Contains("underwear") || key.Contains("bra") || key.Contains("panties");
        bool isShirt = key.Contains("shirt") || key.Contains("upper") || key.Contains("torso") || key.Contains("cloth");
        bool isPants = key.Contains("pants") || key.Contains("trouser") || key.Contains("jean") || key.Contains("lower");
        bool isShoes = key.Contains("shoe") || key.Contains("sneaker") || key.Contains("boot");

        bool slot0 = key.EndsWith("_0") || key.Contains("_0_");
        bool slot1 = key.EndsWith("_1") || key.Contains("_1_");
        bool slot2 = key.EndsWith("_2") || key.Contains("_2_");
        bool slot3 = key.EndsWith("_3") || key.Contains("_3_");
        bool slot4 = key.EndsWith("_4") || key.Contains("_4_");

        if (slot1 && !isEyes && !isMouth)
        {
            return GetImportedMaterial($"{profile}_shirt", new Color(0.18f, 0.24f, 0.34f, 1f), null, 0.2f, 0f);
        }

        if (slot2 && !isEyes && !isMouth)
        {
            return GetImportedMaterial($"{profile}_pants", new Color(0.14f, 0.15f, 0.18f, 1f), null, 0.18f, 0f);
        }

        if (slot3 && !isEyes && !isMouth)
        {
            return GetImportedMaterial($"{profile}_shoes", new Color(0.09f, 0.09f, 0.095f, 1f), null, 0.4f, 0.06f);
        }

        if (slot4 && !isEyes && !isMouth)
        {
            return GetImportedMaterial(
                $"{profile}_hair",
                new Color(0.1f, 0.085f, 0.07f, 1f),
                texturePath: null,
                smoothness: 0.55f,
                metallic: 0f);
        }

        if (isEyes)
        {
            return GetImportedMaterial($"{profile}_eyes", Color.white, eyesTexture, smoothness: 0.9f, metallic: 0f);
        }

        if (isMouth)
        {
            return GetImportedMaterial($"{profile}_mouth", profile == TextureProfile.Antonia ? Color.white : new Color(0.48f, 0.31f, 0.31f, 1f), mouthTexture, smoothness: 0.44f, metallic: 0f);
        }

        if (isHair)
        {
            return GetImportedMaterial(
                $"{profile}_hair",
                new Color(0.1f, 0.085f, 0.07f, 1f),
                texturePath: null,
                smoothness: 0.55f,
                metallic: 0f);
        }

        if (isUnderwear)
        {
            return GetImportedMaterial($"{profile}_underwear", Color.white, AntoniaUnderwearPath, smoothness: 0.34f, metallic: 0f);
        }

        if (isShirt)
        {
            return GetImportedMaterial($"{profile}_shirt", new Color(0.18f, 0.24f, 0.34f, 1f), null, 0.2f, 0f);
        }

        if (isPants)
        {
            return GetImportedMaterial($"{profile}_pants", new Color(0.14f, 0.15f, 0.18f, 1f), null, 0.18f, 0f);
        }

        if (isShoes)
        {
            return GetImportedMaterial($"{profile}_shoes", new Color(0.09f, 0.09f, 0.095f, 1f), null, 0.4f, 0.06f);
        }

        if (isArms)
        {
            return GetImportedMaterial($"{profile}_arms", skinBaseColor, armTexture, smoothness: 0.42f, metallic: 0f);
        }

        if (isLegs)
        {
            return GetImportedMaterial($"{profile}_legs", skinBaseColor, legTexture, smoothness: 0.42f, metallic: 0f);
        }

        if (isHead)
        {
            return GetImportedMaterial($"{profile}_head", skinBaseColor, headTexture, smoothness: 0.44f, metallic: 0f);
        }

        if (key.Contains("ch11_body1"))
        {
            return GetImportedMaterial($"{profile}_ch11_cloth", new Color(0.11f, 0.12f, 0.15f, 1f), null, 0.2f, 0f);
        }

        if (key.Contains("ch11_body"))
        {
            return GetImportedMaterial($"{profile}_ch11_skin", skinBaseColor, skinTexture, smoothness: 0.42f, metallic: 0f);
        }

        if (slot0)
        {
            return GetImportedMaterial($"{profile}_body_slot", skinBaseColor, skinTexture, smoothness: 0.42f, metallic: 0f);
        }

        return GetImportedMaterial($"{profile}_body", skinBaseColor, skinTexture, smoothness: 0.42f, metallic: 0f);
    }

    private static Color ResolveSkinBaseColor(TextureProfile profile)
    {
        return profile switch
        {
            TextureProfile.Antonia => Color.white,
            TextureProfile.MbMale => new Color(0.53f, 0.40f, 0.33f, 1f),
            TextureProfile.Vitruvian => new Color(0.56f, 0.44f, 0.36f, 1f),
            _ => new Color(0.56f, 0.44f, 0.36f, 1f)
        };
    }

    private static Material GetImportedMaterial(string key, Color baseColor, string texturePath, float smoothness, float metallic)
    {
        string normalizedTexturePath = texturePath ?? "none";
        string cacheKey = $"imported_{key}_{normalizedTexturePath}";
        if (fallbackMaterialCache.TryGetValue(cacheKey, out Material cached) && cached != null)
        {
            return cached;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader)
        {
            name = $"SampleScene_{cacheKey}_Mat"
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
        }

        Texture2D albedo = LoadImportedTexture(texturePath);
        Material configured = BuildTexturedLitMaterial(cacheKey, albedo, baseColor, smoothness, metallic, material);
        fallbackMaterialCache[cacheKey] = configured;
        return configured;
    }

    private static Material BuildTexturedLitMaterial(
        string key,
        Texture2D albedo,
        Color baseColor,
        float smoothness,
        float metallic,
        Material targetMaterial = null)
    {
        Material material = targetMaterial;
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                return null;
            }

            material = new Material(shader)
            {
                name = $"SampleScene_{key}_Mat"
            };
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
        }

        Texture2D effectiveAlbedo = albedo ?? GetOrCreateProceduralTexture($"mat_{key}", baseColor, normalMap: false);
        if (effectiveAlbedo != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", effectiveAlbedo);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", effectiveAlbedo);
            }
        }

        Texture2D normalMap = GetOrCreateProceduralTexture($"mat_{key}", Color.gray, normalMap: true);
        if (normalMap != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normalMap);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", Mathf.Clamp01(metallic));
        }

        return material;
    }

#if UNITY_EDITOR
    private static void EnsureModelImporterConfigured(string modelPath)
    {
        if (string.IsNullOrWhiteSpace(modelPath))
        {
            return;
        }

        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null)
        {
            return;
        }

        bool changed = false;

        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }

        if (!importer.importAnimation)
        {
            importer.importAnimation = true;
            changed = true;
        }

        if (importer.materialImportMode == ModelImporterMaterialImportMode.None)
        {
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }
#endif

    private static Texture2D LoadImportedTexture(string texturePath)
    {
        if (string.IsNullOrWhiteSpace(texturePath))
        {
            return null;
        }

        if (importedTextureCache.TryGetValue(texturePath, out Texture2D cached) && cached != null)
        {
            return cached;
        }

#if UNITY_EDITOR
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
#else
        Texture2D texture = null;
#endif
        if (texture != null)
        {
            importedTextureCache[texturePath] = texture;
        }

        return texture;
    }

    private static void FitModelToTargetHeight(Transform visualRoot, float targetHeight)
    {
        if (visualRoot == null || targetHeight <= 0.01f)
        {
            return;
        }

        if (!TryGetModelHeight(visualRoot, out float sourceHeight))
        {
            return;
        }

        float scaleFactor = Mathf.Clamp(targetHeight / sourceHeight, 0.55f, 2.35f);
        visualRoot.localScale *= scaleFactor;
    }

    private static bool TryGetModelHeight(Transform visualRoot, out float sourceHeight)
    {
        sourceHeight = 0f;
        if (visualRoot == null)
        {
            return false;
        }

        if (TryGetHumanoidHeight(visualRoot, out sourceHeight))
        {
            return true;
        }

        if (!TryGetCombinedRendererBounds(visualRoot, out Bounds bounds))
        {
            return false;
        }

        sourceHeight = bounds.size.y;
        return sourceHeight > 0.0001f;
    }

    private static bool TryGetHumanoidHeight(Transform visualRoot, out float sourceHeight)
    {
        sourceHeight = 0f;
        if (visualRoot == null)
        {
            return false;
        }

        Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
        if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.isHuman)
        {
            return false;
        }

        Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
        Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        if (head == null)
        {
            return false;
        }

        float feetY = float.PositiveInfinity;
        if (leftFoot != null)
        {
            feetY = Mathf.Min(feetY, leftFoot.position.y);
        }

        if (rightFoot != null)
        {
            feetY = Mathf.Min(feetY, rightFoot.position.y);
        }

        if (float.IsInfinity(feetY))
        {
            return false;
        }

        sourceHeight = head.position.y - feetY;
        return sourceHeight > 0.1f;
    }

    private static float ResolveActorGroundY(GameObject actorRoot)
    {
        if (actorRoot == null)
        {
            return 0f;
        }

        CharacterController characterController = actorRoot.GetComponent<CharacterController>();
        if (characterController != null)
        {
            return actorRoot.transform.position.y + characterController.center.y - (characterController.height * 0.5f);
        }

        CapsuleCollider capsuleCollider = actorRoot.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            return actorRoot.transform.position.y + capsuleCollider.center.y - (capsuleCollider.height * 0.5f);
        }

        Collider colliderComponent = actorRoot.GetComponent<Collider>();
        if (colliderComponent != null)
        {
            return colliderComponent.bounds.min.y;
        }

        return actorRoot.transform.position.y;
    }

    private static void AlignActorVisualRootsToGround(GameObject actorRoot, float footOffset)
    {
        if (actorRoot == null)
        {
            return;
        }

        float targetGroundY = ResolveActorGroundY(actorRoot) + Mathf.Max(0f, footOffset);
        Transform actorTransform = actorRoot.transform;
        for (int i = 0; i < actorTransform.childCount; i++)
        {
            Transform child = actorTransform.GetChild(i);
            if (child == null || !child.name.StartsWith("Visual_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AlignModelFeetToWorldY(child, targetGroundY);
        }
    }

    private static void AlignModelFeetToWorldY(Transform visualRoot, float targetWorldY)
    {
        if (visualRoot == null)
        {
            return;
        }

        if (!TryGetHumanoidFeetY(visualRoot, out float feetY))
        {
            if (!TryGetCombinedRendererBounds(visualRoot, out Bounds bounds))
            {
                return;
            }

            feetY = bounds.min.y;
        }

        float deltaY = targetWorldY - feetY;
        visualRoot.position += new Vector3(0f, deltaY, 0f);
    }

    private static bool TryGetHumanoidFeetY(Transform visualRoot, out float feetY)
    {
        feetY = 0f;
        if (visualRoot == null)
        {
            return false;
        }

        Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
        if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.isHuman)
        {
            return false;
        }

        Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        float minFootY = float.PositiveInfinity;
        if (leftFoot != null)
        {
            minFootY = Mathf.Min(minFootY, leftFoot.position.y);
        }

        if (rightFoot != null)
        {
            minFootY = Mathf.Min(minFootY, rightFoot.position.y);
        }

        if (float.IsInfinity(minFootY))
        {
            return false;
        }

        feetY = minFootY;
        return true;
    }

    private static bool TryGetCombinedRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;
        bounds = default;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            string rendererName = rendererComponent.name.ToLowerInvariant();
            if (!rendererComponent.enabled ||
                rendererName.Contains("dagger") ||
                rendererName.Contains("sword") ||
                rendererName.Contains("weapon"))
            {
                continue;
            }

            if (!initialized)
            {
                bounds = rendererComponent.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(rendererComponent.bounds);
            }
        }

        return initialized;
    }

    private static void SetLayerRecursive(Transform root, int layer)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursive(root.GetChild(i), layer);
        }
    }

    private static void StripPrimitiveVisualComponents(GameObject actorRoot)
    {
        if (actorRoot == null)
        {
            return;
        }

        RemoveComponent<MeshRenderer>(actorRoot);
        RemoveComponent<MeshFilter>(actorRoot);
    }

    private static void RemoveComponent<T>(GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            return;
        }

        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(component);
        }
        else
        {
            DestroyImmediate(component);
        }
    }

    private static void RemoveLegacyRagdollHierarchy(GameObject actorRoot)
    {
        if (actorRoot == null)
        {
            return;
        }

        RemoveChildByName(actorRoot.transform, "HumanoidRagdoll");
        RemoveChildByName(actorRoot.transform, "HumanoidPoseTargets");
    }

    private static void RemoveChildByName(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrWhiteSpace(childName))
        {
            return;
        }

        Transform child = parent.Find(childName);
        if (child == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(child.gameObject);
        }
        else
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private static void RemoveLegacyVisualRoots(Transform actorRoot, string keepName)
    {
        if (actorRoot == null)
        {
            return;
        }

        for (int i = actorRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = actorRoot.GetChild(i);
            if (child == null || string.Equals(child.name, keepName, StringComparison.Ordinal))
            {
                continue;
            }

            if (!child.name.StartsWith("Visual_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private static bool TryFindNavMeshPosition(Vector3 origin, float maxDistance, out Vector3 navmeshPosition)
    {
        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            navmeshPosition = hit.position;
            return true;
        }

        navmeshPosition = origin;
        return false;
    }

    private static void SnapActorToGround(GameObject actor, CapsuleCollider capsule)
    {
        if (actor == null || capsule == null)
        {
            return;
        }

        Vector3 origin = actor.transform.position + Vector3.up * 5f;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, 20f, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        bool foundGround = false;
        float nearestDistance = float.MaxValue;
        RaycastHit nearestHit = default;
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider == null)
            {
                continue;
            }

            Transform hitTransform = hit.collider.transform;
            if (hitTransform == actor.transform || hitTransform.IsChildOf(actor.transform))
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestHit = hit;
                foundGround = true;
            }
        }

        if (!foundGround)
        {
            return;
        }

        float halfHeight = capsule.height * 0.5f;
        float groundedY = nearestHit.point.y - capsule.center.y + halfHeight;
        actor.transform.position = new Vector3(actor.transform.position.x, groundedY, actor.transform.position.z);
    }

    private static void KeepActorAboveGround(GameObject actor, float minimumClearance)
    {
        if (actor == null)
        {
            return;
        }

        GameObject ground = FindGameObjectInActiveScene(groundObjectName);
        if (ground == null || !ground.TryGetComponent(out Collider groundCollider))
        {
            return;
        }

        float groundTop = groundCollider.bounds.max.y + Mathf.Max(0f, minimumClearance);
        float actorBottom = float.PositiveInfinity;

        CharacterController controller = actor.GetComponent<CharacterController>();
        if (controller != null)
        {
            actorBottom = actor.transform.position.y + controller.center.y - (controller.height * 0.5f);
        }
        else
        {
            CapsuleCollider capsule = actor.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                actorBottom = actor.transform.position.y + capsule.center.y - (capsule.height * 0.5f);
            }
            else if (actor.TryGetComponent(out Collider actorCollider))
            {
                actorBottom = actorCollider.bounds.min.y;
            }
        }

        if (float.IsInfinity(actorBottom))
        {
            return;
        }

        if (actorBottom < groundTop)
        {
            float raiseAmount = groundTop - actorBottom;
            actor.transform.position += new Vector3(0f, raiseAmount, 0f);
        }
    }

    private static void ApplyFallbackCharacterMaterials(Renderer[] renderers)
    {
        ApplyFallbackCharacterMaterials(renderers, false);
    }

    private static void ApplyFallbackCharacterMaterials(Renderer[] renderers, bool forceReplace)
    {
        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Material[] shared = renderer.sharedMaterials;
            if (shared == null || shared.Length == 0)
            {
                Material fallback = ResolveMaterialForRenderer(renderer.name);
                if (fallback != null)
                {
                    renderer.sharedMaterial = fallback;
                }
                continue;
            }

            bool changed = false;
            for (int m = 0; m < shared.Length; m++)
            {
                Material current = shared[m];
                if (!forceReplace && current != null && HasAlbedoTexture(current))
                {
                    continue;
                }

                Material fallback = ResolveMaterialForRenderer(
                    renderer.name,
                    current != null ? current.name : string.Empty,
                    m,
                    shared.Length);
                if (fallback == null)
                {
                    continue;
                }

                shared[m] = fallback;
                changed = true;
            }

            if (changed)
            {
                renderer.sharedMaterials = shared;
            }
        }
    }

    private static bool HasAlbedoTexture(Material material)
    {
        if (material == null)
        {
            return false;
        }

        if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null)
        {
            return true;
        }

        return material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != null;
    }

    private static Material ResolveMaterialForRenderer(
        string rendererName,
        string currentMaterialName = "",
        int slotIndex = -1,
        int slotCount = 0)
    {
        string rendererKey = (rendererName ?? string.Empty).ToLowerInvariant();
        string materialKey = (currentMaterialName ?? string.Empty).ToLowerInvariant();
        string classificationKey = rendererKey + "|" + materialKey;

        bool eyelash = classificationKey.Contains("eyelash") || classificationKey.Contains("_lash") || classificationKey.Contains(" lash");

        if (!eyelash &&
            (classificationKey.Contains("eye") ||
             classificationKey.Contains("iris") ||
             classificationKey.Contains("sclera") ||
             classificationKey.Contains("pupil") ||
             classificationKey.Contains("cornea")))
        {
            return GetFallbackMaterial("eyes", new Color(0.95f, 0.95f, 0.96f, 1f));
        }

        if (classificationKey.Contains("mouth") || classificationKey.Contains("lip") || classificationKey.Contains("teeth") || classificationKey.Contains("tongue"))
        {
            return GetFallbackMaterial("mouth", new Color(0.54f, 0.31f, 0.31f, 1f));
        }

        if (classificationKey.Contains("hair") || classificationKey.Contains("eyelash") || classificationKey.Contains("brow"))
        {
            return GetFallbackMaterial("hair", new Color(0.09f, 0.08f, 0.07f, 1f));
        }

        if (classificationKey.Contains("shoe") || classificationKey.Contains("sneaker") || classificationKey.Contains("boot") || classificationKey.Contains("footwear"))
        {
            return GetFallbackMaterial("shoe", new Color(0.07f, 0.07f, 0.08f, 1f));
        }

        if (classificationKey.Contains("sock"))
        {
            return GetFallbackMaterial("socks", new Color(0.82f, 0.82f, 0.84f, 1f));
        }

        if (classificationKey.Contains("shirt") || classificationKey.Contains("upper") || classificationKey.Contains("cloth") || classificationKey.Contains("torso"))
        {
            return GetFallbackMaterial("shirt", new Color(0.17f, 0.23f, 0.34f, 1f));
        }

        if (classificationKey.Contains("pants") || classificationKey.Contains("trousers") || classificationKey.Contains("jeans") || classificationKey.Contains("lower"))
        {
            return GetFallbackMaterial("pants", new Color(0.14f, 0.15f, 0.18f, 1f));
        }

        if (classificationKey.Contains("underwear") || classificationKey.Contains("bra") || classificationKey.Contains("panties"))
        {
            return GetFallbackMaterial("underwear", new Color(0.80f, 0.80f, 0.82f, 1f));
        }

        if (classificationKey.Contains("ch11_body1"))
        {
            return GetFallbackMaterial("shirt", new Color(0.17f, 0.23f, 0.34f, 1f));
        }

        if (classificationKey.Contains("skin") || classificationKey.Contains("body") || classificationKey.Contains("head") || classificationKey.Contains("face"))
        {
            return GetFallbackMaterial("body", new Color(0.72f, 0.58f, 0.49f, 1f));
        }

        // Last-resort heuristic for models with anonymous slots.
        if (slotCount > 1)
        {
            if (slotCount == 2)
            {
                return slotIndex == 0
                    ? GetFallbackMaterial("body", new Color(0.72f, 0.58f, 0.49f, 1f))
                    : GetFallbackMaterial("shirt", new Color(0.17f, 0.23f, 0.34f, 1f));
            }

            if (slotIndex == 1)
            {
                return GetFallbackMaterial("shirt", new Color(0.17f, 0.23f, 0.34f, 1f));
            }

            if (slotIndex >= 2)
            {
                return GetFallbackMaterial("pants", new Color(0.14f, 0.15f, 0.18f, 1f));
            }
        }

        return GetFallbackMaterial("body", new Color(0.72f, 0.58f, 0.49f, 1f));
    }

    private static Material GetFallbackMaterial(string key, Color baseColor)
    {
        if (fallbackMaterialCache.TryGetValue(key, out Material cached) && cached != null)
        {
            return cached;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader)
        {
            name = $"SampleScene_{key}_Mat"
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
        }

        bool eyeMaterial = key != null && key.IndexOf("eye", StringComparison.OrdinalIgnoreCase) >= 0;
        Texture2D baseMap = eyeMaterial
            ? GetOrCreateEyeTexture(key)
            : GetOrCreateProceduralTexture(key, baseColor, normalMap: false);
        if (baseMap != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", baseMap);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", baseMap);
            }
        }

        Texture2D normalMap = GetOrCreateProceduralTexture(key, Color.gray, normalMap: true);
        if (normalMap != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normalMap);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", ResolveSmoothnessForKey(key));
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", ResolveMetallicForKey(key));
        }

        fallbackMaterialCache[key] = material;
        return material;
    }

    private static Texture2D GetOrCreateEyeTexture(string key)
    {
        string cacheKey = $"{key}_Eye_C";
        if (fallbackTextureCache.TryGetValue(cacheKey, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            name = $"SampleScene_{cacheKey}_Tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Vector2 center = new Vector2(0.5f, 0.5f);
        const float irisRadius = 0.23f;
        const float pupilRadius = 0.09f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);
                float v = y / (float)(size - 1);
                Vector2 uv = new Vector2(u, v);
                float d = Vector2.Distance(uv, center);

                Color color = new Color(0.93f, 0.94f, 0.95f, 1f); // sclera
                float vein = Mathf.PerlinNoise((u + 0.1f) * 18f, (v + 0.3f) * 18f);
                color *= Mathf.Lerp(0.95f, 1.02f, vein);

                if (d <= irisRadius)
                {
                    float irisNoise = Mathf.PerlinNoise((u + 0.2f) * 46f, (v + 0.4f) * 46f);
                    Color iris = new Color(0.24f, 0.31f, 0.38f, 1f) * Mathf.Lerp(0.72f, 1.25f, irisNoise);
                    color = Color.Lerp(color, iris, 0.9f);
                }

                if (d <= pupilRadius)
                {
                    color = new Color(0.03f, 0.03f, 0.03f, 1f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        fallbackTextureCache[cacheKey] = texture;
        return texture;
    }

    private static float ResolveSmoothnessForKey(string key)
    {
        if (key.Contains("interior_floor_wood") || key.Contains("interior_trim"))
        {
            return 0.34f;
        }

        if (key.Contains("interior_wall"))
        {
            return 0.12f;
        }

        if (key.Contains("interior_fabric") || key.Contains("interior_rug"))
        {
            return 0.08f;
        }

        if (key.Contains("interior_blood"))
        {
            return 0.42f;
        }

        if (key.Contains("interior_hazard"))
        {
            return 0.26f;
        }

        if (key.Contains("interior_glass"))
        {
            return 0.78f;
        }

        if (key.Contains("interior_metal"))
        {
            return 0.62f;
        }

        if (key.Contains("skin") || key.Contains("body"))
        {
            return 0.46f;
        }

        if (key.Contains("hair"))
        {
            return 0.72f;
        }

        if (key.Contains("shoe"))
        {
            return 0.58f;
        }

        return 0.28f;
    }

    private static float ResolveMetallicForKey(string key)
    {
        if (key.Contains("interior_metal"))
        {
            return 0.55f;
        }

        if (key.Contains("interior_glass"))
        {
            return 0.06f;
        }

        if (key.Contains("interior_hazard"))
        {
            return 0.04f;
        }

        if (key.Contains("shoe"))
        {
            return 0.12f;
        }

        return 0f;
    }

    private static Texture2D GetOrCreateProceduralTexture(string key, Color baseColor, bool normalMap)
    {
        string cacheKey = $"{key}_{(normalMap ? "N" : "C")}";
        if (fallbackTextureCache.TryGetValue(cacheKey, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            name = $"SampleScene_{cacheKey}_Tex",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        bool woodLike = key.Contains("interior_floor_wood") || key.Contains("interior_trim") || key.Contains("interior_accent");
        bool wallLike = key.Contains("interior_wall") || key.Contains("interior_concrete");
        bool fabricLike = key.Contains("interior_fabric") || key.Contains("interior_rug");
        bool bloodLike = key.Contains("interior_blood");
        bool hazardLike = key.Contains("interior_hazard");
        float noiseScale = normalMap ? 8.5f : 2.2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);
                float v = y / (float)(size - 1);

                if (normalMap)
                {
                    float nx = (Mathf.PerlinNoise((u + 0.11f) * noiseScale * 2.4f, (v + 0.37f) * noiseScale * 2.4f) - 0.5f) * 0.14f;
                    float ny = (Mathf.PerlinNoise((u + 0.73f) * noiseScale * 2.4f, (v + 0.09f) * noiseScale * 2.4f) - 0.5f) * 0.14f;
                    if (woodLike)
                    {
                        float grain = Mathf.Sin((u * 42f) + (Mathf.PerlinNoise(v * 6f, u * 2f) * 6f));
                        nx += grain * 0.03f;
                    }
                    else if (fabricLike)
                    {
                        float weaveX = Mathf.Sin(u * Mathf.PI * 72f) * 0.018f;
                        float weaveY = Mathf.Sin(v * Mathf.PI * 72f) * 0.018f;
                        nx += weaveX;
                        ny += weaveY;
                    }
                    else if (bloodLike)
                    {
                        float clotNoise = Mathf.PerlinNoise((u + 0.13f) * 16f, (v + 0.47f) * 16f);
                        nx += (clotNoise - 0.5f) * 0.08f;
                        ny += (clotNoise - 0.5f) * 0.08f;
                    }
                    else if (hazardLike)
                    {
                        float stripe = Mathf.Sin((u + v) * Mathf.PI * 22f);
                        nx += stripe * 0.02f;
                        ny -= stripe * 0.02f;
                    }

                    Vector3 n = new Vector3(nx, ny, 1f).normalized;
                    texture.SetPixel(x, y, new Color((n.x * 0.5f) + 0.5f, (n.y * 0.5f) + 0.5f, (n.z * 0.5f) + 0.5f, 1f));
                }
                else
                {
                    float lowNoise = Mathf.PerlinNoise((u + 0.17f) * noiseScale, (v + 0.31f) * noiseScale);
                    float shade = Mathf.Lerp(0.97f, 1.03f, lowNoise);
                    if (woodLike)
                    {
                        float plankBand = Mathf.Floor(u * 9f) * 0.022f;
                        float grain = Mathf.Sin((u * 48f) + (Mathf.PerlinNoise(v * 7f, u * 3f) * 4f)) * 0.06f;
                        shade = 0.88f + plankBand + grain + (lowNoise * 0.06f);
                    }
                    else if (wallLike)
                    {
                        float plasterNoise = Mathf.PerlinNoise((u + 0.44f) * 4.5f, (v + 0.29f) * 4.5f);
                        shade = Mathf.Lerp(0.92f, 1.04f, plasterNoise);
                    }
                    else if (fabricLike)
                    {
                        float weaveX = Mathf.Abs(Mathf.Sin(u * Mathf.PI * 64f));
                        float weaveY = Mathf.Abs(Mathf.Sin(v * Mathf.PI * 64f));
                        shade = 0.9f + ((weaveX * 0.04f) + (weaveY * 0.04f)) + ((lowNoise - 0.5f) * 0.04f);
                    }
                    else if (bloodLike)
                    {
                        float clotNoise = Mathf.PerlinNoise((u + 0.51f) * 9.5f, (v + 0.23f) * 9.5f);
                        float streak = Mathf.PerlinNoise((u + 0.17f) * 22f, (v + 0.79f) * 5.5f);
                        shade = Mathf.Lerp(0.52f, 1.16f, clotNoise) + ((streak - 0.5f) * 0.16f);
                    }
                    else if (hazardLike)
                    {
                        float stripe = Mathf.Sin((u + (v * 0.94f)) * Mathf.PI * 20f);
                        shade = stripe > 0f ? 1.08f : 0.26f;
                        shade += (lowNoise - 0.5f) * 0.06f;
                    }

                    Color color = new Color(
                        Mathf.Clamp01(baseColor.r * shade),
                        Mathf.Clamp01(baseColor.g * shade),
                        Mathf.Clamp01(baseColor.b * shade),
                        1f);
                    texture.SetPixel(x, y, color);
                }
            }
        }

        texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        fallbackTextureCache[cacheKey] = texture;
        return texture;
    }

    private static void ResetRuntimeMaterialCaches()
    {
        foreach (Material material in fallbackMaterialCache.Values)
        {
            if (material == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }

        foreach (Texture2D texture in fallbackTextureCache.Values)
        {
            if (texture == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }

        fallbackMaterialCache.Clear();
        fallbackTextureCache.Clear();
        importedTextureCache.Clear();
    }


    private static void EnsureRuntimeGraphicsQualityController()
    {
        RuntimeGraphicsQualityController controller = FindInActiveScene<RuntimeGraphicsQualityController>();
        if (controller != null)
        {
            return;
        }

        GameObject host = new GameObject("RuntimeGraphicsQuality");
        host.hideFlags = HideFlags.DontSave;
        host.AddComponent<RuntimeGraphicsQualityController>();
    }
    private static void HideLegacySampleTextArtifact()
    {
        DisableObjectByName("Text (TMP)");
        DisableObjectByName("New Text");
    }

    private static void DisableObjectByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return;
        }

        GameObject sceneObject = FindGameObjectInActiveScene(objectName, includeInactive: true);
        if (sceneObject != null)
        {
            sceneObject.SetActive(false);
        }
    }

    private static T FindInActiveScene<T>(FindObjectsInactive includeInactive = FindObjectsInactive.Exclude) where T : Component
    {
        Scene activeScene = SceneManager.GetActiveScene();
        T[] components = UnityEngine.Object.FindObjectsByType<T>(includeInactive);
        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];
            if (component != null && component.gameObject.scene == activeScene)
            {
                return component;
            }
        }

        return null;
    }

    private static GameObject FindGameObjectInActiveScene(string objectName, bool includeInactive = false)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform == null || !string.Equals(transform.name, objectName, StringComparison.Ordinal))
            {
                continue;
            }

            if (transform.gameObject.scene == activeScene)
            {
                return transform.gameObject;
            }
        }

        return null;
    }
}

[DisallowMultipleComponent]
public class SampleSceneAmbientSoundscape : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float baseVolume = 0.16f;
    [SerializeField] [Range(0f, 1f)] private float modulationDepth = 0.12f;
    [SerializeField] private float modulationSpeed = 0.42f;

    private AudioSource lowHumSource;
    private AudioSource highBuzzSource;
    private AudioClip lowHumClip;
    private AudioClip highBuzzClip;

    public void Configure(float volume, float depth)
    {
        baseVolume = Mathf.Clamp01(volume);
        modulationDepth = Mathf.Clamp01(depth);
        EnsureSources();
    }

    private void Awake()
    {
        EnsureSources();
    }

    private void OnEnable()
    {
        EnsureSources();
        if (lowHumSource != null && !lowHumSource.isPlaying)
        {
            lowHumSource.Play();
        }

        if (highBuzzSource != null && !highBuzzSource.isPlaying)
        {
            highBuzzSource.Play();
        }
    }

    private void Update()
    {
        if (lowHumSource == null || highBuzzSource == null)
        {
            return;
        }

        float pulse = 0.5f + (Mathf.Sin(Time.time * modulationSpeed) * 0.5f);
        float swing = 1f - (modulationDepth * pulse);

        lowHumSource.volume = baseVolume * swing;
        highBuzzSource.volume = (baseVolume * 0.38f) * Mathf.Lerp(0.72f, 1f, pulse);
    }

    private void EnsureSources()
    {
        if (lowHumSource == null)
        {
            GameObject lowGo = new GameObject("AmbientLowHum");
            lowGo.transform.SetParent(transform, false);
            lowHumSource = lowGo.AddComponent<AudioSource>();
            lowHumSource.loop = true;
            lowHumSource.playOnAwake = false;
            lowHumSource.spatialBlend = 0f;
            lowHumSource.rolloffMode = AudioRolloffMode.Linear;
        }

        if (highBuzzSource == null)
        {
            GameObject highGo = new GameObject("AmbientHighBuzz");
            highGo.transform.SetParent(transform, false);
            highBuzzSource = highGo.AddComponent<AudioSource>();
            highBuzzSource.loop = true;
            highBuzzSource.playOnAwake = false;
            highBuzzSource.spatialBlend = 0f;
            highBuzzSource.rolloffMode = AudioRolloffMode.Linear;
        }

        if (lowHumClip == null)
        {
            lowHumClip = BuildLoopClip("SampleScene_AmbientLowHum", 22050, 1.75f, 46f, 52f, 0.06f);
            lowHumSource.clip = lowHumClip;
        }

        if (highBuzzClip == null)
        {
            highBuzzClip = BuildLoopClip("SampleScene_AmbientHighBuzz", 22050, 1.15f, 140f, 168f, 0.14f);
            highBuzzSource.clip = highBuzzClip;
        }
    }

    private static AudioClip BuildLoopClip(
        string clipName,
        int sampleRate,
        float lengthSeconds,
        float baseFrequency,
        float harmonicFrequency,
        float noiseMix)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * Mathf.Max(0.2f, lengthSeconds)));
        float[] samples = new float[sampleCount];
        float basePhase = 0f;
        float harmonicPhase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            basePhase += (baseFrequency * Mathf.PI * 2f) / sampleRate;
            harmonicPhase += (harmonicFrequency * Mathf.PI * 2f) / sampleRate;
            float tone = (Mathf.Sin(basePhase) * 0.65f) + (Mathf.Sin(harmonicPhase) * 0.35f);
            float wobble = Mathf.Sin(t * 1.7f) * 0.12f;
            float noise = ((UnityEngine.Random.value * 2f) - 1f) * noiseMix;
            samples[i] = (tone + wobble + noise) * 0.22f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

[DisallowMultipleComponent]
public class SampleSceneFirstPersonPresentation : MonoBehaviour
{
    private const string ViewmodelPistolAssetPath = "Assets/Game/Art/Environment/Kits/SciFiEssentials/FBX/Gun_Pistol.fbx";
    private const string ViewmodelRevolverAssetPath = "Assets/Game/Art/Environment/Kits/SciFiEssentials/FBX/Gun_Revolver.fbx";
    private const string ViewmodelRifleAssetPath = "Assets/Game/Art/Environment/Kits/SciFiEssentials/FBX/Gun_Rifle.fbx";
    private const string ViewmodelSmgAssetPath = "Assets/Game/Art/Environment/Kits/SciFiEssentials/FBX/Gun_SMG_Ammo.fbx";
    private const string ViewmodelSniperAssetPath = "Assets/Game/Art/Environment/Kits/SciFiEssentials/FBX/Gun_Sniper.fbx";
    private const string ViewmodelArmsModelPath = "Assets/Game/Art/Characters/Source/Ch01_nonPBR@Double Dagger Stab.fbx";

    [Header("References")]
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private RealTimeCombat combat;

    [Header("Viewmodel Motion")]
    [SerializeField] private Vector3 baseLocalPosition = new Vector3(0.19f, -0.22f, 0.44f);
    [SerializeField] private Vector3 baseLocalEuler = new Vector3(12f, -20f, 2f);
    [SerializeField] private float bobAmplitude = 0.012f;
    [SerializeField] private float bobFrequency = 9.5f;
    [SerializeField] private float swayAmount = 0.018f;
    [SerializeField] private float swayRotation = 2.2f;
    [SerializeField] private float recoilKick = 0.065f;
    [SerializeField] private float recoilRotation = 6f;
    [SerializeField] private float settleSpeed = 10f;

    [Header("Viewmodel Source")]
    [SerializeField] private bool useRiggedArms = false;
    [SerializeField] private Vector3 weaponOnlyLocalPosition = new Vector3(0.10f, -0.17f, 0.35f);
    [SerializeField] private Vector3 weaponOnlyLocalEuler = new Vector3(-6f, -4f, 0f);
    [SerializeField] private Vector3 weaponOnlyLocalScale = new Vector3(0.85f, 0.85f, 0.85f);

    [Header("Rigged Hold Pose")]
    [SerializeField] private Vector3 riggedArmsLocalPosition = new Vector3(0.05f, -1.17f, 0.13f);
    [SerializeField] private Vector3 riggedArmsLocalEuler = new Vector3(2f, 180f, 0f);
    [SerializeField] private float riggedArmsScale = 1.08f;
    [SerializeField] private Vector3 rightUpperArmHoldEuler = new Vector3(-18f, 40f, 31f);
    [SerializeField] private Vector3 rightForearmHoldEuler = new Vector3(-63f, 6f, 16f);
    [SerializeField] private Vector3 rightHandHoldEuler = new Vector3(-16f, 6f, 12f);
    [SerializeField] private Vector3 leftUpperArmHoldEuler = new Vector3(-20f, -33f, -20f);
    [SerializeField] private Vector3 leftForearmHoldEuler = new Vector3(-56f, -12f, -10f);
    [SerializeField] private Vector3 leftHandHoldEuler = new Vector3(-18f, -3f, -22f);
    [SerializeField] private float holdPoseBlendSpeed = 14f;
    [SerializeField] private Vector3 rightHandWeaponLocalPosition = new Vector3(0.03f, -0.03f, 0.13f);
    [SerializeField] private Vector3 rightHandWeaponLocalEuler = new Vector3(-76f, 99f, 89f);
    [SerializeField] private Vector3 rightHandWeaponLocalScale = new Vector3(1.32f, 1.32f, 1.32f);

    [Header("Audio")]
    [SerializeField] private float fireVolume = 0.28f;
    [SerializeField] private float pitchJitter = 0.06f;

    private Transform viewmodelRoot;
    private Transform rightUpperArmBone;
    private Transform rightForearmBone;
    private Transform rightHandBone;
    private Transform leftUpperArmBone;
    private Transform leftForearmBone;
    private Transform leftHandBone;
    private Quaternion rightUpperArmBaseRotation;
    private Quaternion rightForearmBaseRotation;
    private Quaternion rightHandBaseRotation;
    private Quaternion leftUpperArmBaseRotation;
    private Quaternion leftForearmBaseRotation;
    private Quaternion leftHandBaseRotation;
    private bool hasRiggedHands;
    private Renderer[] cachedPlayerBodyRenderers = Array.Empty<Renderer>();
    private bool[] cachedPlayerBodyInitialEnabled = Array.Empty<bool>();
    private bool playerBodyCacheReady;
    private bool playerBodyHidden;

    private AudioSource audioSource;
    private AudioClip fireClip;
    private float bobPhase;
    private float recoilState;
    private float recoilVelocity;
    private bool eventsBound;

    public void Configure(HolstinCameraRig rig, PlayerMover mover, RealTimeCombat combatController)
    {
        cameraRig = rig;
        playerMover = mover;
        combat = combatController;
        TryBindCombatEvents();
    }

    private void Awake()
    {
        ResolveReferences();
        BuildViewmodelIfNeeded();
        TryBindCombatEvents();
    }

    private void OnEnable()
    {
        TryBindCombatEvents();
    }

    private void OnDisable()
    {
        if (eventsBound && combat != null)
        {
            combat.Fired -= OnCombatFired;
            eventsBound = false;
        }

        SetPlayerBodyHidden(false);
    }

    private void Update()
    {
        ResolveReferences();
        BuildViewmodelIfNeeded();
        TryBindCombatEvents();
        UpdateViewmodelVisibility();
        UpdateViewmodelPose();
    }

    private void ResolveReferences()
    {
        if (cameraRig == null)
        {
            cameraRig = FindAnyObjectByType<HolstinCameraRig>();
        }

        if (playerMover == null)
        {
            playerMover = FindAnyObjectByType<PlayerMover>();
        }

        if (combat == null)
        {
            combat = FindAnyObjectByType<RealTimeCombat>();
        }
    }

    private void TryBindCombatEvents()
    {
        if (combat == null || eventsBound)
        {
            return;
        }

        combat.Fired += OnCombatFired;
        eventsBound = true;
    }

    private void BuildViewmodelIfNeeded()
    {
        if (viewmodelRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject("FP_Viewmodel");
        viewmodelRoot = rootObject.transform;
        viewmodelRoot.SetParent(transform, false);
        viewmodelRoot.localPosition = baseLocalPosition;
        viewmodelRoot.localRotation = Quaternion.Euler(baseLocalEuler);

        Material weaponMaterial = CreateRuntimeMaterial("fp_weapon", new Color(0.20f, 0.21f, 0.23f, 1f), 0.36f, 0.26f);
        Material accentMaterial = CreateRuntimeMaterial("fp_weapon_accent", new Color(0.33f, 0.03f, 0.03f, 1f), 0.28f, 0.1f);

        Transform weapon = null;
        hasRiggedHands = false;
        if (useRiggedArms)
        {
            Material skinMaterial = CreateRuntimeMaterial("fp_skin", new Color(0.64f, 0.50f, 0.42f, 1f), 0.42f, 0f);
            Material sleeveMaterial = CreateRuntimeMaterial("fp_sleeve", new Color(0.10f, 0.12f, 0.16f, 1f), 0.24f, 0f);
            weapon = BuildRiggedHandsAndWeapon(skinMaterial, sleeveMaterial, weaponMaterial);
            hasRiggedHands = weapon != null && rightHandBone != null && leftHandBone != null;
        }

        if (weapon == null)
        {
            weapon = BuildWeaponOnlyViewmodel(weaponMaterial);
        }

        if (weapon != null)
        {
            CreateCube("FP_MuzzleAccent", weapon, new Vector3(0f, 0.02f, 0.18f), Vector3.zero, new Vector3(0.02f, 0.02f, 0.04f), accentMaterial);
        }

        EnsureAudioSource();
    }

    private Transform BuildRiggedHandsAndWeapon(Material skinMaterial, Material sleeveMaterial, Material weaponMaterial)
    {
        _ = skinMaterial;
        _ = sleeveMaterial;
#if UNITY_EDITOR
        EnsureModelReadableForViewmodelAsset(ViewmodelArmsModelPath);
        GameObject armsAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ViewmodelArmsModelPath);
        if (armsAsset == null)
        {
            return null;
        }

        GameObject armsInstance = UnityEngine.Object.Instantiate(armsAsset, viewmodelRoot);
        armsInstance.name = "FP_RiggedArms";
        armsInstance.transform.localPosition = riggedArmsLocalPosition;
        armsInstance.transform.localRotation = Quaternion.Euler(riggedArmsLocalEuler);
        armsInstance.transform.localScale = Vector3.one * Mathf.Max(0.6f, riggedArmsScale);

        DisableModelPhysics(armsInstance);
        if (!TryResolveArmBones(armsInstance, out Animator armAnimator))
        {
            UnityEngine.Object.Destroy(armsInstance);
            return null;
        }

        if (armAnimator != null)
        {
            armAnimator.applyRootMotion = false;
            armAnimator.enabled = false;
        }

        bool anyArmsMesh = TrimToArmAndHandGeometryOnly(armsInstance, skinMaterial, sleeveMaterial);
        if (!anyArmsMesh)
        {
            UnityEngine.Object.Destroy(armsInstance);
            return null;
        }

        CacheArmBaseRotations();
        Transform weaponMount = EnsureChildTransform(rightHandBone, "FP_WeaponMount");
        weaponMount.localPosition = rightHandWeaponLocalPosition;
        weaponMount.localRotation = Quaternion.Euler(rightHandWeaponLocalEuler);
        weaponMount.localScale = Vector3.one;

        Transform weapon = TryInstantiateBestWeaponViewmodel(weaponMount, rightHandWeaponLocalScale, weaponMaterial);

        return weapon;
#else
        _ = weaponMaterial;
        return null;
#endif
    }

    private Transform BuildWeaponOnlyViewmodel(Material weaponMaterial)
    {
        Transform weaponMount = EnsureChildTransform(viewmodelRoot, "FP_WeaponOnlyMount");
        if (weaponMount == null)
        {
            return null;
        }

        weaponMount.localPosition = weaponOnlyLocalPosition;
        weaponMount.localRotation = Quaternion.Euler(weaponOnlyLocalEuler);
        weaponMount.localScale = Vector3.one;

        Transform weapon = TryInstantiateBestWeaponViewmodel(weaponMount, weaponOnlyLocalScale, weaponMaterial);
        if (weapon == null)
        {
            weapon = CreateCube("FP_WeaponBody", weaponMount, Vector3.zero, Vector3.zero, new Vector3(0.09f, 0.07f, 0.30f), weaponMaterial);
            CreateCube("FP_WeaponBarrel", weapon, new Vector3(0f, 0.005f, 0.17f), Vector3.zero, new Vector3(0.03f, 0.03f, 0.14f), weaponMaterial);
        }

        return weapon;
    }

    private void EnsureAudioSource()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = fireVolume;
        fireClip = BuildProceduralFireClip();
    }

    private void UpdateViewmodelVisibility()
    {
        if (viewmodelRoot == null)
        {
            return;
        }

        float fpBlend = cameraRig != null ? cameraRig.FirstPersonBlend : 0f;
        bool visible = fpBlend > 0.22f && !GameplayPauseFacade.IsPaused;
        if (viewmodelRoot.gameObject.activeSelf != visible)
        {
            viewmodelRoot.gameObject.SetActive(visible);
        }

        SetPlayerBodyHidden(fpBlend > 0.08f);
    }

    private void UpdateViewmodelPose()
    {
        if (viewmodelRoot == null || !viewmodelRoot.gameObject.activeSelf)
        {
            return;
        }

        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        float speed = playerMover != null ? playerMover.CurrentPlanarSpeed : 0f;
        float moveWeight = Mathf.Clamp01(speed / 4f);
        bobPhase += dt * Mathf.Lerp(2f, bobFrequency, moveWeight);

        Vector2 look = InputReader.GetLookDelta() * 0.0022f;
        Vector3 bobOffset = new Vector3(
            Mathf.Sin(bobPhase * 0.5f) * bobAmplitude * moveWeight,
            Mathf.Abs(Mathf.Cos(bobPhase)) * bobAmplitude * moveWeight,
            0f);

        recoilState = Mathf.SmoothDamp(recoilState, 0f, ref recoilVelocity, 1f / Mathf.Max(0.01f, settleSpeed), Mathf.Infinity, dt);
        Vector3 recoilOffset = new Vector3(0f, 0f, -recoilState * recoilKick);
        Vector3 localPos = baseLocalPosition + bobOffset + recoilOffset;

        float swayYaw = -look.x * swayRotation;
        float swayPitch = look.y * swayRotation;
        Vector3 localEuler = baseLocalEuler + new Vector3(swayPitch - (recoilState * recoilRotation), swayYaw, look.x * swayAmount * 60f);

        viewmodelRoot.localPosition = Vector3.Lerp(viewmodelRoot.localPosition, localPos, dt * settleSpeed);
        viewmodelRoot.localRotation = Quaternion.Slerp(viewmodelRoot.localRotation, Quaternion.Euler(localEuler), dt * settleSpeed);

        if (hasRiggedHands)
        {
            ApplyRiggedHoldPose(dt);
        }
    }

    private void OnCombatFired()
    {
        recoilState = Mathf.Clamp01(recoilState + 1f);
        if (audioSource != null && fireClip != null)
        {
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchJitter, pitchJitter);
            audioSource.PlayOneShot(fireClip, fireVolume);
        }
    }

    private static Material CreateRuntimeMaterial(string key, Color color, float smoothness, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader) { name = $"SampleScene_{key}_Mat" };
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        return material;
    }

    private void ApplyRiggedHoldPose(float dt)
    {
        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, holdPoseBlendSpeed) * dt);
        if (rightUpperArmBone != null)
        {
            rightUpperArmBone.localRotation = Quaternion.Slerp(
                rightUpperArmBone.localRotation,
                rightUpperArmBaseRotation * Quaternion.Euler(rightUpperArmHoldEuler),
                blend);
        }

        if (rightForearmBone != null)
        {
            rightForearmBone.localRotation = Quaternion.Slerp(
                rightForearmBone.localRotation,
                rightForearmBaseRotation * Quaternion.Euler(rightForearmHoldEuler),
                blend);
        }

        if (rightHandBone != null)
        {
            rightHandBone.localRotation = Quaternion.Slerp(
                rightHandBone.localRotation,
                rightHandBaseRotation * Quaternion.Euler(rightHandHoldEuler),
                blend);
        }

        if (leftUpperArmBone != null)
        {
            leftUpperArmBone.localRotation = Quaternion.Slerp(
                leftUpperArmBone.localRotation,
                leftUpperArmBaseRotation * Quaternion.Euler(leftUpperArmHoldEuler),
                blend);
        }

        if (leftForearmBone != null)
        {
            leftForearmBone.localRotation = Quaternion.Slerp(
                leftForearmBone.localRotation,
                leftForearmBaseRotation * Quaternion.Euler(leftForearmHoldEuler),
                blend);
        }

        if (leftHandBone != null)
        {
            leftHandBone.localRotation = Quaternion.Slerp(
                leftHandBone.localRotation,
                leftHandBaseRotation * Quaternion.Euler(leftHandHoldEuler),
                blend);
        }
    }

    private static Transform CreateCube(string name, Transform parent, Vector3 localPosition, Vector3 localEuler, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localRotation = Quaternion.Euler(localEuler);
        cube.transform.localScale = localScale;
        if (cube.TryGetComponent(out Collider collider))
        {
            UnityEngine.Object.Destroy(collider);
        }

        if (cube.TryGetComponent(out MeshRenderer renderer) && material != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        return cube.transform;
    }

#if UNITY_EDITOR
    private static void EnsureModelReadableForViewmodelAsset(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return;
        }

        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        if (importer is not ModelImporter modelImporter)
        {
            return;
        }

        if (modelImporter.isReadable)
        {
            return;
        }

        modelImporter.isReadable = true;
        modelImporter.SaveAndReimport();
    }

    private static Transform TryInstantiateBestWeaponViewmodel(Transform parent, Vector3 localScale, Material fallbackMaterial)
    {
        if (parent == null)
        {
            return null;
        }

        string[] candidates =
        {
            ViewmodelRevolverAssetPath,
            ViewmodelPistolAssetPath,
            ViewmodelSmgAssetPath,
            ViewmodelRifleAssetPath,
            ViewmodelSniperAssetPath
        };

        for (int i = 0; i < candidates.Length; i++)
        {
            string path = candidates[i];
            Transform weapon = TryInstantiateViewmodelAsset(
                $"FP_WeaponModel_{i}",
                path,
                parent,
                Vector3.zero,
                Vector3.zero,
                localScale,
                fallbackMaterial);
            if (weapon != null)
            {
                return weapon;
            }
        }

        return null;
    }

    private bool TryResolveArmBones(GameObject armRoot, out Animator animator)
    {
        animator = armRoot != null ? armRoot.GetComponentInChildren<Animator>(true) : null;
        if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.isHuman)
        {
            return false;
        }

        rightUpperArmBone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        rightForearmBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        rightHandBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
        leftUpperArmBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        leftForearmBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        leftHandBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);

        return rightUpperArmBone != null &&
               rightForearmBone != null &&
               rightHandBone != null &&
               leftUpperArmBone != null &&
               leftForearmBone != null &&
               leftHandBone != null;
    }

    private void CacheArmBaseRotations()
    {
        rightUpperArmBaseRotation = rightUpperArmBone != null ? rightUpperArmBone.localRotation : Quaternion.identity;
        rightForearmBaseRotation = rightForearmBone != null ? rightForearmBone.localRotation : Quaternion.identity;
        rightHandBaseRotation = rightHandBone != null ? rightHandBone.localRotation : Quaternion.identity;
        leftUpperArmBaseRotation = leftUpperArmBone != null ? leftUpperArmBone.localRotation : Quaternion.identity;
        leftForearmBaseRotation = leftForearmBone != null ? leftForearmBone.localRotation : Quaternion.identity;
        leftHandBaseRotation = leftHandBone != null ? leftHandBone.localRotation : Quaternion.identity;
    }

    private static bool TrimToArmAndHandGeometryOnly(GameObject armRoot, Material skinMaterial, Material sleeveMaterial)
    {
        if (armRoot == null)
        {
            return false;
        }

        bool keptAny = false;
        SkinnedMeshRenderer[] skinRenderers = armRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinRenderers.Length; i++)
        {
            SkinnedMeshRenderer skinRenderer = skinRenderers[i];
            if (skinRenderer == null || skinRenderer.sharedMesh == null)
            {
                continue;
            }

            Mesh trimmed = CreateArmOnlyMesh(skinRenderer.sharedMesh, skinRenderer.bones);
            if (GetMeshTriangleCountSafe(trimmed) < 3)
            {
                skinRenderer.enabled = false;
                continue;
            }

            skinRenderer.sharedMesh = trimmed;
            skinRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            skinRenderer.receiveShadows = false;
            skinRenderer.updateWhenOffscreen = true;
            skinRenderer.quality = SkinQuality.Auto;

            Material guessed = GuessArmMaterial(skinRenderer, skinMaterial, sleeveMaterial);
            if (guessed != null)
            {
                skinRenderer.sharedMaterial = guessed;
            }

            keptAny = true;
        }

        Renderer[] allRenderers = armRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer renderer = allRenderers[i];
            if (renderer == null || renderer is SkinnedMeshRenderer)
            {
                continue;
            }

            renderer.enabled = false;
        }

        return keptAny;
    }

    private static Mesh CreateArmOnlyMesh(Mesh source, Transform[] bones)
    {
        if (source == null || bones == null || bones.Length == 0)
        {
            return null;
        }

        if (!source.isReadable)
        {
            return null;
        }

        BoneWeight[] boneWeights = source.boneWeights;
        if (boneWeights == null || boneWeights.Length != source.vertexCount)
        {
            return null;
        }

        HashSet<int> armBoneIndices = new HashSet<int>();
        for (int i = 0; i < bones.Length; i++)
        {
            Transform bone = bones[i];
            if (bone == null)
            {
                continue;
            }

            if (IsArmBoneName(bone.name))
            {
                armBoneIndices.Add(i);
            }
        }

        if (armBoneIndices.Count == 0)
        {
            return null;
        }

        float[] armInfluence = new float[source.vertexCount];
        for (int i = 0; i < source.vertexCount; i++)
        {
            BoneWeight bw = boneWeights[i];
            float influence = 0f;
            if (armBoneIndices.Contains(bw.boneIndex0)) influence += bw.weight0;
            if (armBoneIndices.Contains(bw.boneIndex1)) influence += bw.weight1;
            if (armBoneIndices.Contains(bw.boneIndex2)) influence += bw.weight2;
            if (armBoneIndices.Contains(bw.boneIndex3)) influence += bw.weight3;
            armInfluence[i] = influence;
        }

        List<int> keptTriangles = new List<int>(1024);
        int subMeshCount = Mathf.Max(1, source.subMeshCount);
        for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
        {
            int[] triangles;
            try
            {
                triangles = source.GetTriangles(subMesh);
            }
            catch
            {
                continue;
            }

            for (int t = 0; t + 2 < triangles.Length; t += 3)
            {
                int i0 = triangles[t];
                int i1 = triangles[t + 1];
                int i2 = triangles[t + 2];
                if (i0 < 0 || i1 < 0 || i2 < 0 ||
                    i0 >= armInfluence.Length ||
                    i1 >= armInfluence.Length ||
                    i2 >= armInfluence.Length)
                {
                    continue;
                }

                float w0 = armInfluence[i0];
                float w1 = armInfluence[i1];
                float w2 = armInfluence[i2];
                float avg = (w0 + w1 + w2) / 3f;
                float max = Mathf.Max(w0, Mathf.Max(w1, w2));
                float min = Mathf.Min(w0, Mathf.Min(w1, w2));
                bool keep = avg >= 0.36f || max >= 0.74f || (max >= 0.56f && min >= 0.22f);
                if (!keep)
                {
                    continue;
                }

                keptTriangles.Add(i0);
                keptTriangles.Add(i1);
                keptTriangles.Add(i2);
            }
        }

        if (keptTriangles.Count < 3)
        {
            return null;
        }

        Mesh trimmed = UnityEngine.Object.Instantiate(source);
        trimmed.name = source.name + "_FPHandsOnly";
        trimmed.subMeshCount = 1;
        trimmed.SetTriangles(keptTriangles, 0, true);
        trimmed.RecalculateBounds();
        return trimmed;
    }

    private static int GetMeshTriangleCountSafe(Mesh mesh)
    {
        if (mesh == null)
        {
            return 0;
        }

        try
        {
            int[] triangles = mesh.triangles;
            return triangles != null ? triangles.Length : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static bool IsArmBoneName(string boneName)
    {
        if (string.IsNullOrWhiteSpace(boneName))
        {
            return false;
        }

        string key = boneName.ToLowerInvariant();
        return key.Contains("upperarm") ||
               key.Contains("lowerarm") ||
               key.Contains("forearm") ||
               key.Contains("hand") ||
               key.Contains("wrist") ||
               key.Contains("thumb") ||
               key.Contains("index") ||
               key.Contains("middle") ||
               key.Contains("ring") ||
               key.Contains("little") ||
               key.Contains("pinky") ||
               key.Contains("clavicle") ||
               key.Contains("shoulder");
    }

    private static Material GuessArmMaterial(Renderer renderer, Material skinMaterial, Material sleeveMaterial)
    {
        if (renderer == null)
        {
            return skinMaterial;
        }

        string key = renderer.name.ToLowerInvariant();
        if (key.Contains("shirt") || key.Contains("sleeve") || key.Contains("cloth"))
        {
            return sleeveMaterial != null ? sleeveMaterial : skinMaterial;
        }

        return skinMaterial != null ? skinMaterial : sleeveMaterial;
    }

    private static void DisableModelPhysics(GameObject modelRoot)
    {
        if (modelRoot == null)
        {
            return;
        }

        Collider[] colliders = modelRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                UnityEngine.Object.Destroy(colliders[i]);
            }
        }

        Rigidbody[] rigidbodies = modelRoot.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody body = rigidbodies[i];
            if (body != null)
            {
                UnityEngine.Object.Destroy(body);
            }
        }
    }

    private static Transform EnsureChildTransform(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject created = new GameObject(childName);
        created.transform.SetParent(parent, false);
        created.transform.localPosition = Vector3.zero;
        created.transform.localRotation = Quaternion.identity;
        created.transform.localScale = Vector3.one;
        return created.transform;
    }

    private void SetPlayerBodyHidden(bool hide)
    {
        if (playerMover == null)
        {
            return;
        }

        if (!playerBodyCacheReady)
        {
            CachePlayerBodyRenderers();
        }

        if (!playerBodyCacheReady || playerBodyHidden == hide)
        {
            return;
        }

        for (int i = 0; i < cachedPlayerBodyRenderers.Length; i++)
        {
            Renderer renderer = cachedPlayerBodyRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            bool initial = i < cachedPlayerBodyInitialEnabled.Length && cachedPlayerBodyInitialEnabled[i];
            renderer.enabled = hide ? false : initial;
        }

        playerBodyHidden = hide;
    }

    private void CachePlayerBodyRenderers()
    {
        if (playerMover == null)
        {
            return;
        }

        Renderer[] all = playerMover.GetComponentsInChildren<Renderer>(true);
        List<Renderer> filtered = new List<Renderer>(all.Length);
        List<bool> initialEnabled = new List<bool>(all.Length);
        for (int i = 0; i < all.Length; i++)
        {
            Renderer renderer = all[i];
            if (renderer == null)
            {
                continue;
            }

            if (viewmodelRoot != null && renderer.transform.IsChildOf(viewmodelRoot))
            {
                continue;
            }

            if (renderer.gameObject == gameObject || renderer.transform.IsChildOf(transform))
            {
                continue;
            }

            filtered.Add(renderer);
            initialEnabled.Add(renderer.enabled);
        }

        cachedPlayerBodyRenderers = filtered.ToArray();
        cachedPlayerBodyInitialEnabled = initialEnabled.ToArray();
        playerBodyCacheReady = true;
    }

    private static Transform TryInstantiateViewmodelAsset(
        string instanceName,
        string assetPath,
        Transform parent,
        Vector3 localPosition,
        Vector3 localEuler,
        Vector3 localScale,
        Material fallbackMaterial)
    {
        if (parent == null || string.IsNullOrWhiteSpace(assetPath))
        {
            return null;
        }

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            return null;
        }

        GameObject instance = UnityEngine.Object.Instantiate(asset, parent);
        instance.name = instanceName;
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(localEuler);
        instance.transform.localScale = localScale;

        Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                UnityEngine.Object.Destroy(colliders[i]);
            }
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            Material[] mats = renderer.sharedMaterials;
            if ((mats == null || mats.Length == 0) && fallbackMaterial != null)
            {
                renderer.sharedMaterial = fallbackMaterial;
            }
        }

        return instance.transform;
    }
#endif

    private static AudioClip BuildProceduralFireClip()
    {
        const int sampleRate = 22050;
        const float length = 0.11f;
        int sampleCount = Mathf.RoundToInt(sampleRate * length);
        float[] samples = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Exp(-32f * t);
            float noise = (UnityEngine.Random.value * 2f) - 1f;
            phase += 0.42f;
            float body = Mathf.Sin(phase) * 0.45f;
            samples[i] = (noise * 0.7f + body * 0.3f) * envelope;
        }

        AudioClip clip = AudioClip.Create("SampleScene_FP_Fire", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}




