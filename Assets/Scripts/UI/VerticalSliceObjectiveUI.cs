using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class VerticalSliceObjectiveUI : MonoBehaviour
{
    private const string InspectMilestone = "inspect_exterior_note";
    private const string OldKeyMilestone = "pickup_exterior_key";
    private const string GateMilestone = "unlock_interior_gate";
    private const string ServiceKeyMilestone = "npc_reward_key";
    private const string ConsoleMilestone = "console_service_unlock";
    private const string SliceCompleteMilestone = "vertical_slice_complete";

    [Header("References")]
    [SerializeField] private InventorySystem inventory;

    [Header("Objectives")]
    [SerializeField] [Min(0)] private int requiredEnemyKills;
    [SerializeField] private float enemyRescanInterval = 1.25f;

    [Header("Style")]
    [SerializeField] private Color panelColor = new Color(0.05f, 0.06f, 0.08f, 0.8f);
    [SerializeField] private Color pendingColor = new Color(0.95f, 0.78f, 0.44f, 1f);
    [SerializeField] private Color completeColor = new Color(0.62f, 0.95f, 0.66f, 1f);
    [SerializeField] private Color titleColor = new Color(0.88f, 0.92f, 0.98f, 1f);

    private Canvas canvas;
    private RectTransform panelRoot;
    private TextMeshProUGUI objectiveText;

    private readonly Dictionary<Damageable, Action> enemyDeathHandlers = new Dictionary<Damageable, Action>();
    private readonly HashSet<EnemyController> defeatedEnemyIds = new HashSet<EnemyController>();

    private bool hasOldKey;
    private bool inspectedNote;
    private bool gateUnlocked;
    private bool hasServiceKey;
    private bool consoleActivated;
    private bool sliceCompleted;
    private int enemyKills;
    private float nextEnemyScanTime;

    private void Awake()
    {
        ResolveReferences();
        BuildUIIfNeeded();
        EvaluateObjectiveState(false);
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (inventory != null)
        {
            inventory.InventoryChanged += OnInventoryChanged;
        }

        InfectionDirector.MilestoneNotified += OnMilestoneNotified;

        RefreshEnemySubscriptions();
        EvaluateObjectiveState(false);
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= OnInventoryChanged;
        }

        InfectionDirector.MilestoneNotified -= OnMilestoneNotified;
        ClearEnemySubscriptions();
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextEnemyScanTime)
        {
            nextEnemyScanTime = Time.unscaledTime + Mathf.Max(0.35f, enemyRescanInterval);
            RefreshEnemySubscriptions();
        }

        bool previousConsoleState = consoleActivated;
        consoleActivated |= IsConsoleAlreadyUnlocked();
        if (consoleActivated != previousConsoleState)
        {
            EvaluateObjectiveState(true);
        }
    }

    private void OnInventoryChanged()
    {
        EvaluateObjectiveState(true);
    }

    private void OnMilestoneNotified(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId))
        {
            return;
        }

        if (string.Equals(milestoneId, OldKeyMilestone, StringComparison.OrdinalIgnoreCase))
        {
            hasOldKey = true;
        }
        else if (string.Equals(milestoneId, InspectMilestone, StringComparison.OrdinalIgnoreCase))
        {
            inspectedNote = true;
        }
        else if (string.Equals(milestoneId, GateMilestone, StringComparison.OrdinalIgnoreCase))
        {
            gateUnlocked = true;
        }
        else if (string.Equals(milestoneId, ServiceKeyMilestone, StringComparison.OrdinalIgnoreCase))
        {
            hasServiceKey = true;
        }
        else if (string.Equals(milestoneId, ConsoleMilestone, StringComparison.OrdinalIgnoreCase))
        {
            consoleActivated = true;
        }
        else if (string.Equals(milestoneId, SliceCompleteMilestone, StringComparison.OrdinalIgnoreCase))
        {
            sliceCompleted = true;
        }

        EvaluateObjectiveState(true);
    }

    private void ResolveReferences()
    {
        if (inventory == null)
        {
            PlayerMover player = FindAnyObjectByType<PlayerMover>();
            if (player != null)
            {
                inventory = player.GetComponent<InventorySystem>();
            }
        }
    }

    private void RefreshEnemySubscriptions()
    {
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Include);
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null)
            {
                continue;
            }

            Damageable damageable = enemy.GetComponent<Damageable>();
            if (damageable == null || enemyDeathHandlers.ContainsKey(damageable))
            {
                continue;
            }

            EnemyController capturedEnemy = enemy;
            Action handler = () => RegisterEnemyKill(capturedEnemy);
            damageable.Died += handler;
            enemyDeathHandlers.Add(damageable, handler);

            if (damageable.IsDead)
            {
                RegisterEnemyKill(enemy);
            }
        }
    }

    private void RegisterEnemyKill(EnemyController enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!defeatedEnemyIds.Add(enemy))
        {
            return;
        }

        enemyKills++;
        EvaluateObjectiveState(true);
    }

    private void ClearEnemySubscriptions()
    {
        foreach (KeyValuePair<Damageable, Action> pair in enemyDeathHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.Died -= pair.Value;
            }
        }

        enemyDeathHandlers.Clear();
    }

    private void EvaluateObjectiveState(bool announceChanges)
    {
        bool oldKeyBefore = hasOldKey;
        bool inspectBefore = inspectedNote;
        bool gateBefore = gateUnlocked;
        bool serviceKeyBefore = hasServiceKey;
        bool consoleBefore = consoleActivated;
        bool killGoalBefore = requiredEnemyKills <= 0 || enemyKills >= requiredEnemyKills;
        bool completedBefore = sliceCompleted;

        if (inventory != null)
        {
            hasOldKey |= inventory.HasItem("old_key");
            hasServiceKey |= inventory.HasItem("service_key");
        }

        consoleActivated |= IsConsoleAlreadyUnlocked();

        bool killGoalComplete = requiredEnemyKills <= 0 || enemyKills >= requiredEnemyKills;
        bool nowCompleted = inspectedNote && hasOldKey && gateUnlocked && hasServiceKey && consoleActivated && killGoalComplete;
        sliceCompleted |= nowCompleted;

        if (announceChanges)
        {
            AnnounceObjectiveChange(inspectBefore, inspectedNote, "Objective complete: inspect the field ledger.");
            AnnounceObjectiveChange(oldKeyBefore, hasOldKey, "Objective complete: secure the Old Key.");
            AnnounceObjectiveChange(gateBefore, gateUnlocked, "Objective complete: unlock the interior gate.");
            AnnounceObjectiveChange(serviceKeyBefore, hasServiceKey, "Objective complete: obtain the Service Key.");
            AnnounceObjectiveChange(consoleBefore, consoleActivated, "Objective complete: activate the service console.");
            if (requiredEnemyKills > 0 && !killGoalBefore && killGoalComplete)
            {
                HolstinFeedback.ShowMessage("Objective complete: hostile neutralized.", 1.7f);
            }
        }

        if (!completedBefore && sliceCompleted)
        {
            InfectionDirector.NotifyMilestoneGlobal(SliceCompleteMilestone);
            HolstinFeedback.ShowMessage("Vertical slice complete: full gameplay loop validated.", 3f);
        }

        RefreshUIText();
    }

    private static void AnnounceObjectiveChange(bool before, bool after, string message)
    {
        if (!before && after && !string.IsNullOrWhiteSpace(message))
        {
            HolstinFeedback.ShowMessage(message, 1.7f);
        }
    }

    private bool IsConsoleAlreadyUnlocked()
    {
        GameObject barrier = GameObject.Find("VS_ServiceBarrier");
        return barrier != null && !barrier.activeInHierarchy;
    }

    private void BuildUIIfNeeded()
    {
        if (objectiveText != null)
        {
            return;
        }

        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 111;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();
        }

        panelRoot = CreateRect("ObjectivesPanel", transform);
        panelRoot.anchorMin = new Vector2(1f, 1f);
        panelRoot.anchorMax = new Vector2(1f, 1f);
        panelRoot.pivot = new Vector2(1f, 1f);
        panelRoot.anchoredPosition = new Vector2(-18f, -18f);
        panelRoot.sizeDelta = new Vector2(430f, 230f);
        Image panelBg = panelRoot.gameObject.AddComponent<Image>();
        panelBg.color = panelColor;
        panelBg.raycastTarget = false;

        RectTransform titleRect = CreateRect("Title", panelRoot);
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleRect.sizeDelta = new Vector2(0f, 30f);
        TextMeshProUGUI title = titleRect.gameObject.AddComponent<TextMeshProUGUI>();
        title.text = "VERTICAL SLICE OBJECTIVES";
        title.fontSize = 18f;
        title.alignment = TextAlignmentOptions.Center;
        title.color = titleColor;
        title.raycastTarget = false;

        RectTransform bodyRect = CreateRect("Body", panelRoot);
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(14f, 14f);
        bodyRect.offsetMax = new Vector2(-14f, -42f);
        objectiveText = bodyRect.gameObject.AddComponent<TextMeshProUGUI>();
        objectiveText.fontSize = 15f;
        objectiveText.alignment = TextAlignmentOptions.TopLeft;
        objectiveText.raycastTarget = false;
        objectiveText.textWrappingMode = TextWrappingModes.Normal;
    }

    private void RefreshUIText()
    {
        if (objectiveText == null)
        {
            return;
        }

        bool killGoalComplete = enemyKills >= requiredEnemyKills;
        if (requiredEnemyKills <= 0)
        {
            killGoalComplete = true;
        }

        string inspectLine = FormatLine(inspectedNote, "Inspect Field Ledger");
        string oldKeyLine = FormatLine(hasOldKey, "Secure Old Key in Exterior");
        string gateLine = FormatLine(gateUnlocked, "Unlock Interior Gate");
        string serviceLine = FormatLine(hasServiceKey, "Acquire Service Key from NPC");
        string killLine = requiredEnemyKills > 0
            ? FormatLine(killGoalComplete, $"Neutralize Hostiles ({Mathf.Min(enemyKills, requiredEnemyKills)}/{requiredEnemyKills})")
            : string.Empty;
        string consoleLine = FormatLine(consoleActivated, "Activate Underpass Service Console");
        string finalLine = sliceCompleted
            ? "<color=#9ef5aa>[DONE] Vertical slice loop complete.</color>"
            : "<color=#f0cf95>[LIVE] Complete all objectives to validate the slice.</color>";

        if (requiredEnemyKills > 0)
        {
            objectiveText.text =
                inspectLine + "\n" +
                oldKeyLine + "\n" +
                gateLine + "\n" +
                serviceLine + "\n" +
                killLine + "\n" +
                consoleLine + "\n\n" +
                finalLine;
            return;
        }

        objectiveText.text =
            inspectLine + "\n" +
            oldKeyLine + "\n" +
            gateLine + "\n" +
            serviceLine + "\n" +
            consoleLine + "\n\n" +
            finalLine;
    }

    private string FormatLine(bool complete, string label)
    {
        string color = ColorUtility.ToHtmlStringRGB(complete ? completeColor : pendingColor);
        string marker = complete ? "[X]" : "[ ]";
        return $"<color=#{color}>{marker} {label}</color>";
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
