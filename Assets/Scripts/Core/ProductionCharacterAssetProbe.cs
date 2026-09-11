using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-shot production smoke probe for the real player and horror character assets.
/// It verifies runtime-loadable assets and the actual live player renderer after the
/// runtime humanoid installer has had a chance to bind the canonical skin.
/// </summary>
[DisallowMultipleComponent]
public sealed class ProductionCharacterAssetProbe : MonoBehaviour
{
    private const string RakeResourcePath = "ThirdParty/TheRake/TheRake";
    private const string SnowmanResourcePath = "ThirdParty/AbominableSnowman/AbominableSnowman";

    private static readonly string[] SupportedScenes =
    {
        "Scena",
        "SampleScene",
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
        PlayerHumanoidRuntimeInstaller.EnsureAllPlayers();
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
                $"playerResource={PlayerHumanoidRuntimeInstaller.PlayerResourcePath} " +
                $"playerRenderers={playerRenderers} humanoidBound={humanoidBound} " +
                $"rakeRenderers={rakeRenderers} rakeClips={rakeClips} " +
                $"snowmanRenderers={snowmanRenderers} snowmanClips={snowmanClips}");
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

        GameObject playerResource = Resources.Load<GameObject>(PlayerHumanoidRuntimeInstaller.PlayerResourcePath);
        if (playerResource == null)
        {
            Debug.LogError(
                $"[ProductionCharacterAssetProbe] Player runtime resource missing at Resources/{PlayerHumanoidRuntimeInstaller.PlayerResourcePath}.");
            return false;
        }

        PlayerMover player = FindAnyObjectByType<PlayerMover>();
        if (player == null)
        {
            Debug.LogError("[ProductionCharacterAssetProbe] Shipping scene has no PlayerMover.");
            return false;
        }

        PlayerHumanoidRuntimeInstaller.EnsurePlayer(player.gameObject);

        Transform visualRoot = player.transform.Find(PlayerHumanoidRuntimeInstaller.VisualRootName);
        if (visualRoot == null)
        {
            Debug.LogError("[ProductionCharacterAssetProbe] Player is missing runtime StoreModelVisual.");
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

        bool hasVisibleSkin = false;
        for (int i = 0; renderers != null && i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
            {
                hasVisibleSkin = true;
                break;
            }
        }

        if (skinnedRendererCount <= 0 || !humanoidBound || !hasVisibleSkin)
        {
            Debug.LogError(
                $"[ProductionCharacterAssetProbe] Player humanoid binding incomplete: " +
                $"skinnedRenderers={skinnedRendererCount}, humanoidBound={humanoidBound}, visibleSkin={hasVisibleSkin}.");
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
