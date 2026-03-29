using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Hotspots/Highlight")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_highlight.html")]
	public class Highlight : MonoBehaviour
	{
		public bool highlightWhenSelected = true;

		public bool brightenMaterials = true;

		public bool affectChildren = true;

		public float maxHighlight = 2f;

		public float fadeTime = 0.3f;

		public float flashHoldTime;

		public bool callEvents;

		public UnityEvent onHighlightOn;

		public UnityEvent onHighlightOff;

		protected float minHighlight = 1f;

		protected float highlight = 1f;

		protected int direction = 1;

		protected float fadeStartTime;

		protected HighlightState highlightState;

		protected List<Color> originalColors = new List<Color>();

		protected Renderer _renderer;

		protected Renderer[] childRenderers;

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

		protected void Awake()
		{
			_renderer = GetComponent<Renderer>();
			if (_renderer != null)
			{
				Material[] materials = _renderer.materials;
				foreach (Material material in materials)
				{
					if (material.HasProperty("_Color"))
					{
						originalColors.Add(material.color);
					}
				}
			}
			childRenderers = GetComponentsInChildren<Renderer>();
			Renderer[] array = childRenderers;
			foreach (Renderer renderer in array)
			{
				Material[] materials2 = renderer.materials;
				foreach (Material material2 in materials2)
				{
					if (material2.HasProperty("_Color"))
					{
						originalColors.Add(material2.color);
					}
				}
			}
		}

		public float GetHighlightIntensity()
		{
			return (highlight - 1f) / maxHighlight;
		}

		public float GetHighlightAlpha()
		{
			return highlight - 1f;
		}

		public void SetMinHighlight(float _minHighlight)
		{
			minHighlight = _minHighlight + 1f;
			if (minHighlight < 1f)
			{
				minHighlight = 1f;
			}
			else if (minHighlight > maxHighlight)
			{
				minHighlight = maxHighlight;
			}
		}

		public void HighlightOn()
		{
			if (highlightState != HighlightState.On && (highlightState != HighlightState.Normal || direction != 1))
			{
				highlightState = HighlightState.Normal;
				direction = 1;
				fadeStartTime = Time.time;
				if (highlight > minHighlight)
				{
					fadeStartTime -= (highlight - minHighlight) / (maxHighlight - minHighlight) * fadeTime;
				}
				else
				{
					highlight = minHighlight;
				}
				if (callEvents && onHighlightOn != null)
				{
					onHighlightOn.Invoke();
				}
			}
		}

		public void HighlightOnInstant()
		{
			highlightState = HighlightState.On;
			highlight = maxHighlight;
			UpdateMaterials();
			if (callEvents && onHighlightOn != null)
			{
				onHighlightOn.Invoke();
			}
		}

		public void HighlightOff()
		{
			highlightState = HighlightState.Normal;
			direction = -1;
			fadeStartTime = Time.time;
			if (highlight < maxHighlight)
			{
				fadeStartTime -= (maxHighlight - highlight) / (maxHighlight - minHighlight) * fadeTime;
			}
			else
			{
				highlight = maxHighlight;
			}
			if (callEvents && onHighlightOff != null)
			{
				onHighlightOff.Invoke();
			}
		}

		public void HighlightOffInstant()
		{
			minHighlight = 1f;
			highlightState = HighlightState.None;
			highlight = minHighlight;
			UpdateMaterials();
			if (callEvents && onHighlightOff != null)
			{
				onHighlightOff.Invoke();
			}
		}

		public void Flash()
		{
			if (highlightState != HighlightState.Flash && (highlightState == HighlightState.None || direction == -1))
			{
				highlightState = HighlightState.Flash;
				highlight = minHighlight;
				direction = 1;
				fadeStartTime = Time.time;
			}
		}

		public float GetFlashTime()
		{
			return fadeTime * 2f;
		}

		public void CancelFlash()
		{
			if (direction >= 0 && highlightState == HighlightState.Flash)
			{
				direction = 0;
				fadeStartTime = 0f;
			}
		}

		public float GetFlashAlpha(float original)
		{
			if (highlightState == HighlightState.Flash)
			{
				return highlight - 1f;
			}
			return Mathf.Lerp(original, 0f, Time.deltaTime * 5f);
		}

		public float GetFadeTime()
		{
			return fadeTime;
		}

		public void Pulse()
		{
			highlightState = HighlightState.Pulse;
			highlight = minHighlight;
			direction = 1;
			fadeStartTime = Time.time;
		}

		public void _Update()
		{
			if (highlightState != HighlightState.None)
			{
				if (direction > 0)
				{
					highlight = Mathf.Lerp(minHighlight, maxHighlight, AdvGame.Interpolate(fadeStartTime, fadeTime, MoveMethod.Linear));
					if (highlight >= maxHighlight)
					{
						highlight = maxHighlight;
						switch (highlightState)
						{
						case HighlightState.Flash:
							direction = 0;
							fadeStartTime = flashHoldTime;
							break;
						case HighlightState.Pulse:
							direction = -1;
							fadeStartTime = Time.time;
							break;
						default:
							highlightState = HighlightState.On;
							break;
						}
					}
				}
				else if (direction < 0)
				{
					highlight = Mathf.Lerp(maxHighlight, minHighlight, AdvGame.Interpolate(fadeStartTime, fadeTime, MoveMethod.Linear));
					if (highlight <= 1f)
					{
						highlight = 1f;
						if (highlightState == HighlightState.Pulse)
						{
							direction = 1;
							fadeStartTime = Time.time;
						}
						else
						{
							highlightState = HighlightState.None;
						}
					}
				}
				else
				{
					fadeStartTime -= Time.deltaTime;
					if (fadeStartTime <= 0f)
					{
						direction = -1;
						highlight = maxHighlight;
						fadeStartTime = Time.time;
					}
				}
				UpdateMaterials();
			}
			else if (!Mathf.Approximately(highlight, minHighlight))
			{
				highlight = minHighlight;
				UpdateMaterials();
			}
		}

		protected void UpdateMaterials()
		{
			if (!brightenMaterials)
			{
				return;
			}
			int num = 0;
			if ((bool)_renderer)
			{
				Material[] materials = _renderer.materials;
				foreach (Material material in materials)
				{
					if (material.HasProperty("_Color"))
					{
						float a = material.color.a;
						Color color = originalColors[num] * highlight;
						color.a = a;
						material.color = color;
						num++;
					}
				}
			}
			if (!affectChildren)
			{
				return;
			}
			Renderer[] array = childRenderers;
			foreach (Renderer renderer in array)
			{
				Material[] materials2 = renderer.materials;
				foreach (Material material2 in materials2)
				{
					if (originalColors.Count <= num)
					{
						break;
					}
					if (material2.HasProperty("_Color"))
					{
						float a = material2.color.a;
						Color color2 = originalColors[num] * highlight;
						color2.a = a;
						material2.color = color2;
						num++;
					}
				}
			}
		}
	}
}
