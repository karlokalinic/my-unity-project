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
        float minimumFacingDot,
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
                float score = ScoreCandidate(playerTransform, viewCamera, candidateCollider.bounds.center, interactable.InteractionRadius, minimumFacingDot) + interactable.GetPriority(interactor) * 10f;
                if (score > bestScore && PassesLineOfSight(viewCamera, interactionMask, interactable, candidateCollider))
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
                float score = ScoreCandidate(playerTransform, viewCamera, candidateCollider.bounds.center, interactRadius, minimumFacingDot);
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
        float minimumFacingDot)
    {
        Vector3 toTarget = candidatePosition - playerTransform.position;
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
        LayerMask interactionMask,
        InteractableBase interactable,
        Collider candidateCollider)
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
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance + 0.1f, interactionMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.transform == candidateCollider.transform || hit.collider.transform.IsChildOf(interactable.transform);
        }

        return true;
    }
}
