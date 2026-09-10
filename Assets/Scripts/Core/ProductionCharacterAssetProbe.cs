using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-shot production smoke probe for the real player and horror character assets.
/// It does not own gameplay or render state; it only verifies the exact objects that
/// must already exist in a shipping scene and emits an actionable browser/player log.
/// </summary>
[DisallowMultipleComponent]
public sealed class ProductionCharacterAssetProbe : MonoBehaviour
{
    private const string RakeResourcePath = "ThirdParty/TheRake/TheRake";
    private const string SnowmanResourcePath = "ThirdParty/AbominableSnowman/AbominableSnowman";
    private const string PlayerVisualRootName = "StoreModelVisual";

    private static readonly string[] SupportedScenes =
    {
        "Scena",
        "INTERAKCIJA",
        "VerticalSlice_Consolidated"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !IsSupportedScene(scene.name))
        {
            return;
        }

        if (FindAnyObjectByType<ProductionCharacterAssetProbe>() != null)
        {
            return;
        }

        GameObject host = new GameObject("__ProductionCharacterAssetProbe");
        host.hideFlags = HideFlags.DontSave;
        host.AddComponent<ProductionCharacterAssetProbe>();
    }

    private IEnumerator Start()
    {
        // Build-time injected scene objects have already been deserialized by this point,
        // but one frame gives Awake/OnEnable based binding a chance to finish as well.
        yield return null;
        ValidateProductionCharacters();
        Destroy(gameObject);
    }

    private void ValidateProductionCharacters()
    {
        bool rakeReady = ValidateCreature("The Rake", RakeResourcePath, out int rakeRenderers, out int rakeClips);
        bool snowmanReady = ValidateCreature("The Abominable Snowman", SnowmanResourcePath, out int snowRenderers, out int snowClips);
        bool playerReady = ValidatePlayer(out int playerRenderers, out bool humanoidBound);

        if (rakeReady && snowmanReady && playerReady)
        {
            Debug.Log(
                $"[ProductionCharacterAssetProbe] ONLINE_ASSET_READY " +
                $"playerRenderers={playerRenderers} humanoidBound={humanoidBound} " +
                $"rakeRenderers={rakeRenderers} rakeClips={rakeClips} " +
                $"snowmanRenderers={snowRenderers} snowmanClips={snowClips}");
            return;
        }

        Debug.LogError(
            $"[ProductionCharacterAssetProbe] ONLINE_ASSET_FAILURE " +
            $"player={playerReady} rake={rakeReady} snowman={snowmanReady}. " +
            "The shipping build is missing or failed to bind one or more canonical character assets.");
    }

    private static bool ValidateCreature(
        string label,
        string resourcePath,
        out int skinnedRendererCount,
        out int animationClipCount)
    {
        skinnedRendererCount = 0;
        animationClipCount = 0;

        GameObject model = Resources.Load<GameObject>(resourcePath);
        if (model == null)
        {
            Debug.LogError($"[ProductionCharacterAssetProbe] {label} missing at Resources/{resourcePath}.");
            return false;
        }

        SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        skinnedRendererCount = renderers != null ? renderers.Length : 0;

        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(resourcePath);
        animationClipCount = clips != null ? clips.Length : 0;

        if (skinnedRendererCount <= 0 || animationClipCount <= 0)
        {
            Debug.LogError(
                $"[ProductionCharacterAssetProbe] {label} imported incompletely: " +
                $"skinnedRenderers={skinnedRendererCount}, animationClips={animationClipCount}.");
            return false;
        }

        return true;
    }

    private static bool ValidatePlayer(out int skinnedRendererCount, out bool humanoidBound)
    {
        skinnedRendererCount = 0;
        humanoidBound = false;

        PlayerMover player = FindAnyObjectByType<PlayerMover>();
        if (player == null)
        {
            Debug.LogError("[ProductionCharacterAssetProbe] Shipping scene has no PlayerMover.");
            return false;
        }

        Transform visualRoot = player.transform.Find(PlayerVisualRootName);
        if (visualRoot == null)
        {
            Debug.LogError("[ProductionCharacterAssetProbe] Player is missing build-injected StoreModelVisual.");
            return false;
        }

        SkinnedMeshRenderer[] renderers = visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        skinnedRendererCount = renderers != null ? renderers.Length : 0;

        PlayerHumanoidVisualDriver driver = player.GetComponent<PlayerHumanoidVisualDriver>();
        if (driver != null)
        {
            driver.ResolveNow();
            humanoidBound = driver.IsHumanoidBound;
        }

        if (skinnedRendererCount <= 0 || !humanoidBound)
        {
            Debug.LogError(
                $"[ProductionCharacterAssetProbe] Player humanoid binding incomplete: " +
                $"skinnedRenderers={skinnedRendererCount}, humanoidBound={humanoidBound}.");
            return false;
        }

        return true;
    }

    private static bool IsSupportedScene(string sceneName)
    {
        for (int i = 0; i < SupportedScenes.Length; i++)
        {
            if (string.Equals(sceneName, SupportedScenes[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
