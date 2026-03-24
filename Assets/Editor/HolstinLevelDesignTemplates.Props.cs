
// ============================================================
// FILE: HolstinLevelDesignTemplates.Props.cs
// Structural elements remain primitives (beams, stairs, roofs).
// Furniture and decorative props are now AssetPlaceholder markers
// ready for store-asset replacement.
// ============================================================
using UnityEditor;
using UnityEngine;

public static partial class HolstinLevelDesignTemplates
{
    // ----- Structural (keep as primitives -- these define layout) -----

    private static void CreateBeamLine(Transform parent, Vector3 start, int count, float spacing)
    {
        for (int i = 0; i < count; i++)
            CreatePrimitive(PrimitiveType.Cube, $"Beam_{i}", start + new Vector3(i * spacing, 1.5f, 0f), new Vector3(0.28f, 3f, 0.28f), parent, darkMaterial);
    }

    private static void CreateRoofCaps(Transform parent, Vector3 center)
    {
        GameObject l = CreatePrimitive(PrimitiveType.Cube, "RoofLeft",  center + new Vector3(-2.6f, 0f, 0f), new Vector3(6.8f, 0.35f, 8f), parent, roofMaterial);
        GameObject r = CreatePrimitive(PrimitiveType.Cube, "RoofRight", center + new Vector3( 2.6f, 0f, 0f), new Vector3(6.8f, 0.35f, 8f), parent, roofMaterial);
        l.transform.rotation = Quaternion.Euler(0f, 0f,  28f);
        r.transform.rotation = Quaternion.Euler(0f, 0f, -28f);
    }

    private static void CreateConnectorStair(Vector3 start, int steps, float run, float rise, float width, float treadH, float treadD, Quaternion rot, string rootName, Transform parent = null)
    {
        GameObject root = new GameObject(rootName);
        root.transform.SetParent(parent);
        root.transform.position = start;
        root.transform.rotation = rot;
        for (int i = 0; i < steps; i++)
        {
            Vector3 lp = new Vector3(0f, i * rise + treadH * 0.5f, i * run);
            CreatePrimitive(PrimitiveType.Cube, $"Step_{i}", root.transform.TransformPoint(lp), new Vector3(width, treadH, treadD), root.transform, woodMaterial);
        }
    }

    private static void CreateColumn(Transform parent, Vector3 pos, string name)
        => CreatePrimitive(PrimitiveType.Cylinder, name, pos + new Vector3(0f, 1.5f, 0f), new Vector3(0.4f, 1.5f, 0.4f), parent, wallMaterial);

    private static void CreateWaterChannel(Transform parent, Vector3 pos, Vector3 scale, string name)
    {
        GameObject water = CreatePrimitive(PrimitiveType.Cube, name, pos, scale, parent, accentMaterial);
        Collider c = water.GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    // ----- Furniture (AssetPlaceholder -- swap with store models) -----

    private static void CreateTable(Transform parent, Vector3 pos, Vector3 topScale, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Furniture, "table",
            new Vector3(topScale.x, 0.85f, topScale.z));
    }

