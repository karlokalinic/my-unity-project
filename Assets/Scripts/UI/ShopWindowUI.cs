using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime shop window. Opened by SkillCheckNpcInteractable or ShopKeeper interaction.
/// Shows buy/sell tabs with prices affected by reputation and difficulty.
/// </summary>
public class ShopWindowUI : MonoBehaviour
{
    [SerializeField] private ShopKeeper currentShop;
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private CurrencyWallet playerWallet;
    [SerializeField] private ReputationSystem playerRep;

    private Canvas canvas;
    private RectTransform panel;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI goldText;
    private TextMeshProUGUI messageText;
    private RectTransform contentRoot;
    private readonly List<ShopRowWidget> rows = new List<ShopRowWidget>();
    private bool isOpen;
    private bool buyMode = true;
    private bool ownsInputContext;

    private class ShopRowWidget
    {
        public RectTransform root;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI priceLabel;
        public Button actionBtn;
        public TextMeshProUGUI actionLabel;
        public int index;
    }

    private void Awake()
    {
        BuildUI();
        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        bool keyboardCancel = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepadCancel = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        if (keyboardCancel || gamepadCancel) Close();
    }

    public void Open(ShopKeeper shop, InventorySystem inv, CurrencyWallet wallet, ReputationSystem rep)
    {
        currentShop = shop;
        playerInventory = inv;
        playerWallet = wallet;
        playerRep = rep;
        buyMode = true;
        isOpen = true;
        AcquireUiContext();
        panel.gameObject.SetActive(true);
        titleText.text = shop != null ? shop.ShopName : "SHOP";
        RefreshContent();
    }

    public void Close()
    {
        isOpen = false;
        panel.gameObject.SetActive(false);
        currentShop = null;
        ReleaseUiContext();
    }

    public void SetBuyMode(bool buy)
    {
        buyMode = buy;
        RefreshContent();
    }

    private void RefreshContent()
    {
        if (currentShop == null || playerInventory == null) return;

        goldText.text = $"Gold: {(playerWallet != null ? playerWallet.GetAmount("gold") : 0)}";
        messageText.text = "";

        // Clear old rows
        for (int i = 0; i < rows.Count; i++) rows[i].root.gameObject.SetActive(false);

        if (buyMode)
            PopulateBuyList();
        else
            PopulateSellList();
    }

    private void PopulateBuyList()
    {
        var stock = currentShop.ShopInventory;
        for (int i = 0; i < stock.Count; i++)
        {
            if (stock[i].stock <= 0) continue;
            var row = EnsureRow(i);
            row.nameLabel.text = $"{stock[i].item.displayName} (x{stock[i].stock})";
            int price = currentShop.GetBuyPrice(stock[i], playerRep);
            row.priceLabel.text = $"{price}g";
            row.actionLabel.text = "BUY";
            row.index = i;
            row.root.gameObject.SetActive(true);
        }
    }

    private void PopulateSellList()
    {
        var slots = playerInventory.Slots;
        int rowIdx = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty) continue;
            if (slots[i].item.category == ItemDefinition.ItemCategory.QuestItem) continue;

