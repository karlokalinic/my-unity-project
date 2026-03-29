using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSceneCheckAttribute : ActionCheck
	{
		public int attributeID;

		public int attributeNumber;

		public int intValue;

		public float floatValue;

		public IntCondition intCondition;

		public bool isAdditive;

		public BoolValue boolValue = BoolValue.True;

		public BoolCondition boolCondition;

		public string stringValue;

		public bool checkCase = true;

		protected SceneSettings sceneSettings;

		public ActionSceneCheckAttribute()
		{
			isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Check attribute";
			description = "Queries the value of a scene attribute declared in the Scene Manager.";
		}

		public override void AssignParentList(ActionList actionList)
		{
			if (sceneSettings == null)
			{
				sceneSettings = KickStarter.sceneSettings;
			}
			base.AssignParentList(actionList);
		}

		public override ActionEnd End(List<Action> actions)
		{
			if (attributeID == -1)
			{
				return GenerateStopActionEnd();
			}
			InvVar attribute = sceneSettings.GetAttribute(attributeID);
			if (attribute != null)
			{
				return ProcessResult(CheckCondition(attribute), actions);
			}
			LogWarning("Cannot find the scene attribute with an ID of " + attributeID);
			return GenerateStopActionEnd();
		}

		protected bool CheckCondition(InvVar attribute)
		{
			if (attribute == null)
			{
				LogWarning("Cannot check state of attribute since it cannot be found!");
				return false;
			}
			switch (attribute.type)
			{
			case VariableType.Boolean:
			{
				int val2 = attribute.val;
				int num3 = (int)boolValue;
				if (boolCondition == BoolCondition.EqualTo)
				{
					if (val2 == num3)
					{
						return true;
					}
				}
				else if (val2 != num3)
				{
					return true;
				}
				break;
			}
			case VariableType.Integer:
			case VariableType.PopUp:
			{
				int val = attribute.val;
				int num2 = intValue;
				if (intCondition == IntCondition.EqualTo)
				{
					if (val == num2)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (val != num2)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (val < num2)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan && val > num2)
				{
					return true;
				}
				break;
			}
			case VariableType.Float:
			{
				float floatVal = attribute.floatVal;
				float num = floatValue;
				if (intCondition == IntCondition.EqualTo)
				{
					if (Mathf.Approximately(floatVal, num))
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (!Mathf.Approximately(floatVal, num))
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (floatVal < num)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan && floatVal > num)
				{
					return true;
				}
				break;
			}
			case VariableType.String:
			{
				string text = attribute.textVal;
				string text2 = AdvGame.ConvertTokens(stringValue);
				if (!checkCase)
				{
					text = text.ToLower();
					text2 = text2.ToLower();
				}
				if (boolCondition == BoolCondition.EqualTo)
				{
					if (text == text2)
					{
						return true;
					}
				}
				else if (text != text2)
				{
					return true;
				}
				break;
			}
			}
			return false;
		}

		protected int GetVarNumber(List<InvVar> attributes, int ID)
		{
			int num = 0;
			foreach (InvVar attribute in attributes)
			{
				if (attribute.id == ID)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		public static ActionSceneCheckAttribute CreateNew(int attributeID, bool value)
		{
			ActionSceneCheckAttribute actionSceneCheckAttribute = ScriptableObject.CreateInstance<ActionSceneCheckAttribute>();
			actionSceneCheckAttribute.attributeID = attributeID;
			actionSceneCheckAttribute.boolValue = (value ? BoolValue.True : BoolValue.False);
			return actionSceneCheckAttribute;
		}

		public static ActionSceneCheckAttribute CreateNew(int attributeID, int value, IntCondition condition = IntCondition.EqualTo)
		{
			ActionSceneCheckAttribute actionSceneCheckAttribute = ScriptableObject.CreateInstance<ActionSceneCheckAttribute>();
			actionSceneCheckAttribute.attributeID = attributeID;
			actionSceneCheckAttribute.intValue = value;
			actionSceneCheckAttribute.intCondition = condition;
			return actionSceneCheckAttribute;
		}

		public static ActionSceneCheckAttribute CreateNew(int attributeID, float value, IntCondition condition = IntCondition.EqualTo)
		{
			ActionSceneCheckAttribute actionSceneCheckAttribute = ScriptableObject.CreateInstance<ActionSceneCheckAttribute>();
			actionSceneCheckAttribute.attributeID = attributeID;
			actionSceneCheckAttribute.floatValue = value;
			actionSceneCheckAttribute.intCondition = condition;
			return actionSceneCheckAttribute;
		}

		public static ActionSceneCheckAttribute CreateNew(int attributeID, string value, bool isCaseSensitive = false)
		{
			ActionSceneCheckAttribute actionSceneCheckAttribute = ScriptableObject.CreateInstance<ActionSceneCheckAttribute>();
			actionSceneCheckAttribute.attributeID = attributeID;
			actionSceneCheckAttribute.stringValue = value;
			actionSceneCheckAttribute.checkCase = isCaseSensitive;
			return actionSceneCheckAttribute;
		}
	}
}
