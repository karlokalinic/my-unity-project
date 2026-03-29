using System;

namespace AC
{
	[Serializable]
	public class ContainerItem
	{
		public int linkedID = -1;

		public int count;

		public int id;

		public bool IsEmpty
		{
			get
			{
				return linkedID < 0;
			}
			set
			{
				linkedID = -1;
			}
		}

		public ContainerItem()
		{
			IsEmpty = true;
		}

		public ContainerItem(ContainerItem containerItem)
		{
			linkedID = containerItem.linkedID;
			count = containerItem.count;
			id = containerItem.id;
		}

		public ContainerItem(int _linkedID, int[] idArray)
		{
			count = 1;
			linkedID = _linkedID;
			id = 0;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
		}

		public ContainerItem(int _linkedID, int _count, int[] idArray)
		{
			count = _count;
			linkedID = _linkedID;
			id = 0;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
		}

		public ContainerItem(int _linkedID, int _count, int _id)
		{
			linkedID = _linkedID;
			count = _count;
			id = _id;
		}

		public InvItem GetLinkedInventoryItem()
		{
			if (KickStarter.inventoryManager != null)
			{
				return KickStarter.inventoryManager.GetItem(linkedID);
			}
			return null;
		}
	}
}
