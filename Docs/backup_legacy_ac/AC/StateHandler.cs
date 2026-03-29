using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_state_handler.html")]
	public class StateHandler : MonoBehaviour
	{
		protected GameState _gameState;

		protected Music music;

		protected Ambience ambience;

		protected bool inScriptedCutscene;

		protected GameState previousUpdateState;

		protected GameState lastNonPausedState;

		protected bool isACDisabled;

		protected bool cursorIsOff;

		protected bool inputIsOff;

		protected bool interactionIsOff;

		protected bool draggablesIsOff;

		protected bool menuIsOff;

		protected bool movementIsOff;

		protected bool cameraIsOff;

		protected bool triggerIsOff;

		protected bool playerIsOff;

		protected bool runAtLeastOnce;

		protected bool hasGameEngine;

		protected List<ArrowPrompt> arrowPrompts = new List<ArrowPrompt>();

		protected List<DragBase> dragBases = new List<DragBase>();

		protected List<Parallax2D> parallax2Ds = new List<Parallax2D>();

		protected List<Hotspot> hotspots = new List<Hotspot>();

		protected List<Highlight> highlights = new List<Highlight>();

		protected List<AC_Trigger> triggers = new List<AC_Trigger>();

		protected List<_Camera> cameras = new List<_Camera>();

		protected List<Sound> sounds = new List<Sound>();

		protected List<LimitVisibility> limitVisibilitys = new List<LimitVisibility>();

		protected List<Char> characters = new List<Char>();

		protected List<FollowSortingMap> followSortingMaps = new List<FollowSortingMap>();

		protected List<NavMeshBase> navMeshBases = new List<NavMeshBase>();

		protected List<SortingMap> sortingMaps = new List<SortingMap>();

		protected List<BackgroundCamera> backgroundCameras = new List<BackgroundCamera>();

		protected List<BackgroundImage> backgroundImages = new List<BackgroundImage>();

		protected List<ConstantID> constantIDs = new List<ConstantID>();

		protected int _i;

		public GameState gameState
		{
			get
			{
				return _gameState;
			}
			set
			{
				if ((bool)KickStarter.mainCamera)
				{
					KickStarter.mainCamera.CancelPauseGame();
				}
				_gameState = value;
			}
		}

		public bool UnpausedLastFrame
		{
			get
			{
				if (gameState != GameState.Paused && previousUpdateState == GameState.Paused)
				{
					return true;
				}
				return false;
			}
		}

		public List<Char> Characters
		{
			get
			{
				return characters;
			}
		}

		public List<Hotspot> Hotspots
		{
			get
			{
				return hotspots;
			}
		}

		public List<FollowSortingMap> FollowSortingMaps
		{
			get
			{
				return followSortingMaps;
			}
		}

		public List<SortingMap> SortingMaps
		{
			get
			{
				return sortingMaps;
			}
		}

		public List<BackgroundCamera> BackgroundCameras
		{
			get
			{
				return backgroundCameras;
			}
		}

		public List<BackgroundImage> BackgroundImages
		{
			get
			{
				return backgroundImages;
			}
		}

		public List<ConstantID> ConstantIDs
		{
			get
			{
				return constantIDs;
			}
		}

		public bool MovementIsOff
		{
			get
			{
				return movementIsOff;
			}
		}

		public void OnAwake()
		{
			Time.timeScale = 1f;
			Object.DontDestroyOnLoad(this);
			inScriptedCutscene = false;
			InitPersistentEngine();
		}

		protected void Start()
		{
			if (KickStarter.settingsManager == null)
			{
				hasGameEngine = false;
			}
		}

		protected void Update()
		{
			if (isACDisabled || !hasGameEngine)
			{
				return;
			}
			if (KickStarter.settingsManager.IsInLoadingScene() || KickStarter.sceneChanger.IsLoading())
			{
				if (!menuIsOff)
				{
					KickStarter.playerMenus.UpdateLoadingMenus();
				}
				return;
			}
			if (gameState != GameState.Paused)
			{
				lastNonPausedState = gameState;
			}
			if (!inputIsOff)
			{
				if (gameState == GameState.DialogOptions)
				{
					KickStarter.playerInput.DetectConversationInputs();
				}
				KickStarter.playerInput.UpdateInput();
				if (IsInGameplay())
				{
					KickStarter.playerInput.UpdateDirectInput();
				}
				if (gameState != GameState.Paused)
				{
					KickStarter.playerQTE.UpdateQTE();
				}
			}
			KickStarter.dialog._Update();
			KickStarter.playerInteraction.UpdateInteractionLabel();
			if (!cursorIsOff)
			{
				KickStarter.playerCursor.UpdateCursor();
				bool flag = KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && KickStarter.settingsManager.hideUnhandledHotspots;
				bool flag2 = KickStarter.settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never;
				bool flag3 = KickStarter.settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity && KickStarter.settingsManager.placeDistantHotspotsOnSeparateLayer && KickStarter.player != null;
				for (_i = 0; _i < hotspots.Count; _i++)
				{
					if (!flag || hotspots[_i].UpdateUnhandledVisibility())
					{
						if (flag2 && KickStarter.settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never)
						{
							hotspots[_i].UpdateIcon();
							if (KickStarter.settingsManager.hotspotDrawing == ScreenWorld.WorldSpace)
							{
								hotspots[_i].DrawHotspotIcon(true);
							}
						}
						if (flag3)
						{
							hotspots[_i].UpdateProximity(KickStarter.player.hotspotDetector);
						}
					}
				}
			}
			if (!menuIsOff)
			{
				KickStarter.playerMenus.CheckForInput();
			}
			if (!menuIsOff && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen && KickStarter.playerInput.GetMouseState() != MouseState.Normal)
			{
				KickStarter.playerMenus.UpdateAllMenus();
			}
			if (!interactionIsOff)
			{
				KickStarter.playerInteraction.UpdateInteraction();
				for (_i = 0; _i < highlights.Count; _i++)
				{
					highlights[_i]._Update();
				}
				if (KickStarter.settingsManager.hotspotDetection == HotspotDetection.MouseOver && KickStarter.settingsManager.scaleHighlightWithMouseProximity)
				{
					bool proximity = IsInGameplay();
					for (_i = 0; _i < hotspots.Count; _i++)
					{
						hotspots[_i].SetProximity(proximity);
					}
				}
			}
			if (!triggerIsOff)
			{
				for (_i = 0; _i < triggers.Count; _i++)
				{
					triggers[_i]._Update();
				}
			}
			if (!menuIsOff)
			{
				KickStarter.playerMenus.UpdateAllMenus();
			}
			KickStarter.actionListManager.UpdateActionListManager();
			for (_i = 0; _i < dragBases.Count; _i++)
			{
				dragBases[_i].UpdateMovement();
			}
			if (!movementIsOff)
			{
				if (IsInGameplay() && (bool)KickStarter.settingsManager && KickStarter.settingsManager.movementMethod != MovementMethod.None)
				{
					KickStarter.playerMovement.UpdatePlayerMovement();
				}
				KickStarter.playerMovement.UpdateFPCamera();
			}
			if (!interactionIsOff)
			{
				KickStarter.playerInteraction.UpdateInventory();
			}
			for (_i = 0; _i < limitVisibilitys.Count; _i++)
			{
				limitVisibilitys[_i]._Update();
			}
			for (_i = 0; _i < sounds.Count; _i++)
			{
				sounds[_i]._Update();
			}
			for (_i = 0; _i < characters.Count; _i++)
			{
				if (characters[_i] != null && (!playerIsOff || !characters[_i].IsPlayer))
				{
					characters[_i]._Update();
				}
			}
			if (!cameraIsOff)
			{
				for (_i = 0; _i < cameras.Count; _i++)
				{
					cameras[_i]._Update();
				}
			}
			if (HasGameStateChanged())
			{
				KickStarter.eventManager.Call_OnChangeGameState(previousUpdateState);
				if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson && (IsInGameplay() || (gameState == GameState.DialogOptions && KickStarter.settingsManager.useFPCamDuringConversations)))
				{
					KickStarter.mainCamera.SetFirstPerson();
				}
				if (Time.time > 0f && gameState != GameState.Paused)
				{
					AudioListener.pause = false;
				}
				if (gameState == GameState.Cutscene && previousUpdateState != GameState.Cutscene)
				{
					KickStarter.playerMenus.MakeUINonInteractive();
				}
				else if (gameState != GameState.Cutscene && previousUpdateState == GameState.Cutscene)
				{
					KickStarter.playerMenus.MakeUIInteractive();
				}
			}
			previousUpdateState = gameState;
		}

		protected void LateUpdate()
		{
			if (isACDisabled || !hasGameEngine || (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene()))
			{
				return;
			}
			for (_i = 0; _i < characters.Count; _i++)
			{
				if (!playerIsOff || !characters[_i].IsPlayer)
				{
					characters[_i]._LateUpdate();
				}
			}
			if (!cameraIsOff)
			{
				KickStarter.mainCamera._LateUpdate();
			}
			for (_i = 0; _i < parallax2Ds.Count; _i++)
			{
				parallax2Ds[_i].UpdateOffset();
			}
			for (_i = 0; _i < sortingMaps.Count; _i++)
			{
				sortingMaps[_i].UpdateSimilarFollowers();
			}
			KickStarter.dialog._LateUpdate();
		}

		protected void FixedUpdate()
		{
			if (isACDisabled || !hasGameEngine || (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene()))
			{
				return;
			}
			for (_i = 0; _i < characters.Count; _i++)
			{
				if (!playerIsOff || !characters[_i].IsPlayer)
				{
					characters[_i]._FixedUpdate();
				}
			}
			for (_i = 0; _i < dragBases.Count; _i++)
			{
				dragBases[_i]._FixedUpdate();
			}
			KickStarter.playerInput._FixedUpdate();
		}

		protected void OnGUI()
		{
			if (!isACDisabled)
			{
				_OnGUI();
			}
		}

		public void _OnGUI()
		{
			if (!hasGameEngine)
			{
				return;
			}
			StatusBox.DrawDebugWindow();
			if (KickStarter.settingsManager.IsInLoadingScene() || KickStarter.sceneChanger.IsLoading())
			{
				if (!cameraIsOff && !KickStarter.settingsManager.IsInLoadingScene())
				{
					KickStarter.mainCamera.DrawCameraFade();
				}
				if (!menuIsOff)
				{
					KickStarter.playerMenus.DrawLoadingMenus();
				}
				if (!cameraIsOff)
				{
					KickStarter.mainCamera.DrawBorders();
				}
				return;
			}
			if (!cursorIsOff && !KickStarter.saveSystem.IsTakingSaveScreenshot)
			{
				if (KickStarter.settingsManager.hotspotIconDisplay != HotspotIconDisplay.Never && KickStarter.settingsManager.hotspotDrawing == ScreenWorld.ScreenSpace)
				{
					for (_i = 0; _i < hotspots.Count; _i++)
					{
						hotspots[_i].DrawHotspotIcon();
					}
				}
				if (IsInGameplay())
				{
					for (_i = 0; _i < dragBases.Count; _i++)
					{
						dragBases[_i].DrawGrabIcon();
					}
				}
			}
			if (!inputIsOff)
			{
				if (gameState == GameState.DialogOptions)
				{
					KickStarter.playerInput.DetectConversationNumerics();
				}
				KickStarter.playerInput.DrawDragLine();
				for (_i = 0; _i < arrowPrompts.Count; _i++)
				{
					arrowPrompts[_i].DrawArrows();
				}
			}
			if (!menuIsOff)
			{
				KickStarter.playerMenus.DrawMenus();
			}
			if (!cursorIsOff && KickStarter.cursorManager.cursorRendering == CursorRendering.Software)
			{
				KickStarter.playerCursor.DrawCursor();
			}
			if (!cameraIsOff)
			{
				KickStarter.mainCamera.DrawCameraFade();
				KickStarter.mainCamera.DrawBorders();
			}
		}

		public void RegisterWithGameEngine()
		{
			if (!hasGameEngine)
			{
				hasGameEngine = true;
			}
		}

		public void UnregisterWithGameEngine()
		{
			hasGameEngine = false;
		}

		public void AfterLoad()
		{
			inScriptedCutscene = false;
		}

		public bool PlayGlobalOnStart()
		{
			if (runAtLeastOnce)
			{
				return false;
			}
			runAtLeastOnce = true;
			ActiveInput.Upgrade();
			if (KickStarter.settingsManager.activeInputs != null)
			{
				foreach (ActiveInput activeInput in KickStarter.settingsManager.activeInputs)
				{
					activeInput.SetDefaultState();
				}
			}
			if (gameState != GameState.Paused)
			{
				AudioListener.pause = false;
			}
			if ((bool)KickStarter.settingsManager.actionListOnStart)
			{
				AdvGame.RunActionListAsset(KickStarter.settingsManager.actionListOnStart);
				return true;
			}
			return false;
		}

		public void CanGlobalOnStart()
		{
			runAtLeastOnce = false;
		}

		public void IgnoreNavMeshCollisions()
		{
			Collider[] allColliders = Object.FindObjectsOfType(typeof(Collider)) as Collider[];
			for (_i = 0; _i < navMeshBases.Count; _i++)
			{
				navMeshBases[_i].IgnoreNavMeshCollisions(allColliders);
			}
		}

		public void UpdateAllMaxVolumes()
		{
			foreach (Sound sound in sounds)
			{
				sound.SetMaxVolume();
			}
		}

		public GameState GetLastNonPausedState()
		{
			return lastNonPausedState;
		}

		public void RestoreLastNonPausedState()
		{
			if (Time.timeScale <= 0f)
			{
				KickStarter.sceneSettings.UnpauseGame(KickStarter.playerInput.timeScale);
			}
			if (KickStarter.playerInteraction.InPreInteractionCutscene)
			{
				gameState = GameState.Cutscene;
			}
			else if (KickStarter.actionListManager.IsGameplayBlocked() || inScriptedCutscene)
			{
				gameState = GameState.Cutscene;
			}
			else if (KickStarter.playerInput.IsInConversation(true))
			{
				gameState = GameState.DialogOptions;
			}
			else
			{
				gameState = GameState.Normal;
			}
		}

		public void LimitHotspotsToCamera(_Camera _camera)
		{
			if (_camera != null)
			{
				for (_i = 0; _i < hotspots.Count; _i++)
				{
					hotspots[_i].LimitToCamera(_camera);
				}
			}
		}

		public void StartCutscene()
		{
			inScriptedCutscene = true;
			gameState = GameState.Cutscene;
		}

		public void EndCutscene()
		{
			inScriptedCutscene = false;
			if (KickStarter.playerMenus.ArePauseMenusOn())
			{
				KickStarter.mainCamera.PauseGame();
			}
			else
			{
				RestoreLastNonPausedState();
			}
		}

		public bool IsInScriptedCutscene()
		{
			return inScriptedCutscene;
		}

		public bool IsInCutscene()
		{
			return !isACDisabled && gameState == GameState.Cutscene;
		}

		public bool IsPaused()
		{
			return !isACDisabled && gameState == GameState.Paused;
		}

		public bool IsInGameplay()
		{
			if (isACDisabled)
			{
				return false;
			}
			if (gameState == GameState.Normal)
			{
				return true;
			}
			if (gameState == GameState.DialogOptions && KickStarter.settingsManager.allowGameplayDuringConversations)
			{
				return true;
			}
			return false;
		}

		public void SetACState(bool state)
		{
			isACDisabled = !state;
		}

		public bool IsACEnabled()
		{
			return !isACDisabled;
		}

		public void SetCursorSystem(bool state)
		{
			cursorIsOff = !state;
		}

		public void SetInputSystem(bool state)
		{
			inputIsOff = !state;
		}

		public void SetInteractionSystem(bool state)
		{
			interactionIsOff = !state;
			if (!state)
			{
				KickStarter.playerInteraction.DeselectHotspot(true);
			}
		}

		public void SetDraggableSystem(bool state)
		{
			draggablesIsOff = !state;
			if (!state)
			{
				KickStarter.playerInput.LetGo();
			}
		}

		public bool CanInteract()
		{
			return !interactionIsOff;
		}

		public bool CanInteractWithDraggables()
		{
			return !draggablesIsOff;
		}

		public void SetMenuSystem(bool state)
		{
			menuIsOff = !state;
		}

		public void SetMovementSystem(bool state)
		{
			movementIsOff = !state;
		}

		public void SetCameraSystem(bool state)
		{
			cameraIsOff = !state;
		}

		public void SetTriggerSystem(bool state)
		{
			triggerIsOff = !state;
		}

		public void SetPlayerSystem(bool state)
		{
			playerIsOff = !state;
		}

		public bool AreTriggersDisabled()
		{
			return triggerIsOff;
		}

		public MainData SaveMainData(MainData mainData)
		{
			mainData.cursorIsOff = cursorIsOff;
			mainData.inputIsOff = inputIsOff;
			mainData.interactionIsOff = interactionIsOff;
			mainData.menuIsOff = menuIsOff;
			mainData.movementIsOff = movementIsOff;
			mainData.cameraIsOff = cameraIsOff;
			mainData.triggerIsOff = triggerIsOff;
			mainData.playerIsOff = playerIsOff;
			if (music != null)
			{
				mainData = music.SaveMainData(mainData);
			}
			if (ambience != null)
			{
				mainData = ambience.SaveMainData(mainData);
			}
			mainData = KickStarter.runtimeObjectives.SaveGlobalObjectives(mainData);
			return mainData;
		}

		public void LoadMainData(MainData mainData)
		{
			cursorIsOff = mainData.cursorIsOff;
			inputIsOff = mainData.inputIsOff;
			interactionIsOff = mainData.interactionIsOff;
			menuIsOff = mainData.menuIsOff;
			movementIsOff = mainData.movementIsOff;
			cameraIsOff = mainData.cameraIsOff;
			triggerIsOff = mainData.triggerIsOff;
			playerIsOff = mainData.playerIsOff;
			if (music == null)
			{
				CreateMusicEngine();
			}
			music.LoadMainData(mainData);
			if (ambience == null)
			{
				CreateAmbienceEngine();
			}
			ambience.LoadMainData(mainData);
			KickStarter.runtimeObjectives.AssignGlobalObjectives(mainData);
		}

		public Music GetMusicEngine()
		{
			if (music == null)
			{
				CreateMusicEngine();
			}
			return music;
		}

		public Ambience GetAmbienceEngine()
		{
			if (ambience == null)
			{
				CreateAmbienceEngine();
			}
			return ambience;
		}

		protected void InitPersistentEngine()
		{
			KickStarter.runtimeLanguages.OnAwake();
			KickStarter.options.OnStart();
			KickStarter.localVariables.OnStart();
			KickStarter.sceneChanger.OnAwake();
			KickStarter.levelStorage.OnAwake();
			KickStarter.runtimeVariables.OnStart();
			KickStarter.runtimeInventory.OnStart();
			KickStarter.runtimeDocuments.OnStart();
			KickStarter.playerMenus.OnStart();
		}

		protected bool HasGameStateChanged()
		{
			if (previousUpdateState != gameState)
			{
				return true;
			}
			return false;
		}

		protected void CreateMusicEngine()
		{
			if (music == null)
			{
				music = CreateSoundtrackEngine<Music>("MusicEngine");
			}
		}

		protected void CreateAmbienceEngine()
		{
			if (ambience == null)
			{
				ambience = CreateSoundtrackEngine<Ambience>("AmbienceEngine");
			}
		}

		protected T CreateSoundtrackEngine<T>(string resourceName) where T : Soundtrack
		{
			GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load(resourceName));
			if (gameObject != null)
			{
				gameObject.name = AdvGame.GetName(resourceName);
				return gameObject.GetComponent<T>();
			}
			ACDebug.LogError("Cannot find " + resourceName + " prefab in /AdventureCreator/Resources - did you import AC completely?");
			return (T)null;
		}

		public void Register(ArrowPrompt _object)
		{
			if (!arrowPrompts.Contains(_object))
			{
				arrowPrompts.Add(_object);
			}
		}

		public void Unregister(ArrowPrompt _object)
		{
			if (arrowPrompts.Contains(_object))
			{
				arrowPrompts.Remove(_object);
			}
		}

		public void Register(DragBase _object)
		{
			if (!dragBases.Contains(_object))
			{
				dragBases.Add(_object);
			}
		}

		public void Unregister(DragBase _object)
		{
			if (dragBases.Contains(_object))
			{
				dragBases.Remove(_object);
			}
		}

		public void Register(Parallax2D _object)
		{
			if (!parallax2Ds.Contains(_object))
			{
				parallax2Ds.Add(_object);
			}
		}

		public void Unregister(Parallax2D _object)
		{
			if (parallax2Ds.Contains(_object))
			{
				parallax2Ds.Remove(_object);
			}
		}

		public void Register(Hotspot _object)
		{
			if (!hotspots.Contains(_object))
			{
				hotspots.Add(_object);
				if (KickStarter.eventManager != null)
				{
					KickStarter.eventManager.Call_OnRegisterHotspot(_object, true);
				}
			}
		}

		public void Unregister(Hotspot _object)
		{
			if (hotspots.Contains(_object))
			{
				hotspots.Remove(_object);
				if (KickStarter.eventManager != null)
				{
					KickStarter.eventManager.Call_OnRegisterHotspot(_object, false);
				}
			}
		}

		public void Register(Highlight _object)
		{
			if (!highlights.Contains(_object))
			{
				highlights.Add(_object);
			}
		}

		public void Unregister(Highlight _object)
		{
			if (highlights.Contains(_object))
			{
				highlights.Remove(_object);
			}
		}

		public void Register(AC_Trigger _object)
		{
			if (!triggers.Contains(_object))
			{
				triggers.Add(_object);
			}
		}

		public void Unregister(AC_Trigger _object)
		{
			if (triggers.Contains(_object))
			{
				triggers.Remove(_object);
			}
		}

		public void Register(_Camera _object)
		{
			if (!cameras.Contains(_object))
			{
				cameras.Add(_object);
			}
		}

		public void Unregister(_Camera _object)
		{
			if (cameras.Contains(_object))
			{
				cameras.Remove(_object);
			}
		}

		public void Register(Sound _object)
		{
			if (!sounds.Contains(_object))
			{
				sounds.Add(_object);
			}
		}

		public void Unregister(Sound _object)
		{
			if (sounds.Contains(_object))
			{
				sounds.Remove(_object);
			}
		}

		public void Register(LimitVisibility _object)
		{
			if (!limitVisibilitys.Contains(_object))
			{
				limitVisibilitys.Add(_object);
			}
		}

		public void Unregister(LimitVisibility _object)
		{
			if (limitVisibilitys.Contains(_object))
			{
				limitVisibilitys.Remove(_object);
			}
		}

		public void Register(Char _object)
		{
			if (!characters.Contains(_object))
			{
				characters.Add(_object);
			}
		}

		public void Unregister(Char _object)
		{
			if (characters.Contains(_object))
			{
				characters.Remove(_object);
			}
		}

		public void Register(FollowSortingMap _object)
		{
			if (!followSortingMaps.Contains(_object))
			{
				followSortingMaps.Add(_object);
				_object.UpdateSortingMap();
			}
		}

		public void Unregister(FollowSortingMap _object)
		{
			if (followSortingMaps.Contains(_object))
			{
				followSortingMaps.Remove(_object);
			}
		}

		public void Register(NavMeshBase _object)
		{
			if (!navMeshBases.Contains(_object))
			{
				navMeshBases.Add(_object);
				_object.IgnoreNavMeshCollisions();
			}
		}

		public void Unregister(NavMeshBase _object)
		{
			if (navMeshBases.Contains(_object))
			{
				navMeshBases.Remove(_object);
			}
		}

		public void Register(SortingMap _object)
		{
			if (!sortingMaps.Contains(_object))
			{
				sortingMaps.Add(_object);
			}
		}

		public void Unregister(SortingMap _object)
		{
			if (sortingMaps.Contains(_object))
			{
				sortingMaps.Remove(_object);
			}
		}

		public void Register(BackgroundCamera _object)
		{
			if (!backgroundCameras.Contains(_object))
			{
				backgroundCameras.Add(_object);
				_object.UpdateRect();
			}
		}

		public void Unregister(BackgroundCamera _object)
		{
			if (backgroundCameras.Contains(_object))
			{
				backgroundCameras.Remove(_object);
			}
		}

		public void Register(BackgroundImage _object)
		{
			if (!backgroundImages.Contains(_object))
			{
				backgroundImages.Add(_object);
			}
		}

		public void Unregister(BackgroundImage _object)
		{
			if (backgroundImages.Contains(_object))
			{
				backgroundImages.Remove(_object);
			}
		}

		public void Register(ConstantID _object)
		{
			if (!constantIDs.Contains(_object))
			{
				constantIDs.Add(_object);
			}
		}

		public void Unregister(ConstantID _object)
		{
			if (constantIDs.Contains(_object))
			{
				constantIDs.Remove(_object);
			}
		}
	}
}
