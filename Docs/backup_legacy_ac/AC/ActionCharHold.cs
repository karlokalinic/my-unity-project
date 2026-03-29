using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionCharHold : Action
	{
		public int objectToHoldParameterID = -1;

		public int _charID;

		public int objectToHoldID;

		public GameObject objectToHold;

		public bool isPlayer;

		public Char _char;

		protected Char runtimeChar;

		public bool rotate90;

		public Vector3 localEulerAngles;

		public int localEulerAnglesParameterID = -1;

		protected GameObject loadedObject;

		public Hand hand;

		public ActionCharHold()
		{
			isDisplayed = true;
			category = ActionCategory.Character;
			title = "Hold object";
			description = "Parents a GameObject to a Character's hand Transform, as chosen in the Character's inspector. The local transforms of the GameObject will be cleared. Note that this action only works with 3D characters.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile(_charID, _char);
			objectToHold = AssignFile(parameters, objectToHoldParameterID, objectToHoldID, objectToHold);
			if (objectToHold != null && !objectToHold.activeInHierarchy)
			{
				loadedObject = UnityEngine.Object.Instantiate(objectToHold);
			}
			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
			Upgrade();
			localEulerAngles = AssignVector3(parameters, localEulerAnglesParameterID, localEulerAngles);
		}

		protected void Upgrade()
		{
			if (rotate90)
			{
				localEulerAngles = new Vector3(0f, 0f, 90f);
				rotate90 = false;
			}
		}

		protected GameObject GetObjectToHold()
		{
			if ((bool)loadedObject)
			{
				return loadedObject;
			}
			return objectToHold;
		}

		public override float Run()
		{
			if ((bool)runtimeChar)
			{
				if (runtimeChar.GetAnimEngine() != null && runtimeChar.GetAnimEngine().ActionCharHoldPossible() && runtimeChar.HoldObject(GetObjectToHold(), hand))
				{
					GetObjectToHold().transform.localEulerAngles = localEulerAngles;
				}
			}
			else
			{
				LogWarning("Could not create animation engine!");
			}
			return 0f;
		}

		public static ActionCharHold CreateNew(Char characterToUpdate, GameObject objectToHold, Hand handToUse, Vector3 localEulerAngles = default(Vector3))
		{
			ActionCharHold actionCharHold = ScriptableObject.CreateInstance<ActionCharHold>();
			actionCharHold._char = characterToUpdate;
			actionCharHold.objectToHold = objectToHold;
			actionCharHold.hand = handToUse;
			actionCharHold.localEulerAngles = localEulerAngles;
			return actionCharHold;
		}
	}
}
