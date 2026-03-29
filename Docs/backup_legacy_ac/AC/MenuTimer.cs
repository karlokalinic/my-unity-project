using UnityEngine;
using UnityEngine.UI;

namespace AC
{
	public class MenuTimer : MenuElement
	{
		public Slider uiSlider;

		public bool doInvert;

		public Texture2D timerTexture;

		public AC_TimerType timerType;

		public UISelectableHideStyle uiSelectableHideStyle;

		public float smoothingFactor;

		private LerpUtils.FloatLerp progressSmoothing = new LerpUtils.FloatLerp();

		private float progress;

		private Rect timerRect;

		public override void Declare()
		{
			uiSlider = null;
			doInvert = false;
			isVisible = true;
			isClickable = false;
			timerType = AC_TimerType.Conversation;
			numSlots = 1;
			SetSize(new Vector2(20f, 5f));
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			smoothingFactor = 0f;
			base.Declare();
		}

		public override MenuElement DuplicateSelf(bool fromEditor, bool ignoreUnityUI)
		{
			MenuTimer menuTimer = ScriptableObject.CreateInstance<MenuTimer>();
			menuTimer.Declare();
			menuTimer.CopyTimer(this, ignoreUnityUI);
			return menuTimer;
		}

		private void CopyTimer(MenuTimer _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlider = null;
			}
			else
			{
				uiSlider = _element.uiSlider;
			}
			doInvert = _element.doInvert;
			timerTexture = _element.timerTexture;
			timerType = _element.timerType;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			smoothingFactor = _element.smoothingFactor;
			base.Copy(_element);
		}

		public override void LoadUnityUI(Menu _menu, Canvas canvas)
		{
			uiSlider = LinkUIElement<Slider>(canvas);
			if ((bool)uiSlider)
			{
				uiSlider.minValue = 0f;
				uiSlider.maxValue = 1f;
				uiSlider.wholeNumbers = false;
				uiSlider.value = 1f;
				uiSlider.interactable = false;
			}
		}

		public override RectTransform GetRectTransform(int _slot)
		{
			if ((bool)uiSlider)
			{
				return uiSlider.GetComponent<RectTransform>();
			}
			return null;
		}

		public override void OnMenuTurnOn(Menu menu)
		{
			progress = -1f;
			progressSmoothing.Reset();
			base.OnMenuTurnOn(menu);
		}

		public override void PreDisplay(int _slot, int languageNumber, bool isActive)
		{
			if (Application.isPlaying)
			{
				float newTargetValue = GetProgress();
				if (progress < 0f || smoothingFactor <= 0f)
				{
					progress = newTargetValue;
				}
				else
				{
					float newMoveSpeed = -9.5f * smoothingFactor + 10f;
					progress = progressSmoothing.Update(progress, newTargetValue, newMoveSpeed);
				}
				if (doInvert)
				{
					progress = 1f - progress;
				}
				if ((bool)uiSlider)
				{
					uiSlider.value = progress;
					UpdateUISelectable(uiSlider, uiSelectableHideStyle);
				}
				else
				{
					timerRect = relativeRect;
					timerRect.width *= progress;
				}
			}
			else
			{
				timerRect = relativeRect;
			}
		}

		public override void Display(GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if ((bool)timerTexture)
			{
				GUI.DrawTexture(ZoomRect(timerRect, zoom), timerTexture, ScaleMode.StretchToFill, true, 0f);
			}
			base.Display(_style, _slot, zoom, isActive);
		}

		private float GetProgress()
		{
			switch (timerType)
			{
			case AC_TimerType.Conversation:
				if ((bool)KickStarter.playerInput.activeConversation && KickStarter.playerInput.activeConversation.isTimed)
				{
					return KickStarter.playerInput.activeConversation.GetTimeRemaining();
				}
				return 0f;
			case AC_TimerType.QuickTimeEventProgress:
				if (KickStarter.playerQTE.QTEIsActive())
				{
					return KickStarter.playerQTE.GetProgress();
				}
				return 0f;
			case AC_TimerType.QuickTimeEventRemaining:
				if (KickStarter.playerQTE.QTEIsActive())
				{
					return KickStarter.playerQTE.GetRemainingTimeFactor();
				}
				return 0f;
			case AC_TimerType.LoadingProgress:
				return KickStarter.sceneChanger.GetLoadingProgress();
			default:
				return 0f;
			}
		}
	}
}
