using System;

/// <summary>
/// Lightweight inventory entry for snapshots and legacy compatibility.
/// </summary>
[Serializable]
public class InventoryEntry
{
    public string itemId;
    public string displayName;
    public int count = 1;
}
