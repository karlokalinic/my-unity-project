using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility: generates a starter set of ItemDefinition ScriptableObjects.
/// Menu: Tools > Holstin > Generate Starter Items
/// </summary>
public static class ItemDatabaseBootstrap
{
    private const string BasePath = "Assets/Data/Items";

    [MenuItem("Tools/Holstin/Generate Starter Items")]
    public static void GenerateAll()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder(BasePath);

        // --- Weapons ---
        CreateWeapon("wpn_pistol",    "Service Pistol",  ItemDefinition.WeaponType.Pistol,  15f, 0.4f,  40f, "ammo_pistol", 80, 30);
        CreateWeapon("wpn_rifle",     "Hunting Rifle",   ItemDefinition.WeaponType.Rifle,   35f, 1.0f,  80f, "ammo_rifle",  200, 80);
        CreateWeapon("wpn_shotgun",   "Sawn-Off",        ItemDefinition.WeaponType.Shotgun, 45f, 0.9f,  15f, "ammo_shell",  150, 55);
        CreateWeapon("wpn_knife",     "Utility Knife",   ItemDefinition.WeaponType.Melee,   12f, 0.5f,  2f,  "",            25, 8);
        CreateWeapon("wpn_pipe",      "Lead Pipe",       ItemDefinition.WeaponType.Melee,   18f, 0.7f,  2f,  "",            15, 5);

        // --- Ammo ---
        CreateAmmo("ammo_pistol", "Pistol Rounds",  5, 2, 30);
        CreateAmmo("ammo_rifle",  "Rifle Rounds",   8, 3, 20);
        CreateAmmo("ammo_shell",  "Shotgun Shells", 10, 4, 15);

        // --- Keys ---
        CreateKey("old_key",     "Old Key",      "A corroded key with a district stamp.");
        CreateKey("service_key", "Service Key",  "Opens service corridors beneath the boarding house.");
        CreateKey("cellar_key",  "Cellar Key",   "Grants access to the underground cellar network.");

        // --- Consumables ---
        CreateConsumable("med_bandage",  "Bandage",          25f, 0f,  12, 4);
        CreateConsumable("med_medkit",   "Medical Kit",      60f, 0f,  40, 15);
        CreateConsumable("med_stimulant","Stimulant",        0f,  50f, 30, 10);
        CreateConsumable("food_ration",  "Ration Pack",      15f, 20f, 8,  3);
        CreateConsumable("food_canned",  "Canned Meat",      10f, 10f, 5,  2);

        // --- Armor ---
        CreateArmor("armor_jacket",  "Padded Jacket",    5f,  50,  18);
        CreateArmor("armor_vest",    "Ballistic Vest",   12f, 150, 60);

        // --- Quest Items ---
        CreateQuestItem("quest_letter",   "Sealed Letter",     "A letter from someone in the quarantine zone.");
        CreateQuestItem("quest_badge",    "District Badge",    "Official identification. Partially scratched.");
        CreateQuestItem("quest_sample",   "Infection Sample",  "A sealed vial of infected tissue.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ItemDatabaseBootstrap] Generated starter items in {BasePath}");
    }

    [MenuItem("Tools/Holstin/Apply Weapon Presets To Existing Items")]
    public static void ApplyWeaponPresetsToExistingItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition", new[] { BasePath });
        int changed = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item == null || item.category != ItemDefinition.ItemCategory.Weapon)
            {
                continue;
            }

            ApplyWeaponPreset(item);
            EditorUtility.SetDirty(item);
            changed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ItemDatabaseBootstrap] Applied weapon presets to {changed} item(s).");
    }

    private static void CreateWeapon(string id, string name, ItemDefinition.WeaponType wType, float dmg, float rate, float range, string ammoId, int buy, int sell)
    {
        var item = CreateBase(id, name, ItemDefinition.ItemCategory.Weapon, false);
        item.weaponType = wType;
        item.baseDamage = dmg;
        item.fireRate = rate;
        item.range = range;
        item.requiredAmmoId = ammoId;
        item.buyPrice = buy;
        item.sellPrice = sell;
        item.stackable = false;
        item.maxStack = 1;
        ApplyWeaponPreset(item);
        Save(item, id);
    }

    private static void ApplyWeaponPreset(ItemDefinition item)
    {
        switch (item.weaponType)
        {
            case ItemDefinition.WeaponType.Pistol:
                item.automaticFire = false;
                item.magazineSize = 12;
                item.reloadSeconds = 1.25f;
                item.projectilesPerShot = 1;
                item.spreadAngle = 1.2f;
                break;

            case ItemDefinition.WeaponType.Rifle:
                item.automaticFire = true;
                item.magazineSize = 24;
                item.reloadSeconds = 1.7f;
                item.projectilesPerShot = 1;
                item.spreadAngle = 0.8f;
                break;

            case ItemDefinition.WeaponType.Shotgun:
                item.automaticFire = false;
                item.magazineSize = 6;
                item.reloadSeconds = 2.2f;
                item.projectilesPerShot = 7;
                item.spreadAngle = 4.5f;
                break;

            case ItemDefinition.WeaponType.Melee:
                item.automaticFire = true;
                item.magazineSize = 1;
                item.reloadSeconds = 0.05f;
                item.projectilesPerShot = 1;
                item.spreadAngle = 0f;
                item.requiredAmmoId = string.Empty;
                break;

            default:
                item.automaticFire = true;
                item.magazineSize = 8;
                item.reloadSeconds = 1.2f;
                item.projectilesPerShot = 1;
                item.spreadAngle = 1f;
                break;
        }
    }

    private static void CreateAmmo(string id, string name, int buy, int sell, int maxStack)
    {
        var item = CreateBase(id, name, ItemDefinition.ItemCategory.Ammo, true);
        item.buyPrice = buy;
        item.sellPrice = sell;
        item.maxStack = maxStack;
        Save(item, id);
    }

    private static void CreateKey(string id, string name, string desc)
    {
        var item = CreateBase(id, name, ItemDefinition.ItemCategory.Key, false);
        item.description = desc;
        item.stackable = false;
        item.maxStack = 1;
        Save(item, id);
    }

    private static void CreateConsumable(string id, string name, float heal, float stamina, int buy, int sell)
    {
        var item = CreateBase(id, name, ItemDefinition.ItemCategory.Consumable, true);
        item.healAmount = heal;
        item.staminaRestoreAmount = stamina;
        item.buyPrice = buy;
        item.sellPrice = sell;
        item.maxStack = 10;
        Save(item, id);
    }

    private static void CreateArmor(string id, string name, float armor, int buy, int sell)
    {
        var item = CreateBase(id, name, ItemDefinition.ItemCategory.Armor, false);
        item.armorValue = armor;
        item.buyPrice = buy;
        item.sellPrice = sell;
        item.stackable = false;
        item.maxStack = 1;
        Save(item, id);
    }

    private static void CreateQuestItem(string id, string name, string desc)
    {
        var item = CreateBase(id, name, ItemDefinition.ItemCategory.QuestItem, false);
        item.description = desc;
        item.stackable = false;
        item.maxStack = 1;
        Save(item, id);
    }

    private static ItemDefinition CreateBase(string id, string name, ItemDefinition.ItemCategory cat, bool stackable)
    {
        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = id;
        item.displayName = name;
        item.category = cat;
        item.stackable = stackable;
        return item;
    }

    private static void Save(ItemDefinition item, string id)
    {
        string path = $"{BasePath}/{id}.asset";
        AssetDatabase.CreateAsset(item, path);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
