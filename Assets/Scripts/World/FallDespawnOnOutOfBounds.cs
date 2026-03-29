using UnityEngine;

[DisallowMultipleComponent]
public class FallDespawnOnOutOfBounds : MonoBehaviour
{
    [SerializeField] private float despawnHeight = -12f;
    [SerializeField] private bool disableInsteadOfDestroy;

    private bool processed;

    public void Configure(float configuredDespawnHeight, bool disableObject)
    {
        despawnHeight = configuredDespawnHeight;
        disableInsteadOfDestroy = disableObject;
    }

    private void Update()
    {
        if (processed || transform.position.y >= despawnHeight)
        {
            return;
        }

        processed = true;
        if (disableInsteadOfDestroy)
        {
            gameObject.SetActive(false);
            return;
        }

        Destroy(gameObject);
    }
}
