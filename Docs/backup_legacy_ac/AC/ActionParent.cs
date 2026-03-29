using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionParent : Action
	{
		public enum ParentAction
		{
			SetParent = 0,
			ClearParent = 1
		}

		public int parentTransformID;

		public int parentTransformParameterID = -1;

		public int obToAffectID;

		public int obToAffectParameterID = -1;

		public ParentAction parentAction;

		public Transform parentTransform;

		protected Transform runtimeParentTransform;

		public GameObject obToAffect;

		protected GameObject runtimeObToAffect;

		public bool isPlayer;

		public bool setPosition;

		public Vector3 newPosition;

		public bool setRotation;

		public Vector3 newRotation;

		public ActionParent()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Set parent";
			description = "Parent one GameObject to another. Can also set the child's local position and rotation.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeParentTransform = AssignFile(parameters, parentTransformParameterID, parentTransformID, parentTransform);
			runtimeObToAffect = AssignFile(parameters, obToAffectParameterID, obToAffectID, obToAffect);
			if (isPlayer && (bool)KickStarter.player)
			{
				runtimeObToAffect = KickStarter.player.gameObject;
			}
		}

		public override float Run()
		{
			if (parentAction == ParentAction.SetParent && (bool)runtimeParentTransform)
			{
				runtimeObToAffect.transform.parent = runtimeParentTransform;
				if (setPosition)
				{
					runtimeObToAffect.transform.localPosition = newPosition;
				}
				if (setRotation)
				{
					runtimeObToAffect.transform.localRotation = Quaternion.LookRotation(newRotation);
				}
			}
			else if (parentAction == ParentAction.ClearParent)
			{
				runtimeObToAffect.transform.parent = null;
			}
			return 0f;
		}

		public static ActionParent CreateNew_SetParent(GameObject objectToParent, Transform newParent)
		{
			ActionParent actionParent = ScriptableObject.CreateInstance<ActionParent>();
			actionParent.parentAction = ParentAction.SetParent;
			actionParent.obToAffect = objectToParent;
			actionParent.parentTransform = newParent;
			return actionParent;
		}

		public static ActionParent CreateNew_ClearParent(GameObject objectToClear)
		{
			ActionParent actionParent = ScriptableObject.CreateInstance<ActionParent>();
			actionParent.parentAction = ParentAction.ClearParent;
			actionParent.obToAffect = objectToClear;
			return actionParent;
		}
	}
}
