using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointLiteManager : MonoBehaviour
{
    [Header("Respawn Rules")]
    [SerializeField] private bool autoRespawnOnFall = true;
    [SerializeField] private float fallRespawnHeight = -18f;
    [SerializeField] private bool retainInventoryOnRespawn = true;

    [Header("Feedback")]
    [SerializeField] private bool showCheckpointMessages = true;
    [SerializeField] private float checkpointMessageDuration = 2.4f;
    [SerializeField] private float respawnMessageDuration = 2.8f;
    [SerializeField] private string defaultCheckpointMessage = "Checkpoint updated.";
    [SerializeField] private string respawnMessage = "You return to the last checkpoint.";

    private PlayerInventory inventory;
    private CharacterController characterController;
    private PlayerMover playerMover;
    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private bool hasCheckpoint;
    private List<PlayerInventory.InventoryEntry> checkpointInventorySnapshot = new List<PlayerInventory.InventoryEntry>();

    public bool RetainInventoryOnRespawn
    {
        get => retainInventoryOnRespawn;
        set => retainInventoryOnRespawn = value;
    }

    public void Configure(PlayerInventory configuredInventory, CharacterController configuredController, PlayerMover configuredMover)
    {
        if (configuredInventory != null)
        {
            inventory = configuredInventory;
        }

        if (configuredController != null)
        {
            characterController = configuredController;
        }

        if (configuredMover != null)
        {
            playerMover = configuredMover;
        }
    }

    public void SetCheckpoint(Transform checkpointTransform, string label = null)
    {
        if (checkpointTransform == null)
        {
            return;
        }

        SetCheckpoint(checkpointTransform.position, checkpointTransform.rotation, label);
    }

    public void SetCheckpoint(Vector3 position, Quaternion rotation, string label = null)
    {
        checkpointPosition = position;
        checkpointRotation = rotation;
        hasCheckpoint = true;

        if (!retainInventoryOnRespawn && inventory != null)
        {
            checkpointInventorySnapshot = inventory.CreateSnapshot();
        }

        if (!showCheckpointMessages || !Application.isPlaying)
        {
            return;
        }

        string message = string.IsNullOrWhiteSpace(label) ? defaultCheckpointMessage : label;
        HolstinFeedback.ShowMessage(message, checkpointMessageDuration);
    }

    public void RespawnNow()
    {
        if (!hasCheckpoint)
        {
            InitializeDefaultCheckpoint();
        }

        bool hadController = characterController != null && characterController.enabled;
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);

        if (characterController != null)
        {
            characterController.enabled = hadController;
        }

        if (!retainInventoryOnRespawn && inventory != null)
        {
            inventory.RestoreSnapshot(checkpointInventorySnapshot);
        }

        if (playerMover != null)
        {
            playerMover.ResetMotion();
        }

        if (Application.isPlaying)
        {
            HolstinFeedback.ShowMessage(respawnMessage, respawnMessageDuration);
        }
    }

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerInventory>();
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (playerMover == null)
        {
            playerMover = GetComponent<PlayerMover>();
        }

        InitializeDefaultCheckpoint();
    }

    private void Update()
    {
        if (!autoRespawnOnFall || !Application.isPlaying)
        {
            return;
        }

        if (transform.position.y < fallRespawnHeight)
        {
            RespawnNow();
        }
    }

    private void InitializeDefaultCheckpoint()
    {
        if (hasCheckpoint)
        {
            return;
        }

        checkpointPosition = transform.position;
        checkpointRotation = transform.rotation;
        hasCheckpoint = true;

        if (!retainInventoryOnRespawn && inventory != null)
        {
            checkpointInventorySnapshot = inventory.CreateSnapshot();
        }
    }
}
