using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharFaceDirection : Action
	{
		public enum RelativeTo
		{
			Camera = 0,
			Character = 1
		}

		public int charToMoveParameterID = -1;

		public int charToMoveID;

		public bool isInstant;

		public CharDirection direction;

		public Char charToMove;

		protected Char runtimeCharToMove;

		public bool isPlayer;

		[SerializeField]
		protected RelativeTo relativeTo;

		public ActionCharFaceDirection()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Face direction";
			description = "Makes a Character turn, either instantly or over time, to face a direction relative to the camera – i.e. up, down, left or right.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeCharToMove = AssignFile(parameters, charToMoveParameterID, charToMoveID, charToMove);
			if (isPlayer)
			{
				runtimeCharToMove = KickStarter.player;
			}
		}

		public override float Run()
		{
			if (!isRunning)
			{
				isRunning = true;
				if (runtimeCharToMove != null)
				{
					if (!isInstant && runtimeCharToMove.IsMovingAlongPath())
					{
						runtimeCharToMove.EndPath();
					}
					Vector3 charLookVector = AdvGame.GetCharLookVector(direction, (relativeTo != RelativeTo.Character) ? null : runtimeCharToMove);
					runtimeCharToMove.SetLookDirection(charLookVector, isInstant);
					if (!isInstant && willWait)
					{
						return base.defaultPauseTime;
					}
				}
				return 0f;
			}
			if (runtimeCharToMove.IsTurning())
			{
				return base.defaultPauseTime;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			if (runtimeCharToMove != null)
			{
				Vector3 charLookVector = AdvGame.GetCharLookVector(direction, (relativeTo != RelativeTo.Character) ? null : runtimeCharToMove);
				runtimeCharToMove.SetLookDirection(charLookVector, true);
			}
		}

		public static ActionCharFaceDirection CreateNew(Char characterToTurn, CharDirection directionToFace, RelativeTo relativeTo = RelativeTo.Camera, bool isInstant = false, bool waitUntilFinish = false)
		{
			ActionCharFaceDirection actionCharFaceDirection = ScriptableObject.CreateInstance<ActionCharFaceDirection>();
			actionCharFaceDirection.charToMove = characterToTurn;
			actionCharFaceDirection.direction = directionToFace;
			actionCharFaceDirection.relativeTo = relativeTo;
			actionCharFaceDirection.isInstant = isInstant;
			actionCharFaceDirection.willWait = waitUntilFinish;
			return actionCharFaceDirection;
		}
	}
}
