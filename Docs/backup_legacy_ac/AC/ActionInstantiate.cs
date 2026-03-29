using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionInstantiate : Action
	{
		public GameObject gameObject;

		public int parameterID = -1;

		public int constantID;

		public GameObject replaceGameObject;

		public int replaceParameterID = -1;

		public int replaceConstantID;

		public GameObject relativeGameObject;

		public int relativeGameObjectID;

		public int relativeGameObjectParameterID = -1;

		public int relativeVectorParameterID = -1;

		public Vector3 relativeVector = Vector3.zero;

		public int vectorVarParameterID = -1;

		public int vectorVarID;

		public VariableLocation variableLocation;

		public InvAction invAction;

		public PositionRelativeTo positionRelativeTo;

		protected GameObject _gameObject;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		protected LocalVariables localVariables;

		public int spawnedObjectParameterID = -1;

		protected ActionParameter runtimeSpawnedObjectParameter;

		public ActionInstantiate()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Add or remove";
			description = "Instantiates or deletes GameObjects within the current scene. To ensure this works with save games correctly, place any prefabs to be added in a Resources asset folder.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (invAction == InvAction.Add || invAction == InvAction.Replace)
			{
				_gameObject = AssignFile(parameters, parameterID, 0, gameObject);
				if (invAction == InvAction.Replace)
				{
					replaceGameObject = AssignFile(parameters, replaceParameterID, replaceConstantID, replaceGameObject);
				}
				else if (invAction == InvAction.Add)
				{
					relativeGameObject = AssignFile(parameters, relativeGameObjectParameterID, relativeGameObjectID, relativeGameObject);
				}
			}
			else if (invAction == InvAction.Remove)
			{
				_gameObject = AssignFile(parameters, parameterID, constantID, gameObject);
			}
			relativeVector = AssignVector3(parameters, relativeVectorParameterID, relativeVector);
			if (invAction == InvAction.Add && positionRelativeTo == PositionRelativeTo.VectorVariable)
			{
				runtimeVariable = null;
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
			runtimeSpawnedObjectParameter = null;
			if (invAction == InvAction.Add)
			{
				runtimeSpawnedObjectParameter = GetParameterWithID(parameters, spawnedObjectParameterID);
				if (runtimeSpawnedObjectParameter != null && runtimeSpawnedObjectParameter.parameterType != ParameterType.GameObject)
				{
					runtimeSpawnedObjectParameter = null;
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
			if (_gameObject == null)
			{
				return 0f;
			}
			if (invAction == InvAction.Add)
			{
				GameObject gameObject = AssignFile(constantID, _gameObject);
				if (_gameObject.activeInHierarchy || (gameObject != null && gameObject.activeInHierarchy))
				{
					RememberTransform component = gameObject.GetComponent<RememberTransform>();
					if (!(component != null) || !component.saveScenePresence || component.linkedPrefabID == 0)
					{
						LogWarning(_gameObject.name + " won't be instantiated, as it is already present in the scene.", _gameObject);
						return 0f;
					}
				}
				Vector3 position = _gameObject.transform.position;
				Quaternion rotation = _gameObject.transform.rotation;
				if (positionRelativeTo != PositionRelativeTo.Nothing)
				{
					float z = _gameObject.transform.position.z;
					float x = _gameObject.transform.position.x;
					float y = _gameObject.transform.position.y;
					if (positionRelativeTo == PositionRelativeTo.RelativeToActiveCamera)
					{
						Transform transform = KickStarter.mainCamera.transform;
						position = transform.position + transform.forward * z + transform.right * x + transform.up * y;
						rotation.eulerAngles += transform.transform.rotation.eulerAngles;
					}
					else if (positionRelativeTo == PositionRelativeTo.RelativeToPlayer)
					{
						if ((bool)KickStarter.player)
						{
							Transform transform2 = KickStarter.player.transform;
							position = transform2.position + transform2.forward * z + transform2.right * x + transform2.up * y;
							rotation.eulerAngles += transform2.rotation.eulerAngles;
						}
					}
					else if (positionRelativeTo == PositionRelativeTo.RelativeToGameObject)
					{
						if (relativeGameObject != null)
						{
							Transform transform3 = relativeGameObject.transform;
							position = transform3.position + transform3.forward * z + transform3.right * x + transform3.up * y;
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
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate(_gameObject, position, rotation);
				gameObject2.name = _gameObject.name;
				if ((bool)gameObject2.GetComponent<RememberTransform>())
				{
					gameObject2.GetComponent<RememberTransform>().OnSpawn();
				}
				KickStarter.stateHandler.IgnoreNavMeshCollisions();
				if (runtimeSpawnedObjectParameter != null)
				{
					runtimeSpawnedObjectParameter.SetValue(gameObject2);
				}
			}
			else if (invAction == InvAction.Remove)
			{
				KickStarter.sceneChanger.ScheduleForDeletion(_gameObject);
			}
			else if (invAction == InvAction.Replace)
			{
				if (replaceGameObject == null)
				{
					LogWarning("Cannot perform swap because the object to remove was not found in the scene.");
					return 0f;
				}
				Vector3 position2 = replaceGameObject.transform.position;
				Quaternion rotation2 = replaceGameObject.transform.rotation;
				GameObject gameObject3 = AssignFile(constantID, _gameObject);
				if (this.gameObject.activeInHierarchy || (gameObject3 != null && gameObject3.activeInHierarchy))
				{
					Log(_gameObject.name + " won't be instantiated, as it is already present in the scene.", _gameObject);
					return 0f;
				}
				KickStarter.sceneChanger.ScheduleForDeletion(replaceGameObject);
				GameObject gameObject4 = UnityEngine.Object.Instantiate(_gameObject, position2, rotation2);
				gameObject4.name = _gameObject.name;
				KickStarter.stateHandler.IgnoreNavMeshCollisions();
			}
			return 0f;
		}

		public static ActionInstantiate CreateNew_Add(GameObject prefabToAdd)
		{
			ActionInstantiate actionInstantiate = ScriptableObject.CreateInstance<ActionInstantiate>();
			actionInstantiate.invAction = InvAction.Add;
			actionInstantiate.gameObject = prefabToAdd;
			return actionInstantiate;
		}

		public static ActionInstantiate CreateNew_Remove(GameObject objectToRemove)
		{
			ActionInstantiate actionInstantiate = ScriptableObject.CreateInstance<ActionInstantiate>();
			actionInstantiate.invAction = InvAction.Remove;
			actionInstantiate.gameObject = objectToRemove;
			return actionInstantiate;
		}

		public static ActionInstantiate CreateNew_Replace(GameObject prefabToAdd, GameObject objectToRemove)
		{
			ActionInstantiate actionInstantiate = ScriptableObject.CreateInstance<ActionInstantiate>();
			actionInstantiate.invAction = InvAction.Replace;
			actionInstantiate.gameObject = prefabToAdd;
			actionInstantiate.replaceGameObject = objectToRemove;
			return actionInstantiate;
		}
	}
}
