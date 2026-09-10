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
    private const string RakeResourcePath = "ThirdParty/TheRake/TheRake";
    private const string SnowmanResourcePath = "ThirdParty/AbominableSnowman/AbominableSnowman";

    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        ValidateEnabledBuildScenes();
        ValidatePitchBlackCreatureAssets();
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

    private static void ValidatePitchBlackCreatureAssets()
    {
        ValidateCreatureAsset("The Rake", RakeResourcePath);
        ValidateCreatureAsset("The Abominable Snowman", SnowmanResourcePath);

        ValidateRequiredTexture("Assets/Resources/ThirdParty/TheRake/lambert1_Base_Color3.jpg");
        ValidateRequiredTexture("Assets/Resources/ThirdParty/TheRake/lambert1_Base_Color4.jpg");
        ValidateRequiredTexture("Assets/Resources/ThirdParty/TheRake/lambert1_Normal_OpenGL.png");
        ValidateRequiredTexture("Assets/Resources/ThirdParty/TheRake/lambert1_Roughness.png");
    }

    private static void ValidateCreatureAsset(string label, string resourcePath)
    {
        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            throw new BuildFailedException(
                $"{label} cinematic asset did not import as a loadable GameObject at Resources/{resourcePath}. " +
                "Cloud deployment is blocked instead of shipping the procedural stand-in.");
        }

        if (prefab.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
        {
            throw new BuildFailedException(
                $"{label} cinematic asset at Resources/{resourcePath} has no SkinnedMeshRenderer. " +
                "The requested rigged creature did not import correctly.");
        }

        var clips = Resources.LoadAll<AnimationClip>(resourcePath);
        if (clips == null || clips.Length == 0)
        {
            throw new BuildFailedException(
                $"{label} cinematic asset at Resources/{resourcePath} contains no loadable AnimationClip sub-assets.");
        }

        AnimationClip chargeClip = FindChargeClip(clips);
        if (chargeClip == null)
        {
            throw new BuildFailedException(
                $"{label} cinematic asset has animation clips, but none match run/sprint/charge/walk. " +
                "The monster charge would not use its intended animation.");
        }

        Debug.Log(
            $"[CloudBuildGuard] {label}: prefab + skinned renderer + charge clip '{chargeClip.name}' validated from Resources/{resourcePath}.");
    }

    private static AnimationClip FindChargeClip(AnimationClip[] clips)
    {
        string[] priorities = { "run", "sprint", "charge", "walk" };
        for (var priorityIndex = 0; priorityIndex < priorities.Length; priorityIndex++)
        {
            var token = priorities[priorityIndex];
            for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
            {
                var clip = clips[clipIndex];
                if (clip != null && clip.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return clip;
                }
            }
        }

        return null;
    }

    private static void ValidateRequiredTexture(string assetPath)
    {
        if (!File.Exists(assetPath) || AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath) == null)
        {
            throw new BuildFailedException($"Required cinematic texture failed to import: '{assetPath}'.");
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
