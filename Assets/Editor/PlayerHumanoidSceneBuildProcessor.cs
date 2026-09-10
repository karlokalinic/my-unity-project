using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PlayerHumanoidSceneBuildProcessor : IProcessSceneWithReport
{
    internal const string PlayerModelPath = "Assets/Ch01_nonPBR@Double Dagger Stab.fbx";
    private const string VisualRootName = "StoreModelVisual";
    private const float TargetPlayerHeight = 1.78f;

    private static readonly string[] ProductionSceneNames =
    {
        "Scena",
        "INTERAKCIJA",
        "VerticalSlice_Consolidated"
    };

    public int callbackOrder => -900;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (!IsProductionScene(scene))
        {
            return;
        }

        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
        if (modelAsset == null)
        {
            throw new BuildFailedException($"Player humanoid model is missing at '{PlayerModelPath}'.");
        }

        int playerCount = 0;
        int injectedCount = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
        {
            PlayerMover[] movers = roots[rootIndex].GetComponentsInChildren<PlayerMover>(true);
            for (int moverIndex = 0; moverIndex < movers.Length; moverIndex++)
            {
                PlayerMover mover = movers[moverIndex];
                if (mover == null)
                {
                    continue;
                }

                playerCount++;
                if (EnsurePlayerVisualForBuild(mover.gameObject, modelAsset))
                {
                    injectedCount++;
                }
            }
        }

        if (playerCount == 0)
        {
            throw new BuildFailedException($"Production scene '{scene.name}' has no PlayerMover to receive the humanoid visual.");
        }

        Debug.Log($"[PlayerHumanoidSceneBuildProcessor] {scene.name}: validated {playerCount} player(s), injected {injectedCount} missing humanoid visual(s).");
    }

    public static bool EnsurePlayerVisualForBuild(GameObject actorRoot, GameObject modelAsset = null)
    {
        if (actorRoot == null)
        {
            return false;
        }

        Transform existing = actorRoot.transform.Find(VisualRootName);
        if (existing != null && HasUsableHumanoidVisual(existing))
        {
            DisableImportedPhysics(existing.gameObject);
            SetLayerRecursive(existing, actorRoot.layer);
            FitToHeight(existing, TargetPlayerHeight);
            AlignFeetToControllerGround(actorRoot, existing);
            ConfigureRendering(existing.gameObject);
            EnsurePlayerDetailDrivers(actorRoot);
            return false;
        }

        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing.gameObject);
        }

        if (modelAsset == null)
        {
            modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
        }
        if (modelAsset == null)
        {
            throw new BuildFailedException($"Cannot attach player humanoid because '{PlayerModelPath}' failed to import.");
        }

        GameObject visualRootObject = new GameObject(VisualRootName);
        Scene actorScene = actorRoot.scene;
        if (actorScene.IsValid() && actorScene.isLoaded && visualRootObject.scene != actorScene)
        {
            SceneManager.MoveGameObjectToScene(visualRootObject, actorScene);
        }
        visualRootObject.transform.SetParent(actorRoot.transform, false);
        visualRootObject.transform.localPosition = Vector3.zero;
        visualRootObject.transform.localRotation = Quaternion.identity;
        visualRootObject.transform.localScale = Vector3.one;

        GameObject modelInstance = actorScene.IsValid() && actorScene.isLoaded
            ? PrefabUtility.InstantiatePrefab(modelAsset, actorScene) as GameObject
            : PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
        if (modelInstance == null)
        {
            modelInstance = UnityEngine.Object.Instantiate(modelAsset);
            if (modelInstance != null && actorScene.IsValid() && actorScene.isLoaded && modelInstance.scene != actorScene)
            {
                SceneManager.MoveGameObjectToScene(modelInstance, actorScene);
            }
        }
        if (modelInstance == null)
        {
            UnityEngine.Object.DestroyImmediate(visualRootObject);
            throw new BuildFailedException("Failed to instantiate the canonical player humanoid model for the build scene.");
        }

        modelInstance.name = modelAsset.name;
        modelInstance.transform.SetParent(visualRootObject.transform, false);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;

        DisableImportedPhysics(modelInstance);
        SetLayerRecursive(visualRootObject.transform, actorRoot.layer);
        FitToHeight(visualRootObject.transform, TargetPlayerHeight);
        AlignFeetToControllerGround(actorRoot, visualRootObject.transform);
        ConfigureRendering(visualRootObject);
        EnsurePlayerDetailDrivers(actorRoot);

        if (!HasUsableHumanoidVisual(visualRootObject.transform))
        {
            throw new BuildFailedException("Canonical player model was injected but did not produce a valid Humanoid Animator + skinned renderer hierarchy.");
        }

        return true;
    }

    private static bool IsProductionScene(Scene scene)
    {
        if (!scene.IsValid())
        {
            return false;
        }

        for (int i = 0; i < ProductionSceneNames.Length; i++)
        {
            if (string.Equals(scene.name, ProductionSceneNames[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasUsableHumanoidVisual(Transform visualRoot)
    {
        if (visualRoot == null || visualRoot.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
        {
            return false;
        }

        Animator animator = visualRoot.GetComponentInChildren<Animator>(true);
        return animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman;
    }

    private static void EnsurePlayerDetailDrivers(GameObject actorRoot)
    {
        if (actorRoot.GetComponent<ProceduralHumanoidRig>() == null)
        {
            actorRoot.AddComponent<ProceduralHumanoidRig>();
        }
        if (actorRoot.GetComponent<PlayerAnimationController>() == null)
        {
            actorRoot.AddComponent<PlayerAnimationController>();
        }
        if (actorRoot.GetComponent<PlayerHumanoidVisualDriver>() == null)
        {
            actorRoot.AddComponent<PlayerHumanoidVisualDriver>();
        }
        if (actorRoot.GetComponent<PlayerMicroMotionDetailDriver>() == null)
        {
            actorRoot.AddComponent<PlayerMicroMotionDetailDriver>();
        }
        if (actorRoot.GetComponent<PlayerAnatomicalDetailDriver>() == null)
        {
            actorRoot.AddComponent<PlayerAnatomicalDetailDriver>();
        }
        if (actorRoot.GetComponent<PlayerFootGroundingDetailDriver>() == null)
        {
            actorRoot.AddComponent<PlayerFootGroundingDetailDriver>();
        }
        if (actorRoot.GetComponent<PlayerFacialMicroMotion>() == null)
        {
            actorRoot.AddComponent<PlayerFacialMicroMotion>();
        }
    }

    private static void DisableImportedPhysics(GameObject modelRoot)
    {
        Collider[] colliders = modelRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
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

            renderer.receiveShadows = true;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.allowOcclusionWhenDynamic = true;
        }
    }

    private static void FitToHeight(Transform visualRoot, float targetHeight)
    {
        if (!TryGetRendererBounds(visualRoot, out Bounds bounds) || bounds.size.y <= 0.001f)
        {
            throw new BuildFailedException("Player humanoid has no usable renderer bounds for world-scale fitting.");
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
            if (transforms[i] != null)
            {
                transforms[i].gameObject.layer = layer;
            }
        }
    }
}
