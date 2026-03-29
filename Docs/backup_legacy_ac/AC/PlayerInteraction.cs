using System.Collections;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_interaction.html")]
	public class PlayerInteraction : MonoBehaviour
	{
		protected bool inPreInteractionCutscene;

		protected string interactionLabel;

		protected Hotspot hotspotMovingTo;

		protected Hotspot hotspot;

		protected Hotspot lastHotspot;

		protected Button button;

		protected int interactionIndex = -1;

		protected Hotspot manualHotspot;

		protected string movingToHotspotLabel = string.Empty;

		protected bool preventInteractionsThisFrame;

		protected int lastClickedCursorID;

		protected LayerMask hotspotLayerMask;

		public string MovingToHotspotLabel
		{
			get
			{
				return movingToHotspotLabel;
			}
		}

		public bool InPreInteractionCutscene
		{
			get
			{
				return inPreInteractionCutscene;
			}
		}

		public string InteractionLabel
		{
			get
			{
				return interactionLabel;
			}
		}

		protected LayerMask HotspotLayerMask
		{
			get
			{
				return hotspotLayerMask;
			}
			set
			{
				hotspotLayerMask = value;
			}
		}

		public void UpdateInteraction()
		{
			HotspotLayerMask = 1 << LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			if (KickStarter.stateHandler.IsInGameplay())
			{
				if (KickStarter.playerInput.GetDragState() == DragState.Moveable)
				{
					DeselectHotspot(true);
					preventInteractionsThisFrame = false;
					return;
				}
				if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.CustomScript && KickStarter.playerInput.GetMouseState() == MouseState.RightClick && KickStarter.runtimeInventory.SelectedItem != null && !KickStarter.playerMenus.IsMouseOverMenu() && (KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.CyclingCursorAndClickingHotspot || !KickStarter.settingsManager.cycleInventoryCursors))
				{
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.cycleInventoryCursors)
					{
						KickStarter.playerInput.ResetMouseClick();
						KickStarter.runtimeInventory.SetNull();
					}
					else if (KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple || KickStarter.settingsManager.RightClickInventory == RightClickInventory.DeselectsItem)
					{
						KickStarter.playerInput.ResetMouseClick();
						KickStarter.runtimeInventory.SetNull();
					}
					else if (KickStarter.settingsManager.RightClickInventory == RightClickInventory.ExaminesItem && KickStarter.cursorManager.lookUseCursorAction != LookUseCursorAction.RightClickCyclesModes)
					{
						KickStarter.playerInput.ResetMouseClick();
						KickStarter.runtimeInventory.Look(KickStarter.runtimeInventory.SelectedItem);
					}
				}
				if (KickStarter.playerInput.IsCursorLocked() && KickStarter.settingsManager.onlyInteractWhenCursorUnlocked && KickStarter.settingsManager.IsInFirstPerson())
				{
					DeselectHotspot(true);
					preventInteractionsThisFrame = false;
					return;
				}
				if (UnityUIBlocksClick())
				{
					DeselectHotspot(true);
					preventInteractionsThisFrame = false;
					return;
				}
				if (!KickStarter.playerInput.IsCursorReadable())
				{
					if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.player != null && KickStarter.player.hotspotDetector != null)
					{
						KickStarter.player.hotspotDetector.HighlightAll();
					}
					preventInteractionsThisFrame = false;
					return;
				}
				HandleInteractionMenu();
				if (KickStarter.settingsManager.playerFacesHotspots && KickStarter.player != null)
				{
					if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || !KickStarter.settingsManager.onlyFaceHotspotOnSelect)
					{
						if ((bool)hotspot && hotspot.playerTurnsHead)
						{
							KickStarter.player.SetHeadTurnTarget(hotspot.transform, hotspot.GetIconPosition(true), false, HeadFacing.Hotspot);
						}
						else if (button == null)
						{
							KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
						}
					}
					else if (button == null && hotspot == null && !KickStarter.playerMenus.IsInteractionMenuOn())
					{
						KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
					}
				}
			}
			else if (KickStarter.stateHandler.gameState == GameState.Paused && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot && KickStarter.playerMenus.IsPausingInteractionMenuOn())
			{
				HandleInteractionMenu();
			}
			preventInteractionsThisFrame = false;
		}

		public void UpdateInteractionLabel()
		{
			interactionLabel = GetInteractionLabel(Options.GetLanguage());
		}

		protected void HandleInteractionMenu()
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.CustomScript)
			{
				return;
			}
			if (KickStarter.playerInput.GetMouseState() == MouseState.LetGo && !KickStarter.playerMenus.IsMouseOverInteractionMenu() && KickStarter.settingsManager.ReleaseClickInteractions())
			{
				KickStarter.playerMenus.CloseInteractionMenus();
			}
			if (KickStarter.playerInput.GetMouseState() == MouseState.LetGo && !KickStarter.playerMenus.IsMouseOverInteractionMenu() && KickStarter.settingsManager.ReleaseClickInteractions())
			{
				KickStarter.playerMenus.CloseInteractionMenus();
			}
			if (!KickStarter.playerMenus.IsMouseOverMenu() && (bool)KickStarter.CameraMain && !KickStarter.playerInput.ActiveArrowsDisablingHotspots() && KickStarter.mainCamera.IsPointInCamera(KickStarter.playerInput.GetMousePosition()))
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						ContextSensitiveClick();
					}
					else if (!KickStarter.playerMenus.IsMouseOverInteractionMenu())
					{
						ChooseHotspotThenInteractionClick();
					}
				}
				else
				{
					ContextSensitiveClick();
				}
			}
			else if (!KickStarter.playerMenus.IsMouseOverInteractionMenu() || KickStarter.runtimeInventory.hoverItem != null)
			{
				DeselectHotspot();
			}
		}

		public void UpdateInventory()
		{
			if (hotspot == null && button == null && IsDroppingInventory() && (!(KickStarter.playerMenus.EventSystem != null) || !KickStarter.playerMenus.EventSystem.IsPointerOverGameObject()))
			{
				KickStarter.runtimeInventory.SetNull();
			}
		}

		public void SetActiveHotspot(Hotspot _hotspot)
		{
			hotspot = (manualHotspot = _hotspot);
			if (KickStarter.settingsManager.hotspotDetection != HotspotDetection.CustomScript)
			{
				ACDebug.LogWarning("The 'Hotspot detection method' setting must be set to 'Custom Script' in order for Hotspots to be set active manually.");
			}
		}

		protected Hotspot CheckForHotspots()
		{
			if (!KickStarter.playerInput.IsMouseOnScreen())
			{
				return null;
			}
			if (KickStarter.settingsManager.InventoryDragDrop && KickStarter.playerInput.GetMousePosition() == Vector2.zero)
			{
				return null;
			}
			if (KickStarter.playerInput.GetDragState() == DragState._Camera)
			{
				return null;
			}
			if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.CustomScript)
			{
				return manualHotspot;
			}
			if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity)
			{
				if (KickStarter.player != null && KickStarter.player.hotspotDetector != null)
				{
					if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct || KickStarter.settingsManager.IsInFirstPerson())
					{
						if (KickStarter.settingsManager.hotspotsInVicinity != HotspotsInVicinity.ShowAll)
						{
							return CheckHotspotValid(KickStarter.player.hotspotDetector.GetSelected());
						}
						KickStarter.player.hotspotDetector.HighlightAll();
					}
					else
					{
						KickStarter.player.hotspotDetector.HighlightAll();
					}
				}
				else
				{
					ACDebug.LogWarning("Both a Player and a Hotspot Detector on that Player are required for Hotspots to be detected by 'Player Vicinity'");
				}
			}
			if (SceneSettings.IsUnity2D())
			{
				RaycastHit2D raycastHit2D;
				if (KickStarter.mainCamera.IsOrthographic())
				{
					raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(KickStarter.playerInput.GetMousePosition()), Vector3.zero, KickStarter.settingsManager.hotspotRaycastLength, HotspotLayerMask);
				}
				else
				{
					Vector3 position = KickStarter.playerInput.GetMousePosition();
					position.z = 0f - KickStarter.CameraMain.transform.position.z;
					raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(position), Vector2.zero, KickStarter.settingsManager.hotspotRaycastLength, HotspotLayerMask);
				}
				if (raycastHit2D.collider != null)
				{
					Hotspot component = raycastHit2D.collider.gameObject.GetComponent<Hotspot>();
					if (component != null)
					{
						if (KickStarter.settingsManager.hotspotDetection != HotspotDetection.PlayerVicinity)
						{
							return CheckHotspotValid(component);
						}
						if ((bool)KickStarter.player.hotspotDetector && KickStarter.player.hotspotDetector.IsHotspotInTrigger(component))
						{
							return CheckHotspotValid(component);
						}
					}
				}
			}
			else
			{
				Camera cameraMain = KickStarter.CameraMain;
				if ((bool)cameraMain)
				{
					Ray ray = cameraMain.ScreenPointToRay(KickStarter.playerInput.GetMousePosition());
					RaycastHit hitInfo;
					if (Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.hotspotRaycastLength, HotspotLayerMask))
					{
						Hotspot component2 = hitInfo.collider.gameObject.GetComponent<Hotspot>();
						if (component2 != null)
						{
							if (KickStarter.settingsManager.hotspotDetection != HotspotDetection.PlayerVicinity)
							{
								return CheckHotspotValid(component2);
							}
							if (KickStarter.player != null && KickStarter.player.hotspotDetector != null && KickStarter.player.hotspotDetector.IsHotspotInTrigger(component2))
							{
								return CheckHotspotValid(component2);
							}
						}
					}
				}
			}
			return null;
		}

		protected Hotspot CheckHotspotValid(Hotspot hotspot)
		{
			if (hotspot == null)
			{
				return null;
			}
			if (!hotspot.PlayerIsWithinBoundary())
			{
				return null;
			}
			if (KickStarter.settingsManager.AutoDisableUnhandledHotspots && KickStarter.runtimeInventory.SelectedItem != null && !hotspot.HasInventoryInteraction(KickStarter.runtimeInventory.SelectedItem))
			{
				return null;
			}
			return hotspot;
		}

		protected bool CanDoDoubleTap()
		{
			if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.InventoryDragDrop)
			{
				return false;
			}
			if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.settingsManager.doubleTapHotspots)
			{
				return true;
			}
			return false;
		}

		protected void ChooseHotspotThenInteractionClick()
		{
			if (CanDoDoubleTap())
			{
				if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick)
				{
					ChooseHotspotThenInteractionClick_Process(true);
				}
			}
			else
			{
				ChooseHotspotThenInteractionClick_Process(false);
			}
		}

		protected void ChooseHotspotThenInteractionClick_Process(bool doubleTap)
		{
			Hotspot hotspot = CheckForHotspots();
			if (this.hotspot != null && hotspot == null)
			{
				DeselectHotspot();
			}
			else
			{
				if (!(hotspot != null))
				{
					return;
				}
				if (hotspot.IsSingleInteraction())
				{
					ContextSensitiveClick();
					return;
				}
				if (KickStarter.playerInput.GetMouseState() == MouseState.HeldDown && KickStarter.playerInput.GetDragState() == DragState.Player)
				{
					DeselectHotspot();
					return;
				}
				bool flag = false;
				if (hotspot != this.hotspot)
				{
					flag = true;
					if ((bool)this.hotspot)
					{
						this.hotspot.Deselect();
						KickStarter.playerMenus.DisableHotspotMenus();
					}
					if (KickStarter.settingsManager.cancelInteractions != CancelInteractions.ViaScriptOnly && (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || !KickStarter.settingsManager.CanClickOffInteractionMenu()))
					{
						if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && this.hotspot == null)
						{
							KickStarter.playerMenus.CloseInteractionMenus();
						}
						if (this.hotspot != null)
						{
							KickStarter.playerMenus.CloseInteractionMenus();
						}
					}
					lastHotspot = (this.hotspot = hotspot);
					this.hotspot.Select();
				}
				if (!this.hotspot)
				{
					return;
				}
				if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || (KickStarter.settingsManager.InventoryDragDrop && IsDroppingInventory()) || (KickStarter.settingsManager.MouseOverForInteractionMenu() && KickStarter.runtimeInventory.hoverItem == null && KickStarter.runtimeInventory.SelectedItem == null && flag && !IsDroppingInventory()))
				{
					if (KickStarter.runtimeInventory.hoverItem == null && KickStarter.playerInput.GetMouseState() == MouseState.SingleClick && KickStarter.settingsManager.MouseOverForInteractionMenu() && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.ClickingMenu && KickStarter.settingsManager.cancelInteractions != CancelInteractions.ClickOffMenu && (KickStarter.runtimeInventory.SelectedItem == null || KickStarter.settingsManager.cycleInventoryCursors))
					{
						return;
					}
					if (KickStarter.runtimeInventory.SelectedItem != null)
					{
						if (KickStarter.settingsManager.InventoryDragDrop || !flag || !doubleTap)
						{
							HandleInteraction();
						}
					}
					else
					{
						if (!KickStarter.playerMenus)
						{
							return;
						}
						if (KickStarter.settingsManager.playerFacesHotspots && KickStarter.player != null && KickStarter.settingsManager.onlyFaceHotspotOnSelect && (bool)this.hotspot && this.hotspot.playerTurnsHead)
						{
							KickStarter.player.SetHeadTurnTarget(this.hotspot.transform, this.hotspot.GetIconPosition(true), false, HeadFacing.Hotspot);
						}
						if (KickStarter.playerMenus.IsInteractionMenuOn() && KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot && KickStarter.playerInput.GetMouseState() == MouseState.SingleClick)
						{
							ClickHotspotToInteract();
						}
						else
						{
							if ((flag && doubleTap) || KickStarter.settingsManager.SeeInteractions == SeeInteractions.ViaScriptOnly)
							{
								return;
							}
							KickStarter.playerMenus.EnableInteractionMenus(this.hotspot);
							if (KickStarter.settingsManager.SeeInteractions == SeeInteractions.ClickOnHotspot)
							{
								if (KickStarter.settingsManager.stopPlayerOnClickHotspot && (bool)KickStarter.player)
								{
									StopMovingToHotspot();
								}
								StopInteraction();
								KickStarter.runtimeInventory.SetNull();
							}
						}
					}
				}
				else if (KickStarter.playerInput.GetMouseState() == MouseState.RightClick)
				{
					this.hotspot.Deselect();
				}
			}
		}

		protected bool IsInvokingDefaultInteraction()
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.allowDefaultinteractions && KickStarter.playerInput.InputGetButtonDown("DefaultInteraction") && KickStarter.runtimeInventory.SelectedItem == null)
			{
				return true;
			}
			return false;
		}

		protected void ContextSensitiveClick()
		{
			if (hotspot != null && IsInvokingDefaultInteraction() && hotspot.provideUseInteraction)
			{
				UseHotspot(hotspot);
				return;
			}
			if (CanDoDoubleTap())
			{
				if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || KickStarter.playerInput.GetMouseState() == MouseState.DoubleClick)
				{
					ContextSensitiveClick_Process(true, CheckForHotspots());
				}
				else if (KickStarter.playerInput.GetMouseState() == MouseState.RightClick)
				{
					HandleInteraction();
				}
				return;
			}
			ContextSensitiveClick_Process(false, CheckForHotspots());
			if (KickStarter.playerMenus.IsMouseOverMenu() || !hotspot || (KickStarter.playerInput.GetMouseState() != MouseState.SingleClick && KickStarter.playerInput.GetMouseState() != MouseState.DoubleClick && KickStarter.playerInput.GetMouseState() != MouseState.RightClick && !IsDroppingInventory()))
			{
				return;
			}
			if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot && (KickStarter.runtimeInventory.SelectedItem == null || (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.cycleInventoryCursors)))
			{
				if (KickStarter.playerInput.GetMouseState() != MouseState.RightClick)
				{
					ClickHotspotToInteract();
				}
			}
			else
			{
				HandleInteraction();
			}
		}

		protected void ContextSensitiveClick_Process(bool doubleTap, Hotspot newHotspot)
		{
			if (hotspot != null && newHotspot == null)
			{
				DeselectHotspot();
			}
			else
			{
				if (!(newHotspot != null))
				{
					return;
				}
				if (KickStarter.playerInput.GetMouseState() == MouseState.HeldDown && KickStarter.playerInput.GetDragState() == DragState.Player)
				{
					DeselectHotspot();
				}
				else if (newHotspot != hotspot)
				{
					DeselectHotspot();
					lastHotspot = (hotspot = newHotspot);
					hotspot.Select();
					if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
					{
						KickStarter.runtimeInventory.MatchInteractions();
						RestoreHotspotInteraction();
					}
				}
				else if (hotspot != null && doubleTap)
				{
					HandleInteraction();
				}
			}
		}

		public void DeselectHotspot(bool isInstant = false)
		{
			if ((bool)hotspot)
			{
				if (isInstant)
				{
					hotspot.DeselectInstant();
				}
				else
				{
					hotspot.Deselect();
				}
				hotspot = null;
			}
		}

		public bool DoesHotspotHaveInventoryInteraction()
		{
			if ((bool)hotspot && (bool)KickStarter.runtimeInventory && KickStarter.runtimeInventory.SelectedItem != null)
			{
				for (int i = 0; i < hotspot.invButtons.Count; i++)
				{
					if (hotspot.invButtons[i].invID == KickStarter.runtimeInventory.SelectedItem.id && !hotspot.invButtons[i].isDisabled)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected void HandleInteraction()
		{
			if (!hotspot)
			{
				return;
			}
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || KickStarter.playerInput.GetMouseState() == MouseState.DoubleClick)
				{
					if (KickStarter.runtimeInventory.SelectedItem == null && KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.RightClickCyclesModes)
					{
						if (KickStarter.playerCursor.ContextCycleExamine && hotspot.HasContextLook())
						{
							ClickButton(InteractionType.Examine, -1, -1);
						}
						else if (hotspot.HasContextUse())
						{
							ClickButton(InteractionType.Use, -1, -1);
						}
					}
					else if (KickStarter.runtimeInventory.SelectedItem == null && hotspot.HasContextUse())
					{
						ClickButton(InteractionType.Use, -1, -1);
					}
					else if (KickStarter.runtimeInventory.SelectedItem != null)
					{
						ClickButton(InteractionType.Inventory, -1, KickStarter.runtimeInventory.SelectedItem.id);
						if (KickStarter.settingsManager.inventoryDisableLeft)
						{
							KickStarter.runtimeInventory.SetNull();
						}
					}
					else if (hotspot.HasContextLook() && KickStarter.cursorManager.leftClickExamine)
					{
						ClickButton(InteractionType.Examine, -1, -1);
					}
					else if ((bool)hotspot.walkToMarker)
					{
						ClickHotspotToWalk(hotspot.walkToMarker.transform);
					}
				}
				else if (KickStarter.playerInput.GetMouseState() == MouseState.RightClick)
				{
					if (KickStarter.runtimeInventory.SelectedItem == null && hotspot.HasContextLook() && KickStarter.cursorManager.lookUseCursorAction != LookUseCursorAction.RightClickCyclesModes)
					{
						ClickButton(InteractionType.Examine, -1, -1);
					}
				}
				else if (KickStarter.settingsManager.InventoryDragDrop && IsDroppingInventory())
				{
					ClickButton(InteractionType.Inventory, -1, KickStarter.runtimeInventory.SelectedItem.id);
					KickStarter.runtimeInventory.SetNull();
				}
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && (bool)KickStarter.playerCursor && (bool)KickStarter.cursorManager)
			{
				if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick)
				{
					if (KickStarter.runtimeInventory.SelectedItem == null && hotspot.provideUseInteraction)
					{
						if (GetActiveHotspot() != null && GetActiveHotspot().IsSingleInteraction())
						{
							ClickButton(InteractionType.Use, -1, -1);
						}
						else if (KickStarter.playerCursor.GetSelectedCursor() >= 0)
						{
							ClickButton(InteractionType.Use, KickStarter.cursorManager.cursorIcons[KickStarter.playerCursor.GetSelectedCursor()].id, -1, GetActiveHotspot());
						}
						else if (KickStarter.cursorManager.allowWalkCursor && hotspot != null && (bool)hotspot.walkToMarker)
						{
							ClickHotspotToWalk(hotspot.walkToMarker.transform);
						}
					}
					else if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.playerCursor.GetSelectedCursor() == -2)
					{
						KickStarter.playerCursor.ResetSelectedCursor();
						ClickButton(InteractionType.Inventory, -1, KickStarter.runtimeInventory.SelectedItem.id);
						if (KickStarter.settingsManager.inventoryDisableLeft)
						{
							KickStarter.runtimeInventory.SetNull();
						}
					}
				}
				else if (KickStarter.settingsManager.InventoryDragDrop && IsDroppingInventory())
				{
					ClickButton(InteractionType.Inventory, -1, KickStarter.runtimeInventory.SelectedItem.id);
				}
			}
			else
			{
				if (KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					return;
				}
				if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.CanSelectItems(false))
				{
					if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick || KickStarter.playerInput.GetMouseState() == MouseState.DoubleClick)
					{
						ClickButton(InteractionType.Inventory, -1, KickStarter.runtimeInventory.SelectedItem.id);
						if (KickStarter.settingsManager.inventoryDisableLeft)
						{
							KickStarter.runtimeInventory.SetNull();
						}
					}
					else if (KickStarter.settingsManager.InventoryDragDrop && IsDroppingInventory())
					{
						ClickButton(InteractionType.Inventory, -1, KickStarter.runtimeInventory.SelectedItem.id);
						KickStarter.runtimeInventory.SetNull();
					}
				}
				else if (KickStarter.runtimeInventory.SelectedItem == null && hotspot.IsSingleInteraction())
				{
					ClickButton(InteractionType.Use, -1, -1);
					if (KickStarter.settingsManager.inventoryDisableLeft)
					{
						KickStarter.runtimeInventory.SetNull();
					}
				}
			}
		}

		protected void ClickHotspotToWalk(Transform walkToMarker)
		{
			StopInteraction();
			KickStarter.playerInput.ResetMouseClick();
			KickStarter.playerInput.ResetClick();
			if ((bool)KickStarter.player)
			{
				KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
				Vector3[] pointsArray = KickStarter.navigationManager.navigationEngine.GetPointsArray(KickStarter.player.transform.position, walkToMarker.position, KickStarter.player);
				KickStarter.player.MoveAlongPoints(pointsArray, false);
			}
		}

		public void UseHotspot(Hotspot _hotspot, int selectedCursorID = -1)
		{
			ClickButton(InteractionType.Use, selectedCursorID, -1, _hotspot);
		}

		public void ExamineHotspot(Hotspot _hotspot)
		{
			ClickButton(InteractionType.Examine, -1, -1, _hotspot);
		}

		public void UseInventoryOnHotspot(Hotspot _hotspot, int inventoryItemID, bool requireCarry = true)
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.CustomScript && requireCarry && !KickStarter.runtimeInventory.IsCarryingItem(inventoryItemID))
			{
				ACDebug.Log("Cannot use item with ID " + inventoryItemID + " as the player is not carrying it.");
			}
			else
			{
				ClickButton(InteractionType.Inventory, -1, inventoryItemID, _hotspot);
			}
		}

		protected void ClickButton(InteractionType _interactionType, int selectedCursorID, int selectedItemID, Hotspot clickedHotspot = null)
		{
			if (preventInteractionsThisFrame)
			{
				return;
			}
			inPreInteractionCutscene = false;
			StopCoroutine("UseObject");
			lastClickedCursorID = selectedCursorID;
			if (clickedHotspot != null)
			{
				lastHotspot = (hotspot = clickedHotspot);
			}
			if (hotspot == null)
			{
				ACDebug.LogWarning("Cannot process Hotspot interaction, because no Hotspot was set!");
				return;
			}
			if ((bool)KickStarter.player)
			{
				KickStarter.player.EndPath();
			}
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.settingsManager.autoCycleWhenInteract)
				{
					SetNextInteraction();
				}
				else
				{
					ResetInteractionIndex();
				}
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.autoCycleWhenInteract)
			{
				KickStarter.playerCursor.ResetSelectedCursor();
			}
			KickStarter.playerInput.ResetMouseClick();
			KickStarter.playerInput.ResetClick();
			button = null;
			switch (_interactionType)
			{
			case InteractionType.Use:
			{
				if (selectedCursorID == -1)
				{
					button = hotspot.GetFirstUseButton();
					break;
				}
				foreach (Button useButton in hotspot.useButtons)
				{
					if (useButton.iconID == selectedCursorID && !useButton.isDisabled)
					{
						button = useButton;
						break;
					}
				}
				if (button == null && KickStarter.cursorManager.AllowUnhandledIcons() && hotspot.provideUnhandledUseInteraction && !hotspot.unhandledUseButton.isDisabled)
				{
					button = hotspot.unhandledUseButton;
				}
				if (button != null || !KickStarter.cursorManager.AllowUnhandledIcons())
				{
					break;
				}
				ActionListAsset unhandledInteraction = KickStarter.cursorManager.GetUnhandledInteraction(selectedCursorID);
				RunUnhandledHotspotInteraction(unhandledInteraction, clickedHotspot, KickStarter.cursorManager.passUnhandledHotspotAsParameter);
				KickStarter.runtimeInventory.SetNull();
				if (KickStarter.player != null)
				{
					KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
				}
				return;
			}
			case InteractionType.Examine:
				button = hotspot.lookButton;
				break;
			case InteractionType.Inventory:
				if (selectedItemID < 0)
				{
					break;
				}
				foreach (Button invButton in hotspot.invButtons)
				{
					if (invButton.invID == selectedItemID && !invButton.isDisabled && ((KickStarter.runtimeInventory.IsGivingItem() && invButton.selectItemMode == SelectItemMode.Give) || (!KickStarter.runtimeInventory.IsGivingItem() && invButton.selectItemMode == SelectItemMode.Use) || !KickStarter.settingsManager.CanGiveItems()))
					{
						button = invButton;
						break;
					}
				}
				if (button == null && hotspot.provideUnhandledInvInteraction && hotspot.unhandledInvButton != null)
				{
					button = hotspot.unhandledInvButton;
				}
				break;
			}
			if (button != null && button.isDisabled)
			{
				button = null;
				if (_interactionType != InteractionType.Inventory)
				{
					if (KickStarter.player != null)
					{
						KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
					}
					return;
				}
			}
			KickStarter.eventManager.Call_OnInteractHotspot(hotspot, button);
			StartCoroutine("UseObject", selectedItemID);
		}

		protected IEnumerator UseObject(int selectedItemID)
		{
			bool doRun = false;
			bool doSnap = false;
			if (hotspotMovingTo == hotspot && KickStarter.playerInput.LastClickWasDouble())
			{
				KickStarter.eventManager.Call_OnDoubleClickHotspot(hotspot);
				if (hotspotMovingTo.doubleClickingHotspot == DoubleClickingHotspot.TriggersInteractionInstantly)
				{
					doSnap = true;
				}
				else if (hotspotMovingTo.doubleClickingHotspot == DoubleClickingHotspot.MakesPlayerRun)
				{
					doRun = true;
				}
			}
			if (KickStarter.playerInput != null)
			{
				if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysWalk)
				{
					doRun = false;
				}
				else if (KickStarter.playerInput.runLock == PlayerMoveLock.AlwaysRun)
				{
					doRun = true;
				}
			}
			if ((bool)KickStarter.player)
			{
				if (button != null && (button.playerAction == PlayerAction.WalkToMarker || button.playerAction == PlayerAction.WalkTo))
				{
					if (KickStarter.playerInput.AllDirectionsLocked())
					{
						KickStarter.stateHandler.gameState = GameState.Normal;
					}
					else
					{
						if (button.isBlocking)
						{
							inPreInteractionCutscene = true;
							KickStarter.stateHandler.gameState = GameState.Cutscene;
						}
						else
						{
							KickStarter.stateHandler.gameState = GameState.Normal;
						}
						hotspotMovingTo = hotspot;
						movingToHotspotLabel = button.GetFullLabel(hotspot, Options.GetLanguage());
					}
				}
				else
				{
					if (button != null && button.playerAction != PlayerAction.DoNothing)
					{
						inPreInteractionCutscene = true;
						KickStarter.stateHandler.gameState = GameState.Cutscene;
					}
					else
					{
						KickStarter.stateHandler.gameState = GameState.Normal;
					}
					hotspotMovingTo = null;
				}
			}
			Hotspot _hotspot = hotspot;
			if (KickStarter.player == null || inPreInteractionCutscene || (button != null && button.playerAction == PlayerAction.DoNothing))
			{
				DeselectHotspot();
			}
			if ((bool)KickStarter.player)
			{
				if (button != null && button.playerAction != PlayerAction.DoNothing)
				{
					Vector3 lookVector = Vector3.zero;
					Vector3 targetPos = _hotspot.transform.position;
					if (SceneSettings.ActInScreenSpace())
					{
						Vector3 targetWorldPosition = ((!(_hotspot.centrePoint != null)) ? _hotspot.transform.position : _hotspot.centrePoint.position);
						lookVector = AdvGame.GetScreenDirection(KickStarter.player.transform.position, targetWorldPosition);
					}
					else
					{
						Vector3 vector = ((!(_hotspot.centrePoint != null)) ? _hotspot.transform.position : _hotspot.centrePoint.position);
						lookVector = vector - KickStarter.player.transform.position;
						lookVector.y = 0f;
					}
					KickStarter.player.SetLookDirection(lookVector, false);
					if (button.playerAction == PlayerAction.TurnToFace)
					{
						while (KickStarter.player.IsTurning())
						{
							yield return new WaitForFixedUpdate();
						}
					}
					if (button.playerAction == PlayerAction.WalkToMarker && _hotspot.walkToMarker != null)
					{
						if (!KickStarter.playerInput.AllDirectionsLocked() && Vector3.Distance(KickStarter.player.transform.position, _hotspot.walkToMarker.transform.position) > KickStarter.settingsManager.GetDestinationThreshold())
						{
							if ((bool)KickStarter.navigationManager)
							{
								Vector3 vector2 = _hotspot.walkToMarker.transform.position;
								if (SceneSettings.ActInScreenSpace())
								{
									vector2 = AdvGame.GetScreenNavMesh(vector2);
								}
								Vector3[] pointsArray = KickStarter.navigationManager.navigationEngine.GetPointsArray(KickStarter.player.transform.position, vector2, KickStarter.player);
								if (pointsArray.Length > 0)
								{
									KickStarter.player.MoveAlongPoints(pointsArray, doRun);
									targetPos = pointsArray[pointsArray.Length - 1];
								}
								else
								{
									ACDebug.LogWarning("Cannot calculate path to Hotspot " + _hotspot.name + "'s marker.  Moving without pathfinding!", _hotspot.walkToMarker);
									KickStarter.player.MoveToPoint(vector2, doRun);
									targetPos = vector2;
								}
								if (KickStarter.player.retroPathfinding)
								{
									KickStarter.player._Update();
								}
							}
							while ((bool)KickStarter.player.GetPath())
							{
								if (doSnap)
								{
									KickStarter.player.Teleport(targetPos);
									break;
								}
								yield return new WaitForFixedUpdate();
							}
						}
						if (button.faceAfter)
						{
							lookVector = _hotspot.walkToMarker.transform.forward;
							lookVector.y = 0f;
							KickStarter.player.EndPath();
							KickStarter.player.SetLookDirection(lookVector, false);
							while (KickStarter.player.IsTurning())
							{
								if (doSnap)
								{
									KickStarter.player.SetLookDirection(lookVector, true);
									break;
								}
								yield return new WaitForEndOfFrame();
							}
						}
					}
					else if (button.playerAction == PlayerAction.WalkTo)
					{
						float dist = Vector3.Distance(KickStarter.player.transform.position, targetPos);
						if ((bool)_hotspot.walkToMarker)
						{
							dist = Vector3.Distance(KickStarter.player.transform.position, _hotspot.walkToMarker.transform.position);
						}
						if (!KickStarter.playerInput.AllDirectionsLocked() && ((button.setProximity && dist > button.proximity) || (!button.setProximity && dist > 2f)))
						{
							if ((bool)KickStarter.navigationManager)
							{
								Vector3 vector3 = _hotspot.transform.position;
								if ((bool)_hotspot.walkToMarker)
								{
									vector3 = _hotspot.walkToMarker.transform.position;
								}
								if (SceneSettings.ActInScreenSpace())
								{
									vector3 = AdvGame.GetScreenNavMesh(vector3);
								}
								Vector3[] pointsArray2 = KickStarter.navigationManager.navigationEngine.GetPointsArray(KickStarter.player.transform.position, vector3, KickStarter.player);
								KickStarter.player.MoveAlongPoints(pointsArray2, doRun);
								targetPos = ((pointsArray2.Length <= 0) ? KickStarter.player.transform.position : pointsArray2[pointsArray2.Length - 1]);
								if (KickStarter.player.retroPathfinding)
								{
									KickStarter.player._Update();
								}
							}
							if (button.setProximity)
							{
								button.proximity = Mathf.Max(button.proximity, 1f);
								targetPos.y = KickStarter.player.transform.position.y;
								while (Vector3.Distance(KickStarter.player.transform.position, targetPos) > button.proximity && (bool)KickStarter.player.GetPath() && !doSnap)
								{
									yield return new WaitForFixedUpdate();
								}
							}
							else if (!doSnap)
							{
								yield return new WaitForSeconds(0.6f);
							}
						}
						if (button.faceAfter)
						{
							Vector3 centrePoint = _hotspot.GetIconPosition();
							if (SceneSettings.ActInScreenSpace())
							{
								lookVector = AdvGame.GetScreenDirection(KickStarter.player.transform.position, centrePoint);
							}
							else
							{
								lookVector = centrePoint - KickStarter.player.transform.position;
								lookVector.y = 0f;
							}
							KickStarter.player.EndPath();
							KickStarter.player.SetLookDirection(lookVector, false);
							while (KickStarter.player.IsTurning())
							{
								if (doSnap)
								{
									KickStarter.player.SetLookDirection(lookVector, true);
									break;
								}
								yield return new WaitForEndOfFrame();
							}
						}
					}
				}
				else if (KickStarter.settingsManager.movementMethod == MovementMethod.PointAndClick || KickStarter.settingsManager.movementMethod == MovementMethod.StraightToCursor || KickStarter.settingsManager.movementMethod == MovementMethod.None)
				{
					KickStarter.player.StartDecelerating();
				}
				else
				{
					KickStarter.player.charState = CharState.Decelerate;
				}
				KickStarter.player.EndPath();
				hotspotMovingTo = null;
			}
			DeselectHotspot();
			inPreInteractionCutscene = false;
			KickStarter.playerMenus.CloseInteractionMenus();
			if ((bool)KickStarter.player)
			{
				KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
			}
			if (button == null)
			{
				if (selectedItemID >= 0 && KickStarter.runtimeInventory.GetItem(selectedItemID) != null && (bool)KickStarter.runtimeInventory.GetItem(selectedItemID).unhandledActionList)
				{
					ActionListAsset unhandledActionList = KickStarter.runtimeInventory.GetItem(selectedItemID).unhandledActionList;
					RunUnhandledHotspotInteraction(unhandledActionList, _hotspot, KickStarter.inventoryManager.passUnhandledHotspotAsParameter);
				}
				else if (selectedItemID >= 0 && (bool)KickStarter.runtimeInventory.unhandledGive && KickStarter.runtimeInventory.IsGivingItem())
				{
					RunUnhandledHotspotInteraction(KickStarter.runtimeInventory.unhandledGive, _hotspot, KickStarter.inventoryManager.passUnhandledHotspotAsParameter);
				}
				else if (selectedItemID >= 0 && (bool)KickStarter.runtimeInventory.unhandledHotspot && !KickStarter.runtimeInventory.IsGivingItem())
				{
					RunUnhandledHotspotInteraction(KickStarter.runtimeInventory.unhandledHotspot, _hotspot, KickStarter.inventoryManager.passUnhandledHotspotAsParameter);
				}
				else
				{
					KickStarter.actionListManager.SetCorrectGameState();
					if (KickStarter.settingsManager.InventoryDragDrop)
					{
						KickStarter.runtimeInventory.SetNull();
					}
				}
			}
			else
			{
				if (KickStarter.settingsManager.InventoryDragDrop || KickStarter.settingsManager.inventoryDisableDefined)
				{
					KickStarter.runtimeInventory.SetNull();
				}
				if (_hotspot.interactionSource == InteractionSource.AssetFile)
				{
					if (button.assetFile != null)
					{
						if (button.invParameterID >= 0)
						{
							ActionParameter parameter = button.assetFile.GetParameter(button.invParameterID);
							if (parameter != null && parameter.parameterType == ParameterType.InventoryItem)
							{
								parameter.intValue = selectedItemID;
							}
						}
						if (button.parameterID >= 0)
						{
							ActionParameter parameter2 = button.assetFile.GetParameter(button.parameterID);
							if (parameter2 != null && parameter2.parameterType == ParameterType.GameObject)
							{
								parameter2.gameObject = _hotspot.gameObject;
								if ((bool)_hotspot.gameObject.GetComponent<ConstantID>())
								{
									parameter2.intValue = _hotspot.gameObject.GetComponent<ConstantID>().constantID;
								}
								else
								{
									ACDebug.LogWarning("Cannot set the value of parameter " + button.parameterID + " ('" + parameter2.label + "') as " + _hotspot.gameObject.name + " has no Constant ID component.", _hotspot);
								}
							}
							else if (parameter2 != null && parameter2.parameterType == ParameterType.ComponentVariable)
							{
								parameter2.variables = _hotspot.gameObject.GetComponent<Variables>();
							}
						}
						AdvGame.RunActionListAsset(button.assetFile);
					}
					else if (_hotspot.GetButtonInteractionType(button) == HotspotInteractionType.UnhandledUse && KickStarter.cursorManager.AllowUnhandledIcons())
					{
						ActionListAsset unhandledInteraction = KickStarter.cursorManager.GetUnhandledInteraction(lastClickedCursorID);
						RunUnhandledHotspotInteraction(unhandledInteraction, _hotspot, KickStarter.cursorManager.passUnhandledHotspotAsParameter);
					}
					else
					{
						KickStarter.actionListManager.SetCorrectGameState();
					}
				}
				else if (_hotspot.interactionSource == InteractionSource.CustomScript)
				{
					if (button.customScriptObject != null && !string.IsNullOrEmpty(button.customScriptFunction))
					{
						if (selectedItemID >= 0)
						{
							button.customScriptObject.SendMessage(button.customScriptFunction, selectedItemID);
						}
						else
						{
							button.customScriptObject.SendMessage(button.customScriptFunction);
						}
					}
				}
				else if (_hotspot.interactionSource == InteractionSource.InScene)
				{
					if (button.interaction != null)
					{
						if (button.parameterID >= 0 && _hotspot != null)
						{
							ActionParameter parameter3 = button.interaction.GetParameter(button.parameterID);
							if (parameter3 != null && parameter3.parameterType == ParameterType.GameObject)
							{
								parameter3.gameObject = _hotspot.gameObject;
							}
							else if (parameter3 != null && parameter3.parameterType == ParameterType.ComponentVariable)
							{
								parameter3.variables = _hotspot.gameObject.GetComponent<Variables>();
							}
						}
						if (button.invParameterID >= 0)
						{
							ActionParameter parameter4 = button.interaction.GetParameter(button.invParameterID);
							if (parameter4 != null && parameter4.parameterType == ParameterType.InventoryItem)
							{
								parameter4.intValue = selectedItemID;
							}
						}
						button.interaction.Interact();
					}
					else if (_hotspot.GetButtonInteractionType(button) == HotspotInteractionType.UnhandledUse && KickStarter.cursorManager.AllowUnhandledIcons())
					{
						ActionListAsset unhandledInteraction2 = KickStarter.cursorManager.GetUnhandledInteraction(lastClickedCursorID);
						RunUnhandledHotspotInteraction(unhandledInteraction2, _hotspot, KickStarter.cursorManager.passUnhandledHotspotAsParameter);
					}
					else
					{
						KickStarter.actionListManager.SetCorrectGameState();
					}
				}
			}
			button = null;
			if (KickStarter.stateHandler.IsInGameplay())
			{
				preventInteractionsThisFrame = true;
				UpdateInteraction();
			}
		}

		protected void RunUnhandledHotspotInteraction(ActionListAsset _actionListAsset, Hotspot _hotspot, bool optionValue)
		{
			if (KickStarter.settingsManager.inventoryDisableUnhandled)
			{
				KickStarter.runtimeInventory.SetNull();
			}
			if (_actionListAsset != null)
			{
				if (optionValue && _hotspot != null)
				{
					AdvGame.RunActionListAsset(_actionListAsset, _hotspot.gameObject);
				}
				else
				{
					AdvGame.RunActionListAsset(_actionListAsset);
				}
			}
		}

		public string GetLabelPrefix(Hotspot _hotspot, InvItem _invItem, int languageNumber = 0, int cursorID = -1)
		{
			if (_invItem != null)
			{
				_hotspot = null;
			}
			bool flag = cursorID >= 0;
			if (!flag)
			{
				cursorID = ((cursorID != -1 || !(_hotspot != null) || !_hotspot.IsSingleInteraction() || KickStarter.runtimeInventory.SelectedItem != null || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive) ? KickStarter.playerCursor.GetSelectedCursorID() : _hotspot.GetFirstUseIcon());
			}
			string result = string.Empty;
			if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeCursor)
			{
				result = KickStarter.runtimeInventory.GetHotspotPrefixLabel(KickStarter.runtimeInventory.SelectedItem, KickStarter.runtimeInventory.SelectedItem.GetLabel(languageNumber), languageNumber, true);
			}
			else
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingMenuAndClickingHotspot && _invItem == null && _hotspot != null && interactionIndex >= 0 && KickStarter.playerMenus.IsInteractionMenuOn() && interactionIndex >= _hotspot.useButtons.Count)
				{
					int num = interactionIndex - _hotspot.useButtons.Count;
					if (_hotspot.invButtons.Count > num)
					{
						InvItem item = KickStarter.runtimeInventory.GetItem(_hotspot.invButtons[num].invID);
						if (item != null)
						{
							KickStarter.runtimeInventory.SetSelectItemMode(_hotspot.invButtons[num].selectItemMode);
						}
					}
				}
				if (KickStarter.cursorManager.addHotspotPrefix)
				{
					if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
					{
						if ((bool)_hotspot && _hotspot.provideUseInteraction && KickStarter.runtimeInventory.SelectedItem == null)
						{
							Button firstUseButton = _hotspot.GetFirstUseButton();
							if (firstUseButton != null)
							{
								result = KickStarter.cursorManager.GetLabelFromID(firstUseButton.iconID, languageNumber);
							}
						}
					}
					else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.CustomScript)
					{
						result = KickStarter.cursorManager.GetLabelFromID(cursorID, languageNumber);
					}
					else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						if (KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot || KickStarter.settingsManager.selectInteractions == SelectInteractions.ClickingMenu)
						{
							result = KickStarter.cursorManager.GetLabelFromID(cursorID, languageNumber);
						}
						else if (KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingMenuAndClickingHotspot)
						{
							if (_invItem != null)
							{
								if (interactionIndex >= 0 && KickStarter.playerMenus.IsInteractionMenuOn())
								{
									if (_invItem.interactions.Count > interactionIndex)
									{
										result = KickStarter.cursorManager.GetLabelFromID(_invItem.interactions[interactionIndex].icon.id, languageNumber);
									}
									else
									{
										int num2 = interactionIndex - _invItem.interactions.Count;
										if (_invItem.interactions.Count > num2)
										{
											InvItem item2 = KickStarter.runtimeInventory.GetItem(_invItem.combineID[num2]);
											if (item2 != null)
											{
												result = KickStarter.runtimeInventory.GetHotspotPrefixLabel(item2, item2.GetLabel(languageNumber), languageNumber);
											}
										}
									}
								}
							}
							else if (_hotspot != null)
							{
								if (interactionIndex >= 0 && KickStarter.playerMenus.IsInteractionMenuOn())
								{
									if (interactionIndex < _hotspot.useButtons.Count)
									{
										result = KickStarter.cursorManager.GetLabelFromID(_hotspot.useButtons[interactionIndex].iconID, languageNumber);
									}
									else
									{
										int num3 = interactionIndex - _hotspot.useButtons.Count;
										if (_hotspot.invButtons.Count > num3)
										{
											InvItem item3 = KickStarter.runtimeInventory.GetItem(_hotspot.invButtons[num3].invID);
											if (item3 != null)
											{
												result = KickStarter.runtimeInventory.GetHotspotPrefixLabel(item3, item3.GetLabel(languageNumber), languageNumber, true);
											}
										}
									}
								}
								else if (_hotspot.IsSingleInteraction() && _hotspot.provideUseInteraction && KickStarter.runtimeInventory.SelectedItem == null)
								{
									Button firstUseButton2 = _hotspot.GetFirstUseButton();
									if (firstUseButton2 != null)
									{
										result = KickStarter.cursorManager.GetLabelFromID(firstUseButton2.iconID, languageNumber);
									}
								}
							}
						}
					}
				}
			}
			if (!flag && cursorID == -1 && KickStarter.runtimeInventory.SelectedItem == null && KickStarter.cursorManager.addWalkPrefix && !KickStarter.playerMenus.IsInteractionMenuOn() && _invItem == null && (!(_hotspot != null) || KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive || _hotspot.GetFirstUseButton() == null))
			{
				result = KickStarter.runtimeLanguages.GetTranslation(KickStarter.cursorManager.walkPrefix.label, KickStarter.cursorManager.walkPrefix.lineID, languageNumber);
			}
			return result;
		}

		protected void StopInteraction()
		{
			button = null;
			inPreInteractionCutscene = false;
			StopCoroutine("UseObject");
			hotspotMovingTo = null;
		}

		public Vector2 GetHotspotScreenCentre()
		{
			if ((bool)hotspot)
			{
				Vector2 iconScreenPosition = hotspot.GetIconScreenPosition();
				return new Vector2(iconScreenPosition.x / (float)ACScreen.width, 1f - iconScreenPosition.y / (float)ACScreen.height);
			}
			return Vector2.zero;
		}

		public Vector2 GetLastHotspotScreenCentre()
		{
			if ((bool)GetLastOrActiveHotspot())
			{
				Vector2 iconScreenPosition = GetLastOrActiveHotspot().GetIconScreenPosition();
				return new Vector2(iconScreenPosition.x / (float)ACScreen.width, 1f - iconScreenPosition.y / (float)ACScreen.height);
			}
			return Vector2.zero;
		}

		public bool IsMouseOverHotspot()
		{
			if ((bool)KickStarter.settingsManager && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && (bool)KickStarter.playerCursor && KickStarter.playerCursor.GetSelectedCursor() == -1)
			{
				return false;
			}
			if (SceneSettings.IsUnity2D())
			{
				RaycastHit2D raycastHit2D = default(RaycastHit2D);
				if (KickStarter.mainCamera.IsOrthographic())
				{
					raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(KickStarter.playerInput.GetMousePosition()), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, HotspotLayerMask);
				}
				else
				{
					Vector3 position = KickStarter.playerInput.GetMousePosition();
					position.z = 0f - KickStarter.CameraMain.transform.position.z;
					raycastHit2D = UnityVersionHandler.Perform2DRaycast(KickStarter.CameraMain.ScreenToWorldPoint(position), Vector2.zero, KickStarter.settingsManager.navMeshRaycastLength, HotspotLayerMask);
				}
				if (raycastHit2D.collider != null && (bool)raycastHit2D.collider.gameObject.GetComponent<Hotspot>())
				{
					return true;
				}
			}
			else
			{
				Ray ray = KickStarter.CameraMain.ScreenPointToRay(KickStarter.playerInput.GetMousePosition());
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.hotspotRaycastLength, HotspotLayerMask) && (bool)hitInfo.collider.gameObject.GetComponent<Hotspot>())
				{
					return true;
				}
				if (Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.moveableRaycastLength, HotspotLayerMask) && (bool)hitInfo.collider.gameObject.GetComponent<DragBase>())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsDroppingInventory()
		{
			if (!KickStarter.settingsManager.CanSelectItems(false))
			{
				return false;
			}
			if (KickStarter.stateHandler.gameState == GameState.Cutscene || KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				return false;
			}
			if (KickStarter.runtimeInventory.SelectedItem == null || !KickStarter.runtimeInventory.localItems.Contains(KickStarter.runtimeInventory.SelectedItem))
			{
				return false;
			}
			if (KickStarter.settingsManager.InventoryDragDrop && KickStarter.playerInput.GetMouseState() == MouseState.Normal && KickStarter.playerInput.GetDragState() == DragState.Inventory)
			{
				return true;
			}
			if (KickStarter.settingsManager.InventoryDragDrop && KickStarter.playerInput.CanClick() && KickStarter.playerInput.GetMouseState() == MouseState.Normal && KickStarter.playerInput.GetDragState() == DragState.None)
			{
				return true;
			}
			if (KickStarter.playerInput.GetMouseState() == MouseState.SingleClick && KickStarter.settingsManager.inventoryDisableLeft)
			{
				return true;
			}
			if (KickStarter.playerInput.GetMouseState() == MouseState.RightClick && KickStarter.settingsManager.RightClickInventory == RightClickInventory.DeselectsItem && (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive || KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single))
			{
				return true;
			}
			if (KickStarter.playerInput.GetMouseState() == MouseState.RightClick && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.cycleInventoryCursors)
			{
				return true;
			}
			return false;
		}

		public Hotspot GetActiveHotspot()
		{
			return hotspot;
		}

		public Hotspot GetLastOrActiveHotspot()
		{
			if (hotspot != null)
			{
				lastHotspot = hotspot;
				return hotspot;
			}
			return lastHotspot;
		}

		public int GetActiveUseButtonIconID()
		{
			if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
				{
					if (interactionIndex == -1)
					{
						if (KickStarter.runtimeInventory.hoverItem.interactions == null || KickStarter.runtimeInventory.hoverItem.interactions.Count == 0)
						{
							return -1;
						}
						interactionIndex = 0;
						return 0;
					}
					if (KickStarter.runtimeInventory.hoverItem.interactions != null && interactionIndex < KickStarter.runtimeInventory.hoverItem.interactions.Count)
					{
						return KickStarter.runtimeInventory.hoverItem.interactions[interactionIndex].icon.id;
					}
				}
				else if ((bool)GetActiveHotspot())
				{
					if (interactionIndex == -1)
					{
						if (GetActiveHotspot().GetFirstUseButton() == null)
						{
							return -1;
						}
						interactionIndex = GetActiveHotspot().FindFirstEnabledInteraction();
						return interactionIndex;
					}
					if (interactionIndex < GetActiveHotspot().useButtons.Count)
					{
						if (!GetActiveHotspot().useButtons[interactionIndex].isDisabled)
						{
							return GetActiveHotspot().useButtons[interactionIndex].iconID;
						}
						interactionIndex = -1;
						if (GetActiveHotspot().GetFirstUseButton() == null)
						{
							return -1;
						}
						interactionIndex = GetActiveHotspot().FindFirstEnabledInteraction();
						return interactionIndex;
					}
				}
			}
			else if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
				{
					if (interactionIndex == -1)
					{
						return -1;
					}
					if (KickStarter.runtimeInventory.hoverItem.interactions != null && interactionIndex < KickStarter.runtimeInventory.hoverItem.interactions.Count)
					{
						return KickStarter.runtimeInventory.hoverItem.interactions[interactionIndex].icon.id;
					}
				}
				else if ((bool)GetActiveHotspot())
				{
					if (interactionIndex == -1)
					{
						if (GetActiveHotspot().GetFirstUseButton() == null)
						{
							return GetActiveHotspot().FindFirstEnabledInteraction();
						}
						interactionIndex = 0;
						return 0;
					}
					if (interactionIndex < GetActiveHotspot().useButtons.Count)
					{
						return GetActiveHotspot().useButtons[interactionIndex].iconID;
					}
				}
			}
			return -1;
		}

		public int GetActiveInvButtonID()
		{
			if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
				{
					int num = ((KickStarter.runtimeInventory.hoverItem.interactions != null) ? KickStarter.runtimeInventory.hoverItem.interactions.Count : 0);
					if (interactionIndex >= num && KickStarter.runtimeInventory.matchingInvInteractions.Count > 0)
					{
						int index = KickStarter.runtimeInventory.matchingInvInteractions[interactionIndex - num];
						return KickStarter.runtimeInventory.hoverItem.combineID[index];
					}
				}
				else if ((bool)GetActiveHotspot() && interactionIndex >= GetActiveHotspot().useButtons.Count)
				{
					int num2 = interactionIndex - GetActiveHotspot().useButtons.Count;
					if (num2 < KickStarter.runtimeInventory.matchingInvInteractions.Count)
					{
						Button button = GetActiveHotspot().invButtons[KickStarter.runtimeInventory.matchingInvInteractions[num2]];
						return button.invID;
					}
				}
			}
			else if (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
			{
				int num3 = ((KickStarter.runtimeInventory.hoverItem.interactions != null) ? KickStarter.runtimeInventory.hoverItem.interactions.Count : 0);
				if (interactionIndex >= num3 && KickStarter.runtimeInventory.matchingInvInteractions.Count > 0)
				{
					return KickStarter.runtimeInventory.hoverItem.combineID[KickStarter.runtimeInventory.matchingInvInteractions[interactionIndex - num3]];
				}
			}
			else if ((bool)GetActiveHotspot())
			{
				int num4 = interactionIndex - GetActiveHotspot().useButtons.Count;
				if (num4 >= 0 && KickStarter.runtimeInventory.matchingInvInteractions.Count > num4)
				{
					int num5 = KickStarter.runtimeInventory.matchingInvInteractions[num4];
					if (GetActiveHotspot().invButtons.Count > num5)
					{
						Button button2 = GetActiveHotspot().invButtons[num5];
						return button2.invID;
					}
				}
			}
			return -1;
		}

		public void SetNextInteraction()
		{
			OffsetInteraction(true);
		}

		public void SetPreviousInteraction()
		{
			OffsetInteraction(false);
		}

		protected void OffsetInteraction(bool goForward)
		{
			if (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				if ((KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.hoverItem == null && hotspot == null) || (KickStarter.runtimeInventory.hoverItem != null && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Single))
				{
					return;
				}
				if (KickStarter.runtimeInventory.hoverItem != null)
				{
					if (goForward)
					{
						interactionIndex = KickStarter.runtimeInventory.hoverItem.GetNextInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
					else
					{
						interactionIndex = KickStarter.runtimeInventory.hoverItem.GetPreviousInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
				}
				else if (GetActiveHotspot() != null)
				{
					if (goForward)
					{
						interactionIndex = GetActiveHotspot().GetNextInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
					else
					{
						interactionIndex = GetActiveHotspot().GetPreviousInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
				}
				if (GetActiveInvButtonID() >= 0)
				{
					if (KickStarter.settingsManager.cycleInventoryCursors)
					{
						KickStarter.runtimeInventory.SelectItemByID(GetActiveInvButtonID());
					}
				}
				else
				{
					KickStarter.runtimeInventory.SetNull();
				}
				if (KickStarter.runtimeInventory.hoverItem != null)
				{
					KickStarter.runtimeInventory.hoverItem.lastInteractionIndex = interactionIndex;
				}
				else if (GetActiveHotspot() != null)
				{
					GetActiveHotspot().lastInteractionIndex = interactionIndex;
				}
			}
			else if (KickStarter.runtimeInventory.hoverItem != null)
			{
				if (goForward)
				{
					interactionIndex = KickStarter.runtimeInventory.hoverItem.GetNextInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
				}
				else
				{
					interactionIndex = KickStarter.runtimeInventory.hoverItem.GetPreviousInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
				}
			}
			else
			{
				if (!(GetActiveHotspot() != null))
				{
					return;
				}
				if (KickStarter.settingsManager.cycleInventoryCursors)
				{
					if (goForward)
					{
						interactionIndex = GetActiveHotspot().GetNextInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
					else
					{
						interactionIndex = GetActiveHotspot().GetPreviousInteraction(interactionIndex, KickStarter.runtimeInventory.matchingInvInteractions.Count);
					}
				}
				else if (goForward)
				{
					interactionIndex = GetActiveHotspot().GetNextInteraction(interactionIndex, 0);
				}
				else
				{
					interactionIndex = GetActiveHotspot().GetPreviousInteraction(interactionIndex, 0);
				}
			}
		}

		public void ResetInteractionIndex()
		{
			interactionIndex = -1;
			if ((bool)GetActiveHotspot())
			{
				interactionIndex = GetActiveHotspot().FindFirstEnabledInteraction();
			}
			else if (KickStarter.runtimeInventory.hoverItem != null)
			{
				interactionIndex = 0;
			}
		}

		public int GetInteractionIndex()
		{
			return interactionIndex;
		}

		public void SetInteractionIndex(int _interactionIndex)
		{
			interactionIndex = _interactionIndex;
		}

		public void RestoreInventoryInteraction()
		{
			if ((KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.CanSelectItems(false)) || KickStarter.settingsManager.SelectInteractionMethod() != SelectInteractions.CyclingCursorAndClickingHotspot || KickStarter.runtimeInventory.hoverItem == null)
			{
				return;
			}
			if (KickStarter.settingsManager.whenReselectHotspot == WhenReselectHotspot.ResetIcon)
			{
				KickStarter.runtimeInventory.hoverItem.lastInteractionIndex = (interactionIndex = 0);
				return;
			}
			interactionIndex = KickStarter.runtimeInventory.hoverItem.lastInteractionIndex;
			if (!KickStarter.settingsManager.cycleInventoryCursors && GetActiveInvButtonID() >= 0)
			{
				interactionIndex = -1;
				return;
			}
			int activeInvButtonID = GetActiveInvButtonID();
			if (activeInvButtonID >= 0)
			{
				KickStarter.runtimeInventory.SelectItemByID(activeInvButtonID);
			}
			else if (KickStarter.settingsManager.cycleInventoryCursors && KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
			{
				KickStarter.runtimeInventory.SetNull();
			}
		}

		protected void RestoreHotspotInteraction()
		{
			if (!KickStarter.settingsManager.cycleInventoryCursors && KickStarter.runtimeInventory.SelectedItem != null)
			{
				return;
			}
			if (KickStarter.settingsManager.whenReselectHotspot == WhenReselectHotspot.ResetIcon)
			{
				hotspot.lastInteractionIndex = (interactionIndex = 0);
			}
			else
			{
				if (!(hotspot != null))
				{
					return;
				}
				interactionIndex = hotspot.lastInteractionIndex;
				if (!KickStarter.settingsManager.cycleInventoryCursors && GetActiveInvButtonID() >= 0)
				{
					interactionIndex = -1;
					return;
				}
				int activeInvButtonID = GetActiveInvButtonID();
				if (activeInvButtonID >= 0)
				{
					KickStarter.runtimeInventory.SelectItemByID(activeInvButtonID);
				}
				else
				{
					KickStarter.runtimeInventory.SetNull();
				}
			}
		}

		protected void ClickHotspotToInteract()
		{
			int activeInvButtonID = GetActiveInvButtonID();
			if (activeInvButtonID == -1)
			{
				ClickButton(InteractionType.Use, GetActiveUseButtonIconID(), -1);
			}
			else
			{
				ClickButton(InteractionType.Inventory, -1, activeInvButtonID);
			}
		}

		public void ClickInteractionIcon(Menu _menu, int iconID)
		{
			if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				ACDebug.LogWarning("This element is not compatible with the Context-Sensitive interaction method.");
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				KickStarter.playerCursor.SetCursorFromID(iconID);
			}
			else if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.ClickingMenu || (KickStarter.settingsManager.SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot && _menu.IsUnityUI() && _menu.ignoreMouseClicks)))
			{
				if (_menu.TargetInvItem != null)
				{
					_menu.TurnOff();
					KickStarter.runtimeInventory.RunInteraction(iconID, _menu.TargetInvItem);
				}
				else if (_menu.TargetHotspot != null)
				{
					_menu.TurnOff();
					ClickButton(InteractionType.Use, iconID, -1, _menu.TargetHotspot);
				}
			}
		}

		public Hotspot GetHotspotMovingTo()
		{
			return hotspotMovingTo;
		}

		public void StopMovingToHotspot()
		{
			if ((bool)KickStarter.player)
			{
				KickStarter.player.EndPath();
				KickStarter.player.ClearHeadTurnTarget(false, HeadFacing.Hotspot);
			}
			KickStarter.eventManager.Call_OnHotspotStopMovingTo(hotspotMovingTo);
			StopInteraction();
		}

		protected string GetInteractionLabel(int _language)
		{
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations)
			{
				return string.Empty;
			}
			if (KickStarter.stateHandler.IsInCutscene())
			{
				return string.Empty;
			}
			InvItem selectedItem = KickStarter.runtimeInventory.SelectedItem;
			if (hotspot != null)
			{
				return hotspot.GetFullLabel(_language);
			}
			if (selectedItem != null)
			{
				if (KickStarter.cursorManager.onlyShowInventoryLabelOverHotspots)
				{
					return string.Empty;
				}
				if (KickStarter.cursorManager.inventoryHandling != InventoryHandling.ChangeCursor)
				{
					return KickStarter.runtimeInventory.GetHotspotPrefixLabel(selectedItem, selectedItem.GetLabel(_language), _language);
				}
			}
			else
			{
				if (KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					int selectedCursorID = KickStarter.playerCursor.GetSelectedCursorID();
					if (selectedCursorID >= 0)
					{
						return KickStarter.cursorManager.GetLabelFromID(selectedCursorID, _language);
					}
				}
				if (KickStarter.playerCursor.GetSelectedCursor() == -1 && KickStarter.cursorManager.addWalkPrefix)
				{
					return KickStarter.runtimeLanguages.GetTranslation(KickStarter.cursorManager.walkPrefix.label, KickStarter.cursorManager.walkPrefix.lineID, _language);
				}
			}
			return string.Empty;
		}

		protected virtual bool UnityUIBlocksClick()
		{
			if (KickStarter.settingsManager.unityUIClicksAlwaysBlocks && KickStarter.settingsManager.hotspotDetection == HotspotDetection.MouseOver)
			{
				if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && KickStarter.playerMenus.EventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
					{
						return true;
					}
					return false;
				}
				if (KickStarter.playerMenus.EventSystem.IsPointerOverGameObject())
				{
					return true;
				}
			}
			return false;
		}
	}
}
