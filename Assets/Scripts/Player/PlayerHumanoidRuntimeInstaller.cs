using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime authority for attaching the canonical rendered humanoid to PlayerMover actors.
/// The player FBX lives under Resources so WebGL cannot silently omit it when build-scene
/// processing changes or a scene was authored without a serialized visual reference.
/// </summary>
public static class PlayerHumanoidRuntimeInstaller
{
    public const string PlayerResourcePath = "Player/Ch01_nonPBR@Double Dagger Stab";
    public const string VisualRootName = "StoreModelVisual";
    public const float TargetPlayerHeight = 1.78f;

    private static GameObject cachedModel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallInitialScene()
    {
        EnsureAllPlayers();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureAllPlayers();
    }

    public static bool EnsureAllPlayers()
    {
        PlayerMover[] players = Object.FindObjectsByType<PlayerMover>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (players == null || players.Length == 0)
        {
            return false;
        }

        bool allReady = true;
        for (int i = 0; i < players.Length; i++)
        {
            PlayerMover player = players[i];
            if (player == null)
            {
                continue;
            }

            allReady &= EnsurePlayer(player.gameObject);
        }

        return allReady;
    }

    public static bool EnsurePlayer(GameObject actorRoot)
    {
        if (actorRoot == null)
        {
            return false;
        }

        GameObject modelAsset = ResolveModelAsset();
        if (modelAsset == null)
        {
            Debug.LogError(
                $"[PlayerHumanoidRuntimeInstaller] Missing canonical runtime humanoid at Resources/{PlayerResourcePath}. " +
                "The player will remain physically functional but no procedural primitive is allowed as a visual substitute.",
                actorRoot);
            HideProceduralRenderers(actorRoot);
            return false;
        }

        Transform visualRoot = actorRoot.transform.Find(VisualRootName);
        if (visualRoot == null || !HasUsableHumanoidVisual(visualRoot))
        {
            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(false);
                visualRoot.name = "__InvalidStoreModelVisual";
                Object.Destroy(visualRoot.gameObject);
            }

            GameObject visualRootObject = new GameObject(VisualRootName);
            visualRoot = visualRootObject.transform;
            visualRoot.SetParent(actorRoot.transform, false);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;

            GameObject modelInstance = Object.Instantiate(modelAsset, visualRoot);
            modelInstance.name = modelAsset.name;
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;
        }

        DisableImportedPhysics(visualRoot.gameObject);
        SetLayerRecursive(visualRoot, actorRoot.layer);
        FitToHeight(visualRoot, TargetPlayerHeight);
        AlignFeetToControllerGround(actorRoot, visualRoot);
        ConfigureRendering(visualRoot.gameObject);

        EnsurePlayerDetailDrivers(actorRoot);

        ProceduralHumanoidRig rig = actorRoot.GetComponent<ProceduralHumanoidRig>();
        if (rig != null)
        {
            rig.EnsureBuilt();
            rig.ConfigureRendererVisibility(false, false);
            rig.SetRagdollRenderersVisible(false);
        }

        PlayerHumanoidVisualDriver visualDriver = actorRoot.GetComponent<PlayerHumanoidVisualDriver>();
        if (visualDriver != null)
        {
            visualDriver.ResolveNow();
        }

        bool ready = visualDriver != null && visualDriver.IsHumanoidBound && HasVisibleSkinnedRenderer(visualRoot);
        if (!ready)
        {
            Debug.LogError(
                "[PlayerHumanoidRuntimeInstaller] Canonical player model loaded but failed Humanoid/render binding. " +
                "Procedural ragdoll renderers remain hidden so the failure cannot masquerade as the final character.",
                actorRoot);
            HideProceduralRenderers(actorRoot);
            return false;
        }

