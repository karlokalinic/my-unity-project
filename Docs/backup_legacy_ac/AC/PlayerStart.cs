using UnityEngine;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_player_start.html")]
	public class PlayerStart : Marker
	{
		public ChooseSceneBy chooseSceneBy;

		public int previousScene;

		public string previousSceneName;

		public bool fadeInOnStart;

		public float fadeSpeed = 0.5f;

		public _Camera cameraOnStart;

		protected GameObject playerOb;

		public void SetPlayerStart()
		{
			if (!KickStarter.mainCamera)
			{
				return;
			}
			if (fadeInOnStart)
			{
				KickStarter.mainCamera.FadeIn(fadeSpeed);
			}
			if (!KickStarter.settingsManager)
			{
				return;
			}
			if ((bool)KickStarter.player)
			{
				KickStarter.player.SetLookDirection(base.transform.forward, true);
				KickStarter.player.Teleport(KickStarter.sceneChanger.GetStartPosition(base.transform.position));
				if (SceneSettings.ActInScreenSpace())
				{
					KickStarter.player.transform.position = AdvGame.GetScreenNavMesh(KickStarter.player.transform.position);
				}
			}
			if (KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
			{
				KickStarter.mainCamera.SetFirstPerson();
			}
			else if (cameraOnStart != null)
			{
				SetCameraOnStart();
			}
			else if (!KickStarter.settingsManager.IsInFirstPerson())
			{
				ACDebug.LogWarning("PlayerStart '" + base.name + "' has no Camera On Start", this);
				if (KickStarter.sceneSettings != null && this != KickStarter.sceneSettings.defaultPlayerStart)
				{
					KickStarter.sceneSettings.defaultPlayerStart.SetCameraOnStart();
				}
			}
			KickStarter.eventManager.Call_OnOccupyPlayerStart(KickStarter.player, this);
		}

		public void SetCameraOnStart()
		{
			if (cameraOnStart != null)
			{
				KickStarter.mainCamera.SetGameCamera(cameraOnStart);
				KickStarter.mainCamera.lastNavCamera = cameraOnStart;
				cameraOnStart.MoveCameraInstant();
				KickStarter.mainCamera.SetGameCamera(cameraOnStart);
			}
		}
	}
}
