using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInvProperty : Action
	{
		protected enum SetVarAsPropertyMethod
		{
			SpecificItem = 0,
			SelectedItem = 1
		}

		public int varParameterID = -1;

		public int variableID;

		public VariableLocation varLocation;

		[SerializeField]
		protected SetVarAsPropertyMethod setVarAsPropertyMethod;

		public bool multiplyByItemCount;

		public int invID;

		public int invParameterID;

		public int propertyID;

		protected LocalVariables localVariables;

		protected InventoryManager inventoryManager;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		public ActionInvProperty()
		{
			isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Property to Variable";
			description = "Sets the value of a Variable as an Inventory item property.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			invID = AssignInvItemID(parameters, invParameterID, invID);
			runtimeVariable = null;
			switch (varLocation)
			{
			case VariableLocation.Global:
				variableID = AssignVariableID(parameters, varParameterID, variableID);
				runtimeVariable = GlobalVariables.GetVariable(variableID, true);
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					variableID = AssignVariableID(parameters, varParameterID, variableID);
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
				runtimeVariable = AssignVariable(parameters, varParameterID, runtimeVariable);
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

		public override float Run()
		{
			int num = -1;
			if (setVarAsPropertyMethod == SetVarAsPropertyMethod.SelectedItem)
			{
				if (KickStarter.runtimeInventory.SelectedItem != null)
				{
					num = KickStarter.runtimeInventory.SelectedItem.id;
				}
			}
			else
			{
				num = invID;
			}
			InvVar invVar = null;
			if (num >= 0)
			{
				if (multiplyByItemCount)
				{
					invVar = KickStarter.runtimeInventory.GetPropertyTotals(propertyID, num);
				}
				else
				{
					InvItem item = KickStarter.inventoryManager.GetItem(num);
					if (item != null)
					{
						invVar = item.GetProperty(propertyID);
					}
				}
			}
			if (invVar == null)
			{
				LogWarning("Cannot find property with ID " + propertyID + " on Inventory item ID " + num);
				return 0f;
			}
			if (runtimeVariable.type == VariableType.String)
			{
				runtimeVariable.textVal = invVar.GetDisplayValue(Options.GetLanguage());
			}
			else if (runtimeVariable.type == invVar.type)
			{
				if (invVar.type == VariableType.Float)
				{
					runtimeVariable.FloatValue = invVar.FloatValue;
				}
				else if (invVar.type == VariableType.Vector3)
				{
					runtimeVariable.Vector3Value = invVar.Vector3Value;
				}
				else
				{
					runtimeVariable.IntegerValue = invVar.IntegerValue;
				}
			}
			else
			{
				LogWarning("Cannot assign " + varLocation.ToString() + " Variable " + runtimeVariable.label + "'s value from '" + invVar.label + "' property because their types do not match.");
			}
			return 0f;
		}

		public static ActionInvProperty CreateNew_ToGlobalVariable(int variableID, int propertyID, int itemID = -1)
		{
			ActionInvProperty actionInvProperty = ScriptableObject.CreateInstance<ActionInvProperty>();
			actionInvProperty.setVarAsPropertyMethod = ((itemID < 0) ? SetVarAsPropertyMethod.SelectedItem : SetVarAsPropertyMethod.SpecificItem);
			actionInvProperty.propertyID = propertyID;
			actionInvProperty.varLocation = VariableLocation.Global;
			actionInvProperty.variableID = variableID;
			return actionInvProperty;
		}

		public static ActionInvProperty CreateNew_ToLocalVariable(int variableID, int propertyID, int itemID = -1)
		{
			ActionInvProperty actionInvProperty = ScriptableObject.CreateInstance<ActionInvProperty>();
			actionInvProperty.setVarAsPropertyMethod = ((itemID < 0) ? SetVarAsPropertyMethod.SelectedItem : SetVarAsPropertyMethod.SpecificItem);
			actionInvProperty.propertyID = propertyID;
			actionInvProperty.varLocation = VariableLocation.Local;
			actionInvProperty.variableID = variableID;
			return actionInvProperty;
		}

		public static ActionInvProperty CreateNew_ToComponentVariable(Variables variables, int variableID, int propertyID, int itemID = -1)
		{
			ActionInvProperty actionInvProperty = ScriptableObject.CreateInstance<ActionInvProperty>();
			actionInvProperty.setVarAsPropertyMethod = ((itemID < 0) ? SetVarAsPropertyMethod.SelectedItem : SetVarAsPropertyMethod.SpecificItem);
			actionInvProperty.propertyID = propertyID;
			actionInvProperty.varLocation = VariableLocation.Component;
			actionInvProperty.variableID = variableID;
			return actionInvProperty;
		}
	}
}
