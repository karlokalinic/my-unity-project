using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Shapeable")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_shapeable.html")]
	public class RememberShapeable : Remember
	{
		public override string SaveData()
		{
			ShapeableData shapeableData = new ShapeableData();
			shapeableData.objectID = constantID;
			shapeableData.savePrevented = savePrevented;
			Shapeable component = GetComponent<Shapeable>();
			if (component != null)
			{
				List<int> list = new List<int>();
				List<float> list2 = new List<float>();
				foreach (ShapeGroup shapeGroup in component.shapeGroups)
				{
					list.Add(shapeGroup.GetActiveKeyID());
					list2.Add(shapeGroup.GetActiveKeyValue());
				}
				shapeableData._activeKeyIDs = ArrayToString(list.ToArray());
				shapeableData._values = ArrayToString(list2.ToArray());
			}
			return Serializer.SaveScriptData<ShapeableData>(shapeableData);
		}

		public override void LoadData(string stringData)
		{
			ShapeableData shapeableData = Serializer.LoadScriptData<ShapeableData>(stringData);
			if (shapeableData == null)
			{
				return;
			}
			base.SavePrevented = shapeableData.savePrevented;
			if (savePrevented)
			{
				return;
			}
			Shapeable component = GetComponent<Shapeable>();
			if (!(component != null))
			{
				return;
			}
			int[] array = StringToIntArray(shapeableData._activeKeyIDs);
			float[] array2 = StringToFloatArray(shapeableData._values);
			for (int i = 0; i < array.Length; i++)
			{
				if (array2.Length > i)
				{
					component.shapeGroups[i].SetActive(array[i], array2[i], 0f, MoveMethod.Linear, null);
				}
			}
		}
	}
}
