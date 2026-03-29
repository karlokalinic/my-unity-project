using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionEndGame : Action
	{
		public enum AC_EndGameType
		{
			QuitGame = 0,
			LoadAutosave = 1,
			ResetScene = 2,
			RestartGame = 3
		}

		public AC_EndGameType endGameType;

		public ChooseSceneBy chooseSceneBy;

		public int sceneNumber;

		public string sceneName;

		public bool resetMenus;

		public ActionEndGame()
		{
			isDisplayed = true;
			category = ActionCategory.Engine;
			title = "End game";
			description = "Ends the current game, either by loading an autosave, restarting or quitting the game executable.";
			numSockets = 0;
		}

		public override float Run()
		{
			if (endGameType == AC_EndGameType.QuitGame)
			{
				Application.Quit();
			}
			else if (endGameType == AC_EndGameType.LoadAutosave)
			{
				SaveSystem.LoadAutoSave();
			}
			else
			{
				KickStarter.runtimeInventory.SetNull();
				KickStarter.runtimeInventory.RemoveRecipes();
				if ((bool)KickStarter.player)
				{
					UnityEngine.Object.DestroyImmediate(KickStarter.player.gameObject);
				}
				if (endGameType == AC_EndGameType.RestartGame)
				{
					KickStarter.ResetPlayer(KickStarter.settingsManager.GetDefaultPlayer(), KickStarter.settingsManager.GetDefaultPlayerID(), false, Quaternion.identity);
					KickStarter.saveSystem.ClearAllData();
					KickStarter.levelStorage.ClearAllLevelData();
					KickStarter.runtimeInventory.OnStart();
					KickStarter.runtimeDocuments.OnStart();
					KickStarter.runtimeVariables.OnStart();
					if (resetMenus)
					{
						KickStarter.playerMenus.RebuildMenus();
					}
					KickStarter.eventManager.Call_OnRestartGame();
					KickStarter.stateHandler.CanGlobalOnStart();
					KickStarter.sceneChanger.ChangeScene(new SceneInfo(chooseSceneBy, sceneName, sceneNumber), false, true);
				}
				else if (endGameType == AC_EndGameType.ResetScene)
				{
					sceneNumber = UnityVersionHandler.GetCurrentSceneNumber();
					KickStarter.levelStorage.ClearCurrentLevelData();
					KickStarter.sceneChanger.ChangeScene(new SceneInfo(string.Empty, sceneNumber), false, true);
				}
			}
			return 0f;
		}

		public override ActionEnd End(List<Action> actions)
		{
			return GenerateStopActionEnd();
		}

		public static ActionEndGame CreateNew_QuitGame()
		{
			ActionEndGame actionEndGame = ScriptableObject.CreateInstance<ActionEndGame>();
			actionEndGame.endGameType = AC_EndGameType.QuitGame;
			return actionEndGame;
		}

		public static ActionEndGame CreateNew_ResetScene()
		{
			ActionEndGame actionEndGame = ScriptableObject.CreateInstance<ActionEndGame>();
			actionEndGame.endGameType = AC_EndGameType.ResetScene;
			return actionEndGame;
		}

		public static ActionEndGame CreateNew_LoadAutosave()
		{
			ActionEndGame actionEndGame = ScriptableObject.CreateInstance<ActionEndGame>();
			actionEndGame.endGameType = AC_EndGameType.LoadAutosave;
			return actionEndGame;
		}

		public static ActionEndGame CreateNew_RestartGame(int newSceneBuildIndex, bool resetMenus = true)
		{
			ActionEndGame actionEndGame = ScriptableObject.CreateInstance<ActionEndGame>();
			actionEndGame.endGameType = AC_EndGameType.RestartGame;
			actionEndGame.chooseSceneBy = ChooseSceneBy.Number;
			actionEndGame.resetMenus = resetMenus;
			return actionEndGame;
		}
	}
}
