using UnityEngine;

/// <summary>
/// Interactable that opens a ShopWindowUI when the player interacts.
/// Attach alongside a ShopKeeper on any NPC or terminal object.
/// </summary>
[RequireComponent(typeof(ShopKeeper))]
public class ShopInteractable : InteractableBase
{
    private ShopKeeper shopKeeper;

    private void Awake()
    {
        shopKeeper = GetComponent<ShopKeeper>();
    }

    public override string GetPrompt(PlayerInteraction interactor, InventorySystem inventory)
    {
        string name = shopKeeper != null ? shopKeeper.ShopName : "Merchant";
        return $"[{InputReader.GetInteractLabel()}] Trade with {name}";
    }

    public override int GetPriority(PlayerInteraction interactor) => 4;

    public override void Interact(PlayerInteraction interactor, InventorySystem inventory)
    {
        if (shopKeeper == null || interactor == null) return;

        ShopWindowUI shopUI = FindAnyObjectByType<ShopWindowUI>();
        if (shopUI == null)
        {
            shopUI = CreateShopWindow();
        }

        CurrencyWallet wallet = interactor.GetComponent<CurrencyWallet>();
        ReputationSystem rep = interactor.GetComponent<ReputationSystem>();

        shopUI.Open(shopKeeper, inventory, wallet, rep);
    }

    private static ShopWindowUI CreateShopWindow()
    {
        GameObject go = new GameObject("ShopWindowUI");
        return go.AddComponent<ShopWindowUI>();
    }
}
