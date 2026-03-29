using System;

namespace AC
{
	[Serializable]
	public class ActionTemplate : Action
	{
		public ActionTemplate()
		{
			isDisplayed = true;
			category = ActionCategory.Custom;
			title = "Template";
			description = "This is a blank Action template.";
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				return base.defaultPauseTime;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			Run();
		}
	}
}
