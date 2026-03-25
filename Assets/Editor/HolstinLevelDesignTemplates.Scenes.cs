
// ============================================================
// FILE: HolstinLevelDesignTemplates.Scenes.cs
// Full scene menus + interactable sandbox
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;

public static partial class HolstinLevelDesignTemplates
{
    [MenuItem("Tools/Holstin Level Design Templates/Create Full Slice Scene")]
    public static void CreateFullSliceScene()
    {
        if (!EditorUtility.DisplayDialog("Create Holstin Full Slice",
            "This creates a new scene with a denser greybox slice: exterior, cutaway house, underpass, NPCs, enemies, narrative zones, camera zones, and stabilized player/camera rig. Continue?",
            "Create", "Cancel")) return;
        if (!ConfirmCanCreateNewScene()) return;

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        CleanupDefaultScene();
        ResetMaterialCache();
        EnsureSceneRootGroups();
        CreateDirectionalLight();
        CreateSkyGround(new Vector3(0f,-0.1f,0f), new Vector3(18f,0.2f,18f), "SceneGround");
        EnsureCoreRig(new Vector3(-6f,1.2f,-6f));
        CreateFogCourtyardExterior(new Vector3(-18f,0f,-6f));
        CreateBoardingHouseInterior(new Vector3(14f,0f,-2f));
        CreateUnderpassTemplate(new Vector3(12f,-5.5f,10f));
        CreateConnectorStair(new Vector3(10f,-0.05f,6f), 6, 1.02f, 0.2f, 1.8f, 0.2f, 1f, Quaternion.Euler(0f,180f,0f), "HouseToUnderpassSteps");
        EnsureVerticalSliceBootstrap();
        Selection.activeGameObject = FindPlayer()?.gameObject;
        FinalizeScene("Holstin full slice scene created. Bake NavMesh after opening Window > AI > Navigation. Use the template pack as layout scaffolding, not as an excuse to avoid actual level design.");
    }

    [MenuItem("Tools/Holstin Level Design Templates/Create Template Pack In Current Scene")]
    public static void CreateTemplatePackInCurrentScene()
    {
        ResetMaterialCache();
        EnsureSceneRootGroups();
        DestroyAllByName(
            "Template_Interior_BoardingHouse",
            "Template_Exterior_FogCourtyard",
            "Template_Underpass_Catacombs",
            "HouseToUnderpassSteps",
            "Template_Interactable_Sandbox");
        EnsureCoreRig(new Vector3(0f,1.2f,-8f));
        EnsureDirectionalLight();
        CreateBoardingHouseInterior(new Vector3(0f,0f,0f));
        CreateFogCourtyardExterior(new Vector3(40f,0f,0f));
        CreateUnderpassTemplate(new Vector3(0f,0f,42f));
        EnsureVerticalSliceBootstrap();
        FinalizeScene("Template pack added to current scene.");
    }

    [MenuItem("Tools/Holstin Level Design Templates/Create Interactable Test Scene")]
    public static void CreateInteractableTestScene()
    {
        if (!EditorUtility.DisplayDialog("Create Holstin Interactable Test Scene",
            "This creates an expanded tutorial slice (multiple rooms, inspect/pickup/door/NPC/console/combat beats, checkpoints, and a relay puzzle) in a single modular sandbox. Continue?",
            "Create", "Cancel")) return;
        if (!ConfirmCanCreateNewScene()) return;

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        CleanupDefaultScene();
        ResetMaterialCache();
        EnsureSceneRootGroups();
        CreateDirectionalLight();
        CreateSkyGround(new Vector3(10f, -0.1f, 0f), new Vector3(44f, 0.2f, 14f), "InteractableGround");
        EnsureCoreRig(new Vector3(-8f, 1.2f, 0f));
        CreateInteractableSandbox(Vector3.zero);
        EnsureVerticalSliceBootstrap();
        Selection.activeGameObject = FindPlayer()?.gameObject;
        FinalizeScene("Expanded interactable tutorial scene created. Flow: Inspect -> Pickup -> Unlock -> NPC -> Relay puzzle -> Combat lane -> Final console.");
    }

    [MenuItem("Tools/Holstin Level Design Templates/Apply Vertical Slice Bootstrap To Current Scene")]
    public static void ApplyVerticalSliceBootstrapToCurrentScene()
    {
        ResetMaterialCache();
        EnsureSceneRootGroups();
        EnsureCoreRig(new Vector3(-6f,1.2f,-6f));
        EnsureDirectionalLight();
        EnsureVerticalSliceBootstrap();
        FinalizeScene("Vertical slice bootstrap applied to current scene. Use the component context menu to force a refresh if needed.");
    }

