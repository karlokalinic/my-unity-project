
// ============================================================
// FILE: HolstinLevelDesignTemplates.Underpass.cs
// Catacombs/underpass template + menu entry
// ============================================================
using UnityEditor;
using UnityEngine;

public static partial class HolstinLevelDesignTemplates
{
    [MenuItem("Tools/Holstin Level Design Templates/Add Underpass Template")]
    public static void AddUnderpassTemplateMenu()
    {
        ResetMaterialCache();
        EnsureSceneRootGroups();
        EnsureCoreRig(new Vector3(-5f, 1.2f, -6f));
        EnsureDirectionalLight();
        CreateUnderpassTemplate(Vector3.zero);
        EnsureVerticalSliceBootstrap();
        FinalizeScene("Underpass template added.");
    }

    private static void CreateUnderpassTemplate(Vector3 origin)
    {
        GameObject root = NewRoot("Template_Underpass_Catacombs", origin);

        CreateFloor(root.transform, "EntryRoom",        origin + new Vector3(-8f, 0f,  0f), new Vector3( 8f,0.2f,6f), floorMaterial);
        CreateFloor(root.transform, "CanalWalk",        origin + new Vector3(-1f,-0.08f,0f),new Vector3(10f,0.2f,3f), floorMaterial);
        CreateFloor(root.transform, "ColumnHall",       origin + new Vector3( 7f, 0f,  0f), new Vector3( 8f,0.2f,8f), floorMaterial);
        CreateFloor(root.transform, "PortalRoom",       origin + new Vector3(16f, 0f,  1f), new Vector3( 7f,0.2f,6f), floorMaterial);
        CreateFloor(root.transform, "CollapsedStorage", origin + new Vector3(10f, 0f, -7f), new Vector3( 7f,0.2f,5f), floorMaterial);

        CreateWall(root.transform, origin + new Vector3(-12f,2f,  0f),  new Vector3(0.35f,4f, 6f),  wallMaterial, "Entry_West");
        CreateWall(root.transform, origin + new Vector3( -4f,2f,  0f),  new Vector3(0.35f,4f, 6f),  wallMaterial, "Entry_East");
        CreateWall(root.transform, origin + new Vector3( -8f,2f,  2.8f),new Vector3(8f,   4f,0.35f),wallMaterial, "Entry_North");
        CreateWall(root.transform, origin + new Vector3(  3f,2f,  4f),  new Vector3(0.35f,4f, 8f),  wallMaterial, "Hall_West");
        CreateWall(root.transform, origin + new Vector3( 11f,2f,  4f),  new Vector3(0.35f,4f, 8f),  wallMaterial, "Hall_East");
        CreateWall(root.transform, origin + new Vector3(  7f,2f,  8f),  new Vector3(8f,   4f,0.35f),wallMaterial, "Hall_North");
        CreateWall(root.transform, origin + new Vector3(19.5f,2f, 1f),  new Vector3(0.35f,4f, 6f),  wallMaterial, "Portal_East");
        CreateWall(root.transform, origin + new Vector3( 16f,2f,  3.8f),new Vector3(7f,   4f,0.35f),wallMaterial, "Portal_North");
        CreateWall(root.transform, origin + new Vector3( 16f,2f, -1.8f),new Vector3(7f,   4f,0.35f),wallMaterial, "Portal_South");
        CreateWall(root.transform, origin + new Vector3( 10f,2f, -9.4f),new Vector3(7f,   4f,0.35f),wallMaterial, "Storage_South");
        CreateWall(root.transform, origin + new Vector3(6.6f,2f, -7f),  new Vector3(0.35f,4f, 5f),  wallMaterial, "Storage_West");
        CreateWall(root.transform, origin + new Vector3(13.4f,2f,-7f),  new Vector3(0.35f,4f, 5f),  wallMaterial, "Storage_East");

        CreateColumn(root.transform, origin + new Vector3(5.5f,0f, 1.8f), "Column_A");
        CreateColumn(root.transform, origin + new Vector3(8.2f,0f, 1.8f), "Column_B");
        CreateColumn(root.transform, origin + new Vector3(5.5f,0f,-1.8f), "Column_C");
        CreateColumn(root.transform, origin + new Vector3(8.2f,0f,-1.8f), "Column_D");
        CreateWaterChannel(root.transform, origin + new Vector3(-1f,-0.18f,0f), new Vector3(10f,0.1f,1.1f), "Canal");
        CreateConnectorStair(origin + new Vector3(-4.1f,0f,0f), 5, 0.65f,0.22f,2.2f,0.2f,1f, Quaternion.Euler(0f, 90f,0f),"EntrySteps",  root.transform);
        CreateConnectorStair(origin + new Vector3(12.8f,0f,1f), 4, 0.65f,0.18f,2f, 0.2f,1f, Quaternion.Euler(0f,-90f,0f),"PortalSteps", root.transform);
        CreateCrateCluster(root.transform, origin + new Vector3(10.1f,0f,-7f), 6, "CollapsedStorageCrates");
        CreateBarrel(root.transform, origin + new Vector3(-9.5f,0f,-2.2f), "CanalBarrel_A");
        CreateBarrel(root.transform, origin + new Vector3(-8.4f,0f,-2.4f), "CanalBarrel_B");
        CreateLantern(root.transform, origin + new Vector3(-9.5f,2.6f, 2.1f),"EntryTorch",  new Color(1f,0.72f,0.42f));
        CreateLantern(root.transform, origin + new Vector3( 7f,  2.4f, 3.1f),"HallTorch_A", new Color(1f,0.7f, 0.35f));
        CreateLantern(root.transform, origin + new Vector3( 7f,  2.4f,-3.1f),"HallTorch_B", new Color(1f,0.7f, 0.35f));
        CreatePortal(root.transform, origin + new Vector3(16.7f,1.55f,1f), "PortalRift");

        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-8f,1f, 0f), new Vector3(7f,2f,5f), "Refuge Alcove", "A shelter for those priced out of sunlight.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3( 7f,1f, 0f), new Vector3(7f,2f,7f), "Column Hall",   "Every pillar was patched after the city above forgot what it rests on.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(16f,1f, 1f), new Vector3(6f,2f,5f), "Rift Chamber",  "An impossible aperture in very possible masonry.");

        CreateCameraZone(root.transform, origin + new Vector3(-8f,1f, 0f), new Vector3( 7f,2f,5f), 45f,  "EntryCameraZone");
        CreateCameraZone(root.transform, origin + new Vector3( 7f,1f, 0f), new Vector3(10f,2f,7f), 135f, "HallCameraZone");
        CreateCameraZone(root.transform, origin + new Vector3(16f,1f, 1f), new Vector3( 6f,2f,5f), 45f,  "PortalCameraZone");

        CreateInspectableNote(root.transform, origin + new Vector3(-8.3f,0.72f,1.4f),  new Vector3(0.36f,0.06f,0.28f), "Prayer Slip","Half a confession, half a shopping list, equally desperate.");
        CreateInspectableKey(root.transform,  origin + new Vector3(10.2f,0.84f,-7.4f), "Rust Token",  "Stamped with a district seal the surface no longer acknowledges.");

        CreateNPC(root.transform, origin + new Vector3(-9.5f,0f,1.1f), "Tunnel Hermit", "The gate opens for grief, not for courage.");
        CreateEnemyPatrol(root.transform, origin + new Vector3(6.2f,0f,-0.6f), "Tunnel Stalker",
            new Vector3[] { origin+new Vector3(5f,0f,2.2f), origin+new Vector3(8.6f,0f,2f), origin+new Vector3(8.4f,0f,-2.1f), origin+new Vector3(5.3f,0f,-2.2f) });

        // Boss encounter in the portal room
        CreateBossEnemy(root.transform, origin + new Vector3(16.5f, 0f, 1f),
            "Rift Warden", 200f, 22, "underworld", -15);

        // Skill-check NPC guarding collapsed storage
        CreateSkillCheckNPC(root.transform, origin + new Vector3(9.8f, 0f, -6.5f),
            "Collapsed Scavenger",
            "Hoards salvage and suspicion in equal measure.",
            "The scavenger eyes you from behind a barricade of crates.",
            "lockpicking", 14,
            "You've got steady hands. Take what you need from the pile.",
            "Touch my haul and I'll bury you under the next collapse.",
            "scavenger_stash", 20, "underworld", 3);
    }
}
