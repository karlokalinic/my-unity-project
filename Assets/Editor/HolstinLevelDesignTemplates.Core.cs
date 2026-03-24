// ============================================================
// FILE: HolstinLevelDesignTemplates.Core.cs
// Shared fields, materials, scene root groups, primitive builder
// ============================================================

using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static partial class HolstinLevelDesignTemplates
{
    // --- Material cache ---
    private static Material wallMaterial;
    private static Material floorMaterial;
    private static Material woodMaterial;
    private static Material darkMaterial;
    private static Material roofMaterial;
    private static Material metalMaterial;
    private static Material groundMaterial;
    private static Material accentMaterial;
    private static Material npcMaterial;
    private static Material enemyMaterial;

    // --- Scene root groups ---
    private static Transform sceneCoreRoot;
    private static Transform sceneWorldRoot;
    private static Transform sceneGameplayRoot;
    private static Transform sceneLightingRoot;
    private static Transform sceneDebugRoot;
    private static Transform sceneUiRoot;

    // ----------------------------------------------------------------
    // Scene utilities
    // ----------------------------------------------------------------

    private static void CleanupDefaultScene()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            Object.DestroyImmediate(root);
        sceneCoreRoot = sceneWorldRoot = sceneGameplayRoot = sceneLightingRoot = sceneDebugRoot = sceneUiRoot = null;
    }

    private static void EnsureSceneRootGroups()
    {
        sceneCoreRoot     = EnsureRootGroup("_Core");
        sceneWorldRoot    = EnsureRootGroup("_World");
        sceneGameplayRoot = EnsureRootGroup("_Gameplay");
        sceneLightingRoot = EnsureRootGroup("_Lighting");
        sceneDebugRoot    = EnsureRootGroup("_Debug");
        sceneUiRoot       = EnsureChildGroup(sceneCoreRoot, "UI");

        ReparentIfFound("Player",               sceneCoreRoot);
        ReparentIfFound("HolstinCameraRig",     sceneCoreRoot);
        ReparentIfFound("NavMeshSurfaceRoot",   sceneCoreRoot);
        ReparentIfFound("EventSystem",          sceneCoreRoot);
        ReparentIfFound("HolstinSceneContext",  sceneCoreRoot);
        ReparentIfFound("InteractionPromptUI",  sceneUiRoot);
        ReparentIfFound("DialoguePanelUI",      sceneUiRoot);

        ReparentIfFound("SceneGround",                       sceneWorldRoot);
        ReparentIfFound("Template_Exterior_FogCourtyard",    sceneWorldRoot);
        ReparentIfFound("Template_Interior_BoardingHouse",   sceneWorldRoot);
        ReparentIfFound("Template_Underpass_Catacombs",      sceneWorldRoot);
        ReparentIfFound("HouseToUnderpassSteps",             sceneWorldRoot);
        ReparentIfFound("Directional Light",                 sceneLightingRoot);
    }

    private static Transform EnsureRootGroup(string name)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == name) return root.transform;
        return new GameObject(name).transform;
    }

    private static Transform EnsureChildGroup(Transform parent, string name)
    {
        if (parent == null) return EnsureRootGroup(name);
        Transform child = parent.Find(name);
        if (child != null) return child;
        GameObject created = new GameObject(name);
        created.transform.SetParent(parent, false);
        return created.transform;
    }

    private static void ReparentIfFound(string objectName, Transform parent)
    {
        if (parent == null) return;
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform found = FindTransformRecursive(root.transform, objectName);
            if (found == null) continue;
            if (found.parent != parent) found.SetParent(parent, true);
            return;
        }
    }

    private static Transform FindTransformRecursive(Transform parent, string targetName)
    {
        if (parent == null) return null;
        if (parent.name == targetName) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindTransformRecursive(parent.GetChild(i), targetName);
            if (found != null) return found;
        }
        return null;
    }

    private static GameObject NewRoot(string name, Vector3 origin)
    {
        EnsureSceneRootGroups();
        GameObject root = new GameObject(name);
        if (sceneWorldRoot != null) root.transform.SetParent(sceneWorldRoot, false);
        root.transform.position = origin;
        return root;
    }

    private static void CreateSkyGround(Vector3 position, Vector3 scale, string name)
    {
        EnsureSceneRootGroups();
        GameObject ground = CreatePrimitive(PrimitiveType.Cube, name, position, scale, sceneWorldRoot, groundMaterial, true);
        ground.GetComponent<BoxCollider>().size = Vector3.one;
    }

    private static void EnsureDirectionalLight()
    {
        if (Object.FindAnyObjectByType<Light>() == null) CreateDirectionalLight();
    }

    private static void CreateDirectionalLight()
    {
        EnsureSceneRootGroups();
        Light[] lights = Object.FindObjectsByType<Light>();
        foreach (Light l in lights)
        {
            if (l != null && l.type == LightType.Directional)
            {
                if (sceneLightingRoot != null && l.transform.parent != sceneLightingRoot)
                    l.transform.SetParent(sceneLightingRoot, true);
                return;
            }
        }
        GameObject go = new GameObject("Directional Light");
        Light lc = go.AddComponent<Light>();
        lc.type      = LightType.Directional;
        lc.intensity = 1.1f;
        lc.color     = new Color(0.95f, 0.95f, 1f);
        go.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        if (sceneLightingRoot != null) go.transform.SetParent(sceneLightingRoot, true);
    }

    private static void EnsureVerticalSliceBootstrap()
    {
        EnsureSceneRootGroups();
        GameObject bootstrapRoot = sceneGameplayRoot != null
            ? EnsureChildGroup(sceneGameplayRoot, "VS_RuntimeBootstrapRoot").gameObject
            : new GameObject("VS_RuntimeBootstrapRoot");

        Type bootstrapType = Type.GetType("VerticalSliceScenaBootstrap, Assembly-CSharp");
        if (bootstrapType == null)
        {
            Debug.LogWarning("VerticalSliceScenaBootstrap type could not be resolved. Compile scripts once and retry.");
            return;
        }

        Component bootstrap = bootstrapRoot.GetComponent(bootstrapType) ?? bootstrapRoot.AddComponent(bootstrapType);
        SerializedObject so = new SerializedObject(bootstrap);
        SerializedProperty p = so.FindProperty("targetSceneName");
        if (p != null) p.stringValue = SceneManager.GetActiveScene().name;
        so.ApplyModifiedPropertiesWithoutUndo();
        bootstrapType.GetMethod("ApplyRetrofit")?.Invoke(bootstrap, null);
    }

    private static PlayerMover FindPlayer() => Object.FindAnyObjectByType<PlayerMover>();

    private static bool ConfirmCanCreateNewScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return true;
        }

        Debug.LogWarning("Scene creation cancelled because modified scenes were not saved.");
        return false;
    }

    private static void FinalizeScene(string message)
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(message);
    }

    // ----------------------------------------------------------------
    // Primitive builder
    // ----------------------------------------------------------------

    private static GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Transform parent, Material mat, bool isStatic = true)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        if (mat != null && r != null)
        {
            r.sharedMaterial = mat;
        }

        if (isStatic)
        {
            GameObjectUtility.SetStaticEditorFlags(go,
                StaticEditorFlags.BatchingStatic   |
                StaticEditorFlags.OccluderStatic   |
                StaticEditorFlags.OccludeeStatic   |
                StaticEditorFlags.ReflectionProbeStatic);
        }

        return go;
    }

    private static GameObject CreatePrimitive(
        PrimitiveType type,
        string name,
        Vector3 pos,
        Vector3 scale,
        Transform parent,
        Material mat,
        bool isStatic,
        bool includeCollider)
    {
        GameObject go = CreatePrimitive(type, name, pos, scale, parent, mat, isStatic);
        if (!includeCollider)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }
        }

        return go;
    }

    private static GameObject CreateDetailPrimitive(
        PrimitiveType type,
        string name,
        Vector3 pos,
        Vector3 scale,
        Transform parent,
        Material mat,
        bool castShadows = false)
    {
        GameObject go = CreatePrimitive(type, name, pos, scale, parent, mat, true, false);
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = castShadows;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        return go;
    }

    private static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        if (gameObject == null)
        {
            return null;
        }

        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    private static GameObject CreateFloor(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        => CreatePrimitive(PrimitiveType.Cube, name, pos, scale, parent, mat);

    private static GameObject CreateWall(Transform parent, Vector3 pos, Vector3 scale, Material mat, string name)
        => CreatePrimitive(PrimitiveType.Cube, name, pos, scale, parent, mat);

    // ----------------------------------------------------------------
    // Asset placeholder helper
    // ----------------------------------------------------------------

    /// <summary>
    /// Creates a lightweight placeholder GameObject with an AssetPlaceholder component.
    /// Use this for furniture, props, and decorations that will be replaced with store assets.
    /// A small cube gizmo shows the footprint in-editor.
    /// </summary>
    private static GameObject CreateAssetPlaceholder(
        Transform parent,
        Vector3 pos,
        string name,
        AssetPlaceholder.PlaceholderCategory category,
        string assetTag,
        Vector3 boundsSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;

        AssetPlaceholder placeholder = go.AddComponent<AssetPlaceholder>();
        placeholder.Configure(category, assetTag, boundsSize);

        // Tiny visual marker so it's visible in scene view
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "PlaceholderMarker";
        marker.transform.SetParent(go.transform, false);
        marker.transform.localPosition = Vector3.up * boundsSize.y * 0.5f;
        marker.transform.localScale = boundsSize * 0.15f;
        Renderer r = marker.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = accentMaterial;
        Collider c = marker.GetComponent<Collider>();
        if (c != null) Object.DestroyImmediate(c);

        // Add a box collider on the root matching the bounds for gameplay
        BoxCollider box = go.AddComponent<BoxCollider>();
        box.center = Vector3.up * boundsSize.y * 0.5f;
        box.size = boundsSize;

        GameObjectUtility.SetStaticEditorFlags(go,
            StaticEditorFlags.BatchingStatic |
            StaticEditorFlags.OccludeeStatic);

        return go;
    }

    // ----------------------------------------------------------------
    // Material helpers
    // ----------------------------------------------------------------

    private static void ResetMaterialCache()
    {
        wallMaterial   = GetMaterial(ref wallMaterial,   "Wall",   new Color(0.73f, 0.69f, 0.62f));
        floorMaterial  = GetMaterial(ref floorMaterial,  "Floor",  new Color(0.45f, 0.44f, 0.42f));
        woodMaterial   = GetMaterial(ref woodMaterial,   "Wood",   new Color(0.34f, 0.24f, 0.18f));
        darkMaterial   = GetMaterial(ref darkMaterial,   "Dark",   new Color(0.17f, 0.18f, 0.20f));
        roofMaterial   = GetMaterial(ref roofMaterial,   "Roof",   new Color(0.16f, 0.24f, 0.28f));
        metalMaterial  = GetMaterial(ref metalMaterial,  "Metal",  new Color(0.42f, 0.42f, 0.44f));
        groundMaterial = GetMaterial(ref groundMaterial, "Ground", new Color(0.31f, 0.28f, 0.24f));
        accentMaterial = GetMaterial(ref accentMaterial, "Accent", new Color(0.30f, 0.45f, 0.56f));
        npcMaterial    = GetMaterial(ref npcMaterial,    "NPC",    new Color(0.55f, 0.58f, 0.62f));
        enemyMaterial  = GetMaterial(ref enemyMaterial,  "Enemy",  new Color(0.40f, 0.24f, 0.24f));

        // Material pass keeps generated scenes readable while staying lightweight.
        TuneMaterial(wallMaterial,   new Color(0.73f, 0.69f, 0.62f), 0.14f, 0.04f);
        TuneMaterial(floorMaterial,  new Color(0.45f, 0.44f, 0.42f), 0.11f, 0.03f);
        TuneMaterial(woodMaterial,   new Color(0.34f, 0.24f, 0.18f), 0.24f, 0.02f);
        TuneMaterial(darkMaterial,   new Color(0.17f, 0.18f, 0.20f), 0.08f, 0.07f);
        TuneMaterial(roofMaterial,   new Color(0.16f, 0.24f, 0.28f), 0.18f, 0.06f);
        TuneMaterial(metalMaterial,  new Color(0.42f, 0.42f, 0.44f), 0.46f, 0.75f);
        TuneMaterial(groundMaterial, new Color(0.31f, 0.28f, 0.24f), 0.09f, 0.02f);
        TuneMaterial(accentMaterial, new Color(0.30f, 0.45f, 0.56f), 0.33f, 0.15f);
        TuneMaterial(npcMaterial,    new Color(0.55f, 0.58f, 0.62f), 0.22f, 0.03f);
        TuneMaterial(enemyMaterial,  new Color(0.40f, 0.24f, 0.24f), 0.19f, 0.04f);
    }

    private static Material GetMaterial(ref Material cache, string name, Color color)
    {
        if (cache != null) return cache;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Standard")
                     ?? Shader.Find("HDRP/Lit")
                     ?? Shader.Find("Sprites/Default");
        cache = new Material(shader) { name = "HolstinTemplate_" + name };
        SetMaterialColor(cache, color);
        SetMaterialSmoothness(cache, 0.18f);
        cache.enableInstancing = true;
        return cache;
    }

    private static Material GetEmissiveMaterial(string name, Color emission)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Standard")
                     ?? Shader.Find("Sprites/Default");
        Material mat = new Material(shader) { name = name };
        SetMaterialColor(mat, new Color(0.15f, 0.16f, 0.18f));
        SetMaterialMetallic(mat, 0.18f);
        SetMaterialSmoothness(mat, 0.68f);
        mat.enableInstancing = true;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
        }
        return mat;
    }

    private static void SetMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     color);
    }

    private static void SetMaterialSmoothness(Material mat, float value)
    {
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", value);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", value);
    }

    private static void SetMaterialMetallic(Material mat, float value)
    {
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", Mathf.Clamp01(value));
    }

    private static void TuneMaterial(Material mat, Color color, float smoothness, float metallic)
    {
        if (mat == null)
        {
            return;
        }

        SetMaterialColor(mat, color);
        SetMaterialSmoothness(mat, smoothness);
        SetMaterialMetallic(mat, metallic);
        mat.enableInstancing = true;
    }
}
