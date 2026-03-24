using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime UI for turn-based boss combat. Auto-generates action buttons and status display.
/// Subscribes to TurnBasedCombatManager events.
/// </summary>
public class TurnBasedCombatUI : MonoBehaviour
{
    [SerializeField] private TurnBasedCombatManager combatManager;

    private Canvas canvas;
    private RectTransform panel;
    private TextMeshProUGUI logText;
    private TextMeshProUGUI statusText;
    private Button attackBtn;
    private Button defendBtn;
    private Button itemBtn;
    private Button fleeBtn;
    private bool isShowing;

    private void Awake()
    {
        if (combatManager == null) combatManager = FindAnyObjectByType<TurnBasedCombatManager>();
        BuildUI();
        panel.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (combatManager == null) return;
        combatManager.CombatStarted += OnCombatStarted;
        combatManager.CombatEnded += OnCombatEnded;
        combatManager.PhaseChanged += OnPhaseChanged;
        combatManager.CombatLog += OnCombatLog;
    }

    private void OnDisable()
    {
        if (combatManager == null) return;
        combatManager.CombatStarted -= OnCombatStarted;
        combatManager.CombatEnded -= OnCombatEnded;
        combatManager.PhaseChanged -= OnPhaseChanged;
        combatManager.CombatLog -= OnCombatLog;
    }

    private void OnCombatStarted()
    {
        panel.gameObject.SetActive(true);
        isShowing = true;
        logText.text = "Boss encounter!\n";
        SetButtonsInteractable(true);
    }

    private void OnCombatEnded(bool victory)
    {
        logText.text += victory ? "\n<color=#4f4>VICTORY!</color>" : "\n<color=#f44>DEFEATED</color>";
        SetButtonsInteractable(false);
        Invoke(nameof(HidePanel), 2.5f);
    }

    private void HidePanel()
    {
        panel.gameObject.SetActive(false);
        isShowing = false;
    }

    private void OnPhaseChanged(TurnBasedCombatManager.TurnPhase phase)
    {
        bool isPlayerTurn = phase == TurnBasedCombatManager.TurnPhase.PlayerTurn;
        SetButtonsInteractable(isPlayerTurn);
        statusText.text = isPlayerTurn ? "YOUR TURN" : "BOSS TURN";
        statusText.color = isPlayerTurn ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.9f, 0.3f, 0.3f);
    }

    private void OnCombatLog(string message)
    {
        if (logText == null) return;
        logText.text += message + "\n";
        // Keep last 8 lines
        string[] lines = logText.text.Split('\n');
        if (lines.Length > 8)
            logText.text = string.Join("\n", lines, lines.Length - 8, 8);
    }

    private void SetButtonsInteractable(bool state)
    {
        if (attackBtn != null) attackBtn.interactable = state;
        if (defendBtn != null) defendBtn.interactable = state;
        if (itemBtn != null) itemBtn.interactable = state;
        if (fleeBtn != null) fleeBtn.interactable = state;
    }

    private void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        panel = MakeRect("CombatPanel", transform);
        panel.anchorMin = new Vector2(0.5f, 0f);
        panel.anchorMax = new Vector2(0.5f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 16f);
        panel.sizeDelta = new Vector2(480f, 240f);
        Image panelBg = panel.gameObject.AddComponent<Image>();
        panelBg.color = new Color(0.06f, 0.06f, 0.08f, 0.92f);

        // Status label
        RectTransform statusRt = MakeRect("Status", panel);
        statusRt.anchorMin = new Vector2(0f, 1f);
        statusRt.anchorMax = new Vector2(1f, 1f);
        statusRt.pivot = new Vector2(0.5f, 1f);
        statusRt.anchoredPosition = new Vector2(0f, -4f);
        statusRt.sizeDelta = new Vector2(0f, 28f);
        statusText = statusRt.gameObject.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 16f;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.white;

        // Log area
        RectTransform logRt = MakeRect("Log", panel);
        logRt.anchorMin = new Vector2(0f, 0.35f);
        logRt.anchorMax = new Vector2(1f, 0.85f);
        logRt.offsetMin = new Vector2(10f, 0f);
        logRt.offsetMax = new Vector2(-10f, 0f);
        logText = logRt.gameObject.AddComponent<TextMeshProUGUI>();
        logText.fontSize = 12f;
        logText.color = new Color(0.8f, 0.8f, 0.8f);
        logText.alignment = TextAlignmentOptions.TopLeft;

        // Action buttons
        float btnW = 100f;
        float btnH = 36f;
        float spacing = 8f;
        float startX = -(btnW * 2 + spacing * 1.5f);

        attackBtn = CreateButton(panel, "Attack", startX, btnW, btnH, new Color(0.7f, 0.2f, 0.2f));
        defendBtn = CreateButton(panel, "Defend", startX + btnW + spacing, btnW, btnH, new Color(0.2f, 0.4f, 0.7f));
        itemBtn = CreateButton(panel, "Use Item", startX + (btnW + spacing) * 2, btnW, btnH, new Color(0.2f, 0.6f, 0.3f));
        fleeBtn = CreateButton(panel, "Flee", startX + (btnW + spacing) * 3, btnW, btnH, new Color(0.5f, 0.5f, 0.3f));

        attackBtn.onClick.AddListener(() => combatManager?.SubmitPlayerAction(TurnBasedCombatManager.CombatAction.Attack));
        defendBtn.onClick.AddListener(() => combatManager?.SubmitPlayerAction(TurnBasedCombatManager.CombatAction.Defend));
        itemBtn.onClick.AddListener(() => combatManager?.SubmitPlayerAction(TurnBasedCombatManager.CombatAction.UseItem));
        fleeBtn.onClick.AddListener(() => combatManager?.SubmitPlayerAction(TurnBasedCombatManager.CombatAction.Flee));
    }

    private Button CreateButton(RectTransform parent, string label, float x, float w, float h, Color color)
    {
        RectTransform rt = MakeRect(label + "Btn", parent);
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(x, 8f);
        rt.sizeDelta = new Vector2(w, h);

        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        Button btn = rt.gameObject.AddComponent<Button>();

        RectTransform txtRt = MakeRect("Label", rt);
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txtRt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 13f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btn;
    }

    private static RectTransform MakeRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
