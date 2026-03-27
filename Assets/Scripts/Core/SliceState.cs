using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SliceState : MonoBehaviour
{
    [Serializable]
    public struct CheckpointData
    {
        public bool hasValue;
        public Vector3 position;
        public Quaternion rotation;
        public string label;
    }

    [Header("Core Slice State")]
    [SerializeField] private string currentObjectiveId = "inspect_exterior_note";
    [SerializeField] private int pressureStage;
    [SerializeField] private List<string> acquiredKeyItems = new List<string>();
    [SerializeField] private List<string> interactedMilestones = new List<string>();
    [SerializeField] private CheckpointData checkpointData;

    private readonly Dictionary<string, bool> boolFlags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> intFlags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> stringFlags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static SliceState Instance { get; private set; }

    public event Action<string> ObjectiveChanged;
    public event Action<string, bool> BoolFlagChanged;
    public event Action<string, int> IntFlagChanged;
    public event Action<string, string> StringFlagChanged;
    public event Action<string> KeyItemAcquired;
    public event Action<string> MilestoneInteracted;
    public event Action<CheckpointData> CheckpointSaved;
    public event Action<CheckpointData> CheckpointRestored;
    public event Action<int> PressureStageChanged;

    public string CurrentObjectiveId => currentObjectiveId;
    public int PressureStage => pressureStage;
    public IReadOnlyList<string> AcquiredKeyItems => acquiredKeyItems;
    public IReadOnlyList<string> InteractedMilestones => interactedMilestones;
    public bool HasCheckpointData => checkpointData.hasValue;
    public CheckpointData LatestCheckpointData => checkpointData;

    public static bool TryGet(out SliceState state)
    {
        state = Instance;
        return state != null;
    }

    public void SetCurrentObjective(string objectiveId)
    {
        string normalized = string.IsNullOrWhiteSpace(objectiveId) ? string.Empty : objectiveId.Trim();
        if (string.Equals(currentObjectiveId, normalized, StringComparison.Ordinal))
        {
            return;
        }

        currentObjectiveId = normalized;
        ObjectiveChanged?.Invoke(currentObjectiveId);
    }

    public void SetBoolFlag(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        string normalized = key.Trim();
        if (boolFlags.TryGetValue(normalized, out bool existing) && existing == value)
        {
            return;
        }

        boolFlags[normalized] = value;
        BoolFlagChanged?.Invoke(normalized, value);
    }

    public bool GetBoolFlag(string key, bool defaultValue = false)
    {
        return TryGetBoolFlag(key, out bool value) ? value : defaultValue;
    }

    public bool TryGetBoolFlag(string key, out bool value)
    {
        value = false;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return boolFlags.TryGetValue(key.Trim(), out value);
    }

    public void SetIntFlag(string key, int value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        string normalized = key.Trim();
        if (intFlags.TryGetValue(normalized, out int existing) && existing == value)
        {
            return;
        }

        intFlags[normalized] = value;
        IntFlagChanged?.Invoke(normalized, value);
    }

    public int GetIntFlag(string key, int defaultValue = 0)
    {
        return TryGetIntFlag(key, out int value) ? value : defaultValue;
    }

    public bool TryGetIntFlag(string key, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return intFlags.TryGetValue(key.Trim(), out value);
    }

    public void SetStringFlag(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        string normalized = key.Trim();
        string normalizedValue = value ?? string.Empty;
        if (stringFlags.TryGetValue(normalized, out string existing) && string.Equals(existing, normalizedValue, StringComparison.Ordinal))
        {
            return;
        }

        stringFlags[normalized] = normalizedValue;
        StringFlagChanged?.Invoke(normalized, normalizedValue);
    }

    public string GetStringFlag(string key, string defaultValue = "")
    {
        return TryGetStringFlag(key, out string value) ? value : defaultValue;
    }

    public bool TryGetStringFlag(string key, out string value)
    {
        value = string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return stringFlags.TryGetValue(key.Trim(), out value);
    }

    public bool AcquireKeyItem(string keyItemId)
    {
        if (string.IsNullOrWhiteSpace(keyItemId))
        {
            return false;
        }

        string normalized = keyItemId.Trim();
        if (acquiredKeyItems.Exists(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        acquiredKeyItems.Add(normalized);
        KeyItemAcquired?.Invoke(normalized);
        return true;
    }

    public bool HasKeyItem(string keyItemId)
    {
        if (string.IsNullOrWhiteSpace(keyItemId))
        {
            return false;
        }

        string normalized = keyItemId.Trim();
        for (int i = 0; i < acquiredKeyItems.Count; i++)
        {
            if (string.Equals(acquiredKeyItems[i], normalized, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public bool MarkMilestone(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId))
        {
            return false;
        }

        string normalized = milestoneId.Trim();
        if (interactedMilestones.Exists(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        interactedMilestones.Add(normalized);
        MilestoneInteracted?.Invoke(normalized);
        return true;
    }

    public bool HasMilestone(string milestoneId)
    {
        if (string.IsNullOrWhiteSpace(milestoneId))
        {
            return false;
        }

        string normalized = milestoneId.Trim();
        for (int i = 0; i < interactedMilestones.Count; i++)
        {
            if (string.Equals(interactedMilestones[i], normalized, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void SaveCheckpoint(Vector3 position, Quaternion rotation, string label = "")
    {
        checkpointData = new CheckpointData
        {
            hasValue = true,
            position = position,
            rotation = rotation,
            label = label ?? string.Empty
        };

        CheckpointSaved?.Invoke(checkpointData);
    }

    public bool RestoreCheckpoint(Transform target, CharacterController characterController = null)
    {
        if (!checkpointData.hasValue || target == null)
        {
            return false;
        }

        bool reenableController = characterController != null && characterController.enabled;
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        target.SetPositionAndRotation(checkpointData.position, checkpointData.rotation);

        if (characterController != null)
        {
            characterController.enabled = reenableController;
        }

        CheckpointRestored?.Invoke(checkpointData);
        return true;
    }

    internal void SetPressureStageInternal(int stage)
    {
        if (pressureStage == stage)
        {
            return;
        }

        pressureStage = stage;
        PressureStageChanged?.Invoke(pressureStage);
    }

    private void Awake()
    {
        RegisterInstance();
    }

    private void OnEnable()
    {
        InfectionDirector.MilestoneNotified += HandleExternalMilestone;
    }

    private void OnDisable()
    {
        InfectionDirector.MilestoneNotified -= HandleExternalMilestone;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleExternalMilestone(string milestoneId)
    {
        MarkMilestone(milestoneId);
    }

    private void RegisterInstance()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple SliceState instances detected. Keeping the first instance.");
            return;
        }

        Instance = this;
    }
}
