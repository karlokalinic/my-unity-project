using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTransform : Action
	{
		public enum SetVectorMethod
		{
			EnteredHere = 0,
			FromVector3Variable = 1
		}

		public enum ToBy
		{
			To = 0,
			By = 1
		}

		public bool isPlayer;

		public int markerParameterID = -1;

		public int markerID;

		public Marker marker;

		protected Marker runtimeMarker;

		public bool doEulerRotation;

		public bool clearExisting = true;

		public bool inWorldSpace;

		public AnimationCurve timeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		public int parameterID = -1;

		public int constantID;

		public Moveable linkedProp;

		protected Moveable runtimeLinkedProp;

		public SetVectorMethod setVectorMethod;

		public int newVectorParameterID = -1;

		public Vector3 newVector;

		public int vectorVarParameterID = -1;

		public int vectorVarID;

		public VariableLocation variableLocation;

		public float transitionTime;

		public int transitionTimeParameterID = -1;

		public TransformType transformType;

		public MoveMethod moveMethod;

		public ToBy toBy;

		protected Vector3 nonSkipTargetVector = Vector3.zero;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		protected LocalVariables localVariables;

		public ActionTransform()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Transform";
			description = "Transforms a GameObject over time, by or to a given amount, or towards a Marker in the scene. The GameObject must have a Moveable script attached.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if (KickStarter.player != null)
				{
					runtimeLinkedProp = KickStarter.player.GetComponent<Moveable>();
				}
				else
				{
					runtimeLinkedProp = null;
				}
			}
			else
			{
				runtimeLinkedProp = AssignFile(parameters, parameterID, constantID, linkedProp);
			}
			runtimeMarker = AssignFile(parameters, markerParameterID, markerID, marker);
			transitionTime = AssignFloat(parameters, transitionTimeParameterID, transitionTime);
			newVector = AssignVector3(parameters, newVectorParameterID, newVector);
			if (transformType != TransformType.CopyMarker && (transformType != TransformType.Translate || toBy != ToBy.To) && (transformType != TransformType.Rotate || toBy != ToBy.To))
			{
				inWorldSpace = false;
			}
			runtimeVariable = null;
			if (transformType == TransformType.CopyMarker || setVectorMethod != SetVectorMethod.FromVector3Variable)
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
			if (!isRunning)
			{
				isRunning = true;
				if (runtimeLinkedProp != null)
				{
					float num = Mathf.Max(transitionTime, 0f);
					RunToTime(num, false);
					if (willWait && num > 0f)
					{
						return base.defaultPauseTime;
					}
				}
				else if (isPlayer && KickStarter.player != null)
				{
					LogWarning(string.Concat("The player ", KickStarter.player, " requires a Moveable component to be moved."), KickStarter.player);
				}
			}
			else if (runtimeLinkedProp != null)
			{
				if (runtimeLinkedProp.IsMoving(transformType))
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
			}
			return 0f;
		}

		public override void Skip()
		{
			if (runtimeLinkedProp != null)
			{
				RunToTime(0f, true);
			}
		}

		protected void RunToTime(float _time, bool isSkipping)
		{
			if (transformType == TransformType.CopyMarker)
			{
				if (runtimeMarker != null)
				{
					runtimeLinkedProp.Move(runtimeMarker, moveMethod, inWorldSpace, _time, timeCurve);
				}
				return;
			}
			Vector3 vector = Vector3.zero;
			if (setVectorMethod == SetVectorMethod.FromVector3Variable)
			{
				if (runtimeVariable != null)
				{
					vector = runtimeVariable.Vector3Value;
				}
			}
			else if (setVectorMethod == SetVectorMethod.EnteredHere)
			{
				vector = newVector;
			}
			if (transformType == TransformType.Translate)
			{
				if (toBy == ToBy.By)
				{
					vector = SetRelativeTarget(vector, isSkipping, runtimeLinkedProp.transform.localPosition);
				}
			}
			else if (transformType == TransformType.Rotate)
			{
				if (toBy == ToBy.By)
				{
					int num = 0;
					if (Mathf.Approximately(vector.x, 0f))
					{
						num++;
					}
					if (Mathf.Approximately(vector.y, 0f))
					{
						num++;
					}
					if (Mathf.Approximately(vector.z, 0f))
					{
						num++;
					}
					if (num == 2)
					{
						vector = SetRelativeTarget(vector, isSkipping, runtimeLinkedProp.transform.eulerAngles);
					}
					else
					{
						Quaternion localRotation = runtimeLinkedProp.transform.localRotation;
						runtimeLinkedProp.transform.Rotate(vector, Space.World);
						vector = runtimeLinkedProp.transform.localEulerAngles;
						runtimeLinkedProp.transform.localRotation = localRotation;
					}
				}
			}
			else if (transformType == TransformType.Scale && toBy == ToBy.By)
			{
				vector = SetRelativeTarget(vector, isSkipping, runtimeLinkedProp.transform.localScale);
			}
			if (transformType == TransformType.Rotate)
			{
				runtimeLinkedProp.Move(vector, moveMethod, inWorldSpace, _time, transformType, doEulerRotation, timeCurve, clearExisting);
			}
			else
			{
				runtimeLinkedProp.Move(vector, moveMethod, inWorldSpace, _time, transformType, false, timeCurve, clearExisting);
			}
		}

		protected Vector3 SetRelativeTarget(Vector3 _targetVector, bool isSkipping, Vector3 normalAddition)
		{
			if (isSkipping && nonSkipTargetVector != Vector3.zero)
			{
				_targetVector = nonSkipTargetVector;
			}
			else
			{
				_targetVector += normalAddition;
				nonSkipTargetVector = _targetVector;
			}
			return _targetVector;
		}

		public static ActionTransform CreateNew(Moveable objectToMove, Marker markerToMoveTo, bool inWorldSpace = true, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform actionTransform = ScriptableObject.CreateInstance<ActionTransform>();
			actionTransform.linkedProp = objectToMove;
			actionTransform.transformType = TransformType.CopyMarker;
			actionTransform.inWorldSpace = inWorldSpace;
			actionTransform.transitionTime = transitionTime;
			actionTransform.moveMethod = moveMethod;
			actionTransform.timeCurve = timeCurve;
			actionTransform.willWait = waitUntilFinish;
			return actionTransform;
		}
	}
}
