using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionParamCheck : ActionCheck
	{
		protected enum GameObjectCompareType
		{
			GameObject = 0,
			ConstantID = 1
		}

		public ActionListSource actionListSource;

		public ActionListAsset actionListAsset;

		public ActionList actionList;

		public int actionListConstantID;

		public int parameterID = -1;

		public int compareParameterID = -1;

		public bool checkOwn = true;

		public int intValue;

		public float floatValue;

		public IntCondition intCondition;

		public string stringValue;

		public int compareVariableID;

		public Variables compareVariables;

		public Vector3 vector3Value;

		public GameObject compareObject;

		public int compareObjectConstantID;

		protected GameObject runtimeCompareObject;

		public UnityEngine.Object compareUnityObject;

		public BoolValue boolValue = BoolValue.True;

		public BoolCondition boolCondition;

		public VectorCondition vectorCondition;

		protected ActionParameter _parameter;

		protected ActionParameter _compareParameter;

		protected Variables runtimeCompareVariables;

		[SerializeField]
		protected GameObjectCompareType gameObjectCompareType;

		public ActionParamCheck()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Check parameter";
			description = "Queries the value of parameters defined in the parent ActionList.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			_compareParameter = null;
			_parameter = null;
			runtimeCompareVariables = null;
			if (!checkOwn)
			{
				if (actionListSource == ActionListSource.InScene)
				{
					actionList = AssignFile(actionListConstantID, actionList);
					if (actionList != null)
					{
						if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null)
						{
							if (actionList.syncParamValues && actionList.assetFile.useParameters)
							{
								_parameter = GetParameterWithID(actionList.assetFile.GetParameters(), parameterID);
								_compareParameter = GetParameterWithID(actionList.assetFile.GetParameters(), compareParameterID);
							}
							else
							{
								_parameter = GetParameterWithID(actionList.parameters, parameterID);
								_compareParameter = GetParameterWithID(actionList.parameters, compareParameterID);
							}
						}
						else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
						{
							_parameter = GetParameterWithID(actionList.parameters, parameterID);
							_compareParameter = GetParameterWithID(actionList.parameters, compareParameterID);
						}
					}
				}
				else if (actionListSource == ActionListSource.AssetFile && actionListAsset != null)
				{
					_parameter = GetParameterWithID(actionListAsset.GetParameters(), parameterID);
					_compareParameter = GetParameterWithID(actionListAsset.GetParameters(), compareParameterID);
				}
			}
			else
			{
				_parameter = GetParameterWithID(parameters, parameterID);
				_compareParameter = GetParameterWithID(parameters, compareParameterID);
			}
			if (_compareParameter == _parameter)
			{
				_compareParameter = null;
			}
			runtimeCompareObject = AssignFile(compareObjectConstantID, compareObject);
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (_parameter == null)
			{
				return GenerateStopActionEnd();
			}
			GVar compareVar = null;
			InvItem compareItem = null;
			Document compareDoc = null;
			if (_parameter.parameterType == ParameterType.GlobalVariable || _parameter.parameterType == ParameterType.LocalVariable || _parameter.parameterType == ParameterType.ComponentVariable || _parameter.parameterType == ParameterType.InventoryItem || _parameter.parameterType == ParameterType.Document)
			{
				if (compareVariableID == -1)
				{
					return GenerateStopActionEnd();
				}
				if (_parameter.parameterType == ParameterType.GlobalVariable)
				{
					compareVar = GlobalVariables.GetVariable(compareVariableID, true);
				}
				else if (_parameter.parameterType == ParameterType.LocalVariable && !isAssetFile)
				{
					compareVar = LocalVariables.GetVariable(compareVariableID);
				}
				else if (_parameter.parameterType == ParameterType.ComponentVariable)
				{
					runtimeCompareVariables = AssignFile(compareObjectConstantID, compareVariables);
					if (runtimeCompareVariables != null)
					{
						compareVar = runtimeCompareVariables.GetVariable(compareVariableID);
					}
				}
				else if (_parameter.parameterType == ParameterType.InventoryItem)
				{
					compareItem = KickStarter.inventoryManager.GetItem(compareVariableID);
				}
				else if (_parameter.parameterType == ParameterType.Document)
				{
					compareDoc = KickStarter.inventoryManager.GetDocument(compareVariableID);
				}
			}
			return ProcessResult(CheckCondition(compareItem, compareVar, compareDoc), actions);
		}

		protected bool CheckCondition(InvItem _compareItem, GVar _compareVar, Document _compareDoc)
		{
			if (_parameter == null)
			{
				LogWarning("Cannot check state of variable since it cannot be found!");
				return false;
			}
			if (_parameter.parameterType == ParameterType.Boolean)
			{
				int num = _parameter.intValue;
				int num2 = (int)boolValue;
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					num2 = _compareParameter.intValue;
				}
				switch (boolCondition)
				{
				case BoolCondition.EqualTo:
					return num == num2;
				case BoolCondition.NotEqualTo:
					return num != num2;
				}
			}
			else if (_parameter.parameterType == ParameterType.Integer)
			{
				int num3 = _parameter.intValue;
				int num4 = intValue;
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					num4 = _compareParameter.intValue;
				}
				switch (intCondition)
				{
				case IntCondition.EqualTo:
					return num3 == num4;
				case IntCondition.NotEqualTo:
					return num3 != num4;
				case IntCondition.LessThan:
					return num3 < num4;
				case IntCondition.MoreThan:
					return num3 > num4;
				}
			}
			else if (_parameter.parameterType == ParameterType.Float)
			{
				float num5 = _parameter.floatValue;
				float num6 = floatValue;
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					num6 = _compareParameter.floatValue;
				}
				switch (intCondition)
				{
				case IntCondition.EqualTo:
					return Mathf.Approximately(num5, num6);
				case IntCondition.NotEqualTo:
					return !Mathf.Approximately(num5, num6);
				case IntCondition.MoreThan:
					return num5 > num6;
				case IntCondition.LessThan:
					return num5 < num6;
				}
			}
			else if (_parameter.parameterType == ParameterType.Vector3)
			{
				switch (vectorCondition)
				{
				case VectorCondition.EqualTo:
					if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
					{
						return _parameter.vector3Value == _compareParameter.vector3Value;
					}
					return _parameter.vector3Value == vector3Value;
				case VectorCondition.MagnitudeGreaterThan:
					if (_compareParameter != null && _compareParameter.parameterType == ParameterType.Float)
					{
						return _parameter.vector3Value.magnitude > _compareParameter.floatValue;
					}
					return _parameter.vector3Value.magnitude > floatValue;
				}
			}
			else if (_parameter.parameterType == ParameterType.String)
			{
				string text = _parameter.stringValue;
				string text2 = AdvGame.ConvertTokens(stringValue);
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					text2 = _compareParameter.stringValue;
				}
				switch (boolCondition)
				{
				case BoolCondition.EqualTo:
					return text == text2;
				case BoolCondition.NotEqualTo:
					return text != text2;
				}
			}
			else if (_parameter.parameterType == ParameterType.GameObject)
			{
				switch (gameObjectCompareType)
				{
				case GameObjectCompareType.GameObject:
					if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
					{
						compareObjectConstantID = _compareParameter.intValue;
						runtimeCompareObject = _compareParameter.gameObject;
					}
					if ((runtimeCompareObject != null && _parameter.gameObject == runtimeCompareObject) || (compareObjectConstantID != 0 && _parameter.intValue == compareObjectConstantID))
					{
						return true;
					}
					if (runtimeCompareObject == null && _parameter.gameObject == null)
					{
						return true;
					}
					break;
				case GameObjectCompareType.ConstantID:
				{
					int num7 = intValue;
					if (_compareParameter != null && _compareParameter.parameterType == ParameterType.Integer)
					{
						num7 = _compareParameter.intValue;
					}
					return _parameter.intValue == num7;
				}
				}
			}
			else if (_parameter.parameterType == ParameterType.UnityObject)
			{
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					compareUnityObject = _compareParameter.objectValue;
				}
				if (compareUnityObject != null && _parameter.objectValue == compareUnityObject)
				{
					return true;
				}
				if (compareUnityObject == null && _parameter.objectValue == null)
				{
					return true;
				}
			}
			else if (_parameter.parameterType == ParameterType.GlobalVariable || _parameter.parameterType == ParameterType.LocalVariable)
			{
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					return _compareParameter.intValue == _parameter.intValue;
				}
				if (_compareVar != null && _parameter.intValue == _compareVar.id)
				{
					return true;
				}
			}
			else if (_parameter.parameterType == ParameterType.ComponentVariable)
			{
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					return _compareParameter.intValue == _parameter.intValue && _compareParameter.variables == _parameter.variables;
				}
				if (_compareVar != null && _parameter.intValue == _compareVar.id && _parameter.variables == runtimeCompareVariables)
				{
					return true;
				}
			}
			else if (_parameter.parameterType == ParameterType.InventoryItem)
			{
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					return _compareParameter.intValue == _parameter.intValue;
				}
				if (_compareItem != null && _parameter.intValue == _compareItem.id)
				{
					return true;
				}
			}
			else if (_parameter.parameterType == ParameterType.Document)
			{
				if (_compareParameter != null && _compareParameter.parameterType == _parameter.parameterType)
				{
					return _compareParameter.intValue == _parameter.intValue;
				}
				if (_compareDoc != null && _parameter.intValue == _compareDoc.ID)
				{
					return true;
				}
			}
			return false;
		}

		public static ActionParamCheck CreateNew(int parameterID, bool checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.boolValue = (checkValue ? BoolValue.True : BoolValue.False);
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, bool checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.boolValue = (checkValue ? BoolValue.True : BoolValue.False);
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, bool checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.boolValue = (checkValue ? BoolValue.True : BoolValue.False);
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, int checkValue, IntCondition condition = IntCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.intValue = checkValue;
			actionParamCheck.intCondition = condition;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, int checkValue, IntCondition condition = IntCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.intValue = checkValue;
			actionParamCheck.intCondition = condition;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, int checkValue, IntCondition condition = IntCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.intValue = checkValue;
			actionParamCheck.intCondition = condition;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, float checkValue, IntCondition condition = IntCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.floatValue = checkValue;
			actionParamCheck.intCondition = condition;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, float checkValue, IntCondition condition = IntCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.floatValue = checkValue;
			actionParamCheck.intCondition = condition;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, float checkValue, IntCondition condition = IntCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.floatValue = checkValue;
			actionParamCheck.intCondition = condition;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, string checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.stringValue = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, string checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.stringValue = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, string checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.stringValue = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, Vector3 checkValue, VectorCondition condition = VectorCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.vector3Value = checkValue;
			actionParamCheck.floatValue = checkValue.magnitude;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, Vector3 checkValue, VectorCondition condition = VectorCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.vector3Value = checkValue;
			actionParamCheck.floatValue = checkValue.magnitude;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, Vector3 checkValue, VectorCondition condition = VectorCondition.EqualTo)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.vector3Value = checkValue;
			actionParamCheck.floatValue = checkValue.magnitude;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, Variables variables, int checkComponentVariableID)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareVariables = variables;
			actionParamCheck.compareVariableID = checkComponentVariableID;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, Variables variables, int checkComponentVariableID)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareVariables = variables;
			actionParamCheck.compareVariableID = checkComponentVariableID;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, Variables variables, int checkComponentVariableID)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareVariables = variables;
			actionParamCheck.compareVariableID = checkComponentVariableID;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, GameObject checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareObject = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, GameObject checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareObject = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, GameObject checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareObject = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(int parameterID, UnityEngine.Object checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = true;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareUnityObject = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionList actionList, int parameterID, UnityEngine.Object checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.InScene;
			actionParamCheck.actionList = actionList;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareUnityObject = checkValue;
			return actionParamCheck;
		}

		public static ActionParamCheck CreateNew(ActionListAsset actionListAsset, int parameterID, UnityEngine.Object checkValue)
		{
			ActionParamCheck actionParamCheck = ScriptableObject.CreateInstance<ActionParamCheck>();
			actionParamCheck.checkOwn = false;
			actionParamCheck.actionListSource = ActionListSource.AssetFile;
			actionParamCheck.actionListAsset = actionListAsset;
			actionParamCheck.parameterID = parameterID;
			actionParamCheck.compareUnityObject = checkValue;
			return actionParamCheck;
		}
	}
}
