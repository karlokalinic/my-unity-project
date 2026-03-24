using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupInteractable : InteractableBase
{
    [Header("Item")]
    [SerializeField] private string itemId = "old_key";
    [SerializeField] private string itemDisplayName = "Old Key";
    [SerializeField] [TextArea(2, 4)] private string pickupDescription = "A corroded key with a district stamp.";
    [SerializeField] private string infectionMilestoneOnPickup;

    [Header("Animation")]
    [SerializeField] private float pickupDuration = 0.45f;
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool destroyAfterPickup = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] [Range(0f, 1f)] private float pickupSoundVolume = 1f;

    private bool isPickingUp;
    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;
    private PlayerInteraction activeInteractor;

    public void ConfigureItem(string newItemId, string newItemDisplayName, string newDescription, string infectionMilestone = "")
    {
        if (!string.IsNullOrWhiteSpace(newItemId))
        {
            itemId = newItemId;
        }

        if (!string.IsNullOrWhiteSpace(newItemDisplayName))
        {
            itemDisplayName = newItemDisplayName;
        }

        if (!string.IsNullOrWhiteSpace(newDescription))
        {
            pickupDescription = newDescription;
        }

        infectionMilestoneOnPickup = infectionMilestone;
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        return $"[{InputReader.InteractKeyLabel}] Pick up {itemDisplayName}";
    }

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (interactor == null || inventory == null || isPickingUp)
        {
            return;
        }

        activeInteractor = interactor;
        StartCoroutine(PickupRoutine(interactor, inventory));
    }

    private IEnumerator PickupRoutine(PlayerInteraction interactor, InventorySystem inventory)
    {
        isPickingUp = true;
        interactor.SetBusy(true);
        CacheComponents();
        SetCollidersEnabled(false);

        Transform followTarget = interactor.GetPickupAnchor();
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;

        float elapsed = 0f;
        while (elapsed < pickupDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pickupDuration);
            float curveT = moveCurve != null ? moveCurve.Evaluate(t) : t;
            Vector3 targetPosition = followTarget != null ? followTarget.position : interactor.transform.position + Vector3.up * 1.5f;
            transform.position = Vector3.Lerp(startPosition, targetPosition, curveT);
            transform.rotation = startRotation * Quaternion.Euler(0f, spinSpeed * elapsed, 0f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, curveT);
            yield return null;
        }

        inventory.AddItem(itemId, itemDisplayName, 1);
        interactor.ShowTransientMessage($"Picked up {itemDisplayName}\n{pickupDescription}", 2.8f);
        HolstinAudio.PlayOneShot(pickupSound, transform, pickupSoundVolume);

        if (!string.IsNullOrWhiteSpace(infectionMilestoneOnPickup))
        {
            InfectionDirector.NotifyMilestoneGlobal(infectionMilestoneOnPickup);
        }

        interactor.SetBusy(false);
        activeInteractor = null;
        isPickingUp = false;

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            SetRenderersEnabled(false);
            enabled = false;
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ReleaseBusyIfNeeded();
    }

    private void OnDestroy()
    {
        ReleaseBusyIfNeeded();
    }

    private void ReleaseBusyIfNeeded()
    {
        if (!isPickingUp)
        {
            return;
        }

        if (activeInteractor != null)
        {
            activeInteractor.SetBusy(false);
        }

        activeInteractor = null;
        isPickingUp = false;
    }

    private void CacheComponents()
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
        }

        if (cachedColliders == null || cachedColliders.Length == 0)
        {
            cachedColliders = GetComponentsInChildren<Collider>(true);
        }
    }

    private void SetRenderersEnabled(bool visible)
    {
        if (cachedRenderers == null)
        {
            return;
        }

        foreach (Renderer rendererComponent in cachedRenderers)
        {
            if (rendererComponent != null)
            {
                rendererComponent.enabled = visible;
            }
        }
    }

    private void SetCollidersEnabled(bool visible)
    {
        if (cachedColliders == null)
        {
            return;
        }

        foreach (Collider colliderComponent in cachedColliders)
        {
            if (colliderComponent != null)
            {
                colliderComponent.enabled = visible;
            }
        }
    }
}
