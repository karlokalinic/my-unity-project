using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_action_list_asset.html")]
	public class ActionListAsset : ScriptableObject
	{
		public List<Action> actions = new List<Action>();

		public bool isSkippable = true;

		public ActionListType actionListType;

		public bool unfreezePauseMenus = true;

		public bool useParameters;

		public bool canRunMultipleInstances;

		public bool canSurviveSceneChanges;

		public bool revertToDefaultParametersAfterRunning;

		[SerializeField]
		private List<ActionParameter> parameters = new List<ActionParameter>();

		private List<ActionParameter> runtimeParameters = new List<ActionParameter>();

		[HideInInspector]
		public int tagID;

		public List<ActionParameter> DefaultParameters
		{
			get
			{
				return parameters;
			}
			set
			{
				parameters = value;
			}
		}

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

		public bool IsSkippable()
		{
			if (isSkippable && actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}

		public void Interact()
		{
			AdvGame.RunActionListAsset(this);
		}

		public void Interact(List<ActionParameter> newParameters)
		{
			AssignParameterValues(newParameters);
			Interact();
		}

		public void RunFromIndex(int index)
		{
			AdvGame.RunActionListAsset(this, index, true);
		}

		public RuntimeActionList Interact(int parameterID, int parameterValue)
		{
			return AdvGame.RunActionListAsset(this, parameterID, parameterValue);
		}

		public void KillAllInstances()
		{
			if (KickStarter.actionListAssetManager != null)
			{
				int num = KickStarter.actionListAssetManager.EndAssetList(this, null, true);
				ACDebug.Log("Ended " + num + " instances of the ActionList Asset '" + base.name + "'", this);
			}
		}

		public ActionParameter GetParameter(int _ID)
		{
			if (useParameters && parameters != null)
			{
				if (runtimeParameters == null)
				{
					runtimeParameters = new List<ActionParameter>();
				}
				foreach (ActionParameter runtimeParameter in runtimeParameters)
				{
					if (runtimeParameter.ID == _ID)
					{
						return runtimeParameter;
					}
				}
				foreach (ActionParameter parameter in parameters)
				{
					if (parameter.ID == _ID)
					{
						ActionParameter actionParameter = new ActionParameter(parameter, true);
						runtimeParameters.Add(actionParameter);
						return actionParameter;
					}
				}
			}
			return null;
		}

		public List<ActionParameter> GetParameters()
		{
			if (useParameters && parameters != null)
			{
				if (runtimeParameters == null)
				{
					runtimeParameters = new List<ActionParameter>();
				}
				foreach (ActionParameter parameter in parameters)
				{
					GetParameter(parameter.ID);
				}
				return runtimeParameters;
			}
			return null;
		}

		public void AssignParameterValues(List<ActionParameter> newParameters)
		{
			if (!useParameters || parameters == null)
			{
				return;
			}
			if (runtimeParameters == null)
			{
				runtimeParameters = new List<ActionParameter>();
			}
			foreach (ActionParameter newParameter in newParameters)
			{
				ActionParameter parameter = GetParameter(newParameter.ID);
				if (parameter != null)
				{
					parameter.CopyValues(newParameter);
				}
			}
		}

		public void AfterDownloading()
		{
			if (revertToDefaultParametersAfterRunning)
			{
				ResetParameters();
			}
		}

		private void ResetParameters()
		{
			runtimeParameters.Clear();
		}
	}
}