    private static void CreateInteractableSandbox(Vector3 origin)
    {
        GameObject root = NewRoot("Template_Interactable_Sandbox", origin);

        CreateFloor(root.transform, "SandboxFloor_Main", origin + new Vector3(10f, -0.05f, 0f), new Vector3(40f, 0.2f, 12f), floorMaterial);
        CreateWall(root.transform, origin + new Vector3(-10f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 12f), wallMaterial, "WestWall");
        CreateWall(root.transform, origin + new Vector3(30f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 12f), wallMaterial, "EastWall");
        CreateWall(root.transform, origin + new Vector3(10f, 1.6f, 6f), new Vector3(40f, 3.2f, 0.35f), wallMaterial, "NorthWall");
        CreateWall(root.transform, origin + new Vector3(10f, 1.6f, -6f), new Vector3(40f, 3.2f, 0.35f), wallMaterial, "SouthWall");

        CreatePartitionWithOpening(root.transform, origin + new Vector3(-2f, 0f, 0f), "Partition_Entry");
        CreatePartitionWithOpening(root.transform, origin + new Vector3(6f, 0f, 0f), "Partition_Service");
        CreatePartitionWithOpening(root.transform, origin + new Vector3(14f, 0f, 0f), "Partition_Relay");
        CreatePartitionWithOpening(root.transform, origin + new Vector3(22f, 0f, 0f), "Partition_Exit");

        CreateTable(root.transform, origin + new Vector3(-7.4f, 0f, 2.1f), new Vector3(2f, 0.14f, 0.95f), "IntakeDesk");
        CreateDesk(root.transform, origin + new Vector3(1.3f, 0f, -2f), "CaretakerDesk");
        CreateBench(root.transform, origin + new Vector3(9.5f, 0.35f, 2.6f), 2.2f, "RelayBench");
        CreateBench(root.transform, origin + new Vector3(18.2f, 0.35f, -2.4f), 2.4f, "ArchiveBench");
        CreateLantern(root.transform, origin + new Vector3(-7.7f, 2.55f, 2f), "Lantern_Entry", new Color(1f, 0.78f, 0.48f));
        CreateLantern(root.transform, origin + new Vector3(1.8f, 2.55f, 2.5f), "Lantern_Service", new Color(1f, 0.75f, 0.44f));
        CreateLantern(root.transform, origin + new Vector3(10.4f, 2.55f, -2.3f), "Lantern_Relay", new Color(0.55f, 0.8f, 1f));
        CreateLantern(root.transform, origin + new Vector3(18.7f, 2.55f, 2.3f), "Lantern_Archive", new Color(0.72f, 0.86f, 1f));
        CreateLantern(root.transform, origin + new Vector3(27.4f, 2.55f, 0f), "Lantern_Finale", new Color(1f, 0.64f, 0.44f));

        CreateInspectableNote(
            root.transform,
            origin + new Vector3(-7.4f, 0.98f, 2.1f),
            new Vector3(0.38f, 0.05f, 0.28f),
            "Intake Brief",
            "We train by doing. Move, read, improvise. If you inspect paperwork, turn it under the light, then step away when you are done.");
        CreateInspectableNote(
            root.transform,
            origin + new Vector3(10.1f, 0.98f, 2.6f),
            new Vector3(0.36f, 0.05f, 0.26f),
            "Relay Schematic",
            "Two fuses wake two rails. Both rails down means passage.");
        CreateInspectableNote(
            root.transform,
            origin + new Vector3(26.1f, 0.98f, -1.9f),
            new Vector3(0.34f, 0.05f, 0.25f),
            "Exit Protocol",
            "No panic sprinting. Aim before firing. Confirm seal before leaving.");

        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-6f, 1f, 0f), new Vector3(7f, 2f, 9f), "Starter Room", "Follow the clues, not the panic.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(2f, 1f, 0f), new Vector3(7f, 2f, 9f), "Service Hall", "A lock is just a conversation with the wrong timing.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(10f, 1f, 0f), new Vector3(7f, 2f, 9f), "Relay Hall", "Power and fear spread in similar patterns.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(18f, 1f, 0f), new Vector3(7f, 2f, 9f), "Archive Lane", "Records outlive witnesses.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(26f, 1f, 0f), new Vector3(7f, 2f, 9f), "Exit Chamber", "You are not done. You are simply certified.");

        CreateCameraZone(root.transform, origin + new Vector3(-6f, 1f, 0f), new Vector3(7f, 2f, 9f), 45f, "CameraZone_Entry");
        CreateCameraZone(root.transform, origin + new Vector3(2f, 1f, 0f), new Vector3(7f, 2f, 9f), 135f, "CameraZone_Service");
        CreateCameraZone(root.transform, origin + new Vector3(10f, 1f, 0f), new Vector3(7f, 2f, 9f), 45f, "CameraZone_Relay");
        CreateCameraZone(root.transform, origin + new Vector3(18f, 1f, 0f), new Vector3(7f, 2f, 9f), 135f, "CameraZone_Archive");
        CreateCameraZone(root.transform, origin + new Vector3(26f, 1f, 0f), new Vector3(7f, 2f, 9f), 45f, "CameraZone_Exit");

        CreatePickupKey(
            root.transform,
            origin + new Vector3(-6f, 0f, -1.7f),
            "Old Key Pickup",
            "old_key",
            "Old Key",
            "A rusted key for the first service door.",
            "pickup_exterior_key");
        CreateLockedDoor(
            root.transform,
            origin + new Vector3(-2f, 0f, 0f),
            "Service Door",
            "old_key",
            "Old Key",
            "The door lock expects an old district key.",
            "unlock_interior_gate");

        CreateKeyGiverNPC(
            root.transform,
            origin + new Vector3(1.8f, 0f, 1.3f),
            "Caretaker",
            "Assigns jobs in short, practical bursts.",
            "service_key",
            "Service Key",
            "npc_reward_key",
            "Take the service key. Keep your head down and your story short.",
            "You already have the key. Doors first, questions later.");
        CreateLockedDoor(
            root.transform,
            origin + new Vector3(6f, 0f, 0f),
            "Relay Access Door",
            "service_key",
            "Service Key",
            "Badge lock: service key required.",
            "service_wing_unlock");

        CreatePickupKey(
            root.transform,
            origin + new Vector3(8.5f, 0f, 2.3f),
            "Relay Fuse Alpha Pickup",
            "relay_fuse_a",
            "Relay Fuse A",
            "Stamped for rail segment A.",
            "relay_fuse_a_picked");
        CreatePickupKey(
            root.transform,
            origin + new Vector3(11.8f, 0f, -2.4f),
            "Relay Fuse Beta Pickup",
            "relay_fuse_b",
            "Relay Fuse B",
            "Stamped for rail segment B.",
            "relay_fuse_b_picked");

        GameObject relayBarrierNorth = CreatePrimitive(PrimitiveType.Cube, "RelayBarrierNorth", origin + new Vector3(14f, 1.1f, 1.25f), new Vector3(0.22f, 2.2f, 1.2f), root.transform, accentMaterial, false);
        GameObject relayBarrierSouth = CreatePrimitive(PrimitiveType.Cube, "RelayBarrierSouth", origin + new Vector3(14f, 1.1f, -1.25f), new Vector3(0.22f, 2.2f, 1.2f), root.transform, accentMaterial, false);

        GameObject relayIndicatorA = new GameObject("RelayIndicatorA");
        relayIndicatorA.transform.SetParent(root.transform);
        relayIndicatorA.transform.position = origin + new Vector3(13f, 2.25f, 2.4f);
        Light relayLightA = relayIndicatorA.AddComponent<Light>();
        relayLightA.type = LightType.Point;
        relayLightA.range = 6f;
        relayLightA.intensity = 4.2f;
        relayLightA.color = new Color(0.44f, 0.78f, 1f, 1f);
        relayIndicatorA.SetActive(false);

        GameObject relayIndicatorB = new GameObject("RelayIndicatorB");
        relayIndicatorB.transform.SetParent(root.transform);
        relayIndicatorB.transform.position = origin + new Vector3(13f, 2.25f, -2.4f);
        Light relayLightB = relayIndicatorB.AddComponent<Light>();
        relayLightB.type = LightType.Point;
        relayLightB.range = 6f;
        relayLightB.intensity = 4.2f;
        relayLightB.color = new Color(0.44f, 0.78f, 1f, 1f);
        relayIndicatorB.SetActive(false);

        CreateServiceConsole(
            root.transform,
            origin + new Vector3(9f, 0f, 2.45f),
            "Relay Console A",
            "relay_fuse_a",
            "Relay Fuse A",
            relayBarrierNorth,
            relayIndicatorA,
            true,
            "Rail A online. One lock left.",
            "Console A needs Relay Fuse A.",
            "relay_a_online");
        CreateServiceConsole(
            root.transform,
            origin + new Vector3(12f, 0f, -2.45f),
            "Relay Console B",
            "relay_fuse_b",
            "Relay Fuse B",
            relayBarrierSouth,
            relayIndicatorB,
            true,
            "Rail B online. Passage is clear.",
            "Console B needs Relay Fuse B.",
            "relay_b_online");

        CreateEnemyPatrol(
            root.transform,
            origin + new Vector3(18.2f, 0f, 2.3f),
            "Archive Enforcer",
            new[]
            {
                origin + new Vector3(16.5f, 0f, 2.5f),
                origin + new Vector3(19.6f, 0f, 2.5f),
                origin + new Vector3(19.6f, 0f, -2.5f),
                origin + new Vector3(16.5f, 0f, -2.5f)
            });

        CreateKeyGiverNPC(
            root.transform,
            origin + new Vector3(18.4f, 0f, -1.6f),
            "Archivist",
            "Protects passes and stories with equal suspicion.",
            "archive_key",
            "Archive Key",
            "archive_key_granted",
            "You made it through the relay. Take this archive key. Keep your breathing quiet.",
            "Archive key is already with you. Finish the chamber.");

        CreateLockedDoor(
            root.transform,
            origin + new Vector3(22f, 0f, 0f),
            "Archive Exit Door",
            "archive_key",
            "Archive Key",
            "Archive lock: no key, no exit.",
            "archive_door_opened");

        GameObject finalBarrier = CreatePrimitive(PrimitiveType.Cube, "FinalSecurityBarrier", origin + new Vector3(26f, 1.1f, 0f), new Vector3(0.24f, 2.2f, 2.4f), root.transform, accentMaterial, false);
        GameObject finalBeacon = new GameObject("FinalBeacon");
        finalBeacon.transform.SetParent(root.transform);
        finalBeacon.transform.position = origin + new Vector3(27.8f, 2.3f, -2.2f);
        Light finalLight = finalBeacon.AddComponent<Light>();
        finalLight.type = LightType.Point;
        finalLight.range = 8f;
        finalLight.intensity = 4.8f;
        finalLight.color = new Color(1f, 0.62f, 0.42f, 1f);
        finalBeacon.SetActive(false);

        CreateServiceConsole(
            root.transform,
            origin + new Vector3(24.1f, 0f, -1.9f),
            "Exit Chamber Console",
            "service_key",
            "Service Key",
            finalBarrier,
            finalBeacon,
            false,
            "Seal acknowledged. Exit chamber unlocked.",
            "The chamber console expects a service key.",
            "console_service_unlock");

        CreateCheckpoint(root.transform, "Checkpoint_Service", origin + new Vector3(2f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Service hall.");
        CreateCheckpoint(root.transform, "Checkpoint_Relay", origin + new Vector3(10f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Relay hall.");
        CreateCheckpoint(root.transform, "Checkpoint_Archive", origin + new Vector3(18f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Archive lane.");
        CreateCheckpoint(root.transform, "Checkpoint_Exit", origin + new Vector3(26f, 1f, 0f), new Vector3(3.5f, 2f, 3.5f), "Checkpoint set: Exit chamber.");
    }

    private static void CreatePartitionWithOpening(Transform parent, Vector3 origin, string label)
    {
        CreateWall(parent, origin + new Vector3(0f, 1.6f, -3.9f), new Vector3(0.35f, 3.2f, 2.1f), wallMaterial, label + "_South");
        CreateWall(parent, origin + new Vector3(0f, 1.6f, 3.9f), new Vector3(0.35f, 3.2f, 2.1f), wallMaterial, label + "_North");
        CreateWall(parent, origin + new Vector3(0f, 2.8f, 0f), new Vector3(0.35f, 0.8f, 2.4f), wallMaterial, label + "_Lintel");
    }

    private static void DestroyAllByName(params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            return;
        }

        HashSet<string> lookup = new HashSet<string>(names);
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = allTransforms.Length - 1; i >= 0; i--)
        {
            Transform transformComponent = allTransforms[i];
            if (transformComponent == null || !lookup.Contains(transformComponent.name))
            {
                continue;
            }

            Object.DestroyImmediate(transformComponent.gameObject);
        }
    }

    private static void CreateCheckpoint(Transform parent, string name, Vector3 pos, Vector3 size, string message)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent);
        zone.transform.position = pos;
        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;

        CheckpointZone checkpoint = zone.AddComponent<CheckpointZone>();
        checkpoint.Configure(message, true);
    }
}
