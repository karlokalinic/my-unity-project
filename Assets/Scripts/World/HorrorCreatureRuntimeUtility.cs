using UnityEngine;

/// <summary>
/// Shared runtime helpers for binding the real horror-creature assets to gameplay actors.
/// Keeps rendering, scale and collision aligned to the same physical creature.
/// </summary>
public static class HorrorCreatureRuntimeUtility
{
    private const string CollisionRootName = "__CreatureCollision";

    public static bool TryInstantiateVisual(
        Transform host,
        string resourcePath,
        string visualName,
        float targetHeight,
        float groundY,
        out GameObject visual)
    {
        visual = null;
        if (host == null || string.IsNullOrWhiteSpace(resourcePath))
        {
            return false;
        }

        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[HorrorCreatureRuntimeUtility] Missing creature resource '{resourcePath}'.");
            return false;
        }

        visual = Object.Instantiate(prefab, host);
        visual.name = string.IsNullOrWhiteSpace(visualName) ? prefab.name : visualName;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        DisableImportedPhysics(visual);
        FitVisualToHeight(visual.transform, targetHeight);
        AlignVisualFeet(visual.transform, groundY);
        ConfigureRenderers(visual);
        return true;
    }

    public static void HideExistingRenderers(GameObject host, Transform preservedVisualRoot)
    {
        if (host == null)
        {
            return;
        }

        Renderer[] renderers = host.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            if (preservedVisualRoot != null &&
                (rendererComponent.transform == preservedVisualRoot ||
                 rendererComponent.transform.IsChildOf(preservedVisualRoot)))
            {
                continue;
            }

            rendererComponent.enabled = false;
        }
    }

    public static void ConfigureCompoundCollision(
        GameObject host,
        float targetHeight,
        float bulkScale,
        float groundY,
        bool disableExistingRootSolidColliders)
    {
        if (host == null || host.transform.Find(CollisionRootName) != null)
        {
            return;
        }

        if (disableExistingRootSolidColliders)
        {
            Collider[] existingRootColliders = host.GetComponents<Collider>();
            for (int i = 0; i < existingRootColliders.Length; i++)
            {
                Collider colliderComponent = existingRootColliders[i];
                if (colliderComponent != null && !colliderComponent.isTrigger)
                {
                    colliderComponent.enabled = false;
                }
            }
        }

        float lossyY = Mathf.Max(0.01f, Mathf.Abs(host.transform.lossyScale.y));
        float lossyXZ = Mathf.Max(
            0.01f,
            (Mathf.Abs(host.transform.lossyScale.x) + Mathf.Abs(host.transform.lossyScale.z)) * 0.5f);

        float h = Mathf.Max(0.6f, (targetHeight / 2.3f) / lossyY);
        float b = Mathf.Max(0.6f, bulkScale / lossyXZ);

        GameObject collisionRoot = new GameObject(CollisionRootName);
        collisionRoot.transform.SetParent(host.transform, false);
        collisionRoot.transform.position = new Vector3(
            host.transform.position.x,
            groundY,
            host.transform.position.z);

        CapsuleCollider torso = collisionRoot.AddComponent<CapsuleCollider>();
        torso.center = new Vector3(0f, 1.25f * h, 0f);
        torso.radius = 0.29f * b;
        torso.height = 1.55f * h;
        torso.direction = 1;

        SphereCollider head = collisionRoot.AddComponent<SphereCollider>();
        head.center = new Vector3(0f, 2.05f * h, 0.02f);
        head.radius = 0.25f * Mathf.Max(h, b);

        AddLimbCollider(collisionRoot.transform, "Arm_L", new Vector3(-0.45f * b, 1.22f * h, 0f), new Vector3(0f, 0f, -18f), 1.35f * h, 0.10f * b);
        AddLimbCollider(collisionRoot.transform, "Arm_R", new Vector3(0.45f * b, 1.22f * h, 0f), new Vector3(0f, 0f, 18f), 1.35f * h, 0.10f * b);
        AddLimbCollider(collisionRoot.transform, "Leg_L", new Vector3(-0.18f * b, 0.52f * h, 0f), new Vector3(7f, 0f, 0f), 1.05f * h, 0.12f * b);
        AddLimbCollider(collisionRoot.transform, "Leg_R", new Vector3(0.18f * b, 0.52f * h, 0f), new Vector3(-7f, 0f, 0f), 1.05f * h, 0.12f * b);
    }

    public static void SetCompoundCollisionEnabled(GameObject host, bool enabled)
    {
        if (host == null)
        {
            return;
        }

        if (enabled)
        {
            Collider[] rootColliders = host.GetComponents<Collider>();
            for (int i = 0; i < rootColliders.Length; i++)
            {
                Collider colliderComponent = rootColliders[i];
                if (colliderComponent != null && !colliderComponent.isTrigger)
                {
                    colliderComponent.enabled = false;
                }
            }
        }

        Transform collisionRoot = host.transform.Find(CollisionRootName);
        if (collisionRoot == null)
        {
            return;
        }

        Collider[] compound = collisionRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < compound.Length; i++)
        {
            if (compound[i] != null)
            {
                compound[i].enabled = enabled;
            }
        }
    }

    public static bool TryGetBounds(Transform root, out Bounds combined)
    {
        combined = default;
        if (root == null)
        {
            return false;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                combined = rendererComponent.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(rendererComponent.bounds);
            }
        }

        return hasBounds;
    }

    private static void DisableImportedPhysics(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        Rigidbody[] bodies = visual.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            Rigidbody body = bodies[i];
            if (body == null)
            {
                continue;
            }

            body.isKinematic = true;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private static void FitVisualToHeight(Transform visualRoot, float targetHeight)
    {
        if (!TryGetBounds(visualRoot, out Bounds bounds) || bounds.size.y <= 0.001f)
        {
            return;
        }

        float scale = Mathf.Clamp(targetHeight / bounds.size.y, 0.02f, 20f);
        visualRoot.localScale *= scale;
    }

    private static void AlignVisualFeet(Transform visualRoot, float groundY)
    {
        if (!TryGetBounds(visualRoot, out Bounds bounds))
        {
            return;
        }

        visualRoot.position += Vector3.up * (groundY - bounds.min.y);
    }

    private static void ConfigureRenderers(GameObject visual)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            rendererComponent.enabled = true;
            rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rendererComponent.receiveShadows = true;
            rendererComponent.allowOcclusionWhenDynamic = true;
        }
    }

    private static void AddLimbCollider(
        Transform parent,
        string name,
        Vector3 localPosition,
        Vector3 localEuler,
        float height,
        float radius)
    {
        GameObject limb = new GameObject(name);
        limb.transform.SetParent(parent, false);
        limb.transform.localPosition = localPosition;
        limb.transform.localRotation = Quaternion.Euler(localEuler);

        CapsuleCollider colliderComponent = limb.AddComponent<CapsuleCollider>();
        colliderComponent.direction = 1;
        colliderComponent.height = Mathf.Max(radius * 2f, height);
        colliderComponent.radius = Mathf.Max(0.02f, radius);
    }
}
