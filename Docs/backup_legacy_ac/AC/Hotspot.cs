using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[AddComponentMenu("Adventure Creator/Hotspots/Hotspot")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_hotspot.html")]
	public class Hotspot : MonoBehaviour, ITranslatable
	{
		public InteractionSource interactionSource;

		public _Camera limitToCamera;

		public InteractiveBoundary interactiveBoundary;

		public string hotspotName;

		public int lineID = -1;

		public Highlight highlight;

		public Marker walkToMarker;

		public Transform centrePoint;

		public bool provideUseInteraction;

		public Button useButton = new Button();

		public List<Button> useButtons = new List<Button>();

		public bool oneClick;

		public bool provideLookInteraction;

		public Button lookButton = new Button();

		public bool provideInvInteraction;

		public List<Button> invButtons = new List<Button>();

		public bool provideUnhandledUseInteraction;

		public bool provideUnhandledInvInteraction;

		public Button unhandledUseButton = new Button();

		public Button unhandledInvButton = new Button();

		public bool drawGizmos = true;

		public int lastInteractionIndex;

		public int displayLineID = -1;

		public string iconSortingLayer = string.Empty;

		public int iconSortingOrder;

		public DoubleClickingHotspot doubleClickingHotspot;

		public bool playerTurnsHead = true;

		protected Collider _collider;

		protected Collider2D _collider2D;

		protected bool isOn = true;

		protected float iconAlpha;

		protected Sprite iconSprite;

		protected SpriteRenderer iconRenderer;

		protected CursorIcon mainIcon;

		protected LerpUtils.FloatLerp iconAlphaLerp = new LerpUtils.FloatLerp(true);

		protected float manualShowIconSpeed = 5f;

		protected bool manuallyShowIcon;

		protected bool tooFarAway;

		protected void Awake()
		{
			if ((bool)KickStarter.settingsManager && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				UpgradeSelf();
			}
			_collider = GetComponent<Collider>();
			_collider2D = GetComponent<Collider2D>();
			lastInteractionIndex = FindFirstEnabledInteraction();
			displayLineID = lineID;
		}

		protected void OnEnable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void Start()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Register(this);
			}
		}

		protected void OnDisable()
		{
			if ((bool)KickStarter.stateHandler)
			{
				KickStarter.stateHandler.Unregister(this);
			}
		}

		public void RunExamineInteraction()
		{
			if (lookButton != null)
			{
				KickStarter.playerInteraction.ExamineHotspot(this);
			}
		}

		public void RunUseInteraction(int iconID = -1)
		{
			if (useButtons != null && useButtons.Count != 0)
			{
				iconID = Mathf.Max(-1, iconID);
				KickStarter.playerInteraction.UseHotspot(this, iconID);
			}
		}

		public void RunInventoryInteraction(int invID = -1)
		{
			if (invID < 0)
			{
				if (KickStarter.runtimeInventory.SelectedItem == null)
				{
					return;
				}
				invID = KickStarter.runtimeInventory.SelectedItem.id;
			}
			KickStarter.playerInteraction.UseInventoryOnHotspot(this, invID);
		}

		public void RunInventoryInteraction(InvItem invItem = null)
		{
			int num = -1;
			if (invItem != null)
			{
				num = invItem.id;
			}
			else
			{
				if (KickStarter.runtimeInventory.SelectedItem == null)
				{
					return;
				}
				num = KickStarter.runtimeInventory.SelectedItem.id;
			}
			KickStarter.playerInteraction.UseInventoryOnHotspot(this, num);
		}

		public void SetProximity(bool isGameplay)
		{
			if (!(highlight != null))
			{
				return;
			}
			if (base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer) || !isGameplay || !IsOn())
			{
				highlight.SetMinHighlight(0f);
				return;
			}
			float num = (GetIconScreenPosition() - KickStarter.playerInput.GetMousePosition()).magnitude / ACScreen.safeArea.size.magnitude;
			if (num < 0f)
			{
				num = 0f;
			}
			else if (num > 1f)
			{
				num = 1f;
			}
			highlight.SetMinHighlight(1f - num * KickStarter.settingsManager.highlightProximityFactor);
		}

		public bool UpgradeSelf()
		{
			if (useButton.IsButtonModified())
			{
				Button button = new Button();
				button.CopyButton(useButton);
				useButtons.Add(button);
				useButton = new Button();
				provideUseInteraction = true;
				if (Application.isPlaying)
				{
					ACDebug.Log("Hotspot '" + base.gameObject.name + "' has been temporarily upgraded - please view its Inspector when the game ends and save the scene.", base.gameObject);
				}
				else
				{
					ACDebug.Log("Upgraded Hotspot '" + base.gameObject.name + "', please save the scene.", base.gameObject);
				}
				return true;
			}
			return false;
		}

		public void DrawHotspotIcon(bool inWorldSpace = false)
		{
			if (iconAlpha > 0f)
			{
				if (!KickStarter.mainCamera.IsPointInCamera(GetIconScreenPosition()))
				{
					return;
				}
				if (inWorldSpace)
				{
					if (iconRenderer == null)
					{
						GameObject gameObject = new GameObject(base.name + " - icon");
						iconRenderer = gameObject.AddComponent<SpriteRenderer>();
						gameObject.transform.localScale = Vector3.one * (25f * KickStarter.settingsManager.hotspotIconSize);
						if (iconSortingLayer != string.Empty)
						{
							iconRenderer.GetComponent<SpriteRenderer>().sortingLayerName = iconSortingLayer;
						}
						iconRenderer.GetComponent<SpriteRenderer>().sortingOrder = iconSortingOrder;
					}
					if (KickStarter.settingsManager.hotspotIcon == HotspotIcon.UseIcon)
					{
						GetMainIcon();
						if (mainIcon != null)
						{
							iconRenderer.sprite = mainIcon.GetSprite();
						}
					}
					else
					{
						if (iconSprite == null && KickStarter.settingsManager.hotspotIconTexture != null)
						{
							iconSprite = Sprite.Create(KickStarter.settingsManager.hotspotIconTexture, new Rect(0f, 0f, KickStarter.settingsManager.hotspotIconTexture.width, KickStarter.settingsManager.hotspotIconTexture.height), new Vector2(0.5f, 0.5f));
						}
						if (iconSprite != iconRenderer.sprite)
						{
							iconRenderer.sprite = iconSprite;
						}
					}
					iconRenderer.transform.position = GetIconPosition();
					iconRenderer.transform.LookAt(iconRenderer.transform.position + KickStarter.mainCamera.transform.rotation * Vector3.forward, KickStarter.mainCamera.transform.rotation * Vector3.up);
				}
				else
				{
					if (iconRenderer != null)
					{
						Object.Destroy(iconRenderer.gameObject);
						iconRenderer = null;
					}
					Color color = GUI.color;
					Color color2 = color;
					color.a = iconAlpha;
					GUI.color = color;
					if (KickStarter.settingsManager.hotspotIcon == HotspotIcon.UseIcon)
					{
						GetMainIcon();
						if (mainIcon != null)
						{
							mainIcon.Draw(GetIconScreenPosition(), !KickStarter.playerMenus.IsMouseOverInteractionMenu());
						}
					}
					else if (KickStarter.settingsManager.hotspotIconTexture != null)
					{
						GUI.DrawTexture(AdvGame.GUIBox(GetIconScreenPosition(), KickStarter.settingsManager.hotspotIconSize), KickStarter.settingsManager.hotspotIconTexture, ScaleMode.ScaleToFit, true, 0f);
					}
					GUI.color = color2;
				}
			}
			if (inWorldSpace && iconRenderer != null)
			{
				Color color3 = iconRenderer.color;
				color3.a = iconAlpha;
				iconRenderer.color = color3;
			}
		}

		public string GetFullLabel(int languageNumber = 0, int cursorID = -1)
		{
			if (KickStarter.stateHandler.gameState == GameState.DialogOptions && !KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && !KickStarter.settingsManager.allowGameplayDuringConversations)
			{
				return string.Empty;
			}
			return AdvGame.CombineLanguageString(KickStarter.playerInteraction.GetLabelPrefix(this, null, languageNumber, cursorID), GetName(languageNumber), languageNumber);
		}

		public void UpdateIcon()
		{
			CanDisplayHotspotIcon();
		}

		public void UpdateProximity(DetectHotspots detectHotspots)
		{
			if (!(detectHotspots == null))
			{
				tooFarAway = !detectHotspots.IsHotspotInTrigger(this);
				if (tooFarAway)
				{
					PlaceOnDistantLayer();
				}
				else
				{
					PlaceOnHotspotLayer();
				}
			}
		}

		public bool UpdateUnhandledVisibility()
		{
			if (!HasEnabledUseInteraction(KickStarter.playerCursor.GetSelectedCursorID()))
			{
				PlaceOnDistantLayer();
				return false;
			}
			PlaceOnHotspotLayer();
			return true;
		}

		public void SetIconVisibility(bool makeVisible, float speed = 5f)
		{
			manuallyShowIcon = makeVisible;
			manualShowIconSpeed = speed;
		}

		public Button GetFirstUseButton()
		{
			foreach (Button useButton in useButtons)
			{
				if (useButton != null && !useButton.isDisabled)
				{
					return useButton;
				}
			}
			return null;
		}

		public int GetFirstUseIcon()
		{
			foreach (Button useButton in useButtons)
			{
				if (useButton != null && !useButton.isDisabled)
				{
					return useButton.iconID;
				}
			}
			return -1;
		}

		public Button GetUseButton(int iconID)
		{
			foreach (Button useButton in useButtons)
			{
				if (useButton != null && useButton.iconID == iconID)
				{
					return useButton;
				}
			}
			return null;
		}

		public Button GetInvButton(int invID)
		{
			foreach (Button invButton in invButtons)
			{
				if (invButton != null && invButton.invID == invID)
				{
					return invButton;
				}
			}
			return null;
		}

		public HotspotInteractionType GetButtonInteractionType(Button _button)
		{
			if (_button != null)
			{
				if (lookButton == _button)
				{
					return HotspotInteractionType.Examine;
				}
				if (unhandledInvButton == _button)
				{
					return HotspotInteractionType.UnhandledInventory;
				}
				if (unhandledUseButton == _button)
				{
					return HotspotInteractionType.UnhandledUse;
				}
				foreach (Button useButton in useButtons)
				{
					if (useButton != null && _button == useButton)
					{
						return HotspotInteractionType.Use;
					}
				}
				foreach (Button invButton in invButtons)
				{
					if (invButton != null && _button == invButton)
					{
						return HotspotInteractionType.Inventory;
					}
				}
			}
			return HotspotInteractionType.NotFound;
		}

		public int FindFirstEnabledInteraction()
		{
			if (useButtons != null && useButtons.Count > 0)
			{
				for (int i = 0; i < useButtons.Count; i++)
				{
					if (!useButtons[i].isDisabled)
					{
						return i;
					}
				}
			}
			return 0;
		}

		public void LimitToCamera(_Camera _limitToCamera)
		{
			if (limitToCamera != null && _limitToCamera != null)
			{
				if (_limitToCamera == limitToCamera && isOn)
				{
					TurnOn(false);
				}
				else
				{
					TurnOff(false);
				}
			}
		}

		public void TurnOn()
		{
			TurnOn(true);
		}

		public virtual void TurnOn(bool manualSet)
		{
			if (tooFarAway)
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.distantHotspotLayer);
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			}
			if (manualSet)
			{
				if (!isOn && KickStarter.eventManager != null)
				{
					KickStarter.eventManager.Call_OnTurnHotspot(this, true);
				}
				isOn = true;
				if (KickStarter.mainCamera != null)
				{
					LimitToCamera(KickStarter.mainCamera.attachedCamera);
				}
			}
		}

		public void TurnOff()
		{
			TurnOff(true);
		}

		public virtual void TurnOff(bool manualSet)
		{
			base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer);
			if (manualSet)
			{
				if (isOn && KickStarter.eventManager != null)
				{
					KickStarter.eventManager.Call_OnTurnHotspot(this, false);
				}
				isOn = false;
				if (KickStarter.player != null && KickStarter.player.hotspotDetector != null)
				{
					KickStarter.player.hotspotDetector.ForceRemoveHotspot(this);
				}
			}
		}

		public virtual bool IsOn()
		{
			if (this == null || base.gameObject == null)
			{
				return false;
			}
			if (base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer) && !isOn)
			{
				return false;
			}
			return true;
		}

		public bool PlayerIsWithinBoundary()
		{
			if (interactiveBoundary == null || KickStarter.player == null)
			{
				return true;
			}
			return interactiveBoundary.PlayerIsPresent;
		}

		public void Select()
		{
			KickStarter.eventManager.Call_OnChangeHotspot(this, true);
			if (highlight != null && highlight.highlightWhenSelected)
			{
				highlight.HighlightOn();
			}
		}

		public void Deselect()
		{
			KickStarter.eventManager.Call_OnChangeHotspot(this, false);
			if ((bool)highlight)
			{
				highlight.HighlightOff();
			}
		}

		public void DeselectInstant()
		{
			KickStarter.eventManager.Call_OnChangeHotspot(this, false);
			if ((bool)highlight)
			{
				highlight.HighlightOffInstant();
			}
		}

		public void ShowInteractionMenus()
		{
			if (KickStarter.playerMenus != null)
			{
				KickStarter.playerMenus.EnableInteractionMenus(this);
			}
		}

		public bool IsSingleInteraction()
		{
			if (oneClick && provideUseInteraction && useButtons != null && GetFirstUseButton() != null)
			{
				return true;
			}
			return false;
		}

		public bool HasInventoryInteraction(InvItem invItem)
		{
			if (invItem != null)
			{
				if (provideUnhandledInvInteraction && unhandledInvButton != null && !unhandledInvButton.isDisabled)
				{
					return true;
				}
				if (provideInvInteraction && invButtons != null && invButtons.Count > 0)
				{
					for (int i = 0; i < invButtons.Count; i++)
					{
						if (!invButtons[i].isDisabled && invButtons[i].invID == invItem.id)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public Vector2 GetIconScreenPosition()
		{
			Vector3 vector = KickStarter.CameraMain.WorldToScreenPoint(GetIconPosition());
			return new Vector3(vector.x, vector.y);
		}

		public virtual Vector3 GetIconPosition(bool inLocalSpace = false)
		{
			Vector3 vector = base.transform.position;
			if (centrePoint != null)
			{
				if (inLocalSpace)
				{
					return centrePoint.position - base.transform.position;
				}
				return centrePoint.position;
			}
			if (_collider != null)
			{
				vector = _collider.bounds.center;
			}
			else if (_collider2D != null)
			{
				vector = _collider2D.bounds.center;
			}
			if (inLocalSpace)
			{
				return vector - base.transform.position;
			}
			return vector;
		}

		public void ResetMainIcon()
		{
			mainIcon = null;
		}

		public int GetPreviousInteraction(int i, int numInvInteractions)
		{
			if (i > useButtons.Count && numInvInteractions > 0)
			{
				return i - 1;
			}
			if (i == 0)
			{
				return FindLastEnabledInteraction(numInvInteractions);
			}
			if (i <= useButtons.Count)
			{
				i--;
				while (i > 0 && useButtons[i].isDisabled)
				{
					i--;
				}
				if (i < 0)
				{
					return FindLastEnabledInteraction(numInvInteractions);
				}
				if (i == 0 && useButtons.Count > 0 && useButtons[0].isDisabled)
				{
					return FindLastEnabledInteraction(numInvInteractions);
				}
				return i;
			}
			return i - 1;
		}

		public string GetName(int languageNumber)
		{
			string text = base.gameObject.name;
			if (!string.IsNullOrEmpty(hotspotName))
			{
				text = hotspotName;
			}
			if (languageNumber > 0)
			{
				return KickStarter.runtimeLanguages.GetTranslation(text, displayLineID, languageNumber);
			}
			return text;
		}

		public void SetName(string newName, int _lineID)
		{
			hotspotName = newName;
			if (_lineID >= 0)
			{
				displayLineID = _lineID;
			}
			else
			{
				displayLineID = lineID;
			}
		}

		public bool HasContextUse()
		{
			if ((oneClick || KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive) && provideUseInteraction && useButtons != null && GetFirstUseButton() != null)
			{
				return true;
			}
			return false;
		}

		public bool HasContextLook()
		{
			if (provideLookInteraction && lookButton != null && !lookButton.isDisabled)
			{
				return true;
			}
			return false;
		}

		public int GetNextInteraction(int i, int numInvInteractions)
		{
			if (i < useButtons.Count)
			{
				i = ((!IsSingleInteraction()) ? (i + 1) : useButtons.Count);
				while (i < useButtons.Count && useButtons[i].isDisabled)
				{
					i++;
				}
				if (i >= useButtons.Count + numInvInteractions)
				{
					return FindFirstEnabledInteraction();
				}
				return i;
			}
			if (i >= useButtons.Count - 1 + numInvInteractions)
			{
				return FindFirstEnabledInteraction();
			}
			return i + 1;
		}

		protected void FindFirstInteractionIndex()
		{
			lastInteractionIndex = 0;
			foreach (Button useButton in useButtons)
			{
				if (!useButton.isDisabled)
				{
					lastInteractionIndex = useButtons.IndexOf(useButton);
					break;
				}
			}
		}

		protected void PlaceOnDistantLayer()
		{
			if (base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer))
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.distantHotspotLayer);
			}
		}

		protected void PlaceOnHotspotLayer()
		{
			if (base.gameObject.layer == LayerMask.NameToLayer(KickStarter.settingsManager.distantHotspotLayer))
			{
				base.gameObject.layer = LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer);
			}
		}

		protected bool CanDisplayHotspotIcon()
		{
			if (base.gameObject.layer != LayerMask.NameToLayer(KickStarter.settingsManager.deactivatedLayer))
			{
				if (KickStarter.CameraMain == null)
				{
					return false;
				}
				Vector3 vector = base.transform.position - KickStarter.CameraMain.transform.position;
				if (Vector3.Angle(vector, KickStarter.CameraMain.transform.forward) > 90f)
				{
					iconAlpha = 0f;
					return false;
				}
				if (SceneSettings.CameraPerspective != CameraPerspective.TwoD && KickStarter.settingsManager.occludeIcons)
				{
					Ray ray = new Ray(KickStarter.CameraMain.transform.position, GetIconPosition() - KickStarter.CameraMain.transform.position);
					RaycastHit hitInfo;
					if (Physics.Raycast(ray, out hitInfo, KickStarter.settingsManager.hotspotRaycastLength, 1 << LayerMask.NameToLayer(KickStarter.settingsManager.hotspotLayer)) && hitInfo.collider.gameObject != base.gameObject)
					{
						iconAlpha = 0f;
						return false;
					}
				}
				if (!KickStarter.stateHandler.IsInGameplay())
				{
					iconAlpha = 0f;
					return false;
				}
				if (KickStarter.playerMenus.IsInteractionMenuOn() && KickStarter.settingsManager.hideIconUnderInteractionMenu)
				{
					iconAlpha = iconAlphaLerp.Update(iconAlpha, 0f, 5f);
				}
				else if (KickStarter.settingsManager.hotspotIconDisplay == HotspotIconDisplay.ViaScriptOnly)
				{
					if (manualShowIconSpeed > 0f)
					{
						iconAlpha = iconAlphaLerp.Update(iconAlpha, (!manuallyShowIcon) ? 0f : 1f, manualShowIconSpeed);
					}
					else
					{
						iconAlpha = ((!manuallyShowIcon) ? 0f : 1f);
					}
				}
				else if (KickStarter.settingsManager.hotspotIconDisplay == HotspotIconDisplay.OnlyWhenHighlighting || KickStarter.settingsManager.hotspotIconDisplay == HotspotIconDisplay.OnlyWhenFlashing)
				{
					if ((bool)highlight)
					{
						if (KickStarter.settingsManager.hotspotIconDisplay == HotspotIconDisplay.OnlyWhenHighlighting)
						{
							iconAlpha = highlight.GetHighlightAlpha();
						}
						else
						{
							iconAlpha = highlight.GetFlashAlpha(iconAlpha);
						}
					}
					else
					{
						ACDebug.LogWarning("Cannot display correct Hotspot Icon alpha on " + base.name + " because it has no associated Highlight object.", base.gameObject);
					}
				}
				else if (KickStarter.settingsManager.hotspotIconDisplay == HotspotIconDisplay.Always)
				{
					iconAlpha = 1f;
				}
				else
				{
					iconAlpha = 0f;
				}
				return true;
			}
			iconAlpha = 0f;
			return false;
		}

		protected void GetMainIcon()
		{
			if (mainIcon != null || KickStarter.cursorManager == null)
			{
				return;
			}
			if (provideUseInteraction && useButton != null && useButton.iconID >= 0 && !useButton.isDisabled)
			{
				mainIcon = new CursorIcon();
				mainIcon.Copy(KickStarter.cursorManager.GetCursorIconFromID(useButton.iconID), true);
			}
			else if (provideLookInteraction && lookButton != null && lookButton.iconID >= 0 && !lookButton.isDisabled)
			{
				mainIcon = new CursorIcon();
				mainIcon.Copy(KickStarter.cursorManager.GetCursorIconFromID(lookButton.iconID), true);
			}
			else
			{
				if (!provideUseInteraction || useButtons == null || useButtons.Count <= 0)
				{
					return;
				}
				for (int i = 0; i < useButtons.Count; i++)
				{
					if (!useButtons[i].isDisabled)
					{
						mainIcon = new CursorIcon();
						mainIcon.Copy(KickStarter.cursorManager.GetCursorIconFromID(useButtons[i].iconID), true);
						break;
					}
				}
			}
		}

		protected int FindLastEnabledInteraction(int numInvInteractions)
		{
			if (numInvInteractions > 0)
			{
				if (useButtons != null)
				{
					return useButtons.Count - 1 + numInvInteractions;
				}
				return numInvInteractions - 1;
			}
			if (useButtons != null && useButtons.Count > 0)
			{
				for (int num = useButtons.Count - 1; num >= 0; num--)
				{
					if (!useButtons[num].isDisabled)
					{
						return num;
					}
				}
			}
			return 0;
		}

		protected bool HasEnabledUseInteraction(int _iconID)
		{
			if (_iconID >= 0)
			{
				for (int i = 0; i < useButtons.Count; i++)
				{
					if (useButtons[i].iconID == _iconID && !useButtons[i].isDisabled)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected int GetNumInteractions(int numInvInteractions)
		{
			int num = 0;
			foreach (Button useButton in useButtons)
			{
				if (!useButton.isDisabled)
				{
					num++;
				}
			}
			return num + numInvInteractions;
		}

		public string GetTranslatableString(int index)
		{
			if (!string.IsNullOrEmpty(hotspotName))
			{
				return hotspotName;
			}
			return base.name;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}
	}
}
