using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Faction reputation tracker. NPCs and shops reference this for pricing, dialogue, and access.
/// </summary>
public class ReputationSystem : MonoBehaviour
{
    [Serializable]
    public class FactionReputation
    {
        public string factionId;
        public string displayName;
        public int reputation; // -100 to 100
    }

    [SerializeField] private List<FactionReputation> factions = new List<FactionReputation>();

    private readonly Dictionary<string, FactionReputation> factionMap = new Dictionary<string, FactionReputation>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<FactionReputation> AllFactions => factions;

    public event Action<FactionReputation> ReputationChanged;

    private void Awake()
    {
        RebuildMap();
        if (factions.Count == 0) InitializeDefaultFactions();
    }

    public int GetReputation(string factionId)
    {
        return factionMap.TryGetValue(factionId, out FactionReputation f) ? f.reputation : 0;
    }

    public void ModifyReputation(string factionId, int delta)
    {
        if (!factionMap.TryGetValue(factionId, out FactionReputation f))
        {
            f = new FactionReputation { factionId = factionId, displayName = factionId, reputation = 0 };
            factions.Add(f);
            factionMap[factionId] = f;
        }

        f.reputation = Mathf.Clamp(f.reputation + delta, -100, 100);
        ReputationChanged?.Invoke(f);
    }

    public string GetStanding(string factionId)
    {
        int rep = GetReputation(factionId);
        if (rep >= 75) return "Revered";
        if (rep >= 40) return "Friendly";
        if (rep >= 10) return "Neutral";
        if (rep >= -20) return "Wary";
        if (rep >= -60) return "Hostile";
        return "Nemesis";
    }

    public float GetShopDiscount(string factionId)
    {
        int rep = GetReputation(factionId);
        return Mathf.Lerp(1.3f, 0.7f, Mathf.InverseLerp(-100f, 100f, rep));
    }

    private void RebuildMap()
    {
        factionMap.Clear();
        for (int i = 0; i < factions.Count; i++)
        {
            if (factions[i] != null && !string.IsNullOrWhiteSpace(factions[i].factionId))
                factionMap[factions[i].factionId] = factions[i];
        }
    }

    private void InitializeDefaultFactions()
    {
        AddFaction("district_guard",  "District Guard");
        AddFaction("boarding_house",  "Boarding House");
        AddFaction("tunnel_dwellers", "Tunnel Dwellers");
        AddFaction("merchants",       "Merchants Guild");
        AddFaction("infected",        "The Infected");
        RebuildMap();
    }

    private void AddFaction(string id, string name)
    {
        factions.Add(new FactionReputation { factionId = id, displayName = name, reputation = 0 });
    }
}
