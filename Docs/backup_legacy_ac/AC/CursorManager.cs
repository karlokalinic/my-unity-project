using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class CursorManager : ScriptableObject
	{
		public CursorRendering cursorRendering;

		public CursorDisplay cursorDisplay;

		public bool allowMainCursor;

		public bool lockSystemCursor = true;

		public bool keepCursorWithinScreen = true;

		public bool confineSystemCursor;

		public bool allowWalkCursor;

		public bool addWalkPrefix;

		public HotspotPrefix walkPrefix = new HotspotPrefix("Walk to");

		public bool addHotspotPrefix;

		public bool allowInteractionCursor;

		public bool allowInteractionCursorForInventory;

		public bool cycleCursors;

		public bool onlyAnimateOverHotspots;

		public bool leftClickExamine;

		public bool onlyWalkWhenOverNavMesh;

		public bool onlyShowInventoryLabelOverHotspots;

		public float inventoryCursorSize = 0.06f;

		public bool allowIconInput = true;

		public CursorIconBase waitIcon = new CursorIcon();

		public CursorIconBase pointerIcon = new CursorIcon();

		public CursorIconBase walkIcon = new CursorIcon();

		public CursorIconBase mouseOverIcon = new CursorIcon();

		public InventoryHandling inventoryHandling;

		public HotspotPrefix hotspotPrefix1 = new HotspotPrefix("Use");

		public HotspotPrefix hotspotPrefix2 = new HotspotPrefix("on");

		public HotspotPrefix hotspotPrefix3 = new HotspotPrefix("Give");

		public HotspotPrefix hotspotPrefix4 = new HotspotPrefix("to");

		public List<CursorIcon> cursorIcons = new List<CursorIcon>();

		public List<ActionListAsset> unhandledCursorInteractions = new List<ActionListAsset>();

		public bool passUnhandledHotspotAsParameter;

		public LookUseCursorAction lookUseCursorAction;

		public int lookCursor_ID;

		public bool hideCursorWhenDraggingMoveables = true;

		private SettingsManager settingsManager;

		public bool AllowUnhandledIcons()
		{
			if (KickStarter.settingsManager != null)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					return true;
				}
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot && !KickStarter.settingsManager.autoHideInteractionIcons)
				{
					return true;
				}
			}
			return false;
		}

		public string[] GetLabelsArray(bool includeNone = false)
		{
			List<string> list = new List<string>();
			if (includeNone)
			{
				list.Add("(None)");
			}
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				list.Add(cursorIcon.id + ": " + cursorIcon.label);
			}
			return list.ToArray();
		}

		public string GetLabelFromID(int _ID, int languageNumber)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					if (Application.isPlaying)
					{
						return KickStarter.runtimeLanguages.GetTranslation(cursorIcon.label, cursorIcon.lineID, languageNumber);
					}
					return cursorIcon.label;
				}
			}
			return string.Empty;
		}

		public CursorIcon GetCursorIconFromID(int _ID)
		{
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					return cursorIcon;
				}
			}
			return null;
		}

		public int GetIntFromID(int _ID)
		{
			int num = 0;
			int num2 = -1;
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				if (cursorIcon.id == _ID)
				{
					num2 = num;
				}
				num++;
			}
			if (num2 == -1)
			{
				num2 = 0;
			}
			return num2;
		}

		public ActionListAsset GetUnhandledInteraction(int _ID)
		{
			if (AllowUnhandledIcons())
			{
				foreach (CursorIcon cursorIcon in cursorIcons)
				{
					if (cursorIcon.id == _ID)
					{
						int num = cursorIcons.IndexOf(cursorIcon);
						if (unhandledCursorInteractions.Count > num)
						{
							return unhandledCursorInteractions[num];
						}
						return null;
					}
				}
			}
			return null;
		}

		private int[] GetIDArray()
		{
			List<int> list = new List<int>();
			foreach (CursorIcon cursorIcon in cursorIcons)
			{
				list.Add(cursorIcon.id);
			}
			list.Sort();
			return list.ToArray();
		}
	}
}
