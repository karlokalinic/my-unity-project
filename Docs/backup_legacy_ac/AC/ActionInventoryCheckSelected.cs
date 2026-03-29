using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInventoryCheckSelected : ActionCheck
	{
		public enum SelectedCheckMethod
		{
			SpecificItem = 0,
			InSpecificCategory = 1,
			NoneSelected = 2
		}

		public int parameterID = -1;

		public int invID;

		public int binID;

		public bool checkNothing;

		public bool includeLast;

		[SerializeField]
		protected SelectedCheckMethod selectedCheckMethod;

		public ActionInventoryCheckSelected()
		{
			isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Check selected";
			description = "Queries whether or not the chosen item, or no item, is currently selected.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			invID = AssignInvItemID(parameters, parameterID, invID);
			Upgrade();
		}

		public override bool CheckCondition()
		{
			if ((bool)KickStarter.runtimeInventory)
			{
				switch (selectedCheckMethod)
				{
				case SelectedCheckMethod.NoneSelected:
					if (KickStarter.runtimeInventory.SelectedItem == null)
					{
						return true;
					}
					break;
				case SelectedCheckMethod.SpecificItem:
					if (includeLast)
					{
						if (KickStarter.runtimeInventory.LastSelectedItem != null && KickStarter.runtimeInventory.LastSelectedItem.id == invID)
						{
							return true;
						}
					}
					else if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.id == invID)
					{
						return true;
					}
					break;
				case SelectedCheckMethod.InSpecificCategory:
					if (includeLast)
					{
						if (KickStarter.runtimeInventory.LastSelectedItem != null && KickStarter.runtimeInventory.LastSelectedItem.binID == binID)
						{
							return true;
						}
					}
					else if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.binID == binID)
					{
						return true;
					}
					break;
				}
			}
			return false;
		}

		private void Upgrade()
		{
			if (checkNothing)
			{
				selectedCheckMethod = SelectedCheckMethod.NoneSelected;
				checkNothing = false;
			}
		}

		public static ActionInventoryCheckSelected CreateNew_SpecificItem(int itemID, bool includeLastSelected = false)
		{
			ActionInventoryCheckSelected actionInventoryCheckSelected = ScriptableObject.CreateInstance<ActionInventoryCheckSelected>();
			actionInventoryCheckSelected.selectedCheckMethod = SelectedCheckMethod.SpecificItem;
			actionInventoryCheckSelected.invID = itemID;
			actionInventoryCheckSelected.includeLast = includeLastSelected;
			return actionInventoryCheckSelected;
		}

		public static ActionInventoryCheckSelected CreateNew_InSpecificCategory(int categoryID, bool includeLastSelected = false)
		{
			ActionInventoryCheckSelected actionInventoryCheckSelected = ScriptableObject.CreateInstance<ActionInventoryCheckSelected>();
			actionInventoryCheckSelected.selectedCheckMethod = SelectedCheckMethod.InSpecificCategory;
			actionInventoryCheckSelected.binID = categoryID;
			actionInventoryCheckSelected.includeLast = includeLastSelected;
			return actionInventoryCheckSelected;
		}

		public static ActionInventoryCheckSelected CreateNew_NoneSelected()
		{
			ActionInventoryCheckSelected actionInventoryCheckSelected = ScriptableObject.CreateInstance<ActionInventoryCheckSelected>();
			actionInventoryCheckSelected.selectedCheckMethod = SelectedCheckMethod.NoneSelected;
			return actionInventoryCheckSelected;
		}
	}
}
