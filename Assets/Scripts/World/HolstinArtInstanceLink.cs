using UnityEngine;

[DisallowMultipleComponent]
public class HolstinArtInstanceLink : MonoBehaviour
{
    [SerializeField] private AssetPlaceholder sourcePlaceholder;
    [SerializeField] private string sourceTag;

    public AssetPlaceholder SourcePlaceholder => sourcePlaceholder;
    public string SourceTag => sourceTag;

    public void Bind(AssetPlaceholder placeholder)
    {
        sourcePlaceholder = placeholder;
        sourceTag = placeholder != null ? placeholder.AssetTag : string.Empty;
    }
}
