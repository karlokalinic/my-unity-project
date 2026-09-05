using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public sealed class RakeEncounterController : MonoBehaviour
{
    private enum EncounterState
    {
        Dormant,
        Exploring,
        Charging,
        Blackout,
        Recovering,
        Complete
    }

    private const string ResourcePath = "ThirdParty/TheRake/TheRake";
    private const float RoomWidth = 3.8f;
    private const float RoomDepth = 6.4f;
    private const float RoomHeight = 3.2f;
    private const float DoorWidth = 1.1f;
    private const float DoorHeight = 2.18f;

    [Header("Encounter")]
    [SerializeField] private float firstPersonDepth = 3.8f;
    [SerializeField] private float revealDepth = 4.75f;
    [SerializeField] private float chargeDuration = 1.05f;
    [SerializeField] private float blackoutHold = 1.05f;
    [SerializeField] private float recoverDuration = 0.9f;
    [SerializeField] private float cinematicFov = 56f;

    [Header("Atmosphere")]
    [SerializeField] private float flashlightRange = 13f;
    [SerializeField] private float flashlightIntensity = 4.2f;
    [SerializeField] private float flashlightSpotAngle = 34f;

    private PlayerMover playerMover;
    private CharacterController playerController;
    private HolstinCameraRig cameraRig;
    private Camera viewCamera;

    private Transform roomRoot;
    private BoxCollider entryTrigger;
    private Vector3 entryPoint;
    private Vector3 depthAxis;

    private Transform monsterRoot;
    private Vector3 monsterStartPosition;
    private AnimationClip runClip;
    private GameObject monsterAnimationRoot;
    private float runClipTime;

    private Light encounterFlashlight;
    private AudioSource breathingSource;
    private AudioSource stingSource;
    private Image blackoutImage;

    private EncounterState state;
    private float chargeElapsed;
    private float blackoutElapsed;
    private float recoveryElapsed;
    private float currentCameraBlend;
    private float deepestProgress;

    private Material roomMaterial;
    private Material creatureMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForSupportedScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() ||
            (!string.Equals(scene.name, "Scena", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(scene.name, "INTERAKCIJA", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(scene.name, "VerticalSlice_Consolidated", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        if (FindAnyObjectByType<RakeEncounterController>() != null)
        {
            return;
        }

        GameObject host = new GameObject("RakeEncounter_Runtime");
        host.AddComponent<RakeEncounterController>();
    }

    private void Start()
    {
        ResolveReferences();
        if (playerMover == null || cameraRig == null || viewCamera == null)
        {
            Debug.LogWarning("Rake encounter disabled: required player/camera references are missing.", this);
            enabled = false;
            return;
        }

        BuildEncounterRoom();
        CreateBlackoutOverlay();
        CreateAudio();
        state = EncounterState.Dormant;
    }

    private void Update()
    {
        if (state == EncounterState.Complete || !enabled)
        {
            return;
        }

        switch (state)
        {
            case EncounterState.Dormant:
                UpdateDormant();
                break;
            case EncounterState.Exploring:
                UpdateExploring();
                break;
            case EncounterState.Charging:
                UpdateCharging();
                break;
            case EncounterState.Blackout:
                UpdateBlackout();
                break;
            case EncounterState.Recovering:
                UpdateRecovery();
                break;
        }
    }

    private void OnDisable()
    {
        if (cameraRig != null)
        {
            cameraRig.ClearCinematicFirstPerson();
        }

        if (playerMover != null && state == EncounterState.Blackout)
        {
            playerMover.enabled = true;
        }

        if (breathingSource != null)
        {
            breathingSource.Stop();
        }
    }

    private void ResolveReferences()
    {
        playerMover = FindAnyObjectByType<PlayerMover>();
        cameraRig = FindAnyObjectByType<HolstinCameraRig>();

        if (playerMover != null)
        {
            playerController = playerMover.GetComponent<CharacterController>();
        }

        if (cameraRig != null)
        {
            viewCamera = cameraRig.ControlledCamera;
        }

        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }
    }

    private void BuildEncounterRoom()
    {
        GameObject interior = GameObject.Find("Template_Interior_BoardingHouse");
        Vector3 interiorOrigin = interior != null ? interior.transform.position : new Vector3(14f, 0f, -2f);

        GameObject world = GameObject.Find("_World");
        GameObject room = new GameObject("RakeEncounter_PitchBlackRoom");
        roomRoot = room.transform;
        if (world != null)
        {
            roomRoot.SetParent(world.transform, true);
        }

        roomRoot.position = interiorOrigin + new Vector3(4.7f, 0f, -7.65f);
        depthAxis = Vector3.back;

        roomMaterial = CreateRuntimeMaterial(
            "RakeRoom_Black",
            new Color(0.0035f, 0.0035f, 0.0045f, 1f),
            0.08f,
            0f);
        creatureMaterial = CreateRuntimeMaterial(
            "RakeFallback_Pale",
            new Color(0.62f, 0.64f, 0.62f, 1f),
            0.12f,
            0f);

        float halfDepth = RoomDepth * 0.5f;
        float halfWidth = RoomWidth * 0.5f;
        float backZ = halfDepth;
        float farZ = -halfDepth;

        CreateRoomCube("Floor", new Vector3(0f, -0.1f, 0f), new Vector3(RoomWidth, 0.2f, RoomDepth));
        CreateRoomCube("Ceiling", new Vector3(0f, RoomHeight + 0.1f, 0f), new Vector3(RoomWidth, 0.2f, RoomDepth));
        CreateRoomCube("LeftWall", new Vector3(-halfWidth, RoomHeight * 0.5f, 0f), new Vector3(0.22f, RoomHeight, RoomDepth));
        CreateRoomCube("RightWall", new Vector3(halfWidth, RoomHeight * 0.5f, 0f), new Vector3(0.22f, RoomHeight, RoomDepth));
        CreateRoomCube("FarWall", new Vector3(0f, RoomHeight * 0.5f, farZ), new Vector3(RoomWidth, RoomHeight, 0.22f));

        float sideWidth = (RoomWidth - DoorWidth) * 0.5f;
        float sideCenter = (DoorWidth * 0.5f) + (sideWidth * 0.5f);
        CreateRoomCube("DoorWallLeft", new Vector3(-sideCenter, RoomHeight * 0.5f, backZ), new Vector3(sideWidth, RoomHeight, 0.22f));
        CreateRoomCube("DoorWallRight", new Vector3(sideCenter, RoomHeight * 0.5f, backZ), new Vector3(sideWidth, RoomHeight, 0.22f));
        CreateRoomCube(
            "DoorLintel",
            new Vector3(0f, DoorHeight + ((RoomHeight - DoorHeight) * 0.5f), backZ),
            new Vector3(DoorWidth, RoomHeight - DoorHeight, 0.22f));

        BuildPhysicalDoor(backZ);
        BuildEntryTrigger(backZ);
        BuildDarknessVolume();

        entryPoint = roomRoot.TransformPoint(new Vector3(0f, 1f, backZ - 0.55f));

        monsterRoot = BuildMonster(roomRoot.TransformPoint(new Vector3(0f, 0f, farZ + 0.78f)));
        monsterStartPosition = monsterRoot.position;
        monsterRoot.gameObject.SetActive(false);
    }

    private void BuildPhysicalDoor(float backZ)
    {
        GameObject hingeObject = new GameObject("RakeRoom_DoorHinge");
        Transform hinge = hingeObject.transform;
        hinge.SetParent(roomRoot, false);
        hinge.localPosition = new Vector3(-DoorWidth * 0.5f, 0f, backZ - 0.03f);

        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leaf.name = "RakeRoom_DoorLeaf";
        leaf.transform.SetParent(hinge, false);
        leaf.transform.localPosition = new Vector3(DoorWidth * 0.5f, DoorHeight * 0.5f, 0f);
        leaf.transform.localScale = new Vector3(DoorWidth, DoorHeight, 0.12f);

        Renderer rendererComponent = leaf.GetComponent<Renderer>();
        if (rendererComponent != null)
        {
            rendererComponent.sharedMaterial = CreateRuntimeMaterial(
                "RakeRoom_Door",
                new Color(0.045f, 0.035f, 0.025f, 1f),
                0.18f,
                0f);
        }

        DoorInteractable door = hingeObject.AddComponent<DoorInteractable>();
        door.ConfigureMotion(
            hinge,
            DoorInteractable.DoorMotionType.Swing,
            Vector3.zero,
            new Vector3(0f, 102f, 0f),
            0.72f);
    }

    private void BuildEntryTrigger(float backZ)
    {
        GameObject triggerObject = new GameObject("RakeRoom_EntryTrigger");
        triggerObject.transform.SetParent(roomRoot, false);
        triggerObject.transform.localPosition = new Vector3(0f, 1.25f, backZ - 0.82f);
        entryTrigger = triggerObject.AddComponent<BoxCollider>();
        entryTrigger.isTrigger = true;
        entryTrigger.size = new Vector3(2.6f, 2.5f, 0.75f);
    }

    private void BuildDarknessVolume()
    {
        GameObject volumeObject = new GameObject("RakeRoom_DarknessVolume");
        volumeObject.transform.SetParent(roomRoot, false);
        volumeObject.transform.localPosition = new Vector3(0f, RoomHeight * 0.5f, 0f);

        BoxCollider volumeCollider = volumeObject.AddComponent<BoxCollider>();
        volumeCollider.isTrigger = true;
        volumeCollider.size = new Vector3(RoomWidth - 0.15f, RoomHeight, RoomDepth - 0.15f);

        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = false;
        volume.priority = 80f;
        volume.blendDistance = 0.5f;
        volume.weight = 1f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "RakeRoom_RuntimeProfile";

        ColorAdjustments color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(-3.15f);
        color.contrast.Override(18f);
        color.saturation.Override(-22f);
        color.colorFilter.Override(new Color(0.74f, 0.8f, 0.9f, 1f));

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.52f);
        vignette.smoothness.Override(0.38f);

        FilmGrain grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Thin1);
        grain.intensity.Override(0.22f);
        grain.response.Override(0.78f);

        volume.profile = profile;
    }

    private GameObject CreateRoomCube(string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(roomRoot, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = localScale;

        Renderer rendererComponent = cube.GetComponent<Renderer>();
        if (rendererComponent != null)
        {
            rendererComponent.sharedMaterial = roomMaterial;
        }

        return cube;
    }

    private Transform BuildMonster(Vector3 worldPosition)
    {
        GameObject monster = new GameObject("RakeEncounter_Monster");
        monster.transform.SetParent(roomRoot, true);
        monster.transform.position = worldPosition;
        monster.transform.rotation = Quaternion.LookRotation(-depthAxis, Vector3.up);

        GameObject downloadedPrefab = Resources.Load<GameObject>(ResourcePath);
        if (downloadedPrefab != null)
        {
            GameObject visual = Instantiate(downloadedPrefab, monster.transform);
            visual.name = "TheRake_SealifeFan3_CC_BY_4";
            DisableImportedPhysics(visual);
            FitVisualToHeight(visual.transform, 2.3f);
            AlignVisualFeet(visual.transform);
            monsterAnimationRoot = visual;
            ResolveRunClip(visual);
        }
        else
        {
            BuildFallbackCreature(monster.transform);
            monsterAnimationRoot = monster;
            Debug.LogWarning(
                "The Rake cinematic is active with a procedural stand-in. " +
                "Place the attributed Sealife Fan 3 model prefab at Resources/" + ResourcePath +
                " to bind the exact animated asset.", this);
        }

        AddCompoundCreatureCollision(monster);

        Rigidbody body = monster.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        return monster.transform;
    }

    private void BuildFallbackCreature(Transform parent)
    {
        CreateCreaturePrimitive(PrimitiveType.Capsule, "Torso", parent, new Vector3(0f, 1.25f, 0f), new Vector3(0.48f, 0.88f, 0.36f), Quaternion.identity);
        CreateCreaturePrimitive(PrimitiveType.Sphere, "Head", parent, new Vector3(0f, 2.05f, 0.03f), new Vector3(0.38f, 0.46f, 0.42f), Quaternion.identity);

        CreateCreaturePrimitive(PrimitiveType.Capsule, "Arm_L", parent, new Vector3(-0.47f, 1.22f, 0f), new Vector3(0.18f, 0.86f, 0.18f), Quaternion.Euler(0f, 0f, -18f));
        CreateCreaturePrimitive(PrimitiveType.Capsule, "Arm_R", parent, new Vector3(0.47f, 1.22f, 0f), new Vector3(0.18f, 0.86f, 0.18f), Quaternion.Euler(0f, 0f, 18f));

        CreateCreaturePrimitive(PrimitiveType.Capsule, "Leg_L", parent, new Vector3(-0.19f, 0.52f, 0f), new Vector3(0.2f, 0.62f, 0.2f), Quaternion.Euler(7f, 0f, 0f));
        CreateCreaturePrimitive(PrimitiveType.Capsule, "Leg_R", parent, new Vector3(0.19f, 0.52f, 0f), new Vector3(0.2f, 0.62f, 0.2f), Quaternion.Euler(-7f, 0f, 0f));

        CreateCreaturePrimitive(PrimitiveType.Cube, "Claws_L", parent, new Vector3(-0.68f, 0.62f, -0.08f), new Vector3(0.08f, 0.72f, 0.08f), Quaternion.Euler(8f, 0f, -8f));
        CreateCreaturePrimitive(PrimitiveType.Cube, "Claws_R", parent, new Vector3(0.68f, 0.62f, -0.08f), new Vector3(0.08f, 0.72f, 0.08f), Quaternion.Euler(8f, 0f, 8f));
    }

    private void CreateCreaturePrimitive(
        PrimitiveType type,
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Quaternion localRotation)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localRotation = localRotation;
        primitive.transform.localScale = localScale;

        Collider primitiveCollider = primitive.GetComponent<Collider>();
        if (primitiveCollider != null)
        {
            primitiveCollider.enabled = false;
            Destroy(primitiveCollider);
        }

        Renderer rendererComponent = primitive.GetComponent<Renderer>();
        if (rendererComponent != null)
        {
            rendererComponent.sharedMaterial = creatureMaterial;
        }
    }

    private static void AddCompoundCreatureCollision(GameObject monster)
    {
        CapsuleCollider torso = monster.AddComponent<CapsuleCollider>();
        torso.center = new Vector3(0f, 1.25f, 0f);
        torso.radius = 0.29f;
        torso.height = 1.55f;

        SphereCollider head = monster.AddComponent<SphereCollider>();
        head.center = new Vector3(0f, 2.05f, 0.02f);
        head.radius = 0.25f;

        AddLimbCollider(monster.transform, "PhysicalArm_L", new Vector3(-0.45f, 1.22f, 0f), new Vector3(0f, 0f, -18f), 1.35f, 0.10f);
        AddLimbCollider(monster.transform, "PhysicalArm_R", new Vector3(0.45f, 1.22f, 0f), new Vector3(0f, 0f, 18f), 1.35f, 0.10f);
        AddLimbCollider(monster.transform, "PhysicalLeg_L", new Vector3(-0.18f, 0.52f, 0f), new Vector3(7f, 0f, 0f), 1.05f, 0.12f);
        AddLimbCollider(monster.transform, "PhysicalLeg_R", new Vector3(0.18f, 0.52f, 0f), new Vector3(-7f, 0f, 0f), 1.05f, 0.12f);
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
        colliderComponent.height = height;
        colliderComponent.radius = radius;
    }

    private static void DisableImportedPhysics(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody[] bodies = visual.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].isKinematic = true;
            bodies[i].useGravity = false;
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

    private static void AlignVisualFeet(Transform visualRoot)
    {
        if (!TryGetBounds(visualRoot, out Bounds bounds))
        {
            return;
        }

        float deltaY = visualRoot.parent.position.y - bounds.min.y;
        visualRoot.position += Vector3.up * deltaY;
    }

    private static bool TryGetBounds(Transform root, out Bounds combined)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        combined = default;

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

    private void ResolveRunClip(GameObject visual)
    {
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(ResourcePath);
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name.IndexOf("run", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                runClip = clips[i];
                return;
            }
        }

        Animator animator = visual.GetComponentInChildren<Animator>(true);
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] controllerClips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < controllerClips.Length; i++)
            {
                AnimationClip candidate = controllerClips[i];
                if (candidate != null && candidate.name.IndexOf("run", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    runClip = candidate;
                    return;
                }
            }
        }
    }

    private void CreateBlackoutOverlay()
    {
        GameObject canvasObject = new GameObject("RakeEncounter_BlackoutCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        GameObject imageObject = new GameObject("Blackout", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        blackoutImage = imageObject.GetComponent<Image>();
        blackoutImage.raycastTarget = false;
        SetBlackoutAlpha(0f);
    }

    private void CreateAudio()
    {
        breathingSource = gameObject.AddComponent<AudioSource>();
        breathingSource.playOnAwake = false;
        breathingSource.loop = true;
        breathingSource.spatialBlend = 0f;
        breathingSource.volume = 0f;
        breathingSource.clip = CreateBreathingClip();

        stingSource = gameObject.AddComponent<AudioSource>();
        stingSource.playOnAwake = false;
        stingSource.loop = false;
        stingSource.spatialBlend = 0f;
        stingSource.volume = 0.8f;
    }

    private static AudioClip CreateBreathingClip()
    {
        const int sampleRate = 22050;
        const float duration = 5.5f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        uint noiseState = 0xA341316Cu;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float breath = 0.5f + 0.5f * Mathf.Sin((t / 3.5f) * Mathf.PI * 2f - 1.1f);
            breath *= breath;

            noiseState = (noiseState * 1664525u) + 1013904223u;
            float noise = (((noiseState >> 8) & 0xFFFF) / 32767.5f) - 1f;

            float chest = Mathf.Sin(t * Mathf.PI * 2f * 62f) * 0.022f;
            float air = noise * 0.032f * breath;
            samples[i] = Mathf.Clamp(chest + air, -0.16f, 0.16f);
        }

        AudioClip clip = AudioClip.Create("RakeEncounter_Breathing", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateRevealSting()
    {
        const int sampleRate = 22050;
        const float duration = 0.62f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        uint noiseState = 0x91E10DA5u;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 5.6f);
            noiseState = (noiseState * 1103515245u) + 12345u;
            float noise = (((noiseState >> 9) & 0x7FFF) / 16383.5f) - 1f;
            float low = Mathf.Sin(t * Mathf.PI * 2f * (78f - 24f * t)) * 0.5f;
            samples[i] = Mathf.Clamp((low + noise * 0.42f) * envelope * 0.65f, -0.9f, 0.9f);
        }

        AudioClip clip = AudioClip.Create("RakeEncounter_RevealSting", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void UpdateDormant()
    {
        if (entryTrigger == null || playerMover == null)
        {
            return;
        }

        Vector3 probe = playerMover.transform.position + Vector3.up;
        if (!entryTrigger.bounds.Contains(probe))
        {
            return;
        }

        state = EncounterState.Exploring;
        if (breathingSource != null && breathingSource.clip != null)
        {
            breathingSource.Play();
        }

        EnsureFlashlight();
    }

    private void UpdateExploring()
    {
        float depth = Vector3.Dot(playerMover.transform.position - entryPoint, depthAxis);
        float progress = Mathf.Clamp01(depth / Mathf.Max(0.1f, firstPersonDepth));
        deepestProgress = Mathf.Max(deepestProgress, progress);

        currentCameraBlend = SmoothStep(progress);
        ApplyCameraMotion(currentCameraBlend, 0f);
        UpdateFlashlight(progress, false);

        if (breathingSource != null)
        {
            breathingSource.volume = Mathf.Lerp(0.04f, 0.42f, currentCameraBlend);
            breathingSource.pitch = Mathf.Lerp(0.92f, 1.08f, currentCameraBlend);
        }

        if (depth >= revealDepth)
        {
            BeginCharge();
        }
    }

    private void BeginCharge()
    {
        state = EncounterState.Charging;
        chargeElapsed = 0f;
        runClipTime = 0f;

        if (monsterRoot != null)
        {
            monsterRoot.position = monsterStartPosition;
            monsterRoot.gameObject.SetActive(true);
        }

        if (stingSource != null)
        {
            stingSource.PlayOneShot(CreateRevealSting(), 0.9f);
        }
    }

    private void UpdateCharging()
    {
        chargeElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(chargeElapsed / Mathf.Max(0.1f, chargeDuration));
        float eased = t * t;

        if (monsterRoot != null && playerMover != null)
        {
            Vector3 playerTarget = playerMover.transform.position - depthAxis * 0.48f;
            playerTarget.y = monsterStartPosition.y;
            monsterRoot.position = Vector3.Lerp(monsterStartPosition, playerTarget, eased);

            Vector3 face = playerMover.transform.position - monsterRoot.position;
            face.y = 0f;
            if (face.sqrMagnitude > 0.001f)
            {
                monsterRoot.rotation = Quaternion.LookRotation(face.normalized, Vector3.up);
            }

            if (runClip != null)
            {
                runClipTime += Time.deltaTime;
                float sampleTime = runClip.length > 0.001f ? runClipTime % runClip.length : 0f;
                runClip.SampleAnimation(monsterAnimationRoot != null ? monsterAnimationRoot : monsterRoot.gameObject, sampleTime);
            }
            else
            {
                Vector3 local = monsterRoot.localPosition;
                local.y = Mathf.Abs(Mathf.Sin(Time.time * 12f)) * 0.045f;
                monsterRoot.localPosition = local;
            }
        }

        float shake = Mathf.Lerp(0.004f, 0.042f, t);
        ApplyCameraMotion(1f, shake);
        UpdateFlashlight(1f, true);

        if (breathingSource != null)
        {
            breathingSource.volume = Mathf.Lerp(0.42f, 0.62f, t);
            breathingSource.pitch = Mathf.Lerp(1.08f, 1.22f, t);
        }

        if (t >= 0.78f)
        {
            SetBlackoutAlpha(SmoothStep(Mathf.InverseLerp(0.78f, 1f, t)));
        }

        if (t >= 1f)
        {
            EnterBlackout();
        }
    }

    private void EnterBlackout()
    {
        state = EncounterState.Blackout;
        blackoutElapsed = 0f;
        SetBlackoutAlpha(1f);

        if (playerMover != null)
        {
            playerMover.ResetMotion();
            playerMover.enabled = false;
        }

        if (monsterRoot != null)
        {
            monsterRoot.gameObject.SetActive(false);
        }

        if (encounterFlashlight != null)
        {
            encounterFlashlight.enabled = false;
        }
    }

    private void UpdateBlackout()
    {
        blackoutElapsed += Time.deltaTime;

        if (breathingSource != null)
        {
            breathingSource.volume = Mathf.MoveTowards(breathingSource.volume, 0f, Time.deltaTime * 1.2f);
        }

        if (blackoutElapsed < blackoutHold)
        {
            return;
        }

        if (playerMover != null)
        {
            playerMover.enabled = true;
        }

        state = EncounterState.Recovering;
        recoveryElapsed = 0f;
    }

    private void UpdateRecovery()
    {
        recoveryElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(recoveryElapsed / Mathf.Max(0.1f, recoverDuration));

        SetBlackoutAlpha(1f - SmoothStep(Mathf.Clamp01(t / 0.72f)));
        currentCameraBlend = 1f - SmoothStep(t);
        ApplyCameraMotion(currentCameraBlend, 0f);

        if (t < 1f)
        {
            return;
        }

        SetBlackoutAlpha(0f);
        cameraRig.ClearCinematicFirstPerson();

        if (breathingSource != null)
        {
            breathingSource.Stop();
        }

        if (encounterFlashlight != null)
        {
            Destroy(encounterFlashlight.gameObject);
            encounterFlashlight = null;
        }

        state = EncounterState.Complete;
    }

    private void ApplyCameraMotion(float blend, float shake)
    {
        if (cameraRig == null || playerMover == null)
        {
            return;
        }

        float speed01 = Mathf.Clamp01(playerMover.CurrentPlanarSpeed / 4.2f);
        float time = Time.time;

        float stepPhase = time * Mathf.Lerp(5.8f, 9.2f, speed01);
        float lateralBob = Mathf.Sin(stepPhase) * 0.014f * speed01;
        float verticalBob = Mathf.Abs(Mathf.Sin(stepPhase)) * 0.022f * speed01;

        float breathPhase = time * 1.65f;
        float breathingY = Mathf.Sin(breathPhase) * 0.012f * blend;
        float breathingPitch = Mathf.Sin(breathPhase + 0.7f) * 0.55f * blend;
        float roll = Mathf.Sin(stepPhase * 0.5f) * 0.38f * speed01 * blend;

        if (shake > 0f)
        {
            float nx = Mathf.PerlinNoise(17.2f, time * 19f) - 0.5f;
            float ny = Mathf.PerlinNoise(41.8f, time * 23f) - 0.5f;
            lateralBob += nx * shake;
            verticalBob += ny * shake;
            roll += nx * shake * 18f;
            breathingPitch += ny * shake * 16f;
        }

        float fov = state == EncounterState.Charging
            ? Mathf.Lerp(cinematicFov, cinematicFov + 8f, Mathf.Clamp01(chargeElapsed / Mathf.Max(0.1f, chargeDuration)))
            : cinematicFov;

        cameraRig.SetCinematicFirstPerson(
            blend,
            new Vector3(lateralBob, verticalBob + breathingY, 0f),
            new Vector3(breathingPitch, 0f, roll),
            fov,
            true);
    }

    private void EnsureFlashlight()
    {
        if (encounterFlashlight != null || viewCamera == null)
        {
            return;
        }

        GameObject lightObject = new GameObject("RakeEncounter_CheapFlashlight");
        lightObject.transform.SetParent(viewCamera.transform, false);
        lightObject.transform.localPosition = new Vector3(0.11f, -0.13f, 0.08f);
        lightObject.transform.localRotation = Quaternion.Euler(1.5f, -1.2f, 0f);

        encounterFlashlight = lightObject.AddComponent<Light>();
        encounterFlashlight.type = LightType.Spot;
        encounterFlashlight.color = new Color(0.78f, 0.84f, 0.94f, 1f);
        encounterFlashlight.range = flashlightRange;
        encounterFlashlight.spotAngle = flashlightSpotAngle;
        encounterFlashlight.innerSpotAngle = flashlightSpotAngle * 0.52f;
        encounterFlashlight.intensity = 0f;
        encounterFlashlight.shadows = LightShadows.Hard;
        encounterFlashlight.renderMode = LightRenderMode.ForcePixel;
    }

    private void UpdateFlashlight(float progress, bool panic)
    {
        if (encounterFlashlight == null)
        {
            return;
        }

        float baseIntensity = flashlightIntensity * Mathf.SmoothStep(0.18f, 1f, progress);
        float flickerRate = panic ? 31f : 11f;
        float flickerDepth = panic ? 0.62f : 0.12f;
        float noise = Mathf.PerlinNoise(Time.time * flickerRate, 0.31f);
        float flicker = Mathf.Lerp(1f - flickerDepth, 1f, noise);
        encounterFlashlight.intensity = baseIntensity * flicker;
        encounterFlashlight.enabled = encounterFlashlight.intensity > 0.02f;
    }

    private void SetBlackoutAlpha(float alpha)
    {
        if (blackoutImage == null)
        {
            return;
        }

        blackoutImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
    }

    private static float SmoothStep(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private static Material CreateRuntimeMaterial(string name, Color color, float smoothness, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = name;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }
        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", smoothness);
        }
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        material.enableInstancing = true;
        return material;
    }
}
