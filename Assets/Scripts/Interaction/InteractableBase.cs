using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    [SerializeField] private string interactionLabel = "Interact";
    [SerializeField] private string subjectLabel = "Object";
    [SerializeField] private float interactionRadius = 2.2f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private bool disableAfterUse;

    private bool hasBeenConsumed;

    public float InteractionRadius => interactionRadius;
    public bool RequireLineOfSight => requireLineOfSight;
    public bool HasBeenConsumed => hasBeenConsumed;

    public virtual string GetPrompt(PlayerInteraction interactor, PlayerInventory inventory)
    {
        return $"[{InputReader.InteractKeyLabel}] {interactionLabel} {subjectLabel}";
    }

    public virtual bool IsAvailable(PlayerInteraction interactor, PlayerInventory inventory)
    {
        return enabled && gameObject.activeInHierarchy && !hasBeenConsumed;
    }

    public virtual int GetPriority(PlayerInteraction interactor)
    {
        return 0;
    }

    public virtual bool TryGetReachTarget(PlayerInteraction interactor, out Vector3 worldPoint)
    {
        ReachTargetAnchor explicitAnchor = GetComponentInChildren<ReachTargetAnchor>();
        if (explicitAnchor != null && explicitAnchor.TryGetPoint(out worldPoint))
        {
            return true;
        }

        Collider colliderComponent = GetComponentInChildren<Collider>();
        if (ReachTargetResolver.TryResolveFromCollider(colliderComponent, out worldPoint))
        {
            return true;
        }

        worldPoint = transform.position + Vector3.up;
        return true;
    }

    public abstract void Interact(PlayerInteraction interactor, PlayerInventory inventory);

    protected void MarkConsumedIfNeeded()
    {
        if (!disableAfterUse)
        {
            return;
        }

        hasBeenConsumed = true;
        enabled = false;
    }

    public virtual void ForceRefreshPrompt(PlayerInteraction interactor, PlayerInventory inventory)
    {
        if (interactor != null)
        {
            interactor.RefreshContextPrompt();
        }
    }
}
