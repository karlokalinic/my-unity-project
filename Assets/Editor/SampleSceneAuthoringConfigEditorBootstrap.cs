#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SampleSceneAuthoringConfigEditorBootstrap
{
    private const string SampleSceneName = "SampleScene";
    private const string ConfigObjectName = "SampleSceneAuthoringConfig";

    [MenuItem("Tools/Holstin/Authoring/Select Or Create SampleScene Config", false, 30)]
    private static void SelectOrCreateConfigInActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogWarning("No active scene found.");
            return;
        }

        SampleSceneAuthoringConfig config = FindInScene<SampleSceneAuthoringConfig>(scene);
        if (config == null)
        {
            GameObject configObject = new GameObject(ConfigObjectName);
            SceneManager.MoveGameObjectToScene(configObject, scene);
            config = configObject.AddComponent<SampleSceneAuthoringConfig>();
            EditorSceneManager.MarkSceneDirty(scene);
        }

        Selection.activeObject = config.gameObject;
        EditorGUIUtility.PingObject(config.gameObject);
    }

    static SampleSceneAuthoringConfigEditorBootstrap()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += EnsureForActiveScene;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        _ = mode;
        EnsureConfigObject(scene);
    }

    private static void EnsureForActiveScene()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EnsureConfigObject(SceneManager.GetActiveScene());
    }

    private static void EnsureConfigObject(Scene scene)
    {
        if (!scene.IsValid() || Application.isPlaying)
        {
            return;
        }

        if (!string.Equals(scene.name, SampleSceneName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (FindInScene<SampleSceneAuthoringConfig>(scene) != null)
        {
            return;
        }

        GameObject configObject = new GameObject(ConfigObjectName);
        SceneManager.MoveGameObjectToScene(configObject, scene);
        configObject.AddComponent<SampleSceneAuthoringConfig>();
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            T found = roots[i].GetComponentInChildren<T>(true);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
#endif
