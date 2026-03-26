using UnityEngine;

/// <summary>
/// Damageable component. Attach to anything that can take damage (enemies, NPCs, destructibles).
/// Works with CharacterStats if present, or standalone with basic HP.
/// </summary>
public class Damageable : MonoBehaviour
{
    [SerializeField] private float standaloneMaxHealth = 50f;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 0.5f;
    [SerializeField] private bool isBoss;

    private CharacterStats stats;
    private float standaloneHealth;
    private bool dead;

    public bool IsDead => dead || (stats != null && stats.IsDead);
    public bool IsBoss => isBoss;
    public float HealthNormalized => stats != null ? stats.HealthNormalized :
        (standaloneMaxHealth > 0f ? standaloneHealth / standaloneMaxHealth : 0f);

    public event System.Action<float> DamageTaken;
    public event System.Action Died;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        standaloneHealth = standaloneMaxHealth;

        if (stats != null)
        {
            stats.Died += OnStatsDied;
        }
    }

    private void OnDestroy()
    {
        if (stats != null) stats.Died -= OnStatsDied;
    }

    /// <summary>
    /// Apply pre-scaled damage. Callers are responsible for applying difficulty
    /// multipliers (PlayerDamageMultiplier / EnemyDamageMultiplier) before calling
    /// this, since only the caller knows the damage source.
    /// </summary>
    public void ApplyDamage(float damage, DamageType type = DamageType.Physical)
    {
        if (dead) return;

        if (stats != null)
        {
            stats.TakeDamage(damage, type);
        }
        else
        {
            standaloneHealth -= Mathf.Max(1f, damage);
            if (standaloneHealth <= 0f)
            {
                standaloneHealth = 0f;
                dead = true;
                Died?.Invoke();
                HandleDeath();
            }
        }

        DamageTaken?.Invoke(damage);
    }

    public void Heal(float amount)
    {
        if (stats != null) stats.Heal(amount);
        else standaloneHealth = Mathf.Min(standaloneMaxHealth, standaloneHealth + amount);
    }

    private void OnStatsDied()
    {
        dead = true;
        Died?.Invoke();
        HandleDeath();
    }

    private void HandleDeath()
    {
        if (!destroyOnDeath) return;

        DeathRagdollController deathRagdoll = GetComponent<DeathRagdollController>();
        if (deathRagdoll != null && deathRagdoll.enabled && deathRagdoll.HasRig)
        {
            return;
        }

        Destroy(gameObject, deathDelay);
    }

    public void Configure(float maxHp, bool boss, bool destroyOnDeathFlag = true)
    {
        standaloneMaxHealth = maxHp;
        standaloneHealth = maxHp;
        isBoss = boss;
        destroyOnDeath = destroyOnDeathFlag;
    }
}
