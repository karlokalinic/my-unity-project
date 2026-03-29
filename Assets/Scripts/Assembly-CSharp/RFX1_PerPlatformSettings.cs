using UnityEngine;

public class RFX1_PerPlatformSettings : MonoBehaviour
{
	public bool DisableOnMobiles;

	public bool RenderMobileDistortion;

	[Range(0.1f, 1f)]
	public float ParticleBudgetForMobiles = 1f;

	private bool isMobile;

	private void Awake()
	{
		isMobile = IsMobilePlatform();
		if (isMobile)
		{
			if (DisableOnMobiles)
			{
				base.gameObject.SetActive(false);
			}
			else if (ParticleBudgetForMobiles < 0.99f)
			{
				ChangeParticlesBudget(ParticleBudgetForMobiles);
			}
		}
	}

	private void OnEnable()
	{
		Camera main = Camera.main;
		LWRP_Rendering_Check(main);
		Legacy_Rendering_Check(main);
	}

	private void Update()
	{
		Camera main = Camera.main;
		LWRP_Rendering_Check(main);
		Legacy_Rendering_Check(main);
	}

	private void LWRP_Rendering_Check(Camera cam)
	{
	}

	private void Legacy_Rendering_Check(Camera cam)
	{
		if (!(cam == null) && RenderMobileDistortion && !DisableOnMobiles && isMobile)
		{
			RFX1_LegacyRenderDistortion rFX1_LegacyRenderDistortion = cam.GetComponent<RFX1_LegacyRenderDistortion>();
			if (rFX1_LegacyRenderDistortion == null)
			{
				rFX1_LegacyRenderDistortion = cam.gameObject.AddComponent<RFX1_LegacyRenderDistortion>();
			}
			rFX1_LegacyRenderDistortion.IsActive = true;
		}
	}

	private void OnDisable()
	{
		Camera main = Camera.main;
		if (!(main == null) && RenderMobileDistortion && !DisableOnMobiles && isMobile)
		{
			RFX1_LegacyRenderDistortion component = main.GetComponent<RFX1_LegacyRenderDistortion>();
			if (component != null)
			{
				component.IsActive = false;
			}
		}
	}

	private bool IsMobilePlatform()
	{
		bool result = false;
		if (Application.isMobilePlatform)
		{
			result = true;
		}
		return result;
	}

	private void ChangeParticlesBudget(float particlesMul)
	{
		ParticleSystem component = GetComponent<ParticleSystem>();
		if (component == null)
		{
			return;
		}
		ParticleSystem.MainModule main = component.main;
		main.maxParticles = Mathf.Max(1, (int)((float)main.maxParticles * particlesMul));
		ParticleSystem.EmissionModule emission = component.emission;
		if (!emission.enabled)
		{
			return;
		}
		ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
		if (rateOverTime.constantMin > 1f)
		{
			rateOverTime.constantMin *= particlesMul;
		}
		if (rateOverTime.constantMax > 1f)
		{
			rateOverTime.constantMax *= particlesMul;
		}
		emission.rateOverTime = rateOverTime;
		ParticleSystem.MinMaxCurve rateOverDistance = emission.rateOverDistance;
		if (rateOverDistance.constantMin > 1f)
		{
			if (rateOverDistance.constantMin > 1f)
			{
				rateOverDistance.constantMin *= particlesMul;
			}
			if (rateOverDistance.constantMax > 1f)
			{
				rateOverDistance.constantMax *= particlesMul;
			}
			emission.rateOverDistance = rateOverDistance;
		}
		ParticleSystem.Burst[] array = new ParticleSystem.Burst[emission.burstCount];
		emission.GetBursts(array);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].minCount > 1)
			{
				array[i].minCount = (short)((float)array[i].minCount * particlesMul);
			}
			if (array[i].maxCount > 1)
			{
				array[i].maxCount = (short)((float)array[i].maxCount * particlesMul);
			}
		}
		emission.SetBursts(array);
	}
}
