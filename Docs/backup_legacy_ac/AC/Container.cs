using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_container.html")]
	public class Container : MonoBehaviour
	{
		public List<ContainerItem> items = new List<ContainerItem>();

		public bool limitToCategory;

		public List<int> categoryIDs = new List<int>();

		public int maxSlots;

		public int Count
		{
			get
			{
				int num = 0;
				foreach (ContainerItem item in items)
				{
					if (!item.IsEmpty)
					{
						num += item.count;
					}
				}
				return num;
			}
		}

		public int FilledSlots
		{
			get
			{
				int num = 0;
				foreach (ContainerItem item in items)
				{
					if (!item.IsEmpty)
					{
						num++;
					}
				}
				return num;
			}
		}

		protected void Awake()
		{
			RemoveWrongItems();
		}

		public void Interact()
		{
			KickStarter.playerInput.activeContainer = this;
		}

		public bool Add(ContainerItem containerItem)
		{
			if (containerItem != null && !containerItem.IsEmpty)
			{
				return Add(containerItem.linkedID, containerItem.count);
			}
			return false;
		}

		public bool Add(int _id, int amount)
		{
			InvItem item = KickStarter.inventoryManager.GetItem(_id);
			if (item != null)
			{
				if (item.canCarryMultiple && !item.useSeparateSlots)
				{
					foreach (ContainerItem item2 in items)
					{
						if (item2 != null && item2.linkedID == _id)
						{
							item2.count += amount;
							PlayerMenus.ResetInventoryBoxes();
							return true;
						}
					}
				}
				if (limitToCategory && !categoryIDs.Contains(item.binID))
				{
					return false;
				}
				if (!item.canCarryMultiple)
				{
					amount = 1;
				}
				ContainerItem containerItem = new ContainerItem(_id, amount, GetIDArray());
				if (KickStarter.settingsManager.canReorderItems)
				{
					for (int i = 0; i < items.Count; i++)
					{
						if (items[i].IsEmpty)
						{
							items[i] = containerItem;
							PlayerMenus.ResetInventoryBoxes();
							return true;
						}
					}
				}
				items.Add(containerItem);
				PlayerMenus.ResetInventoryBoxes();
				return true;
			}
			return false;
		}

		public void Remove(ContainerItem containerItem, int amount = -1)
		{
			if (containerItem == null)
			{
				return;
			}
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != containerItem)
				{
					continue;
				}
				if (amount < 0)
				{
					containerItem.count = 0;
				}
				else if (containerItem.count > 0)
				{
					containerItem.count -= amount;
				}
				if (containerItem.count < 1)
				{
					if (KickStarter.settingsManager.canReorderItems && i < items.Count - 1)
					{
						containerItem.IsEmpty = true;
					}
					else
					{
						items.Remove(containerItem);
					}
					PlayerMenus.ResetInventoryBoxes();
				}
			}
		}

		public void Remove(int _id, int amount, bool removeAllInstances = false)
		{
			for (int i = 0; i < items.Count; i++)
			{
				ContainerItem containerItem = items[i];
				if (containerItem == null || containerItem.linkedID != _id)
				{
					continue;
				}
				if (containerItem.count > 0)
				{
					containerItem.count -= amount;
				}
				if (containerItem.count < 1)
				{
					if (KickStarter.settingsManager.canReorderItems)
					{
						items[i] = null;
					}
					else
					{
						items.Remove(containerItem);
					}
				}
				PlayerMenus.ResetInventoryBoxes();
				if (!removeAllInstances)
				{
					break;
				}
			}
		}

		public void RemoveAll()
		{
			items.Clear();
			PlayerMenus.ResetInventoryBoxes();
		}

		public int GetCount(int _id)
		{
			foreach (ContainerItem item in items)
			{
				if (item != null && item.linkedID == _id)
				{
					return item.count;
				}
			}
			return 0;
		}

		public ContainerItem InsertAt(InvItem _item, int _index, int count = 0)
		{
			if (limitToCategory && !categoryIDs.Contains(_item.binID))
			{
				return null;
			}
			ContainerItem containerItem = new ContainerItem(_item.id, GetIDArray());
			if (count > 0)
			{
				containerItem.count = count;
			}
			else
			{
				containerItem.count = _item.count;
			}
			if (_index < items.Count)
			{
				if (!items[_index].IsEmpty && items[_index].linkedID == _item.id && _item.canCarryMultiple && !_item.useSeparateSlots)
				{
					containerItem.count += items[_index].count;
					items[_index] = containerItem;
				}
				else if (items[_index].IsEmpty && KickStarter.settingsManager.canReorderItems)
				{
					items[_index] = containerItem;
				}
				else
				{
					items.Insert(_index, containerItem);
				}
			}
			else
			{
				if (KickStarter.settingsManager.canReorderItems)
				{
					while (items.Count < _index - 1)
					{
						ContainerItem containerItem2 = new ContainerItem();
						containerItem2.IsEmpty = true;
						items.Add(containerItem2);
					}
				}
				items.Add(containerItem);
			}
			PlayerMenus.ResetInventoryBoxes();
			return containerItem;
		}

		public int[] GetIDArray()
		{
			List<int> list = new List<int>();
			foreach (ContainerItem item in items)
			{
				if (!item.IsEmpty)
				{
					list.Add(item.id);
				}
			}
			list.Sort();
			return list.ToArray();
		}

		public ContainerItem[] GetItemsWithInvID(int invID)
		{
			List<ContainerItem> list = new List<ContainerItem>();
			if (items != null)
			{
				foreach (ContainerItem item in items)
				{
					if (!item.IsEmpty && item.linkedID == invID)
					{
						list.Add(item);
					}
				}
			}
			return list.ToArray();
		}

		protected void RemoveWrongItems()
		{
			if (limitToCategory && categoryIDs.Count > 0)
			{
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i].IsEmpty)
					{
						continue;
					}
					InvItem item = KickStarter.inventoryManager.GetItem(items[i].linkedID);
					if (!categoryIDs.Contains(item.binID))
					{
						if (KickStarter.settingsManager.canReorderItems)
						{
							items[i].IsEmpty = true;
						}
						else
						{
							items.RemoveAt(i);
						}
						i--;
					}
				}
			}
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if (!items[num].IsEmpty)
				{
					InvItem item2 = KickStarter.inventoryManager.GetItem(items[num].linkedID);
					if (item2 != null && item2.canCarryMultiple && item2.useSeparateSlots && items[num].count > 1)
					{
						while (items[num].count > 1)
						{
							ContainerItem item3 = new ContainerItem(items[num].linkedID, 1, GetIDArray());
							items.Insert(num + 1, item3);
							items[num].count--;
						}
					}
				}
			}
			if (maxSlots > 0 && items.Count > maxSlots)
			{
				items.RemoveRange(maxSlots, items.Count - maxSlots);
			}
		}
	}
}
