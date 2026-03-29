using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionParamSet : Action
	{
		public ActionListSource actionListSource;

		public ActionList actionList;

		public int actionListConstantID;

		public ActionListAsset actionListAsset;

		public bool changeOwn;

		public int parameterID = -1;

		public int parameterToCopyID = -1;

		public int intValue;

		public int intValueMax;

		public float floatValue;

		public float floatValueMax;

		public string stringValue;

		public GameObject gameobjectValue;

		protected GameObject runtimeGameobjectValue;

		public int gameObjectConstantID;

		public UnityEngine.Object unityObjectValue;

		public Vector3 vector3Value;

		public SetParamMethod setParamMethod;

		public int globalVariableID;

		public int ownParamID = -1;

		public Variables variables;

		protected Variables runtimeVariables;

		protected ActionParameter _parameter;

		protected ActionParameter _parameterToCopy;

		public Animator animator;

		public int animatorConstantID;

		public int animatorParameterID = -1;

		protected Animator runtimeAnimator;

		public string animatorParameterName;

		public ActionParamSet()
		{
			isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Set parameter";
			description = "Sets the value of a parameter in an ActionList.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (!changeOwn)
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
								_parameterToCopy = GetParameterWithID(actionList.assetFile.GetParameters(), parameterToCopyID);
							}
							else
							{
								_parameter = GetParameterWithID(actionList.parameters, parameterID);
								_parameterToCopy = GetParameterWithID(actionList.parameters, parameterToCopyID);
							}
						}
						else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
						{
							_parameter = GetParameterWithID(actionList.parameters, parameterID);
							_parameterToCopy = GetParameterWithID(actionList.parameters, parameterToCopyID);
						}
					}
				}
				else if (actionListSource == ActionListSource.AssetFile && actionListAsset != null)
				{
					_parameter = GetParameterWithID(actionListAsset.GetParameters(), parameterID);
					_parameterToCopy = GetParameterWithID(actionListAsset.GetParameters(), parameterToCopyID);
					if (_parameter.parameterType == ParameterType.GameObject && !isAssetFile && gameobjectValue != null && gameObjectConstantID == 0)
					{
						if ((bool)gameobjectValue.GetComponent<ConstantID>())
						{
							gameObjectConstantID = gameobjectValue.GetComponent<ConstantID>().constantID;
						}
						else
						{
							ACDebug.LogWarning("The GameObject '" + gameobjectValue.name + "' must have a Constant ID component in order to be passed as a parameter to an asset file.", gameobjectValue);
						}
					}
				}
			}
			else
			{
				_parameter = GetParameterWithID(parameters, parameterID);
				_parameterToCopy = GetParameterWithID(parameters, parameterToCopyID);
				if (_parameter.parameterType == ParameterType.GameObject && isAssetFile && gameobjectValue != null && gameObjectConstantID == 0)
				{
					if ((bool)gameobjectValue.GetComponent<ConstantID>())
					{
						gameObjectConstantID = gameobjectValue.GetComponent<ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning("The GameObject '" + gameobjectValue.name + "' must have a Constant ID component in order to be passed as a parameter to an asset file.", gameobjectValue);
					}
				}
			}
			runtimeGameobjectValue = AssignFile(gameObjectConstantID, gameobjectValue);
			if (setParamMethod == SetParamMethod.EnteredHere && _parameter != null)
			{
				switch (_parameter.parameterType)
				{
				case ParameterType.Boolean:
				{
					BoolValue field = ((intValue == 1) ? BoolValue.True : BoolValue.False);
					field = AssignBoolean(parameters, ownParamID, field);
					intValue = ((field == BoolValue.True) ? 1 : 0);
					break;
				}
				case ParameterType.Float:
					floatValue = AssignFloat(parameters, ownParamID, floatValue);
					break;
				case ParameterType.GameObject:
					runtimeGameobjectValue = AssignFile(parameters, ownParamID, gameObjectConstantID, gameobjectValue);
					break;
				case ParameterType.GlobalVariable:
				case ParameterType.LocalVariable:
					intValue = AssignVariableID(parameters, ownParamID, intValue);
					break;
				case ParameterType.ComponentVariable:
				{
					runtimeVariables = AssignFile(gameObjectConstantID, variables);
					ActionParameter parameterWithID = GetParameterWithID(parameters, ownParamID);
					if (parameterWithID != null && parameterWithID.parameterType == ParameterType.ComponentVariable)
					{
						runtimeVariables = parameterWithID.variables;
						intValue = parameterWithID.intValue;
					}
					break;
				}
				case ParameterType.Integer:
					intValue = AssignInteger(parameters, ownParamID, intValue);
					break;
				case ParameterType.InventoryItem:
					intValue = AssignInvItemID(parameters, ownParamID, intValue);
					break;
				case ParameterType.Document:
					intValue = AssignDocumentID(parameters, ownParamID, intValue);
					break;
				case ParameterType.String:
					stringValue = AssignString(parameters, ownParamID, stringValue);
					break;
				case ParameterType.UnityObject:
					unityObjectValue = AssignObject<UnityEngine.Object>(parameters, ownParamID, unityObjectValue);
					break;
				case ParameterType.Vector3:
					vector3Value = AssignVector3(parameters, ownParamID, vector3Value);
					break;
				}
			}
			else if (setParamMethod == SetParamMethod.CopiedFromAnimator)
			{
				runtimeAnimator = AssignFile(parameters, animatorParameterID, animatorConstantID, animator);
			}
		}

		public override float Run()
		{
			if (_parameter == null)
			{
				LogWarning("Cannot set parameter value since it cannot be found!");
				return 0f;
			}
			if (setParamMethod == SetParamMethod.CopiedFromGlobalVariable)
			{
				GVar variable = GlobalVariables.GetVariable(globalVariableID);
				if (variable != null)
				{
					switch (_parameter.parameterType)
					{
					case ParameterType.Integer:
					case ParameterType.Boolean:
						_parameter.intValue = variable.val;
						break;
					case ParameterType.Float:
						_parameter.floatValue = variable.floatVal;
						break;
					case ParameterType.Vector3:
						_parameter.vector3Value = variable.vector3Val;
						break;
					case ParameterType.String:
						_parameter.stringValue = GlobalVariables.GetStringValue(globalVariableID, true, Options.GetLanguage());
						break;
					}
				}
			}
			else if (setParamMethod == SetParamMethod.EnteredHere)
			{
				switch (_parameter.parameterType)
				{
				case ParameterType.InventoryItem:
				case ParameterType.GlobalVariable:
				case ParameterType.LocalVariable:
				case ParameterType.Integer:
				case ParameterType.Boolean:
				case ParameterType.Document:
					_parameter.intValue = intValue;
					break;
				case ParameterType.Float:
					_parameter.floatValue = floatValue;
					break;
				case ParameterType.String:
					_parameter.stringValue = stringValue;
					break;
				case ParameterType.GameObject:
					_parameter.gameObject = runtimeGameobjectValue;
					_parameter.intValue = gameObjectConstantID;
					break;
				case ParameterType.UnityObject:
					_parameter.objectValue = unityObjectValue;
					break;
				case ParameterType.Vector3:
					_parameter.vector3Value = vector3Value;
					break;
				case ParameterType.ComponentVariable:
					_parameter.SetValue(runtimeVariables, intValue);
					break;
				}
			}
			else if (setParamMethod == SetParamMethod.Random)
			{
				switch (_parameter.parameterType)
				{
				case ParameterType.Boolean:
					_parameter.intValue = UnityEngine.Random.Range(0, 2);
					break;
				case ParameterType.Integer:
					_parameter.intValue = UnityEngine.Random.Range(intValue, intValueMax + 1);
					break;
				case ParameterType.Float:
					_parameter.floatValue = UnityEngine.Random.Range(floatValue, floatValueMax);
					break;
				default:
					LogWarning(string.Concat("Parameters of type '", _parameter.parameterType, "' cannot be set randomly."));
					break;
				}
			}
			else if (setParamMethod == SetParamMethod.CopiedFromParameter)
			{
				if (_parameterToCopy == null)
				{
					LogWarning("Cannot copy parameter value since it cannot be found!");
					return 0f;
				}
				_parameter.CopyValues(_parameterToCopy);
			}
			else if (setParamMethod == SetParamMethod.CopiedFromAnimator)
			{
				if (runtimeAnimator == null)
				{
					LogWarning("Cannot set the value of parameter " + _parameter.label + ", because no Animator was found.");
					return 0f;
				}
				if (string.IsNullOrEmpty(animatorParameterName))
				{
					LogWarning("Cannot set a parameter value from Animator because no Animator parameter was named.");
					return 0f;
				}
				switch (_parameter.parameterType)
				{
				case ParameterType.Boolean:
				{
					bool flag = runtimeAnimator.GetBool(animatorParameterName);
					_parameter.SetValue(flag ? 1 : 0);
					break;
				}
				case ParameterType.Integer:
					_parameter.SetValue(runtimeAnimator.GetInteger(animatorParameterName));
					break;
				case ParameterType.Float:
					_parameter.SetValue(runtimeAnimator.GetFloat(animatorParameterName));
					break;
				default:
					LogWarning(string.Concat("Parameters of type '", _parameter.parameterType, "' cannot be set from an Animator."));
					break;
				}
			}
			return 0f;
		}

		public static ActionParamSet CreateNew(int parameterID, bool newBoolValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.intValue = (newBoolValue ? 1 : 0);
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, bool newBoolValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.intValue = (newBoolValue ? 1 : 0);
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, bool newBoolValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.intValue = (newBoolValue ? 1 : 0);
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, int newIntegerValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.intValue = newIntegerValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, int newIntegerValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.intValue = newIntegerValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, int newIntegerValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.intValue = newIntegerValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, float newFloatValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.floatValue = newFloatValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, float newFloatValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.floatValue = newFloatValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, float newFloatValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.floatValue = newFloatValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, string newStringValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.stringValue = newStringValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, string newStringValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.stringValue = newStringValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, string newStringValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.stringValue = newStringValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, Vector3 newVectorValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.vector3Value = newVectorValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, Vector3 newVectorValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.vector3Value = newVectorValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, Vector3 newVectorValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.vector3Value = newVectorValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, Variables variables, int newComponentVariableIDValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.variables = variables;
			actionParamSet.intValue = newComponentVariableIDValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, Variables variables, int newComponentVariableIDValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.variables = variables;
			actionParamSet.intValue = newComponentVariableIDValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, Variables variables, int newComponentVariableIDValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.variables = variables;
			actionParamSet.intValue = newComponentVariableIDValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, GameObject newGameObjectValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.gameobjectValue = newGameObjectValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, GameObject newGameObjectValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.gameobjectValue = newGameObjectValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, GameObject newGameObjectValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.gameobjectValue = newGameObjectValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(int parameterID, UnityEngine.Object newObjectValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = true;
			actionParamSet.parameterID = parameterID;
			actionParamSet.unityObjectValue = newObjectValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionList actionList, int parameterID, UnityEngine.Object newObjectValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.InScene;
			actionParamSet.actionList = actionList;
			actionParamSet.parameterID = parameterID;
			actionParamSet.unityObjectValue = newObjectValue;
			return actionParamSet;
		}

		public static ActionParamSet CreateNew(ActionListAsset actionListAsset, int parameterID, UnityEngine.Object newObjectValue)
		{
			ActionParamSet actionParamSet = ScriptableObject.CreateInstance<ActionParamSet>();
			actionParamSet.changeOwn = false;
			actionParamSet.actionListSource = ActionListSource.AssetFile;
			actionParamSet.actionListAsset = actionListAsset;
			actionParamSet.parameterID = parameterID;
			actionParamSet.unityObjectValue = newObjectValue;
			return actionParamSet;
		}
	}
}
