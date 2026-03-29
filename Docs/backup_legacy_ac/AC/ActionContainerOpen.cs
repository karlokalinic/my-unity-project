using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionContainerOpen : Action
	{
		public bool useActive;

		public int parameterID = -1;

		public int constantID;

		public Container container;

		protected Container runtimeContainer;

		public ActionContainerOpen()
		{
			isDisplayed = true;
			category = ActionCategory.Container;
			title = "Open";
			description = "Opens a chosen Container, causing any Menu of Appear type: On Container to open. To close the Container, simply close the Menu.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
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
			if (runtimeContainer != null)
			{
				runtimeContainer.Interact();
			}
			return 0f;
		}

		public static ActionContainerOpen CreateNew(Container containerToOpen)
		{
			ActionContainerOpen actionContainerOpen = ScriptableObject.CreateInstance<ActionContainerOpen>();
			actionContainerOpen.container = containerToOpen;
			return actionContainerOpen;
		}
	}
}
