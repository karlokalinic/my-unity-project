using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Legacy adapter kept for old scenes that still serialize PlayerInventory.
/// Internally delegates to InventorySystem.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(InventorySystem))]
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
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private bool ensureStarterGun = true;
    [SerializeField] private string starterGunItemId = "wpn_pistol";
    [SerializeField] private string starterAmmoItemId = "ammo_pistol";
    [SerializeField] private int starterAmmoMinimumCount = 24;

    public event Action Changed;

    private void Awake()
    {
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }

        if (inventorySystem == null)
        {
            inventorySystem = gameObject.AddComponent<InventorySystem>();
        }

        SeedStartingItems();
        EnsureStarterCombatLoadout();
    }

    private void OnValidate()
    {
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }

#if UNITY_EDITOR
        if (!Application.isPlaying && inventorySystem == null)
        {
            inventorySystem = gameObject.AddComponent<InventorySystem>();
        }
#endif
    }

    public bool HasItem(string itemId)
    {
        return inventorySystem != null && inventorySystem.HasItem(itemId);
    }

    public int GetCount(string itemId)
    {
        return inventorySystem != null ? inventorySystem.GetCount(itemId) : 0;
    }

    public string GetDisplayName(string itemId)
    {
        if (inventorySystem == null)
        {
            return itemId;
        }

        string displayName = inventorySystem.GetDisplayName(itemId);
        return string.IsNullOrWhiteSpace(displayName) ? itemId : displayName;
    }

    public void AddItem(string itemId, string displayName, int amount = 1)
    {
        if (inventorySystem == null || amount <= 0 || string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        inventorySystem.AddItem(itemId, displayName, amount);
        Changed?.Invoke();

        if (logInventoryEvents)
        {
            Debug.Log($"Inventory gained: {GetDisplayName(itemId)} x{amount} (total {GetCount(itemId)})");
        }
    }

    public bool TryConsumeItem(string itemId, int amount = 1)
    {
        if (inventorySystem == null || amount <= 0 || string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        bool consumed = inventorySystem.TryConsumeItem(itemId, amount);
        if (!consumed)
        {
            return false;
        }

        Changed?.Invoke();
        if (logInventoryEvents)
        {
            Debug.Log($"Inventory used/lost: {GetDisplayName(itemId)} x{amount} (remaining {GetCount(itemId)})");
        }

        return true;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        return TryConsumeItem(itemId, amount);
    }

    public List<InventoryEntry> GetAllItems()
    {
        List<InventoryEntry> result = new List<InventoryEntry>();
        if (inventorySystem == null)
        {
            return result;
        }

        List<global::InventoryEntry> sourceEntries = inventorySystem.GetAllItems();
        for (int i = 0; i < sourceEntries.Count; i++)
        {
            global::InventoryEntry source = sourceEntries[i];
            if (source == null || source.count <= 0 || string.IsNullOrWhiteSpace(source.itemId))
            {
                continue;
            }

            result.Add(new InventoryEntry
            {
                itemId = source.itemId,
                displayName = string.IsNullOrWhiteSpace(source.displayName) ? source.itemId : source.displayName,
                count = source.count
            });
        }

        return result;
    }

    public List<InventoryEntry> CreateSnapshot()
    {
        return GetAllItems();
    }

    public void RestoreSnapshot(List<InventoryEntry> snapshot)
    {
        if (inventorySystem == null)
        {
            return;
        }

        List<global::InventoryEntry> converted = new List<global::InventoryEntry>();
        if (snapshot != null)
        {
            for (int i = 0; i < snapshot.Count; i++)
            {
                InventoryEntry entry = snapshot[i];
                if (entry == null || entry.count <= 0 || string.IsNullOrWhiteSpace(entry.itemId))
                {
                    continue;
                }

                converted.Add(new global::InventoryEntry
                {
                    itemId = entry.itemId,
                    displayName = string.IsNullOrWhiteSpace(entry.displayName) ? entry.itemId : entry.displayName,
                    count = entry.count,
                    isLegacy = true
                });
            }
        }

        inventorySystem.RestoreSnapshot(converted);
        Changed?.Invoke();
    }

    private void SeedStartingItems()
    {
        if (inventorySystem == null || startingItems == null || startingItems.Count == 0)
        {
            return;
        }

        for (int i = 0; i < startingItems.Count; i++)
        {
            InventoryEntry entry = startingItems[i];
            if (entry == null || entry.count <= 0 || string.IsNullOrWhiteSpace(entry.itemId))
            {
                continue;
            }

            inventorySystem.AddItem(entry.itemId, entry.displayName, entry.count);
        }
    }

    private void EnsureStarterCombatLoadout()
    {
        if (!ensureStarterGun || inventorySystem == null || string.IsNullOrWhiteSpace(starterGunItemId))
        {
            return;
        }

        ItemDefinition weaponDefinition = inventorySystem.FindItemById(starterGunItemId);
        if (weaponDefinition == null)
        {
            return;
        }

        if (!inventorySystem.HasItem(weaponDefinition))
        {
            inventorySystem.TryAddItemById(starterGunItemId, 1, weaponDefinition.displayName);
        }

        if (inventorySystem.WeaponSlot == null || inventorySystem.WeaponSlot.equippedItem == null)
        {
            inventorySystem.Equip(weaponDefinition);
        }

        if (string.IsNullOrWhiteSpace(starterAmmoItemId) || starterAmmoMinimumCount <= 0)
        {
            return;
        }

        int currentAmmo = inventorySystem.GetCount(starterAmmoItemId);
        if (currentAmmo >= starterAmmoMinimumCount)
        {
            return;
        }

        int toAdd = starterAmmoMinimumCount - currentAmmo;
        inventorySystem.TryAddItemById(starterAmmoItemId, toAdd, starterAmmoItemId);
    }
}
