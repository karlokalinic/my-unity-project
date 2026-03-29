using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionVarSet : Action, ITranslatable
	{
		public enum SetVarMethodVector
		{
			SetValue = 0,
			IncreaseByValue = 1
		}

		public SetVarMethod setVarMethod;

		public SetVarMethodString setVarMethodString;

		public SetVarMethodIntBool setVarMethodIntBool;

		public SetVarMethodVector setVarMethodVector;

		public int parameterID = -1;

		public int variableID;

		public int variableNumber;

		public int setParameterID = -1;

		public int slotNumber;

		public int slotNumberParameterID = -1;

		public int intValue;

		public float floatValue;

		public BoolValue boolValue;

		public string stringValue;

		public string formula;

		public Vector3 vector3Value;

		public int lineID = -1;

		public VariableLocation location;

		public string menuName;

		public string elementName;

		public Animator animator;

		public string parameterName;

		protected LocalVariables localVariables;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		protected Variables runtimeVariables;

		public ActionVarSet()
		{
			isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Set";
			description = "Sets the value of both Global and Local Variables, as declared in the Variables Manager. Integers can be set to absolute, incremented or assigned a random value. Strings can also be set to the value of a MenuInput element, while Integers, Booleans and Floats can also be set to the value of a Mecanim parameter. When setting Integers and Floats, you can also opt to type in a forumla (e.g. 2 + 3 *4), which can also include tokens of the form [var:ID] to denote the value of a Variable, where ID is the unique number given to a Variable in the Variables Manager.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			intValue = AssignInteger(parameters, setParameterID, intValue);
			boolValue = AssignBoolean(parameters, setParameterID, boolValue);
			floatValue = AssignFloat(parameters, setParameterID, floatValue);
			vector3Value = AssignVector3(parameters, setParameterID, vector3Value);
			stringValue = AssignString(parameters, setParameterID, stringValue);
			formula = AssignString(parameters, setParameterID, formula);
			slotNumber = AssignInteger(parameters, slotNumberParameterID, slotNumber);
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

		public override float Run()
		{
			if (runtimeVariable != null)
			{
				SetVariable(runtimeVariable, location, false);
			}
			return 0f;
		}

		public override void Skip()
		{
			if (runtimeVariable != null)
			{
				SetVariable(runtimeVariable, location, true);
			}
		}

		protected void SetVariable(GVar var, VariableLocation location, bool doSkip)
		{
			if (var == null)
			{
				return;
			}
			switch (var.type)
			{
			case VariableType.Integer:
			{
				int newValue3 = 0;
				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					newValue3 = ((setVarMethod != SetVarMethod.Formula) ? intValue : ((int)AdvGame.CalculateFormula(AdvGame.ConvertTokens(formula, Options.GetLanguage(), localVariables))));
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter && (bool)animator && !string.IsNullOrEmpty(parameterName))
				{
					newValue3 = animator.GetInteger(parameterName);
					setVarMethod = SetVarMethod.SetValue;
				}
				if (setVarMethod == SetVarMethod.IncreaseByValue && doSkip)
				{
					var.RestoreBackupValue();
				}
				var.SetValue(newValue3, setVarMethod);
				if (doSkip)
				{
					var.BackupValue();
				}
				break;
			}
			case VariableType.Float:
			{
				float newValue2 = 0f;
				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					newValue2 = ((setVarMethod != SetVarMethod.Formula) ? floatValue : ((float)AdvGame.CalculateFormula(AdvGame.ConvertTokens(formula, Options.GetLanguage(), localVariables))));
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter && (bool)animator && !string.IsNullOrEmpty(parameterName))
				{
					newValue2 = animator.GetFloat(parameterName);
					setVarMethod = SetVarMethod.SetValue;
				}
				if (setVarMethod == SetVarMethod.IncreaseByValue && doSkip)
				{
					var.RestoreBackupValue();
				}
				var.SetFloatValue(newValue2, setVarMethod);
				if (doSkip)
				{
					var.BackupValue();
				}
				break;
			}
			case VariableType.Boolean:
			{
				int newValue = 0;
				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					newValue = (int)boolValue;
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter && (bool)animator && !string.IsNullOrEmpty(parameterName) && animator.GetBool(parameterName))
				{
					newValue = 1;
				}
				var.SetValue(newValue);
				break;
			}
			case VariableType.Vector3:
			{
				Vector3 vector = vector3Value;
				if (setVarMethodVector == SetVarMethodVector.IncreaseByValue)
				{
					vector += var.vector3Val;
				}
				var.SetVector3Value(vector);
				break;
			}
			case VariableType.PopUp:
			{
				int num2 = 0;
				num2 = ((setVarMethod == SetVarMethod.Formula) ? ((int)AdvGame.CalculateFormula(AdvGame.ConvertTokens(formula, Options.GetLanguage(), localVariables))) : ((setVarMethod != SetVarMethod.SetAsRandom) ? Mathf.Clamp(intValue, 0, var.GetNumPopUpValues() - 1) : var.GetNumPopUpValues()));
				if (setVarMethod == SetVarMethod.IncreaseByValue && doSkip)
				{
					var.RestoreBackupValue();
				}
				var.SetValue(num2, setVarMethod);
				if (doSkip)
				{
					var.BackupValue();
				}
				break;
			}
			case VariableType.String:
			{
				string text = string.Empty;
				if (setVarMethodString == SetVarMethodString.EnteredHere)
				{
					text = AdvGame.ConvertTokens(stringValue, Options.GetLanguage(), localVariables);
				}
				else if (setVarMethodString == SetVarMethodString.SetAsMenuElementText)
				{
					MenuElement elementWithName = PlayerMenus.GetElementWithName(menuName, elementName);
					if (elementWithName != null)
					{
						if (elementWithName is MenuInput)
						{
							MenuInput menuInput = (MenuInput)elementWithName;
							text = menuInput.GetContents();
							if (KickStarter.runtimeLanguages.LanguageReadsRightToLeft(Options.GetLanguage()) && text.Length > 0)
							{
								char[] array = text.ToCharArray();
								text = string.Empty;
								for (int num = array.Length - 1; num >= 0; num--)
								{
									text += array[num];
								}
							}
						}
						else
						{
							PlayerMenus.GetMenuWithName(menuName).Recalculate();
							elementWithName.PreDisplay(slotNumber, Options.GetLanguage(), false);
							text = elementWithName.GetLabel(slotNumber, Options.GetLanguage());
						}
					}
					else
					{
						LogWarning("Could not find MenuInput '" + elementName + "' in Menu '" + menuName + "'");
					}
				}
				var.SetStringValue(text, lineID);
				break;
			}
			}
			var.Upload(location, runtimeVariables);
			KickStarter.actionListManager.VariableChanged();
		}

		public string GetTranslatableString(int index)
		{
			return stringValue;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}

		public static ActionVarSet CreateNew_Global(int globalVariableID, int newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Global;
			actionVarSet.variableID = globalVariableID;
			actionVarSet.intValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Global(int globalVariableID, float newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Global;
			actionVarSet.variableID = globalVariableID;
			actionVarSet.floatValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Global(int globalVariableID, bool newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Global;
			actionVarSet.variableID = globalVariableID;
			actionVarSet.intValue = (newValue ? 1 : 0);
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Global(int globalVariableID, Vector3 newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Global;
			actionVarSet.variableID = globalVariableID;
			actionVarSet.vector3Value = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Global(int globalVariableID, string newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Global;
			actionVarSet.variableID = globalVariableID;
			actionVarSet.stringValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Local(int localVariableID, int newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Local;
			actionVarSet.variableID = localVariableID;
			actionVarSet.intValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Local(int localVariableID, float newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Local;
			actionVarSet.variableID = localVariableID;
			actionVarSet.floatValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Local(int localVariableID, bool newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Local;
			actionVarSet.variableID = localVariableID;
			actionVarSet.intValue = (newValue ? 1 : 0);
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Local(int localVariableID, Vector3 newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Local;
			actionVarSet.variableID = localVariableID;
			actionVarSet.vector3Value = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Local(int localVariableID, string newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Local;
			actionVarSet.variableID = localVariableID;
			actionVarSet.stringValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Component(Variables variables, int componentVariableID, int newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Component;
			actionVarSet.variables = variables;
			actionVarSet.variableID = componentVariableID;
			actionVarSet.intValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Component(Variables variables, int componentVariableID, float newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Component;
			actionVarSet.variables = variables;
			actionVarSet.variableID = componentVariableID;
			actionVarSet.floatValue = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Component(Variables variables, int componentVariableID, bool newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Component;
			actionVarSet.variables = variables;
			actionVarSet.variableID = componentVariableID;
			actionVarSet.intValue = (newValue ? 1 : 0);
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Component(Variables variables, int componentVariableID, Vector3 newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Component;
			actionVarSet.variables = variables;
			actionVarSet.variableID = componentVariableID;
			actionVarSet.vector3Value = newValue;
			return actionVarSet;
		}

		public static ActionVarSet CreateNew_Component(Variables variables, int componentVariableID, string newValue)
		{
			ActionVarSet actionVarSet = ScriptableObject.CreateInstance<ActionVarSet>();
			actionVarSet.location = VariableLocation.Component;
			actionVarSet.variables = variables;
			actionVarSet.variableID = componentVariableID;
			actionVarSet.stringValue = newValue;
			return actionVarSet;
		}
	}
}
