using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPlayerTeleportInactive : Action
	{
		public int playerID;

		public int playerIDParameterID = -1;

		public Transform newTransform;

		public int newTransformConstantID;

		public int newTransformParameterID = -1;

		protected Transform runtimeNewTransform;

		public _Camera associatedCamera;

		public int associatedCameraConstantID;

		public int associatedCameraParameterID = -1;

		protected _Camera runtimeAssociatedCamera;

		public ActionPlayerTeleportInactive()
		{
			isDisplayed = true;
			category = ActionCategory.Player;
			title = "Teleport inactive";
			description = "Moves the recorded position of an inactive Player to the current scene.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			playerID = AssignInteger(parameters, playerIDParameterID, playerID);
			runtimeNewTransform = AssignFile(parameters, newTransformParameterID, newTransformConstantID, newTransform);
			runtimeAssociatedCamera = AssignFile(parameters, associatedCameraParameterID, associatedCameraConstantID, associatedCamera);
		}

		public override float Run()
		{
			KickStarter.saveSystem.MoveInactivePlayerToCurrentScene(playerID, runtimeNewTransform, runtimeAssociatedCamera);
			return 0f;
		}

		public static ActionPlayerTeleportInactive CreateNew(int playerID, Transform newTransform, _Camera newCamera = null)
		{
			ActionPlayerTeleportInactive actionPlayerTeleportInactive = ScriptableObject.CreateInstance<ActionPlayerTeleportInactive>();
			actionPlayerTeleportInactive.playerID = playerID;
			actionPlayerTeleportInactive.newTransform = newTransform;
			actionPlayerTeleportInactive.associatedCamera = newCamera;
			return actionPlayerTeleportInactive;
		}
	}
}
