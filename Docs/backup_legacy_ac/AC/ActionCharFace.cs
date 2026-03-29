using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharFace : Action
	{
		public int charToMoveParameterID = -1;

		public int faceObjectParameterID = -1;

		public int charToMoveID;

		public int faceObjectID;

		public bool isInstant;

		public Char charToMove;

		protected Char runtimeCharToMove;

		public GameObject faceObject;

		protected GameObject runtimeFaceObject;

		public bool copyRotation;

		public bool facePlayer;

		public CharFaceType faceType;

		public bool isPlayer;

		public bool lookUpDown;

		public bool stopLooking;

		public bool lookAtHead;

		public ActionCharFace()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Face object";
			description = "Makes a Character turn, either instantly or over time. Can turn to face another object, or copy that object's facing direction.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeCharToMove = AssignFile(parameters, charToMoveParameterID, charToMoveID, charToMove);
			runtimeFaceObject = AssignFile(parameters, faceObjectParameterID, faceObjectID, faceObject);
			if (isPlayer)
			{
				runtimeCharToMove = KickStarter.player;
			}
			else if (facePlayer && (bool)KickStarter.player)
			{
				runtimeFaceObject = KickStarter.player.gameObject;
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (runtimeFaceObject == null && (faceType == CharFaceType.Body || (faceType == CharFaceType.Head && !stopLooking)))
				{
					return 0f;
				}
				if ((bool)runtimeCharToMove)
				{
					if (faceType == CharFaceType.Body)
					{
						if (!isInstant && runtimeCharToMove.IsMovingAlongPath())
						{
							runtimeCharToMove.EndPath();
						}
						if (lookUpDown && isPlayer && KickStarter.settingsManager.IsInFirstPerson())
						{
							Player player = (Player)runtimeCharToMove;
							player.SetTilt(runtimeFaceObject.transform.position, isInstant);
						}
						runtimeCharToMove.SetLookDirection(GetLookVector(KickStarter.settingsManager), isInstant);
					}
					else if (faceType == CharFaceType.Head)
					{
						if (stopLooking)
						{
							runtimeCharToMove.ClearHeadTurnTarget(isInstant, HeadFacing.Manual);
						}
						else
						{
							Vector3 headTurnTargetOffset = Vector3.zero;
							Hotspot component = runtimeFaceObject.GetComponent<Hotspot>();
							Char component2 = runtimeFaceObject.GetComponent<Char>();
							if (lookAtHead && component2 != null)
							{
								Transform neckBone = component2.neckBone;
								if (neckBone != null)
								{
									runtimeFaceObject = neckBone.gameObject;
								}
								else
								{
									Log("Cannot look at " + component2.name + "'s head as their 'Neck bone' has not been defined.", component2);
								}
							}
							else if (component != null)
							{
								if (component.centrePoint != null)
								{
									runtimeFaceObject = component.centrePoint.gameObject;
								}
								else
								{
									headTurnTargetOffset = component.GetIconPosition(true);
								}
							}
							runtimeCharToMove.SetHeadTurnTarget(runtimeFaceObject.transform, headTurnTargetOffset, isInstant);
						}
					}
					if (isInstant)
					{
						return 0f;
					}
					if (willWait)
					{
						return base.defaultPauseTime;
					}
					return 0f;
				}
				return 0f;
			}
			if (faceType == CharFaceType.Head && runtimeCharToMove.IsMovingHead())
			{
				return base.defaultPauseTime;
			}
			if (faceType == CharFaceType.Body && runtimeCharToMove.IsTurning())
			{
				return base.defaultPauseTime;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			if ((runtimeFaceObject == null && (faceType == CharFaceType.Body || (faceType == CharFaceType.Head && !stopLooking))) || !runtimeCharToMove)
			{
				return;
			}
			if (faceType == CharFaceType.Body)
			{
				runtimeCharToMove.SetLookDirection(GetLookVector(KickStarter.settingsManager), true);
				if (lookUpDown && isPlayer && KickStarter.settingsManager.IsInFirstPerson())
				{
					Player player = (Player)runtimeCharToMove;
					player.SetTilt(runtimeFaceObject.transform.position, true);
				}
			}
			else
			{
				if (faceType != CharFaceType.Head)
				{
					return;
				}
				if (stopLooking)
				{
					runtimeCharToMove.ClearHeadTurnTarget(true, HeadFacing.Manual);
					return;
				}
				Vector3 headTurnTargetOffset = Vector3.zero;
				if ((bool)runtimeFaceObject.GetComponent<Hotspot>())
				{
					headTurnTargetOffset = runtimeFaceObject.GetComponent<Hotspot>().GetIconPosition(true);
				}
				else if (lookAtHead && (bool)runtimeFaceObject.GetComponent<Char>())
				{
					Transform neckBone = runtimeFaceObject.GetComponent<Char>().neckBone;
					if (neckBone != null)
					{
						runtimeFaceObject = neckBone.gameObject;
					}
					else
					{
						ACDebug.Log("Cannot look at " + runtimeFaceObject.name + "'s head as their 'Neck bone' has not been defined.", runtimeFaceObject);
					}
				}
				runtimeCharToMove.SetHeadTurnTarget(runtimeFaceObject.transform, headTurnTargetOffset, true);
			}
		}

		protected Vector3 GetLookVector(SettingsManager settingsManager)
		{
			if (copyRotation)
			{
				return runtimeFaceObject.transform.forward;
			}
			if (SceneSettings.ActInScreenSpace())
			{
				return AdvGame.GetScreenDirection(runtimeCharToMove.transform.position, runtimeFaceObject.transform.position);
			}
			return runtimeFaceObject.transform.position - runtimeCharToMove.transform.position;
		}

		protected GameObject GetActualHeadFaceObject(GameObject _originalObject)
		{
			Hotspot component = _originalObject.GetComponent<Hotspot>();
			Char component2 = _originalObject.GetComponent<Char>();
			if (lookAtHead && component2 != null)
			{
				Transform neckBone = component2.neckBone;
				if (neckBone != null)
				{
					return neckBone.gameObject;
				}
			}
			else if (component != null && component.centrePoint != null)
			{
				return component.centrePoint.gameObject;
			}
			return _originalObject;
		}

		public static ActionCharFace CreateNew_BodyFace(Char characterToTurn, GameObject objectToFace, bool useObjectRotation = false, bool isInstant = false, bool waitUntilFinish = false)
		{
			ActionCharFace actionCharFace = ScriptableObject.CreateInstance<ActionCharFace>();
			actionCharFace.charToMove = characterToTurn;
			actionCharFace.faceType = CharFaceType.Body;
			actionCharFace.faceObject = objectToFace;
			actionCharFace.copyRotation = useObjectRotation;
			actionCharFace.isInstant = isInstant;
			actionCharFace.willWait = waitUntilFinish;
			return actionCharFace;
		}

		public static ActionCharFace CreateNew_HeadTurn(Char characterToTurn, GameObject objectToFace, bool faceHeadIfCharacter = true, bool isInstant = false, bool waitUntilFinish = false)
		{
			ActionCharFace actionCharFace = ScriptableObject.CreateInstance<ActionCharFace>();
			actionCharFace.charToMove = characterToTurn;
			actionCharFace.faceType = CharFaceType.Head;
			actionCharFace.faceObject = objectToFace;
			actionCharFace.lookAtHead = true;
			actionCharFace.isInstant = isInstant;
			actionCharFace.willWait = waitUntilFinish;
			return actionCharFace;
		}

		public static ActionCharFace CreateNew_HeadStop(Char characterToTurn, bool isInstant = false)
		{
			ActionCharFace actionCharFace = ScriptableObject.CreateInstance<ActionCharFace>();
			actionCharFace.charToMove = characterToTurn;
			actionCharFace.faceType = CharFaceType.Head;
			actionCharFace.stopLooking = true;
			actionCharFace.isInstant = isInstant;
			return actionCharFace;
		}
	}
}
