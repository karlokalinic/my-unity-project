using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPauseActionList : Action
	{
		public enum PauseResume
		{
			Pause = 0,
			Resume = 1
		}

		public PauseResume pauseResume;

		public ActionRunActionList.ListSource listSource;

		public ActionListAsset actionListAsset;

		public bool rerunPausedActions;

		public ActionList actionList;

		protected ActionList _runtimeActionList;

		public int constantID;

		public int parameterID = -1;

		protected RuntimeActionList[] runtimeActionLists = new RuntimeActionList[0];

		public ActionPauseActionList()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Pause or resume";
			description = "Pauses and resumes ActionLists.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				_runtimeActionList = AssignFile(parameters, parameterID, constantID, actionList);
			}
			else
			{
				actionListAsset = (ActionListAsset)AssignObject<ActionListAsset>(parameters, parameterID, actionListAsset);
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				runtimeActionLists = new RuntimeActionList[0];
				if (pauseResume == PauseResume.Pause)
				{
					if (listSource == ActionRunActionList.ListSource.AssetFile && actionListAsset != null)
					{
						if (actionListAsset.actions.Contains(this))
						{
							LogWarning("An ActionList Asset cannot Pause itself - it must be performed indirectly.");
						}
						else
						{
							runtimeActionLists = KickStarter.actionListAssetManager.Pause(actionListAsset);
							if (willWait && runtimeActionLists.Length > 0)
							{
								return base.defaultPauseTime;
							}
						}
					}
					else if (listSource == ActionRunActionList.ListSource.InScene && _runtimeActionList != null)
					{
						if (_runtimeActionList.actions.Contains(this))
						{
							LogWarning("An ActionList cannot Pause itself - it must be performed indirectly.");
						}
						else
						{
							_runtimeActionList.Pause();
							if (willWait)
							{
								return base.defaultPauseTime;
							}
						}
					}
				}
				else if (pauseResume == PauseResume.Resume)
				{
					if (listSource == ActionRunActionList.ListSource.AssetFile && actionListAsset != null && !actionListAsset.actions.Contains(this))
					{
						KickStarter.actionListAssetManager.Resume(actionListAsset, rerunPausedActions);
					}
					else if (listSource == ActionRunActionList.ListSource.InScene && _runtimeActionList != null && !_runtimeActionList.actions.Contains(this))
					{
						KickStarter.actionListManager.Resume(_runtimeActionList, rerunPausedActions);
					}
				}
				return 0f;
			}
			if (listSource == ActionRunActionList.ListSource.AssetFile)
			{
				RuntimeActionList[] array = runtimeActionLists;
				foreach (RuntimeActionList runtimeActionList in array)
				{
					if (runtimeActionList != null && KickStarter.actionListManager.IsListRunning(runtimeActionList))
					{
						return base.defaultPauseTime;
					}
				}
			}
			else if (listSource == ActionRunActionList.ListSource.InScene && KickStarter.actionListManager.IsListRunning(_runtimeActionList))
			{
				return base.defaultPauseTime;
			}
			isRunning = false;
			return 0f;
		}

		public static ActionPauseActionList CreateNew_Pause(ActionList actionList, bool waitUntilFinish = false)
		{
			ActionPauseActionList actionPauseActionList = ScriptableObject.CreateInstance<ActionPauseActionList>();
			actionPauseActionList.pauseResume = PauseResume.Pause;
			actionPauseActionList.listSource = ActionRunActionList.ListSource.InScene;
			actionPauseActionList.actionList = actionList;
			actionPauseActionList.willWait = waitUntilFinish;
			return actionPauseActionList;
		}

		public static ActionPauseActionList CreateNew_Pause(ActionListAsset actionListAsset, bool waitUntilFinish = false)
		{
			ActionPauseActionList actionPauseActionList = ScriptableObject.CreateInstance<ActionPauseActionList>();
			actionPauseActionList.pauseResume = PauseResume.Pause;
			actionPauseActionList.listSource = ActionRunActionList.ListSource.AssetFile;
			actionPauseActionList.actionListAsset = actionListAsset;
			actionPauseActionList.willWait = waitUntilFinish;
			return actionPauseActionList;
		}

		public static ActionPauseActionList CreateNew_Resume(ActionList actionList, bool rerunLastAction = false)
		{
			ActionPauseActionList actionPauseActionList = ScriptableObject.CreateInstance<ActionPauseActionList>();
			actionPauseActionList.pauseResume = PauseResume.Resume;
			actionPauseActionList.listSource = ActionRunActionList.ListSource.InScene;
			actionPauseActionList.actionList = actionList;
			actionPauseActionList.rerunPausedActions = rerunLastAction;
			return actionPauseActionList;
		}

		public static ActionPauseActionList CreateNew_Resume(ActionListAsset actionListAsset, bool rerunLastAction = false)
		{
			ActionPauseActionList actionPauseActionList = ScriptableObject.CreateInstance<ActionPauseActionList>();
			actionPauseActionList.pauseResume = PauseResume.Resume;
			actionPauseActionList.listSource = ActionRunActionList.ListSource.AssetFile;
			actionPauseActionList.actionListAsset = actionListAsset;
			actionPauseActionList.rerunPausedActions = rerunLastAction;
			return actionPauseActionList;
		}
	}
}
