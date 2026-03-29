using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharMove : Action
	{
		public enum MovePathMethod
		{
			MoveOnNewPath = 0,
			StopMoving = 1,
			ResumeLastSetPath = 2
		}

		public MovePathMethod movePathMethod;

		public int charToMoveParameterID = -1;

		public int movePathParameterID = -1;

		public int charToMoveID;

		public int movePathID;

		public bool stopInstantly;

		public Paths movePath;

		protected Paths runtimeMovePath;

		public bool isPlayer;

		public Char charToMove;

		public bool doTeleport;

		public bool doStop;

		public bool startRandom;

		protected Char runtimeChar;

		public ActionCharMove()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Move along path";
			description = "Moves the Character along a pre-determined path. Will adhere to the speed setting selected in the relevant Paths object. Can also be used to stop a character from moving, or resume moving along a path if it was previously stopped.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile(parameters, charToMoveParameterID, charToMoveID, charToMove);
			runtimeMovePath = AssignFile(parameters, movePathParameterID, movePathID, movePath);
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
		}

		protected void UpgradeSelf()
		{
			if (doStop)
			{
				doStop = false;
				movePathMethod = MovePathMethod.StopMoving;
				if (Application.isPlaying)
				{
					ACDebug.Log("'Character: Move along path' Action has been temporarily upgraded - - please view its Inspector when the game ends and save the scene.");
				}
				else
				{
					ACDebug.Log("Upgraded 'Character: Move along path' Action, please save the scene.");
				}
			}
		}

		public override float Run()
		{
			UpgradeSelf();
			if ((bool)runtimeMovePath && (bool)runtimeMovePath.GetComponent<Char>())
			{
				LogWarning("Can't follow a Path attached to a Character!");
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				if ((bool)runtimeChar)
				{
					if (runtimeChar is NPC)
					{
						NPC nPC = (NPC)runtimeChar;
						nPC.StopFollowing();
					}
					if (movePathMethod == MovePathMethod.StopMoving)
					{
						runtimeChar.EndPath();
						if (runtimeChar.IsPlayer && KickStarter.playerInteraction.GetHotspotMovingTo() != null)
						{
							KickStarter.playerInteraction.StopMovingToHotspot();
						}
						if (stopInstantly)
						{
							runtimeChar.Halt();
						}
					}
					else if (movePathMethod == MovePathMethod.MoveOnNewPath)
					{
						if ((bool)runtimeMovePath)
						{
							int num = -1;
							if (runtimeMovePath.pathType == AC_PathType.IsRandom && startRandom && runtimeMovePath.nodes.Count > 1)
							{
								num = UnityEngine.Random.Range(0, runtimeMovePath.nodes.Count);
							}
							PrepareCharacter(num);
							if (willWait && runtimeMovePath.pathType != AC_PathType.ForwardOnly && runtimeMovePath.pathType != AC_PathType.ReverseOnly)
							{
								willWait = false;
								LogWarning("Cannot pause while character moves along a linear path, as this will create an indefinite cutscene.");
							}
							if (num >= 0)
							{
								runtimeChar.SetPath(runtimeMovePath, num, 0);
							}
							else
							{
								runtimeChar.SetPath(runtimeMovePath);
							}
							if (willWait)
							{
								return base.defaultPauseTime;
							}
						}
					}
					else if (movePathMethod == MovePathMethod.ResumeLastSetPath)
					{
						runtimeChar.ResumeLastPath();
					}
				}
				return 0f;
			}
			if (runtimeChar.GetPath() != runtimeMovePath)
			{
				isRunning = false;
				return 0f;
			}
			return base.defaultPauseTime;
		}

		public override void Skip()
		{
			if (!runtimeChar)
			{
				return;
			}
			runtimeChar.EndPath(runtimeMovePath);
			if (runtimeChar is NPC)
			{
				NPC nPC = (NPC)runtimeChar;
				nPC.StopFollowing();
			}
			if (doStop)
			{
				runtimeChar.EndPath();
			}
			else
			{
				if (!runtimeMovePath)
				{
					return;
				}
				int num = -1;
				if (runtimeMovePath.pathType == AC_PathType.ForwardOnly)
				{
					int num2 = runtimeMovePath.nodes.Count - 1;
					runtimeChar.Teleport(runtimeMovePath.nodes[num2]);
					if (num2 > 0)
					{
						runtimeChar.SetLookDirection(runtimeMovePath.nodes[num2] - runtimeMovePath.nodes[num2 - 1], true);
					}
					return;
				}
				if (runtimeMovePath.pathType == AC_PathType.ReverseOnly)
				{
					runtimeChar.Teleport(runtimeMovePath.transform.position);
					if (runtimeMovePath.nodes.Count > 1)
					{
						runtimeChar.SetLookDirection(runtimeMovePath.nodes[0] - runtimeMovePath.nodes[1], true);
					}
					return;
				}
				if (runtimeMovePath.pathType == AC_PathType.IsRandom && startRandom && runtimeMovePath.nodes.Count > 1)
				{
					num = UnityEngine.Random.Range(0, runtimeMovePath.nodes.Count);
				}
				PrepareCharacter(num);
				if (!isPlayer)
				{
					if (num >= 0)
					{
						runtimeChar.SetPath(runtimeMovePath, num, 0);
					}
					else
					{
						runtimeChar.SetPath(runtimeMovePath);
					}
				}
			}
		}

		protected void PrepareCharacter(int randomIndex)
		{
			if (!doTeleport)
			{
				return;
			}
			if (randomIndex >= 0)
			{
				runtimeChar.Teleport(runtimeMovePath.nodes[randomIndex]);
				return;
			}
			int count = runtimeMovePath.nodes.Count;
			if (runtimeMovePath.pathType == AC_PathType.ReverseOnly)
			{
				runtimeChar.Teleport(runtimeMovePath.nodes[count - 1]);
				if (count > 2)
				{
					runtimeChar.SetLookDirection(runtimeMovePath.nodes[count - 2] - runtimeMovePath.nodes[count - 1], true);
				}
			}
			else
			{
				runtimeChar.Teleport(runtimeMovePath.transform.position);
				if (count > 1)
				{
					runtimeChar.SetLookDirection(runtimeMovePath.nodes[1] - runtimeMovePath.nodes[0], true);
				}
			}
		}

		public static ActionCharMove CreateNew_NewPath(Char characterToMove, Paths pathToFollow, bool teleportToStart = false)
		{
			ActionCharMove actionCharMove = ScriptableObject.CreateInstance<ActionCharMove>();
			actionCharMove.movePathMethod = MovePathMethod.MoveOnNewPath;
			actionCharMove.charToMove = characterToMove;
			actionCharMove.movePath = pathToFollow;
			actionCharMove.doTeleport = teleportToStart;
			return actionCharMove;
		}

		public static ActionCharMove CreateNew_ResumeLastPath(Char characterToMove)
		{
			ActionCharMove actionCharMove = ScriptableObject.CreateInstance<ActionCharMove>();
			actionCharMove.movePathMethod = MovePathMethod.ResumeLastSetPath;
			actionCharMove.charToMove = characterToMove;
			return actionCharMove;
		}

		public static ActionCharMove CreateNew_StopMoving(Char characterToStop, bool stopInstantly = false)
		{
			ActionCharMove actionCharMove = ScriptableObject.CreateInstance<ActionCharMove>();
			actionCharMove.movePathMethod = MovePathMethod.StopMoving;
			actionCharMove.charToMove = characterToStop;
			actionCharMove.stopInstantly = stopInstantly;
			return actionCharMove;
		}
	}
}
