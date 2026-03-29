using System.Collections.Generic;
using UnityEngine;

namespace DFTGames.Tools
{
	[AddComponentMenu("Camera/Fade obstructors by raycast")]
	[RequireComponent(typeof(Camera))]
	public class FadeObstructors : FadeObstructorsBaseClass
	{
		public bool useSpherecast;

		public float spherecastRadius = 0.5f;

		private RaycastHit[] hit;

		private void FixedUpdate()
		{
			if (playerTransform == null)
			{
				return;
			}
			if (!useSpherecast)
			{
				hit = Physics.RaycastAll(myTransform.position, myTransform.forward, (playerTransform.position - myTransform.position).magnitude + offset, layersToFade, ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
			}
			else
			{
				hit = Physics.SphereCastAll(myTransform.position, spherecastRadius, myTransform.forward, (playerTransform.position - myTransform.position).magnitude + offset, layersToFade, ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
			}
			Debug.DrawLine(myTransform.position, playerTransform.position + myTransform.forward * offset, fadingColorToUse, Time.fixedDeltaTime);
			List<int> list = new List<int>();
			if (hit != null)
			{
				for (int i = 0; i < hit.Length; i++)
				{
					if ((hit[i].collider.isTrigger && ignoreTriggers) || hit[i].collider.CompareTag(playerTag))
					{
						continue;
					}
					Renderer[] componentsInChildren = hit[i].collider.gameObject.GetComponentsInChildren<Renderer>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						if (!(componentsInChildren[j] != null))
						{
							continue;
						}
						list.Add(componentsInChildren[j].GetInstanceID());
						if (modifiedShaders.ContainsKey(componentsInChildren[j].GetInstanceID()))
						{
							continue;
						}
						ShaderData shaderData = new ShaderData();
						FadingManager component = componentsInChildren[j].gameObject.GetComponent<FadingManager>();
						if (component != null)
						{
							component.GoAway();
						}
						shaderData.renderer = componentsInChildren[j];
						shaderData.materials = componentsInChildren[j].materials;
						Material[] materials = componentsInChildren[j].materials;
						shaderData.color = new Color[componentsInChildren[j].materials.Length];
						for (int k = 0; k < materials.Length; k++)
						{
							shaderData.color[k] = materials[k].color;
							materials[k] = transparentMaterial;
							materials[k].color = fadingColorToUse;
							if (replicateTexture)
							{
								materials[k].mainTexture = componentsInChildren[j].materials[k].mainTexture;
							}
							else
							{
								materials[k].mainTexture = null;
							}
						}
						componentsInChildren[j].materials = materials;
						modifiedShaders.Add(componentsInChildren[j].GetInstanceID(), shaderData);
						component = componentsInChildren[j].gameObject.AddComponent<FadingManager>();
						component.fadingTime = fadingTime;
						component.fadingAmount = transparenceValue;
					}
				}
			}
			List<int> list2 = new List<int>();
			foreach (KeyValuePair<int, ShaderData> modifiedShader in modifiedShaders)
			{
				if (!list.Contains(modifiedShader.Key))
				{
					list2.Add(modifiedShader.Key);
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				ShaderData shaderData2 = modifiedShaders[list2[l]];
				modifiedShaders.Remove(list2[l]);
				for (int m = 0; m < shaderData2.materials.Length; m++)
				{
					FadingManager component2 = shaderData2.renderer.gameObject.GetComponent<FadingManager>();
					if (component2 != null)
					{
						component2.GoAway();
					}
					component2 = shaderData2.renderer.gameObject.AddComponent<FadingManager>();
					component2.fadingTime = fadingTime;
					component2.fadingAmount = transparenceValue;
					component2.fadeOut = false;
					component2.matIdx = m;
					component2.oldMat = shaderData2.materials[m];
					component2.oldColor = shaderData2.color[m];
				}
			}
			Resources.UnloadUnusedAssets();
		}
	}
}
