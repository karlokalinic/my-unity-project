using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPlayerLock : Action
	{
		public LockType doUpLock = LockType.NoChange;

		public LockType doDownLock = LockType.NoChange;

		public LockType doLeftLock = LockType.NoChange;

		public LockType doRightLock = LockType.NoChange;

		public PlayerMoveLock doRunLock = PlayerMoveLock.NoChange;

		public LockType doJumpLock = LockType.NoChange;

		public LockType freeAimLock = LockType.NoChange;

		public LockType cursorState = LockType.NoChange;

		public LockType doGravityLock = LockType.NoChange;

		public LockType doHotspotHeadTurnLock = LockType.NoChange;

		public Paths movePath;

		public ActionPlayerLock()
		{
			isDisplayed = true;
			category = ActionCategory.Player;
			title = "Constrain";
			description = "Locks and unlocks various aspects of Player control. When using Direct or First Person control, can also be used to specify a Path object to restrict movement to.";
		}

		public override float Run()
		{
			Player player = KickStarter.player;
			if ((bool)KickStarter.playerInput)
			{
				if (IsSingleLockMovement())
				{
					doLeftLock = doUpLock;
					doRightLock = doUpLock;
					doDownLock = doUpLock;
				}
				if (doUpLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetUpLock(true);
				}
				else if (doUpLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetUpLock(false);
				}
				if (doDownLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetDownLock(true);
				}
				else if (doDownLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetDownLock(false);
				}
				if (doLeftLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetLeftLock(true);
				}
				else if (doLeftLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetLeftLock(false);
				}
				if (doRightLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetRightLock(true);
				}
				else if (doRightLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetRightLock(false);
				}
				if (KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					if (doJumpLock == LockType.Disabled)
					{
						KickStarter.playerInput.SetJumpLock(true);
					}
					else if (doJumpLock == LockType.Enabled)
					{
						KickStarter.playerInput.SetJumpLock(false);
					}
				}
				if (IsInFirstPerson())
				{
					if (freeAimLock == LockType.Disabled)
					{
						KickStarter.playerInput.SetFreeAimLock(true);
					}
					else if (freeAimLock == LockType.Enabled)
					{
						KickStarter.playerInput.SetFreeAimLock(false);
					}
				}
				if (cursorState == LockType.Disabled)
				{
					KickStarter.playerInput.SetInGameCursorState(false);
				}
				else if (cursorState == LockType.Enabled)
				{
					KickStarter.playerInput.SetInGameCursorState(true);
				}
				if (doRunLock != PlayerMoveLock.NoChange)
				{
					KickStarter.playerInput.runLock = doRunLock;
				}
			}
			if ((bool)player)
			{
				if ((bool)movePath)
				{
					player.SetLockedPath(movePath);
					player.SetMoveDirectionAsForward();
				}
				else if ((bool)player.GetPath())
				{
					if (player.IsPathfinding() && !ChangingMovementLock() && (doRunLock == PlayerMoveLock.AlwaysWalk || doRunLock == PlayerMoveLock.AlwaysRun))
					{
						if (doRunLock == PlayerMoveLock.AlwaysRun)
						{
							player.GetPath().pathSpeed = PathSpeed.Run;
							player.isRunning = true;
						}
						else if (doRunLock == PlayerMoveLock.AlwaysWalk)
						{
							player.GetPath().pathSpeed = PathSpeed.Walk;
							player.isRunning = false;
						}
					}
					else
					{
						player.EndPath();
					}
				}
				if (doGravityLock == LockType.Enabled)
				{
					player.ignoreGravity = false;
				}
				else if (doGravityLock == LockType.Disabled)
				{
					player.ignoreGravity = true;
				}
				if (AllowHeadTurning())
				{
					if (doHotspotHeadTurnLock == LockType.Disabled)
					{
						player.SetHotspotHeadTurnLock(true);
					}
					else if (doHotspotHeadTurnLock == LockType.Enabled)
					{
						player.SetHotspotHeadTurnLock(false);
					}
				}
			}
			return 0f;
		}

		protected bool AllowHeadTurning()
		{
			if (SceneSettings.CameraPerspective != CameraPerspective.TwoD && AdvGame.GetReferences().settingsManager.playerFacesHotspots)
			{
				return true;
			}
			return false;
		}

		protected bool IsSingleLockMovement()
		{
			if ((bool)AdvGame.GetReferences().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences().settingsManager;
				if (settingsManager.movementMethod == MovementMethod.PointAndClick || settingsManager.movementMethod == MovementMethod.Drag || settingsManager.movementMethod == MovementMethod.StraightToCursor)
				{
					return true;
				}
			}
			return false;
		}

		protected bool ChangingMovementLock()
		{
			if (doUpLock != LockType.NoChange)
			{
				return true;
			}
			if (!IsSingleLockMovement() && (doDownLock != LockType.NoChange || doLeftLock != LockType.NoChange || doRightLock != LockType.NoChange))
			{
				return true;
			}
			return false;
		}

		protected bool IsInFirstPerson()
		{
			if ((bool)AdvGame.GetReferences().settingsManager && AdvGame.GetReferences().settingsManager.IsInFirstPerson())
			{
				return true;
			}
			return false;
		}

		public static ActionPlayerLock CreateNew(LockType movementLock, LockType jumpLock = LockType.NoChange, LockType freeAimLock = LockType.NoChange, LockType cursorLock = LockType.NoChange, PlayerMoveLock movementSpeedLock = PlayerMoveLock.NoChange, LockType gravityLock = LockType.NoChange, LockType hotspotHeadTurnLock = LockType.NoChange, Paths limitToPath = null)
		{
			ActionPlayerLock actionPlayerLock = ScriptableObject.CreateInstance<ActionPlayerLock>();
			actionPlayerLock.doUpLock = movementLock;
			actionPlayerLock.doLeftLock = movementLock;
			actionPlayerLock.doRightLock = movementLock;
			actionPlayerLock.doDownLock = movementLock;
			actionPlayerLock.doJumpLock = jumpLock;
			actionPlayerLock.freeAimLock = freeAimLock;
			actionPlayerLock.cursorState = cursorLock;
			actionPlayerLock.doRunLock = movementSpeedLock;
			actionPlayerLock.movePath = limitToPath;
			actionPlayerLock.doHotspotHeadTurnLock = hotspotHeadTurnLock;
			return actionPlayerLock;
		}

		public static ActionPlayerLock CreateNew(LockType upMovementLock, LockType downMovementLock, LockType leftMovementLock, LockType rightMovementLock, LockType jumpLock = LockType.NoChange, LockType freeAimLock = LockType.NoChange, LockType cursorLock = LockType.NoChange, PlayerMoveLock movementSpeedLock = PlayerMoveLock.NoChange, LockType gravityLock = LockType.NoChange, LockType hotspotHeadTurnLock = LockType.NoChange, Paths limitToPath = null)
		{
			ActionPlayerLock actionPlayerLock = ScriptableObject.CreateInstance<ActionPlayerLock>();
			actionPlayerLock.doUpLock = upMovementLock;
			actionPlayerLock.doLeftLock = leftMovementLock;
			actionPlayerLock.doRightLock = rightMovementLock;
			actionPlayerLock.doDownLock = downMovementLock;
			actionPlayerLock.doJumpLock = jumpLock;
			actionPlayerLock.freeAimLock = freeAimLock;
			actionPlayerLock.cursorState = cursorLock;
			actionPlayerLock.doRunLock = movementSpeedLock;
			actionPlayerLock.movePath = limitToPath;
			actionPlayerLock.doHotspotHeadTurnLock = hotspotHeadTurnLock;
			return actionPlayerLock;
		}
	}
}
