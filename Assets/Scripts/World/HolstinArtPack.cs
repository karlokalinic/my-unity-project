using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Holstin/Art Pack", fileName = "HolstinArtPack")]
public class HolstinArtPack : ScriptableObject
{
    [Serializable]
    public class PlaceholderMapping
    {
        public string id = "mapping";
        public AssetPlaceholder.PlaceholderCategory category = AssetPlaceholder.PlaceholderCategory.Prop;
        public string assetTag = string.Empty;
        public GameObject prefab;
        public Vector3 localPositionOffset = Vector3.zero;
        public Vector3 localRotationEuler = Vector3.zero;
        public Vector3 localScaleMultiplier = Vector3.one;
        public bool fitToPlaceholderBounds = true;
        public bool preservePlaceholder = true;
        public bool setStaticFlags = true;
        public Material[] materialOverrides = Array.Empty<Material>();
    }

    [SerializeField] private bool caseInsensitiveTags = true;
    [SerializeField] private bool allowCategoryFallback = true;
    [SerializeField] private List<PlaceholderMapping> mappings = new List<PlaceholderMapping>();

    public IReadOnlyList<PlaceholderMapping> Mappings => mappings;
    public bool CaseInsensitiveTags => caseInsensitiveTags;
    public bool AllowCategoryFallback => allowCategoryFallback;

    public bool TryFindMapping(AssetPlaceholder placeholder, out PlaceholderMapping mapping)
    {
        mapping = null;
        if (placeholder == null || mappings == null || mappings.Count == 0)
        {
            return false;
        }

        StringComparison comparison = caseInsensitiveTags ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        string tag = placeholder.AssetTag ?? string.Empty;
        bool hasTag = !string.IsNullOrWhiteSpace(tag);

        // Exact category + tag match first.
        for (int i = 0; i < mappings.Count; i++)
        {
            PlaceholderMapping candidate = mappings[i];
            if (candidate == null || candidate.prefab == null)
            {
                continue;
            }

            if (candidate.category != placeholder.Category)
            {
                continue;
            }

            bool candidateHasTag = !string.IsNullOrWhiteSpace(candidate.assetTag);
            if (!hasTag && !candidateHasTag)
            {
                mapping = candidate;
                return true;
            }

            if (hasTag && candidateHasTag && string.Equals(candidate.assetTag, tag, comparison))
            {
                mapping = candidate;
                return true;
            }
        }

        if (allowCategoryFallback)
        {
            // Category-only fallback.
            for (int i = 0; i < mappings.Count; i++)
            {
                PlaceholderMapping candidate = mappings[i];
                if (candidate == null || candidate.prefab == null)
                {
                    continue;
                }

                if (candidate.category == placeholder.Category && string.IsNullOrWhiteSpace(candidate.assetTag))
                {
                    mapping = candidate;
                    return true;
                }
            }
        }

        return false;
    }

#if UNITY_EDITOR
    public List<PlaceholderMapping> EditorMappings => mappings;
#endif
}
