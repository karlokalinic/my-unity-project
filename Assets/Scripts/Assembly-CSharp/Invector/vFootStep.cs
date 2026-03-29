using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
	public class vFootStep : vFootPlantingPlayer
	{
		public AnimationType animationType;

		public bool debugTextureName;

		private int surfaceIndex;

		private Terrain terrain;

		private TerrainCollider terrainCollider;

		private TerrainData terrainData;

		private Vector3 terrainPos;

		public vFootStepTrigger leftFootTrigger;

		public vFootStepTrigger rightFootTrigger;

		public Transform currentStep;

		public List<vFootStepTrigger> footStepTriggers;

		private void Start()
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			if (animationType == AnimationType.Humanoid)
			{
				if (leftFootTrigger == null && rightFootTrigger == null)
				{
					Debug.Log("Missing FootStep Sphere Trigger, please unfold the FootStep Component to create the triggers.");
					return;
				}
				leftFootTrigger.trigger.isTrigger = true;
				rightFootTrigger.trigger.isTrigger = true;
				Physics.IgnoreCollision(leftFootTrigger.trigger, rightFootTrigger.trigger);
				foreach (Collider collider in componentsInChildren)
				{
					if (collider.enabled && collider.gameObject != leftFootTrigger.gameObject)
					{
						Physics.IgnoreCollision(leftFootTrigger.trigger, collider);
					}
					if (collider.enabled && collider.gameObject != rightFootTrigger.gameObject)
					{
						Physics.IgnoreCollision(rightFootTrigger.trigger, collider);
					}
				}
				return;
			}
			foreach (Collider collider2 in componentsInChildren)
			{
				for (int k = 0; k < footStepTriggers.Count; k++)
				{
					vFootStepTrigger vFootStepTrigger2 = footStepTriggers[k];
					vFootStepTrigger2.trigger.isTrigger = true;
					if (collider2.enabled && collider2.gameObject != vFootStepTrigger2.gameObject)
					{
						Physics.IgnoreCollision(vFootStepTrigger2.trigger, collider2);
					}
				}
			}
		}

		private void UpdateTerrainInfo(Terrain newTerrain)
		{
			if (terrain == null || terrain != newTerrain)
			{
				terrain = newTerrain;
				if (terrain != null)
				{
					terrainData = terrain.terrainData;
					terrainPos = terrain.transform.position;
					terrainCollider = terrain.GetComponent<TerrainCollider>();
				}
			}
		}

		private float[] GetTextureMix(FootStepObject footStepObj)
		{
			UpdateTerrainInfo(footStepObj.terrain);
			Vector3 position = footStepObj.sender.position;
			int x = (int)((position.x - terrainPos.x) / terrainData.size.x * (float)terrainData.alphamapWidth);
			int y = (int)((position.z - terrainPos.z) / terrainData.size.z * (float)terrainData.alphamapHeight);
			if (!terrainCollider.bounds.Contains(position))
			{
				return new float[0];
			}
			float[,,] alphamaps = terrainData.GetAlphamaps(x, y, 1, 1);
			float[] array = new float[alphamaps.GetUpperBound(2) + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = alphamaps[0, 0, i];
			}
			return array;
		}

		private int GetMainTexture(FootStepObject footStepObj)
		{
			float[] textureMix = GetTextureMix(footStepObj);
			if (textureMix == null)
			{
				return -1;
			}
			float num = 0f;
			int result = 0;
			for (int i = 0; i < textureMix.Length; i++)
			{
				if (textureMix[i] > num)
				{
					result = i;
					num = textureMix[i];
				}
			}
			return result;
		}

		public void StepOnTerrain(FootStepObject footStepObject)
		{
			if (currentStep != null && currentStep == footStepObject.sender)
			{
				return;
			}
			currentStep = footStepObject.sender;
			surfaceIndex = GetMainTexture(footStepObject);
			if (surfaceIndex != -1)
			{
				string text = (footStepObject.name = ((!(terrainData != null) || terrainData.splatPrototypes.Length <= 0) ? string.Empty : terrainData.splatPrototypes[surfaceIndex].texture.name));
				PlayFootFallSound(footStepObject);
				if (debugTextureName)
				{
					Debug.Log(terrain.name + " " + text);
				}
			}
		}

		public void StepOnMesh(FootStepObject footStepObject)
		{
			if (!(currentStep != null) || !(currentStep == footStepObject.sender))
			{
				currentStep = footStepObject.sender;
				PlayFootFallSound(footStepObject);
				if (debugTextureName)
				{
					Debug.Log(footStepObject.name);
				}
			}
		}

		private void OnDestroy()
		{
			if (leftFootTrigger != null)
			{
				Object.Destroy(leftFootTrigger.gameObject);
			}
			if (rightFootTrigger != null)
			{
				Object.Destroy(rightFootTrigger.gameObject);
			}
			if (footStepTriggers == null || footStepTriggers.Count <= 0)
			{
				return;
			}
			foreach (vFootStepTrigger footStepTrigger in footStepTriggers)
			{
				Object.Destroy(footStepTrigger.gameObject);
			}
		}
	}
}
