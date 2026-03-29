using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVarPreset : Action
	{
		public VariableLocation location;

		public int presetID;

		public int parameterID = -1;

		public bool ignoreOptionLinked;

		protected LocalVariables localVariables;

		public ActionVarPreset()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Assign preset";
			description = "Bulk-assigns the values of all Global or Local values to a predefined preset within the Variables Manager.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			presetID = AssignVariableID(parameters, parameterID, presetID);
		}

		public override void AssignParentList(ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject(actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			base.AssignParentList(actionList);
		}

		public override float Run()
		{
			if (location == VariableLocation.Local && !isAssetFile)
			{
				if (localVariables != null)
				{
					localVariables.AssignFromPreset(presetID);
				}
			}
			else
			{
				KickStarter.runtimeVariables.AssignFromPreset(presetID, ignoreOptionLinked);
			}
			return 0f;
		}

		public static ActionVarPreset CreateNew_Global(int presetID)
		{
			ActionVarPreset actionVarPreset = ScriptableObject.CreateInstance<ActionVarPreset>();
			actionVarPreset.location = VariableLocation.Global;
			actionVarPreset.presetID = presetID;
			return actionVarPreset;
		}

		public static ActionVarPreset CreateNew_Local(int presetID)
		{
			ActionVarPreset actionVarPreset = ScriptableObject.CreateInstance<ActionVarPreset>();
			actionVarPreset.location = VariableLocation.Local;
			actionVarPreset.presetID = presetID;
			return actionVarPreset;
		}
	}
}
