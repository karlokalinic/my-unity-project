using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shop/Trade system. Attach to an NPC or shop terminal.
/// </summary>
public class ShopKeeper : MonoBehaviour
{
    [Serializable]
    public class ShopEntry
    {
        public ItemDefinition item;
        public int stock;
        public int priceOverride; // 0 = use item.buyPrice
    }

    [SerializeField] private string shopName = "Merchant";
    [SerializeField] private string factionId = "merchants";
    [SerializeField] private string currencyId = "gold";
    [SerializeField] private List<ShopEntry> inventory = new List<ShopEntry>();

    public string ShopName => shopName;
    public string FactionId => factionId;
    public IReadOnlyList<ShopEntry> ShopInventory => inventory;

    public event Action ShopChanged;

    public int GetBuyPrice(ShopEntry entry, ReputationSystem rep)
    {
        int basePrice = entry.priceOverride > 0 ? entry.priceOverride : (entry.item != null ? entry.item.buyPrice : 0);
        float diffMult = DifficultyManager.Instance != null ? DifficultyManager.Instance.ShopPriceMultiplier : 1f;
        float repDiscount = rep != null ? rep.GetShopDiscount(factionId) : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(basePrice * diffMult * repDiscount));
    }

    public int GetSellPrice(ItemDefinition item, ReputationSystem rep)
    {
        if (item == null) return 0;
        float repDiscount = rep != null ? rep.GetShopDiscount(factionId) : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(item.sellPrice * (2f - repDiscount)));
    }

    public bool TryBuy(ShopEntry entry, InventorySystem playerInv, CurrencyWallet wallet, ReputationSystem rep)
    {
        if (entry == null || entry.item == null || entry.stock <= 0) return false;
        if (playerInv == null || wallet == null) return false;

        int price = GetBuyPrice(entry, rep);
        if (!wallet.CanAfford(currencyId, price)) return false;

        wallet.TrySpend(currencyId, price);
        playerInv.TryAddItem(entry.item, 1);
        entry.stock--;
        ShopChanged?.Invoke();
        return true;
    }

    public bool TrySell(ItemDefinition item, InventorySystem playerInv, CurrencyWallet wallet, ReputationSystem rep)
    {
        if (item == null || playerInv == null || wallet == null) return false;
        if (!playerInv.HasItem(item)) return false;

        int price = GetSellPrice(item, rep);
        playerInv.TryRemoveItem(item, 1);
        wallet.Add(currencyId, price);
        ShopChanged?.Invoke();
        return true;
    }

    public void AddStock(ItemDefinition item, int count, int priceOverride = 0)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == item)
            {
                inventory[i].stock += count;
                ShopChanged?.Invoke();
                return;
            }
        }

        inventory.Add(new ShopEntry { item = item, stock = count, priceOverride = priceOverride });
        ShopChanged?.Invoke();
    }
}
