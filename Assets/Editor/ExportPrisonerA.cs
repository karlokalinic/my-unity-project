using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// One-click tool: finds PrisonerAPrefab in the open scene,
/// saves it as a proper prefab asset, and exports a .unitypackage
/// with all dependencies so it can be dropped into any Unity 6 project.
/// </summary>
public class ExportPrisonerA : EditorWindow
{
    private const string PREFAB_FOLDER  = "Assets/Prefabs/Characters";
    private const string PREFAB_PATH    = "Assets/Prefabs/Characters/CHAR_PrisonerA.prefab";
    private const string EXPORT_PATH    = "C:/Users/kalin/Desktop/CHAR_PrisonerA_Export.unitypackage";
    private const string SCENE_OBJ_NAME = "PrisonerAPrefab";

    [MenuItem("Tools/Holstin/Export PrisonerA Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ExportPrisonerA>("Export PrisonerA");
    }

    private void OnGUI()
    {
        GUILayout.Label("PrisonerA Prefab Exporter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This will:\n" +
            "1. Find PrisonerAPrefab in the open scene\n" +
            "2. Save it as Assets/Prefabs/Characters/CHAR_PrisonerA.prefab\n" +
            "3. Export a .unitypackage to the Desktop with all dependencies",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Run Export", GUILayout.Height(40)))
        {
            RunExport();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Step 1 only: Save Prefab", GUILayout.Height(30)))
        {
            SavePrefab();
        }
        if (GUILayout.Button("Step 2 only: Export Package", GUILayout.Height(30)))
        {
            ExportPackage();
        }
    }

    public static void RunExport()
    {
        if (!SavePrefab()) return;
        ExportPackage();
        EditorUtility.DisplayDialog("Done",
            $"Prefab saved and package exported to:\n{EXPORT_PATH}", "OK");
        Debug.Log($"[ExportPrisonerA] Export complete → {EXPORT_PATH}");
    }

    private static bool SavePrefab()
    {
        // Find the object in the open scene
        GameObject sceneObj = null;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                sceneObj = FindInHierarchy(root, SCENE_OBJ_NAME);
                if (sceneObj != null) break;
            }
            if (sceneObj != null) break;
        }

        if (sceneObj == null)
        {
            EditorUtility.DisplayDialog("Not Found",
                $"Could not find '{SCENE_OBJ_NAME}' in any open scene.\n" +
                "Make sure Prison_C scene is loaded and the object is active.", "OK");
            return false;
        }

        // Ensure destination folder exists
        if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
        {
            string[] parts = PREFAB_FOLDER.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        // Save as prefab (overwrite if it already exists)
        bool success;
        PrefabUtility.SaveAsPrefabAsset(sceneObj, PREFAB_PATH, out success);

        if (success)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ExportPrisonerA] Prefab saved → {PREFAB_PATH}");
            return true;
        }
        else
        {
            EditorUtility.DisplayDialog("Save Failed",
                $"PrefabUtility.SaveAsPrefabAsset failed for '{sceneObj.name}'.\n" +
                "Check the Console for details.", "OK");
            return false;
        }
    }

    private static void ExportPackage()
    {
        // Collect the prefab and all its dependencies
        string[] assetPaths = AssetDatabase.GetDependencies(PREFAB_PATH, recursive: true);

        // Make sure the output directory exists
        string dir = Path.GetDirectoryName(EXPORT_PATH);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        AssetDatabase.ExportPackage(
            assetPaths,
            EXPORT_PATH,
            ExportPackageOptions.Interactive | ExportPackageOptions.Recurse
        );

        Debug.Log($"[ExportPrisonerA] Package exported → {EXPORT_PATH}");
    }

    // Recursive search through child hierarchy
    private static GameObject FindInHierarchy(GameObject root, string name)
    {
        if (root.name == name) return root;
        foreach (Transform child in root.transform)
        {
            GameObject found = FindInHierarchy(child.gameObject, name);
            if (found != null) return found;
        }
        return null;
    }
}
