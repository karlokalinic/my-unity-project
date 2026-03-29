using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObjects : MonoBehaviour
{
	private enum CastType
	{
		ray = 0,
		sphere = 1,
		capsule = 2
	}

	[SerializeField]
	private CastType castType;

	[SerializeField]
	private GameObject player;

	[SerializeField]
	private bool includeTriggers;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float radius = 1f;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float capsuleHeight = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float maxTransparency = 0.5f;

	[SerializeField]
	[Range(0.01f, 5f)]
	private float fadeSpeed = 1f;

	[SerializeField]
	[Range(0f, 5f)]
	private float depthTolerance = 0.5f;

	private QueryTriggerInteraction triggerSetting;

	private RaycastHit[] castHits;

	private List<RaycastHit> activehits = new List<RaycastHit>();

	private float rayDistance;

	private void Update()
	{
		rayDistance = Vector3.Distance(player.transform.position, base.transform.position);
		if (includeTriggers)
		{
			triggerSetting = QueryTriggerInteraction.Collide;
		}
		else
		{
			triggerSetting = QueryTriggerInteraction.Ignore;
		}
		if (castType == CastType.ray)
		{
			castHits = Physics.RaycastAll(base.transform.position, Vector3.Normalize(player.transform.position - base.transform.position), rayDistance, -5, triggerSetting);
		}
		else if (castType == CastType.sphere)
		{
			castHits = Physics.SphereCastAll(base.transform.position, radius, Vector3.Normalize(player.transform.position - base.transform.position), rayDistance, -5, triggerSetting);
		}
		else if (castType == CastType.capsule)
		{
			castHits = Physics.CapsuleCastAll(new Vector3(base.transform.position.x, base.transform.position.y - capsuleHeight * 0.5f, base.transform.position.z), new Vector3(base.transform.position.x, base.transform.position.y + capsuleHeight * 0.5f, base.transform.position.z), radius, Vector3.Normalize(player.transform.position - base.transform.position), rayDistance, -5, QueryTriggerInteraction.Collide);
		}
		for (int i = 0; i < castHits.Length; i++)
		{
			RaycastHit raycastHit = castHits[i];
			float num = Vector3.Distance(base.transform.position, raycastHit.transform.position);
			if (num + depthTolerance > rayDistance)
			{
				continue;
			}
			Renderer component = raycastHit.transform.GetComponent<Renderer>();
			bool flag = false;
			Transform parent = raycastHit.transform.parent;
			if (raycastHit.transform.tag == "CanFade")
			{
				flag = true;
			}
			else
			{
				while (parent != null)
				{
					if (raycastHit.transform.parent.tag == "CanFade")
					{
						flag = true;
					}
					parent = parent.parent;
				}
			}
			if (!component || !flag)
			{
				continue;
			}
			bool flag2 = true;
			foreach (RaycastHit activehit in activehits)
			{
				if (activehit.transform == raycastHit.transform)
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				activehits.Add(raycastHit);
				StartCoroutine(FadeMaterial(component, raycastHit));
			}
		}
	}

	private IEnumerator FadeMaterial(Renderer rend, RaycastHit hit)
	{
		string colorPropertyName = "_Color";
		Material[] sharedMaterials = rend.sharedMaterials;
		Material[] fadeMaterials = new Material[sharedMaterials.Length];
		float[] startAlphas = new float[sharedMaterials.Length];
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if ((bool)hit.transform.GetComponent<FadeObjectOverride>())
			{
				if ((bool)hit.transform.GetComponent<FadeObjectOverride>().fadeShader)
				{
					fadeMaterials[i] = new Material(hit.transform.GetComponent<FadeObjectOverride>().fadeShader);
				}
				else
				{
					fadeMaterials[i] = new Material(sharedMaterials[i].shader);
				}
				fadeMaterials[i].CopyPropertiesFromMaterial(sharedMaterials[i]);
				colorPropertyName = hit.transform.GetComponent<FadeObjectOverride>().alphaPropertyName;
				if (fadeMaterials[i].GetFloat("_Mode") == 0f || fadeMaterials[i].GetFloat("_Mode") == 1f)
				{
					SetMaterialProperties(fadeMaterials[i], 2);
				}
			}
			else
			{
				fadeMaterials[i] = new Material(sharedMaterials[i].shader);
				fadeMaterials[i].CopyPropertiesFromMaterial(sharedMaterials[i]);
				if (fadeMaterials[i].GetFloat("_Mode") == 0f || fadeMaterials[i].GetFloat("_Mode") == 1f)
				{
					SetMaterialProperties(fadeMaterials[i], 2);
				}
			}
			startAlphas[i] = fadeMaterials[i].GetColor(colorPropertyName).a;
		}
		rend.materials = fadeMaterials;
		float t = 0f;
		while (t < 1f / fadeSpeed)
		{
			t += Time.deltaTime;
			for (int j = 0; j < fadeMaterials.Length; j++)
			{
				fadeMaterials[j].SetColor(colorPropertyName, new Color(fadeMaterials[j].GetColor(colorPropertyName).r, fadeMaterials[j].GetColor(colorPropertyName).g, fadeMaterials[j].GetColor(colorPropertyName).b, Mathf.Clamp(fadeMaterials[j].GetColor(colorPropertyName).a - Time.deltaTime * fadeSpeed, maxTransparency, 1f)));
			}
			yield return null;
		}
		bool isBlocking = true;
		while (isBlocking)
		{
			isBlocking = false;
			RaycastHit[] array = castHits;
			foreach (RaycastHit raycastHit in array)
			{
				if (raycastHit.transform == hit.transform)
				{
					isBlocking = true;
				}
			}
			yield return null;
		}
		t = 0f;
		while (t < 1f / fadeSpeed)
		{
			t += Time.deltaTime;
			for (int l = 0; l < fadeMaterials.Length; l++)
			{
				fadeMaterials[l].SetColor(colorPropertyName, new Color(fadeMaterials[l].GetColor(colorPropertyName).r, fadeMaterials[l].GetColor(colorPropertyName).g, fadeMaterials[l].GetColor(colorPropertyName).b, Mathf.Clamp(fadeMaterials[l].GetColor(colorPropertyName).a + Time.deltaTime * fadeSpeed, maxTransparency, startAlphas[l])));
			}
			yield return null;
		}
		for (int m = 0; m < sharedMaterials.Length; m++)
		{
			UnityEngine.Object.Destroy(fadeMaterials[m]);
		}
		rend.materials = sharedMaterials;
		Array.Clear(sharedMaterials, 0, sharedMaterials.Length);
		Array.Clear(fadeMaterials, 0, fadeMaterials.Length);
		Array.Clear(startAlphas, 0, startAlphas.Length);
		activehits.Remove(hit);
	}

	private void SetMaterialProperties(Material material, int blendMode)
	{
		switch (blendMode)
		{
		case 0:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 0);
			material.SetInt("_ZWrite", 1);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = -1;
			break;
		case 1:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 0);
			material.SetInt("_ZWrite", 1);
			material.EnableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 2450;
			break;
		case 2:
			material.SetInt("_SrcBlend", 5);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		case 3:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		}
	}
}
