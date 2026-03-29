using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AC
{
	[Serializable]
	public class SettingsManager : ScriptableObject
	{
		public string saveFileName = string.Empty;

		public SaveTimeDisplay saveTimeDisplay;

		public string customSaveFormat = "MMMM dd, yyyy";

		public bool takeSaveScreenshots;

		public float screenshotResolutionFactor = 1f;

		public bool useProfiles;

		public int maxSaves = 5;

		public bool orderSavesByUpdateTime;

		public bool reloadSceneWhenLoading;

		public bool useJsonSerialization;

		public List<InvVar> sceneAttributes = new List<InvVar>();

		public ActionListAsset actionListOnStart;

		public bool blackOutWhenSkipping;

		public PlayerSwitching playerSwitching = PlayerSwitching.DoNotAllow;

		public Player player;

		public List<PlayerPrefab> players = new List<PlayerPrefab>();

		public MovementMethod movementMethod;

		public InputMethod inputMethod;

		public AC_InteractionMethod interactionMethod;

		public SelectInteractions selectInteractions;

		public bool autoHideInteractionIcons = true;

		public CancelInteractions cancelInteractions;

		public SeeInteractions seeInteractions;

		public bool closeInteractionMenuIfTapHotspot = true;

		public bool stopPlayerOnClickHotspot;

		public bool cycleInventoryCursors = true;

		public bool scaleCursorSpeedWithScreen = true;

		public bool autoCycleWhenInteract;

		public bool showHoverInteractionInHotspotLabel;

		public bool allowDefaultinteractions;

		public WhenReselectHotspot whenReselectHotspot;

		public bool lockCursorOnStart;

		public bool hideLockedCursor;

		public bool disableFreeAimWhenDragging;

		public bool runConversationsWithKeys;

		public bool clickUpInteractions;

		public bool defaultMouseClicks = true;

		public bool allowGameplayDuringConversations;

		public bool shareInventory;

		public bool inventoryDragDrop;

		public float dragDropThreshold;

		public bool allowInventoryInteractionsDuringConversations;

		public bool inventoryDropLook;

		public InventoryInteractions inventoryInteractions = InventoryInteractions.Single;

		public bool inventoryDisableLeft = true;

		public bool allowDefaultInventoryInteractions;

		public bool inventoryDisableUnhandled = true;

		public bool inventoryDisableDefined = true;

		public bool activeWhenHover;

		public InventoryActiveEffect inventoryActiveEffect = InventoryActiveEffect.Simple;

		public float inventoryPulseSpeed = 1f;

		public bool activeWhenUnhandled = true;

		public bool canReorderItems;

		public SelectInventoryDisplay selectInventoryDisplay;

		public RightClickInventory rightClickInventory;

		public bool reverseInventoryCombinations;

		public bool canMoveWhenActive = true;

		public bool selectInvWithUnhandled;

		public int selectInvWithIconID;

		public bool giveInvWithUnhandled;

		public int giveInvWithIconID;

		public bool autoDisableUnhandledHotspots;

		public Transform clickPrefab;

		public ClickMarkerPosition clickMarkerPosition;

		public float walkableClickRange = 0.5f;

		public NavMeshSearchDirection navMeshSearchDirection = NavMeshSearchDirection.RadiallyOutwardsFromCursor;

		public bool ignoreOffScreenNavMesh = true;

		public DoubleClickMovement doubleClickMovement;

		public bool magnitudeAffectsDirect;

		public bool directTurnsInstantly;

		public bool disableMovementWhenInterationMenusAreOpen;

		public DirectMovementType directMovementType;

		public LimitDirectMovement limitDirectMovement;

		public bool directMovementPerspective;

		public float destinationAccuracy = 0.8f;

		public bool experimentalAccuracy;

		public bool unityUIClicksAlwaysBlocks;

		public float pathfindUpdateFrequency;

		public float verticalReductionFactor = 0.7f;

		public bool rotationsAffectedByVerticalReduction = true;

		public float jumpSpeed = 4f;

		public bool singleTapStraight;

		public bool singleTapStraightPathfind;

		public float clickHoldSeparationStraight = 0.3f;

		public bool useFPCamDuringConversations = true;

		public bool onlyInteractWhenCursorUnlocked;

		public float freeAimSmoothSpeed = 50f;

		public bool assumeInputsDefined;

		public List<ActiveInput> activeInputs = new List<ActiveInput>();

		public float freeAimTouchSpeed = 0.01f;

		public float dragWalkThreshold = 5f;

		public float dragRunThreshold = 20f;

		public bool drawDragLine;

		public float dragLineWidth = 3f;

		public Color dragLineColor = Color.white;

		public bool offsetTouchCursor;

		public bool doubleTapHotspots = true;

		public FirstPersonTouchScreen firstPersonTouchScreen = FirstPersonTouchScreen.OneTouchToMoveAndTurn;

		public DirectTouchScreen directTouchScreen;

		public bool touchUpWhenPaused;

		public bool forceAspectRatio;

		public float wantedAspectRatio = 1.5f;

		public bool landscapeModeOnly = true;

		public CameraPerspective cameraPerspective = CameraPerspective.ThreeD;

		public bool cacheCameraMain;

		private int cameraPerspective_int;

		public MovingTurning movingTurning = MovingTurning.Unity2D;

		public HotspotDetection hotspotDetection;

		public bool closeInteractionMenusIfPlayerLeavesVicinity;

		public bool placeDistantHotspotsOnSeparateLayer = true;

		public HotspotsInVicinity hotspotsInVicinity;

		public HotspotIconDisplay hotspotIconDisplay;

		public HotspotIcon hotspotIcon;

		public Texture2D hotspotIconTexture;

		public float hotspotIconSize = 0.04f;

		public bool playerFacesHotspots;

		public bool onlyFaceHotspotOnSelect;

		public bool scaleHighlightWithMouseProximity;

		public float highlightProximityFactor = 4f;

		public bool occludeIcons;

		public bool hideIconUnderInteractionMenu;

		public ScreenWorld hotspotDrawing;

		public bool hideUnhandledHotspots;

		public float navMeshRaycastLength = 100f;

		public float hotspotRaycastLength = 100f;

		public float moveableRaycastLength = 30f;

		public string hotspotLayer = "Default";

		public string distantHotspotLayer = "DistantHotspot";

		public string navMeshLayer = "NavMesh";

		public string backgroundImageLayer = "BackgroundImage";

		public string deactivatedLayer = "Ignore Raycast";

		public bool useLoadingScreen;

		public bool manualSceneActivation;

		public ChooseSceneBy loadingSceneIs;

		public string loadingSceneName = string.Empty;

		public int loadingScene;

		public bool useAsyncLoading;

		public float loadingDelay;

		public bool playMusicWhilePaused;

		public List<MusicStorage> musicStorages = new List<MusicStorage>();

		public List<MusicStorage> ambienceStorages = new List<MusicStorage>();

		public VolumeControl volumeControl;

		public AudioMixerGroup musicMixerGroup;

		public AudioMixerGroup sfxMixerGroup;

		public AudioMixerGroup speechMixerGroup;

		public string musicAttentuationParameter = "musicVolume";

		public string sfxAttentuationParameter = "sfxVolume";

		public string speechAttentuationParameter = "speechVolume";

		public int defaultLanguage;

		public int defaultVoiceLanguage;

		public bool defaultShowSubtitles;

		public float defaultSfxVolume = 0.9f;

		public float defaultMusicVolume = 0.6f;

		public float defaultSpeechVolume = 1f;

		public ShowDebugLogs showDebugLogs;

		public bool printActionCommentsInConsole;

		public DebugWindowDisplays showActiveActionLists;

		public SeeInteractions SeeInteractions
		{
			get
			{
				if (CanUseCursor())
				{
					return seeInteractions;
				}
				return SeeInteractions.ClickOnHotspot;
			}
		}

		public RightClickInventory RightClickInventory
		{
			get
			{
				if (CanSelectItems(false) && !inventoryDragDrop && (interactionMethod == AC_InteractionMethod.ContextSensitive || inventoryInteractions == InventoryInteractions.Single) && (interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot || !cycleInventoryCursors))
				{
					return rightClickInventory;
				}
				return RightClickInventory.DoesNothing;
			}
		}

		public string SavePrefix
		{
			get
			{
				if (string.IsNullOrEmpty(saveFileName))
				{
					string[] array = Application.dataPath.Split('/');
					saveFileName = array[array.Length - 2];
				}
				return saveFileName;
			}
		}

		public float AspectRatio
		{
			get
			{
				return (!forceAspectRatio) ? (-1f) : wantedAspectRatio;
			}
		}

		public bool InventoryDragDrop
		{
			get
			{
				if (CanSelectItems(false))
				{
					return inventoryDragDrop;
				}
				return false;
			}
		}

		public bool AutoDisableUnhandledHotspots
		{
			get
			{
				if (CanSelectItems(false))
				{
					return autoDisableUnhandledHotspots;
				}
				return false;
			}
		}

		private string SmartAddInput(string existingResult, string newInput)
		{
			newInput = "\n" + newInput;
			if (!existingResult.Contains(newInput))
			{
				return existingResult + newInput;
			}
			return existingResult;
		}

		private string GetInputList()
		{
			string existingResult = string.Empty;
			if (inputMethod != InputMethod.TouchScreen)
			{
				existingResult = SmartAddInput(existingResult, "InteractionA (Button)");
				existingResult = SmartAddInput(existingResult, "InteractionB (Button)");
				existingResult = SmartAddInput(existingResult, "CursorHorizontal (Axis)");
				existingResult = SmartAddInput(existingResult, "CursorVertical (Axis)");
			}
			existingResult = SmartAddInput(existingResult, "ToggleCursor (Button)");
			if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson || inputMethod == InputMethod.KeyboardOrController)
			{
				if (inputMethod != InputMethod.TouchScreen)
				{
					existingResult = SmartAddInput(existingResult, "Horizontal (Axis)");
					existingResult = SmartAddInput(existingResult, "Vertical (Axis)");
					if (movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson)
					{
						existingResult = SmartAddInput(existingResult, "Run (Button/Axis)");
						existingResult = SmartAddInput(existingResult, "ToggleRun (Button)");
						existingResult = SmartAddInput(existingResult, "Jump (Button)");
					}
				}
				if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.MouseAndKeyboard)
				{
					existingResult = SmartAddInput(existingResult, "Mouse ScrollWheel (Axis)");
					existingResult = SmartAddInput(existingResult, "CursorHorizontal (Axis)");
					existingResult = SmartAddInput(existingResult, "CursorVertical (Axis)");
				}
				if ((movementMethod == MovementMethod.Direct || movementMethod == MovementMethod.FirstPerson) && hotspotDetection == HotspotDetection.PlayerVicinity && hotspotsInVicinity == HotspotsInVicinity.CycleMultiple)
				{
					existingResult = SmartAddInput(existingResult, "CycleHotspotsLeft (Button)");
					existingResult = SmartAddInput(existingResult, "CycleHotspotsRight (Button)");
					existingResult = SmartAddInput(existingResult, "CycleHotspots (Axis)");
				}
			}
			if (SelectInteractionMethod() == SelectInteractions.CyclingMenuAndClickingHotspot)
			{
				existingResult = SmartAddInput(existingResult, "CycleInteractionsLeft (Button)");
				existingResult = SmartAddInput(existingResult, "CycleInteractionsRight (Button)");
				existingResult = SmartAddInput(existingResult, "CycleInteractions (Axis)");
			}
			if (SelectInteractionMethod() == SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				existingResult = SmartAddInput(existingResult, "CycleCursors (Button)");
				existingResult = SmartAddInput(existingResult, "CycleCursorsBack (Button)");
			}
			else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
			{
				existingResult = SmartAddInput(existingResult, "CycleCursors (Button)");
				if (allowDefaultinteractions || (inventoryInteractions == InventoryInteractions.Multiple && CanSelectItems(false) && allowDefaultInventoryInteractions))
				{
					existingResult = SmartAddInput(existingResult, "DefaultInteraction (Button)");
				}
				if (KickStarter.cursorManager != null && KickStarter.cursorManager.allowIconInput)
				{
					if (KickStarter.cursorManager.allowMainCursor && KickStarter.cursorManager.allowWalkCursor)
					{
						existingResult = SmartAddInput(existingResult, "Icon_Walk (Button)");
					}
					if (KickStarter.cursorManager.cursorIcons != null)
					{
						foreach (CursorIcon cursorIcon in KickStarter.cursorManager.cursorIcons)
						{
							string buttonName = cursorIcon.GetButtonName();
							if (!string.IsNullOrEmpty(buttonName))
							{
								existingResult = SmartAddInput(existingResult, buttonName + " (Button)");
							}
						}
					}
				}
			}
			existingResult = SmartAddInput(existingResult, "FlashHotspots (Button)");
			if (AdvGame.GetReferences().speechManager != null && (AdvGame.GetReferences().speechManager.allowSpeechSkipping || AdvGame.GetReferences().speechManager.displayForever || AdvGame.GetReferences().speechManager.displayNarrationForever))
			{
				existingResult = SmartAddInput(existingResult, "SkipSpeech (Button)");
			}
			existingResult = SmartAddInput(existingResult, "EndCutscene (Button)");
			existingResult = SmartAddInput(existingResult, "EndConversation (Button)");
			existingResult = SmartAddInput(existingResult, "ThrowMoveable (Button)");
			existingResult = SmartAddInput(existingResult, "RotateMoveable (Button)");
			existingResult = SmartAddInput(existingResult, "RotateMoveableToggle (Button)");
			existingResult = SmartAddInput(existingResult, "ZoomMoveable (Axis)");
			if (AdvGame.GetReferences().menuManager != null && AdvGame.GetReferences().menuManager.menus != null)
			{
				foreach (Menu menu in AdvGame.GetReferences().menuManager.menus)
				{
					if (menu.appearType == AppearType.OnInputKey && menu.toggleKey != string.Empty)
					{
						existingResult = SmartAddInput(existingResult, menu.toggleKey + " (Button)");
					}
				}
			}
			if (activeInputs != null)
			{
				foreach (ActiveInput activeInput in activeInputs)
				{
					if (activeInput.inputName != string.Empty)
					{
						existingResult = SmartAddInput(existingResult, activeInput.inputName + " (Button)");
					}
				}
			}
			if (runConversationsWithKeys)
			{
				existingResult = SmartAddInput(existingResult, "DialogueOption[1-9] (Buttons)");
			}
			return existingResult;
		}

		public bool PlayerCanReverse()
		{
			if (movementMethod == MovementMethod.Direct && directMovementType == DirectMovementType.TankControls)
			{
				return true;
			}
			if (movementMethod == MovementMethod.FirstPerson)
			{
				return true;
			}
			return false;
		}

		public bool IsFirstPersonDragRotation()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && firstPersonTouchScreen == FirstPersonTouchScreen.TouchControlsTurningOnly)
			{
				return true;
			}
			return false;
		}

		public bool IsFirstPersonDragComplex()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && firstPersonTouchScreen == FirstPersonTouchScreen.OneTouchToTurnAndTwoTouchesToMove)
			{
				return true;
			}
			return false;
		}

		public bool IsFirstPersonDragMovement()
		{
			if (movementMethod == MovementMethod.FirstPerson && inputMethod == InputMethod.TouchScreen && firstPersonTouchScreen == FirstPersonTouchScreen.OneTouchToMoveAndTurn)
			{
				return true;
			}
			return false;
		}

		private int[] GetPlayerIDArray()
		{
			List<int> list = new List<int>();
			foreach (PlayerPrefab player in players)
			{
				list.Add(player.ID);
			}
			list.Sort();
			return list.ToArray();
		}

		public int GetDefaultPlayerID()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			foreach (PlayerPrefab player in players)
			{
				if (player.isDefault)
				{
					return player.ID;
				}
			}
			return 0;
		}

		public Player GetPlayer(int ID)
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			foreach (PlayerPrefab player in players)
			{
				if (player.ID == ID)
				{
					return player.playerOb;
				}
			}
			return null;
		}

		public PlayerPrefab GetPlayerPrefab(int ID)
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return null;
			}
			foreach (PlayerPrefab player in players)
			{
				if (player.ID == ID)
				{
					return player;
				}
			}
			return null;
		}

		public int GetEmptyPlayerID()
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return 0;
			}
			foreach (PlayerPrefab player in players)
			{
				if (player.playerOb == null)
				{
					return player.ID;
				}
			}
			return 0;
		}

		public Player GetDefaultPlayer(bool showError = true)
		{
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return player;
			}
			foreach (PlayerPrefab player in players)
			{
				if (player.isDefault)
				{
					if (player.playerOb != null)
					{
						return player.playerOb;
					}
					if (showError)
					{
						ACDebug.LogWarning("Default Player has no prefab!");
					}
					return null;
				}
			}
			if (showError)
			{
				ACDebug.LogWarning("Cannot find default player!");
			}
			return null;
		}

		public void SetDefaultPlayer(Player defaultPlayer)
		{
			if (defaultPlayer == null)
			{
				return;
			}
			if (playerSwitching == PlayerSwitching.DoNotAllow)
			{
				player = defaultPlayer;
				return;
			}
			bool flag = false;
			foreach (PlayerPrefab player in players)
			{
				if (player.playerOb == defaultPlayer)
				{
					player.isDefault = true;
					flag = true;
				}
				else
				{
					player.isDefault = false;
				}
			}
			if (!flag)
			{
				PlayerPrefab playerPrefab = new PlayerPrefab(GetPlayerIDArray());
				playerPrefab.playerOb = defaultPlayer;
				players.Add(playerPrefab);
			}
		}

		private void SetDefaultPlayer(PlayerPrefab defaultPlayer)
		{
			foreach (PlayerPrefab player in players)
			{
				if (player == defaultPlayer)
				{
					player.isDefault = true;
				}
				else
				{
					player.isDefault = false;
				}
			}
		}

		public bool CanClickOffInteractionMenu()
		{
			if (cancelInteractions == CancelInteractions.ClickOffMenu || !CanUseCursor())
			{
				return true;
			}
			return false;
		}

		public bool MouseOverForInteractionMenu()
		{
			if (seeInteractions == SeeInteractions.CursorOverHotspot && CanUseCursor())
			{
				return true;
			}
			return false;
		}

		public bool ShouldCloseInteractionMenu()
		{
			if (inputMethod == InputMethod.TouchScreen)
			{
				if (KickStarter.playerInteraction.GetActiveHotspot() == null && KickStarter.runtimeInventory.hoverItem == null)
				{
					return true;
				}
				return closeInteractionMenuIfTapHotspot;
			}
			return true;
		}

		private bool CanUseCursor()
		{
			if (inputMethod != InputMethod.TouchScreen || CanDragCursor())
			{
				return true;
			}
			return false;
		}

		private bool DoPlayerAnimEnginesMatch()
		{
			AnimationEngine animationEngine = AnimationEngine.Legacy;
			bool flag = false;
			foreach (PlayerPrefab player in players)
			{
				if (player.playerOb != null)
				{
					if (!flag)
					{
						flag = true;
						animationEngine = player.playerOb.animationEngine;
					}
					else if (player.playerOb.animationEngine != animationEngine)
					{
						return false;
					}
				}
			}
			return true;
		}

		public SelectInteractions SelectInteractionMethod()
		{
			if (inputMethod != InputMethod.TouchScreen && interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
			{
				return selectInteractions;
			}
			return SelectInteractions.ClickingMenu;
		}

		public bool ShowHoverInteractionInHotspotLabel()
		{
			if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && showHoverInteractionInHotspotLabel)
			{
				return true;
			}
			return false;
		}

		public bool IsInLoadingScene()
		{
			if (useLoadingScreen)
			{
				if (loadingSceneIs == ChooseSceneBy.Name)
				{
					if (UnityVersionHandler.GetCurrentSceneName() != string.Empty && UnityVersionHandler.GetCurrentSceneName() == loadingSceneName)
					{
						return true;
					}
				}
				else if (loadingSceneIs == ChooseSceneBy.Number && UnityVersionHandler.GetCurrentSceneName() != string.Empty && UnityVersionHandler.GetCurrentSceneNumber() == loadingScene)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsInFirstPerson()
		{
			if (movementMethod == MovementMethod.FirstPerson)
			{
				return true;
			}
			if (KickStarter.player != null && KickStarter.player.FirstPersonCamera != null && KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera != null && KickStarter.mainCamera.attachedCamera.transform == KickStarter.player.FirstPersonCamera)
			{
				return true;
			}
			return false;
		}

		public bool CanGiveItems()
		{
			if (interactionMethod != AC_InteractionMethod.ContextSensitive && CanSelectItems(false))
			{
				return true;
			}
			return false;
		}

		public bool CanSelectItems(bool showError)
		{
			if (interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || selectInteractions != SelectInteractions.CyclingCursorAndClickingHotspot)
			{
				return true;
			}
			if (!cycleInventoryCursors)
			{
				return true;
			}
			if (showError)
			{
				ACDebug.LogWarning("Inventory items cannot be selected with this combination of settings - they are included in Interaction cycles instead.");
			}
			return false;
		}

		public bool CanDragCursor()
		{
			if (offsetTouchCursor && inputMethod == InputMethod.TouchScreen && movementMethod != MovementMethod.FirstPerson)
			{
				return true;
			}
			return false;
		}

		public bool ReleaseClickInteractions()
		{
			if (inputMethod == InputMethod.TouchScreen)
			{
				return clickUpInteractions;
			}
			if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && SelectInteractionMethod() == SelectInteractions.ClickingMenu && clickUpInteractions)
			{
				return true;
			}
			return false;
		}

		public float GetDestinationThreshold(float offset = 0.1f)
		{
			return 1f + offset - destinationAccuracy;
		}
	}
}
