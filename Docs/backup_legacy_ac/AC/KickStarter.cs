using System;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_kick_starter.html")]
	public class KickStarter : MonoBehaviour
	{
		private static Player playerPrefab;

		private static MainCamera mainCameraPrefab;

		private static Camera cameraMain;

		private static GameObject persistentEnginePrefab;

		private static GameObject gameEnginePrefab;

		private static SceneManager sceneManagerPrefab;

		private static SettingsManager settingsManagerPrefab;

		private static ActionsManager actionsManagerPrefab;

		private static VariablesManager variablesManagerPrefab;

		private static InventoryManager inventoryManagerPrefab;

		private static SpeechManager speechManagerPrefab;

		private static CursorManager cursorManagerPrefab;

		private static MenuManager menuManagerPrefab;

		private static Options optionsComponent;

		private static RuntimeInventory runtimeInventoryComponent;

		private static RuntimeVariables runtimeVariablesComponent;

		private static PlayerMenus playerMenusComponent;

		private static StateHandler stateHandlerComponent;

		private static SceneChanger sceneChangerComponent;

		private static SaveSystem saveSystemComponent;

		private static LevelStorage levelStorageComponent;

		private static RuntimeLanguages runtimeLanguagesComponent;

		private static RuntimeDocuments runtimeDocumentsComponent;

		private static RuntimeObjectives runtimeObjectivesComponent;

		private static ActionListAssetManager actionListAssetManagerComponent;

		private static MenuSystem menuSystemComponent;

		private static Dialog dialogComponent;

		private static PlayerInput playerInputComponent;

		private static PlayerInteraction playerInteractionComponent;

		private static PlayerMovement playerMovementComponent;

		private static PlayerCursor playerCursorComponent;

		private static PlayerQTE playerQTEComponent;

		private static SceneSettings sceneSettingsComponent;

		private static NavigationManager navigationManagerComponent;

		private static ActionListManager actionListManagerComponent;

		private static LocalVariables localVariablesComponent;

		private static MenuPreview menuPreviewComponent;

		private static EventManager eventManagerComponent;

		private static KickStarter kickStarterComponent;

		public static SceneManager sceneManager
		{
			get
			{
				if (sceneManagerPrefab != null)
				{
					return sceneManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().sceneManager)
				{
					sceneManagerPrefab = AdvGame.GetReferences().sceneManager;
					return sceneManagerPrefab;
				}
				return null;
			}
			set
			{
				sceneManagerPrefab = value;
			}
		}

		public static SettingsManager settingsManager
		{
			get
			{
				if (settingsManagerPrefab != null)
				{
					return settingsManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().settingsManager)
				{
					settingsManagerPrefab = AdvGame.GetReferences().settingsManager;
					return settingsManagerPrefab;
				}
				return null;
			}
			set
			{
				settingsManagerPrefab = value;
			}
		}

		public static ActionsManager actionsManager
		{
			get
			{
				if (actionsManagerPrefab != null)
				{
					return actionsManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().actionsManager)
				{
					actionsManagerPrefab = AdvGame.GetReferences().actionsManager;
					return actionsManagerPrefab;
				}
				return null;
			}
			set
			{
				actionsManagerPrefab = value;
			}
		}

		public static VariablesManager variablesManager
		{
			get
			{
				if (variablesManagerPrefab != null)
				{
					return variablesManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().variablesManager)
				{
					variablesManagerPrefab = AdvGame.GetReferences().variablesManager;
					return variablesManagerPrefab;
				}
				return null;
			}
			set
			{
				variablesManagerPrefab = value;
			}
		}

		public static InventoryManager inventoryManager
		{
			get
			{
				if (inventoryManagerPrefab != null)
				{
					return inventoryManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().inventoryManager)
				{
					inventoryManagerPrefab = AdvGame.GetReferences().inventoryManager;
					return inventoryManagerPrefab;
				}
				return null;
			}
			set
			{
				inventoryManagerPrefab = value;
			}
		}

		public static SpeechManager speechManager
		{
			get
			{
				if (speechManagerPrefab != null)
				{
					return speechManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().speechManager)
				{
					speechManagerPrefab = AdvGame.GetReferences().speechManager;
					return speechManagerPrefab;
				}
				return null;
			}
			set
			{
				speechManagerPrefab = value;
			}
		}

		public static CursorManager cursorManager
		{
			get
			{
				if (cursorManagerPrefab != null)
				{
					return cursorManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().cursorManager)
				{
					cursorManagerPrefab = AdvGame.GetReferences().cursorManager;
					return cursorManagerPrefab;
				}
				return null;
			}
			set
			{
				cursorManagerPrefab = value;
			}
		}

		public static MenuManager menuManager
		{
			get
			{
				if (menuManagerPrefab != null)
				{
					return menuManagerPrefab;
				}
				if ((bool)AdvGame.GetReferences() && (bool)AdvGame.GetReferences().menuManager)
				{
					menuManagerPrefab = AdvGame.GetReferences().menuManager;
					return menuManagerPrefab;
				}
				return null;
			}
			set
			{
				menuManagerPrefab = value;
			}
		}

		public static Options options
		{
			get
			{
				if (optionsComponent != null)
				{
					return optionsComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					optionsComponent = persistentEnginePrefab.GetComponent<Options>();
					return optionsComponent;
				}
				return null;
			}
		}

		public static RuntimeInventory runtimeInventory
		{
			get
			{
				if (runtimeInventoryComponent != null)
				{
					return runtimeInventoryComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					runtimeInventoryComponent = persistentEnginePrefab.GetComponent<RuntimeInventory>();
					return runtimeInventoryComponent;
				}
				return null;
			}
		}

		public static RuntimeVariables runtimeVariables
		{
			get
			{
				if (runtimeVariablesComponent != null)
				{
					return runtimeVariablesComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					runtimeVariablesComponent = persistentEnginePrefab.GetComponent<RuntimeVariables>();
					return runtimeVariablesComponent;
				}
				return null;
			}
		}

		public static PlayerMenus playerMenus
		{
			get
			{
				if (playerMenusComponent != null)
				{
					return playerMenusComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					playerMenusComponent = persistentEnginePrefab.GetComponent<PlayerMenus>();
					return playerMenusComponent;
				}
				return null;
			}
		}

		public static StateHandler stateHandler
		{
			get
			{
				if (stateHandlerComponent != null)
				{
					return stateHandlerComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					stateHandlerComponent = persistentEnginePrefab.GetComponent<StateHandler>();
					return stateHandlerComponent;
				}
				return null;
			}
		}

		public static SceneChanger sceneChanger
		{
			get
			{
				if (sceneChangerComponent != null)
				{
					return sceneChangerComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					sceneChangerComponent = persistentEnginePrefab.GetComponent<SceneChanger>();
					return sceneChangerComponent;
				}
				return null;
			}
		}

		public static SaveSystem saveSystem
		{
			get
			{
				if (saveSystemComponent != null)
				{
					return saveSystemComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					saveSystemComponent = persistentEnginePrefab.GetComponent<SaveSystem>();
					return saveSystemComponent;
				}
				return null;
			}
		}

		public static LevelStorage levelStorage
		{
			get
			{
				if (levelStorageComponent != null)
				{
					return levelStorageComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					levelStorageComponent = persistentEnginePrefab.GetComponent<LevelStorage>();
					return levelStorageComponent;
				}
				return null;
			}
		}

		public static RuntimeLanguages runtimeLanguages
		{
			get
			{
				if (runtimeLanguagesComponent != null)
				{
					return runtimeLanguagesComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					runtimeLanguagesComponent = persistentEnginePrefab.GetComponent<RuntimeLanguages>();
					return runtimeLanguagesComponent;
				}
				return null;
			}
		}

		public static RuntimeDocuments runtimeDocuments
		{
			get
			{
				if (runtimeDocumentsComponent != null)
				{
					return runtimeDocumentsComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					runtimeDocumentsComponent = persistentEnginePrefab.GetComponent<RuntimeDocuments>();
					return runtimeDocumentsComponent;
				}
				return null;
			}
		}

		public static RuntimeObjectives runtimeObjectives
		{
			get
			{
				if (runtimeObjectivesComponent != null)
				{
					return runtimeObjectivesComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					runtimeObjectivesComponent = persistentEnginePrefab.GetComponent<RuntimeObjectives>();
					return runtimeObjectivesComponent;
				}
				return null;
			}
		}

		public static ActionListAssetManager actionListAssetManager
		{
			get
			{
				if (actionListAssetManagerComponent != null)
				{
					return actionListAssetManagerComponent;
				}
				if ((bool)persistentEnginePrefab)
				{
					actionListAssetManagerComponent = persistentEnginePrefab.GetComponent<ActionListAssetManager>();
					return actionListAssetManagerComponent;
				}
				return null;
			}
		}

		public static MenuSystem menuSystem
		{
			get
			{
				if (menuSystemComponent != null)
				{
					return menuSystemComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					menuSystemComponent = gameEnginePrefab.GetComponent<MenuSystem>();
					return menuSystemComponent;
				}
				return null;
			}
		}

		public static Dialog dialog
		{
			get
			{
				if (dialogComponent != null)
				{
					return dialogComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					dialogComponent = gameEnginePrefab.GetComponent<Dialog>();
					return dialogComponent;
				}
				return null;
			}
		}

		public static PlayerInput playerInput
		{
			get
			{
				if (playerInputComponent != null)
				{
					return playerInputComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					playerInputComponent = gameEnginePrefab.GetComponent<PlayerInput>();
					return playerInputComponent;
				}
				return null;
			}
		}

		public static PlayerInteraction playerInteraction
		{
			get
			{
				if (playerInteractionComponent != null)
				{
					return playerInteractionComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					playerInteractionComponent = gameEnginePrefab.GetComponent<PlayerInteraction>();
					return playerInteractionComponent;
				}
				return null;
			}
		}

		public static PlayerMovement playerMovement
		{
			get
			{
				if (playerMovementComponent != null)
				{
					return playerMovementComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					playerMovementComponent = gameEnginePrefab.GetComponent<PlayerMovement>();
					return playerMovementComponent;
				}
				return null;
			}
		}

		public static PlayerCursor playerCursor
		{
			get
			{
				if (playerCursorComponent != null)
				{
					return playerCursorComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					playerCursorComponent = gameEnginePrefab.GetComponent<PlayerCursor>();
					return playerCursorComponent;
				}
				return null;
			}
		}

		public static PlayerQTE playerQTE
		{
			get
			{
				if (playerQTEComponent != null)
				{
					return playerQTEComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					playerQTEComponent = gameEnginePrefab.GetComponent<PlayerQTE>();
					return playerQTEComponent;
				}
				return null;
			}
		}

		public static SceneSettings sceneSettings
		{
			get
			{
				if (sceneSettingsComponent != null && Application.isPlaying)
				{
					return sceneSettingsComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					sceneSettingsComponent = gameEnginePrefab.GetComponent<SceneSettings>();
					return sceneSettingsComponent;
				}
				return null;
			}
		}

		public static NavigationManager navigationManager
		{
			get
			{
				if (navigationManagerComponent != null)
				{
					return navigationManagerComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					navigationManagerComponent = gameEnginePrefab.GetComponent<NavigationManager>();
					return navigationManagerComponent;
				}
				return null;
			}
		}

		public static ActionListManager actionListManager
		{
			get
			{
				if (actionListManagerComponent != null)
				{
					return actionListManagerComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					actionListManagerComponent = gameEnginePrefab.GetComponent<ActionListManager>();
					return actionListManagerComponent;
				}
				return null;
			}
		}

		public static LocalVariables localVariables
		{
			get
			{
				if (localVariablesComponent != null)
				{
					return localVariablesComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					localVariablesComponent = gameEnginePrefab.GetComponent<LocalVariables>();
					return localVariablesComponent;
				}
				return null;
			}
		}

		public static MenuPreview menuPreview
		{
			get
			{
				if (menuPreviewComponent != null)
				{
					return menuPreviewComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					menuPreviewComponent = gameEnginePrefab.GetComponent<MenuPreview>();
					return menuPreviewComponent;
				}
				return null;
			}
		}

		public static EventManager eventManager
		{
			get
			{
				if (eventManagerComponent != null)
				{
					return eventManagerComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					eventManagerComponent = gameEnginePrefab.GetComponent<EventManager>();
					return eventManagerComponent;
				}
				return null;
			}
		}

		public static KickStarter kickStarter
		{
			get
			{
				if (kickStarterComponent != null)
				{
					return kickStarterComponent;
				}
				SetGameEngine();
				if ((bool)gameEnginePrefab)
				{
					kickStarterComponent = gameEnginePrefab.GetComponent<KickStarter>();
					return kickStarterComponent;
				}
				return null;
			}
		}

		public static Music music
		{
			get
			{
				if (stateHandler != null)
				{
					return stateHandler.GetMusicEngine();
				}
				return null;
			}
		}

		public static Player player
		{
			get
			{
				if (playerPrefab != null)
				{
					return playerPrefab;
				}
				Player player = UnityEngine.Object.FindObjectOfType<Player>();
				if (player != null)
				{
					playerPrefab = player.GetComponent<Player>();
					return playerPrefab;
				}
				return null;
			}
		}

		public static MainCamera mainCamera
		{
			get
			{
				if (mainCameraPrefab != null)
				{
					return mainCameraPrefab;
				}
				MainCamera mainCamera = (MainCamera)UnityEngine.Object.FindObjectOfType(typeof(MainCamera));
				if ((bool)mainCamera)
				{
					mainCameraPrefab = mainCamera;
				}
				return mainCameraPrefab;
			}
			set
			{
				if (value != null)
				{
					mainCameraPrefab = value;
				}
			}
		}

		public static Camera CameraMain
		{
			get
			{
				if (settingsManager.cacheCameraMain)
				{
					if (cameraMain == null)
					{
						cameraMain = Camera.main;
					}
					return cameraMain;
				}
				return Camera.main;
			}
			set
			{
				if (value != null)
				{
					cameraMain = value;
				}
			}
		}

		protected void Awake()
		{
			if (GetComponent<MultiSceneChecker>() == null)
			{
				ACDebug.LogError("A 'MultiSceneChecker' component must be attached to the GameEngine prefab - please re-import AC.", base.gameObject);
			}
		}

		protected void OnDestroy()
		{
			if ((bool)stateHandler)
			{
				stateHandler.UnregisterWithGameEngine();
			}
		}

		public static void SetGameEngine(GameObject _gameEngine = null)
		{
			if (_gameEngine != null)
			{
				gameEnginePrefab = _gameEngine;
				menuSystemComponent = null;
				playerCursorComponent = null;
				playerInputComponent = null;
				playerInteractionComponent = null;
				playerMovementComponent = null;
				playerMenusComponent = null;
				playerQTEComponent = null;
				kickStarterComponent = null;
				sceneSettingsComponent = null;
				dialogComponent = null;
				menuPreviewComponent = null;
				navigationManagerComponent = null;
				actionListManagerComponent = null;
				localVariablesComponent = null;
				eventManagerComponent = null;
			}
			else if (gameEnginePrefab == null)
			{
				SceneSettings sceneSettings = UnityVersionHandler.GetKickStarterComponent<SceneSettings>();
				if (sceneSettings != null)
				{
					gameEnginePrefab = sceneSettings.gameObject;
				}
			}
		}

		private static void SetPersistentEngine()
		{
			if (!(persistentEnginePrefab == null))
			{
				return;
			}
			StateHandler stateHandler = UnityVersionHandler.GetKickStarterComponent<StateHandler>();
			if (stateHandler != null)
			{
				persistentEnginePrefab = stateHandler.gameObject;
				return;
			}
			try
			{
				persistentEnginePrefab = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("PersistentEngine"));
				persistentEnginePrefab.name = AdvGame.GetName("PersistentEngine");
			}
			catch (Exception ex)
			{
				ACDebug.LogWarning("Could not create PersistentEngine - make sure PersistentEngine, prefab is present in a Resources folder. Exception: " + ex);
			}
			if (persistentEnginePrefab != null)
			{
				stateHandler = persistentEnginePrefab.GetComponent<StateHandler>();
				if (stateHandler != null)
				{
					stateHandler.OnAwake();
				}
				else
				{
					ACDebug.LogWarning("Could not find StateHandler component on the PersistentEngine - is one attached?", persistentEnginePrefab);
				}
			}
		}

		public static void ClearManagerCache()
		{
			sceneManagerPrefab = null;
			settingsManagerPrefab = null;
			actionsManagerPrefab = null;
			variablesManagerPrefab = null;
			inventoryManagerPrefab = null;
			speechManagerPrefab = null;
			cursorManagerPrefab = null;
			menuManagerPrefab = null;
		}

		public static void ResetPlayer(Player ref_player, int ID, bool resetReferences, Quaternion _rotation, bool keepInventory = false, bool deleteInstantly = false, bool replacesOld = false, bool snapCamera = true)
		{
			Player[] array = UnityEngine.Object.FindObjectsOfType<Player>();
			if (array != null && array.Length > 0)
			{
				Player[] array2 = array;
				foreach (Player player in array2)
				{
					if (!(player != null))
					{
						continue;
					}
					player.ReleaseHeldObjects();
					if (deleteInstantly)
					{
						UnityEngine.Object.DestroyImmediate(player.gameObject);
						continue;
					}
					Renderer[] componentsInChildren = player.gameObject.GetComponentsInChildren<Renderer>();
					Renderer[] array3 = componentsInChildren;
					foreach (Renderer renderer in array3)
					{
						renderer.enabled = false;
					}
					Collider[] componentsInChildren2 = player.gameObject.GetComponentsInChildren<Collider>();
					Collider[] array4 = componentsInChildren2;
					foreach (Collider collider in array4)
					{
						if (!(collider is CharacterController))
						{
							collider.isTrigger = true;
						}
					}
					sceneChanger.ScheduleForDeletion(player.gameObject);
				}
			}
			if ((bool)ref_player)
			{
				SettingsManager settingsManager = AdvGame.GetReferences().settingsManager;
				Player player2 = UnityEngine.Object.Instantiate(ref_player, Vector3.zero, _rotation);
				player2.TransformRotation = _rotation;
				player2.ID = ID;
				player2.name = ref_player.name;
				playerPrefab = player2;
				UnityEngine.Object.DontDestroyOnLoad(player2);
				if ((bool)runtimeInventory)
				{
					runtimeInventory.SetNull();
					runtimeInventory.RemoveRecipes();
					runtimeObjectives.ClearUniqueToPlayer();
					if (settingsManager.playerSwitching == PlayerSwitching.Allow && !settingsManager.shareInventory && !keepInventory)
					{
						runtimeInventory.localItems.Clear();
						runtimeDocuments.ClearCollection();
					}
					if (saveSystem != null && saveSystem.DoesPlayerDataExist(ID))
					{
						bool doInventory = !settingsManager.shareInventory;
						bool doCamera = !replacesOld;
						if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow || (!settingsManager.shareInventory && keepInventory))
						{
							doInventory = false;
						}
						saveSystem.AssignPlayerData(ID, doInventory, doCamera, snapCamera);
					}
					foreach (Menu menu in PlayerMenus.GetMenus())
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
				}
				player2.Initialise();
				if (eventManager != null)
				{
					eventManager.Call_OnSetPlayer(player2);
				}
			}
			if (!resetReferences)
			{
				return;
			}
			playerMovement.AssignFPCamera();
			stateHandler.IgnoreNavMeshCollisions();
			stateHandler.UpdateAllMaxVolumes();
			_Camera[] array5 = UnityEngine.Object.FindObjectsOfType(typeof(_Camera)) as _Camera[];
			if (array5 != null)
			{
				_Camera[] array6 = array5;
				foreach (_Camera camera in array6)
				{
					camera.ResetTarget();
				}
			}
		}

		public void OnAwake()
		{
			ClearVariables();
			References references = (References)Resources.Load("References");
			if ((bool)references)
			{
				SceneManager sceneManager = AdvGame.GetReferences().sceneManager;
				SettingsManager settingsManager = AdvGame.GetReferences().settingsManager;
				ActionsManager actionsManager = AdvGame.GetReferences().actionsManager;
				InventoryManager inventoryManager = AdvGame.GetReferences().inventoryManager;
				VariablesManager variablesManager = AdvGame.GetReferences().variablesManager;
				SpeechManager speechManager = AdvGame.GetReferences().speechManager;
				CursorManager cursorManager = AdvGame.GetReferences().cursorManager;
				MenuManager menuManager = AdvGame.GetReferences().menuManager;
				if (sceneManager == null)
				{
					ACDebug.LogError("No Scene Manager found - please set one using the main Adventure Creator window");
				}
				if (settingsManager == null)
				{
					ACDebug.LogError("No Settings Manager found - please set one using the main Adventure Creator window");
				}
				else
				{
					if (settingsManager.IsInLoadingScene())
					{
						ACDebug.Log("Bypassing regular AC startup because the current scene is the 'Loading' scene.");
						SetPersistentEngine();
						return;
					}
					Player player = UnityEngine.Object.FindObjectOfType<Player>();
					if (player != null && settingsManager.GetDefaultPlayer() != null && player.name == settingsManager.GetDefaultPlayer().name + "(Clone)")
					{
						UnityEngine.Object.DestroyImmediate(player);
						ACDebug.LogWarning("Player clone found in scene - this may have been hidden by a Unity bug, and has been destroyed.");
					}
					player = UnityEngine.Object.FindObjectOfType<Player>();
					if (player == null)
					{
						ResetPlayer(settingsManager.GetDefaultPlayer(), settingsManager.GetDefaultPlayerID(), false, Quaternion.identity, false, true);
					}
					else
					{
						playerPrefab = player;
						SetPersistentEngine();
						if ((sceneChanger == null || sceneChanger.GetPlayerOnTransition() == null) && playerPrefab != null)
						{
							playerPrefab.Initialise();
							SetLocalPlayerID(playerPrefab);
						}
						AssignLocalPlayer();
					}
					if (KickStarter.player == null && KickStarter.settingsManager.movementMethod != MovementMethod.None)
					{
						ACDebug.LogWarning("No Player found - this can be assigned in the Settings Manager.");
					}
				}
				if (actionsManager == null)
				{
					ACDebug.LogError("No Actions Manager found - please set one using the main Adventure Creator window");
				}
				if (inventoryManager == null)
				{
					ACDebug.LogError("No Inventory Manager found - please set one using the main Adventure Creator window");
				}
				if (variablesManager == null)
				{
					ACDebug.LogError("No Variables Manager found - please set one using the main Adventure Creator window");
				}
				if (speechManager == null)
				{
					ACDebug.LogError("No Speech Manager found - please set one using the main Adventure Creator window");
				}
				if (cursorManager == null)
				{
					ACDebug.LogError("No Cursor Manager found - please set one using the main Adventure Creator window");
				}
				if (menuManager == null)
				{
					ACDebug.LogError("No Menu Manager found - please set one using the main Adventure Creator window");
				}
			}
			else
			{
				ACDebug.LogError("No References object found. Please set one using the main Adventure Creator window");
			}
			SetPersistentEngine();
			if (KickStarter.player != null)
			{
				if (saveSystem != null && saveSystem.loadingGame == LoadingGame.JustSwitchingPlayer && KickStarter.settingsManager != null && KickStarter.settingsManager.useLoadingScreen)
				{
					saveSystem.AssignPlayerAllData(KickStarter.player);
				}
				else
				{
					saveSystem.AssignPlayerAnimData(KickStarter.player);
				}
			}
		}

		private static void SetLocalPlayerID(Player player)
		{
			player.ID = -2 - UnityVersionHandler.GetCurrentSceneNumber();
			if (settingsManager != null && settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				ACDebug.LogWarning("The use of 'in-scene' local Players is not recommended when Player-switching is enabled - consider using the 'Player: Switch' Action to change Player instead.");
			}
			if (saveSystem != null && saveSystem.DoesPlayerDataExist(player.ID))
			{
				saveSystem.AssignPlayerAnimData(player);
			}
		}

		public void AfterLoad()
		{
			Player player = UnityEngine.Object.FindObjectOfType<Player>();
			if (player != null)
			{
				playerPrefab = player;
			}
		}

		public static void TurnOnAC()
		{
			if (stateHandler != null && actionListManager != null)
			{
				stateHandler.SetACState(true);
				ACDebug.Log("Adventure Creator has been turned on.");
			}
			else
			{
				ACDebug.LogWarning("Cannot turn AC on because the PersistentEngine and GameEngine are not present!");
			}
		}

		public static void TurnOffAC()
		{
			if (actionListManager != null)
			{
				actionListManager.KillAllLists();
				dialog.KillDialog(true, true, SpeechMenuLimit.All, SpeechMenuType.All, string.Empty);
				Moveable[] array = UnityEngine.Object.FindObjectsOfType(typeof(Moveable)) as Moveable[];
				Moveable[] array2 = array;
				foreach (Moveable moveable in array2)
				{
					moveable.StopMoving();
				}
				Char[] array3 = UnityEngine.Object.FindObjectsOfType(typeof(Char)) as Char[];
				Char[] array4 = array3;
				foreach (Char obj in array4)
				{
					obj.EndPath();
				}
				if ((bool)stateHandler)
				{
					stateHandler.SetACState(false);
					ACDebug.Log("Adventure Creator has been turned off.");
				}
			}
			else
			{
				ACDebug.LogWarning("Cannot turn AC off because it is not on!");
			}
		}

		private static void AssignLocalPlayer()
		{
			if (!(sceneChanger != null) || !(sceneChanger.GetPlayerOnTransition() != null))
			{
				return;
			}
			Player[] array = UnityEngine.Object.FindObjectsOfType<Player>();
			Player[] array2 = array;
			foreach (Player player in array2)
			{
				if (sceneChanger.GetPlayerOnTransition() != player)
				{
					sceneChanger.DestroyOldPlayer();
					playerPrefab = player;
					SetLocalPlayerID(playerPrefab);
					break;
				}
			}
		}

		public void ClearVariables()
		{
			playerPrefab = null;
			mainCameraPrefab = null;
			persistentEnginePrefab = null;
			gameEnginePrefab = null;
			sceneManagerPrefab = null;
			settingsManagerPrefab = null;
			actionsManagerPrefab = null;
			variablesManagerPrefab = null;
			inventoryManagerPrefab = null;
			speechManagerPrefab = null;
			cursorManagerPrefab = null;
			menuManagerPrefab = null;
			optionsComponent = null;
			runtimeInventoryComponent = null;
			runtimeVariablesComponent = null;
			playerMenusComponent = null;
			stateHandlerComponent = null;
			sceneChangerComponent = null;
			saveSystemComponent = null;
			levelStorageComponent = null;
			runtimeLanguagesComponent = null;
			actionListAssetManagerComponent = null;
			menuSystemComponent = null;
			dialogComponent = null;
			playerInputComponent = null;
			playerInteractionComponent = null;
			playerMovementComponent = null;
			playerCursorComponent = null;
			playerQTEComponent = null;
			sceneSettingsComponent = null;
			navigationManagerComponent = null;
			actionListManagerComponent = null;
			localVariablesComponent = null;
			menuPreviewComponent = null;
			eventManagerComponent = null;
			SetGameEngine();
		}
	}
}
