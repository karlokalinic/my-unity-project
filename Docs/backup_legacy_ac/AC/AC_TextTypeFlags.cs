namespace AC
{
	public enum AC_TextTypeFlags
	{
		Speech = 1,
		Hotspot = 2,
		DialogueOption = 4,
		InventoryItem = 8,
		CursorIcon = 0x10,
		MenuElement = 0x20,
		HotspotPrefix = 0x40,
		JournalEntry = 0x80,
		InventoryItemProperty = 0x100,
		Variable = 0x200,
		Character = 0x400,
		Document = 0x800,
		Custom = 0x1000,
		Objective = 0x2000
	}
}