    private static void CreateDesk(Transform parent, Vector3 pos, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Furniture, "desk",
            new Vector3(1.6f, 0.85f, 0.75f));
    }

    private static void CreateBench(Transform parent, Vector3 pos, float length, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Furniture, "bench",
            new Vector3(length, 0.5f, 0.45f));
    }

    private static void CreateShelf(Transform parent, Vector3 pos, int levels, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Furniture, "shelf",
            new Vector3(1.2f, 0.25f + levels * 0.75f, 0.45f));
    }

    private static void CreateBed(Transform parent, Vector3 pos, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Furniture, "bed",
            new Vector3(1.2f, 0.65f, 2.3f));
    }

    private static void CreateBarrel(Transform parent, Vector3 pos, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Container, "barrel",
            new Vector3(0.5f, 0.9f, 0.5f));
    }

    private static void CreateCrateCluster(Transform parent, Vector3 center, int count, string rootName)
    {
        CreateAssetPlaceholder(parent, center, rootName,
            AssetPlaceholder.PlaceholderCategory.Container, "crate_cluster",
            new Vector3(count * 0.5f, 0.6f, Mathf.CeilToInt(count / 3f) * 0.5f));
    }

    private static void CreateWell(Transform parent, Vector3 pos, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Prop, "well",
            new Vector3(1.4f, 2.6f, 1.4f));
    }

    private static void CreateCart(Transform parent, Vector3 pos, string name)
    {
        CreateAssetPlaceholder(parent, pos, name,
            AssetPlaceholder.PlaceholderCategory.Prop, "cart",
            new Vector3(2.2f, 1.2f, 1.3f));
    }

    // ----- Lighting props (keep functional -- lights are gameplay) -----

    private static void CreateLantern(Transform parent, Vector3 pos, string name, Color lightColor)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = pos;

        // Placeholder for visual mesh
        AssetPlaceholder ph = root.AddComponent<AssetPlaceholder>();
        ph.Configure(AssetPlaceholder.PlaceholderCategory.Light, "lantern", new Vector3(0.2f, 0.4f, 0.2f));

        // Functional light (this stays regardless of model)
        Light lc = root.AddComponent<Light>();
        lc.type = LightType.Point;
        lc.range = 8f;
        lc.intensity = 5f;
        lc.color = lightColor;
        lc.shadows = LightShadows.None;

        TutorialAmbientMotion motion = root.AddComponent<TutorialAmbientMotion>();
        SerializedObject so = new SerializedObject(motion);
        so.FindProperty("bobAmplitude").vector3Value = new Vector3(0f, 0.025f, 0f);
        so.FindProperty("bobSpeed").floatValue = 1.1f;
        so.FindProperty("yawSpeed").floatValue = 7f;
        so.FindProperty("targetLight").objectReferenceValue = lc;
        so.FindProperty("pulseAmplitude").floatValue = 0.28f;
        so.FindProperty("pulseSpeed").floatValue = 1.55f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateLampPost(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = pos;

        AssetPlaceholder ph = root.AddComponent<AssetPlaceholder>();
        ph.Configure(AssetPlaceholder.PlaceholderCategory.Light, "lamp_post", new Vector3(0.4f, 3.2f, 0.4f));

        // Functional light at top
        GameObject lightHolder = new GameObject("LampHead");
        lightHolder.transform.SetParent(root.transform);
        lightHolder.transform.position = pos + new Vector3(0f, 3f, 0f);
        Light lc = lightHolder.AddComponent<Light>();
        lc.type = LightType.Point;
        lc.range = 8f;
        lc.intensity = 5f;
        lc.color = new Color(1f, 0.72f, 0.42f);
        lc.shadows = LightShadows.None;
    }

    // ----- Structural fencing/gates (layout elements, keep as primitives) -----

    private static void CreateFenceLine(Transform parent, Vector3 start, int count, float spacing, Quaternion rot, string rootName)
    {
        GameObject root = new GameObject(rootName);
        root.transform.SetParent(parent);
        for (int i = 0; i < count; i++)
        {
            GameObject post = CreatePrimitive(PrimitiveType.Cube, $"FencePost_{i}", start + rot * new Vector3(i * spacing, 0.9f, 0f), new Vector3(0.14f, 1.8f, 0.14f), root.transform, woodMaterial);
            post.transform.rotation = rot;
        }
        for (int i = 0; i < count - 1; i++)
        {
            GameObject rail = CreatePrimitive(PrimitiveType.Cube, $"FenceRail_{i}", start + rot * new Vector3((i + 0.5f) * spacing, 1.2f, 0f), new Vector3(spacing, 0.12f, 0.12f), root.transform, darkMaterial);
            rail.transform.rotation = rot;
        }
    }

    private static void CreateGate(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cube, "Pillar_A", pos + new Vector3(0f, 2.2f, -1.2f), new Vector3(0.45f, 4.4f, 0.45f), root.transform, wallMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Pillar_B", pos + new Vector3(0f, 2.2f,  1.2f), new Vector3(0.45f, 4.4f, 0.45f), root.transform, wallMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Lintel",   pos + new Vector3(0f, 4.3f,  0f),   new Vector3(0.5f,  0.35f, 2.9f), root.transform, wallMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Door_A",   pos + new Vector3(0f, 1.3f, -0.6f), new Vector3(0.14f, 2.6f, 1.1f),  root.transform, woodMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Door_B",   pos + new Vector3(0f, 1.3f,  0.6f), new Vector3(0.14f, 2.6f, 1.1f),  root.transform, woodMaterial);
    }

    private static void CreatePortal(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = pos;

        AssetPlaceholder ph = root.AddComponent<AssetPlaceholder>();
        ph.Configure(AssetPlaceholder.PlaceholderCategory.Prop, "portal", new Vector3(1.7f, 2.2f, 0.4f));

        Light lc = root.AddComponent<Light>();
        lc.type = LightType.Point;
        lc.range = 10f;
        lc.intensity = 6f;
        lc.color = new Color(0.45f, 0.7f, 1f);
        lc.shadows = LightShadows.None;

        TutorialAmbientMotion motion = root.AddComponent<TutorialAmbientMotion>();
        SerializedObject so = new SerializedObject(motion);
        so.FindProperty("bobAmplitude").vector3Value = new Vector3(0f, 0.04f, 0f);
        so.FindProperty("bobSpeed").floatValue = 0.95f;
        so.FindProperty("yawSpeed").floatValue = 11f;
        so.FindProperty("targetLight").objectReferenceValue = lc;
        so.FindProperty("pulseAmplitude").floatValue = 0.45f;
        so.FindProperty("pulseSpeed").floatValue = 1.3f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
