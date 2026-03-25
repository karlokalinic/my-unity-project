using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Runtime inventory panel. Toggle with I key. Shows slots, equipment, and item details.
/// </summary>
public class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Key toggleKey = Key.I;

    private Canvas canvas;
    private RectTransform panel;
    private RectTransform contentRoot;
    private TextMeshProUGUI detailText;
    private readonly List<SlotWidget> slotWidgets = new List<SlotWidget>();
    private bool isOpen;

    private class SlotWidget
    {
        public RectTransform root;
        public Image icon;
        public TextMeshProUGUI label;
        public TextMeshProUGUI countText;
        public Button button;
        public int slotIndex;
    }

    private void Awake()
    {
        if (inventory == null) inventory = FindAnyObjectByType<PlayerMover>()?.GetComponent<InventorySystem>();
        BuildUI();
        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (TogglePressed()) Toggle();
    }

    private bool TogglePressed()
    {
        if (toggleKey == Key.None) return false;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        var keyControl = keyboard[toggleKey];
        return keyControl != null && keyControl.wasPressedThisFrame;
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        panel.gameObject.SetActive(isOpen);
        if (isOpen) RefreshSlots();
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.InventoryChanged += OnChanged;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.InventoryChanged -= OnChanged;
    }

    private void OnChanged()
    {
        if (isOpen) RefreshSlots();
    }

    private void RefreshSlots()
    {
        if (inventory == null) return;
        var slots = inventory.Slots;

        // Ensure enough widgets
        while (slotWidgets.Count < slots.Count) CreateSlotWidget(slotWidgets.Count);

        for (int i = 0; i < slotWidgets.Count; i++)
        {
            var w = slotWidgets[i];
            if (i >= slots.Count || slots[i].IsEmpty)
            {
                w.root.gameObject.SetActive(false);
                continue;
            }

            w.root.gameObject.SetActive(true);
            var slot = slots[i];
            w.label.text = slot.item.displayName;
            w.countText.text = slot.count > 1 ? $"x{slot.count}" : "";
            w.slotIndex = i;
            if (w.icon != null)
            {
                w.icon.sprite = slot.item.icon;
                w.icon.enabled = slot.item.icon != null;
            }
        }

        // Equipment summary
        string weaponName = inventory.WeaponSlot?.equippedItem != null ? inventory.WeaponSlot.equippedItem.displayName : "None";
        string armorName = inventory.ArmorSlot?.equippedItem != null ? inventory.ArmorSlot.equippedItem.displayName : "None";
        detailText.text = $"Weapon: {weaponName}\nArmor: {armorName}";
    }

    private void OnSlotClicked(int index)
    {
        if (inventory == null) return;
        var slots = inventory.Slots;
        if (index < 0 || index >= slots.Count || slots[index].IsEmpty) return;

        var item = slots[index].item;
        if (item.category == ItemDefinition.ItemCategory.Weapon || item.category == ItemDefinition.ItemCategory.Armor)
        {
            inventory.Equip(item);
            RefreshSlots();
        }
        else if (item.category == ItemDefinition.ItemCategory.Consumable)
        {
            CharacterStats stats = inventory.GetComponent<CharacterStats>();
            if (stats != null)
            {
                if (item.healAmount > 0) stats.Heal(item.healAmount);
                if (item.staminaRestoreAmount > 0) stats.RestoreStamina(item.staminaRestoreAmount);
            }
            inventory.TryRemoveItem(item, 1);
            RefreshSlots();
        }
    }

    private void BuildUI()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
        }

        panel = MakeRect("InventoryPanel", transform);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(400f, 500f);
        panel.anchoredPosition = Vector2.zero;
        Image panelBg = panel.gameObject.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);

        // Title
        RectTransform titleRt = MakeRect("Title", panel);
        SetAnchorsTop(titleRt, 0f, 30f);
        TextMeshProUGUI title = titleRt.gameObject.AddComponent<TextMeshProUGUI>();
        title.text = "INVENTORY";
        title.fontSize = 18f;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;

        // Scroll content
        contentRoot = MakeRect("Content", panel);
        contentRoot.anchorMin = new Vector2(0f, 0.15f);
        contentRoot.anchorMax = new Vector2(1f, 0.9f);
        contentRoot.offsetMin = new Vector2(8f, 0f);
        contentRoot.offsetMax = new Vector2(-8f, 0f);

        // Detail / equipment area
        RectTransform detailRt = MakeRect("Detail", panel);
        detailRt.anchorMin = Vector2.zero;
        detailRt.anchorMax = new Vector2(1f, 0.14f);
        detailRt.offsetMin = new Vector2(8f, 4f);
        detailRt.offsetMax = new Vector2(-8f, 0f);
        detailText = detailRt.gameObject.AddComponent<TextMeshProUGUI>();
        detailText.fontSize = 13f;
        detailText.color = new Color(0.7f, 0.7f, 0.7f);
        detailText.alignment = TextAlignmentOptions.TopLeft;
    }

    private void CreateSlotWidget(int index)
    {
        float y = -index * 30f;
        RectTransform rt = MakeRect($"Slot_{index}", contentRoot);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(0f, 28f);

        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 0.6f);

        Button btn = rt.gameObject.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
        btn.colors = cb;

        RectTransform labelRt = MakeRect("Label", rt);
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = new Vector2(0.75f, 1f);
        labelRt.offsetMin = new Vector2(6f, 0f);
        labelRt.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = 13f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;

        RectTransform countRt = MakeRect("Count", rt);
        countRt.anchorMin = new Vector2(0.75f, 0f);
        countRt.anchorMax = Vector2.one;
        countRt.offsetMin = Vector2.zero;
        countRt.offsetMax = new Vector2(-4f, 0f);
        TextMeshProUGUI countTxt = countRt.gameObject.AddComponent<TextMeshProUGUI>();
        countTxt.fontSize = 12f;
        countTxt.color = new Color(0.6f, 0.8f, 0.6f);
        countTxt.alignment = TextAlignmentOptions.MidlineRight;
        countTxt.raycastTarget = false;

        var widget = new SlotWidget
        {
            root = rt,
            label = label,
            countText = countTxt,
            button = btn,
            slotIndex = index
        };

        int capturedIndex = index;
        btn.onClick.AddListener(() => OnSlotClicked(capturedIndex));

        slotWidgets.Add(widget);
    }

    private void SetAnchorsTop(RectTransform rt, float yOffset, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, yOffset);
        rt.sizeDelta = new Vector2(0f, height);
    }

    private static RectTransform MakeRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
}
