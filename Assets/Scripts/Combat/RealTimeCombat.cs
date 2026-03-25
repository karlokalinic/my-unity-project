using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Real-time combat controller for normal enemies.
/// Handles firing, magazine/reload flow, and hit detection from the center of the screen.
/// </summary>
public class RealTimeCombat : MonoBehaviour
{
    private const string FallbackWeaponKey = "__fallback_weapon__";

    private struct WeaponFireData
    {
        public ItemDefinition weapon;
        public string weaponKey;
        public float damage;
        public float range;
        public float fireRate;
        public DamageType damageType;
        public bool isMelee;
        public bool automaticFire;
        public int magazineSize;
        public float reloadSeconds;
        public int projectilesPerShot;
        public float spreadAngle;
        public string ammoId;
    }

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
    [SerializeField] private bool fallbackAutomaticFire = true;
    [SerializeField] private int fallbackMagazineSize = 10;
    [SerializeField] private float fallbackReloadSeconds = 1.15f;

    [Header("Combat")]
    [SerializeField] private LayerMask fireMask = ~0;
    [SerializeField] private float impactForce = 8f;

    [Header("Stamina")]
    [SerializeField] private float staminaCostPerShot = 5f;
    [SerializeField] private float staminaCostPerMelee = 12f;

    private readonly Dictionary<string, int> weaponMagazineCounts = new Dictionary<string, int>();

    private float nextFireTime;
    private bool isReloading;
    private float reloadFinishTime;
    private string reloadWeaponKey;
    private string reloadAmmoId;
    private int reloadMagazineSize;

    public bool IsReloading => isReloading;
    public bool ShouldShowCombatHud => cameraRig != null && cameraRig.IsInFirstPerson && !GameplayPauseFacade.IsPaused;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (GameplayPauseFacade.IsPaused) return;
        if (cameraRig == null || !cameraRig.IsInFirstPerson) return;
        if (viewCamera == null) return;

        HandleReloadProgress();

        if (!TryGetCurrentWeaponData(out WeaponFireData weaponData))
        {
            return;
        }

        if (InputReader.ReloadPressed())
        {
            TryBeginReload(weaponData, true);
        }

