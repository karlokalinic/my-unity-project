using UnityEngine;

[DefaultExecutionOrder(120)]
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHumanoidVisualDriver))]
public sealed class PlayerFootGroundingDetailDriver : MonoBehaviour
{
    private const float TwoPi = Mathf.PI * 2f;

    [Header("References")]
    [SerializeField] private PlayerHumanoidVisualDriver visualDriver;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private DeathRagdollController ragdollController;

    [Header("Ground Contact")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float probeLift = 0.28f;
    [SerializeField] private float probeDistance = 0.62f;
    [SerializeField] private float maxGroundAngle = 52f;
    [SerializeField] private float maxAnkleTilt = 24f;
    [SerializeField] private float groundRotationResponse = 18f;
    [SerializeField] [Range(0f, 1f)] private float swingFootGroundInfluence = 0.16f;

    private readonly RaycastHit[] hits = new RaycastHit[8];
    private Transform leftFoot;
    private Transform rightFoot;
    private bool resolved;
    private float resolveCooldown;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveFeet();
    }

    private void LateUpdate()
    {
        if (ragdollController != null && ragdollController.RagdollActive)
        {
            return;
        }

        if (!resolved)
        {
            resolveCooldown -= Time.deltaTime;
            if (resolveCooldown <= 0f)
            {
                resolveCooldown = 0.35f;
                ResolveReferences();
                ResolveFeet();
            }
            if (!resolved)
            {
                return;
            }
        }

        if (characterController != null && !characterController.isGrounded)
        {
            return;
        }

        float locomotion = animationController != null ? animationController.LocomotionWeight : 0f;
        float cycle = animationController != null ? Mathf.Repeat(animationController.GaitPhase / TwoPi, 1f) : 0f;
        float leftStance = Mathf.Lerp(1f, ResolveStanceWeight(cycle), locomotion);
        float rightStance = Mathf.Lerp(1f, ResolveStanceWeight(Mathf.Repeat(cycle + 0.5f, 1f)), locomotion);

        ApplyGroundAlignment(leftFoot, leftStance);
        ApplyGroundAlignment(rightFoot, rightStance);
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
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        if (ragdollController == null)
        {
            ragdollController = GetComponent<DeathRagdollController>();
        }
    }

    private void ResolveFeet()
    {
        resolved = false;
        leftFoot = null;
        rightFoot = null;
        if (visualDriver == null)
        {
            return;
        }

        visualDriver.ResolveNow();
        bool hasLeft = visualDriver.TryGetVisualBone(HumanBodyBones.LeftFoot, out leftFoot);
        bool hasRight = visualDriver.TryGetVisualBone(HumanBodyBones.RightFoot, out rightFoot);
        resolved = hasLeft && hasRight && leftFoot != null && rightFoot != null;
    }

    private void ApplyGroundAlignment(Transform foot, float stanceWeight)
    {
        if (foot == null)
        {
            return;
        }

        Vector3 up = transform.up;
        Vector3 origin = foot.position + up * Mathf.Max(0.05f, probeLift);
        int count = Physics.RaycastNonAlloc(
            origin,
            -up,
            hits,
            Mathf.Max(0.1f, probeLift + probeDistance),
            groundMask,
            QueryTriggerInteraction.Ignore);

        bool found = false;
        RaycastHit bestHit = default;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = hits[i];
            Collider collider = hit.collider;
            if (collider == null)
            {
                continue;
            }

            Transform hitTransform = collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                bestHit = hit;
                found = true;
            }
        }

        if (!found)
        {
            return;
        }

        float surfaceAngle = Vector3.Angle(up, bestHit.normal);
        if (surfaceAngle > Mathf.Max(1f, maxGroundAngle))
        {
            return;
        }

        float tiltFraction = surfaceAngle <= 0.001f
            ? 1f
            : Mathf.Clamp01(Mathf.Max(0f, maxAnkleTilt) / surfaceAngle);
        Vector3 ankleNormal = Vector3.Slerp(up, bestHit.normal, tiltFraction).normalized;
        Quaternion slopeDelta = Quaternion.FromToRotation(up, ankleNormal);
        Quaternion desired = slopeDelta * foot.rotation;

        float effectiveContact = Mathf.Lerp(
            Mathf.Clamp01(swingFootGroundInfluence),
            1f,
            Mathf.Clamp01(stanceWeight));
        float response = 1f - Mathf.Exp(-Mathf.Max(0.01f, groundRotationResponse) * effectiveContact * Time.deltaTime);
        foot.rotation = Quaternion.Slerp(foot.rotation, desired, response);
    }

    private static float ResolveStanceWeight(float phase)
    {
        phase = Mathf.Repeat(phase, 1f);

        // Heel strike: softly establish contact as the foot returns in front.
        if (phase >= 0.90f)
        {
            return Smooth01(Mathf.InverseLerp(0.90f, 1f, phase));
        }

        // Foot-flat / mid-stance carries most of the body weight.
        if (phase <= 0.58f)
        {
            if (phase < 0.08f)
            {
                return Mathf.Lerp(0.72f, 1f, Smooth01(Mathf.InverseLerp(0f, 0.08f, phase)));
            }
            return 1f;
        }

        // Toe-off unloads the ankle before the swing phase begins.
        if (phase < 0.72f)
        {
            return 1f - Smooth01(Mathf.InverseLerp(0.58f, 0.72f, phase));
        }

        return 0f;
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
