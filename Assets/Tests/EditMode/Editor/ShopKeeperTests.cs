using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ShopKeeperTests
{
    [Test]
    public void TryBuy_FailsWhenNoStock_DoesNotChargeOrAddItem()
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "test_item";
        item.stackable = false;
        item.maxStack = 1;
        item.buyPrice = 10;

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        GameObject walletGo = new GameObject("Wallet");
        CurrencyWallet wallet = walletGo.AddComponent<CurrencyWallet>();
        InvokePrivateMethod(wallet, "Awake");

        GameObject shopGo = new GameObject("Shop");
        ShopKeeper shop = shopGo.AddComponent<ShopKeeper>();
        shop.AddStock(item, 0, item.buyPrice);

        bool result = shop.TryBuy(shop.ShopInventory[0], inventory, wallet, null);

        Assert.IsFalse(result, "Purchase should fail when stock is zero.");
        Assert.AreEqual(100, wallet.GetAmount("gold"), "Wallet should not be charged when stock is empty.");
        Assert.AreEqual(0, inventory.GetItemCount(item), "Inventory should not receive item when stock is empty.");
        Assert.AreEqual(0, shop.ShopInventory[0].stock, "Stock should remain at zero.");

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(walletGo);
        Object.DestroyImmediate(shopGo);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void TryBuy_FailsWhenNotEnoughMoney_DoesNotConsumeStock()
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "test_item";
        item.stackable = false;
        item.maxStack = 1;
        item.buyPrice = 250;

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        GameObject walletGo = new GameObject("Wallet");
        CurrencyWallet wallet = walletGo.AddComponent<CurrencyWallet>();
        InvokePrivateMethod(wallet, "Awake");

        GameObject shopGo = new GameObject("Shop");
        ShopKeeper shop = shopGo.AddComponent<ShopKeeper>();
        shop.AddStock(item, 1, item.buyPrice);

        bool result = shop.TryBuy(shop.ShopInventory[0], inventory, wallet, null);

        Assert.IsFalse(result, "Purchase should fail when wallet cannot afford item.");
        Assert.AreEqual(100, wallet.GetAmount("gold"), "Wallet should remain unchanged when purchase fails.");
        Assert.AreEqual(1, shop.ShopInventory[0].stock, "Stock should not decrease when wallet cannot afford.");
        Assert.AreEqual(0, inventory.GetItemCount(item), "Inventory should not gain item when purchase fails.");

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(walletGo);
        Object.DestroyImmediate(shopGo);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void TryBuy_FailsWhenInventoryFull_DoesNotChargeOrConsumeStock()
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "test_item";
        item.stackable = false;
        item.maxStack = 1;
        item.buyPrice = 10;

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        SetPrivateField(inventory, "maxSlots", 0);
        InvokePrivateMethod(inventory, "Awake");

        GameObject walletGo = new GameObject("Wallet");
        CurrencyWallet wallet = walletGo.AddComponent<CurrencyWallet>();
        InvokePrivateMethod(wallet, "Awake");

        GameObject shopGo = new GameObject("Shop");
        ShopKeeper shop = shopGo.AddComponent<ShopKeeper>();
        shop.AddStock(item, 1, item.buyPrice);

        bool result = shop.TryBuy(shop.ShopInventory[0], inventory, wallet, null);

        Assert.IsFalse(result, "Purchase should fail when inventory has no slots.");
        Assert.AreEqual(100, wallet.GetAmount("gold"), "Wallet should not be charged on failure.");
        Assert.AreEqual(1, shop.ShopInventory[0].stock, "Stock should remain unchanged on failure.");

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(walletGo);
        Object.DestroyImmediate(shopGo);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void TryBuy_RollsBackItem_WhenSpendFailsAfterAdd()
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "test_item";
        item.stackable = false;
        item.maxStack = 1;
        item.buyPrice = 10;

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        GameObject walletGo = new GameObject("Wallet");
        CurrencyWallet wallet = walletGo.AddComponent<CurrencyWallet>();
        InvokePrivateMethod(wallet, "Awake");

        // Simulate an external wallet state change between affordability check and final spend.
        inventory.ItemAdded += _ => wallet.TrySpend("gold", wallet.GetAmount("gold"));

        GameObject shopGo = new GameObject("Shop");
        ShopKeeper shop = shopGo.AddComponent<ShopKeeper>();
        shop.AddStock(item, 1, item.buyPrice);

        bool result = shop.TryBuy(shop.ShopInventory[0], inventory, wallet, null);

        Assert.IsFalse(result, "Purchase should fail when spend step fails.");
        Assert.AreEqual(0, inventory.GetItemCount(item), "Inventory should be rolled back when spend fails.");
        Assert.AreEqual(1, shop.ShopInventory[0].stock, "Stock should remain unchanged when spend fails.");

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(walletGo);
        Object.DestroyImmediate(shopGo);
        Object.DestroyImmediate(item);
    }

    [Test]
    public void TryBuy_Succeeds_WhenSpaceAndFundsAvailable()
    {
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = "test_item";
        item.stackable = false;
        item.maxStack = 1;
        item.buyPrice = 10;

        GameObject inventoryGo = new GameObject("Inventory");
        InventorySystem inventory = inventoryGo.AddComponent<InventorySystem>();
        InvokePrivateMethod(inventory, "Awake");

        GameObject walletGo = new GameObject("Wallet");
        CurrencyWallet wallet = walletGo.AddComponent<CurrencyWallet>();
        InvokePrivateMethod(wallet, "Awake");

        GameObject shopGo = new GameObject("Shop");
        ShopKeeper shop = shopGo.AddComponent<ShopKeeper>();
        shop.AddStock(item, 1, item.buyPrice);

        bool result = shop.TryBuy(shop.ShopInventory[0], inventory, wallet, null);

        Assert.IsTrue(result, "Purchase should succeed when there is capacity and funds.");
        Assert.AreEqual(90, wallet.GetAmount("gold"), "Wallet should be charged item price.");
        Assert.AreEqual(0, shop.ShopInventory[0].stock, "Stock should decrease after successful purchase.");
        Assert.AreEqual(1, inventory.GetItemCount(item), "Item should be added to inventory.");

        Object.DestroyImmediate(inventoryGo);
        Object.DestroyImmediate(walletGo);
        Object.DestroyImmediate(shopGo);
        Object.DestroyImmediate(item);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(target, null);
    }
}
