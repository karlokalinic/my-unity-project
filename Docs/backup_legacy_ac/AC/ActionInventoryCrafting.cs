using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInventoryCrafting : Action
	{
		public enum ActionCraftingMethod
		{
			ClearRecipe = 0,
			CreateRecipe = 1
		}

		public ActionCraftingMethod craftingMethod;

		public ActionInventoryCrafting()
		{
			isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Crafting";
			description = "Either clears the current arrangement of crafting ingredients, or evaluates them to create an appropriate result (if this is not done automatically by the recipe itself).";
		}

		public override float Run()
		{
			if (craftingMethod == ActionCraftingMethod.ClearRecipe)
			{
				KickStarter.runtimeInventory.RemoveRecipes();
			}
			else if (craftingMethod == ActionCraftingMethod.CreateRecipe)
			{
				PlayerMenus.CreateRecipe();
			}
			return 0f;
		}

		public static ActionInventoryCrafting CreateNew(ActionCraftingMethod craftingMethod)
		{
			ActionInventoryCrafting actionInventoryCrafting = ScriptableObject.CreateInstance<ActionInventoryCrafting>();
			actionInventoryCrafting.craftingMethod = craftingMethod;
			return actionInventoryCrafting;
		}
	}
}
