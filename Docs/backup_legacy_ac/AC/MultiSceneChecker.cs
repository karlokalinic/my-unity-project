using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_multi_scene_checker.html")]
	public class MultiSceneChecker : MonoBehaviour
	{
		protected KickStarter ownKickStarter;

		protected void Awake()
		{
			if (!UnityVersionHandler.ObjectIsInActiveScene(base.gameObject))
			{
				GameObject gameObject = new GameObject();
				SubScene subScene = gameObject.AddComponent<SubScene>();
				subScene.Initialise(this);
				return;
			}
			ownKickStarter = GetComponent<KickStarter>();
			GameObject gameObject2 = GameObject.FindWithTag("MainCamera");
			if (gameObject2 == null)
			{
				ACDebug.LogError("No MainCamera found - please click 'Organise room objects' in the Scene Manager to create one.");
			}
			else if (gameObject2.GetComponent<MainCamera>() == null && gameObject2.GetComponentInParent<MainCamera>() == null)
			{
				ACDebug.LogError("MainCamera has no MainCamera component.", gameObject2);
			}
			if (ownKickStarter != null)
			{
				KickStarter.mainCamera.OnAwake();
				ownKickStarter.OnAwake();
				KickStarter.playerInput.OnAwake();
				KickStarter.playerQTE.OnAwake();
				KickStarter.sceneSettings.OnAwake();
				KickStarter.dialog.OnAwake();
				KickStarter.navigationManager.OnAwake();
				KickStarter.actionListManager.OnAwake();
				KickStarter.stateHandler.RegisterWithGameEngine();
			}
			else
			{
				ACDebug.LogError("No KickStarter component found in the scene!", base.gameObject);
			}
		}

		protected void Start()
		{
			if (UnityVersionHandler.ObjectIsInActiveScene(base.gameObject) && ownKickStarter != null)
			{
				KickStarter.sceneSettings.OnStart();
				KickStarter.playerMovement.OnStart();
				KickStarter.mainCamera.OnStart();
			}
		}
	}
}
