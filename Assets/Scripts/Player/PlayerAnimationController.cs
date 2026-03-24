using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public class PlayerAnimationController : MonoBehaviour
{
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

    [Header("Locomotion")]
    [SerializeField] private float walkArmSwing = 22f;
    [SerializeField] private float sprintArmSwing = 32f;
    [SerializeField] private float walkLegSwing = 26f;
    [SerializeField] private float sprintLegSwing = 38f;
    [SerializeField] private float gaitFrequency = 8.2f;
    [SerializeField] private float poseLerpSpeed = 14f;

    [Header("Reach")]
    [SerializeField] private float reachUpperArmPitch = -42f;
    [SerializeField] private float reachLowerArmPitch = -64f;
    [SerializeField] private float reachHandPitch = 10f;
    [SerializeField] private float reachInfluenceSpeed = 7f;

    private readonly Dictionary<string, Quaternion> baseLocalRotations = new Dictionary<string, Quaternion>();
    private float gaitTime;
    private float reachWeight;
    private bool reaching;
    private Vector3 reachWorldPoint;

    private Transform chestTarget;
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
            animator = GetComponentInChildren<Animator>();
        }

        EnsureBoneCache();
    }

    private void Update()
    {
        EnsureBoneCache();
        if (rig == null)
        {
            return;
        }

        float planarSpeed = playerMover != null ? playerMover.CurrentPlanarSpeed : 0f;
        bool sprinting = InputReader.SprintHeld() && planarSpeed > 0.12f;

        MotionState state = ResolveState(planarSpeed, sprinting);
        UpdateAnimator(state, planarSpeed, sprinting);
        UpdatePose(state, planarSpeed, sprinting);
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

    private void UpdateAnimator(MotionState state, float speed, bool sprinting)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat("moveSpeed", speed);
        animator.SetBool("isSprinting", sprinting);
        animator.SetBool("isReaching", state == MotionState.Reach);
        animator.SetInteger("state", (int)state);
    }

    private void UpdatePose(MotionState state, float speed, bool sprinting)
    {
        float targetReachWeight = state == MotionState.Reach ? 1f : 0f;
        reachWeight = Mathf.MoveTowards(reachWeight, targetReachWeight, reachInfluenceSpeed * Time.deltaTime);

        float locomotionWeight = Mathf.Clamp01(speed / 4.6f);
        float legSwing = Mathf.Lerp(walkLegSwing, sprintLegSwing, sprinting ? 1f : 0f) * locomotionWeight;
        float armSwing = Mathf.Lerp(walkArmSwing, sprintArmSwing, sprinting ? 1f : 0f) * locomotionWeight;

        gaitTime += Time.deltaTime * gaitFrequency * Mathf.Lerp(0.3f, 1.8f, locomotionWeight);
        float gaitSin = Mathf.Sin(gaitTime);
        float gaitCos = Mathf.Cos(gaitTime);

        SetLocalRotation(leftUpperLeg, "LeftUpperLeg", Quaternion.Euler(gaitSin * legSwing, 0f, 0f));
        SetLocalRotation(rightUpperLeg, "RightUpperLeg", Quaternion.Euler(-gaitSin * legSwing, 0f, 0f));
        SetLocalRotation(leftLowerLeg, "LeftLowerLeg", Quaternion.Euler(Mathf.Max(0f, -gaitSin) * (legSwing * 0.7f), 0f, 0f));
        SetLocalRotation(rightLowerLeg, "RightLowerLeg", Quaternion.Euler(Mathf.Max(0f, gaitSin) * (legSwing * 0.7f), 0f, 0f));
        SetLocalRotation(leftFoot, "LeftFoot", Quaternion.Euler(-gaitSin * (legSwing * 0.25f), 0f, 0f));
        SetLocalRotation(rightFoot, "RightFoot", Quaternion.Euler(gaitSin * (legSwing * 0.25f), 0f, 0f));

        SetLocalRotation(leftUpperArm, "LeftUpperArm", Quaternion.Euler(-gaitSin * armSwing, 0f, 0f));
        SetLocalRotation(rightUpperArm, "RightUpperArm", Quaternion.Euler(gaitSin * armSwing, 0f, 0f));
        SetLocalRotation(leftLowerArm, "LeftLowerArm", Quaternion.Euler(-Mathf.Max(0f, gaitSin) * (armSwing * 0.22f), 0f, 0f));
        SetLocalRotation(rightLowerArm, "RightLowerArm", Quaternion.Euler(-Mathf.Max(0f, -gaitSin) * (armSwing * 0.22f), 0f, 0f));
        SetLocalRotation(leftHand, "LeftHand", Quaternion.identity);
        SetLocalRotation(rightHand, "RightHand", Quaternion.identity);

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
        bone.localRotation = Quaternion.Slerp(locomotionPose, blended, poseLerpSpeed * Time.deltaTime);
    }

    private void SetLocalRotation(Transform bone, string key, Quaternion additivePose)
    {
        if (bone == null || !baseLocalRotations.TryGetValue(key, out Quaternion baseRotation))
        {
            return;
        }

        Quaternion targetRotation = baseRotation * additivePose;
        bone.localRotation = Quaternion.Slerp(bone.localRotation, targetRotation, poseLerpSpeed * Time.deltaTime);
    }

    private void EnsureBoneCache()
    {
        if (rig == null)
        {
            return;
        }

        rig.EnsureBuilt();
        if (baseLocalRotations.Count > 0 && chestTarget != null)
        {
            return;
        }

        chestTarget = rig.GetBone("Chest", true);
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

        CacheBaseRotation("LeftUpperArm", leftUpperArm);
        CacheBaseRotation("RightUpperArm", rightUpperArm);
        CacheBaseRotation("LeftLowerArm", leftLowerArm);
        CacheBaseRotation("RightLowerArm", rightLowerArm);
        CacheBaseRotation("LeftHand", leftHand);
        CacheBaseRotation("RightHand", rightHand);
        CacheBaseRotation("LeftUpperLeg", leftUpperLeg);
        CacheBaseRotation("RightUpperLeg", rightUpperLeg);
        CacheBaseRotation("LeftLowerLeg", leftLowerLeg);
        CacheBaseRotation("RightLowerLeg", rightLowerLeg);
        CacheBaseRotation("LeftFoot", leftFoot);
        CacheBaseRotation("RightFoot", rightFoot);
    }

    private void CacheBaseRotation(string key, Transform bone)
    {
        if (bone == null)
        {
            return;
        }

        baseLocalRotations[key] = bone.localRotation;
    }
}
