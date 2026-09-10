using UnityEngine;

[DefaultExecutionOrder(80)]
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHumanoidVisualDriver))]
[RequireComponent(typeof(PlayerEyeGazeDetailDriver))]
public sealed class PlayerAnatomicalDetailDriver : MonoBehaviour
{
    private const float TwoPi = Mathf.PI * 2f;

    [Header("References")]
    [SerializeField] private PlayerHumanoidVisualDriver visualDriver;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerMicroMotionDetailDriver microMotion;
    [SerializeField] private DeathRagdollController ragdollController;

    [Header("Neck")]
    [SerializeField] private float idleNeckYaw = 0.55f;
    [SerializeField] private float locomotionNeckYaw = 0.9f;
    [SerializeField] private float neckBreathPitch = 0.12f;

    [Header("Shoulders")]
    [SerializeField] private float shoulderBreathLift = 0.42f;
    [SerializeField] private float sprintShoulderSet = 1.15f;
    [SerializeField] private float shoulderCounterSwing = 0.85f;

    [Header("Toe-Off")]
    [SerializeField] private float walkToeExtension = 6.5f;
    [SerializeField] private float sprintToeExtension = 10.5f;

    [Header("Recovery")]
    [SerializeField] private float unresolvedAvatarRetrySeconds = 0.45f;

    private Transform neck;
    private Transform leftShoulder;
    private Transform rightShoulder;
    private Transform leftToes;
    private Transform rightToes;

    private Quaternion lastNeckAdditive = Quaternion.identity;
    private Quaternion lastLeftShoulderAdditive = Quaternion.identity;
    private Quaternion lastRightShoulderAdditive = Quaternion.identity;
    private Quaternion lastLeftToesAdditive = Quaternion.identity;
    private Quaternion lastRightToesAdditive = Quaternion.identity;

