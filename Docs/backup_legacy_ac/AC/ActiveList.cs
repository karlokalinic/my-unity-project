using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	public class ActiveList
	{
		public ActionList actionList;

		public ActionListAsset actionListAsset;

		public int startIndex;

		public bool inSkipQueue;

		private bool isRunning;

		private bool isConversationOverride;

		private int[] resumeIndices;

		private Conversation conversationOnEnd;

		private string parameterData;

		public ActiveList()
		{
			actionList = null;
			actionListAsset = null;
			conversationOnEnd = null;
			inSkipQueue = false;
			isRunning = false;
			resumeIndices = new int[0];
			parameterData = string.Empty;
		}

		public ActiveList(ActionList _actionList, bool _inSkipQueue, int _startIndex)
		{
			actionList = _actionList;
			if ((bool)actionList.conversation)
			{
				conversationOnEnd = actionList.conversation;
			}
			RuntimeActionList runtimeActionList = actionList as RuntimeActionList;
			if (runtimeActionList != null)
			{
				actionListAsset = runtimeActionList.assetSource;
			}
			else
			{
				actionListAsset = null;
			}
			inSkipQueue = _inSkipQueue;
			startIndex = _startIndex;
			isRunning = true;
			resumeIndices = new int[0];
			parameterData = string.Empty;
		}

		public bool IsRunning()
		{
			if (actionList != null)
			{
				return isRunning;
			}
			return false;
		}

		public bool IsConversationOverride()
		{
			return isConversationOverride;
		}

		public bool IsNecessary()
		{
			if (IsRunning() || isConversationOverride || inSkipQueue || resumeIndices.Length > 0)
			{
				return true;
			}
			return false;
		}

		public void ClearNecessity()
		{
			resumeIndices = new int[0];
			isConversationOverride = false;
		}

		public void Reset(bool removeFromSkipQueue)
		{
			isRunning = false;
			if (actionList != null)
			{
				actionList.ResetList();
				RuntimeActionList runtimeActionList = actionList as RuntimeActionList;
				if (runtimeActionList != null)
				{
					runtimeActionList.DestroySelf();
				}
			}
			if (removeFromSkipQueue)
			{
				inSkipQueue = false;
			}
		}

		public bool CanResetSkipVars()
		{
			if (isRunning || inSkipQueue)
			{
				return false;
			}
			return true;
		}

		public void ShowGUI()
		{
			if (actionList != null && IsRunning() && !GUILayout.Button(actionList.gameObject.name))
			{
			}
		}

		public bool IsFor(ActionList _actionList)
		{
			if (_actionList != null && actionList == _actionList)
			{
				return true;
			}
			return false;
		}

		public bool IsFor(ActionListAsset _actionListAsset)
		{
			if (_actionListAsset != null && actionListAsset == _actionListAsset)
			{
				return true;
			}
			return false;
		}

		public void Skip()
		{
			if (inSkipQueue)
			{
				if (actionListAsset != null)
				{
					bool flag = isRunning;
					bool flag2 = inSkipQueue;
					KickStarter.actionListAssetManager.DestroyAssetList(actionListAsset);
					isRunning = flag;
					inSkipQueue = flag2;
					actionList = AdvGame.SkipActionListAsset(actionListAsset, startIndex, conversationOnEnd);
				}
				else if (actionList != null)
				{
					actionList.Skip(startIndex);
				}
			}
		}

		public void UpdateParameterData()
		{
			parameterData = actionList.GetParameterData();
		}

		public bool CanUnfreezePauseMenus()
		{
			if (actionListAsset != null && actionListAsset.unfreezePauseMenus && actionListAsset.actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}

		public void SetConversationOverride(ActionConversation actionConversation)
		{
			if (!(actionList != null))
			{
				return;
			}
			foreach (Action action in actionList.actions)
			{
				if (action == actionConversation)
				{
					startIndex = actionList.actions.IndexOf(action);
					isConversationOverride = true;
					Reset(true);
					break;
				}
			}
		}

		public bool ResumeConversationOverride()
		{
			if (isConversationOverride)
			{
				isConversationOverride = false;
				if (actionListAsset != null)
				{
					actionList = AdvGame.RunActionListAsset(actionListAsset, startIndex, true);
				}
				else if (actionList != null)
				{
					actionList.Interact(startIndex, true);
				}
				return true;
			}
			return false;
		}

		public Conversation GetConversationOnEnd()
		{
			if (conversationOnEnd != null)
			{
				if ((bool)KickStarter.stateHandler)
				{
					KickStarter.stateHandler.gameState = GameState.Cutscene;
				}
				else
				{
					ACDebug.LogWarning("Could not set correct GameState!");
				}
				return conversationOnEnd;
			}
			return null;
		}

		public void RunConversation()
		{
			conversationOnEnd.Interact();
			conversationOnEnd = null;
		}

		public void Resume(RuntimeActionList runtimeActionList = null, bool rerunPausedActions = false)
		{
			if (runtimeActionList != null)
			{
				actionList = runtimeActionList;
				runtimeActionList.Resume(startIndex, resumeIndices, parameterData, rerunPausedActions);
			}
			else
			{
				actionList.Resume(startIndex, resumeIndices, parameterData, rerunPausedActions);
			}
		}

		public void SetResumeIndices(int[] _resumeIndices)
		{
			List<int> list = new List<int>();
			foreach (int item in _resumeIndices)
			{
				list.Add(item);
			}
			resumeIndices = list.ToArray();
		}

		public string GetSaveData(SubScene subScene)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			if (IsRunning())
			{
				return string.Empty;
			}
			string empty = string.Empty;
			if (actionListAsset != null)
			{
				text = AdvGame.PrepareStringForSaving(actionListAsset.name);
			}
			else if (actionList != null)
			{
				if (!actionList.GetComponent<ConstantID>())
				{
					ACDebug.LogWarning("Data for the ActionList '" + actionList.gameObject.name + "' was not saved because it has no Constant ID.", actionList.gameObject);
					return string.Empty;
				}
				text = actionList.GetComponent<ConstantID>().constantID.ToString();
				if ((!(subScene == null) || !UnityVersionHandler.ObjectIsInActiveScene(actionList.gameObject)) && (!(subScene != null) || !UnityVersionHandler.GetSceneInfoFromGameObject(actionList.gameObject).Matches(subScene.SceneInfo)))
				{
					return string.Empty;
				}
			}
			if (actionList != null)
			{
				empty = actionList.GetParameterData();
			}
			if (conversationOnEnd != null && (bool)conversationOnEnd.GetComponent<ConstantID>())
			{
				text2 = conversationOnEnd.GetComponent<ConstantID>().ToString();
			}
			return text + ":" + ConvertIndicesToString() + ":" + startIndex + ":" + (inSkipQueue ? 1 : 0) + ":" + (isRunning ? 1 : 0) + ":" + text2 + ":" + empty;
		}

		public bool LoadData(string dataString, SubScene subScene = null)
		{
			if (string.IsNullOrEmpty(dataString))
			{
				return false;
			}
			string[] array = dataString.Split(":"[0]);
			string text = AdvGame.PrepareStringForLoading(array[0]);
			resumeIndices = new int[0];
			string[] array2 = array[1].Split("]"[0]);
			if (array2.Length > 0)
			{
				List<int> list = new List<int>();
				for (int i = 0; i < array2.Length; i++)
				{
					int result = -1;
					if (int.TryParse(array2[i], out result) && result >= 0)
					{
						list.Add(result);
					}
				}
				resumeIndices = list.ToArray();
			}
			int.TryParse(array[2], out startIndex);
			int result2 = 0;
			int.TryParse(array[3], out result2);
			inSkipQueue = result2 == 1;
			result2 = 0;
			int.TryParse(array[4], out result2);
			isRunning = result2 == 1;
			int result3 = 0;
			int.TryParse(array[5], out result3);
			if (result3 != 0)
			{
				conversationOnEnd = Serializer.returnComponent<Conversation>(result3, (!(subScene != null)) ? null : subScene.gameObject);
			}
			parameterData = array[6];
			int result4 = 0;
			if (int.TryParse(text, out result4))
			{
				ConstantID constantID = Serializer.returnComponent<ConstantID>(result4, (!(subScene != null)) ? null : subScene.gameObject);
				if (constantID != null && constantID.GetComponent<ActionList>() != null)
				{
					actionList = constantID.GetComponent<ActionList>();
					return true;
				}
			}
			else
			{
				ActionListAsset actionListAsset = ScriptableObject.CreateInstance<ActionListAsset>();
				this.actionListAsset = AssetLoader.RetrieveAsset(actionListAsset, text);
				if (this.actionListAsset != null && this.actionListAsset != actionListAsset)
				{
					return true;
				}
				ACDebug.LogWarning("Could not restore data related to the ActionList asset '" + text + "' - to restore it correctly, the asset must be placed in a folder named Resources.");
			}
			return false;
		}

		protected string ConvertIndicesToString()
		{
			string text = string.Empty;
			if (resumeIndices != null && resumeIndices.Length > 0)
			{
				for (int i = 0; i < resumeIndices.Length; i++)
				{
					text += resumeIndices[i];
					if (i < resumeIndices.Length - 1)
					{
						text += "]";
					}
				}
			}
			return text;
		}
	}
}
