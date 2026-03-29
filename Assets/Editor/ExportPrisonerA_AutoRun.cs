using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Auto-runs the PrisonerA export once on next Unity compile.
/// Deletes itself after completion so it doesn't run again.
/// </summary>
[InitializeOnLoad]
public static class ExportPrisonerA_AutoRun
{
    private const string DONE_KEY    = "ExportPrisonerA_Done";
    private const string PREFAB_FOLDER = "Assets/Prefabs/Characters";
    private const string PREFAB_PATH   = "Assets/Prefabs/Characters/CHAR_PrisonerA.prefab";
    private const string EXPORT_PATH   = "C:/Users/kalin/Desktop/CHAR_PrisonerA_Export.unitypackage";
    private const string SCENE_OBJ    = "PrisonerAPrefab";

    static ExportPrisonerA_AutoRun()
    {
        // Only run once
        if (EditorPrefs.GetBool(DONE_KEY, false)) return;

        // Defer until editor is fully initialised
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        EditorPrefs.SetBool(DONE_KEY, true);

        Debug.Log("[ExportPrisonerA_AutoRun] Starting export...");

        // ── 1. Find PrisonerAPrefab in any open scene ─────────────────────
        GameObject target = null;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            foreach (GameObject root in SceneManager.GetSceneAt(i).GetRootGameObjects())
            {
                target = FindDeep(root, SCENE_OBJ);
                if (target != null) break;
            }
            if (target != null) break;
        }

        if (target == null)
        {
            Debug.LogError($"[ExportPrisonerA_AutoRun] '{SCENE_OBJ}' not found in any open scene. " +
                           "Open Prison_C scene and reset the EditorPref 'ExportPrisonerA_Done' to retry.");
            return;
        }

        // ── 2. Ensure output folder exists ────────────────────────────────
        if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
        {
            string[] parts = PREFAB_FOLDER.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        // ── 3. Save as prefab ─────────────────────────────────────────────
        bool ok;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(target, PREFAB_PATH, out ok);
        if (!ok || prefab == null)
        {
            Debug.LogError("[ExportPrisonerA_AutoRun] SaveAsPrefabAsset failed. See above for details.");
            return;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ExportPrisonerA_AutoRun] Prefab saved → {PREFAB_PATH}");

        // ── 4. Export package with all dependencies ────────────────────────
        string desktopDir = Path.GetDirectoryName(EXPORT_PATH);
        if (!Directory.Exists(desktopDir)) Directory.CreateDirectory(desktopDir);

        string[] deps = AssetDatabase.GetDependencies(PREFAB_PATH, recursive: true);
        AssetDatabase.ExportPackage(deps, EXPORT_PATH, ExportPackageOptions.Recurse);
        Debug.Log($"[ExportPrisonerA_AutoRun] Package exported → {EXPORT_PATH}");

        // ── 5. Self-cleanup ────────────────────────────────────────────────
        // Delete both autorun and main export script so they don't clutter the project
        string autoRunPath = "Assets/Editor/ExportPrisonerA_AutoRun.cs";
        string mainPath    = "Assets/Editor/ExportPrisonerA.cs";
        AssetDatabase.DeleteAsset(autoRunPath);
        AssetDatabase.DeleteAsset(mainPath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Export Complete ✓",
            $"CHAR_PrisonerA.prefab saved to:\n  {PREFAB_PATH}\n\n" +
            $"Unitypackage exported to:\n  {EXPORT_PATH}\n\n" +
            "Both editor scripts have been removed.",
            "OK");
    }

    private static GameObject FindDeep(GameObject go, string name)
    {
        if (go.name == name) return go;
        foreach (Transform t in go.transform)
        {
            var found = FindDeep(t.gameObject, name);
            if (found != null) return found;
        }
        return null;
    }
}
