using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public class InventoryEntry
    {
        public string itemId;
        public string displayName;
        public int count = 1;
    }

    [SerializeField] private List<InventoryEntry> startingItems = new List<InventoryEntry>();
    [SerializeField] private bool logInventoryEvents = true;

    private readonly Dictionary<string, InventoryEntry> entriesById = new Dictionary<string, InventoryEntry>();

    public event Action Changed;

    private void Awake()
    {
        RebuildRuntimeInventory();
    }

    public bool HasItem(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && entriesById.TryGetValue(itemId, out InventoryEntry entry) && entry.count > 0;
    }

    public int GetCount(string itemId)
    {
        return entriesById.TryGetValue(itemId, out InventoryEntry entry) ? entry.count : 0;
    }

    public string GetDisplayName(string itemId)
    {
        if (entriesById.TryGetValue(itemId, out InventoryEntry entry) && !string.IsNullOrWhiteSpace(entry.displayName))
        {
            return entry.displayName;
        }

        return itemId;
    }

    public void AddItem(string itemId, string displayName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return;
        }

        if (!entriesById.TryGetValue(itemId, out InventoryEntry entry))
        {
            entry = new InventoryEntry
            {
                itemId = itemId,
                displayName = string.IsNullOrWhiteSpace(displayName) ? itemId : displayName,
                count = 0
            };
            entriesById.Add(itemId, entry);
            startingItems.Add(entry);
        }

        entry.count += amount;
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            entry.displayName = displayName;
        }

        Changed?.Invoke();

        if (logInventoryEvents)
        {
            Debug.Log($"Inventory gained: {entry.displayName} x{amount} (total {entry.count})");
        }
    }

    public bool TryConsumeItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return false;
        }

        if (!entriesById.TryGetValue(itemId, out InventoryEntry entry) || entry.count < amount)
        {
            return false;
        }

        entry.count -= amount;
        if (entry.count <= 0)
        {
            entry.count = 0;
        }

        Changed?.Invoke();

        if (logInventoryEvents)
        {
            Debug.Log($"Inventory used/lost: {entry.displayName} x{amount} (remaining {entry.count})");
        }

        return true;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        return TryConsumeItem(itemId, amount);
    }

    public List<InventoryEntry> GetAllItems()
    {
        List<InventoryEntry> items = new List<InventoryEntry>();
        foreach (InventoryEntry source in entriesById.Values)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.itemId) || source.count <= 0)
            {
                continue;
            }

            items.Add(new InventoryEntry
            {
                itemId = source.itemId,
                displayName = source.displayName,
                count = source.count
            });
        }

        return items;
    }

    public List<InventoryEntry> CreateSnapshot()
    {
        List<InventoryEntry> snapshot = new List<InventoryEntry>();
        foreach (InventoryEntry source in entriesById.Values)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.itemId) || source.count <= 0)
            {
                continue;
            }

            snapshot.Add(new InventoryEntry
            {
                itemId = source.itemId,
                displayName = source.displayName,
                count = source.count
            });
        }

        return snapshot;
    }

    public void RestoreSnapshot(List<InventoryEntry> snapshot)
    {
        entriesById.Clear();

        if (snapshot != null)
        {
            for (int i = 0; i < snapshot.Count; i++)
            {
                InventoryEntry source = snapshot[i];
                if (source == null || string.IsNullOrWhiteSpace(source.itemId) || source.count <= 0)
                {
                    continue;
                }

                entriesById[source.itemId] = new InventoryEntry
                {
                    itemId = source.itemId,
                    displayName = string.IsNullOrWhiteSpace(source.displayName) ? source.itemId : source.displayName,
                    count = source.count
                };
            }
        }

        Changed?.Invoke();
    }

    private void RebuildRuntimeInventory()
    {
        entriesById.Clear();

        for (int i = 0; i < startingItems.Count; i++)
        {
            InventoryEntry source = startingItems[i];
            if (source == null || string.IsNullOrWhiteSpace(source.itemId) || source.count <= 0)
            {
                continue;
            }

            if (!entriesById.TryGetValue(source.itemId, out InventoryEntry entry))
            {
                entry = new InventoryEntry
                {
                    itemId = source.itemId,
                    displayName = string.IsNullOrWhiteSpace(source.displayName) ? source.itemId : source.displayName,
                    count = 0
                };
                entriesById.Add(source.itemId, entry);
            }

            entry.count += source.count;
        }
    }
}
