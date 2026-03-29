using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionDialogOption : Action
	{
		public enum SwitchType
		{
			On = 0,
			Off = 1,
			OnForever = 2,
			OffForever = 3
		}

		public SwitchType switchType;

		public int optionNumber;

		public int constantID;

		public Conversation linkedConversation;

		protected Conversation runtimeLinkedConversation;

		public ActionDialogOption()
		{
			isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Toggle option";
			description = "Sets the display of a dialogue option. Can hide, show, and lock options.";
		}

		public override void AssignValues()
		{
			runtimeLinkedConversation = AssignFile(constantID, linkedConversation);
		}

		public override float Run()
		{
			if ((bool)runtimeLinkedConversation)
			{
				bool flag = false;
				if (switchType == SwitchType.On || switchType == SwitchType.OnForever)
				{
					flag = true;
				}
				bool isLocked = false;
				if (switchType == SwitchType.OffForever || switchType == SwitchType.OnForever)
				{
					isLocked = true;
				}
				runtimeLinkedConversation.SetOptionState(optionNumber + 1, flag, isLocked);
			}
			return 0f;
		}

		public static ActionDialogOption CreateNew(Conversation conversationToModify, int dialogueOptionID, SwitchType optionSwitchType)
		{
			ActionDialogOption actionDialogOption = ScriptableObject.CreateInstance<ActionDialogOption>();
			actionDialogOption.linkedConversation = conversationToModify;
			actionDialogOption.optionNumber = dialogueOptionID - 1;
			actionDialogOption.switchType = optionSwitchType;
			return actionDialogOption;
		}
	}
}
