using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Currency wallet. Supports multiple currency types (gold, tokens, etc).
/// </summary>
public class CurrencyWallet : MonoBehaviour
{
    [Serializable]
    public class CurrencyEntry
    {
        public string currencyId;
        public string displayName;
        public int amount;
    }

    [SerializeField] private List<CurrencyEntry> currencies = new List<CurrencyEntry>();

    private readonly Dictionary<string, CurrencyEntry> currencyMap = new Dictionary<string, CurrencyEntry>(StringComparer.OrdinalIgnoreCase);

    public event Action<string, int> CurrencyChanged;

    private void Awake()
    {
        RebuildMap();
        if (currencies.Count == 0)
        {
            AddCurrency("gold", "Gold", DifficultyManager.Instance != null ? DifficultyManager.Instance.StartingCurrency : 100);
        }
    }

    public int GetAmount(string currencyId)
    {
        return currencyMap.TryGetValue(currencyId, out CurrencyEntry e) ? e.amount : 0;
    }

    public bool CanAfford(string currencyId, int cost)
    {
        return GetAmount(currencyId) >= cost;
    }

    public bool TrySpend(string currencyId, int amount)
    {
        if (amount <= 0) return true;
        if (!currencyMap.TryGetValue(currencyId, out CurrencyEntry e) || e.amount < amount)
            return false;

        e.amount -= amount;
        CurrencyChanged?.Invoke(currencyId, e.amount);
        return true;
    }

    public void Add(string currencyId, int amount)
    {
        if (amount <= 0) return;

        if (!currencyMap.TryGetValue(currencyId, out CurrencyEntry e))
        {
            e = new CurrencyEntry { currencyId = currencyId, displayName = currencyId, amount = 0 };
            currencies.Add(e);
            currencyMap[currencyId] = e;
        }

        e.amount += amount;
        CurrencyChanged?.Invoke(currencyId, e.amount);
    }

    private void AddCurrency(string id, string name, int startAmount)
    {
        var e = new CurrencyEntry { currencyId = id, displayName = name, amount = startAmount };
        currencies.Add(e);
        currencyMap[id] = e;
    }

    private void RebuildMap()
    {
        currencyMap.Clear();
        for (int i = 0; i < currencies.Count; i++)
        {
            if (currencies[i] != null && !string.IsNullOrWhiteSpace(currencies[i].currencyId))
                currencyMap[currencies[i].currencyId] = currencies[i];
        }
    }
}

/// <summary>
/// Shop/Trade system. Attach to an NPC or shop terminal.
/// Player interacts -> opens shop UI -> buy/sell items.
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
