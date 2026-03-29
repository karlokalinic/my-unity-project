using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInteractionCheck : ActionCheck
	{
		public int parameterID = -1;

		public int constantID;

		public Hotspot hotspot;

		protected Hotspot runtimeHotspot;

		public InteractionType interactionType;

		public int number;

		public ActionInteractionCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Check interaction enabled";
			description = "Checks the enabled state of individual Interactions on a Hotspot.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile(parameters, parameterID, constantID, hotspot);
		}

		public override bool CheckCondition()
		{
			if (runtimeHotspot == null)
			{
				return false;
			}
			if (interactionType == InteractionType.Use)
			{
				if (runtimeHotspot.useButtons.Count > number)
				{
					return !runtimeHotspot.useButtons[number].isDisabled;
				}
				ACDebug.LogWarning("Cannot check Hotspot " + runtimeHotspot.gameObject.name + "'s Use button " + number + " because it doesn't exist!", runtimeHotspot);
			}
			else
			{
				if (interactionType == InteractionType.Examine)
				{
					return !runtimeHotspot.lookButton.isDisabled;
				}
				if (interactionType == InteractionType.Inventory)
				{
					if (runtimeHotspot.invButtons.Count > number)
					{
						return !runtimeHotspot.invButtons[number].isDisabled;
					}
					ACDebug.LogWarning("Cannot check Hotspot " + runtimeHotspot.gameObject.name + "'s Inventory button " + number + " because it doesn't exist!", runtimeHotspot);
				}
			}
			return false;
		}

		public static ActionInteractionCheck CreateNew(Hotspot hotspotToCheck, InteractionType interactionType, int index)
		{
			ActionInteractionCheck actionInteractionCheck = ScriptableObject.CreateInstance<ActionInteractionCheck>();
			actionInteractionCheck.hotspot = hotspotToCheck;
			actionInteractionCheck.interactionType = interactionType;
			actionInteractionCheck.number = index;
			return actionInteractionCheck;
		}
	}
}
