using System;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Logic/Set Inventory Interaction parameters")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_set_inventory_interaction_parameters.html")]
	public class SetInventoryInteractionParameters : SetParametersBase
	{
		protected enum InteractionType
		{
			Use = 0,
			Examine = 1
		}

		[SerializeField]
		protected int itemID;

		[SerializeField]
		protected int cursorIndex;

		[SerializeField]
		protected InteractionType interactionType;

		protected void OnEnable()
		{
			EventManager.OnInventoryInteract = (EventManager.Delegate_ChangeInventory)Delegate.Combine(EventManager.OnInventoryInteract, new EventManager.Delegate_ChangeInventory(OnInventoryInteract));
		}

		protected void OnDisable()
		{
			EventManager.OnInventoryInteract = (EventManager.Delegate_ChangeInventory)Delegate.Remove(EventManager.OnInventoryInteract, new EventManager.Delegate_ChangeInventory(OnInventoryInteract));
		}

		protected void OnInventoryInteract(InvItem invItem, int iconID)
		{
			if (invItem.id != itemID)
			{
				return;
			}
			if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
			{
				if (cursorIndex < invItem.interactions.Count && invItem.interactions[cursorIndex].icon.id == iconID)
				{
					AssignParameterValues(invItem.interactions[cursorIndex].actionList);
				}
				return;
			}
			switch (interactionType)
			{
			case InteractionType.Use:
				if (iconID == 0)
				{
					AssignParameterValues(invItem.useActionList);
				}
				break;
			case InteractionType.Examine:
				if (iconID == KickStarter.cursorManager.lookCursor_ID)
				{
					AssignParameterValues(invItem.lookActionList);
				}
				break;
			}
		}
	}
}
