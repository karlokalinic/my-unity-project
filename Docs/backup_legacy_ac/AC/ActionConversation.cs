using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionConversation : ActionCheckMultiple
	{
		public int parameterID = -1;

		public int constantID;

		public Conversation conversation;

		protected Conversation runtimeConversation;

		public bool overrideOptions;

		public ActionConversation()
		{
			isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Start conversation";
			description = "Enters Conversation mode, and displays the available dialogue options in a specified conversation.";
			numSockets = 0;
		}

		public override void AssignValues(List<ActionParameter> parameters)
		{
			runtimeConversation = AssignFile(parameters, parameterID, constantID, conversation);
		}

		public override float Run()
		{
			if (runtimeConversation == null)
			{
				return 0f;
			}
			if (isRunning)
			{
				if (runtimeConversation.IsActive(true))
				{
					return base.defaultPauseTime;
				}
				isRunning = false;
				return 0f;
			}
			isRunning = false;
			if (overrideOptions)
			{
				if (runtimeConversation.lastOption >= 0)
				{
					KickStarter.actionListManager.ignoreNextConversationSkip = true;
					return 0f;
				}
				KickStarter.actionListManager.ignoreNextConversationSkip = false;
			}
			if (overrideOptions)
			{
				runtimeConversation.Interact(this);
			}
			else
			{
				runtimeConversation.Interact();
				if (willWait && !KickStarter.settingsManager.allowGameplayDuringConversations)
				{
					isRunning = true;
					return base.defaultPauseTime;
				}
			}
			return 0f;
		}

		public override void Skip()
		{
			if (KickStarter.actionListManager.ignoreNextConversationSkip)
			{
				KickStarter.actionListManager.ignoreNextConversationSkip = false;
			}
			else
			{
				Run();
			}
		}

		public override ActionEnd End(List<Action> actions)
		{
			if ((bool)runtimeConversation)
			{
				int lastOption = runtimeConversation.lastOption;
				runtimeConversation.lastOption = -1;
				if (overrideOptions && lastOption >= 0 && endings.Count > lastOption)
				{
					return endings[lastOption];
				}
				if (!overrideOptions && !KickStarter.settingsManager.allowGameplayDuringConversations && willWait && endings.Count > 0)
				{
					return endings[0];
				}
			}
			return GenerateStopActionEnd();
		}

		public static ActionConversation CreateNew(Conversation conversationToRun)
		{
			ActionConversation actionConversation = ScriptableObject.CreateInstance<ActionConversation>();
			actionConversation.conversation = conversationToRun;
			return actionConversation;
		}
	}
}
