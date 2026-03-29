using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInputCheck : ActionCheck
	{
		public string inputName;

		public int parameterID = -1;

		public InputCheckType checkType;

		public IntCondition axisCondition;

		public float axisValue;

		public ActionInputCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Input;
			title = "Check";
			description = "Queries whether or not the player is invoking a button or axis declared in Unity's Input manager.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			inputName = AssignString(parameters, parameterID, inputName);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				return base.defaultPauseTime / 6f;
			}
			isRunning = false;
			return 0f;
		}

		public override bool CheckCondition()
		{
			switch (checkType)
			{
			case InputCheckType.SingleTapOrClick:
				return KickStarter.playerInput.ClickedRecently();
			case InputCheckType.DoubleTapOrClick:
				return KickStarter.playerInput.ClickedRecently(true);
			case InputCheckType.Button:
				if (inputName != string.Empty && KickStarter.playerInput.InputGetButton(inputName))
				{
					return true;
				}
				break;
			case InputCheckType.Axis:
				if (inputName != string.Empty)
				{
					return CheckAxisValue(KickStarter.playerInput.InputGetAxis(inputName));
				}
				break;
			}
			return false;
		}

		protected bool CheckAxisValue(float fieldValue)
		{
			if (axisCondition == IntCondition.EqualTo)
			{
				if (Mathf.Approximately(fieldValue, axisValue))
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.NotEqualTo)
			{
				if (!Mathf.Approximately(fieldValue, axisValue))
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.LessThan)
			{
				if (fieldValue < axisValue)
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.MoreThan && fieldValue > axisValue)
			{
				return true;
			}
			return false;
		}

		public static ActionInputCheck CreateNew_Button(string buttonName)
		{
			ActionInputCheck actionInputCheck = ScriptableObject.CreateInstance<ActionInputCheck>();
			actionInputCheck.checkType = InputCheckType.Button;
			actionInputCheck.inputName = buttonName;
			return actionInputCheck;
		}

		public static ActionInputCheck CreateNew_Axis(string axisName, float axisValue = 0.2f, IntCondition condition = IntCondition.MoreThan)
		{
			ActionInputCheck actionInputCheck = ScriptableObject.CreateInstance<ActionInputCheck>();
			actionInputCheck.checkType = InputCheckType.Axis;
			actionInputCheck.inputName = axisName;
			actionInputCheck.axisValue = axisValue;
			actionInputCheck.axisCondition = condition;
			return actionInputCheck;
		}

		public static ActionInputCheck CreateNew_TapOrClick(bool requireDoubleClick = false)
		{
			ActionInputCheck actionInputCheck = ScriptableObject.CreateInstance<ActionInputCheck>();
			actionInputCheck.checkType = ((!requireDoubleClick) ? InputCheckType.SingleTapOrClick : InputCheckType.DoubleTapOrClick);
			return actionInputCheck;
		}
	}
}