        Debug.Log(
            $"[PlayerHumanoidRuntimeInstaller] REAL_HUMANOID_READY resource={PlayerResourcePath} height={TargetPlayerHeight:0.00}m",
            actorRoot);
        return true;
    }

    private static GameObject ResolveModelAsset()
    {
        if (cachedModel == null)
        {
            cachedModel = Resources.Load<GameObject>(PlayerResourcePath);
        }

        return cachedModel;
    }

    private static void EnsurePlayerDetailDrivers(GameObject actorRoot)
    {
        EnsureComponent<ProceduralHumanoidRig>(actorRoot);
        EnsureComponent<PlayerAnimationController>(actorRoot);
        EnsureComponent<PlayerHumanoidVisualDriver>(actorRoot);
        EnsureComponent<PlayerMicroMotionDetailDriver>(actorRoot);
        EnsureComponent<PlayerEyeGazeDetailDriver>(actorRoot);
        EnsureComponent<PlayerAnatomicalDetailDriver>(actorRoot);
        EnsureComponent<PlayerFootGroundingDetailDriver>(actorRoot);
        EnsureComponent<PlayerFacialMicroMotion>(actorRoot);
        EnsureComponent<PlayerHeadAnchorDriver>(actorRoot);
    }

    private static T EnsureComponent<T>(GameObject root) where T : Component
    {
        T existing = root.GetComponent<T>();
        return existing != null ? existing : root.AddComponent<T>();
    }

    private static bool HasUsableHumanoidVisual(Transform visualRoot)
    {
        if (visualRoot == null || visualRoot.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
        {
            return false;
        }

        Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
        return animator != null &&
               animator.avatar != null &&
               animator.avatar.isValid &&
               animator.avatar.isHuman;
    }

    private static bool HasVisibleSkinnedRenderer(Transform visualRoot)
    {
        SkinnedMeshRenderer[] renderers = visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];
            if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private static void HideProceduralRenderers(GameObject actorRoot)
    {
        ProceduralHumanoidRig rig = actorRoot.GetComponent<ProceduralHumanoidRig>();
        if (rig == null)
        {
            return;
        }

        rig.SetRagdollRenderersVisible(false);
    }

    private static void DisableImportedPhysics(GameObject modelRoot)
    {
        Collider[] colliders = modelRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider colliderComponent = colliders[i];
            if (colliderComponent != null)
            {
                colliderComponent.enabled = false;
            }
        }

        Rigidbody[] bodies = modelRoot.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody body = bodies[i];
            if (body == null)
            {
                continue;
            }

            body.isKinematic = true;
            body.useGravity = false;
            body.detectCollisions = false;
        }
    }

    private static void ConfigureRendering(GameObject visualRoot)
    {
        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = true;
            renderer.receiveShadows = true;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.allowOcclusionWhenDynamic = true;
        }
    }

    private static void FitToHeight(Transform visualRoot, float targetHeight)
    {
        if (!TryGetRendererBounds(visualRoot, out Bounds bounds) || bounds.size.y <= 0.001f)
        {
            return;
        }

        float scale = Mathf.Clamp(targetHeight / bounds.size.y, 0.01f, 10f);
        visualRoot.localScale *= scale;
    }

    private static void AlignFeetToControllerGround(GameObject actorRoot, Transform visualRoot)
    {
        if (!TryGetRendererBounds(visualRoot, out Bounds bounds))
        {
            return;
        }

        float groundY = actorRoot.transform.position.y;
        CharacterController controller = actorRoot.GetComponent<CharacterController>();
        if (controller != null)
        {
            Vector3 localBottom = controller.center + Vector3.down * (controller.height * 0.5f);
            groundY = actorRoot.transform.TransformPoint(localBottom).y;
        }

        visualRoot.position += Vector3.up * (groundY - bounds.min.y);
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds combined)
    {
        combined = default;
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                combined = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    private static void SetLayerRecursive(Transform root, int layer)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            if (current != null)
            {
                current.gameObject.layer = layer;
            }
        }
    }
}
