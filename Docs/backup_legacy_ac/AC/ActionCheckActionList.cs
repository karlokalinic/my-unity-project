using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCheckActionList : ActionCheck
	{
		public enum ListSource
		{
			InScene = 0,
			AssetFile = 1
		}

		public ListSource listSource;

		public bool checkSelfSkipping;

		public ActionList actionList;

		protected ActionList runtimeActionList;

		public ActionListAsset actionListAsset;

		public int constantID;

		public int parameterID = -1;

		protected bool isSkipping;

		public ActionCheckActionList()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Check running";
			description = "Queries whether or not a supplied ActionList is currently running. By looping the If condition is not met field back onto itself, this will effectively “wait” until the supplied ActionList has completed before continuing.";
		}

		public override float Run()
		{
			isSkipping = false;
			return 0f;
		}

		public override void Skip()
		{
			isSkipping = true;
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				runtimeActionList = AssignFile(parameters, parameterID, constantID, actionList);
			}
		}

		public override bool CheckCondition()
		{
			if (checkSelfSkipping)
			{
				return isSkipping;
			}
			if (isSkipping && IsTargetSkippable())
			{
				return false;
			}
			if (listSource == ListSource.InScene && runtimeActionList != null)
			{
				return KickStarter.actionListManager.IsListRunning(runtimeActionList);
			}
			if (listSource == ListSource.AssetFile && actionListAsset != null)
			{
				return KickStarter.actionListAssetManager.IsListRunning(actionListAsset);
			}
			return false;
		}

		public override void SetLastResult(ActionEnd _actionEnd)
		{
			if (!IsTargetSkippable() && !checkSelfSkipping)
			{
				base.SetLastResult(_actionEnd);
			}
			else
			{
				lastResult = new ActionEnd(-10);
			}
		}

		protected bool IsTargetSkippable()
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				return actionList.IsSkippable();
			}
			if (listSource == ListSource.AssetFile && actionListAsset != null)
			{
				return actionListAsset.IsSkippable();
			}
			return false;
		}

		public static ActionCheckActionList CreateNew_CheckSelfIsSkipping()
		{
			ActionCheckActionList actionCheckActionList = ScriptableObject.CreateInstance<ActionCheckActionList>();
			actionCheckActionList.checkSelfSkipping = true;
			return actionCheckActionList;
		}

		public static ActionCheckActionList CreateNew_CheckOther(ActionList actionList)
		{
			ActionCheckActionList actionCheckActionList = ScriptableObject.CreateInstance<ActionCheckActionList>();
			actionCheckActionList.listSource = ListSource.InScene;
			actionCheckActionList.actionList = actionList;
			return actionCheckActionList;
		}

		public static ActionCheckActionList CreateNew_CheckOther(ActionListAsset actionListAsset)
		{
			ActionCheckActionList actionCheckActionList = ScriptableObject.CreateInstance<ActionCheckActionList>();
			actionCheckActionList.listSource = ListSource.AssetFile;
			actionCheckActionList.actionListAsset = actionListAsset;
			return actionCheckActionList;
		}
	}
}