            var row = EnsureRow(rowIdx);
            row.nameLabel.text = $"{slots[i].item.displayName} (x{slots[i].count})";
            int price = currentShop.GetSellPrice(slots[i].item, playerRep);
            row.priceLabel.text = $"{price}g";
            row.actionLabel.text = "SELL";
            row.index = i;
            row.root.gameObject.SetActive(true);
            rowIdx++;
        }
    }

    private void OnActionClicked(int index)
    {
        if (currentShop == null) return;

        if (buyMode)
        {
            var stock = currentShop.ShopInventory;
            if (index >= 0 && index < stock.Count)
            {
                bool ok = currentShop.TryBuy(stock[index], playerInventory, playerWallet, playerRep);
                messageText.text = ok ? $"Bought {stock[index].item.displayName}" : "Cannot afford!";
            }
        }
        else
        {
            var slots = playerInventory.Slots;
            if (index >= 0 && index < slots.Count && !slots[index].IsEmpty)
            {
                bool ok = currentShop.TrySell(slots[index].item, playerInventory, playerWallet, playerRep);
                messageText.text = ok ? "Sold!" : "Cannot sell.";
            }
        }

        RefreshContent();
    }

    private ShopRowWidget EnsureRow(int idx)
    {
        while (rows.Count <= idx) CreateRow(rows.Count);
        return rows[idx];
    }

    private void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 105;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        panel = MakeRect("ShopPanel", transform);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(420f, 460f);
        panel.anchoredPosition = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.09f, 0.94f);

        // Title
        titleText = MakeLabel(panel, "Title", new Vector2(0f, -8f), 20f, TextAlignmentOptions.Center);

        // Gold
        goldText = MakeLabel(panel, "Gold", new Vector2(8f, -36f), 14f, TextAlignmentOptions.TopLeft);

        // Buy/Sell tabs
        CreateTabButton(panel, "BuyTab", new Vector2(-60f, -58f), "BUY", true);
        CreateTabButton(panel, "SellTab", new Vector2(60f, -58f), "SELL", false);

        // Content area
        contentRoot = MakeRect("Content", panel);
        contentRoot.anchorMin = new Vector2(0f, 0.1f);
        contentRoot.anchorMax = new Vector2(1f, 0.78f);
        contentRoot.offsetMin = new Vector2(8f, 0f);
        contentRoot.offsetMax = new Vector2(-8f, 0f);

        // Message
        messageText = MakeLabel(panel, "Message", new Vector2(0f, 0f), 12f, TextAlignmentOptions.Center);
        var msgRt = messageText.GetComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0f, 0f);
        msgRt.anchorMax = new Vector2(1f, 0.1f);
        msgRt.offsetMin = Vector2.zero;
        msgRt.offsetMax = Vector2.zero;
        messageText.color = new Color(0.9f, 0.8f, 0.3f);
    }

    private void CreateTabButton(RectTransform parent, string name, Vector2 pos, string label, bool isBuy)
    {
        RectTransform rt = MakeRect(name, parent);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(100f, 26f);
        rt.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() => SetBuyMode(isBuy));

        var txt = MakeRect("L", rt);
        txt.anchorMin = Vector2.zero;
        txt.anchorMax = Vector2.one;
        txt.offsetMin = Vector2.zero;
        txt.offsetMax = Vector2.zero;
        var tmp = txt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 13f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }

    private void CreateRow(int idx)
    {
        float y = -idx * 28f;
        RectTransform rt = MakeRect($"Row_{idx}", contentRoot);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(0f, 26f);
        rt.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 0.5f);

        var nameRt = MakeRect("Name", rt);
        nameRt.anchorMin = Vector2.zero;
        nameRt.anchorMax = new Vector2(0.55f, 1f);
        nameRt.offsetMin = new Vector2(6f, 0f);
        nameRt.offsetMax = Vector2.zero;
        var nameL = nameRt.gameObject.AddComponent<TextMeshProUGUI>();
        nameL.fontSize = 12f;
        nameL.color = Color.white;
        nameL.alignment = TextAlignmentOptions.MidlineLeft;
        nameL.raycastTarget = false;

        var priceRt = MakeRect("Price", rt);
        priceRt.anchorMin = new Vector2(0.55f, 0f);
        priceRt.anchorMax = new Vector2(0.75f, 1f);
        priceRt.offsetMin = Vector2.zero;
        priceRt.offsetMax = Vector2.zero;
        var priceL = priceRt.gameObject.AddComponent<TextMeshProUGUI>();
        priceL.fontSize = 12f;
        priceL.color = new Color(0.9f, 0.8f, 0.3f);
        priceL.alignment = TextAlignmentOptions.MidlineRight;
        priceL.raycastTarget = false;

        var btnRt = MakeRect("Action", rt);
        btnRt.anchorMin = new Vector2(0.78f, 0.1f);
        btnRt.anchorMax = new Vector2(0.98f, 0.9f);
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;
        btnRt.gameObject.AddComponent<Image>().color = new Color(0.25f, 0.35f, 0.25f);
        Button btn = btnRt.gameObject.AddComponent<Button>();

        var actionTxtRt = MakeRect("AL", btnRt);
        actionTxtRt.anchorMin = Vector2.zero;
        actionTxtRt.anchorMax = Vector2.one;
        actionTxtRt.offsetMin = Vector2.zero;
        actionTxtRt.offsetMax = Vector2.zero;
        var actionL = actionTxtRt.gameObject.AddComponent<TextMeshProUGUI>();
        actionL.fontSize = 11f;
        actionL.alignment = TextAlignmentOptions.Center;
        actionL.color = Color.white;
        actionL.raycastTarget = false;

        var widget = new ShopRowWidget
        {
            root = rt,
            nameLabel = nameL,
            priceLabel = priceL,
            actionBtn = btn,
            actionLabel = actionL,
            index = idx
        };

        int cap = idx;
        btn.onClick.AddListener(() => OnActionClicked(widget.index));

        rows.Add(widget);
    }

    private TextMeshProUGUI MakeLabel(RectTransform parent, string name, Vector2 pos, float fontSize, TextAlignmentOptions align)
    {
        RectTransform rt = MakeRect(name, parent);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(0f, 24f);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static RectTransform MakeRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        ReleaseUiContext();
    }

    private void AcquireUiContext()
    {
        if (ownsInputContext)
        {
            return;
        }

        InputReader.PushContext(InputReader.InputContext.UI);
        ownsInputContext = true;
    }

    private void ReleaseUiContext()
    {
        if (!ownsInputContext)
        {
            return;
        }

        InputReader.PopContext(InputReader.InputContext.Gameplay);
        ownsInputContext = false;
    }
}
