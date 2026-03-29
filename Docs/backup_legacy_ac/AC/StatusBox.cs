using UnityEngine;

namespace AC
{
	public static class StatusBox
	{
		private static Rect debugWindowRect = new Rect(0f, 0f, 260f, 500f);

		private static GUISkin sceneManagerSkin = null;

		public static void DrawDebugWindow()
		{
			if (KickStarter.settingsManager.showActiveActionLists != DebugWindowDisplays.Never && KickStarter.settingsManager.showActiveActionLists != DebugWindowDisplays.EditorOnly)
			{
				debugWindowRect.height = 21f;
				debugWindowRect = GUILayout.Window(10, debugWindowRect, StatusWindow, "AC status", GUILayout.Width(260f));
			}
		}

		private static void StatusWindow(int windowID)
		{
			if (sceneManagerSkin == null)
			{
				sceneManagerSkin = (GUISkin)Resources.Load("SceneManagerSkin");
			}
			GUI.skin = sceneManagerSkin;
			GUILayout.Label("Current game state: " + KickStarter.stateHandler.gameState);
			if (KickStarter.settingsManager.useProfiles)
			{
				GUILayout.Label("Current profile ID: " + Options.GetActiveProfileID());
			}
			if (!(KickStarter.player != null) || GUILayout.Button("Current player: " + KickStarter.player.gameObject.name))
			{
			}
			if (KickStarter.mainCamera != null)
			{
				KickStarter.mainCamera.DrawStatus();
			}
			if (KickStarter.stateHandler.gameState != GameState.DialogOptions || !KickStarter.playerInput.IsInConversation() || GUILayout.Button("Conversation: " + KickStarter.playerInput.activeConversation.gameObject.name))
			{
			}
			GUILayout.Space(4f);
			bool flag = false;
			foreach (ActiveList activeList in KickStarter.actionListManager.activeLists)
			{
				if (activeList.IsRunning())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				GUILayout.Label("ActionLists running:");
				for (int i = 0; i < KickStarter.actionListManager.activeLists.Count; i++)
				{
					KickStarter.actionListManager.activeLists[i].ShowGUI();
				}
			}
			flag = false;
			foreach (ActiveList activeList2 in KickStarter.actionListAssetManager.activeLists)
			{
				if (activeList2.IsRunning())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				GUILayout.Label("ActionList Assets running:");
				foreach (ActiveList activeList3 in KickStarter.actionListAssetManager.activeLists)
				{
					activeList3.ShowGUI();
				}
			}
			if (KickStarter.actionListManager.IsGameplayBlocked())
			{
				GUILayout.Space(4f);
				GUILayout.Label("Gameplay is blocked");
			}
			GUI.DragWindow();
		}
	}
}
