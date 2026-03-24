using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal HUD showing health and stamina bars. Auto-generates UI if not pre-built.
/// </summary>
public class HealthStaminaHUD : MonoBehaviour
{
    [SerializeField] private CharacterStats stats;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image staminaFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;

    private void Awake()
    {
        if (stats == null) stats = FindAnyObjectByType<PlayerMover>()?.GetComponent<CharacterStats>();
        if (healthFill == null) BuildUI();
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.HealthChanged += OnHealthChanged;
            stats.StaminaChanged += OnStaminaChanged;
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.HealthChanged -= OnHealthChanged;
            stats.StaminaChanged -= OnStaminaChanged;
        }
    }

    private void Start()
    {
        if (stats != null) Refresh();
    }

    private void OnHealthChanged(float cur, float max) => Refresh();
    private void OnStaminaChanged(float cur, float max) => Refresh();

    private void Refresh()
    {
        if (stats == null) return;
        if (healthFill != null) healthFill.fillAmount = stats.HealthNormalized;
        if (staminaFill != null) staminaFill.fillAmount = stats.StaminaNormalized;
        if (healthText != null) healthText.text = $"{Mathf.CeilToInt(stats.Health)}/{Mathf.CeilToInt(stats.MaxHealth)}";
        if (staminaText != null) staminaText.text = $"{Mathf.CeilToInt(stats.Stamina)}/{Mathf.CeilToInt(stats.MaxStamina)}";
    }

    private void BuildUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        RectTransform root = CreateRect("VitalsRoot", transform);
        root.anchorMin = new Vector2(0f, 1f);
        root.anchorMax = new Vector2(0f, 1f);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = new Vector2(16f, -16f);
        root.sizeDelta = new Vector2(220f, 60f);

        healthFill = CreateBar(root, "HealthBar", 0f, new Color(0.8f, 0.2f, 0.2f));
        staminaFill = CreateBar(root, "StaminaBar", -28f, new Color(0.2f, 0.6f, 0.9f));

        healthText = CreateLabel(root, "HealthLabel", 0f);
        staminaText = CreateLabel(root, "StaminaLabel", -28f);
    }

    private Image CreateBar(RectTransform parent, string name, float yOffset, Color color)
    {
        RectTransform bg = CreateRect(name + "BG", parent);
        bg.anchorMin = new Vector2(0f, 1f);
        bg.anchorMax = new Vector2(1f, 1f);
        bg.pivot = new Vector2(0f, 1f);
        bg.anchoredPosition = new Vector2(0f, yOffset);
        bg.sizeDelta = new Vector2(0f, 22f);
        Image bgImg = bg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        RectTransform fill = CreateRect(name + "Fill", bg);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        Image fillImg = fill.gameObject.AddComponent<Image>();
        fillImg.color = color;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        return fillImg;
    }

    private TextMeshProUGUI CreateLabel(RectTransform parent, string name, float yOffset)
    {
        RectTransform rt = CreateRect(name, parent);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(4f, yOffset);
        rt.sizeDelta = new Vector2(0f, 22f);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 12f;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
