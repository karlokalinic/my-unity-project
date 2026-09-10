using System;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public sealed class PlayerHumanoidVisualDriver : MonoBehaviour
{
    [Serializable]
    private struct BoneMap
    {
        public string rigBoneName;
        public HumanBodyBones humanBone;

        public BoneMap(string rigBoneNameValue, HumanBodyBones humanBoneValue)
        {
            rigBoneName = rigBoneNameValue;
            humanBone = humanBoneValue;
        }
    }

    private sealed class BoneLink
    {
        public Transform targetBone;
        public Transform physicalBone;
        public Transform visualBone;
        public Quaternion targetBindActorRotation;
        public Quaternion physicalBindActorRotation;
        public Quaternion visualBindActorRotation;
        public Vector3 targetBindActorPosition;
        public Vector3 physicalBindActorPosition;
        public Vector3 visualBindActorPosition;
        public bool drivesPosition;
    }

    private static readonly BoneMap[] RequiredBoneMaps =
    {
        new BoneMap("Hips", HumanBodyBones.Hips),
        new BoneMap("Spine", HumanBodyBones.Spine),
        new BoneMap("Chest", HumanBodyBones.Chest),
        new BoneMap("Head", HumanBodyBones.Head),
        new BoneMap("LeftUpperArm", HumanBodyBones.LeftUpperArm),
        new BoneMap("LeftLowerArm", HumanBodyBones.LeftLowerArm),
        new BoneMap("LeftHand", HumanBodyBones.LeftHand),
        new BoneMap("RightUpperArm", HumanBodyBones.RightUpperArm),
        new BoneMap("RightLowerArm", HumanBodyBones.RightLowerArm),
        new BoneMap("RightHand", HumanBodyBones.RightHand),
        new BoneMap("LeftUpperLeg", HumanBodyBones.LeftUpperLeg),
        new BoneMap("LeftLowerLeg", HumanBodyBones.LeftLowerLeg),
        new BoneMap("LeftFoot", HumanBodyBones.LeftFoot),
        new BoneMap("RightUpperLeg", HumanBodyBones.RightUpperLeg),
        new BoneMap("RightLowerLeg", HumanBodyBones.RightLowerLeg),
        new BoneMap("RightFoot", HumanBodyBones.RightFoot)
    };

    [Header("References")]
    [SerializeField] private ProceduralHumanoidRig rig;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator humanoidAnimator;

    [Header("Skin Drive")]
    [SerializeField] private bool driveHumanoidSkin = true;
    [SerializeField] private float ragdollBlendSpeed = 14f;
    [SerializeField] private float poseRotationResponse = 28f;
    [SerializeField] private float pelvisPositionResponse = 26f;
    [SerializeField] private float unresolvedRetrySeconds = 1f;

    [Header("Rendering")]
    [SerializeField] private bool tuneCharacterMaterials = true;
    [SerializeField] private bool forceDynamicShadows = true;
    [SerializeField] private bool enableSkinnedMotionVectors = true;

    private BoneLink[] links = Array.Empty<BoneLink>();
    private bool resolved;
    private bool ragdollMode;
    private bool renderingTuned;
    private float ragdollBlend;
    private float nextResolveAttemptTime;
    private Transform visualHead;
    private MaterialPropertyBlock materialBlock;

    public bool IsHumanoidBound => resolved && humanoidAnimator != null && humanoidAnimator.avatar != null && humanoidAnimator.avatar.isValid && humanoidAnimator.avatar.isHuman;
    public Transform VisualHead => visualHead;
    public Animator HumanoidAnimator => humanoidAnimator;

    private void Awake()
    {
        ResolveNow();
    }

    private void OnEnable()
    {
        if (!resolved)
        {
            ResolveNow();
        }
    }

    private void LateUpdate()
    {
        if (!driveHumanoidSkin)
        {
            return;
        }

        if (!resolved)
        {
            if (Time.unscaledTime < nextResolveAttemptTime)
            {
                return;
            }

            ResolveNow();
            if (!resolved)
            {
                return;
            }
        }

        float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);
        float targetRagdollBlend = ragdollMode ? 1f : 0f;
        ragdollBlend = Mathf.MoveTowards(
            ragdollBlend,
            targetRagdollBlend,
            Mathf.Max(0.01f, ragdollBlendSpeed) * deltaTime);

        float rotationBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, poseRotationResponse) * deltaTime);
        float positionBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, pelvisPositionResponse) * deltaTime);

        Vector3 visualPelvisOffset = animationController != null && !ragdollMode
            ? animationController.VisualPelvisOffset
            : Vector3.zero;

        for (int i = 0; i < links.Length; i++)
        {
            BoneLink link = links[i];
            if (link == null || link.visualBone == null || link.targetBone == null || link.physicalBone == null)
            {
                continue;
            }

            Quaternion targetDesired = ResolveDesiredVisualRotation(
                link.targetBone,
                link.targetBindActorRotation,
                link.visualBindActorRotation);
            Quaternion physicalDesired = ResolveDesiredVisualRotation(
                link.physicalBone,
                link.physicalBindActorRotation,
                link.visualBindActorRotation);
            Quaternion desiredRotation = Quaternion.Slerp(targetDesired, physicalDesired, ragdollBlend);
            link.visualBone.rotation = Quaternion.Slerp(link.visualBone.rotation, desiredRotation, rotationBlend);

            if (!link.drivesPosition)
            {
                continue;
            }

            Vector3 targetDesiredPosition = ResolveDesiredVisualPosition(
                link.targetBone,
                link.targetBindActorPosition,
                link.visualBindActorPosition,
                visualPelvisOffset);
            Vector3 physicalDesiredPosition = ResolveDesiredVisualPosition(
                link.physicalBone,
                link.physicalBindActorPosition,
                link.visualBindActorPosition,
                Vector3.zero);
            Vector3 desiredPosition = Vector3.Lerp(targetDesiredPosition, physicalDesiredPosition, ragdollBlend);
            link.visualBone.position = Vector3.Lerp(link.visualBone.position, desiredPosition, positionBlend);
        }
    }

    public void SetRagdollMode(bool enabled)
    {
        ragdollMode = enabled;
        if (!resolved)
        {
            ResolveNow();
        }
    }

    public bool TryGetVisualBone(HumanBodyBones bone, out Transform result)
    {
        result = null;
        if (!resolved)
        {
            ResolveNow();
        }

        if (!IsHumanoidBound)
        {
            return false;
        }

        result = humanoidAnimator.GetBoneTransform(bone);
        return result != null;
    }

    public void ResolveNow()
    {
        resolved = false;
        visualHead = null;
        nextResolveAttemptTime = Time.unscaledTime + Mathf.Max(0.2f, unresolvedRetrySeconds);

        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }

        if (rig == null)
        {
            return;
        }

        rig.EnsureBuilt();

        if (visualRoot == null)
        {
            visualRoot = transform.Find("StoreModelVisual");
        }
        if (visualRoot == null)
        {
            return;
        }

        if (humanoidAnimator == null || !humanoidAnimator.transform.IsChildOf(visualRoot))
        {
            humanoidAnimator = visualRoot.GetComponentInChildren<Animator>(true);
        }
        if (humanoidAnimator == null || humanoidAnimator.avatar == null || !humanoidAnimator.avatar.isValid || !humanoidAnimator.avatar.isHuman)
        {
            return;
        }

        humanoidAnimator.applyRootMotion = false;
        humanoidAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        BoneLink[] builtLinks = new BoneLink[RequiredBoneMaps.Length];
        int resolvedCount = 0;
        for (int i = 0; i < RequiredBoneMaps.Length; i++)
        {
            BoneMap map = RequiredBoneMaps[i];
            Transform target = rig.GetBone(map.rigBoneName, true);
            Transform physical = rig.GetBone(map.rigBoneName, false);
            Transform visual = ResolveVisualBone(map.humanBone, map.rigBoneName);
            if (target == null || physical == null || visual == null)
            {
                continue;
            }

            builtLinks[i] = new BoneLink
            {
                targetBone = target,
                physicalBone = physical,
                visualBone = visual,
                targetBindActorRotation = ToActorLocalRotation(target.rotation),
                physicalBindActorRotation = ToActorLocalRotation(physical.rotation),
                visualBindActorRotation = ToActorLocalRotation(visual.rotation),
                targetBindActorPosition = transform.InverseTransformPoint(target.position),
                physicalBindActorPosition = transform.InverseTransformPoint(physical.position),
                visualBindActorPosition = transform.InverseTransformPoint(visual.position),
                drivesPosition = string.Equals(map.rigBoneName, "Hips", StringComparison.OrdinalIgnoreCase)
            };

            resolvedCount++;
            if (map.humanBone == HumanBodyBones.Head)
            {
                visualHead = visual;
            }
        }

        links = builtLinks;
        resolved = resolvedCount >= 14 && visualHead != null;
        if (resolved)
        {
            nextResolveAttemptTime = float.PositiveInfinity;
        }

        if (resolved && tuneCharacterMaterials && !renderingTuned)
        {
            TuneCharacterRendering();
            renderingTuned = true;
        }
    }

    private Transform ResolveVisualBone(HumanBodyBones humanBone, string rigBoneName)
    {
        Transform bone = humanoidAnimator.GetBoneTransform(humanBone);
        if (bone != null)
        {
            return bone;
        }

        if (string.Equals(rigBoneName, "Chest", StringComparison.OrdinalIgnoreCase))
        {
            bone = humanoidAnimator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (bone != null)
            {
                return bone;
            }
        }

        return null;
    }

    private Quaternion ResolveDesiredVisualRotation(
        Transform sourceBone,
        Quaternion sourceBindActorRotation,
        Quaternion visualBindActorRotation)
    {
        Quaternion sourceActorRotation = ToActorLocalRotation(sourceBone.rotation);
        Quaternion actorSpaceDelta = sourceActorRotation * Quaternion.Inverse(sourceBindActorRotation);
        Quaternion desiredActorRotation = actorSpaceDelta * visualBindActorRotation;
        return transform.rotation * desiredActorRotation;
    }

    private Vector3 ResolveDesiredVisualPosition(
        Transform sourceBone,
        Vector3 sourceBindActorPosition,
        Vector3 visualBindActorPosition,
        Vector3 actorSpaceSecondaryOffset)
    {
        Vector3 sourceActorPosition = transform.InverseTransformPoint(sourceBone.position);
        Vector3 sourceDelta = sourceActorPosition - sourceBindActorPosition;
        Vector3 desiredActorPosition = visualBindActorPosition + sourceDelta + actorSpaceSecondaryOffset;
        return transform.TransformPoint(desiredActorPosition);
    }

    private Quaternion ToActorLocalRotation(Quaternion worldRotation)
    {
        return Quaternion.Inverse(transform.rotation) * worldRotation;
    }

    private void TuneCharacterRendering()
    {
        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        if (materialBlock == null)
        {
            materialBlock = new MaterialPropertyBlock();
        }

        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer rendererComponent = renderers[rendererIndex];
            if (rendererComponent == null)
            {
                continue;
            }

            rendererComponent.receiveShadows = true;
            rendererComponent.allowOcclusionWhenDynamic = true;
            if (forceDynamicShadows)
            {
                rendererComponent.shadowCastingMode = ShadowCastingMode.On;
            }
            rendererComponent.motionVectorGenerationMode = MotionVectorGenerationMode.Object;

            SkinnedMeshRenderer skinned = rendererComponent as SkinnedMeshRenderer;
            if (skinned != null)
            {
                skinned.quality = SkinQuality.Auto;
                skinned.updateWhenOffscreen = false;
                skinned.skinnedMotionVectors = enableSkinnedMotionVectors;
            }

            Material[] materials = rendererComponent.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material material = materials[materialIndex];
                if (material == null)
                {
                    continue;
                }

                string lowerName = material.name.ToLowerInvariant();
                float smoothness = ResolveMaterialSmoothness(lowerName);

                materialBlock.Clear();
                rendererComponent.GetPropertyBlock(materialBlock, materialIndex);
                if (material.HasProperty("_Smoothness"))
                {
                    materialBlock.SetFloat("_Smoothness", smoothness);
                }
                if (material.HasProperty("_Glossiness"))
                {
                    materialBlock.SetFloat("_Glossiness", smoothness);
                }
                if (material.HasProperty("_Metallic"))
                {
                    materialBlock.SetFloat("_Metallic", IsMetalMaterial(lowerName) ? 0.55f : 0f);
                }
                if (material.HasProperty("_OcclusionStrength"))
                {
                    materialBlock.SetFloat("_OcclusionStrength", 1f);
                }
                if (material.HasProperty("_BumpScale"))
                {
                    materialBlock.SetFloat("_BumpScale", 1f);
                }

                rendererComponent.SetPropertyBlock(materialBlock, materialIndex);
            }
        }
    }

    private static float ResolveMaterialSmoothness(string lowerName)
    {
        if (ContainsAny(lowerName, "eye", "cornea"))
        {
            return 0.78f;
        }
        if (ContainsAny(lowerName, "skin", "face", "head", "body"))
        {
            return 0.38f;
        }
        if (ContainsAny(lowerName, "leather", "boot", "shoe", "belt"))
        {
            return 0.32f;
        }
        if (ContainsAny(lowerName, "hair", "brow", "lash"))
        {
            return 0.2f;
        }
        if (ContainsAny(lowerName, "cloth", "shirt", "pants", "trouser", "jacket", "coat", "fabric"))
        {
            return 0.15f;
        }

        return 0.24f;
    }

    private static bool IsMetalMaterial(string lowerName)
    {
        return ContainsAny(lowerName, "metal", "buckle", "zipper", "button", "blade", "knife");
    }

    private static bool ContainsAny(string value, params string[] tokens)
    {
        if (string.IsNullOrEmpty(value) || tokens == null)
        {
            return false;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            if (!string.IsNullOrEmpty(tokens[i]) && value.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
