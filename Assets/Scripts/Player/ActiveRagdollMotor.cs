using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(ProceduralHumanoidRig))]
public class ActiveRagdollMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralHumanoidRig rig;
    [SerializeField] private Transform rootFollow;

    [Header("Drive")]
    [SerializeField] private float positionSpring = 900f;
    [SerializeField] private float positionDamping = 62f;
    [SerializeField] private float rotationSpring = 250f;
    [SerializeField] private float rotationDamping = 18f;
    [SerializeField] private float maxAngularVelocity = 22f;
    [SerializeField] private float maxLinearVelocity = 10f;

    [Header("Root")]
    [SerializeField] private bool alignRootRotation = true;

    private ProceduralHumanoidRig.BoneBinding rootBinding;
    private bool hasRootBinding;

    private void Awake()
    {
        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }

        if (rootFollow == null)
        {
            rootFollow = transform;
        }

        EnsureRig();
    }

    private void FixedUpdate()
    {
        if (rig == null)
        {
            return;
        }

        EnsureRig();
        SyncRoot();
        DriveChildrenTowardTargets();
    }

    private void EnsureRig()
    {
        rig.EnsureBuilt();
        hasRootBinding = rig.TryGetBinding("Hips", out rootBinding) && rootBinding != null;
    }

    private void SyncRoot()
    {
        if (!hasRootBinding || rootBinding.body == null || rootBinding.physical == null || rootFollow == null)
        {
            return;
        }

        Rigidbody rootBody = rootBinding.body;
        if (!rootBody.isKinematic)
        {
            rootBody.isKinematic = true;
        }

        Vector3 rootWorld = rootFollow.TransformPoint(rootBinding.physical.localPosition);
        rootBody.MovePosition(rootWorld);
        if (alignRootRotation)
        {
            rootBody.MoveRotation(rootFollow.rotation);
        }
    }

    private void DriveChildrenTowardTargets()
    {
        ProceduralHumanoidRig.BoneBinding[] bindings = rig.Bindings;
        if (bindings == null || bindings.Length == 0)
        {
            return;
        }

        for (int i = 0; i < bindings.Length; i++)
        {
            ProceduralHumanoidRig.BoneBinding binding = bindings[i];
            if (binding == null || string.IsNullOrWhiteSpace(binding.parentBoneName))
            {
                continue;
            }

            if (binding.body == null || binding.target == null)
            {
                continue;
            }

            Rigidbody body = binding.body;
            body.isKinematic = false;
            body.useGravity = false;

            Vector3 positionError = binding.target.position - body.position;
            Vector3 desiredForce = (positionError * positionSpring) - (body.linearVelocity * positionDamping);
            body.AddForce(desiredForce, ForceMode.Acceleration);

            Quaternion targetRotation = binding.target.rotation;
            Quaternion delta = targetRotation * Quaternion.Inverse(body.rotation);
            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (!float.IsNaN(axis.x) && axis.sqrMagnitude > 0.0001f)
            {
                Vector3 angularError = axis.normalized * (angle * Mathf.Deg2Rad);
                Vector3 desiredTorque = (angularError * rotationSpring) - (body.angularVelocity * rotationDamping);
                body.AddTorque(desiredTorque, ForceMode.Acceleration);
            }

            body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, maxAngularVelocity);
            body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, maxLinearVelocity);
        }
    }
}
