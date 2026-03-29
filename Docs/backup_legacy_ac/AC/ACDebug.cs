using UnityEngine;

namespace AC
{
	public static class ACDebug
	{
		private static string hr = "\n\n -> AC debug logger";

		public static void Log(object message, Object context = null)
		{
			if (CanDisplay(true))
			{
				Debug.Log(string.Concat(message, hr), context);
			}
		}

		public static void LogWarning(object message, Object context = null)
		{
			if (CanDisplay())
			{
				Debug.LogWarning(string.Concat(message, hr), context);
			}
		}

		public static void LogError(object message, Object context = null)
		{
			if (CanDisplay())
			{
				Debug.LogError(string.Concat(message, hr), context);
			}
		}

		public static void Log(object message, ActionList actionList, Action action, Object context = null)
		{
			if (CanDisplay(true))
			{
				if (context == null)
				{
					context = actionList;
				}
				Debug.Log(string.Concat(message, GetActionListSuffix(actionList, action), hr), context);
			}
		}

		public static void LogWarning(object message, ActionList actionList, Action action, Object context = null)
		{
			if (CanDisplay())
			{
				if (context == null)
				{
					context = actionList;
				}
				Debug.LogWarning(string.Concat(message, GetActionListSuffix(actionList, action), hr), context);
			}
		}

		public static void LogError(object message, ActionList actionList, Action action, Object context = null)
		{
			if (CanDisplay())
			{
				if (context == null)
				{
					context = actionList;
				}
				Debug.LogError(string.Concat(message, GetActionListSuffix(actionList, action), hr), context);
			}
		}

		private static string GetActionListSuffix(ActionList actionList, Action action)
		{
			if (actionList != null && actionList.actions.Contains(action))
			{
				return "\n(From Action #" + actionList.actions.IndexOf(action) + " in ActionList '" + actionList.name + "')";
			}
			return string.Empty;
		}

		private static bool CanDisplay(bool isInfo = false)
		{
			if (KickStarter.settingsManager != null)
			{
				switch (KickStarter.settingsManager.showDebugLogs)
				{
				case ShowDebugLogs.Always:
					return true;
				case ShowDebugLogs.Never:
					return false;
				case ShowDebugLogs.OnlyWarningsOrErrors:
					if (!isInfo)
					{
						return true;
					}
					return false;
				}
			}
			return true;
		}
	}
}
