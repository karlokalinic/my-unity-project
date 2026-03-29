using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionContainerCheck : ActionCheck
	{
		public enum IntCondition
		{
			EqualTo = 0,
			NotEqualTo = 1,
			LessThan = 2,
			MoreThan = 3
		}

		public int invParameterID = -1;

		public int invID;

		protected int invNumber;

		public bool useActive;

		public int parameterID = -1;

		public int constantID;

		public Container container;

		protected Container runtimeContainer;

		public bool doCount;

		public int intValue = 1;

		public IntCondition intCondition;

		public ActionContainerCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Container;
			title = "Check";
			description = "Queries the contents of a Container for a stored Item, and reacts accordingly.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeContainer = AssignFile(parameters, parameterID, constantID, container);
			invID = AssignInvItemID(parameters, invParameterID, invID);
			if (useActive)
			{
				runtimeContainer = KickStarter.playerInput.activeContainer;
			}
		}

		public override bool CheckCondition()
		{
			if (runtimeContainer == null)
			{
				return false;
			}
			int count = runtimeContainer.GetCount(invID);
			if (doCount)
			{
				if (intCondition == IntCondition.EqualTo)
				{
					if (count == intValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (count != intValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (count < intValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan && count > intValue)
				{
					return true;
				}
			}
			else if (count > 0)
			{
				return true;
			}
			return false;
		}

		public static ActionContainerCheck CreateNew(Container container, int itemID)
		{
			ActionContainerCheck actionContainerCheck = ScriptableObject.CreateInstance<ActionContainerCheck>();
			actionContainerCheck.container = container;
			actionContainerCheck.invID = itemID;
			return actionContainerCheck;
		}
	}
}
