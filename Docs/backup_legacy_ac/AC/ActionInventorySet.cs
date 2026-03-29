using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInventorySet : Action
	{
		public InvAction invAction;

		public int parameterID = -1;

		public int invID;

		public int replaceParameterID = -1;

		public int invIDReplace;

		protected int invNumber;

		protected int replaceInvNumber;

		public bool setAmount;

		public int amountParameterID = -1;

		public int amount = 1;

		public bool setPlayer;

		public int playerID;

		public bool addToFront;

		public bool removeLast;

		public ActionInventorySet()
		{
			isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Add or remove";
			description = "Adds or removes an item from the Player's inventory. Items are defined in the Inventory Manager. If the player can carry multiple amounts of the item, more options will show.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			invID = AssignInvItemID(parameters, parameterID, invID);
			invIDReplace = AssignInvItemID(parameters, replaceParameterID, invIDReplace);
			amount = AssignInteger(parameters, amountParameterID, amount);
		}

		public override float Run()
		{
			if ((bool)KickStarter.runtimeInventory)
			{
				if (!setAmount)
				{
					amount = 1;
				}
				int num = -1;
				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && setPlayer)
				{
					num = playerID;
				}
				if (invAction == InvAction.Add)
				{
					KickStarter.runtimeInventory.Add(invID, amount, false, num, addToFront);
				}
				else if (invAction == InvAction.Remove)
				{
					if (removeLast)
					{
						KickStarter.runtimeInventory.SetNull();
						KickStarter.runtimeInventory.Remove(KickStarter.runtimeInventory.LastSelectedItem);
					}
					else
					{
						if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.id == invID)
						{
							KickStarter.runtimeInventory.SetNull();
						}
						if (num >= 0)
						{
							if (setAmount)
							{
								KickStarter.runtimeInventory.RemoveFromOtherPlayer(invID, amount, num);
							}
							else
							{
								KickStarter.runtimeInventory.RemoveFromOtherPlayer(invID, num);
							}
						}
						else if (setAmount)
						{
							KickStarter.runtimeInventory.Remove(invID, amount);
						}
						else
						{
							KickStarter.runtimeInventory.Remove(invID);
						}
					}
				}
				else if (invAction == InvAction.Replace)
				{
					if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.id == invIDReplace)
					{
						KickStarter.runtimeInventory.SetNull();
					}
					KickStarter.runtimeInventory.Replace(invID, invIDReplace, amount);
				}
			}
			return 0f;
		}

		public static ActionInventorySet CreateNew_Add(int itemID, bool addToFront = false, int amountToAdd = 1, int playerID = -1)
		{
			ActionInventorySet actionInventorySet = ScriptableObject.CreateInstance<ActionInventorySet>();
			actionInventorySet.invAction = InvAction.Add;
			actionInventorySet.invID = itemID;
			actionInventorySet.addToFront = addToFront;
			actionInventorySet.amount = amountToAdd;
			actionInventorySet.playerID = playerID;
			return actionInventorySet;
		}

		public static ActionInventorySet CreateNew_Remove(int itemID, bool removeAllInstances = true, int amountToRemove = 1, int playerID = -1)
		{
			ActionInventorySet actionInventorySet = ScriptableObject.CreateInstance<ActionInventorySet>();
			actionInventorySet.invAction = InvAction.Remove;
			actionInventorySet.invID = itemID;
			actionInventorySet.setAmount = !removeAllInstances;
			actionInventorySet.amount = amountToRemove;
			actionInventorySet.playerID = playerID;
			return actionInventorySet;
		}

		public static ActionInventorySet CreateNew_Replace(int itemIDToAdd, int itemIDToRemove, int amountToAdd = 1)
		{
			ActionInventorySet actionInventorySet = ScriptableObject.CreateInstance<ActionInventorySet>();
			actionInventorySet.invAction = InvAction.Replace;
			actionInventorySet.invID = itemIDToAdd;
			actionInventorySet.invIDReplace = itemIDToRemove;
			actionInventorySet.amount = amountToAdd;
			return actionInventorySet;
		}
	}
}
