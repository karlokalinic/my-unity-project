using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_save_system.html")]
	public class SaveSystem : MonoBehaviour
	{
		[HideInInspector]
		public LoadingGame loadingGame;

		[HideInInspector]
		public List<SaveFile> foundSaveFiles = new List<SaveFile>();

		[HideInInspector]
		public List<SaveFile> foundImportFiles = new List<SaveFile>();

		public const string pipe = "|";

		public const string colon = ":";

		public const string mainDataDivider = "||";

		private const string mainDataDivider_Replacement = "*DOUBLEPIPE*";

		private float gameplayInvokeTime = 0.01f;

		private SaveData saveData = new SaveData();

		private SelectiveLoad activeSelectiveLoad = new SelectiveLoad();

		private static iSaveFileHandler saveFileHandlerOverride;

		private static iFileFormatHandler fileFormatHandlerOverride;

		private static iFileFormatHandler optionsFileFormatHandlerOverride;

		private SaveFile requestedLoad;

		private SaveFile requestedImport;

		private SaveFile requestedSave;

		private bool isTakingSaveScreenshot;

		private bool loadedInitialScene;

		public virtual int ScreenshotWidth
		{
			get
			{
				int a = (int)(KickStarter.mainCamera.GetPlayableScreenArea(false).width * KickStarter.settingsManager.screenshotResolutionFactor);
				return Mathf.Min(a, Screen.width);
			}
		}

		public virtual int ScreenshotHeight
		{
			get
			{
				int a = (int)(KickStarter.mainCamera.GetPlayableScreenArea(false).height * KickStarter.settingsManager.screenshotResolutionFactor);
				return Mathf.Min(a, Screen.height);
			}
		}

		public bool IsTakingSaveScreenshot
		{
			get
			{
				return isTakingSaveScreenshot;
			}
		}

		public static iSaveFileHandler SaveFileHandler
		{
			get
			{
				if (saveFileHandlerOverride != null)
				{
					return saveFileHandlerOverride;
				}
				return new SaveFileHandler_SystemFile();
			}
			set
			{
				saveFileHandlerOverride = value;
			}
		}

		public static iFileFormatHandler FileFormatHandler
		{
			get
			{
				if (fileFormatHandlerOverride != null)
				{
					return fileFormatHandlerOverride;
				}
				if (UnityVersionHandler.CanUseJson() && KickStarter.settingsManager != null && KickStarter.settingsManager.useJsonSerialization)
				{
					return new FileFormatHandler_Json();
				}
				return new FileFormatHandler_Binary();
			}
			set
			{
				fileFormatHandlerOverride = value;
			}
		}

		public static iFileFormatHandler OptionsFileFormatHandler
		{
			get
			{
				if (optionsFileFormatHandlerOverride != null)
				{
					return optionsFileFormatHandlerOverride;
				}
				return FileFormatHandler;
			}
			set
			{
				optionsFileFormatHandlerOverride = value;
			}
		}

		private void Awake()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
		}

		private void OnDestroy()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
		}

		private void SceneLoaded(Scene _scene, LoadSceneMode _loadSceneMode)
		{
			if (loadedInitialScene)
			{
				_OnLevelWasLoaded();
			}
			loadedInitialScene = true;
		}

		public void SetGameplayReturnTime(float _gameplayInvokeTime)
		{
			gameplayInvokeTime = _gameplayInvokeTime;
		}

		public void GatherSaveFiles()
		{
			foundSaveFiles = SaveFileHandler.GatherSaveFiles(Options.GetActiveProfileID());
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.orderSavesByUpdateTime)
			{
				foundSaveFiles.Sort((SaveFile a, SaveFile b) => a.updatedTime.CompareTo(b.updatedTime));
			}
			UpdateSaveFileLabels();
		}

		private void UpdateSaveFileLabels()
		{
			if (Options.optionsData == null || string.IsNullOrEmpty(Options.optionsData.saveFileNames))
			{
				return;
			}
			string[] array = Options.optionsData.saveFileNames.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				string label = array3[1];
				for (int j = 0; j < Mathf.Min(50, foundSaveFiles.Count); j++)
				{
					if (foundSaveFiles[j].saveID == result)
					{
						SaveFile saveFile = new SaveFile(foundSaveFiles[j]);
						saveFile.SetLabel(label);
						foundSaveFiles[j] = saveFile;
					}
				}
			}
		}

		public void GatherImportFiles(string projectName, string filePrefix, int boolID)
		{
			foundImportFiles = SaveFileHandler.GatherImportFiles(Options.GetActiveProfileID(), boolID, projectName, filePrefix);
		}

		public static string GetSaveExtension()
		{
			return FileFormatHandler.GetSaveExtension();
		}

		public static bool DoesImportExist(int saveID)
		{
			if ((bool)KickStarter.saveSystem)
			{
				foreach (SaveFile foundImportFile in KickStarter.saveSystem.foundImportFiles)
				{
					if (foundImportFile.saveID == saveID)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool DoesSaveExist(int elementSlot, int saveID, bool useSaveID)
		{
			if (!useSaveID)
			{
				saveID = ((elementSlot < 0 || KickStarter.saveSystem.foundSaveFiles.Count <= elementSlot) ? (-1) : KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID);
			}
			if ((bool)KickStarter.saveSystem)
			{
				foreach (SaveFile foundSaveFile in KickStarter.saveSystem.foundSaveFiles)
				{
					if (foundSaveFile.saveID == saveID)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool DoesSaveExist(int saveID)
		{
			return DoesSaveExist(0, saveID, true);
		}

		public static void LoadAutoSave()
		{
			if ((bool)KickStarter.saveSystem)
			{
				if (DoesSaveExist(0))
				{
					LoadGame(0);
				}
				else
				{
					ACDebug.LogWarning("Could not load autosave - file does not exist.");
				}
			}
		}

		public static void ImportGame(int elementSlot, int saveID, bool useSaveID)
		{
			if ((bool)KickStarter.saveSystem)
			{
				if (!useSaveID && KickStarter.saveSystem.foundImportFiles.Count > elementSlot)
				{
					saveID = KickStarter.saveSystem.foundImportFiles[elementSlot].saveID;
				}
				if (saveID >= 0)
				{
					KickStarter.saveSystem.ImportSaveGame(saveID);
				}
			}
		}

		public void SetSelectiveLoadOptions(SelectiveLoad selectiveLoad)
		{
			activeSelectiveLoad = selectiveLoad;
		}

		public static void ContinueGame()
		{
			if (Options.optionsData != null && Options.optionsData.lastSaveID >= 0)
			{
				LoadGame(Options.optionsData.lastSaveID);
			}
		}

		public static void LoadGame(int saveID)
		{
			LoadGame(0, saveID, true);
		}

		public static void LoadGame(int elementSlot, int saveID, bool useSaveID)
		{
			if (!KickStarter.saveSystem)
			{
				return;
			}
			if (!useSaveID)
			{
				if (elementSlot >= 0 && KickStarter.saveSystem.foundSaveFiles.Count > elementSlot)
				{
					saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID;
				}
				else
				{
					ACDebug.LogWarning("Can't select save slot " + elementSlot + " because only " + KickStarter.saveSystem.foundSaveFiles.Count + " have been found!");
				}
			}
			foreach (SaveFile foundSaveFile in KickStarter.saveSystem.foundSaveFiles)
			{
				if (foundSaveFile.saveID == saveID)
				{
					SaveFile saveFile = foundSaveFile;
					KickStarter.saveSystem.LoadSaveGame(saveFile);
					return;
				}
			}
			ACDebug.LogWarning("Could not load game: file with ID " + saveID + " does not exist.");
		}

		public void ClearAllData()
		{
			saveData = new SaveData();
		}

		private void ImportSaveGame(int saveID)
		{
			foreach (SaveFile foundImportFile in foundImportFiles)
			{
				if (foundImportFile.saveID == saveID)
				{
					requestedImport = new SaveFile(foundImportFile);
					string saveFileContents = SaveFileHandler.Load(foundImportFile, true);
					ReceiveDataToImport(foundImportFile, saveFileContents);
					break;
				}
			}
		}

		public void ReceiveDataToImport(SaveFile saveFile, string saveFileContents)
		{
			if (requestedImport != null && saveFile != null && requestedImport.saveID == saveFile.saveID && requestedImport.profileID == saveFile.profileID)
			{
				requestedImport = null;
				if (!string.IsNullOrEmpty(saveFileContents))
				{
					KickStarter.eventManager.Call_OnImport(FileAccessState.Before);
					saveData = ExtractMainData(saveFileContents);
					KillActionLists();
					AssignVariables(saveData.mainData.runtimeVariablesData);
					KickStarter.eventManager.Call_OnImport(FileAccessState.After);
				}
				else
				{
					KickStarter.eventManager.Call_OnImport(FileAccessState.Fail);
				}
			}
		}

		private void LoadSaveGame(SaveFile saveFile)
		{
			requestedLoad = new SaveFile(saveFile);
			string saveFileContents = SaveFileHandler.Load(saveFile, true);
			ReceiveDataToLoad(saveFile, saveFileContents);
		}

		public static SaveData ExtractMainData(string saveFileContents)
		{
			if (!string.IsNullOrEmpty(saveFileContents))
			{
				int divider = GetDivider(saveFileContents);
				string text = saveFileContents.Substring(0, divider);
				text = text.Replace("*DOUBLEPIPE*", "||");
				return Serializer.DeserializeObject<SaveData>(text);
			}
			return null;
		}

		public static List<SingleLevelData> ExtractSceneData(string saveFileContents)
		{
			int startIndex = GetDivider(saveFileContents) + "||".Length;
			string text = saveFileContents.Substring(startIndex);
			text = text.Replace("*DOUBLEPIPE*", "||");
			return FileFormatHandler.DeserializeAllRoomData(text);
		}

		public static List<GVar> ExtractSaveFileVariables(SaveFile saveFile)
		{
			if (saveFile != null)
			{
				string saveFileContents = SaveFileHandler.Load(saveFile, false);
				SaveData saveData = ExtractMainData(saveFileContents);
				if (saveData != null)
				{
					string runtimeVariablesData = saveData.mainData.runtimeVariablesData;
					return UnloadVariablesData(runtimeVariablesData, KickStarter.runtimeVariables.globalVars);
				}
				ACDebug.LogWarning("Cannot extract variable data from save file ID = " + saveFile.saveID);
			}
			return null;
		}

		protected static int GetDivider(string saveFileContents)
		{
			return saveFileContents.IndexOf("||");
		}

		protected static string MergeData(string _mainData, string _levelData)
		{
			return _mainData.Replace("||", "*DOUBLEPIPE*") + "||" + _levelData.Replace("||", "*DOUBLEPIPE*");
		}

		public void ReceiveDataToLoad(SaveFile saveFile, string saveFileContents)
		{
			if (requestedLoad == null || saveFile == null || requestedLoad.saveID != saveFile.saveID || requestedLoad.profileID != saveFile.profileID)
			{
				return;
			}
			requestedLoad = null;
			if (!string.IsNullOrEmpty(saveFileContents))
			{
				KickStarter.eventManager.Call_OnLoad(FileAccessState.Before, saveFile.saveID, saveFile);
				saveData = ExtractMainData(saveFileContents);
				if (activeSelectiveLoad.loadSceneObjects)
				{
					KickStarter.levelStorage.allLevelData = ExtractSceneData(saveFileContents);
				}
				KillActionLists();
				bool flag = false;
				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && ((KickStarter.player == null && saveData.mainData.currentPlayerID != KickStarter.settingsManager.GetEmptyPlayerID()) || (KickStarter.player != null && KickStarter.player.ID != saveData.mainData.currentPlayerID)))
				{
					KickStarter.ResetPlayer(GetPlayerByID(saveData.mainData.currentPlayerID), saveData.mainData.currentPlayerID, true, Quaternion.identity, false, true);
					flag = true;
				}
				bool reloadSceneWhenLoading = KickStarter.settingsManager.reloadSceneWhenLoading;
				int playerScene = GetPlayerScene(saveData.mainData.currentPlayerID, saveData.playerData);
				if (reloadSceneWhenLoading || (playerScene != UnityVersionHandler.GetCurrentSceneNumber() && activeSelectiveLoad.loadScene))
				{
					if (flag && KickStarter.settingsManager.reloadSceneWhenLoading)
					{
						KickStarter.mainCamera.FadeOut(0f);
					}
					loadingGame = LoadingGame.InNewScene;
					KickStarter.sceneChanger.ChangeScene(new SceneInfo(playerScene), false, reloadSceneWhenLoading);
					return;
				}
				loadingGame = LoadingGame.InSameScene;
				Sound[] array = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
				Sound[] array2 = array;
				foreach (Sound sound in array2)
				{
					if ((bool)sound.GetComponent<AudioSource>() && sound.soundType != SoundType.Music && !sound.GetComponent<AudioSource>().loop)
					{
						sound.Stop();
					}
				}
				_OnLevelWasLoaded();
			}
			else
			{
				KickStarter.eventManager.Call_OnLoad(FileAccessState.Fail, saveFile.saveID);
			}
		}

		private Player GetPlayerByID(int id)
		{
			SettingsManager settingsManager = KickStarter.settingsManager;
			foreach (PlayerPrefab player in settingsManager.players)
			{
				if (player.ID == id)
				{
					if ((bool)player.playerOb)
					{
						return player.playerOb;
					}
					return null;
				}
			}
			return null;
		}

		public PlayerData GetPlayerData(int playerID)
		{
			if (saveData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData playerDatum in saveData.playerData)
				{
					if (playerDatum.playerID == playerID)
					{
						return playerDatum;
					}
				}
			}
			return null;
		}

		private int GetPlayerScene(int playerID, List<PlayerData> _playerData)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow && playerID > 0)
			{
				playerID = 0;
			}
			PlayerData playerData = GetPlayerData(playerID);
			if (playerData != null)
			{
				return playerData.currentScene;
			}
			return UnityVersionHandler.GetCurrentSceneNumber();
		}

		private string GetPlayerSceneName(int playerID, List<PlayerData> _playerData)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow && playerID > 0)
			{
				playerID = 0;
			}
			PlayerData playerData = GetPlayerData(playerID);
			if (playerData != null)
			{
				return playerData.currentSceneName;
			}
			return UnityVersionHandler.GetCurrentSceneName();
		}

		private void _OnLevelWasLoaded()
		{
			KickStarter.stateHandler.AfterLoad();
			if (KickStarter.settingsManager.IsInLoadingScene() || KickStarter.sceneSettings == null)
			{
				return;
			}
			ResetSceneObjects();
			if (loadingGame == LoadingGame.InNewScene || loadingGame == LoadingGame.InSameScene)
			{
				if ((bool)KickStarter.dialog)
				{
					KickStarter.dialog.KillDialog(true, true, SpeechMenuLimit.All, SpeechMenuType.All, string.Empty);
				}
				if ((bool)KickStarter.playerInteraction)
				{
					KickStarter.playerInteraction.StopMovingToHotspot();
				}
				ReturnMainData();
				KickStarter.levelStorage.ReturnCurrentLevelData(true);
				CustomLoadHook();
				KickStarter.eventManager.Call_OnLoad(FileAccessState.After, -1);
			}
			if ((bool)KickStarter.runtimeInventory)
			{
				KickStarter.runtimeInventory.RemoveRecipes();
			}
			if (loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				PlayerData playerData = GetPlayerData(KickStarter.player.ID);
				if (playerData != null)
				{
					ReturnCameraData(playerData);
					KickStarter.playerInput.LoadPlayerData(playerData);
					KickStarter.sceneChanger.LoadPlayerData(playerData);
				}
				KickStarter.sceneSettings.UnpauseGame(KickStarter.playerInput.timeScale);
				KickStarter.stateHandler.gameState = GameState.Cutscene;
				KickStarter.mainCamera.FadeIn(0.5f);
			}
			else
			{
				activeSelectiveLoad = new SelectiveLoad();
			}
			AssetLoader.UnloadAssets();
			StartCoroutine(ReturnToGameplay());
		}

		public static void SaveNewGame(bool overwriteLabel = true, string newLabel = "")
		{
			if ((bool)KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SaveNewSaveGame(overwriteLabel, newLabel);
			}
		}

		private void SaveNewSaveGame(bool overwriteLabel = true, string newLabel = "")
		{
			if (foundSaveFiles != null && foundSaveFiles.Count > 0)
			{
				int num = -1;
				for (int i = 0; i < foundSaveFiles.Count; i++)
				{
					if (num != -1 && num != foundSaveFiles[i].saveID)
					{
						SaveSaveGame(num, overwriteLabel, newLabel);
						return;
					}
					num = foundSaveFiles[i].saveID + 1;
				}
				int saveID = foundSaveFiles[foundSaveFiles.Count - 1].saveID + 1;
				SaveSaveGame(saveID, overwriteLabel, newLabel);
			}
			else
			{
				SaveSaveGame(1, overwriteLabel, newLabel);
			}
		}

		public static void SaveAutoSave()
		{
			if ((bool)KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SaveSaveGame(0, true, string.Empty);
			}
		}

		public static void SaveGame(int saveID, bool overwriteLabel = true, string newLabel = "")
		{
			SaveGame(0, saveID, true, overwriteLabel, newLabel);
		}

		public static void SaveGame(int elementSlot, int saveID, bool useSaveID, bool overwriteLabel = true, string newLabel = "")
		{
			if ((bool)KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					saveID = ((KickStarter.saveSystem.foundSaveFiles.Count <= elementSlot) ? (-1) : KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID);
				}
				if (saveID == -1)
				{
					SaveNewGame(overwriteLabel, newLabel);
				}
				else
				{
					KickStarter.saveSystem.SaveSaveGame(saveID, overwriteLabel, newLabel);
				}
			}
		}

		private void SaveSaveGame(int saveID, bool overwriteLabel = true, string newLabel = "")
		{
			if (GetNumSaves() >= KickStarter.settingsManager.maxSaves && !DoesSaveExist(saveID))
			{
				ACDebug.LogWarning("Cannot save - maximum number of save files has already been reached.");
				KickStarter.eventManager.Call_OnSave(FileAccessState.Fail, saveID);
				return;
			}
			KickStarter.eventManager.Call_OnSave(FileAccessState.Before, saveID);
			CustomSaveHook();
			KickStarter.levelStorage.StoreAllOpenLevelData();
			if ((bool)KickStarter.playerInput && (bool)KickStarter.runtimeInventory && (bool)KickStarter.sceneChanger && (bool)KickStarter.settingsManager && (bool)KickStarter.stateHandler)
			{
				StartCoroutine(PrepareSaveCoroutine(saveID, overwriteLabel, newLabel));
				return;
			}
			if (KickStarter.playerInput == null)
			{
				ACDebug.LogWarning("Save failed - no PlayerInput found.");
			}
			if (KickStarter.runtimeInventory == null)
			{
				ACDebug.LogWarning("Save failed - no RuntimeInventory found.");
			}
			if (KickStarter.sceneChanger == null)
			{
				ACDebug.LogWarning("Save failed - no SceneChanger found.");
			}
			if (KickStarter.settingsManager == null)
			{
				ACDebug.LogWarning("Save failed - no Settings Manager found.");
			}
		}

		private IEnumerator PrepareSaveCoroutine(int saveID, bool overwriteLabel = true, string newLabel = "")
		{
			while (loadingGame != LoadingGame.No)
			{
				ACDebug.LogWarning("Delaying request to save due to the game currently loading.");
				yield return new WaitForEndOfFrame();
			}
			Player player = KickStarter.player;
			if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData playerDatum in saveData.playerData)
				{
					if (player != null && playerDatum.playerID == player.ID)
					{
						saveData.playerData.Remove(playerDatum);
						break;
					}
					if (player == null && playerDatum.playerID == KickStarter.settingsManager.GetEmptyPlayerID())
					{
						saveData.playerData.Remove(playerDatum);
						break;
					}
				}
			}
			else
			{
				saveData = new SaveData();
				saveData.mainData = default(MainData);
				saveData.playerData = new List<PlayerData>();
			}
			PlayerData playerData = SavePlayerData(player);
			saveData.playerData.Add(playerData);
			saveData.mainData = KickStarter.stateHandler.SaveMainData(saveData.mainData);
			saveData.mainData.movementMethod = (int)KickStarter.settingsManager.movementMethod;
			saveData.mainData.activeInputsData = ActiveInput.CreateSaveData(KickStarter.settingsManager.activeInputs);
			if (player != null)
			{
				saveData.mainData.currentPlayerID = player.ID;
			}
			else
			{
				saveData.mainData.currentPlayerID = KickStarter.settingsManager.GetEmptyPlayerID();
			}
			saveData.mainData = KickStarter.playerInput.SaveMainData(saveData.mainData);
			saveData.mainData = KickStarter.runtimeInventory.SaveMainData(saveData.mainData);
			saveData.mainData = KickStarter.runtimeVariables.SaveMainData(saveData.mainData);
			saveData.mainData = KickStarter.playerMenus.SaveMainData(saveData.mainData);
			saveData.mainData = KickStarter.runtimeLanguages.SaveMainData(saveData.mainData);
			saveData.mainData.activeAssetLists = KickStarter.actionListAssetManager.GetSaveData();
			string mainData = Serializer.SerializeObject<SaveData>(saveData, true);
			string levelData = FileFormatHandler.SerializeAllRoomData(KickStarter.levelStorage.allLevelData);
			string allData = MergeData(mainData, levelData);
			if (overwriteLabel)
			{
				if (string.IsNullOrEmpty(newLabel))
				{
					newLabel = SaveFileHandler.GetDefaultSaveLabel(saveID);
				}
			}
			else
			{
				newLabel = string.Empty;
			}
			int profileID = Options.GetActiveProfileID();
			SaveFile saveFile = new SaveFile(saveID, profileID, newLabel, string.Empty, false, null, string.Empty);
			if (KickStarter.settingsManager.takeSaveScreenshots)
			{
				isTakingSaveScreenshot = true;
				KickStarter.playerMenus.PreScreenshotBackup();
				yield return new WaitForEndOfFrame();
				Texture2D screenshotTex = GetScreenshotTexture();
				if (screenshotTex != null)
				{
					saveFile.screenShot = screenshotTex;
					SaveFileHandler.SaveScreenshot(saveFile);
					Object.Destroy(screenshotTex);
				}
				KickStarter.playerMenus.PostScreenshotBackup();
				isTakingSaveScreenshot = false;
			}
			requestedSave = new SaveFile(saveFile);
			SaveFileHandler.Save(requestedSave, allData);
			yield return null;
		}

		protected virtual Texture2D GetScreenshotTexture()
		{
			if (KickStarter.mainCamera != null)
			{
				Texture2D texture2D = new Texture2D(ScreenshotWidth, ScreenshotHeight);
				Rect playableScreenArea = KickStarter.mainCamera.GetPlayableScreenArea(false);
				texture2D.ReadPixels(playableScreenArea, 0, 0);
				texture2D.Apply();
				return texture2D;
			}
			ACDebug.LogWarning("Cannot take screenshot - no main Camera found!");
			return null;
		}

		public void OnFinishSaveRequest(SaveFile saveFile, bool wasSuccesful)
		{
			if (requestedSave == null || saveFile == null || requestedSave.saveID != saveFile.saveID || requestedSave.profileID != saveFile.profileID)
			{
				return;
			}
			requestedSave = null;
			if (!wasSuccesful)
			{
				KickStarter.eventManager.Call_OnSave(FileAccessState.Fail, saveFile.saveID);
				return;
			}
			GatherSaveFiles();
			if (!string.IsNullOrEmpty(saveFile.label))
			{
				for (int i = 0; i < Mathf.Min(50, foundSaveFiles.Count); i++)
				{
					if (foundSaveFiles[i].saveID == saveFile.saveID)
					{
						SaveFile saveFile2 = new SaveFile(foundSaveFiles[i]);
						saveFile2.SetLabel(saveFile.label);
						foundSaveFiles[i] = saveFile2;
						break;
					}
				}
			}
			Options.optionsData.lastSaveID = saveFile.saveID;
			Options.UpdateSaveLabels(foundSaveFiles.ToArray());
			UpdateSaveFileLabels();
			KickStarter.eventManager.Call_OnSave(FileAccessState.After, saveFile.saveID, saveFile);
		}

		public void SaveCurrentPlayerData()
		{
			if (loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				return;
			}
			if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData playerDatum in saveData.playerData)
				{
					if ((KickStarter.player != null && playerDatum.playerID == KickStarter.player.ID) || (KickStarter.player == null && playerDatum.playerID == KickStarter.settingsManager.GetEmptyPlayerID()))
					{
						saveData.playerData.Remove(playerDatum);
						break;
					}
				}
			}
			else
			{
				saveData = new SaveData();
				saveData.mainData = default(MainData);
				saveData.playerData = new List<PlayerData>();
			}
			PlayerData item = SavePlayerData(KickStarter.player);
			saveData.playerData.Add(item);
		}

		private PlayerData SavePlayerData(Player player)
		{
			PlayerData playerData = new PlayerData();
			playerData.currentScene = UnityVersionHandler.GetCurrentSceneNumber();
			playerData.currentSceneName = UnityVersionHandler.GetCurrentSceneName();
			playerData = KickStarter.sceneChanger.SavePlayerData(playerData);
			playerData = KickStarter.playerInput.SavePlayerData(playerData);
			KickStarter.runtimeInventory.RemoveRecipes();
			playerData.inventoryData = CreateInventoryData(KickStarter.runtimeInventory.localItems);
			playerData = KickStarter.runtimeDocuments.SavePlayerDocuments(playerData);
			playerData = KickStarter.runtimeObjectives.SavePlayerObjectives(playerData);
			MainCamera mainCamera = KickStarter.mainCamera;
			playerData = mainCamera.SaveData(playerData);
			if (player == null)
			{
				playerData.playerPortraitGraphic = string.Empty;
				playerData.playerID = KickStarter.settingsManager.GetEmptyPlayerID();
				return playerData;
			}
			return player.SavePlayerData(playerData);
		}

		public static int GetNumImportSlots()
		{
			return KickStarter.saveSystem.foundImportFiles.Count;
		}

		public static int GetNumSlots()
		{
			return KickStarter.saveSystem.foundSaveFiles.Count;
		}

		public bool DoImportCheck(string fileData, int boolID)
		{
			if (!string.IsNullOrEmpty(fileData.ToString()))
			{
				SaveData saveData = ExtractMainData(fileData);
				if (saveData == null)
				{
					saveData = new SaveData();
				}
				string runtimeVariablesData = saveData.mainData.runtimeVariablesData;
				if (!string.IsNullOrEmpty(runtimeVariablesData))
				{
					string[] array = runtimeVariablesData.Split("|"[0]);
					string[] array2 = array;
					foreach (string text in array2)
					{
						string[] array3 = text.Split(":"[0]);
						int result = 0;
						int.TryParse(array3[0], out result);
						if (result == boolID)
						{
							int result2 = 0;
							int.TryParse(array3[1], out result2);
							if (result2 == 1)
							{
								return true;
							}
							return false;
						}
					}
				}
			}
			return false;
		}

		public static string GenerateSaveSuffix(int saveID, int profileID = -1)
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.useProfiles)
			{
				if (profileID == -1)
				{
					profileID = Options.GetActiveProfileID();
				}
				return "_" + saveID + "_" + profileID;
			}
			return "_" + saveID;
		}

		private void KillActionLists()
		{
			KickStarter.actionListManager.KillAllLists();
			Moveable[] array = Object.FindObjectsOfType(typeof(Moveable)) as Moveable[];
			Moveable[] array2 = array;
			foreach (Moveable moveable in array2)
			{
				moveable.StopMoving();
			}
		}

		public static string GetImportSlotLabel(int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundImportFiles != null)
			{
				return KickStarter.saveSystem.GetSlotLabel(elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundImportFiles.ToArray());
			}
			return "Save test (01/01/2001 12:00:00)";
		}

		public static string GetSaveSlotLabel(int elementSlot, int saveID, bool useSaveID)
		{
			if (!Application.isPlaying)
			{
				if (useSaveID)
				{
					elementSlot = saveID;
				}
				return SaveFileHandler.GetDefaultSaveLabel(elementSlot);
			}
			if (KickStarter.saveSystem.foundSaveFiles != null)
			{
				return KickStarter.saveSystem.GetSlotLabel(elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundSaveFiles.ToArray());
			}
			return "Save game file";
		}

		public string GetSlotLabel(int elementSlot, int saveID, bool useSaveID, SaveFile[] saveFiles)
		{
			if (Application.isPlaying)
			{
				if (useSaveID)
				{
					foreach (SaveFile saveFile in saveFiles)
					{
						if (saveFile.saveID == saveID)
						{
							return saveFile.label;
						}
					}
				}
				else if (elementSlot >= 0 && elementSlot < saveFiles.Length)
				{
					return saveFiles[elementSlot].label;
				}
				return string.Empty;
			}
			return "Save test (01/01/2001 12:00:00)";
		}

		public static Texture2D GetImportSlotScreenshot(int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundImportFiles != null)
			{
				return KickStarter.saveSystem.GetScreenshot(elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundImportFiles.ToArray());
			}
			return null;
		}

		public static Texture2D GetSaveSlotScreenshot(int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundSaveFiles != null)
			{
				return KickStarter.saveSystem.GetScreenshot(elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundSaveFiles.ToArray());
			}
			return null;
		}

		public Texture2D GetScreenshot(int elementSlot, int saveID, bool useSaveID, SaveFile[] saveFiles)
		{
			if (Application.isPlaying)
			{
				if (useSaveID)
				{
					foreach (SaveFile saveFile in saveFiles)
					{
						if (saveFile.saveID == saveID)
						{
							return saveFile.screenShot;
						}
					}
				}
				else if (elementSlot >= 0 && elementSlot < saveFiles.Length)
				{
					return saveFiles[elementSlot].screenShot;
				}
			}
			return null;
		}

		private void ReturnMainData()
		{
			if ((bool)KickStarter.playerInput && (bool)KickStarter.runtimeInventory && (bool)KickStarter.settingsManager && (bool)KickStarter.stateHandler)
			{
				PlayerData playerData = new PlayerData();
				int currentPlayerID = saveData.mainData.currentPlayerID;
				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow && currentPlayerID > 0)
				{
					currentPlayerID = 0;
				}
				PlayerData playerData2 = GetPlayerData(saveData.mainData.currentPlayerID);
				if (playerData2 != null)
				{
					playerData = playerData2;
				}
				if (activeSelectiveLoad.loadPlayer)
				{
					ReturnPlayerData(playerData, KickStarter.player);
				}
				if (activeSelectiveLoad.loadSceneObjects)
				{
					ReturnCameraData(playerData);
				}
				KickStarter.stateHandler.LoadMainData(saveData.mainData);
				KickStarter.actionListAssetManager.LoadData(saveData.mainData.activeAssetLists);
				KickStarter.settingsManager.movementMethod = (MovementMethod)saveData.mainData.movementMethod;
				ActiveInput.LoadSaveData(saveData.mainData.activeInputsData);
				if (activeSelectiveLoad.loadScene)
				{
					KickStarter.sceneChanger.LoadPlayerData(playerData, activeSelectiveLoad.loadSubScenes);
				}
				if (activeSelectiveLoad.loadPlayer)
				{
					KickStarter.playerInput.LoadPlayerData(playerData);
				}
				KickStarter.runtimeInventory.RemoveRecipes();
				if (activeSelectiveLoad.loadInventory)
				{
					KickStarter.runtimeInventory.AssignPlayerInventory(AssignInventory(KickStarter.runtimeInventory, playerData.inventoryData));
					KickStarter.runtimeDocuments.AssignPlayerDocuments(playerData);
					KickStarter.runtimeObjectives.AssignPlayerObjectives(playerData);
					if (saveData.mainData.selectedInventoryID > -1)
					{
						if (saveData.mainData.isGivingItem)
						{
							KickStarter.runtimeInventory.SelectItemByID(saveData.mainData.selectedInventoryID, SelectItemMode.Give);
						}
						else
						{
							KickStarter.runtimeInventory.SelectItemByID(saveData.mainData.selectedInventoryID);
						}
					}
					else
					{
						KickStarter.runtimeInventory.SetNull();
					}
					KickStarter.runtimeInventory.RemoveRecipes();
				}
				KickStarter.playerInput.LoadMainData(saveData.mainData);
				if (activeSelectiveLoad.loadVariables)
				{
					AssignVariables(saveData.mainData.runtimeVariablesData);
					KickStarter.runtimeVariables.AssignCustomTokensFromString(saveData.mainData.customTokenData);
				}
				KickStarter.playerMenus.LoadMainData(saveData.mainData);
				KickStarter.runtimeLanguages.LoadMainData(saveData.mainData);
				KickStarter.mainCamera.HideScene();
				KickStarter.sceneSettings.UnpauseGame(KickStarter.playerInput.timeScale);
				KickStarter.stateHandler.gameState = GameState.Cutscene;
				KickStarter.mainCamera.FadeIn(0.5f);
			}
			else
			{
				if (KickStarter.playerInput == null)
				{
					ACDebug.LogWarning("Load failed - no PlayerInput found.");
				}
				if (KickStarter.runtimeInventory == null)
				{
					ACDebug.LogWarning("Load failed - no RuntimeInventory found.");
				}
				if (KickStarter.sceneChanger == null)
				{
					ACDebug.LogWarning("Load failed - no SceneChanger found.");
				}
				if (KickStarter.settingsManager == null)
				{
					ACDebug.LogWarning("Load failed - no Settings Manager found.");
				}
			}
		}

		public bool DoesPlayerDataExist(int ID, bool doSceneCheck = false)
		{
			PlayerData playerData = GetPlayerData(ID);
			if (playerData != null)
			{
				if (doSceneCheck && playerData.currentScene == -1 && string.IsNullOrEmpty(playerData.currentSceneName))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public int GetPlayerScene(int ID)
		{
			if ((bool)KickStarter.player)
			{
				PlayerData playerData = GetPlayerData(ID);
				if (playerData != null)
				{
					return playerData.currentScene;
				}
			}
			return UnityVersionHandler.GetCurrentSceneNumber();
		}

		public string GetPlayerSceneName(int ID)
		{
			if ((bool)KickStarter.player)
			{
				PlayerData playerData = GetPlayerData(ID);
				if (playerData != null)
				{
					return playerData.currentSceneName;
				}
			}
			return UnityVersionHandler.GetCurrentSceneName();
		}

		public void MoveInactivePlayerToCurrentScene(int ID, Transform newTransform, _Camera associatedCamera = null)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
				return;
			}
			if (KickStarter.player != null && KickStarter.player.ID == ID)
			{
				ACDebug.LogWarning(string.Concat("Cannot update position of player ", ID, " because that Player (", KickStarter.player, ") is currently active!"));
				return;
			}
			Quaternion newRotation = ((!(newTransform.GetComponent<NPC>() != null)) ? newTransform.rotation : newTransform.GetComponent<NPC>().TransformRotation);
			if (saveData == null)
			{
				ClearAllData();
			}
			PlayerData playerData = GetPlayerData(ID);
			if (playerData != null)
			{
				playerData.UpdatePosition(UnityVersionHandler.GetCurrentSceneInfo(), newTransform.position, newRotation, associatedCamera);
				PlaceAssociatedNPC(playerData);
				return;
			}
			Player player = KickStarter.settingsManager.GetPlayer(ID);
			playerData = player.SavePlayerData(new PlayerData());
			playerData.playerID = ID;
			playerData.UpdatePosition(UnityVersionHandler.GetCurrentSceneInfo(), newTransform.position, newRotation, associatedCamera);
			saveData.playerData.Add(playerData);
			PlaceAssociatedNPC(playerData);
		}

		private void PlaceAssociatedNPC(PlayerData playerData)
		{
			NPC nPC = null;
			NPC nPC2 = null;
			foreach (PlayerPrefab player in KickStarter.settingsManager.players)
			{
				if (player.ID != playerData.playerID)
				{
					continue;
				}
				if (!(player.playerOb != null))
				{
					break;
				}
				nPC2 = player.playerOb.associatedNPCPrefab;
				if (nPC2 != null)
				{
					ConstantID component = nPC2.GetComponent<ConstantID>();
					if (component != null)
					{
						nPC = Serializer.returnComponent<NPC>(component.constantID);
					}
				}
				break;
			}
			if (!(nPC2 == null))
			{
				if (nPC == null)
				{
					GameObject gameObject = Object.Instantiate(nPC2.gameObject);
					gameObject.name = nPC2.gameObject.name;
					nPC = gameObject.GetComponent<NPC>();
				}
				if (nPC != null)
				{
					Vector3 position = new Vector3(playerData.playerLocX, playerData.playerLocY, playerData.playerLocZ);
					nPC.Teleport(position);
					nPC.SetRotation(playerData.playerRotY);
				}
			}
		}

		public void AssignPlayerAnimData(Player player)
		{
			if (!(player != null) || saveData.playerData.Count <= 0)
			{
				return;
			}
			foreach (PlayerData playerDatum in saveData.playerData)
			{
				if (player.ID == playerDatum.playerID)
				{
					player.LoadPlayerData(playerDatum, true);
				}
			}
		}

		public void AssignPlayerAllData(Player player)
		{
			if (!(player != null) || saveData.playerData.Count <= 0)
			{
				return;
			}
			foreach (PlayerData playerDatum in saveData.playerData)
			{
				if (player.ID == playerDatum.playerID)
				{
					player.LoadPlayerData(playerDatum);
				}
			}
		}

		public void AssignPlayerData(int ID, bool doInventory, bool doCamera, bool snapCamera = true)
		{
			if ((bool)KickStarter.player)
			{
				PlayerData playerData = GetPlayerData(ID);
				if (playerData != null)
				{
					if (playerData.currentScene != -1)
					{
						ReturnPlayerData(playerData, KickStarter.player);
						if (doCamera)
						{
							ReturnCameraData(playerData, snapCamera);
						}
						KickStarter.playerInput.LoadPlayerData(playerData);
						KickStarter.sceneChanger.LoadPlayerData(playerData);
					}
					KickStarter.runtimeInventory.SetNull();
					KickStarter.runtimeInventory.RemoveRecipes();
					if (doInventory)
					{
						KickStarter.runtimeInventory.AssignPlayerInventory(AssignInventory(KickStarter.runtimeInventory, playerData.inventoryData));
						KickStarter.runtimeDocuments.AssignPlayerDocuments(playerData);
						KickStarter.runtimeObjectives.AssignPlayerObjectives(playerData);
					}
					return;
				}
			}
			AssetLoader.UnloadAssets();
		}

		private void ReturnPlayerData(PlayerData playerData, Player player)
		{
			if (!(player == null))
			{
				player.LoadPlayerData(playerData);
			}
		}

		private void ReturnCameraData(PlayerData playerData, bool snapCamera = true)
		{
			MainCamera mainCamera = KickStarter.mainCamera;
			mainCamera.LoadData(playerData, snapCamera);
		}

		private IEnumerator ReturnToGameplay()
		{
			yield return new WaitForEndOfFrame();
			if (KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera != null)
			{
				KickStarter.mainCamera.attachedCamera.MoveCameraInstant();
			}
			yield return new WaitForSeconds(gameplayInvokeTime);
			if (loadingGame != LoadingGame.No)
			{
				KickStarter.playerInput.ReturnToGameplayAfterLoad();
				if ((bool)KickStarter.sceneSettings)
				{
					KickStarter.sceneSettings.OnLoad();
				}
			}
			if (loadingGame == LoadingGame.No)
			{
				RemoveAssociatedNPCs(false);
			}
			else if (loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				RemoveAssociatedNPCs(true);
			}
			if (KickStarter.eventManager != null)
			{
				KickStarter.eventManager.Call_OnAfterChangeScene(loadingGame);
			}
			loadingGame = LoadingGame.No;
		}

		private void RemoveAssociatedNPCs(bool switchingPlayer)
		{
			RemoveAssociatedNPC(KickStarter.player, switchingPlayer);
		}

		private void RemoveAssociatedNPC(Player player, bool repositionPlayer = false)
		{
			if (player == null)
			{
				return;
			}
			NPC runtimeAssociatedNPC = player.GetRuntimeAssociatedNPC();
			if (runtimeAssociatedNPC != null)
			{
				if (repositionPlayer)
				{
					player.RepositionToTransform(runtimeAssociatedNPC.transform);
				}
				runtimeAssociatedNPC.HideFromView(player);
			}
		}

		public static void AssignVariables(string runtimeVariablesData, bool fromOptions = false)
		{
			if (runtimeVariablesData != null)
			{
				KickStarter.runtimeVariables.ClearSpeechLog();
				KickStarter.runtimeVariables.globalVars = UnloadVariablesData(runtimeVariablesData, KickStarter.runtimeVariables.globalVars, fromOptions);
				GlobalVariables.UploadAll();
			}
		}

		private List<InvItem> AssignInventory(RuntimeInventory _runtimeInventory, string inventoryData)
		{
			List<InvItem> list = new List<InvItem>();
			if (!string.IsNullOrEmpty(inventoryData))
			{
				string[] array = inventoryData.Split("|"[0]);
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split(":"[0]);
					int result = 0;
					int.TryParse(array3[0], out result);
					int result2 = 0;
					int.TryParse(array3[1], out result2);
					list = _runtimeInventory.Add(result, result2, list, false);
				}
			}
			return list;
		}

		private string CreateInventoryData(List<InvItem> invItems)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (InvItem invItem in invItems)
			{
				if (invItem != null)
				{
					stringBuilder.Append(invItem.id.ToString());
					stringBuilder.Append(":");
					stringBuilder.Append(invItem.count.ToString());
					stringBuilder.Append("|");
				}
			}
			if (invItems != null && invItems.Count > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		public static string CreateVariablesData(List<GVar> vars, bool isOptionsData, VariableLocation location)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GVar var in vars)
			{
				if ((isOptionsData && var.link == VarLink.OptionsData) || (!isOptionsData && var.link != VarLink.OptionsData) || location == VariableLocation.Local || location == VariableLocation.Component)
				{
					stringBuilder.Append(var.id.ToString());
					stringBuilder.Append(":");
					if (var.type == VariableType.String)
					{
						string textVal = var.textVal;
						textVal = AdvGame.PrepareStringForSaving(textVal);
						stringBuilder.Append(textVal);
						stringBuilder.Append(":");
						stringBuilder.Append(var.textValLineID);
					}
					else if (var.type == VariableType.Float)
					{
						stringBuilder.Append(var.floatVal.ToString());
					}
					else if (var.type == VariableType.Vector3)
					{
						string text = var.vector3Val.x + "," + var.vector3Val.y + "," + var.vector3Val.z;
						text = AdvGame.PrepareStringForSaving(text);
						stringBuilder.Append(text);
					}
					else
					{
						stringBuilder.Append(var.val.ToString());
					}
					stringBuilder.Append("|");
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		public static List<GVar> UnloadVariablesData(string data, List<GVar> existingVars, bool fromOptions = false)
		{
			if (existingVars == null)
			{
				return null;
			}
			List<GVar> list = new List<GVar>();
			foreach (GVar existingVar in existingVars)
			{
				list.Add(new GVar(existingVar));
			}
			if (string.IsNullOrEmpty(data))
			{
				return list;
			}
			string[] array = data.Split("|"[0]);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(":"[0]);
				int result = 0;
				int.TryParse(array3[0], out result);
				foreach (GVar item in list)
				{
					if (item == null || item.id != result || (fromOptions && item.link != VarLink.OptionsData))
					{
						continue;
					}
					if (item.type == VariableType.String)
					{
						string text2 = array3[1];
						text2 = AdvGame.PrepareStringForLoading(text2);
						int result2 = -1;
						if (array3.Length > 2)
						{
							int.TryParse(array3[2], out result2);
						}
						item.SetStringValue(text2, result2);
					}
					else if (item.type == VariableType.Float)
					{
						float result3 = 0f;
						float.TryParse(array3[1], out result3);
						item.SetFloatValue(result3);
					}
					else if (item.type == VariableType.Vector3)
					{
						string text3 = array3[1];
						text3 = AdvGame.PrepareStringForLoading(text3);
						Vector3 vector3Value = Vector3.zero;
						string[] array4 = text3.Split(","[0]);
						if (array4 != null && array4.Length == 3)
						{
							float result4 = 0f;
							float.TryParse(array4[0], out result4);
							float result5 = 0f;
							float.TryParse(array4[1], out result5);
							float result6 = 0f;
							float.TryParse(array4[2], out result6);
							vector3Value = new Vector3(result4, result5, result6);
						}
						item.SetVector3Value(vector3Value);
					}
					else
					{
						int result7 = 0;
						int.TryParse(array3[1], out result7);
						item.SetValue(result7);
					}
					break;
				}
			}
			return list;
		}

		public List<InvItem> GetItemsFromPlayer(int _playerID)
		{
			if (KickStarter.player.ID == _playerID)
			{
				return KickStarter.runtimeInventory.localItems;
			}
			PlayerData playerData = GetPlayerData(_playerID);
			if (playerData != null)
			{
				return AssignInventory(KickStarter.runtimeInventory, playerData.inventoryData);
			}
			return new List<InvItem>();
		}

		public void AssignItemsToPlayer(List<InvItem> invItems, int _playerID)
		{
			string inventoryData = CreateInventoryData(invItems);
			if (saveData == null)
			{
				ClearAllData();
			}
			PlayerData playerData = GetPlayerData(_playerID);
			if (playerData != null)
			{
				playerData.inventoryData = inventoryData;
				return;
			}
			Player player = KickStarter.settingsManager.GetPlayer(_playerID);
			playerData = player.SavePlayerData(new PlayerData());
			playerData.playerID = _playerID;
			playerData.inventoryData = inventoryData;
			playerData.currentScene = -1;
			saveData.playerData.Add(playerData);
		}

		public void AssignObjectivesToPlayer(string dataString, int _playerID)
		{
			if (saveData == null)
			{
				ClearAllData();
			}
			PlayerData playerData = GetPlayerData(_playerID);
			if (playerData != null)
			{
				playerData.playerObjectivesData = dataString;
				return;
			}
			Player player = KickStarter.settingsManager.GetPlayer(_playerID);
			playerData = player.SavePlayerData(new PlayerData());
			playerData.playerID = _playerID;
			playerData.playerObjectivesData = dataString;
			playerData.currentScene = -1;
			saveData.playerData.Add(playerData);
		}

		private void CustomSaveHook()
		{
			ISave[] saveHooks = GetSaveHooks(GetComponents(typeof(ISave)));
			if (saveHooks != null && saveHooks.Length > 0)
			{
				ISave[] array = saveHooks;
				foreach (ISave save in array)
				{
					save.PreSave();
				}
			}
		}

		private void CustomLoadHook()
		{
			ISave[] saveHooks = GetSaveHooks(GetComponents(typeof(ISave)));
			if (saveHooks != null && saveHooks.Length > 0)
			{
				ISave[] array = saveHooks;
				foreach (ISave save in array)
				{
					save.PostLoad();
				}
			}
		}

		private ISave[] GetSaveHooks(IList list)
		{
			ISave[] array = new ISave[list.Count];
			list.CopyTo(array, 0);
			return array;
		}

		public void RenameSave(string newLabel, int saveIndex)
		{
			if (!string.IsNullOrEmpty(newLabel))
			{
				GatherSaveFiles();
				if (foundSaveFiles.Count > saveIndex && saveIndex >= 0)
				{
					SaveFile saveFile = new SaveFile(foundSaveFiles[saveIndex]);
					saveFile.SetLabel(newLabel);
					foundSaveFiles[saveIndex] = saveFile;
					Options.UpdateSaveLabels(foundSaveFiles.ToArray());
				}
			}
		}

		public void DeleteProfile(int profileIndex = -2, bool includeActive = true)
		{
			if (!KickStarter.settingsManager.useProfiles)
			{
				return;
			}
			int num = KickStarter.options.ProfileIndexToID(profileIndex, includeActive);
			if (num == -1)
			{
				ACDebug.LogWarning("Invalid profile index: " + profileIndex + " - nothing to delete!");
				return;
			}
			if (profileIndex == -2)
			{
				num = Options.GetActiveProfileID();
			}
			DeleteProfileID(num);
		}

		public void DeleteProfileID(int profileID)
		{
			if (!KickStarter.settingsManager.useProfiles || profileID < 0)
			{
				return;
			}
			if (!Options.DoesProfileIDExist(profileID))
			{
				ACDebug.LogWarning("Cannot delete profile ID " + profileID + " as it does not exist!");
				return;
			}
			SaveFileHandler.DeleteAll(profileID);
			bool flag = ((profileID == Options.GetActiveProfileID()) ? true : false);
			Options.DeleteProfilePrefs(profileID);
			if (flag)
			{
				GatherSaveFiles();
			}
			KickStarter.playerMenus.RecalculateAll();
			ACDebug.Log("Profile ID " + profileID + " deleted.");
		}

		public static void DeleteSave(int saveID)
		{
			KickStarter.saveSystem.DeleteSave(0, saveID, true);
		}

		public void DeleteSave(int elementSlot, int saveID, bool useSaveID)
		{
			if (!useSaveID)
			{
				saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID;
			}
			foreach (SaveFile foundSaveFile in foundSaveFiles)
			{
				if (foundSaveFile.saveID == saveID)
				{
					SaveFileHandler.Delete(foundSaveFile);
				}
			}
			GatherSaveFiles();
			foreach (SaveFile foundSaveFile2 in foundSaveFiles)
			{
				if (foundSaveFile2.saveID == saveID)
				{
					foundSaveFiles.Remove(foundSaveFile2);
					Options.UpdateSaveLabels(foundSaveFiles.ToArray());
					break;
				}
			}
			if (Options.optionsData != null && Options.optionsData.lastSaveID == saveID)
			{
				Options.optionsData.lastSaveID = -1;
				Options.SavePrefs();
			}
			KickStarter.playerMenus.RecalculateAll();
		}

		public int GetNumSaves(bool includeAutoSaves = true)
		{
			int num = 0;
			foreach (SaveFile foundSaveFile in foundSaveFiles)
			{
				if (!foundSaveFile.isAutoSave || includeAutoSaves)
				{
					num++;
				}
			}
			return num;
		}

		public SaveFile GetSaveFile(int saveID)
		{
			foreach (SaveFile foundSaveFile in foundSaveFiles)
			{
				if (foundSaveFile.saveID == saveID)
				{
					return foundSaveFile;
				}
			}
			return null;
		}

		private void ResetSceneObjects()
		{
			Char[] array = Object.FindObjectsOfType(typeof(Char)) as Char[];
			Char[] array2 = array;
			foreach (Char obj in array2)
			{
				obj.AfterLoad();
			}
			Sound[] array3 = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
			Sound[] array4 = array3;
			foreach (Sound sound in array4)
			{
				if (sound != null)
				{
					sound.AfterLoad();
				}
			}
			FirstPersonCamera[] array5 = Object.FindObjectsOfType(typeof(FirstPersonCamera)) as FirstPersonCamera[];
			FirstPersonCamera[] array6 = array5;
			foreach (FirstPersonCamera firstPersonCamera in array6)
			{
				firstPersonCamera.AfterLoad();
			}
			FollowTintMap[] array7 = Object.FindObjectsOfType(typeof(FollowTintMap)) as FollowTintMap[];
			FollowTintMap[] array8 = array7;
			foreach (FollowTintMap followTintMap in array8)
			{
				followTintMap.AfterLoad();
			}
			FollowSortingMap[] array9 = Object.FindObjectsOfType(typeof(FollowSortingMap)) as FollowSortingMap[];
			FollowSortingMap[] array10 = array9;
			foreach (FollowSortingMap followSortingMap in array10)
			{
				followSortingMap.AfterLoad();
			}
			DetectHotspots[] array11 = Object.FindObjectsOfType(typeof(DetectHotspots)) as DetectHotspots[];
			DetectHotspots[] array12 = array11;
			foreach (DetectHotspots detectHotspots in array12)
			{
				detectHotspots.AfterLoad();
			}
			KickStarter.playerMenus.AfterLoad();
			KickStarter.runtimeInventory.AfterLoad();
			KickStarter.sceneChanger.AfterLoad();
			KickStarter.options.AfterLoad();
			KickStarter.kickStarter.AfterLoad();
		}
	}
}
