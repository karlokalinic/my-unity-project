
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
        NPCPlaceholder ph = npc.AddComponent<NPCPlaceholder>();
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
        SimpleEnemyAgent ea = enemy.AddComponent<SimpleEnemyAgent>();
        ea.SetPatrolPoints(points.ToArray());
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
        CreatePrimitive(PrimitiveType.Cube,     "Stem",  pos,                              new Vector3(0.3f,  0.05f, 0.05f), key.transform, metalMaterial, false);
        GameObject bow = CreatePrimitive(PrimitiveType.Cylinder, "Bow", pos + new Vector3(-0.14f, 0f, 0f), new Vector3(0.12f, 0.02f, 0.12f), key.transform, metalMaterial, false);
        bow.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        CreatePrimitive(PrimitiveType.Cube, "Tooth", pos + new Vector3(0.12f, -0.03f, 0f), new Vector3(0.05f, 0.08f, 0.05f), key.transform, metalMaterial, false);
        key.AddComponent<BoxCollider>();
        InspectableItem ii = key.AddComponent<InspectableItem>();
        ii.Configure(itemName, description);
    }
}