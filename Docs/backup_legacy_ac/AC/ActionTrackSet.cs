using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTrackSet : Action
	{
		public Moveable_Drag dragObject;

		public int dragParameterID = -1;

		public int dragConstantID;

		protected Moveable_Drag runtimeDragObject;

		public float positionAlong;

		public int positionParameterID = -1;

		public float speed = 200f;

		public bool removePlayerControl;

		public bool isInstant;

		public bool stopOnCollide;

		public LayerMask layerMask;

		public ActionTrackSet()
		{
			isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Set track position";
			description = "Moves a Draggable object along its track automatically to a specific point. The effect will be disabled once the object reaches the intended point, or the Action is run again with the speed value set as a negative number.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeDragObject = AssignFile(parameters, dragParameterID, dragConstantID, dragObject);
			positionAlong = AssignFloat(parameters, positionParameterID, positionAlong);
			positionAlong = Mathf.Max(0f, positionAlong);
			positionAlong = Mathf.Min(1f, positionAlong);
		}

		public override float Run()
		{
			if (runtimeDragObject == null)
			{
				isRunning = false;
				return 0f;
			}
			if (!isRunning)
			{
				isRunning = true;
				if (isInstant)
				{
					runtimeDragObject.AutoMoveAlongTrack(positionAlong, 0f, removePlayerControl);
				}
				else if (stopOnCollide)
				{
					runtimeDragObject.AutoMoveAlongTrack(positionAlong, speed, removePlayerControl, layerMask);
				}
				else
				{
					runtimeDragObject.AutoMoveAlongTrack(positionAlong, speed, removePlayerControl);
				}
				if (willWait && !isInstant && speed > 0f)
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
				return 0f;
			}
			if (runtimeDragObject.IsAutoMoving(false))
			{
				return base.defaultPauseTime;
			}
			isRunning = false;
			return 0f;
		}

		public override void Skip()
		{
			if (!(runtimeDragObject == null))
			{
				runtimeDragObject.AutoMoveAlongTrack(positionAlong, 0f, removePlayerControl);
			}
		}

		public static ActionTrackSet CreateNew(Moveable_Drag draggableObject, float newTrackPosition, float movementSpeed = 0f, bool removePlayerControl = false, bool stopUponCollision = false, LayerMask collisionLayer = default(LayerMask), bool waitUntilFinish = false)
		{
			ActionTrackSet actionTrackSet = ScriptableObject.CreateInstance<ActionTrackSet>();
			actionTrackSet.dragObject = draggableObject;
			actionTrackSet.positionAlong = newTrackPosition;
			actionTrackSet.isInstant = movementSpeed <= 0f;
			actionTrackSet.speed = movementSpeed;
			actionTrackSet.removePlayerControl = removePlayerControl;
			actionTrackSet.stopOnCollide = stopUponCollision;
			actionTrackSet.layerMask = collisionLayer;
			actionTrackSet.willWait = waitUntilFinish;
			return actionTrackSet;
		}
	}
}
