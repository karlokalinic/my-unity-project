using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCameraShake : Action
	{
		public int shakeIntensity;

		public int shakeIntensityParameterID = -1;

		public float duration = 1f;

		public int durationParameterID = -1;

		public CameraShakeEffect cameraShakeEffect = CameraShakeEffect.TranslateAndRotate;

		public ActionCameraShake()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Shake";
			description = "Causes the camera to shake, giving an earthquake screen effect. The method of shaking, i.e. moving or rotating, depends on the type of camera the Main Camera is linked to.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			shakeIntensity = AssignInteger(parameters, shakeIntensityParameterID, shakeIntensity);
			duration = AssignFloat(parameters, durationParameterID, duration);
			if (duration < 0f)
			{
				duration = 0f;
			}
		}

		public override float Run()
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			if ((bool)mainCamera)
			{
				if (isRunning)
				{
					isRunning = false;
					return 0f;
				}
				isRunning = true;
				DoShake(mainCamera, shakeIntensity, duration);
				if (willWait)
				{
					return duration;
				}
			}
			return 0f;
		}

		public override void Skip()
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			if ((bool)mainCamera)
			{
				DoShake(mainCamera, 0f, 0f);
			}
		}

		protected void DoShake(MainCamera mainCam, float _intensity, float _duration)
		{
			if (mainCam.attachedCamera is GameCamera)
			{
				mainCam.Shake(_intensity / 67f, _duration, cameraShakeEffect);
			}
			else if (mainCam.attachedCamera is GameCamera25D)
			{
				mainCam.Shake(_intensity / 67f, _duration, cameraShakeEffect);
				GameCamera25D gameCamera25D = (GameCamera25D)mainCam.attachedCamera;
				if ((bool)gameCamera25D.backgroundImage)
				{
					gameCamera25D.backgroundImage.Shake(_intensity / 0.67f, _duration);
				}
			}
			else if (mainCam.attachedCamera is GameCamera2D)
			{
				mainCam.Shake(_intensity / 33f, _duration, cameraShakeEffect);
			}
			else
			{
				mainCam.Shake(_intensity / 67f, _duration, cameraShakeEffect);
			}
		}

		public static ActionCameraShake CreateNew(int intensity = 1, float duration = 1f, bool waitUntilFinish = true)
		{
			ActionCameraShake actionCameraShake = ScriptableObject.CreateInstance<ActionCameraShake>();
			actionCameraShake.shakeIntensity = intensity;
			actionCameraShake.duration = duration;
			actionCameraShake.willWait = waitUntilFinish;
			return actionCameraShake;
		}
	}
}
