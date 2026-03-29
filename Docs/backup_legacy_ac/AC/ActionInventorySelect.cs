using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInventorySelect : Action
	{
		public enum InventorySelectType
		{
			SelectItem = 0,
			DeselectActive = 1
		}

		public InventorySelectType selectType;

		public SelectItemMode selectItemMode;

		public bool giveToPlayer;

		public int parameterID = -1;

		public int invID;

		protected int invNumber;

		public ActionInventorySelect()
		{
			isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Select";
			description = "Selects a chosen inventory item, as though the player clicked on it in the Inventory menu. Will optionally add the specified item to the inventory if it is not currently held.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			invID = AssignInvItemID(parameters, parameterID, invID);
		}

		public override float Run()
		{
			if ((bool)KickStarter.runtimeInventory)
			{
				if (selectType == InventorySelectType.DeselectActive)
				{
					KickStarter.runtimeInventory.SetNull();
				}
				else
				{
					if (!KickStarter.settingsManager.CanSelectItems(true))
					{
						return 0f;
					}
					if (giveToPlayer)
					{
						KickStarter.runtimeInventory.Add(invID);
					}
					KickStarter.runtimeInventory.SelectItemByID(invID, selectItemMode);
				}
			}
			return 0f;
		}

		public static ActionInventorySelect CreateNew_Select(int itemID, bool addIfNotCarrying = false, SelectItemMode selectItemMode = SelectItemMode.Use)
		{
			ActionInventorySelect actionInventorySelect = ScriptableObject.CreateInstance<ActionInventorySelect>();
			actionInventorySelect.selectType = InventorySelectType.SelectItem;
			actionInventorySelect.invID = itemID;
			actionInventorySelect.giveToPlayer = addIfNotCarrying;
			actionInventorySelect.selectItemMode = selectItemMode;
			return actionInventorySelect;
		}

		public static ActionInventorySelect CreateNew_DeselectActive()
		{
			ActionInventorySelect actionInventorySelect = ScriptableObject.CreateInstance<ActionInventorySelect>();
			actionInventorySelect.selectType = InventorySelectType.DeselectActive;
			return actionInventorySelect;
		}
	}
}
