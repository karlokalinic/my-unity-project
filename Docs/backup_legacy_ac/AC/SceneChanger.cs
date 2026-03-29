using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_scene_changer.html")]
	public class SceneChanger : MonoBehaviour
	{
		protected SceneInfo previousSceneInfo;

		protected SceneInfo previousGlobalSceneInfo;

		protected List<SubScene> subScenes = new List<SubScene>();

		protected Vector3 relativePosition;

		protected AsyncOperation preloadAsync;

		protected SceneInfo preloadSceneInfo;

		protected SceneInfo thisSceneInfo;

		protected Player playerOnTransition;

		protected Texture2D textureOnTransition;

		protected bool isLoading;

		protected float loadingProgress;

		protected bool takeNPCPosition;

		protected Vector2 simulaterCursorPositionOnExit;

		protected bool completeSceneActivation;

		public void OnAwake()
		{
			previousSceneInfo = new SceneInfo(string.Empty, -1);
			previousGlobalSceneInfo = new SceneInfo(string.Empty, -1);
			relativePosition = Vector3.zero;
			isLoading = false;
			AssignThisSceneInfo();
		}

		public void AfterLoad()
		{
			loadingProgress = 0f;
			AssignThisSceneInfo();
			if (KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
			{
				KickStarter.playerInput.SetSimulatedCursorPosition(simulaterCursorPositionOnExit);
			}
		}

		public void SetRelativePosition(Transform markerTransform)
		{
			if (KickStarter.player == null || markerTransform == null)
			{
				relativePosition = Vector2.zero;
				return;
			}
			relativePosition = KickStarter.player.transform.position - markerTransform.position;
			if (SceneSettings.IsUnity2D())
			{
				relativePosition.z = 0f;
			}
			else if (SceneSettings.IsTopDown())
			{
				relativePosition.y = 0f;
			}
		}

		public virtual Vector3 GetStartPosition(Vector3 playerStartPosition)
		{
			Vector3 result = playerStartPosition + relativePosition;
			relativePosition = Vector2.zero;
			return result;
		}

		public float GetLoadingProgress()
		{
			if (KickStarter.settingsManager.useAsyncLoading)
			{
				return loadingProgress;
			}
			ACDebug.LogWarning("Cannot get the loading progress because asynchronous loading is not enabled in the Settings Manager.");
			return 0f;
		}

		public bool IsLoading()
		{
			return isLoading;
		}

		public void PreloadScene(SceneInfo nextSceneInfo)
		{
			if (preloadSceneInfo != null && preloadSceneInfo.Matches(nextSceneInfo))
			{
				ACDebug.Log("Skipping preload of scene '" + nextSceneInfo.GetLabel() + "' - already preloaded.");
			}
			else
			{
				StartCoroutine(PreloadLevelAsync(nextSceneInfo));
			}
		}

		public bool ChangeScene(SceneInfo nextSceneInfo, bool saveRoomData, bool forceReload = false, bool _takeNPCPosition = false)
		{
			takeNPCPosition = false;
			if (!isLoading)
			{
				if (!nextSceneInfo.Matches(thisSceneInfo) || forceReload)
				{
					if (preloadSceneInfo != null && preloadSceneInfo.IsValid() && !preloadSceneInfo.Matches(nextSceneInfo))
					{
						ACDebug.LogWarning("Opening scene '" + nextSceneInfo.GetLabel() + "', but have preloaded scene '" + preloadSceneInfo.GetLabel() + "'.  Preloaded data must be scrapped.");
						if (preloadAsync != null)
						{
							preloadAsync.allowSceneActivation = true;
						}
						preloadSceneInfo = null;
					}
					takeNPCPosition = _takeNPCPosition;
					PrepareSceneForExit(!KickStarter.settingsManager.useAsyncLoading, saveRoomData);
					LoadLevel(nextSceneInfo, KickStarter.settingsManager.useLoadingScreen, KickStarter.settingsManager.useAsyncLoading, forceReload);
					return true;
				}
			}
			else
			{
				ACDebug.LogWarning("Cannot switch scene while another scene-loading operation is underway.");
			}
			return false;
		}

		public void LoadPreviousScene()
		{
			if (previousSceneInfo != null && !previousSceneInfo.IsNull)
			{
				ChangeScene(previousSceneInfo, true);
			}
			else
			{
				ACDebug.LogWarning("Cannot load previous scene - no scene data present!");
			}
		}

		public Player GetPlayerOnTransition()
		{
			return playerOnTransition;
		}

		public void DestroyOldPlayer()
		{
			if ((bool)playerOnTransition)
			{
				ACDebug.Log("New player found - " + playerOnTransition.name + " deleted");
				Object.DestroyImmediate(playerOnTransition.gameObject);
			}
		}

		public void SetTransitionTexture(Texture2D _texture)
		{
			textureOnTransition = _texture;
		}

		public Texture2D GetAndResetTransitionTexture()
		{
			Texture2D result = textureOnTransition;
			textureOnTransition = null;
			return result;
		}

		public void ScheduleForDeletion(GameObject _gameObject)
		{
			if ((bool)_gameObject.GetComponentInChildren<ActionList>())
			{
				ActionList component = _gameObject.GetComponent<ActionList>();
				if (component != null && KickStarter.actionListManager.IsListRunning(component))
				{
					component.Kill();
					ACDebug.LogWarning("The ActionList '" + component.name + "' is being removed from the scene while running!  Killing it now to prevent hanging.");
				}
			}
			StartCoroutine(ScheduleForDeletionCoroutine(_gameObject));
		}

		public void PrepareSceneForExit()
		{
			PrepareSceneForExit(false, true);
		}

		protected void AssignThisSceneInfo()
		{
			thisSceneInfo = new SceneInfo(UnityVersionHandler.GetCurrentSceneName(), UnityVersionHandler.GetCurrentSceneNumber());
		}

		protected IEnumerator ScheduleForDeletionCoroutine(GameObject _gameObject)
		{
			yield return new WaitForEndOfFrame();
			Object.DestroyImmediate(_gameObject);
		}

		protected void LoadLevel(SceneInfo nextSceneInfo, bool useLoadingScreen, bool useAsyncLoading, bool forceReload = false)
		{
			if (useLoadingScreen)
			{
				StartCoroutine(LoadLoadingScreen(nextSceneInfo, new SceneInfo(KickStarter.settingsManager.loadingSceneIs, KickStarter.settingsManager.loadingSceneName, KickStarter.settingsManager.loadingScene), useAsyncLoading));
			}
			else if (useAsyncLoading && !forceReload)
			{
				StartCoroutine(LoadLevelAsync(nextSceneInfo));
			}
			else
			{
				StartCoroutine(LoadLevelCo(nextSceneInfo, forceReload));
			}
		}

		protected IEnumerator LoadLoadingScreen(SceneInfo nextSceneInfo, SceneInfo loadingSceneInfo, bool loadAsynchronously = false)
		{
			if (preloadSceneInfo != null && !preloadSceneInfo.IsNull)
			{
				ACDebug.LogWarning("Cannot use preloaded scene '" + preloadSceneInfo.GetLabel() + "' because the loading scene overrides it - discarding preloaded data.");
			}
			preloadAsync = null;
			preloadSceneInfo = new SceneInfo(string.Empty, -1);
			isLoading = true;
			loadingProgress = 0f;
			loadingSceneInfo.LoadLevel();
			yield return null;
			if (KickStarter.player != null)
			{
				KickStarter.player.transform.position += new Vector3(0f, -10000f, 0f);
			}
			PrepareSceneForExit(true, false);
			if (loadAsynchronously)
			{
				if (KickStarter.settingsManager.loadingDelay > 0f)
				{
					float waitForTime = Time.realtimeSinceStartup + KickStarter.settingsManager.loadingDelay;
					while (Time.realtimeSinceStartup < waitForTime && KickStarter.settingsManager.loadingDelay > 0f)
					{
						yield return null;
					}
				}
				AsyncOperation aSync = nextSceneInfo.LoadLevelASync();
				aSync.allowSceneActivation = false;
				while (aSync.progress < 0.9f)
				{
					loadingProgress = aSync.progress;
					yield return null;
				}
				loadingProgress = 1f;
				isLoading = false;
				if (KickStarter.settingsManager.manualSceneActivation)
				{
					if (KickStarter.eventManager != null)
					{
						completeSceneActivation = false;
						KickStarter.eventManager.Call_OnAwaitSceneActivation(nextSceneInfo);
					}
					while (!completeSceneActivation)
					{
						yield return null;
					}
					completeSceneActivation = false;
				}
				if (KickStarter.settingsManager.loadingDelay > 0f)
				{
					float waitForTime2 = Time.realtimeSinceStartup + KickStarter.settingsManager.loadingDelay;
					while (Time.realtimeSinceStartup < waitForTime2 && KickStarter.settingsManager.loadingDelay > 0f)
					{
						yield return null;
					}
				}
				aSync.allowSceneActivation = true;
				KickStarter.stateHandler.IgnoreNavMeshCollisions();
			}
			else
			{
				nextSceneInfo.LoadLevel();
			}
			isLoading = false;
			StartCoroutine(OnCompleteSceneChange());
		}

		public void ActivateLoadedScene()
		{
			completeSceneActivation = true;
		}

		protected IEnumerator LoadLevelAsync(SceneInfo nextSceneInfo)
		{
			isLoading = true;
			loadingProgress = 0f;
			PrepareSceneForExit(true, false);
			AsyncOperation aSync = null;
			if (nextSceneInfo.Matches(preloadSceneInfo))
			{
				aSync = preloadAsync;
				aSync.allowSceneActivation = true;
				while (!aSync.isDone)
				{
					loadingProgress = aSync.progress;
					yield return null;
				}
				loadingProgress = 1f;
			}
			else
			{
				aSync = nextSceneInfo.LoadLevelASync();
				aSync.allowSceneActivation = false;
				while (aSync.progress < 0.9f)
				{
					loadingProgress = aSync.progress;
					yield return null;
				}
				loadingProgress = 1f;
				isLoading = false;
				if (KickStarter.settingsManager.manualSceneActivation)
				{
					if (KickStarter.eventManager != null)
					{
						completeSceneActivation = false;
						KickStarter.eventManager.Call_OnAwaitSceneActivation(nextSceneInfo);
					}
					while (!completeSceneActivation)
					{
						yield return null;
					}
					completeSceneActivation = false;
				}
				yield return new WaitForEndOfFrame();
				aSync.allowSceneActivation = true;
			}
			KickStarter.stateHandler.IgnoreNavMeshCollisions();
			isLoading = false;
			preloadAsync = null;
			preloadSceneInfo = new SceneInfo(string.Empty, -1);
			StartCoroutine(OnCompleteSceneChange());
		}

		protected IEnumerator PreloadLevelAsync(SceneInfo nextSceneInfo)
		{
			while (isLoading)
			{
				yield return null;
			}
			loadingProgress = 0f;
			preloadSceneInfo = nextSceneInfo;
			preloadAsync = nextSceneInfo.LoadLevelASync();
			preloadAsync.allowSceneActivation = false;
			while (!preloadAsync.isDone)
			{
				loadingProgress = preloadAsync.progress;
				if (loadingProgress >= 0.9f)
				{
					break;
				}
				loadingProgress = 1f;
				yield return null;
			}
			if (KickStarter.eventManager != null)
			{
				KickStarter.eventManager.Call_OnCompleteScenePreload(nextSceneInfo);
			}
		}

		protected IEnumerator LoadLevelCo(SceneInfo nextSceneInfo, bool forceReload = false)
		{
			isLoading = true;
			yield return new WaitForEndOfFrame();
			nextSceneInfo.LoadLevel(forceReload);
			isLoading = false;
			StartCoroutine(OnCompleteSceneChange());
		}

		protected IEnumerator OnCompleteSceneChange()
		{
			bool _takeNPCPosition = takeNPCPosition;
			takeNPCPosition = false;
			yield return new WaitForEndOfFrame();
			if (_takeNPCPosition)
			{
				NPC runtimeAssociatedNPC = KickStarter.player.GetRuntimeAssociatedNPC();
				if (runtimeAssociatedNPC != null)
				{
					KickStarter.player.RepositionToTransform(runtimeAssociatedNPC.transform);
				}
			}
		}

		protected virtual void PrepareSceneForExit(bool isInstant, bool saveRoomData)
		{
			if (isInstant)
			{
				KickStarter.mainCamera.FadeOut(0f);
				if ((bool)KickStarter.player)
				{
					KickStarter.player.EndPath();
					KickStarter.player.Halt(false);
				}
				KickStarter.stateHandler.gameState = GameState.Normal;
			}
			if (KickStarter.dialog != null)
			{
				KickStarter.dialog.KillDialog(true, true, SpeechMenuLimit.All, SpeechMenuType.All, string.Empty);
			}
			Sound[] array = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
			Sound[] array2 = array;
			foreach (Sound sound in array2)
			{
				sound.TryDestroy();
			}
			KickStarter.playerMenus.ClearParents();
			if (saveRoomData)
			{
				KickStarter.levelStorage.StoreAllOpenLevelData();
				previousSceneInfo = new SceneInfo();
				previousGlobalSceneInfo = new SceneInfo();
				KickStarter.saveSystem.SaveCurrentPlayerData();
			}
			subScenes.Clear();
			playerOnTransition = KickStarter.player;
			if (KickStarter.playerInput != null)
			{
				simulaterCursorPositionOnExit = KickStarter.playerInput.GetMousePosition();
			}
			if (KickStarter.eventManager != null)
			{
				KickStarter.eventManager.Call_OnBeforeChangeScene();
			}
		}

		public bool AddSubScene(SceneInfo sceneInfo)
		{
			if (sceneInfo.Matches(thisSceneInfo))
			{
				return false;
			}
			foreach (SubScene subScene in subScenes)
			{
				if (subScene.SceneInfo.Matches(sceneInfo))
				{
					return false;
				}
			}
			sceneInfo.AddLevel();
			KickStarter.playerMenus.AfterSceneAdd();
			return true;
		}

		public IEnumerator AddSubSceneCoroutine(SceneInfo sceneInfo)
		{
			yield return new WaitForEndOfFrame();
			AddSubScene(sceneInfo);
		}

		public void RegisterSubScene(SubScene subScene)
		{
			if (!subScenes.Contains(subScene))
			{
				subScenes.Add(subScene);
				KickStarter.levelStorage.ReturnSubSceneData(subScene, isLoading);
				KickStarter.stateHandler.IgnoreNavMeshCollisions();
			}
		}

		public SubScene[] GetSubScenes()
		{
			return subScenes.ToArray();
		}

		public bool RemoveScene(SceneInfo sceneInfo)
		{
			KickStarter.actionListManager.KillAllFromScene(sceneInfo);
			if (thisSceneInfo.Matches(sceneInfo))
			{
				if (subScenes == null || subScenes.Count == 0)
				{
					ACDebug.LogWarning("Cannot remove scene " + sceneInfo.number + ", as it is the only one open!");
					return false;
				}
				KickStarter.levelStorage.StoreCurrentLevelData();
				SubScene subScene = subScenes[subScenes.Count - 1];
				KickStarter.mainCamera.gameObject.SetActive(false);
				subScene.MakeMain();
				subScenes.Remove(subScene);
				StartCoroutine(CloseScene(thisSceneInfo));
				thisSceneInfo = subScene.SceneInfo;
				return true;
			}
			for (int i = 0; i < subScenes.Count; i++)
			{
				if (subScenes[i].SceneInfo.Matches(sceneInfo))
				{
					KickStarter.levelStorage.StoreSubSceneData(subScenes[i]);
					StartCoroutine(CloseScene(subScenes[i].SceneInfo));
					subScenes.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		protected IEnumerator CloseScene(SceneInfo _sceneInfo)
		{
			yield return new WaitForEndOfFrame();
			_sceneInfo.CloseLevel();
			yield return new WaitForEndOfFrame();
			KickStarter.stateHandler.RegisterWithGameEngine();
		}

		public PlayerData SavePlayerData(PlayerData playerData)
		{
			playerData.previousScene = previousSceneInfo.number;
			playerData.previousSceneName = previousSceneInfo.name;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SubScene subScene in subScenes)
			{
				stringBuilder.Append(subScene.SceneInfo.name + ":" + subScene.SceneInfo.number + "|");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			playerData.openSubScenes = stringBuilder.ToString();
			return playerData;
		}

		public void LoadPlayerData(PlayerData playerData, bool loadSubScenes = true)
		{
			previousSceneInfo = new SceneInfo(playerData.previousSceneName, playerData.previousScene);
			foreach (SubScene subScene in subScenes)
			{
				subScene.SceneInfo.CloseLevel();
			}
			subScenes.Clear();
			if (loadSubScenes && playerData.openSubScenes != null && playerData.openSubScenes.Length > 0)
			{
				string[] array = playerData.openSubScenes.Split("|"[0]);
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split(":"[0]);
					int result = 0;
					int.TryParse(array3[0], out result);
					SceneInfo sceneInfo = new SceneInfo(array3[0], result);
					StartCoroutine(AddSubSceneCoroutine(sceneInfo));
				}
			}
			KickStarter.stateHandler.RegisterWithGameEngine();
		}

		public SceneInfo GetPreviousSceneInfo(bool forPlayer = true)
		{
			if (forPlayer)
			{
				return previousSceneInfo;
			}
			return previousGlobalSceneInfo;
		}

		public int GetSubSceneIndexOfGameObject(GameObject gameObject)
		{
			if (gameObject != null && subScenes != null && subScenes.Count > 0)
			{
				for (int i = 0; i < subScenes.Count; i++)
				{
					if (gameObject.scene == subScenes[i].SceneSettings.gameObject.scene)
					{
						return i + 1;
					}
				}
			}
			return 0;
		}
	}
}