        bool wantsFire = weaponData.automaticFire ? InputReader.FireHeld() : InputReader.FirePressed();
        if (wantsFire)
        {
            TryFire(weaponData);
        }
    }

    public string GetWeaponHudText()
    {
        if (!TryGetCurrentWeaponData(out WeaponFireData weaponData))
        {
            return string.Empty;
        }

        string weaponLabel = weaponData.weapon != null ? weaponData.weapon.displayName : "Fallback";
        if (!RequiresMagazine(weaponData))
        {
            return weaponData.isMelee ? $"{weaponLabel} [MELEE]" : $"{weaponLabel} [INF]";
        }

        int magazine = GetMagazineCount(weaponData.weaponKey, weaponData.magazineSize);
        int reserve = inventory != null ? inventory.GetCount(weaponData.ammoId) : 0;
        string reloadState = isReloading ? " (RELOADING)" : string.Empty;
        return $"{weaponLabel} {magazine}/{weaponData.magazineSize} [{reserve}]{reloadState}";
    }

    private bool TryGetCurrentWeaponData(out WeaponFireData weaponData)
    {
        ItemDefinition weapon = inventory?.WeaponSlot?.equippedItem;
        bool isMelee = weapon != null && weapon.weaponType == ItemDefinition.WeaponType.Melee;

        weaponData = new WeaponFireData
        {
            weapon = weapon,
            weaponKey = ResolveWeaponKey(weapon),
            damage = weapon != null ? weapon.baseDamage : fallbackDamage,
            range = weapon != null ? weapon.range : fallbackRange,
            fireRate = weapon != null ? weapon.fireRate : fallbackFireRate,
            damageType = weapon != null ? weapon.damageType : DamageType.Physical,
            isMelee = isMelee,
            automaticFire = weapon != null ? weapon.automaticFire : fallbackAutomaticFire,
            magazineSize = weapon != null ? Mathf.Max(1, weapon.magazineSize) : Mathf.Max(1, fallbackMagazineSize),
            reloadSeconds = weapon != null ? Mathf.Max(0.05f, weapon.reloadSeconds) : Mathf.Max(0.05f, fallbackReloadSeconds),
            projectilesPerShot = weapon != null ? Mathf.Max(1, weapon.projectilesPerShot) : 1,
            spreadAngle = weapon != null ? Mathf.Max(0f, weapon.spreadAngle) : 0f,
            ammoId = weapon != null ? weapon.requiredAmmoId : string.Empty
        };

        EnsureMagazineInitialized(weaponData.weaponKey, weaponData.magazineSize);
        return true;
    }

    private void TryFire(WeaponFireData weaponData)
    {
        if (Time.time < nextFireTime) return;
        if (isReloading) return;

        if (RequiresMagazine(weaponData))
        {
            int currentMagazine = GetMagazineCount(weaponData.weaponKey, weaponData.magazineSize);
            if (currentMagazine <= 0)
            {
                if (!TryBeginReload(weaponData, false))
                {
                    HolstinFeedback.ShowMessage("Out of ammo!", 1.4f);
                }
                return;
            }
        }

        float staminaCost = weaponData.isMelee ? staminaCostPerMelee : staminaCostPerShot;
        if (playerStats != null && !playerStats.TryConsumeStamina(staminaCost))
        {
            return;
        }

        nextFireTime = Time.time + Mathf.Max(0.02f, weaponData.fireRate);

        float finalDamage = ComputeDamageWithBonuses(weaponData);
        bool startedTurnCombat = false;

        for (int projectileIndex = 0; projectileIndex < weaponData.projectilesPerShot; projectileIndex++)
        {
            Ray ray = BuildShotRay(weaponData.spreadAngle);
            if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, fireMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForceAtPosition(ray.direction * impactForce, hit.point, ForceMode.Impulse);
                }

                Damageable target = hit.collider.GetComponentInParent<Damageable>();
                if (target == null)
                {
                    continue;
                }

                if (target.IsBoss)
                {
                    TurnBasedCombatManager tbm = FindAnyObjectByType<TurnBasedCombatManager>();
                    if (tbm != null && !tbm.InCombat)
                    {
                        tbm.StartCombat(target);
                        startedTurnCombat = true;
                        break;
                    }
                }

                bool wasAlive = !target.IsDead;
                target.ApplyDamage(finalDamage, weaponData.damageType);
                if (wasAlive && target.IsDead)
                {
                    HolstinFeedback.ShowMessage("Target neutralized", 0.9f);
                }
            }

            Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.red, 0.2f);
        }

        if (RequiresMagazine(weaponData))
        {
            int remaining = Mathf.Max(0, GetMagazineCount(weaponData.weaponKey, weaponData.magazineSize) - 1);
            weaponMagazineCounts[weaponData.weaponKey] = remaining;

            if (remaining <= 0)
            {
                TryBeginReload(weaponData, false);
            }
        }

        if (!startedTurnCombat && skillSystem != null)
        {
            skillSystem.AddExperience(weaponData.isMelee ? "melee" : "firearms", 5);
        }
    }

    private float ComputeDamageWithBonuses(WeaponFireData weaponData)
    {
        float skillBonus = 1f;
        if (skillSystem != null && playerStats != null)
        {
            string skillId = weaponData.isMelee ? "melee" : "firearms";
            int level = skillSystem.GetEffectiveSkillLevel(skillId, playerStats);
            skillBonus = 1f + level * 0.05f;
        }

        float finalDamage = weaponData.damage * skillBonus;
        if (DifficultyManager.Instance != null)
        {
            finalDamage *= DifficultyManager.Instance.PlayerDamageMultiplier;
        }

        return finalDamage;
    }

    private bool TryBeginReload(WeaponFireData weaponData, bool manualRequest)
    {
        if (isReloading) return false;
        if (!RequiresMagazine(weaponData)) return false;
        if (inventory == null) return false;

        int currentMagazine = GetMagazineCount(weaponData.weaponKey, weaponData.magazineSize);
        if (currentMagazine >= weaponData.magazineSize)
        {
            return false;
        }

        int reserve = inventory.GetCount(weaponData.ammoId);
        if (reserve <= 0)
        {
            if (manualRequest)
            {
                HolstinFeedback.ShowMessage("No reserve ammo.", 1.2f);
            }
            return false;
        }

        isReloading = true;
        reloadFinishTime = Time.time + weaponData.reloadSeconds;
        reloadWeaponKey = weaponData.weaponKey;
        reloadAmmoId = weaponData.ammoId;
        reloadMagazineSize = weaponData.magazineSize;
        return true;
    }

    private void HandleReloadProgress()
    {
        if (!isReloading || Time.time < reloadFinishTime)
        {
            return;
        }

        isReloading = false;

        if (inventory == null || string.IsNullOrWhiteSpace(reloadAmmoId))
        {
            return;
        }

        int currentMagazine = GetMagazineCount(reloadWeaponKey, reloadMagazineSize);
        int needed = Mathf.Max(0, reloadMagazineSize - currentMagazine);
        if (needed <= 0)
        {
            return;
        }

        int reserve = inventory.GetCount(reloadAmmoId);
        int toLoad = Mathf.Min(needed, reserve);
        if (toLoad <= 0)
        {
            return;
        }

        if (inventory.TryConsumeItem(reloadAmmoId, toLoad))
        {
            weaponMagazineCounts[reloadWeaponKey] = currentMagazine + toLoad;
        }
    }

    private static string ResolveWeaponKey(ItemDefinition weapon)
    {
        if (weapon == null)
        {
            return FallbackWeaponKey;
        }

        if (!string.IsNullOrWhiteSpace(weapon.itemId))
        {
            return weapon.itemId;
        }

        return weapon.name;
    }

    private void EnsureMagazineInitialized(string weaponKey, int magazineSize)
    {
        if (string.IsNullOrWhiteSpace(weaponKey))
        {
            return;
        }

        if (!weaponMagazineCounts.ContainsKey(weaponKey))
        {
            weaponMagazineCounts[weaponKey] = Mathf.Max(1, magazineSize);
        }
    }

    private int GetMagazineCount(string weaponKey, int magazineSize)
    {
        EnsureMagazineInitialized(weaponKey, magazineSize);
        return weaponMagazineCounts.TryGetValue(weaponKey, out int value) ? value : Mathf.Max(1, magazineSize);
    }

    private static bool RequiresMagazine(WeaponFireData weaponData)
    {
        return !weaponData.isMelee &&
               !string.IsNullOrWhiteSpace(weaponData.ammoId) &&
               weaponData.magazineSize > 0;
    }

    private Ray BuildShotRay(float spreadAngle)
    {
        Ray baseRay = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (spreadAngle <= 0.01f)
        {
            return baseRay;
        }

        float spreadRadians = Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
        Vector2 randomSpread = Random.insideUnitCircle * spreadRadians;
        Vector3 direction =
            (baseRay.direction + (viewCamera.transform.right * randomSpread.x) + (viewCamera.transform.up * randomSpread.y)).normalized;

        return new Ray(baseRay.origin, direction);
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
