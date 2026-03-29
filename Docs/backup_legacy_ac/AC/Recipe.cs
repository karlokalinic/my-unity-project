using System;
using System.Collections.Generic;

namespace AC
{
	[Serializable]
	public class Recipe
	{
		public string label;

		public int id;

		public List<Ingredient> ingredients = new List<Ingredient>();

		public int resultID;

		public bool useSpecificSlots;

		public OnCreateRecipe onCreateRecipe;

		public ActionListAsset invActionList;

		public ActionListAsset actionListOnCreate;

		public Recipe(int[] idArray)
		{
			ingredients = new List<Ingredient>();
			resultID = 0;
			useSpecificSlots = false;
			invActionList = null;
			onCreateRecipe = OnCreateRecipe.JustMoveToInventory;
			actionListOnCreate = null;
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			label = "Recipe " + (id + 1);
		}
	}
}
