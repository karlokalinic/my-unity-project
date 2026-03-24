using System;
using UnityEngine;

/// <summary>
/// Player-level experience and character level tracker.
/// Leveling grants attribute points that feed into CharacterStats.
/// </summary>
public class ExperienceSystem : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int experience;
    [SerializeField] private int unspentAttributePoints;
    [SerializeField] private int baseXpPerLevel = 200;
    [SerializeField] private float xpScalingFactor = 1.4f;

    public int Level => level;
    public int Experience => experience;
    public int UnspentAttributePoints => unspentAttributePoints;
    public int ExperienceToNextLevel => Mathf.RoundToInt(baseXpPerLevel * Mathf.Pow(xpScalingFactor, level - 1));

    public event Action<int> LeveledUp;
    public event Action<int> ExperienceGained;

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        experience += amount;
        ExperienceGained?.Invoke(amount);

        while (experience >= ExperienceToNextLevel)
        {
            experience -= ExperienceToNextLevel;
            level++;
            unspentAttributePoints += 2;
            LeveledUp?.Invoke(level);
        }
    }

    public bool TrySpendAttributePoint(CharacterStats stats, AttributeType attribute)
    {
        if (unspentAttributePoints <= 0 || stats == null) return false;
        unspentAttributePoints--;
        stats.SetAttribute(attribute, stats.GetAttribute(attribute) + 1);
        return true;
    }
}
