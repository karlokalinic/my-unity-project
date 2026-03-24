
// ============================================================
// FILE: HolstinLevelDesignTemplates.Interior.cs
// Boarding house template + menu entry
// ============================================================
using UnityEditor;
using UnityEngine;

public static partial class HolstinLevelDesignTemplates
{
    [MenuItem("Tools/Holstin Level Design Templates/Add Interior Template")]
    public static void AddInteriorTemplate()
    {
        ResetMaterialCache();
        EnsureSceneRootGroups();
        EnsureCoreRig(new Vector3(0f, 1.2f, -6f));
        EnsureDirectionalLight();
        CreateBoardingHouseInterior(Vector3.zero);
        EnsureVerticalSliceBootstrap();
        FinalizeScene("Interior template added.");
    }

    private static void CreateBoardingHouseInterior(Vector3 origin)
    {
        GameObject root = NewRoot("Template_Interior_BoardingHouse", origin);

        CreateFloor(root.transform, "Foundation",      origin + new Vector3(0f, -0.2f, 0f),  new Vector3(15f, 0.4f, 10f),   floorMaterial);
        CreateFloor(root.transform, "GroundFloor_Main",origin + new Vector3(0f,  0f,   0f),  new Vector3(14f, 0.2f,  9f),   floorMaterial);
        CreateFloor(root.transform, "UpperFloor",      origin + new Vector3(0f,  3.2f, 0.25f), new Vector3(12f, 0.2f, 7.5f), woodMaterial);
        CreateFloor(root.transform, "UpperBalcony",    origin + new Vector3(0f,  3.2f,-3.35f), new Vector3(12f, 0.2f,  1f),  woodMaterial);

        CreateWall(root.transform, origin + new Vector3(0f,    1.6f, 4.35f), new Vector3(14f,   3.2f, 0.35f), wallMaterial, "BackWall_Ground");
        CreateWall(root.transform, origin + new Vector3(-6.85f,1.6f, 0f),    new Vector3(0.35f, 3.2f, 9f),   wallMaterial, "LeftWall_Ground");
        CreateWall(root.transform, origin + new Vector3( 6.85f,1.6f, 0f),    new Vector3(0.35f, 3.2f, 9f),   wallMaterial, "RightWall_Ground");
        CreateWall(root.transform, origin + new Vector3(-1.6f, 1.6f, 0.9f),  new Vector3(0.25f, 3.2f, 6.5f), wallMaterial, "Partition_Kitchen");
        CreateWall(root.transform, origin + new Vector3( 2.7f, 1.6f, 0.5f),  new Vector3(0.25f, 3.2f, 5.5f), wallMaterial, "Partition_Office");
        CreateWall(root.transform, origin + new Vector3(0f,    4.8f, 3.7f),  new Vector3(12f,   3.2f, 0.35f), wallMaterial, "BackWall_Upper");
        CreateWall(root.transform, origin + new Vector3(-5.85f,4.8f, 0f),    new Vector3(0.35f, 3.2f, 7.5f), wallMaterial, "LeftWall_Upper");
        CreateWall(root.transform, origin + new Vector3( 5.85f,4.8f, 0f),    new Vector3(0.35f, 3.2f, 7.5f), wallMaterial, "RightWall_Upper");
        CreateWall(root.transform, origin + new Vector3( 0.8f, 4.8f, 0.5f),  new Vector3(0.25f, 3.2f, 5.5f), wallMaterial, "Partition_Sickroom");

        CreateBeamLine(root.transform, origin + new Vector3(-5.3f, 3.2f, -3.3f), 6, 2.1f);
        CreateBeamLine(root.transform, origin + new Vector3(-5.3f, 0f,   -3.3f), 6, 2.1f);
        CreateRoofCaps(root.transform, origin + new Vector3(0f, 7.1f, 0.5f));

        CreateConnectorStair(origin + new Vector3(-4.8f, 0f, -1.4f), 9, 0.78f, 0.355f, 1.6f, 0.2f, 1.05f, Quaternion.identity,          "InteriorStairA", root.transform);
        CreateConnectorStair(origin + new Vector3( 4.6f, 0f, -1.2f), 4, 0.65f, 0.18f,  1.5f, 0.2f, 0.9f,  Quaternion.Euler(0f,180f,0f), "PorchSteps",     root.transform);

        CreateTable(root.transform, origin + new Vector3(-4.4f, 0.55f, 1.2f), new Vector3(2.2f, 0.18f, 1.1f), "KitchenTable");
        CreateBench(root.transform, origin + new Vector3(-4.4f, 0.35f, 2.2f), 2f, "KitchenBench_A");
        CreateBench(root.transform, origin + new Vector3(-4.4f, 0.35f, 0.2f), 2f, "KitchenBench_B");
        CreateShelf(root.transform, origin + new Vector3(-6.1f, 0f, 2.2f), 3, "KitchenShelf");
        CreateBed(root.transform,   origin + new Vector3( 3.9f, 0f, 1.8f), "OfficeCot");
        CreateDesk(root.transform,  origin + new Vector3( 4.6f, 0f,-0.6f), "LandlordDesk");
        CreateShelf(root.transform, origin + new Vector3( 6f,   0f, 2.4f), 2, "OfficeCabinet");
        CreateBed(root.transform,   origin + new Vector3(-3.9f, 3.2f, 1.4f), "DormBed_A");
        CreateBed(root.transform,   origin + new Vector3(-1.4f, 3.2f, 1.6f), "DormBed_B");
        CreateBed(root.transform,   origin + new Vector3( 4f,   3.2f, 1.3f), "SickBed");
        CreateDesk(root.transform,  origin + new Vector3( 3.9f, 3.2f,-1.3f), "UpstairsDesk");
        CreateCrateCluster(root.transform, origin + new Vector3(-5.6f, 3.2f,-3.2f), 3, "UpperStorage");
        CreateBarrel(root.transform, origin + new Vector3( 5.4f, 0f, 3.4f), "Barrel_A");
        CreateBarrel(root.transform, origin + new Vector3(-6f,   0f, 3.3f), "Barrel_B");

        CreateLantern(root.transform, origin + new Vector3(-5.2f, 2.55f, 1.5f), "KitchenLantern",  new Color(1f, 0.80f, 0.55f));
        CreateLantern(root.transform, origin + new Vector3( 4.2f, 2.55f, 0.6f), "OfficeLantern",   new Color(1f, 0.78f, 0.50f));
        CreateLantern(root.transform, origin + new Vector3(-1.5f, 5.6f,  1.3f), "DormLantern",     new Color(1f, 0.74f, 0.45f));
        CreateLantern(root.transform, origin + new Vector3( 3.8f, 5.6f,  1.3f), "SickroomLantern", new Color(1f, 0.72f, 0.40f));

        CreateInspectableNote(root.transform, origin + new Vector3(4.8f, 0.95f,-0.6f),  new Vector3(0.35f,0.06f,0.28f), "Rent Ledger",   "Lists residents who vanished without settling their debts.");
        CreateInspectableNote(root.transform, origin + new Vector3(3.8f, 3.82f,-1.25f), new Vector3(0.32f,0.06f,0.24f), "Triage Notes",  "A few names are crossed out so hard the paper tore.");
        CreateInspectableKey(root.transform,  origin + new Vector3(5.5f, 0.88f, 2.25f), "Cellar Key", "Probably opens something below the house. What a comforting sentence.");

        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-4.4f,1f, 1.2f), new Vector3(4.2f,2f,3.4f), "Shared Kitchen", "This is where gossip ferments faster than stew.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3( 4.4f,1f, 0.6f), new Vector3(3.6f,2f,3.6f), "Landlord Office","Debt ledgers, withheld medicine, and the smell of wet paper.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3(-2.4f,4.15f,1.4f),new Vector3(5.8f,2f,3f),  "Dormitory",      "Too many beds, too little privacy, and less hope than either.");
        CreateNarrativeZoneBox(root.transform, origin + new Vector3( 3.8f,4.15f,1.2f),new Vector3(3.6f,2f,3f),  "Sickroom",       "A place where recovery and disposal are separated mostly by paperwork.");

        CreateCameraZone(root.transform, origin + new Vector3(-4.2f,1f, 0.8f), new Vector3(5.5f,2f,5f),  45f,  "KitchenCameraZone");
        CreateCameraZone(root.transform, origin + new Vector3( 4.5f,1f, 0.5f), new Vector3(4.5f,2f,4.5f),135f, "OfficeCameraZone");
        CreateCameraZone(root.transform, origin + new Vector3(-1.5f,4.15f,1.3f),new Vector3(11f,2f,5f),  45f,  "UpperFloorCameraZone");

        CreateNPC(root.transform, origin + new Vector3( 4.1f,0f,1.4f), "Landlady",      "Collects rent, inventories supplies, and edits her memory to remain employable.");
        CreateNPC(root.transform, origin + new Vector3(-2f, 3.2f,0.4f),"Convalescent",  "Talks in feverish fragments about the tunnel shrine under the house.");

        // RPG skill-check NPC in the dormitory
        CreateSkillCheckNPC(root.transform, origin + new Vector3(-4.2f, 3.2f, -0.5f),
            "Dormitory Elder",
            "Knows too much and says too little unless convinced.",
            "The elder watches you from a corner bed, arms folded.",
            "rhetoric", 10,
            "You remind me of someone who survived. Take the back stairway code.",
            "I have nothing to say to strangers with nothing to trade.",
            "stairway_code", 20, "townsfolk", 3);
    }
}