using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class ProceduralHumanoidRig : MonoBehaviour
{
    [Serializable]
    public class BoneBinding
    {
        public string boneName;
        public string parentBoneName;
        public Transform physical;
        public Transform target;
        public Rigidbody body;
        public ConfigurableJoint joint;
    }

    [Serializable]
    private struct BoneDefinition
    {
        public string boneName;
        public string parentBoneName;
        public PrimitiveType primitiveType;
        public Vector3 localPosition;
        public Vector3 localScale;
        public float mass;
        public float lowAngularXLimit;
        public float highAngularXLimit;
        public float angularYLimit;
        public float angularZLimit;
        public float springForce;
        public float damperForce;
    }

    [Header("Build")]
    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private bool hideSourceRenderers = false;
    [SerializeField] private bool showRagdollRenderersInLife = false;
    [SerializeField] private bool setRagdollLayerToIgnoreRaycast = true;
    [Tooltip("Physics layer index used for ragdoll bones. Set Physics collision matrix to disable self-collision on this layer.")]
    [SerializeField] private int ragdollLayer = 10;

    [Header("Roots")]
    [SerializeField] private Transform physicalRoot;
    [SerializeField] private Transform targetRoot;

    [Header("Tuning")]
    [SerializeField] private float rootHeight = 0f;

    [SerializeField] private BoneBinding[] bindings = Array.Empty<BoneBinding>();

    private readonly Dictionary<string, BoneBinding> bindingsByName = new Dictionary<string, BoneBinding>(StringComparer.OrdinalIgnoreCase);
#if UNITY_EDITOR
    private bool validateBuildQueued;
#endif

    public Transform PhysicalRoot => physicalRoot;
    public Transform TargetRoot => targetRoot;
    public BoneBinding[] Bindings => bindings;
    public bool HideSourceRenderers => hideSourceRenderers;
    public bool ShowRagdollRenderersInLife => showRagdollRenderersInLife;

    public void EnsureBuilt()
    {
        ResolveRoots();
        BuildOrResolveBindings();
        ApplyRendererVisibility();
    }

    public void ConfigureRendererVisibility(bool hideOriginalRenderers, bool showRagdollWhenAlive)
    {
        hideSourceRenderers = hideOriginalRenderers;
        showRagdollRenderersInLife = showRagdollWhenAlive;
        ApplyRendererVisibility();
    }

    public void SetRagdollRenderersVisible(bool visible)
    {
        if (physicalRoot == null)
        {
            return;
        }

        Renderer[] ragdollRenderers = physicalRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < ragdollRenderers.Length; i++)
        {
            if (ragdollRenderers[i] != null)
            {
                ragdollRenderers[i].enabled = visible;
            }
        }
    }

    public bool TryGetBinding(string boneName, out BoneBinding binding)
    {
        if (string.IsNullOrWhiteSpace(boneName))
        {
            binding = null;
            return false;
        }

        if (bindingsByName.Count == 0)
        {
            RebuildLookupCache();
        }

        return bindingsByName.TryGetValue(boneName, out binding);
    }

    public Transform GetBone(string boneName, bool targetRig)
    {
        if (!TryGetBinding(boneName, out BoneBinding binding) || binding == null)
        {
            return null;
        }

        return targetRig ? binding.target : binding.physical;
    }

    private void Awake()
    {
        if (buildOnAwake)
        {
            EnsureBuilt();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

#if UNITY_EDITOR
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        // Never rebuild rig while Unity imports prefab assets in background workers.
        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            return;
        }

        if (!gameObject.scene.IsValid() || string.IsNullOrWhiteSpace(gameObject.scene.path))
        {
            return;
        }

        if (validateBuildQueued)
        {
            return;
        }

        validateBuildQueued = true;
        EditorApplication.delayCall += RunDeferredValidate;
#else
        ResolveRoots();
        BuildOrResolveBindings();
        ApplyRendererVisibility();
#endif
    }

#if UNITY_EDITOR
    private void RunDeferredValidate()
    {
        validateBuildQueued = false;
        if (this == null || Application.isPlaying)
        {
            return;
        }

        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            return;
        }

        if (!gameObject.scene.IsValid() || string.IsNullOrWhiteSpace(gameObject.scene.path))
        {
            return;
        }

        ResolveRoots();
        BuildOrResolveBindings();
        ApplyRendererVisibility();
    }
