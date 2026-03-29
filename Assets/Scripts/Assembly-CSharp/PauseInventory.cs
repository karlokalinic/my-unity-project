using AC;
using UnityEngine;

public class PauseInventory : MonoBehaviour
{
	private MenuInventoryBox inventoryBox;

	private void Start()
	{
		inventoryBox = PlayerMenus.GetElementWithName("Inventory", "InventoryBox") as MenuInventoryBox;
	}

	private void Update()
	{
		if (KickStarter.stateHandler.IsPaused() && KickStarter.runtimeDocuments.ActiveDocument != null)
		{
			inventoryBox.preventInteractions = true;
		}
		else
		{
			inventoryBox.preventInteractions = false;
		}
	}
}
