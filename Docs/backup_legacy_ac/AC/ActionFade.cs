using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionFade : Action
	{
		public FadeType fadeType;

		public bool isInstant;

		public float fadeSpeed = 0.5f;

		public int fadeSpeedParameterID = -1;

		public bool setTexture;

		public Texture2D tempTexture;

		public int tempTextureParameterID = -1;

		public bool forceCompleteTransition = true;

		public ActionFade()
		{
			isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Fade";
			description = "Fades the camera in or out. The fade speed can be adjusted, as can the overlay texture – this is black by default.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			tempTexture = (Texture2D)AssignObject<Texture2D>(parameters, tempTextureParameterID, tempTexture);
			fadeSpeed = AssignFloat(parameters, fadeSpeedParameterID, fadeSpeed);
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				MainCamera mainCamera = KickStarter.mainCamera;
				RunSelf(mainCamera, fadeSpeed);
				if (willWait && !isInstant)
				{
					return fadeSpeed;
				}
				return 0f;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			RunSelf(KickStarter.mainCamera, 0f);
		}

		protected void RunSelf(MainCamera mainCam, float _time)
		{
			if (mainCam == null)
			{
				return;
			}
			mainCam.StopCrossfade();
			if (fadeType == FadeType.fadeIn)
			{
				if (isInstant)
				{
					mainCam.FadeIn(0f);
				}
				else
				{
					mainCam.FadeIn(_time, forceCompleteTransition);
				}
				return;
			}
			Texture2D tempTex = tempTexture;
			if (!setTexture)
			{
				tempTex = null;
			}
			float fadeDuration = _time;
			if (isInstant)
			{
				fadeDuration = 0f;
			}
			mainCam.FadeOut(fadeDuration, tempTex, forceCompleteTransition);
		}

		public static ActionFade CreateNew(FadeType fadeType, float transitionTime = 1f, bool waitUntilFinish = true)
		{
			ActionFade actionFade = ScriptableObject.CreateInstance<ActionFade>();
			actionFade.fadeType = fadeType;
			actionFade.fadeSpeed = transitionTime;
			actionFade.isInstant = transitionTime < 0f;
			actionFade.willWait = waitUntilFinish;
			return actionFade;
		}
	}
}
