using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public abstract class Action : ScriptableObject
	{
		public int id;

		public ActionCategory category = ActionCategory.Custom;

		public string title;

		public string description;

		public bool isDisplayed;

		public bool showComment;

		public string comment;

		public int numSockets = 1;

		public bool willWait;

		[NonSerialized]
		public bool isRunning;

		[NonSerialized]
		public ActionEnd lastResult = new ActionEnd(-10);

		public ResultAction endAction;

		public int skipAction = -1;

		public Action skipActionActual;

		public Cutscene linkedCutscene;

		public ActionListAsset linkedAsset;

		public bool isEnabled = true;

		public bool isAssetFile;

		[NonSerialized]
		public bool isMarked;

		public bool isBreakPoint;

		public float defaultPauseTime
		{
			get
			{
				return -1f;
			}
		}

		public Action()
		{
			isDisplayed = true;
		}

		public virtual float Run()
		{
			return 0f;
		}

		public virtual void Skip()
		{
			Run();
		}

		public virtual ActionEnd End(List<Action> actions)
		{
			return GenerateActionEnd(endAction, linkedAsset, linkedCutscene, skipAction, skipActionActual, actions);
		}

		public void PrintComment(ActionList actionList)
		{
			if (showComment && !string.IsNullOrEmpty(comment))
			{
				string text = AdvGame.ConvertTokens(comment, 0, null, actionList.parameters);
				string text2 = text;
				text = text2 + "\n(From Action '(" + actionList.actions.IndexOf(this) + ") " + KickStarter.actionsManager.GetActionTypeLabel(this) + "' in ActionList '" + actionList.gameObject.name + "')";
				ACDebug.Log(text, actionList);
			}
		}

		public virtual void AssignParentList(ActionList actionList)
		{
		}

		protected void Log(string message, UnityEngine.Object context = null)
		{
			ACDebug.Log(message, context);
		}

		protected void LogWarning(string message, UnityEngine.Object context = null)
		{
			ACDebug.LogWarning(message, context);
		}

		protected void LogError(string message, UnityEngine.Object context = null)
		{
			ACDebug.LogError(message, context);
		}

		public virtual void AssignValues(List<ActionParameter> parameters)
		{
			AssignValues();
		}

		public virtual void AssignValues()
		{
		}

		protected ActionParameter GetParameterWithID(List<ActionParameter> parameters, int _id)
		{
			if (parameters != null && _id >= 0)
			{
				foreach (ActionParameter parameter in parameters)
				{
					if (parameter.ID == _id)
					{
						return parameter;
					}
				}
			}
			return null;
		}

		public virtual void Reset(ActionList actionList)
		{
			isRunning = false;
		}

		protected string AssignString(List<ActionParameter> parameters, int _parameterID, string field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.String)
			{
				return parameterWithID.stringValue;
			}
			return field;
		}

		public BoolValue AssignBoolean(List<ActionParameter> parameters, int _parameterID, BoolValue field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.Boolean)
			{
				if (parameterWithID.intValue == 1)
				{
					return BoolValue.True;
				}
				return BoolValue.False;
			}
			return field;
		}

		public int AssignInteger(List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.Integer)
			{
				return parameterWithID.intValue;
			}
			return field;
		}

		public float AssignFloat(List<ActionParameter> parameters, int _parameterID, float field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.Float)
			{
				return parameterWithID.floatValue;
			}
			return field;
		}

		protected Vector3 AssignVector3(List<ActionParameter> parameters, int _parameterID, Vector3 field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.Vector3)
			{
				return parameterWithID.vector3Value;
			}
			return field;
		}

		protected int AssignVariableID(List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && (parameterWithID.parameterType == ParameterType.GlobalVariable || parameterWithID.parameterType == ParameterType.LocalVariable))
			{
				return parameterWithID.intValue;
			}
			return field;
		}

		protected GVar AssignVariable(List<ActionParameter> parameters, int _parameterID, GVar field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null)
			{
				return parameterWithID.GetVariable();
			}
			return field;
		}

		protected Variables AssignVariablesComponent(List<ActionParameter> parameters, int _parameterID, Variables field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.ComponentVariable)
			{
				return parameterWithID.variables;
			}
			return field;
		}

		protected int AssignInvItemID(List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.InventoryItem)
			{
				return parameterWithID.intValue;
			}
			return field;
		}

		protected int AssignDocumentID(List<ActionParameter> parameters, int _parameterID, int field)
		{
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.Document)
			{
				return parameterWithID.intValue;
			}
			return field;
		}

		public Transform AssignFile(List<ActionParameter> parameters, int _parameterID, int _constantID, Transform field)
		{
			Transform transform = field;
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.GameObject)
			{
				if (parameterWithID.intValue != 0)
				{
					ConstantID constantID = Serializer.returnConstantID(parameterWithID.intValue);
					if (constantID != null)
					{
						transform = constantID.gameObject.transform;
					}
				}
				if (transform == null)
				{
					if (parameterWithID.gameObject != null)
					{
						transform = parameterWithID.gameObject.transform;
					}
					else if (parameterWithID.intValue != 0)
					{
						ConstantID constantID2 = Serializer.returnConstantID(parameterWithID.intValue);
						if (constantID2 != null)
						{
							transform = constantID2.gameObject.transform;
						}
					}
				}
			}
			else if (parameterWithID != null && parameterWithID.parameterType == ParameterType.ComponentVariable)
			{
				if (parameterWithID.variables != null)
				{
					transform = parameterWithID.variables.transform;
				}
			}
			else if (_constantID != 0)
			{
				ConstantID constantID3 = Serializer.returnConstantID(_constantID);
				if (constantID3 != null)
				{
					transform = constantID3.gameObject.transform;
				}
			}
			return transform;
		}

		public Collider AssignFile(List<ActionParameter> parameters, int _parameterID, int _constantID, Collider field)
		{
			Collider collider = field;
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.GameObject)
			{
				collider = null;
				if (parameterWithID.intValue != 0)
				{
					collider = Serializer.returnComponent<Collider>(parameterWithID.intValue);
				}
				if (collider == null)
				{
					if (parameterWithID.gameObject != null && (bool)parameterWithID.gameObject.GetComponent<Collider>())
					{
						collider = parameterWithID.gameObject.GetComponent<Collider>();
					}
					else if (parameterWithID.intValue != 0)
					{
						collider = Serializer.returnComponent<Collider>(parameterWithID.intValue);
					}
				}
			}
			else if (parameterWithID != null && parameterWithID.parameterType == ParameterType.ComponentVariable)
			{
				if (parameterWithID.variables != null)
				{
					collider = parameterWithID.variables.GetComponent<Collider>();
				}
			}
			else if (_constantID != 0)
			{
				Collider collider2 = Serializer.returnComponent<Collider>(_constantID);
				if (collider2 != null)
				{
					collider = collider2;
				}
			}
			return collider;
		}

		protected GameObject AssignFile(List<ActionParameter> parameters, int _parameterID, int _constantID, GameObject field)
		{
			GameObject gameObject = field;
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.GameObject)
			{
				gameObject = null;
				if (parameterWithID.intValue != 0)
				{
					ConstantID constantID = Serializer.returnConstantID(parameterWithID.intValue);
					if (constantID != null)
					{
						gameObject = constantID.gameObject;
					}
				}
				if (gameObject == null)
				{
					if (parameterWithID.gameObject != null)
					{
						gameObject = parameterWithID.gameObject;
					}
					else if (parameterWithID.intValue != 0)
					{
						ConstantID constantID2 = Serializer.returnConstantID(parameterWithID.intValue);
						if (constantID2 != null)
						{
							gameObject = constantID2.gameObject;
						}
					}
				}
			}
			else if (parameterWithID != null && parameterWithID.parameterType == ParameterType.ComponentVariable)
			{
				if (parameterWithID.variables != null)
				{
					gameObject = parameterWithID.variables.gameObject;
				}
			}
			else if (_constantID != 0)
			{
				ConstantID constantID3 = Serializer.returnConstantID(_constantID);
				if (constantID3 != null)
				{
					gameObject = constantID3.gameObject;
				}
			}
			return gameObject;
		}

		protected UnityEngine.Object AssignObject<T>(List<ActionParameter> parameters, int _parameterID, UnityEngine.Object field) where T : UnityEngine.Object
		{
			UnityEngine.Object result = field;
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.UnityObject)
			{
				result = null;
				if (parameterWithID.objectValue != null)
				{
					if (parameterWithID.objectValue is T)
					{
						result = parameterWithID.objectValue;
					}
					else
					{
						ACDebug.LogWarning(string.Concat("Cannot convert ", parameterWithID.objectValue.name, " to type '", typeof(T), "'"));
					}
				}
			}
			return result;
		}

		public T AssignFile<T>(List<ActionParameter> parameters, int _parameterID, int _constantID, T field, bool doLog = true) where T : Behaviour
		{
			T val = field;
			ActionParameter parameterWithID = GetParameterWithID(parameters, _parameterID);
			if (parameterWithID != null && parameterWithID.parameterType == ParameterType.GameObject)
			{
				val = (T)null;
				if (parameterWithID.intValue != 0)
				{
					val = Serializer.returnComponent<T>(parameterWithID.intValue);
					if (val == null && parameterWithID.gameObject != null && parameterWithID.intValue != -1 && doLog)
					{
						ACDebug.LogWarning(string.Concat("No ", typeof(T), " component attached to ", parameterWithID.gameObject, "!"), parameterWithID.gameObject);
					}
				}
				if (val == null)
				{
					if (parameterWithID.gameObject != null && (bool)parameterWithID.gameObject.GetComponent<T>())
					{
						val = parameterWithID.gameObject.GetComponent<T>();
					}
					else if (parameterWithID.intValue != 0)
					{
						val = Serializer.returnComponent<T>(parameterWithID.intValue);
					}
					else if (parameterWithID.gameObject != null && parameterWithID.gameObject.GetComponent<T>() == null && doLog)
					{
						ACDebug.LogWarning(string.Concat("No ", typeof(T), " component attached to ", parameterWithID.gameObject, "!"), parameterWithID.gameObject);
					}
				}
			}
			else if (parameterWithID != null && parameterWithID.parameterType == ParameterType.ComponentVariable)
			{
				if (parameterWithID.variables != null)
				{
					val = parameterWithID.variables.GetComponent<T>();
				}
			}
			else if (_constantID != 0)
			{
				T val2 = Serializer.returnComponent<T>(_constantID);
				if (val2 != null)
				{
					val = val2;
				}
			}
			return val;
		}

		public T AssignFile<T>(int _constantID, T field) where T : Behaviour
		{
			if (_constantID != 0)
			{
				T val = Serializer.returnComponent<T>(_constantID);
				if (val != null)
				{
					return val;
				}
			}
			return field;
		}

		protected GameObject AssignFile(int _constantID, GameObject field)
		{
			if (_constantID != 0)
			{
				ConstantID constantID = Serializer.returnConstantID(_constantID);
				if (constantID != null)
				{
					return constantID.gameObject;
				}
			}
			return field;
		}

		public Transform AssignFile(int _constantID, Transform field)
		{
			if (_constantID != 0)
			{
				ConstantID constantID = Serializer.returnConstantID(_constantID);
				if (constantID != null)
				{
					return constantID.transform;
				}
			}
			return field;
		}

		protected ActionEnd GenerateActionEnd(ResultAction _resultAction, ActionListAsset _linkedAsset, Cutscene _linkedCutscene, int _skipAction, Action _skipActionActual, List<Action> _actions)
		{
			ActionEnd actionEnd = new ActionEnd();
			actionEnd.resultAction = _resultAction;
			actionEnd.linkedAsset = _linkedAsset;
			actionEnd.linkedCutscene = _linkedCutscene;
			switch (_resultAction)
			{
			case ResultAction.RunCutscene:
				if (isAssetFile && _linkedAsset != null)
				{
					actionEnd.linkedAsset = _linkedAsset;
				}
				else if (!isAssetFile && _linkedCutscene != null)
				{
					actionEnd.linkedCutscene = _linkedCutscene;
				}
				break;
			case ResultAction.Skip:
			{
				int num = _skipAction;
				if ((bool)_skipActionActual && _actions.Contains(_skipActionActual))
				{
					num = _actions.IndexOf(_skipActionActual);
				}
				else if (num == -1)
				{
					num = 0;
				}
				actionEnd.skipAction = num;
				break;
			}
			}
			return actionEnd;
		}

		protected ActionEnd GenerateStopActionEnd()
		{
			ActionEnd actionEnd = new ActionEnd();
			actionEnd.resultAction = ResultAction.Stop;
			return actionEnd;
		}

		public virtual void SetLastResult(ActionEnd _actionEnd)
		{
			lastResult = _actionEnd;
		}

		public void SetOutput(ActionEnd actionEnd)
		{
			endAction = actionEnd.resultAction;
			skipAction = actionEnd.skipAction;
			skipActionActual = actionEnd.skipActionActual;
			linkedCutscene = actionEnd.linkedCutscene;
			linkedAsset = actionEnd.linkedAsset;
		}
	}
}
