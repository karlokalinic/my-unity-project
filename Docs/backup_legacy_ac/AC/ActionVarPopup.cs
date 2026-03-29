using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVarPopup : ActionCheckMultiple
	{
		public int variableID;

		public int variableNumber;

		public VariableLocation location;

		protected LocalVariables localVariables;

		public Variables variables;

		public int variablesConstantID;

		[SerializeField]
		protected int parameterID = -1;

		protected GVar runtimeVariable;

		public ActionVarPopup()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Pop Up switch";
			description = "Uses the value of a Pop Up Variable to determine which Action is run next. An option for each possible value the Variable can take will be displayed, allowing for different subsequent Actions to run.";
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

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeVariable = null;
			switch (location)
			{
			case VariableLocation.Global:
				variableID = AssignVariableID(parameters, parameterID, variableID);
				runtimeVariable = GlobalVariables.GetVariable(variableID, true);
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					variableID = AssignVariableID(parameters, parameterID, variableID);
					runtimeVariable = LocalVariables.GetVariable(variableID, localVariables);
				}
				break;
			case VariableLocation.Component:
			{
				Variables variables = AssignFile(variablesConstantID, this.variables);
				if (variables != null)
				{
					runtimeVariable = variables.GetVariable(variableID);
				}
				runtimeVariable = AssignVariable(parameters, parameterID, runtimeVariable);
				break;
			}
			}
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (runtimeVariable != null && runtimeVariable.type != VariableType.PopUp)
			{
				LogWarning("Variable: Pop Up switch Action is referencing a Variable that is not a PopUp!");
				runtimeVariable = null;
			}
			else if (runtimeVariable == null)
			{
				LogWarning("Variable: Pop Up switch Action is referencing a Variable that does not exist!");
			}
			if (numSockets <= 0)
			{
				LogWarning("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd();
			}
			if (runtimeVariable != null)
			{
				return ProcessResult(runtimeVariable.val, actions);
			}
			return GenerateStopActionEnd();
		}

		protected GVar GetVariable()
		{
			GVar gVar = null;
			switch (location)
			{
			case VariableLocation.Global:
				if ((bool)AdvGame.GetReferences().variablesManager)
				{
					gVar = AdvGame.GetReferences().variablesManager.GetVariable(variableID);
				}
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					gVar = LocalVariables.GetVariable(variableID, localVariables);
				}
				break;
			case VariableLocation.Component:
			{
				Variables variables = AssignFile(variablesConstantID, this.variables);
				if (variables != null)
				{
					gVar = variables.GetVariable(variableID);
				}
				break;
			}
			}
			if (gVar != null && gVar.type == VariableType.PopUp)
			{
				return gVar;
			}
			return null;
		}

		public static ActionVarPopup CreateNew_Global(int globalVariableID)
		{
			ActionVarPopup actionVarPopup = ScriptableObject.CreateInstance<ActionVarPopup>();
			actionVarPopup.location = VariableLocation.Global;
			actionVarPopup.variableID = globalVariableID;
			GVar variable = actionVarPopup.GetVariable();
			if (variable != null)
			{
				actionVarPopup.numSockets = variable.GetNumPopUpValues();
			}
			return actionVarPopup;
		}

		public static ActionVarPopup CreateNew_Local(int localVariableID)
		{
			ActionVarPopup actionVarPopup = ScriptableObject.CreateInstance<ActionVarPopup>();
			actionVarPopup.location = VariableLocation.Local;
			actionVarPopup.variableID = localVariableID;
			GVar variable = actionVarPopup.GetVariable();
			if (variable != null)
			{
				actionVarPopup.numSockets = variable.GetNumPopUpValues();
			}
			return actionVarPopup;
		}

		public static ActionVarPopup CreateNew_Component(Variables variables, int componentVariableID)
		{
			ActionVarPopup actionVarPopup = ScriptableObject.CreateInstance<ActionVarPopup>();
			actionVarPopup.location = VariableLocation.Component;
			actionVarPopup.variables = variables;
			actionVarPopup.variableID = componentVariableID;
			GVar variable = actionVarPopup.GetVariable();
			if (variable != null)
			{
				actionVarPopup.numSockets = variable.GetNumPopUpValues();
			}
			return actionVarPopup;
		}
	}
}
