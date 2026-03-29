using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_cursor.html")]
	public class PlayerCursor : MonoBehaviour
	{
		protected Menu limitCursorToMenu;

		protected int selectedCursor = -10;

		protected bool showCursor;

		protected bool canShowHardwareCursor;

		protected float pulse;

		protected int pulseDirection;

		protected CursorIconBase activeIcon;

		protected CursorIconBase activeLookIcon;

		protected string lastCursorName;

		protected Texture2D currentCursorTexture2D;

		protected Texture currentCursorTexture;

		protected bool contextCycleExamine;

		protected int manualCursorID = -1;

		protected bool isDrawingHiddenCursor;

		protected bool forceOffCursor;

		public bool ForceOffCursor
		{
			set
			{
				forceOffCursor = value;
			}
		}

		public bool ContextCycleExamine
		{
			get
			{
				return contextCycleExamine;
			}
		}

		protected int SelectedCursor
		{
			set
			{
				if (selectedCursor != value)
				{
					selectedCursor = value;
					if (KickStarter.eventManager != null)
					{
						KickStarter.eventManager.Call_OnChangeCursorMode(selectedCursor);
					}
				}
			}
		}

		public Menu LimitCursorToMenu
		{
			get
			{
				return limitCursorToMenu;
			}
			set
			{
				limitCursorToMenu = value;
			}
		}

		protected void Start()
		{
			if (KickStarter.cursorManager != null && KickStarter.cursorManager.cursorDisplay != CursorDisplay.Never && KickStarter.cursorManager.allowMainCursor && (KickStarter.cursorManager.pointerIcon == null || KickStarter.cursorManager.pointerIcon.texture == null))
			{
				ACDebug.LogWarning("Main cursor has no texture - please assign one in the Cursor Manager.");
			}
			SelectedCursor = -1;
		}

		public void UpdateCursor()
		{
			if (KickStarter.cursorManager.cursorRendering == CursorRendering.Software)
			{
				bool flag = false;
				flag = canShowHardwareCursor && KickStarter.playerInput.GetDragState() != DragState.Moveable && (((bool)KickStarter.settingsManager && (bool)KickStarter.cursorManager && (!KickStarter.cursorManager.allowMainCursor || KickStarter.cursorManager.pointerIcon.texture == null) && (KickStarter.runtimeInventory.SelectedItem == null || KickStarter.cursorManager.inventoryHandling == InventoryHandling.ChangeHotspotLabel) && KickStarter.settingsManager.inputMethod == InputMethod.MouseAndKeyboard && KickStarter.stateHandler.gameState != GameState.Cutscene) || ((KickStarter.cursorManager == null) ? true : false));
				SetCursorVisibility(flag);
			}
			if ((bool)KickStarter.settingsManager && (bool)KickStarter.stateHandler)
			{
				if (forceOffCursor)
				{
					showCursor = false;
				}
				else if (KickStarter.stateHandler.gameState == GameState.Cutscene)
				{
					if (KickStarter.cursorManager.waitIcon.texture != null)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else if (KickStarter.stateHandler.gameState != GameState.Normal && KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen)
				{
					if (KickStarter.stateHandler.gameState == GameState.Paused && !KickStarter.menuManager.keyboardControlWhenPaused)
					{
						showCursor = true;
					}
					else if (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.menuManager.keyboardControlWhenDialogOptions)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else if ((bool)KickStarter.cursorManager)
				{
					if (KickStarter.stateHandler.gameState == GameState.Paused && (KickStarter.cursorManager.cursorDisplay == CursorDisplay.OnlyWhenPaused || KickStarter.cursorManager.cursorDisplay == CursorDisplay.Always))
					{
						showCursor = true;
					}
					else if (KickStarter.playerInput.GetDragState() == DragState.Moveable && KickStarter.cursorManager.hideCursorWhenDraggingMoveables)
					{
						showCursor = false;
					}
					else if (KickStarter.stateHandler.IsInGameplay() || KickStarter.stateHandler.gameState == GameState.DialogOptions)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else
				{
					showCursor = true;
				}
				if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.CustomScript)
				{
					if (KickStarter.stateHandler.IsInGameplay() && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.cursorManager != null && ((KickStarter.cursorManager.cycleCursors && KickStarter.playerInput.GetMouseState() == MouseState.RightClick) || KickStarter.playerInput.InputGetButtonDown("CycleCursors")))
					{
						CycleCursors();
					}
					else if (KickStarter.stateHandler.IsInGameplay() && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot && (KickStarter.playerInput.GetMouseState() == MouseState.RightClick || KickStarter.playerInput.InputGetButtonDown("CycleCursors")))
					{
						KickStarter.playerInteraction.SetNextInteraction();
					}
					else if (KickStarter.stateHandler.IsInGameplay() && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.playerInput.InputGetButtonDown("CycleCursorsBack"))
					{
						KickStarter.playerInteraction.SetPreviousInteraction();
					}
					else if (CanCycleContextSensitiveMode() && KickStarter.playerInput.GetMouseState() == MouseState.RightClick)
					{
						Hotspot activeHotspot = KickStarter.playerInteraction.GetActiveHotspot();
						if (activeHotspot != null)
						{
							if (activeHotspot.HasContextUse() && activeHotspot.HasContextLook())
							{
								KickStarter.playerInput.ResetMouseClick();
								contextCycleExamine = !contextCycleExamine;
							}
						}
						else if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.runtimeInventory.hoverItem.lookActionList != null)
						{
							KickStarter.playerInput.ResetMouseClick();
							contextCycleExamine = !contextCycleExamine;
						}
					}
				}
			}
			if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
			{
				SetCursorVisibility(showCursor);
				DrawCursor();
			}
		}

		public void SetSelectedCursorID(int _cursorID)
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.CustomScript)
			{
				manualCursorID = _cursorID;
			}
			else
			{
				ACDebug.LogWarning("The cursor ID can only be set manually if the 'Interaction method' is set to 'Custom Script'");
			}
		}

		public void DrawCursor()
		{
			if (!showCursor)
			{
				if (!isDrawingHiddenCursor)
				{
					activeIcon = (activeLookIcon = null);
					SetHardwareCursor(null, Vector2.zero);
					isDrawingHiddenCursor = true;
				}
				return;
			}
			isDrawingHiddenCursor = false;
			if (KickStarter.playerInput.IsCursorLocked() && KickStarter.settingsManager.hideLockedCursor)
			{
				canShowHardwareCursor = false;
				return;
			}
			GUI.depth = -1;
			canShowHardwareCursor = true;
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.CustomScript)
			{
				ShowCycleCursor(manualCursorID);
				return;
			}
			if (KickStarter.runtimeInventory.SelectedItem != null)
			{
				SelectedCursor = -2;
				canShowHardwareCursor = false;
			}
			else if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && KickStarter.cursorManager.allowInteractionCursorForInventory && KickStarter.runtimeInventory.hoverItem != null)
				{
					ShowContextIcons(KickStarter.runtimeInventory.hoverItem);
					return;
				}
				if (KickStarter.playerInteraction.GetActiveHotspot() != null && KickStarter.stateHandler.IsInGameplay() && (KickStarter.playerInteraction.GetActiveHotspot().HasContextUse() || KickStarter.playerInteraction.GetActiveHotspot().HasContextLook()))
				{
					SelectedCursor = 0;
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
					{
						Button firstUseButton = KickStarter.playerInteraction.GetActiveHotspot().GetFirstUseButton();
						if (firstUseButton != null)
						{
							SelectedCursor = firstUseButton.iconID;
						}
						if (KickStarter.cursorManager.allowInteractionCursor)
						{
							canShowHardwareCursor = false;
							ShowContextIcons();
						}
						else if (KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon(KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor();
						}
					}
				}
				else
				{
					SelectedCursor = -1;
				}
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				if (KickStarter.stateHandler.gameState == GameState.DialogOptions || KickStarter.stateHandler.gameState == GameState.Paused)
				{
					SelectedCursor = -1;
				}
				else if (KickStarter.playerInteraction.GetActiveHotspot() != null && !KickStarter.playerInteraction.GetActiveHotspot().IsSingleInteraction() && !KickStarter.cursorManager.allowInteractionCursor && KickStarter.cursorManager.mouseOverIcon.texture != null)
				{
					DrawIcon(KickStarter.cursorManager.mouseOverIcon, false);
					return;
				}
			}
			if (KickStarter.stateHandler.gameState == GameState.Cutscene && KickStarter.cursorManager.waitIcon.texture != null)
			{
				int elementOverCursorID = KickStarter.playerMenus.GetElementOverCursorID();
				if (elementOverCursorID >= 0)
				{
					DrawIcon(KickStarter.cursorManager.GetCursorIconFromID(elementOverCursorID), false);
				}
				else
				{
					DrawIcon(KickStarter.cursorManager.waitIcon, false);
				}
			}
			else if (selectedCursor == -2 && KickStarter.runtimeInventory.SelectedItem != null)
			{
				canShowHardwareCursor = false;
				if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.settingsManager.cycleInventoryCursors && KickStarter.playerInteraction.GetActiveHotspot() == null && KickStarter.runtimeInventory.hoverItem == null && KickStarter.playerInteraction.GetInteractionIndex() >= 0)
				{
					KickStarter.playerInteraction.ResetInteractionIndex();
					KickStarter.runtimeInventory.SetNull();
					return;
				}
				if (KickStarter.settingsManager.inventoryActiveEffect != InventoryActiveEffect.None && KickStarter.runtimeInventory.SelectedItem.CanBeAnimated() && !string.IsNullOrEmpty(KickStarter.playerMenus.GetHotspotLabel()) && (KickStarter.settingsManager.activeWhenUnhandled || KickStarter.playerInteraction.DoesHotspotHaveInventoryInteraction() || (KickStarter.runtimeInventory.hoverItem != null && KickStarter.runtimeInventory.hoverItem.DoesHaveInventoryInteraction(KickStarter.runtimeInventory.SelectedItem))))
				{
					if (KickStarter.cursorManager.inventoryHandling == InventoryHandling.ChangeHotspotLabel)
					{
						DrawMainCursor();
					}
					else
					{
						DrawActiveInventoryCursor();
					}
				}
				else if (KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeHotspotLabel && KickStarter.runtimeInventory.SelectedItem.HasCursorIcon())
				{
					DrawInventoryCursor();
				}
				else
				{
					if (KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeHotspotLabel && !KickStarter.runtimeInventory.SelectedItem.HasCursorIcon())
					{
						ACDebug.LogWarning("Cannot change cursor to display the selected Inventory item because the item '" + KickStarter.runtimeInventory.SelectedItem.label + "' has no associated graphic.");
					}
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
					{
						if (KickStarter.playerInteraction.GetActiveHotspot() == null)
						{
							DrawMainCursor();
						}
						else if (KickStarter.cursorManager.allowInteractionCursor)
						{
							canShowHardwareCursor = false;
							ShowContextIcons();
						}
						else if (KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon(KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor();
						}
					}
					else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.stateHandler.gameState != GameState.DialogOptions && KickStarter.stateHandler.gameState != GameState.Paused)
					{
						if (KickStarter.playerInteraction.GetActiveHotspot() != null && !KickStarter.playerInteraction.GetActiveHotspot().IsSingleInteraction() && !KickStarter.cursorManager.allowInteractionCursor && KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon(KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor();
						}
					}
				}
				if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.canCarryMultiple && !KickStarter.runtimeInventory.SelectedItem.CanSelectSingle())
				{
					KickStarter.runtimeInventory.DrawInventoryCount(KickStarter.playerInput.GetMousePosition(), KickStarter.cursorManager.inventoryCursorSize, KickStarter.runtimeInventory.SelectedItem.count);
				}
			}
			else if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				ShowCycleCursor(KickStarter.playerInteraction.GetActiveUseButtonIconID());
			}
			else
			{
				if (!KickStarter.cursorManager.allowMainCursor && KickStarter.settingsManager.inputMethod != InputMethod.KeyboardOrController)
				{
					return;
				}
				pulseDirection = 0;
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.runtimeInventory.hoverItem == null && KickStarter.playerInteraction.GetActiveHotspot() != null && (!KickStarter.playerMenus.IsInteractionMenuOn() || KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot))
					{
						if (KickStarter.playerInteraction.GetActiveHotspot().IsSingleInteraction())
						{
							ShowContextIcons();
						}
						else if (KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon(KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor();
						}
					}
					else
					{
						DrawMainCursor();
					}
				}
				else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (selectedCursor == -1)
					{
						DrawMainCursor();
					}
					else if (selectedCursor == -2 && KickStarter.runtimeInventory.SelectedItem == null)
					{
						SelectedCursor = -1;
					}
				}
				else
				{
					if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot)
					{
						return;
					}
					if (KickStarter.playerInteraction.GetActiveHotspot() != null && KickStarter.playerInteraction.GetActiveHotspot().IsSingleInteraction())
					{
						if (KickStarter.cursorManager.allowInteractionCursor)
						{
							ShowContextIcons();
						}
						else if (KickStarter.cursorManager.mouseOverIcon.texture != null)
						{
							DrawIcon(KickStarter.cursorManager.mouseOverIcon, false);
						}
						else
						{
							DrawMainCursor();
						}
					}
					else if (selectedCursor >= 0)
					{
						if (KickStarter.cursorManager.allowInteractionCursor)
						{
							pulseDirection = 0;
							canShowHardwareCursor = false;
							bool canAnimate = false;
							if (!KickStarter.cursorManager.onlyAnimateOverHotspots || KickStarter.playerInteraction.GetActiveHotspot() != null || (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple && KickStarter.runtimeInventory.hoverItem != null))
							{
								canAnimate = true;
							}
							DrawIcon(KickStarter.cursorManager.cursorIcons[selectedCursor], false, canAnimate);
						}
						else
						{
							DrawMainCursor();
						}
					}
					else if (selectedCursor == -1)
					{
						DrawMainCursor();
					}
					else if (selectedCursor == -2 && KickStarter.runtimeInventory.SelectedItem == null)
					{
						SelectedCursor = -1;
					}
				}
			}
		}

		protected void DrawMainCursor()
		{
			if (!showCursor || KickStarter.cursorManager.cursorDisplay == CursorDisplay.Never || !KickStarter.cursorManager.allowMainCursor || (KickStarter.stateHandler.gameState != GameState.Paused && KickStarter.cursorManager.cursorDisplay == CursorDisplay.OnlyWhenPaused) || KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}
			bool flag = false;
			int elementOverCursorID = KickStarter.playerMenus.GetElementOverCursorID();
			if (elementOverCursorID >= 0)
			{
				DrawIcon(KickStarter.cursorManager.GetCursorIconFromID(elementOverCursorID), false);
				return;
			}
			if (KickStarter.cursorManager.allowWalkCursor && (bool)KickStarter.playerInput && !KickStarter.playerMenus.IsMouseOverMenu() && !KickStarter.playerMenus.IsInteractionMenuOn() && KickStarter.stateHandler.IsInGameplay())
			{
				if (KickStarter.cursorManager.onlyWalkWhenOverNavMesh)
				{
					if (KickStarter.playerMovement.ClickPoint(KickStarter.playerInput.GetMousePosition(), true) != Vector3.zero)
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				DrawIcon(KickStarter.cursorManager.walkIcon, false);
			}
			else if ((bool)KickStarter.cursorManager.pointerIcon.texture)
			{
				DrawIcon(KickStarter.cursorManager.pointerIcon, false);
			}
		}

		protected void ShowContextIcons()
		{
			Hotspot activeHotspot = KickStarter.playerInteraction.GetActiveHotspot();
			if (activeHotspot == null)
			{
				return;
			}
			if (activeHotspot.HasContextUse())
			{
				if (!activeHotspot.HasContextLook())
				{
					DrawIcon(KickStarter.cursorManager.GetCursorIconFromID(activeHotspot.GetFirstUseButton().iconID), false);
					return;
				}
				Button firstUseButton = activeHotspot.GetFirstUseButton();
				if (activeHotspot.HasContextUse() && activeHotspot.HasContextLook() && CanDisplayIconsSideBySide())
				{
					CursorIcon cursorIconFromID = KickStarter.cursorManager.GetCursorIconFromID(firstUseButton.iconID);
					DrawIcon(new Vector2((0f - cursorIconFromID.size) * (float)ACScreen.width / 2f, 0f), cursorIconFromID, false);
				}
				else if (CanCycleContextSensitiveMode() && contextCycleExamine && activeHotspot.HasContextLook())
				{
					CursorIcon cursorIconFromID2 = KickStarter.cursorManager.GetCursorIconFromID(KickStarter.cursorManager.lookCursor_ID);
					DrawIcon(Vector2.zero, cursorIconFromID2, true);
				}
				else
				{
					DrawIcon(KickStarter.cursorManager.GetCursorIconFromID(firstUseButton.iconID), false);
				}
			}
			if (activeHotspot.HasContextLook() && (!activeHotspot.HasContextUse() || (activeHotspot.HasContextUse() && CanDisplayIconsSideBySide())) && KickStarter.cursorManager.cursorIcons.Count > 0)
			{
				CursorIcon cursorIconFromID3 = KickStarter.cursorManager.GetCursorIconFromID(KickStarter.cursorManager.lookCursor_ID);
				if (activeHotspot.HasContextUse() && activeHotspot.HasContextLook() && CanDisplayIconsSideBySide())
				{
					DrawIcon(new Vector2(cursorIconFromID3.size * (float)ACScreen.width / 2f, 0f), cursorIconFromID3, true);
				}
				else
				{
					DrawIcon(cursorIconFromID3, true);
				}
			}
		}

		protected void ShowContextIcons(InvItem invItem)
		{
			if (KickStarter.cursorManager.cursorIcons.Count <= 0)
			{
				return;
			}
			if (invItem.lookActionList != null && CanDisplayIconsSideBySide())
			{
				if (invItem.useIconID < 0)
				{
					if (invItem.lookActionList != null)
					{
						CursorIcon cursorIconFromID = KickStarter.cursorManager.GetCursorIconFromID(KickStarter.cursorManager.lookCursor_ID);
						DrawIcon(cursorIconFromID, true);
					}
					return;
				}
				CursorIcon cursorIconFromID2 = KickStarter.cursorManager.GetCursorIconFromID(invItem.useIconID);
				DrawIcon(new Vector2((0f - cursorIconFromID2.size) * (float)ACScreen.width / 2f, 0f), cursorIconFromID2, false);
			}
			else if (!CanCycleContextSensitiveMode() || !contextCycleExamine || !(invItem.lookActionList != null))
			{
				DrawIcon(KickStarter.cursorManager.GetCursorIconFromID(invItem.useIconID), false);
				return;
			}
			if (!(invItem.lookActionList != null))
			{
				return;
			}
			CursorIcon cursorIconFromID3 = KickStarter.cursorManager.GetCursorIconFromID(KickStarter.cursorManager.lookCursor_ID);
			if (invItem.lookActionList != null && CanDisplayIconsSideBySide())
			{
				DrawIcon(new Vector2(cursorIconFromID3.size * (float)ACScreen.width / 2f, 0f), cursorIconFromID3, true);
			}
			else if (CanCycleContextSensitiveMode())
			{
				if (contextCycleExamine)
				{
					DrawIcon(Vector2.zero, cursorIconFromID3, true);
				}
			}
			else
			{
				DrawIcon(cursorIconFromID3, true);
			}
		}

		protected void ShowCycleCursor(int useCursorID)
		{
			if (KickStarter.runtimeInventory.SelectedItem != null)
			{
				SelectedCursor = -2;
				if (KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeHotspotLabel)
				{
					DrawActiveInventoryCursor();
				}
			}
			else if (useCursorID >= 0)
			{
				SelectedCursor = useCursorID;
				DrawIcon(KickStarter.cursorManager.GetCursorIconFromID(selectedCursor), false);
			}
			else if (useCursorID == -1)
			{
				SelectedCursor = -1;
				DrawMainCursor();
			}
		}

		protected void DrawInventoryCursor()
		{
			InvItem selectedItem = KickStarter.runtimeInventory.SelectedItem;
			if (selectedItem == null)
			{
				return;
			}
			if (selectedItem.cursorIcon.texture != null)
			{
				if (KickStarter.settingsManager.inventoryActiveEffect != InventoryActiveEffect.None)
				{
					DrawIcon(selectedItem.cursorIcon, false, false);
				}
				else
				{
					DrawIcon(selectedItem.cursorIcon, false);
				}
			}
			else
			{
				DrawIcon(AdvGame.GUIBox(KickStarter.playerInput.GetMousePosition(), KickStarter.cursorManager.inventoryCursorSize), KickStarter.runtimeInventory.SelectedItem.tex);
			}
			pulseDirection = 0;
		}

		protected void DrawActiveInventoryCursor()
		{
			InvItem selectedItem = KickStarter.runtimeInventory.SelectedItem;
			if (selectedItem == null)
			{
				return;
			}
			if (selectedItem.cursorIcon.texture != null)
			{
				DrawIcon(selectedItem.cursorIcon, false);
			}
			else if (selectedItem.activeTex == null)
			{
				DrawInventoryCursor();
			}
			else if (KickStarter.settingsManager.inventoryActiveEffect == InventoryActiveEffect.Simple)
			{
				DrawIcon(AdvGame.GUIBox(KickStarter.playerInput.GetMousePosition(), KickStarter.cursorManager.inventoryCursorSize), selectedItem.activeTex);
			}
			else if (KickStarter.settingsManager.inventoryActiveEffect == InventoryActiveEffect.Pulse && (bool)selectedItem.tex)
			{
				if (pulseDirection == 0)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulse > 1f)
				{
					pulse = 1f;
					pulseDirection = -1;
				}
				else if (pulse < 0f)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulseDirection == 1)
				{
					pulse += KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				else if (pulseDirection == -1)
				{
					pulse -= KickStarter.settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				Color color = GUI.color;
				Color color2 = GUI.color;
				color2.a = pulse;
				GUI.color = color2;
				DrawIcon(AdvGame.GUIBox(KickStarter.playerInput.GetMousePosition(), KickStarter.cursorManager.inventoryCursorSize), selectedItem.activeTex);
				GUI.color = color;
				DrawIcon(AdvGame.GUIBox(KickStarter.playerInput.GetMousePosition(), KickStarter.cursorManager.inventoryCursorSize), selectedItem.tex);
			}
		}

		protected void DrawIcon(Rect _rect, Texture _tex)
		{
			if (_tex != null)
			{
				RecordCursorTexture(_tex);
				if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
				{
					lastCursorName = string.Empty;
					activeIcon = (activeLookIcon = null);
					SetHardwareCursor(currentCursorTexture2D, Vector2.zero);
				}
				else
				{
					GUI.DrawTexture(_rect, currentCursorTexture, ScaleMode.ScaleToFit, true, 0f);
				}
			}
		}

		protected void SetHardwareCursor(Texture2D texture2D, Vector2 clickOffset)
		{
			Cursor.SetCursor(texture2D, clickOffset, CursorMode.Auto);
			KickStarter.eventManager.Call_OnSetHardwareCursor(texture2D, clickOffset);
		}

		protected void DrawIcon(Vector2 offset, CursorIconBase icon, bool isLook, bool canAnimate = true)
		{
			if (icon == null)
			{
				return;
			}
			bool flag = false;
			if (isLook && activeLookIcon != icon)
			{
				activeLookIcon = icon;
				flag = true;
				icon.Reset();
			}
			else if (!isLook && activeIcon != icon)
			{
				activeIcon = icon;
				flag = true;
				icon.Reset();
			}
			if (KickStarter.cursorManager.cursorRendering == CursorRendering.Hardware)
			{
				if (icon.isAnimated)
				{
					Texture2D animatedTexture = icon.GetAnimatedTexture(canAnimate);
					if (icon.GetName() != lastCursorName)
					{
						lastCursorName = icon.GetName();
						RecordCursorTexture(animatedTexture);
						SetHardwareCursor(currentCursorTexture2D, icon.clickOffset);
					}
				}
				else if (flag)
				{
					RecordCursorTexture(icon.texture);
					SetHardwareCursor(currentCursorTexture2D, icon.clickOffset);
				}
			}
			else
			{
				Texture newCursorTexture = icon.Draw(KickStarter.playerInput.GetMousePosition() + offset, canAnimate);
				RecordCursorTexture(newCursorTexture);
			}
		}

		protected void DrawIcon(CursorIconBase icon, bool isLook, bool canAnimate = true)
		{
			if (icon != null)
			{
				DrawIcon(new Vector2(0f, 0f), icon, isLook, canAnimate);
			}
		}

		protected void RecordCursorTexture(Texture newCursorTexture)
		{
			if (newCursorTexture != null && currentCursorTexture != newCursorTexture)
			{
				currentCursorTexture = newCursorTexture;
				if (newCursorTexture is Texture2D)
				{
					Texture2D texture2D = (Texture2D)newCursorTexture;
					currentCursorTexture2D = texture2D;
				}
			}
		}

		public Texture GetCurrentCursorTexture()
		{
			return currentCursorTexture;
		}

		protected void CycleCursors()
		{
			if (KickStarter.playerInteraction.GetActiveHotspot() != null && KickStarter.playerInteraction.GetActiveHotspot().IsSingleInteraction())
			{
				return;
			}
			int num = selectedCursor;
			if (KickStarter.cursorManager.cursorIcons.Count > 0)
			{
				num++;
				if (num >= 0 && num < KickStarter.cursorManager.cursorIcons.Count && KickStarter.cursorManager.cursorIcons[num].dontCycle)
				{
					while (KickStarter.cursorManager.cursorIcons[num].dontCycle)
					{
						num++;
						if (num >= KickStarter.cursorManager.cursorIcons.Count)
						{
							num = -1;
							break;
						}
					}
				}
				else if (num >= KickStarter.cursorManager.cursorIcons.Count)
				{
					num = -1;
				}
			}
			else
			{
				num = -1;
			}
			if (num == -1 && selectedCursor >= 0 && KickStarter.settingsManager.cycleInventoryCursors)
			{
				KickStarter.runtimeInventory.ReselectLastItem();
				KickStarter.playerInput.ResetMouseClick();
			}
			SelectedCursor = num;
		}

		public int GetSelectedCursor()
		{
			return selectedCursor;
		}

		public int GetSelectedCursorID()
		{
			if ((bool)KickStarter.cursorManager && KickStarter.cursorManager.cursorIcons.Count > 0 && selectedCursor > -1)
			{
				return KickStarter.cursorManager.cursorIcons[selectedCursor].id;
			}
			return -1;
		}

		public void ResetSelectedCursor()
		{
			SelectedCursor = -1;
		}

		public void SetCursorFromID(int ID)
		{
			if (!KickStarter.cursorManager || KickStarter.cursorManager.cursorIcons.Count <= 0)
			{
				return;
			}
			foreach (CursorIcon cursorIcon in KickStarter.cursorManager.cursorIcons)
			{
				if (cursorIcon.id == ID)
				{
					SetCursor(cursorIcon);
				}
			}
		}

		public void SetCursor(CursorIcon _icon)
		{
			KickStarter.runtimeInventory.SetNull();
			SelectedCursor = KickStarter.cursorManager.cursorIcons.IndexOf(_icon);
		}

		protected bool CanDisplayIconsSideBySide()
		{
			if (KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide && KickStarter.cursorManager.cursorRendering == CursorRendering.Software && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				return true;
			}
			return false;
		}

		protected bool CanCycleContextSensitiveMode()
		{
			if (KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.RightClickCyclesModes && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				return true;
			}
			return false;
		}

		protected void OnApplicationQuit()
		{
			if (!(KickStarter.cursorManager != null))
			{
				return;
			}
			KickStarter.cursorManager.waitIcon.ClearCache();
			KickStarter.cursorManager.pointerIcon.ClearCache();
			KickStarter.cursorManager.walkIcon.ClearCache();
			KickStarter.cursorManager.mouseOverIcon.ClearCache();
			foreach (CursorIcon cursorIcon in KickStarter.cursorManager.cursorIcons)
			{
				cursorIcon.ClearCache();
			}
		}

		public void SetCursorVisibility(bool state)
		{
			Cursor.visible = state;
		}
	}
}
