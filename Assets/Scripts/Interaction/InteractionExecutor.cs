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

        TryLegacyInteractRaycast(viewCamera, interactDistance, interactionMask, interactor, inspectViewer);
    }

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
