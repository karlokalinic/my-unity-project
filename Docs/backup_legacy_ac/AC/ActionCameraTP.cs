using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCameraTP : Action
	{
		public int constantID;

		public int parameterID = -1;

		public GameCameraThirdPerson linkedCamera;

		protected GameCameraThirdPerson runtimeLinkedCamera;

		public float transitionTime;

		public int transitionTimeParameterID = -1;

		public bool controlPitch;

		public bool controlSpin;

		public bool isRelativeToTarget;

		public float newPitchAngle;

		public float newSpinAngle;

		public AnimationCurve timeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		public MoveMethod moveMethod;

		public ActionCameraTP()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Rotate third-person";
			description = "Rotates a Game Camera Third-person to face a certain direction, either fixed or relative to its target.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeLinkedCamera = AssignFile(parameters, parameterID, constantID, linkedCamera);
			transitionTime = AssignFloat(parameters, transitionTimeParameterID, transitionTime);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (DoRotation(transitionTime) && transitionTime > 0f && willWait)
				{
					return transitionTime;
				}
				return 0f;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			DoRotation(0f);
		}

		protected bool DoRotation(float _transitionTime)
		{
			if (runtimeLinkedCamera != null && (controlPitch || controlSpin))
			{
				float num = newPitchAngle;
				float num2 = newSpinAngle;
				if (controlSpin)
				{
					num2 = ((!isRelativeToTarget) ? (num2 + 180f) : (num2 + runtimeLinkedCamera.target.localEulerAngles.y));
				}
				if (num2 > 360f)
				{
					num2 -= 360f;
				}
				if (_transitionTime > 0f)
				{
					runtimeLinkedCamera.ForceRotation(controlPitch, num, controlSpin, num2, _transitionTime, moveMethod, timeCurve);
				}
				else
				{
					runtimeLinkedCamera.ForceRotation(controlPitch, num, controlSpin, num2);
				}
				return true;
			}
			return false;
		}

		public static ActionCameraTP CreateNew(float newPitchAngle, float newSpinAngle, bool spinAngleIsRelativeToTarget = false, float transitionTime = 1f, bool waitUntilFinish = false)
		{
			ActionCameraTP actionCameraTP = ScriptableObject.CreateInstance<ActionCameraTP>();
			actionCameraTP.controlPitch = true;
			actionCameraTP.newPitchAngle = newPitchAngle;
			actionCameraTP.controlSpin = true;
			actionCameraTP.newSpinAngle = newSpinAngle;
			actionCameraTP.isRelativeToTarget = spinAngleIsRelativeToTarget;
			actionCameraTP.transitionTime = transitionTime;
			actionCameraTP.willWait = waitUntilFinish;
			actionCameraTP.moveMethod = MoveMethod.Smooth;
			return actionCameraTP;
		}
	}
}
