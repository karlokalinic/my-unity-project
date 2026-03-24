using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointZone : MonoBehaviour
{
    [SerializeField] private string checkpointLabel = "Checkpoint updated.";
    [SerializeField] private bool activateOnlyOnce = true;
    [SerializeField] private bool disableColliderAfterActivation = true;

    private bool hasActivated;
    private CheckpointLiteManager cachedCheckpointManager;

    public void Configure(string label, bool oneShot = true)
    {
        if (!string.IsNullOrWhiteSpace(label))
        {
            checkpointLabel = label;
        }

        activateOnlyOnce = oneShot;
    }

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void Awake()
    {
        ResolveCheckpointManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated && activateOnlyOnce)
        {
            return;
        }

        PlayerMover mover = other.GetComponentInParent<PlayerMover>();
        if (mover == null)
        {
            return;
        }

        CheckpointLiteManager checkpointManager = mover.GetComponent<CheckpointLiteManager>();
        if (checkpointManager == null)
        {
            checkpointManager = ResolveCheckpointManager();
        }

        if (checkpointManager == null)
        {
            return;
        }

        checkpointManager.SetCheckpoint(transform, checkpointLabel);
        hasActivated = true;

        if (activateOnlyOnce && disableColliderAfterActivation && TryGetComponent(out Collider trigger))
        {
            trigger.enabled = false;
        }
    }

    private CheckpointLiteManager ResolveCheckpointManager()
    {
        if (cachedCheckpointManager != null)
        {
            return cachedCheckpointManager;
        }

        if (HolstinSceneContext.TryGet(out HolstinSceneContext context) && context.PlayerMover != null)
        {
            cachedCheckpointManager = context.PlayerMover.GetComponent<CheckpointLiteManager>();
            if (cachedCheckpointManager != null)
            {
                return cachedCheckpointManager;
            }
        }

        cachedCheckpointManager = FindAnyObjectByType<CheckpointLiteManager>();
        return cachedCheckpointManager;
    }
}
