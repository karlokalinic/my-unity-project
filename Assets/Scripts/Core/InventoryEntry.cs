using System;
using UnityEngine;

/// <summary>
/// Lightweight inventory entry for snapshots and legacy compatibility.
/// </summary>
[Serializable]
public class InventoryEntry
{
    public string itemId;
    public string displayName;
    public int count = 1;
    public ItemDefinition item;
    public bool isLegacy;
    public bool equipWeapon;
    public bool equipArmor;
}
