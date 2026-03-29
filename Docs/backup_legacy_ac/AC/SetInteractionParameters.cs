using System;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Hotspots/Set Interaction parameters")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_set_interaction_parameters.html")]
	public class SetInteractionParameters : SetParametersBase
	{
		protected enum InteractionType
		{
			Use = 0,
			Examine = 1,
			Inventory = 2,
			UnhandledInventory = 3
		}

		[SerializeField]
		protected Hotspot hotspot;

		[SerializeField]
		protected InteractionType interactionType;

		[SerializeField]
		protected int buttonIndex;

		protected void OnEnable()
		{
			EventManager.OnHotspotInteract = (EventManager.Delegate_InteractHotspot)Delegate.Combine(EventManager.OnHotspotInteract, new EventManager.Delegate_InteractHotspot(OnHotspotInteract));
		}

		protected void OnDisable()
		{
			EventManager.OnHotspotInteract = (EventManager.Delegate_InteractHotspot)Delegate.Remove(EventManager.OnHotspotInteract, new EventManager.Delegate_InteractHotspot(OnHotspotInteract));
		}

		protected void OnHotspotInteract(Hotspot hotspot, Button button)
		{
			if (!(this.hotspot == hotspot) || button == null)
			{
				return;
			}
			switch (interactionType)
			{
			case InteractionType.Use:
				if (hotspot.provideUseInteraction && hotspot.useButtons.Contains(button) && hotspot.useButtons.IndexOf(button) == buttonIndex)
				{
					ProcessButton(button);
				}
				break;
			case InteractionType.Examine:
				if (hotspot.provideLookInteraction && hotspot.lookButton == button)
				{
					ProcessButton(button);
				}
				break;
			case InteractionType.Inventory:
				if (hotspot.provideInvInteraction && hotspot.invButtons.Contains(button) && hotspot.invButtons.IndexOf(button) == buttonIndex)
				{
					ProcessButton(button);
				}
				break;
			case InteractionType.UnhandledInventory:
				if (hotspot.provideUnhandledInvInteraction && hotspot.unhandledInvButton == button)
				{
					ProcessButton(button);
				}
				break;
			}
		}

		protected void ProcessButton(Button button)
		{
			if (hotspot.interactionSource == InteractionSource.AssetFile)
			{
				AssignParameterValues(button.assetFile);
			}
			else if (hotspot.interactionSource == InteractionSource.InScene)
			{
				AssignParameterValues(button.interaction);
			}
		}
	}
}
