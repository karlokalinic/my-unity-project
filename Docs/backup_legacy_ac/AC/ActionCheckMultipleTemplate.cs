using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class ActionCheckMultipleTemplate : ActionCheckMultiple
	{
		public ActionCheckMultipleTemplate()
		{
			isDisplayed = true;
			category = ActionCategory.Custom;
			title = "Check multiple template";
			description = "This is a blank 'Check multiple' Action template.";
		}

		public override ActionEnd End(List<Action> actions)
		{
			int i = 0;
			return ProcessResult(i, actions);
		}
	}
}
