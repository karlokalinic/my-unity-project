using System.Collections;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Misc/Sprite fader")]
	[RequireComponent(typeof(SpriteRenderer))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_sprite_fader.html")]
	public class SpriteFader : MonoBehaviour
	{
		public bool affectChildren;

		[HideInInspector]
		public bool isFading = true;

		[HideInInspector]
		public float fadeStartTime;

		[HideInInspector]
		public float fadeTime;

		[HideInInspector]
		public FadeType fadeType;

		protected SpriteRenderer spriteRenderer;

		protected SpriteRenderer[] childSprites;

		protected void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			if (affectChildren)
			{
				childSprites = GetComponentsInChildren<SpriteRenderer>();
			}
		}

		public void SetAlpha(float _alpha)
		{
			if (affectChildren && childSprites != null)
			{
				SpriteRenderer[] array = childSprites;
				foreach (SpriteRenderer spriteRenderer in array)
				{
					SetSpriteAlpha(spriteRenderer, _alpha);
				}
			}
			else
			{
				SetSpriteAlpha(this.spriteRenderer, _alpha);
			}
		}

		public float GetAlpha()
		{
			return spriteRenderer.color.a;
		}

		public void Fade(FadeType _fadeType, float _fadeTime, float startAlpha = -1f)
		{
			StopCoroutine("DoFade");
			float num = GetAlpha();
			if (startAlpha >= 0f)
			{
				num = startAlpha;
				SetAlpha(startAlpha);
			}
			else if (!spriteRenderer.enabled)
			{
				SetEnabledState(true);
				if (_fadeType == FadeType.fadeIn)
				{
					num = 0f;
					SetAlpha(0f);
				}
			}
			if (_fadeType == FadeType.fadeOut)
			{
				fadeStartTime = Time.time - num * _fadeTime;
			}
			else
			{
				fadeStartTime = Time.time - (1f - num) * _fadeTime;
			}
			fadeTime = _fadeTime;
			fadeType = _fadeType;
			if (fadeTime > 0f)
			{
				StartCoroutine("DoFade");
			}
			else
			{
				EndFade();
			}
		}

		public void EndFade()
		{
			StopCoroutine("DoFade");
			isFading = false;
			if (fadeType == FadeType.fadeIn)
			{
				SetAlpha(1f);
			}
			else
			{
				SetAlpha(0f);
			}
		}

		protected void SetSpriteAlpha(SpriteRenderer _spriteRenderer, float alpha)
		{
			Color color = _spriteRenderer.color;
			color.a = alpha;
			_spriteRenderer.color = color;
		}

		protected void SetEnabledState(bool value)
		{
			this.spriteRenderer.enabled = value;
			if (affectChildren && childSprites != null)
			{
				SpriteRenderer[] array = childSprites;
				foreach (SpriteRenderer spriteRenderer in array)
				{
					spriteRenderer.enabled = value;
				}
			}
		}

		protected IEnumerator DoFade()
		{
			SetEnabledState(true);
			isFading = true;
			float alpha = GetAlpha();
			if (fadeType == FadeType.fadeIn)
			{
				while (alpha < 1f)
				{
					alpha = -1f + AdvGame.Interpolate(fadeStartTime, fadeTime, MoveMethod.Linear);
					SetAlpha(alpha);
					yield return new WaitForFixedUpdate();
				}
				SetAlpha(1f);
			}
			else
			{
				while (alpha > 0f)
				{
					alpha = 2f - AdvGame.Interpolate(fadeStartTime, fadeTime, MoveMethod.Linear);
					SetAlpha(alpha);
					yield return new WaitForFixedUpdate();
				}
				SetAlpha(0f);
			}
			isFading = false;
		}
	}
}
