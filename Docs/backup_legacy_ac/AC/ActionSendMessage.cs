using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionSendMessage : Action
	{
		public enum MessageToSend
		{
			TurnOn = 0,
			TurnOff = 1,
			Interact = 2,
			Kill = 3,
			Custom = 4
		}

		public int constantID;

		public int parameterID = -1;

		public bool isPlayer;

		public GameObject linkedObject;

		protected GameObject runtimeLinkedObject;

		public bool affectChildren;

		public MessageToSend messageToSend;

		public int customMessageParameterID = -1;

		public string customMessage;

		public bool sendValue;

		public int customValueParameterID = -1;

		public int customValue;

		public bool ignoreWhenSkipping;

		public ActionSendMessage()
		{
			isDisplayed = true;
			category = ActionCategory.Object;
			title = "Send message";
			description = "Sends a given message to a GameObject. Can be either a message commonly-used by Adventure Creator (Interact, TurnOn, etc) or a custom one, with an integer argument.";
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
			customMessage = AssignString(parameters, customMessageParameterID, customMessage);
			customValue = AssignInteger(parameters, customValueParameterID, customValue);
		}

		public override float Run()
		{
			if (runtimeLinkedObject != null)
			{
				if (messageToSend == MessageToSend.Custom)
				{
					if (affectChildren)
					{
						if (!sendValue)
						{
							runtimeLinkedObject.BroadcastMessage(customMessage, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							runtimeLinkedObject.BroadcastMessage(customMessage, customValue, SendMessageOptions.DontRequireReceiver);
						}
					}
					else if (!sendValue)
					{
						runtimeLinkedObject.SendMessage(customMessage);
					}
					else
					{
						runtimeLinkedObject.SendMessage(customMessage, customValue);
					}
				}
				else if (affectChildren)
				{
					runtimeLinkedObject.BroadcastMessage(messageToSend.ToString(), SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					runtimeLinkedObject.SendMessage(messageToSend.ToString());
				}
			}
			return 0f;
		}

		public override void Skip()
		{
			if (!ignoreWhenSkipping)
			{
				Run();
			}
		}

		public override ActionEnd End(List<Action> actions)
		{
			if ((bool)runtimeLinkedObject && messageToSend == MessageToSend.Interact)
			{
				Cutscene component = runtimeLinkedObject.GetComponent<Cutscene>();
				if (component != null && component.triggerTime <= 0f)
				{
					ActionEnd actionEnd = new ActionEnd();
					actionEnd.resultAction = ResultAction.RunCutscene;
					return actionEnd;
				}
			}
			return base.End(actions);
		}

		public static ActionSendMessage CreateNew(GameObject receivingObject, string messageName, bool affectChildren = false, bool ignoreWhenSkipping = false)
		{
			ActionSendMessage actionSendMessage = ScriptableObject.CreateInstance<ActionSendMessage>();
			actionSendMessage.linkedObject = receivingObject;
			actionSendMessage.messageToSend = MessageToSend.Custom;
			actionSendMessage.customMessage = messageName;
			actionSendMessage.sendValue = false;
			actionSendMessage.affectChildren = affectChildren;
			actionSendMessage.ignoreWhenSkipping = ignoreWhenSkipping;
			return actionSendMessage;
		}

		public static ActionSendMessage CreateNew(GameObject receivingObject, string messageName, int parameterValue, bool affectChildren = false, bool ignoreWhenSkipping = false)
		{
			ActionSendMessage actionSendMessage = ScriptableObject.CreateInstance<ActionSendMessage>();
			actionSendMessage.linkedObject = receivingObject;
			actionSendMessage.messageToSend = MessageToSend.Custom;
			actionSendMessage.customMessage = messageName;
			actionSendMessage.sendValue = true;
			actionSendMessage.customValue = parameterValue;
			actionSendMessage.affectChildren = affectChildren;
			actionSendMessage.ignoreWhenSkipping = ignoreWhenSkipping;
			return actionSendMessage;
		}
	}
}
