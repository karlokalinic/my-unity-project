using UnityEngine;

public static class ReachTargetResolver
{
    public static bool TryResolveTarget(
        PlayerInteraction interactor,
        InteractableBase interactable,
        InspectableItem inspectable,
        out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;

        if (interactable != null && interactable.TryGetReachTarget(interactor, out worldPoint))
        {
            return true;
        }

        if (inspectable != null)
        {
            if (TryResolveFromCollider(inspectable.GetComponentInChildren<Collider>(), out worldPoint))
            {
                return true;
            }

            worldPoint = inspectable.transform.position + Vector3.up * 1.1f;
            return true;
        }

        return false;
    }

    public static bool TryResolveFromCollider(Collider candidate, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        if (candidate == null)
        {
            return false;
        }

        Bounds bounds = candidate.bounds;
        worldPoint = bounds.center;
        worldPoint.y = Mathf.Lerp(bounds.min.y, bounds.max.y, 0.68f);
        return true;
    }
}

public class ReachTargetAnchor : MonoBehaviour
{
    [SerializeField] private Transform explicitAnchor;

    public bool TryGetPoint(out Vector3 point)
    {
        if (explicitAnchor != null)
        {
            point = explicitAnchor.position;
            return true;
        }

        Collider colliderComponent = GetComponentInChildren<Collider>();
        if (ReachTargetResolver.TryResolveFromCollider(colliderComponent, out point))
        {
            return true;
        }

        point = transform.position + Vector3.up;
        return true;
    }
}
