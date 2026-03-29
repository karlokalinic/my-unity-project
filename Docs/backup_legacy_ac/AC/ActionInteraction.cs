using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInteraction : Action
	{
		public int parameterID = -1;

		public int constantID;

		public Hotspot hotspot;

		protected Hotspot runtimeHotspot;

		public InteractionType interactionType;

		public ChangeType changeType;

		public int number;

		public ActionInteraction()
		{
			isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Change interaction";
			description = "Enables and disables individual Interactions on a Hotspot.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile(parameters, parameterID, constantID, hotspot);
		}

		public override float Run()
		{
			if (runtimeHotspot == null)
			{
				return 0f;
			}
			if (interactionType == InteractionType.Use)
			{
				if (runtimeHotspot.useButtons.Count > number)
				{
					ChangeButton(runtimeHotspot.useButtons[number]);
				}
				else
				{
					LogWarning("Cannot change Hotspot " + runtimeHotspot.gameObject.name + "'s Use button " + number + " because it doesn't exist!");
				}
			}
			else if (interactionType == InteractionType.Examine)
			{
				ChangeButton(runtimeHotspot.lookButton);
			}
			else if (interactionType == InteractionType.Inventory)
			{
				if (runtimeHotspot.invButtons.Count > number)
				{
					ChangeButton(runtimeHotspot.invButtons[number]);
				}
				else
				{
					LogWarning("Cannot change Hotspot " + runtimeHotspot.gameObject.name + "'s Inventory button " + number + " because it doesn't exist!");
				}
			}
			runtimeHotspot.ResetMainIcon();
			return 0f;
		}

		protected void ChangeButton(Button button)
		{
			if (button != null)
			{
				if (changeType == ChangeType.Enable)
				{
					button.isDisabled = false;
				}
				else if (changeType == ChangeType.Disable)
				{
					button.isDisabled = true;
				}
			}
		}

		public static ActionInteraction CreateNew(Hotspot hotspot, ChangeType changeType, InteractionType interactionType, int interactionIndex = 0)
		{
			ActionInteraction actionInteraction = ScriptableObject.CreateInstance<ActionInteraction>();
			actionInteraction.hotspot = hotspot;
			actionInteraction.interactionType = interactionType;
			actionInteraction.changeType = changeType;
			actionInteraction.number = interactionIndex;
			return actionInteraction;
		}
	}
}
