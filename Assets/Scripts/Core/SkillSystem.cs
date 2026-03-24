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
        int roll = UnityEngine.Random.Range(1, 21);
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
        AddSkill("firearms",     "Firearms",      AttributeType.Dexterity);
        AddSkill("melee",        "Melee Combat",  AttributeType.Strength);
        AddSkill("stealth",      "Stealth",       AttributeType.Dexterity);
        AddSkill("lockpicking",  "Lockpicking",   AttributeType.Dexterity);
        AddSkill("persuasion",   "Persuasion",    AttributeType.Charisma);
        AddSkill("intimidation", "Intimidation",  AttributeType.Strength);
        AddSkill("medicine",     "Medicine",      AttributeType.Intelligence);
        AddSkill("perception",   "Perception",    AttributeType.Perception);
        AddSkill("crafting",     "Crafting",      AttributeType.Intelligence);
        AddSkill("survival",     "Survival",      AttributeType.Constitution);
        AddSkill("lore",         "Lore",          AttributeType.Intelligence);
        AddSkill("barter",       "Barter",        AttributeType.Charisma);
        RebuildMap();
    }

    private void AddSkill(string id, string name, AttributeType attr)
    {
        var s = new Skill { skillId = id, displayName = name, level = 1, experience = 0, governingAttribute = attr };
        skills.Add(s);
    }
}
