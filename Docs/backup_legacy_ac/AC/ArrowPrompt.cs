using System.Collections;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_arrow_prompt.html")]
	public class ArrowPrompt : MonoBehaviour
	{
		public ActionListSource source;

		public ArrowPromptType arrowPromptType = ArrowPromptType.KeyAndClick;

		public Arrow upArrow;

		public Arrow downArrow;

		public Arrow leftArrow;

		public Arrow rightArrow;

		public bool disableHotspots = true;

		public float positionFactor = 1f;

		public float scaleFactor = 1f;

		protected bool isOn;

		protected AC_Direction directionToAnimate;

		protected float alpha;

		protected float arrowSize = 0.05f;

		protected float LargeSize
		{
			get
			{
				return arrowSize * 2f * scaleFactor;
			}
		}

		protected float SmallSize
		{
			get
			{
				return arrowSize * scaleFactor;
			}
		}

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public void DrawArrows()
		{
			if (!(alpha > 0f))
			{
				return;
			}
			if (directionToAnimate != AC_Direction.None)
			{
				SetGUIAlpha(alpha);
				switch (directionToAnimate)
				{
				case AC_Direction.Up:
					upArrow.rect = GetUpRect(arrowSize);
					break;
				case AC_Direction.Down:
					downArrow.rect = GetDownRect(arrowSize);
					break;
				case AC_Direction.Left:
					leftArrow.rect = GetLeftRect(arrowSize);
					break;
				case AC_Direction.Right:
					rightArrow.rect = GetRightRect(arrowSize);
					break;
				}
			}
			else
			{
				SetGUIAlpha(alpha);
				if (upArrow.isPresent)
				{
					upArrow.rect = GetUpRect();
				}
				if (downArrow.isPresent)
				{
					downArrow.rect = GetDownRect();
				}
				if (leftArrow.isPresent)
				{
					leftArrow.rect = GetLeftRect();
				}
				if (rightArrow.isPresent)
				{
					rightArrow.rect = GetRightRect();
				}
			}
			upArrow.Draw();
			downArrow.Draw();
			leftArrow.Draw();
			rightArrow.Draw();
		}

		public void TurnOn()
		{
			if (upArrow.isPresent || downArrow.isPresent || leftArrow.isPresent || rightArrow.isPresent)
			{
				if ((bool)KickStarter.playerInput)
				{
					KickStarter.playerInput.activeArrows = this;
				}
				StartCoroutine("FadeIn");
				directionToAnimate = AC_Direction.None;
				arrowSize = 0.05f;
			}
		}

		public void TurnOff()
		{
			Disable();
			StopCoroutine("FadeIn");
			alpha = 0f;
		}

		public void DoUp()
		{
			if (upArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine(FadeOut(AC_Direction.Up));
				Disable();
				upArrow.Run(source);
			}
		}

		public void DoDown()
		{
			if (downArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine(FadeOut(AC_Direction.Down));
				Disable();
				downArrow.Run(source);
			}
		}

		public void DoLeft()
		{
			if (leftArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine(FadeOut(AC_Direction.Left));
				Disable();
				leftArrow.Run(source);
			}
		}

		public void DoRight()
		{
			if (rightArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine(FadeOut(AC_Direction.Right));
				Disable();
				rightArrow.Run(source);
			}
		}

		protected Rect GetUpRect(float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect(AdvGame.GUIRect(0.5f, 0.1f * positionFactor, scale * 2f * scaleFactor, scale * scaleFactor));
		}

		protected Rect GetDownRect(float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect(AdvGame.GUIRect(0.5f, 1f - 0.1f * positionFactor, scale * 2f * scaleFactor, scale * scaleFactor));
		}

		protected Rect GetLeftRect(float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect(AdvGame.GUIRect(0.05f * positionFactor * 2f, 0.5f, scale * scaleFactor, scale * 2f * scaleFactor));
		}

		protected Rect GetRightRect(float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect(AdvGame.GUIRect(1f - 0.05f * positionFactor * 2f, 0.5f, scale * scaleFactor, scale * 2f * scaleFactor));
		}

		protected void Disable()
		{
			if ((bool)KickStarter.playerInput)
			{
				KickStarter.playerInput.activeArrows = null;
			}
			isOn = false;
		}

		protected IEnumerator FadeIn()
		{
			alpha = 0f;
			if (alpha < 1f)
			{
				while (alpha < 0.95f)
				{
					alpha += 0.05f;
					alpha = Mathf.Clamp01(alpha);
					yield return new WaitForFixedUpdate();
				}
				alpha = 1f;
				isOn = true;
			}
		}

		protected IEnumerator FadeOut(AC_Direction direction)
		{
			arrowSize = 0.05f;
			alpha = 1f;
			directionToAnimate = direction;
			if (alpha > 0f)
			{
				while (alpha > 0.05f)
				{
					arrowSize += 0.005f;
					alpha -= 0.05f;
					alpha = Mathf.Clamp01(alpha);
					yield return new WaitForFixedUpdate();
				}
				alpha = 0f;
			}
		}

		protected void SetGUIAlpha(float alpha)
		{
			Color color = GUI.color;
			color.a = alpha;
			GUI.color = color;
		}
	}
}
