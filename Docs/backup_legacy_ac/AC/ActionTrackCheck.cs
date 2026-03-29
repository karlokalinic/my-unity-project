using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionTrackCheck : ActionCheck
	{
		protected enum TrackCheckMethod
		{
			PositionValue = 0,
			WithinSnapRegion = 1
		}

		public Moveable_Drag dragObject;

		public int dragConstantID;

		public int dragParameterID = -1;

		protected Moveable_Drag runtimeDragObject;

		public float checkPosition;

		public int checkPositionParameterID = -1;

		public float errorMargin = 0.05f;

		public IntCondition condition;

		public int snapID;

		public int snapParameterID = -1;

		[SerializeField]
		protected TrackCheckMethod method;

		public ActionTrackCheck()
		{
			isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Check track position";
			description = "Queries how far a Draggable object is along its track.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeDragObject = AssignFile(parameters, dragParameterID, dragConstantID, dragObject);
			checkPosition = AssignFloat(parameters, checkPositionParameterID, checkPosition);
			checkPosition = Mathf.Max(0f, checkPosition);
			checkPosition = Mathf.Min(1f, checkPosition);
			snapID = AssignInteger(parameters, snapParameterID, snapID);
		}

		public override ActionEnd End(List<Action> actions)
		{
			return ProcessResult(CheckCondition(), actions);
		}

		public override bool CheckCondition()
		{
			if (runtimeDragObject == null)
			{
				return false;
			}
			if (method == TrackCheckMethod.PositionValue)
			{
				float positionAlong = runtimeDragObject.GetPositionAlong();
				switch (condition)
				{
				case IntCondition.EqualTo:
					if (positionAlong > checkPosition - errorMargin && positionAlong < checkPosition + errorMargin)
					{
						return true;
					}
					break;
				case IntCondition.NotEqualTo:
					if (positionAlong < checkPosition - errorMargin || positionAlong > checkPosition + errorMargin)
					{
						return true;
					}
					break;
				case IntCondition.LessThan:
					if (positionAlong < checkPosition)
					{
						return true;
					}
					break;
				case IntCondition.MoreThan:
					if (positionAlong > checkPosition)
					{
						return true;
					}
					break;
				}
			}
			else if (method == TrackCheckMethod.WithinSnapRegion && runtimeDragObject.track != null)
			{
				return runtimeDragObject.track.IsWithinSnapRegion(runtimeDragObject.trackValue, snapID);
			}
			return false;
		}

		public static ActionTrackCheck CreateNew(Moveable_Drag dragObject, float trackPosition, IntCondition condition = IntCondition.MoreThan, float errorMargin = 0.05f)
		{
			ActionTrackCheck actionTrackCheck = ScriptableObject.CreateInstance<ActionTrackCheck>();
			actionTrackCheck.method = TrackCheckMethod.PositionValue;
			actionTrackCheck.dragObject = dragObject;
			actionTrackCheck.checkPosition = trackPosition;
			actionTrackCheck.errorMargin = errorMargin;
			return actionTrackCheck;
		}

		public static ActionTrackCheck CreateNew(Moveable_Drag dragObject, int snapRegionID)
		{
			ActionTrackCheck actionTrackCheck = ScriptableObject.CreateInstance<ActionTrackCheck>();
			actionTrackCheck.method = TrackCheckMethod.WithinSnapRegion;
			actionTrackCheck.dragObject = dragObject;
			actionTrackCheck.snapID = snapRegionID;
			return actionTrackCheck;
		}
	}
}
