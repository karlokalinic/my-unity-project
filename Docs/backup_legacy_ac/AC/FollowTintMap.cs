using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Characters/Follow TintMap")]
	[RequireComponent(typeof(SpriteRenderer))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_follow_tint_map.html")]
	public class FollowTintMap : MonoBehaviour
	{
		public bool useDefaultTintMap = true;

		public TintMap tintMap;

		public float intensity = 1f;

		public bool affectChildren;

		protected TintMap actualTintMap;

		protected SpriteRenderer _spriteRenderer;

		protected SpriteRenderer[] _spriteRenderers;

		protected float targetIntensity;

		protected float initialIntensity;

		protected float fadeStartTime;

		protected float fadeTime;

		protected void Awake()
		{
			if (!KickStarter.settingsManager || !KickStarter.settingsManager.IsInLoadingScene())
			{
				_spriteRenderer = GetComponent<SpriteRenderer>();
				_spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
				targetIntensity = (initialIntensity = intensity);
				ResetTintMap();
			}
		}

		protected void LateUpdate()
		{
			if (!actualTintMap)
			{
				return;
			}
			if (fadeTime > 0f)
			{
				intensity = Mathf.Lerp(initialIntensity, targetIntensity, AdvGame.Interpolate(fadeStartTime, fadeTime, MoveMethod.Linear));
				if (Time.time > fadeStartTime + fadeTime)
				{
					intensity = targetIntensity;
					fadeTime = (fadeStartTime = 0f);
				}
			}
			if (affectChildren)
			{
				for (int i = 0; i < _spriteRenderers.Length; i++)
				{
					_spriteRenderers[i].color = actualTintMap.GetColorData(base.transform.position, intensity, _spriteRenderers[i].color.a);
				}
			}
			else
			{
				_spriteRenderer.color = actualTintMap.GetColorData(base.transform.position, intensity, _spriteRenderer.color.a);
			}
		}

		public void AfterLoad()
		{
			ResetTintMap();
		}

		public void ResetTintMap()
		{
			actualTintMap = tintMap;
			if (useDefaultTintMap && (bool)KickStarter.sceneSettings)
			{
				if ((bool)KickStarter.sceneSettings.tintMap)
				{
					actualTintMap = KickStarter.sceneSettings.tintMap;
				}
				else
				{
					ACDebug.Log(base.gameObject.name + " cannot find Tint Map to follow!");
				}
			}
			if (affectChildren)
			{
				for (int i = 0; i < _spriteRenderers.Length; i++)
				{
					_spriteRenderers[i].color = new Color(1f, 1f, 1f, _spriteRenderers[i].color.a);
				}
			}
			else
			{
				_spriteRenderer.color = new Color(1f, 1f, 1f, _spriteRenderer.color.a);
			}
		}

		public void SetIntensity(float _targetIntensity, float _fadeTime = 0f)
		{
			targetIntensity = _targetIntensity;
			initialIntensity = intensity;
			if (_fadeTime <= 0f)
			{
				intensity = _targetIntensity;
				fadeStartTime = 0f;
				fadeTime = _fadeTime;
			}
			else
			{
				fadeStartTime = Time.time;
				fadeTime = _fadeTime;
			}
		}

		public VisibilityData SaveData(VisibilityData visibilityData)
		{
			visibilityData.useDefaultTintMap = useDefaultTintMap;
			visibilityData.tintIntensity = targetIntensity;
			visibilityData.tintMapID = 0;
			if (!useDefaultTintMap && tintMap != null && tintMap.gameObject != null)
			{
				visibilityData.tintMapID = Serializer.GetConstantID(tintMap.gameObject);
			}
			return visibilityData;
		}

		public void LoadData(VisibilityData data)
		{
			useDefaultTintMap = data.useDefaultTintMap;
			SetIntensity(data.tintIntensity);
			if (!useDefaultTintMap && data.tintMapID != 0)
			{
				tintMap = Serializer.returnComponent<TintMap>(data.tintMapID);
			}
			ResetTintMap();
		}
	}
}
