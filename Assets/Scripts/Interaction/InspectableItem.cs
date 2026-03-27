using UnityEngine;

public class InspectableItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Inspectable Item";
    [SerializeField] [TextArea(2, 5)] private string itemDescription = "A useful greybox stand-in for a lore object.";
    [SerializeField] private string milestoneOnInspect;
    [SerializeField] private Vector3 previewLocalPosition = new Vector3(0f, 0f, 1.25f);
    [SerializeField] private Vector3 previewLocalEuler = new Vector3(12f, -30f, 0f);
    [SerializeField] private float previewScale = 1.25f;
    [SerializeField] private float autoRotateSpeed = 35f;

    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;

    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public string MilestoneOnInspect => milestoneOnInspect;
    public Vector3 PreviewLocalPosition => previewLocalPosition;
    public Vector3 PreviewLocalEuler => previewLocalEuler;
    public float PreviewScale => previewScale;
    public float AutoRotateSpeed => autoRotateSpeed;


    public void Configure(string newName, string newDescription)
    {
        itemName = newName;
        itemDescription = newDescription;
    }

    public void ConfigureMilestone(string milestoneId)
    {
        milestoneOnInspect = milestoneId;
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
    }

    public void SetWorldVisible(bool visible)
    {
        if (cachedRenderers == null || cachedColliders == null)
        {
            CacheComponents();
        }

        foreach (Renderer rendererComponent in cachedRenderers)
        {
            if (rendererComponent != null)
            {
                rendererComponent.enabled = visible;
            }
        }

        foreach (Collider colliderComponent in cachedColliders)
        {
            if (colliderComponent != null)
            {
                colliderComponent.enabled = visible;
            }
        }
    }

    private void CacheComponents()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
    }
}
