using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionQTE : ActionCheck
	{
		public enum QTEType
		{
			SingleKeypress = 0,
			HoldKey = 1,
			ButtonMash = 2,
			SingleAxis = 3,
			ThumbstickRotation = 4
		}

		public QTEType qteType;

		public int menuNameParameterID = -1;

		public string menuName;

		public bool animateUI;

		public bool wrongKeyFails;

		public float axisThreshold = 0.2f;

		public int inputNameParameterID = -1;

		public string inputName;

		public int durationParameterID = -1;

		public float duration;

		public float holdDuration;

		public float cooldownTime;

		public int targetPresses;

		public bool doCooldown;

		public string verticalInputName;

		public int verticalInputNameParameterID = -1;

		public bool rotationIsClockwise = true;

		public float targetRotations = 1f;

		public int targetRotationsParameterID = -1;

		public ActionQTE()
		{
			isDisplayed = true;
			category = ActionCategory.Input;
			title = "QTE";
			description = "Initiates a Quick Time Event for a set duration. The QTE type can either be a single key- press, holding a button down, or button-mashing. The Input button must be defined in Unity's Input Manager.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			menuName = AssignString(parameters, menuNameParameterID, menuName);
			inputName = AssignString(parameters, inputNameParameterID, inputName);
			duration = AssignFloat(parameters, durationParameterID, duration);
		}

		public override float Run()
		{
			if (string.IsNullOrEmpty(inputName) && (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen || qteType == QTEType.SingleAxis))
			{
				isRunning = false;
				return 0f;
			}
			if (duration <= 0f)
			{
				isRunning = false;
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				Animator animator = null;
				if (!string.IsNullOrEmpty(menuName))
				{
					Menu menuWithName = PlayerMenus.GetMenuWithName(menuName);
					if (menuWithName != null)
					{
						menuWithName.TurnOn();
						if (animateUI && menuWithName.RuntimeCanvas != null && (bool)menuWithName.RuntimeCanvas.GetComponent<Animator>())
						{
							animator = menuWithName.RuntimeCanvas.GetComponent<Animator>();
						}
					}
				}
				switch (qteType)
				{
				case QTEType.SingleKeypress:
					KickStarter.playerQTE.StartSinglePressQTE(inputName, duration, animator, wrongKeyFails);
					break;
				case QTEType.SingleAxis:
					KickStarter.playerQTE.StartSingleAxisQTE(inputName, duration, axisThreshold, animator, wrongKeyFails);
					break;
				case QTEType.HoldKey:
					KickStarter.playerQTE.StartHoldKeyQTE(inputName, duration, holdDuration, animator, wrongKeyFails);
					break;
				case QTEType.ButtonMash:
					KickStarter.playerQTE.StartButtonMashQTE(inputName, duration, targetPresses, doCooldown, cooldownTime, animator, wrongKeyFails);
					break;
				case QTEType.ThumbstickRotation:
					KickStarter.playerQTE.StartThumbstickRotationQTE(inputName, verticalInputName, duration, targetRotations, rotationIsClockwise, animator, wrongKeyFails);
					break;
				}
				return base.defaultPauseTime;
			}
			if (KickStarter.playerQTE.GetState() == QTEState.None)
			{
				return base.defaultPauseTime;
			}
			if (!string.IsNullOrEmpty(menuName))
			{
				Menu menuWithName2 = PlayerMenus.GetMenuWithName(menuName);
				if (menuWithName2 != null)
				{
					menuWithName2.TurnOff();
				}
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			KickStarter.playerQTE.SkipQTE();
			if (!string.IsNullOrEmpty(menuName))
			{
				PlayerMenus.GetMenuWithName(menuName).TurnOff();
			}
		}

		public override bool CheckCondition()
		{
			if (KickStarter.playerQTE.GetState() == QTEState.Win)
			{
				return true;
			}
			return false;
		}

		public static ActionQTE CreateNew_SingleKeypress(string inputButtonName, float duration, bool wrongButtonFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE actionQTE = ScriptableObject.CreateInstance<ActionQTE>();
			actionQTE.qteType = QTEType.SingleKeypress;
			actionQTE.inputName = inputButtonName;
			actionQTE.duration = duration;
			actionQTE.wrongKeyFails = wrongButtonFails;
			actionQTE.menuName = menuToDisplay;
			actionQTE.animateUI = animateUI;
			return actionQTE;
		}

		public static ActionQTE CreateNew_SingleAxis(string inputAxisName, float duration, float axisThreshold = 0.2f, bool wrongDirectionFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE actionQTE = ScriptableObject.CreateInstance<ActionQTE>();
			actionQTE.qteType = QTEType.SingleAxis;
			actionQTE.inputName = inputAxisName;
			actionQTE.duration = duration;
			actionQTE.axisThreshold = axisThreshold;
			actionQTE.wrongKeyFails = wrongDirectionFails;
			actionQTE.menuName = menuToDisplay;
			actionQTE.animateUI = animateUI;
			return actionQTE;
		}

		public static ActionQTE CreateNew_HoldKey(string inputButtonName, float duration, float requiredDuration, bool wrongButtonFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE actionQTE = ScriptableObject.CreateInstance<ActionQTE>();
			actionQTE.qteType = QTEType.HoldKey;
			actionQTE.inputName = inputButtonName;
			actionQTE.duration = duration;
			actionQTE.holdDuration = requiredDuration;
			actionQTE.wrongKeyFails = wrongButtonFails;
			actionQTE.menuName = menuToDisplay;
			actionQTE.animateUI = animateUI;
			return actionQTE;
		}

		public static ActionQTE CreateNew_ButtonMash(string inputButtonName, float duration, int requiredPresses, float cooldownTime = -1f, bool wrongButtonFails = false, string menuToDisplay = "", bool animateUI = false)
		{
			ActionQTE actionQTE = ScriptableObject.CreateInstance<ActionQTE>();
			actionQTE.qteType = QTEType.ButtonMash;
			actionQTE.inputName = inputButtonName;
			actionQTE.duration = duration;
			actionQTE.targetPresses = requiredPresses;
			actionQTE.cooldownTime = cooldownTime;
			actionQTE.doCooldown = cooldownTime >= 0f;
			actionQTE.wrongKeyFails = wrongButtonFails;
			actionQTE.menuName = menuToDisplay;
			actionQTE.animateUI = animateUI;
			return actionQTE;
		}
	}
}
