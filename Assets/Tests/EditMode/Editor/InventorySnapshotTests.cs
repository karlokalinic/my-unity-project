using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class InventorySnapshotTests
{
    [Test]
    public void RestoreSnapshot_RestoresSlotsLegacyAndEquipment()
    {
        ItemDefinition weapon = CreateItem("weapon_01", "Service Pistol", ItemDefinition.ItemCategory.Weapon, false, 1);
        ItemDefinition consumable = CreateItem("med_01", "Med Kit", ItemDefinition.ItemCategory.Consumable, true, 10);

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        Assert.IsTrue(inventory.TryAddItem(weapon, 1));
        Assert.IsTrue(inventory.TryAddItem(consumable, 3));
        Assert.IsTrue(inventory.Equip(weapon));
        inventory.AddItem("legacy_key", "Legacy Key", 2);

        List<InventoryEntry> snapshot = inventory.CreateSnapshot();

        Assert.IsTrue(inventory.TryRemoveItem(weapon, 1));
        Assert.IsTrue(inventory.TryRemoveItem(consumable, 3));
        inventory.Unequip(inventory.WeaponSlot);
        Assert.IsTrue(inventory.TryConsumeItem("legacy_key", 2));

        inventory.RestoreSnapshot(snapshot);

        Assert.AreEqual(1, inventory.GetItemCount(weapon));
        Assert.AreEqual(3, inventory.GetItemCount(consumable));
        Assert.AreEqual(2, inventory.GetCount("legacy_key"));
        Assert.AreEqual(weapon, inventory.WeaponSlot.equippedItem);

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(weapon);
        Object.DestroyImmediate(consumable);
    }

    [Test]
    public void RestoreSnapshot_ClearsExistingSlotStateBeforeApplyingSnapshot()
    {
        ItemDefinition oldItem = CreateItem("old_item", "Old Item", ItemDefinition.ItemCategory.Misc, true, 10);
        ItemDefinition newItem = CreateItem("new_item", "New Item", ItemDefinition.ItemCategory.Misc, true, 10);

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        Assert.IsTrue(inventory.TryAddItem(oldItem, 1));

        List<InventoryEntry> snapshot = new List<InventoryEntry>
        {
            new InventoryEntry
            {
                itemId = newItem.itemId,
                displayName = newItem.displayName,
                count = 2,
                item = newItem,
                isLegacy = false
            }
        };

        inventory.RestoreSnapshot(snapshot);

        Assert.AreEqual(0, inventory.GetItemCount(oldItem));
        Assert.AreEqual(2, inventory.GetItemCount(newItem));

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(oldItem);
        Object.DestroyImmediate(newItem);
    }

    [Test]
    public void RestoreSnapshot_NullSnapshot_ClearsInventoryAndEquipment()
    {
        ItemDefinition weapon = CreateItem("weapon_02", "Pipe", ItemDefinition.ItemCategory.Weapon, false, 1);

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        Assert.IsTrue(inventory.TryAddItem(weapon, 1));
        Assert.IsTrue(inventory.Equip(weapon));
        inventory.AddItem("legacy_key", "Legacy Key", 1);

        inventory.RestoreSnapshot(null);

        Assert.AreEqual(0, inventory.GetItemCount(weapon));
        Assert.AreEqual(0, inventory.GetCount("legacy_key"));
        Assert.IsNull(inventory.WeaponSlot.equippedItem);
        Assert.IsNull(inventory.ArmorSlot.equippedItem);

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(weapon);
    }

    private static ItemDefinition CreateItem(
        string itemId,
        string displayName,
        ItemDefinition.ItemCategory category,
        bool stackable,
        int maxStack)
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = itemId;
        item.displayName = displayName;
        item.category = category;
        item.stackable = stackable;
        item.maxStack = maxStack;
        return item;
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(target, null);
    }
}
