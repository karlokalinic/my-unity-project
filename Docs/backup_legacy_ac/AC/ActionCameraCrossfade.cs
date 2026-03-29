using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCameraCrossfade : Action
	{
		public int parameterID = -1;

		public int constantID;

		public _Camera linkedCamera;

		protected _Camera runtimeLinkedCamera;

		public float transitionTime;

		public int transitionTimeParameterID = -1;

		public bool returnToLast;

		public ActionCameraCrossfade()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Crossfade";
			description = "Crossfades the camera from its current GameCamera to a new one, over a specified time.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeLinkedCamera = AssignFile(parameters, parameterID, constantID, linkedCamera);
			transitionTime = AssignFloat(parameters, transitionTimeParameterID, transitionTime);
			if (returnToLast)
			{
				runtimeLinkedCamera = KickStarter.mainCamera.GetLastGameplayCamera();
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				MainCamera mainCamera = KickStarter.mainCamera;
				if (runtimeLinkedCamera != null && mainCamera.attachedCamera != runtimeLinkedCamera)
				{
					if (runtimeLinkedCamera is GameCameraThirdPerson)
					{
						GameCameraThirdPerson gameCameraThirdPerson = (GameCameraThirdPerson)runtimeLinkedCamera;
						gameCameraThirdPerson.ResetRotation();
					}
					else if (runtimeLinkedCamera is GameCameraAnimated)
					{
						GameCameraAnimated gameCameraAnimated = (GameCameraAnimated)runtimeLinkedCamera;
						gameCameraAnimated.PlayClip();
					}
					runtimeLinkedCamera.MoveCameraInstant();
					mainCamera.Crossfade(transitionTime, runtimeLinkedCamera);
					if (transitionTime > 0f && willWait)
					{
						return transitionTime;
					}
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			if (runtimeLinkedCamera != null && mainCamera.attachedCamera != runtimeLinkedCamera)
			{
				if (runtimeLinkedCamera is GameCameraThirdPerson)
				{
					GameCameraThirdPerson gameCameraThirdPerson = (GameCameraThirdPerson)runtimeLinkedCamera;
					gameCameraThirdPerson.ResetRotation();
				}
				else if (runtimeLinkedCamera is GameCameraAnimated)
				{
					GameCameraAnimated gameCameraAnimated = (GameCameraAnimated)runtimeLinkedCamera;
					gameCameraAnimated.PlayClip();
				}
				runtimeLinkedCamera.MoveCameraInstant();
				mainCamera.SetGameCamera(runtimeLinkedCamera);
			}
		}

		public static ActionCameraCrossfade CreateNew(_Camera newCamera, float transitionTime = 1f, bool waitUntilFinish = false)
		{
			ActionCameraCrossfade actionCameraCrossfade = ScriptableObject.CreateInstance<ActionCameraCrossfade>();
			actionCameraCrossfade.linkedCamera = newCamera;
			actionCameraCrossfade.transitionTime = transitionTime;
			actionCameraCrossfade.willWait = waitUntilFinish;
			return actionCameraCrossfade;
		}
	}
}
