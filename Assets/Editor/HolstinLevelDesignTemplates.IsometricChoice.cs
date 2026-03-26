// ============================================================
// FILE: HolstinLevelDesignTemplates.IsometricChoice.cs
// Isometric mega-scene generator with branching choices
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class HolstinLevelDesignTemplates
{
    [MenuItem("Tools/Holstin Level Design Templates/Create Isometric Choice Megastructure")]
    public static void CreateIsometricChoiceMegastructure()
    {
        if (!EditorUtility.DisplayDialog(
                "Create Isometric Choice Megastructure",
                "Generates a large multi-floor isometric scene with branch choices, state-driven world shifts, room-specific camera zones, sound anchors, and post-processing volumes. Continue?",
                "Create",
                "Cancel"))
        {
            return;
        }
        if (!ConfirmCanCreateNewScene())
        {
            return;
        }

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        CleanupDefaultScene();
        ResetMaterialCache();
        EnsureSceneRootGroups();
        CreateDirectionalLight();
        CreateSkyGround(new Vector3(0f, -0.2f, 0f), new Vector3(140f, 0.4f, 140f), "SceneGround");
        EnsureCoreRig(new Vector3(0f, 1.2f, -22f));
        CreateIsometricChoiceMegastructureContent(Vector3.zero);
        ApplyProductionAestheticPassInternal();
        Selection.activeGameObject = FindPlayer()?.gameObject;
        FinalizeScene("Isometric choice megastructure created. Use camera zones, branch milestones, and ChoiceWorldStateDirector reactions to iterate layout psychology.");
    }

    private static void CreateIsometricChoiceMegastructureContent(Vector3 origin)
    {
        GameObject root = NewRoot("Template_Isometric_ChoiceMegastructure", origin);

        const float lowerWallHeight = 3.2f;
        const float upperWallHeight = 3.2f;
        const float lowerWallY = 1.6f;
        const float upperWallY = 4.8f;
        const float upperFloorY = 3.2f;
        const float wallThickness = 0.35f;

        // Structural base and floor plates (single coherent footprint, no floating islands).
        CreateFloor(root.transform, "MegaFoundation", origin + new Vector3(0f, -0.26f, 0f), new Vector3(58f, 0.52f, 58f), darkMaterial);

        // Lower level layout.
        CreateFloor(root.transform, "HubFloor_L0", origin + new Vector3(0f, 0f, 0f), new Vector3(16f, 0.24f, 16f), floorMaterial);
        CreateFloor(root.transform, "EntryHall_L0", origin + new Vector3(0f, 0f, -14f), new Vector3(10f, 0.24f, 12f), floorMaterial);
        CreateFloor(root.transform, "EntryFoyer_L0", origin + new Vector3(0f, 0f, -23f), new Vector3(12f, 0.24f, 8f), floorMaterial);
        CreateFloor(root.transform, "WestHall_L0", origin + new Vector3(-14f, 0f, 0f), new Vector3(12f, 0.24f, 10f), floorMaterial);
        CreateFloor(root.transform, "WestChamber_L0", origin + new Vector3(-23f, 0f, 0f), new Vector3(6f, 0.24f, 12f), floorMaterial);
        CreateFloor(root.transform, "EastHall_L0", origin + new Vector3(14f, 0f, 0f), new Vector3(12f, 0.24f, 10f), floorMaterial);
        CreateFloor(root.transform, "EastChamber_L0", origin + new Vector3(23f, 0f, 0f), new Vector3(6f, 0.24f, 12f), floorMaterial);
        CreateFloor(root.transform, "NorthHall_L0", origin + new Vector3(0f, 0f, 14f), new Vector3(12f, 0.24f, 12f), floorMaterial);
        CreateFloor(root.transform, "NorthChamber_L0", origin + new Vector3(0f, 0f, 23f), new Vector3(12f, 0.24f, 8f), floorMaterial);

        // Upper level layout.
        CreateFloor(root.transform, "HubFloor_L1", origin + new Vector3(0f, upperFloorY, 0f), new Vector3(12f, 0.22f, 12f), woodMaterial);
        CreateFloor(root.transform, "Catwalk_West", origin + new Vector3(-10f, upperFloorY, 0f), new Vector3(8f, 0.2f, 2f), metalMaterial);
        CreateFloor(root.transform, "WestFloor_L1", origin + new Vector3(-18f, upperFloorY, 0f), new Vector3(8f, 0.22f, 8f), woodMaterial);
        CreateFloor(root.transform, "Catwalk_East", origin + new Vector3(10f, upperFloorY, 0f), new Vector3(8f, 0.2f, 2f), metalMaterial);
        CreateFloor(root.transform, "EastFloor_L1", origin + new Vector3(18f, upperFloorY, 0f), new Vector3(8f, 0.22f, 8f), woodMaterial);
        CreateFloor(root.transform, "Catwalk_North", origin + new Vector3(0f, upperFloorY, 10f), new Vector3(2f, 0.2f, 8f), metalMaterial);
        CreateFloor(root.transform, "NorthFloor_L1", origin + new Vector3(0f, upperFloorY, 18f), new Vector3(8f, 0.22f, 8f), woodMaterial);
        CreateFloor(root.transform, "NorthGateLanding_L1", origin + new Vector3(0f, upperFloorY, 23.8f), new Vector3(4f, 0.22f, 3.6f), woodMaterial);

        // Roof.
        CreateFloor(root.transform, "MegaRoof", origin + new Vector3(0f, 6.45f, 0f), new Vector3(58f, 0.26f, 58f), roofMaterial);

        // Lower outer shell.
        CreateWall(root.transform, origin + new Vector3(-29f, lowerWallY, 0f), new Vector3(wallThickness, lowerWallHeight, 58f), wallMaterial, "OuterWall_West_L0");
        CreateWall(root.transform, origin + new Vector3(29f, lowerWallY, 0f), new Vector3(wallThickness, lowerWallHeight, 58f), wallMaterial, "OuterWall_East_L0");
        CreateWall(root.transform, origin + new Vector3(0f, lowerWallY, -29f), new Vector3(58f, lowerWallHeight, wallThickness), wallMaterial, "OuterWall_South_L0");
        CreateWall(root.transform, origin + new Vector3(0f, lowerWallY, 29f), new Vector3(58f, lowerWallHeight, wallThickness), wallMaterial, "OuterWall_North_L0");

        // Upper shell continues the structure.
        CreateWall(root.transform, origin + new Vector3(-29f, upperWallY, 0f), new Vector3(wallThickness, upperWallHeight, 58f), wallMaterial, "OuterWall_West_L1");
        CreateWall(root.transform, origin + new Vector3(29f, upperWallY, 0f), new Vector3(wallThickness, upperWallHeight, 58f), wallMaterial, "OuterWall_East_L1");
        CreateWall(root.transform, origin + new Vector3(0f, upperWallY, -29f), new Vector3(58f, upperWallHeight, wallThickness), wallMaterial, "OuterWall_South_L1");
        CreateWall(root.transform, origin + new Vector3(0f, upperWallY, 29f), new Vector3(58f, upperWallHeight, wallThickness), wallMaterial, "OuterWall_North_L1");

        // Hub separators with real openings (connected architecture).
        CreateWall(root.transform, origin + new Vector3(-4.25f, lowerWallY, -8f), new Vector3(5.5f, lowerWallHeight, wallThickness), wallMaterial, "Partition_EntryHub_Left");
        CreateWall(root.transform, origin + new Vector3(4.25f, lowerWallY, -8f), new Vector3(5.5f, lowerWallHeight, wallThickness), wallMaterial, "Partition_EntryHub_Right");
        CreateWall(root.transform, origin + new Vector3(0f, 2.75f, -8f), new Vector3(3f, 0.9f, wallThickness), wallMaterial, "Partition_EntryHub_Lintel");

        CreateWall(root.transform, origin + new Vector3(-4.25f, lowerWallY, 8f), new Vector3(5.5f, lowerWallHeight, wallThickness), wallMaterial, "Partition_NorthHub_Left");
        CreateWall(root.transform, origin + new Vector3(4.25f, lowerWallY, 8f), new Vector3(5.5f, lowerWallHeight, wallThickness), wallMaterial, "Partition_NorthHub_Right");
        CreateWall(root.transform, origin + new Vector3(0f, 2.75f, 8f), new Vector3(3f, 0.9f, wallThickness), wallMaterial, "Partition_NorthHub_Lintel");

        CreateWall(root.transform, origin + new Vector3(-8f, lowerWallY, -4.25f), new Vector3(wallThickness, lowerWallHeight, 5.5f), wallMaterial, "Partition_WestHub_South");
        CreateWall(root.transform, origin + new Vector3(-8f, lowerWallY, 4.25f), new Vector3(wallThickness, lowerWallHeight, 5.5f), wallMaterial, "Partition_WestHub_North");
        CreateWall(root.transform, origin + new Vector3(-8f, 2.75f, 0f), new Vector3(wallThickness, 0.9f, 3f), wallMaterial, "Partition_WestHub_Lintel");

        CreateWall(root.transform, origin + new Vector3(8f, lowerWallY, -4.25f), new Vector3(wallThickness, lowerWallHeight, 5.5f), wallMaterial, "Partition_EastHub_South");
        CreateWall(root.transform, origin + new Vector3(8f, lowerWallY, 4.25f), new Vector3(wallThickness, lowerWallHeight, 5.5f), wallMaterial, "Partition_EastHub_North");
        CreateWall(root.transform, origin + new Vector3(8f, 2.75f, 0f), new Vector3(wallThickness, 0.9f, 3f), wallMaterial, "Partition_EastHub_Lintel");

        // Chamber thresholds.
        CreateWall(root.transform, origin + new Vector3(-18f, lowerWallY, -3.75f), new Vector3(wallThickness, lowerWallHeight, 4.5f), wallMaterial, "Partition_WestHallChamber_South");
        CreateWall(root.transform, origin + new Vector3(-18f, lowerWallY, 3.75f), new Vector3(wallThickness, lowerWallHeight, 4.5f), wallMaterial, "Partition_WestHallChamber_North");
        CreateWall(root.transform, origin + new Vector3(-18f, 2.75f, 0f), new Vector3(wallThickness, 0.9f, 3f), wallMaterial, "Partition_WestHallChamber_Lintel");

        CreateWall(root.transform, origin + new Vector3(18f, lowerWallY, -3.75f), new Vector3(wallThickness, lowerWallHeight, 4.5f), wallMaterial, "Partition_EastHallChamber_South");
        CreateWall(root.transform, origin + new Vector3(18f, lowerWallY, 3.75f), new Vector3(wallThickness, lowerWallHeight, 4.5f), wallMaterial, "Partition_EastHallChamber_North");
        CreateWall(root.transform, origin + new Vector3(18f, 2.75f, 0f), new Vector3(wallThickness, 0.9f, 3f), wallMaterial, "Partition_EastHallChamber_Lintel");

        CreateWall(root.transform, origin + new Vector3(-3.75f, lowerWallY, 18f), new Vector3(4.5f, lowerWallHeight, wallThickness), wallMaterial, "Partition_NorthHallChamber_Left");
        CreateWall(root.transform, origin + new Vector3(3.75f, lowerWallY, 18f), new Vector3(4.5f, lowerWallHeight, wallThickness), wallMaterial, "Partition_NorthHallChamber_Right");
        CreateWall(root.transform, origin + new Vector3(0f, 2.75f, 18f), new Vector3(3f, 0.9f, wallThickness), wallMaterial, "Partition_NorthHallChamber_Lintel");

        // Structural support columns under upper level and roof.
        Vector3[] supportPoints =
        {
            new Vector3(-6f, 0f, -6f), new Vector3(6f, 0f, -6f), new Vector3(-6f, 0f, 6f), new Vector3(6f, 0f, 6f),
            new Vector3(-18f, 0f, -3f), new Vector3(-18f, 0f, 3f), new Vector3(18f, 0f, -3f), new Vector3(18f, 0f, 3f),
            new Vector3(-3f, 0f, 18f), new Vector3(3f, 0f, 18f)
        };
        for (int i = 0; i < supportPoints.Length; i++)
        {
            Vector3 basePoint = origin + supportPoints[i];
            CreatePrimitive(PrimitiveType.Cube, "Support_L0_" + i, basePoint + new Vector3(0f, lowerWallY, 0f), new Vector3(0.55f, lowerWallHeight, 0.55f), root.transform, darkMaterial);
            CreatePrimitive(PrimitiveType.Cube, "Support_L1_" + i, basePoint + new Vector3(0f, upperWallY, 0f), new Vector3(0.46f, upperWallHeight, 0.46f), root.transform, darkMaterial);
        }

        // Vertical connectors.
        CreateConnectorStair(origin + new Vector3(-3.8f, 0f, -2f), 8, 0.65f, 0.45f, 2f, 0.16f, 0.62f, Quaternion.Euler(0f, 90f, 0f), "Stair_ToWest_L1", root.transform);
        CreateConnectorStair(origin + new Vector3(3.8f, 0f, -2f), 8, 0.65f, 0.45f, 2f, 0.16f, 0.62f, Quaternion.Euler(0f, -90f, 0f), "Stair_ToEast_L1", root.transform);
        CreateConnectorStair(origin + new Vector3(-1.1f, 0f, 3.8f), 8, 0.65f, 0.45f, 2f, 0.16f, 0.62f, Quaternion.identity, "Stair_ToNorth_L1", root.transform);
        CreateVisualDetailPass(root.transform, origin);

        // Branch locks and core progression doors.
        CreateLockedDoor(root.transform, origin + new Vector3(-16f, 0f, 0f), "West Branch Door", "ward_badge_left", "Left Ward Badge", "West branch lock: left badge required.", "west_branch_unlocked");
        CreateLockedDoor(root.transform, origin + new Vector3(16f, 0f, 0f), "East Branch Door", "ward_badge_right", "Right Ward Badge", "East branch lock: right badge required.", "east_branch_unlocked");
        CreateLockedDoor(root.transform, origin + new Vector3(0f, 0f, 16f), "North Vault Door", "vault_key", "Vault Key", "North vault lock: vault key required.", "north_vault_unlocked");

        // Choice pickups (exclusive via world state director reactions)
        CreatePickupKey(
            root.transform,
            origin + new Vector3(-2.4f, 0f, -23f),
            "Left Ward Badge Pickup",
            "ward_badge_left",
            "Left Ward Badge",
            "Stamped for west access. Choosing it will reshape your route.",
            "choice_left_pick");
        CreatePickupKey(
            root.transform,
            origin + new Vector3(2.4f, 0f, -23f),
            "Right Ward Badge Pickup",
            "ward_badge_right",
            "Right Ward Badge",
            "Stamped for east access. Choosing it will reshape your route.",
            "choice_right_pick");

        // Branch objectives and finale gate
        GameObject westBarrier = CreatePrimitive(PrimitiveType.Cube, "WestBranchBarrier", origin + new Vector3(-22f, 1.1f, 4.2f), new Vector3(0.24f, 2.2f, 2.8f), root.transform, accentMaterial, false);
        GameObject eastBarrier = CreatePrimitive(PrimitiveType.Cube, "EastBranchBarrier", origin + new Vector3(22f, 1.1f, 4.2f), new Vector3(0.24f, 2.2f, 2.8f), root.transform, accentMaterial, false);
        GameObject finalGate = CreatePrimitive(PrimitiveType.Cube, "FinalGate", origin + new Vector3(0f, 4.3f, 23.8f), new Vector3(2.8f, 2.2f, 0.24f), root.transform, accentMaterial, false);

        GameObject westBeacon = CreateSpatialAudioAnchor(root.transform, origin + new Vector3(-23f, 2.6f, 6.5f), "WestBeaconAudio");
        GameObject eastBeacon = CreateSpatialAudioAnchor(root.transform, origin + new Vector3(23f, 2.6f, 6.5f), "EastBeaconAudio");
        GameObject finalBeacon = CreateSpatialAudioAnchor(root.transform, origin + new Vector3(0f, 5.2f, 24.8f), "FinalBeaconAudio");
        westBeacon.SetActive(false);
        eastBeacon.SetActive(false);
        finalBeacon.SetActive(false);

        CreateServiceConsole(
            root.transform,
            origin + new Vector3(-23f, 0f, -1.8f),
            "West Console",
            "ward_badge_left",
            "Left Ward Badge",
            westBarrier,
            westBeacon,
            false,
            "West relay online. The vault map twists toward the north.",
            "West console accepts only Left Ward Badge.",
            "left_branch_console");
        CreateServiceConsole(
            root.transform,
            origin + new Vector3(23f, 0f, -1.8f),
            "East Console",
            "ward_badge_right",
            "Right Ward Badge",
            eastBarrier,
            eastBeacon,
            false,
            "East relay online. The vault map bends in response.",
            "East console accepts only Right Ward Badge.",
            "right_branch_console");

        CreateKeyGiverNPC(
            root.transform,
            origin + new Vector3(0f, 0f, 23f),
            "Surveyor",
            "Tracks structural mood swings in the district architecture.",
            "vault_key",
            "Vault Key",
            "vault_key_granted",
            "You mapped enough of the drift. Take the vault key and commit.",
            "Vault key is already assigned. Finish what you opened.");

        CreateServiceConsole(
            root.transform,
            origin + new Vector3(0f, upperFloorY, 18.8f),
            "Hub Rewrite Console",
            "vault_key",
            "Vault Key",
            finalGate,
            finalBeacon,
            false,
            "Rewrite accepted. Hub topology updated in real time.",
            "Hub rewrite console requires Vault Key.",
            "hub_rewrite");

        // Notes and NPC lines guide controls through fiction instead of direct tutorial spam.
        CreateInspectableNote(root.transform, origin + new Vector3(0f, 0.96f, -22.1f), new Vector3(0.38f, 0.05f, 0.28f), "Entry Brief", "Move with intent. Inspect evidence, rotate it under light, and leave your hands free before pushing deeper.");
        CreateInspectableNote(root.transform, origin + new Vector3(-23f, 0.96f, -3.4f), new Vector3(0.34f, 0.05f, 0.24f), "West Dossier", "West favors patience and short sightlines. Keep your camera angled; don't chase every noise.");
        CreateInspectableNote(root.transform, origin + new Vector3(23f, 0.96f, -3.4f), new Vector3(0.34f, 0.05f, 0.24f), "East Dossier", "East rewards aggression but punishes tunnel vision. Aim before firing, then reposition.");
        CreateInspectableNote(root.transform, origin + new Vector3(0f, 4.05f, 18.3f), new Vector3(0.34f, 0.05f, 0.24f), "Rewrite Protocol", "Your first choice changes what the complex becomes. Nothing is decorative.");

        // Narrative + room-specific camera optimization
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(0f, 1f, -22f), new Vector3(10f, 2f, 8f), "Entry Causeway", "Two badges, two stories, one irreversible first commitment.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-22f, 1f, 0f), new Vector3(8f, 2f, 12f), "West Arm", "Compressed routes, defensive rhythm.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(22f, 1f, 0f), new Vector3(8f, 2f, 12f), "East Arm", "Wide reveals, risky confidence.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(0f, 1f, 22f), new Vector3(12f, 2f, 8f), "North Vault", "Authority hides here, dressed as procedure.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(0f, 4.2f, 18f), new Vector3(8f, 2f, 8f), "Upper Rewrite Deck", "Topology is policy made physical.");

        CreateCameraZone(root.transform, origin + new Vector3(0f, 1f, -22f), new Vector3(10f, 2f, 8f), 45f, "Cam_Entry");
        CreateCameraZone(root.transform, origin + new Vector3(-22f, 1f, 0f), new Vector3(8f, 2f, 12f), 135f, "Cam_West");
        CreateCameraZone(root.transform, origin + new Vector3(22f, 1f, 0f), new Vector3(8f, 2f, 12f), 45f, "Cam_East");
        CreateCameraZone(root.transform, origin + new Vector3(0f, 1f, 22f), new Vector3(12f, 2f, 8f), 180f, "Cam_North");
        CreateCameraZone(root.transform, origin + new Vector3(0f, 4.2f, 18f), new Vector3(8f, 2f, 8f), 0f, "Cam_UpperDeck");
        CreateCameraZone(root.transform, origin + new Vector3(-3.8f, 2.2f, -2f), new Vector3(3f, 4f, 6f), 120f, "Cam_StairWest");
        CreateCameraZone(root.transform, origin + new Vector3(3.8f, 2.2f, -2f), new Vector3(3f, 4f, 6f), 60f, "Cam_StairEast");
        CreateCameraZone(root.transform, origin + new Vector3(-1.1f, 2.2f, 3.8f), new Vector3(3.6f, 4f, 6f), 0f, "Cam_StairNorth");

        // Combat showcase beats
        CreateEnemyPatrol(
            root.transform,
            origin + new Vector3(-23f, 0f, 5f),
            "West Sentinel",
            new[]
            {
                origin + new Vector3(-25f, 0f, 5f),
                origin + new Vector3(-20.5f, 0f, 5f),
                origin + new Vector3(-20.5f, 0f, -3f),
                origin + new Vector3(-25f, 0f, -3f)
            });
        CreateEnemyPatrol(
            root.transform,
            origin + new Vector3(23f, 0f, 5f),
            "East Sentinel",
            new[]
            {
                origin + new Vector3(25f, 0f, 5f),
                origin + new Vector3(20.5f, 0f, 5f),
                origin + new Vector3(20.5f, 0f, -3f),
                origin + new Vector3(25f, 0f, -3f)
            });

        // Ambient audio anchors (clip assignment later; architecture ready now)
        CreateSpatialAudioAnchor(root.transform, origin + new Vector3(0f, 2.6f, -22f), "EntryAmbient");
        CreateSpatialAudioAnchor(root.transform, origin + new Vector3(-22f, 2.6f, 0f), "WestAmbient");
        CreateSpatialAudioAnchor(root.transform, origin + new Vector3(22f, 2.6f, 0f), "EastAmbient");
        CreateSpatialAudioAnchor(root.transform, origin + new Vector3(0f, 2.6f, 22f), "NorthAmbient");
        CreateSpatialAudioAnchor(root.transform, origin + new Vector3(0f, 5.3f, 18f), "UpperAmbient");

        // Checkpoints
        CreateCheckpointZone(root.transform, "Checkpoint_Entry", origin + new Vector3(0f, 1f, -22f), new Vector3(4f, 2f, 4f), "Checkpoint set: Entry causeway.");
        CreateCheckpointZone(root.transform, "Checkpoint_West", origin + new Vector3(-22f, 1f, 0f), new Vector3(4f, 2f, 4f), "Checkpoint set: West arm.");
        CreateCheckpointZone(root.transform, "Checkpoint_East", origin + new Vector3(22f, 1f, 0f), new Vector3(4f, 2f, 4f), "Checkpoint set: East arm.");
        CreateCheckpointZone(root.transform, "Checkpoint_North", origin + new Vector3(0f, 1f, 22f), new Vector3(4f, 2f, 4f), "Checkpoint set: North vault.");
        CreateCheckpointZone(root.transform, "Checkpoint_Upper", origin + new Vector3(0f, 4.2f, 18f), new Vector3(4f, 2f, 4f), "Checkpoint set: Upper rewrite deck.");

        // Post-processing volumes (global + choice mood zones)
        GameObject postRoot = new GameObject("Choice_PostVolumes");
        postRoot.transform.SetParent(root.transform);
        Volume globalVolume = postRoot.AddComponent<Volume>();
        globalVolume.isGlobal = true;
        globalVolume.priority = -5f;
        globalVolume.weight = 0.42f;
        globalVolume.profile = CreateMoodProfile("Choice_Global_Profile", new Color(0.92f, 0.93f, 0.97f, 1f), -0.08f, 0.25f, 0.22f);

        GameObject leftMood = CreateLocalMoodVolume(postRoot.transform, "Choice_LeftMood", origin + new Vector3(-22f, 2f, 0f), new Vector3(12f, 8f, 16f), new Color(0.78f, 0.9f, 1f, 1f), -0.15f, 0.35f, 0.28f);
        GameObject rightMood = CreateLocalMoodVolume(postRoot.transform, "Choice_RightMood", origin + new Vector3(22f, 2f, 0f), new Vector3(12f, 8f, 16f), new Color(1f, 0.86f, 0.78f, 1f), -0.12f, 0.32f, 0.24f);
        GameObject rewriteMood = CreateLocalMoodVolume(postRoot.transform, "Choice_RewriteMood", origin + new Vector3(0f, 4.7f, 18f), new Vector3(12f, 8f, 12f), new Color(0.88f, 0.8f, 1f, 1f), -0.24f, 0.5f, 0.34f);
        rewriteMood.SetActive(false);

        // Choice-driven psychogeography (react to milestones)
        GameObject leftBridge = CreatePrimitive(PrimitiveType.Cube, "LeftChoiceBridge", origin + new Vector3(-13f, upperFloorY + 0.01f, -3.5f), new Vector3(2.6f, 0.2f, 2f), root.transform, accentMaterial, false);
        GameObject rightBridge = CreatePrimitive(PrimitiveType.Cube, "RightChoiceBridge", origin + new Vector3(13f, upperFloorY + 0.01f, -3.5f), new Vector3(2.6f, 0.2f, 2f), root.transform, accentMaterial, false);
        GameObject leftCollapse = CreatePrimitive(PrimitiveType.Cube, "LeftCollapseBlock", origin + new Vector3(-11f, 1f, 0f), new Vector3(2f, 2f, 2f), root.transform, darkMaterial, false);
        GameObject rightCollapse = CreatePrimitive(PrimitiveType.Cube, "RightCollapseBlock", origin + new Vector3(11f, 1f, 0f), new Vector3(2f, 2f, 2f), root.transform, darkMaterial, false);
        GameObject leftRouteSignal = CreateAnimatedBeacon(root.transform, origin + new Vector3(-20f, 0f, -4.5f), "LeftRouteSignal", new Color(0.56f, 0.84f, 1f, 1f), 0.9f);
        GameObject rightRouteSignal = CreateAnimatedBeacon(root.transform, origin + new Vector3(20f, 0f, -4.5f), "RightRouteSignal", new Color(1f, 0.72f, 0.54f, 1f), 0.9f);
        GameObject rewriteHalo = CreateAnimatedBeacon(root.transform, origin + new Vector3(0f, upperFloorY + 0.05f, 22.4f), "RewriteHalo", new Color(0.84f, 0.7f, 1f, 1f), 1.1f);
        leftBridge.SetActive(false);
        rightBridge.SetActive(false);
        leftCollapse.SetActive(false);
        rightCollapse.SetActive(false);
        leftRouteSignal.SetActive(false);
        rightRouteSignal.SetActive(false);
        rewriteHalo.SetActive(false);

        Transform leftPickup = FindTransformRecursive(root.transform, "LeftWardBadgePickup");
        Transform rightPickup = FindTransformRecursive(root.transform, "RightWardBadgePickup");
        Transform leftDoor = FindTransformRecursive(root.transform, "WestBranchDoor_Frame");
        Transform rightDoor = FindTransformRecursive(root.transform, "EastBranchDoor_Frame");

        GameObject directorObject = new GameObject("ChoiceStateDirector");
        directorObject.transform.SetParent(root.transform);
        ChoiceWorldStateDirector director = directorObject.AddComponent<ChoiceWorldStateDirector>();
        director.Configure(
            true,
            new[] { "choice_left_pick", "choice_right_pick" },
            new[]
            {
                new ChoiceWorldStateDirector.WorldReaction
                {
                    milestoneId = "choice_left_pick",
                    consumeOnce = true,
                    activate = new[] { leftBridge, rightCollapse, leftMood, leftRouteSignal },
                    deactivate = new[] { rightPickup != null ? rightPickup.gameObject : null, rightDoor != null ? rightDoor.gameObject : null, rightMood, rightRouteSignal },
                    feedbackMessage = "Left route chosen. East route hardens against you.",
                    feedbackDuration = 3.4f
                },
                new ChoiceWorldStateDirector.WorldReaction
                {
                    milestoneId = "choice_right_pick",
                    consumeOnce = true,
                    activate = new[] { rightBridge, leftCollapse, rightMood, rightRouteSignal },
                    deactivate = new[] { leftPickup != null ? leftPickup.gameObject : null, leftDoor != null ? leftDoor.gameObject : null, leftMood, leftRouteSignal },
                    feedbackMessage = "Right route chosen. West route retracts.",
                    feedbackDuration = 3.4f
                },
                new ChoiceWorldStateDirector.WorldReaction
                {
                    milestoneId = "hub_rewrite",
                    consumeOnce = true,
                    activate = new[] { rewriteMood, rewriteHalo },
                    deactivate = new GameObject[] { },
                    feedbackMessage = "Topology rewritten. Your path authored the district's mood.",
                    feedbackDuration = 4.2f
                }
            });
    }

    private static void CreateCheckpointZone(Transform parent, string name, Vector3 position, Vector3 size, string message)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent);
        zone.transform.position = position;
        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        CheckpointZone checkpoint = zone.AddComponent<CheckpointZone>();
        checkpoint.Configure(message, true);
    }

    private static GameObject CreateSpatialAudioAnchor(Transform parent, Vector3 position, string name)
    {
        GameObject anchor = new GameObject(name);
        anchor.transform.SetParent(parent);
        anchor.transform.position = position;
        AudioSource source = anchor.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 2f;
        source.maxDistance = 36f;
        source.volume = 0.6f;
        source.dopplerLevel = 0f;
        source.priority = 140;
        return anchor;
    }

    private static VolumeProfile CreateMoodProfile(string profileName, Color filter, float exposure, float bloomIntensity, float vignetteIntensity)
    {
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = profileName;
        ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
        colorAdjustments.active = true;
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = filter;
        colorAdjustments.postExposure.overrideState = true;
        colorAdjustments.postExposure.value = exposure;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.contrast.value = 12f;
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.value = 8f;

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = bloomIntensity;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.88f;
        bloom.tint.overrideState = true;
        bloom.tint.value = new Color(1f, 0.96f, 0.92f, 1f);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = vignetteIntensity;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.42f;

        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.active = true;
        tonemapping.mode.overrideState = true;
        tonemapping.mode.value = TonemappingMode.ACES;

        return profile;
    }

    private static GameObject CreateLocalMoodVolume(Transform parent, string name, Vector3 position, Vector3 size, Color filter, float exposure, float bloom, float vignette)
    {
        GameObject volumeObject = new GameObject(name);
        volumeObject.transform.SetParent(parent);
        volumeObject.transform.position = position;
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = false;
        volume.priority = 8f;
        volume.blendDistance = 1.5f;
        volume.weight = 0.65f;
        volume.profile = CreateMoodProfile(name + "_Profile", filter, exposure, bloom, vignette);
        BoxCollider box = volumeObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        return volumeObject;
    }

    private static void CreateVisualDetailPass(Transform parent, Vector3 origin)
    {
        GameObject detailRoot = new GameObject("Choice_Detail");
        detailRoot.transform.SetParent(parent);
        const float upperFloorY = 3.2f;
        const float railHeight = 1.05f;
        float railY = upperFloorY + railHeight;
        float railPostCenterY = upperFloorY + (railHeight * 0.5f);

        // Catwalk rails are now aligned to deck height and anchored with posts.
        CreateDetailPrimitive(PrimitiveType.Cube, "Rail_West_A", origin + new Vector3(-10f, railY, -1.02f), new Vector3(7.2f, 0.08f, 0.08f), detailRoot.transform, darkMaterial, true);
        CreateDetailPrimitive(PrimitiveType.Cube, "Rail_West_B", origin + new Vector3(-10f, railY, 1.02f), new Vector3(7.2f, 0.08f, 0.08f), detailRoot.transform, darkMaterial, true);
        CreateDetailPrimitive(PrimitiveType.Cube, "Rail_East_A", origin + new Vector3(10f, railY, -1.02f), new Vector3(7.2f, 0.08f, 0.08f), detailRoot.transform, darkMaterial, true);
        CreateDetailPrimitive(PrimitiveType.Cube, "Rail_East_B", origin + new Vector3(10f, railY, 1.02f), new Vector3(7.2f, 0.08f, 0.08f), detailRoot.transform, darkMaterial, true);
        CreateDetailPrimitive(PrimitiveType.Cube, "Rail_North_A", origin + new Vector3(-1.02f, railY, 10f), new Vector3(0.08f, 0.08f, 7.2f), detailRoot.transform, darkMaterial, true);
        CreateDetailPrimitive(PrimitiveType.Cube, "Rail_North_B", origin + new Vector3(1.02f, railY, 10f), new Vector3(0.08f, 0.08f, 7.2f), detailRoot.transform, darkMaterial, true);

        float[] sidePostX = { -13.6f, -11.2f, -8.8f, -6.4f };
        for (int i = 0; i < sidePostX.Length; i++)
        {
            float x = sidePostX[i];
            CreateDetailPrimitive(PrimitiveType.Cube, "RailPost_West_" + i + "_A", origin + new Vector3(x, railPostCenterY, -0.98f), new Vector3(0.1f, railHeight, 0.1f), detailRoot.transform, darkMaterial, true);
            CreateDetailPrimitive(PrimitiveType.Cube, "RailPost_West_" + i + "_B", origin + new Vector3(x, railPostCenterY, 0.98f), new Vector3(0.1f, railHeight, 0.1f), detailRoot.transform, darkMaterial, true);
            CreateDetailPrimitive(PrimitiveType.Cube, "CatwalkSupport_West_" + i, origin + new Vector3(x, 1.6f, 0f), new Vector3(0.18f, 3.2f, 0.18f), detailRoot.transform, darkMaterial, true);
        }

        float[] sidePostXE = { 6.4f, 8.8f, 11.2f, 13.6f };
        for (int i = 0; i < sidePostXE.Length; i++)
        {
            float x = sidePostXE[i];
            CreateDetailPrimitive(PrimitiveType.Cube, "RailPost_East_" + i + "_A", origin + new Vector3(x, railPostCenterY, -0.98f), new Vector3(0.1f, railHeight, 0.1f), detailRoot.transform, darkMaterial, true);
            CreateDetailPrimitive(PrimitiveType.Cube, "RailPost_East_" + i + "_B", origin + new Vector3(x, railPostCenterY, 0.98f), new Vector3(0.1f, railHeight, 0.1f), detailRoot.transform, darkMaterial, true);
            CreateDetailPrimitive(PrimitiveType.Cube, "CatwalkSupport_East_" + i, origin + new Vector3(x, 1.6f, 0f), new Vector3(0.18f, 3.2f, 0.18f), detailRoot.transform, darkMaterial, true);
        }

        float[] northPostZ = { 6.4f, 8.8f, 11.2f, 13.6f };
        for (int i = 0; i < northPostZ.Length; i++)
        {
            float z = northPostZ[i];
            CreateDetailPrimitive(PrimitiveType.Cube, "RailPost_North_" + i + "_A", origin + new Vector3(-0.98f, railPostCenterY, z), new Vector3(0.1f, railHeight, 0.1f), detailRoot.transform, darkMaterial, true);
            CreateDetailPrimitive(PrimitiveType.Cube, "RailPost_North_" + i + "_B", origin + new Vector3(0.98f, railPostCenterY, z), new Vector3(0.1f, railHeight, 0.1f), detailRoot.transform, darkMaterial, true);
            CreateDetailPrimitive(PrimitiveType.Cube, "CatwalkSupport_North_" + i, origin + new Vector3(0f, 1.6f, z), new Vector3(0.18f, 3.2f, 0.18f), detailRoot.transform, darkMaterial, true);
        }

        // Ceiling truss grid sits directly under roof slab and ties into perimeter walls.
        for (int i = 0; i < 9; i++)
        {
            float x = -24f + (i * 6f);
            CreateDetailPrimitive(PrimitiveType.Cube, "CeilingTruss_" + i, origin + new Vector3(x, 6.02f, 0f), new Vector3(0.16f, 0.16f, 53.2f), detailRoot.transform, metalMaterial, false);
        }
        CreateDetailPrimitive(PrimitiveType.Cube, "CeilingPerimeter_West", origin + new Vector3(-26.2f, 6.06f, 0f), new Vector3(0.24f, 0.24f, 53.2f), detailRoot.transform, metalMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, "CeilingPerimeter_East", origin + new Vector3(26.2f, 6.06f, 0f), new Vector3(0.24f, 0.24f, 53.2f), detailRoot.transform, metalMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, "CeilingPerimeter_South", origin + new Vector3(0f, 6.06f, -26.2f), new Vector3(53.2f, 0.24f, 0.24f), detailRoot.transform, metalMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, "CeilingPerimeter_North", origin + new Vector3(0f, 6.06f, 26.2f), new Vector3(53.2f, 0.24f, 0.24f), detailRoot.transform, metalMaterial, false);

        // Strong floor readability for isometric navigation.
        CreateDetailPrimitive(PrimitiveType.Cube, "FloorGuide_Entry", origin + new Vector3(0f, 0.13f, -14f), new Vector3(8f, 0.03f, 0.22f), detailRoot.transform, accentMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, "FloorGuide_West", origin + new Vector3(-14f, 0.13f, 0f), new Vector3(0.22f, 0.03f, 8f), detailRoot.transform, accentMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, "FloorGuide_East", origin + new Vector3(14f, 0.13f, 0f), new Vector3(0.22f, 0.03f, 8f), detailRoot.transform, accentMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, "FloorGuide_North", origin + new Vector3(0f, 0.13f, 14f), new Vector3(8f, 0.03f, 0.22f), detailRoot.transform, accentMaterial, false);

        // Ventilation fans are attached to roof trusses instead of hovering at arbitrary height.
        CreateAnimatedVentFan(detailRoot.transform, origin + new Vector3(-20f, 5.95f, -8f), "Fan_West");
        CreateAnimatedVentFan(detailRoot.transform, origin + new Vector3(20f, 5.95f, -8f), "Fan_East");
        CreateAnimatedVentFan(detailRoot.transform, origin + new Vector3(0f, 5.95f, 22f), "Fan_North");
    }

    private static void CreateAnimatedVentFan(Transform parent, Vector3 position, string name)
    {
        GameObject fanRoot = new GameObject(name);
        fanRoot.transform.SetParent(parent);
        fanRoot.transform.position = position;

        CreateDetailPrimitive(PrimitiveType.Cylinder, name + "_Housing", position, new Vector3(0.8f, 0.18f, 0.8f), fanRoot.transform, metalMaterial, false);
        GameObject rotor = CreateDetailPrimitive(PrimitiveType.Cube, name + "_Rotor", position + new Vector3(0f, 0.02f, 0f), new Vector3(1.2f, 0.05f, 0.14f), fanRoot.transform, darkMaterial, false);
        CreateDetailPrimitive(PrimitiveType.Cube, name + "_RotorCross", position + new Vector3(0f, 0.02f, 0f), new Vector3(0.14f, 0.05f, 1.2f), fanRoot.transform, darkMaterial, false);

        TutorialAmbientMotion motion = rotor.AddComponent<TutorialAmbientMotion>();
        SerializedObject so = new SerializedObject(motion);
        so.FindProperty("bobAmplitude").vector3Value = Vector3.zero;
        so.FindProperty("bobSpeed").floatValue = 0.3f;
        so.FindProperty("yawSpeed").floatValue = 240f;
        so.FindProperty("positionSmoothing").floatValue = 20f;
        so.FindProperty("rotationSmoothing").floatValue = 20f;
        so.FindProperty("randomizePhaseOnAwake").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateAnimatedBeacon(Transform parent, Vector3 position, string name, Color lightColor, float scale)
    {
        GameObject beacon = new GameObject(name);
        beacon.transform.SetParent(parent);
        beacon.transform.position = position;

        CreateDetailPrimitive(PrimitiveType.Cylinder, name + "_Base", position + new Vector3(0f, 0.22f, 0f), new Vector3(0.36f, 0.22f, 0.36f), beacon.transform, metalMaterial, true);
        GameObject globe = CreateDetailPrimitive(PrimitiveType.Sphere, name + "_Glow", position + new Vector3(0f, 0.62f, 0f), Vector3.one * (0.28f * scale), beacon.transform, accentMaterial, false);
        Renderer renderer = globe.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = GetEmissiveMaterial(name + "_Emissive", lightColor * 1.25f);
        }

        Light pulseLight = beacon.AddComponent<Light>();
        pulseLight.type = LightType.Point;
        pulseLight.range = 7.5f * scale;
        pulseLight.intensity = 2.2f;
        pulseLight.color = lightColor;
        pulseLight.shadows = LightShadows.None;

        TutorialAmbientMotion motion = beacon.AddComponent<TutorialAmbientMotion>();
        SerializedObject so = new SerializedObject(motion);
        so.FindProperty("bobAmplitude").vector3Value = new Vector3(0f, 0.06f * scale, 0f);
        so.FindProperty("bobSpeed").floatValue = 1.1f;
        so.FindProperty("yawSpeed").floatValue = 22f;
        so.FindProperty("targetLight").objectReferenceValue = pulseLight;
        so.FindProperty("pulseAmplitude").floatValue = 0.45f;
        so.FindProperty("pulseSpeed").floatValue = 1.6f;
        so.ApplyModifiedPropertiesWithoutUndo();

        return beacon;
    }
}
