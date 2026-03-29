using UnityEngine;

public class InteractionScanner
{
    private readonly Collider[] overlapResults;

    public InteractionScanner(Collider[] resultBuffer)
    {
        overlapResults = resultBuffer;
    }

    public void FindBestContextCandidate(
        Transform playerTransform,
        Camera viewCamera,
        PlayerInteraction interactor,
        InventorySystem inventory,
        float interactRadius,
        LayerMask interactionMask,
        LayerMask visibilityMask,
        float minimumFacingDot,
        float maxVerticalDelta,
        out InteractableBase bestInteractable,
        out InspectableItem bestInspectable)
    {
        bestInteractable = null;
        bestInspectable = null;

        if (playerTransform == null || viewCamera == null || overlapResults == null)
        {
            return;
        }

        Vector3 origin = playerTransform.position + Vector3.up * 1f;
        int count = Physics.OverlapSphereNonAlloc(origin, interactRadius, overlapResults, interactionMask, QueryTriggerInteraction.Collide);
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < count; i++)
        {
            Collider candidateCollider = overlapResults[i];
            if (candidateCollider == null)
            {
                continue;
            }

            InteractableBase interactable = candidateCollider.GetComponentInParent<InteractableBase>();
            if (interactable != null && interactable.IsAvailable(interactor, inventory))
            {
                float score = ScoreCandidate(playerTransform, viewCamera, candidateCollider.bounds.center, interactable.InteractionRadius, minimumFacingDot, maxVerticalDelta) + interactable.GetPriority(interactor) * 10f;
                if (score > bestScore && PassesLineOfSight(viewCamera, visibilityMask, interactable, candidateCollider, playerTransform))
                {
                    bestScore = score;
                    bestInteractable = interactable;
                    bestInspectable = null;
                }

                continue;
            }

            InspectableItem inspectable = candidateCollider.GetComponentInParent<InspectableItem>();
            if (inspectable != null)
            {
                float score = ScoreCandidate(playerTransform, viewCamera, candidateCollider.bounds.center, interactRadius, minimumFacingDot, maxVerticalDelta);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestInteractable = null;
                    bestInspectable = inspectable;
                }
            }
        }
    }

    private static float ScoreCandidate(
        Transform playerTransform,
        Camera viewCamera,
        Vector3 candidatePosition,
        float candidateRadius,
        float minimumFacingDot,
        float maxVerticalDelta)
    {
        Vector3 toTarget = candidatePosition - playerTransform.position;
        if (Mathf.Abs(toTarget.y) > Mathf.Max(0.1f, maxVerticalDelta))
        {
            return float.NegativeInfinity;
        }

        float planarDistance = Vector3.ProjectOnPlane(toTarget, Vector3.up).magnitude;
        if (planarDistance > candidateRadius)
        {
            return float.NegativeInfinity;
        }

        Vector3 forward = viewCamera != null ? viewCamera.transform.forward : playerTransform.forward;
        float facing = Vector3.Dot(forward.normalized, toTarget.normalized);
        if (facing < minimumFacingDot)
        {
            return float.NegativeInfinity;
        }

        return (facing * 10f) - planarDistance;
    }

    private static bool PassesLineOfSight(
        Camera viewCamera,
        LayerMask visibilityMask,
        InteractableBase interactable,
        Collider candidateCollider,
        Transform playerTransform)
    {
        if (!interactable.RequireLineOfSight || viewCamera == null)
        {
            return true;
        }

        Vector3 origin = viewCamera.transform.position;
        Vector3 targetPoint = candidateCollider.bounds.center;
        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
        {
            return true;
        }

        direction /= distance;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance + 0.1f, visibilityMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            return true;
        }

        System.Array.Sort(hits, static (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            Transform hitTransform = hitCollider.transform;
            if (playerTransform != null && hitTransform.IsChildOf(playerTransform))
            {
                continue;
            }

            return hitTransform == candidateCollider.transform || hitTransform.IsChildOf(interactable.transform);
        }

        return true;
    }
}
