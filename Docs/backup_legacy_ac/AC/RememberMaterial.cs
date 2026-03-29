using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Material")]
	[RequireComponent(typeof(Renderer))]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_material.html")]
	public class RememberMaterial : Remember
	{
		public override string SaveData()
		{
			MaterialData materialData = new MaterialData();
			materialData.objectID = constantID;
			materialData.savePrevented = savePrevented;
			List<string> list = new List<string>();
			Material[] materials = GetComponent<Renderer>().materials;
			Material[] array = materials;
			foreach (Material originalFile in array)
			{
				list.Add(AssetLoader.GetAssetInstanceID(originalFile));
			}
			materialData._materialIDs = ArrayToString(list.ToArray());
			return Serializer.SaveScriptData<MaterialData>(materialData);
		}

		public override void LoadData(string stringData)
		{
			MaterialData materialData = Serializer.LoadScriptData<MaterialData>(stringData);
			if (materialData == null)
			{
				return;
			}
			base.SavePrevented = materialData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			Material[] materials = GetComponent<Renderer>().materials;
			string[] array = StringToStringArray(materialData._materialIDs);
			for (int i = 0; i < array.Length; i++)
			{
				if (materials.Length >= i)
				{
					Material material = AssetLoader.RetrieveAsset(materials[i], array[i]);
					if (material != null)
					{
						materials[i] = material;
					}
				}
			}
			GetComponent<Renderer>().materials = materials;
		}
	}
}
