using UnityEngine;

public class InteractionExecutor
{
    public void TryInteract(
        Camera viewCamera,
        float interactDistance,
        LayerMask interactionMask,
        PlayerInteraction interactor,
        InventorySystem inventory,
        InteractableBase currentInteractable,
        InspectableItem currentInspectable,
        InspectItemViewer inspectViewer)
    {
        if (interactor == null || viewCamera == null)
        {
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact(interactor, inventory);
            return;
        }

        if (currentInspectable != null)
        {
            BeginInspect(interactor, inspectViewer, currentInspectable);
            return;
        }

        // LEGACY KEEP REMOVE_LATER:
        // Emergency interaction path when scanner fails to resolve InteractableBase/InspectableItem.
        TryLegacyInteractRaycast(viewCamera, interactDistance, interactionMask, interactor, inspectViewer);
    }

    // LEGACY KEEP REMOVE_LATER:
    // Runtime emergency fallback raycast path. Primary path is InteractionScanner context selection.
    private static void TryLegacyInteractRaycast(
        Camera viewCamera,
        float interactDistance,
        LayerMask interactionMask,
        PlayerInteraction interactor,
        InspectItemViewer inspectViewer)
    {
        Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionMask, QueryTriggerInteraction.Collide))
        {
            return;
        }

        InspectableItem item = hit.collider.GetComponentInParent<InspectableItem>();
        if (item != null)
        {
            BeginInspect(interactor, inspectViewer, item);
            return;
        }

        NarrativeZone narrativeZone = hit.collider.GetComponentInParent<NarrativeZone>();
        if (narrativeZone != null)
        {
            narrativeZone.Reveal();
        }
    }

    private static void BeginInspect(PlayerInteraction interactor, InspectItemViewer inspectViewer, InspectableItem item)
    {
        if (inspectViewer != null)
        {
            inspectViewer.BeginInspect(item);
        }
        else
        {
            interactor.ShowTransientMessage("Inspect system is not available in this scene.", 2f);
        }
    }
}
