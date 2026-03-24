using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill system with leveling. Skills are used for dialogue checks, crafting, and combat bonuses.
/// </summary>
public class SkillSystem : MonoBehaviour
{
    [Serializable]
    public class Skill
    {
        public string skillId;
        public string displayName;
        public int level;
        public int experience;
        public AttributeType governingAttribute = AttributeType.Intelligence;

        public int ExperienceToNextLevel => (level + 1) * 100;
        public bool CanLevelUp => experience >= ExperienceToNextLevel;
    }

    [SerializeField] private List<Skill> skills = new List<Skill>();

    private readonly Dictionary<string, Skill> skillMap = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Skill> AllSkills => skills;

    public event Action<Skill> SkillLeveledUp;
    public event Action<Skill> SkillExperienceGained;

    private void Awake()
    {
        RebuildMap();
        if (skills.Count == 0) InitializeDefaultSkills();
    }

    public int GetSkillLevel(string skillId)
    {
        return skillMap.TryGetValue(skillId, out Skill s) ? s.level : 0;
    }

    public int GetEffectiveSkillLevel(string skillId, CharacterStats stats)
    {
        if (!skillMap.TryGetValue(skillId, out Skill s)) return 0;
        int attrBonus = stats != null ? (stats.GetAttribute(s.governingAttribute) - 10) / 2 : 0;
        return s.level + attrBonus;
    }

    public bool SkillCheck(string skillId, int dc, CharacterStats stats)
    {
        int effective = GetEffectiveSkillLevel(skillId, stats);
        int roll = UnityEngine.Random.Range(1, 21); // d20
        return roll + effective >= dc;
    }

    public void AddExperience(string skillId, int amount)
    {
        if (!skillMap.TryGetValue(skillId, out Skill s) || amount <= 0) return;

        s.experience += amount;
        SkillExperienceGained?.Invoke(s);

        while (s.CanLevelUp)
        {
            s.experience -= s.ExperienceToNextLevel;
            s.level++;
            SkillLeveledUp?.Invoke(s);
        }
    }

    public Skill GetSkill(string skillId)
    {
        return skillMap.TryGetValue(skillId, out Skill s) ? s : null;
    }

    private void RebuildMap()
    {
        skillMap.Clear();
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i] != null && !string.IsNullOrWhiteSpace(skills[i].skillId))
                skillMap[skills[i].skillId] = skills[i];
        }
    }

    private void InitializeDefaultSkills()
    {
        AddSkill("firearms",    "Firearms",     AttributeType.Dexterity);
        AddSkill("melee",       "Melee Combat", AttributeType.Strength);
        AddSkill("stealth",     "Stealth",      AttributeType.Dexterity);
        AddSkill("lockpicking", "Lockpicking",  AttributeType.Dexterity);
        AddSkill("persuasion",  "Persuasion",   AttributeType.Charisma);
        AddSkill("intimidation","Intimidation",  AttributeType.Strength);
        AddSkill("medicine",    "Medicine",     AttributeType.Intelligence);
        AddSkill("perception",  "Perception",   AttributeType.Perception);
        AddSkill("crafting",    "Crafting",     AttributeType.Intelligence);
        AddSkill("survival",    "Survival",     AttributeType.Constitution);
        AddSkill("lore",        "Lore",         AttributeType.Intelligence);
        AddSkill("barter",      "Barter",       AttributeType.Charisma);
        RebuildMap();
    }

    private void AddSkill(string id, string name, AttributeType attr)
    {
        var s = new Skill { skillId = id, displayName = name, level = 1, experience = 0, governingAttribute = attr };
        skills.Add(s);
    }
}

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
