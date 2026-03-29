using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionParallel : Action
	{
		public List<ActionEnd> endings = new List<ActionEnd>();

		public ActionParallel()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Run in parallel";
			description = "Runs any subsequent Actions (whether in the same list or in a new one) simultaneously. This is useful when making complex cutscenes that require timing to be exact.";
			if (numSockets == 1)
			{
				numSockets = 2;
			}
		}

		public ActionEnd[] Ends(List<Action> actions, bool doSkip)
		{
			foreach (ActionEnd ending in endings)
			{
				if (ending.resultAction == ResultAction.Skip)
				{
					int num = ending.skipAction;
					if ((bool)skipActionActual && actions.Contains(ending.skipActionActual))
					{
						num = actions.IndexOf(ending.skipActionActual);
					}
					else if (num == -1)
					{
						num = 0;
					}
					ending.skipAction = num;
				}
			}
			return endings.ToArray();
		}

		public static ActionParallel CreateNew(ActionEnd[] actionEnds)
		{
			ActionParallel actionParallel = ScriptableObject.CreateInstance<ActionParallel>();
			actionParallel.endings = new List<ActionEnd>();
			foreach (ActionEnd actionEnd in actionEnds)
			{
				actionParallel.endings.Add(new ActionEnd(actionEnd));
			}
			return actionParallel;
		}
	}
}
