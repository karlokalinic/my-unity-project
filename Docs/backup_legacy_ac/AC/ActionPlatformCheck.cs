using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPlatformCheck : ActionCheck
	{
		public PlatformType platformType;

		public ActionPlatformCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Check platform";
			description = "Queries either the plaform the game is running on.";
		}

		public override bool CheckCondition()
		{
			switch (platformType)
			{
			case PlatformType.Desktop:
				return true;
			case PlatformType.TouchScreen:
				return false;
			case PlatformType.WebPlayer:
				return false;
			case PlatformType.Windows:
				return true;
			case PlatformType.Mac:
				return false;
			case PlatformType.Linux:
				return false;
			case PlatformType.iOS:
				return false;
			case PlatformType.Android:
				return false;
			default:
				return false;
			}
		}

		public static ActionPlatformCheck CreateNew(PlatformType platformToCheck)
		{
			ActionPlatformCheck actionPlatformCheck = ScriptableObject.CreateInstance<ActionPlatformCheck>();
			actionPlatformCheck.platformType = platformToCheck;
			return actionPlatformCheck;
		}
	}
}
