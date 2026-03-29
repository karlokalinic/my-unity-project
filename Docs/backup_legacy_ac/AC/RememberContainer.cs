using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Save system/Remember Container")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_container.html")]
	public class RememberContainer : Remember
	{
		private Container container;

		private Container _Container
		{
			get
			{
				if (container == null)
				{
					container = GetComponent<Container>();
				}
				return container;
			}
		}

		public override string SaveData()
		{
			ContainerData containerData = new ContainerData();
			containerData.objectID = constantID;
			containerData.savePrevented = savePrevented;
			if (_Container != null)
			{
				List<int> list = new List<int>();
				List<int> list2 = new List<int>();
				List<int> list3 = new List<int>();
				for (int i = 0; i < _Container.items.Count; i++)
				{
					ContainerItem containerItem = _Container.items[i];
					list.Add(containerItem.linkedID);
					list2.Add(containerItem.count);
					list3.Add(containerItem.id);
				}
				containerData._linkedIDs = ArrayToString(list.ToArray());
				containerData._counts = ArrayToString(list2.ToArray());
				containerData._IDs = ArrayToString(list3.ToArray());
			}
			return Serializer.SaveScriptData<ContainerData>(containerData);
		}

		public override void LoadData(string stringData)
		{
			ContainerData containerData = Serializer.LoadScriptData<ContainerData>(stringData);
			if (containerData == null)
			{
				return;
			}
			base.SavePrevented = containerData.savePrevented;
			if (savePrevented || !(_Container != null))
			{
				return;
			}
			_Container.items.Clear();
			int[] array = StringToIntArray(containerData._linkedIDs);
			int[] array2 = StringToIntArray(containerData._counts);
			int[] array3 = StringToIntArray(containerData._IDs);
			if (array3 != null)
			{
				for (int i = 0; i < array3.Length; i++)
				{
					ContainerItem item = new ContainerItem(array[i], array2[i], array3[i]);
					_Container.items.Add(item);
				}
			}
		}
	}
}
