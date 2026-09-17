using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime compatibility layer for physical-world rules that were previously implemented
/// in the obsolete demo repository. It only upgrades known legacy generated objects and
/// leaves cinematic encounter doors/rooms untouched.
/// </summary>
public static class PhysicalWorldRuntimeInstaller
{
    private const float PlayerHeight = 1.8f;
    private const float PlayerRadius = 0.34f;
    private const float DoorWidth = 1.1f;
    private const float DoorHeight = 2.18f;
    private const float DoorThickness = 0.14f;

    private const float ChestWidth = 1.05f;
    private const float ChestDepth = 0.68f;
    private const float ChestHeight = 0.5f;
    private const float ChestBoard = 0.07f;
    private const float ChestLidThickness = 0.09f;

    private static Material wallMaterial;
    private static Material woodMaterial;
    private static Material contentsMaterial;
    private static AudioClip ambientClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallAfterSceneLoad()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !IsGameplayScene(scene.name))
        {
            return;
        }

        NormalizePlayerScale();
        UpgradeKnownLegacyDoors();
        EnsureHollowStorage();
        EnsureAmbientScore();
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return string.Equals(sceneName, "Scena", System.StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sceneName, "INTERAKCIJA", System.StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sceneName, "VerticalSlice_Consolidated", System.StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sceneName, "SampleScene", System.StringComparison.OrdinalIgnoreCase);
    }

    private static void NormalizePlayerScale()
    {
        PlayerMover mover = Object.FindAnyObjectByType<PlayerMover>();
        if (mover == null)
        {
            return;
        }

        CharacterController controller = mover.GetComponent<CharacterController>();
        if (controller == null)
        {
            return;
        }

        float bottom = controller.center.y - controller.height * 0.5f;
        controller.height = PlayerHeight;
        controller.radius = PlayerRadius;
        controller.center = new Vector3(controller.center.x, bottom + PlayerHeight * 0.5f, controller.center.z);
        controller.skinWidth = 0.035f;
        controller.stepOffset = Mathf.Min(0.28f, PlayerHeight * 0.2f);
    }

    private static void UpgradeKnownLegacyDoors()
    {
        DoorInteractable[] doors = Object.FindObjectsByType<DoorInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < doors.Length; i++)
        {
            DoorInteractable door = doors[i];
            if (door == null || !IsLegacyGeneratedDoor(door.gameObject.name))
            {
                continue;
            }

            UpgradeCenteredLeafToPhysicalHinge(door);
        }
    }

    private static bool IsLegacyGeneratedDoor(string objectName)
    {
        if (string.Equals(objectName, "VS_InteriorGate_Door", System.StringComparison.Ordinal))
        {
            return true;
        }

        return objectName != null &&
               objectName.StartsWith("INT_", System.StringComparison.Ordinal) &&
               objectName.EndsWith("_Leaf", System.StringComparison.Ordinal);
    }

    private static void UpgradeCenteredLeafToPhysicalHinge(DoorInteractable door)
    {
        Transform leaf = door.transform;
        if (leaf.parent != null && leaf.parent.name.EndsWith("_PhysicalHinge", System.StringComparison.Ordinal))
        {
            return;
        }

        BoxCollider leafCollider = leaf.GetComponent<BoxCollider>();
        if (leafCollider == null)
        {
            return;
        }

        Vector3 scale = leaf.localScale;
        bool widthAlongZ = Mathf.Abs(scale.z * leafCollider.size.z) >= Mathf.Abs(scale.x * leafCollider.size.x);

        if (widthAlongZ)
        {
            scale.z = PreserveSign(scale.z, DoorWidth);
            scale.x = PreserveSign(scale.x, Mathf.Min(Mathf.Abs(scale.x), DoorThickness));
        }
        else
        {
            scale.x = PreserveSign(scale.x, DoorWidth);
            scale.z = PreserveSign(scale.z, Mathf.Min(Mathf.Abs(scale.z), DoorThickness));
        }
        scale.y = PreserveSign(scale.y, DoorHeight);
        leaf.localScale = scale;

        Vector3 localHingePoint = leafCollider.center;
        if (widthAlongZ)
        {
            localHingePoint.z -= leafCollider.size.z * 0.5f;
        }
        else
        {
            localHingePoint.x -= leafCollider.size.x * 0.5f;
        }

        Vector3 hingeWorld = leaf.TransformPoint(localHingePoint);
        Transform oldParent = leaf.parent;
        GameObject hingeObject = new GameObject(leaf.name + "_PhysicalHinge");
        Transform hinge = hingeObject.transform;
        hinge.SetParent(oldParent, true);
        hinge.position = hingeWorld;
        hinge.rotation = leaf.rotation;

        leaf.SetParent(hinge, true);
        CreateDoorJambFillers(oldParent, leaf, widthAlongZ);

        door.ConfigureMotion(
            hinge,
            DoorInteractable.DoorMotionType.Swing,
            Vector3.zero,
            new Vector3(0f, 102f, 0f),
            0.5f);
    }

    private static float PreserveSign(float source, float magnitude)
    {
        return source < 0f ? -magnitude : magnitude;
    }

    private static void CreateDoorJambFillers(Transform parent, Transform leaf, bool widthAlongZ)
    {
        if (parent == null || leaf == null || parent.Find(leaf.name + "_MetricJambA") != null)
        {
            return;
        }

        EnsureMaterials();
        const float legacyOpeningWidth = 2.0f;
        float fillerWidth = Mathf.Max(0f, (legacyOpeningWidth - DoorWidth) * 0.5f);
        if (fillerWidth <= 0.01f)
        {
            return;
        }

        Vector3 widthAxis = widthAlongZ ? leaf.forward : leaf.right;
        float offset = DoorWidth * 0.5f + fillerWidth * 0.5f;
        Vector3 fillerScale = widthAlongZ
            ? new Vector3(0.35f, DoorHeight, fillerWidth)
            : new Vector3(fillerWidth, DoorHeight, 0.35f);

        Vector3 center = leaf.position;
        center.y = leaf.position.y;
        CreateWorldBox(parent, leaf.name + "_MetricJambA", center - widthAxis * offset, leaf.rotation, fillerScale, wallMaterial);
        CreateWorldBox(parent, leaf.name + "_MetricJambB", center + widthAxis * offset, leaf.rotation, fillerScale, wallMaterial);
    }

    private static void EnsureHollowStorage()
    {
        GameObject interior = GameObject.Find("Template_Interior_BoardingHouse");
        if (interior == null || interior.transform.Find("UNITYLAPTOP_PhysicalStorage") != null)
        {
            return;
        }

        EnsureMaterials();
        GameObject storageRoot = new GameObject("UNITYLAPTOP_PhysicalStorage");
        storageRoot.transform.SetParent(interior.transform, false);

        CreateChest(storageRoot.transform, "KitchenSupplyChest", new Vector3(-5.65f, 0.1f, -2.75f), Quaternion.identity, "food_canned", "Sealed Rations");
        CreateChest(storageRoot.transform, "OfficeSupplyChest", new Vector3(5.55f, 0.1f, -2.75f), Quaternion.Euler(0f, 180f, 0f), "ammo_pistol", "Pistol Ammunition");
    }

    private static void CreateChest(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, string itemId, string itemDisplayName)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPosition;
        root.transform.localRotation = localRotation;

        CreateLocalBox(root.transform, "Bottom", new Vector3(0f, ChestBoard * 0.5f, 0f), new Vector3(ChestWidth, ChestBoard, ChestDepth), woodMaterial);
        CreateLocalBox(root.transform, "LeftSide", new Vector3(-ChestWidth * 0.5f + ChestBoard * 0.5f, ChestHeight * 0.5f, 0f), new Vector3(ChestBoard, ChestHeight, ChestDepth), woodMaterial);
        CreateLocalBox(root.transform, "RightSide", new Vector3(ChestWidth * 0.5f - ChestBoard * 0.5f, ChestHeight * 0.5f, 0f), new Vector3(ChestBoard, ChestHeight, ChestDepth), woodMaterial);
        CreateLocalBox(root.transform, "Front", new Vector3(0f, ChestHeight * 0.5f, -ChestDepth * 0.5f + ChestBoard * 0.5f), new Vector3(ChestWidth - ChestBoard * 2f, ChestHeight, ChestBoard), woodMaterial);
        CreateLocalBox(root.transform, "Back", new Vector3(0f, ChestHeight * 0.5f, ChestDepth * 0.5f - ChestBoard * 0.5f), new Vector3(ChestWidth - ChestBoard * 2f, ChestHeight, ChestBoard), woodMaterial);

        GameObject hingeObject = new GameObject("LidPivot");
        Transform hinge = hingeObject.transform;
        hinge.SetParent(root.transform, false);
        hinge.localPosition = new Vector3(0f, ChestHeight, ChestDepth * 0.5f - ChestBoard * 0.5f);

        GameObject lid = CreateLocalBox(
            hinge,
            "Lid",
            new Vector3(0f, ChestLidThickness * 0.5f, -ChestDepth * 0.5f + ChestBoard * 0.5f),
            new Vector3(ChestWidth, ChestLidThickness, ChestDepth),
            woodMaterial);
        BoxCollider lidCollider = lid.GetComponent<BoxCollider>();

        GameObject contentsRoot = new GameObject("Contents");
        contentsRoot.transform.SetParent(root.transform, false);
        contentsRoot.transform.localPosition = new Vector3(0f, ChestBoard + 0.13f, 0f);

        GameObject pickup = CreateLocalBox(contentsRoot.transform, "Loot", Vector3.zero, new Vector3(0.28f, 0.16f, 0.2f), contentsMaterial);
        PickupInteractable pickupInteractable = pickup.AddComponent<PickupInteractable>();
        pickupInteractable.ConfigureItem(itemId, itemDisplayName, "Recovered from a physically sealed storage chest.");
        contentsRoot.SetActive(false);

        PhysicalChestInteractable chest = root.AddComponent<PhysicalChestInteractable>();
        chest.Configure(hinge, lidCollider, contentsRoot, 105f, 0.55f);
    }

    private static GameObject CreateLocalBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPosition;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = localScale;
        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
        return box;
    }

    private static GameObject CreateWorldBox(Transform parent, string name, Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, true);
        box.transform.position = worldPosition;
        box.transform.rotation = worldRotation;
        box.transform.localScale = worldScale;
        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
        return box;
    }

    private static void EnsureAmbientScore()
    {
        if (GameObject.Find("UNITYLAPTOP_AmbientScore") != null)
        {
            return;
        }

        GameObject audioRoot = new GameObject("UNITYLAPTOP_AmbientScore");
        AudioSource source = audioRoot.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0.16f;
        source.priority = 180;

        if (ambientClip == null)
        {
            ambientClip = CreateProceduralAmbientClip();
        }

        if (ambientClip != null)
        {
            source.clip = ambientClip;
            source.Play();
        }
    }

    private static AudioClip CreateProceduralAmbientClip()
    {
        const int sampleRate = 44100;
        const int seconds = 8;
        int sampleCount = sampleRate * seconds;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float slow = 0.72f + 0.28f * Mathf.Sin(Mathf.PI * 2f * 0.125f * t);
            float drone = Mathf.Sin(Mathf.PI * 2f * 43.25f * t) * 0.18f;
            drone += Mathf.Sin(Mathf.PI * 2f * 55.75f * t + 1.7f) * 0.10f;
            drone += Mathf.Sin(Mathf.PI * 2f * 91.125f * t + 0.4f) * 0.035f;
            float pulse = Mathf.Sin(Mathf.PI * 2f * 0.25f * t);
            pulse = pulse * pulse * 0.025f;
            samples[i] = Mathf.Clamp((drone * slow) - pulse, -0.32f, 0.32f);
        }

        AudioClip clip = AudioClip.Create("UNITYLAPTOP_LosslessProceduralAmbient", sampleCount, 1, sampleRate, false);
        if (!clip.SetData(samples, 0))
        {
            Object.Destroy(clip);
            return null;
        }

        return clip;
    }

    private static void EnsureMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null)
        {
            return;
        }

        if (wallMaterial == null)
        {
            wallMaterial = new Material(shader) { name = "UNITYLAPTOP_RuntimeWall" };
            SetMaterialColor(wallMaterial, new Color(0.20f, 0.19f, 0.17f, 1f));
        }

        if (woodMaterial == null)
        {
            woodMaterial = new Material(shader) { name = "UNITYLAPTOP_RuntimeWood" };
            SetMaterialColor(woodMaterial, new Color(0.20f, 0.12f, 0.075f, 1f));
        }

        if (contentsMaterial == null)
        {
            contentsMaterial = new Material(shader) { name = "UNITYLAPTOP_RuntimeContents" };
            SetMaterialColor(contentsMaterial, new Color(0.31f, 0.29f, 0.22f, 1f));
        }
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }
}
