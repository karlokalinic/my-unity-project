using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_level_storage.html")]
	public class LevelStorage : MonoBehaviour
	{
		[HideInInspector]
		public List<SingleLevelData> allLevelData = new List<SingleLevelData>();

		public void OnAwake()
		{
			ClearAllLevelData();
		}

		public void ClearAllLevelData()
		{
			allLevelData.Clear();
			allLevelData = new List<SingleLevelData>();
		}

		public void ClearCurrentLevelData()
		{
			if (allLevelData == null)
			{
				allLevelData = new List<SingleLevelData>();
			}
			foreach (SingleLevelData allLevelDatum in allLevelData)
			{
				if (allLevelDatum.sceneNumber == UnityVersionHandler.GetCurrentSceneNumber())
				{
					allLevelData.Remove(allLevelDatum);
					break;
				}
			}
		}

		public void ReturnCurrentLevelData(bool restoringSaveFile)
		{
			SingleLevelData levelData = GetLevelData();
			if (levelData != null)
			{
				SendDataToScene(levelData, restoringSaveFile);
				AssetLoader.UnloadAssets();
			}
		}

		public void ReturnSubSceneData(SubScene subScene, bool restoringSaveFile)
		{
			SingleLevelData levelData = GetLevelData(subScene.SceneInfo.number);
			if (levelData != null)
			{
				SendDataToScene(levelData, restoringSaveFile, subScene);
				AssetLoader.UnloadAssets();
			}
		}

		private SingleLevelData GetLevelData()
		{
			return GetLevelData(UnityVersionHandler.GetCurrentSceneNumber());
		}

		private SingleLevelData GetLevelData(int sceneNumber)
		{
			if (allLevelData == null)
			{
				allLevelData = new List<SingleLevelData>();
			}
			if (allLevelData != null)
			{
				foreach (SingleLevelData allLevelDatum in allLevelData)
				{
					if (allLevelDatum.sceneNumber == sceneNumber)
					{
						return allLevelDatum;
					}
				}
			}
			return null;
		}

		public PlayerData SavePlayerData(Player player, PlayerData playerData)
		{
			List<ScriptData> list = new List<ScriptData>();
			Remember[] componentsInChildren = player.gameObject.GetComponentsInChildren<Remember>();
			Remember[] array = componentsInChildren;
			foreach (Remember remember in array)
			{
				if (remember.constantID != 0)
				{
					if (remember.retainInPrefab)
					{
						list.Add(new ScriptData(remember.constantID, remember.SaveData()));
					}
					else
					{
						ACDebug.LogWarning("Could not save GameObject " + remember.name + " because 'Retain in prefab?' is not checked!", remember);
					}
				}
				else
				{
					ACDebug.LogWarning("GameObject " + remember.name + " was not saved because its ConstantID has not been set!", remember);
				}
			}
			playerData.playerScriptData = list;
			return playerData;
		}

		public void LoadPlayerData(Player player, PlayerData playerData)
		{
			Remember[] componentsInChildren = player.gameObject.GetComponentsInChildren<Remember>();
			if (playerData.playerScriptData != null)
			{
				foreach (ScriptData playerScriptDatum in playerData.playerScriptData)
				{
					if (playerScriptDatum.data == null || playerScriptDatum.data.Length <= 0)
					{
						continue;
					}
					Remember[] array = componentsInChildren;
					foreach (Remember remember in array)
					{
						if (remember.constantID == playerScriptDatum.objectID)
						{
							remember.LoadData(playerScriptDatum.data, true);
						}
					}
				}
			}
			AssetLoader.UnloadAssets();
		}

		private void SendDataToScene(SingleLevelData levelData, bool restoringSaveFile, SubScene subScene = null)
		{
			SceneSettings sceneSettings = ((!(subScene == null)) ? subScene.SceneSettings : KickStarter.sceneSettings);
			LocalVariables localVariables = ((!(subScene == null)) ? subScene.LocalVariables : KickStarter.localVariables);
			KickStarter.actionListManager.LoadData(levelData.activeLists, subScene);
			UnloadCutsceneOnLoad(levelData.onLoadCutscene, sceneSettings);
			UnloadCutsceneOnStart(levelData.onStartCutscene, sceneSettings);
			UnloadNavMesh(levelData.navMesh, sceneSettings);
			UnloadPlayerStart(levelData.playerStart, sceneSettings);
			UnloadSortingMap(levelData.sortingMap, sceneSettings);
			UnloadTintMap(levelData.tintMap, sceneSettings);
			UnloadTransformData(levelData.allTransformData, subScene);
			foreach (ScriptData allScriptDatum in levelData.allScriptData)
			{
				if (allScriptDatum.data == null || allScriptDatum.data.Length <= 0)
				{
					continue;
				}
				Remember[] array = Serializer.returnComponents<Remember>(allScriptDatum.objectID, (!(subScene != null)) ? null : subScene.gameObject);
				Remember[] array2 = array;
				foreach (Remember remember in array2)
				{
					if (remember != null && ((subScene != null && UnityVersionHandler.ObjectIsInScene(remember.gameObject, levelData.sceneNumber, restoringSaveFile)) || (subScene == null && UnityVersionHandler.ObjectIsInActiveScene(remember.gameObject, restoringSaveFile))))
					{
						Remember[] components = remember.gameObject.GetComponents<Remember>();
						Remember[] array3 = components;
						foreach (Remember remember2 in array3)
						{
							remember2.LoadData(allScriptDatum.data, restoringSaveFile);
						}
					}
				}
			}
			localVariables.localVars = SaveSystem.UnloadVariablesData(levelData.localVariablesData, localVariables.localVars);
		}

		public void StoreCurrentLevelData()
		{
			SendSceneToData();
		}

		public void StoreAllOpenLevelData()
		{
			SendSceneToData();
			SubScene[] subScenes = KickStarter.sceneChanger.GetSubScenes();
			foreach (SubScene subScene in subScenes)
			{
				SendSceneToData(subScene);
			}
		}

		public void StoreSubSceneData(SubScene subScene)
		{
			SendSceneToData(subScene);
		}

		private void SendSceneToData(SubScene subScene = null)
		{
			SceneSettings sceneSettings = ((!(subScene == null)) ? subScene.SceneSettings : KickStarter.sceneSettings);
			LocalVariables localVariables = ((!(subScene == null)) ? subScene.LocalVariables : KickStarter.localVariables);
			List<TransformData> allTransformData = PopulateTransformData(subScene);
			List<ScriptData> allScriptData = PopulateScriptData(subScene);
			SingleLevelData singleLevelData = new SingleLevelData();
			singleLevelData.sceneNumber = ((!(subScene == null)) ? subScene.SceneInfo.number : UnityVersionHandler.GetCurrentSceneNumber());
			singleLevelData.activeLists = KickStarter.actionListManager.GetSaveData(subScene);
			if (sceneSettings != null)
			{
				if ((bool)sceneSettings.navMesh && (bool)sceneSettings.navMesh.GetComponent<ConstantID>())
				{
					singleLevelData.navMesh = Serializer.GetConstantID(sceneSettings.navMesh.gameObject);
				}
				if ((bool)sceneSettings.defaultPlayerStart && (bool)sceneSettings.defaultPlayerStart.GetComponent<ConstantID>())
				{
					singleLevelData.playerStart = Serializer.GetConstantID(sceneSettings.defaultPlayerStart.gameObject);
				}
				if ((bool)sceneSettings.sortingMap && (bool)sceneSettings.sortingMap.GetComponent<ConstantID>())
				{
					singleLevelData.sortingMap = Serializer.GetConstantID(sceneSettings.sortingMap.gameObject);
				}
				if ((bool)sceneSettings.cutsceneOnLoad && (bool)sceneSettings.cutsceneOnLoad.GetComponent<ConstantID>())
				{
					singleLevelData.onLoadCutscene = Serializer.GetConstantID(sceneSettings.cutsceneOnLoad.gameObject);
				}
				if ((bool)sceneSettings.cutsceneOnStart && (bool)sceneSettings.cutsceneOnStart.GetComponent<ConstantID>())
				{
					singleLevelData.onStartCutscene = Serializer.GetConstantID(sceneSettings.cutsceneOnStart.gameObject);
				}
				if ((bool)sceneSettings.tintMap && (bool)sceneSettings.tintMap.GetComponent<ConstantID>())
				{
					singleLevelData.tintMap = Serializer.GetConstantID(sceneSettings.tintMap.gameObject);
				}
			}
			singleLevelData.localVariablesData = SaveSystem.CreateVariablesData(localVariables.localVars, false, VariableLocation.Local);
			singleLevelData.allTransformData = allTransformData;
			singleLevelData.allScriptData = allScriptData;
			if (allLevelData == null)
			{
				allLevelData = new List<SingleLevelData>();
			}
			for (int i = 0; i < allLevelData.Count; i++)
			{
				if (allLevelData[i].DataMatchesScene(singleLevelData))
				{
					allLevelData[i] = singleLevelData;
					return;
				}
			}
			allLevelData.Add(singleLevelData);
		}

		private void UnloadNavMesh(int navMeshInt, SceneSettings sceneSettings)
		{
			NavigationMesh navigationMesh = Serializer.returnComponent<NavigationMesh>(navMeshInt, sceneSettings.gameObject);
			if ((bool)navigationMesh && (bool)sceneSettings && sceneSettings.navigationMethod != AC_NavigationMethod.UnityNavigation)
			{
				if ((bool)sceneSettings.navMesh)
				{
					NavigationMesh navMesh = sceneSettings.navMesh;
					navMesh.TurnOff();
				}
				navigationMesh.TurnOn();
				sceneSettings.navMesh = navigationMesh;
				navigationMesh.TurnOff();
				navigationMesh.TurnOn();
			}
		}

		private void UnloadPlayerStart(int playerStartInt, SceneSettings sceneSettings)
		{
			PlayerStart playerStart = Serializer.returnComponent<PlayerStart>(playerStartInt, sceneSettings.gameObject);
			if ((bool)playerStart && (bool)sceneSettings)
			{
				sceneSettings.defaultPlayerStart = playerStart;
			}
		}

		private void UnloadSortingMap(int sortingMapInt, SceneSettings sceneSettings)
		{
			SortingMap sortingMap = Serializer.returnComponent<SortingMap>(sortingMapInt, sceneSettings.gameObject);
			if ((bool)sortingMap && (bool)sceneSettings)
			{
				sceneSettings.sortingMap = sortingMap;
				KickStarter.sceneSettings.UpdateAllSortingMaps();
			}
		}

		private void UnloadTintMap(int tintMapInt, SceneSettings sceneSettings)
		{
			TintMap tintMap = Serializer.returnComponent<TintMap>(tintMapInt, sceneSettings.gameObject);
			if ((bool)tintMap && (bool)sceneSettings)
			{
				sceneSettings.tintMap = tintMap;
				FollowTintMap[] array = Object.FindObjectsOfType(typeof(FollowTintMap)) as FollowTintMap[];
				FollowTintMap[] array2 = array;
				foreach (FollowTintMap followTintMap in array2)
				{
					followTintMap.ResetTintMap();
				}
			}
		}

		private void UnloadCutsceneOnLoad(int cutsceneInt, SceneSettings sceneSettings)
		{
			Cutscene cutscene = Serializer.returnComponent<Cutscene>(cutsceneInt, sceneSettings.gameObject);
			if ((bool)cutscene && (bool)sceneSettings)
			{
				sceneSettings.cutsceneOnLoad = cutscene;
			}
		}

		private void UnloadCutsceneOnStart(int cutsceneInt, SceneSettings sceneSettings)
		{
			Cutscene cutscene = Serializer.returnComponent<Cutscene>(cutsceneInt, sceneSettings.gameObject);
			if ((bool)cutscene && (bool)sceneSettings)
			{
				sceneSettings.cutsceneOnStart = cutscene;
			}
		}

		private List<TransformData> PopulateTransformData(SubScene subScene)
		{
			List<TransformData> list = new List<TransformData>();
			RememberTransform[] ownSceneComponents = UnityVersionHandler.GetOwnSceneComponents<RememberTransform>((!(subScene != null)) ? null : subScene.gameObject);
			RememberTransform[] array = ownSceneComponents;
			foreach (RememberTransform rememberTransform in array)
			{
				if (rememberTransform.constantID != 0)
				{
					list.Add(rememberTransform.SaveTransformData());
				}
				else
				{
					ACDebug.LogWarning("GameObject " + rememberTransform.name + " was not saved because its ConstantID has not been set!", rememberTransform);
				}
			}
			return list;
		}

		private void UnloadTransformData(List<TransformData> _transforms, SubScene subScene)
		{
			RememberTransform[] ownSceneComponents = UnityVersionHandler.GetOwnSceneComponents<RememberTransform>((!(subScene != null)) ? null : subScene.gameObject);
			RememberTransform[] array = ownSceneComponents;
			foreach (RememberTransform rememberTransform in array)
			{
				if (!rememberTransform.saveScenePresence)
				{
					continue;
				}
				bool flag = false;
				foreach (TransformData _transform in _transforms)
				{
					if (_transform.objectID == rememberTransform.constantID)
					{
						flag = !_transform.savePrevented;
					}
				}
				if (!flag)
				{
					KickStarter.sceneChanger.ScheduleForDeletion(rememberTransform.gameObject);
				}
			}
			Object[] array2 = Resources.LoadAll("SaveableData/Prefabs", typeof(GameObject));
			if (array2 == null || array2.Length == 0)
			{
				array2 = Resources.LoadAll(string.Empty, typeof(GameObject));
			}
			foreach (TransformData _transform2 in _transforms)
			{
				RememberTransform rememberTransform2 = Serializer.returnComponent<RememberTransform>(_transform2.objectID, (!(subScene != null)) ? null : subScene.gameObject);
				if (rememberTransform2 == null && _transform2.bringBack && !_transform2.savePrevented)
				{
					bool flag2 = false;
					Object[] array3 = array2;
					foreach (Object obj in array3)
					{
						if (!(obj is GameObject))
						{
							continue;
						}
						GameObject gameObject = (GameObject)obj;
						if (!gameObject.GetComponent<RememberTransform>())
						{
							continue;
						}
						int constantID = gameObject.GetComponent<ConstantID>().constantID;
						if ((_transform2.linkedPrefabID == 0 || constantID != _transform2.linkedPrefabID) && (_transform2.linkedPrefabID != 0 || constantID != _transform2.objectID))
						{
							continue;
						}
						GameObject gameObject2 = Object.Instantiate(gameObject);
						gameObject2.name = gameObject.name;
						rememberTransform2 = gameObject2.GetComponent<RememberTransform>();
						flag2 = true;
						if (_transform2.linkedPrefabID != 0 && constantID == _transform2.linkedPrefabID)
						{
							ConstantID[] components = rememberTransform2.GetComponents<ConstantID>();
							ConstantID[] array4 = components;
							foreach (ConstantID constantID2 in array4)
							{
								constantID2.constantID = _transform2.objectID;
							}
						}
						break;
					}
					if (!flag2)
					{
						ACDebug.LogWarning("Could not find Resources prefab with ID " + _transform2.objectID + " - is it placed in a Resources folder?");
					}
				}
				if (rememberTransform2 != null)
				{
					rememberTransform2.LoadTransformData(_transform2);
				}
			}
			Resources.UnloadUnusedAssets();
			KickStarter.stateHandler.IgnoreNavMeshCollisions();
		}

		private List<ScriptData> PopulateScriptData(SubScene subScene)
		{
			List<ScriptData> list = new List<ScriptData>();
			Remember[] ownSceneComponents = UnityVersionHandler.GetOwnSceneComponents<Remember>((!(subScene != null)) ? null : subScene.gameObject);
			Remember[] array = ownSceneComponents;
			foreach (Remember remember in array)
			{
				Player componentInParent = remember.gameObject.GetComponentInParent<Player>();
				if (!(componentInParent != null) || componentInParent.IsLocalPlayer())
				{
					if (remember.constantID != 0)
					{
						list.Add(new ScriptData(remember.constantID, remember.SaveData()));
					}
					else
					{
						ACDebug.LogWarning("GameObject " + remember.name + " was not saved because its ConstantID has not been set!", remember);
					}
				}
			}
			return list;
		}

		private void AssignMenuLocks(List<Menu> menus, string menuLockData)
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
	}
}
