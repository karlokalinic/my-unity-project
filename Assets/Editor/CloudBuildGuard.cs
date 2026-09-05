using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CloudBuildGuard : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        ValidateEnabledBuildScenes();
        Debug.Log($"[CloudBuildGuard] Pre-build validation passed for {report.summary.platform}.");
    }

    private static void ValidateEnabledBuildScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledCount = 0;

        foreach (var sceneEntry in scenes)
        {
            if (!sceneEntry.enabled)
            {
                continue;
            }

            enabledCount++;
            var path = sceneEntry.path;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new BuildFailedException($"Enabled build scene is missing: '{path}'.");
            }

            ValidateScene(path);
        }

        if (enabledCount == 0)
        {
            throw new BuildFailedException("No enabled scenes exist in EditorBuildSettings.");
        }
    }

    private static void ValidateScene(string path)
    {
        var existing = SceneManager.GetSceneByPath(path);
        var openedForValidation = !existing.IsValid() || !existing.isLoaded;
        var scene = openedForValidation
            ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive)
            : existing;

        try
        {
            var missingScripts = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                missingScripts += CountMissingScriptsRecursive(root.transform);
            }

            if (missingScripts > 0)
            {
                throw new BuildFailedException(
                    $"Scene '{path}' contains {missingScripts} missing MonoBehaviour script reference(s). " +
                    "Cloud deployment is blocked until they are fixed.");
            }
        }
        finally
        {
            if (openedForValidation && scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    private static int CountMissingScriptsRecursive(Transform transform)
    {
        var total = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
        for (var i = 0; i < transform.childCount; i++)
        {
            total += CountMissingScriptsRecursive(transform.GetChild(i));
        }

        return total;
    }
}
