using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVarCheck : ActionCheck
	{
		public int parameterID = -1;

		public int variableID;

		public int variableNumber;

		public int checkParameterID = -1;

		public GetVarMethod getVarMethod;

		public int compareVariableID;

		public int intValue;

		public float floatValue;

		public IntCondition intCondition;

		public bool isAdditive;

		public BoolValue boolValue = BoolValue.True;

		public BoolCondition boolCondition;

		public string stringValue;

		public bool checkCase = true;

		public Vector3 vector3Value;

		public VectorCondition vectorCondition;

		public VariableLocation location;

		protected LocalVariables localVariables;

		public Variables variables;

		public int variablesConstantID;

		public Variables compareVariables;

		public int compareVariablesConstantID;

		protected GVar runtimeVariable;

		protected GVar runtimeCompareVariable;

		public ActionVarCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Check";
			description = "Queries the value of both Global and Local Variables declared in the Variables Manager. Variables can be compared with a fixed value, or with the values of other Variables.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			intValue = AssignInteger(parameters, checkParameterID, intValue);
			boolValue = AssignBoolean(parameters, checkParameterID, boolValue);
			floatValue = AssignFloat(parameters, checkParameterID, floatValue);
			vector3Value = AssignVector3(parameters, checkParameterID, vector3Value);
			stringValue = AssignString(parameters, checkParameterID, stringValue);
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
			runtimeCompareVariable = null;
			switch (getVarMethod)
			{
			case GetVarMethod.GlobalVariable:
				compareVariableID = AssignVariableID(parameters, checkParameterID, compareVariableID);
				runtimeCompareVariable = GlobalVariables.GetVariable(compareVariableID, true);
				break;
			case GetVarMethod.LocalVariable:
				compareVariableID = AssignVariableID(parameters, checkParameterID, compareVariableID);
				runtimeCompareVariable = LocalVariables.GetVariable(compareVariableID, localVariables);
				break;
			case GetVarMethod.ComponentVariable:
			{
				Variables variables2 = AssignFile(compareVariablesConstantID, compareVariables);
				if (variables2 != null)
				{
					runtimeCompareVariable = variables2.GetVariable(compareVariableID);
				}
				runtimeCompareVariable = AssignVariable(parameters, checkParameterID, runtimeCompareVariable);
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
			if ((getVarMethod == GetVarMethod.GlobalVariable || getVarMethod == GetVarMethod.LocalVariable || getVarMethod == GetVarMethod.ComponentVariable) && runtimeCompareVariable == null)
			{
				LogWarning("The 'Variable: Check' Action halted the ActionList because it cannot find the " + getVarMethod.ToString() + " to compare with.");
				return GenerateStopActionEnd();
			}
			if (runtimeVariable != null)
			{
				return ProcessResult(CheckCondition(runtimeVariable, runtimeCompareVariable), actions);
			}
			LogWarning("The 'Variable: Check' Action halted the ActionList because it cannot find the " + location.ToString() + " Variable with an ID of " + variableID);
			return GenerateStopActionEnd();
		}

		protected bool CheckCondition(GVar _var, GVar _compareVar)
		{
			if (_var == null)
			{
				LogWarning("Cannot check state of variable since it cannot be found!");
				return false;
			}
			if (_compareVar != null && _var != null && _compareVar.type != _var.type)
			{
				LogWarning("Cannot compare " + _var.label + " and " + _compareVar.label + " as they are not the same type!");
				return false;
			}
			if (_var.type == VariableType.Boolean)
			{
				int val = _var.val;
				int val2 = (int)boolValue;
				if (_compareVar != null)
				{
					val2 = _compareVar.val;
				}
				switch (boolCondition)
				{
				case BoolCondition.EqualTo:
					return val == val2;
				case BoolCondition.NotEqualTo:
					return val != val2;
				}
			}
			else if (_var.type == VariableType.Integer || _var.type == VariableType.PopUp)
			{
				int val3 = _var.val;
				int val4 = intValue;
				if (_compareVar != null)
				{
					val4 = _compareVar.val;
				}
				switch (intCondition)
				{
				case IntCondition.EqualTo:
					return val3 == val4;
				case IntCondition.NotEqualTo:
					return val3 != val4;
				case IntCondition.LessThan:
					return val3 < val4;
				case IntCondition.MoreThan:
					return val3 > val4;
				}
			}
			else if (_var.type == VariableType.Float)
			{
				float floatVal = _var.floatVal;
				float floatVal2 = floatValue;
				if (_compareVar != null)
				{
					floatVal2 = _compareVar.floatVal;
				}
				switch (intCondition)
				{
				case IntCondition.EqualTo:
					return Mathf.Approximately(floatVal, floatVal2);
				case IntCondition.NotEqualTo:
					return !Mathf.Approximately(floatVal, floatVal2);
				case IntCondition.LessThan:
					return floatVal < floatVal2;
				case IntCondition.MoreThan:
					return floatVal > floatVal2;
				}
			}
			else if (_var.type == VariableType.String)
			{
				string text = _var.textVal;
				string text2 = AdvGame.ConvertTokens(stringValue);
				if (_compareVar != null)
				{
					text2 = _compareVar.textVal;
				}
				if (!checkCase)
				{
					text = text.ToLower();
					text2 = text2.ToLower();
				}
				switch (boolCondition)
				{
				case BoolCondition.EqualTo:
					return text == text2;
				case BoolCondition.NotEqualTo:
					return text != text2;
				}
			}
			else if (_var.type == VariableType.Vector3)
			{
				switch (vectorCondition)
				{
				case VectorCondition.EqualTo:
					return _var.vector3Val == vector3Value;
				case VectorCondition.MagnitudeGreaterThan:
					return _var.vector3Val.magnitude > floatValue;
				}
			}
			return false;
		}

		protected int GetVarNumber(List<GVar> vars, int ID)
		{
			int num = 0;
			foreach (GVar var in vars)
			{
				if (var.id == ID)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		public static ActionVarCheck CreateNew_Global(int globalVariableID, int checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Global;
			actionVarCheck.variableID = globalVariableID;
			actionVarCheck.intValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Global(int globalVariableID, float checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Global;
			actionVarCheck.variableID = globalVariableID;
			actionVarCheck.floatValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Global(int globalVariableID, bool checkValue = true)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Global;
			actionVarCheck.variableID = globalVariableID;
			actionVarCheck.intValue = (checkValue ? 1 : 0);
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Global(int globalVariableID, Vector3 checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Global;
			actionVarCheck.variableID = globalVariableID;
			actionVarCheck.vector3Value = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Global(int globalVariableID, string checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Global;
			actionVarCheck.variableID = globalVariableID;
			actionVarCheck.stringValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Local(int localVariableID, int checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Local;
			actionVarCheck.variableID = localVariableID;
			actionVarCheck.intValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Local(int localVariableID, float checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Local;
			actionVarCheck.variableID = localVariableID;
			actionVarCheck.floatValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Local(int localVariableID, bool checkValue = true)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Local;
			actionVarCheck.variableID = localVariableID;
			actionVarCheck.intValue = (checkValue ? 1 : 0);
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Local(int localVariableID, Vector3 checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Local;
			actionVarCheck.variableID = localVariableID;
			actionVarCheck.vector3Value = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Local(int localVariableID, string checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Local;
			actionVarCheck.variableID = localVariableID;
			actionVarCheck.stringValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Component(Variables variables, int componentVariableID, int checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Component;
			actionVarCheck.variables = variables;
			actionVarCheck.variableID = componentVariableID;
			actionVarCheck.intValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Component(Variables variables, int componentVariableID, float checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Component;
			actionVarCheck.variables = variables;
			actionVarCheck.variableID = componentVariableID;
			actionVarCheck.floatValue = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Component(Variables variables, int componentVariableID, bool checkValue = true)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Component;
			actionVarCheck.variables = variables;
			actionVarCheck.variableID = componentVariableID;
			actionVarCheck.intValue = (checkValue ? 1 : 0);
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Component(Variables variables, int componentVariableID, Vector3 checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Component;
			actionVarCheck.variables = variables;
			actionVarCheck.variableID = componentVariableID;
			actionVarCheck.vector3Value = checkValue;
			return actionVarCheck;
		}

		public static ActionVarCheck CreateNew_Component(Variables variables, int componentVariableID, string checkValue)
		{
			ActionVarCheck actionVarCheck = ScriptableObject.CreateInstance<ActionVarCheck>();
			actionVarCheck.location = VariableLocation.Component;
			actionVarCheck.variables = variables;
			actionVarCheck.variableID = componentVariableID;
			actionVarCheck.stringValue = checkValue;
			return actionVarCheck;
		}
	}
}