#endif

    private void ResolveRoots()
    {
        if (physicalRoot == null)
        {
            Transform existing = transform.Find("HumanoidRagdoll");
            if (existing == null)
            {
                GameObject root = new GameObject("HumanoidRagdoll");
                root.transform.SetParent(transform, false);
                existing = root.transform;
            }

            physicalRoot = existing;
        }

        if (targetRoot == null)
        {
            Transform existing = transform.Find("HumanoidPoseTargets");
            if (existing == null)
            {
                GameObject root = new GameObject("HumanoidPoseTargets");
                root.transform.SetParent(transform, false);
                existing = root.transform;
            }

            targetRoot = existing;
        }

        physicalRoot.localPosition = new Vector3(0f, rootHeight, 0f);
        targetRoot.localPosition = new Vector3(0f, rootHeight, 0f);
    }

    private void BuildOrResolveBindings()
    {
        List<BoneDefinition> definitions = GetBoneDefinitions();
        List<BoneBinding> resolved = new List<BoneBinding>(definitions.Count);

        Dictionary<string, Transform> physicalByName = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, Transform> targetByName = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < definitions.Count; i++)
        {
            BoneDefinition definition = definitions[i];
            Transform parentPhysical = string.IsNullOrWhiteSpace(definition.parentBoneName)
                ? physicalRoot
                : (physicalByName.TryGetValue(definition.parentBoneName, out Transform parentResult) ? parentResult : physicalRoot);
            Transform parentTarget = string.IsNullOrWhiteSpace(definition.parentBoneName)
                ? targetRoot
                : (targetByName.TryGetValue(definition.parentBoneName, out Transform parentTargetResult) ? parentTargetResult : targetRoot);

            Transform physical = EnsurePhysicalBone(definition, parentPhysical);
            Transform target = EnsureTargetBone(definition, parentTarget);

            physicalByName[definition.boneName] = physical;
            targetByName[definition.boneName] = target;
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            BoneDefinition definition = definitions[i];
            Transform physical = physicalByName[definition.boneName];
            Transform target = targetByName[definition.boneName];
            Rigidbody body = EnsureRigidbody(physical, definition);
            ConfigurableJoint joint = EnsureJoint(definition, physical, physicalByName);

            BoneBinding binding = new BoneBinding
            {
                boneName = definition.boneName,
                parentBoneName = definition.parentBoneName,
                physical = physical,
                target = target,
                body = body,
                joint = joint
            };
            resolved.Add(binding);
        }

        bindings = resolved.ToArray();
        RebuildLookupCache();
    }

    private Transform EnsurePhysicalBone(BoneDefinition definition, Transform parent)
    {
        Transform existing = parent.Find(definition.boneName);
        GameObject boneObject = null;

        if (existing == null)
        {
            boneObject = GameObject.CreatePrimitive(definition.primitiveType);
            boneObject.name = definition.boneName;
            boneObject.transform.SetParent(parent, false);
            existing = boneObject.transform;
        }
        else
        {
            boneObject = existing.gameObject;
        }

        existing.localPosition = definition.localPosition;
        existing.localRotation = Quaternion.identity;
        existing.localScale = definition.localScale;

        if (setRagdollLayerToIgnoreRaycast)
        {
            existing.gameObject.layer = ragdollLayer;
        }

        Collider colliderComponent = existing.GetComponent<Collider>();
        if (colliderComponent == null)
        {
            colliderComponent = existing.gameObject.AddComponent<CapsuleCollider>();
        }
        colliderComponent.isTrigger = false;

        return existing;
    }

    private Transform EnsureTargetBone(BoneDefinition definition, Transform parent)
    {
        Transform existing = parent.Find(definition.boneName);
        if (existing == null)
        {
            GameObject target = new GameObject(definition.boneName);
            target.transform.SetParent(parent, false);
            existing = target.transform;
        }

        existing.localPosition = definition.localPosition;
        existing.localRotation = Quaternion.identity;
        existing.localScale = Vector3.one;
        return existing;
    }

    private static Rigidbody EnsureRigidbody(Transform physical, BoneDefinition definition)
    {
        Rigidbody body = physical.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = physical.gameObject.AddComponent<Rigidbody>();
        }

        bool isRoot = string.IsNullOrWhiteSpace(definition.parentBoneName);
        body.mass = Mathf.Max(0.05f, definition.mass);
        body.useGravity = false;
        body.linearDamping = 0.15f;
        body.angularDamping = 0.2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.isKinematic = isRoot;
        return body;
    }

    private static ConfigurableJoint EnsureJoint(BoneDefinition definition, Transform physical, Dictionary<string, Transform> physicalByName)
    {
        ConfigurableJoint joint = physical.GetComponent<ConfigurableJoint>();
        if (string.IsNullOrWhiteSpace(definition.parentBoneName))
        {
            if (joint != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(joint);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(joint);
                }
            }
            return null;
        }

        if (!physicalByName.TryGetValue(definition.parentBoneName, out Transform parent))
        {
            return null;
        }

        if (joint == null)
        {
            joint = physical.gameObject.AddComponent<ConfigurableJoint>();
        }

        Rigidbody parentBody = parent.GetComponent<Rigidbody>();
        joint.connectedBody = parentBody;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit lowXLimit = new SoftJointLimit { limit = definition.lowAngularXLimit };
        SoftJointLimit highXLimit = new SoftJointLimit { limit = definition.highAngularXLimit };
        SoftJointLimit yLimit = new SoftJointLimit { limit = definition.angularYLimit };
        SoftJointLimit zLimit = new SoftJointLimit { limit = definition.angularZLimit };
        joint.lowAngularXLimit = lowXLimit;
        joint.highAngularXLimit = highXLimit;
        joint.angularYLimit = yLimit;
        joint.angularZLimit = zLimit;
        joint.rotationDriveMode = RotationDriveMode.Slerp;
        joint.slerpDrive = new JointDrive
        {
            positionSpring = definition.springForce > 0f ? definition.springForce : 380f,
            positionDamper = definition.damperForce > 0f ? definition.damperForce : 45f,
            maximumForce = 1_000_000f
        };

        return joint;
    }

    private void ApplyRendererVisibility()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        bool hasSourceRenderer = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            bool isRagdollRenderer = physicalRoot != null && rendererComponent.transform.IsChildOf(physicalRoot);
            if (!isRagdollRenderer)
            {
                hasSourceRenderer = true;
                break;
            }
        }

        bool showRagdollInLife = showRagdollRenderersInLife || hideSourceRenderers || !hasSourceRenderer;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererComponent = renderers[i];
            if (rendererComponent == null)
            {
                continue;
            }

            if (physicalRoot != null && rendererComponent.transform.IsChildOf(physicalRoot))
            {
                rendererComponent.enabled = showRagdollInLife;
                continue;
            }

            rendererComponent.enabled = !hideSourceRenderers;
        }
    }

    private void RebuildLookupCache()
    {
        bindingsByName.Clear();
        if (bindings == null)
        {
            return;
        }

        for (int i = 0; i < bindings.Length; i++)
        {
            BoneBinding binding = bindings[i];
            if (binding == null || string.IsNullOrWhiteSpace(binding.boneName))
            {
                continue;
            }

            bindingsByName[binding.boneName] = binding;
        }
    }

    private static List<BoneDefinition> GetBoneDefinitions()
    {
        // Per-bone angular limits (degrees) and spring/damper profiles for realistic ragdoll.
        // lowAngularX / highAngularX = pitch range, angularY = twist, angularZ = side bend.
        return new List<BoneDefinition>
        {
            new BoneDefinition { boneName = "Hips", parentBoneName = "", primitiveType = PrimitiveType.Cube, localPosition = new Vector3(0f, 1f, 0f), localScale = new Vector3(0.28f, 0.18f, 0.2f), mass = 1.25f, lowAngularXLimit = 0f, highAngularXLimit = 0f, angularYLimit = 0f, angularZLimit = 0f, springForce = 0f, damperForce = 0f },

            // Spine: limited bend
            new BoneDefinition { boneName = "Spine", parentBoneName = "Hips", primitiveType = PrimitiveType.Cube, localPosition = new Vector3(0f, 0.24f, 0f), localScale = new Vector3(0.26f, 0.2f, 0.18f), mass = 0.8f, lowAngularXLimit = -20f, highAngularXLimit = 30f, angularYLimit = 15f, angularZLimit = 12f, springForce = 500f, damperForce = 55f },
            new BoneDefinition { boneName = "Chest", parentBoneName = "Spine", primitiveType = PrimitiveType.Cube, localPosition = new Vector3(0f, 0.24f, 0f), localScale = new Vector3(0.35f, 0.24f, 0.2f), mass = 0.95f, lowAngularXLimit = -15f, highAngularXLimit = 25f, angularYLimit = 18f, angularZLimit = 10f, springForce = 480f, damperForce = 50f },
            new BoneDefinition { boneName = "Head", parentBoneName = "Chest", primitiveType = PrimitiveType.Sphere, localPosition = new Vector3(0f, 0.29f, 0f), localScale = new Vector3(0.18f, 0.18f, 0.18f), mass = 0.45f, lowAngularXLimit = -40f, highAngularXLimit = 55f, angularYLimit = 60f, angularZLimit = 25f, springForce = 350f, damperForce = 40f },

            // Arms: wide range for upper, constrained elbow (hinge-like)
            new BoneDefinition { boneName = "LeftUpperArm", parentBoneName = "Chest", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(-0.24f, 0.12f, 0f), localScale = new Vector3(0.11f, 0.24f, 0.11f), mass = 0.45f, lowAngularXLimit = -80f, highAngularXLimit = 80f, angularYLimit = 70f, angularZLimit = 45f, springForce = 300f, damperForce = 35f },
            new BoneDefinition { boneName = "LeftLowerArm", parentBoneName = "LeftUpperArm", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(-0.22f, 0f, 0f), localScale = new Vector3(0.09f, 0.23f, 0.09f), mass = 0.35f, lowAngularXLimit = -5f, highAngularXLimit = 140f, angularYLimit = 8f, angularZLimit = 8f, springForce = 280f, damperForce = 30f },
            new BoneDefinition { boneName = "LeftHand", parentBoneName = "LeftLowerArm", primitiveType = PrimitiveType.Sphere, localPosition = new Vector3(-0.18f, 0f, 0f), localScale = new Vector3(0.11f, 0.11f, 0.11f), mass = 0.2f, lowAngularXLimit = -50f, highAngularXLimit = 50f, angularYLimit = 20f, angularZLimit = 25f, springForce = 200f, damperForce = 25f },

            new BoneDefinition { boneName = "RightUpperArm", parentBoneName = "Chest", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(0.24f, 0.12f, 0f), localScale = new Vector3(0.11f, 0.24f, 0.11f), mass = 0.45f, lowAngularXLimit = -80f, highAngularXLimit = 80f, angularYLimit = 70f, angularZLimit = 45f, springForce = 300f, damperForce = 35f },
            new BoneDefinition { boneName = "RightLowerArm", parentBoneName = "RightUpperArm", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(0.22f, 0f, 0f), localScale = new Vector3(0.09f, 0.23f, 0.09f), mass = 0.35f, lowAngularXLimit = -5f, highAngularXLimit = 140f, angularYLimit = 8f, angularZLimit = 8f, springForce = 280f, damperForce = 30f },
            new BoneDefinition { boneName = "RightHand", parentBoneName = "RightLowerArm", primitiveType = PrimitiveType.Sphere, localPosition = new Vector3(0.18f, 0f, 0f), localScale = new Vector3(0.11f, 0.11f, 0.11f), mass = 0.2f, lowAngularXLimit = -50f, highAngularXLimit = 50f, angularYLimit = 20f, angularZLimit = 25f, springForce = 200f, damperForce = 25f },

            // Legs: constrained knees (hinge), reasonable hip range
            new BoneDefinition { boneName = "LeftUpperLeg", parentBoneName = "Hips", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(-0.1f, -0.26f, 0f), localScale = new Vector3(0.12f, 0.27f, 0.12f), mass = 0.8f, lowAngularXLimit = -80f, highAngularXLimit = 30f, angularYLimit = 25f, angularZLimit = 20f, springForce = 450f, damperForce = 50f },
            new BoneDefinition { boneName = "LeftLowerLeg", parentBoneName = "LeftUpperLeg", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(0f, -0.31f, 0f), localScale = new Vector3(0.11f, 0.27f, 0.11f), mass = 0.65f, lowAngularXLimit = -140f, highAngularXLimit = 5f, angularYLimit = 6f, angularZLimit = 6f, springForce = 420f, damperForce = 45f },
            new BoneDefinition { boneName = "LeftFoot", parentBoneName = "LeftLowerLeg", primitiveType = PrimitiveType.Cube, localPosition = new Vector3(0f, -0.28f, 0.08f), localScale = new Vector3(0.13f, 0.07f, 0.24f), mass = 0.35f, lowAngularXLimit = -30f, highAngularXLimit = 45f, angularYLimit = 12f, angularZLimit = 15f, springForce = 250f, damperForce = 30f },

            new BoneDefinition { boneName = "RightUpperLeg", parentBoneName = "Hips", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(0.1f, -0.26f, 0f), localScale = new Vector3(0.12f, 0.27f, 0.12f), mass = 0.8f, lowAngularXLimit = -80f, highAngularXLimit = 30f, angularYLimit = 25f, angularZLimit = 20f, springForce = 450f, damperForce = 50f },
            new BoneDefinition { boneName = "RightLowerLeg", parentBoneName = "RightUpperLeg", primitiveType = PrimitiveType.Capsule, localPosition = new Vector3(0f, -0.31f, 0f), localScale = new Vector3(0.11f, 0.27f, 0.11f), mass = 0.65f, lowAngularXLimit = -140f, highAngularXLimit = 5f, angularYLimit = 6f, angularZLimit = 6f, springForce = 420f, damperForce = 45f },
            new BoneDefinition { boneName = "RightFoot", parentBoneName = "RightLowerLeg", primitiveType = PrimitiveType.Cube, localPosition = new Vector3(0f, -0.28f, 0.08f), localScale = new Vector3(0.13f, 0.07f, 0.24f), mass = 0.35f, lowAngularXLimit = -30f, highAngularXLimit = 45f, angularYLimit = 12f, angularZLimit = 15f, springForce = 250f, damperForce = 30f }
        };
    }
}
