using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RFX1_Reflection : MonoBehaviour
{
	public RenderTexture tex;

	private ReflectionProbe reflectionProbe;

	private List<Light> dirLight;

	private List<float> lightIntencity;

	private void Awake()
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
		dirLight = new List<Light>();
		lightIntencity = new List<float>();
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			if (light.type == LightType.Directional)
			{
				dirLight.Add(light);
				lightIntencity.Add(light.intensity);
			}
		}
		reflectionProbe = GetComponent<ReflectionProbe>();
		tex = new RenderTexture(reflectionProbe.resolution, reflectionProbe.resolution, 0);
		tex.dimension = TextureDimension.Cube;
		tex.useMipMap = true;
		Shader.SetGlobalTexture("RFX1_Reflection", tex);
		reflectionProbe.RenderProbe(tex);
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < dirLight.Count; i++)
		{
			if (Math.Abs(dirLight[i].intensity - lightIntencity[i]) > 0.001f)
			{
				flag = true;
				lightIntencity[i] = dirLight[i].intensity;
			}
		}
		if (flag)
		{
			reflectionProbe.RenderProbe(tex);
		}
	}
}
