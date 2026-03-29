using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SampleSceneSourceModelIdlePose : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Transform sourceRoot;

    [Header("Motion")]
    [SerializeField] private float blendSpeed = 9f;
    [SerializeField] private float idleCycleSpeed = 0.95f;
    [SerializeField] private float walkCycleSpeed = 6.3f;
    [SerializeField] private float runCycleSpeed = 9.2f;
    [SerializeField] private float fullWalkSpeed = 3.8f;
    [SerializeField] private float runSpeedThreshold = 4.9f;
    [SerializeField] private float movementDeadZone = 0.18f;
    [SerializeField] private float feetGroundOffset = 0.015f;
    [SerializeField] private bool neutralMode = false;
    [SerializeField] private float neutralSwayAmplitude = 1.6f;
    [SerializeField] private float neutralSwayFrequency = 0.55f;

    private readonly Dictionary<string, int> muscleIndexCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Transform, Quaternion> genericBaseRotations = new();
    private readonly Dictionary<string, Transform> genericBones = new(StringComparer.OrdinalIgnoreCase);

    private HumanPoseHandler poseHandler;
    private HumanPose humanPose;
    private Transform poseRoot;
    private Animator sourceAnimator;
    private PlayerMover playerMover;
    private Rigidbody body;
    private float phase;
    private bool genericRigCached;
    private bool neutralBaseCaptured;
    private Quaternion neutralBaseLocalRotation;

    public void Configure(Transform configuredSourceRoot)
    {
        if (sourceRoot == configuredSourceRoot)
        {
            return;
        }

        sourceRoot = configuredSourceRoot;
        poseHandler = null;
        poseRoot = null;
        sourceAnimator = null;
        muscleIndexCache.Clear();
        ResetGenericRig();
        phase = 0f;
        neutralBaseCaptured = false;
        neutralBaseLocalRotation = Quaternion.identity;
        CacheMotionSources();
    }

    public void SetNeutralMode(bool enabled)
    {
        neutralMode = enabled;
    }

    private void Awake()
    {
        CacheMotionSources();
    }

    private void LateUpdate()
    {
        if (sourceRoot == null)
        {
            return;
        }

        if (neutralMode)
        {
            float dtNeutral = Mathf.Max(0.0001f, Time.deltaTime);
            phase += dtNeutral * 0.65f;
            DisableSourceAnimators();
            CaptureNeutralBaseRotation();
            if (EnsureHumanoidPose())
            {
                ApplyNeutralHumanoidStandPose(dtNeutral);
            }
            else
            {
                ApplyNeutralGenericStandPose(dtNeutral);
            }

            ApplyNeutralStandMotion(dtNeutral);
            RegroundVisualToActorBottom();
            return;
        }

        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        float speed = ResolveMovementSpeed();
        float locomotionSpeed = Mathf.Max(0f, speed - Mathf.Max(0f, movementDeadZone));
        float moveWeight = Mathf.Clamp01(locomotionSpeed / Mathf.Max(0.2f, fullWalkSpeed));
        if (moveWeight < 0.02f)
        {
            moveWeight = 0f;
        }

        float runWeight = Mathf.Clamp01((speed - runSpeedThreshold) / 1.7f);
        phase += dt * Mathf.Lerp(idleCycleSpeed, Mathf.Lerp(walkCycleSpeed, runCycleSpeed, runWeight), moveWeight);

        if (EnsureHumanoidPose())
        {
            DisableSourceAnimators();
            ApplyHumanoidPose(moveWeight, runWeight, dt);
            RegroundVisualToActorBottom();
            return;
        }

        DisableSourceAnimators();
        ApplyGenericPose(moveWeight, dt);
        RegroundVisualToActorBottom();
    }

    private void CacheMotionSources()
    {
        playerMover = GetComponent<PlayerMover>();
        body = GetComponent<Rigidbody>();
    }

    private float ResolveMovementSpeed()
    {
        if (playerMover != null)
        {
            return playerMover.CurrentPlanarSpeed;
        }

        if (body != null)
        {
            Vector3 planarVelocity = body.linearVelocity;
            planarVelocity.y = 0f;
            return planarVelocity.magnitude;
        }

        return 0f;
    }

    private bool EnsureHumanoidPose()
    {
        Animator[] animators = sourceRoot.GetComponentsInChildren<Animator>(true);
        sourceAnimator = ResolvePrimaryAnimator(animators, sourceRoot);
        if (sourceAnimator == null ||
            sourceAnimator.avatar == null ||
            !sourceAnimator.avatar.isValid ||
            !sourceAnimator.isHuman)
        {
            return false;
        }

        if (poseHandler == null || poseRoot != sourceAnimator.transform)
        {
            poseHandler = new HumanPoseHandler(sourceAnimator.avatar, sourceAnimator.transform);
            poseRoot = sourceAnimator.transform;
            muscleIndexCache.Clear();
            humanPose = new HumanPose
            {
                muscles = new float[HumanTrait.MuscleCount]
            };
        }

        if (humanPose.muscles == null || humanPose.muscles.Length != HumanTrait.MuscleCount)
        {
            humanPose.muscles = new float[HumanTrait.MuscleCount];
        }

        return true;
    }

    private static Animator ResolvePrimaryAnimator(Animator[] candidates, Transform preferredRoot)
    {
        if (candidates == null || candidates.Length == 0)
        {
            return null;
        }

        Animator best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < candidates.Length; i++)
        {
            Animator candidate = candidates[i];
            if (candidate == null)
            {
                continue;
            }

            int score = ScoreAnimator(candidate, preferredRoot);
            if (score <= bestScore)
            {
                continue;
            }

            bestScore = score;
            best = candidate;
        }

        return best;
    }

    private static int ScoreAnimator(Animator animator, Transform preferredRoot)
    {
        if (animator == null)
        {
            return int.MinValue;
        }

        int score = 0;
        if (animator.transform == preferredRoot)
        {
            score += 1200;
        }

        if (animator.avatar != null && animator.avatar.isValid)
        {
            score += animator.isHuman ? 420 : 160;
        }

        if (animator.runtimeAnimatorController != null)
        {
            score += 120;
        }

        SkinnedMeshRenderer[] skinned = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        score += Mathf.Min(skinned.Length, 64) * 12;

        int depth = ResolveDepthFromRoot(preferredRoot, animator.transform);
        if (depth >= 0)
        {
            score += Mathf.Max(0, 80 - (depth * 7));
        }

        if (!animator.gameObject.activeInHierarchy)
        {
            score -= 80;
        }

        return score;
    }

    private static int ResolveDepthFromRoot(Transform root, Transform target)
    {
        if (root == null || target == null)
        {
            return -1;
        }

        int depth = 0;
        Transform walker = target;
        while (walker != null)
        {
            if (walker == root)
            {
                return depth;
            }

            depth++;
            walker = walker.parent;
        }

        return -1;
    }

    private void DisableSourceAnimators()
    {
        Animator[] animators = sourceRoot.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
            {
                continue;
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.enabled = false;
        }
    }

    private void ApplyHumanoidPose(float moveWeight, float runWeight, float dt)
    {
        poseHandler.GetHumanPose(ref humanPose);
        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, blendSpeed) * dt);

        for (int i = 0; i < humanPose.muscles.Length; i++)
        {
            humanPose.muscles[i] = Mathf.Lerp(humanPose.muscles[i], 0f, blend);
        }

        float gait = Mathf.Sin(phase);
        float gaitCos = Mathf.Cos(phase);
        float breath = Mathf.Sin(phase * 0.52f);
        float idleLookYaw = Mathf.Sin(phase * 0.23f + 0.7f) * 0.07f * (1f - moveWeight);
        float idleLookPitch = Mathf.Sin(phase * 0.19f + 1.4f) * 0.04f * (1f - moveWeight);

        AddMuscle("Spine Front-Back", 0.02f + breath * 0.025f, blend);
        AddMuscle("Chest Front-Back", 0.03f + breath * 0.03f, blend);
        AddMuscle("Neck Nod Down-Up", idleLookPitch, blend);
        AddMuscle("Head Nod Down-Up", idleLookPitch * 0.7f, blend);
        AddMuscle("Neck Left-Right", idleLookYaw, blend);
        AddMuscle("Head Left-Right", idleLookYaw * 0.7f, blend);

        float armSwing = Mathf.Lerp(0.08f, 0.22f, runWeight) * moveWeight;
        float armDownBase = -0.55f + breath * 0.01f;
        SetMuscle("Left Arm Down-Up", armDownBase + (gait * armSwing * 0.18f), blend);
        SetMuscle("Right Arm Down-Up", armDownBase - (gait * armSwing * 0.18f), blend);
        SetMuscle("Left Arm Front-Back", -(gait * armSwing), blend);
        SetMuscle("Right Arm Front-Back", gait * armSwing, blend);
        SetMuscle("Left Forearm Stretch", 0.16f * (1f - (moveWeight * 0.4f)), blend);
        SetMuscle("Right Forearm Stretch", 0.16f * (1f - (moveWeight * 0.4f)), blend);
        SetMuscle("Left Hand Down-Up", breath * 0.015f, blend);
        SetMuscle("Right Hand Down-Up", breath * 0.015f, blend);

        float legSwing = Mathf.Lerp(0.12f, 0.28f, runWeight) * moveWeight;
        float leftKneeDrive = Mathf.Max(0f, -gait) * (0.22f + 0.1f * runWeight) * moveWeight;
        float rightKneeDrive = Mathf.Max(0f, gait) * (0.22f + 0.1f * runWeight) * moveWeight;

        float hipStandBase = -0.18f;
        SetMuscle("Left Upper Leg Front-Back", hipStandBase + (gait * legSwing), blend);
        SetMuscle("Right Upper Leg Front-Back", hipStandBase - (gait * legSwing), blend);
        SetMuscle("Left Upper Leg In-Out", 0.008f * moveWeight, blend);
        SetMuscle("Right Upper Leg In-Out", -0.008f * moveWeight, blend);
        SetMuscle("Left Lower Leg Stretch", Mathf.Clamp(0.92f - leftKneeDrive, -1f, 1f), blend);
        SetMuscle("Right Lower Leg Stretch", Mathf.Clamp(0.92f - rightKneeDrive, -1f, 1f), blend);
        SetMuscle("Left Foot Up-Down", -gait * 0.08f * moveWeight, blend);
        SetMuscle("Right Foot Up-Down", gait * 0.08f * moveWeight, blend);
        SetMuscle("Left Foot Twist In-Out", gaitCos * 0.05f * moveWeight, blend);
        SetMuscle("Right Foot Twist In-Out", -gaitCos * 0.05f * moveWeight, blend);

        ApplyFingerCurl(0.01f + breath * 0.01f, blend);
        poseHandler.SetHumanPose(ref humanPose);
    }

    private void ApplyFingerCurl(float curl, float blend)
    {
        SetMuscle("Left Thumb 1 Stretched", curl, blend);
        SetMuscle("Left Index 1 Stretched", curl, blend);
        SetMuscle("Left Middle 1 Stretched", curl, blend);
        SetMuscle("Left Ring 1 Stretched", curl, blend);
        SetMuscle("Left Little 1 Stretched", curl, blend);
        SetMuscle("Right Thumb 1 Stretched", curl, blend);
        SetMuscle("Right Index 1 Stretched", curl, blend);
        SetMuscle("Right Middle 1 Stretched", curl, blend);
        SetMuscle("Right Ring 1 Stretched", curl, blend);
        SetMuscle("Right Little 1 Stretched", curl, blend);
    }

    private void SetMuscle(string muscleName, float targetValue, float blend)
    {
        int index = ResolveMuscleIndex(muscleName);
        if (index < 0 || index >= humanPose.muscles.Length)
        {
            return;
        }

        float clamped = Mathf.Clamp(targetValue, -1f, 1f);
        humanPose.muscles[index] = Mathf.Lerp(humanPose.muscles[index], clamped, blend);
    }

    private void AddMuscle(string muscleName, float additiveValue, float blend)
    {
        int index = ResolveMuscleIndex(muscleName);
        if (index < 0 || index >= humanPose.muscles.Length)
        {
            return;
        }

        float current = humanPose.muscles[index];
        float target = Mathf.Clamp(current + additiveValue, -1f, 1f);
        humanPose.muscles[index] = Mathf.Lerp(current, target, blend);
    }

    private int ResolveMuscleIndex(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return -1;
        }

        if (muscleIndexCache.TryGetValue(query, out int cached))
        {
            return cached;
        }

        string normalizedQuery = Normalize(query);
        for (int i = 0; i < HumanTrait.MuscleCount; i++)
        {
            string normalizedCandidate = Normalize(HumanTrait.MuscleName[i]);
            if (normalizedCandidate == normalizedQuery ||
                normalizedCandidate.Contains(normalizedQuery) ||
                normalizedQuery.Contains(normalizedCandidate))
            {
                muscleIndexCache[query] = i;
                return i;
            }
        }

        muscleIndexCache[query] = -1;
        return -1;
    }

    private void ApplyGenericPose(float moveWeight, float dt)
    {
        if (!EnsureGenericRig())
        {
            return;
        }

        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, blendSpeed) * dt);
        float gait = Mathf.Sin(phase);
        float breath = Mathf.Sin(phase * 0.5f);

        ApplyGeneric("Spine", Quaternion.Euler(-1.5f + breath * 0.6f, 0f, 0f), blend);
        ApplyGeneric("Chest", Quaternion.Euler(0.8f + breath * 0.8f, 0f, 0f), blend);
        ApplyGeneric("LeftUpperArm", Quaternion.Euler(-22f - gait * 9f * moveWeight, 0f, 8f), blend);
        ApplyGeneric("RightUpperArm", Quaternion.Euler(-22f + gait * 9f * moveWeight, 0f, -8f), blend);
        ApplyGeneric("LeftForearm", Quaternion.Euler(-8f + (gait * 1.5f * moveWeight), 0f, 0f), blend);
        ApplyGeneric("RightForearm", Quaternion.Euler(-8f - (gait * 1.5f * moveWeight), 0f, 0f), blend);
        ApplyGeneric("LeftHand", Quaternion.Euler(0f, 0f, 0f), blend);
        ApplyGeneric("RightHand", Quaternion.Euler(0f, 0f, 0f), blend);
        ApplyGeneric("LeftUpperLeg", Quaternion.Euler(-20f + (gait * 8f * moveWeight), 0f, 0f), blend);
        ApplyGeneric("RightUpperLeg", Quaternion.Euler(-20f - (gait * 8f * moveWeight), 0f, 0f), blend);
        ApplyGeneric("LeftLowerLeg", Quaternion.Euler(22f + Mathf.Max(0f, -gait) * 5f * moveWeight, 0f, 0f), blend);
        ApplyGeneric("RightLowerLeg", Quaternion.Euler(22f + Mathf.Max(0f, gait) * 5f * moveWeight, 0f, 0f), blend);
        ApplyGeneric("LeftFoot", Quaternion.Euler(-4f - gait * 1.5f * moveWeight, 0f, 0f), blend);
        ApplyGeneric("RightFoot", Quaternion.Euler(-4f + gait * 1.5f * moveWeight, 0f, 0f), blend);
    }

    private void ApplyNeutralHumanoidStandPose(float dt)
    {
        if (poseHandler == null)
        {
            return;
        }

        poseHandler.GetHumanPose(ref humanPose);
        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, blendSpeed) * dt);
        float breath = Mathf.Sin(phase * 0.5f);

        for (int i = 0; i < humanPose.muscles.Length; i++)
        {
            humanPose.muscles[i] = Mathf.Lerp(humanPose.muscles[i], 0f, blend);
        }

        AddMuscle("Spine Front-Back", 0.015f + breath * 0.012f, blend);
        AddMuscle("Chest Front-Back", 0.02f + breath * 0.016f, blend);
        SetMuscle("Left Arm Down-Up", -0.72f, blend);
        SetMuscle("Right Arm Down-Up", -0.72f, blend);
        SetMuscle("Left Arm Front-Back", 0f, blend);
        SetMuscle("Right Arm Front-Back", 0f, blend);
        SetMuscle("Left Forearm Stretch", 0.04f, blend);
        SetMuscle("Right Forearm Stretch", 0.04f, blend);
        SetMuscle("Left Upper Leg Front-Back", 0f, blend);
        SetMuscle("Right Upper Leg Front-Back", 0f, blend);
        SetMuscle("Left Lower Leg Stretch", 0.98f, blend);
        SetMuscle("Right Lower Leg Stretch", 0.98f, blend);
        SetMuscle("Left Foot Up-Down", 0f, blend);
        SetMuscle("Right Foot Up-Down", 0f, blend);

        poseHandler.SetHumanPose(ref humanPose);
    }

    private void ApplyNeutralGenericStandPose(float dt)
    {
        if (!EnsureGenericRig())
        {
            return;
        }

        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, blendSpeed) * dt);
        float breath = Mathf.Sin(phase * 0.5f);

        ApplyGeneric("Spine", Quaternion.Euler(1.0f + breath * 0.6f, 0f, 0f), blend);
        ApplyGeneric("Chest", Quaternion.Euler(1.6f + breath * 0.8f, 0f, 0f), blend);
        ApplyGeneric("LeftUpperArm", Quaternion.Euler(0f, 0f, 62f), blend);
        ApplyGeneric("RightUpperArm", Quaternion.Euler(0f, 0f, -62f), blend);
        ApplyGeneric("LeftForearm", Quaternion.Euler(2f, 0f, 0f), blend);
        ApplyGeneric("RightForearm", Quaternion.Euler(2f, 0f, 0f), blend);
        ApplyGeneric("LeftUpperLeg", Quaternion.Euler(0f, 0f, 0f), blend);
        ApplyGeneric("RightUpperLeg", Quaternion.Euler(0f, 0f, 0f), blend);
        ApplyGeneric("LeftLowerLeg", Quaternion.Euler(3f, 0f, 0f), blend);
        ApplyGeneric("RightLowerLeg", Quaternion.Euler(3f, 0f, 0f), blend);
        ApplyGeneric("LeftFoot", Quaternion.Euler(0f, 0f, 0f), blend);
        ApplyGeneric("RightFoot", Quaternion.Euler(0f, 0f, 0f), blend);
    }

    private void CaptureNeutralBaseRotation()
    {
        if (neutralBaseCaptured || sourceRoot == null)
        {
            return;
        }

        neutralBaseLocalRotation = sourceRoot.localRotation;
        neutralBaseCaptured = true;
    }

    private void ApplyNeutralStandMotion(float dt)
    {
        if (sourceRoot == null)
        {
            return;
        }

        float breath = Mathf.Sin(phase * 0.48f);
        float sway = Mathf.Sin(phase * neutralSwayFrequency) * neutralSwayAmplitude;
        Quaternion target = neutralBaseLocalRotation * Quaternion.Euler(0f, sway, breath * 0.45f);
        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, blendSpeed) * dt);
        sourceRoot.localRotation = Quaternion.Slerp(sourceRoot.localRotation, target, blend);
    }

    private bool EnsureGenericRig()
    {
        if (genericRigCached)
        {
            return genericBones.ContainsKey("LeftUpperArm") && genericBones.ContainsKey("RightUpperArm");
        }

        genericRigCached = true;
        genericBones.Clear();
        genericBaseRotations.Clear();

        Transform[] transforms = sourceRoot.GetComponentsInChildren<Transform>(true);
        CaptureGenericBone("Spine", transforms, new[] { "spine", "torso" }, null, new[] { "ik", "target", "twist" });
        CaptureGenericBone("Chest", transforms, new[] { "chest", "spine2", "spine_02" }, null, new[] { "ik", "target", "twist" });
        CaptureGenericBone("LeftUpperArm", transforms, new[] { "upperarm", "upper_arm", "arm" }, new[] { "left", ".l", "_l", "mixamorig:left" }, new[] { "fore", "lower", "hand", "ik", "target", "twist" });
        CaptureGenericBone("RightUpperArm", transforms, new[] { "upperarm", "upper_arm", "arm" }, new[] { "right", ".r", "_r", "mixamorig:right" }, new[] { "fore", "lower", "hand", "ik", "target", "twist" });
        CaptureGenericBone("LeftForearm", transforms, new[] { "forearm", "lowerarm", "lower_arm" }, new[] { "left", ".l", "_l", "mixamorig:left" }, new[] { "hand", "ik", "target", "twist" });
        CaptureGenericBone("RightForearm", transforms, new[] { "forearm", "lowerarm", "lower_arm" }, new[] { "right", ".r", "_r", "mixamorig:right" }, new[] { "hand", "ik", "target", "twist" });
        CaptureGenericBone("LeftHand", transforms, new[] { "hand" }, new[] { "left", ".l", "_l", "mixamorig:left" }, new[] { "thumb", "index", "middle", "ring", "little", "ik", "target" });
        CaptureGenericBone("RightHand", transforms, new[] { "hand" }, new[] { "right", ".r", "_r", "mixamorig:right" }, new[] { "thumb", "index", "middle", "ring", "little", "ik", "target" });
        CaptureGenericBone("LeftUpperLeg", transforms, new[] { "upleg", "upperleg", "thigh" }, new[] { "left", ".l", "_l", "mixamorig:left" }, new[] { "ik", "target", "twist" });
        CaptureGenericBone("RightUpperLeg", transforms, new[] { "upleg", "upperleg", "thigh" }, new[] { "right", ".r", "_r", "mixamorig:right" }, new[] { "ik", "target", "twist" });
        CaptureGenericBone("LeftLowerLeg", transforms, new[] { "lowerleg", "calf", "shin", "leg" }, new[] { "left", ".l", "_l", "mixamorig:left" }, new[] { "up", "upper", "ik", "target", "twist" });
        CaptureGenericBone("RightLowerLeg", transforms, new[] { "lowerleg", "calf", "shin", "leg" }, new[] { "right", ".r", "_r", "mixamorig:right" }, new[] { "up", "upper", "ik", "target", "twist" });
        CaptureGenericBone("LeftFoot", transforms, new[] { "foot" }, new[] { "left", ".l", "_l", "mixamorig:left" }, new[] { "toe", "ik", "target" });
        CaptureGenericBone("RightFoot", transforms, new[] { "foot" }, new[] { "right", ".r", "_r", "mixamorig:right" }, new[] { "toe", "ik", "target" });

        return genericBones.ContainsKey("LeftUpperArm") && genericBones.ContainsKey("RightUpperArm");
    }

    private void CaptureGenericBone(string key, Transform[] transforms, string[] includeTokens, string[] sideTokens, string[] excludeTokens)
    {
        Transform bone = FindBestBone(transforms, includeTokens, sideTokens, excludeTokens);
        if (bone == null)
        {
            return;
        }

        genericBones[key] = bone;
        genericBaseRotations[bone] = bone.localRotation;
    }

    private void ApplyGeneric(string key, Quaternion additive, float blend)
    {
        if (!genericBones.TryGetValue(key, out Transform bone) ||
            bone == null ||
            !genericBaseRotations.TryGetValue(bone, out Quaternion baseRotation))
        {
            return;
        }

        Quaternion target = baseRotation * additive;
        bone.localRotation = Quaternion.Slerp(bone.localRotation, target, blend);
    }

    private static Transform FindBestBone(Transform[] transforms, string[] includeTokens, string[] sideTokens, string[] excludeTokens)
    {
        Transform best = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null)
            {
                continue;
            }

            string candidateName = Normalize(candidate.name);
            if (ContainsAny(candidateName, excludeTokens))
            {
                continue;
            }

            int includeScore = 0;
            for (int tokenIndex = 0; tokenIndex < includeTokens.Length; tokenIndex++)
            {
                if (candidateName.Contains(Normalize(includeTokens[tokenIndex])))
                {
                    includeScore += 4;
                }
            }

            if (includeScore == 0)
            {
                continue;
            }

            if (sideTokens != null && sideTokens.Length > 0)
            {
                includeScore += ContainsAny(candidateName, sideTokens) ? 5 : -3;
            }

            int depth = 0;
            Transform walker = candidate;
            while (walker != null)
            {
                depth++;
                walker = walker.parent;
            }

            includeScore -= depth;
            if (includeScore > bestScore)
            {
                bestScore = includeScore;
                best = candidate;
            }
        }

        return best;
    }

    private void ResetGenericRig()
    {
        genericRigCached = false;
        genericBones.Clear();
        genericBaseRotations.Clear();
    }

    private static bool ContainsAny(string value, string[] tokens)
    {
        if (tokens == null)
        {
            return false;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = Normalize(tokens[i]);
            if (!string.IsNullOrEmpty(token) && value.Contains(token))
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty)
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .ToLowerInvariant();
    }

    private void RegroundVisualToActorBottom()
    {
        if (sourceRoot == null)
        {
            return;
        }

        if (!TryGetVisualFeetY(out float feetY))
        {
            return;
        }

        float actorBottom = ResolveActorBottomY();
        float targetFeetY = actorBottom + Mathf.Max(0f, feetGroundOffset);
        float delta = targetFeetY - feetY;
        if (Mathf.Abs(delta) < 0.0005f)
        {
            return;
        }

        sourceRoot.position += new Vector3(0f, delta, 0f);
    }

    private bool TryGetVisualFeetY(out float feetY)
    {
        feetY = 0f;

        if (sourceAnimator != null &&
            sourceAnimator.avatar != null &&
            sourceAnimator.avatar.isValid &&
            sourceAnimator.isHuman)
        {
            Transform leftFoot = sourceAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = sourceAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
            float minFoot = float.PositiveInfinity;
            if (leftFoot != null)
            {
                minFoot = Mathf.Min(minFoot, leftFoot.position.y);
            }

            if (rightFoot != null)
            {
                minFoot = Mathf.Min(minFoot, rightFoot.position.y);
            }

            if (!float.IsInfinity(minFoot))
            {
                feetY = minFoot;
                return true;
            }
        }

        Renderer[] renderers = sourceRoot.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;
        float minY = 0f;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            string name = renderer.name.ToLowerInvariant();
            if (name.Contains("weapon") || name.Contains("dagger") || name.Contains("sword"))
            {
                continue;
            }

            float rendererMinY = renderer.bounds.min.y;
            if (!initialized || rendererMinY < minY)
            {
                minY = rendererMinY;
                initialized = true;
            }
        }

        if (!initialized)
        {
            return false;
        }

        feetY = minY;
        return true;
    }

    private float ResolveActorBottomY()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            return transform.position.y + controller.center.y - (controller.height * 0.5f);
        }

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            return transform.position.y + capsule.center.y - (capsule.height * 0.5f);
        }

        Collider colliderComponent = GetComponent<Collider>();
        if (colliderComponent != null)
        {
            return colliderComponent.bounds.min.y;
        }

        return transform.position.y;
    }
}
