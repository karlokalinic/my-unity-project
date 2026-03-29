using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTransformRecord : Action
	{
		public enum TransformRecordType
		{
			Position = 0,
			Rotation = 1,
			Scale = 2
		}

		public bool isPlayer;

		public GameObject obToRead;

		public int obToReadParameterID = -1;

		public int obToReadConstantID;

		protected GameObject runtimeObToRead;

		public TransformRecordType transformRecordType;

		public GlobalLocal transformLocation;

		public VariableLocation variableLocation;

		public int variableID;

		public int variableParameterID = -1;

		public Variables variables;

		public int variablesConstantID;

		protected GVar runtimeVariable;

		protected Variables runtimeVariables;

		protected LocalVariables localVariables;

		public ActionTransformRecord()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Record transform";
			description = "Records the transform values of a GameObject.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if (KickStarter.player != null)
				{
					runtimeObToRead = KickStarter.player.gameObject;
				}
				else
				{
					runtimeObToRead = null;
				}
			}
			runtimeObToRead = AssignFile(parameters, obToReadParameterID, obToReadConstantID, obToRead);
			runtimeVariable = null;
			switch (variableLocation)
			{
			case VariableLocation.Global:
				variableID = AssignVariableID(parameters, variableParameterID, variableID);
				runtimeVariable = GlobalVariables.GetVariable(variableID, true);
				break;
			case VariableLocation.Local:
				if (!isAssetFile)
				{
					variableID = AssignVariableID(parameters, variableParameterID, variableID);
					runtimeVariable = LocalVariables.GetVariable(variableID, localVariables);
				}
				break;
			case VariableLocation.Component:
				runtimeVariables = AssignFile(variablesConstantID, variables);
				if (runtimeVariables != null)
				{
					runtimeVariable = runtimeVariables.GetVariable(variableID);
				}
				runtimeVariable = AssignVariable(parameters, variableParameterID, runtimeVariable);
				runtimeVariables = AssignVariablesComponent(parameters, variableParameterID, runtimeVariables);
				break;
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
			if (runtimeObToRead != null && runtimeVariable != null)
			{
				switch (transformRecordType)
				{
				case TransformRecordType.Position:
					if (transformLocation == GlobalLocal.Global)
					{
						runtimeVariable.SetVector3Value(runtimeObToRead.transform.position);
					}
					else if (transformLocation == GlobalLocal.Local)
					{
						runtimeVariable.SetVector3Value(runtimeObToRead.transform.localPosition);
					}
					break;
				case TransformRecordType.Rotation:
					if (transformLocation == GlobalLocal.Global)
					{
						runtimeVariable.SetVector3Value(runtimeObToRead.transform.eulerAngles);
					}
					else if (transformLocation == GlobalLocal.Local)
					{
						runtimeVariable.SetVector3Value(runtimeObToRead.transform.localEulerAngles);
					}
					break;
				case TransformRecordType.Scale:
					if (transformLocation == GlobalLocal.Global)
					{
						runtimeVariable.SetVector3Value(runtimeObToRead.transform.lossyScale);
					}
					else if (transformLocation == GlobalLocal.Local)
					{
						runtimeVariable.SetVector3Value(runtimeObToRead.transform.localScale);
					}
					break;
				}
				runtimeVariable.Upload(variableLocation, runtimeVariables);
			}
			return 0f;
		}

		public static ActionTransformRecord CreateNew(GameObject objectToRecord, TransformRecordType recordType, bool inWorldSpace, VariableLocation variableLocation, int variableID, Variables variables = null)
		{
			ActionTransformRecord actionTransformRecord = ScriptableObject.CreateInstance<ActionTransformRecord>();
			actionTransformRecord.obToRead = objectToRecord;
			actionTransformRecord.transformRecordType = recordType;
			actionTransformRecord.transformLocation = ((!inWorldSpace) ? GlobalLocal.Local : GlobalLocal.Global);
			actionTransformRecord.variableLocation = variableLocation;
			actionTransformRecord.variableID = variableID;
			actionTransformRecord.variables = variables;
			return actionTransformRecord;
		}
	}
}
