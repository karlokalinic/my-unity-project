using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVarCopy : Action
	{
		public int oldParameterID = -1;

		public int oldVariableID;

		public VariableLocation oldLocation;

		public int newParameterID = -1;

		public int newVariableID;

		public VariableLocation newLocation;

		public Variables oldVariables;

		public int oldVariablesConstantID;

		public Variables newVariables;

		public int newVariablesConstantID;

		protected LocalVariables localVariables;

		protected GVar oldRuntimeVariable;

		protected GVar newRuntimeVariable;

		protected Variables newRuntimeVariables;

		public ActionVarCopy()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Copy";
			description = "Copies the value of one Variable to another. This can be between Global and Local Variables, but only of those with the same type, such as Integer or Float.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			oldRuntimeVariable = null;
			switch (oldLocation)
			{
			case VariableLocation.Global:
				oldVariableID = AssignVariableID(parameters, oldParameterID, oldVariableID);
				oldRuntimeVariable = GlobalVariables.GetVariable(oldVariableID);
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					oldVariableID = AssignVariableID(parameters, oldParameterID, oldVariableID);
					oldRuntimeVariable = LocalVariables.GetVariable(oldVariableID, localVariables);
				}
				break;
			case VariableLocation.Component:
			{
				Variables variables = AssignFile(oldVariablesConstantID, oldVariables);
				if (variables != null)
				{
					oldRuntimeVariable = variables.GetVariable(oldVariableID);
				}
				oldRuntimeVariable = AssignVariable(parameters, oldParameterID, oldRuntimeVariable);
				break;
			}
			}
			newRuntimeVariable = null;
			switch (newLocation)
			{
			case VariableLocation.Global:
				newVariableID = AssignVariableID(parameters, newParameterID, newVariableID);
				newRuntimeVariable = GlobalVariables.GetVariable(newVariableID, true);
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					newVariableID = AssignVariableID(parameters, newParameterID, newVariableID);
					newRuntimeVariable = LocalVariables.GetVariable(newVariableID, localVariables);
				}
				break;
			case VariableLocation.Component:
				newRuntimeVariables = AssignFile(newVariablesConstantID, newVariables);
				if (newRuntimeVariables != null)
				{
					newRuntimeVariable = newRuntimeVariables.GetVariable(newVariableID);
				}
				newRuntimeVariable = AssignVariable(parameters, newParameterID, newRuntimeVariable);
				newRuntimeVariables = AssignVariablesComponent(parameters, newParameterID, newRuntimeVariables);
				break;
			}
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
			if (oldRuntimeVariable != null && newRuntimeVariable != null)
			{
				CopyVariable(newRuntimeVariable, oldRuntimeVariable);
				newRuntimeVariable.Upload(newLocation, newRuntimeVariables);
			}
			return 0f;
		}

		protected void CopyVariable(GVar newVar, GVar oldVar)
		{
			if (newVar == null || oldVar == null)
			{
				LogWarning("Cannot copy variable since it cannot be found!");
				return;
			}
			newVar.CopyFromVariable(oldVar, oldLocation);
			KickStarter.actionListManager.VariableChanged();
		}

		public static ActionVarCopy CreateNew(VariableLocation fromVariableLocation, Variables fromVariables, int fromVariableID, VariableLocation toVariableLocation, Variables toVariables, int toVariableID)
		{
			ActionVarCopy actionVarCopy = ScriptableObject.CreateInstance<ActionVarCopy>();
			actionVarCopy.oldLocation = fromVariableLocation;
			actionVarCopy.oldVariables = fromVariables;
			actionVarCopy.oldVariableID = fromVariableID;
			actionVarCopy.newLocation = toVariableLocation;
			actionVarCopy.newVariables = toVariables;
			actionVarCopy.newVariableID = toVariableID;
			return actionVarCopy;
		}
	}
}
