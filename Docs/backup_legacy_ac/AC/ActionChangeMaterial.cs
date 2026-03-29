using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionChangeMaterial : Action
	{
		public int constantID;

		public int parameterID = -1;

		public bool isPlayer;

		public GameObject obToAffect;

		protected GameObject runtimeObToAffect;

		public int materialIndex;

		public Material newMaterial;

		public int newMaterialParameterID = -1;

		public ActionChangeMaterial()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Change material";
			description = "Changes the material on any scene-based mesh object.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				runtimeObToAffect = GetPlayerRenderer(KickStarter.player);
			}
			else
			{
				runtimeObToAffect = AssignFile(parameters, parameterID, constantID, obToAffect);
			}
			newMaterial = (Material)AssignObject<Material>(parameters, newMaterialParameterID, newMaterial);
		}

		public override float Run()
		{
			if ((bool)runtimeObToAffect && (bool)newMaterial)
			{
				Renderer component = runtimeObToAffect.GetComponent<Renderer>();
				if (component != null)
				{
					Material[] materials = component.materials;
					materials[materialIndex] = newMaterial;
					runtimeObToAffect.GetComponent<Renderer>().materials = materials;
				}
			}
			return 0f;
		}

		protected GameObject GetPlayerRenderer(Player player)
		{
			if (player == null)
			{
				return null;
			}
			if (player.spriteChild != null && (bool)player.spriteChild.GetComponent<Renderer>())
			{
				return player.spriteChild.gameObject;
			}
			if ((bool)player.GetComponentInChildren<Renderer>())
			{
				return player.gameObject.GetComponentInChildren<Renderer>().gameObject;
			}
			return player.gameObject;
		}

		public static ActionChangeMaterial CreateNew(Renderer renderer, Material newMaterial, int materialIndex = 0)
		{
			ActionChangeMaterial actionChangeMaterial = ScriptableObject.CreateInstance<ActionChangeMaterial>();
			actionChangeMaterial.obToAffect = ((!(renderer != null)) ? null : renderer.gameObject);
			actionChangeMaterial.newMaterial = newMaterial;
			actionChangeMaterial.materialIndex = materialIndex;
			return actionChangeMaterial;
		}
	}
}
