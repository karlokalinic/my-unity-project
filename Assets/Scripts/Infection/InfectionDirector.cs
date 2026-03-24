using System;
using System.Collections.Generic;
using UnityEngine;

public class InfectionDirector : MonoBehaviour
{
    [Serializable]
    public class SpreadStep
    {
        public InfectionNode node;
        public InfectionStage targetStage = InfectionStage.Active;
        [TextArea(1, 3)] public string note;
    }

    [Serializable]
    public class MilestoneSpreadJump
    {
        public string milestoneId;
        [Min(1)] public int steps = 1;
        public bool consumeOnce = true;
        [TextArea(1, 3)] public string note;
    }

    [SerializeField] private bool autoDiscoverNodes = true;
    [SerializeField] private InfectionNode[] nodes = Array.Empty<InfectionNode>();
    [SerializeField] private SpreadStep[] deterministicSteps = Array.Empty<SpreadStep>();
    [SerializeField] private MilestoneSpreadJump[] milestoneJumps = Array.Empty<MilestoneSpreadJump>();

    [Header("Time Spread")]
    [SerializeField] private bool runTimeTicks = true;
    [SerializeField] private float tickIntervalSeconds = 85f;
    [SerializeField] private bool logSpreadEvents = true;

    private readonly HashSet<string> consumedMilestones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private int currentStepIndex;
    private float nextTickTime;

    public static InfectionDirector Instance { get; private set; }
    public static event Action<string> MilestoneNotified;

    public int CurrentStepIndex => currentStepIndex;
    public int StepCount => deterministicSteps != null ? deterministicSteps.Length : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            return;
        }

        Instance = this;
        RefreshNodeListIfNeeded();
        EnsureFallbackStepsIfNeeded();
        ResetSpread();
    }

    private void Update()
    {
        if (!runTimeTicks || !Application.isPlaying || deterministicSteps == null || deterministicSteps.Length == 0)
        {
            return;
        }

        if (Time.time >= nextTickTime)
        {
            nextTickTime = Time.time + Mathf.Max(2f, tickIntervalSeconds);
            AdvanceSpread(1);
        }
    }

    public void ConfigureGraph(
        InfectionNode[] orderedNodes,
        SpreadStep[] spreadSteps,
        MilestoneSpreadJump[] milestoneStepJumps,
        float tickInterval,
        bool enableTickSpreading)
    {
        nodes = orderedNodes ?? Array.Empty<InfectionNode>();
        deterministicSteps = spreadSteps ?? Array.Empty<SpreadStep>();
        milestoneJumps = milestoneStepJumps ?? Array.Empty<MilestoneSpreadJump>();
        tickIntervalSeconds = Mathf.Max(2f, tickInterval);
        runTimeTicks = enableTickSpreading;

        ResetSpread();
    }

    public void NotifyMilestone(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId) || milestoneJumps == null || milestoneJumps.Length == 0)
        {
            if (!string.IsNullOrWhiteSpace(milestoneId))
            {
                MilestoneNotified?.Invoke(milestoneId);
            }
            return;
        }

        MilestoneNotified?.Invoke(milestoneId);
        bool handled = false;

        for (int i = 0; i < milestoneJumps.Length; i++)
        {
            MilestoneSpreadJump jump = milestoneJumps[i];
            if (jump == null || string.IsNullOrWhiteSpace(jump.milestoneId))
            {
                continue;
            }

            if (!string.Equals(jump.milestoneId, milestoneId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (jump.consumeOnce && consumedMilestones.Contains(jump.milestoneId))
            {
                continue;
            }

            consumedMilestones.Add(jump.milestoneId);
            handled |= AdvanceSpread(Mathf.Max(1, jump.steps));

            if (logSpreadEvents)
            {
                Debug.Log($"Infection milestone '{milestoneId}' advanced spread by {Mathf.Max(1, jump.steps)} step(s). {jump.note}");
            }
        }

        if (!handled && logSpreadEvents)
        {
            Debug.Log($"Infection milestone '{milestoneId}' received, but no active jump was configured.");
        }
    }

    public bool AdvanceSpread(int steps = 1)
    {
        bool advanced = false;
        int safeSteps = Mathf.Max(1, steps);

        for (int i = 0; i < safeSteps; i++)
        {
            if (!ApplyNextStep())
            {
                break;
            }

            advanced = true;
        }

        return advanced;
    }

    public void ResetSpread()
    {
        currentStepIndex = 0;
        consumedMilestones.Clear();
        nextTickTime = Time.time + Mathf.Max(2f, tickIntervalSeconds);

        RefreshNodeListIfNeeded();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].SetStage(nodes[i].InitialStage, true);
            }
        }
    }

    public static void NotifyMilestoneGlobal(string milestoneId)
    {
        if (Instance != null)
        {
            Instance.NotifyMilestone(milestoneId);
            return;
        }

        if (!string.IsNullOrWhiteSpace(milestoneId))
        {
            MilestoneNotified?.Invoke(milestoneId);
        }
    }

    private bool ApplyNextStep()
    {
        if (deterministicSteps == null || deterministicSteps.Length == 0 || currentStepIndex >= deterministicSteps.Length)
        {
            return false;
        }

        SpreadStep step = deterministicSteps[currentStepIndex];
        currentStepIndex++;

        if (step?.node != null)
        {
            step.node.SetStage(step.targetStage);
            if (logSpreadEvents)
            {
                Debug.Log($"Infection spread step {currentStepIndex}/{deterministicSteps.Length}: {step.node.NodeId} -> {step.targetStage}. {step.note}");
            }
        }

        return true;
    }

    private void RefreshNodeListIfNeeded()
    {
        if (!autoDiscoverNodes && nodes != null && nodes.Length > 0)
        {
            return;
        }

        InfectionNode[] discovered = FindObjectsByType<InfectionNode>(FindObjectsInactive.Exclude);
        Array.Sort(discovered, CompareNodesByName);
        nodes = discovered;
    }

    private void EnsureFallbackStepsIfNeeded()
    {
        if (deterministicSteps != null && deterministicSteps.Length > 0)
        {
            return;
        }

        List<SpreadStep> fallback = new List<SpreadStep>();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == null)
            {
                continue;
            }

            fallback.Add(new SpreadStep { node = nodes[i], targetStage = InfectionStage.Active, note = "Fallback active stage." });
            fallback.Add(new SpreadStep { node = nodes[i], targetStage = InfectionStage.Overrun, note = "Fallback overrun stage." });
        }

        deterministicSteps = fallback.ToArray();
    }

    private static int CompareNodesByName(InfectionNode left, InfectionNode right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        return string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase);
    }
}
