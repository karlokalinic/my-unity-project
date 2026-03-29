using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SceneColorControl_Anim : MonoBehaviour
{
	public Material skyMaterial;

	public Color skyColor = new Color(0.15f, 0.3f, 0.5f, 1f);

	public Color horizonColor = new Color(0.7f, 0.85f, 1f, 1f);

	public Color groundColor = new Color(0.4f, 0.35f, 0.3f, 1f);

	public float skyIntensity = 1.1f;

	public float skyFocus = 0.2f;

	public float horizonColorBanding = 0.25f;

	public bool customFogColor;

	public Color fogColor = new Color(0.7f, 0.85f, 1f, 1f);

	private void Start()
	{
		skyMaterial = RenderSettings.skybox;
	}

	private void Update()
	{
		UpdateColors();
	}

	private void OnValidate()
	{
		UpdateColors();
	}

	private void UpdateColors()
	{
		if (skyMaterial == null)
		{
			skyMaterial = RenderSettings.skybox;
		}
		skyMaterial.SetColor("_SkyColor", skyColor);
		skyMaterial.SetColor("_HorizonColor", horizonColor);
		skyMaterial.SetColor("_GroundColor", groundColor);
		skyMaterial.SetFloat("_SkyIntensity", skyIntensity);
		skyMaterial.SetFloat("_SunSkyFocus", skyFocus);
		skyMaterial.SetFloat("_HorizonBand", horizonColorBanding);
		Color color = (skyColor * 1.4f + horizonColor * 0.8f + groundColor * 0.8f) * 0.33f;
		if (RenderSettings.ambientMode == AmbientMode.Flat)
		{
			RenderSettings.ambientSkyColor = color * skyIntensity;
		}
		else
		{
			RenderSettings.ambientSkyColor = (skyColor * 1.5f + horizonColor * 0.5f + color) * 0.33f * skyIntensity;
			RenderSettings.ambientEquatorColor = (horizonColor + skyColor + groundColor + color) * 0.25f * skyIntensity;
			RenderSettings.ambientGroundColor = (groundColor + horizonColor + color) * 0.33f * skyIntensity;
		}
		if (customFogColor)
		{
			RenderSettings.fogColor = fogColor;
			return;
		}
		fogColor = (color + horizonColor + groundColor) * 0.33f * skyIntensity;
		RenderSettings.fogColor = fogColor;
	}
}
