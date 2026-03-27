using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slot-based inventory with equipment, stacking, and events.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public class InventorySlot
    {
        public ItemDefinition item;
        public int count;

        public bool IsEmpty => item == null || count <= 0;
    }

    [Serializable]
    public class EquipmentSlot
    {
        public string slotName;
        public ItemDefinition equippedItem;
    }

    [SerializeField] private int maxSlots = 30;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    [Header("Equipment")]
    [SerializeField] private EquipmentSlot weaponSlot = new EquipmentSlot { slotName = "Weapon" };
    [SerializeField] private EquipmentSlot armorSlot = new EquipmentSlot { slotName = "Armor" };

    public int MaxSlots => maxSlots;
    public IReadOnlyList<InventorySlot> Slots => slots;
    public EquipmentSlot WeaponSlot => weaponSlot;
    public EquipmentSlot ArmorSlot => armorSlot;

    public event Action InventoryChanged;
    public event Action<ItemDefinition> ItemAdded;
    public event Action<ItemDefinition> ItemRemoved;
    public event Action<EquipmentSlot> EquipmentChanged;

    // LEGACY KEEP REMOVE_LATER:
    // Backward compatibility with string-based item paths still used at runtime by:
    // NPCKeyGiverInteractable, ItemConsumeInteractable, DoorInteractable, SkillCheckNpcInteractable.
    // Remove after all runtime callsites are migrated to ItemDefinition-based APIs.
    private readonly Dictionary<string, int> legacyItems = new Dictionary<string, int>();
    private readonly Dictionary<string, string> legacyDisplayNames = new Dictionary<string, string>();

    private void Awake()
    {
        if (slots.Count == 0)
        {
            for (int i = 0; i < maxSlots; i++)
                slots.Add(new InventorySlot());
        }
    }

    // --- Core slot-based API ---

    public bool TryAddItem(ItemDefinition item, int amount = 1)
    {
        if (!TryAddItemToSlots(item, amount))
        {
            return false;
        }

        ItemAdded?.Invoke(item);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool TryRemoveItem(ItemDefinition item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        if (GetItemCount(item) < amount) return false;

        int remaining = amount;
        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            if (slots[i].item != item) continue;

            int toRemove = Mathf.Min(remaining, slots[i].count);
            slots[i].count -= toRemove;
            remaining -= toRemove;

            if (slots[i].count <= 0)
            {
                slots[i].item = null;
                slots[i].count = 0;
            }
        }

        ItemRemoved?.Invoke(item);
        InventoryChanged?.Invoke();
        return true;
    }

    public int GetItemCount(ItemDefinition item)
    {
        if (item == null) return 0;
        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item) total += slots[i].count;
        }
        return total;
    }

    public bool HasItem(ItemDefinition item, int amount = 1)
    {
        return GetItemCount(item) >= amount;
    }

    public ItemDefinition FindItemById(string itemId)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && slots[i].item.itemId == itemId)
                return slots[i].item;
        }
        return null;
    }

    public bool Equip(ItemDefinition item)
    {
        if (item == null) return false;

        EquipmentSlot targetSlot = null;
        if (item.category == ItemDefinition.ItemCategory.Weapon)
            targetSlot = weaponSlot;
        else if (item.category == ItemDefinition.ItemCategory.Armor)
            targetSlot = armorSlot;

        if (targetSlot == null) return false;

        // Unequip current if occupied
        if (targetSlot.equippedItem != null)
            Unequip(targetSlot);

        targetSlot.equippedItem = item;
        EquipmentChanged?.Invoke(targetSlot);
        InventoryChanged?.Invoke();
        return true;
    }

    public void Unequip(EquipmentSlot slot)
    {
        if (slot == null || slot.equippedItem == null) return;
        slot.equippedItem = null;
        EquipmentChanged?.Invoke(slot);
        InventoryChanged?.Invoke();
    }

    private int FindEmptySlot()
    {
        EnsureSlotsInitialized();
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty) return i;
        }
        return -1;
    }

    // LEGACY KEEP REMOVE_LATER:
    // String-id API remains active while runtime interaction scripts are migrated.

    public bool HasItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return false;

        // Check new slot system first
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && slots[i].item.itemId == itemId && slots[i].count > 0)
                return true;
        }

        // LEGACY KEEP REMOVE_LATER: emergency fallback path for remaining string-id callsites.
        return legacyItems.TryGetValue(itemId, out int c) && c > 0;
    }

    public void AddItem(string itemId, string displayName, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return;

        if (!legacyItems.ContainsKey(itemId))
            legacyItems[itemId] = 0;
        legacyItems[itemId] += amount;
        legacyDisplayNames[itemId] = displayName;

        InventoryChanged?.Invoke();
    }

    public bool TryConsumeItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return false;

        // Check new system first
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && slots[i].item.itemId == itemId && slots[i].count >= amount)
            {
                slots[i].count -= amount;
                if (slots[i].count <= 0) { slots[i].item = null; slots[i].count = 0; }
                InventoryChanged?.Invoke();
                return true;
            }
        }

        // LEGACY KEEP REMOVE_LATER: emergency fallback path for remaining string-id callsites.
        if (legacyItems.TryGetValue(itemId, out int c) && c >= amount)
        {
            legacyItems[itemId] -= amount;
            InventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    public int GetCount(string itemId)
    {
        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && slots[i].item.itemId == itemId)
                total += slots[i].count;
        }
        if (legacyItems.TryGetValue(itemId, out int c))
            total += c;
        return total;
    }

    public string GetDisplayName(string itemId)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && slots[i].item.itemId == itemId)
                return slots[i].item.displayName;
        }
        return legacyDisplayNames.TryGetValue(itemId, out string dn) ? dn : itemId;
    }

    public List<InventoryEntry> GetAllItems()
    {
        var result = new List<InventoryEntry>();
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item != null && slots[i].count > 0)
            {
                result.Add(new InventoryEntry
                {
                    itemId = slots[i].item.itemId,
                    displayName = slots[i].item.displayName,
                    count = slots[i].count,
                    item = slots[i].item,
                    isLegacy = false,
                    equipWeapon = weaponSlot != null && weaponSlot.equippedItem == slots[i].item,
                    equipArmor = armorSlot != null && armorSlot.equippedItem == slots[i].item
                });
            }
        }
        foreach (var kv in legacyItems)
        {
            if (kv.Value > 0)
                result.Add(new InventoryEntry
                {
                    itemId = kv.Key,
                    displayName = legacyDisplayNames.TryGetValue(kv.Key, out string dn) ? dn : kv.Key,
                    count = kv.Value,
                    isLegacy = true
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
        EnsureSlotsInitialized();
        ClearSlots();
        legacyItems.Clear();
        legacyDisplayNames.Clear();
        ItemDefinition weaponToEquip = null;
        ItemDefinition armorToEquip = null;

        if (weaponSlot != null)
        {
            weaponSlot.equippedItem = null;
        }

        if (armorSlot != null)
        {
            armorSlot.equippedItem = null;
        }

        if (snapshot == null)
        {
            InventoryChanged?.Invoke();
            return;
        }

        for (int i = 0; i < snapshot.Count; i++)
        {
            var e = snapshot[i];
            if (e == null || e.count <= 0 || string.IsNullOrWhiteSpace(e.itemId))
            {
                continue;
            }

            if (e.isLegacy || e.item == null)
            {
                AddLegacyItem(e.itemId, e.displayName, e.count);
                continue;
            }

            if (!TryAddItemToSlots(e.item, e.count))
            {
                Debug.LogWarning($"[InventorySystem] Could not fully restore snapshot item '{e.itemId}'.");
            }

            if (e.equipWeapon)
            {
                weaponToEquip = e.item;
            }

            if (e.equipArmor)
            {
                armorToEquip = e.item;
            }
        }

        if (weaponSlot != null && weaponToEquip != null && HasItem(weaponToEquip))
        {
            weaponSlot.equippedItem = weaponToEquip;
            EquipmentChanged?.Invoke(weaponSlot);
        }

        if (armorSlot != null && armorToEquip != null && HasItem(armorToEquip))
        {
            armorSlot.equippedItem = armorToEquip;
            EquipmentChanged?.Invoke(armorSlot);
        }

        InventoryChanged?.Invoke();
    }

    private bool TryAddItemToSlots(ItemDefinition item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        EnsureSlotsInitialized();
        int remaining = amount;

        // Stack into existing slots first.
        if (item.stackable)
        {
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (slots[i].item == item && slots[i].count < item.maxStack)
                {
                    int canAdd = Mathf.Min(remaining, item.maxStack - slots[i].count);
                    slots[i].count += canAdd;
                    remaining -= canAdd;
                }
            }
        }

        // Fill empty slots.
        while (remaining > 0)
        {
            int emptyIndex = FindEmptySlot();
            if (emptyIndex < 0)
            {
                return false;
            }

            int toAdd = item.stackable ? Mathf.Min(remaining, item.maxStack) : 1;
            slots[emptyIndex].item = item;
            slots[emptyIndex].count = toAdd;
            remaining -= toAdd;
        }

        return true;
    }

    private void AddLegacyItem(string itemId, string displayName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return;
        }

        if (!legacyItems.ContainsKey(itemId))
        {
            legacyItems[itemId] = 0;
        }

        legacyItems[itemId] += amount;
        legacyDisplayNames[itemId] = string.IsNullOrWhiteSpace(displayName) ? itemId : displayName;
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].item = null;
            slots[i].count = 0;
        }
    }

    private void EnsureSlotsInitialized()
    {
        if (slots == null)
        {
            slots = new List<InventorySlot>();
        }

        if (slots.Count == 0 && maxSlots > 0)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                slots.Add(new InventorySlot());
            }
        }
    }
}
