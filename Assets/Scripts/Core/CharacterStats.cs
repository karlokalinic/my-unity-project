using System;
using UnityEngine;

/// <summary>
/// Core character stats: Health, Stamina, plus extensible attribute block.
/// Attach to Player, NPCs, and enemies. All combat/skill systems read from this.
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [Serializable]
    public struct StatEntry
    {
        public string statName;
        public float baseValue;
        public float maxValue;
        [NonSerialized] public float currentValue;
    }

    [Header("Vitals")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 8f;
    [SerializeField] private float staminaRegenDelay = 1.5f;

    [Header("Core Attributes")]
    [SerializeField] private int strength = 10;
    [SerializeField] private int dexterity = 10;
    [SerializeField] private int constitution = 10;
    [SerializeField] private int intelligence = 10;
    [SerializeField] private int charisma = 10;
    [SerializeField] private int perception = 10;

    [Header("Derived")]
    [SerializeField] private float baseMoveSpeed = 4.5f;
    [SerializeField] private float baseArmor;

    private float currentHealth;
    private float currentStamina;
    private float lastStaminaUseTime;
    private bool isDead;

    public float MaxHealth => maxHealth;
    public float MaxStamina => maxStamina;
    public float Health => currentHealth;
    public float Stamina => currentStamina;
    public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 0f;
    public bool IsDead => isDead;

    public int Strength => strength;
    public int Dexterity => dexterity;
    public int Constitution => constitution;
    public int Intelligence => intelligence;
    public int Charisma => charisma;
    public int Perception => perception;
    public float MoveSpeed => baseMoveSpeed;
    public float Armor => baseArmor;

    public event Action<float, float> HealthChanged;
    public event Action<float, float> StaminaChanged;
    public event Action Died;
    public event Action Revived;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (isDead) return;
        RegenerateStamina();
    }

    public void TakeDamage(float rawDamage, DamageType type = DamageType.Physical)
    {
        if (isDead || rawDamage <= 0f) return;

        float mitigated = CalculateMitigation(rawDamage, type);
        float finalDamage = Mathf.Max(1f, rawDamage - mitigated);

        currentHealth = Mathf.Max(0f, currentHealth - finalDamage);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            isDead = true;
            Died?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Revive(float healthPercent = 0.3f)
    {
        if (!isDead) return;

        isDead = false;
        currentHealth = maxHealth * Mathf.Clamp01(healthPercent);
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Revived?.Invoke();
    }

    public bool TryConsumeStamina(float amount)
    {
        if (isDead || currentStamina < amount) return false;

        currentStamina -= amount;
        lastStaminaUseTime = Time.time;
        StaminaChanged?.Invoke(currentStamina, maxStamina);
        return true;
    }

    public void RestoreStamina(float amount)
    {
        if (isDead || amount <= 0f) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        StaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void SetAttributes(int str, int dex, int con, int intel, int cha, int per)
    {
        strength = str;
        dexterity = dex;
        constitution = con;
        intelligence = intel;
        charisma = cha;
        perception = per;
        RecalculateDerived();
    }

    public int GetAttribute(AttributeType attr)
    {
        return attr switch
        {
            AttributeType.Strength => strength,
            AttributeType.Dexterity => dexterity,
            AttributeType.Constitution => constitution,
            AttributeType.Intelligence => intelligence,
            AttributeType.Charisma => charisma,
            AttributeType.Perception => perception,
            _ => 0
        };
    }

    public void SetAttribute(AttributeType attr, int value)
    {
        switch (attr)
        {
            case AttributeType.Strength: strength = value; break;
            case AttributeType.Dexterity: dexterity = value; break;
            case AttributeType.Constitution: constitution = value; break;
            case AttributeType.Intelligence: intelligence = value; break;
            case AttributeType.Charisma: charisma = value; break;
            case AttributeType.Perception: perception = value; break;
        }
        RecalculateDerived();
    }

    private void RecalculateDerived()
    {
        maxHealth = 80f + constitution * 4f;
        maxStamina = 60f + constitution * 2f + dexterity * 1.5f;
        baseMoveSpeed = 3.5f + dexterity * 0.1f;
        baseArmor = constitution * 0.5f;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }

    private float CalculateMitigation(float rawDamage, DamageType type)
    {
        if (type == DamageType.True) return 0f;
        return baseArmor;
    }

    private void RegenerateStamina()
    {
        if (currentStamina >= maxStamina) return;
        if (Time.time - lastStaminaUseTime < staminaRegenDelay) return;

        currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        StaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}

public enum AttributeType
{
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Charisma,
    Perception
}

public enum DamageType
{
    Physical,
    Fire,
    Poison,
    True
}
