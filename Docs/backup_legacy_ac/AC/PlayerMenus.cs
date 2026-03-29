using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_menus.html")]
	public class PlayerMenus : MonoBehaviour
	{
		protected struct InteractionMenuData
		{
			public Menu menuFor;

			public Hotspot hotspotFor;

			public InvItem itemFor;

			public InteractionMenuData(Menu _menuFor, Hotspot _hotspotFor, InvItem _itemFor)
			{
				menuFor = _menuFor;
				hotspotFor = _hotspotFor;
				itemFor = _itemFor;
			}
		}

		protected bool isMouseOverMenu;

		protected bool isMouseOverInteractionMenu;

		protected bool canKeyboardControl;

		protected bool interactionMenuIsOn;

		protected bool interactionMenuPauses;

		protected bool lockSave;

		protected int selected_option;

		protected bool foundMouseOverMenu;

		protected bool foundMouseOverInteractionMenu;

		protected bool foundMouseOverInventory;

		protected bool foundCanKeyboardControl;

		protected bool isMouseOverInventory;

		protected float pauseAlpha;

		protected List<Menu> menus = new List<Menu>();

		protected List<Menu> dupSpeechMenus = new List<Menu>();

		protected List<Menu> customMenus = new List<Menu>();

		protected Texture2D pauseTexture;

		protected string menuIdentifier = string.Empty;

		protected string lastMenuIdentifier = string.Empty;

		protected string elementIdentifier = string.Empty;

		protected string lastElementIdentifier = string.Empty;

		protected MenuInput selectedInputBox;

		protected string selectedInputBoxMenuName;

		protected MenuInventoryBox activeInventoryBox;

		protected MenuCrafting activeCrafting;

		protected Menu activeInventoryBoxMenu;

		protected InvItem oldHoverItem;

		protected int doResizeMenus;

		protected Menu mouseOverMenu;

		protected MenuElement mouseOverElement;

		protected int mouseOverElementSlot;

		protected Menu crossFadeTo;

		protected Menu crossFadeFrom;

		protected EventSystem eventSystem;

		protected int elementOverCursorID = -1;

		protected GUIStyle normalStyle = new GUIStyle();

		protected GUIStyle highlightedStyle = new GUIStyle();

		protected Rect lastSafeRect;

		protected float lastAspectRatio;

		protected string hotspotLabelOverride;

		public EventSystem EventSystem
		{
			get
			{
				return eventSystem;
			}
		}

		public float PauseTextureAlpha
		{
			get
			{
				return pauseAlpha;
			}
		}

		public void OnStart()
		{
			RebuildMenus();
		}

		public void RebuildMenus(MenuManager menuManager = null)
		{
			if (menuManager != null)
			{
				KickStarter.menuManager = menuManager;
			}
			foreach (Menu menu2 in menus)
			{
				if (menu2.menuSource == MenuSource.UnityUiPrefab && menu2.RuntimeCanvas != null && menu2.RuntimeCanvas.gameObject != null && !menu2.GetsDuplicated())
				{
					Object.Destroy(menu2.RuntimeCanvas.gameObject);
				}
			}
			menus = new List<Menu>();
			if ((bool)KickStarter.menuManager)
			{
				KickStarter.menuManager.Upgrade();
				pauseTexture = KickStarter.menuManager.pauseTexture;
				foreach (Menu menu3 in KickStarter.menuManager.menus)
				{
					Menu menu = ScriptableObject.CreateInstance<Menu>();
					menu.Copy(menu3, false);
					if (!menu3.GetsDuplicated() && menu.IsUnityUI())
					{
						menu.LoadUnityUI();
					}
					menu.Recalculate();
					menus.Add(menu);
					menu.Initalise();
				}
			}
			CreateEventSystem();
			foreach (Menu menu4 in menus)
			{
				menu4.Recalculate();
			}
			KickStarter.eventManager.Call_OnGenerateMenus();
			StartCoroutine(CycleMouseOverUIs());
		}

		protected IEnumerator CycleMouseOverUIs()
		{
			foreach (Menu menu in menus)
			{
				if (menu.menuSource != MenuSource.AdventureCreator && menu.appearType == AppearType.MouseOver)
				{
					menu.EnableUI();
				}
			}
			yield return new WaitForEndOfFrame();
			foreach (Menu menu2 in menus)
			{
				if (menu2.menuSource != MenuSource.AdventureCreator && menu2.appearType == AppearType.MouseOver)
				{
					menu2.DisableUI();
				}
			}
		}

		protected void CreateEventSystem()
		{
			EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
			if (eventSystem == null)
			{
				EventSystem eventSystem2 = null;
				if ((bool)KickStarter.menuManager)
				{
					if (KickStarter.menuManager.eventSystem != null)
					{
						eventSystem2 = Object.Instantiate(KickStarter.menuManager.eventSystem);
						eventSystem2.gameObject.name = KickStarter.menuManager.eventSystem.name;
					}
					else if (AreAnyMenusUI())
					{
						GameObject gameObject = new GameObject();
						gameObject.name = "EventSystem";
						eventSystem2 = gameObject.AddComponent<EventSystem>();
						if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							gameObject.AddComponent<StandaloneInputModule>();
						}
						else
						{
							gameObject.AddComponent<OptionalMouseInputModule>();
						}
					}
				}
				if (eventSystem2 != null)
				{
					this.eventSystem = eventSystem2;
				}
			}
			else if (this.eventSystem == null)
			{
				this.eventSystem = eventSystem;
				ACDebug.LogWarning("A local EventSystem object was found in the scene.  This will override the one created by AC, and may cause problems.  A custom EventSystem prefab can be assigned in the Menu Manager.", eventSystem);
			}
		}

		protected bool AreAnyMenusUI()
		{
			Menu[] array = GetMenus(true).ToArray();
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				if (menu.menuSource == MenuSource.UnityUiInScene || menu.menuSource == MenuSource.UnityUiPrefab)
				{
					return true;
				}
			}
			return false;
		}

		public void AfterLoad()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene())
			{
				return;
			}
			CreateEventSystem();
			foreach (Menu menu in menus)
			{
				menu.AfterSceneChange();
			}
			foreach (Menu customMenu in customMenus)
			{
				customMenu.AfterSceneChange();
			}
			StartCoroutine(CycleMouseOverUIs());
		}

		public void AfterSceneAdd()
		{
			foreach (Menu menu in menus)
			{
				if (menu.menuSource == MenuSource.UnityUiInScene)
				{
					menu.LoadUnityUI();
					menu.Initalise();
				}
			}
		}

		public void ClearParents()
		{
			foreach (Menu menu in menus)
			{
				if (menu.IsUnityUI() && menu.RuntimeCanvas != null)
				{
					menu.ClearParent();
				}
			}
		}

		protected void UpdatePauseBackground(bool fadeIn)
		{
			float num = 0.5f;
			if (fadeIn)
			{
				if (pauseAlpha < 1f)
				{
					pauseAlpha += 0.2f * num;
				}
				else
				{
					pauseAlpha = 1f;
				}
			}
			else if (pauseAlpha > 0f)
			{
				pauseAlpha -= 0.2f * num;
			}
			else
			{
				pauseAlpha = 0f;
			}
		}

		public void DrawLoadingMenus()
		{
			for (int i = 0; i < menus.Count; i++)
			{
				int language = Options.GetLanguage();
				if (menus[i].appearType == AppearType.WhileLoading)
				{
					DrawMenu(menus[i], language);
				}
			}
		}

		public void DrawMenus()
		{
			if (doResizeMenus > 0)
			{
				return;
			}
			elementOverCursorID = -1;
			if (!KickStarter.playerInteraction || !KickStarter.playerInput || !KickStarter.menuSystem || !KickStarter.stateHandler || !KickStarter.settingsManager)
			{
				return;
			}
			GUI.depth = KickStarter.menuManager.globalDepth;
			if (pauseTexture != null && pauseAlpha > 0f)
			{
				Color color = GUI.color;
				color.a = pauseAlpha;
				GUI.color = color;
				GUI.DrawTexture(AdvGame.GUIRect(0.5f, 0.5f, 1f, 1f), pauseTexture, ScaleMode.ScaleToFit, true, 0f);
			}
			if ((bool)selectedInputBox)
			{
				Event current = Event.current;
				if (current.isKey && current.type == EventType.KeyDown)
				{
					selectedInputBox.CheckForInput(current.keyCode.ToString(), current.character.ToString(), current.shift, selectedInputBoxMenuName);
				}
			}
			int language = Options.GetLanguage();
			for (int i = 0; i < menus.Count; i++)
			{
				DrawMenu(menus[i], language);
			}
			for (int j = 0; j < dupSpeechMenus.Count; j++)
			{
				DrawMenu(dupSpeechMenus[j], language);
			}
			for (int k = 0; k < customMenus.Count; k++)
			{
				DrawMenu(customMenus[k], language);
			}
		}

		public Menu GetMenuWithElement(MenuElement _element)
		{
			foreach (Menu menu in menus)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element == _element)
					{
						return menu;
					}
				}
			}
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				foreach (MenuElement element2 in dupSpeechMenu.elements)
				{
					if (element2 == _element)
					{
						return dupSpeechMenu;
					}
				}
			}
			foreach (Menu customMenu in customMenus)
			{
				foreach (MenuElement element3 in customMenu.elements)
				{
					if (element3 == _element)
					{
						return customMenu;
					}
				}
			}
			return null;
		}

		public void DrawMenu(Menu menu, int languageNumber = 0)
		{
			Color color = GUI.color;
			bool flag = !menu.IsUnityUI();
			if (menu.IsEnabled())
			{
				if ((!menu.HasTransition() && menu.IsFading()) || (menu.hideDuringSaveScreenshots && KickStarter.saveSystem.IsTakingSaveScreenshot))
				{
					return;
				}
				if (flag)
				{
					if (menu.transitionType == MenuTransition.Fade || menu.transitionType == MenuTransition.FadeAndPan)
					{
						color.a = 1f - menu.GetFadeProgress();
						GUI.color = color;
					}
					else
					{
						color.a = 1f;
						GUI.color = color;
					}
					menu.StartDisplay();
				}
				for (int i = 0; i < menu.NumElements; i++)
				{
					if (!menu.elements[i].IsVisible)
					{
						continue;
					}
					if (flag)
					{
						SetStyles(menu.elements[i]);
					}
					for (int j = 0; j < menu.elements[i].GetNumSlots(); j++)
					{
						if (menu.IsEnabled() && KickStarter.stateHandler.gameState != GameState.Cutscene && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot && menu.appearType == AppearType.OnInteraction)
						{
							if (menu.elements[i] is MenuInteraction)
							{
								MenuInteraction menuInteraction = (MenuInteraction)menu.elements[i];
								if (menuInteraction.iconID == KickStarter.playerInteraction.GetActiveUseButtonIconID())
								{
									if (flag)
									{
										menu.elements[i].Display(highlightedStyle, j, menu.GetZoom(), true);
									}
								}
								else if (flag)
								{
									menu.elements[i].Display(normalStyle, j, menu.GetZoom(), false);
								}
							}
							else if (menu.elements[i] is MenuInventoryBox)
							{
								MenuInventoryBox menuInventoryBox = (MenuInventoryBox)menu.elements[i];
								if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.HotspotBased && menuInventoryBox.items[j].id == KickStarter.playerInteraction.GetActiveInvButtonID())
								{
									if (flag)
									{
										menu.elements[i].Display(highlightedStyle, j, menu.GetZoom(), true);
									}
								}
								else if (flag)
								{
									menu.elements[i].Display(normalStyle, j, menu.GetZoom(), false);
								}
							}
							else if (flag)
							{
								menu.elements[i].Display(normalStyle, j, menu.GetZoom(), false);
							}
						}
						else if (menu.IsClickable() && KickStarter.playerInput.IsCursorReadable() && SlotIsInteractive(menu, i, j))
						{
							if (flag)
							{
								float zoom = 1f;
								if (menu.transitionType == MenuTransition.Zoom)
								{
									zoom = menu.GetZoom();
								}
								if ((!interactionMenuIsOn || menu.appearType == AppearType.OnInteraction) && (KickStarter.playerInput.GetDragState() == DragState.None || (KickStarter.playerInput.GetDragState() == DragState.Inventory && CanElementBeDroppedOnto(menu.elements[i]))))
								{
									menu.elements[i].Display(highlightedStyle, j, zoom, true);
									if (menu.elements[i].changeCursor)
									{
										elementOverCursorID = menu.elements[i].cursorID;
									}
								}
								else
								{
									menu.elements[i].Display(normalStyle, j, zoom, false);
								}
							}
							else if ((!interactionMenuIsOn || menu.appearType == AppearType.OnInteraction) && (KickStarter.playerInput.GetDragState() == DragState.None || (KickStarter.playerInput.GetDragState() == DragState.Inventory && CanElementBeDroppedOnto(menu.elements[i]))) && menu.elements[i].changeCursor)
							{
								elementOverCursorID = menu.elements[i].cursorID;
							}
						}
						else if (flag && menu.elements[i] is MenuInteraction)
						{
							MenuInteraction menuInteraction2 = (MenuInteraction)menu.elements[i];
							if (menuInteraction2.IsDefaultIcon)
							{
								menu.elements[i].Display(highlightedStyle, j, menu.GetZoom(), false);
							}
							else
							{
								menu.elements[i].Display(normalStyle, j, menu.GetZoom(), false);
							}
						}
						else if (flag)
						{
							menu.elements[i].Display(normalStyle, j, menu.GetZoom(), false);
						}
					}
				}
				if (flag)
				{
					menu.EndDisplay();
				}
			}
			if (flag)
			{
				color.a = 1f;
				GUI.color = color;
			}
		}

		public void UpdateMenuPosition(Menu menu, Vector2 invertedMouse, bool force = false)
		{
			if (!menu.IsEnabled() && !force)
			{
				return;
			}
			if (!menu.oneMenuPerSpeech && menu.appearType == AppearType.WhenSpeechPlays)
			{
				Speech latestSpeech = KickStarter.dialog.GetLatestSpeech();
				if (latestSpeech != null && !latestSpeech.MenuCanShow(menu))
				{
					return;
				}
			}
			if (menu.IsUnityUI())
			{
				if (!Application.isPlaying)
				{
					return;
				}
				Vector2 zero = Vector2.zero;
				switch (menu.uiPositionType)
				{
				case UIPositionType.Manual:
					break;
				case UIPositionType.FollowCursor:
					if (menu.RuntimeCanvas != null && menu.RuntimeCanvas.renderMode == RenderMode.WorldSpace)
					{
						zero = new Vector2(invertedMouse.x, (float)ACScreen.height + 1f - invertedMouse.y);
						Vector3 centre3D = menu.RuntimeCanvas.worldCamera.ScreenToWorldPoint(new Vector3(zero.x, zero.y, 10f));
						menu.SetCentre3D(centre3D);
					}
					else
					{
						zero = new Vector2(invertedMouse.x, (float)ACScreen.height + 1f - invertedMouse.y);
						menu.SetCentre(zero);
					}
					break;
				case UIPositionType.OnHotspot:
					if (isMouseOverMenu || canKeyboardControl)
					{
						if (menu.TargetInvItem == null && menu.TargetHotspot != null)
						{
							break;
						}
						if (activeCrafting != null)
						{
							if (menu.TargetInvItem != null)
							{
								int itemSlot = activeCrafting.GetItemSlot(menu.TargetInvItem.id);
								zero = activeInventoryBoxMenu.GetSlotCentre(activeCrafting, itemSlot);
								menu.SetCentre(new Vector2(zero.x, (float)ACScreen.height - zero.y));
							}
							else if (KickStarter.runtimeInventory.hoverItem != null)
							{
								int itemSlot2 = activeCrafting.GetItemSlot(KickStarter.runtimeInventory.hoverItem.id);
								zero = activeInventoryBoxMenu.GetSlotCentre(activeCrafting, itemSlot2);
								menu.SetCentre(new Vector2(zero.x, (float)ACScreen.height - zero.y));
							}
						}
						else if (activeInventoryBox != null)
						{
							if (menu.TargetInvItem != null)
							{
								int itemSlot3 = activeInventoryBox.GetItemSlot(menu.TargetInvItem.id);
								zero = activeInventoryBoxMenu.GetSlotCentre(activeInventoryBox, itemSlot3);
								menu.SetCentre(new Vector2(zero.x, (float)ACScreen.height - zero.y));
							}
							else if (KickStarter.runtimeInventory.hoverItem != null)
							{
								int itemSlot4 = activeInventoryBox.GetItemSlot(KickStarter.runtimeInventory.hoverItem.id);
								zero = activeInventoryBoxMenu.GetSlotCentre(activeInventoryBox, itemSlot4);
								menu.SetCentre(new Vector2(zero.x, (float)ACScreen.height - zero.y));
							}
						}
					}
					else if (menu.TargetInvItem == null && !MoveUIMenuToHotspot(menu, menu.TargetHotspot) && !MoveUIMenuToHotspot(menu, KickStarter.playerInteraction.GetActiveHotspot()) && AreInteractionMenusOn())
					{
						MoveUIMenuToHotspot(menu, KickStarter.playerInteraction.GetLastOrActiveHotspot());
					}
					break;
				case UIPositionType.AboveSpeakingCharacter:
				{
					Char obj = null;
					bool flag = true;
					if (dupSpeechMenus.Contains(menu))
					{
						if (menu.speech != null)
						{
							obj = menu.speech.GetSpeakingCharacter();
							if (!menu.moveWithCharacter)
							{
								flag = !menu.HasMoved;
							}
						}
					}
					else
					{
						obj = KickStarter.dialog.GetSpeakingCharacter();
					}
					if (obj != null && flag)
					{
						if (menu.RuntimeCanvas != null && menu.RuntimeCanvas.renderMode == RenderMode.WorldSpace)
						{
							menu.SetCentre3D(obj.GetSpeechWorldPosition());
							break;
						}
						zero = obj.GetSpeechScreenPosition(menu.fitWithinScreen);
						zero = KickStarter.mainCamera.ConvertRelativeScreenSpaceToUI(zero);
						menu.SetCentre(zero);
					}
					break;
				}
				case UIPositionType.AbovePlayer:
					if ((bool)KickStarter.player)
					{
						if (menu.RuntimeCanvas.renderMode == RenderMode.WorldSpace)
						{
							menu.SetCentre3D(KickStarter.player.GetSpeechWorldPosition());
							break;
						}
						zero = KickStarter.player.GetSpeechScreenPosition(menu.fitWithinScreen);
						zero = KickStarter.mainCamera.ConvertRelativeScreenSpaceToUI(zero);
						menu.SetCentre(zero);
					}
					break;
				case UIPositionType.AppearAtCursorAndFreeze:
					break;
				}
				return;
			}
			if (menu.sizeType == AC_SizeType.Automatic && menu.autoSizeEveryFrame)
			{
				menu.Recalculate();
			}
			if (invertedMouse == Vector2.zero)
			{
				invertedMouse = KickStarter.playerInput.GetInvertedMouse();
			}
			switch (menu.positionType)
			{
			case AC_PositionType.FollowCursor:
			{
				Vector2 vector5 = KickStarter.mainCamera.ConvertToMenuSpace(invertedMouse);
				menu.SetCentre(new Vector2(vector5.x + menu.manualPosition.x / 100f - 0.5f, vector5.y + menu.manualPosition.y / 100f - 0.5f));
				break;
			}
			case AC_PositionType.OnHotspot:
				if (isMouseOverInventory)
				{
					if (menu.TargetInvItem == null && menu.TargetHotspot != null)
					{
						break;
					}
					if (activeCrafting != null)
					{
						if (menu.TargetInvItem != null)
						{
							int itemSlot5 = activeCrafting.GetItemSlot(menu.TargetInvItem.id);
							Vector2 slotCentre = activeInventoryBoxMenu.GetSlotCentre(activeCrafting, itemSlot5);
							Vector2 vector = new Vector2(slotCentre.x / (float)ACScreen.width, slotCentre.y / (float)ACScreen.height);
							menu.SetCentre(new Vector2(vector.x + menu.manualPosition.x / 100f - 0.5f, vector.y + menu.manualPosition.y / 100f - 0.5f));
						}
						else if (KickStarter.runtimeInventory.hoverItem != null)
						{
							int itemSlot6 = activeCrafting.GetItemSlot(KickStarter.runtimeInventory.hoverItem.id);
							Vector2 slotCentre2 = activeInventoryBoxMenu.GetSlotCentre(activeCrafting, itemSlot6);
							Vector2 vector2 = new Vector2(slotCentre2.x / (float)ACScreen.width, slotCentre2.y / (float)ACScreen.height);
							menu.SetCentre(new Vector2(vector2.x + menu.manualPosition.x / 100f - 0.5f, vector2.y + menu.manualPosition.y / 100f - 0.5f));
						}
					}
					else if (activeInventoryBox != null)
					{
						if (menu.TargetInvItem != null)
						{
							int itemSlot7 = activeInventoryBox.GetItemSlot(menu.TargetInvItem.id);
							Vector2 slotCentre3 = activeInventoryBoxMenu.GetSlotCentre(activeInventoryBox, itemSlot7);
							Vector2 vector3 = new Vector2(slotCentre3.x / (float)ACScreen.width, slotCentre3.y / (float)ACScreen.height);
							menu.SetCentre(new Vector2(vector3.x + menu.manualPosition.x / 100f - 0.5f, vector3.y + menu.manualPosition.y / 100f - 0.5f));
						}
						else if (KickStarter.runtimeInventory.hoverItem != null)
						{
							int itemSlot8 = activeInventoryBox.GetItemSlot(KickStarter.runtimeInventory.hoverItem.id);
							Vector2 slotCentre4 = activeInventoryBoxMenu.GetSlotCentre(activeInventoryBox, itemSlot8);
							Vector2 vector4 = new Vector2(slotCentre4.x / (float)ACScreen.width, slotCentre4.y / (float)ACScreen.height);
							menu.SetCentre(new Vector2(vector4.x + menu.manualPosition.x / 100f - 0.5f, vector4.y + menu.manualPosition.y / 100f - 0.5f));
						}
					}
				}
				else if (menu.TargetInvItem == null && !MoveMenuToHotspot(menu, menu.TargetHotspot) && !MoveMenuToHotspot(menu, KickStarter.playerInteraction.GetActiveHotspot()) && AreInteractionMenusOn())
				{
					MoveMenuToHotspot(menu, KickStarter.playerInteraction.GetLastOrActiveHotspot());
				}
				break;
			case AC_PositionType.AboveSpeakingCharacter:
			{
				Char obj2 = null;
				bool flag2 = true;
				if (dupSpeechMenus.Contains(menu))
				{
					if (menu.speech != null)
					{
						obj2 = menu.speech.GetSpeakingCharacter();
						if (!menu.moveWithCharacter)
						{
							flag2 = !menu.HasMoved;
						}
					}
				}
				else
				{
					obj2 = KickStarter.dialog.GetSpeakingCharacter();
				}
				if (obj2 != null && flag2)
				{
					Vector2 speechScreenPosition2 = obj2.GetSpeechScreenPosition(menu.fitWithinScreen);
					menu.SetCentre(new Vector2(speechScreenPosition2.x + menu.manualPosition.x / 100f - 0.5f, speechScreenPosition2.y + menu.manualPosition.y / 100f - 0.5f), true);
				}
				break;
			}
			case AC_PositionType.AbovePlayer:
				if ((bool)KickStarter.player)
				{
					Vector2 speechScreenPosition = KickStarter.player.GetSpeechScreenPosition(menu.fitWithinScreen);
					menu.SetCentre(new Vector2(speechScreenPosition.x + menu.manualPosition.x / 100f - 0.5f, speechScreenPosition.y + menu.manualPosition.y / 100f - 0.5f), true);
				}
				break;
			case AC_PositionType.AppearAtCursorAndFreeze:
				break;
			}
		}

		protected bool MoveMenuToHotspot(Menu menu, Hotspot hotspot)
		{
			if (hotspot != null)
			{
				Vector2 iconScreenPosition = hotspot.GetIconScreenPosition();
				Vector2 vector = new Vector2(iconScreenPosition.x / (float)ACScreen.width, 1f - iconScreenPosition.y / (float)ACScreen.height);
				menu.SetCentre(new Vector2(vector.x + menu.manualPosition.x / 100f - 0.5f, vector.y + menu.manualPosition.y / 100f - 0.5f));
				return true;
			}
			return false;
		}

		protected bool MoveUIMenuToHotspot(Menu menu, Hotspot hotspot)
		{
			if (hotspot != null)
			{
				if (menu.RuntimeCanvas == null)
				{
					ACDebug.LogWarning("Cannot move UI menu " + menu.title + " as no Canvas is assigned!");
				}
				else if (menu.RuntimeCanvas.renderMode == RenderMode.WorldSpace)
				{
					menu.SetCentre3D(hotspot.GetIconPosition());
				}
				else
				{
					Vector2 iconScreenPosition = hotspot.GetIconScreenPosition();
					Vector2 vector = new Vector2(iconScreenPosition.x / (float)ACScreen.width, 1f - iconScreenPosition.y / (float)ACScreen.height);
					vector = new Vector2(vector.x * (float)ACScreen.width, (1f - vector.y) * (float)ACScreen.height);
					menu.SetCentre(vector);
				}
				return true;
			}
			return false;
		}

		public void UpdateMenu(Menu menu, int languageNumber = 0, bool justPosition = false, bool updateElements = true)
		{
			Vector2 invertedMouse = KickStarter.playerInput.GetInvertedMouse();
			UpdateMenuPosition(menu, invertedMouse);
			if (justPosition)
			{
				return;
			}
			menu.HandleTransition();
			if (menu.IsEnabled() && !KickStarter.playerMenus.IsCyclingInteractionMenu())
			{
				KickStarter.playerInput.InputControlMenu(menu);
			}
			if (menu.IsOn() && menu.CanCurrentlyKeyboardControl())
			{
				foundCanKeyboardControl = true;
			}
			switch (menu.appearType)
			{
			case AppearType.Manual:
				if (menu.IsVisible() && !menu.isLocked && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
				break;
			case AppearType.OnViewDocument:
				if (KickStarter.runtimeDocuments.ActiveDocument != null && !menu.isLocked && (!KickStarter.stateHandler.IsPaused() || menu.IsBlocking()))
				{
					if (menu.IsVisible() && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
					menu.TurnOn();
				}
				else
				{
					menu.TurnOff();
				}
				break;
			case AppearType.DuringGameplay:
				if (KickStarter.stateHandler.IsInGameplay() && !menu.isLocked)
				{
					if (menu.IsOff())
					{
						menu.TurnOn();
					}
					if (menu.IsOn() && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused || KickStarter.stateHandler.gameState == GameState.DialogOptions)
				{
					menu.TurnOff();
				}
				else if (menu.IsOn() && KickStarter.actionListManager.IsGameplayBlocked())
				{
					menu.TurnOff();
				}
				break;
			case AppearType.DuringGameplayAndConversations:
				if (!menu.isLocked && (KickStarter.stateHandler.gameState == GameState.Normal || KickStarter.stateHandler.gameState == GameState.DialogOptions))
				{
					if (menu.IsOff())
					{
						menu.TurnOn();
					}
					if (menu.IsOn() && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff();
				}
				else if (menu.IsOn() && KickStarter.actionListManager.IsGameplayBlocked())
				{
					menu.TurnOff();
				}
				break;
			case AppearType.ExceptWhenPaused:
				if (KickStarter.stateHandler.gameState != GameState.Paused && !menu.isLocked)
				{
					if (menu.IsOff())
					{
						menu.TurnOn();
					}
					if (menu.IsOn() && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff();
				}
				break;
			case AppearType.DuringCutscene:
				if (KickStarter.stateHandler.gameState == GameState.Cutscene && !menu.isLocked)
				{
					if (menu.IsOff())
					{
						menu.TurnOn();
					}
					if (menu.IsOn() && menu.IsPointInside(invertedMouse))
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.TurnOff();
				}
				else if (menu.IsOn() && !KickStarter.actionListManager.IsGameplayBlocked())
				{
					menu.TurnOff();
				}
				break;
			case AppearType.MouseOver:
				if (menu.pauseWhenEnabled)
				{
					if ((KickStarter.stateHandler.gameState == GameState.Paused || KickStarter.stateHandler.IsInGameplay()) && !menu.isLocked && menu.IsPointInside(invertedMouse) && KickStarter.playerInput.GetDragState() != DragState.Moveable)
					{
						if (menu.IsOff())
						{
							menu.TurnOn();
						}
						if (!menu.ignoreMouseClicks)
						{
							foundMouseOverMenu = true;
						}
					}
					else
					{
						menu.TurnOff();
					}
				}
				else if (KickStarter.stateHandler.IsInGameplay() && !menu.isLocked && menu.IsPointInside(invertedMouse) && KickStarter.playerInput.GetDragState() != DragState.Moveable)
				{
					if (menu.IsOff())
					{
						menu.TurnOn();
					}
					if (!menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff();
				}
				else
				{
					menu.TurnOff();
				}
				break;
			case AppearType.OnContainer:
				if (KickStarter.playerInput.activeContainer != null && !menu.isLocked && (KickStarter.stateHandler.IsInGameplay() || (KickStarter.stateHandler.gameState == GameState.Paused && menu.IsBlocking())))
				{
					if (menu.IsVisible() && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverMenu = true;
					}
					menu.TurnOn();
				}
				else
				{
					menu.TurnOff();
				}
				break;
			case AppearType.DuringConversation:
				if (menu.IsEnabled() && !menu.isLocked && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
				if (KickStarter.playerInput.IsInConversation() && KickStarter.stateHandler.gameState == GameState.DialogOptions)
				{
					menu.TurnOn();
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					menu.ForceOff();
				}
				else
				{
					menu.TurnOff();
				}
				break;
			case AppearType.OnInputKey:
				if (menu.IsEnabled() && !menu.isLocked && menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
				{
					foundMouseOverMenu = true;
				}
				try
				{
					if (!KickStarter.playerInput.InputGetButtonDown(menu.toggleKey, true))
					{
						break;
					}
					if (!menu.IsEnabled())
					{
						if (KickStarter.stateHandler.gameState == GameState.Paused)
						{
							CrossFade(menu);
						}
						else
						{
							menu.TurnOn();
						}
					}
					else
					{
						menu.TurnOff();
					}
				}
				catch
				{
					if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
					{
						ACDebug.LogWarning("No '" + menu.toggleKey + "' button exists - please define one in the Input Manager.");
					}
				}
				break;
			case AppearType.OnHotspot:
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && !menu.isLocked && KickStarter.runtimeInventory.SelectedItem == null)
				{
					Hotspot activeHotspot = KickStarter.playerInteraction.GetActiveHotspot();
					if (activeHotspot != null)
					{
						menu.HideInteractions();
						if (activeHotspot.HasContextUse())
						{
							menu.MatchUseInteraction(activeHotspot.GetFirstUseButton());
						}
						if (activeHotspot.HasContextLook())
						{
							menu.MatchLookInteraction();
						}
						menu.Recalculate();
					}
				}
				if (menu.GetsDuplicated())
				{
					if (KickStarter.stateHandler.gameState == GameState.Cutscene)
					{
						menu.TurnOff();
					}
					else if (menu.TargetInvItem != null)
					{
						InvItem hoverItem = KickStarter.runtimeInventory.hoverItem;
						if (hoverItem != null && menu.TargetInvItem == hoverItem)
						{
							menu.TurnOn();
						}
						else
						{
							menu.TurnOff();
						}
					}
					else if (menu.TargetHotspot != null)
					{
						Hotspot activeHotspot2 = KickStarter.playerInteraction.GetActiveHotspot();
						if (activeHotspot2 != null && menu.TargetHotspot == activeHotspot2)
						{
							menu.TurnOn();
						}
						else
						{
							menu.TurnOff();
						}
					}
					else
					{
						menu.TurnOff();
					}
				}
				else if (!string.IsNullOrEmpty(GetHotspotLabel()) && !menu.isLocked && KickStarter.stateHandler.gameState != GameState.Cutscene)
				{
					menu.TurnOn();
					if (menu.IsUnityUI())
					{
						UpdateMenuPosition(menu, invertedMouse);
					}
				}
				else
				{
					menu.TurnOff();
				}
				break;
			case AppearType.OnInteraction:
				if (KickStarter.player != null && KickStarter.settingsManager.hotspotDetection != HotspotDetection.MouseOver && KickStarter.player.hotspotDetector != null && KickStarter.settingsManager.closeInteractionMenusIfPlayerLeavesVicinity && menu.TargetHotspot != null && !KickStarter.player.hotspotDetector.IsHotspotInTrigger(menu.TargetHotspot))
				{
					menu.TurnOff();
					return;
				}
				if (KickStarter.settingsManager.CanClickOffInteractionMenu())
				{
					if (menu.IsEnabled() && (KickStarter.stateHandler.IsInGameplay() || menu.pauseWhenEnabled || (KickStarter.stateHandler.IsPaused() && menu.TargetInvItem != null && menu.GetGameStateWhenTurnedOn() == GameState.Paused)))
					{
						interactionMenuPauses = menu.pauseWhenEnabled;
						if (menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
						{
							foundMouseOverInteractionMenu = true;
						}
						else if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick && KickStarter.settingsManager.ShouldCloseInteractionMenu())
						{
							KickStarter.playerInput.ResetMouseClick();
							menu.TurnOff();
						}
					}
					else if (KickStarter.stateHandler.gameState == GameState.Paused)
					{
						menu.ForceOff();
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot() == null)
					{
						menu.TurnOff();
					}
				}
				else if (menu.IsEnabled() && (KickStarter.stateHandler.IsInGameplay() || menu.pauseWhenEnabled || (KickStarter.stateHandler.IsPaused() && menu.TargetInvItem != null && menu.GetGameStateWhenTurnedOn() == GameState.Paused)))
				{
					if (menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks)
					{
						foundMouseOverInteractionMenu = true;
					}
					else if (!menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks && KickStarter.playerInteraction.GetActiveHotspot() == null && KickStarter.runtimeInventory.hoverItem == null && (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || KickStarter.settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenuOrHotspot))
					{
						menu.TurnOff();
					}
					else if (!menu.IsPointInside(invertedMouse) && !menu.ignoreMouseClicks && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.cancelInteractions == CancelInteractions.CursorLeavesMenu && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.ClickingMenu && !menu.IsFadingIn())
					{
						menu.TurnOff();
					}
					else if (KickStarter.playerInteraction.GetActiveHotspot() == null && KickStarter.runtimeInventory.hoverItem == null && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingMenuAndClickingHotspot)
					{
						menu.TurnOff();
					}
					else if ((KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.CyclingMenuAndClickingHotspot || !(KickStarter.playerInteraction.GetActiveHotspot() != null)) && (KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.CyclingMenuAndClickingHotspot || KickStarter.runtimeInventory.hoverItem == null) && !(KickStarter.playerInteraction.GetActiveHotspot() == null) && KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
					{
						if (KickStarter.runtimeInventory.SelectedItem == null && KickStarter.playerInteraction.GetActiveHotspot() != null && KickStarter.runtimeInventory.hoverItem != null)
						{
							menu.TurnOff();
						}
						else if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem != KickStarter.runtimeInventory.hoverItem)
						{
							menu.TurnOff();
						}
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					if (menu.TargetInvItem == null || menu.GetGameStateWhenTurnedOn() != GameState.Paused)
					{
						menu.ForceOff();
					}
				}
				else if (KickStarter.playerInteraction.GetActiveHotspot() == null)
				{
					menu.TurnOff();
				}
				break;
			case AppearType.WhenSpeechPlays:
			{
				if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					if (!menu.showWhenPaused)
					{
						menu.TurnOff();
					}
					break;
				}
				Speech speech = menu.speech;
				if (!menu.oneMenuPerSpeech)
				{
					speech = KickStarter.dialog.GetLatestSpeech();
				}
				if (speech != null && speech.MenuCanShow(menu))
				{
					if (menu.forceSubtitles || Options.optionsData == null || (Options.optionsData != null && Options.optionsData.showSubtitles) || (KickStarter.speechManager.forceSubtitles && !KickStarter.dialog.FoundAudio()))
					{
						menu.TurnOn();
					}
					else
					{
						menu.TurnOff();
					}
				}
				else
				{
					menu.TurnOff();
				}
				break;
			}
			case AppearType.WhileLoading:
				if (KickStarter.sceneChanger.IsLoading())
				{
					menu.TurnOn();
				}
				else
				{
					menu.TurnOff();
				}
				break;
			case AppearType.WhileInventorySelected:
				if (KickStarter.runtimeInventory.SelectedItem != null)
				{
					menu.TurnOn();
				}
				else
				{
					menu.TurnOff();
				}
				break;
			}
			if (updateElements)
			{
				UpdateElements(menu, languageNumber, justPosition);
			}
		}

		protected void UpdateElements(Menu menu, int languageNumber, bool justDisplay = false)
		{
			if ((!menu.HasTransition() && menu.IsFading()) || (!menu.updateWhenFadeOut && menu.IsFadingOut()))
			{
				return;
			}
			if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointInside(KickStarter.playerInput.GetInvertedMouse()) && !menu.ignoreMouseClicks)
			{
				menuIdentifier = menu.IDString;
				mouseOverMenu = menu;
				mouseOverElement = null;
				mouseOverElementSlot = 0;
			}
			for (int i = 0; i < menu.NumElements; i++)
			{
				if ((menu.elements[i].GetNumSlots() == 0 || !menu.elements[i].IsVisible) && menu.menuSource != MenuSource.AdventureCreator)
				{
					menu.elements[i].HideAllUISlots();
				}
				for (int j = 0; j < menu.elements[i].GetNumSlots(); j++)
				{
					bool flag = KickStarter.stateHandler.gameState != GameState.Cutscene && SlotIsInteractive(menu, i, j);
					menu.elements[i].PreDisplay(j, languageNumber, flag);
					if (justDisplay)
					{
						continue;
					}
					if (flag)
					{
						string value = KickStarter.eventManager.Call_OnRequestMenuElementHotspotLabel(menu, menu.elements[i], j, languageNumber);
						if (string.IsNullOrEmpty(value))
						{
							value = menu.elements[i].GetHotspotLabelOverride(j, languageNumber);
						}
						if (!string.IsNullOrEmpty(value))
						{
							hotspotLabelOverride = value;
						}
					}
					if (menu.IsVisible() && menu.elements[i].IsVisible && menu.elements[i].isClickable && j == 0 && !string.IsNullOrEmpty(menu.elements[i].alternativeInputButton) && KickStarter.playerInput.InputGetButtonDown(menu.elements[i].alternativeInputButton))
					{
						CheckClick(menu, menu.elements[i], j, MouseState.SingleClick);
					}
					if (menu.elements[i] is MenuInput)
					{
						MenuInput menuInput = menu.elements[i] as MenuInput;
						if (selectedInputBox == null && (SlotIsInteractive(menu, i, 0) || !menuInput.requireSelection))
						{
							if (!menu.IsUnityUI())
							{
								SelectInputBox(menuInput);
							}
							selectedInputBoxMenuName = menu.title;
						}
						else if (selectedInputBox == menuInput && !SlotIsInteractive(menu, i, 0) && menuInput.requireSelection && !menu.IsUnityUI())
						{
							DeselectInputBox();
						}
					}
					if (!menu.elements[i].IsVisible || !SlotIsInteractive(menu, i, j))
					{
						continue;
					}
					if ((!interactionMenuIsOn || menu.appearType == AppearType.OnInteraction) && (KickStarter.playerInput.GetDragState() == DragState.None || (KickStarter.playerInput.GetDragState() == DragState.Inventory && CanElementBeDroppedOnto(menu.elements[i]))) && lastElementIdentifier != menu.IDString + menu.elements[i].IDString + j)
					{
						KickStarter.sceneSettings.PlayDefaultSound(menu.elements[i].GetHoverSound(j), false);
					}
					if (!menu.ignoreMouseClicks)
					{
						elementIdentifier = menu.IDString + menu.elements[i].IDString + j;
						mouseOverMenu = menu;
						mouseOverElement = menu.elements[i];
						mouseOverElementSlot = j;
					}
					if (KickStarter.stateHandler.gameState == GameState.Cutscene)
					{
						continue;
					}
					if (menu.elements[i] is MenuInventoryBox)
					{
						if (!KickStarter.stateHandler.IsInGameplay() && KickStarter.stateHandler.gameState != GameState.Paused && (KickStarter.stateHandler.gameState != GameState.DialogOptions || (!KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations)))
						{
							continue;
						}
						if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single && KickStarter.runtimeInventory.SelectedItem == null)
						{
							KickStarter.playerCursor.ResetSelectedCursor();
						}
						MenuInventoryBox menuInventoryBox = (MenuInventoryBox)menu.elements[i];
						if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.HotspotBased)
						{
							if (!menu.ignoreMouseClicks)
							{
								KickStarter.runtimeInventory.UpdateSelectItemModeForMenu(menuInventoryBox, j);
							}
							continue;
						}
						foundMouseOverInventory = true;
						if (isMouseOverInteractionMenu)
						{
							continue;
						}
						if (interactionMenuIsOn && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.ClickingMenu && KickStarter.settingsManager.CanClickOffInteractionMenu())
						{
							return;
						}
						InvItem item = menuInventoryBox.GetItem(j);
						KickStarter.runtimeInventory.SetHoverItem(item, menuInventoryBox);
						if (oldHoverItem != item)
						{
							KickStarter.runtimeInventory.MatchInteractions();
							KickStarter.playerInteraction.RestoreInventoryInteraction();
							activeInventoryBox = menuInventoryBox;
							activeCrafting = null;
							activeInventoryBoxMenu = menu;
							if (interactionMenuIsOn)
							{
								CloseInteractionMenus();
							}
						}
					}
					else if (menu.elements[i] is MenuCrafting && (KickStarter.stateHandler.IsInGameplay() || KickStarter.stateHandler.gameState == GameState.Paused))
					{
						MenuCrafting menuCrafting = (MenuCrafting)menu.elements[i];
						KickStarter.runtimeInventory.SetHoverItem(menuCrafting.GetItem(j), menuCrafting);
						if (KickStarter.runtimeInventory.hoverItem != null)
						{
							activeCrafting = menuCrafting;
							activeInventoryBox = null;
							activeInventoryBoxMenu = menu;
						}
						foundMouseOverInventory = true;
					}
				}
			}
		}

		public bool IsEventSystemSelectingObject(GameObject _gameObject)
		{
			if (eventSystem != null && _gameObject != null && eventSystem.currentSelectedGameObject == _gameObject)
			{
				return true;
			}
			return false;
		}

		public bool IsEventSystemSelectingObject()
		{
			if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
			{
				return true;
			}
			return false;
		}

		public bool DeselectEventSystemMenu(Menu _menu)
		{
			if (eventSystem != null && eventSystem.currentSelectedGameObject != null && _menu.menuSource != MenuSource.AdventureCreator && _menu.RuntimeCanvas != null && _menu.RuntimeCanvas.gameObject != null && eventSystem.currentSelectedGameObject.transform.IsChildOf(_menu.RuntimeCanvas.transform))
			{
				eventSystem.SetSelectedGameObject(null);
				return true;
			}
			return false;
		}

		protected bool SlotIsInteractive(Menu menu, int elementIndex, int slotIndex)
		{
			if (!menu.IsVisible() || !menu.elements[elementIndex].isClickable || !menu.elements[elementIndex].IsVisible)
			{
				return false;
			}
			if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController || (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.CanCurrentlyKeyboardControl() && menu.menuSource == MenuSource.AdventureCreator))
			{
				if (menu.menuSource != MenuSource.AdventureCreator)
				{
					return menu.IsElementSelectedByEventSystem(elementIndex, slotIndex);
				}
				if (KickStarter.stateHandler.IsInGameplay())
				{
					if (!KickStarter.playerInput.canKeyboardControlMenusDuringGameplay && menu.IsPointerOverSlot(menu.elements[elementIndex], slotIndex, KickStarter.playerInput.GetInvertedMouse()))
					{
						return true;
					}
					if (KickStarter.playerInput.canKeyboardControlMenusDuringGameplay && menu.CanPause() && !menu.pauseWhenEnabled && menu.selected_element == menu.elements[elementIndex] && menu.selected_slot == slotIndex)
					{
						return true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Cutscene)
				{
					if (menu.CanClickInCutscenes() && menu.selected_element == menu.elements[elementIndex] && menu.selected_slot == slotIndex)
					{
						return true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.DialogOptions)
				{
					if (KickStarter.menuManager.keyboardControlWhenDialogOptions)
					{
						if (menu.selected_element == menu.elements[elementIndex] && menu.selected_slot == slotIndex)
						{
							return true;
						}
					}
					else if (menu.IsPointerOverSlot(menu.elements[elementIndex], slotIndex, KickStarter.playerInput.GetInvertedMouse()))
					{
						return true;
					}
				}
				else if (KickStarter.stateHandler.gameState == GameState.Paused)
				{
					if (KickStarter.menuManager.keyboardControlWhenPaused)
					{
						if (menu.selected_element == menu.elements[elementIndex] && menu.selected_slot == slotIndex)
						{
							return true;
						}
					}
					else if (menu.IsPointerOverSlot(menu.elements[elementIndex], slotIndex, KickStarter.playerInput.GetInvertedMouse()))
					{
						return true;
					}
				}
			}
			else
			{
				if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
				{
					return menu.IsPointerOverSlot(menu.elements[elementIndex], slotIndex, KickStarter.playerInput.GetInvertedMouse());
				}
				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					return menu.IsPointerOverSlot(menu.elements[elementIndex], slotIndex, KickStarter.playerInput.GetInvertedMouse());
				}
			}
			return false;
		}

		protected void CheckClicks(Menu menu)
		{
			if (!menu.HasTransition() && menu.IsFading())
			{
				return;
			}
			if (KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && menu.IsPointInside(KickStarter.playerInput.GetInvertedMouse()) && !menu.ignoreMouseClicks)
			{
				menuIdentifier = menu.IDString;
				mouseOverMenu = menu;
				mouseOverElement = null;
				mouseOverElementSlot = 0;
			}
			for (int i = 0; i < menu.NumElements; i++)
			{
				if (!menu.elements[i].IsVisible)
				{
					continue;
				}
				for (int j = 0; j < menu.elements[i].GetNumSlots(); j++)
				{
					if (!SlotIsInteractive(menu, i, j))
					{
						continue;
					}
					if (!menu.IsUnityUI() && KickStarter.playerInput.GetMouseState() != MouseState.Normal && (KickStarter.playerInput.GetDragState() == DragState.None || KickStarter.playerInput.GetDragState() == DragState.Menu))
					{
						if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || KickStarter.playerInput.GetMouseState() == MouseState.LetGo || KickStarter.playerInput.GetMouseState() == MouseState.RightClick)
						{
							if (!(menu.elements[i] is MenuInput))
							{
								DeselectInputBox();
							}
							CheckClick(menu, menu.elements[i], j, KickStarter.playerInput.GetMouseState());
						}
						else if (KickStarter.playerInput.GetMouseState() == MouseState.HeldDown)
						{
							CheckContinuousClick(menu, menu.elements[i], j, KickStarter.playerInput.GetMouseState());
						}
					}
					else if (menu.IsUnityUI() && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.settingsManager.InventoryDragDrop && KickStarter.playerInput.GetMouseState() == MouseState.HeldDown && KickStarter.playerInput.GetDragState() == DragState.None)
					{
						if (menu.elements[i] is MenuInventoryBox || menu.elements[i] is MenuCrafting)
						{
							CheckClick(menu, menu.elements[i], j, MouseState.SingleClick);
						}
					}
					else if (KickStarter.playerInteraction.IsDroppingInventory() && CanElementBeDroppedOnto(menu.elements[i]))
					{
						if (menu.IsUnityUI() && KickStarter.settingsManager.InventoryDragDrop && (menu.elements[i] is MenuInventoryBox || menu.elements[i] is MenuCrafting))
						{
							menu.elements[i].ProcessClick(menu, j, MouseState.SingleClick);
						}
						else if (!menu.IsUnityUI())
						{
							DeselectInputBox();
							CheckClick(menu, menu.elements[i], j, MouseState.SingleClick);
						}
					}
					else if (menu.IsUnityUI() && KickStarter.playerInput.GetMouseState() == MouseState.HeldDown)
					{
						CheckContinuousClick(menu, menu.elements[i], j, KickStarter.playerInput.GetMouseState());
					}
				}
			}
		}

		public void RefreshDialogueOptions()
		{
			Menu[] array = GetMenus(true).ToArray();
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				menu.RefreshDialogueOptions();
			}
		}

		public void UpdateLoadingMenus()
		{
			int language = Options.GetLanguage();
			for (int i = 0; i < menus.Count; i++)
			{
				if (menus[i].appearType == AppearType.WhileLoading)
				{
					UpdateMenu(menus[i], language, false, menus[i].IsEnabled());
				}
			}
		}

		public void CheckForInput()
		{
			if (!(Time.time > 0f))
			{
				return;
			}
			if (customMenus != null)
			{
				for (int num = customMenus.Count - 1; num >= 0; num--)
				{
					if (customMenus[num].IsEnabled() && !customMenus[num].ignoreMouseClicks)
					{
						CheckClicks(customMenus[num]);
					}
				}
			}
			for (int num2 = menus.Count - 1; num2 >= 0; num2--)
			{
				if (menus[num2].IsEnabled() && !menus[num2].ignoreMouseClicks)
				{
					CheckClicks(menus[num2]);
				}
			}
		}

		public void UpdateAllMenus()
		{
			interactionMenuIsOn = AreInteractionMenusOn();
			if (doResizeMenus > 0)
			{
				doResizeMenus++;
				if (doResizeMenus == 4)
				{
					doResizeMenus = 0;
					List<Menu> list = GetMenus(true);
					foreach (Menu item in list)
					{
						item.Recalculate();
						KickStarter.mainCamera.SetCameraRect();
						item.Recalculate();
					}
				}
			}
			if (!(Time.time > 0f))
			{
				return;
			}
			int language = Options.GetLanguage();
			if (!interactionMenuIsOn || !isMouseOverInteractionMenu)
			{
				oldHoverItem = KickStarter.runtimeInventory.hoverItem;
				KickStarter.runtimeInventory.hoverItem = null;
			}
			if (KickStarter.stateHandler.gameState == GameState.Paused && Time.timeScale > 0f && KickStarter.stateHandler.IsACEnabled())
			{
				KickStarter.sceneSettings.PauseGame();
			}
			elementIdentifier = string.Empty;
			foundMouseOverMenu = false;
			foundMouseOverInteractionMenu = false;
			foundMouseOverInventory = false;
			foundCanKeyboardControl = false;
			hotspotLabelOverride = string.Empty;
			for (int i = 0; i < menus.Count; i++)
			{
				UpdateMenu(menus[i], language, false, menus[i].IsEnabled());
				if (!menus[i].IsEnabled() && menus[i].IsOff() && menuIdentifier == menus[i].IDString)
				{
					menuIdentifier = string.Empty;
				}
			}
			for (int j = 0; j < dupSpeechMenus.Count; j++)
			{
				UpdateMenu(dupSpeechMenus[j], language);
				if (dupSpeechMenus[j].IsOff() && KickStarter.stateHandler.gameState != GameState.Paused)
				{
					Menu menu = dupSpeechMenus[j];
					dupSpeechMenus.RemoveAt(j);
					if (menu.menuSource != MenuSource.AdventureCreator && menu.RuntimeCanvas != null && menu.RuntimeCanvas.gameObject != null)
					{
						Object.DestroyImmediate(menu.RuntimeCanvas.gameObject);
					}
					Object.DestroyImmediate(menu);
					j = 0;
				}
			}
			for (int k = 0; k < customMenus.Count; k++)
			{
				UpdateMenu(customMenus[k], language, false, customMenus[k].IsEnabled());
				if (customMenus.Count > k && customMenus[k] != null && !customMenus[k].IsEnabled() && customMenus[k].IsOff() && menuIdentifier == customMenus[k].IDString)
				{
					menuIdentifier = string.Empty;
				}
			}
			UpdatePauseBackground(ArePauseMenusOn());
			isMouseOverMenu = foundMouseOverMenu;
			isMouseOverInteractionMenu = foundMouseOverInteractionMenu;
			isMouseOverInventory = foundMouseOverInventory;
			canKeyboardControl = foundCanKeyboardControl;
			if (mouseOverMenu != null && (lastElementIdentifier != elementIdentifier || lastMenuIdentifier != menuIdentifier))
			{
				KickStarter.eventManager.Call_OnMouseOverMenuElement(mouseOverMenu, mouseOverElement, mouseOverElementSlot);
			}
			lastElementIdentifier = elementIdentifier;
			lastMenuIdentifier = menuIdentifier;
			UpdateAllMenusAgain();
		}

		protected void UpdateAllMenusAgain()
		{
			int language = Options.GetLanguage();
			for (int i = 0; i < menus.Count; i++)
			{
				UpdateMenu(menus[i], language, true, menus[i].IsEnabled());
			}
		}

		public void CheckCrossfade(Menu _menu)
		{
			if (crossFadeFrom == _menu && crossFadeTo != null)
			{
				crossFadeFrom.ForceOff();
				crossFadeTo.TurnOn();
				crossFadeTo = null;
			}
		}

		public virtual void SelectInputBox(MenuInput input)
		{
			selectedInputBox = input;
		}

		public void OnTurnOffMenu(Menu menu)
		{
			if (!(selectedInputBox != null) || !(menu != null))
			{
				return;
			}
			foreach (MenuElement element in menu.elements)
			{
				if (element != null && element == selectedInputBox)
				{
					DeselectInputBox();
				}
			}
		}

		public void DeselectInputBox(MenuElement menuElement)
		{
			if (selectedInputBox != null && menuElement == selectedInputBox)
			{
				DeselectInputBox();
			}
		}

		protected virtual void DeselectInputBox()
		{
			if ((bool)selectedInputBox)
			{
				selectedInputBox.Deselect();
				selectedInputBox = null;
			}
		}

		protected void CheckClick(Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			if (_menu == null || _element == null)
			{
				return;
			}
			KickStarter.playerInput.ResetMouseClick();
			if (_mouseState == MouseState.LetGo)
			{
				if (_menu.appearType == AppearType.OnInteraction)
				{
					_mouseState = ((KickStarter.settingsManager.ReleaseClickInteractions() && !KickStarter.settingsManager.CanDragCursor() && KickStarter.runtimeInventory.SelectedItem == null) ? MouseState.SingleClick : MouseState.Normal);
				}
				else
				{
					if (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen || KickStarter.settingsManager.CanDragCursor() || KickStarter.runtimeInventory.SelectedItem != null || _element is MenuInventoryBox || _element is MenuCrafting)
					{
						_mouseState = MouseState.Normal;
						return;
					}
					_mouseState = MouseState.SingleClick;
				}
			}
			if (_mouseState != MouseState.Normal)
			{
				_element.ProcessClick(_menu, _slot, _mouseState);
				ResetInventoryBoxes();
			}
		}

		protected void CheckContinuousClick(Menu _menu, MenuElement _element, int _slot, MouseState _mouseState)
		{
			if (_menu.IsClickable())
			{
				_element.ProcessContinuousClick(_menu, _mouseState);
			}
		}

		public void RemoveSpeechFromMenu(Speech speech)
		{
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				if (dupSpeechMenu.speech == speech)
				{
					dupSpeechMenu.speech = null;
				}
			}
		}

		public void AssignSpeechToMenu(Speech speech)
		{
			foreach (Menu menu2 in menus)
			{
				if (menu2.appearType == AppearType.WhenSpeechPlays && menu2.oneMenuPerSpeech && speech.MenuCanShow(menu2) && (menu2.forceSubtitles || Options.optionsData == null || (Options.optionsData != null && Options.optionsData.showSubtitles) || (KickStarter.speechManager.forceSubtitles && !KickStarter.dialog.FoundAudio())))
				{
					Menu menu = ScriptableObject.CreateInstance<Menu>();
					dupSpeechMenus.Add(menu);
					menu.DuplicateInGame(menu2);
					menu.SetSpeech(speech);
					if (menu.IsUnityUI())
					{
						menu.LoadUnityUI();
					}
					menu.Recalculate();
					menu.Initalise();
					menu.TurnOn();
				}
			}
		}

		public Menu[] GetMenusAssignedToSpeech(Speech speech)
		{
			if (speech == null)
			{
				return new Menu[0];
			}
			List<Menu> list = new List<Menu>();
			foreach (Menu menu in menus)
			{
				if (menu.speech == speech)
				{
					list.Add(menu);
				}
			}
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				if (dupSpeechMenu.speech == speech)
				{
					list.Add(dupSpeechMenu);
				}
			}
			return list.ToArray();
		}

		public void CrossFade(Menu _menuTo)
		{
			if (_menuTo.isLocked)
			{
				ACDebug.Log("Cannot crossfade to menu " + _menuTo.title + " as it is locked.");
			}
			else
			{
				if (_menuTo.IsEnabled())
				{
					return;
				}
				crossFadeFrom = null;
				foreach (Menu menu in menus)
				{
					if (menu.IsVisible())
					{
						if (menu.appearType == AppearType.OnHotspot || menu.fadeSpeed <= 0f || !menu.HasTransition())
						{
							menu.ForceOff();
							continue;
						}
						if (menu.appearType == AppearType.DuringConversation && KickStarter.playerInput.IsInConversation())
						{
							ACDebug.LogWarning("Cannot turn off Menu '" + menu.title + "' as a Conversation is currently active.");
							continue;
						}
						menu.TurnOff();
						crossFadeFrom = menu;
					}
					else
					{
						menu.ForceOff();
					}
				}
				if (crossFadeFrom != null)
				{
					crossFadeTo = _menuTo;
				}
				else
				{
					_menuTo.TurnOn();
				}
			}
		}

		public void CloseInteractionMenus()
		{
			SetInteractionMenus(false, null, null);
		}

		protected void SetInteractionMenus(bool turnOn, Hotspot _hotspotFor, InvItem _itemFor)
		{
			if (turnOn)
			{
				KickStarter.playerInput.ResetMouseClick();
			}
			foreach (Menu menu in menus)
			{
				if (menu.appearType == AppearType.OnInteraction)
				{
					if (turnOn)
					{
						InteractionMenuData interactionMenuData = new InteractionMenuData(menu, _hotspotFor, _itemFor);
						interactionMenuPauses = menu.pauseWhenEnabled;
						StopCoroutine("SwapInteractionMenu");
						StartCoroutine("SwapInteractionMenu", interactionMenuData);
					}
					else
					{
						menu.TurnOff();
					}
				}
			}
			if (turnOn)
			{
				KickStarter.eventManager.Call_OnEnableInteractionMenus(_hotspotFor, _itemFor);
			}
		}

		public void EnableInteractionMenus(Hotspot hotspotFor)
		{
			SetInteractionMenus(true, hotspotFor, null);
		}

		public void EnableInteractionMenus(InvItem itemFor)
		{
			SetInteractionMenus(true, null, itemFor);
		}

		protected IEnumerator SwapInteractionMenu(InteractionMenuData interactionMenuData)
		{
			if (interactionMenuData.itemFor == null)
			{
				interactionMenuData.itemFor = KickStarter.runtimeInventory.hoverItem;
			}
			if (interactionMenuData.hotspotFor == null)
			{
				interactionMenuData.hotspotFor = KickStarter.playerInteraction.GetActiveHotspot();
			}
			if (interactionMenuData.itemFor != null && interactionMenuData.menuFor.TargetInvItem != interactionMenuData.itemFor)
			{
				interactionMenuData.menuFor.TurnOff();
			}
			else if (interactionMenuData.hotspotFor != null && interactionMenuData.menuFor.TargetHotspot != interactionMenuData.hotspotFor)
			{
				interactionMenuData.menuFor.TurnOff();
			}
			while (interactionMenuData.menuFor.IsFading())
			{
				yield return new WaitForFixedUpdate();
			}
			KickStarter.playerInteraction.ResetInteractionIndex();
			if (interactionMenuData.itemFor != null)
			{
				interactionMenuData.menuFor.MatchInteractions(interactionMenuData.itemFor, KickStarter.settingsManager.cycleInventoryCursors);
			}
			else if (interactionMenuData.hotspotFor != null)
			{
				interactionMenuData.menuFor.MatchInteractions(interactionMenuData.hotspotFor, KickStarter.settingsManager.cycleInventoryCursors);
			}
			interactionMenuData.menuFor.TurnOn();
		}

		public void DisableHotspotMenus()
		{
			Menu[] array = GetMenus(true).ToArray();
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				if (menu.appearType == AppearType.OnHotspot)
				{
					menu.ForceOff();
				}
			}
		}

		public string GetHotspotLabel()
		{
			if (!string.IsNullOrEmpty(hotspotLabelOverride))
			{
				return hotspotLabelOverride;
			}
			return KickStarter.playerInteraction.InteractionLabel;
		}

		protected void SetStyles(MenuElement element)
		{
			normalStyle.normal.textColor = element.fontColor;
			normalStyle.font = element.font;
			normalStyle.fontSize = element.GetFontSize();
			normalStyle.alignment = TextAnchor.MiddleCenter;
			highlightedStyle.font = element.font;
			highlightedStyle.fontSize = element.GetFontSize();
			highlightedStyle.normal.textColor = element.fontHighlightColor;
			highlightedStyle.normal.background = element.highlightTexture;
			highlightedStyle.alignment = TextAnchor.MiddleCenter;
		}

		protected bool CanElementBeDroppedOnto(MenuElement element)
		{
			if (element is MenuInventoryBox)
			{
				MenuInventoryBox menuInventoryBox = (MenuInventoryBox)element;
				if (menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.Default || menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.Container || menuInventoryBox.inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					return true;
				}
			}
			else if (element is MenuCrafting)
			{
				MenuCrafting menuCrafting = (MenuCrafting)element;
				if (menuCrafting.craftingType == CraftingElementType.Ingredients)
				{
					return true;
				}
			}
			return false;
		}

		protected void OnDestroy()
		{
			menus = null;
		}

		public static List<Menu> GetMenus(bool includeDuplicatesAndCustom = false)
		{
			if ((bool)KickStarter.playerMenus)
			{
				if (KickStarter.playerMenus.menus.Count == 0 && KickStarter.menuManager != null && KickStarter.menuManager.menus.Count > 0)
				{
					ACDebug.LogError("A custom script is calling 'PlayerMenus.GetMenus ()' before the Menus have been initialised - consider adjusting your script's Script Execution Order.");
					return null;
				}
				if (!includeDuplicatesAndCustom)
				{
					return KickStarter.playerMenus.menus;
				}
				List<Menu> list = new List<Menu>();
				foreach (Menu menu in KickStarter.playerMenus.menus)
				{
					list.Add(menu);
				}
				foreach (Menu dupSpeechMenu in KickStarter.playerMenus.dupSpeechMenus)
				{
					list.Add(dupSpeechMenu);
				}
				{
					foreach (Menu customMenu in KickStarter.playerMenus.customMenus)
					{
						list.Add(customMenu);
					}
					return list;
				}
			}
			return null;
		}

		public static Menu GetMenuWithName(string menuName)
		{
			menuName = AdvGame.ConvertTokens(menuName);
			if ((bool)KickStarter.playerMenus && KickStarter.playerMenus.menus != null)
			{
				if (KickStarter.playerMenus.menus.Count == 0 && KickStarter.menuManager != null && KickStarter.menuManager.menus.Count > 0)
				{
					ACDebug.LogError("A custom script is calling 'PlayerMenus.GetMenuWithName ()' before the Menus have been initialised - consider adjusting your script's Script Execution Order.");
					return null;
				}
				for (int i = 0; i < KickStarter.playerMenus.menus.Count; i++)
				{
					if (KickStarter.playerMenus.menus[i].title == menuName)
					{
						return KickStarter.playerMenus.menus[i];
					}
				}
			}
			ACDebug.LogWarning("Couldn't find menu with the name '" + menuName + "'");
			return null;
		}

		public static MenuElement GetElementWithName(string menuName, string menuElementName)
		{
			if ((bool)KickStarter.playerMenus && KickStarter.playerMenus.menus != null)
			{
				menuName = AdvGame.ConvertTokens(menuName);
				menuElementName = AdvGame.ConvertTokens(menuElementName);
				if (KickStarter.playerMenus.menus.Count == 0 && KickStarter.menuManager != null && KickStarter.menuManager.menus.Count > 0)
				{
					ACDebug.LogError("A custom script is calling 'PlayerMenus.GetElementWithName ()' before the Menus have been initialised - consider adjusting your script's Script Execution Order.");
					return null;
				}
				foreach (Menu menu in KickStarter.playerMenus.menus)
				{
					if (!(menu.title == menuName))
					{
						continue;
					}
					foreach (MenuElement element in menu.elements)
					{
						if (element.title == menuElementName)
						{
							return element;
						}
					}
				}
			}
			ACDebug.LogWarning("Couldn't find menu element '" + menuElementName + "' in a menu named '" + menuName + "'");
			return null;
		}

		public static bool IsSavingLocked(Action _actionToIgnore = null, bool showDebug = false)
		{
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				if (!KickStarter.settingsManager.allowGameplayDuringConversations)
				{
					if (showDebug)
					{
						ACDebug.LogWarning("Cannot save at this time - a Conversation is active.");
					}
					return true;
				}
				if (KickStarter.actionListManager.IsOverrideConversationRunning())
				{
					if (showDebug)
					{
						ACDebug.LogWarning("Cannot save at this time - a Conversation is active.");
					}
					return true;
				}
			}
			if (KickStarter.stateHandler.gameState == GameState.Paused && KickStarter.playerInput.IsInConversation())
			{
				if (showDebug)
				{
					ACDebug.LogWarning("Cannot save at this time - a Conversation is active.");
				}
				return true;
			}
			if (KickStarter.actionListManager.IsGameplayBlocked(_actionToIgnore, showDebug))
			{
				return true;
			}
			if (KickStarter.playerMenus.lockSave)
			{
				ACDebug.LogWarning("Cannot save at this time - saving has been manually locked.");
				return true;
			}
			return false;
		}

		public static void ResetInventoryBoxes()
		{
			if (!KickStarter.playerMenus)
			{
				return;
			}
			for (int i = 0; i < KickStarter.playerMenus.menus.Count; i++)
			{
				Menu menu = KickStarter.playerMenus.menus[i];
				bool flag = false;
				for (int j = 0; j < menu.elements.Count; j++)
				{
					if (menu.elements[j] is MenuInventoryBox)
					{
						flag = true;
					}
				}
				if (flag)
				{
					menu.Recalculate();
				}
			}
			for (int k = 0; k < KickStarter.playerMenus.customMenus.Count; k++)
			{
				Menu menu2 = KickStarter.playerMenus.customMenus[k];
				bool flag2 = false;
				for (int l = 0; l < menu2.elements.Count; l++)
				{
					if (menu2.elements[l] is MenuInventoryBox)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					menu2.Recalculate();
				}
			}
		}

		public static void CreateRecipe()
		{
			Menu[] array = GetMenus(true).ToArray();
			if (array == null)
			{
				return;
			}
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element is MenuCrafting)
					{
						MenuCrafting menuCrafting = (MenuCrafting)element;
						menuCrafting.SetOutput(menu.menuSource);
					}
				}
			}
		}

		public static void ForceOffAllMenus(bool onlyPausing = false)
		{
			Menu[] array = GetMenus(true).ToArray();
			if (array == null)
			{
				return;
			}
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				if (menu.IsEnabled() && (!onlyPausing || (onlyPausing && menu.IsBlocking())))
				{
					menu.ForceOff(true);
				}
			}
		}

		public static void SimulateClick(string menuName, string menuElementName, int slot = 1)
		{
			if ((bool)KickStarter.playerMenus)
			{
				Menu menuWithName = GetMenuWithName(menuName);
				MenuElement elementWithName = GetElementWithName(menuName, menuElementName);
				KickStarter.playerMenus.CheckClick(menuWithName, elementWithName, slot, MouseState.SingleClick);
			}
		}

		public static void SimulateClick(string menuName, MenuElement _element, int _slot = 1)
		{
			if ((bool)KickStarter.playerMenus)
			{
				Menu menuWithName = GetMenuWithName(menuName);
				KickStarter.playerMenus.CheckClick(menuWithName, _element, _slot, MouseState.SingleClick);
			}
		}

		public bool ArePauseMenusOn(Menu excludingMenu = null)
		{
			List<Menu> list = GetMenus(true);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsEnabled() && list[i].IsBlocking() && (excludingMenu == null || list[i] != excludingMenu))
				{
					return true;
				}
			}
			return false;
		}

		public void ForceOffSubtitles(SpeechMenuLimit speechMenuLimit = SpeechMenuLimit.All)
		{
			foreach (Menu menu in menus)
			{
				ForceOffSubtitles(menu, speechMenuLimit);
			}
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				ForceOffSubtitles(dupSpeechMenu, speechMenuLimit);
			}
			foreach (Menu customMenu in customMenus)
			{
				ForceOffSubtitles(customMenu, speechMenuLimit);
			}
		}

		protected void ForceOffSubtitles(Menu menu, SpeechMenuLimit speechMenuLimit)
		{
			if (menu.IsEnabled() && menu.appearType == AppearType.WhenSpeechPlays && (speechMenuLimit == SpeechMenuLimit.All || menu.speech == null || (speechMenuLimit == SpeechMenuLimit.BlockingOnly && menu.speech != null && !menu.speech.isBackground) || (speechMenuLimit == SpeechMenuLimit.BlockingOnly && menu.speech != null && menu.speech.isBackground)))
			{
				menu.ForceOff(true);
			}
		}

		public Menu GetMenuWithCanvas(Canvas canvas)
		{
			if (canvas == null)
			{
				return null;
			}
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				if (dupSpeechMenu.RuntimeCanvas == canvas)
				{
					return dupSpeechMenu;
				}
			}
			foreach (Menu menu in menus)
			{
				if (menu.RuntimeCanvas == canvas)
				{
					return menu;
				}
			}
			foreach (Menu customMenu in customMenus)
			{
				if (customMenu.RuntimeCanvas == canvas)
				{
					return customMenu;
				}
			}
			return null;
		}

		public void RegisterCustomMenu(Menu menu, bool deleteWhenTurnOff = false)
		{
			if (customMenus == null)
			{
				customMenus = new List<Menu>();
			}
			if (customMenus.Contains(menu))
			{
				ACDebug.LogWarning("Already registed custom menu '" + menu.title + "'");
				return;
			}
			customMenus.Add(menu);
			menu.deleteUIWhenTurnOff = deleteWhenTurnOff;
			menu.ID = -1 * (customMenus.IndexOf(menu) * 100 + menu.id);
			ACDebug.Log("Registered custom menu '" + menu.title + "'");
		}

		public void UnregisterCustomMenu(Menu menu, bool showError = true)
		{
			if (customMenus != null && customMenus.Contains(menu))
			{
				customMenus.Remove(menu);
				ACDebug.Log("Unregistered custom menu '" + menu.title + "'");
			}
			else
			{
				ACDebug.LogWarning("Custom menu '" + menu.title + "' is not registered.");
			}
		}

		public Menu[] GetRegisteredCustomMenus()
		{
			return customMenus.ToArray();
		}

		public void DestroyCustomMenus()
		{
			int num;
			for (num = 0; num < customMenus.Count; num++)
			{
				if (customMenus[num].RuntimeCanvas != null && customMenus[num].RuntimeCanvas.gameObject != null)
				{
					Object.Destroy(customMenus[num].RuntimeCanvas.gameObject);
				}
				customMenus.RemoveAt(num);
				num = -1;
			}
		}

		public void RecalculateAll()
		{
			doResizeMenus = 1;
			if ((bool)KickStarter.mainCamera)
			{
				KickStarter.mainCamera.RecalculateRects();
			}
		}

		public void FindFirstSelectedElement(Menu menuToIgnore = null)
		{
			if (eventSystem == null || menus.Count == 0)
			{
				return;
			}
			GameObject gameObject = null;
			for (int num = menus.Count - 1; num >= 0; num--)
			{
				Menu menu = menus[num];
				if ((!(menuToIgnore != null) || !(menu == menuToIgnore)) && menu.IsEnabled())
				{
					gameObject = menu.GetObjectToSelect();
					if (gameObject != null)
					{
						break;
					}
				}
			}
			if (gameObject != null)
			{
				SelectUIElement(gameObject);
			}
		}

		public void SelectUIElement(GameObject objectToSelect)
		{
			StartCoroutine(SelectUIElementCoroutine(objectToSelect));
		}

		protected IEnumerator SelectUIElementCoroutine(GameObject objectToSelect)
		{
			eventSystem.SetSelectedGameObject(null);
			yield return null;
			eventSystem.SetSelectedGameObject(objectToSelect);
		}

		public int GetElementOverCursorID()
		{
			return elementOverCursorID;
		}

		public void SetManualSaveLock(bool state)
		{
			lockSave = state;
		}

		public bool IsMouseOverMenu()
		{
			return isMouseOverMenu;
		}

		public bool IsMouseOverInventory()
		{
			return isMouseOverInventory;
		}

		public bool IsMouseOverInteractionMenu()
		{
			return isMouseOverInteractionMenu;
		}

		public bool IsInteractionMenuOn()
		{
			return interactionMenuIsOn;
		}

		public bool IsCyclingInteractionMenu()
		{
			if (interactionMenuIsOn && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				return true;
			}
			return false;
		}

		public bool IsPausingInteractionMenuOn()
		{
			if (interactionMenuIsOn)
			{
				return interactionMenuPauses;
			}
			return false;
		}

		public void MakeUIInteractive()
		{
			Menu[] array = GetMenus(true).ToArray();
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				menu.MakeUIInteractive();
			}
		}

		public void MakeUINonInteractive()
		{
			Menu[] array = GetMenus(true).ToArray();
			Menu[] array2 = array;
			foreach (Menu menu in array2)
			{
				menu.MakeUINonInteractive();
			}
		}

		public MainData SaveMainData(MainData mainData)
		{
			mainData.menuLockData = CreateMenuLockData();
			mainData.menuVisibilityData = CreateMenuVisibilityData();
			mainData.menuElementVisibilityData = CreateMenuElementVisibilityData();
			mainData.menuJournalData = CreateMenuJournalData();
			return mainData;
		}

		public void LoadMainData(MainData mainData)
		{
			foreach (Menu menu in menus)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (element is MenuInventoryBox)
					{
						MenuInventoryBox menuInventoryBox = (MenuInventoryBox)element;
						menuInventoryBox.ResetOffset();
					}
				}
			}
			AssignMenuLocks(mainData.menuLockData);
			AssignMenuVisibility(mainData.menuVisibilityData);
			AssignMenuElementVisibility(mainData.menuElementVisibilityData);
			AssignMenuJournals(mainData.menuJournalData);
		}

		protected string CreateMenuLockData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Menu menu in menus)
			{
				stringBuilder.Append(menu.IDString);
				stringBuilder.Append(":");
				stringBuilder.Append(menu.isLocked.ToString());
				stringBuilder.Append("|");
			}
			if (menus.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		protected string CreateMenuVisibilityData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (Menu menu in menus)
			{
				if (menu.IsManualControlled())
				{
					flag = true;
					stringBuilder.Append(menu.IDString);
					stringBuilder.Append(":");
					stringBuilder.Append(menu.IsEnabled().ToString());
					stringBuilder.Append("|");
				}
			}
			if (flag)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		protected string CreateMenuElementVisibilityData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Menu menu in menus)
			{
				if (menu.NumElements <= 0)
				{
					continue;
				}
				stringBuilder.Append(menu.IDString);
				stringBuilder.Append(":");
				foreach (MenuElement element in menu.elements)
				{
					stringBuilder.Append(element.IDString);
					stringBuilder.Append("=");
					stringBuilder.Append(element.IsVisible.ToString());
					stringBuilder.Append("+");
				}
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				stringBuilder.Append("|");
			}
			if (menus.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		protected string CreateMenuJournalData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Menu menu in menus)
			{
				foreach (MenuElement element in menu.elements)
				{
					if (!(element is MenuJournal))
					{
						continue;
					}
					MenuJournal menuJournal = (MenuJournal)element;
					stringBuilder.Append(menu.IDString);
					stringBuilder.Append(":");
					stringBuilder.Append(menuJournal.ID);
					stringBuilder.Append(":");
					foreach (JournalPage page in menuJournal.pages)
					{
						stringBuilder.Append(page.lineID);
						stringBuilder.Append("~");
					}
					if (menuJournal.pages.Count > 0)
					{
						stringBuilder.Remove(stringBuilder.Length - 1, 1);
					}
					stringBuilder.Append(":");
					stringBuilder.Append(menuJournal.showPage);
					stringBuilder.Append("|");
				}
			}
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		protected void AssignMenuLocks(string menuLockData)
		{
			if (string.IsNullOrEmpty(menuLockData))
			{
				return;
			}
			string[] array = menuLockData.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				bool result2 = false;
				bool.TryParse(array3[1], out result2);
				foreach (Menu menu in menus)
				{
					if (menu.id == result)
					{
						menu.isLocked = result2;
						break;
					}
				}
			}
		}

		protected void AssignMenuVisibility(string menuVisibilityData)
		{
			if (string.IsNullOrEmpty(menuVisibilityData))
			{
				return;
			}
			string[] array = menuVisibilityData.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				bool result2 = false;
				bool.TryParse(array3[1], out result2);
				foreach (Menu menu in menus)
				{
					if (menu.id != result)
					{
						continue;
					}
					if (!menu.IsManualControlled())
					{
						break;
					}
					if (menu.ShouldTurnOffWhenLoading())
					{
						if (menu.IsOn() && (bool)menu.actionListOnTurnOff)
						{
							ACDebug.LogWarning("The '" + menu.title + "' menu's 'ActionList On Turn Off' (" + menu.actionListOnTurnOff.name + ") was not run because the menu was turned off as a result of loading.  The SavesList element's 'ActionList after loading' can be used to run the same Actions instead.");
						}
						menu.ForceOff(true);
					}
					else if (result2)
					{
						menu.TurnOn(false);
					}
					else
					{
						menu.TurnOff(false);
					}
					break;
				}
			}
		}

		protected void AssignMenuElementVisibility(string menuElementVisibilityData)
		{
			if (string.IsNullOrEmpty(menuElementVisibilityData))
			{
				return;
			}
			string[] array = menuElementVisibilityData.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				foreach (Menu menu in menus)
				{
					if (menu.id != result)
					{
						continue;
					}
					string[] array4 = array3[1].Split("+"[0]);
					string[] array5 = array4;
					foreach (string text2 in array5)
					{
						string[] array6 = text2.Split("="[0]);
						int result2 = 0;
						int.TryParse(array6[0], out result2);
						bool result3 = false;
						bool.TryParse(array6[1], out result3);
						foreach (MenuElement element in menu.elements)
						{
							if (element.ID == result2 && element.IsVisible != result3)
							{
								element.IsVisible = result3;
								break;
							}
						}
					}
					menu.ResetVisibleElements();
					menu.Recalculate();
					break;
				}
			}
		}

		protected bool AreInteractionMenusOn()
		{
			for (int i = 0; i < menus.Count; i++)
			{
				if (menus[i].appearType == AppearType.OnInteraction && menus[i].IsEnabled() && !menus[i].IsFadingOut())
				{
					return true;
				}
			}
			return false;
		}

		protected void AssignMenuJournals(string menuJournalData)
		{
			if (string.IsNullOrEmpty(menuJournalData))
			{
				return;
			}
			string[] array = menuJournalData.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				int result2 = 0;
				int.TryParse(array3[1], out result2);
				foreach (Menu menu in menus)
				{
					if (menu.id != result)
					{
						continue;
					}
					foreach (MenuElement element in menu.elements)
					{
						if (element.ID != result2 || !(element is MenuJournal))
						{
							continue;
						}
						MenuJournal menuJournal = (MenuJournal)element;
						bool flag = false;
						string[] array4 = array3[2].Split("~"[0]);
						string[] array5 = array4;
						foreach (string text2 in array5)
						{
							int result3 = -1;
							string[] array6 = text2.Split("*"[0]);
							int.TryParse(array6[0], out result3);
							if (array6.Length > 1)
							{
								if (!flag)
								{
									menuJournal.pages = new List<JournalPage>();
									menuJournal.showPage = 1;
									flag = true;
								}
								menuJournal.pages.Add(new JournalPage(result3, array6[1]));
							}
							else if (result3 >= 0)
							{
								if (!flag)
								{
									menuJournal.pages = new List<JournalPage>();
									menuJournal.showPage = 1;
									flag = true;
								}
								SpeechLine line = KickStarter.speechManager.GetLine(result3);
								if (line != null && line.textType == AC_TextType.JournalEntry)
								{
									menuJournal.pages.Add(new JournalPage(result3, line.text));
								}
							}
						}
						if (flag && array3.Length > 3)
						{
							int result4 = 1;
							int.TryParse(array3[3], out result4);
							if (result4 > menuJournal.pages.Count)
							{
								result4 = menuJournal.pages.Count;
							}
							else if (result4 < 1)
							{
								result4 = 1;
							}
							menuJournal.showPage = result4;
						}
						break;
					}
				}
			}
		}

		protected void OnApplicationQuit()
		{
			if (!(KickStarter.playerMenus != null))
			{
				return;
			}
			foreach (Menu menu in KickStarter.playerMenus.menus)
			{
				if (!(menu != null))
				{
					continue;
				}
				foreach (MenuElement element in menu.elements)
				{
					if (element != null && element is MenuGraphic)
					{
						MenuGraphic menuGraphic = (MenuGraphic)element;
						if (menuGraphic.graphic != null)
						{
							menuGraphic.graphic.ClearCache();
						}
					}
				}
			}
		}

		public virtual void PreScreenshotBackup()
		{
			foreach (Menu menu in menus)
			{
				menu.PreScreenshotBackup();
			}
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				dupSpeechMenu.PreScreenshotBackup();
			}
			foreach (Menu customMenu in customMenus)
			{
				customMenu.PreScreenshotBackup();
			}
		}

		public virtual void PostScreenshotBackup()
		{
			foreach (Menu menu in menus)
			{
				menu.PostScreenshotBackup();
			}
			foreach (Menu dupSpeechMenu in dupSpeechMenus)
			{
				dupSpeechMenu.PostScreenshotBackup();
			}
			foreach (Menu customMenu in customMenus)
			{
				customMenu.PostScreenshotBackup();
			}
		}
	}
}
