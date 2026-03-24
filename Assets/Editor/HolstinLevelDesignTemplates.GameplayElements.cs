
// ============================================================
// FILE: HolstinLevelDesignTemplates.GameplayElements.cs
// NPCs, enemies, narrative zones, camera zones, inspectables
// ============================================================
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static partial class HolstinLevelDesignTemplates
{
    private static void CreateNarrativeZoneBox(Transform parent, Vector3 pos, Vector3 size, string locationName, string summary)
    {
        GameObject zone = new GameObject(locationName.Replace(" ", "") + "_NarrativeZone");
        zone.transform.SetParent(parent);
        zone.transform.position = pos;
        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true; box.size = size;
        NarrativeZone nz = zone.AddComponent<NarrativeZone>();
        nz.Configure(locationName, summary, true);
    }

    private static void CreateCameraZone(Transform parent, Vector3 pos, Vector3 size, float yaw, string name)
    {
        GameObject zone = new GameObject(name);
        zone.transform.SetParent(parent);
        zone.transform.position = pos;
        BoxCollider box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true; box.size = size;
        CameraAngleZone az = zone.AddComponent<CameraAngleZone>();
        az.Configure(yaw, false);
    }

    private static void CreateNPC(Transform parent, Vector3 pos, string npcName, string narrativeRole)
    {
        Material mat = npcMaterial ?? GetMaterial(ref npcMaterial, "NPC", new Color(0.55f, 0.58f, 0.62f));
        GameObject npc = CreatePrimitive(PrimitiveType.Capsule, npcName.Replace(" ", ""), pos + new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), parent, mat, false);
        NpcIdentity ph = npc.AddComponent<NpcIdentity>();
        Transform player = FindPlayer()?.transform;
        ph.Configure(npcName, narrativeRole, player);
    }

    private static void CreateEnemyPatrol(Transform parent, Vector3 pos, string enemyName, Vector3[] patrolPositions)
    {
        GameObject root = new GameObject(enemyName.Replace(" ", "") + "_Root");
        root.transform.SetParent(parent);
        Material mat = enemyMaterial ?? GetMaterial(ref enemyMaterial, "Enemy", new Color(0.4f, 0.24f, 0.24f));
        GameObject enemy = CreatePrimitive(PrimitiveType.Capsule, enemyName.Replace(" ", ""), pos + new Vector3(0f, 1f, 0f), new Vector3(0.9f, 1f, 0.9f), root.transform, mat, false);
        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.radius = 0.35f; agent.speed = 3.25f; agent.angularSpeed = 720f; agent.acceleration = 18f;
        List<Transform> points = new List<Transform>();
        for (int i = 0; i < patrolPositions.Length; i++)
        {
            GameObject pt = new GameObject($"PatrolPoint_{i}");
            pt.transform.SetParent(root.transform);
            pt.transform.position = patrolPositions[i];
            points.Add(pt.transform);
        }

        // New RPG-aware enemy
        CharacterStats stats = enemy.AddComponent<CharacterStats>();
        Damageable dmg = enemy.AddComponent<Damageable>();
        dmg.Configure(50f, false, true);
        EnemyController controller = enemy.AddComponent<EnemyController>();
        controller.SetPatrolPoints(points.ToArray());
    }

    private static void CreateInspectableNote(Transform parent, Vector3 pos, Vector3 scale, string itemName, string description)
    {
        GameObject note = CreatePrimitive(PrimitiveType.Cube, itemName.Replace(" ", ""), pos, scale, parent, wallMaterial, false);
        InspectableItem ii = note.AddComponent<InspectableItem>();
        ii.Configure(itemName, description);
    }

    private static void CreateInspectableKey(Transform parent, Vector3 pos, string itemName, string description)
    {
        GameObject key = new GameObject(itemName.Replace(" ", ""));
        key.transform.SetParent(parent);
        key.transform.position = pos;

        AssetPlaceholder ph = key.AddComponent<AssetPlaceholder>();
        ph.Configure(AssetPlaceholder.PlaceholderCategory.Prop, "inspectable_key", new Vector3(0.3f, 0.08f, 0.15f));

        key.AddComponent<BoxCollider>();
        InspectableItem ii = key.AddComponent<InspectableItem>();
        ii.Configure(itemName, description);
    }

    // ----------------------------------------------------------------
    // RPG-aware creation helpers (skill-check NPC, merchant, boss)
    // ----------------------------------------------------------------

    private static void CreateSkillCheckNPC(
        Transform parent,
        Vector3 pos,
        string npcName,
        string narrativeRole,
        string promptLine,
        string skillId,
        int skillDC,
        string passResponse,
        string failResponse,
        string grantItemId = "",
        int xpReward = 25,
        string reputationFaction = "",
        int repDelta = 0)
    {
        Material mat = npcMaterial ?? GetMaterial(ref npcMaterial, "NPC", new Color(0.55f, 0.58f, 0.62f));
        GameObject npc = CreatePrimitive(PrimitiveType.Capsule, npcName.Replace(" ", ""), pos + new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), parent, mat, false);

        NpcIdentity identity = npc.AddComponent<NpcIdentity>();
        identity.Configure(npcName, narrativeRole, FindPlayer()?.transform);

        npc.AddComponent<NpcDialogueController>();

        SkillCheckNpcInteractable npcInteractable = npc.AddComponent<SkillCheckNpcInteractable>();

        SerializedObject so = new SerializedObject(npcInteractable);

        // First encounter node
        SerializedProperty firstNode = so.FindProperty("firstEncounter");
        if (firstNode != null)
        {
            SerializedProperty prompt = firstNode.FindPropertyRelative("promptLine");
            if (prompt != null) prompt.stringValue = promptLine;

            SerializedProperty choices = firstNode.FindPropertyRelative("choices");
            if (choices != null)
            {
                choices.arraySize = 1;
                SerializedProperty choice0 = choices.GetArrayElementAtIndex(0);

                SerializedProperty req = choice0.FindPropertyRelative("requirement");
                if (req != null)
                {
                    SerializedProperty gateType = req.FindPropertyRelative("gateType");
                    if (gateType != null) gateType.enumValueIndex = 0; // SkillCheck
                    SerializedProperty sid = req.FindPropertyRelative("skillId");
                    if (sid != null) sid.stringValue = skillId;
                    SerializedProperty dc = req.FindPropertyRelative("dc");
                    if (dc != null) dc.intValue = skillDC;
                }

                SerializedProperty choiceData = choice0.FindPropertyRelative("choice");
                if (choiceData != null)
                {
                    SerializedProperty text = choiceData.FindPropertyRelative("text");
                    if (text != null) text.stringValue = $"[{skillId} {skillDC}] Press further.";
                    SerializedProperty response = choiceData.FindPropertyRelative("responseLine");
                    if (response != null) response.stringValue = passResponse;
                }

                SerializedProperty failResp = choice0.FindPropertyRelative("failureResponse");
                if (failResp != null) failResp.stringValue = failResponse;

                SerializedProperty xp = choice0.FindPropertyRelative("experienceReward");
                if (xp != null) xp.intValue = xpReward;

                SerializedProperty grantItem = choice0.FindPropertyRelative("grantItemId");
                if (grantItem != null) grantItem.stringValue = grantItemId;

                if (!string.IsNullOrWhiteSpace(reputationFaction))
                {
                    SerializedProperty repFaction = choice0.FindPropertyRelative("reputationFactionId");
                    if (repFaction != null) repFaction.stringValue = reputationFaction;
                    SerializedProperty repDeltaProp = choice0.FindPropertyRelative("reputationDelta");
                    if (repDeltaProp != null) repDeltaProp.intValue = repDelta;
                }
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateShopMerchant(
        Transform parent,
        Vector3 pos,
        string npcName,
        string narrativeRole,
        string factionId = "merchants")
    {
        Material mat = npcMaterial ?? GetMaterial(ref npcMaterial, "NPC", new Color(0.55f, 0.58f, 0.62f));
        GameObject npc = CreatePrimitive(PrimitiveType.Capsule, npcName.Replace(" ", ""), pos + new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), parent, mat, false);

        NpcIdentity identity = npc.AddComponent<NpcIdentity>();
        identity.Configure(npcName, narrativeRole, FindPlayer()?.transform);

        ShopKeeper shop = npc.AddComponent<ShopKeeper>();
        SerializedObject so = new SerializedObject(shop);
        SerializedProperty shopNameProp = so.FindProperty("shopName");
        if (shopNameProp != null) shopNameProp.stringValue = npcName;
        SerializedProperty factionProp = so.FindProperty("factionId");
        if (factionProp != null) factionProp.stringValue = factionId;
        so.ApplyModifiedPropertiesWithoutUndo();

        npc.AddComponent<ShopInteractable>();
    }

    private static void CreateBossEnemy(
        Transform parent,
        Vector3 pos,
        string bossName,
        float maxHealth,
        int attackDamage,
        string reputationFactionOnKill = "",
        int repDeltaOnKill = 0)
    {
        Material mat = enemyMaterial ?? GetMaterial(ref enemyMaterial, "Enemy", new Color(0.4f, 0.24f, 0.24f));
        GameObject boss = CreatePrimitive(PrimitiveType.Capsule, bossName.Replace(" ", ""), pos + new Vector3(0f, 1f, 0f), new Vector3(1.1f, 1.2f, 1.1f), parent, mat, false);

        CharacterStats stats = boss.AddComponent<CharacterStats>();
        SerializedObject statsSO = new SerializedObject(stats);
        SerializedProperty maxHp = statsSO.FindProperty("maxHealth");
        if (maxHp != null) maxHp.floatValue = maxHealth;
        SerializedProperty str = statsSO.FindProperty("strength");
        if (str != null) str.intValue = 18;
        SerializedProperty con = statsSO.FindProperty("constitution");
        if (con != null) con.intValue = 16;
        statsSO.ApplyModifiedPropertiesWithoutUndo();

        Damageable dmg = boss.AddComponent<Damageable>();
        dmg.Configure(maxHealth, true, false);

        NavMeshAgent agent = boss.AddComponent<NavMeshAgent>();
        agent.radius = 0.45f;
        agent.speed = 2.8f;
        agent.angularSpeed = 600f;
        agent.acceleration = 14f;

        EnemyController controller = boss.AddComponent<EnemyController>();
        SerializedObject so = new SerializedObject(controller);
        SerializedProperty aDmg = so.FindProperty("attackDamage");
        if (aDmg != null) aDmg.floatValue = attackDamage;
        SerializedProperty detRadius = so.FindProperty("detectionRadius");
        if (detRadius != null) detRadius.floatValue = 12f;
        SerializedProperty loseRadius = so.FindProperty("loseTargetRadius");
        if (loseRadius != null) loseRadius.floatValue = 20f;

        if (!string.IsNullOrWhiteSpace(reputationFactionOnKill))
        {
            SerializedProperty repFaction = so.FindProperty("reputationFactionOnKill");
            if (repFaction != null) repFaction.stringValue = reputationFactionOnKill;
            SerializedProperty repDelta = so.FindProperty("reputationDeltaOnKill");
            if (repDelta != null) repDelta.intValue = repDeltaOnKill;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}