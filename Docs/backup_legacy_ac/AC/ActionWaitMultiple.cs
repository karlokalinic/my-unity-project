using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionWaitMultiple : Action
	{
		protected int triggersToWait;

		public ActionWaitMultiple()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Wait for preceding";
			description = "This Action will only trigger its 'After running' command once all Actions that can run it have been run.";
		}

		public override float Run()
		{
			triggersToWait--;
			return 0f;
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (triggersToWait > 0)
			{
				return GenerateStopActionEnd();
			}
			triggersToWait = 100;
			return base.End(actions);
		}

		public override void Reset(ActionList actionList)
		{
			triggersToWait = 0;
			int num = actionList.actions.IndexOf(this);
			if (num == 0)
			{
				ACDebug.LogWarning("The Action '" + category.ToString() + ": " + title + "' should not be first in an ActionList, as it will prevent others from running!");
				return;
			}
			for (int i = 0; i < actionList.actions.Count; i++)
			{
				Action action = actionList.actions[i];
				if (!(action != this))
				{
					continue;
				}
				if (action is ActionCheck)
				{
					ActionCheck actionCheck = (ActionCheck)action;
					if ((actionCheck.resultActionFail == ResultAction.Skip && actionCheck.skipActionFail == num) || (actionCheck.resultActionFail == ResultAction.Continue && num == i + 1))
					{
						triggersToWait++;
					}
					else if ((actionCheck.resultActionTrue == ResultAction.Skip && actionCheck.skipActionTrue == num) || (actionCheck.resultActionTrue == ResultAction.Continue && num == i + 1))
					{
						triggersToWait++;
					}
				}
				else if (action is ActionCheckMultiple)
				{
					ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple)action;
					foreach (ActionEnd ending in actionCheckMultiple.endings)
					{
						if ((ending.resultAction == ResultAction.Skip && ending.skipAction == num) || (ending.resultAction == ResultAction.Continue && num == i + 1))
						{
							triggersToWait++;
							break;
						}
					}
				}
				else if (action is ActionParallel)
				{
					ActionParallel actionParallel = (ActionParallel)action;
					foreach (ActionEnd ending2 in actionParallel.endings)
					{
						if ((ending2.resultAction == ResultAction.Skip && ending2.skipAction == num) || (ending2.resultAction == ResultAction.Continue && num == i + 1))
						{
							triggersToWait++;
							break;
						}
					}
				}
				else if ((action.endAction == ResultAction.Skip && action.skipAction == num) || (action.endAction == ResultAction.Continue && num == i + 1))
				{
					triggersToWait++;
				}
			}
			base.Reset(actionList);
		}
	}
}
