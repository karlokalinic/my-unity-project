using UnityEngine;

[DefaultExecutionOrder(-50)]
[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public sealed class PlayerMicroMotionDetailDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralHumanoidRig rig;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private DeathRagdollController ragdollController;

    [Header("Breathing / Exertion")]
    [SerializeField] private float restBreathingRate = 0.22f;
    [SerializeField] private float exertedBreathingRate = 0.52f;
    [SerializeField] private float exertionBuildRate = 0.28f;
    [SerializeField] private float exertionRecoveryRate = 0.10f;
    [SerializeField] private float chestBreathPitch = 0.75f;
    [SerializeField] private float spineBreathPitch = 0.28f;
    [SerializeField] private float shoulderBreathRoll = 0.38f;

    [Header("Head / Torso Inertia")]
    [SerializeField] private float headTurnLead = 0.024f;
    [SerializeField] private float chestTurnLead = 0.010f;
    [SerializeField] private float turnRateResponse = 8f;
    [SerializeField] private float idleHeadYaw = 0.42f;
    [SerializeField] private float idleHeadPitch = 0.18f;
    [SerializeField] private float accelerationHeadPitch = 0.025f;
    [SerializeField] private float accelerationChestPitch = 0.014f;

    [Header("Hands / Asymmetry")]
    [SerializeField] private float wristRelaxRoll = 2.2f;
    [SerializeField] private float wristRelaxYaw = 1.3f;
    [SerializeField] private float armAsymmetry = 0.55f;

    private Transform hips;
    private Transform spine;
    private Transform chest;
    private Transform head;
    private Transform leftUpperArm;
    private Transform rightUpperArm;
    private Transform leftHand;
    private Transform rightHand;

    private Quaternion lastHipsAdditive = Quaternion.identity;
    private Quaternion lastSpineAdditive = Quaternion.identity;
    private Quaternion lastChestAdditive = Quaternion.identity;
    private Quaternion lastHeadAdditive = Quaternion.identity;
    private Quaternion lastLeftUpperArmAdditive = Quaternion.identity;
    private Quaternion lastRightUpperArmAdditive = Quaternion.identity;
    private Quaternion lastLeftHandAdditive = Quaternion.identity;
    private Quaternion lastRightHandAdditive = Quaternion.identity;

    private float exertion;
    private float breathPhase;
    private float microPhase;
    private float previousYaw;
    private float smoothedTurnRate;
    private Vector3 previousLocalVelocity;
    private Vector3 smoothedLocalAcceleration;
    private bool bonesResolved;

    public float Exertion => exertion;

    private void Awake()
    {
        ResolveReferences();
        ResolveBones();
        ResetMotionHistory();
    }

    private void OnEnable()
    {
        ResetMotionHistory();
    }

    private void Update()
    {
        RemovePreviousAdditives();

        if (!bonesResolved)
        {
            ResolveReferences();
            ResolveBones();
        }

        if (!bonesResolved || (ragdollController != null && ragdollController.RagdollActive))
        {
            return;
        }

        float deltaTime = Mathf.Max(0.0001f, Time.deltaTime);
        float speed = playerMover != null ? playerMover.CurrentPlanarSpeed : 0f;
        bool sprinting = animationController != null && animationController.IsSprinting;

        float locomotionDemand = Mathf.Clamp01(speed / 6.2f);
        float targetExertion = sprinting ? 1f : locomotionDemand * 0.28f;
        float exertionRate = targetExertion > exertion ? exertionBuildRate : exertionRecoveryRate;
        exertion = Mathf.MoveTowards(exertion, targetExertion, Mathf.Max(0.001f, exertionRate) * deltaTime);

        float breathingRate = Mathf.Lerp(restBreathingRate, exertedBreathingRate, exertion);
        breathPhase = Mathf.Repeat(breathPhase + Mathf.PI * 2f * breathingRate * deltaTime, Mathf.PI * 2f);
        microPhase = Mathf.Repeat(microPhase + deltaTime, 1000f);

        float yaw = transform.eulerAngles.y;
        float rawTurnRate = Mathf.DeltaAngle(previousYaw, yaw) / deltaTime;
        rawTurnRate = Mathf.Clamp(rawTurnRate, -360f, 360f);
        float turnBlend = 1f - Mathf.Exp(-Mathf.Max(0.01f, turnRateResponse) * deltaTime);
        smoothedTurnRate = Mathf.Lerp(smoothedTurnRate, rawTurnRate, turnBlend);
        previousYaw = yaw;

        Vector3 localVelocity = playerMover != null
            ? transform.InverseTransformDirection(playerMover.CurrentPlanarVelocity)
            : Vector3.zero;
        Vector3 rawAcceleration = (localVelocity - previousLocalVelocity) / deltaTime;
        rawAcceleration = Vector3.ClampMagnitude(rawAcceleration, 30f);
        smoothedLocalAcceleration = Vector3.Lerp(smoothedLocalAcceleration, rawAcceleration, turnBlend);
        previousLocalVelocity = localVelocity;
    }

    private void LateUpdate()
    {
        if (!bonesResolved || (ragdollController != null && ragdollController.RagdollActive))
        {
            return;
        }

        float locomotion = animationController != null ? animationController.LocomotionWeight : 0f;
        float reach = animationController != null ? animationController.ReachWeight : 0f;
        float idleWeight = 1f - Mathf.Clamp01(locomotion * 1.35f);
        float breath = Mathf.Sin(breathPhase);
        float breathSecondary = Mathf.Sin((breathPhase * 2f) + 0.7f);
        float breathDepth = Mathf.Lerp(0.72f, 1.85f, exertion);

        float idleYaw = (Mathf.Sin(microPhase * 0.53f) + Mathf.Sin((microPhase * 0.17f) + 1.9f) * 0.35f)
            * idleHeadYaw * idleWeight;
        float idlePitch = Mathf.Sin((microPhase * 0.31f) + 0.8f) * idleHeadPitch * idleWeight;

        float headTurn = Mathf.Clamp(smoothedTurnRate * headTurnLead, -7.5f, 7.5f);
        float chestTurn = Mathf.Clamp(smoothedTurnRate * chestTurnLead, -3.8f, 3.8f);
        float accelerationPitch = Mathf.Clamp(-smoothedLocalAcceleration.z * accelerationHeadPitch, -1.7f, 1.7f);
        float accelerationChest = Mathf.Clamp(-smoothedLocalAcceleration.z * accelerationChestPitch, -1.1f, 1.1f);
        float lateralInertia = Mathf.Clamp(-smoothedLocalAcceleration.x * 0.014f, -1.2f, 1.2f);

        ApplyAdditive(hips, Quaternion.Euler(0f, 0f, lateralInertia * 0.20f), ref lastHipsAdditive);
        ApplyAdditive(
            spine,
            Quaternion.Euler((breath * spineBreathPitch * breathDepth) + (accelerationChest * 0.45f), -chestTurn * 0.42f, lateralInertia * 0.30f),
            ref lastSpineAdditive);
        ApplyAdditive(
            chest,
            Quaternion.Euler((breath * chestBreathPitch * breathDepth) + accelerationChest, chestTurn, lateralInertia * 0.55f),
            ref lastChestAdditive);
        ApplyAdditive(
            head,
            Quaternion.Euler(idlePitch + accelerationPitch - (breath * 0.08f * breathDepth), idleYaw + headTurn, -lateralInertia * 0.40f),
            ref lastHeadAdditive);

        float shoulderRoll = breath * shoulderBreathRoll * breathDepth;
        float asymmetricShoulder = breathSecondary * armAsymmetry * idleWeight;
        ApplyAdditive(leftUpperArm, Quaternion.Euler(0f, 0f, -shoulderRoll + asymmetricShoulder), ref lastLeftUpperArmAdditive);
        ApplyAdditive(rightUpperArm, Quaternion.Euler(0f, 0f, shoulderRoll + asymmetricShoulder * 0.65f), ref lastRightUpperArmAdditive);

        float relaxedHands = 1f - Mathf.Clamp01(reach * 1.15f);
        float leftWristRoll = (-wristRelaxRoll + Mathf.Sin(microPhase * 0.61f) * 0.35f) * relaxedHands;
        float rightWristRoll = (wristRelaxRoll * 0.72f + Mathf.Sin((microPhase * 0.57f) + 1.2f) * 0.28f) * relaxedHands;
        float wristYaw = Mathf.Sin(microPhase * 0.43f) * wristRelaxYaw * idleWeight * relaxedHands;
        ApplyAdditive(leftHand, Quaternion.Euler(0f, wristYaw, leftWristRoll), ref lastLeftHandAdditive);
        ApplyAdditive(rightHand, Quaternion.Euler(0f, -wristYaw * 0.75f, rightWristRoll), ref lastRightHandAdditive);
    }

    private void OnDisable()
    {
        RemovePreviousAdditives();
    }

    private void ResolveReferences()
    {
        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }
        if (playerMover == null)
        {
            playerMover = GetComponent<PlayerMover>();
        }
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }
        if (ragdollController == null)
        {
            ragdollController = GetComponent<DeathRagdollController>();
        }
    }

    private void ResolveBones()
    {
        bonesResolved = false;
        if (rig == null)
        {
            return;
        }

        rig.EnsureBuilt();
        hips = rig.GetBone("Hips", true);
        spine = rig.GetBone("Spine", true);
        chest = rig.GetBone("Chest", true);
        head = rig.GetBone("Head", true);
        leftUpperArm = rig.GetBone("LeftUpperArm", true);
        rightUpperArm = rig.GetBone("RightUpperArm", true);
        leftHand = rig.GetBone("LeftHand", true);
        rightHand = rig.GetBone("RightHand", true);
        bonesResolved = hips != null && spine != null && chest != null && head != null;
    }

    private void ResetMotionHistory()
    {
        previousYaw = transform.eulerAngles.y;
        previousLocalVelocity = playerMover != null
            ? transform.InverseTransformDirection(playerMover.CurrentPlanarVelocity)
            : Vector3.zero;
        smoothedLocalAcceleration = Vector3.zero;
        smoothedTurnRate = 0f;
    }

    private void RemovePreviousAdditives()
    {
        RemoveAdditive(hips, ref lastHipsAdditive);
        RemoveAdditive(spine, ref lastSpineAdditive);
        RemoveAdditive(chest, ref lastChestAdditive);
        RemoveAdditive(head, ref lastHeadAdditive);
        RemoveAdditive(leftUpperArm, ref lastLeftUpperArmAdditive);
        RemoveAdditive(rightUpperArm, ref lastRightUpperArmAdditive);
        RemoveAdditive(leftHand, ref lastLeftHandAdditive);
        RemoveAdditive(rightHand, ref lastRightHandAdditive);
    }

    private static void ApplyAdditive(Transform bone, Quaternion additive, ref Quaternion lastApplied)
    {
        if (bone == null)
        {
            lastApplied = Quaternion.identity;
            return;
        }

        bone.localRotation = bone.localRotation * additive;
        lastApplied = additive;
    }

    private static void RemoveAdditive(Transform bone, ref Quaternion lastApplied)
    {
        if (bone != null && Quaternion.Angle(lastApplied, Quaternion.identity) > 0.0001f)
        {
            bone.localRotation = bone.localRotation * Quaternion.Inverse(lastApplied);
        }

        lastApplied = Quaternion.identity;
    }
}
