using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInventoryCheck : ActionCheck
	{
		protected enum InvCheckType
		{
			CarryingSpecificItem = 0,
			NumberOfItemsCarrying = 1
		}

		public enum IntCondition
		{
			EqualTo = 0,
			NotEqualTo = 1,
			LessThan = 2,
			MoreThan = 3
		}

		public int parameterID = -1;

		public int invID;

		protected int invNumber;

		[SerializeField]
		protected InvCheckType invCheckType;

		public bool checkNumberInCategory;

		public int categoryIDToCheck;

		public bool doCount;

		public int intValueParameterID = -1;

		public int intValue = 1;

		public IntCondition intCondition;

		public bool setPlayer;

		public int playerID;

		public ActionInventoryCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Check";
			description = "Queries whether or not the player is carrying an item. If the player can carry multiple amounts of the item, more options will show.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			invID = AssignInvItemID(parameters, parameterID, invID);
			intValue = AssignInteger(parameters, intValueParameterID, intValue);
		}

		public override bool CheckCondition()
		{
			int num = 0;
			if (invCheckType == InvCheckType.CarryingSpecificItem)
			{
				num = ((KickStarter.settingsManager.playerSwitching != PlayerSwitching.Allow || KickStarter.settingsManager.shareInventory || !setPlayer) ? KickStarter.runtimeInventory.GetCount(invID) : KickStarter.runtimeInventory.GetCount(invID, playerID));
			}
			else if (invCheckType == InvCheckType.NumberOfItemsCarrying)
			{
				num = ((KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && setPlayer) ? ((!checkNumberInCategory) ? KickStarter.runtimeInventory.GetNumberOfItemsCarried(playerID) : KickStarter.runtimeInventory.GetNumberOfItemsCarriedInCategory(playerID, categoryIDToCheck)) : ((!checkNumberInCategory) ? KickStarter.runtimeInventory.GetNumberOfItemsCarried() : KickStarter.runtimeInventory.GetNumberOfItemsCarriedInCategory(categoryIDToCheck)));
			}
			if (doCount || invCheckType == InvCheckType.NumberOfItemsCarrying)
			{
				if (intCondition == IntCondition.EqualTo)
				{
					if (num == intValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (num != intValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (num < intValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan && num > intValue)
				{
					return true;
				}
			}
			else if (num > 0)
			{
				return true;
			}
			return false;
		}

		public static ActionInventoryCheck CreateNew_CarryingSpecificItem(int itemID)
		{
			ActionInventoryCheck actionInventoryCheck = ScriptableObject.CreateInstance<ActionInventoryCheck>();
			actionInventoryCheck.invCheckType = InvCheckType.CarryingSpecificItem;
			actionInventoryCheck.invID = itemID;
			return actionInventoryCheck;
		}

		public static ActionInventoryCheck CreateNew_NumberOfItemsCarrying(int numItems, IntCondition condition = IntCondition.EqualTo)
		{
			ActionInventoryCheck actionInventoryCheck = ScriptableObject.CreateInstance<ActionInventoryCheck>();
			actionInventoryCheck.invCheckType = InvCheckType.NumberOfItemsCarrying;
			actionInventoryCheck.intValue = numItems;
			actionInventoryCheck.intCondition = condition;
			return actionInventoryCheck;
		}
	}
}
