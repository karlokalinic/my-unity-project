using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Name")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_name.html")]
	public class RememberName : Remember
	{
		public override string SaveData()
		{
			NameData nameData = new NameData();
			nameData.objectID = constantID;
			nameData.savePrevented = savePrevented;
			nameData.newName = base.gameObject.name;
			return Serializer.SaveScriptData<NameData>(nameData);
		}

		public override void LoadData(string stringData)
		{
			NameData nameData = Serializer.LoadScriptData<NameData>(stringData);
			if (nameData != null)
			{
				base.SavePrevented = nameData.savePrevented;
				if (!savePrevented)
				{
					base.gameObject.name = nameData.newName;
				}
			}
		}
	}
}
