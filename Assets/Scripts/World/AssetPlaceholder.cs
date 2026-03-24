using UnityEngine;

/// <summary>
/// Marks a GameObject as a placeholder for a future asset-store model.
/// When you import real assets, find all AssetPlaceholders of a given
/// category and swap the prefab reference.
/// </summary>
public class AssetPlaceholder : MonoBehaviour
{
    public enum PlaceholderCategory
    {
        Furniture,
        Prop,
        StructuralDecor,
        Light,
        Character,
        Weapon,
        Container,
        Vegetation,
        Door,
        Custom
    }

    [Header("Asset Replacement")]
    [SerializeField] private PlaceholderCategory category = PlaceholderCategory.Prop;
    [SerializeField] private string assetTag = "";
    [SerializeField] private Vector3 boundsSize = Vector3.one;

    [Header("Visual")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.6f, 0.2f, 0.35f);

    public PlaceholderCategory Category => category;
    public string AssetTag => assetTag;
    public Vector3 BoundsSize => boundsSize;

    public void Configure(PlaceholderCategory cat, string tag, Vector3 bounds)
    {
        category = cat;
        assetTag = tag;
        boundsSize = bounds;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position + Vector3.up * boundsSize.y * 0.5f, boundsSize);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.08f);
        Gizmos.DrawCube(transform.position + Vector3.up * boundsSize.y * 0.5f, boundsSize);
    }
}
