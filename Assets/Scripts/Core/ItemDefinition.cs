using UnityEngine;

/// <summary>
/// Defines an item in the game database. ScriptableObject for designer-friendly editing.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Holstin/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public enum ItemCategory
    {
        Key,
        Weapon,
        Ammo,
        Consumable,
        QuestItem,
        Armor,
        Misc,
        Currency,
        Skill
    }

    public enum WeaponType
    {
        None,
        Pistol,
        Rifle,
        Shotgun,
        Melee
    }

    [Header("Identity")]
    public string itemId;
    public string displayName;
    [TextArea(2, 4)] public string description;
    public ItemCategory category = ItemCategory.Misc;
    public Sprite icon;

    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 99;

    [Header("Economy")]
    public int buyPrice;
    public int sellPrice;

    [Header("Weapon (if applicable)")]
    public WeaponType weaponType = WeaponType.None;
    public float baseDamage;
    public float fireRate = 0.3f;
    public float range = 50f;
    public DamageType damageType = DamageType.Physical;
    public bool automaticFire = true;
    [Min(1)] public int magazineSize = 8;
    [Min(0.05f)] public float reloadSeconds = 1.2f;
    [Min(1)] public int projectilesPerShot = 1;
    [Range(0f, 12f)] public float spreadAngle = 0f;
    public string requiredAmmoId;

    [Header("Consumable (if applicable)")]
    public float healAmount;
    public float staminaRestoreAmount;

    [Header("Armor (if applicable)")]
    public float armorValue;
}
