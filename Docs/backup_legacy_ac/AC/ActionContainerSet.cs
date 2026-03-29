using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionContainerSet : Action
	{
		public enum ContainerAction
		{
			Add = 0,
			Remove = 1,
			RemoveAll = 2
		}

		public ContainerAction containerAction;

		public int invParameterID = -1;

		public int invID;

		protected int invNumber;

		public bool useActive;

		public int constantID;

		public int parameterID = -1;

		public Container container;

		protected Container runtimeContainer;

		public bool setAmount;

		public int amountParameterID = -1;

		public int amount = 1;

		public bool transferToPlayer;

		public bool removeAllInstances;

		public ActionContainerSet()
		{
			isDisplayed = true;
			category = ActionCategory.Container;
			title = "Add or remove";
			description = "Adds or removes Inventory items from a Container.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			invID = AssignInvItemID(parameters, invParameterID, invID);
			amount = AssignInteger(parameters, amountParameterID, amount);
			if (useActive)
			{
				runtimeContainer = KickStarter.playerInput.activeContainer;
			}
			else
			{
				runtimeContainer = AssignFile(parameters, parameterID, constantID, container);
			}
		}

		public override float Run()
		{
			if (runtimeContainer == null)
			{
				return 0f;
			}
			if (!setAmount)
			{
				amount = 1;
			}
			if (containerAction == ContainerAction.Add)
			{
				runtimeContainer.Add(invID, amount);
			}
			else if (containerAction == ContainerAction.Remove)
			{
				ContainerItem[] itemsWithInvID = runtimeContainer.GetItemsWithInvID(invID);
				ContainerItem[] array = itemsWithInvID;
				foreach (ContainerItem containerItem in array)
				{
					ContainerItem containerItem2 = new ContainerItem(containerItem);
					InvItem item = KickStarter.inventoryManager.GetItem(containerItem.linkedID);
					if (!KickStarter.runtimeInventory.IsCarryingItem(item.id) || item.canCarryMultiple)
					{
						if (transferToPlayer && KickStarter.runtimeInventory.CanTransferContainerItemsToInventory(containerItem))
						{
							int num = Mathf.Min(amount, containerItem.count);
							KickStarter.runtimeInventory.Add(containerItem.linkedID, num);
						}
						runtimeContainer.Remove(containerItem, amount);
						KickStarter.eventManager.Call_OnUseContainer(false, runtimeContainer, containerItem2);
						if (!removeAllInstances)
						{
							break;
						}
					}
				}
			}
			else if (containerAction == ContainerAction.RemoveAll)
			{
				if (transferToPlayer)
				{
					for (int j = 0; j < runtimeContainer.items.Count; j++)
					{
						ContainerItem containerItem3 = runtimeContainer.items[j];
						if (!containerItem3.IsEmpty)
						{
							ContainerItem containerItem4 = new ContainerItem(containerItem3);
							InvItem item2 = KickStarter.inventoryManager.GetItem(containerItem3.linkedID);
							if ((!KickStarter.runtimeInventory.IsCarryingItem(item2.id) || item2.canCarryMultiple) && KickStarter.runtimeInventory.CanTransferContainerItemsToInventory(containerItem3))
							{
								KickStarter.runtimeInventory.Add(containerItem3.linkedID, containerItem3.count);
								runtimeContainer.items.Remove(containerItem3);
								j = -1;
								KickStarter.eventManager.Call_OnUseContainer(false, runtimeContainer, containerItem4);
							}
						}
					}
				}
				else
				{
					runtimeContainer.RemoveAll();
				}
			}
			return 0f;
		}

		public static ActionContainerSet CreateNew_Add(Container containerToModify, int itemIDToAdd, int instancesToAdd = 1)
		{
			ActionContainerSet actionContainerSet = ScriptableObject.CreateInstance<ActionContainerSet>();
			actionContainerSet.containerAction = ContainerAction.Add;
			actionContainerSet.container = containerToModify;
			actionContainerSet.invID = itemIDToAdd;
			actionContainerSet.setAmount = true;
			actionContainerSet.amount = instancesToAdd;
			return actionContainerSet;
		}

		public static ActionContainerSet CreateNew_Remove(Container containerToModify, int itemIDToRemove, int instancesToRemove = 1, bool transferToPlayer = false)
		{
			ActionContainerSet actionContainerSet = ScriptableObject.CreateInstance<ActionContainerSet>();
			actionContainerSet.containerAction = ContainerAction.Remove;
			actionContainerSet.container = containerToModify;
			actionContainerSet.invID = itemIDToRemove;
			actionContainerSet.setAmount = true;
			actionContainerSet.amount = instancesToRemove;
			actionContainerSet.transferToPlayer = transferToPlayer;
			return actionContainerSet;
		}

		public static ActionContainerSet CreateNew_RemoveAll(Container containerToModify, bool transferToPlayer = false)
		{
			ActionContainerSet actionContainerSet = ScriptableObject.CreateInstance<ActionContainerSet>();
			actionContainerSet.containerAction = ContainerAction.RemoveAll;
			actionContainerSet.container = containerToModify;
			actionContainerSet.transferToPlayer = transferToPlayer;
			return actionContainerSet;
		}
	}
}
