using UnityEngine;

/// <summary>
/// Real-time combat controller for normal enemies.
/// Right-click to aim, left-click to fire. Uses the equipped weapon from InventorySystem.
/// Replaces the old CombatController class.
/// </summary>
public class RealTimeCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private CharacterStats playerStats;
    [SerializeField] private SkillSystem skillSystem;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private HolstinCameraRig cameraRig;

    [Header("Fallback (no equipped weapon)")]
    [SerializeField] private float fallbackDamage = 10f;
    [SerializeField] private float fallbackRange = 50f;
    [SerializeField] private float fallbackFireRate = 0.3f;

    [Header("Combat")]
    [SerializeField] private LayerMask fireMask = ~0;
    [SerializeField] private float impactForce = 8f;

    [Header("Stamina")]
    [SerializeField] private float staminaCostPerShot = 5f;
    [SerializeField] private float staminaCostPerMelee = 12f;

    private float nextFireTime;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (GameplayPauseFacade.IsPaused) return;
        if (cameraRig == null || !cameraRig.IsInFirstPerson) return;
        if (viewCamera == null) return;

        // Check for boss encounter - if a boss is targeted, don't allow real-time fire
        if (InputReader.FireHeld()) TryFire();
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime) return;

        // Get weapon data
        ItemDefinition weapon = inventory?.WeaponSlot?.equippedItem;
        float damage = weapon != null ? weapon.baseDamage : fallbackDamage;
        float range = weapon != null ? weapon.range : fallbackRange;
        float fireRate = weapon != null ? weapon.fireRate : fallbackFireRate;
        DamageType dmgType = weapon != null ? weapon.damageType : DamageType.Physical;

        // Stamina check
        bool isMelee = weapon != null && weapon.weaponType == ItemDefinition.WeaponType.Melee;
        float staminaCost = isMelee ? staminaCostPerMelee : staminaCostPerShot;
        if (playerStats != null && !playerStats.TryConsumeStamina(staminaCost)) return;

        // Ammo check for ranged
        if (weapon != null && !isMelee && !string.IsNullOrWhiteSpace(weapon.requiredAmmoId))
        {
            if (inventory == null || !inventory.HasItem(weapon.requiredAmmoId))
            {
                HolstinFeedback.ShowMessage("Out of ammo!", 1.5f);
                return;
            }
            inventory.TryConsumeItem(weapon.requiredAmmoId, 1);
        }

        nextFireTime = Time.time + fireRate;

        // Skill bonus
        float skillBonus = 1f;
        if (skillSystem != null && playerStats != null)
        {
            string skillId = isMelee ? "melee" : "firearms";
            int level = skillSystem.GetEffectiveSkillLevel(skillId, playerStats);
            skillBonus = 1f + level * 0.05f;
        }

        float finalDamage = damage * skillBonus;

        // Difficulty multiplier
        if (DifficultyManager.Instance != null)
            finalDamage *= DifficultyManager.Instance.PlayerDamageMultiplier;

        // Raycast
        Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, range, fireMask, QueryTriggerInteraction.Ignore))
        {
            // Apply physics force
            if (hit.rigidbody != null)
                hit.rigidbody.AddForceAtPosition(ray.direction * impactForce, hit.point, ForceMode.Impulse);

            // Damage
            Damageable target = hit.collider.GetComponentInParent<Damageable>();
            if (target != null)
            {
                if (target.IsBoss)
                {
                    // Boss detected - initiate turn-based combat instead
                    TurnBasedCombatManager tbm = FindAnyObjectByType<TurnBasedCombatManager>();
                    if (tbm != null && !tbm.InCombat)
                    {
                        tbm.StartCombat(target);
                        return;
                    }
                }

                target.ApplyDamage(finalDamage, dmgType);

                // Grant skill XP
                if (skillSystem != null)
                {
                    string xpSkill = isMelee ? "melee" : "firearms";
                    skillSystem.AddExperience(xpSkill, 5);
                }
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 0.2f);
    }

    private void ResolveReferences()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (playerStats == null) playerStats = GetComponent<CharacterStats>();
        if (skillSystem == null) skillSystem = GetComponent<SkillSystem>();
        if (inventory == null) inventory = GetComponent<InventorySystem>();
        if (cameraRig == null)
        {
            if (HolstinSceneContext.TryGet(out HolstinSceneContext ctx))
                cameraRig = ctx.CameraRig;
            else
                cameraRig = FindAnyObjectByType<HolstinCameraRig>();
        }
    }
}
