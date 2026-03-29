using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionStopActionList : Action
	{
		public enum ListSource
		{
			InScene = 0,
			AssetFile = 1
		}

		public ListSource listSource;

		public ActionList actionList;

		protected ActionList runtimeActionList;

		public ActionListAsset invActionList;

		public int constantID;

		public int parameterID = -1;

		public ActionStopActionList()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Kill";
			description = "Instantly stops a scene or asset-based ActionList from running.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				runtimeActionList = AssignFile(parameters, parameterID, constantID, actionList);
			}
		}

		public override float Run()
		{
			if (listSource == ListSource.InScene && runtimeActionList != null)
			{
				KickStarter.actionListManager.EndList(runtimeActionList);
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				KickStarter.actionListAssetManager.EndAssetList(invActionList, this);
			}
			return 0f;
		}

		public static ActionStopActionList CreateNew(ActionList actionList)
		{
			ActionStopActionList actionStopActionList = ScriptableObject.CreateInstance<ActionStopActionList>();
			actionStopActionList.listSource = ListSource.InScene;
			actionStopActionList.actionList = actionList;
			return actionStopActionList;
		}

		public static ActionStopActionList CreateNew(ActionListAsset actionListAsset)
		{
			ActionStopActionList actionStopActionList = ScriptableObject.CreateInstance<ActionStopActionList>();
			actionStopActionList.listSource = ListSource.AssetFile;
			actionStopActionList.invActionList = actionListAsset;
			return actionStopActionList;
		}
	}
}
