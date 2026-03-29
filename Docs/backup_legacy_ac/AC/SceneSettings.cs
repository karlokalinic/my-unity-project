using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace AC
{
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_scene_settings.html")]
	public class SceneSettings : MonoBehaviour
	{
		public ActionListSource actionListSource;

		public Cutscene cutsceneOnStart;

		public Cutscene cutsceneOnLoad;

		public Cutscene cutsceneOnVarChange;

		public ActionListAsset actionListAssetOnStart;

		public ActionListAsset actionListAssetOnLoad;

		public ActionListAsset actionListAssetOnVarChange;

		public PlayerStart defaultPlayerStart;

		public AC_NavigationMethod navigationMethod = AC_NavigationMethod.meshCollider;

		public string customNavigationClass;

		public NavigationMesh navMesh;

		public SortingMap sortingMap;

		public Sound defaultSound;

		public TintMap tintMap;

		public List<InvVar> attributes = new List<InvVar>();

		public ManagerPackage requiredManagerPackage;

		public bool overrideVerticalReductionFactor;

		public float verticalReductionFactor = 0.7f;

		public float sharedLayerSeparationDistance = 0.001f;

		[SerializeField]
		protected bool overrideCameraPerspective;

		public CameraPerspective cameraPerspective;

		[SerializeField]
		protected MovingTurning movingTurning = MovingTurning.Unity2D;

		protected VideoPlayer fullScreenMovie;

		protected AudioSource defaultAudioSource;

		public static CameraPerspective CameraPerspective
		{
			get
			{
				if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
				{
					return KickStarter.sceneSettings.cameraPerspective;
				}
				if (KickStarter.settingsManager != null)
				{
					return KickStarter.settingsManager.cameraPerspective;
				}
				return CameraPerspective.ThreeD;
			}
		}

		public void OnAwake()
		{
			KickStarter.navigationManager.OnAwake();
			NavigationMesh[] array = Object.FindObjectsOfType(typeof(NavigationMesh)) as NavigationMesh[];
			NavigationMesh[] array2 = array;
			foreach (NavigationMesh navigationMesh in array2)
			{
				if (navMesh != navigationMesh)
				{
					navigationMesh.TurnOff();
				}
			}
			if ((bool)navMesh)
			{
				navMesh.TurnOn();
			}
			if (defaultSound != null)
			{
				defaultAudioSource = defaultSound.GetComponent<AudioSource>();
			}
		}

		public void OnStart()
		{
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.IsInLoadingScene())
			{
				return;
			}
			if (KickStarter.saveSystem.loadingGame == LoadingGame.No)
			{
				if ((bool)KickStarter.player)
				{
					KickStarter.player.EndPath();
					KickStarter.player.Halt(false);
				}
				KickStarter.levelStorage.ReturnCurrentLevelData(false);
				FindPlayerStart();
			}
			else if (KickStarter.saveSystem.loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				KickStarter.levelStorage.ReturnCurrentLevelData(false);
			}
			CheckRequiredManagerPackage();
			if (KickStarter.saveSystem.loadingGame == LoadingGame.No)
			{
				PlayStartCutscene();
			}
		}

		public void UpdateAllSortingMaps()
		{
			if (!(KickStarter.stateHandler != null))
			{
				return;
			}
			foreach (FollowSortingMap followSortingMap in KickStarter.stateHandler.FollowSortingMaps)
			{
				followSortingMap.UpdateSortingMap();
			}
		}

		public PlayerStart GetPlayerStart()
		{
			PlayerStart[] array = Object.FindObjectsOfType(typeof(PlayerStart)) as PlayerStart[];
			List<PlayerStart> list = new List<PlayerStart>();
			PlayerStart[] array2 = array;
			foreach (PlayerStart playerStart in array2)
			{
				if (defaultPlayerStart == null || playerStart != defaultPlayerStart)
				{
					list.Add(playerStart);
				}
			}
			if (defaultPlayerStart != null && !list.Contains(defaultPlayerStart))
			{
				list.Add(defaultPlayerStart);
			}
			foreach (PlayerStart item in list)
			{
				if (item.chooseSceneBy == ChooseSceneBy.Name && !string.IsNullOrEmpty(item.previousSceneName) && item.previousSceneName == KickStarter.sceneChanger.GetPreviousSceneInfo().name)
				{
					return item;
				}
				if (item.chooseSceneBy == ChooseSceneBy.Number && item.previousScene > -1 && item.previousScene == KickStarter.sceneChanger.GetPreviousSceneInfo().number)
				{
					return item;
				}
			}
			if ((bool)defaultPlayerStart)
			{
				return defaultPlayerStart;
			}
			return null;
		}

		public void OnLoad()
		{
			if (actionListSource == ActionListSource.InScene)
			{
				if (cutsceneOnLoad != null)
				{
					cutsceneOnLoad.Interact();
				}
			}
			else if (actionListSource == ActionListSource.AssetFile && actionListAssetOnLoad != null)
			{
				actionListAssetOnLoad.Interact();
			}
		}

		public void PlayDefaultSound(AudioClip audioClip, bool doLoop)
		{
			if (audioClip == null)
			{
				return;
			}
			if (defaultSound == null)
			{
				ACDebug.Log("Cannot play audio '" + audioClip.name + "' since no Default Sound is defined in the scene - please assign one in the Scene Manager.", audioClip);
				return;
			}
			if (KickStarter.stateHandler.IsPaused() && !defaultSound.playWhilePaused)
			{
				ACDebug.LogWarning("Cannot play audio '" + audioClip.name + "' on Sound '" + defaultSound.gameObject.name + "' while the game is paused - check 'Play while game paused?' on the Sound component's Inspector.", defaultSound);
			}
			if (doLoop)
			{
				defaultAudioSource.clip = audioClip;
				defaultSound.Play(doLoop);
			}
			else
			{
				defaultSound.SetMaxVolume();
				defaultAudioSource.PlayOneShot(audioClip);
			}
		}

		public virtual void PauseGame()
		{
			Sound[] array = Object.FindObjectsOfType(typeof(Sound)) as Sound[];
			List<Sound> list = new List<Sound>();
			Sound[] array2 = array;
			foreach (Sound sound in array2)
			{
				if (sound.playWhilePaused && sound.IsPlaying())
				{
					list.Add(sound);
				}
			}
			Time.timeScale = 0f;
			if (fullScreenMovie != null)
			{
				fullScreenMovie.Pause();
			}
			StartCoroutine(PauseAudio());
		}

		public virtual void UnpauseGame(float newScale)
		{
			StopAllCoroutines();
			Time.timeScale = newScale;
			if (fullScreenMovie != null)
			{
				fullScreenMovie.Play();
			}
		}

		public float GetVerticalReductionFactor()
		{
			if (overrideVerticalReductionFactor)
			{
				return verticalReductionFactor;
			}
			return KickStarter.settingsManager.verticalReductionFactor;
		}

		public void SetFullScreenMovie(VideoPlayer movieTexture)
		{
			fullScreenMovie = movieTexture;
		}

		public void StopFullScreenMovie()
		{
			fullScreenMovie = null;
		}

		public bool OverridesCameraPerspective()
		{
			return overrideCameraPerspective;
		}

		public InvVar GetAttribute(int ID)
		{
			if (ID >= 0)
			{
				foreach (InvVar attribute in attributes)
				{
					if (attribute.id == ID)
					{
						return attribute;
					}
				}
			}
			return null;
		}

		protected void FindPlayerStart()
		{
			PlayerStart playerStart = GetPlayerStart();
			if (playerStart != null)
			{
				playerStart.SetPlayerStart();
			}
		}

		protected void PlayStartCutscene()
		{
			KickStarter.stateHandler.PlayGlobalOnStart();
			KickStarter.eventManager.Call_OnStartScene();
			if (actionListSource == ActionListSource.InScene)
			{
				if (cutsceneOnStart != null)
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
					cutsceneOnStart.Interact();
				}
			}
			else if (actionListSource == ActionListSource.AssetFile && actionListAssetOnStart != null)
			{
				KickStarter.stateHandler.gameState = GameState.Normal;
				actionListAssetOnStart.Interact();
			}
		}

		protected IEnumerator PauseAudio()
		{
			yield return null;
			AudioListener.pause = true;
		}

		protected void CheckRequiredManagerPackage()
		{
			if (!(requiredManagerPackage == null))
			{
			}
		}

		public static bool ActInScreenSpace()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
			{
				if ((KickStarter.sceneSettings.movingTurning == MovingTurning.ScreenSpace || KickStarter.sceneSettings.movingTurning == MovingTurning.Unity2D) && KickStarter.sceneSettings.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			else if (KickStarter.settingsManager != null && (KickStarter.settingsManager.movingTurning == MovingTurning.ScreenSpace || KickStarter.settingsManager.movingTurning == MovingTurning.Unity2D) && KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}

		public static bool IsUnity2D()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
			{
				if (KickStarter.sceneSettings.movingTurning == MovingTurning.Unity2D && KickStarter.sceneSettings.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			else if (KickStarter.settingsManager != null && KickStarter.settingsManager.movingTurning == MovingTurning.Unity2D && KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}

		public static bool IsTopDown()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
			{
				if (KickStarter.sceneSettings.movingTurning == MovingTurning.TopDown && KickStarter.sceneSettings.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			else if (KickStarter.settingsManager != null && KickStarter.settingsManager.movingTurning == MovingTurning.TopDown && KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
			{
				return true;
			}
			return false;
		}
	}
}
