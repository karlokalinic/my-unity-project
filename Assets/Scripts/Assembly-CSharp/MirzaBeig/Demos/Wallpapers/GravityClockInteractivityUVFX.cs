using UnityEngine;

namespace MirzaBeig.Demos.Wallpapers
{
	public class GravityClockInteractivityUVFX : MonoBehaviour
	{
		public GameObject forceAffectors;

		public GameObject forceAffectors2;

		public ParticleSystem gravityClockPrefab;

		private ParticleSystem gravityClock;

		public bool enableGravityClockVisualEffects = true;

		public bool enableGravityClockAttractionForce = true;

		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void SetGravityClockVisualEffectsActive(bool value)
		{
			if (value)
			{
				if (enableGravityClockVisualEffects)
				{
					gravityClock = Object.Instantiate(gravityClockPrefab, base.transform);
					gravityClock.transform.localPosition = Vector3.zero;
				}
			}
			else if ((bool)gravityClock)
			{
				gravityClock.Stop();
				gravityClock.transform.SetParent(null, true);
			}
		}

		public void SetGravityClockAttractionForceActive(bool value)
		{
			if (value)
			{
				if (enableGravityClockAttractionForce)
				{
					forceAffectors.gameObject.SetActive(true);
					forceAffectors2.gameObject.SetActive(true);
				}
			}
			else
			{
				forceAffectors.gameObject.SetActive(false);
				forceAffectors2.gameObject.SetActive(false);
			}
		}
	}
}
