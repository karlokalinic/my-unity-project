
// ============================================================
// FILE: HolstinLevelDesignTemplates.Exterior.cs
// Fog courtyard template + menu entry
// ============================================================
using UnityEditor;
using UnityEngine;

public static partial class HolstinLevelDesignTemplates
{
    [MenuItem("Tools/Holstin Level Design Templates/Add Exterior Template")]
    public static void AddExteriorTemplate()
    {
        ResetMaterialCache();
        EnsureSceneRootGroups();
        EnsureCoreRig(new Vector3(-6f, 1.2f, -8f));
        EnsureDirectionalLight();
        CreateFogCourtyardExterior(Vector3.zero);
        EnsureVerticalSliceBootstrap();
        ApplyProductionAestheticPassInternal();
        FinalizeScene("Exterior template added.");
    }

    private static void CreateFogCourtyardExterior(Vector3 origin)
    {
        GameObject root = NewRoot("Template_Exterior_FogCourtyard", origin);

        CreateFloor(root.transform, "MainStreet",     origin + new Vector3(0f,  -0.05f, 0f), new Vector3(28f,0.2f,10f), groundMaterial);
        CreateFloor(root.transform, "NorthCourtyard", origin + new Vector3(9f,  -0.04f, 8f), new Vector3(10f,0.2f, 8f), groundMaterial);
        CreateFloor(root.transform, "ButcherYard",    origin + new Vector3(-9f, -0.04f,-7f), new Vector3( 8f,0.2f, 8f), groundMaterial);
        CreateFloor(root.transform, "GateApproach",   origin + new Vector3(14f, -0.04f,-1f), new Vector3( 6f,0.2f, 6f), groundMaterial);

        CreateWall(root.transform, origin + new Vector3(-14f,  2.2f, 0f),   new Vector3(0.4f,  4.4f,10f),  wallMaterial, "Facade_WestA");
        CreateWall(root.transform, origin + new Vector3(-6.5f, 2.2f, 4.8f), new Vector3(12.5f, 4.4f,0.4f), wallMaterial, "Facade_NorthA");
        CreateWall(root.transform, origin + new Vector3(14f,   2.2f, 0f),   new Vector3(0.4f,  4.4f, 9f),  wallMaterial, "GateHouseWall");
        CreateWall(root.transform, origin + new Vector3(7.4f,  2.2f,12f),   new Vector3(13f,   4.4f,0.4f), wallMaterial, "Facade_NorthB");
        CreateWall(root.transform, origin + new Vector3(3.5f,  2.2f,-5.2f), new Vector3(15f,   4.4f,0.4f), wallMaterial, "Facade_South");
        CreateFenceLine(root.transform, origin + new Vector3(-12.5f,0f,-10.2f), 7, 1.7f, Quaternion.Euler(0f,90f,0f), "ButcherFence");
        CreateGate(root.transform, origin + new Vector3(14f,0f,-1f), "CheckpointGate");

        CreateWell(root.transform,  origin + new Vector3(5.8f,  0f, 7.8f),  "CourtyardWell");
        CreateLampPost(root.transform, origin + new Vector3(-3.5f, 0f, 1.2f),  "LampPost_A");
        CreateLampPost(root.transform, origin + new Vector3( 8.2f, 0f, 7.3f),  "LampPost_B");
        CreateLampPost(root.transform, origin + new Vector3(-10.2f,0f,-6.2f),  "LampPost_C");
        CreateCrateCluster(root.transform, origin + new Vector3(-8.2f, 0f,-6.5f), 5, "ButcherCrates");
        CreateCrateCluster(root.transform, origin + new Vector3(10.8f, 0f, 1.8f), 4, "GateCrates");
        CreateCart(root.transform,  origin + new Vector3(-1.2f, 0f,-2f),    "StreetCart");
        CreateBench(root.transform, origin + new Vector3( 7f,  0.35f,10.5f),2.4f, "CourtyardBench");
        CreateBarrel(root.transform,origin + new Vector3(11.3f, 0f, 6.8f),  "Barrel_Courtyard");
        CreateBarrel(root.transform,origin + new Vector3(-11.8f,0f,-3.8f),  "Barrel_Butcher");

        CreateNarrativeZoneBox(root.transform, origin + new Vector3( 6f,  1f, 8f),  new Vector3( 8f,2f,6f), "North Courtyard", "A public square in theory and an unofficial triage line in practice.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-9f,  1f,-7f),  new Vector3( 6f,2f,6f), "Butcher Yard",    "The drains run toward the canal because morality, like sewage, follows grade.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(14f,  1f,-1f),  new Vector3( 5f,2f,5f), "Gate Checkpoint", "Leaving costs papers, money, or someone else's place in line.");

        CreateCameraZone(root.transform, origin + new Vector3( 0f,  1f, 0f),  new Vector3(20f,2f,8f), 45f,  "StreetCameraZone");
        CreateCameraZone(root.transform, origin + new Vector3( 8.5f,1f, 7.8f),new Vector3( 9f,2f,7f), 135f, "CourtyardCameraZone");
        CreateCameraZone(root.transform, origin + new Vector3(-8.5f,1f,-7f),  new Vector3( 7f,2f,7f), -45f, "ButcherCameraZone");

        CreateNPC(root.transform, origin + new Vector3(12.4f,0f,-0.5f), "Checkpoint Guard", "Papers. Then passage. No papers, no argument.");
        CreateNPC(root.transform, origin + new Vector3( 4.7f,0f, 9.8f), "Needle Seller",    "Trades bandages, rumors, and occasionally bad advice.");

        // RPG-flavored NPCs
        CreateShopMerchant(root.transform, origin + new Vector3(-3.8f, 0f, 9.2f),
            "District Peddler",
            "Sells provisions at inflated prices and pretends not to notice.",
            "merchants");
        CreateSkillCheckNPC(root.transform, origin + new Vector3(10.8f, 0f, -3.6f),
            "Gate Clerk",
            "Stamps papers when persuaded, burns them when not.",
            "The clerk flips through a stack of blank permits.",
            "persuasion", 12,
            "Fine. One permit, pre-dated. Don't come back.",
            "Permits are earned, not begged for.",
            "gate_permit", 30, "townsfolk", 5);

        CreateEnemyPatrol(root.transform, origin + new Vector3(-7f,  0f, 0.6f), "Street Prowler",
            new Vector3[] { origin+new Vector3(-9f,0f,-1f), origin+new Vector3(-2f,0f,1.4f), origin+new Vector3(2f,0f,-2.2f) });
        CreateEnemyPatrol(root.transform, origin + new Vector3( 8.5f,0f, 5.5f), "Courtyard Prowler",
            new Vector3[] { origin+new Vector3(6f,0f,6f),   origin+new Vector3(10f,0f,7f),  origin+new Vector3(11f,0f,10f) });
    }
}
