using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActiveInput
	{
		protected enum ActiveInputButtonType
		{
			OnButtonDown = 0,
			OnButtonUp = 1
		}

		public string label;

		public int ID;

		public string inputName;

		public bool enabledOnStart = true;

		public GameState gameState;

		public ActionListAsset actionListAsset;

		public SimulateInputType inputType;

		public float axisThreshold = 0.2f;

		[SerializeField]
		protected ActiveInputButtonType buttonType;

		protected bool isEnabled;

		public bool IsEnabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				if (value && string.IsNullOrEmpty(inputName))
				{
					ACDebug.LogWarning("Active input " + ID + " has no input name!");
					value = false;
				}
				isEnabled = value;
			}
		}

		public string Label
		{
			get
			{
				if (string.IsNullOrEmpty(label))
				{
					if (!string.IsNullOrEmpty(inputName))
					{
						label = inputName;
					}
					else
					{
						label = "(Untitled)";
					}
				}
				return label;
			}
		}

		public ActiveInput(int[] idArray)
		{
			inputName = string.Empty;
			gameState = GameState.Normal;
			actionListAsset = null;
			enabledOnStart = true;
			ID = 1;
			inputType = SimulateInputType.Button;
			buttonType = ActiveInputButtonType.OnButtonDown;
			axisThreshold = 0.2f;
			foreach (int num in idArray)
			{
				if (ID == num)
				{
					ID++;
				}
			}
		}

		public ActiveInput(int _ID)
		{
			inputName = string.Empty;
			gameState = GameState.Normal;
			actionListAsset = null;
			enabledOnStart = true;
			ID = _ID;
			inputType = SimulateInputType.Button;
			buttonType = ActiveInputButtonType.OnButtonDown;
			axisThreshold = 0.2f;
		}

		public void SetDefaultState()
		{
			isEnabled = enabledOnStart;
		}

		public bool TestForInput()
		{
			if (IsEnabled)
			{
				switch (inputType)
				{
				case SimulateInputType.Button:
					if (buttonType == ActiveInputButtonType.OnButtonDown)
					{
						if (KickStarter.playerInput.InputGetButtonDown(inputName))
						{
							return TriggerIfStateMatches();
						}
					}
					else if (buttonType == ActiveInputButtonType.OnButtonUp && KickStarter.playerInput.InputGetButtonUp(inputName))
					{
						return TriggerIfStateMatches();
					}
					break;
				case SimulateInputType.Axis:
				{
					float num = KickStarter.playerInput.InputGetAxis(inputName);
					if ((axisThreshold >= 0f && num > axisThreshold) || (axisThreshold < 0f && num < axisThreshold))
					{
						return TriggerIfStateMatches();
					}
					break;
				}
				}
			}
			return false;
		}

		protected bool TriggerIfStateMatches()
		{
			if (KickStarter.stateHandler.gameState == gameState && actionListAsset != null && !KickStarter.actionListAssetManager.IsListRunning(actionListAsset))
			{
				AdvGame.RunActionListAsset(actionListAsset);
				return true;
			}
			return false;
		}

		public static void Upgrade()
		{
			if (AdvGame.GetReferences() != null && AdvGame.GetReferences().settingsManager != null && AdvGame.GetReferences().settingsManager.activeInputs != null && AdvGame.GetReferences().settingsManager.activeInputs.Count > 0 && AdvGame.GetReferences().settingsManager.activeInputs[0].ID == 0)
			{
				for (int i = 0; i < AdvGame.GetReferences().settingsManager.activeInputs.Count; i++)
				{
					AdvGame.GetReferences().settingsManager.activeInputs[i].ID = i + 1;
					AdvGame.GetReferences().settingsManager.activeInputs[i].enabledOnStart = true;
				}
			}
		}

		public static string CreateSaveData(List<ActiveInput> activeInputs)
		{
			if (activeInputs != null && activeInputs.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ActiveInput activeInput in activeInputs)
				{
					if (activeInput != null)
					{
						stringBuilder.Append(activeInput.ID.ToString());
						stringBuilder.Append(":");
						stringBuilder.Append((!activeInput.IsEnabled) ? "0" : "1");
						stringBuilder.Append("|");
					}
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				return stringBuilder.ToString();
			}
			return string.Empty;
		}

		public static void LoadSaveData(string dataString)
		{
			if (string.IsNullOrEmpty(dataString) || KickStarter.settingsManager.activeInputs == null || KickStarter.settingsManager.activeInputs.Count <= 0)
			{
				return;
			}
			string[] array = dataString.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				int result2 = 0;
				int.TryParse(array3[1], out result2);
				foreach (ActiveInput activeInput in KickStarter.settingsManager.activeInputs)
				{
					if (activeInput.ID == result)
					{
						activeInput.isEnabled = result2 == 1;
					}
				}
			}
		}
	}
}
