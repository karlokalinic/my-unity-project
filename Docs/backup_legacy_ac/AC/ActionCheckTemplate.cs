using System;

namespace AC
{
	[Serializable]
	public class ActionCheckTemplate : ActionCheck
	{
		public ActionCheckTemplate()
		{
			isDisplayed = true;
			category = ActionCategory.Custom;
			title = "Check template";
			description = "This is a blank 'Check' Action template.";
		}

		public override bool CheckCondition()
		{
			return false;
		}
	}
}
