using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(InventorySystem))]
public class StarterLoadout : MonoBehaviour
{
    [Serializable]
    private struct AmmoGrant
    {
        public string ammoItemId;
        public int amount;
    }

    [Header("Behavior")]
    [SerializeField] private bool grantOnStart = true;
    [SerializeField] private bool grantOnlyIfInventoryEmpty = true;

    [Header("Items")]
    [SerializeField] private string fallbackWeaponItemId = "wpn_pistol";
    [SerializeField] private ItemDefinition startingWeapon;
    [SerializeField] private ItemDefinition[] startingItems = Array.Empty<ItemDefinition>();
    [SerializeField] private AmmoGrant[] startingAmmo = Array.Empty<AmmoGrant>();

    private bool granted;

    private void Start()
    {
        if (!grantOnStart)
        {
            return;
        }

        TryGrantLoadout();
    }

    public void Configure(ItemDefinition weapon, ItemDefinition[] items, (string ammoId, int amount)[] ammo)
    {
        startingWeapon = weapon;
        startingItems = items ?? Array.Empty<ItemDefinition>();

        if (ammo == null || ammo.Length == 0)
        {
            startingAmmo = Array.Empty<AmmoGrant>();
            return;
        }

        startingAmmo = new AmmoGrant[ammo.Length];
        for (int i = 0; i < ammo.Length; i++)
        {
            startingAmmo[i] = new AmmoGrant
            {
                ammoItemId = ammo[i].ammoId,
                amount = ammo[i].amount
            };
        }
    }

    public void ConfigureByItemIds(string weaponItemId, string[] itemIds, (string ammoId, int amount)[] ammo)
    {
        if (!string.IsNullOrWhiteSpace(weaponItemId))
        {
            fallbackWeaponItemId = weaponItemId;
            startingWeapon = ItemDefinitionLookup.FindById(weaponItemId);
        }

        if (itemIds == null || itemIds.Length == 0)
        {
            startingItems = Array.Empty<ItemDefinition>();
        }
        else
        {
            var resolved = new ItemDefinition[itemIds.Length];
            for (int i = 0; i < itemIds.Length; i++)
            {
                resolved[i] = ItemDefinitionLookup.FindById(itemIds[i]);
            }
            startingItems = resolved;
        }

        Configure(startingWeapon, startingItems, ammo);
    }

    public void ConfigureGrantBehavior(bool shouldGrantOnStart, bool onlyIfEmpty)
    {
        grantOnStart = shouldGrantOnStart;
        grantOnlyIfInventoryEmpty = onlyIfEmpty;
    }

    [ContextMenu("Grant Starter Loadout")]
    public void TryGrantLoadout()
    {
        if (granted)
        {
            return;
        }

        InventorySystem inventory = GetComponent<InventorySystem>();
        if (inventory == null)
        {
            return;
        }

        if (grantOnlyIfInventoryEmpty)
        {
            bool hasAnySlotsUsed = false;
            var slots = inventory.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    hasAnySlotsUsed = true;
                    break;
                }
            }

            if (hasAnySlotsUsed)
            {
                granted = true;
                return;
            }
        }

        ItemDefinition resolvedWeapon = ResolveWeaponDefinition();

        for (int i = 0; i < startingItems.Length; i++)
        {
            if (startingItems[i] != null)
            {
                inventory.TryAddItem(startingItems[i], 1);
            }
        }

        for (int i = 0; i < startingAmmo.Length; i++)
        {
            AmmoGrant ammo = startingAmmo[i];
            if (string.IsNullOrWhiteSpace(ammo.ammoItemId) || ammo.amount <= 0)
            {
                continue;
            }

            ItemDefinition ammoDefinition = ItemDefinitionLookup.FindById(ammo.ammoItemId);
            if (ammoDefinition != null)
            {
                inventory.TryAddItem(ammoDefinition, ammo.amount);
            }
            else
            {
                // Ammo can live in legacy string inventory if there isn't an ItemDefinition slot entry yet.
                inventory.AddItem(ammo.ammoItemId, ammo.ammoItemId, ammo.amount);
            }
        }

        if (resolvedWeapon != null)
        {
            if (!inventory.HasItem(resolvedWeapon))
            {
                inventory.TryAddItem(resolvedWeapon, 1);
            }

            inventory.Equip(resolvedWeapon);
        }

        granted = true;
    }

    private ItemDefinition ResolveWeaponDefinition()
    {
        if (startingWeapon != null)
        {
            return startingWeapon;
        }

        if (!string.IsNullOrWhiteSpace(fallbackWeaponItemId))
        {
            startingWeapon = ItemDefinitionLookup.FindById(fallbackWeaponItemId);
        }

        return startingWeapon;
    }
}
