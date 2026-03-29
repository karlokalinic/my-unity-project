using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionRandomCheck : ActionCheckMultiple
	{
		public bool disallowSuccessive;

		public bool saveToVariable = true;

		protected int ownVarValue = -1;

		public int parameterID = -1;

		public int variableID;

		public VariableLocation location;

		public Variables variables;

		public int variablesConstantID;

		protected LocalVariables localVariables;

		protected GVar runtimeVariable;

		public ActionRandomCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Check random number";
			description = "Picks a number at random between zero and a specified integer – the value of which determine which subsequent Action is run next.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeVariable = null;
			if (!saveToVariable)
			{
				return;
			}
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

		public override ActionEnd End(List<Action> actions)
		{
			if (numSockets <= 0)
			{
				LogWarning("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd();
			}
			int num = UnityEngine.Random.Range(0, numSockets);
			if (numSockets > 1 && disallowSuccessive)
			{
				if (saveToVariable)
				{
					if (runtimeVariable != null && runtimeVariable.type == VariableType.Integer)
					{
						ownVarValue = runtimeVariable.val;
					}
					else
					{
						LogWarning("No Integer variable found!");
					}
				}
				while (ownVarValue == num)
				{
					num = UnityEngine.Random.Range(0, numSockets);
				}
				ownVarValue = num;
				if (saveToVariable && runtimeVariable != null && runtimeVariable.type == VariableType.Integer)
				{
					runtimeVariable.SetValue(ownVarValue);
				}
			}
			return ProcessResult(num, actions);
		}

		public static ActionRandomCheck CreateNew(int numOutcomes, bool disallowSuccessive)
		{
			ActionRandomCheck actionRandomCheck = ScriptableObject.CreateInstance<ActionRandomCheck>();
			actionRandomCheck.numSockets = numOutcomes;
			actionRandomCheck.disallowSuccessive = disallowSuccessive;
			return actionRandomCheck;
		}
	}
}
