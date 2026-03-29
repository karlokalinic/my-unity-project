using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCamera : Action
	{
		public int constantID;

		public int parameterID = -1;

		public _Camera linkedCamera;

		protected _Camera runtimeLinkedCamera;

		public float transitionTime;

		public int transitionTimeParameterID = -1;

		public AnimationCurve timeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		public MoveMethod moveMethod;

		public bool returnToLast;

		public bool retainPreviousSpeed;

		public ActionCamera()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Switch";
			description = "Moves the MainCamera to the position, rotation and field of view of a specified GameCamera. Can be instantaneous or transition over time.";
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
				MainCamera mainCamera = KickStarter.mainCamera;
				if ((bool)mainCamera)
				{
					_Camera lastGameplayCamera = runtimeLinkedCamera;
					if (returnToLast)
					{
						lastGameplayCamera = mainCamera.GetLastGameplayCamera();
					}
					if ((bool)lastGameplayCamera && mainCamera.attachedCamera != lastGameplayCamera)
					{
						if (lastGameplayCamera is GameCameraThirdPerson)
						{
							GameCameraThirdPerson gameCameraThirdPerson = (GameCameraThirdPerson)lastGameplayCamera;
							gameCameraThirdPerson.ResetRotation();
						}
						else if (lastGameplayCamera is GameCameraAnimated)
						{
							GameCameraAnimated gameCameraAnimated = (GameCameraAnimated)lastGameplayCamera;
							gameCameraAnimated.PlayClip();
						}
						if (transitionTime > 0f && runtimeLinkedCamera is GameCamera25D)
						{
							mainCamera.SetGameCamera(lastGameplayCamera);
							LogWarning("Switching to a 2.5D camera (" + runtimeLinkedCamera.name + ") must be instantaneous.");
						}
						else
						{
							mainCamera.SetGameCamera(lastGameplayCamera, transitionTime, moveMethod, timeCurve, retainPreviousSpeed);
							if (willWait)
							{
								if (transitionTime > 0f)
								{
									return transitionTime;
								}
								if (runtimeLinkedCamera is GameCameraAnimated)
								{
									return base.defaultPauseTime;
								}
							}
						}
					}
				}
				return 0f;
			}
			if (runtimeLinkedCamera is GameCameraAnimated && willWait)
			{
				GameCameraAnimated gameCameraAnimated2 = (GameCameraAnimated)runtimeLinkedCamera;
				if (gameCameraAnimated2.isPlaying())
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
				return 0f;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			if (!mainCamera)
			{
				return;
			}
			_Camera lastGameplayCamera = runtimeLinkedCamera;
			if (returnToLast)
			{
				lastGameplayCamera = mainCamera.GetLastGameplayCamera();
			}
			if ((bool)lastGameplayCamera)
			{
				if (lastGameplayCamera is GameCameraThirdPerson)
				{
					GameCameraThirdPerson gameCameraThirdPerson = (GameCameraThirdPerson)lastGameplayCamera;
					gameCameraThirdPerson.ResetRotation();
				}
				lastGameplayCamera.MoveCameraInstant();
				mainCamera.SetGameCamera(lastGameplayCamera);
			}
		}

		public static ActionCamera CreateNew(_Camera newCamera, float duration = 0f, bool waitUntilFinish = true, MoveMethod moveMethod = MoveMethod.Smooth)
		{
			ActionCamera actionCamera = ScriptableObject.CreateInstance<ActionCamera>();
			actionCamera.linkedCamera = newCamera;
			actionCamera.transitionTime = duration;
			actionCamera.moveMethod = moveMethod;
			actionCamera.willWait = waitUntilFinish;
			return actionCamera;
		}
	}
}
