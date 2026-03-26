// ============================================================
// FILE: HolstinSceneBuilder.cs
// Unified editor window for modular scene layout generation.
// Replaces scattered menu items with a single dropdown interface.
// ============================================================
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HolstinSceneBuilder : EditorWindow
{
    private enum LayoutPreset
    {
        ExteriorCourtyard,
        InteriorBoardingHouse,
        UnderpassCatacombs,
        InteractableSandbox,
        IsometricChoiceMega,
        FullVerticalSlice,
        TemplatePack,
        EmptyWithBootstrap
    }

    private LayoutPreset selectedPreset = LayoutPreset.FullVerticalSlice;
    private bool createNewScene = true;
    private Vector3 stampOrigin = Vector3.zero;
    private string sceneName = "VerticalSlice";

    private Vector2 scrollPos;

    [MenuItem("Tools/Holstin/Scene Builder", false, 0)]
    public static void ShowWindow()
    {
        var window = GetWindow<HolstinSceneBuilder>("Holstin Scene Builder");
        window.minSize = new Vector2(380f, 520f);
    }

    [MenuItem("Tools/Holstin/Apply Bootstrap To Current Scene", false, 20)]
    public static void ApplyBootstrap()
    {
        HolstinLevelDesignTemplates.ApplyVerticalSliceBootstrapToCurrentScene();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("HOLSTIN SCENE BUILDER", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a layout preset and generate it in a new scene or stamp it into the current scene. " +
            "Layouts define room geometry, spawn points, interactions, and logic wiring. " +
            "Visual assets (furniture, props) are placed as AssetPlaceholder markers ready for store model swap.",
            MessageType.Info);

        EditorGUILayout.Space(12);

        // --- Preset selector ---
        EditorGUILayout.LabelField("Layout Preset", EditorStyles.miniBoldLabel);
        selectedPreset = (LayoutPreset)EditorGUILayout.EnumPopup("Preset", selectedPreset);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField(GetPresetDescription(selectedPreset), EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.Space(12);

        // --- Options ---
        EditorGUILayout.LabelField("Options", EditorStyles.miniBoldLabel);
        createNewScene = EditorGUILayout.Toggle("Create New Scene", createNewScene);

        if (createNewScene)
        {
            sceneName = EditorGUILayout.TextField("Scene Name", sceneName);
        }
        else
        {
            stampOrigin = EditorGUILayout.Vector3Field("Stamp Origin", stampOrigin);
        }

        EditorGUILayout.Space(16);

        // --- Generate button ---
        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.4f);
        if (GUILayout.Button("Generate Layout", GUILayout.Height(36)))
        {
            GenerateSelectedPreset();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(16);

        // --- Utility buttons ---
        EditorGUILayout.LabelField("Utilities", EditorStyles.miniBoldLabel);

        if (GUILayout.Button("Apply Bootstrap to Current Scene"))
        {
            HolstinLevelDesignTemplates.ApplyVerticalSliceBootstrapToCurrentScene();
        }

        if (GUILayout.Button("Apply Production Aesthetic Pass"))
        {
            HolstinLevelDesignTemplates.ApplyProductionAestheticPassToCurrentScene();
        }

        if (GUILayout.Button("Create Default Art Pack Asset"))
        {
            HolstinArtPackTools.CreateDefaultArtPackAssetMenu();
        }

        if (GUILayout.Button("Apply Default Art Pack"))
        {
            HolstinArtPackTools.ApplyDefaultArtPackToActiveSceneMenu();
        }

        if (GUILayout.Button("Seed Default Art Pack From Placeholders"))
        {
            HolstinArtPackTools.SeedDefaultArtPackFromSceneMenu();
        }

        if (GUILayout.Button("Auto-Assign Art Pack Prefabs (Selected Folder)"))
        {
            HolstinArtPackTools.AutoAssignDefaultArtPackFromSelectedFolderMenu();
        }

        if (GUILayout.Button("Recover Art Pack From Scene Placements"))
        {
            HolstinArtPackTools.RecoverDefaultArtPackFromCurrentSceneMenu();
        }

        if (GUILayout.Button("Find All Asset Placeholders"))
        {
            var placeholders = FindObjectsByType<AssetPlaceholder>();
            Debug.Log($"Found {placeholders.Length} AssetPlaceholder(s) in scene.");
            if (placeholders.Length > 0)
            {
                Selection.objects = new Object[0];
                var gos = new GameObject[placeholders.Length];
                for (int i = 0; i < placeholders.Length; i++)
                    gos[i] = placeholders[i].gameObject;
                Selection.objects = gos;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void GenerateSelectedPreset()
    {
        if (createNewScene)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("Scene generation cancelled.");
                return;
            }
        }

        switch (selectedPreset)
        {
            case LayoutPreset.FullVerticalSlice:
                HolstinLevelDesignTemplates.CreateFullSliceScene();
                break;
            case LayoutPreset.ExteriorCourtyard:
                if (createNewScene) PrepareNewScene();
                HolstinLevelDesignTemplates.AddExteriorTemplate();
                break;
            case LayoutPreset.InteriorBoardingHouse:
                if (createNewScene) PrepareNewScene();
                HolstinLevelDesignTemplates.AddInteriorTemplate();
                break;
            case LayoutPreset.UnderpassCatacombs:
                if (createNewScene) PrepareNewScene();
                HolstinLevelDesignTemplates.AddUnderpassTemplateMenu();
                break;
            case LayoutPreset.InteractableSandbox:
                HolstinLevelDesignTemplates.CreateInteractableTestScene();
                break;
            case LayoutPreset.IsometricChoiceMega:
                HolstinLevelDesignTemplates.CreateIsometricChoiceMegastructure();
                break;
            case LayoutPreset.TemplatePack:
                if (createNewScene) PrepareNewScene();
                HolstinLevelDesignTemplates.CreateTemplatePackInCurrentScene();
                break;
            case LayoutPreset.EmptyWithBootstrap:
                if (createNewScene) PrepareNewScene();
                HolstinLevelDesignTemplates.ApplyVerticalSliceBootstrapToCurrentScene();
                break;
        }
    }

    private void PrepareNewScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
    }

    private static string GetPresetDescription(LayoutPreset preset)
    {
        switch (preset)
        {
            case LayoutPreset.ExteriorCourtyard:
                return "Fog courtyard: 4 connected outdoor areas with streets, gates, wells, lamp posts. " +
                       "NPCs, enemy patrols, camera zones, narrative zones.";
            case LayoutPreset.InteriorBoardingHouse:
                return "Multi-level boarding house: ground floor + upper floor with kitchen, office, " +
                       "dormitory, sickroom. Stairs, furniture placeholders, inspectable documents, 2 NPCs.";
            case LayoutPreset.UnderpassCatacombs:
                return "Underground catacombs: 5 connected chambers (entry, canal, column hall, portal room, " +
                       "collapsed storage). Columns, water channels, portal effects, 2 NPCs + enemies.";
            case LayoutPreset.InteractableSandbox:
                return "Tutorial sandbox: 5 rooms testing inspect, pickup, door unlock, NPC dialogue, " +
                       "relay puzzle, combat, and console mechanics. Ideal for testing all interaction systems.";
            case LayoutPreset.IsometricChoiceMega:
                return "Branching choice megastructure: 9+ chambers with hub, branch doors, relay puzzles, " +
                       "mutually exclusive badge choices, world state reactions. Tests the choice system end-to-end.";
            case LayoutPreset.FullVerticalSlice:
                return "Complete vertical slice: exterior + interior + underpass with all gameplay systems, " +
                       "connecting stairs, NPCs, enemies, narrative zones, camera zones, and bootstrap wiring.";
            case LayoutPreset.TemplatePack:
                return "All 3 environment templates (exterior, interior, underpass) stamped side by side " +
                       "in one scene for layout reference.";
            case LayoutPreset.EmptyWithBootstrap:
                return "Empty scene with core rig (player, camera, UI, context) and bootstrap component. " +
                       "Use this as a starting point for custom layouts.";
            default:
                return "";
        }
    }
}
