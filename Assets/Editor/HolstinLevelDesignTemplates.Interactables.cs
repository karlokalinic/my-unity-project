// ============================================================
// FILE: HolstinLevelDesignTemplates.Interactables.cs
// PickupKey, LockedDoor, KeyGiverNPC, ServiceConsole
// ============================================================
using UnityEditor;
using UnityEngine;

public static partial class HolstinLevelDesignTemplates
{
    private static void CreatePickupKey(
        Transform parent,
        Vector3 pos,
        string rootName,
        string itemId,
        string itemDisplayName,
        string description,
        string infectionMilestone = "")
    {
        CreatePrimitive(PrimitiveType.Cube, rootName + "_Pedestal", pos + new Vector3(0f, 0.35f, 0f), new Vector3(0.7f, 0.7f, 0.7f), parent, floorMaterial);
        GameObject keyRoot = new GameObject(rootName.Replace(" ", ""));
        keyRoot.transform.SetParent(parent);
        keyRoot.transform.position = pos + new Vector3(0f, 0.82f, 0f);
        CreatePrimitive(PrimitiveType.Cube, "Stem", keyRoot.transform.position + new Vector3(0.12f, 0f, 0f), new Vector3(0.28f, 0.04f, 0.05f), keyRoot.transform, metalMaterial, false);
        GameObject bow = CreatePrimitive(PrimitiveType.Cylinder, "Bow", keyRoot.transform.position + new Vector3(-0.1f, 0f, 0f), new Vector3(0.12f, 0.015f, 0.12f), keyRoot.transform, metalMaterial, false);
        bow.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        CreatePrimitive(PrimitiveType.Cube, "Tooth", keyRoot.transform.position + new Vector3(0.23f, -0.03f, 0f), new Vector3(0.05f, 0.08f, 0.05f), keyRoot.transform, metalMaterial, false);
        BoxCollider col = keyRoot.AddComponent<BoxCollider>();
        col.size = new Vector3(0.5f, 0.18f, 0.22f);

        PickupInteractable pickup = keyRoot.AddComponent<PickupInteractable>();
        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("itemId").stringValue = itemId;
        so.FindProperty("itemDisplayName").stringValue = itemDisplayName;
        so.FindProperty("pickupDescription").stringValue = description;
        SerializedProperty milestone = so.FindProperty("infectionMilestoneOnPickup");
        if (milestone != null) milestone.stringValue = infectionMilestone;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateLockedDoor(
        Transform parent,
        Vector3 pos,
        string doorName,
        string requiredItemId,
        string requiredItemName,
        string lockedMessage = "Locked. It needs the old key.",
        string unlockMilestone = "unlock_interior_gate")
    {
        GameObject root = new GameObject(doorName.Replace(" ", "") + "_Frame");
        root.transform.SetParent(parent);
        root.transform.position = pos;
        CreatePrimitive(PrimitiveType.Cube, "FrameLeft", pos + new Vector3(0f, 1.4f, -1.15f), new Vector3(0.38f, 2.8f, 0.24f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "FrameRight", pos + new Vector3(0f, 1.4f, 1.15f), new Vector3(0.38f, 2.8f, 0.24f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "FrameTop", pos + new Vector3(0f, 2.55f, 0f), new Vector3(0.38f, 0.24f, 2.55f), root.transform, darkMaterial);

        GameObject leaf = CreatePrimitive(PrimitiveType.Cube, doorName.Replace(" ", ""), pos + new Vector3(0f, 1.15f, 0f), new Vector3(0.26f, 2.3f, 2.1f), root.transform, woodMaterial, false);
        DoorInteractable door = leaf.AddComponent<DoorInteractable>();
        SerializedObject so = new SerializedObject(door);
        so.FindProperty("startsLocked").boolValue = true;
        so.FindProperty("requiredItemId").stringValue = requiredItemId;
        so.FindProperty("requiredItemDisplayName").stringValue = requiredItemName;
        so.FindProperty("lockedMessage").stringValue = lockedMessage;
        so.FindProperty("openLocalPositionOffset").vector3Value = new Vector3(0f, 0f, 2.4f);
        SerializedProperty milestone = so.FindProperty("infectionMilestoneOnUnlock");
        if (milestone != null) milestone.stringValue = unlockMilestone;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateKeyGiverNPC(
        Transform parent,
        Vector3 pos,
        string npcName,
        string narrativeRole,
        string rewardItemId,
        string rewardItemName,
        string rewardMilestone = "npc_reward_key",
        string firstConversation = null,
        string repeatConversation = null)
    {
        Material mat = npcMaterial ?? GetMaterial(ref npcMaterial, "NPC", new Color(0.55f, 0.58f, 0.62f));
        GameObject npc = CreatePrimitive(PrimitiveType.Capsule, npcName.Replace(" ", ""), pos + new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), parent, mat, false);
        NPCPlaceholder placeholder = npc.AddComponent<NPCPlaceholder>();
        placeholder.Configure(npcName, narrativeRole, FindPlayer()?.transform);

        NPCKeyGiverInteractable giver = npc.AddComponent<NPCKeyGiverInteractable>();
        SerializedObject so = new SerializedObject(giver);
        so.FindProperty("npcName").stringValue = npcName;
        so.FindProperty("firstConversation").stringValue = string.IsNullOrWhiteSpace(firstConversation)
            ? "Take the service key. Keep your head down and your story short."
            : firstConversation;
        so.FindProperty("repeatConversation").stringValue = string.IsNullOrWhiteSpace(repeatConversation)
            ? "You already have the key. Use it before someone notices."
            : repeatConversation;
        so.FindProperty("rewardItemId").stringValue = rewardItemId;
        so.FindProperty("rewardItemDisplayName").stringValue = rewardItemName;
        SerializedProperty milestone = so.FindProperty("infectionMilestoneOnReward");
        if (milestone != null) milestone.stringValue = rewardMilestone;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateServiceConsole(
        Transform parent,
        Vector3 pos,
        string consoleName,
        string requiredItemId,
        string requiredItemName,
        GameObject barrierToDisable,
        GameObject rewardToActivate,
        bool consumeRequiredItem = false,
        string successMessage = null,
        string missingItemMessage = null,
        string successMilestone = "console_service_unlock")
    {
        GameObject console = CreatePrimitive(PrimitiveType.Cube, consoleName.Replace(" ", ""), pos + new Vector3(0f, 0.9f, 0f), new Vector3(1.1f, 1.8f, 0.9f), parent, metalMaterial, false);
        CreatePrimitive(PrimitiveType.Cube, "Screen", pos + new Vector3(0f, 1.35f, 0.46f), new Vector3(0.72f, 0.34f, 0.08f), console.transform, accentMaterial, false);

        ItemConsumeInteractable consume = console.AddComponent<ItemConsumeInteractable>();
        SerializedObject so = new SerializedObject(consume);
        so.FindProperty("requiredItemId").stringValue = requiredItemId;
        so.FindProperty("requiredItemDisplayName").stringValue = requiredItemName;
        so.FindProperty("consumeItem").boolValue = consumeRequiredItem;
        so.FindProperty("successMessage").stringValue = string.IsNullOrWhiteSpace(successMessage)
            ? "Console accepts the service key. Barrier disengaged."
            : successMessage;
        so.FindProperty("missingItemMessage").stringValue = string.IsNullOrWhiteSpace(missingItemMessage)
            ? "The console requires a service key."
            : missingItemMessage;
        SerializedProperty activate = so.FindProperty("activateOnSuccess");
        activate.arraySize = 1;
        activate.GetArrayElementAtIndex(0).objectReferenceValue = rewardToActivate;
        SerializedProperty deactivate = so.FindProperty("deactivateOnSuccess");
        deactivate.arraySize = 1;
        deactivate.GetArrayElementAtIndex(0).objectReferenceValue = barrierToDisable;
        SerializedProperty milestone = so.FindProperty("infectionMilestoneOnSuccess");
        if (milestone != null) milestone.stringValue = successMilestone;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
