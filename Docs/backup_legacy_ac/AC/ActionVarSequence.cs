using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVarSequence : ActionCheckMultiple
	{
		public int parameterID = -1;

		public int variableID;

		public bool doLoop;

		public bool saveToVariable = true;

		protected int ownVarValue;

		public VariableLocation location;

		protected LocalVariables localVariables;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		protected Variables runtimeVariables;

		public ActionVarSequence()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Run sequence";
			description = "Uses the value of an integer Variable to determine which Action is run next. The value is incremented by one each time (and reset to zero when a limit is reached), allowing for different subsequent Actions to play each time the Action is run.";
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
				runtimeVariables = AssignFile(variablesConstantID, variables);
				if (runtimeVariables != null)
				{
					runtimeVariable = runtimeVariables.GetVariable(variableID);
				}
				runtimeVariable = AssignVariable(parameters, parameterID, runtimeVariable);
				runtimeVariables = AssignVariablesComponent(parameters, parameterID, runtimeVariables);
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

		public override ActionEnd End(List<Action> actions)
		{
			if (numSockets <= 0)
			{
				LogWarning("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd();
			}
			if (!saveToVariable)
			{
				int i = ownVarValue;
				ownVarValue++;
				if (ownVarValue >= numSockets)
				{
					if (doLoop)
					{
						ownVarValue = 0;
					}
					else
					{
						ownVarValue = numSockets - 1;
					}
				}
				return ProcessResult(i, actions);
			}
			if (variableID == -1)
			{
				return GenerateStopActionEnd();
			}
			if (runtimeVariable != null)
			{
				if (runtimeVariable.type == VariableType.Integer)
				{
					if (runtimeVariable.val < 1)
					{
						runtimeVariable.val = 1;
					}
					int i2 = runtimeVariable.val - 1;
					runtimeVariable.val++;
					if (runtimeVariable.val > numSockets)
					{
						if (doLoop)
						{
							runtimeVariable.val = 1;
						}
						else
						{
							runtimeVariable.val = numSockets;
						}
					}
					runtimeVariable.Upload(location, runtimeVariables);
					return ProcessResult(i2, actions);
				}
				LogWarning("'Variable: Run sequence' Action is referencing a Variable that does not exist or is not an Integer!");
			}
			return GenerateStopActionEnd();
		}

		public static ActionVarSequence CreateNew(int numOutcomes, bool doLoop)
		{
			ActionVarSequence actionVarSequence = ScriptableObject.CreateInstance<ActionVarSequence>();
			actionVarSequence.numSockets = numOutcomes;
			actionVarSequence.doLoop = doLoop;
			return actionVarSequence;
		}
	}
}
