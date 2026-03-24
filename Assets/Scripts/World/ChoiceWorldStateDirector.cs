using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ChoiceWorldStateDirector : MonoBehaviour
{
    [Serializable]
    public class WorldReaction
    {
        public string milestoneId;
        public bool consumeOnce = true;
        public GameObject[] activate = Array.Empty<GameObject>();
        public GameObject[] deactivate = Array.Empty<GameObject>();
        [TextArea(1, 4)] public string feedbackMessage;
        [Min(0.25f)] public float feedbackDuration = 2.8f;
    }

    [Header("Choice Rules")]
    [SerializeField] private bool enforceExclusiveChoice = true;
    [SerializeField] private string[] exclusiveChoiceMilestones = { "choice_left_pick", "choice_right_pick" };
    [SerializeField] private WorldReaction[] reactions = Array.Empty<WorldReaction>();

    private readonly HashSet<string> consumedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private string lockedChoiceMilestone;

    public string LockedChoiceMilestone => lockedChoiceMilestone;

    public void Configure(bool exclusiveChoice, string[] exclusiveMilestones, WorldReaction[] configuredReactions)
    {
        enforceExclusiveChoice = exclusiveChoice;
        exclusiveChoiceMilestones = exclusiveMilestones ?? Array.Empty<string>();
        reactions = configuredReactions ?? Array.Empty<WorldReaction>();
        consumedKeys.Clear();
        lockedChoiceMilestone = string.Empty;
    }

    private void OnEnable()
    {
        InfectionDirector.MilestoneNotified += HandleMilestone;
    }

    private void OnDisable()
    {
        InfectionDirector.MilestoneNotified -= HandleMilestone;
    }

    private void HandleMilestone(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId))
        {
            return;
        }

        if (enforceExclusiveChoice && IsExclusiveChoiceMilestone(milestoneId))
        {
            if (string.IsNullOrWhiteSpace(lockedChoiceMilestone))
            {
                lockedChoiceMilestone = milestoneId;
            }
            else if (!string.Equals(lockedChoiceMilestone, milestoneId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        for (int i = 0; i < reactions.Length; i++)
        {
            WorldReaction reaction = reactions[i];
            if (reaction == null || string.IsNullOrWhiteSpace(reaction.milestoneId))
            {
                continue;
            }

            if (!string.Equals(reaction.milestoneId, milestoneId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string consumeKey = reaction.milestoneId + "#" + i;
            if (reaction.consumeOnce && consumedKeys.Contains(consumeKey))
            {
                continue;
            }

            consumedKeys.Add(consumeKey);
            ApplyReaction(reaction);
        }
    }

    private bool IsExclusiveChoiceMilestone(string milestoneId)
    {
        if (exclusiveChoiceMilestones == null || exclusiveChoiceMilestones.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < exclusiveChoiceMilestones.Length; i++)
        {
            if (string.Equals(exclusiveChoiceMilestones[i], milestoneId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void ApplyReaction(WorldReaction reaction)
    {
        if (reaction.activate != null)
        {
            for (int i = 0; i < reaction.activate.Length; i++)
            {
                if (reaction.activate[i] != null)
                {
                    reaction.activate[i].SetActive(true);
                }
            }
        }

        if (reaction.deactivate != null)
        {
            for (int i = 0; i < reaction.deactivate.Length; i++)
            {
                if (reaction.deactivate[i] != null)
                {
                    reaction.deactivate[i].SetActive(false);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(reaction.feedbackMessage))
        {
            HolstinFeedback.ShowMessage(reaction.feedbackMessage, reaction.feedbackDuration);
        }
    }
}
