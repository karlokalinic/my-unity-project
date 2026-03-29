using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_action_list.html")]
	public class RuntimeActionList : ActionList
	{
		public ActionListAsset assetSource;

		protected void OnEnable()
		{
			EventManager.OnBeforeChangeScene += OnBeforeChangeScene;
			EventManager.OnAfterChangeScene += OnAfterChangeScene;
		}

		protected void OnDisable()
		{
			EventManager.OnBeforeChangeScene -= OnBeforeChangeScene;
			EventManager.OnAfterChangeScene -= OnAfterChangeScene;
		}

		public void DownloadActions(ActionListAsset actionListAsset, Conversation endConversation, int i, bool doSkip, bool addToSkipQueue, bool dontRun = false)
		{
			assetSource = actionListAsset;
			useParameters = actionListAsset.useParameters;
			parameters = new List<ActionParameter>();
			List<ActionParameter> list = actionListAsset.GetParameters();
			if (list != null)
			{
				foreach (ActionParameter item in list)
				{
					parameters.Add(new ActionParameter(item, true));
				}
			}
			unfreezePauseMenus = actionListAsset.unfreezePauseMenus;
			actionListType = actionListAsset.actionListType;
			if (actionListAsset.actionListType == ActionListType.PauseGameplay)
			{
				isSkippable = actionListAsset.isSkippable;
			}
			else
			{
				isSkippable = false;
			}
			conversation = endConversation;
			actions.Clear();
			foreach (Action action2 in actionListAsset.actions)
			{
				ActionEnd lastResult = action2.lastResult;
				if (action2 != null)
				{
					Action action = ((!actionListAsset.canRunMultipleInstances) ? action2 : Object.Instantiate(action2));
					if (doSkip)
					{
						action.lastResult = lastResult;
					}
					actions.Add(action);
				}
				else
				{
					actions.Add(null);
				}
			}
			actionListAsset.AfterDownloading();
			if (!dontRun)
			{
				if (doSkip)
				{
					Skip(i);
				}
				else
				{
					Interact(i, addToSkipQueue);
				}
			}
			if (actionListAsset.canSurviveSceneChanges && !actionListAsset.IsSkippable())
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		public override void Kill()
		{
			StopAllCoroutines();
			KickStarter.eventManager.Call_OnEndActionList(this, assetSource, isSkipping);
			KickStarter.actionListAssetManager.EndAssetList(this);
		}

		public void DestroySelf()
		{
			Object.Destroy(base.gameObject);
		}

		protected override void BeginActionList(int i, bool addToSkipQueue)
		{
			if (KickStarter.actionListAssetManager != null)
			{
				KickStarter.actionListAssetManager.AddToList(this, assetSource, addToSkipQueue, i, isSkipping);
				KickStarter.eventManager.Call_OnBeginActionList(this, assetSource, i, isSkipping);
				if (KickStarter.actionListManager.IsListRegistered(this))
				{
					ProcessAction(i);
				}
			}
			else
			{
				ACDebug.LogWarning("Cannot run " + base.name + " because no ActionListManager was found.", this);
			}
		}

		protected override void AddResumeToManager(int startIndex)
		{
			if (KickStarter.actionListAssetManager == null)
			{
				ACDebug.LogWarning("Cannot run " + base.name + " because no ActionListAssetManager was found.", this);
			}
			else
			{
				KickStarter.actionListAssetManager.AddToList(this, assetSource, true, startIndex);
			}
		}

		protected new void ReturnLastResultToSource(ActionEnd _lastResult, int i)
		{
			assetSource.actions[i].lastResult = _lastResult;
		}

		protected override void FinishPause()
		{
			KickStarter.actionListAssetManager.AssignResumeIndices(assetSource, resumeIndices.ToArray());
			CheckEndCutscene();
		}

		protected void OnBeforeChangeScene()
		{
			if (assetSource.canSurviveSceneChanges && !assetSource.IsSkippable())
			{
				isChangingScene = true;
			}
		}

		protected void OnAfterChangeScene(LoadingGame loadingGame)
		{
			isChangingScene = false;
		}
	}
}
