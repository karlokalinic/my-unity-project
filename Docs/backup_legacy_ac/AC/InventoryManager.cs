using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class InventoryManager : ScriptableObject
	{
		public List<InvItem> items = new List<InvItem>();

		public List<InvBin> bins = new List<InvBin>();

		public List<InvVar> invVars = new List<InvVar>();

		public ActionListAsset unhandledCombine;

		public ActionListAsset unhandledHotspot;

		public bool passUnhandledHotspotAsParameter;

		public ActionListAsset unhandledGive;

		public List<Recipe> recipes = new List<Recipe>();

		public List<Document> documents = new List<Document>();

		public List<Objective> objectives = new List<Objective>();

		public InvVar GetProperty(int ID)
		{
			if (invVars.Count > 0 && ID >= 0)
			{
				foreach (InvVar invVar in invVars)
				{
					if (invVar.id == ID)
					{
						return invVar;
					}
				}
			}
			return null;
		}

		public Document GetDocument(int ID)
		{
			foreach (Document document in documents)
			{
				if (document.ID == ID)
				{
					return document;
				}
			}
			return null;
		}

		public Objective GetObjective(int ID)
		{
			foreach (Objective objective in objectives)
			{
				if (objective.ID == ID)
				{
					return objective;
				}
			}
			return null;
		}

		public string GetLabel(int _id)
		{
			string result = string.Empty;
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					result = item.label;
				}
			}
			return result;
		}

		public InvItem GetItem(string _name)
		{
			foreach (InvItem item in items)
			{
				if (item.label == _name)
				{
					return item;
				}
			}
			return null;
		}

		public InvItem GetItem(int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			return null;
		}

		public Recipe GetRecipe(int _id)
		{
			foreach (Recipe recipe in recipes)
			{
				if (recipe.id == _id)
				{
					return recipe;
				}
			}
			return null;
		}

		public InvBin GetCategory(int categoryID)
		{
			foreach (InvBin bin in bins)
			{
				if (bin.id == categoryID)
				{
					return bin;
				}
			}
			return null;
		}

		public bool CanCarryMultiple(int itemID)
		{
			foreach (InvItem item in items)
			{
				if (item.id == itemID)
				{
					return item.canCarryMultiple;
				}
			}
			return false;
		}

		public InvItem[] GetItemsInCategory(int categoryID)
		{
			List<InvItem> list = new List<InvItem>();
			foreach (InvItem item in items)
			{
				if (item.binID == categoryID)
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public bool ObjectiveIsPerPlayer(int objectiveID)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				foreach (Objective objective in objectives)
				{
					if (objective.ID == objectiveID)
					{
						return objective.perPlayer;
					}
				}
				ACDebug.LogWarning("An Objective with ID=" + objectiveID + " could not be found.");
				return false;
			}
			return false;
		}
	}
}
