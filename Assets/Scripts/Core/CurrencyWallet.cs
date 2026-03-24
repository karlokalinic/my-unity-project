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
