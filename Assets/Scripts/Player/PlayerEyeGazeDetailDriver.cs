using UnityEngine;

[DefaultExecutionOrder(140)]
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHumanoidVisualDriver))]
public sealed class PlayerEyeGazeDetailDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHumanoidVisualDriver visualDriver;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private DeathRagdollController ragdollController;

    [Header("Saccades")]
    [SerializeField] private Vector2 saccadeIntervalRange = new Vector2(0.65f, 2.3f);
    [SerializeField] private float maxYawDegrees = 2.8f;
    [SerializeField] private float maxPitchDegrees = 1.65f;
    [SerializeField] private float gazeResponse = 24f;
    [SerializeField] private float returnToCenterBias = 0.58f;

    [Header("Locomotion Stabilization")]
    [SerializeField] private float locomotionAmplitudeScale = 0.65f;
    [SerializeField] private float sprintAmplitudeScale = 0.45f;

    [Header("Recovery")]
    [SerializeField] private float unresolvedAvatarRetrySeconds = 0.65f;

    private Transform leftEye;
    private Transform rightEye;
    private bool resolved;
    private float resolveTimer;
    private float nextSaccadeAt;
    private Vector2 targetOffset;
    private Vector2 smoothedOffset;
    private Quaternion lastLeftAdditive = Quaternion.identity;
    private Quaternion lastRightAdditive = Quaternion.identity;

    public bool HasEyeBones => leftEye != null && rightEye != null;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveEyes();
        ScheduleSaccade();
    }

    private void Update()
    {
        RemovePreviousAdditives();

        if (!resolved)
        {
            resolveTimer -= Time.deltaTime;
            if (resolveTimer <= 0f)
            {
                resolveTimer = Mathf.Max(0.15f, unresolvedAvatarRetrySeconds);
                ResolveReferences();
                ResolveEyes();
            }
        }

        if (!resolved || !HasEyeBones || (ragdollController != null && ragdollController.RagdollActive))
        {
            return;
        }

        if (Time.time >= nextSaccadeAt)
        {
            ChooseSaccadeTarget();
            ScheduleSaccade();
        }

        float locomotion = animationController != null ? animationController.LocomotionWeight : 0f;
        bool sprinting = animationController != null && animationController.IsSprinting;
        float amplitude = Mathf.Lerp(1f, locomotionAmplitudeScale, locomotion);
        if (sprinting)
        {
            amplitude *= sprintAmplitudeScale;
        }

        Vector2 desired = targetOffset * Mathf.Clamp01(amplitude);
        float response = 1f - Mathf.Exp(-Mathf.Max(0.01f, gazeResponse) * Time.deltaTime);
        smoothedOffset = Vector2.Lerp(smoothedOffset, desired, response);
    }

    private void LateUpdate()
    {
        if (!resolved || !HasEyeBones || (ragdollController != null && ragdollController.RagdollActive))
        {
            return;
        }

        Quaternion additive =
            Quaternion.AngleAxis(smoothedOffset.x, transform.up) *
            Quaternion.AngleAxis(-smoothedOffset.y, transform.right);

        // Both eyes receive the same tiny world-space correction. Avoid artificial convergence
        // because eye separation / forward axes are model-specific and optional Humanoid data.
        leftEye.rotation = additive * leftEye.rotation;
        rightEye.rotation = additive * rightEye.rotation;
        lastLeftAdditive = additive;
        lastRightAdditive = additive;
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
        if (ragdollController == null)
        {
            ragdollController = GetComponent<DeathRagdollController>();
        }
    }

    private void ResolveEyes()
    {
        resolved = false;
        leftEye = null;
        rightEye = null;
        if (visualDriver == null)
        {
            return;
        }

        visualDriver.ResolveNow();
        if (!visualDriver.IsHumanoidBound)
        {
            return;
        }

        visualDriver.TryGetVisualBone(HumanBodyBones.LeftEye, out leftEye);
        visualDriver.TryGetVisualBone(HumanBodyBones.RightEye, out rightEye);

        // Eye mappings are optional in Unity Humanoid. Resolve even when absent so this driver
        // becomes a cheap no-op rather than repeatedly searching every frame.
        resolved = true;
    }

    private void ChooseSaccadeTarget()
    {
        if (Random.value < Mathf.Clamp01(returnToCenterBias))
        {
            targetOffset = Vector2.zero;
            return;
        }

        float yaw = Random.Range(-Mathf.Abs(maxYawDegrees), Mathf.Abs(maxYawDegrees));
        float pitch = Random.Range(-Mathf.Abs(maxPitchDegrees), Mathf.Abs(maxPitchDegrees));
        targetOffset = new Vector2(yaw, pitch);
    }

    private void ScheduleSaccade()
    {
        float minimum = Mathf.Max(0.25f, Mathf.Min(saccadeIntervalRange.x, saccadeIntervalRange.y));
        float maximum = Mathf.Max(minimum + 0.05f, Mathf.Max(saccadeIntervalRange.x, saccadeIntervalRange.y));
        nextSaccadeAt = Time.time + Random.Range(minimum, maximum);
    }

    private void RemovePreviousAdditives()
    {
        if (leftEye != null && Quaternion.Angle(lastLeftAdditive, Quaternion.identity) > 0.0001f)
        {
            leftEye.rotation = Quaternion.Inverse(lastLeftAdditive) * leftEye.rotation;
        }
        if (rightEye != null && Quaternion.Angle(lastRightAdditive, Quaternion.identity) > 0.0001f)
        {
            rightEye.rotation = Quaternion.Inverse(lastRightAdditive) * rightEye.rotation;
        }

        lastLeftAdditive = Quaternion.identity;
        lastRightAdditive = Quaternion.identity;
    }
}
