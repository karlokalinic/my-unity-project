using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AC
{
	public class UnityVersionHandler
	{
		public static bool CursorLock
		{
			get
			{
				return Cursor.lockState == CursorLockMode.Locked;
			}
			set
			{
				if (value)
				{
					if (KickStarter.cursorManager.cursorRendering != CursorRendering.Software || KickStarter.cursorManager.lockSystemCursor)
					{
						Cursor.lockState = CursorLockMode.Locked;
					}
				}
				else if (KickStarter.cursorManager.confineSystemCursor)
				{
					Cursor.lockState = CursorLockMode.Confined;
				}
				else
				{
					Cursor.lockState = CursorLockMode.None;
				}
			}
		}

		public static RaycastHit2D Perform2DRaycast(Vector2 origin, Vector2 direction, float length, LayerMask layerMask)
		{
			RaycastHit2D[] array = new RaycastHit2D[1];
			ContactFilter2D contactFilter = default(ContactFilter2D);
			contactFilter.useTriggers = true;
			contactFilter.SetLayerMask(layerMask);
			contactFilter.ClearDepth();
			Physics2D.Raycast(origin, direction, contactFilter, array, length);
			return array[0];
		}

		public static RaycastHit2D Perform2DRaycast(Vector2 origin, Vector2 direction, float length)
		{
			RaycastHit2D[] array = new RaycastHit2D[1];
			ContactFilter2D contactFilter = default(ContactFilter2D);
			contactFilter.useTriggers = true;
			contactFilter.ClearDepth();
			Physics2D.Raycast(origin, direction, contactFilter, array, length);
			return array[0];
		}

		public static int GetCurrentSceneNumber()
		{
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		}

		public static SceneInfo GetSceneInfoFromGameObject(GameObject _gameObject)
		{
			return new SceneInfo(_gameObject.scene.name, _gameObject.scene.buildIndex);
		}

		public static LocalVariables GetLocalVariablesOfGameObject(GameObject _gameObject)
		{
			if (ObjectIsInActiveScene(_gameObject))
			{
				return KickStarter.localVariables;
			}
			Scene scene = _gameObject.scene;
			if (Application.isPlaying)
			{
				SubScene[] subScenes = KickStarter.sceneChanger.GetSubScenes();
				foreach (SubScene subScene in subScenes)
				{
					if (subScene.gameObject.scene == scene)
					{
						return subScene.LocalVariables;
					}
				}
			}
			else
			{
				LocalVariables[] array = UnityEngine.Object.FindObjectsOfType<LocalVariables>();
				foreach (LocalVariables localVariables in array)
				{
					if (localVariables.gameObject.scene == scene)
					{
						return localVariables;
					}
				}
			}
			return null;
		}

		public static SceneSettings GetSceneSettingsOfGameObject(GameObject _gameObject)
		{
			if (ObjectIsInActiveScene(_gameObject))
			{
				return KickStarter.sceneSettings;
			}
			Scene scene = _gameObject.scene;
			if (Application.isPlaying)
			{
				SubScene[] subScenes = KickStarter.sceneChanger.GetSubScenes();
				foreach (SubScene subScene in subScenes)
				{
					if (subScene.gameObject.scene == scene)
					{
						return subScene.SceneSettings;
					}
				}
			}
			else
			{
				SceneSettings[] array = UnityEngine.Object.FindObjectsOfType<SceneSettings>();
				foreach (SceneSettings sceneSettings in array)
				{
					if (sceneSettings.gameObject.scene == scene)
					{
						return sceneSettings;
					}
				}
			}
			return null;
		}

		public static string GetCurrentSceneName()
		{
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		}

		public static SceneInfo GetCurrentSceneInfo()
		{
			return new SceneInfo(GetCurrentSceneName(), GetCurrentSceneNumber());
		}

		public static void OpenScene(string sceneName, bool forceReload = false, bool loadAdditively = false)
		{
			if (string.IsNullOrEmpty(sceneName))
			{
				return;
			}
			try
			{
				if (forceReload || GetCurrentSceneName() != sceneName)
				{
					LoadSceneMode mode = (loadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single);
					UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Error when opening scene " + sceneName + ": " + ex);
			}
		}

		public static void OpenScene(int sceneNumber, bool forceReload = false, bool loadAdditively = false)
		{
			if (sceneNumber < 0)
			{
				return;
			}
			if (KickStarter.settingsManager.reloadSceneWhenLoading)
			{
				forceReload = true;
			}
			try
			{
				if (forceReload || GetCurrentSceneNumber() != sceneNumber)
				{
					LoadSceneMode mode = (loadAdditively ? LoadSceneMode.Additive : LoadSceneMode.Single);
					UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNumber, mode);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Error when opening scene " + sceneNumber + ": " + ex);
			}
		}

		public static bool CloseScene(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName))
			{
				return false;
			}
			if (GetCurrentSceneName() != sceneName)
			{
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
				return true;
			}
			return false;
		}

		public static bool CloseScene(int sceneNumber)
		{
			if (sceneNumber < 0)
			{
				return false;
			}
			if (GetCurrentSceneNumber() != sceneNumber)
			{
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneNumber);
				return true;
			}
			return false;
		}

		public static AsyncOperation LoadLevelAsync(int sceneNumber, string sceneName = "")
		{
			if (!string.IsNullOrEmpty(sceneName))
			{
				return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
			}
			return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNumber);
		}

		public static bool CanUseJson()
		{
			return true;
		}

		public static bool PutInFolder(GameObject ob, string folderName)
		{
			if (ob == null || string.IsNullOrEmpty(folderName))
			{
				return false;
			}
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
			UnityEngine.Object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				GameObject gameObject = (GameObject)array2[i];
				if (gameObject.name == folderName && gameObject.transform.position == Vector3.zero && folderName.Contains("_") && gameObject.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
				{
					ob.transform.parent = gameObject.transform;
					return true;
				}
			}
			return false;
		}

		public static bool ObjectIsInActiveScene(GameObject gameObject, bool persistentIsValid = true)
		{
			if (gameObject == null)
			{
				return false;
			}
			Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			if (gameObject.scene == activeScene)
			{
				return true;
			}
			if (persistentIsValid && GameObjectIsPersistent(gameObject))
			{
				return true;
			}
			return false;
		}

		public static bool GameObjectIsPersistent(GameObject gameObject)
		{
			if (gameObject.scene.name == "DontDestroyOnLoad" || (gameObject.scene.name == null && gameObject.scene.buildIndex == -1))
			{
				return true;
			}
			return false;
		}

		public static bool ObjectIsInScene(GameObject gameObject, int sceneIndex, bool persistentIsValid = true)
		{
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.scene.buildIndex == sceneIndex)
			{
				return true;
			}
			if (persistentIsValid && GameObjectIsPersistent(gameObject))
			{
				return true;
			}
			return false;
		}

		public static bool ObjectIsInScene(GameObject gameObject, string sceneName, bool persistentIsValid = true)
		{
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.scene.name == sceneName)
			{
				return true;
			}
			if (persistentIsValid && GameObjectIsPersistent(gameObject))
			{
				return true;
			}
			return false;
		}

		public static T GetKickStarterComponent<T>() where T : Behaviour
		{
			if ((bool)UnityEngine.Object.FindObjectOfType<T>())
			{
				return UnityEngine.Object.FindObjectOfType<T>();
			}
			return (T)null;
		}

		public static T GetOwnSceneInstance<T>(GameObject gameObject) where T : Behaviour
		{
			Scene scene = gameObject.scene;
			T[] array = UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
			T[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				T val = array2[i];
				if (val != null && val.gameObject.scene == scene)
				{
					return val;
				}
			}
			return (T)null;
		}

		public static T[] GetOwnSceneComponents<T>(GameObject gameObject = null) where T : Component
		{
			T[] array = UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
			bool flag = gameObject != null;
			List<T> list = new List<T>();
			T[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				T val = array2[i];
				if (!val)
				{
					continue;
				}
				if (flag)
				{
					if (val.gameObject.scene == gameObject.scene)
					{
						list.Add(val);
					}
				}
				else if (ObjectIsInActiveScene(val.gameObject))
				{
					list.Add(val);
				}
			}
			return list.ToArray();
		}
	}
}
