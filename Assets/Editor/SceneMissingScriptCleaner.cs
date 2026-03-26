#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneMissingScriptCleaner
{
    [MenuItem("Tools/Holstin/Clean Missing Scripts In Open Scene")]
    private static void CleanMissingScriptsInOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("No active scene is loaded.");
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        int removedCount = 0;
        for (int i = 0; i < roots.Length; i++)
        {
            removedCount += RemoveMissingScriptsRecursive(roots[i].transform);
        }

        if (removedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"Removed {removedCount} missing script component(s) from '{scene.name}'.");
        }
        else
        {
            Debug.Log($"No missing script components found in '{scene.name}'.");
        }
    }

    private static int RemoveMissingScriptsRecursive(Transform root)
    {
        if (root == null)
        {
            return 0;
        }

        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root.gameObject);
        for (int i = 0; i < root.childCount; i++)
        {
            removed += RemoveMissingScriptsRecursive(root.GetChild(i));
        }

        return removed;
    }
}
#endif
