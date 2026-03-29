using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionRunActionList : Action
	{
		public enum ListSource
		{
			InScene = 0,
			AssetFile = 1
		}

		public ListSource listSource;

		public ActionList actionList;

		public int constantID;

		public int parameterID = -1;

		public ActionListAsset invActionList;

		public int assetParameterID = -1;

		public bool runFromStart = true;

		public int jumpToAction;

		public int jumpToActionParameterID = -1;

		public Action jumpToActionActual;

		public bool runInParallel;

		public bool isSkippable;

		public List<ActionParameter> localParameters = new List<ActionParameter>();

		public List<int> parameterIDs = new List<int>();

		public bool setParameters;

		protected RuntimeActionList runtimeActionList;

		[SerializeField]
		private bool hasUpgradedAgain;

		public ActionRunActionList()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Run";
			description = "Runs any ActionList (either scene-based like Cutscenes, Triggers and Interactions, or ActionList assets). If the new ActionList takes parameters, this Action can be used to set them.";
		}

		protected void Upgrade()
		{
			if (!runInParallel)
			{
				numSockets = 1;
				runInParallel = true;
				endAction = ResultAction.Stop;
			}
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				actionList = AssignFile(parameters, parameterID, constantID, actionList);
				jumpToAction = AssignInteger(parameters, jumpToActionParameterID, jumpToAction);
			}
			else if (listSource == ListSource.AssetFile)
			{
				invActionList = (ActionListAsset)AssignObject<ActionListAsset>(parameters, assetParameterID, invActionList);
			}
			if (localParameters == null || localParameters.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < localParameters.Count; i++)
			{
				if (parameterIDs == null || parameterIDs.Count <= i || parameterIDs[i] < 0)
				{
					continue;
				}
				int num = parameterIDs[i];
				foreach (ActionParameter parameter in parameters)
				{
					if (parameter.ID == num)
					{
						localParameters[i].CopyValues(parameter);
						break;
					}
				}
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				Upgrade();
				isRunning = true;
				runtimeActionList = null;
				if (listSource == ListSource.InScene && actionList != null && !actionList.actions.Contains(this))
				{
					KickStarter.actionListManager.EndList(actionList);
					if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
					{
						if (actionList.syncParamValues)
						{
							SendParameters(actionList.assetFile.GetParameters(), true);
						}
						else
						{
							SendParameters(actionList.parameters, false);
						}
					}
					else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
					{
						SendParameters(actionList.parameters, false);
					}
					if (runFromStart)
					{
						actionList.Interact(0, !isSkippable);
					}
					else
					{
						actionList.Interact(GetSkipIndex(actionList.actions), !isSkippable);
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null && !invActionList.actions.Contains(this))
				{
					if (!invActionList.canRunMultipleInstances)
					{
						KickStarter.actionListAssetManager.EndAssetList(invActionList);
					}
					if (invActionList.useParameters)
					{
						SendParameters(invActionList.GetParameters(), true);
					}
					if (runFromStart)
					{
						runtimeActionList = AdvGame.RunActionListAsset(invActionList, 0, !isSkippable);
					}
					else
					{
						runtimeActionList = AdvGame.RunActionListAsset(invActionList, GetSkipIndex(invActionList.actions), !isSkippable);
					}
				}
				if (!runInParallel || (runInParallel && willWait))
				{
					return base.defaultPauseTime;
				}
			}
			else if (listSource == ListSource.InScene && actionList != null)
			{
				if (KickStarter.actionListManager.IsListRunning(actionList))
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				if (invActionList.canRunMultipleInstances)
				{
					if (runtimeActionList != null && KickStarter.actionListManager.IsListRunning(runtimeActionList))
					{
						return base.defaultPauseTime;
					}
					isRunning = false;
				}
				else
				{
					if (KickStarter.actionListAssetManager.IsListRunning(invActionList))
					{
						return base.defaultPauseTime;
					}
					isRunning = false;
				}
			}
			return 0f;
		}

		public override void Skip()
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
				{
					if (actionList.syncParamValues)
					{
						SendParameters(actionList.assetFile.GetParameters(), true);
					}
					else
					{
						SendParameters(actionList.parameters, false);
					}
				}
				else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
				{
					SendParameters(actionList.parameters, false);
				}
				if (runFromStart)
				{
					actionList.Skip();
				}
				else
				{
					actionList.Skip(GetSkipIndex(actionList.actions));
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				if (invActionList.useParameters)
				{
					SendParameters(invActionList.GetParameters(), true);
				}
				if (runtimeActionList != null && !invActionList.IsSkippable() && invActionList.canRunMultipleInstances)
				{
					KickStarter.actionListAssetManager.EndAssetList(runtimeActionList);
				}
				if (runFromStart)
				{
					AdvGame.SkipActionListAsset(invActionList);
				}
				else
				{
					AdvGame.SkipActionListAsset(invActionList, GetSkipIndex(invActionList.actions));
				}
			}
		}

		protected int GetSkipIndex(List<Action> _actions)
		{
			int result = jumpToAction;
			if ((bool)jumpToActionActual && _actions.IndexOf(jumpToActionActual) > 0)
			{
				result = _actions.IndexOf(jumpToActionActual);
			}
			return result;
		}

		protected void SendParameters(List<ActionParameter> externalParameters, bool sendingToAsset)
		{
			if (setParameters)
			{
				SyncLists(externalParameters, localParameters);
				SetParametersBase.BulkAssignParameterValues(externalParameters, localParameters, sendingToAsset, isAssetFile);
			}
		}

		protected void SyncLists(List<ActionParameter> externalParameters, List<ActionParameter> oldLocalParameters)
		{
			if (!hasUpgradedAgain)
			{
				if (oldLocalParameters != null && externalParameters != null && oldLocalParameters.Count != externalParameters.Count && oldLocalParameters.Count > 0)
				{
					ACDebug.LogWarning("Parameter mismatch detected - please check the 'ActionList: Run' Action for its parameter values.");
				}
				for (int i = 0; i < externalParameters.Count; i++)
				{
					if (i < oldLocalParameters.Count)
					{
						oldLocalParameters[i].ID = externalParameters[i].ID;
					}
				}
				hasUpgradedAgain = true;
			}
			SetParametersBase.GUIData gUIData = SetParametersBase.SyncLists(externalParameters, new SetParametersBase.GUIData(oldLocalParameters, parameterIDs));
			localParameters = gUIData.fromParameters;
			parameterIDs = gUIData.parameterIDs;
		}

		public static ActionRunActionList CreateNew(ActionList actionList, int startingActionIndex = 0)
		{
			ActionRunActionList actionRunActionList = ScriptableObject.CreateInstance<ActionRunActionList>();
			actionRunActionList.listSource = ListSource.InScene;
			actionRunActionList.actionList = actionList;
			actionRunActionList.runFromStart = startingActionIndex <= 0;
			actionRunActionList.jumpToAction = startingActionIndex;
			return actionRunActionList;
		}

		public static ActionRunActionList CreateNew(ActionListAsset actionListAsset, int startingActionIndex = 0)
		{
			ActionRunActionList actionRunActionList = ScriptableObject.CreateInstance<ActionRunActionList>();
			actionRunActionList.listSource = ListSource.AssetFile;
			actionRunActionList.invActionList = actionListAsset;
			actionRunActionList.runFromStart = startingActionIndex <= 0;
			actionRunActionList.jumpToAction = startingActionIndex;
			return actionRunActionList;
		}
	}
}
