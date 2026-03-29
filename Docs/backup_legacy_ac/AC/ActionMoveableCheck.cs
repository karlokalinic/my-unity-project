using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionMoveableCheck : ActionCheck
	{
		public DragBase dragObject;

		public int constantID;

		public int parameterID = -1;

		protected DragBase runtimeDragObject;

		public ActionMoveableCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Check held by player";
			description = "Queries whether or not a Draggable of PickUp object is currently being manipulated.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeDragObject = AssignFile(parameters, parameterID, constantID, dragObject);
		}

		public override bool CheckCondition()
		{
			if (runtimeDragObject != null)
			{
				return KickStarter.playerInput.IsDragObjectHeld(runtimeDragObject);
			}
			return false;
		}

		public static ActionMoveableCheck CreateNew(DragBase dragObject)
		{
			ActionMoveableCheck actionMoveableCheck = ScriptableObject.CreateInstance<ActionMoveableCheck>();
			actionMoveableCheck.dragObject = dragObject;
			return actionMoveableCheck;
		}
	}
}
