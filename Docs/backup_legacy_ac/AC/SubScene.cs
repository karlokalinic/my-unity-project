using UnityEngine;
using UnityEngine.SceneManagement;

namespace AC
{
	public class SubScene : MonoBehaviour
	{
		protected SceneInfo sceneInfo;

		protected LocalVariables localVariables;

		protected SceneSettings sceneSettings;

		protected KickStarter kickStarter;

		protected MainCamera mainCamera;

		public SceneInfo SceneInfo
		{
			get
			{
				return sceneInfo;
			}
		}

		public LocalVariables LocalVariables
		{
			get
			{
				return localVariables;
			}
		}

		public SceneSettings SceneSettings
		{
			get
			{
				return sceneSettings;
			}
		}

		public void Initialise(MultiSceneChecker _multiSceneChecker)
		{
			Scene scene = _multiSceneChecker.gameObject.scene;
			base.gameObject.name = "SubScene " + scene.buildIndex;
			kickStarter = _multiSceneChecker.GetComponent<KickStarter>();
			sceneInfo = new SceneInfo(scene.name, scene.buildIndex);
			localVariables = _multiSceneChecker.GetComponent<LocalVariables>();
			sceneSettings = _multiSceneChecker.GetComponent<SceneSettings>();
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(base.gameObject, scene);
			kickStarter = UnityVersionHandler.GetOwnSceneInstance<KickStarter>(base.gameObject);
			if (kickStarter != null)
			{
				kickStarter.gameObject.SetActive(false);
			}
			mainCamera = UnityVersionHandler.GetOwnSceneInstance<MainCamera>(base.gameObject);
			if (mainCamera != null)
			{
				mainCamera.gameObject.SetActive(false);
			}
			Player ownSceneInstance = UnityVersionHandler.GetOwnSceneInstance<Player>(base.gameObject);
			if (ownSceneInstance != null)
			{
				ownSceneInstance.gameObject.SetActive(false);
			}
			if (sceneSettings.OverridesCameraPerspective())
			{
				ACDebug.LogError("The added scene (" + scene.name + ", " + scene.buildIndex + ") overrides the default camera perspective - this feature should not be used in conjunction with multiple-open scenes.", base.gameObject);
			}
			KickStarter.sceneChanger.RegisterSubScene(this);
		}

		public void MakeMain()
		{
			if ((bool)mainCamera)
			{
				mainCamera.gameObject.SetActive(true);
				mainCamera.OnAwake(false);
				mainCamera.OnStart();
			}
			if ((bool)kickStarter)
			{
				kickStarter.gameObject.SetActive(true);
			}
			KickStarter.SetGameEngine(base.gameObject);
			KickStarter.mainCamera = mainCamera;
			UnityEngine.SceneManagement.SceneManager.SetActiveScene(base.gameObject.scene);
			Object.Destroy(base.gameObject);
		}
	}
}
