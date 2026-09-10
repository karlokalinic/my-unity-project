using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public class PlayerAnimationController : MonoBehaviour
{
    private const float TwoPi = Mathf.PI * 2f;

    private enum MotionState
    {
        Idle = 0,
        Walk = 1,
        Sprint = 2,
        Reach = 3
    }

    [Header("References")]
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private ProceduralHumanoidRig rig;
    [SerializeField] private Animator animator;

    [Header("Locomotion Timing")]
    [SerializeField] private float walkStrideLength = 1.25f;
    [SerializeField] private float sprintStrideLength = 1.62f;
    [SerializeField] private float locomotionBlendSpeed = 7.5f;
    [SerializeField] private float gaitPhaseResponse = 12f;

    [Header("Leg Motion")]
    [SerializeField] private float walkLegSwing = 29f;
    [SerializeField] private float sprintLegSwing = 43f;
    [SerializeField] private float walkKneeFlex = 39f;
    [SerializeField] private float sprintKneeFlex = 61f;
    [SerializeField] private float footPitch = 11f;
    [SerializeField] private float footRoll = 3.2f;

    [Header("Upper Body Motion")]
    [SerializeField] private float walkArmSwing = 24f;
    [SerializeField] private float sprintArmSwing = 36f;
    [SerializeField] private float relaxedElbowFlex = 8f;
    [SerializeField] private float sprintElbowFlex = 19f;
    [SerializeField] private float pelvisYaw = 4.2f;
    [SerializeField] private float pelvisRoll = 2.4f;
    [SerializeField] private float spineCounterYaw = 2.8f;
    [SerializeField] private float chestCounterYaw = 5.2f;
    [SerializeField] private float headCounterYaw = 1.6f;
    [SerializeField] private float accelerationLean = 0.19f;
    [SerializeField] private float speedLean = 4.2f;
    [SerializeField] private float strafeLean = 3.3f;
    [SerializeField] private float turnLean = 0.018f;
    [SerializeField] private float dynamicsResponse = 9f;

    [Header("Pelvis Secondary Motion")]
    [SerializeField] private float walkPelvisBob = 0.018f;
    [SerializeField] private float sprintPelvisBob = 0.032f;
    [SerializeField] private float pelvisSway = 0.018f;
    [SerializeField] private float pelvisForwardTravel = 0.008f;

    [Header("Idle Life")]
    [SerializeField] private float breathingRate = 0.24f;
    [SerializeField] private float breathingChestPitch = 0.9f;
    [SerializeField] private float breathingSpinePitch = 0.35f;
    [SerializeField] private float idleHeadSway = 0.35f;
    [SerializeField] private float idleWeightShift = 0.45f;

    [Header("Pose Response")]
    [SerializeField] private float poseLerpSpeed = 16f;

    [Header("Reach")]
    [SerializeField] private float reachUpperArmPitch = -42f;
    [SerializeField] private float reachLowerArmPitch = -64f;
    [SerializeField] private float reachHandPitch = 10f;
    [SerializeField] private float reachInfluenceSpeed = 7f;

    [Header("Fire Feedback")]
    [SerializeField] private float fireKickDuration = 0.12f;
    [SerializeField] private float fireKickPitch = 12f;
    [SerializeField] private float fireKickYaw = 2f;
    [SerializeField] private float fireKickRoll = 5f;

    private readonly Dictionary<string, Quaternion> baseLocalRotations = new Dictionary<string, Quaternion>();
    private readonly Dictionary<string, Vector3> baseLocalPositions = new Dictionary<string, Vector3>();

    private bool bonesCached;
    private bool animatorParametersCached;
    private bool hasMoveSpeedParameter;
    private bool hasSprintParameter;
    private bool hasReachParameter;
    private bool hasStateParameter;

    private float gaitPhase;
    private float smoothedPhaseRate;
    private float locomotionWeight;
    private float breathPhase;
    private float reachWeight;
    private bool reaching;
    private Vector3 reachWorldPoint;
    private float fireKickTimer;
    private bool fireKickMelee;
    private Vector3 previousLocalVelocity;
    private Vector3 smoothedLocalAcceleration;
    private float previousYaw;
    private float smoothedTurnRate;

    private Transform hipsTarget;
    private Transform spineTarget;
    private Transform chestTarget;
    private Transform headTarget;
    private Transform leftUpperArm;
    private Transform rightUpperArm;
    private Transform leftLowerArm;
    private Transform rightLowerArm;
    private Transform leftHand;
    private Transform rightHand;
    private Transform leftUpperLeg;
    private Transform rightUpperLeg;
    private Transform leftLowerLeg;
    private Transform rightLowerLeg;
    private Transform leftFoot;
    private Transform rightFoot;

    public float LocomotionWeight => locomotionWeight;
    public float GaitPhase => gaitPhase;
    public float ReachWeight => reachWeight;
    public Vector3 VisualPelvisOffset { get; private set; }
    public bool IsReaching => reaching;
    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }

        if (playerMover == null)
        {
            playerMover = GetComponent<PlayerMover>();
        }

        if (playerInteraction == null)
        {
            playerInteraction = GetComponent<PlayerInteraction>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        EnsureBoneCache();
        CacheAnimatorParameters();
        previousYaw = transform.eulerAngles.y;
        previousLocalVelocity = playerMover != null
            ? transform.InverseTransformDirection(playerMover.CurrentPlanarVelocity)
            : Vector3.zero;
    }

    private void OnEnable()
    {
        previousYaw = transform.eulerAngles.y;
        previousLocalVelocity = playerMover != null
            ? transform.InverseTransformDirection(playerMover.CurrentPlanarVelocity)
            : Vector3.zero;
        smoothedLocalAcceleration = Vector3.zero;
        smoothedTurnRate = 0f;
    }

    private void Update()
    {
        if (!bonesCached)
        {
            EnsureBoneCache();
        }

        if (rig == null || !bonesCached)
        {
            return;
        }

        float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);
        Vector3 planarVelocity = playerMover != null ? playerMover.CurrentPlanarVelocity : Vector3.zero;
        float planarSpeed = planarVelocity.magnitude;
        Vector3 localVelocity = transform.InverseTransformDirection(planarVelocity);

        bool sprinting = InputReader.SprintHeld() && planarSpeed > 0.12f;
        IsSprinting = sprinting;

        UpdateMotionDynamics(localVelocity, deltaTime);
        UpdateGaitClock(planarSpeed, sprinting, deltaTime);

        MotionState state = ResolveState(planarSpeed, sprinting);
        UpdateAnimator(state, planarSpeed, sprinting);
        UpdatePose(state, localVelocity, planarSpeed, sprinting, deltaTime);

        previousLocalVelocity = localVelocity;
        previousYaw = transform.eulerAngles.y;
    }

    public void BeginReach(Vector3 worldPoint)
    {
        reachWorldPoint = worldPoint;
        reaching = true;
    }

    public void EndReach()
    {
        reaching = false;
    }

    public void NotifyFired(bool meleeAttack)
    {
        fireKickMelee = meleeAttack;
        fireKickTimer = Mathf.Max(0.02f, fireKickDuration);
    }

    private void UpdateMotionDynamics(Vector3 localVelocity, float deltaTime)
    {
        Vector3 rawAcceleration = (localVelocity - previousLocalVelocity) / deltaTime;
        rawAcceleration = Vector3.ClampMagnitude(rawAcceleration, 35f);
        float blend = 1f - Mathf.Exp(-Mathf.Max(0.01f, dynamicsResponse) * deltaTime);
        smoothedLocalAcceleration = Vector3.Lerp(smoothedLocalAcceleration, rawAcceleration, blend);

        float yaw = transform.eulerAngles.y;
        float rawTurnRate = Mathf.DeltaAngle(previousYaw, yaw) / deltaTime;
        rawTurnRate = Mathf.Clamp(rawTurnRate, -360f, 360f);
        smoothedTurnRate = Mathf.Lerp(smoothedTurnRate, rawTurnRate, blend);
    }

    private void UpdateGaitClock(float planarSpeed, bool sprinting, float deltaTime)
    {
        float targetLocomotion = Mathf.InverseLerp(0.05f, sprinting ? 5.6f : 3.8f, planarSpeed);
        locomotionWeight = Mathf.MoveTowards(
            locomotionWeight,
            targetLocomotion,
            Mathf.Max(0.1f, locomotionBlendSpeed) * deltaTime);

        float strideLength = Mathf.Lerp(
            Mathf.Max(0.55f, walkStrideLength),
            Mathf.Max(0.75f, sprintStrideLength),
            sprinting ? 1f : 0f);

        float targetPhaseRate = planarSpeed > 0.05f
            ? (planarSpeed / strideLength) * TwoPi
            : 0f;

        float rateBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, gaitPhaseResponse) * deltaTime);
        smoothedPhaseRate = Mathf.Lerp(smoothedPhaseRate, targetPhaseRate, rateBlend);
        gaitPhase = Mathf.Repeat(gaitPhase + (smoothedPhaseRate * deltaTime), TwoPi);

        breathPhase = Mathf.Repeat(
            breathPhase + (TwoPi * Mathf.Max(0.05f, breathingRate) * deltaTime),
            TwoPi);
    }

    private MotionState ResolveState(float planarSpeed, bool sprinting)
    {
        if (reaching)
        {
            return MotionState.Reach;
        }

        if (planarSpeed > 0.18f)
        {
            return sprinting ? MotionState.Sprint : MotionState.Walk;
        }

        return MotionState.Idle;
    }

    private void CacheAnimatorParameters()
    {
        animatorParametersCached = true;
        hasMoveSpeedParameter = false;
        hasSprintParameter = false;
        hasReachParameter = false;
        hasStateParameter = false;

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter == null)
            {
                continue;
            }

            if (parameter.nameHash == Animator.StringToHash("moveSpeed") && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasMoveSpeedParameter = true;
            }
            else if (parameter.nameHash == Animator.StringToHash("isSprinting") && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasSprintParameter = true;
            }
            else if (parameter.nameHash == Animator.StringToHash("isReaching") && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasReachParameter = true;
            }
            else if (parameter.nameHash == Animator.StringToHash("state") && parameter.type == AnimatorControllerParameterType.Int)
            {
                hasStateParameter = true;
            }
        }
    }

    private void UpdateAnimator(MotionState state, float speed, bool sprinting)
    {
        if (!animatorParametersCached)
        {
            CacheAnimatorParameters();
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        if (hasMoveSpeedParameter)
        {
            animator.SetFloat("moveSpeed", speed);
        }
        if (hasSprintParameter)
        {
            animator.SetBool("isSprinting", sprinting);
        }
        if (hasReachParameter)
        {
            animator.SetBool("isReaching", state == MotionState.Reach);
        }
        if (hasStateParameter)
        {
            animator.SetInteger("state", (int)state);
        }
    }

    private void UpdatePose(MotionState state, Vector3 localVelocity, float speed, bool sprinting, float deltaTime)
    {
        float targetReachWeight = state == MotionState.Reach ? 1f : 0f;
        reachWeight = Mathf.MoveTowards(reachWeight, targetReachWeight, reachInfluenceSpeed * deltaTime);

        float sprintBlend = sprinting ? 1f : 0f;
        float locomotion = locomotionWeight;
        float gaitSin = Mathf.Sin(gaitPhase);
        float gaitCos = Mathf.Cos(gaitPhase);
        float doubleStep = Mathf.Cos(gaitPhase * 2f);
        float idleWeight = 1f - Mathf.Clamp01(locomotion * 1.25f);
        float breath = Mathf.Sin(breathPhase);
        float slowSway = Mathf.Sin(breathPhase * 0.47f + 1.3f);

        float legSwing = Mathf.Lerp(walkLegSwing, sprintLegSwing, sprintBlend) * locomotion;
        float kneeFlex = Mathf.Lerp(walkKneeFlex, sprintKneeFlex, sprintBlend) * locomotion;
        float armSwing = Mathf.Lerp(walkArmSwing, sprintArmSwing, sprintBlend) * locomotion;
        float elbowFlex = Mathf.Lerp(relaxedElbowFlex, sprintElbowFlex, sprintBlend);

        float normalizedForward = Mathf.Clamp(localVelocity.z / 6.2f, -1f, 1f);
        float normalizedStrafe = Mathf.Clamp(localVelocity.x / 6.2f, -1f, 1f);
        float forwardLean = Mathf.Clamp(
            (normalizedForward * speedLean) + (smoothedLocalAcceleration.z * accelerationLean),
            -5f,
            9f);
        float sideLean = Mathf.Clamp(
            (-normalizedStrafe * strafeLean) - (smoothedTurnRate * turnLean),
            -7f,
            7f);

        float currentPelvisBob = Mathf.Lerp(walkPelvisBob, sprintPelvisBob, sprintBlend);
        float verticalBob = ((doubleStep - 1f) * 0.5f) * currentPelvisBob * locomotion;
        float lateralSway = gaitCos * pelvisSway * locomotion;
        float forwardTravel = -Mathf.Abs(gaitSin) * pelvisForwardTravel * locomotion;
        float idleShift = slowSway * 0.005f * idleWeight;
        VisualPelvisOffset = new Vector3(lateralSway + idleShift, verticalBob, forwardTravel);

        Quaternion hipsPose = Quaternion.Euler(
            forwardLean * 0.18f,
            gaitSin * pelvisYaw * locomotion,
            (-gaitCos * pelvisRoll * locomotion) + (sideLean * 0.18f) + (slowSway * idleWeight * idleWeightShift));
        SetLocalRotation(hipsTarget, "Hips", hipsPose);

        Quaternion spinePose = Quaternion.Euler(
            (forwardLean * 0.34f) + (breath * breathingSpinePitch * idleWeight),
            -gaitSin * spineCounterYaw * locomotion,
            sideLean * 0.32f);
        SetLocalRotation(spineTarget, "Spine", spinePose);

        Quaternion chestPose = Quaternion.Euler(
            (forwardLean * 0.46f) + (breath * breathingChestPitch),
            -gaitSin * chestCounterYaw * locomotion,
            sideLean * 0.48f);
        SetLocalRotation(chestTarget, "Chest", chestPose);

        Quaternion headPose = Quaternion.Euler(
            (-forwardLean * 0.48f) - (breath * 0.12f),
            (gaitSin * headCounterYaw * locomotion) + (slowSway * idleWeight * idleHeadSway),
            -sideLean * 0.38f);
        SetLocalRotation(headTarget, "Head", headPose);

        float leftKnee = Mathf.Pow(Mathf.Clamp01(-gaitSin), 1.2f) * kneeFlex;
        float rightKnee = Mathf.Pow(Mathf.Clamp01(gaitSin), 1.2f) * kneeFlex;
        float strafeLegYaw = normalizedStrafe * 4f * locomotion;

        SetLocalRotation(leftUpperLeg, "LeftUpperLeg", Quaternion.Euler(gaitSin * legSwing, strafeLegYaw, -normalizedStrafe * 2.4f * locomotion));
        SetLocalRotation(rightUpperLeg, "RightUpperLeg", Quaternion.Euler(-gaitSin * legSwing, strafeLegYaw, -normalizedStrafe * 2.4f * locomotion));
        SetLocalRotation(leftLowerLeg, "LeftLowerLeg", Quaternion.Euler(leftKnee, 0f, 0f));
        SetLocalRotation(rightLowerLeg, "RightLowerLeg", Quaternion.Euler(rightKnee, 0f, 0f));

        float leftFootPitch = (-gaitSin * footPitch * locomotion) - (leftKnee * 0.16f);
        float rightFootPitch = (gaitSin * footPitch * locomotion) - (rightKnee * 0.16f);
        SetLocalRotation(leftFoot, "LeftFoot", Quaternion.Euler(leftFootPitch, 0f, gaitCos * footRoll * locomotion));
        SetLocalRotation(rightFoot, "RightFoot", Quaternion.Euler(rightFootPitch, 0f, -gaitCos * footRoll * locomotion));

        float shoulderBreath = breath * 0.65f * idleWeight;
        SetLocalRotation(leftUpperArm, "LeftUpperArm", Quaternion.Euler((-gaitSin * armSwing) + shoulderBreath, 0f, -1.5f * locomotion));
        SetLocalRotation(rightUpperArm, "RightUpperArm", Quaternion.Euler((gaitSin * armSwing) + shoulderBreath, 0f, 1.5f * locomotion));
        SetLocalRotation(leftLowerArm, "LeftLowerArm", Quaternion.Euler(-relaxedElbowFlex - (Mathf.Clamp01(gaitSin) * elbowFlex * locomotion), 0f, 0f));
        SetLocalRotation(rightLowerArm, "RightLowerArm", Quaternion.Euler(-relaxedElbowFlex - (Mathf.Clamp01(-gaitSin) * elbowFlex * locomotion), 0f, 0f));
        SetLocalRotation(leftHand, "LeftHand", Quaternion.Euler(gaitSin * 2.2f * locomotion, 0f, gaitCos * 2.8f * locomotion));
        SetLocalRotation(rightHand, "RightHand", Quaternion.Euler(-gaitSin * 2.2f * locomotion, 0f, -gaitCos * 2.8f * locomotion));

        if (reachWeight > 0.0001f && chestTarget != null && rightUpperArm != null && rightLowerArm != null && rightHand != null)
        {
            Vector3 localToTarget = chestTarget.InverseTransformPoint(reachWorldPoint);
            float yaw = Mathf.Clamp(localToTarget.x * 140f, -68f, 68f);
            float pitch = Mathf.Clamp(-localToTarget.y * 80f, -42f, 42f);

            Quaternion upperReach = Quaternion.Euler(reachUpperArmPitch + pitch, yaw, -yaw * 0.24f);
            Quaternion lowerReach = Quaternion.Euler(reachLowerArmPitch, yaw * 0.08f, 0f);
            Quaternion handReach = Quaternion.Euler(reachHandPitch, 0f, 0f);

            BlendTowardPose(rightUpperArm, "RightUpperArm", upperReach, reachWeight);
            BlendTowardPose(rightLowerArm, "RightLowerArm", lowerReach, reachWeight);
            BlendTowardPose(rightHand, "RightHand", handReach, reachWeight);

            if (chestTarget != null)
            {
                Quaternion reachChest = Quaternion.Euler(3f, yaw * 0.18f, -yaw * 0.07f);
                BlendTowardPose(chestTarget, "Chest", reachChest, reachWeight * 0.45f);
            }
        }

        ApplyFireKickPose(deltaTime);
    }

    private void ApplyFireKickPose(float deltaTime)
    {
        if (fireKickTimer <= 0f)
        {
            return;
        }

        fireKickTimer = Mathf.Max(0f, fireKickTimer - deltaTime);
        float normalized = fireKickDuration > 0.001f ? (fireKickTimer / fireKickDuration) : 0f;
        float envelope = Mathf.Sin((1f - normalized) * Mathf.PI);
        if (envelope <= 0.0001f)
        {
            return;
        }

        float meleeScale = fireKickMelee ? 0.35f : 1f;
        float pitch = fireKickPitch * envelope * meleeScale;
        float yaw = fireKickYaw * envelope * meleeScale;
        float roll = fireKickRoll * envelope * meleeScale;
        float blend = Mathf.Clamp01(envelope);

        BlendTowardPose(rightUpperArm, "RightUpperArm", Quaternion.Euler(-pitch, yaw, -roll), blend);
        BlendTowardPose(rightLowerArm, "RightLowerArm", Quaternion.Euler(-pitch * 0.78f, 0f, 0f), blend);
        BlendTowardPose(rightHand, "RightHand", Quaternion.Euler(-pitch * 0.45f, 0f, 0f), blend);

        if (chestTarget != null && baseLocalRotations.TryGetValue("Chest", out Quaternion chestBase))
        {
            Quaternion chestKick = chestBase * Quaternion.Euler(-pitch * 0.18f, yaw * 0.24f, 0f);
            chestTarget.localRotation = Quaternion.Slerp(
                chestTarget.localRotation,
                chestKick,
                1f - Mathf.Exp(-Mathf.Max(0.01f, poseLerpSpeed) * deltaTime));
        }
    }

    private void BlendTowardPose(Transform bone, string key, Quaternion additivePose, float blend)
    {
        if (bone == null || !baseLocalRotations.TryGetValue(key, out Quaternion baseRotation))
        {
            return;
        }

        Quaternion locomotionPose = bone.localRotation;
        Quaternion reachPose = baseRotation * additivePose;
        Quaternion blended = Quaternion.Slerp(locomotionPose, reachPose, Mathf.Clamp01(blend));
        float response = 1f - Mathf.Exp(-Mathf.Max(0.01f, poseLerpSpeed) * Time.deltaTime);
        bone.localRotation = Quaternion.Slerp(locomotionPose, blended, response);
    }

    private void SetLocalRotation(Transform bone, string key, Quaternion additivePose)
    {
        if (bone == null || !baseLocalRotations.TryGetValue(key, out Quaternion baseRotation))
        {
            return;
        }

        Quaternion targetRotation = baseRotation * additivePose;
        float response = 1f - Mathf.Exp(-Mathf.Max(0.01f, poseLerpSpeed) * Time.deltaTime);
        bone.localRotation = Quaternion.Slerp(bone.localRotation, targetRotation, response);
    }

    private void EnsureBoneCache()
    {
        if (bonesCached || rig == null)
        {
            return;
        }

        rig.EnsureBuilt();

        hipsTarget = rig.GetBone("Hips", true);
        spineTarget = rig.GetBone("Spine", true);
        chestTarget = rig.GetBone("Chest", true);
        headTarget = rig.GetBone("Head", true);
        leftUpperArm = rig.GetBone("LeftUpperArm", true);
        rightUpperArm = rig.GetBone("RightUpperArm", true);
        leftLowerArm = rig.GetBone("LeftLowerArm", true);
        rightLowerArm = rig.GetBone("RightLowerArm", true);
        leftHand = rig.GetBone("LeftHand", true);
        rightHand = rig.GetBone("RightHand", true);
        leftUpperLeg = rig.GetBone("LeftUpperLeg", true);
        rightUpperLeg = rig.GetBone("RightUpperLeg", true);
        leftLowerLeg = rig.GetBone("LeftLowerLeg", true);
        rightLowerLeg = rig.GetBone("RightLowerLeg", true);
        leftFoot = rig.GetBone("LeftFoot", true);
        rightFoot = rig.GetBone("RightFoot", true);

        CacheBasePose("Hips", hipsTarget);
        CacheBasePose("Spine", spineTarget);
        CacheBasePose("Chest", chestTarget);
        CacheBasePose("Head", headTarget);
        CacheBasePose("LeftUpperArm", leftUpperArm);
        CacheBasePose("RightUpperArm", rightUpperArm);
        CacheBasePose("LeftLowerArm", leftLowerArm);
        CacheBasePose("RightLowerArm", rightLowerArm);
        CacheBasePose("LeftHand", leftHand);
        CacheBasePose("RightHand", rightHand);
        CacheBasePose("LeftUpperLeg", leftUpperLeg);
        CacheBasePose("RightUpperLeg", rightUpperLeg);
        CacheBasePose("LeftLowerLeg", leftLowerLeg);
        CacheBasePose("RightLowerLeg", rightLowerLeg);
        CacheBasePose("LeftFoot", leftFoot);
        CacheBasePose("RightFoot", rightFoot);

        bonesCached = hipsTarget != null && chestTarget != null && headTarget != null &&
                      leftUpperLeg != null && rightUpperLeg != null && leftFoot != null && rightFoot != null;
    }

    private void CacheBasePose(string key, Transform bone)
    {
        if (bone == null)
        {
            return;
        }

        baseLocalRotations[key] = bone.localRotation;
        baseLocalPositions[key] = bone.localPosition;
    }
}
