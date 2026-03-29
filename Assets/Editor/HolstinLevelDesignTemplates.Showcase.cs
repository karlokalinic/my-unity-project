// ============================================================
// FILE: HolstinLevelDesignTemplates.Showcase.cs
// Gigantic feature-showcase tutorial scene generator
// ============================================================
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class HolstinLevelDesignTemplates
{
    [MenuItem("Tools/Holstin Level Design Templates/Create Gigantic Tutorial Showcase Scene", false, 9)]
    public static void CreateGiganticTutorialShowcaseScene()
    {
        if (!EditorUtility.DisplayDialog(
                "Create Gigantic Tutorial Showcase",
                "This creates a large authored scene that chains main tutorial flow + three sidequests.\n\n" +
                "Included: connected interior/exterior/underpass, branching NPC gates, shop lane, combat lane, " +
                "inventory + key usage, and tutorial tip zones. Continue?",
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
        CreateSkyGround(new Vector3(16f, -0.1f, 18f), new Vector3(220f, 0.2f, 180f), "ShowcaseGround");
        EnsureCoreRig(new Vector3(-10f, 1.2f, -10f));

        Vector3 sandboxOrigin = new Vector3(0f, 0f, 0f);
        Vector3 exteriorOrigin = new Vector3(-70f, 0f, -12f);
        Vector3 interiorOrigin = new Vector3(70f, 0f, -8f);
        Vector3 underpassOrigin = new Vector3(66f, -5.5f, 74f);
        Vector3 sideQuestOrigin = sandboxOrigin + new Vector3(46f, 0f, 0f);

        CreateInteractableSandbox(sandboxOrigin);
        CreateFogCourtyardExterior(exteriorOrigin);
        CreateBoardingHouseInterior(interiorOrigin);
        CreateUnderpassTemplate(underpassOrigin);
        CreateShowcaseConnectorNetwork(sandboxOrigin, exteriorOrigin, interiorOrigin, underpassOrigin, sideQuestOrigin);
        CreateShowcaseSideQuestDistrict(sideQuestOrigin);
        CreateGiganticTutorialTips(sandboxOrigin, exteriorOrigin, interiorOrigin, underpassOrigin, sideQuestOrigin);

        EnsureVerticalSliceBootstrap();
        ApplyProductionAestheticPassInternal();

        const string scenePath = "Assets/Scenes/Tutorial_Showcase_Gigantic.unity";
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath, true);
        AssetDatabase.Refresh();
        FinalizeScene(
            "Gigantic tutorial showcase scene created at Assets/Scenes/Tutorial_Showcase_Gigantic.unity. " +
            "Main flow: sandbox objective chain. Sidequests unlock after milestone 'console_service_unlock'.");
    }

    private static void CreateShowcaseConnectorNetwork(
        Vector3 sandboxOrigin,
        Vector3 exteriorOrigin,
        Vector3 interiorOrigin,
        Vector3 underpassOrigin,
        Vector3 sideQuestOrigin)
    {
        GameObject root = NewRoot("Template_Showcase_Connectors", Vector3.zero);

        CreateWalkwayConnector(
            root.transform,
            sandboxOrigin + new Vector3(-10f, 0f, -2.4f),
            exteriorOrigin + new Vector3(25f, 0f, -2.4f),
            4.6f,
            "Connector_Sandbox_Exterior");

        CreateWalkwayConnector(
            root.transform,
            sandboxOrigin + new Vector3(30f, 0f, 1.2f),
            interiorOrigin + new Vector3(-8f, 0f, 1.2f),
            4.8f,
            "Connector_Sandbox_Interior");

        CreateWalkwayConnector(
            root.transform,
            interiorOrigin + new Vector3(8f, 0f, 4f),
            underpassOrigin + new Vector3(-8f, 5.5f, 2f),
            4.4f,
            "Connector_Interior_Underpass");

        CreateWalkwayConnector(
            root.transform,
            sandboxOrigin + new Vector3(30f, 0f, -3.2f),
            sideQuestOrigin + new Vector3(-14f, 0f, -3.2f),
            4.2f,
            "Connector_Sandbox_SideQuests");
    }

    private static void CreateWalkwayConnector(
        Transform parent,
        Vector3 from,
        Vector3 to,
        float width,
        string name)
    {
        Vector3 delta = to - from;
        float length = Mathf.Max(0.5f, delta.magnitude);
        Vector3 center = from + delta * 0.5f;
        Quaternion rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);

        GameObject deck = CreateFloor(parent, name + "_Deck", center + new Vector3(0f, -0.06f, 0f), new Vector3(width, 0.22f, length), floorMaterial);
        deck.transform.rotation = rotation;

        float railHalfOffset = (width * 0.5f) - 0.12f;
        Vector3 right = rotation * Vector3.right;

        GameObject railA = CreateWall(parent, center + right * railHalfOffset + new Vector3(0f, 0.55f, 0f), new Vector3(0.2f, 1.1f, length), darkMaterial, name + "_RailA");
        railA.transform.rotation = rotation;

        GameObject railB = CreateWall(parent, center - right * railHalfOffset + new Vector3(0f, 0.55f, 0f), new Vector3(0.2f, 1.1f, length), darkMaterial, name + "_RailB");
        railB.transform.rotation = rotation;
    }

    private static void CreateShowcaseSideQuestDistrict(Vector3 origin)
    {
        GameObject root = NewRoot("Template_SideQuest_District", origin);

        CreateFloor(root.transform, "SQD_Floor", origin + new Vector3(6f, -0.05f, 0f), new Vector3(30f, 0.2f, 12f), floorMaterial);
        CreateWall(root.transform, origin + new Vector3(-9f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 12f), wallMaterial, "SQD_WestWall");
        CreateWall(root.transform, origin + new Vector3(21f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 12f), wallMaterial, "SQD_EastWall");
        CreateWall(root.transform, origin + new Vector3(6f, 1.6f, 6f), new Vector3(30f, 3.2f, 0.35f), wallMaterial, "SQD_NorthWall");
        CreateWall(root.transform, origin + new Vector3(6f, 1.6f, -6f), new Vector3(30f, 3.2f, 0.35f), wallMaterial, "SQD_SouthWall");
        CreatePartitionWithOpening(root.transform, origin + new Vector3(-1.5f, 0f, 0f), "SQD_Partition_A");
        CreatePartitionWithOpening(root.transform, origin + new Vector3(8f, 0f, 0f), "SQD_Partition_B");

        // Sidequest 1 (unlocked by first main objective)
        CreatePickupKeyWithMilestoneGate(
            root.transform,
            origin + new Vector3(-6.8f, 0f, 2.1f),
            "SQ1_MedCachePass",
            "med_cache_pass",
            "Medical Cache Pass",
            "Stamped pass that opens the field clinic locker.",
            "sq1_pass_found",
            "console_service_unlock",
            "Finish the main relay objective first.");

        CreateMilestoneGatedSkillNpc(
            root.transform,
            origin + new Vector3(-4.8f, 0f, -1.3f),
            "Field Medic",
            "Runs triage in silence and keeps the useful survivors moving.",
            "console_service_unlock",
            "You are cleared from the relay. Need treatment or favors?",
            "Request clinical support",
            "Take this med pack and keep moving.",
            "You are not cleared. Finish the relay objective first.",
            "med_pack",
            "sidequest_medic_complete",
            35);

        // Sidequest 2 (inventory usage + shop)
        CreateLockedDoor(
            root.transform,
            origin + new Vector3(-1.5f, 0f, 0f),
            "SQ2_ClinicDoor",
            "med_pack",
            "Med Pack",
            "The clinic door lock requires triage authorization.",
            "sq2_clinic_opened");

        GameObject marketBarrier = CreatePrimitive(
            PrimitiveType.Cube,
            "SQ2_MarketBarrier",
            origin + new Vector3(6.9f, 1.1f, 1.35f),
            new Vector3(0.22f, 2.2f, 1.35f),
            root.transform,
            accentMaterial,
            false);

        GameObject marketBeacon = new GameObject("SQ2_MarketBeacon");
        marketBeacon.transform.SetParent(root.transform);
        marketBeacon.transform.position = origin + new Vector3(6.4f, 2.2f, -2.2f);
        Light marketLight = marketBeacon.AddComponent<Light>();
        marketLight.type = LightType.Point;
        marketLight.range = 7f;
        marketLight.intensity = 4.2f;
        marketLight.color = new Color(0.95f, 0.72f, 0.45f, 1f);
        marketBeacon.SetActive(false);

        CreatePickupKeyWithMilestoneGate(
            root.transform,
            origin + new Vector3(2.6f, 0f, 2.3f),
            "SQ2_MarketToken",
            "market_token",
            "Market Token",
            "A stamped token for restricted exchange lanes.",
            "sq2_token_found",
            "sidequest_medic_complete",
            "Talk to the Field Medic first.");

        CreateServiceConsole(
            root.transform,
            origin + new Vector3(3.7f, 0f, -2.35f),
            "SQ2_MarketGateConsole",
            "market_token",
            "Market Token",
            marketBarrier,
            marketBeacon,
            true,
            "Token accepted. Market wing unlocked.",
            "This terminal expects a market token.",
            "sidequest_market_complete");

        CreateShopMerchant(
            root.transform,
            origin + new Vector3(7.8f, 0f, 1.2f),
            "Quartermaster",
            "Trades practical tools and bad advice in equal measure.");

        // Sidequest 3 (combat + signal objective)
        CreateMilestoneGatedSkillNpc(
            root.transform,
            origin + new Vector3(12.6f, 0f, -1.5f),
            "Signal Operator",
            "Runs emergency channel routing and expects clean execution.",
            "sidequest_market_complete",
            "Market is open. Ready for a field relay task?",
            "Take relay battery assignment",
            "Good. Carry this battery to the uplink node.",
            "Open the market wing first.",
            "signal_battery",
            "sidequest_signal_ready",
            50);

        GameObject uplinkBarrier = CreatePrimitive(
            PrimitiveType.Cube,
            "SQ3_UplinkBarrier",
            origin + new Vector3(17.1f, 1.1f, 0f),
            new Vector3(0.22f, 2.2f, 2.4f),
            root.transform,
            accentMaterial,
            false);

        GameObject uplinkReward = CreatePrimitive(
            PrimitiveType.Cube,
            "SQ3_UplinkCache",
            origin + new Vector3(19.3f, 0.55f, 2.5f),
            new Vector3(0.8f, 1.1f, 0.8f),
            root.transform,
            metalMaterial,
            false);
        uplinkReward.SetActive(false);

        CreateServiceConsole(
            root.transform,
            origin + new Vector3(15.3f, 0f, -2.2f),
            "SQ3_UplinkConsole",
            "signal_battery",
            "Signal Battery",
            uplinkBarrier,
            uplinkReward,
            true,
            "Uplink restored. Cache unlocked.",
            "The uplink needs a charged battery.",
            "sidequest_signal_complete");

        CreateEnemyPatrol(
            root.transform,
            origin + new Vector3(18.3f, 0f, 0f),
            "Signal Stalker",
            new[]
            {
                origin + new Vector3(16.8f, 0f, 2.8f),
                origin + new Vector3(19.2f, 0f, 2.8f),
                origin + new Vector3(19.2f, 0f, -2.8f),
                origin + new Vector3(16.8f, 0f, -2.8f)
            });

        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-5f, 1f, 0f), new Vector3(7f, 2f, 9f), "Sidequest Bay A", "Field triage support opens only after the relay objective.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(5f, 1f, 0f), new Vector3(7f, 2f, 9f), "Sidequest Bay B", "Inventory-gated market lane. Spend resources to progress.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(15f, 1f, 0f), new Vector3(7f, 2f, 9f), "Sidequest Bay C", "Signal uplink objective with combat pressure.");

        CreateCameraZone(root.transform, origin + new Vector3(-5f, 1f, 0f), new Vector3(8f, 2f, 9f), 45f, "SQ_Camera_A");
        CreateCameraZone(root.transform, origin + new Vector3(5f, 1f, 0f), new Vector3(8f, 2f, 9f), 135f, "SQ_Camera_B");
        CreateCameraZone(root.transform, origin + new Vector3(15f, 1f, 0f), new Vector3(8f, 2f, 9f), 45f, "SQ_Camera_C");
    }

    private static void CreatePickupKeyWithMilestoneGate(
        Transform parent,
        Vector3 pos,
        string rootName,
        string itemId,
        string itemDisplayName,
        string description,
        string pickupMilestone,
        string requiredMilestone,
        string requiredMessage)
    {
        GameObject keyRoot = new GameObject(rootName.Replace(" ", ""));
        keyRoot.transform.SetParent(parent);
        keyRoot.transform.position = pos + new Vector3(0f, 0.82f, 0f);

        AssetPlaceholder ph = keyRoot.AddComponent<AssetPlaceholder>();
        ph.Configure(AssetPlaceholder.PlaceholderCategory.Prop, "key_item", new Vector3(0.3f, 0.1f, 0.15f));

        BoxCollider col = keyRoot.AddComponent<BoxCollider>();
        col.size = new Vector3(0.5f, 0.18f, 0.22f);

        PickupInteractable pickup = keyRoot.AddComponent<PickupInteractable>();
        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("itemId").stringValue = itemId;
        so.FindProperty("itemDisplayName").stringValue = itemDisplayName;
        so.FindProperty("pickupDescription").stringValue = description;
        SerializedProperty pickupMilestoneProp = so.FindProperty("infectionMilestoneOnPickup");
        if (pickupMilestoneProp != null) pickupMilestoneProp.stringValue = pickupMilestone;
        SerializedProperty requiredMilestoneProp = so.FindProperty("requiredMilestoneId");
        if (requiredMilestoneProp != null) requiredMilestoneProp.stringValue = requiredMilestone;
        SerializedProperty requiredMessageProp = so.FindProperty("requiredMilestoneMessage");
        if (requiredMessageProp != null) requiredMessageProp.stringValue = requiredMessage;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateMilestoneGatedSkillNpc(
        Transform parent,
        Vector3 pos,
        string npcName,
        string narrativeRole,
        string requiredMilestone,
        string promptLine,
        string choiceText,
        string successResponse,
        string lockedResponse,
        string grantItemId,
        string completionMilestone,
        int xpReward)
    {
        Material mat = npcMaterial ?? GetMaterial(ref npcMaterial, "NPC", new Color(0.55f, 0.58f, 0.62f));
        GameObject npc = CreatePrimitive(
            PrimitiveType.Capsule,
            npcName.Replace(" ", ""),
            pos + new Vector3(0f, 1f, 0f),
            new Vector3(0.8f, 1f, 0.8f),
            parent,
            mat,
            false);

        NpcIdentity identity = npc.AddComponent<NpcIdentity>();
        identity.Configure(npcName, narrativeRole, FindPlayer()?.transform);
        npc.AddComponent<NpcDialogueController>();

        SkillCheckNpcInteractable interactable = npc.AddComponent<SkillCheckNpcInteractable>();
        SerializedObject so = new SerializedObject(interactable);

        SerializedProperty firstEncounter = so.FindProperty("firstEncounter");
        ConfigureMilestoneGatedDialogueNode(
            firstEncounter,
            npcName,
            promptLine,
            requiredMilestone,
            choiceText,
            successResponse,
            lockedResponse,
            grantItemId,
            completionMilestone,
            xpReward);

        SerializedProperty repeatEncounter = so.FindProperty("repeatEncounter");
        if (repeatEncounter != null)
        {
            SerializedProperty repeatSpeaker = repeatEncounter.FindPropertyRelative("speakerName");
            if (repeatSpeaker != null) repeatSpeaker.stringValue = npcName;

            SerializedProperty repeatPrompt = repeatEncounter.FindPropertyRelative("promptLine");
            if (repeatPrompt != null) repeatPrompt.stringValue = "Mission already assigned. Keep pressure on objectives.";

            SerializedProperty repeatChoices = repeatEncounter.FindPropertyRelative("choices");
            if (repeatChoices != null)
            {
                repeatChoices.arraySize = 1;
                SerializedProperty leaveChoice = repeatChoices.GetArrayElementAtIndex(0);
                ConfigureLeaveChoice(leaveChoice, "Stand down.");
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureMilestoneGatedDialogueNode(
        SerializedProperty node,
        string speakerName,
        string promptLine,
        string requiredMilestone,
        string choiceText,
        string successResponse,
        string lockedResponse,
        string grantItemId,
        string completionMilestone,
        int xpReward)
    {
        if (node == null)
        {
            return;
        }

        SerializedProperty speaker = node.FindPropertyRelative("speakerName");
        if (speaker != null) speaker.stringValue = speakerName;

        SerializedProperty prompt = node.FindPropertyRelative("promptLine");
        if (prompt != null) prompt.stringValue = promptLine;

        SerializedProperty choices = node.FindPropertyRelative("choices");
        if (choices == null)
        {
            return;
        }

        choices.arraySize = 2;

        SerializedProperty questChoice = choices.GetArrayElementAtIndex(0);
        if (questChoice != null)
        {
            SerializedProperty choiceData = questChoice.FindPropertyRelative("choice");
            if (choiceData != null)
            {
                SerializedProperty text = choiceData.FindPropertyRelative("text");
                if (text != null) text.stringValue = choiceText;

                SerializedProperty response = choiceData.FindPropertyRelative("responseLine");
                if (response != null) response.stringValue = successResponse;

                SerializedProperty milestone = choiceData.FindPropertyRelative("milestoneId");
                if (milestone != null) milestone.stringValue = completionMilestone;

                SerializedProperty isLeave = choiceData.FindPropertyRelative("isLeave");
                if (isLeave != null) isLeave.boolValue = false;
            }

            SerializedProperty requirement = questChoice.FindPropertyRelative("requirement");
            if (requirement != null)
            {
                SerializedProperty gateType = requirement.FindPropertyRelative("gateType");
                if (gateType != null) gateType.enumValueIndex = 4; // HasMilestone

                SerializedProperty reqId = requirement.FindPropertyRelative("requiredId");
                if (reqId != null) reqId.stringValue = requiredMilestone;

                SerializedProperty locked = requirement.FindPropertyRelative("lockedLabel");
                if (locked != null) locked.stringValue = "[Main objective first]";
            }

            SerializedProperty failure = questChoice.FindPropertyRelative("failureResponse");
            if (failure != null) failure.stringValue = lockedResponse;

            SerializedProperty xp = questChoice.FindPropertyRelative("experienceReward");
            if (xp != null) xp.intValue = Mathf.Max(0, xpReward);

            SerializedProperty grantItem = questChoice.FindPropertyRelative("grantItemId");
            if (grantItem != null) grantItem.stringValue = grantItemId;
        }

        SerializedProperty leave = choices.GetArrayElementAtIndex(1);
        ConfigureLeaveChoice(leave, "Leave conversation.");
    }

    private static void ConfigureLeaveChoice(SerializedProperty choiceProperty, string textValue)
    {
        if (choiceProperty == null)
        {
            return;
        }

        SerializedProperty choiceData = choiceProperty.FindPropertyRelative("choice");
        if (choiceData != null)
        {
            SerializedProperty text = choiceData.FindPropertyRelative("text");
            if (text != null) text.stringValue = textValue;

            SerializedProperty response = choiceData.FindPropertyRelative("responseLine");
            if (response != null) response.stringValue = string.Empty;

            SerializedProperty isLeave = choiceData.FindPropertyRelative("isLeave");
            if (isLeave != null) isLeave.boolValue = true;
        }

        SerializedProperty requirement = choiceProperty.FindPropertyRelative("requirement");
        if (requirement != null)
        {
            SerializedProperty gateType = requirement.FindPropertyRelative("gateType");
            if (gateType != null) gateType.enumValueIndex = 0; // None
        }
    }

    private static void CreateGiganticTutorialTips(
        Vector3 sandboxOrigin,
        Vector3 exteriorOrigin,
        Vector3 interiorOrigin,
        Vector3 underpassOrigin,
        Vector3 sideQuestOrigin)
    {
        GameObject root = NewRoot("Template_Showcase_TutorialTips", Vector3.zero);

        CreateNarrativeZoneBox(
            root.transform,
            sandboxOrigin + new Vector3(-7f, 1f, 0f),
            new Vector3(5.8f, 2f, 5.8f),
            "Tutorial Tip: Core Controls",
            "WASD move, Space jump, RMB hold for first-person transition, F flashlight, Tab inventory.");

        CreateNarrativeZoneBox(
            root.transform,
            sandboxOrigin + new Vector3(26f, 1f, 0f),
            new Vector3(5f, 2f, 5f),
            "Tutorial Tip: Main Objective",
            "Complete relay and console objectives first. Sidequest district opens after console_service_unlock.");

        CreateNarrativeZoneBox(
            root.transform,
            exteriorOrigin + new Vector3(8f, 1f, -2f),
            new Vector3(6f, 2f, 6f),
            "Tutorial Tip: Exterior Routing",
            "Use camera zones to keep readability. Exterior is scouting, not heavy combat.");

        CreateNarrativeZoneBox(
            root.transform,
            interiorOrigin + new Vector3(0f, 1f, 1f),
            new Vector3(7f, 2f, 6f),
            "Tutorial Tip: Interior Clearance",
            "Doors and inventory gates teach progression beats. Keep traversal lanes wide and unclipped.");

        CreateNarrativeZoneBox(
            root.transform,
            underpassOrigin + new Vector3(7f, 1f, 0f),
            new Vector3(8f, 2f, 7f),
            "Tutorial Tip: Combat + Atmosphere",
            "Underpass is pressure-space: low visibility, combat patrols, and directional lighting cues.");

        CreateNarrativeZoneBox(
            root.transform,
            sideQuestOrigin + new Vector3(6f, 1f, 0f),
            new Vector3(30f, 2f, 10f),
            "Tutorial Tip: Sidequest Chain",
            "SQ1 unlocks SQ2, SQ2 unlocks SQ3. Each quest reinforces inventory, dialogue, and combat systems.");
    }
}
