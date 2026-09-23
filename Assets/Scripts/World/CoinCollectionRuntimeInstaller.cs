using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Installs the production coin trail in INTERAKCIJA and presents a lightweight local HUD.
/// Coordinates intentionally follow the existing exterior -> interior -> archive route.
/// </summary>
[DisallowMultipleComponent]
public sealed class CoinCollectionRuntimeInstaller : MonoBehaviour
{
    private const string SupportedScene = "INTERAKCIJA";
    private const string HostName = "__CoinCollectionRuntime";
    private const string CollectionMilestone = "coin_collection_complete";

    private static readonly Vector3[] RoutePoints =
    {
        // Follow the actual playable center lane. Keep pickups clear of the z = +/-6 perimeter
        // walls and place partition transitions near their real door openings.
        new Vector3(-7.2f, 0f, 1.7f),
        new Vector3(-4.8f, 0f, -1.7f),
        new Vector3(-2.8f, 0f, 0.0f),
        new Vector3(0.5f, 0f, 1.6f),
        new Vector3(4.5f, 0f, -1.7f),
        new Vector3(7.8f, 0f, 1.7f),
        new Vector3(11.0f, 0f, -1.7f),
        new Vector3(13.0f, 0f, 0.0f),
        new Vector3(16.4f, 0f, -2.0f),
        new Vector3(19.4f, 0f, 2.0f),
        new Vector3(21.0f, 0f, 0.0f),
        new Vector3(25.0f, 0f, -1.5f)
    };

    private static readonly RaycastHit[] GroundHits = new RaycastHit[24];

    private TMP_Text counterText;
    private Material coinMaterial;
    private int collectedCount;
    private int totalValue;
    private int targetCount;
    private bool completionAnnounced;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() ||
            !string.Equals(scene.name, SupportedScene, StringComparison.OrdinalIgnoreCase) ||
            GameObject.Find(HostName) != null)
        {
            return;
        }

        GameObject host = new GameObject(HostName);
        host.AddComponent<CoinCollectionRuntimeInstaller>();
    }

    private void Start()
    {
        CoinCollectible.Collected += HandleCoinCollected;
        coinMaterial = CreateCoinMaterial();
        BuildHud();
        SpawnRoute();
        RefreshHud();
    }

    private void OnDestroy()
    {
        CoinCollectible.Collected -= HandleCoinCollected;
        if (coinMaterial != null)
        {
            Destroy(coinMaterial);
            coinMaterial = null;
        }
    }

    private void SpawnRoute()
    {
        targetCount = RoutePoints.Length;
        for (int i = 0; i < RoutePoints.Length; i++)
        {
            Vector3 point = RoutePoints[i];
            float groundY = ResolveGroundY(point);
            CreateCoin(i, new Vector3(point.x, groundY + 0.48f, point.z));
        }

        Debug.Log($"[CoinCollectionRuntimeInstaller] COIN_TRAIL_READY count={targetCount}");
    }

    private void CreateCoin(int index, Vector3 worldPosition)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = $"Coin_{index + 1:00}";
        coin.transform.SetParent(transform, true);
        coin.transform.position = worldPosition;
        coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        coin.transform.localScale = new Vector3(0.19f, 0.035f, 0.19f);

        Renderer rendererComponent = coin.GetComponent<Renderer>();
        if (rendererComponent != null)
        {
            rendererComponent.sharedMaterial = coinMaterial;
            rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rendererComponent.receiveShadows = true;
        }

        Collider primitiveCollider = coin.GetComponent<Collider>();
        if (primitiveCollider != null)
        {
            primitiveCollider.enabled = false;
            Destroy(primitiveCollider);
        }

        MeshFilter filter = coin.GetComponent<MeshFilter>();
        MeshCollider meshCollider = coin.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = filter != null ? filter.sharedMesh : null;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        Rigidbody body = coin.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        CoinCollectible collectible = coin.AddComponent<CoinCollectible>();
        collectible.Configure(1, index * 0.37f);
    }

    private float ResolveGroundY(Vector3 point)
    {
        Vector3 origin = new Vector3(point.x, point.y + 8f, point.z);
        int hitCount = Physics.RaycastNonAlloc(
            origin,
            Vector3.down,
            GroundHits,
            20f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        float bestY = point.y;
        float bestDelta = float.PositiveInfinity;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = GroundHits[i];

            // Coins belong on walkable floor, not on top of perimeter walls, tables or props.
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.65f)
            {
                continue;
            }

            float delta = Mathf.Abs(hit.point.y - point.y);
            if (hit.point.y > point.y + 1.25f || hit.point.y < point.y - 2f || delta >= bestDelta)
            {
                continue;
            }

            bestDelta = delta;
            bestY = hit.point.y;
        }

        return bestY;
    }

    private void HandleCoinCollected(CoinCollectible collectible, int value)
    {
        collectedCount++;
        totalValue += Mathf.Max(1, value);
        RefreshHud();

        HolstinFeedback.ShowMessage(
            collectedCount >= targetCount
                ? $"Coin trail complete  {collectedCount}/{targetCount}"
                : $"Coin collected  {collectedCount}/{targetCount}",
            collectedCount >= targetCount ? 2.2f : 0.9f);

        if (collectedCount < targetCount || completionAnnounced)
        {
            return;
        }

        completionAnnounced = true;
        if (SliceState.TryGet(out SliceState state))
        {
            state.MarkMilestone(CollectionMilestone);
        }

        InfectionDirector.NotifyMilestoneGlobal(CollectionMilestone);
        Debug.Log($"[CoinCollectionRuntimeInstaller] COIN_COLLECTION_COMPLETE value={totalValue}");
    }

    private void BuildHud()
    {
        GameObject canvasObject = new GameObject("CoinCollectionHUD", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 115;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("CoinCounter", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(18f, -18f);
        panel.sizeDelta = new Vector2(220f, 54f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.035f, 0.03f, 0.02f, 0.86f);
        background.raycastTarget = false;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 6f);
        textRect.offsetMax = new Vector2(-12f, -6f);

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.fontSize = 22f;
        textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        textComponent.color = new Color(1f, 0.82f, 0.27f, 1f);
        textComponent.raycastTarget = false;
        counterText = textComponent;
    }

    private void RefreshHud()
    {
        if (counterText != null)
        {
            counterText.text = $"COINS  {collectedCount}/{Mathf.Max(1, targetCount)}";
        }
    }

    private static Material CreateCoinMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            Debug.LogError("[CoinCollectionRuntimeInstaller] No compatible lit shader found.");
            return null;
        }

        Material material = new Material(shader)
        {
            name = "Runtime_Coin_Gold"
        };

        Color gold = new Color(0.95f, 0.62f, 0.08f, 1f);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", gold);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", gold);
        }
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.78f);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.72f);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", gold * 0.3f);
        }

        material.enableInstancing = true;
        return material;
    }
}
