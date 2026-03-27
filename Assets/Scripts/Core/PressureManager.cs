using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PressureManager : MonoBehaviour
{
    [Serializable]
    private class MilestoneStageRule
    {
        public string milestoneId;
        [Range(0, 3)] public int minimumStage;
    }

    private const int MinStage = 0;
    private const int MaxStage = 3;

    [Header("References")]
    [SerializeField] private SliceState sliceState;

    [Header("Behavior")]
    [SerializeField] private bool bridgeToInfectionPresentation = true;
    [SerializeField] private bool advanceFromMilestones = true;
    [SerializeField] private bool logTransitions;
    [SerializeField] private MilestoneStageRule[] milestoneStageRules =
    {
        new MilestoneStageRule { milestoneId = "inspect_exterior_note", minimumStage = 1 },
        new MilestoneStageRule { milestoneId = "pickup_exterior_key", minimumStage = 1 },
        new MilestoneStageRule { milestoneId = "unlock_interior_gate", minimumStage = 2 },
        new MilestoneStageRule { milestoneId = "npc_reward_key", minimumStage = 2 },
        new MilestoneStageRule { milestoneId = "console_service_unlock", minimumStage = 3 },
        new MilestoneStageRule { milestoneId = "vertical_slice_complete", minimumStage = 3 }
    };

    private bool isSubscribedToMilestones;

    public static PressureManager Instance { get; private set; }

    public event Action<int, int, string> StageChanged;

    public int CurrentStage => sliceState != null ? sliceState.PressureStage : MinStage;

    public static bool TryGet(out PressureManager manager)
    {
        manager = Instance;
        return manager != null;
    }

    public void Configure(SliceState configuredSliceState, bool enableBridgeToInfection)
    {
        if (configuredSliceState != null)
        {
            sliceState = configuredSliceState;
        }

        bridgeToInfectionPresentation = enableBridgeToInfection;
        ResolveSliceState();
        UpdateMilestoneSubscription();
    }

    public bool TryAdvance(string reason = "manual")
    {
        return SetStage(CurrentStage + 1, reason);
    }

    public bool SetStage(int stage, string reason = "manual")
    {
        int clamped = Mathf.Clamp(stage, MinStage, MaxStage);
        int previous = CurrentStage;
        if (previous == clamped)
        {
            return false;
        }

        if (sliceState == null)
        {
            ResolveSliceState();
        }

        if (sliceState != null)
        {
            sliceState.SetPressureStageInternal(clamped);
        }

        ApplyPresentationBridge(clamped, reason);
        StageChanged?.Invoke(previous, clamped, reason ?? string.Empty);

        if (logTransitions)
        {
            Debug.Log($"Pressure stage {previous} -> {clamped} ({reason})");
        }

        return true;
    }

    private void Awake()
    {
        RegisterInstance();
        ResolveSliceState();
    }

    private void OnEnable()
    {
        RegisterInstance();
        ResolveSliceState();
        UpdateMilestoneSubscription();
    }

    private void OnDisable()
    {
        RemoveMilestoneSubscription();
    }

    private void OnDestroy()
    {
        RemoveMilestoneSubscription();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void UpdateMilestoneSubscription()
    {
        if (advanceFromMilestones)
        {
            if (!isSubscribedToMilestones)
            {
                InfectionDirector.MilestoneNotified += HandleMilestone;
                isSubscribedToMilestones = true;
            }

            return;
        }

        RemoveMilestoneSubscription();
    }

    private void RemoveMilestoneSubscription()
    {
        if (!isSubscribedToMilestones)
        {
            return;
        }

        InfectionDirector.MilestoneNotified -= HandleMilestone;
        isSubscribedToMilestones = false;
    }

    private void ResolveSliceState()
    {
        if (sliceState != null)
        {
            return;
        }

        if (SliceState.TryGet(out SliceState sharedState))
        {
            sliceState = sharedState;
            return;
        }

        sliceState = FindAnyObjectByType<SliceState>();
    }

    private void HandleMilestone(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId) || milestoneStageRules == null || milestoneStageRules.Length == 0)
        {
            return;
        }

        for (int i = 0; i < milestoneStageRules.Length; i++)
        {
            MilestoneStageRule rule = milestoneStageRules[i];
            if (rule == null || string.IsNullOrWhiteSpace(rule.milestoneId))
            {
                continue;
            }

            if (!string.Equals(rule.milestoneId, milestoneId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SetStage(Mathf.Max(CurrentStage, rule.minimumStage), $"milestone:{milestoneId}");
            break;
        }
    }

    private void ApplyPresentationBridge(int pressureStage, string reason)
    {
        if (!bridgeToInfectionPresentation)
        {
            return;
        }

        InfectionNode[] nodes = FindObjectsByType<InfectionNode>(FindObjectsInactive.Include);
        if (nodes == null || nodes.Length == 0)
        {
            return;
        }

        InfectionStage mappedStage = MapPressureToInfectionStage(pressureStage);
        for (int i = 0; i < nodes.Length; i++)
        {
            InfectionNode node = nodes[i];
            if (node == null)
            {
                continue;
            }

            node.SetStage(mappedStage, true);
        }

        if (logTransitions)
        {
            Debug.Log($"Pressure bridge applied -> Infection stage {mappedStage} ({reason})");
        }
    }

    private static InfectionStage MapPressureToInfectionStage(int pressureStage)
    {
        if (pressureStage >= 3)
        {
            return InfectionStage.Overrun;
        }

        if (pressureStage >= 1)
        {
            return InfectionStage.Active;
        }

        return InfectionStage.Dormant;
    }

    private void RegisterInstance()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PressureManager instances detected. Keeping the first instance.");
            return;
        }

        Instance = this;
    }
}
