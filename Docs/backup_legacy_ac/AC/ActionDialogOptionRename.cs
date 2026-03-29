using System;
using UnityEngine;

namespace AC
{
	[Serializable]
	public class ActionDialogOptionRename : Action, ITranslatable
	{
		public int optionID;

		public string newLabel;

		public int lineID;

		public int constantID;

		public Conversation linkedConversation;

		protected Conversation runtimeLinkedConversation;

		public ActionDialogOptionRename()
		{
			isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Rename option";
			description = "Renames the label of a dialogue option.";
		}

		public override void AssignValues()
		{
			runtimeLinkedConversation = AssignFile(constantID, linkedConversation);
		}

		public override float Run()
		{
			if (runtimeLinkedConversation != null)
			{
				runtimeLinkedConversation.RenameOption(optionID, newLabel, lineID);
			}
			return 0f;
		}

		public string GetTranslatableString(int index)
		{
			return newLabel;
		}

		public int GetTranslationID(int index)
		{
			return lineID;
		}

		public static ActionDialogOptionRename CreateNew(Conversation conversationToModify, int dialogueOptionID, string newLabelText, int translationID = -1)
		{
			ActionDialogOptionRename actionDialogOptionRename = ScriptableObject.CreateInstance<ActionDialogOptionRename>();
			actionDialogOptionRename.linkedConversation = conversationToModify;
			actionDialogOptionRename.optionID = dialogueOptionID;
			actionDialogOptionRename.newLabel = newLabelText;
			actionDialogOptionRename.lineID = translationID;
			return actionDialogOptionRename;
		}
	}
}
