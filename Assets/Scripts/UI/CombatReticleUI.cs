using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CombatReticleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HolstinCameraRig cameraRig;
    [SerializeField] private RealTimeCombat combat;

    [Header("Style")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color reloadColor = new Color(1f, 0.82f, 0.22f, 1f);
    [SerializeField] private Color shotFlashColor = new Color(1f, 0.93f, 0.64f, 1f);
    [SerializeField] private float pulseDecaySpeed = 9f;
    [SerializeField] private float firstPersonLineLength = 16f;
    [SerializeField] private float thirdPersonLineLength = 12f;

    private Canvas canvas;
    private RectTransform root;
    private RectTransform horizontalLineRect;
    private RectTransform verticalLineRect;
    private RectTransform centerDotRect;
    private Image verticalLine;
    private Image horizontalLine;
    private Image centerDot;
    private TextMeshProUGUI ammoText;
    private float shotPulse;
    private RealTimeCombat subscribedCombat;

    private void Awake()
    {
        ResolveReferences();
        BuildUIIfNeeded();
    }

    private void OnEnable()
    {
        TryBindCombatEvents();
    }

    private void OnDisable()
    {
        UnbindCombatEvents();
    }

    private void Update()
    {
        ResolveReferences();
        TryBindCombatEvents();
        shotPulse = Mathf.MoveTowards(shotPulse, 0f, pulseDecaySpeed * Time.deltaTime);
        Refresh();
    }

    private void ResolveReferences()
    {
        if (cameraRig == null)
        {
            if (HolstinSceneContext.TryGet(out HolstinSceneContext ctx))
            {
                cameraRig = ctx.CameraRig;
            }
            else
            {
                cameraRig = FindAnyObjectByType<HolstinCameraRig>();
            }
        }

        if (combat == null)
        {
            combat = FindAnyObjectByType<RealTimeCombat>();
        }
    }

    private void TryBindCombatEvents()
    {
        if (combat == subscribedCombat)
        {
            return;
        }

        UnbindCombatEvents();
        subscribedCombat = combat;
        if (subscribedCombat != null)
        {
            subscribedCombat.Fired += OnCombatFired;
        }
    }

    private void UnbindCombatEvents()
    {
        if (subscribedCombat != null)
        {
            subscribedCombat.Fired -= OnCombatFired;
            subscribedCombat = null;
        }
    }

    private void OnCombatFired()
    {
        shotPulse = 1f;
    }

    private void BuildUIIfNeeded()
    {
        if (root != null)
        {
            return;
        }

        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        root = CreateRect("ReticleRoot", transform);
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(80f, 80f);

        horizontalLine = CreateImage("CrosshairH", root, new Vector2(firstPersonLineLength, 2f), Vector2.zero, activeColor);
        verticalLine = CreateImage("CrosshairV", root, new Vector2(2f, firstPersonLineLength), Vector2.zero, activeColor);
        centerDot = CreateImage("CenterDot", root, new Vector2(4f, 4f), Vector2.zero, activeColor);
        horizontalLineRect = horizontalLine.rectTransform;
        verticalLineRect = verticalLine.rectTransform;
        centerDotRect = centerDot.rectTransform;

        RectTransform ammoRt = CreateRect("AmmoLabel", transform);
        ammoRt.anchorMin = new Vector2(0.5f, 0f);
        ammoRt.anchorMax = new Vector2(0.5f, 0f);
        ammoRt.pivot = new Vector2(0.5f, 0f);
        ammoRt.anchoredPosition = new Vector2(0f, 26f);
        ammoRt.sizeDelta = new Vector2(460f, 36f);
        ammoText = ammoRt.gameObject.AddComponent<TextMeshProUGUI>();
        ammoText.fontSize = 18f;
        ammoText.alignment = TextAlignmentOptions.Center;
        ammoText.color = activeColor;
        ammoText.raycastTarget = false;
    }

    private void Refresh()
    {
        bool firstPerson = cameraRig != null && cameraRig.IsInFirstPerson && !GameplayPauseFacade.IsPaused;
        bool visible = firstPerson;

        if (root != null && root.gameObject.activeSelf != visible)
        {
            root.gameObject.SetActive(visible);
        }

        if (ammoText != null && ammoText.gameObject.activeSelf != visible)
        {
            ammoText.gameObject.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        float baseLineLength = firstPerson ? firstPersonLineLength : thirdPersonLineLength;
        float lineLength = baseLineLength + (shotPulse * 10f);
        float thickness = firstPerson ? 2f : 3f;
        float dotSize = (firstPerson ? 4f : 5f) + (shotPulse * 1.8f);

        if (horizontalLineRect != null) horizontalLineRect.sizeDelta = new Vector2(lineLength, thickness);
        if (verticalLineRect != null) verticalLineRect.sizeDelta = new Vector2(thickness, lineLength);
        if (centerDotRect != null) centerDotRect.sizeDelta = new Vector2(dotSize, dotSize);

        Color baseColor = combat != null && combat.IsReloading ? reloadColor : activeColor;
        Color color = Color.Lerp(baseColor, shotFlashColor, shotPulse);
        if (horizontalLine != null) horizontalLine.color = color;
        if (verticalLine != null) verticalLine.color = color;
        if (centerDot != null) centerDot.color = color;
        if (ammoText != null)
        {
            ammoText.color = color;
            ammoText.text = combat != null ? combat.GetWeaponHudText() : "NO WEAPON DATA";
        }
    }

    private static Image CreateImage(string name, Transform parent, Vector2 size, Vector2 anchoredPos, Color color)
    {
        RectTransform rt = CreateRect(name, parent);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
