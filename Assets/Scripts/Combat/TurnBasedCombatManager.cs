using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn-based combat for boss encounters.
/// When a boss is engaged, gameplay pauses and a turn-based loop begins.
/// Player and boss take turns: player selects action, boss responds.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour
{
    public enum CombatAction { Attack, Defend, UseItem, Flee }
    public enum TurnPhase { PlayerTurn, EnemyTurn, Victory, Defeat }

    [Header("Turn-Based Settings")]
    [SerializeField] private float turnDelay = 0.6f;
    [SerializeField] private float defenseMultiplier = 0.4f;

    [Header("References")]
    [SerializeField] private CharacterStats playerStats;
    [SerializeField] private SkillSystem playerSkills;
    [SerializeField] private InventorySystem playerInventory;

    private Damageable currentBoss;
    private CharacterStats bossStats;
    private int pauseToken;
    private bool playerDefending;
    private TurnPhase currentPhase;

    public bool InCombat => currentBoss != null;
    public TurnPhase CurrentPhase => currentPhase;
    public Damageable CurrentBoss => currentBoss;

    public event Action CombatStarted;
    public event Action<TurnPhase> PhaseChanged;
    public event Action<string> CombatLog;
    public event Action<bool> CombatEnded; // true = victory

    private void Awake()
    {
        ResolveReferences();
    }

    public void StartCombat(Damageable boss)
    {
        if (InCombat || boss == null) return;

        currentBoss = boss;
        bossStats = boss.GetComponent<CharacterStats>();
        pauseToken = GameplayPauseFacade.PushPause();
        playerDefending = false;

        CombatStarted?.Invoke();
        LogEntry("BOSS ENCOUNTER! Turn-based combat initiated.");
        BeginPlayerTurn();
    }

    public void SubmitPlayerAction(CombatAction action)
    {
        if (!InCombat || currentPhase != TurnPhase.PlayerTurn) return;

        switch (action)
        {
            case CombatAction.Attack:
                ExecutePlayerAttack();
                break;
            case CombatAction.Defend:
                ExecutePlayerDefend();
                break;
            case CombatAction.UseItem:
                ExecutePlayerUseItem();
                break;
            case CombatAction.Flee:
                ExecutePlayerFlee();
                return;
        }

        // Check boss death
        if (currentBoss == null || currentBoss.IsDead)
        {
            EndCombat(true);
            return;
        }

        StartCoroutine(EnemyTurnRoutine());
    }

    private void BeginPlayerTurn()
    {
        playerDefending = false;
        currentPhase = TurnPhase.PlayerTurn;
        PhaseChanged?.Invoke(currentPhase);
        LogEntry("--- YOUR TURN --- Choose: Attack / Defend / Use Item / Flee");
    }

    private void ExecutePlayerAttack()
    {
        ItemDefinition weapon = playerInventory?.WeaponSlot?.equippedItem;
        float baseDamage = weapon != null ? weapon.baseDamage : 15f;

        float skillBonus = 1f;
        if (playerSkills != null && playerStats != null)
        {
            bool melee = weapon != null && weapon.weaponType == ItemDefinition.WeaponType.Melee;
            string skillId = melee ? "melee" : "firearms";
            int level = playerSkills.GetEffectiveSkillLevel(skillId, playerStats);
            skillBonus = 1f + level * 0.08f;
        }

        // Crit chance from perception
        bool crit = false;
        if (playerStats != null)
        {
            int perception = playerStats.Perception;
            crit = UnityEngine.Random.Range(0, 100) < (5 + perception);
        }

        float finalDamage = baseDamage * skillBonus;
        if (crit) finalDamage *= 2f;

        if (DifficultyManager.Instance != null)
            finalDamage *= DifficultyManager.Instance.PlayerDamageMultiplier;

        currentBoss.ApplyDamage(finalDamage);
        string critText = crit ? " CRITICAL HIT!" : "";
        LogEntry($"You attack for {finalDamage:F0} damage.{critText}");

        if (playerSkills != null)
        {
            bool isMelee = weapon != null && weapon.weaponType == ItemDefinition.WeaponType.Melee;
            playerSkills.AddExperience(isMelee ? "melee" : "firearms", 10);
        }
    }

    private void ExecutePlayerDefend()
    {
        playerDefending = true;
        LogEntry("You brace yourself. Damage taken this turn is reduced.");
    }

    private void ExecutePlayerUseItem()
    {
        // Use first consumable with heal
        if (playerInventory != null)
        {
            for (int i = 0; i < playerInventory.Slots.Count; i++)
            {
                var slot = playerInventory.Slots[i];
                if (slot.IsEmpty || slot.item.category != ItemDefinition.ItemCategory.Consumable) continue;
                if (slot.item.healAmount <= 0f && slot.item.staminaRestoreAmount <= 0f) continue;

                if (slot.item.healAmount > 0f && playerStats != null)
                {
                    playerStats.Heal(slot.item.healAmount);
                    LogEntry($"Used {slot.item.displayName}, healed {slot.item.healAmount:F0} HP.");
                }

                if (slot.item.staminaRestoreAmount > 0f && playerStats != null)
                {
                    playerStats.RestoreStamina(slot.item.staminaRestoreAmount);
                    LogEntry($"Used {slot.item.displayName}, restored {slot.item.staminaRestoreAmount:F0} stamina.");
                }

                playerInventory.TryRemoveItem(slot.item, 1);
                return;
            }
        }

        LogEntry("No consumables available. Turn wasted!");
    }

    private void ExecutePlayerFlee()
    {
        // Dexterity check to flee
        bool fled = true;
        if (playerStats != null)
        {
            int check = UnityEngine.Random.Range(1, 21) + playerStats.Dexterity;
            fled = check >= 15;
        }

        if (fled)
        {
            LogEntry("You flee from the boss!");
            EndCombat(false);
        }
        else
        {
            LogEntry("Flee failed! The boss blocks your path.");
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator EnemyTurnRoutine()
    {
        currentPhase = TurnPhase.EnemyTurn;
        PhaseChanged?.Invoke(currentPhase);
        LogEntry("--- BOSS TURN ---");

        yield return new WaitForSecondsRealtime(turnDelay);

        // Boss attacks player
        float bossDamage = 20f;
        if (bossStats != null)
            bossDamage = 10f + bossStats.Strength * 2f;

        if (DifficultyManager.Instance != null)
            bossDamage *= DifficultyManager.Instance.EnemyDamageMultiplier;

        if (playerDefending)
            bossDamage *= defenseMultiplier;

        if (playerStats != null)
        {
            playerStats.TakeDamage(bossDamage);
            LogEntry($"Boss attacks for {bossDamage:F0} damage.{(playerDefending ? " (Reduced by defense!)" : "")}");

            if (playerStats.IsDead)
            {
                EndCombat(false);
                yield break;
            }
        }

        yield return new WaitForSecondsRealtime(turnDelay * 0.5f);
        BeginPlayerTurn();
    }

    private void EndCombat(bool victory)
    {
        currentPhase = victory ? TurnPhase.Victory : TurnPhase.Defeat;
        PhaseChanged?.Invoke(currentPhase);
        LogEntry(victory ? "VICTORY! The boss has been defeated." : "DEFEAT! You have fallen in battle.");

        CombatEnded?.Invoke(victory);

        // Grant rewards on victory
        if (victory && playerSkills != null)
        {
            playerSkills.AddExperience("survival", 50);
        }

        GameplayPauseFacade.PopPause(pauseToken);
        pauseToken = 0;
        currentBoss = null;
        bossStats = null;
    }

    private void LogEntry(string msg)
    {
        CombatLog?.Invoke(msg);
        Debug.Log($"[TurnCombat] {msg}");
    }

    private void ResolveReferences()
    {
        if (playerStats == null)
        {
            PlayerMover mover = FindAnyObjectByType<PlayerMover>();
            if (mover != null)
            {
                playerStats = mover.GetComponent<CharacterStats>();
                playerSkills = mover.GetComponent<SkillSystem>();
                playerInventory = mover.GetComponent<InventorySystem>();
            }
        }
    }
}
