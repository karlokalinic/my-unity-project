using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class InvItem : ITranslatable
	{
		public string label;

		public int id;

		public string altLabel;

		public bool carryOnStart;

		public bool carryOnStartNotDefault;

		public int carryOnStartID;

		public List<InvVar> vars = new List<InvVar>();

		public bool canCarryMultiple;

		public int count;

		public bool useSeparateSlots;

		public bool selectSingle;

		public Texture tex;

		public Texture activeTex;

		public Texture selectedTex;

		public GameObject linkedPrefab;

		public CursorIcon cursorIcon = new CursorIcon();

		public int lineID = -1;

		public int useIconID;

		public int binID;

		public int recipeSlot = -1;

		public int lastInteractionIndex;

		public bool overrideUseSyntax;

		public HotspotPrefix hotspotPrefix1 = new HotspotPrefix("Use");

		public HotspotPrefix hotspotPrefix2 = new HotspotPrefix("on");

		public ActionListAsset useActionList;

		public ActionListAsset lookActionList;

		public List<InvInteraction> interactions = new List<InvInteraction>();

		public List<ActionListAsset> combineActionList = new List<ActionListAsset>();

		public List<int> combineID = new List<int>();

		public ActionListAsset unhandledActionList;

		public ActionListAsset unhandledCombineActionList;

		protected bool canBeAnimated;

		public InvItem(int[] idArray)
		{
			count = 0;
			tex = null;
			activeTex = null;
			selectedTex = null;
			cursorIcon = new CursorIcon();
			id = 0;
			binID = -1;
			recipeSlot = -1;
			useSeparateSlots = false;
			selectSingle = false;
			carryOnStartNotDefault = false;
			vars = new List<InvVar>();
			canBeAnimated = false;
			linkedPrefab = null;
			interactions = new List<InvInteraction>();
			combineActionList = new List<ActionListAsset>();
			combineID = new List<int>();
			overrideUseSyntax = false;
			hotspotPrefix1 = new HotspotPrefix("Use");
			hotspotPrefix2 = new HotspotPrefix("on");
			foreach (int num in idArray)
			{
				if (id == num)
				{
					id++;
				}
			}
			label = "Inventory item " + (id + 1);
			altLabel = string.Empty;
		}

		public InvItem(int _id)
		{
			count = 0;
			tex = null;
			activeTex = null;
			selectedTex = null;
			cursorIcon = new CursorIcon();
			id = _id;
			binID = -1;
			recipeSlot = -1;
			useSeparateSlots = false;
			selectSingle = false;
			carryOnStartNotDefault = false;
			vars = new List<InvVar>();
			canBeAnimated = false;
			linkedPrefab = null;
			interactions = new List<InvInteraction>();
			combineActionList = new List<ActionListAsset>();
			combineID = new List<int>();
			overrideUseSyntax = false;
			hotspotPrefix1 = new HotspotPrefix("Use");
			hotspotPrefix2 = new HotspotPrefix("on");
			label = "Inventory item " + (id + 1);
			altLabel = string.Empty;
		}

		public InvItem(InvItem assetItem)
		{
			count = assetItem.count;
			tex = assetItem.tex;
			activeTex = assetItem.activeTex;
			selectedTex = assetItem.selectedTex;
			cursorIcon = new CursorIcon();
			cursorIcon.Copy(assetItem.cursorIcon, true);
			carryOnStart = assetItem.carryOnStart;
			carryOnStartNotDefault = assetItem.carryOnStartNotDefault;
			carryOnStartID = assetItem.carryOnStartID;
			canCarryMultiple = assetItem.canCarryMultiple;
			label = assetItem.label;
			altLabel = assetItem.altLabel;
			id = assetItem.id;
			lineID = assetItem.lineID;
			useIconID = assetItem.useIconID;
			binID = assetItem.binID;
			if (binID == -1 && KickStarter.inventoryManager != null && KickStarter.inventoryManager.bins != null && KickStarter.inventoryManager.bins.Count > 0)
			{
				binID = KickStarter.inventoryManager.bins[0].id;
			}
			useSeparateSlots = assetItem.useSeparateSlots;
			selectSingle = assetItem.selectSingle;
			recipeSlot = -1;
			overrideUseSyntax = assetItem.overrideUseSyntax;
			hotspotPrefix1 = assetItem.hotspotPrefix1;
			hotspotPrefix2 = assetItem.hotspotPrefix2;
			useActionList = assetItem.useActionList;
			lookActionList = assetItem.lookActionList;
			interactions = assetItem.interactions;
			combineActionList = assetItem.combineActionList;
			unhandledActionList = assetItem.unhandledActionList;
			unhandledCombineActionList = assetItem.unhandledCombineActionList;
			combineID = assetItem.combineID;
			linkedPrefab = assetItem.linkedPrefab;
			canBeAnimated = DetermineCanBeAnimated();
			vars = assetItem.vars;
			if (!Application.isPlaying)
			{
				return;
			}
			for (int i = 0; i < vars.Count; i++)
			{
				if (vars[i].type == VariableType.PopUp)
				{
					vars[i].popUpsLineID = KickStarter.inventoryManager.invVars[i].popUpsLineID;
				}
			}
		}

		public bool DoesHaveInventoryInteraction(InvItem invItem)
		{
			if (invItem != null)
			{
				foreach (int item in combineID)
				{
					if (item == invItem.id)
					{
						return true;
					}
				}
			}
			return false;
		}

		public string GetLabel(int languageNumber)
		{
			if (languageNumber > 0)
			{
				return AdvGame.ConvertTokens(KickStarter.runtimeLanguages.GetTranslation(label, lineID, languageNumber));
			}
			if (!string.IsNullOrEmpty(altLabel))
			{
				return AdvGame.ConvertTokens(altLabel);
			}
			return AdvGame.ConvertTokens(label);
		}

		public void RunUseInteraction(int iconID = -1)
		{
			if (iconID < 0)
			{
				KickStarter.runtimeInventory.Use(this);
			}
			else
			{
				KickStarter.runtimeInventory.RunInteraction(iconID, this);
			}
		}

		public void RunDefaultInteraction()
		{
			if (interactions != null && interactions.Count > 0)
			{
				KickStarter.runtimeInventory.RunInteraction(interactions[0].icon.id);
			}
		}

		public void RunExamineInteraction()
		{
			KickStarter.runtimeInventory.Look(this);
		}

		public void CombineWithSelf()
		{
			KickStarter.runtimeInventory.Combine(this, this, true);
		}

		public void CombineWithItem(int otherItemID)
		{
			KickStarter.runtimeInventory.Combine(this, otherItemID);
		}

		public void CombineWithItem(InvItem otherItem)
		{
			KickStarter.runtimeInventory.Combine(this, otherItem);
		}

		public void Select()
		{
			KickStarter.runtimeInventory.SelectItem(this);
		}

		public void ShowInteractionMenus()
		{
			if (KickStarter.playerMenus != null)
			{
				KickStarter.playerMenus.EnableInteractionMenus(this);
			}
		}

		public string GetFullLabel(int languageNumber = 0)
		{
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations)
			{
				return string.Empty;
			}
			if (KickStarter.runtimeInventory.showHoverLabel)
			{
				if (KickStarter.runtimeInventory.SelectedItem == null || this != KickStarter.runtimeInventory.SelectedItem || KickStarter.settingsManager.ShowHoverInteractionInHotspotLabel())
				{
					return AdvGame.CombineLanguageString(KickStarter.playerInteraction.GetLabelPrefix(null, this, languageNumber), GetLabel(languageNumber), languageNumber);
				}
				return GetLabel(languageNumber);
			}
			return string.Empty;
		}

		public int GetNextInteraction(int i, int numInvInteractions)
		{
			if (i < interactions.Count)
			{
				i++;
				if (i >= interactions.Count + numInvInteractions)
				{
					return 0;
				}
				return i;
			}
			if (i == interactions.Count - 1 + numInvInteractions)
			{
				return 0;
			}
			return i + 1;
		}

		public int GetPreviousInteraction(int i, int numInvInteractions)
		{
			if (i > interactions.Count && numInvInteractions > 0)
			{
				return i - 1;
			}
			if (i == 0)
			{
				return GetNumInteractions(numInvInteractions) - 1;
			}
			if (i <= interactions.Count)
			{
				i--;
				if (i < 0)
				{
					return GetNumInteractions(numInvInteractions) - 1;
				}
				return i;
			}
			return i - 1;
		}

		public bool CanBeAnimated()
		{
			return canBeAnimated;
		}

		public bool HasCursorIcon()
		{
			if (tex != null || (cursorIcon != null && cursorIcon.texture != null))
			{
				return true;
			}
			return false;
		}

		public InvVar GetProperty(int ID, bool multiplyByItemCount = false)
		{
			if (vars.Count > 0 && ID >= 0)
			{
				foreach (InvVar var in vars)
				{
					if (var.id == ID)
					{
						if (multiplyByItemCount && count > 1 && (var.type == VariableType.Integer || var.type == VariableType.Float))
						{
							InvVar invVar = new InvVar(var);
							invVar.val *= count;
							invVar.floatVal *= count;
							return invVar;
						}
						return var;
					}
				}
			}
			return null;
		}

		public bool CanSelectSingle(int _count = -1)
		{
			if (canCarryMultiple && !useSeparateSlots && selectSingle)
			{
				if (_count >= 0)
				{
					return _count > 1;
				}
				return count > 1;
			}
			return false;
		}

		public int GetFirstStandardIcon()
		{
			foreach (InvInteraction interaction in interactions)
			{
				if (interaction != null)
				{
					return interaction.icon.id;
				}
			}
			return -1;
		}

		protected int GetNumInteractions(int numInvInteractions)
		{
			return interactions.Count + numInvInteractions;
		}

		protected bool DetermineCanBeAnimated()
		{
			if (cursorIcon != null && cursorIcon.texture != null && cursorIcon.isAnimated)
			{
				return true;
			}
			if (activeTex != null)
			{
				return true;
			}
			return false;
		}

		public string GetTranslatableString(int index)
		{
			if (index == 0)
			{
				if (!string.IsNullOrEmpty(altLabel))
				{
					return altLabel;
				}
				return label;
			}
			return vars[index - 1].textVal;
		}

		public int GetTranslationID(int index)
		{
			if (index == 0)
			{
				return lineID;
			}
			return vars[index - 1].textValLineID;
		}
	}
}
