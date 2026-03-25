using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Damageable))]
public class DeathRagdollController : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool enableForPlayer = true;
    [SerializeField] private bool disableCharacterControllerOnDeath = true;
    [SerializeField] private bool disableNavMeshAgentOnDeath = true;
    [SerializeField] private bool disableRootCollidersOnDeath = true;
    [SerializeField] [Range(0f, 1f)] private float inheritedVelocityScale = 0.75f;

    private Damageable damageable;
    private CharacterStats stats;
    private ProceduralHumanoidRig rig;
    private ActiveRagdollMotor activeRagdollMotor;
    private CharacterController characterController;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Collider[] rootColliders = System.Array.Empty<Collider>();
    private bool[] rootColliderEnabledStates = System.Array.Empty<bool>();
    private bool ragdollActive;

    public bool RagdollActive => ragdollActive;
    public bool HasRig => rig != null || GetComponent<ProceduralHumanoidRig>() != null;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        stats = GetComponent<CharacterStats>();
        rig = GetComponent<ProceduralHumanoidRig>();
        activeRagdollMotor = GetComponent<ActiveRagdollMotor>();
        characterController = GetComponent<CharacterController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (rig != null)
        {
            rig.EnsureBuilt();
            rig.ConfigureRendererVisibility(false, false);
        }

        rootColliders = GetComponents<Collider>();
        rootColliderEnabledStates = new bool[rootColliders.Length];
        for (int i = 0; i < rootColliders.Length; i++)
        {
            rootColliderEnabledStates[i] = rootColliders[i] != null && rootColliders[i].enabled;
        }

        if (damageable != null)
        {
            damageable.Died += HandleDied;
        }

        if (stats != null)
        {
            stats.Revived += HandleRevived;
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.Died -= HandleDied;
        }

        if (stats != null)
        {
            stats.Revived -= HandleRevived;
        }
    }

    [ContextMenu("Activate Death Ragdoll")]
    public void ActivateRagdoll()
    {
        if (ragdollActive)
        {
            return;
        }

        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }

        if (rig == null)
        {
            return;
        }

        rig.EnsureBuilt();
        rig.SetRagdollRenderersVisible(true);

        Vector3 inheritedVelocity = Vector3.zero;
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            inheritedVelocity = navMeshAgent.velocity * inheritedVelocityScale;
        }

        if (disableNavMeshAgentOnDeath && navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.enabled = false;
        }

        if (activeRagdollMotor != null)
        {
            activeRagdollMotor.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (disableCharacterControllerOnDeath && characterController != null)
        {
            characterController.enabled = false;
        }

        if (disableRootCollidersOnDeath)
        {
            for (int i = 0; i < rootColliders.Length; i++)
            {
                Collider colliderComponent = rootColliders[i];
                if (colliderComponent != null)
                {
                    colliderComponent.enabled = false;
                }
            }
        }

        ProceduralHumanoidRig.BoneBinding[] bindings = rig.Bindings;
        if (bindings != null)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                ProceduralHumanoidRig.BoneBinding binding = bindings[i];
                if (binding?.body == null)
                {
                    continue;
                }

                Rigidbody body = binding.body;
                body.isKinematic = false;
                body.useGravity = true;
                body.linearVelocity = inheritedVelocity;
            }
        }

        ragdollActive = true;
    }

    [ContextMenu("Restore From Ragdoll")]
    public void RestoreFromRagdoll()
    {
        if (!ragdollActive)
        {
            return;
        }

        if (rig == null)
        {
            rig = GetComponent<ProceduralHumanoidRig>();
        }

        if (rig == null)
        {
            ragdollActive = false;
            return;
        }

        rig.ConfigureRendererVisibility(rig.HideSourceRenderers, rig.ShowRagdollRenderersInLife);

        ProceduralHumanoidRig.BoneBinding[] bindings = rig.Bindings;
        if (bindings != null)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                ProceduralHumanoidRig.BoneBinding binding = bindings[i];
                if (binding?.body == null)
                {
                    continue;
                }

                Rigidbody body = binding.body;
                bool isRoot = string.IsNullOrWhiteSpace(binding.parentBoneName);
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = isRoot;
            }
        }

        if (disableRootCollidersOnDeath)
        {
            for (int i = 0; i < rootColliders.Length; i++)
            {
                Collider colliderComponent = rootColliders[i];
                if (colliderComponent != null)
                {
                    colliderComponent.enabled = i < rootColliderEnabledStates.Length && rootColliderEnabledStates[i];
                }
            }
        }

        if (disableCharacterControllerOnDeath && characterController != null)
        {
            characterController.enabled = true;
        }

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (activeRagdollMotor != null)
        {
            activeRagdollMotor.enabled = true;
        }

        if (disableNavMeshAgentOnDeath && navMeshAgent != null && !navMeshAgent.enabled &&
            NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            navMeshAgent.enabled = true;
            navMeshAgent.Warp(hit.position);
        }

        ragdollActive = false;
    }

    private void HandleDied()
    {
        if (!enableForPlayer && GetComponent<PlayerMover>() != null)
        {
            return;
        }

        ActivateRagdoll();
    }

    private void HandleRevived()
    {
        RestoreFromRagdoll();
    }
}
