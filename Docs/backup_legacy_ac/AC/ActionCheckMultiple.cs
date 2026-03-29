using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionCheckMultiple : Action
	{
		public List<ActionEnd> endings = new List<ActionEnd>();

		public ActionCheckMultiple()
		{
			numSockets = 2;
		}

		public override ActionEnd End(List<Action> actions)
		{
			return ProcessResult(-1, actions);
		}

		protected ActionEnd ProcessResult(int i, List<Action> actions)
		{
			if (i >= 0 && i < endings.Count && endings[i] != null)
			{
				if (endings[i].resultAction == ResultAction.Skip)
				{
					int num = endings[i].skipAction;
					if ((bool)endings[i].skipActionActual && actions.Contains(endings[i].skipActionActual))
					{
						num = actions.IndexOf(endings[i].skipActionActual);
					}
					else if (num == -1)
					{
						num = 0;
					}
					endings[i].skipAction = num;
				}
				return endings[i];
			}
			ACDebug.LogWarning("Attempting to follow socket " + i + ", but it doesn't exist! (" + category.ToString() + ": " + title + ")", this);
			return GenerateStopActionEnd();
		}

		public void SetOutputs(ActionEnd[] actionEnds)
		{
			endings = new List<ActionEnd>();
			foreach (ActionEnd actionEnd in actionEnds)
			{
				endings.Add(new ActionEnd(actionEnd));
			}
		}
	}
}
