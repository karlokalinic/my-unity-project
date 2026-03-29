using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_action_list.html")]
	public class ActionList : MonoBehaviour
	{
		[HideInInspector]
		public List<Action> actions = new List<Action>();

		[HideInInspector]
		public bool isSkippable = true;

		[HideInInspector]
		public float triggerTime;

		[HideInInspector]
		public bool autosaveAfter;

		[HideInInspector]
		public ActionListType actionListType;

		[HideInInspector]
		public Conversation conversation;

		[HideInInspector]
		public ActionListAsset assetFile;

		[HideInInspector]
		public ActionListSource source;

		[HideInInspector]
		public bool unfreezePauseMenus = true;

		[HideInInspector]
		public bool useParameters;

		[HideInInspector]
		public List<ActionParameter> parameters = new List<ActionParameter>();

		[HideInInspector]
		public int tagID;

		[HideInInspector]
		public bool syncParamValues = true;

		protected bool isSkipping;

		protected LayerMask LayerHotspot;

		protected LayerMask LayerOff;

		protected List<int> resumeIndices = new List<int>();

		private bool pauseWhenActionFinishes;

		private const string parameterSeparator = "{PARAM_SEP}";

		protected bool isChangingScene;

		private int skipIteractions;

		public int NumParameters
		{
			get
			{
				if (useParameters && parameters != null)
				{
					return parameters.Count;
				}
				return 0;
			}
		}

		private void Awake()
		{
			if (KickStarter.settingsManager != null)
			{
				LayerHotspot = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
				LayerOff = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			}
			DownloadParameters();
		}

		private void DownloadParameters()
		{
			if (source != ActionListSource.AssetFile)
			{
				return;
			}
			actions.Clear();
			if (!(assetFile != null) || assetFile.actions.Count <= 0)
			{
				return;
			}
			foreach (Action action in assetFile.actions)
			{
				actions.Add(action);
				actions[actions.Count - 1].isAssetFile = false;
			}
			if (!syncParamValues && useParameters && assetFile.useParameters && parameters.Count == assetFile.DefaultParameters.Count)
			{
				return;
			}
			if (!assetFile.useParameters)
			{
				useParameters = false;
				return;
			}
			if (syncParamValues)
			{
				parameters = assetFile.GetParameters();
				useParameters = true;
				return;
			}
			parameters.Clear();
			foreach (ActionParameter defaultParameter in assetFile.DefaultParameters)
			{
				if (defaultParameter != null)
				{
					ActionParameter item = new ActionParameter(defaultParameter, !useParameters);
					parameters.Add(item);
				}
			}
			useParameters = true;
		}

		public void Initialise()
		{
			actions.Clear();
			if (actions == null || actions.Count < 1)
			{
				actions.Add(GetDefaultAction());
			}
		}

		public void Interact()
		{
			Interact(0, true);
		}

		public void RunFromIndex(int index)
		{
			Interact(index, true);
		}

		public void Interact(int i, bool addToSkipQueue)
		{
			if (!base.gameObject.activeSelf)
			{
				ACDebug.LogWarning("Cannot run ActionList '" + base.name + "' because its GameObject is disabled!", this);
			}
			else if (actions.Count > 0 && actions.Count > i)
			{
				if (triggerTime > 0f && i == 0)
				{
					StartCoroutine("PauseUntilStart", addToSkipQueue);
					return;
				}
				ResetList();
				ResetSkips();
				BeginActionList(i, addToSkipQueue);
			}
			else
			{
				Kill();
			}
		}

		public void Skip()
		{
			Skip(0);
		}

		public void Skip(int i)
		{
			skipIteractions = 0;
			if (actionListType == ActionListType.RunInBackground)
			{
				Interact(i, false);
			}
			else
			{
				if (i < 0 || actions.Count <= i)
				{
					return;
				}
				if (actionListType == ActionListType.RunInBackground || !isSkippable)
				{
					Interact();
				}
				else if (!isSkipping)
				{
					ResetList();
					if (KickStarter.actionListManager.CanResetSkipVars(this))
					{
						ResetSkips();
					}
					isSkipping = true;
					BeginActionList(i, false);
				}
			}
		}

		private IEnumerator PauseUntilStart(bool addToSkipQueue)
		{
			if (triggerTime > 0f)
			{
				yield return new WaitForSeconds(triggerTime);
			}
			ResetList();
			ResetSkips();
			BeginActionList(0, addToSkipQueue);
		}

		private void ResetSkips()
		{
			foreach (Action action in actions)
			{
				if (action != null)
				{
					action.lastResult.skipAction = -10;
				}
			}
		}

		protected virtual void BeginActionList(int i, bool addToSkipQueue)
		{
			pauseWhenActionFinishes = false;
			if ((bool)KickStarter.actionListManager)
			{
				KickStarter.actionListManager.AddToList(this, addToSkipQueue, i);
				KickStarter.eventManager.Call_OnBeginActionList(this, null, i, isSkipping);
				if (KickStarter.actionListManager.IsListRegistered(this))
				{
					ProcessAction(i);
				}
			}
			else
			{
				ACDebug.LogWarning("Cannot run " + base.name + " because no ActionListManager was found.", base.gameObject);
			}
		}

		private IEnumerator DelayProcessAction(int i)
		{
			yield return new WaitForSeconds(0.05f);
			ProcessAction(i);
		}

		protected void ProcessAction(int i)
		{
			if (i >= 0 && i < actions.Count && actions[i] != null && (object)actions[i] != null)
			{
				if (!actions[i].isEnabled)
				{
					ProcessAction(i + 1);
				}
				else
				{
					StartCoroutine("RunAction", actions[i]);
				}
			}
			else
			{
				CheckEndCutscene();
			}
		}

		private IEnumerator RunAction(Action action)
		{
			action.AssignParentList(this);
			if (useParameters)
			{
				action.AssignValues(parameters);
			}
			else
			{
				action.AssignValues(null);
			}
			if (isSkipping)
			{
				skipIteractions++;
				action.Skip();
				if (KickStarter.settingsManager.printActionCommentsInConsole)
				{
					action.PrintComment(this);
				}
			}
			else
			{
				if (action is ActionRunActionList)
				{
					ActionRunActionList actionRunActionList = (ActionRunActionList)action;
					actionRunActionList.isSkippable = IsSkippable();
				}
				if (isChangingScene)
				{
					ACDebug.Log("Cannot run Action while changing scene, will resume once loading is complete.", this, action);
					while (isChangingScene)
					{
						yield return null;
					}
				}
				action.isRunning = false;
				float waitTime = action.Run();
				if (KickStarter.settingsManager.printActionCommentsInConsole)
				{
					action.PrintComment(this);
				}
				if (!(action is ActionParallel) && !Mathf.Approximately(waitTime, 0f))
				{
					while (action.isRunning)
					{
						bool runInRealtime = this is RuntimeActionList && actionListType == ActionListType.PauseGameplay && !unfreezePauseMenus && KickStarter.playerMenus.ArePauseMenusOn();
						if (isChangingScene)
						{
							ACDebug.Log("Cannot continue Action while changing scene, will resume once loading is complete.", this, action);
							while (isChangingScene)
							{
								yield return null;
							}
						}
						if (waitTime < 0f)
						{
							if (!runInRealtime && Time.timeScale <= 0f)
							{
								while (Time.timeScale <= 0f)
								{
									yield return new WaitForEndOfFrame();
								}
							}
							else
							{
								yield return new WaitForEndOfFrame();
							}
						}
						else if (runInRealtime)
						{
							float endTime = Time.realtimeSinceStartup + waitTime;
							while (Time.realtimeSinceStartup < endTime)
							{
								yield return null;
							}
						}
						else
						{
							yield return new WaitForSeconds(waitTime);
						}
						if (!action.isRunning)
						{
							ResetList();
							break;
						}
						waitTime = action.Run();
					}
				}
			}
			if (action is ActionParallel)
			{
				EndActionParallel((ActionParallel)action);
			}
			else
			{
				EndAction(action);
			}
		}

		private void EndAction(Action action)
		{
			action.isRunning = false;
			ActionEnd actionEnd = action.End(actions);
			if (isSkipping && action.lastResult.skipAction != -10 && (action is ActionCheck || action is ActionCheckMultiple))
			{
				actionEnd = new ActionEnd(action.lastResult);
			}
			else
			{
				action.SetLastResult(new ActionEnd(actionEnd));
				ReturnLastResultToSource(actionEnd, actions.IndexOf(action));
			}
			if ((action is ActionCheck || action is ActionCheckMultiple) && actionEnd.resultAction == ResultAction.Skip && actionEnd.skipAction == actions.IndexOf(action))
			{
				ProcessActionEnd(actionEnd, actions.IndexOf(action), true);
			}
			else
			{
				ProcessActionEnd(actionEnd, actions.IndexOf(action));
			}
		}

		private void ProcessActionEnd(ActionEnd actionEnd, int i, bool doStackOverflowDelay = false)
		{
			if (isSkipping && skipIteractions > actions.Count * 3)
			{
				ACDebug.LogWarning("Looping ActionList '" + base.gameObject.name + "' detected while skipping - ending prematurely to avoid a StackOverflow exception.", base.gameObject);
				CheckEndCutscene();
				return;
			}
			if (pauseWhenActionFinishes)
			{
				resumeIndices.Add(i);
				if (!AreActionsRunning())
				{
					FinishPause();
				}
				return;
			}
			if (actionEnd.resultAction == ResultAction.RunCutscene)
			{
				if (actionEnd.linkedAsset != null)
				{
					if (isSkipping)
					{
						AdvGame.SkipActionListAsset(actionEnd.linkedAsset);
					}
					else
					{
						AdvGame.RunActionListAsset(actionEnd.linkedAsset, 0, !IsSkippable());
					}
					CheckEndCutscene();
				}
				else if (actionEnd.linkedCutscene != null)
				{
					if (actionEnd.linkedCutscene != this)
					{
						if (isSkipping)
						{
							actionEnd.linkedCutscene.Skip();
						}
						else
						{
							actionEnd.linkedCutscene.Interact(0, !IsSkippable());
						}
						CheckEndCutscene();
					}
					else if (triggerTime > 0f)
					{
						Kill();
						StartCoroutine("PauseUntilStart", !IsSkippable());
					}
					else
					{
						ProcessAction(0);
					}
				}
				else
				{
					CheckEndCutscene();
				}
			}
			else if (actionEnd.resultAction == ResultAction.Stop)
			{
				CheckEndCutscene();
			}
			else if (actionEnd.resultAction == ResultAction.Skip)
			{
				if (doStackOverflowDelay)
				{
					StartCoroutine(DelayProcessAction(actionEnd.skipAction));
				}
				else
				{
					ProcessAction(actionEnd.skipAction);
				}
			}
			else if (actionEnd.resultAction == ResultAction.Continue)
			{
				ProcessAction(i + 1);
			}
			pauseWhenActionFinishes = false;
		}

		private void EndActionParallel(ActionParallel actionParallel)
		{
			actionParallel.isRunning = false;
			ActionEnd[] array = actionParallel.Ends(actions, isSkipping);
			ActionEnd[] array2 = array;
			foreach (ActionEnd actionEnd in array2)
			{
				ProcessActionEnd(actionEnd, actions.IndexOf(actionParallel));
			}
		}

		private IEnumerator EndCutscene()
		{
			yield return new WaitForEndOfFrame();
			if (!AreActionsRunning())
			{
				Kill();
			}
		}

		protected void CheckEndCutscene()
		{
			if (!AreActionsRunning())
			{
				StartCoroutine("EndCutscene");
			}
		}

		public bool AreActionsRunning()
		{
			for (int i = 0; i < actions.Count; i++)
			{
				if (actions[i] != null && actions[i].isRunning)
				{
					return true;
				}
			}
			return false;
		}

		private void TurnOn()
		{
			base.gameObject.layer = LayerHotspot;
		}

		private void TurnOff()
		{
			base.gameObject.layer = LayerOff;
		}

		public void ResetList()
		{
			isSkipping = false;
			StopAllCoroutines();
			foreach (Action action in actions)
			{
				if (action != null)
				{
					action.Reset(this);
				}
			}
		}

		public virtual void Kill()
		{
			StopAllCoroutines();
			KickStarter.eventManager.Call_OnEndActionList(this, null, isSkipping);
			KickStarter.actionListManager.EndList(this);
		}

		public static Action GetDefaultAction()
		{
			if ((bool)AdvGame.GetReferences().actionsManager)
			{
				string defaultAction = ActionsManager.GetDefaultAction();
				Action action = (Action)ScriptableObject.CreateInstance(defaultAction);
				action.name = defaultAction;
				return action;
			}
			ACDebug.LogError("Cannot create Action - no Actions Manager found.");
			return null;
		}

		protected void ReturnLastResultToSource(ActionEnd _lastResult, int i)
		{
		}

		public bool IsSkippable()
		{
			if (isSkippable && actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}

		public List<Action> GetActions()
		{
			if (source == ActionListSource.AssetFile)
			{
				if ((bool)assetFile)
				{
					return assetFile.actions;
				}
				return null;
			}
			return actions;
		}

		public ActionParameter GetParameter(string label)
		{
			if (useParameters)
			{
				if (source == ActionListSource.InScene)
				{
					return GetParameter(label, parameters);
				}
				if (source == ActionListSource.AssetFile && assetFile != null && assetFile.useParameters)
				{
					if (syncParamValues)
					{
						return GetParameter(label, assetFile.GetParameters());
					}
					return GetParameter(label, parameters);
				}
			}
			return null;
		}

		public ActionParameter GetParameter(int _ID)
		{
			if (useParameters)
			{
				if (source == ActionListSource.InScene)
				{
					return GetParameter(_ID, parameters);
				}
				if (source == ActionListSource.AssetFile && assetFile != null && assetFile.useParameters)
				{
					if (syncParamValues)
					{
						return GetParameter(_ID, assetFile.GetParameters());
					}
					return GetParameter(_ID, parameters);
				}
			}
			return null;
		}

		private ActionParameter GetParameter(int _ID, List<ActionParameter> _parameters)
		{
			if (_parameters != null)
			{
				foreach (ActionParameter _parameter in _parameters)
				{
					if (_parameter.ID == _ID)
					{
						return _parameter;
					}
				}
			}
			return null;
		}

		private ActionParameter GetParameter(string _label, List<ActionParameter> _parameters)
		{
			if (_parameters != null)
			{
				foreach (ActionParameter _parameter in _parameters)
				{
					if (_parameter.label == _label)
					{
						return _parameter;
					}
				}
			}
			return null;
		}

		public void Pause()
		{
			resumeIndices.Clear();
			pauseWhenActionFinishes = true;
			KickStarter.eventManager.Call_OnPauseActionList(this);
		}

		protected virtual void FinishPause()
		{
			KickStarter.actionListManager.AssignResumeIndices(this, resumeIndices.ToArray());
			CheckEndCutscene();
		}

		public void Resume(int _startIndex, int[] _resumeIndices, string _parameterData, bool rerunPreviousAction = false)
		{
			resumeIndices.Clear();
			foreach (int item in _resumeIndices)
			{
				resumeIndices.Add(item);
			}
			if (resumeIndices.Count > 0)
			{
				ResetList();
				ResetSkips();
				SetParameterData(_parameterData);
				pauseWhenActionFinishes = false;
				if (!(KickStarter.actionListManager == null))
				{
					AddResumeToManager(_startIndex);
					KickStarter.eventManager.Call_OnResumeActionList(this);
					{
						foreach (int resumeIndex in resumeIndices)
						{
							if (resumeIndex < 0 || resumeIndex >= actions.Count)
							{
								continue;
							}
							if (rerunPreviousAction)
							{
								ProcessAction(resumeIndex);
								continue;
							}
							Action action = actions[resumeIndex];
							if (useParameters)
							{
								action.AssignValues(parameters);
							}
							else
							{
								action.AssignValues(null);
							}
							if (action is ActionParallel)
							{
								EndActionParallel((ActionParallel)action);
							}
							else
							{
								EndAction(action);
							}
						}
						return;
					}
				}
				ACDebug.LogWarning("Cannot run " + base.name + " because no ActionListManager was found.", base.gameObject);
			}
			else
			{
				Kill();
				Interact();
			}
		}

		protected virtual void AddResumeToManager(int startIndex)
		{
			KickStarter.actionListManager.AddToList(this, true, startIndex);
		}

		public string GetParameterData()
		{
			if (useParameters)
			{
				string text = string.Empty;
				for (int i = 0; i < parameters.Count; i++)
				{
					text += parameters[i].GetSaveData();
					if (i < parameters.Count - 1)
					{
						text += "{PARAM_SEP}";
					}
				}
				return text;
			}
			return string.Empty;
		}

		private void SetParameterData(string dataString)
		{
			if (!useParameters || string.IsNullOrEmpty(dataString))
			{
				return;
			}
			string[] separator = new string[1] { "{PARAM_SEP}" };
			string[] array = dataString.Split(separator, StringSplitOptions.None);
			for (int i = 0; i < parameters.Count; i++)
			{
				if (i < array.Length)
				{
					parameters[i].LoadData(array[i]);
				}
			}
		}
	}
}
