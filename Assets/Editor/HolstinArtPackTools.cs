using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class HolstinArtPackTools
{
    private const string DefaultPackPath = "Assets/Data/Art/HolstinArtPack.asset";
    private const string ArtRootFolder = "Assets/Game/Art";
    private const string ArtPrefabsFolder = "Assets/Game/Art/Prefabs";
    private const string RecoveredPrefabsFolder = ArtPrefabsFolder + "/Recovered";
    private const string ArtMaterialsFolder = "Assets/Game/Art/Materials";

    [MenuItem("Tools/Holstin/Art Direction/Create Default Art Pack Asset", false, 80)]
    public static void CreateDefaultArtPackAssetMenu()
    {
        HolstinArtPack pack = LoadOrCreateDefaultPack();
        if (pack == null)
        {
            Debug.LogError("Failed to create default HolstinArtPack asset.");
            return;
        }

        Selection.activeObject = pack;
        EditorGUIUtility.PingObject(pack);
        Debug.Log(
            $"Default HolstinArtPack ready at '{DefaultPackPath}'. " +
            $"Import prefabs into '{ArtPrefabsFolder}'.");
    }

    [MenuItem("Tools/Holstin/Art Direction/Seed Default Art Pack From Scene Placeholders", false, 81)]
    public static void SeedDefaultArtPackFromSceneMenu()
    {
        HolstinArtPack pack = LoadOrCreateDefaultPack();
        if (pack == null)
        {
            Debug.LogError("Cannot seed mappings because the default HolstinArtPack asset could not be loaded.");
            return;
        }

        int added = SeedMappingsFromScene(pack);
        if (added > 0)
        {
            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();
        }

        Selection.activeObject = pack;
        Debug.Log($"Seeded art pack mappings from placeholders. Added {added} new mapping(s).");
    }

    [MenuItem("Tools/Holstin/Art Direction/Apply Default Art Pack To Active Scene", false, 82)]
    public static void ApplyDefaultArtPackToActiveSceneMenu()
    {
        HolstinArtPack pack = AssetDatabase.LoadAssetAtPath<HolstinArtPack>(DefaultPackPath);
        if (pack == null)
        {
            Debug.LogWarning($"No default HolstinArtPack found at '{DefaultPackPath}'. Create one first.");
            return;
        }

        ApplyArtPackToActiveScene(pack);
    }

    [MenuItem("Tools/Holstin/Art Direction/Apply Selected Art Pack To Active Scene", false, 83)]
    public static void ApplySelectedArtPackToActiveSceneMenu()
    {
        HolstinArtPack pack = Selection.activeObject as HolstinArtPack;
        if (pack == null)
        {
            Debug.LogWarning("Select a HolstinArtPack asset in the Project window first.");
            return;
        }

        ApplyArtPackToActiveScene(pack);
    }

    [MenuItem("Tools/Holstin/Art Direction/Auto-Assign Default Art Pack Prefabs (Selected Folder)", false, 84)]
    public static void AutoAssignDefaultArtPackFromSelectedFolderMenu()
    {
        HolstinArtPack pack = AssetDatabase.LoadAssetAtPath<HolstinArtPack>(DefaultPackPath);
        if (pack == null)
        {
            Debug.LogWarning($"No default HolstinArtPack found at '{DefaultPackPath}'. Create one first.");
            return;
        }

        string folder = ResolveSelectedProjectFolder();
        int assigned = AutoAssignMappingsFromFolder(pack, folder, true);
        if (assigned > 0)
        {
            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();
        }

        Selection.activeObject = pack;
        Debug.Log($"Auto-assigned {assigned} mapping prefab(s) from '{folder}'.");
    }

    [MenuItem("Tools/Holstin/Art Direction/Recover Default Art Pack From Current Scene Placements", false, 85)]
    public static void RecoverDefaultArtPackFromCurrentSceneMenu()
    {
        HolstinArtPack pack = LoadOrCreateDefaultPack();
        if (pack == null)
        {
            Debug.LogError("Cannot recover mappings because the default HolstinArtPack asset could not be loaded.");
            return;
        }

        int seeded = SeedMappingsFromScene(pack);
        int recovered = RecoverMappingsFromCurrentScenePlacements(pack);
        if (seeded > 0 || recovered > 0)
        {
            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();
        }

        Selection.activeObject = pack;
        Debug.Log($"Recovered {recovered} mapping(s) from current scene placements. Seeded {seeded} new mapping key(s).");
    }

    private static void ApplyArtPackToActiveScene(HolstinArtPack pack)
    {
        if (pack == null)
        {
            return;
        }

        AssetPlaceholder[] placeholders = Object.FindObjectsByType<AssetPlaceholder>(FindObjectsInactive.Include);
        if (placeholders.Length == 0)
        {
            Debug.Log("No AssetPlaceholder objects found in the active scene.");
            return;
        }

        int replaced = 0;
        int missingMapping = 0;
        int missingPrefab = 0;

        Undo.SetCurrentGroupName("Apply Holstin Art Pack");
        int undoGroup = Undo.GetCurrentGroup();

        for (int i = 0; i < placeholders.Length; i++)
        {
            AssetPlaceholder placeholder = placeholders[i];
            if (placeholder == null)
            {
                continue;
            }

            if (!pack.TryFindMapping(placeholder, out HolstinArtPack.PlaceholderMapping mapping))
            {
                missingMapping++;
                continue;
            }

            if (mapping.prefab == null)
            {
                missingPrefab++;
                continue;
            }

            RemoveExistingLinkedInstances(placeholder);

            GameObject instance = PrefabUtility.InstantiatePrefab(mapping.prefab, placeholder.transform.parent) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(mapping.prefab, placeholder.transform.parent);
            }

            if (instance == null)
            {
                missingPrefab++;
                continue;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Art Pack Prefab");
            instance.name = placeholder.gameObject.name + "_Art";

            PositionInstance(instance, placeholder, mapping);
            ApplyMaterialOverrides(instance, mapping.materialOverrides);

            HolstinArtInstanceLink link = instance.GetComponent<HolstinArtInstanceLink>();
            if (link == null)
            {
                link = instance.AddComponent<HolstinArtInstanceLink>();
            }
            link.Bind(placeholder);

            if (mapping.setStaticFlags)
            {
                SetStaticFlagsRecursive(instance.transform);
            }

            if (mapping.preservePlaceholder)
            {
                HidePlaceholderVisuals(placeholder);
            }
            else
            {
                Undo.RecordObject(placeholder.gameObject, "Disable Placeholder");
                placeholder.gameObject.SetActive(false);
            }

            replaced++;
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log(
            $"Applied HolstinArtPack '{pack.name}' - replaced {replaced}, missing mapping {missingMapping}, missing prefab {missingPrefab}.");
    }

    private static void PositionInstance(GameObject instance, AssetPlaceholder placeholder, HolstinArtPack.PlaceholderMapping mapping)
    {
        Transform instanceTransform = instance.transform;
        Transform placeholderTransform = placeholder.transform;

        instanceTransform.rotation = placeholderTransform.rotation * Quaternion.Euler(mapping.localRotationEuler);
        instanceTransform.position = placeholderTransform.position + placeholderTransform.TransformVector(mapping.localPositionOffset);
        instanceTransform.localScale = Vector3.Scale(instanceTransform.localScale, mapping.localScaleMultiplier);

        if (mapping.fitToPlaceholderBounds)
        {
            FitInstanceToPlaceholderBounds(instanceTransform, placeholder.BoundsSize, placeholderTransform.position);
        }
    }

    private static void FitInstanceToPlaceholderBounds(Transform instanceTransform, Vector3 targetSize, Vector3 placeholderPosition)
    {
        if (!TryComputeRendererBounds(instanceTransform, out Bounds bounds))
        {
            return;
        }

        Vector3 sourceSize = bounds.size;
        if (sourceSize.x <= 0.001f || sourceSize.y <= 0.001f || sourceSize.z <= 0.001f)
        {
            return;
        }

        float scaleX = targetSize.x / sourceSize.x;
        float scaleY = targetSize.y / sourceSize.y;
        float scaleZ = targetSize.z / sourceSize.z;
        float uniformScale = Mathf.Max(0.01f, Mathf.Min(scaleX, Mathf.Min(scaleY, scaleZ)));
        instanceTransform.localScale *= uniformScale;

        if (!TryComputeRendererBounds(instanceTransform, out Bounds scaledBounds))
        {
            return;
        }

        Vector3 desiredCenter = placeholderPosition + (Vector3.up * (targetSize.y * 0.5f));
        Vector3 correction = desiredCenter - scaledBounds.center;
        instanceTransform.position += correction;
    }

    private static bool TryComputeRendererBounds(Transform root, out Bounds bounds)
    {
        bounds = new Bounds();
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null || !rendererComponent.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = rendererComponent.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(rendererComponent.bounds);
            }
        }

        return hasBounds;
    }

    private static void ApplyMaterialOverrides(GameObject instance, Material[] overrides)
    {
        if (overrides == null || overrides.Length == 0)
        {
            return;
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            Material[] sharedMaterials = rendererComponent.sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0)
            {
                rendererComponent.sharedMaterial = overrides[0];
                continue;
            }

            for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                Material overrideMaterial = overrides[Mathf.Min(materialIndex, overrides.Length - 1)];
                if (overrideMaterial != null)
                {
                    sharedMaterials[materialIndex] = overrideMaterial;
                }
            }

            rendererComponent.sharedMaterials = sharedMaterials;
        }
    }

    private static void RemoveExistingLinkedInstances(AssetPlaceholder placeholder)
    {
        HolstinArtInstanceLink[] links = Object.FindObjectsByType<HolstinArtInstanceLink>(FindObjectsInactive.Include);
        for (int i = 0; i < links.Length; i++)
        {
            HolstinArtInstanceLink link = links[i];
            if (link == null || link.SourcePlaceholder != placeholder)
            {
                continue;
            }

            Undo.DestroyObjectImmediate(link.gameObject);
        }
    }

    private static void SetStaticFlagsRecursive(Transform root)
    {
        StaticEditorFlags flags =
            StaticEditorFlags.BatchingStatic |
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.ReflectionProbeStatic;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null)
            {
                GameObjectUtility.SetStaticEditorFlags(children[i].gameObject, flags);
            }
        }
    }

    private static void HidePlaceholderVisuals(AssetPlaceholder placeholder)
    {
        if (placeholder == null)
        {
            return;
        }

        Renderer[] renderers = placeholder.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }

        Collider[] colliders = placeholder.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }
    }

    private static HolstinArtPack LoadOrCreateDefaultPack()
    {
        EnsureArtImportFolders();

        HolstinArtPack existing = AssetDatabase.LoadAssetAtPath<HolstinArtPack>(DefaultPackPath);
        if (existing != null)
        {
            return existing;
        }

        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/Art");

        HolstinArtPack created = ScriptableObject.CreateInstance<HolstinArtPack>();
        AssetDatabase.CreateAsset(created, DefaultPackPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return created;
    }

    private static void EnsureArtImportFolders()
    {
        EnsureFolder(ArtRootFolder);
        EnsureFolder(ArtPrefabsFolder);
        EnsureFolder(ArtMaterialsFolder);
    }

    private static int SeedMappingsFromScene(HolstinArtPack pack)
    {
        if (pack == null)
        {
            return 0;
        }

        AssetPlaceholder[] placeholders = Object.FindObjectsByType<AssetPlaceholder>(FindObjectsInactive.Include);
        if (placeholders.Length == 0)
        {
            return 0;
        }

        List<HolstinArtPack.PlaceholderMapping> mappings = pack.EditorMappings;
        HashSet<string> existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < mappings.Count; i++)
        {
            HolstinArtPack.PlaceholderMapping mapping = mappings[i];
            if (mapping == null)
            {
                continue;
            }

            existingKeys.Add(BuildKey(mapping.category, mapping.assetTag));
        }

        int added = 0;
        for (int i = 0; i < placeholders.Length; i++)
        {
            AssetPlaceholder placeholder = placeholders[i];
            if (placeholder == null)
            {
                continue;
            }

            string key = BuildKey(placeholder.Category, placeholder.AssetTag);
            if (existingKeys.Contains(key))
            {
                continue;
            }

            HolstinArtPack.PlaceholderMapping mapping = new HolstinArtPack.PlaceholderMapping
            {
                id = $"auto_{placeholder.Category}_{NormalizeTag(placeholder.AssetTag)}_{added:00}",
                category = placeholder.Category,
                assetTag = placeholder.AssetTag,
                prefab = null,
                localPositionOffset = Vector3.zero,
                localRotationEuler = Vector3.zero,
                localScaleMultiplier = Vector3.one,
                fitToPlaceholderBounds = true,
                preservePlaceholder = true,
                setStaticFlags = true
            };
            mappings.Add(mapping);
            existingKeys.Add(key);
            added++;
        }

        return added;
    }

    private static int AutoAssignMappingsFromFolder(HolstinArtPack pack, string folderPath, bool onlyMissingPrefab)
    {
        if (pack == null || pack.EditorMappings == null || pack.EditorMappings.Count == 0)
        {
            return 0;
        }

        string[] searchFolders = { string.IsNullOrWhiteSpace(folderPath) ? "Assets" : folderPath };
        string[] prefabGuids = AssetDatabase.FindAssets("t:prefab", searchFolders);
        if (prefabGuids == null || prefabGuids.Length == 0)
        {
            return 0;
        }

        List<PrefabCandidate> candidates = new List<PrefabCandidate>(prefabGuids.Length);
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                continue;
            }

            candidates.Add(new PrefabCandidate
            {
                prefab = prefab,
                normalizedName = NormalizeForMatch(prefab.name)
            });
        }

        int assigned = 0;
        List<HolstinArtPack.PlaceholderMapping> mappings = pack.EditorMappings;
        for (int i = 0; i < mappings.Count; i++)
        {
            HolstinArtPack.PlaceholderMapping mapping = mappings[i];
            if (mapping == null)
            {
                continue;
            }

            if (onlyMissingPrefab && mapping.prefab != null)
            {
                continue;
            }

            List<string> searchTerms = BuildMappingSearchTerms(mapping);
            if (searchTerms.Count == 0)
            {
                continue;
            }

            GameObject bestPrefab = null;
            int bestScore = int.MinValue;
            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                PrefabCandidate candidate = candidates[candidateIndex];
                if (string.IsNullOrEmpty(candidate.normalizedName))
                {
                    continue;
                }

                for (int termIndex = 0; termIndex < searchTerms.Count; termIndex++)
                {
                    int score = ScoreCandidate(candidate.normalizedName, searchTerms[termIndex]);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPrefab = candidate.prefab;
                    }
                }
            }

            if (bestPrefab == null || bestScore < 0)
            {
                continue;
            }

            mapping.prefab = bestPrefab;
            assigned++;
        }

        return assigned;
    }

    private static int RecoverMappingsFromCurrentScenePlacements(HolstinArtPack pack)
    {
        if (pack == null)
        {
            return 0;
        }

        List<HolstinArtPack.PlaceholderMapping> mappings = pack.EditorMappings;
        if (mappings == null || mappings.Count == 0)
        {
            return 0;
        }

        AssetPlaceholder[] placeholders = Object.FindObjectsByType<AssetPlaceholder>(FindObjectsInactive.Include);
        if (placeholders == null || placeholders.Length == 0)
        {
            return 0;
        }

        List<SceneVisualCandidate> candidates = BuildSceneVisualCandidates();
        if (candidates.Count == 0)
        {
            Debug.LogWarning("No scene visuals were found for mapping recovery.");
            return 0;
        }

        Dictionary<string, HolstinArtPack.PlaceholderMapping> mappingByKey = new Dictionary<string, HolstinArtPack.PlaceholderMapping>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < mappings.Count; i++)
        {
            HolstinArtPack.PlaceholderMapping mapping = mappings[i];
            if (mapping == null)
            {
                continue;
            }

            string key = BuildKey(mapping.category, mapping.assetTag);
            if (!mappingByKey.ContainsKey(key))
            {
                mappingByKey.Add(key, mapping);
            }
        }

        int recovered = 0;
        HashSet<string> recoveredKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < placeholders.Length; i++)
        {
            AssetPlaceholder placeholder = placeholders[i];
            if (placeholder == null)
            {
                continue;
            }

            string key = BuildKey(placeholder.Category, placeholder.AssetTag);
            if (recoveredKeys.Contains(key))
            {
                continue;
            }

            if (!mappingByKey.TryGetValue(key, out HolstinArtPack.PlaceholderMapping mapping) || mapping == null)
            {
                continue;
            }

            if (!TryFindBestSceneCandidateForPlaceholder(placeholder, candidates, out SceneVisualCandidate candidate))
            {
                continue;
            }

            GameObject prefab = ResolveOrCreateRecoveredPrefabAsset(candidate.root, placeholder);
            if (prefab == null)
            {
                continue;
            }

            ApplyRecoveredPlacementData(mapping, placeholder, candidate.root.transform, prefab.transform);
            recovered++;
            recoveredKeys.Add(key);
        }

        return recovered;
    }

    private static List<SceneVisualCandidate> BuildSceneVisualCandidates()
    {
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include);
        List<SceneVisualCandidate> candidates = new List<SceneVisualCandidate>(renderers.Length);
        HashSet<GameObject> seenRoots = new HashSet<GameObject>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            GameObject root = ResolveSceneVisualRoot(rendererComponent.transform);
            if (root == null)
            {
                continue;
            }

            if (seenRoots.Contains(root))
            {
                continue;
            }

            if (!TryComputeRendererBounds(root.transform, out Bounds bounds))
            {
                continue;
            }

            candidates.Add(new SceneVisualCandidate
            {
                root = root,
                bounds = bounds,
                normalizedName = NormalizeForMatch(root.name),
                sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(root)
            });

            seenRoots.Add(root);
        }

        return candidates;
    }

    private static GameObject ResolveSceneVisualRoot(Transform source)
    {
        if (source == null)
        {
            return null;
        }

        Transform root = source;
        GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(source.gameObject);
        if (prefabRoot != null)
        {
            root = prefabRoot.transform;
        }
        else
        {
            while (root.parent != null)
            {
                if (root.parent.GetComponent<AssetPlaceholder>() != null)
                {
                    break;
                }

                root = root.parent;
            }
        }

        GameObject rootObject = root.gameObject;
        if (!rootObject.scene.IsValid())
        {
            return null;
        }

        if (rootObject.GetComponent<AssetPlaceholder>() != null)
        {
            return null;
        }

        if (rootObject.GetComponent<HolstinArtInstanceLink>() != null)
        {
            return null;
        }

        if (IsActorVisualHierarchy(rootObject.transform))
        {
            return null;
        }

        string normalizedName = NormalizeForMatch(rootObject.name);
        if (normalizedName.IndexOf("storemodelvisual", StringComparison.Ordinal) >= 0 ||
            normalizedName.IndexOf("placeholder", StringComparison.Ordinal) >= 0)
        {
            return null;
        }

        return rootObject;
    }

    private static bool IsActorVisualHierarchy(Transform candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        return candidate.GetComponentInParent<PlayerMover>() != null ||
               candidate.GetComponentInParent<EnemyController>() != null ||
               candidate.GetComponentInParent<NPCKeyGiverInteractable>() != null ||
               candidate.GetComponentInParent<SkillCheckNpcInteractable>() != null ||
               candidate.GetComponentInParent<ShopInteractable>() != null ||
               candidate.GetComponentInParent<ProceduralHumanoidRig>() != null ||
               candidate.GetComponentInParent<ActiveRagdollMotor>() != null ||
               candidate.GetComponentInParent<DeathRagdollController>() != null ||
               candidate.GetComponentInParent<PlayerAnimationController>() != null ||
               candidate.GetComponentInParent<CharacterController>() != null ||
               candidate.GetComponentInParent<Animator>() != null ||
               candidate.GetComponentInParent<Damageable>() != null ||
               candidate.GetComponentInParent<CharacterStats>() != null;
    }

    private static bool TryFindBestSceneCandidateForPlaceholder(
        AssetPlaceholder placeholder,
        List<SceneVisualCandidate> candidates,
        out SceneVisualCandidate bestCandidate)
    {
        bestCandidate = default;
        if (placeholder == null || candidates == null || candidates.Count == 0)
        {
            return false;
        }

        float maxDistance = Mathf.Max(2.5f, placeholder.BoundsSize.magnitude * 2.25f);
        float placeholderSizeMagnitude = Mathf.Max(0.1f, placeholder.BoundsSize.magnitude);
        string normalizedTag = NormalizeForMatch(placeholder.AssetTag);
        string normalizedName = NormalizeForMatch(placeholder.gameObject.name);
        bool found = false;
        int bestScore = int.MinValue;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            SceneVisualCandidate candidate = candidates[i];
            if (candidate.root == null)
            {
                continue;
            }

            float candidateSizeMagnitude = candidate.bounds.size.magnitude;
            if (candidateSizeMagnitude > placeholderSizeMagnitude * 6f)
            {
                continue;
            }

            float distance = Vector3.Distance(placeholder.transform.position, candidate.bounds.center);
            if (distance > maxDistance)
            {
                continue;
            }

            int score = Mathf.RoundToInt((maxDistance - distance) * 100f);

            if (!string.IsNullOrWhiteSpace(normalizedTag))
            {
                if (string.Equals(candidate.normalizedName, normalizedTag, StringComparison.Ordinal))
                {
                    score += 800;
                }
                else if (candidate.normalizedName.IndexOf(normalizedTag, StringComparison.Ordinal) >= 0)
                {
                    score += 420;
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedName) &&
                candidate.normalizedName.IndexOf(normalizedName, StringComparison.Ordinal) >= 0)
            {
                score += 220;
            }

            if (candidate.sourcePrefab != null)
            {
                score += 120;
            }

            if (!found || score > bestScore || (score == bestScore && distance < bestDistance))
            {
                found = true;
                bestScore = score;
                bestDistance = distance;
                bestCandidate = candidate;
            }
        }

        return found;
    }

    private static GameObject ResolveOrCreateRecoveredPrefabAsset(GameObject sceneRoot, AssetPlaceholder placeholder)
    {
        if (sceneRoot == null || placeholder == null)
        {
            return null;
        }

        if (IsActorVisualHierarchy(sceneRoot.transform))
        {
            Debug.LogWarning($"Skipping recovery candidate '{sceneRoot.name}' because it belongs to a character hierarchy.");
            return null;
        }

        GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(sceneRoot);
        if (sourcePrefab != null)
        {
            return sourcePrefab;
        }

        EnsureArtImportFolders();
        EnsureFolder(RecoveredPrefabsFolder);

        string keyName = $"{placeholder.Category}_{NormalizeTag(placeholder.AssetTag)}";
        string safeName = SanitizeForAssetName(keyName);
        string prefabPath = $"{RecoveredPrefabsFolder}/{safeName}.prefab";

        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            return existingPrefab;
        }

        bool success;
        GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(sceneRoot, prefabPath, out success);
        if (success && createdPrefab != null)
        {
            return createdPrefab;
        }

        Debug.LogWarning($"Failed to create recovered prefab from '{sceneRoot.name}' at '{prefabPath}'.");
        return null;
    }

    private static void ApplyRecoveredPlacementData(
        HolstinArtPack.PlaceholderMapping mapping,
        AssetPlaceholder placeholder,
        Transform sceneTransform,
        Transform prefabTransform)
    {
        if (mapping == null || placeholder == null || sceneTransform == null || prefabTransform == null)
        {
            return;
        }

        mapping.prefab = prefabTransform.gameObject;
        mapping.fitToPlaceholderBounds = false;
        mapping.preservePlaceholder = true;
        mapping.setStaticFlags = true;

        Transform placeholderTransform = placeholder.transform;
        Vector3 worldOffset = sceneTransform.position - placeholderTransform.position;
        mapping.localPositionOffset = placeholderTransform.InverseTransformVector(worldOffset);

        Quaternion relativeRotation = Quaternion.Inverse(placeholderTransform.rotation) * sceneTransform.rotation;
        mapping.localRotationEuler = NormalizeEuler(relativeRotation.eulerAngles);

        Vector3 prefabScale = prefabTransform.localScale;
        Vector3 sceneScale = sceneTransform.localScale;
        mapping.localScaleMultiplier = new Vector3(
            SafeDivide(sceneScale.x, prefabScale.x),
            SafeDivide(sceneScale.y, prefabScale.y),
            SafeDivide(sceneScale.z, prefabScale.z));
    }

    private static Vector3 NormalizeEuler(Vector3 euler)
    {
        return new Vector3(NormalizeAngle(euler.x), NormalizeAngle(euler.y), NormalizeAngle(euler.z));
    }

    private static float NormalizeAngle(float angle)
    {
        float normalized = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return Mathf.Abs(normalized) < 0.0001f ? 0f : normalized;
    }

    private static float SafeDivide(float numerator, float denominator)
    {
        return Mathf.Abs(denominator) <= 0.0001f ? 1f : numerator / denominator;
    }

    private static string SanitizeForAssetName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "RecoveredModel";
        }

        string trimmed = value.Trim();
        char[] chars = trimmed.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                chars[i] = '_';
            }
        }

        string result = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(result) ? "RecoveredModel" : result;
    }

    private static int ScoreCandidate(string normalizedCandidate, string normalizedTerm)
    {
        if (string.IsNullOrWhiteSpace(normalizedCandidate) || string.IsNullOrWhiteSpace(normalizedTerm))
        {
            return -1;
        }

        if (string.Equals(normalizedCandidate, normalizedTerm, StringComparison.Ordinal))
        {
            return 2000 + normalizedTerm.Length;
        }

        if (normalizedCandidate.IndexOf(normalizedTerm, StringComparison.Ordinal) >= 0)
        {
            return 1000 + normalizedTerm.Length;
        }

        if (normalizedTerm.IndexOf(normalizedCandidate, StringComparison.Ordinal) >= 0)
        {
            return 600 + normalizedCandidate.Length;
        }

        return -1;
    }

    private static List<string> BuildMappingSearchTerms(HolstinArtPack.PlaceholderMapping mapping)
    {
        List<string> terms = new List<string>(8);
        AddSearchTerm(terms, mapping.assetTag);
        AddSearchTerm(terms, mapping.id);
        AddSearchTerm(terms, mapping.category.ToString());

        if (!string.IsNullOrWhiteSpace(mapping.id))
        {
            string[] idTokens = mapping.id.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < idTokens.Length; i++)
            {
                if (idTokens[i].Length >= 3)
                {
                    AddSearchTerm(terms, idTokens[i]);
                }
            }
        }

        return terms;
    }

    private static void AddSearchTerm(List<string> terms, string rawValue)
    {
        string normalized = NormalizeForMatch(rawValue);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        for (int i = 0; i < terms.Count; i++)
        {
            if (string.Equals(terms[i], normalized, StringComparison.Ordinal))
            {
                return;
            }
        }

        terms.Add(normalized);
    }

    private static string NormalizeForMatch(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string lower = value.Trim().ToLowerInvariant();
        char[] buffer = new char[lower.Length];
        int length = 0;
        for (int i = 0; i < lower.Length; i++)
        {
            char c = lower[i];
            if (char.IsLetterOrDigit(c))
            {
                buffer[length++] = c;
            }
        }

        return length == 0 ? string.Empty : new string(buffer, 0, length);
    }

    private static string ResolveSelectedProjectFolder()
    {
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            return "Assets";
        }

        string path = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrWhiteSpace(path))
        {
            return "Assets";
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            path = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        }

        return string.IsNullOrWhiteSpace(path) || !AssetDatabase.IsValidFolder(path)
            ? "Assets"
            : path;
    }

    private struct PrefabCandidate
    {
        public string normalizedName;
        public GameObject prefab;
    }

    private struct SceneVisualCandidate
    {
        public GameObject root;
        public Bounds bounds;
        public string normalizedName;
        public GameObject sourcePrefab;
    }

    private static string BuildKey(AssetPlaceholder.PlaceholderCategory category, string assetTag)
    {
        return category + "::" + (assetTag ?? string.Empty).Trim();
    }

    private static string NormalizeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return "untagged";
        }

        string value = tag.Trim().ToLowerInvariant();
        value = value.Replace(" ", "_").Replace("-", "_");
        return value;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
