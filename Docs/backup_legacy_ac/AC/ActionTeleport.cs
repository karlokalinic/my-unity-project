using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTeleport : Action
	{
		public int obToMoveParameterID = -1;

		public int obToMoveID;

		public GameObject obToMove;

		protected GameObject runtimeObToMove;

		public int markerParameterID = -1;

		public int markerID;

		public Marker teleporter;

		protected Marker runtimeTeleporter;

		public GameObject relativeGameObject;

		public int relativeGameObjectID;

		public int relativeGameObjectParameterID = -1;

		public PositionRelativeTo positionRelativeTo;

		public int relativeVectorParameterID = -1;

		public Vector3 relativeVector;

		public int vectorVarParameterID = -1;

		public int vectorVarID;

		public VariableLocation variableLocation;

		public bool recalculateActivePathFind;

		public bool isPlayer;

		public bool snapCamera;

		public bool copyRotation;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		protected LocalVariables localVariables;

		public ActionTeleport()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Teleport";
			description = "Moves a GameObject to a Marker instantly. Can also copy the Marker's rotation. The final position can optionally be made relative to the active camera, or the player. For example, if the Marker's position is (0, 0, 1) and Positon relative to is set to Relative To Active Camera, then the object will be teleported in front of the camera.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeObToMove = AssignFile(parameters, obToMoveParameterID, obToMoveID, obToMove);
			runtimeTeleporter = AssignFile(parameters, markerParameterID, markerID, teleporter);
			relativeGameObject = AssignFile(parameters, relativeGameObjectParameterID, relativeGameObjectID, relativeGameObject);
			relativeVector = AssignVector3(parameters, relativeVectorParameterID, relativeVector);
			if (isPlayer && (bool)KickStarter.player)
			{
				runtimeObToMove = KickStarter.player.gameObject;
			}
			runtimeVariable = null;
			if (positionRelativeTo != PositionRelativeTo.VectorVariable)
			{
				return;
			}
			switch (variableLocation)
			{
			case VariableLocation.Global:
				vectorVarID = AssignVariableID(parameters, vectorVarParameterID, vectorVarID);
				runtimeVariable = GlobalVariables.GetVariable(vectorVarID, true);
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					vectorVarID = AssignVariableID(parameters, vectorVarParameterID, vectorVarID);
					runtimeVariable = LocalVariables.GetVariable(vectorVarID, localVariables);
				}
				break;
			case VariableLocation.Component:
			{
				Variables variables = AssignFile(variablesConstantID, this.variables);
				if (variables != null)
				{
					runtimeVariable = variables.GetVariable(vectorVarID);
				}
				runtimeVariable = AssignVariable(parameters, vectorVarParameterID, runtimeVariable);
				break;
			}
			}
		}

		public override void AssignParentList(ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject(actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
			base.AssignParentList(actionList);
		}

		public override float Run()
		{
			if (runtimeTeleporter != null && runtimeObToMove != null)
			{
				Vector3 position = runtimeTeleporter.transform.position;
				Quaternion rotation = runtimeTeleporter.transform.rotation;
				if (positionRelativeTo == PositionRelativeTo.RelativeToActiveCamera)
				{
					Transform transform = KickStarter.mainCamera.transform;
					float x = runtimeTeleporter.transform.position.x;
					float y = runtimeTeleporter.transform.position.y;
					float z = runtimeTeleporter.transform.position.z;
					position = transform.position + transform.forward * z + transform.right * x + transform.up * y;
					rotation.eulerAngles += transform.transform.rotation.eulerAngles;
				}
				else if (positionRelativeTo == PositionRelativeTo.RelativeToPlayer && !isPlayer)
				{
					if ((bool)KickStarter.player)
					{
						Transform transform2 = KickStarter.player.transform;
						float x2 = runtimeTeleporter.transform.position.x;
						float y2 = runtimeTeleporter.transform.position.y;
						float z2 = runtimeTeleporter.transform.position.z;
						position = transform2.position + transform2.forward * z2 + transform2.right * x2 + transform2.up * y2;
						rotation.eulerAngles += transform2.rotation.eulerAngles;
					}
				}
				else if (positionRelativeTo == PositionRelativeTo.RelativeToGameObject)
				{
					if (relativeGameObject != null)
					{
						Transform transform3 = relativeGameObject.transform;
						float x3 = runtimeTeleporter.transform.position.x;
						float y3 = runtimeTeleporter.transform.position.y;
						float z3 = runtimeTeleporter.transform.position.z;
						position = transform3.position + transform3.forward * z3 + transform3.right * x3 + transform3.up * y3;
						rotation.eulerAngles += transform3.rotation.eulerAngles;
					}
				}
				else if (positionRelativeTo == PositionRelativeTo.EnteredValue)
				{
					position += relativeVector;
				}
				else if (positionRelativeTo == PositionRelativeTo.VectorVariable && runtimeVariable != null)
				{
					position += runtimeVariable.Vector3Value;
				}
				Char component = runtimeObToMove.GetComponent<Char>();
				if (copyRotation)
				{
					runtimeObToMove.transform.rotation = rotation;
					if (component != null)
					{
						component.SetLookDirection(runtimeTeleporter.transform.forward, true);
						component.Halt();
					}
				}
				if (component != null)
				{
					component.Teleport(position, recalculateActivePathFind);
				}
				else
				{
					runtimeObToMove.transform.position = position;
				}
				if (isPlayer && snapCamera && KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera != null && KickStarter.mainCamera.attachedCamera.targetIsPlayer)
				{
					KickStarter.mainCamera.attachedCamera.MoveCameraInstant();
				}
			}
			return 0f;
		}

		public static ActionTeleport CreateNew(GameObject objectToMove, Marker marketToTeleportTo, bool copyRotation = true)
		{
			ActionTeleport actionTeleport = ScriptableObject.CreateInstance<ActionTeleport>();
			actionTeleport.obToMove = objectToMove;
			actionTeleport.teleporter = marketToTeleportTo;
			actionTeleport.copyRotation = copyRotation;
			return actionTeleport;
		}
	}
}