    private bool resolved;
    private float resolveTimer;
    private float breathPhase;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveOptionalBones();
    }

    private void Update()
    {
        RemovePreviousAdditives();

        if (!resolved)
        {
            resolveTimer -= Time.deltaTime;
            if (resolveTimer <= 0f)
            {
                resolveTimer = Mathf.Max(0.1f, unresolvedAvatarRetrySeconds);
                ResolveReferences();
                ResolveOptionalBones();
            }
        }

        if (ragdollController != null && ragdollController.RagdollActive)
        {
            return;
        }

        float exertion = microMotion != null ? microMotion.Exertion : 0f;
        float breathingRate = Mathf.Lerp(0.22f, 0.52f, exertion);
        breathPhase = Mathf.Repeat(breathPhase + TwoPi * breathingRate * Time.deltaTime, TwoPi);
    }

    private void LateUpdate()
    {
        if (!resolved || (ragdollController != null && ragdollController.RagdollActive))
        {
            return;
        }

        float locomotion = animationController != null ? animationController.LocomotionWeight : 0f;
        float gait = animationController != null ? animationController.GaitPhase : 0f;
        bool sprinting = animationController != null && animationController.IsSprinting;
        float reach = animationController != null ? animationController.ReachWeight : 0f;
        float exertion = microMotion != null ? microMotion.Exertion : 0f;
        float breath = Mathf.Sin(breathPhase);
        float idleWeight = 1f - Mathf.Clamp01(locomotion * 1.25f);
        float gaitSin = Mathf.Sin(gait);

        float neckYaw = (Mathf.Sin(Time.time * 0.37f) * idleNeckYaw * idleWeight) +
                        (gaitSin * locomotionNeckYaw * locomotion);
        float neckPitch = -breath * neckBreathPitch * Mathf.Lerp(0.8f, 1.65f, exertion);
        ApplyWorldAxesAdditive(neck, neckPitch, neckYaw, 0f, ref lastNeckAdditive);

        float breathingLift = breath * shoulderBreathLift * Mathf.Lerp(0.75f, 1.8f, exertion);
        float sprintSet = (sprinting ? sprintShoulderSet : 0f) * (1f - reach * 0.55f);
        float counterSwing = gaitSin * shoulderCounterSwing * locomotion;
        ApplyWorldAxesAdditive(
            leftShoulder,
            -sprintSet * 0.20f,
            counterSwing * 0.25f,
            -breathingLift - sprintSet,
            ref lastLeftShoulderAdditive);
        ApplyWorldAxesAdditive(
            rightShoulder,
            -sprintSet * 0.20f,
            -counterSwing * 0.25f,
            breathingLift + sprintSet,
            ref lastRightShoulderAdditive);

        float cycle = Mathf.Repeat(gait / TwoPi, 1f);
        float toeAmplitude = Mathf.Lerp(walkToeExtension, sprintToeExtension, sprinting ? 1f : 0f) * locomotion;
        float leftToeOff = ResolveToeOff(Mathf.Repeat(cycle, 1f)) * toeAmplitude;
        float rightToeOff = ResolveToeOff(Mathf.Repeat(cycle + 0.5f, 1f)) * toeAmplitude;
        ApplyWorldAxesAdditive(leftToes, -leftToeOff, 0f, 0f, ref lastLeftToesAdditive);
        ApplyWorldAxesAdditive(rightToes, -rightToeOff, 0f, 0f, ref lastRightToesAdditive);
    }

    private void OnDisable()
    {
        RemovePreviousAdditives();
    }

    private void ResolveReferences()
    {
        if (visualDriver == null)
        {
            visualDriver = GetComponent<PlayerHumanoidVisualDriver>();
        }
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }
        if (microMotion == null)
        {
            microMotion = GetComponent<PlayerMicroMotionDetailDriver>();
        }
        if (ragdollController == null)
        {
            ragdollController = GetComponent<DeathRagdollController>();
        }
    }

    private void ResolveOptionalBones()
    {
        resolved = false;
        neck = null;
        leftShoulder = null;
        rightShoulder = null;
        leftToes = null;
        rightToes = null;

        if (visualDriver == null)
        {
            return;
        }

        visualDriver.ResolveNow();
        if (!visualDriver.IsHumanoidBound)
        {
            return;
        }

        visualDriver.TryGetVisualBone(HumanBodyBones.Neck, out neck);
        visualDriver.TryGetVisualBone(HumanBodyBones.LeftShoulder, out leftShoulder);
        visualDriver.TryGetVisualBone(HumanBodyBones.RightShoulder, out rightShoulder);
        visualDriver.TryGetVisualBone(HumanBodyBones.LeftToes, out leftToes);
        visualDriver.TryGetVisualBone(HumanBodyBones.RightToes, out rightToes);

        // Optional bones may legitimately be absent. A valid humanoid skin is enough to resolve the driver.
        resolved = visualDriver.IsHumanoidBound;
    }

    private void RemovePreviousAdditives()
    {
        RemoveWorldAdditive(neck, ref lastNeckAdditive);
        RemoveWorldAdditive(leftShoulder, ref lastLeftShoulderAdditive);
        RemoveWorldAdditive(rightShoulder, ref lastRightShoulderAdditive);
        RemoveWorldAdditive(leftToes, ref lastLeftToesAdditive);
        RemoveWorldAdditive(rightToes, ref lastRightToesAdditive);
    }

    private void ApplyWorldAxesAdditive(
        Transform bone,
        float pitchDegrees,
        float yawDegrees,
        float rollDegrees,
        ref Quaternion lastApplied)
    {
        if (bone == null)
        {
            lastApplied = Quaternion.identity;
            return;
        }

        Quaternion additive =
            Quaternion.AngleAxis(yawDegrees, transform.up) *
            Quaternion.AngleAxis(pitchDegrees, transform.right) *
            Quaternion.AngleAxis(rollDegrees, transform.forward);
        bone.rotation = additive * bone.rotation;
        lastApplied = additive;
    }

    private static void RemoveWorldAdditive(Transform bone, ref Quaternion lastApplied)
    {
        if (bone != null && Quaternion.Angle(lastApplied, Quaternion.identity) > 0.0001f)
        {
            bone.rotation = Quaternion.Inverse(lastApplied) * bone.rotation;
        }

        lastApplied = Quaternion.identity;
    }

    private static float ResolveToeOff(float phase)
    {
        if (phase < 0.52f || phase > 0.74f)
        {
            return 0f;
        }

        float normalized = Mathf.InverseLerp(0.52f, 0.74f, phase);
        return Mathf.Sin(normalized * Mathf.PI);
    }
}
