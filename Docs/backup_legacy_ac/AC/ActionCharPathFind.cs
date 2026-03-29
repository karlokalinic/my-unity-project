using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharPathFind : Action
	{
		protected enum OnReachTimeLimit
		{
			TeleportToDestination = 0,
			StopMoving = 1
		}

		public int charToMoveParameterID = -1;

		public int markerParameterID = -1;

		public int charToMoveID;

		public int markerID;

		public Marker marker;

		public bool isPlayer;

		public Char charToMove;

		public PathSpeed speed;

		public bool pathFind = true;

		public bool doFloat;

		public bool doTimeLimit;

		public int maxTimeParameterID = -1;

		public float maxTime = 10f;

		[SerializeField]
		protected OnReachTimeLimit onReachTimeLimit;

		protected float currentTimer;

		protected Char runtimeChar;

		protected Marker runtimeMarker;

		public bool faceAfter;

		protected bool isFacingAfter;

		public ActionCharPathFind()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Move to point";
			description = "Moves a character to a given Marker object. By default, the character will attempt to pathfind their way to the marker, but can optionally just move in a straight line.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
			else
			{
				runtimeChar = AssignFile(parameters, charToMoveParameterID, charToMoveID, charToMove);
			}
			Hotspot hotspot = AssignFile<Hotspot>(parameters, markerParameterID, markerID, null, false);
			if (hotspot != null && hotspot.walkToMarker != null)
			{
				runtimeMarker = hotspot.walkToMarker;
			}
			else
			{
				runtimeMarker = AssignFile(parameters, markerParameterID, markerID, marker);
			}
			maxTime = AssignFloat(parameters, maxTimeParameterID, maxTime);
			isFacingAfter = false;
		}

		public override float Run()
		{
			if (!isRunning)
			{
				if (runtimeChar != null && runtimeMarker != null)
				{
					isRunning = true;
					Paths component = runtimeChar.GetComponent<Paths>();
					if (component == null)
					{
						LogWarning("Cannot move a character with no Paths component", runtimeChar);
					}
					else
					{
						if (runtimeChar is NPC)
						{
							NPC nPC = (NPC)runtimeChar;
							nPC.StopFollowing();
						}
						component.pathType = AC_PathType.ForwardOnly;
						component.pathSpeed = speed;
						component.affectY = true;
						Vector3 vector = runtimeMarker.transform.position;
						if (SceneSettings.ActInScreenSpace())
						{
							vector = AdvGame.GetScreenNavMesh(vector);
						}
						float num = Vector3.Distance(vector, runtimeChar.transform.position);
						if (num <= KickStarter.settingsManager.GetDestinationThreshold())
						{
							isRunning = false;
							return 0f;
						}
						Vector3[] pointData;
						if (pathFind && (bool)KickStarter.navigationManager)
						{
							pointData = KickStarter.navigationManager.navigationEngine.GetPointsArray(runtimeChar.transform.position, vector, runtimeChar);
						}
						else
						{
							List<Vector3> list = new List<Vector3>();
							list.Add(vector);
							pointData = list.ToArray();
						}
						if (speed == PathSpeed.Walk)
						{
							runtimeChar.MoveAlongPoints(pointData, false, pathFind);
						}
						else
						{
							runtimeChar.MoveAlongPoints(pointData, true, pathFind);
						}
						if ((bool)runtimeChar.GetPath())
						{
							if (!pathFind && doFloat)
							{
								runtimeChar.GetPath().affectY = true;
							}
							else
							{
								runtimeChar.GetPath().affectY = false;
							}
						}
						if (willWait)
						{
							currentTimer = maxTime;
							return base.defaultPauseTime;
						}
					}
				}
				return 0f;
			}
			if (runtimeChar.GetPath() == null)
			{
				if (faceAfter)
				{
					if (!isFacingAfter)
					{
						isFacingAfter = true;
						runtimeChar.SetLookDirection(runtimeMarker.transform.forward, false);
						return base.defaultPauseTime;
					}
					if (runtimeChar.IsTurning())
					{
						return base.defaultPauseTime;
					}
				}
				isRunning = false;
				return 0f;
			}
			if (doTimeLimit)
			{
				currentTimer -= Time.deltaTime;
				if (currentTimer <= 0f)
				{
					switch (onReachTimeLimit)
					{
					case OnReachTimeLimit.StopMoving:
						runtimeChar.EndPath();
						break;
					case OnReachTimeLimit.TeleportToDestination:
						Skip();
						break;
					}
					isRunning = false;
					return 0f;
				}
			}
			return base.defaultPauseTime;
		}

		public override void Skip()
		{
			if (runtimeChar != null && runtimeMarker != null)
			{
				runtimeChar.EndPath();
				if (runtimeChar is NPC)
				{
					NPC nPC = (NPC)runtimeChar;
					nPC.StopFollowing();
				}
				Vector3 vector = runtimeMarker.transform.position;
				if (SceneSettings.ActInScreenSpace())
				{
					vector = AdvGame.GetScreenNavMesh(vector);
				}
				Vector3[] array;
				if (pathFind && (bool)KickStarter.navigationManager)
				{
					array = KickStarter.navigationManager.navigationEngine.GetPointsArray(runtimeChar.transform.position, vector);
					KickStarter.navigationManager.navigationEngine.ResetHoles(KickStarter.sceneSettings.navMesh);
				}
				else
				{
					List<Vector3> list = new List<Vector3>();
					list.Add(vector);
					array = list.ToArray();
				}
				int num = array.Length - 1;
				if (num > 0)
				{
					runtimeChar.SetLookDirection(array[num] - array[num - 1], true);
				}
				else
				{
					runtimeChar.SetLookDirection(array[num] - runtimeChar.transform.position, true);
				}
				runtimeChar.Teleport(array[num]);
				if (faceAfter)
				{
					runtimeChar.SetLookDirection(runtimeMarker.transform.forward, true);
				}
			}
		}

		public static ActionCharPathFind CreateNew(Char charToMove, Marker marker, PathSpeed pathSpeed = PathSpeed.Walk, bool usePathfinding = true, bool waitUntilFinish = true, bool turnToFaceAfter = false)
		{
			ActionCharPathFind actionCharPathFind = ScriptableObject.CreateInstance<ActionCharPathFind>();
			actionCharPathFind.charToMove = charToMove;
			actionCharPathFind.marker = marker;
			actionCharPathFind.speed = pathSpeed;
			actionCharPathFind.pathFind = usePathfinding;
			actionCharPathFind.willWait = waitUntilFinish;
			actionCharPathFind.faceAfter = turnToFaceAfter;
			return actionCharPathFind;
		}
	}
}
