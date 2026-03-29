using System.Collections.Generic;
using UnityEngine;

namespace DFTGames.Tools
{
	[AddComponentMenu("Camera/Fade obstructors by volume")]
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(Rigidbody))]
	public class FadeObstructorsVolumetric : FadeObstructorsBaseClass
	{
		public static bool commonVolume = true;

		public static float commonRadius = 0.05f;

		public float capsuleRadius = 0.05f;

		public float _commonRadius = 0.05f;

		public bool _commonVolume = true;

		private CapsuleCollider capsuleVolume;

		private bool createVolume;

		public override void Start()
		{
			base.Start();
			commonVolume = _commonVolume;
			commonRadius = _commonRadius;
			createVolume = !commonVolume || GetComponent<CapsuleCollider>() == null;
			if (createVolume)
			{
				capsuleVolume = base.gameObject.AddComponent<CapsuleCollider>();
				capsuleVolume.direction = 2;
				capsuleVolume.isTrigger = true;
				capsuleVolume.radius = ((!commonVolume) ? capsuleRadius : _commonRadius);
			}
			Rigidbody component = GetComponent<Rigidbody>();
			component.isKinematic = true;
		}

		public void FixedUpdate()
		{
			if (!(playerTransform == null) && createVolume)
			{
				capsuleVolume.height = (playerTransform.position - myTransform.position).magnitude + offset;
				capsuleVolume.center = new Vector3(0f, 0f, capsuleVolume.height * 0.5f);
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			int num = 1 << other.gameObject.layer;
			if ((other.isTrigger && ignoreTriggers) || other.CompareTag(playerTag) || ((int)layersToFade & num) != num)
			{
				return;
			}
			Renderer[] componentsInChildren = other.gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i] != null) || modifiedShaders.ContainsKey(componentsInChildren[i].GetInstanceID()))
				{
					continue;
				}
				ShaderData shaderData = new ShaderData();
				FadingManager component = componentsInChildren[i].gameObject.GetComponent<FadingManager>();
				if (component != null)
				{
					component.GoAway();
				}
				shaderData.renderer = componentsInChildren[i];
				shaderData.materials = componentsInChildren[i].materials;
				Material[] materials = componentsInChildren[i].materials;
				shaderData.color = new Color[componentsInChildren[i].materials.Length];
				for (int j = 0; j < materials.Length; j++)
				{
					shaderData.color[j] = materials[j].color;
					materials[j] = new Material(transparentMaterial);
					materials[j].color = fadingColorToUse;
					if (replicateTexture)
					{
						materials[j].mainTexture = componentsInChildren[i].materials[j].mainTexture;
					}
					else
					{
						materials[j].mainTexture = null;
					}
				}
				componentsInChildren[i].materials = materials;
				modifiedShaders.Add(componentsInChildren[i].GetInstanceID(), shaderData);
				component = componentsInChildren[i].gameObject.AddComponent<FadingManager>();
				component.fadingTime = fadingTime;
				component.fadingAmount = transparenceValue;
			}
		}

		public void OnTriggerExit(Collider other)
		{
			Renderer[] componentsInChildren = other.gameObject.GetComponentsInChildren<Renderer>();
			List<int> list = new List<int>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				int instanceID = componentsInChildren[i].GetInstanceID();
				if (modifiedShaders.ContainsKey(instanceID))
				{
					list.Add(instanceID);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				ShaderData shaderData = modifiedShaders[list[j]];
				modifiedShaders.Remove(list[j]);
				for (int k = 0; k < shaderData.materials.Length; k++)
				{
					FadingManager component = shaderData.renderer.gameObject.GetComponent<FadingManager>();
					if (component != null)
					{
						component.GoAway();
					}
					component = shaderData.renderer.gameObject.AddComponent<FadingManager>();
					component.fadingTime = fadingTime;
					component.fadingAmount = transparenceValue;
					component.fadeOut = false;
					component.matIdx = k;
					component.oldMat = shaderData.materials[k];
					component.oldColor = shaderData.color[k];
				}
			}
			Resources.UnloadUnusedAssets();
		}
	}
}
