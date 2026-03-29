using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPlayerSwitch : Action
	{
		public int playerID;

		public int playerIDParameterID = -1;

		public NewPlayerPosition newPlayerPosition = NewPlayerPosition.ReplaceNPC;

		public OldPlayer oldPlayer;

		public bool restorePreviousData;

		public bool keepInventory;

		public ChooseSceneBy chooseNewSceneBy;

		public int newPlayerScene;

		public string newPlayerSceneName;

		public int oldPlayerNPC_ID;

		public NPC oldPlayerNPC;

		protected NPC runtimeOldPlayerNPC;

		public int newPlayerNPC_ID;

		public NPC newPlayerNPC;

		protected NPC runtimeNewPlayerNPC;

		public int newPlayerMarker_ID;

		public Marker newPlayerMarker;

		protected Marker runtimeNewPlayerMarker;

		public bool alwaysSnapCamera = true;

		public ActionPlayerSwitch()
		{
			isDisplayed = true;
			category = ActionCategory.Player;
			title = "Switch";
			description = "Swaps out the Player prefab mid-game. If the new prefab has been used before, you can restore that prefab's position data – otherwise you can set the position or scene of the new player. This Action only applies to games for which 'Player switching' has been allowed in the Settings Manager.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeNewPlayerNPC = AssignFile(newPlayerNPC_ID, newPlayerNPC);
			runtimeNewPlayerMarker = AssignFile(newPlayerMarker_ID, newPlayerMarker);
			if (oldPlayer == OldPlayer.ReplaceWithAssociatedNPC && KickStarter.player != null && KickStarter.player.associatedNPCPrefab != null)
			{
				ConstantID component = KickStarter.player.associatedNPCPrefab.GetComponent<ConstantID>();
				if (component != null)
				{
					oldPlayerNPC_ID = component.constantID;
				}
			}
			runtimeOldPlayerNPC = AssignFile(oldPlayerNPC_ID, oldPlayerNPC);
			playerID = AssignInteger(parameters, playerIDParameterID, playerID);
		}

		public override float Run()
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				PlayerPrefab playerPrefab = KickStarter.settingsManager.GetPlayerPrefab(playerID);
				if (playerPrefab != null)
				{
					if (KickStarter.player != null && KickStarter.player.ID == playerID)
					{
						Log("Cannot switch player - already controlling the desired prefab.");
						return 0f;
					}
					if (playerPrefab.playerOb != null)
					{
						KickStarter.saveSystem.SaveCurrentPlayerData();
						Vector3 position = Vector3.zero;
						Quaternion quaternion = default(Quaternion);
						Vector3 localScale = Vector3.one;
						PlayerData playerData = new PlayerData();
						NPCData npcData = new NPCData();
						bool flag = false;
						bool flag2 = false;
						if (KickStarter.player != null)
						{
							position = KickStarter.player.transform.position;
							quaternion = KickStarter.player.TransformRotation;
							localScale = KickStarter.player.transform.localScale;
							playerData = KickStarter.player.SavePlayerData(playerData);
							flag = true;
						}
						if (newPlayerPosition != NewPlayerPosition.ReplaceCurrentPlayer)
						{
							if (oldPlayer == OldPlayer.ReplaceWithAssociatedNPC && (runtimeOldPlayerNPC == null || !runtimeOldPlayerNPC.gameObject.activeInHierarchy) && KickStarter.player.associatedNPCPrefab != null)
							{
								GameObject gameObject = UnityEngine.Object.Instantiate(KickStarter.player.associatedNPCPrefab.gameObject);
								gameObject.name = KickStarter.player.associatedNPCPrefab.gameObject.name;
								runtimeOldPlayerNPC = gameObject.GetComponent<NPC>();
							}
							if ((oldPlayer == OldPlayer.ReplaceWithNPC || oldPlayer == OldPlayer.ReplaceWithAssociatedNPC) && runtimeOldPlayerNPC != null && runtimeOldPlayerNPC.gameObject.activeInHierarchy)
							{
								runtimeOldPlayerNPC.Teleport(position);
								runtimeOldPlayerNPC.SetRotation(quaternion);
								runtimeOldPlayerNPC.transform.localScale = localScale;
								if (flag)
								{
									ApplyRenderData(runtimeOldPlayerNPC, playerData);
								}
								runtimeOldPlayerNPC._Update();
							}
						}
						if ((runtimeNewPlayerNPC == null || newPlayerPosition == NewPlayerPosition.ReplaceAssociatedNPC) && playerPrefab.playerOb.associatedNPCPrefab != null)
						{
							ConstantID component = playerPrefab.playerOb.associatedNPCPrefab.GetComponent<ConstantID>();
							if (component != null && component.constantID != 0)
							{
								newPlayerNPC_ID = component.constantID;
								runtimeNewPlayerNPC = AssignFile<NPC>(component.constantID, null);
							}
						}
						Quaternion rotation = Quaternion.identity;
						if (newPlayerPosition == NewPlayerPosition.ReplaceCurrentPlayer)
						{
							rotation = quaternion;
						}
						else if (newPlayerPosition == NewPlayerPosition.ReplaceNPC && runtimeNewPlayerNPC != null)
						{
							rotation = runtimeNewPlayerNPC.TransformRotation;
						}
						else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker && runtimeNewPlayerMarker != null)
						{
							rotation = runtimeNewPlayerMarker.transform.rotation;
						}
						if (runtimeNewPlayerNPC != null)
						{
							npcData = runtimeNewPlayerNPC.SaveData(npcData);
						}
						bool flag3 = newPlayerPosition == NewPlayerPosition.ReplaceCurrentPlayer && (!restorePreviousData || !KickStarter.saveSystem.DoesPlayerDataExist(playerID, true));
						KickStarter.ResetPlayer(playerPrefab.playerOb, playerID, true, rotation, keepInventory, false, flag3, alwaysSnapCamera);
						Player player = KickStarter.player;
						PlayerMenus.ResetInventoryBoxes();
						if (flag3 && flag)
						{
							ApplyRenderData(player, playerData);
						}
						if (restorePreviousData && KickStarter.saveSystem.DoesPlayerDataExist(playerID, true))
						{
							int playerScene = KickStarter.saveSystem.GetPlayerScene(playerID);
							if (playerScene >= 0 && playerScene != UnityVersionHandler.GetCurrentSceneNumber())
							{
								KickStarter.saveSystem.loadingGame = LoadingGame.JustSwitchingPlayer;
								KickStarter.sceneChanger.ChangeScene(new SceneInfo(string.Empty, playerScene), true);
							}
							else if (runtimeNewPlayerNPC != null)
							{
								player.RepositionToTransform(runtimeNewPlayerNPC.transform);
								runtimeNewPlayerNPC.HideFromView(player);
							}
						}
						else if (newPlayerPosition == NewPlayerPosition.ReplaceCurrentPlayer)
						{
							player.Teleport(position);
							player.SetRotation(quaternion);
							player.transform.localScale = localScale;
						}
						else if (newPlayerPosition == NewPlayerPosition.ReplaceNPC || newPlayerPosition == NewPlayerPosition.ReplaceAssociatedNPC)
						{
							if (runtimeNewPlayerNPC != null)
							{
								player.RepositionToTransform(runtimeNewPlayerNPC.transform);
								runtimeNewPlayerNPC.HideFromView(player);
								if (flag2)
								{
									ApplyRenderData(player, npcData);
								}
							}
						}
						else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker)
						{
							if ((bool)runtimeNewPlayerMarker)
							{
								player.RepositionToTransform(runtimeNewPlayerMarker.transform);
							}
						}
						else if (newPlayerPosition == NewPlayerPosition.AppearInOtherScene)
						{
							if ((chooseNewSceneBy == ChooseSceneBy.Name && newPlayerSceneName == UnityVersionHandler.GetCurrentSceneName()) || (chooseNewSceneBy == ChooseSceneBy.Number && newPlayerScene == UnityVersionHandler.GetCurrentSceneNumber()))
							{
								if ((bool)runtimeNewPlayerNPC && runtimeNewPlayerNPC.gameObject.activeInHierarchy)
								{
									player.RepositionToTransform(runtimeNewPlayerNPC.transform);
									runtimeNewPlayerNPC.HideFromView(player);
								}
							}
							else
							{
								KickStarter.sceneChanger.ChangeScene(new SceneInfo(chooseNewSceneBy, newPlayerSceneName, newPlayerScene), true, false, true);
							}
						}
						if ((bool)KickStarter.mainCamera.attachedCamera && alwaysSnapCamera)
						{
							KickStarter.mainCamera.attachedCamera.MoveCameraInstant();
						}
						AssetLoader.UnloadAssets();
					}
					else
					{
						LogWarning("Cannot switch player - no player prefabs is defined.");
					}
				}
			}
			return 0f;
		}

		protected void ApplyRenderData(Char character, PlayerData playerData)
		{
			character.lockDirection = playerData.playerLockDirection;
			character.lockScale = playerData.playerLockScale;
			if ((bool)character.spriteChild && (bool)character.spriteChild.GetComponent<FollowSortingMap>())
			{
				character.spriteChild.GetComponent<FollowSortingMap>().lockSorting = playerData.playerLockSorting;
			}
			else if ((bool)character.GetComponent<FollowSortingMap>())
			{
				character.GetComponent<FollowSortingMap>().lockSorting = playerData.playerLockSorting;
			}
			else
			{
				character.ReleaseSorting();
			}
			if (playerData.playerLockDirection)
			{
				character.spriteDirection = playerData.playerSpriteDirection;
			}
			if (playerData.playerLockScale)
			{
				character.spriteScale = playerData.playerSpriteScale;
			}
			if (playerData.playerLockSorting)
			{
				if ((bool)character.spriteChild && (bool)character.spriteChild.GetComponent<Renderer>())
				{
					character.spriteChild.GetComponent<Renderer>().sortingOrder = playerData.playerSortingOrder;
					character.spriteChild.GetComponent<Renderer>().sortingLayerName = playerData.playerSortingLayer;
				}
				else if ((bool)character.GetComponent<Renderer>())
				{
					character.GetComponent<Renderer>().sortingOrder = playerData.playerSortingOrder;
					character.GetComponent<Renderer>().sortingLayerName = playerData.playerSortingLayer;
				}
			}
			if (character.GetComponentsInChildren<FollowSortingMap>() == null)
			{
				return;
			}
			FollowSortingMap[] componentsInChildren = character.GetComponentsInChildren<FollowSortingMap>();
			SortingMap sortingMap = Serializer.returnComponent<SortingMap>(playerData.customSortingMapID);
			FollowSortingMap[] array = componentsInChildren;
			foreach (FollowSortingMap followSortingMap in array)
			{
				followSortingMap.followSortingMap = playerData.followSortingMap;
				if (!playerData.followSortingMap && sortingMap != null)
				{
					followSortingMap.SetSortingMap(sortingMap);
				}
				else
				{
					followSortingMap.SetSortingMap(KickStarter.sceneSettings.sortingMap);
				}
			}
		}

		protected void ApplyRenderData(Char character, NPCData npcData)
		{
			character.lockDirection = npcData.lockDirection;
			character.lockScale = npcData.lockScale;
			if ((bool)character.spriteChild && (bool)character.spriteChild.GetComponent<FollowSortingMap>())
			{
				character.spriteChild.GetComponent<FollowSortingMap>().lockSorting = npcData.lockSorting;
			}
			else if ((bool)character.GetComponent<FollowSortingMap>())
			{
				character.GetComponent<FollowSortingMap>().lockSorting = npcData.lockSorting;
			}
			else
			{
				character.ReleaseSorting();
			}
			if (npcData.lockDirection)
			{
				character.spriteDirection = npcData.spriteDirection;
			}
			if (npcData.lockScale)
			{
				character.spriteScale = npcData.spriteScale;
			}
			if (npcData.lockSorting)
			{
				if ((bool)character.spriteChild && (bool)character.spriteChild.GetComponent<Renderer>())
				{
					character.spriteChild.GetComponent<Renderer>().sortingOrder = npcData.sortingOrder;
					character.spriteChild.GetComponent<Renderer>().sortingLayerName = npcData.sortingLayer;
				}
				else if ((bool)character.GetComponent<Renderer>())
				{
					character.GetComponent<Renderer>().sortingOrder = npcData.sortingOrder;
					character.GetComponent<Renderer>().sortingLayerName = npcData.sortingLayer;
				}
			}
			if (character.GetComponentsInChildren<FollowSortingMap>() == null)
			{
				return;
			}
			FollowSortingMap[] componentsInChildren = character.GetComponentsInChildren<FollowSortingMap>();
			SortingMap sortingMap = Serializer.returnComponent<SortingMap>(npcData.customSortingMapID);
			FollowSortingMap[] array = componentsInChildren;
			foreach (FollowSortingMap followSortingMap in array)
			{
				followSortingMap.followSortingMap = npcData.followSortingMap;
				if (!npcData.followSortingMap && sortingMap != null)
				{
					followSortingMap.SetSortingMap(sortingMap);
				}
				else
				{
					followSortingMap.SetSortingMap(KickStarter.sceneSettings.sortingMap);
				}
			}
		}

		public static ActionPlayerSwitch CreateNew(int newPlayerID, bool transferInventory = false, SceneInfo newSceneInfo = null)
		{
			ActionPlayerSwitch actionPlayerSwitch = ScriptableObject.CreateInstance<ActionPlayerSwitch>();
			actionPlayerSwitch.playerID = newPlayerID;
			actionPlayerSwitch.restorePreviousData = true;
			actionPlayerSwitch.keepInventory = transferInventory;
			actionPlayerSwitch.newPlayerPosition = ((newSceneInfo == null) ? NewPlayerPosition.ReplaceAssociatedNPC : NewPlayerPosition.AppearInOtherScene);
			actionPlayerSwitch.oldPlayer = OldPlayer.ReplaceWithAssociatedNPC;
			if (newSceneInfo != null)
			{
				actionPlayerSwitch.newPlayerScene = newSceneInfo.number;
				actionPlayerSwitch.newPlayerSceneName = newSceneInfo.name;
			}
			return actionPlayerSwitch;
		}

		public static ActionPlayerSwitch CreateNew_SwapCurentPlayer(int newPlayerID, bool transferInventory = false)
		{
			ActionPlayerSwitch actionPlayerSwitch = ScriptableObject.CreateInstance<ActionPlayerSwitch>();
			actionPlayerSwitch.playerID = newPlayerID;
			actionPlayerSwitch.restorePreviousData = false;
			actionPlayerSwitch.keepInventory = transferInventory;
			actionPlayerSwitch.newPlayerPosition = NewPlayerPosition.ReplaceCurrentPlayer;
			actionPlayerSwitch.oldPlayer = OldPlayer.RemoveFromScene;
			return actionPlayerSwitch;
		}
	}
}
