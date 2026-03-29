using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionPlayMaker : Action
	{
		public bool isPlayer;

		public int constantID;

		public int parameterID = -1;

		public GameObject linkedObject;

		protected GameObject runtimeLinkedObject;

		public string fsmName;

		public int fsmNameParameterID = -1;

		public string eventName;

		public int eventNameParameterID = -1;

		public ActionPlayMaker()
		{
			isDisplayed = true;
			category = ActionCategory.ThirdParty;
			title = "PlayMaker";
			description = "Calls a specified Event within a PlayMaker FSM. Note that PlayMaker is a separate Unity Asset, and the 'PlayMakerIsPresent' preprocessor must be defined for this to work.";
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if (KickStarter.player != null)
				{
					runtimeLinkedObject = KickStarter.player.gameObject;
				}
			}
			else
			{
				runtimeLinkedObject = AssignFile(parameters, parameterID, constantID, linkedObject);
			}
			fsmName = AssignString(parameters, fsmNameParameterID, fsmName);
			eventName = AssignString(parameters, eventNameParameterID, eventName);
		}

		public override float Run()
		{
			if (isPlayer && KickStarter.player == null)
			{
				LogWarning("Cannot use Player's FSM since no Player was found!");
			}
			if (runtimeLinkedObject != null && !string.IsNullOrEmpty(eventName))
			{
				if (fsmName != string.Empty)
				{
					PlayMakerIntegration.CallEvent(runtimeLinkedObject, eventName, fsmName);
				}
				else
				{
					PlayMakerIntegration.CallEvent(runtimeLinkedObject, eventName);
				}
			}
			return 0f;
		}

		public static ActionPlayMaker CreateNew(GameObject playmakerFSM, string eventToCall, string fsmName = "")
		{
			ActionPlayMaker actionPlayMaker = ScriptableObject.CreateInstance<ActionPlayMaker>();
			actionPlayMaker.linkedObject = playmakerFSM;
			actionPlayMaker.eventName = eventToCall;
			actionPlayMaker.fsmName = fsmName;
			return actionPlayMaker;
		}
	}
}
