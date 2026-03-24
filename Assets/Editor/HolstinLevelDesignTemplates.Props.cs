
// ============================================================
// FILE: HolstinLevelDesignTemplates.Props.cs
// Furniture, props, structural elements
// ============================================================
using UnityEditor;
using UnityEngine;

public static partial class HolstinLevelDesignTemplates
{
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

    private static void CreateTable(Transform parent, Vector3 pos, Vector3 topScale, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cube, "Top", pos + new Vector3(0f, 0.82f, 0f), topScale, root.transform, woodMaterial);
        float x = topScale.x * 0.4f, z = topScale.z * 0.4f;
        CreatePrimitive(PrimitiveType.Cube, "Leg_A", pos + new Vector3(-x, 0.4f, -z), new Vector3(0.15f, 0.8f, 0.15f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Leg_B", pos + new Vector3( x, 0.4f, -z), new Vector3(0.15f, 0.8f, 0.15f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Leg_C", pos + new Vector3(-x, 0.4f,  z), new Vector3(0.15f, 0.8f, 0.15f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Leg_D", pos + new Vector3( x, 0.4f,  z), new Vector3(0.15f, 0.8f, 0.15f), root.transform, darkMaterial);
    }

    private static void CreateDesk(Transform parent, Vector3 pos, string name)
    {
        CreateTable(parent, pos, new Vector3(1.6f, 0.14f, 0.75f), name);
        CreatePrimitive(PrimitiveType.Cube, name + "Drawer", pos + new Vector3(0.45f, 0.58f, 0f), new Vector3(0.45f, 0.45f, 0.62f), parent, woodMaterial);
    }

    private static void CreateBench(Transform parent, Vector3 pos, float length, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cube, "Seat",  pos + new Vector3(0f, 0.45f, 0f), new Vector3(length, 0.12f, 0.45f), root.transform, woodMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Leg_A", pos + new Vector3(-length * 0.35f, 0.22f, 0f), new Vector3(0.14f, 0.44f, 0.14f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Leg_B", pos + new Vector3( length * 0.35f, 0.22f, 0f), new Vector3(0.14f, 0.44f, 0.14f), root.transform, darkMaterial);
    }

    private static void CreateShelf(Transform parent, Vector3 pos, int levels, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cube, "Side_L", pos + new Vector3(-0.55f, 1f, 0f), new Vector3(0.1f, 2f, 0.45f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Side_R", pos + new Vector3( 0.55f, 1f, 0f), new Vector3(0.1f, 2f, 0.45f), root.transform, darkMaterial);
        for (int i = 0; i < levels; i++)
            CreatePrimitive(PrimitiveType.Cube, $"Shelf_{i}", pos + new Vector3(0f, 0.25f + i * 0.75f, 0f), new Vector3(1.2f, 0.08f, 0.45f), root.transform, woodMaterial);
    }

    private static void CreateBed(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cube, "Frame",     pos + new Vector3(0f, 0.25f, 0f),   new Vector3(1.2f, 0.28f, 2.3f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Mattress",  pos + new Vector3(0f, 0.48f, 0f),   new Vector3(1.05f, 0.18f, 2f),  root.transform, wallMaterial);
        CreatePrimitive(PrimitiveType.Cube, "Headboard", pos + new Vector3(0f, 0.82f, 1.05f),new Vector3(1.2f, 0.8f,  0.1f), root.transform, woodMaterial);
    }

    private static void CreateBarrel(Transform parent, Vector3 pos, string name)
    {
        GameObject b = CreatePrimitive(PrimitiveType.Cylinder, name, pos + new Vector3(0f, 0.45f, 0f), new Vector3(0.48f, 0.45f, 0.48f), parent, woodMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, name + "BandTop",    pos + new Vector3(0f, 0.72f, 0f), new Vector3(0.5f, 0.02f, 0.5f), parent, metalMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, name + "BandBottom", pos + new Vector3(0f, 0.20f, 0f), new Vector3(0.5f, 0.02f, 0.5f), parent, metalMaterial);
        b.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
    }

    private static void CreateCrateCluster(Transform parent, Vector3 center, int count, string rootName)
    {
        GameObject root = new GameObject(rootName);
        root.transform.SetParent(parent);
        for (int i = 0; i < count; i++)
        {
            float x = (i % 3) * 0.75f, z = (i / 3) * 0.75f, y = i >= 3 ? 0.58f : 0.28f;
            CreatePrimitive(PrimitiveType.Cube, $"Crate_{i}", center + new Vector3(x, y, z), new Vector3(0.62f, 0.56f, 0.62f), root.transform, woodMaterial);
        }
    }

    private static void CreateLantern(Transform parent, Vector3 pos, string name, Color lightColor)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = pos;
        CreatePrimitive(PrimitiveType.Cylinder, "Frame", pos, new Vector3(0.12f, 0.2f, 0.12f), root.transform, metalMaterial, false);
        GameObject globe = CreatePrimitive(PrimitiveType.Sphere, "Glow", pos + new Vector3(0f, -0.08f, 0f), new Vector3(0.18f, 0.18f, 0.18f), root.transform, accentMaterial, false);
        Renderer gr = globe.GetComponent<Renderer>();
        if (gr != null) gr.sharedMaterial = GetEmissiveMaterial(name + "_Emissive", lightColor * 0.8f);
        Light lc = root.AddComponent<Light>();
        lc.type = LightType.Point; lc.range = 8f; lc.intensity = 5f; lc.color = lightColor; lc.shadows = LightShadows.None;

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
        CreatePrimitive(PrimitiveType.Cylinder, "Pole",     pos + new Vector3(0f,    1.5f, 0f), new Vector3(0.12f, 1.5f, 0.12f), root.transform, metalMaterial);
        CreatePrimitive(PrimitiveType.Cube,     "Crossbar", pos + new Vector3(0.45f, 3.1f, 0f), new Vector3(0.9f,  0.08f, 0.08f), root.transform, metalMaterial);
        CreateLantern(root.transform, pos + new Vector3(0.85f, 2.95f, 0f), "LampHead", new Color(1f, 0.72f, 0.42f));
    }

    private static void CreateWell(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cylinder, "StoneRing",  pos + new Vector3(0f, 0.40f, 0f), new Vector3(1.4f, 0.4f,  1.4f), root.transform, floorMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, "InnerWater", pos + new Vector3(0f, 0.15f, 0f), new Vector3(0.9f, 0.1f,  0.9f), root.transform, accentMaterial);
        CreatePrimitive(PrimitiveType.Cube,     "Beam_A",     pos + new Vector3(-0.95f, 1.5f, 0f), new Vector3(0.16f, 2.2f, 0.16f), root.transform, woodMaterial);
        CreatePrimitive(PrimitiveType.Cube,     "Beam_B",     pos + new Vector3( 0.95f, 1.5f, 0f), new Vector3(0.16f, 2.2f, 0.16f), root.transform, woodMaterial);
        CreatePrimitive(PrimitiveType.Cube,     "Crossbar",   pos + new Vector3(0f, 2.45f, 0f),    new Vector3(2.1f,  0.16f, 0.16f), root.transform, woodMaterial);
    }

    private static void CreateCart(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        CreatePrimitive(PrimitiveType.Cube,     "Body",    pos + new Vector3(0f,   0.70f, 0f),  new Vector3(2.2f,  0.7f,  1.3f), root.transform, woodMaterial);
        CreatePrimitive(PrimitiveType.Cube,     "Handle",  pos + new Vector3(1.8f, 0.75f, 0f),  new Vector3(1.4f,  0.1f,  0.1f), root.transform, woodMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, "Wheel_A", pos + new Vector3(-0.8f, 0.35f,  0.8f), new Vector3(0.38f, 0.12f, 0.38f), root.transform, darkMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, "Wheel_B", pos + new Vector3(-0.8f, 0.35f, -0.8f), new Vector3(0.38f, 0.12f, 0.38f), root.transform, darkMaterial);
    }

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

    private static void CreateColumn(Transform parent, Vector3 pos, string name)
        => CreatePrimitive(PrimitiveType.Cylinder, name, pos + new Vector3(0f, 1.5f, 0f), new Vector3(0.4f, 1.5f, 0.4f), parent, wallMaterial);

    private static void CreateWaterChannel(Transform parent, Vector3 pos, Vector3 scale, string name)
    {
        GameObject water = CreatePrimitive(PrimitiveType.Cube, name, pos, scale, parent, accentMaterial);
        Collider c = water.GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private static void CreatePortal(Transform parent, Vector3 pos, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = pos;
        CreatePrimitive(PrimitiveType.Cylinder, "Frame", pos, new Vector3(1.6f, 0.2f, 1.6f), root.transform, darkMaterial);
        GameObject p = CreatePrimitive(PrimitiveType.Sphere, "Core", pos, new Vector3(1.7f, 2.1f, 0.35f), root.transform, accentMaterial, false);
        p.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Renderer pr = p.GetComponent<Renderer>();
        if (pr != null) pr.sharedMaterial = GetEmissiveMaterial(name + "_Portal", new Color(0.45f, 0.7f, 1f));
        Light lc = root.AddComponent<Light>();
        lc.type = LightType.Point; lc.range = 10f; lc.intensity = 6f; lc.color = new Color(0.45f, 0.7f, 1f); lc.shadows = LightShadows.None;

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
